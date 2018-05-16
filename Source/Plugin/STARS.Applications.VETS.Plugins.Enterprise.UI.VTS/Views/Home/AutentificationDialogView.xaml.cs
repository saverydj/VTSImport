using System.Windows;
using System.Windows.Input;
using STARS.Applications.VETS.Plugins.Enterprise.UI.VTS.ViewModels.Home;

namespace STARS.Applications.VETS.Plugins.Enterprise.UI.VTS.Views.Home
{
    /// <summary>
    /// Interaction logic for AutentificationDialog.xaml
    /// </summary>
    public partial class AutentificationDialogView
    {
        public AutentificationDialogView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(UserTextBox);
            Loaded -= OnLoaded;
        }


        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            ((AutentificationDialogViewModel) DataContext).Password.Value = PasswordBox.Password;
        }
    }
}
