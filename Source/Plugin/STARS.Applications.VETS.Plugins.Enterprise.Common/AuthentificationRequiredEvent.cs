using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Events;
using STARS.Applications.VETS.Plugins.Enterprise.Common.Repository;

namespace STARS.Applications.VETS.Plugins.Enterprise.Common
{
    [Export(EventIDs.AuthentificationRequired, typeof(ICompositePresentationEvent<AuthentificationDetails>))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class AuthentificationRequiredEvent : CompositePresentationEvent<AuthentificationDetails>
    {
    }

    public class AuthentificationDetails
    {
        private readonly ILogonToRepository _logonToRepository;

        public AuthentificationDetails(ILogonToRepository logonToRepository)
        {
            _logonToRepository = logonToRepository;
        }

        public string User { get; set; }

        public string Password { get; set; }
        
        public bool Logon()
        {
            return _logonToRepository.Logon(User, Password);
        }
    }

    public class EventIDs
    {
        public const string AuthentificationRequired = "AuthentificationRequired";
    }
}
