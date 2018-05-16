using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STARS.Applications.VETS.Plugins.VTS.Interface
{
    public class Field
    {
        public string VtsTag { get; private set; }
        public string VetsTag { get; private set; }
        public string ResourceType { get; private set; }
        public string FieldValue { get; set; }

        public Field(string vts, string vets, string resource)
        {
            VtsTag = vts;
            VetsTag = vets;
            ResourceType = resource;
            FieldValue = String.Empty;
        }
    }
}
