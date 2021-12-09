using EmulatedMediaGuide.Handlers;
using Microsoft.Win32;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;

namespace EmulatedMediaGuide
{
    class Config
    {
        private readonly EventLog eventLog;
        private IniFile settingsFile;
        private string ipAddress;
        private int port;
        private int startChannel;
        private string filter;
        private string m3uUrl;
        private string epgUrl;
        private string logoFontFamily;
        private string logoColor;
        private string logoBackground;
        private int gapFillAmount;
        private string gapFillTitle;
        private string logDir;
        private bool textLoggingEnabled;
        private string logFileName;
        private bool autoUpdate;

        public Config(EventLog eventLog) {
            this.eventLog = eventLog;
            this.settingsFile = new IniFile(Path.Combine(this.DataPath, "config.ini"));
            this.logFileName = DateTime.Now.ToString("MM.dd.yyyy.HH.mm") + ".log";
            Read();
            if (checkUpdate())
            {
                if (Directory.Exists(Path.Combine(this.DataPath, "update")))
                {
                    Directory.Delete(Path.Combine(this.DataPath, "update"), true);
                }
                downloadUpdateInBackground();
            }
            //checkStoredSetting();
        }

        internal void checkStoredSetting()
        {
            settingsFile.configLog(string.Format("[{0}]", "Server"), true);
            settingsFile.configLog(string.Format("{0}={1}", "IpAddress", this.ipAddress), true);
            settingsFile.configLog(string.Format("{0}={1}", "Port", this.port), true);
            settingsFile.configLog(string.Format("{0}={1}", "StartChannel", this.startChannel), true);
            settingsFile.configLog(string.Format("[{0}]", "Style"), true);
            settingsFile.configLog(string.Format("{0}={1}", "LogoFontFamily", this.logoFontFamily), true);
            settingsFile.configLog(string.Format("{0}={1}", "LogoColor", this.logoColor), true);
            settingsFile.configLog(string.Format("{0}={1}", "LogoBackground", this.logoBackground), true);
            settingsFile.configLog(string.Format("{0}={1}", "GapFillAmount", this.gapFillAmount), true);
            settingsFile.configLog(string.Format("{0}={1}", "GapFillTitle", this.gapFillTitle), true);
            settingsFile.configLog(string.Format("[{0}]", "Logging"), true);
            settingsFile.configLog(string.Format("{0}={1}", "LogFolder", this.logDir), true);
            settingsFile.configLog(string.Format("{0}={1}", "Enabled", this.textLoggingEnabled), true);
            settingsFile.configLog(string.Format("[{0}]", "Data"), true);
            settingsFile.configLog(string.Format("{0}={1}", "m3uUrl", this.m3uUrl), true);
            settingsFile.configLog(string.Format("{0}={1}", "epgUrl", this.epgUrl), true);
            settingsFile.configLog(string.Format("[{0}]", "Regex"), true);
            settingsFile.configLog(string.Format("{0}={1}", "Filter", this.filter), true);
            settingsFile.configLog(string.Format("[{0}]", "Updates"), true);
            settingsFile.configLog(string.Format("{0}={1}", "AutoUpdate", this.autoUpdate), true);
        }

        public void Read()
        {
            // Read all values from config. Will automatically set variables if empty...
            this.ipAddress = settingsFile.IniReadValue("Server", "IpAddress", "127.0.0.1");
            this.port = int.Parse(settingsFile.IniReadValue("Server", "Port", "6079"));
            this.startChannel = int.Parse(settingsFile.IniReadValue("Server", "StartChannel", "1"));
            this.logoFontFamily = settingsFile.IniReadValue("Style", "LogoFontFamily", "Segoe UI");
            this.logoColor = settingsFile.IniReadValue("Style", "LogoColor", "#DCDCDC");
            this.logoBackground = settingsFile.IniReadValue("Style", "LogoBackground", "0x1");
            this.gapFillAmount = int.Parse(settingsFile.IniReadValue("Style", "GapFillAmount", "0"));
            this.gapFillTitle = settingsFile.IniReadValue("Style", "GapFillTitle", "Unknown Airing");
            this.logDir = settingsFile.IniReadValue("Logging", "LogFolder", Path.Combine(this.DataPath, "logs"));
            this.textLoggingEnabled = bool.Parse(settingsFile.IniReadValue("Logging", "Enabled", "True"));
            this.m3uUrl = settingsFile.IniReadValue("Data", "m3uUrl", "http://yourwebsite.com/emuguide/yourcustomlineup.m3u");
            this.epgUrl = settingsFile.IniReadValue("Data", "epgUrl", "https://yourwebsite.com/emuguide/yourcustomlineup.xml");
            this.filter = settingsFile.IniReadValue("Regex", "Filter", ".*");
            this.autoUpdate = bool.Parse(settingsFile.IniReadValue("Updates", "AutoUpdate", "true"));
        }

