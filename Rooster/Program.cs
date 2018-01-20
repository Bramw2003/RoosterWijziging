using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Timers;

namespace Rooster
{
    static class Program
    {
        static string WebPath = @"/mnt/c/Users/bwdes/Desktop/";
        static StreamWriter WebWriter;
        static List<string> klassen = new List<string>();

        static void Main(string[] args)
        {
            klassen.Add("H3F2");
            klassen.Add("A3C2");
            //klassen.Add("H3D2");
 
            Console.WriteLine(MillisecondsToNextTopOfTheHour());
            Check(klassen, CreateList(DownloadWebpage()));

            Timer aTimer = new Timer(MillisecondsToNextTopOfTheHour());
            aTimer.Elapsed += OnTimedEvent;
            aTimer.Start();

            Console.ReadLine();
            aTimer.Stop();
        }

        public static void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            System.Timers.Timer aTimer = (System.Timers.Timer)source;
            aTimer.Stop();
            if (DateTime.Now.Hour == 7)
            {
                Check(klassen, CreateList(DownloadWebpage()));
            }
            Console.WriteLine("Top of the Hour @" + DateTime.Now);
            aTimer.Interval = MillisecondsToNextTopOfTheHour();
            aTimer.Start();
        }

        private static double MillisecondsToNextTopOfTheHour()
        {
            return DateTime.Today.Add(new TimeSpan(DateTime.Now.Hour, 0, 0)).AddHours(1).Subtract(DateTime.Now).TotalMilliseconds;
        }

        private static string DownloadWebpage()
        {
            WebClient webClient = new WebClient();
            string page = webClient.DownloadString("http://roosters.carmelcollegesalland.nl/havo-vwo/dagelijksewijzigingen/index.html");
            return page;
        }

        private static List<List<string>> CreateList(string Web)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(Web);

            List<List<string>> table = doc.DocumentNode.SelectSingleNode("//table")
                        .Descendants("tr")
                        .Skip(1)
                        .Where(tr => tr.Elements("td").Count() > 1)
                        .Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim()).ToList())
                        .ToList();

            return table;
        }

        private static bool Check(List<string> klassen, List<List<string>> Table)
        {
            List<List<string>> ccc = new List<List<string>>();
            foreach (var item in Table)
            {
                foreach (var titem in item)
                {
                    Console.WriteLine(titem.ToString());
                }
            }
            // ResetWeb();
            foreach (var a in Table)
            {
                try
                {
                    
                    // Console.WriteLine(a[2].ToString());
                    if (klassen.Contains(a[2].ToString()))
                    {
                        PushLinux("o.mEonty4NidFqBOJGdL7nSltQtrbJFF57", a[3] + " - " + a[4], a[9],a[2]);
                        if(a[2].Contains("H3F2"))
                        {
                            Console.WriteLine("dd");
                            ccc.Add(a);
                        }
                    }
                }
                catch (Exception e)
                {   
                    // Console.WriteLine(e.ToString());
                }
            }
            // UpdateWeb(ccc);
            ccc.Clear();
            return true;
        }

        public static void Push(string token, string title, string body, string channel)
        {
            try
            {
                HttpWebRequest Request = (HttpWebRequest)WebRequest.Create("https://api.pushbullet.com/v2/pushes");
                Request.Method = "POST";
                string postData = "{\"type\": \"note\",\"channel_tag\": \"" + channel.ToLower() +"\", \"title\": \"" + title + "\", \"body\": \"" + body + "\"}";
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                Request.ContentLength = byteArray.Length;
                Request.ContentType = "application/json";
                Request.Headers.Add("Access-Token", token);
                Stream dataStream = Request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                WebResponse response = Request.GetResponse();
                dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
                response.Close();
            }
            catch (ArgumentException a)
            {
                Debug.Print(a.Message);
            }
            catch (WebException a)
            {
                Debug.Print(a.Message);
            }
            catch (Exception a)
            {
                Debug.Print(a.Message);
            }
        }

        public static void PushLinux(string token, string title, string body, string channel)
        {
            string postData = "{\"type\": \"note\",\"channel_tag\": \"" + channel.ToLower() +"\", \"title\": \"" + title + "\", \"body\": \"" + body + "\"}";
            Console.WriteLine("curl --header 'Access-Token: "+ token + "' " + "--header 'Content-Type: application/json' " + "--data-binary '" + postData + "' " + "--request POST https://api.pushbullet.com/v2/pushes");
            Bash("curl --header 'Access-Token: "+ token + "' " + "--header 'Content-Type: application/json' " + "--data-binary '" + postData + "' " + "--request POST https://api.pushbullet.com/v2/pushes"  );
        }

        public static void UpdateWeb(List<List<string>> a)
        {
            WebWriter = new StreamWriter(WebPath + "Rooster.txt");
            foreach (var b in a)
            {
                WebWriter.WriteLine(b[3] + " - " + b[4] + " --- " + b[9]);
            }
            
            WebWriter.Flush();
            WebWriter.Close();
        }
        public static void ResetWeb()
        {
            if(File.Exists(WebPath + "Rooster.txt"))
            {
                // WebWriter.Close();
                File.Delete(WebPath + "Rooster.txt");
            }
            // WebWriter.Close();
        }
        public static string Bash(this string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");
            
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return result;
        }
    }
}
