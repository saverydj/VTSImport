using System.Xml.Serialization;

namespace STARS.Applications.VETS.Plugins.Enterprise.Common
{
    public class ImportConfig
    {
        public string TestName { get; set; }

        public string FacilityName { get; set; }

        public string Uri { get; set; }

        public bool IsDebugAreaVisible { get; set; }

        public bool AreFiltersVisible { get; set; }

        [XmlIgnore]
        public static ImportConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (Sync)
                    {
                        if (_instance == null)
                        {
                            _instance = ConfigLoader.ReadConfig<ImportConfig>("ImportConfig");
                        }
                    }
                }

                return _instance;
            }
        }
        
        [XmlIgnore] private static volatile ImportConfig _instance;
        [XmlIgnore] private static readonly object Sync = new object();
    }
}
