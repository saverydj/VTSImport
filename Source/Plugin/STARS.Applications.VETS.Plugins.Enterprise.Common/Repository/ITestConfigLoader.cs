using System.Collections.Generic;

namespace STARS.Applications.VETS.Plugins.Enterprise.Common.Repository
{
    public interface ITestConfigLoader
    {
        IEnumerable<ICondition> GetSimpleConditions(IRepository repository, string type, IEnumerable<ICondition> conditions);
    }
}
