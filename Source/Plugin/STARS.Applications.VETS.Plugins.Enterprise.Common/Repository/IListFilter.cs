using System.Collections.Generic;

namespace STARS.Applications.VETS.Plugins.Enterprise.Common.Repository
{
    public interface IListFilter
    {
        IEnumerable<ICondition> GetSimpleConditions(IRepository repository, string type, IEnumerable<ICondition> conditions);
    }
}
