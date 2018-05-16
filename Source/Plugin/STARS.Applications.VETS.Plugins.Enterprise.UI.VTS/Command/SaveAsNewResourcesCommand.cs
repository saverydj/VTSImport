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

namespace STARS.Applications.VETS.Plugins.VTS.UI.Command
{
    /// <summary>
    /// Implementation of StartTest button
    /// </summary>
    internal class SaveAsNewResourcesCommand : ICommandViewModel
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SaveAsNewResourcesCommand(IImageManager imageManager, IObservableValue<SingleTest> selectedTest, HomeViewModel homeView, 
            ICommandViewModel showTestPropertiesCommand, XMLRepository xmlRepository)
        {
            DisplayInfo = new DisplayInfo
            {
                Description = Resources.SaveAsNewResources,
                Image16 = imageManager.GetImage16Path("Save")
            };

            Command = new RelayCommand(_ =>
                 DoSaveAsNewResources(showTestPropertiesCommand, xmlRepository, homeView),
                _ => (selectedTest.Value != null));
        }

        private static void DoSaveAsNewResources(ICommandViewModel showTestPropertiesCommand, XMLRepository xmlRepository, HomeViewModel homeView)
        {
            string newTestName = homeView.NewTestName;
            string newFuelName = homeView.NewFuelName;
            string newVehicleName = homeView.NewVehicleName;

            if (!homeView.NewTestEnabled) newTestName = null;
            if (!homeView.NewFuelEnabled) newFuelName = null;
            if (!homeView.NewVehicleEnabled) newVehicleName = null;

            if (newTestName != null || newFuelName != null || newVehicleName != null)
            {
                showTestPropertiesCommand.Command.Execute(null);
                xmlRepository.SaveAsNewResources(newTestName, newFuelName, newVehicleName);
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