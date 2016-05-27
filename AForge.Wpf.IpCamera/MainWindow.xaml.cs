using System;
using System.Collections.Generic;
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
using AForge.Video;

namespace AForge.Wpf.IpCamera
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Public properties

        public string ConnectionString
        {
            get { return _connectionString; }
            set { _connectionString = value; this.OnPropertyChanged("ConnectionString"); }
        }

        public bool UseMjpegStream
        {
            get { return _useMJPEGStream; }
            set { _useMJPEGStream = value; this.OnPropertyChanged("UseMjpegStream");}
        }

        public bool UseJpegStream
        {
            get { return _useJPEGStream; }
            set { _useJPEGStream = value; this.OnPropertyChanged("UseJpegStream");}
        }

        #endregion

        #region Private fields

        private string _connectionString;
        private bool _useMJPEGStream;
        private bool _useJPEGStream;
        private IVideoSource _videoSource;

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            ConnectionString = "http://<axis_camera_ip>/axis-cgi/jpg/image.cgi";
            UseJpegStream = true;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {

            // create JPEG video source
            if (UseJpegStream)
            {
                _videoSource = new JPEGStream(ConnectionString);
            }
            else // UseMJpegStream
            {
                _videoSource = new MJPEGStream(ConnectionString);
            }
            _videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame);
            _videoSource.Start();
        }


        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                BitmapImage bi;
                using (var bitmap = (Bitmap)eventArgs.Frame.Clone())
                {
                    bi = bitmap.ToBitmapImage();
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
            _videoSource.SignalToStop();
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
