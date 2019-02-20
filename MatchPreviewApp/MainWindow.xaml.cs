using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MatchPreviewApp
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        HotsMatchPreview hotsMatchPreview = new HotsMatchPreview();
        DispatcherTimer hotsDetectTimer = new DispatcherTimer();
        string Match = "";

        public MainWindow()
        {
            InitializeComponent();
            
            hotsDetectTimer.Interval = TimeSpan.FromSeconds(7);
            hotsDetectTimer.Tick += new EventHandler(hotsDetect_Tick);
            hotsDetectTimer.Start();

        }

        private void hotsDetect_Tick(object sender,  EventArgs e)
        {
            Match = hotsMatchPreview.HotsMatchPreviewAnalyzer();
            if (Match.StartsWith("http"))
            {
                ParseHotsLogsData(Match);
                hotsDetectTimer.Stop();
            }
            else
            {
                try
                {
                    GridRemover(PlayerBox01);
                    GridRemover(PlayerBox02);
                    GridRemover(PlayerBox03);
                    GridRemover(PlayerBox04);
                    GridRemover(PlayerBox05);
                    GridRemover(PlayerBox06);
                    GridRemover(PlayerBox07);
                    GridRemover(PlayerBox08);
                    GridRemover(PlayerBox09);
                    GridRemover(PlayerBox10);
                }
                catch (Exception)
                {

                }

            }
        }


        void ParseHotsLogsData(string MatchUrl)
        {
            HotsLogsParser hParser = new HotsLogsParser();
            hParser.HotsLogsParse(MatchUrl);
            try
            {
                CreatePlayerBox(PlayerBox01, hParser.BlueTeam[0]);
                CreatePlayerBox(PlayerBox02, hParser.BlueTeam[1]);
                CreatePlayerBox(PlayerBox03, hParser.BlueTeam[2]);
                CreatePlayerBox(PlayerBox04, hParser.BlueTeam[3]);
                CreatePlayerBox(PlayerBox05, hParser.BlueTeam[4]);

                CreatePlayerBox(PlayerBox06, hParser.RedTeam[0]);
                CreatePlayerBox(PlayerBox07, hParser.RedTeam[1]);
                CreatePlayerBox(PlayerBox08, hParser.RedTeam[2]);
                CreatePlayerBox(PlayerBox09, hParser.RedTeam[3]);
                CreatePlayerBox(PlayerBox10, hParser.RedTeam[4]);
            }
            catch (Exception e)
            {

            }
            hParser = null;
            System.GC.Collect(0, GCCollectionMode.Forced);
            System.GC.WaitForFullGCComplete();


        }

        void GridRemover(Grid grid)
        {
            grid.Children.Clear();
        }

        void CreatePlayerBox(Grid grid, Player P)
        {
            string NickName, MMR, TierURL, ProfileURL;
            NickName = P.NickName;
            MMR = P.TLMMR;
            TierURL = P.TierURL;
            ProfileURL = P.ProfileURL;

            Label LNickName = new Label
            {
                Content = NickName,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Height = 36,
                Width = 156,
                FontSize = 20,
                FontWeight = FontWeights.Bold
            };

            Label LMMR = new Label
            {
                Content = "MMR : " + MMR,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Height = 40,
                Width = 76,
                Margin = new Thickness(0, 36, 0, 0)
            };

            BitmapImage most1 = new BitmapImage();
            Bitmap Bmost1 = null;
            BitmapImage most2 = new BitmapImage();
            Bitmap Bmost2 = null;
            BitmapImage most3 = new BitmapImage();
            Bitmap Bmost3 = null;


            BitmapImage TierBitmap = new BitmapImage();
            TierBitmap.BeginInit();
            TierBitmap.UriSource = new Uri(TierURL, UriKind.Absolute);
            TierBitmap.EndInit();
            System.Windows.Controls.Image TierImage = new System.Windows.Controls.Image
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Height = 36,
                Width = 40,
                Margin = new Thickness(156, 0, 0, 0),
                Source = TierBitmap
            };

            try
            {
                Bitmap ProfileImage = DownloadBitmap(ProfileURL);
                Bmost1 = (Bitmap)cropImage(ProfileImage, new System.Drawing.Rectangle(new System.Drawing.Point(20, 46), new System.Drawing.Size(60, 60)));
                most1 = BitmapToImageSource(Bmost1);
                Bmost2 = (Bitmap)cropImage(ProfileImage, new System.Drawing.Rectangle(new System.Drawing.Point(90, 46), new System.Drawing.Size(60, 60)));
                most2 = BitmapToImageSource(Bmost2);
                Bmost3 = (Bitmap)cropImage(ProfileImage, new System.Drawing.Rectangle(new System.Drawing.Point(160, 46), new System.Drawing.Size(60, 60)));
                most3 = BitmapToImageSource(Bmost3);
            }
            catch (Exception)
            {

            }


            System.Windows.Controls.Image MostImage1 = new System.Windows.Controls.Image
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Height = 40,
                Width = 40,
                Margin = new Thickness(76, 36, 0, 0),
                Source = most1
            };

            System.Windows.Controls.Image MostImage2 = new System.Windows.Controls.Image
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Height = 40,
                Width = 40,
                Margin = new Thickness(121, 36, 0, 0),
                Source = most2
            };

            System.Windows.Controls.Image MostImage3 = new System.Windows.Controls.Image
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Height = 40,
                Width = 40,
                Margin = new Thickness(166, 36, 0, 0),
                Source = most3
            };

            grid.Children.Add(LNickName);
            grid.Children.Add(LMMR);
            grid.Children.Add(TierImage);
            grid.Children.Add(MostImage1);
            grid.Children.Add(MostImage2);
            grid.Children.Add(MostImage3);

            Bmost1.Dispose();
            Bmost2.Dispose();
            Bmost3.Dispose();
            most1 = null;
            most2 = null;
            most3 = null;

            System.GC.Collect(0, GCCollectionMode.Forced);
            System.GC.WaitForFullGCComplete();

        }
        private Bitmap DownloadBitmap(string url)
        {
            System.Net.WebRequest request = System.Net.WebRequest.Create(url);
            System.Net.WebResponse response = request.GetResponse();
            System.IO.Stream responseStream =
                response.GetResponseStream();
            return new Bitmap(responseStream);
        }
        private static System.Drawing.Image cropImage(System.Drawing.Image img, System.Drawing.Rectangle cropArea)
        {
            Bitmap bmpImage = new Bitmap(img);
            return bmpImage.Clone(cropArea, bmpImage.PixelFormat);
        }
        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            GridRemover(PlayerBox01);
            GridRemover(PlayerBox02);
            GridRemover(PlayerBox03);
            GridRemover(PlayerBox04);
            GridRemover(PlayerBox05);
            GridRemover(PlayerBox06);
            GridRemover(PlayerBox07);
            GridRemover(PlayerBox08);
            GridRemover(PlayerBox09);
            GridRemover(PlayerBox10);
            hotsDetectTimer.Start();
        }
    }
}
