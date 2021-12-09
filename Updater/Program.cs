using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Updater
{
    internal class Program
    {
        static IniFile config;
        static string thisFolder;
        static string parentFolder;

        static void Main(string[] args)
        {
            thisFolder = AppDomain.CurrentDomain.BaseDirectory;
            parentFolder = thisFolder.Replace("\\update", "");
            if (!File.Exists(Path.Combine(thisFolder, "config.ini")))
            {
                Console.WriteLine("Config not found! Writing Defaults!");
            }
            config = new IniFile(Path.Combine(thisFolder, "config.ini"));
            writeDefaultSettings();
            ServiceController sc = new ServiceController();
            sc.ServiceName = "EmulatedMediaGuide";
            Console.WriteLine("The Alerter service status is currently set to {0}", sc.Status.ToString());
            if (sc.Status == ServiceControllerStatus.Running)
            {
                try
                {
                    Console.WriteLine("Stopping the EmulatedMediaGuide service...");
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    Console.WriteLine("The EmulatedMediaGuide service status is now set to {0}.", sc.Status.ToString());
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine("Could not stop the EmulatedMediaGuide service.");
                }
            }
            findFilesToDelete();
            if (sc.Status == ServiceControllerStatus.Stopped)
            {
                try
                {
                    Console.WriteLine("Starting the EmulatedMediaGuide service...");
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                    Console.WriteLine("The EmulatedMediaGuide service status is now set to {0}.", sc.Status.ToString());
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine("Could not restart the EmulatedMediaGuide service.");
                }
            }
            Console.ReadLine();
        }

        static void writeDefaultSettings()
        {
            config.IniReadValue("Service", "DataDir", parentFolder);
            config.IniReadValue("FileManagement", "EmulatedMediaGuide.exe", "replace");
            config.IniReadValue("FileManagement", "config.ini", "keep");
            config.IniReadValue("FileManagement", "EmulatedMediaGuide.exe.config", "keep");
            config.IniReadValue("FileManagement", "EmulatedMediaGuide.InstallLog", "keep");
            config.IniReadValue("FileManagement", "EmulatedMediaGuide.InstallState", "keep");
            config.IniReadValue("FileManagement", "InstallUtil.InstallLog", "keep");
            config.IniReadValue("FileManagement", "Newtonsoft.Json.dll", "replace");
            config.IniReadValue("FileManagement", "System.IO.Compression.dll", "replace");
            config.IniReadValue("FileManagement", "System.IO.Compression.ZipFile.dll", "replace");
            config.IniReadValue("FileManagement", "uhttpsharp.dll", "replace");
            config.IniReadValue("FileManagement", "Microsoft.CSharp.dll", "replace");
            config.IniReadValue("FileManagement", "System.Configuration.Install.dll", "replace");
            config.IniReadValue("FileManagement", "System.Data.DataSetExtensions.dll", "replace");
            config.IniReadValue("FileManagement", "System.Data.dll", "replace");
            config.IniReadValue("FileManagement", "System.Drawing.dll", "replace");
            config.IniReadValue("FileManagement", "System.Management.dll", "replace");
            config.IniReadValue("FileManagement", "System.Net.Http.dll", "replace");
            config.IniReadValue("FileManagement", "System.ServiceProcess.dll", "replace");
            config.IniReadValue("FileManagement", "System.Xml.dll", "replace");
            config.IniReadValue("FileManagement", "System.Xml.Linq.dll", "replace");
        }

        static void findFilesToDelete()
        {
            config = new IniFile(Path.Combine(thisFolder, "config.ini"));
            string ServiceDataDir = config.IniReadValue("Service", "DataDir", "../");
            Dictionary<string,string> fileConfig = config.GetKeysAndValues("FileManagement");
            foreach (KeyValuePair<string,string> kvp in fileConfig)
            {
                string oldFile = Path.Combine(ServiceDataDir, kvp.Key);
                string filToCopy = Path.Combine(thisFolder, kvp.Key);
                switch (kvp.Value)
                {
                    case "replace":
                        //delete the old one
                        if (File.Exists(oldFile))
                        {
                            Console.WriteLine($"Deleted {oldFile}");
                            File.Delete(oldFile);
                        }
                        //copy new file to datapath
                        if (File.Exists(filToCopy))
                        {
                            Console.WriteLine($"Copied {filToCopy} to {oldFile}");
                            File.Copy(filToCopy, oldFile);
                        }
                        break;
                    case "delete":
                        if (File.Exists(oldFile))
                        {
                            Console.WriteLine($"Deleted {oldFile}");
                            File.Delete(oldFile);
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
