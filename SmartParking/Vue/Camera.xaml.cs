using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace SmartParking
{
    public partial class Camera : Page
    {
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;
        private bool isCameraRunning = false;

        public Camera()
        {
            InitializeComponent();
            Loaded += Camera_Loaded;
            Unloaded += Camera_Unloaded;
        }

        private void Camera_Loaded(object sender, RoutedEventArgs e)
        {
            if (!isCameraRunning)
                StartCamera();

            // Ajoute une sécurité supplémentaire : arrêter caméra quand on change de page
            if (this.NavigationService != null)
                this.NavigationService.Navigating += Camera_Navigating;
        }

        private void Camera_Unloaded(object sender, RoutedEventArgs e)
        {
            StopCamera();
        }

        private void Camera_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            StopCamera();
        }

        private void StartCamera()
        {
            try
            {
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (videoDevices.Count > 0)
                {
                    videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
                    videoSource.NewFrame += VideoSource_NewFrame;
                    videoSource.Start();
                    isCameraRunning = true;
                }
                else
                {
                    MessageBox.Show("Aucune caméra détectée !");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur au démarrage de la caméra : " + ex.Message);
            }
        }

        private void StopCamera()
        {
            try
            {
                if (videoSource != null && videoSource.IsRunning)
                {
                    var source = videoSource;
                    videoSource.NewFrame -= VideoSource_NewFrame;
                    videoSource = null;
                    isCameraRunning = false;
                    cameraPreview.Source = null;

                    // Arrêt dans un thread séparé
                    new System.Threading.Thread(() =>
                    {
                        try
                        {
                            source.SignalToStop();
                            source.WaitForStop(); // ne bloque plus le thread UI
                        }
                        catch (Exception ex)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                MessageBox.Show("Erreur à l'arrêt de la caméra : " + ex.Message);
                            });
                        }
                    }).Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur à l'arrêt de la caméra : " + ex.Message);
            }
        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                using (Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone())
                {
                    BitmapImage image = BitmapToImageSource(bitmap);

                    Dispatcher.Invoke(() =>
                    {
                        cameraPreview.Source = image;
                    });
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("Erreur d'affichage du flux : " + ex.Message);
                });
            }
        }

        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // Important pour éviter les conflits multi-thread
                return bitmapImage;
            }
        }
    }
}
