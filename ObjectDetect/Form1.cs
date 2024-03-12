using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Imaging;
using AForge.Video;
using AForge.Video.DirectShow;
using static System.Net.Mime.MediaTypeNames;

namespace ObjectDetect
{
    public partial class Form1 : Form
    {
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;
        Point lastPosition;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count == 0)
            {
                MessageBox.Show("No se encontraron dispositivos de video.");
                return;
            }

            foreach (FilterInfo device in videoDevices)
            {
                comboBoxDevices.Items.Add(device.Name);
            }
            comboBoxDevices.SelectedIndex = 0;
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            if (videoSource != null && videoSource.IsRunning)
            {
                buttonStart.Text = "Iniciar";
                videoSource.SignalToStop();
                videoSource.WaitForStop();
                return;
            }

            videoSource = new VideoCaptureDevice(videoDevices[comboBoxDevices.SelectedIndex].MonikerString);
            videoSource.NewFrame += VideoSource_NewFrame;
            videoSource.Start();
            buttonStart.Text = "Detener";

        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
            AForge.Imaging.Filters.Grayscale grayscaleFilter = new AForge.Imaging.Filters.Grayscale(0.2125, 0.7154, 0.0721);
            Bitmap grayImage = grayscaleFilter.Apply(bitmap);

            AForge.Vision.Motion.SimpleBackgroundModelingDetector motionDetector = new AForge.Vision.Motion.SimpleBackgroundModelingDetector();
            UnmanagedImage unmanagedImage = UnmanagedImage.FromManagedImage(grayImage);
            motionDetector.ProcessFrame(unmanagedImage);
            Bitmap motionImage = motionDetector.MotionFrame.ToManagedImage();

            BlobCounter blobCounter = new BlobCounter();
            blobCounter.FilterBlobs = true;
            blobCounter.MinWidth = 1;
            blobCounter.MinHeight = 1;

            blobCounter.ProcessImage(motionImage);
            Blob[] blobs = blobCounter.GetObjectsInformation();

            if (blobs.Length > 0)
            {
                Blob blob = blobs[0];
                Rectangle rect = blob.Rectangle;
                Point newPosition = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);

                if (lastPosition != Point.Empty)
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.DrawLine(Pens.Red, lastPosition, newPosition);
                    }
                }

                lastPosition = newPosition;
            }

            pictureBoxVideo.Image = grayImage;
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource.WaitForStop();
            }
        }
    }
}
