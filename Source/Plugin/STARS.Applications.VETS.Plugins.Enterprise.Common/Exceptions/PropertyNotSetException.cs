using System;

namespace STARS.Applications.VETS.Plugins.Enterprise.Common.Exceptions
{
    public class PropertyNotSetException : Exception
    {
        public PropertyNotSetException(string propertyName, string propertyBagName)
            : base(string.Format(Properties.Resources.Error_PropertyNotSet, propertyName, propertyBagName))
        {
            PropertyName = propertyName;
            PropertyBagName = propertyBagName;
        }

        public string PropertyBagName { get; private set; }

        public string PropertyName { get; private set; }
    }
}
