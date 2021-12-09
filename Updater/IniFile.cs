using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Updater
{
    /// <summary>
    /// Create a New INI file to store or load data
    /// </summary>
    public class IniFile
    {
        public string path;

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
            //actually write the setting now...
            WritePrivateProfileString(Section, Key, Value, this.path);
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
            StringBuilder temp = new StringBuilder(5000);
            int i = GetPrivateProfileString(Section, Key, "", temp,
                                            5000, this.path);
            if (string.IsNullOrEmpty(temp.ToString()))
            {
                //setting wasnt found. Lets see if the default var was set...
                if (defStr != null)
                {
                    //string is set, did we want to save that?
                    if (saveDefStr)
                    {
                        IniWriteValue(Section, Key, defStr);
                    }
                }
                return defStr;
            }
            return temp.ToString();
        }


        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileSection(string lpAppName, byte[] lpszReturnBuffer, int nSize, string lpFileName);

        public Dictionary<string, string> GetKeysAndValues(string category)
        {
            byte[] buffer = new byte[2048];
            GetPrivateProfileSection(category, buffer, 2048, this.path);
            String[] tmp = Encoding.ASCII.GetString(buffer).Trim('\0').Split('\0');
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (String entry in tmp)
            {
                result.Add(entry.Substring(0, entry.IndexOf("=")), entry.Substring(entry.IndexOf("=") + 1));
            }
            return result;
        }
    }
}