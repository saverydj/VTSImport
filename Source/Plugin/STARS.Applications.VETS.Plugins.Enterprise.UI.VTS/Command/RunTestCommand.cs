using System.Windows.Input;
using STARS.Applications.Interfaces;
using STARS.Applications.Interfaces.ViewModels;
using STARS.Applications.UI.Common;
using STARS.Applications.VETS.Interfaces;
using STARS.Applications.VETS.Plugins.Enterprise.Common;
using STARS.Applications.VETS.Plugins.VTS.UI.Properties;
using STARS.Applications.VETS.UI.ViewModels.PropertyEditors;
using STARS.Applications.VETS.Plugins.VTS.Interface;

namespace STARS.Applications.VETS.Plugins.VTS.UI.Command
{
    /// <summary>
    /// Implementation of StartTest button
    /// </summary>
    internal class RunTestCommand : ICommandViewModel
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="service"></param>
        /// <param name="selectedTest"></param>
        public RunTestCommand(IImageManager imageManager, ObservableValue<SingleTest> selectedTest, ICommandViewModel showTestPropertiesCommand, XMLRepository xmlRepository)
        {
            Command = new RelayCommand(_ =>
                 DoRunTest(selectedTest.Value.TestIDNumber, showTestPropertiesCommand, xmlRepository),
                _ => (selectedTest.Value != null));
            DisplayInfo = new DisplayInfo
            {
                Description = Resources.RunSelectedTest,
                Image16 = imageManager.GetImage16Path("Run")
            };
        }

        private void DoRunTest(string resourceName, ICommandViewModel showTestPropertiesCommand, XMLRepository xmlRepository)
        {
            showTestPropertiesCommand.Command.Execute(null);
            xmlRepository.RunTest(resourceName);
        }

        public DisplayInfo DisplayInfo { get; private set; }

        public string DisplayName
        {
            get { return Resources.RunTest; }
        }

        public ICommand Command { get; private set; }
    }
}
