using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace PPK.EmulatedMediaGuide
{
    /// <summary>
    /// Create a New INI file to store or load data
    /// </summary>
    public class IniFile
    {
        public string path;
        public string[] categories;
        public bool empty;
        public string[] exemptFromSanitization;
        public string logFileName;
        public string logFileDir;

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section,
            string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section,
                 string key, string def, StringBuilder retVal,
            int size, string filePath);

        /// <summary>
        /// INIFile Constructor.
        /// </summary>
        /// <PARAM name="INIPath"></PARAM>
        public IniFile(string INIPath)
        {
            path = INIPath;
            customInit();
            configLog("Initialized Config!");
        }

        internal void customInit()
        {
            categories = new string[0];
            exemptFromSanitization = new string[] { "Regex" };
            generateCategories();
        }

        internal string CurrentTimestamp(string formt = "Console")
        {
            switch (formt)
            {
                case "FileName":
                    return DateTime.Now.ToString("MM.dd.yyyy.HH.mm");
                default:
                    return DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
            }
        }

        internal void generateCategories()
        {
            configLog($"Checking for INI file...");
            if (!File.Exists(this.path))
            {
                configLog($"File does not exist! Creating...");
                FileStream fs = File.OpenWrite(this.path);
                fs.Flush();
                fs.Close();
                this.empty = true;
                configLog($"Empty Config Created!");
                return;
            }
            configLog($"INI File exists!");
            string[] iniFile;
            //Open the config as read-only
            configLog($"Opening config file with readonly priveledges...");
            using (FileStream fs = File.Open(this.path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                configLog($"Attaching streamreader...");
                using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                {
                    configLog($"Reading contents....");
                    string fileContents = sr.ReadToEnd();
                    configLog($"Closing streamreader...");
                    sr.Close();
                    configLog($"Splitting file on newlines...");
                    iniFile = fileContents.Split(
                        new string[] { Environment.NewLine },
                        StringSplitOptions.None
                    );
                }
                configLog($"Closing file....");
                fs.Close();
            }
            configLog($"Ensuring config file was loaded into memory...");
            //did we even create the string array?
            if (iniFile == null)
            {
                configLog($"Config file was not loaded properly... Exiting function...");
                //no? well i guess itll be empty then....
                this.empty = true;
                return;
            }
            configLog($"Checking if config was empty....");
            //are there any lines in it?
            if (iniFile.Length <= 0)
            {
                configLog($"Config is empty... Exiting function...");
                //no? well then its empty...
                this.empty = true;
                return;
            }
            configLog($"Parsing categories from config....");
            List<string> categoriesFound = new List<string>();
            //we found some text, time to parse it
            foreach (string settingLine in iniFile)
            {
                // Step 1: create new Regex.
                Regex regex = new Regex(@"\[[^\]]*\]");

                // Step 2: call Match on Regex instance.
                Match match = regex.Match(settingLine);

                // Step 3: test for Success.
                if (match.Success)
                {
                    configLog($"Found config category: {settingLine}");
                    string cleanedCategory = settingLine;
                    if (cleanedCategory.StartsWith("["))
                    {
                        configLog($"Cleaning leading square bracket from category name...");
                        cleanedCategory = cleanedCategory.Remove(0, 1);
                        configLog($"{settingLine} => {cleanedCategory}");
                    }
                    if (cleanedCategory.EndsWith("]"))
                    {
                        configLog($"Cleaning trailing square bracket from category name...");
                        cleanedCategory = cleanedCategory.Remove(cleanedCategory.Length - 1);
                        configLog($"{settingLine} => {cleanedCategory}");
                    }
                    configLog($"Adding category {cleanedCategory} to temporary list...");
                    categoriesFound.Add(cleanedCategory);
                }
            }
            configLog($"Converting temporary list to array and assigning to global vars...");
            this.categories = categoriesFound.ToArray();
            configLog($"Setting empty var...");
            this.empty = false;
            configLog($"Category generation complete!");
        }

        /// <summary>
        /// Write Data to the INI File
        /// </summary>
        /// <PARAM name="Section"></PARAM>
        /// Section name
        /// <PARAM name="Key"></PARAM>
        /// Key Name
        /// <PARAM name="Value"></PARAM>
        /// Value Name
        public void IniWriteValue(string Section, string Key, string Value)
        {
            configLog($"Preparing to save setting...");
            //were adding an item, so the config is no longer empty...
            this.empty = false;
            configLog($"Checking if category is in the global list...");
            //check if this category is already in our list...
            if (!this.categories.Contains(Section))
            {
                configLog($"Category doesn't exist! Preparing to update global categories list...");
                configLog($"Copying global list to temp variable...");
                //make a temp list to work with our data
                List<string> tmpCategories = this.categories.ToList();
                configLog($"Adding new category to temp variable...");
                //add our new category to our temp data
                tmpCategories.Add(Section);
                configLog($"Clearing old global list...");
                //clear the global array to be sure
                Array.Clear(this.categories, 0, this.categories.Length);
                configLog($"Assigning temp variable to global....");
                //push our temp data into the global array...
                this.categories = tmpCategories.ToArray();
            }

            configLog($"Writing setting to config...");
            //actually write the setting now...
            WritePrivateProfileString(Section, Key, Value, this.path);
            configLog($"Setting saved!");
        }

        /// <summary>
        /// Read Data Value From the Ini File
        /// </summary>
        /// <PARAM name="Section">Category to look for setting in</PARAM>
        /// <PARAM name="Key">Setting to find</PARAM>
        /// <PARAM name="defStr">Default value to use if key doesnt exist</PARAM>
        /// <PARAM name="saveDefStr">Do you want to save the default value in the config</PARAM>
        /// <returns>Setting value or default param passed with defStr</returns>
        public string IniReadValue(string Section, string Key, string defStr = null, bool saveDefStr = true)
        {
            configLog($"Preparing to retrieve value of {Key} from {Section}...");
            configLog($"Creating temp variable...");
            StringBuilder temp = new StringBuilder(5000);
            configLog($"Loading setting from config into temp variable...");
            int i = GetPrivateProfileString(Section, Key, "", temp,
                                            5000, this.path);
            configLog($"Setting loaded...");
            configLog($"The value of {Key} from {Section} is {temp.ToString()}");
            configLog($"Checking if setting value was empty...");
            if (string.IsNullOrEmpty(temp.ToString()))
            {
                configLog($"Setting is empty...");
                //setting wasnt found. Lets see if the default var was set...
                if (defStr != null)
                {
                    configLog($"Default value passed in... Parsing....");
                    //string is set, did we want to save that?
                    if (saveDefStr)
                    {
                        configLog($"Saving default value into config...");
                        IniWriteValue(Section, Key, defStr);
                    }
                }
                configLog($"Returning default setting...");
                return defStr;
            }
            configLog($"Returning value....");
            configLog($"Value: {temp.ToString()}");
            return temp.ToString();
        }


        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileSection(string lpAppName, byte[] lpszReturnBuffer, int nSize, string lpFileName);

        private Dictionary<string, string> GetKeysAndValues(string category)
        {
            byte[] buffer = new byte[2048];
            configLog($"Getting all keys and values for category {category}");
            GetPrivateProfileSection(category, buffer, 2048, this.path);
            String[] tmp = Encoding.ASCII.GetString(buffer).Trim('\0').Split('\0');

            configLog($"Creating temp variable to store keys and values...");
            Dictionary<string, string> result = new Dictionary<string, string>();
            configLog($"Looping through all settings in [{category}]");
            foreach (String entry in tmp)
            {
                result.Add(entry.Substring(0, entry.IndexOf("=")), entry.Substring(entry.IndexOf("=") + 1));
            }
            configLog($"Returning list...");
            return result;
        }

        public List<Tuple<string, string, string>> GetAllKeysAndValues()
        {
            configLog($"Getting all Keys and Values for ALL categories...");
            List<Tuple<string, string, string>> allSettings = new List<Tuple<string, string, string>>();
            configLog($"Looping through global category list...");
            foreach (string Category in categories)
            {
                byte[] buffer = new byte[2048];
                configLog($"Getting settings for [{Category}]");
                GetPrivateProfileSection(Category, buffer, 2048, this.path);
                String[] tmp = Encoding.ASCII.GetString(buffer).Trim('\0').Split('\0');
                configLog($"Looping through all pairs in [{Category}]");
                foreach (String entry in tmp)
                {
                    Tuple<string, string, string> toAdd = new Tuple<string, string, string>(Category, entry.Substring(0, entry.IndexOf("=")), entry.Substring(entry.IndexOf("=") + 1));
                    configLog($"Adding temp variable to list...");
                    allSettings.Add(toAdd);
                }
                configLog($"Parsing next category...");
            }
            configLog($"Returning data..");
            return allSettings;
        }

        /*internal void configLog(string message, bool enabled = false)
        {
            if (!enabled)
                return;

            string logPath = Path.Combine(Path.GetDirectoryName(this.path), "Logs", "configs");
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }
            Console.WriteLine(message);
            File.AppendAllText(Path.Combine(logPath, "log.txt"), message);
            File.AppendAllText(Path.Combine(logPath, "log.txt"), Environment.NewLine);
        }*/

        internal void loadDefaultLogSettings()
        {
            if (string.IsNullOrEmpty(logFileName))
            {
                logFileName = CurrentTimestamp("FileName") + ".log";
            }
            if (string.IsNullOrEmpty(logFileDir))
            {
                logFileDir = Path.Combine(Path.GetDirectoryName(this.path), "Logs", "configs");
            }
        }

        internal void configLog(string message, bool externalSource = false, bool enabled = true, bool useNewline = true)
        {
            if (!enabled)
                return;

            if (!externalSource)
                loadDefaultLogSettings();

            string logFile = Path.Combine(logFileDir, logFileName);

            if (!Directory.Exists(logFileDir))
            {
                Directory.CreateDirectory(logFileDir);
            }

            string messageFormat = String.Format("{0}: {1}", CurrentTimestamp(), message);
            if (useNewline)
                messageFormat = messageFormat + Environment.NewLine;

            File.AppendAllText(logFile, messageFormat);
            Console.WriteLine(messageFormat);
        }
    }
}