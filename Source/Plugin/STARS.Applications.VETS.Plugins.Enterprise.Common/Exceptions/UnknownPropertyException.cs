using System;
using STARS.Applications.VETS.Plugins.Enterprise.Common.Properties;

namespace STARS.Applications.VETS.Plugins.Enterprise.Common.Exceptions
{
    public class UnknownPropertyException : Exception
    {
        public UnknownPropertyException(string propertyBagName, string propertyName)
        {
            PropertyBagName = propertyBagName;
            PropertyName = propertyName;
        }

        public override string Message
        {
            get
            {
                return string.Format(Properties.Resources.Error_PropertyNotFound, PropertyName,
                    PropertyBagName);
            }
        }

        public string PropertyBagName { get; private set; }

        public string PropertyName { get; private set; }
    }
}