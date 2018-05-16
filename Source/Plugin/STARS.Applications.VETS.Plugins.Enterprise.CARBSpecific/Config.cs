using System;
using System.Configuration;
using System.IO;
using System.Security.Permissions;

namespace STARS.Applications.VETS.Plugins.VTS.Interface
{
    public static class Config
    {
        public static string SharedFolder { get; private set; }
        public static string TempFolder { get; private set; }
        public static string ConfigFolder { get; private set; }
        public static string VtsSchedule { get; private set; }
        public static string VtsFeedBack { get; private set; }
        public static string FieldMap { get; private set; }
        public static string FieldThresholds { get; private set; }
        public static string UniqueFieldModifications { get; private set; }
        public static string ValidTestNames { get; private set; }
        public static string VtsFormattedData { get; private set; }
        public static string VtsRepository { get; private set; }
        public static string TestResourceName { get; private set; }
        public static string FuelResourceName { get; private set; }
        public static string VehicleResourceName { get; private set; }
        public static string SamplingConfigurationResourceName { get; private set; }
        public static string SampleLineConfigurationResourceName { get; private set; }
        public static string IOChannelsSetupResourceName { get; private set; }
        public static bool CheckThresholdValues { get; private set; }
        public static bool InformUserOfFieldsOutsideThreshold { get; private set; }
        public static bool InformUserOfInvalidTests { get; private set; }
        public static bool CrossReferencePreconditionOnTestRun { get; private set; }
        public static string NullValueKey { get; private set; }    
        public static string KeepOldValueKey { get; private set; }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        static Config()
        {
            string path = typeof(Config).Assembly.Location + ".config";
            string dir = Path.GetDirectoryName(path);
            string file = Path.GetFileName(path);

            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = dir;
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.Filter = file;
            watcher.Changed += new FileSystemEventHandler(OnAppConfigChanged);
            watcher.EnableRaisingEvents = true;

            UpdateFields();
        }

        private static void OnAppConfigChanged(object source, FileSystemEventArgs e)
        {
            try
            {
                UpdateFields();
            }
            catch(Exception ex)
            {
                if (!ex.Message.Contains("being used by another process"))
                {
                    throw ex;
                }
            }
        }

        private static void UpdateFields()
        {
            SharedFolder = FormatPath(AppConfig("SharedFolder"));
            TempFolder = FormatPath(AppConfig("TempFolder"));
            ConfigFolder = SharedFolder + FormatPath(AppConfig("ConfigFolder"));
            VtsSchedule = SharedFolder + AppConfig("VtsSchedule");
            VtsFeedBack = SharedFolder + AppConfig("VtsFeedBack");
            FieldMap = ConfigFolder + AppConfig("FieldMap");
            FieldThresholds = ConfigFolder + AppConfig("FieldThresholds");
            UniqueFieldModifications = ConfigFolder + AppConfig("UniqueFieldModifications");
            ValidTestNames = ConfigFolder + AppConfig("ValidTestNames");
            VtsFormattedData = TempFolder + AppConfig("VtsFormattedData");
            VtsRepository = TempFolder + AppConfig("VtsRepository");
            TestResourceName = AppConfig("TestResourceName");
            FuelResourceName = AppConfig("FuelResourceName");
            VehicleResourceName = AppConfig("VehicleResourceName");
            SamplingConfigurationResourceName = AppConfig("SamplingConfigurationResourceName");
            SampleLineConfigurationResourceName = AppConfig("SampleLineConfigurationResourceName");
            IOChannelsSetupResourceName = AppConfig("IOChannelsSetupResourceName");
            CheckThresholdValues = TypeCast.ToBool(AppConfig("CheckThresholdValues"));
            InformUserOfFieldsOutsideThreshold = TypeCast.ToBool(AppConfig("InformUserOfFieldsOutsideThreshold"));
            InformUserOfInvalidTests = TypeCast.ToBool(AppConfig("InformUserOfInvalidTests"));
            CrossReferencePreconditionOnTestRun = TypeCast.ToBool(AppConfig("CrossReferencePreconditionOnTestRun"));
            NullValueKey = AppConfig("NullValueKey");
            KeepOldValueKey = AppConfig("KeepOldValueKey");
        }

        private static string FormatPath(string path)
        {
            if(!path.EndsWith(@"\"))
            {
                return path + @"\";
            }
            return path;
        }

        private static string AppConfig(string key)
        {
            Configuration config = null;
            string exeConfigPath = typeof(Config).Assembly.Location;
            config = ConfigurationManager.OpenExeConfiguration(exeConfigPath);
            if (config == null || config.AppSettings.Settings.Count == 0)
            {
                throw new Exception(String.Format("Config file {0}.config is missing or could not be loaded.", exeConfigPath));
            }

            KeyValueConfigurationElement element = config.AppSettings.Settings[key];
            if (element != null)
            {
                string value = element.Value;
                if (!string.IsNullOrEmpty(value))
                    return value;
            }             
            return string.Empty; ;
        }

    }
}
