using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;
using static Red_Alert_Engine_V2.Program;

namespace Red_Alert_Engine_V2
{
    internal class Program
    {
        public bool main_server_down;
        public string IDF_URL = @"https://www.oref.org.il/WarningMessages/alert/alerts.json";
        public struct Alert
        {
            public string desc;
            public string title;
            public List<string> data;

            public Alert(string desc, string title, List<string> data)
            {
                this.desc = desc;
                this.title = title;
                this.data = data;
            }
        }

        public struct Alert_History
        {
            public string alertDate;
            public string title;
            public string data;

            public Alert_History(string alertDate, string title, string data)
            {
                this.alertDate = alertDate;
                this.title = title;
                this.data = data;
            }
        }
        static void Main(string[] args)
        {
            Program program = new Program();
            program.Serch_Alert();
        }
        Task Serch_Alert()
        {
            Alert alert = JsonConvert.DeserializeObject<Alert>("{}");
            var old_alert = alert;
            string Alert_Downloaded_Old, Alert_Downloaded_New;

            Program program = new Program();

            Alert_Downloaded_Old = program.WebClient_(IDF_URL);
            Task.Delay(1000).Wait();
            Alert_Downloaded_New = program.WebClient_(IDF_URL);

            while (main_server_down == false)
            {
                Alert_Downloaded_New = program.WebClient_(IDF_URL);

                if (Alert_Downloaded_New != "Error" && Alert_Downloaded_New != null && Alert_Downloaded_New != Alert_Downloaded_Old && Alert_Downloaded_New != "\r\n")
                {
                    alert = JsonConvert.DeserializeObject<Alert>(Alert_Downloaded_New);
                    if (Alert_Downloaded_Old != null && Alert_Downloaded_Old != "\r\n")
                    {
                        old_alert = JsonConvert.DeserializeObject<Alert>(Alert_Downloaded_Old);
                        alert.data = alert.data.Except(old_alert.data).ToList();
                    }
                    Alert_Log(alert);
                    Console.WriteLine("Alert!");  
                }
                else if(Alert_Downloaded_New == "Error")
                {
                    Serch_Alert_Altrante_server();
                    main_server_down = true;
                }
                Alert_Downloaded_Old = Alert_Downloaded_New;
            }
            return Task.CompletedTask;
        }

        Task Serch_Alert_Altrante_server()
        {
            Console.WriteLine("Conected to Altrnate servers....");
            Alert_History alert = JsonConvert.DeserializeObject<Alert_History>("{}");
            string Alert_Downloaded_Old, Alert_Downloaded_New;
            string Alert;
            Alert = @"https://www.oref.org.il/WarningMessages/History/AlertsHistory.json";
            Program program = new Program();

            while (WebClient_(IDF_URL) == "Error")
            {
                Alert_Downloaded_Old = program.WebClient_(Alert);
                Task.Delay(1000).Wait();
                Alert_Downloaded_New = program.WebClient_(Alert);

                if (Alert_Downloaded_New != null && Alert_Downloaded_New != Alert_Downloaded_Old)
                {
                    alert = JsonConvert.DeserializeObject<Alert_History>(Alert_Downloaded_New);
                    //alert.title[0]; --> to get latest alert
                    Console.WriteLine("Alert!");
                }
                else if (Alert_Downloaded_New == "Error")
                {
                    System.Diagnostics.Process.Start(System.AppDomain.CurrentDomain.FriendlyName);
                    Environment.Exit(1);
                    break;
                }
            }
            main_server_down = false;
            Serch_Alert();
            return Task.CompletedTask;
        }

        String WebClient_(string link)
        {
            //////////////////////////////////////////////////////////////////////////////
            WebClient _clinet = new WebClient();
            _clinet.Headers.Add("user-agent",
    "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.100 Safari/537.36");
            _clinet.Headers.Add("X-Requested-With", "XMLHttpRequest");
            _clinet.Headers.Add("Referer", "https://www.oref.org.il/11226-he/pakar.aspx");
            //////////////////////////////////////////////////////////////////////////////
            try
            {
                return _clinet.DownloadString(link);
            }
            catch(Exception) 
            {
                return "Error";
            }
        }

        void Alert_Log(Alert alert)
        {
            
            string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Alerts\" + System.DateTime.Now.ToString("yyyy-MM-dd");
            Create_Dir(docPath);

            // Write the string array to a new file named "Date.txt".
            using (StreamWriter outputFile = File.CreateText(docPath + @"\" + System.DateTime.Now.ToString("HH-mm-ss") + ".txt"))
            {
                outputFile.WriteLine("Alert ----------------------->" + System.DateTime.Now.ToString());
                outputFile.WriteLine("Title: " + alert.title);
                outputFile.WriteLine("Desc: " + alert.desc);
                outputFile.WriteLine("Data: ");
                foreach (string AlertData in alert.data)
                    outputFile.WriteLine(AlertData);
                
            }
        }

        //create dir
        void Create_Dir(string docPath)
        {
            if (!Directory.Exists(docPath))
            {
                Directory.CreateDirectory(docPath);
            }
        }
    }
}
// Alart link: https://www.oref.org.il/WarningMessages/alert/alerts.json
//Alert history link: https://www.oref.org.il/WarningMessages/History/AlertsHistory.json