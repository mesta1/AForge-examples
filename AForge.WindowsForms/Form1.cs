using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;

namespace AForge.WindowsForms
{
    public partial class Form1 : Form
    {
        private FilterInfoCollection videoDevicesList;
        private IVideoSource videoSource;

        public Form1()
        {
            InitializeComponent();
            // get list of video devices
            videoDevicesList = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo videoDevice in videoDevicesList)
            {
                cmbVideoSource.Items.Add(videoDevice.Name);
            }
            if (cmbVideoSource.Items.Count > 0)
            {
                cmbVideoSource.SelectedIndex = 0;
            }
            else
            {
                MessageBox.Show("No video sources found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            // stop the camera on window close
            this.Closing += Form1_Closing;
        }

        private void Form1_Closing(object sender, CancelEventArgs e)
        {
            // signal to stop
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
            }
        }

        private void video_NewFrame(object sender,NewFrameEventArgs eventArgs)
        {
            Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
            pictureBox1.Image = bitmap;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            videoSource = new VideoCaptureDevice(videoDevicesList[cmbVideoSource.SelectedIndex].MonikerString);
            videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame);
            videoSource.Start();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            videoSource.SignalToStop();
            if (videoSource != null && videoSource.IsRunning && pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
            }
        }
    }
}
