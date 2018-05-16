using System.Collections.Generic;
using System.Xml.Serialization;

namespace STARS.Applications.VETS.Plugins.Enterprise.Common.Resources
{
    [XmlRoot("UnitsMappings")]
    public class UnitsMappingConfig
    {
        [XmlElement("UnitMapping")]
        public List<UnitsMapping> UnitMappings { get; set; }
    }

    public class UnitsMapping
    {
        [XmlElement("EnterpriseUnit")]
        public string EnterpriseUnit { get; set; }

        [XmlElement("VETSUnit")]
        public string VetsUnit { get; set; }
    }
}
