using System;
using System.Collections.Generic;
using STARS.Applications.VETS.Plugins.Enterprise.Common.Repository;
using STARS.Applications.VETS.Plugins.Enterprise.Common.Resources;

namespace STARS.Applications.VETS.Plugins.Enterprise.Common
{
    /// <summary>
    /// Enterprise test executions service interface
    /// </summary>
    public interface IImportManager
    {
        IDefaultFilters DefaultFilters { get; }
        
        void GetTestList(IEnumerable<ICondition> filters, Action<IEnumerable<string>> callback = null);

        IEnumerable<string> GetOrders(IEnumerable<ICondition> filters);

        void GetTest(string testName, bool useCache, Action<PropertyBag> callback = null);
        
        bool CanRunTest();
        
        void RunTest(string testName);

        string GetLastError();
    }
}
