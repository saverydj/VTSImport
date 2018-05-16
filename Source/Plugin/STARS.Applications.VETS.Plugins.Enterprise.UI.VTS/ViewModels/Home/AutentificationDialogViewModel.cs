using Caliburn.PresentationFramework;
using STARS.Applications.Interfaces.ViewModels.PropertyEditors;
using STARS.Applications.VETS.UI.ViewModels.PropertyEditors;

namespace STARS.Applications.VETS.Plugins.Enterprise.UI.VTS.ViewModels.Home
{
    class AutentificationDialogViewModel : PropertyChangedBase
    {
        public AutentificationDialogViewModel(bool firstAttempt)
        {
            User = new ObservableValue<string>();
            Password = new ObservableValue<string>();
            ShowInvalidCredentialsMessage = !firstAttempt;
        }

        public IObservableValue<string> User { get; private set; }

        public IObservableValue<string> Password { get; private set; }

        public bool ShowInvalidCredentialsMessage { get; private set; }
    }
}
