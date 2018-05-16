using STARS.Applications.VETS.Interfaces.Entities;
using STARS.Applications.VETS.Plugins.Enterprise.Common.Resources;

namespace STARS.Applications.VETS.Plugins.Enterprise.Common
{
    public interface ITestSetup
    {
        void SetupTest(Test vetsTest, PropertyBag enterpriseTest);
    }
}
