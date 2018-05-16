using System.IO;
using System.Xml.Serialization;
using Stars;

namespace STARS.Applications.VETS.Plugins.Enterprise.Common
{
    public class ConfigLoader
    {
        public static T ReadConfig<T>(string fileName)
        {
            var path = string.Format(@"{0}\Import\{1}.xml", AppDataPath.GetAppDataPath(),
                fileName);

            using (var reader = new StreamReader(path))
            {
                var xs = new XmlSerializer(typeof(T));
                return (T)xs.Deserialize(reader);
            }
        }
    }
}
