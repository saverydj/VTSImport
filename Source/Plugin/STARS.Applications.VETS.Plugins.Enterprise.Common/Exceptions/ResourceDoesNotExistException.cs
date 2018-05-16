using System;
using System.Collections.Generic;
using System.Linq;
using STARS.Applications.VETS.Plugins.Enterprise.Common.Repository;

namespace STARS.Applications.VETS.Plugins.Enterprise.Common.Exceptions
{
    public class ResourceDoesNotExistException : Exception
    {
        private readonly string _message;

        public ResourceDoesNotExistException(string resourceType, string property, string value)
        {
            _message = string.Format(Properties.Resources.Error_ResourceWithPropertyNotFound, resourceType,
                    property, value);
        }

        public ResourceDoesNotExistException(string resourceType, IEnumerable<ICondition> conditions)
        {
            _message = string.Format(Properties.Resources.Error_ResourceSatisfingConditionNotFound, resourceType,
                    string.Join(" & ", conditions.Select(c => c.ToString()).ToArray()));
        }

        public override string Message
        {
            get { return _message; }
        }
    }
}