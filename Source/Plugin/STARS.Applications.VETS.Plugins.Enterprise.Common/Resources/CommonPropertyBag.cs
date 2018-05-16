using System;
using System.Collections.Generic;
using System.Linq;
using STARS.Applications.VETS.Plugins.Enterprise.Common.Exceptions;

namespace STARS.Applications.VETS.Plugins.Enterprise.Common.Resources
{
    public class CommonPropertyBag : PropertyBag
    {
        private readonly Dictionary<string, object> _propertyBag = new Dictionary<string, object>();

        public CommonPropertyBag(object id) : base(id)
        {
        }

        public void SetProperty(string name, object value)
        {
            if (_propertyBag.ContainsKey(name))
            {
                _propertyBag[name] = value;
            }
            else
            {
                _propertyBag.Add(name, value);
            }

        }

        public override T GetProperty<T>(string name)
        {
            var value = GetProperty(name);

            if (value.IsNumericType())
                return (T)Convert.ChangeType(value, Type.GetTypeCode(typeof(T)));

            return (T)value;
        }

        public override object GetProperty(string name)
        {
            if (_propertyBag.ContainsKey(name))
            {
                return _propertyBag[name];
            }

            throw new UnknownPropertyException(GetType().ToString(), name);
        }

        public override IEnumerable<string> GetAllProperties()
        {
            return _propertyBag.Keys.ToList();
        }
    }
}
