using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Caliburn.PresentationFramework;
using STARS.Applications.Interfaces;
using STARS.Applications.Interfaces.ViewModels;
using STARS.Applications.Interfaces.ViewModels.PropertyEditors;
using STARS.Applications.UI.Common;
using STARS.Applications.VETS.Interfaces;
using STARS.Applications.VETS.Plugins.Enterprise.Common;
using STARS.Applications.VETS.Plugins.Enterprise.Common.Properties;
using STARS.Applications.VETS.Plugins.Enterprise.Common.Resources;
using STARS.Applications.VETS.Plugins.VTS.UI.Properties;
using STARS.Applications.VETS.Plugins.VTS.UI.ViewModels;
using STARS.Applications.VETS.Plugins.VTS.Interface;
using STARS.Applications.VETS.Plugins.VTS.UI.ViewModels.Home;
using System.Windows;

namespace STARS.Applications.VETS.Plugins.VTS.UI.Command
{
    /// <summary>
    /// Implementation of StartTest button
    /// </summary>
    internal class ControlSaveAsCommand : ICommandViewModel
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ControlSaveAsCommand(IImageManager imageManager, IObservableValue<SingleTest> selectedTest, HomeViewModel homeView)
        {
            DisplayInfo = new DisplayInfo
            {
                Description = Resources.SaveAsNewResources,
                Image16 = imageManager.GetImage16Path("Save")
            };

            Command = new RelayCommand(_ =>
                 DoControlSaveAs(homeView),
                _ => (selectedTest.Value != null));
        }

        private static void DoControlSaveAs(HomeViewModel homeView)
        {
            if (homeView.IsOverlayVisible == Visibility.Visible)
            {
                homeView.IsOverlayVisible = Visibility.Hidden;
                homeView.NewTestName = null;
                homeView.NewFuelName = null;
                homeView.NewVehicleName = null;
                homeView.NewTestEnabled = true;
                homeView.NewFuelEnabled = true;
                homeView.NewVehicleEnabled = true;
            }
            else
            {
                homeView.IsOverlayVisible = Visibility.Visible;
                if (homeView.SelectedTest != null)
                {
                    homeView.NewTestName = "VTS Test " + homeView.SelectedTest.Value.TestIDNumber.ToString();
                    homeView.NewFuelName = "VTS Fuel " + homeView.SelectedTest.Value.TestIDNumber.ToString();
                    homeView.NewVehicleName = "VTS Vehicle " + homeView.SelectedTest.Value.TestIDNumber.ToString();
                }
                else
                {
                    homeView.NewTestName = null;
                    homeView.NewFuelName = null;
                    homeView.NewVehicleName = null;
                }
                homeView.NewTestEnabled = true;
                homeView.NewFuelEnabled = true;
                homeView.NewVehicleEnabled = true;
            }
        }

        public DisplayInfo DisplayInfo { get; private set; }

        public string DisplayName
        {
            get { return Resources.SaveAsNewResources; }
        }

        public ICommand Command { get; private set; }
    }
}