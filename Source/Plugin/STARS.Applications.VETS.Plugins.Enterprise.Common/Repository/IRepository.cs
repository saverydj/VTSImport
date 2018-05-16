using System.Collections.Generic;
using STARS.Applications.VETS.Plugins.Enterprise.Common.Exceptions;
using STARS.Applications.VETS.Plugins.Enterprise.Common.Resources;

namespace STARS.Applications.VETS.Plugins.Enterprise.Common.Repository
{
    public interface IRepository
    {
        RepositoryException LastException { get; }

        IEnumerable<IListItem> GetList(string type, IList<ICondition> conditions = null);

        IEnumerable<PropertyBag> GetResources(string type, IList<ICondition> conditions = null);

        PropertyBag GetFirstResource(string type, IList<ICondition> conditions);

        PropertyBag GetResource(IListItem item, bool fullTree = false, bool useCache = true);

        PropertyBag GetResource(string type, object id, bool fullTree = false, bool useCache = true);

        IEnumerable<KeyValuePair<string, string>> GetUnits(string type);
    }
}
