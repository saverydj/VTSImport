using STARS.Applications.VETS.Plugins.Enterprise.Common.Repository;

namespace STARS.Applications.VETS.Plugins.Enterprise.Common
{
    public interface IUiLoginService
    {
        bool Login(ILogonToRepository repository, bool firstAttempt = true);
    }
}
