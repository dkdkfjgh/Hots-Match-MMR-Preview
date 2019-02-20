using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using Heroes.ReplayParser;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Net.Http;
using System.Diagnostics;
using System.Net;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace MatchPreview
{
    class Program
    {

        static void Main(string[] args)
        {
            HotsMatchPreview hOTSAnalyzer = new HotsMatchPreview();
            HotsLogsParser hotsLogsParser = new HotsLogsParser();

            hotsLogsParser.HotsLogsParse();
            while (true)
            {
                // hOTSAnalyzer.HotsMatchPreviewAnalyzer();
            }



        }


    }

    class HotsMatchPreview
    {
        static string TemporaryBattlelobbyFileLocation;
        float currentDpi;
        DateTime recentBattlelobbyLastWriteTime;
        string content;
        private static float GetCurrentDpi()
        {
            float result;
            using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\FontDPI"))
            {
                if (registryKey != null)
                {
                    result = (float)((int)registryKey.GetValue("LogPixels"));
                }
                else
                {
                    result = 96f;
                }
            }
            return result;
        }
        private static Tuple<int, byte[]> BitmapToBytes(Bitmap bitmap)
        {
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(default(System.Drawing.Point), bitmap.Size), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            byte[] array;
            try
            {
                array = new byte[bitmapData.Stride * bitmap.Height];
                Marshal.Copy(bitmapData.Scan0, array, 0, array.Length);
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
            return new Tuple<int, byte[]>(bitmapData.Stride, array);
        }
        private static void BytesToBitmap(Bitmap bitmap, byte[] bytes)
        {
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(default(System.Drawing.Point), bitmap.Size), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            try
            {
                Marshal.Copy(bytes, 0, bitmapData.Scan0, bytes.Length);
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }

        private string MatchURL = "";

        public void HotsMatchPreviewAnalyzer()
        {
            Thread.Sleep(1000);
            try
            {
                TemporaryBattlelobbyFileLocation = Path.GetTempPath() + "\\Heroes of the Storm\\TempWriteReplayP1\\replay.server.BATTLELOBBY";
                currentDpi = GetCurrentDpi();
                recentBattlelobbyLastWriteTime = File.GetLastWriteTimeUtc(TemporaryBattlelobbyFileLocation);
                content = StandaloneBattleLobbyParser.Base64EncodeStandaloneBattlelobby(StandaloneBattleLobbyParser.Parse(File.ReadAllBytes(TemporaryBattlelobbyFileLocation)));
            }
            catch (Exception)
            {
                Console.WriteLine("File Not Found... Maybe not in Lobby");
                return;
            }

            Console.WriteLine("File Found...! Trying To Parse URL");

            using (Bitmap bitmap = new Bitmap((int)((float)Screen.PrimaryScreen.Bounds.Width * (currentDpi / 96f)), (int)((float)Screen.PrimaryScreen.Bounds.Height * (currentDpi / 96f))))
            {
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, bitmap.Size);
                }
                Tuple<int, byte[]> tuple = BitmapToBytes(bitmap);
                int item = tuple.Item1;
                byte[] item2 = tuple.Item2;
                for (int k = 0; k < bitmap.Width; k++)
                {
                    for (int j = 0; j < bitmap.Height; j++)
                    {
                        int num = j * item + k * 3;
                        decimal d = k / bitmap.Width;
                        bool flag = d < 0.25m || d > 0.75m;
                        if (flag && item2[num] == 255 && item2[num + 1] == 255 && item2[num + 2] == 255)
                        {
                            item2[num] = (item2[num + 1] = (item2[num + 2] = 0));
                        }
                        else if (flag && item2[num] >= 90 && item2[num] <= 160 && item2[num + 1] >= 240 && item2[num + 2] >= 155 && item2[num + 2] <= 200)
                        {
                            item2[num] = (item2[num + 1] = (item2[num + 2] = 1));
                        }
                        else if (flag && item2[num] == 255 && item2[num + 1] >= 138 && item2[num + 1] <= 165 && item2[num + 2] >= 54 && item2[num + 2] <= 99)
                        {
                            item2[num] = (item2[num + 1] = (item2[num + 2] = 2));
                        }
                        else if (flag && item2[num] >= 99 && item2[num] <= 132 && item2[num + 1] >= 58 && item2[num + 1] <= 100 && item2[num + 2] == 255)
                        {
                            item2[num] = (item2[num + 1] = (item2[num + 2] = 3));
                        }
                        else
                        {
                            item2[num] = (item2[num + 1] = (item2[num + 2] = byte.MaxValue));
                        }
                    }
                }
                BytesToBitmap(bitmap, item2);
                string text = string.Empty;
                try
                {
                    using (HttpClient httpClient = new HttpClient())
                    {
                        MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();
                        multipartFormDataContent.Add(new StringContent("4"), "Version");
                        multipartFormDataContent.Add(new StringContent(content), "Data");
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            bitmap.Save(memoryStream, ImageFormat.Png);
                            bitmap.Save("ScreenShot.png", ImageFormat.Png);
                            multipartFormDataContent.Add(new ByteArrayContent(memoryStream.ToArray()), "ScreenShot", "ScreenShot.png");
                        }
                        using (HttpResponseMessage result = httpClient.PostAsync("https://www.hotslogs.com/Player/MatchPreview", multipartFormDataContent).Result)
                        {
                            text = result.Content.ReadAsStringAsync().Result;
                        }
                    }
                }

                catch (Exception)
                {
                    Console.WriteLine("Error!");
                    return;
                }
                Dictionary<string, string> dictionary = (from i in text.Split(new char[] { '\r' })
                                                         select i.Split(new char[] { ':' }) into i
                                                         where i.Length == 2
                                                         select i).ToDictionary((string[] i) => i[0], (string[] i) => i[1]);
                if (dictionary.ContainsKey("Status") && dictionary["Status"] == "Success" && dictionary.ContainsKey("MatchID"))
                {
                    Console.WriteLine("https://www.hotslogs.com/Player/MatchPreview?MatchID=" + dictionary["MatchID"]);
                    MatchURL = "https://www.hotslogs.com/Player/MatchPreview?MatchID=" + dictionary["MatchID"];
                    return;
                }
            }
        }

    }
    class HotsLogsParser
    {
        public List<Player> Players = new List<Player>();

        public void HotsLogsParse()
        {
            HtmlAgilityPack.HtmlDocument Hdocument;
            HtmlWeb Hweb = new HtmlWeb();
            Hdocument = Hweb.Load("https://www.hotslogs.com/Player/MatchPreview?MatchID=ce4c1345");



            string NickName;
            string PlayerID;

            string TLTier;
            string TLMMR;
            string Team;

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    NickName = Hdocument.DocumentNode.SelectSingleNode(string.Format("//*[@id=\"ctl00_MainContent_RadGridMatchPreview_ctl00__{0}\"]/td[3]/a", i)).InnerText;
                }
                catch (Exception)
                {
                    break;
                }

                Team = Hdocument.DocumentNode.SelectSingleNode(string.Format("//*[@id=\"ctl00_MainContent_RadGridMatchPreview_ctl00__{0}\"]/td[2]", i)).InnerText;

                string tmp = Hdocument.DocumentNode.SelectSingleNode(string.Format("//*[@id=\"ctl00_MainContent_RadGridMatchPreview_ctl00__{0}\"]/td[3]/a", i)).OuterHtml;
                tmp = tmp.Substring(tmp.IndexOf("PlayerID="));
                PlayerID = tmp.Substring(9, tmp.IndexOf("\"") - 9);

                WebClient client = new WebClient();
                client.Encoding = Encoding.UTF8;
                string json = client.DownloadString("https://api.hotslogs.com/Public/Players/" + PlayerID);
                try
                {
                    json = json.Substring(json.IndexOf("TeamLeague"));
                    json = json.Substring(0, json.IndexOf("}"));

                    TLTier = json.Substring(json.IndexOf("LeagueID") + 10, json.IndexOf(",") - 10);
                    TLMMR = json.Substring(json.IndexOf("CurrentMMR") + 12);
                }
                catch (Exception)
                {
                    TLTier = "?";
                    TLMMR = "?";
                }


                Players.Add(new Player(NickName, PlayerID, TLTier, TLMMR, Team));

            }

        }
    }
    struct Player
    {
        string NickName;
        string PlayerID;
        string TLTier;
        string TLMMR;
        string Team;
        string ProfileURL;
        string TierURL;
        public Player(string NN, string PID, string TLT, string TLMMR, string Team)
        {
            NickName = NN;
            PlayerID = PID;
            TLTier = TLT;
            this.TLMMR = TLMMR;
            this.Team = Team;
            this.ProfileURL = "https://www.hotslogs.com/Images/PlayerProfileImage/" + PlayerID + ".jpeg";
            this.TierURL = "https://d1i1jxrdh2kvwy.cloudfront.net/Images/Leagues/" + TLTier + ".png";
        }
    }
}

