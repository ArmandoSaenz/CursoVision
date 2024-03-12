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
using AForge;
using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Imaging;
using AForge.Vision.Motion;
using AForge.Vision;

namespace FaceDetection
{
    public partial class Form1 : Form
    {
        FilterInfoCollection videoDevices;
        VideoCaptureDevice videoSource;
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
                return;

            videoSource = new VideoCaptureDevice(videoDevices[comboBoxDevices.SelectedIndex].MonikerString);
            videoSource.NewFrame += VideoSource_NewFrame;
            videoSource.Start();
        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
            AForge.Imaging.Filters.Grayscale grayscaleFilter = new AForge.Imaging.Filters.Grayscale(0.2125, 0.7154, 0.0721);
            Bitmap grayImage = grayscaleFilter.Apply(bitmap);

            /*AForge.Vision.Face.FaceDetector faceDetector = new AForge.Vision.Face.FaceDetector();
            Rectangle[] faces = faceDetector.ProcessFrame(grayImage);

            foreach (Rectangle face in faces)
            {
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    using (Pen pen = new Pen(Color.Red, 2))
                    {
                        graphics.DrawRectangle(pen, face);
                    }
                }
            }*/

            pictureBoxVideo.Image = bitmap;
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
