using STARS.Applications.VETS.Plugins.Enterprise.Common.Exceptions;

namespace STARS.Applications.VETS.Plugins.Enterprise.Common.Repository
{
    public interface ILogonToRepository
    {
        bool IsLoggedIn { get; }

        RepositoryException LastException { get; }

        bool Logon(string user, string password);

        bool LoginRefresh(string oldToken);
    }
}
