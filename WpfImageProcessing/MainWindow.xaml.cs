using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AForge.Imaging.Filters;
using AForge.Video;
using AForge.Video.DirectShow;

namespace WpfImageProcessing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        #region Public properties

        public ObservableCollection<FilterInfo> VideoDevices { get; set; }

        public FilterInfo CurrentDevice
        {
            get { return _currentDevice; }
            set { _currentDevice = value; this.OnPropertyChanged("CurrentDevice"); }
        }
        private FilterInfo _currentDevice;
       
        public bool Original
        {
            get { return _original; }
            set { _original = value; this.OnPropertyChanged("Original");}
        }
        private bool _original;

        public bool Grayscaled
        {
            get { return _grayscale; }
            set { _grayscale = value; this.OnPropertyChanged("Grayscaled");}
        }
        private bool _grayscale;

        public bool Thresholded
        {
            get { return _thresholded; }
            set { _thresholded = value; this.OnPropertyChanged("Thresholded");}
        }
        private bool _thresholded;


        public int Threshold
        {
            get { return _threshold; }
            set { _threshold = value; this.OnPropertyChanged("Threshold"); }
        }
        private int _threshold;

        #endregion


        #region Private fields

        private IVideoSource _videoSource;

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            GetVideoDevices();
            Threshold = 127;
            Original = true;
            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopCamera();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            StartCamera();
        }

        private void video_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            try
            {
                BitmapImage bi;
                using (var bitmap = (Bitmap)eventArgs.Frame.Clone())
                {
                    if (Grayscaled)
                    {
                        using (var grayscaledBitmap = Grayscale.CommonAlgorithms.BT709.Apply(bitmap))
                        {
                            bi = grayscaledBitmap.ToBitmapImage();
                        }
                    }
                    else if (Thresholded)
                    {
                        using (var grayscaledBitmap = Grayscale.CommonAlgorithms.BT709.Apply(bitmap))
                        using (var thresholdedBitmap = new Threshold(Threshold).Apply(grayscaledBitmap))
                        {
                            bi = thresholdedBitmap.ToBitmapImage();
                        }
                    }
                    else // original
                    {
                        bi = bitmap.ToBitmapImage();
                    }
                }
                bi.Freeze(); // avoid cross thread operations and prevents leaks
                Dispatcher.BeginInvoke(new ThreadStart(delegate { videoPlayer.Source = bi; }));
            }
            catch (Exception exc)
            {
                MessageBox.Show("Error on _videoSource_NewFrame:\n" + exc.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StopCamera();
            }
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            StopCamera();
        }

        private void GetVideoDevices()
        {
            VideoDevices = new ObservableCollection<FilterInfo>();
            foreach (FilterInfo filterInfo in new FilterInfoCollection(FilterCategory.VideoInputDevice))
            {
                VideoDevices.Add(filterInfo);
            }
            if (VideoDevices.Any())
            {
                CurrentDevice = VideoDevices[0];
            }
            else
            {
                MessageBox.Show("No video sources found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartCamera()
        {
            if (CurrentDevice != null)
            {
                _videoSource = new VideoCaptureDevice(CurrentDevice.MonikerString);
                _videoSource.NewFrame += video_NewFrame;
                _videoSource.Start();
            }
        }

        private void StopCamera()
        {
            if (_videoSource != null && _videoSource.IsRunning)
            {
                _videoSource.SignalToStop();
                _videoSource.NewFrame -= new NewFrameEventHandler(video_NewFrame);
            }
        }

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion
    }
}

