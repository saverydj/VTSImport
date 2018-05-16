using System.Collections.Generic;
using STARS.Applications.VETS.Plugins.Enterprise.Common.Repository;

namespace STARS.Applications.VETS.Plugins.Enterprise.Common
{
    public interface IDefaultFilters
    {
        IEnumerable<ICondition> Conditions { get; }
    }
}
