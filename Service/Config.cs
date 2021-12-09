using Microsoft.Win32;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

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

        public Config(EventLog eventLog) {
            this.eventLog = eventLog;
            this.settingsFile = new IniFile(Path.Combine(this.DataPath, "config.ini"));
            this.logFileName = DateTime.Now.ToString("MM.dd.yyyy.HH.mm") + ".log";
            Read();
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
        }

        public string DataPath
        {
            get
            {
                return System.AppDomain.CurrentDomain.BaseDirectory;
                //return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            }
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
    }
}
