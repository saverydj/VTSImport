using System;
using System.Collections.Generic;
using System.Linq;
using STARS.Applications.VETS.Plugins.Enterprise.Common.Exceptions;

namespace STARS.Applications.VETS.Plugins.Enterprise.Common.Resources
{
    public abstract class PropertyBag
    {
        private readonly object _id;

        protected PropertyBag(object id)
        {
            _id = id;
        }

        public object Id
        {
            get { return _id; }
        }

        public virtual T GetProperty<T>(string name)
        {
            var type = GetType();
            try
            {
                var value = type.GetProperty(name).GetValue(this, null);
                if (value.IsNumericType())
                    return (T) Convert.ChangeType(value, Type.GetTypeCode(typeof(T)));
                return (T) value;
            }
            catch (NullReferenceException)
            {
                throw new UnknownPropertyException(type.ToString(), name);

            }
        }

        public virtual object GetProperty(string name)
        {
            return GetType().GetProperty(name).GetValue(this, null);
        }

        public virtual IEnumerable<string> GetAllProperties()
        {
            return GetType().GetProperties().Select(p => p.Name).ToList();
        }

        public virtual bool HasProperty(string name)
        {
            return GetType().GetProperty(name) == null;
        }

        public override string ToString()
        {
            var type = GetType().ToString();
            string name = Properties.Resources.NA;
            if (HasProperty(Constants.Type))
                type = GetProperty<string>(Constants.Type);
            if (HasProperty(Constants.DisplayName))
                name = GetProperty<string>(Constants.DisplayName);
            else if (HasProperty(Constants.Name))
                name = GetProperty<string>(Constants.Name);

            return string.Format("{0} ({1})", name, type);
        }
    }
}