        public string DataPath
        {
            get
            {
                return System.AppDomain.CurrentDomain.BaseDirectory;
                //return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            }
        }

        public string updateUrl
        {
            get
            {
                return "https://github.com/Gigawiz/Emulated-Media-Guide/releases/latest/download/latest.zip";
            }
        }

        public string internetVersion
        {
            get
            {
                return "https://raw.githubusercontent.com/Gigawiz/Emulated-Media-Guide/main/Resources/Updates/version.txt";
            }
        }

        public int[] programVersion
        {
            get
            {
                return new int[] { 1, 0, 2, 2 };
            }
        }

        public bool AutoUpdate
        {
            get { return this.autoUpdate; }
        }

        public string IpAddress
        {
            get { return this.ipAddress; }
        }

        public int Port
        {
            get { return this.port; }
        }

        public string M3UURL
        {
            get { return this.m3uUrl; }
        }

        public string EPGURL
        {
            get { return this.epgUrl; }
        }

        public string Filter
        {
            get { return this.filter; }
        }

        public int StartChannel
        {
            get { return this.startChannel; }
        }

        public int MaxChannels
        {
            get { return 420; }
        }

        public string LogoFontFamily
        {
            get { return this.logoFontFamily; }
        }

        public string LogoColor
        {
            get { return this.logoColor; }
        }

        public string LogoBackground
        {
            get { return this.logoBackground; }
        }

        public int GapFillAmount
        {
            get { return this.gapFillAmount; }
        }

        public string GapFillTitle
        {
            get { return this.gapFillTitle; }
        }

        public string LogDirectory
        {
            get { return this.logDir; }
        }

        public bool TextLoggingEnabled
        {
            get { return this.textLoggingEnabled; }
        }

        public string LogFileName
        {
            get { return this.logFileName; }
        }

        public List<Tuple<string,string,string>> getAllSettings
        {
            get { return this.settingsFile.GetAllKeysAndValues(); }
        }

        /**
         * Write an entry in the event log and console if available. 
         */
        public void WriteLog(bool error, string format, params object[] args)
        {
            var message = String.Format(format, args);
            Console.WriteLine(message);
            if (eventLog != null)
            {
                eventLog.WriteEntry(message, error ? EventLogEntryType.Error : EventLogEntryType.Information);
            }
            TextLog(message);
        }

        public void TextLog(string message)
        {
            this.settingsFile.logFileDir = this.logDir;
            this.settingsFile.logFileName = LogFileName;
            this.settingsFile.configLog(message, true, this.textLoggingEnabled);
        } 

        /**
         * Format a URL that points to the IPTV HTTP server. 
         */
        public string ServerUrl(string path = "")
        {
            return String.Format("http://{0}:{1}{2}", IpAddress, Port, path);
        }

        private static string ToLiteral(string input)
        {
            using (var writer = new StringWriter())
            {
                using (var provider = CodeDomProvider.CreateProvider("CSharp"))
                {
                    provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, null);
                    return writer.ToString();
                }
            }
        }

        public void downloadUpdateInBackground()
        {
            WebClient client = new WebClient();
            string updateDiskLocation = Path.Combine(this.DataPath, "latest.zip");
            client.DownloadProgressChanged += Client_DownloadProgressChanged;
            client.DownloadFileCompleted += Client_DownloadFileCompleted;
            client.DownloadFileAsync(new Uri(updateUrl), updateDiskLocation);
        }

        private void Client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            //update downloaded!
            TextLog($"Update file downloaded!");
            if (AutoUpdate)
            {
                TextLog($"Preparing server update!");
                string zipPath = Path.Combine(this.DataPath, "latest.zip");
                string extractPath = Path.Combine(this.DataPath, "update");

                if (!Directory.Exists(extractPath))
                {
                    Directory.CreateDirectory(extractPath);
                }

                using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                {
                    var totalZipEntries = archive.Entries.Count;
                    var completedZipEntries = 0;
                    foreach (var entry in archive.Entries)
                    {
                        // Gets the full path to ensure that relative segments are removed.
                        string destinationPath = Path.GetFullPath(Path.Combine(extractPath, entry.FullName));

                        // Ordinal match is safest, case-sensitive volumes can be mounted within volumes that
                        // are case-insensitive.
                        if (destinationPath.StartsWith(extractPath, StringComparison.Ordinal))
                            entry.ExtractToFile(destinationPath);


                        // update progess there
                        completedZipEntries++;

                        TextLog($"Update file extracting...");
                        TextLog(extractionProgress(totalZipEntries, completedZipEntries));
                    }
                }
                //extraction complete. Time for the restart!
                TextLog($"Update file extracted! Restarting to apply update!");
                Process.Start(Path.Combine(this.DataPath, "update", "Updater.exe"));
            }
        }

        private string extractionProgress(int totalEntries, int processedEntries)
        {
            string progress = "0";
            bool error = false;
            try
            {
                progress = (processedEntries * 100.0 / totalEntries).ToString();
            }
            catch (Exception ex)
            {
                progress = "Error calculating progress!";
            }
            return string.Format((error ?  "{0}" : "{0}% complete"), progress);
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage >= 1)
            {
                if (e.ProgressPercentage % 10 == 0)
                {
                    TextLog($"Update file downloading... {e.ProgressPercentage}% complete...");
                }
            }
        }

        public bool checkUpdate()
        {
            int[] webVer = webVersion();
            int i = 0;
            foreach (int webVerInt in webVer)
            {
                if (webVerInt > programVersion[i])
                {
                    return true;
                }
                i++;
            }
            return false;
        }

        private int[] webVersion()
        {
            List<int> retTmp = new List<int>();
            WebClient client = new WebClient();
            string reply = client.DownloadString(internetVersion);
            
            foreach (string replyStr in reply.Split('.'))
            {
                retTmp.Add(int.Parse(replyStr));
            }
            return retTmp.ToArray();
        }
    }
}
