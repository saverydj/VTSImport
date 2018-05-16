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

namespace STARS.Applications.VETS.Plugins.VTS.UI.Command
{
    /// <summary>
    /// Implementation of StartTest button
    /// </summary>
    internal class ShowTestPropertiesCommand : ICommandViewModel
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ShowTestPropertiesCommand(IImageManager imageManager, IObservableValue<SingleTest> selectedTest, 
            BindableCollection<ResourcePropertyModel> testProperties4Debug, XMLRepository xmlRepository)
        {
            DisplayInfo = new DisplayInfo
            {
                Description = Resources.ImportAndShowProperties,
                Image16 = imageManager.GetImage16Path("Import")
            };

            Command = new RelayCommand(_ =>
            {
                FillUiWithImportedTest(testProperties4Debug, 
                    xmlRepository.ShowTestProperties(selectedTest.Value.TestIDNumber));
            }, _ => (selectedTest.Value != null));
        }

        private static void FillUiWithImportedTest(BindableCollection<ResourcePropertyModel> testProperties4Debug, PropertyBag test)
        {
            if (test == null)
                return;

            var defaultType = string.Empty;
            if (test.HasProperty(Constants.Type))
            {
                defaultType = test.GetProperty<string>(Constants.Type);
                System.Diagnostics.Trace.TraceInformation("----- FillUiWithImportedTest: defaultType = {0}\n", defaultType);
            }


            testProperties4Debug.Clear();
            int i = 1;
            foreach (
                var prop in
                test.GetAllProperties()
                    .SelectMany(p => GetResourcePropertyModel(test, p, defaultType))
                    .OrderBy(m => m.ResourceType))
            {
                if (prop.Key != Constants.Type)
                {
                    System.Diagnostics.Trace.TraceInformation("----- FillUiWithImportedTest: {0} {1} = {2}", i++, prop.Key, prop.Value);
                    testProperties4Debug.Add(prop);
                }
            }

        }

        private static IEnumerable<ResourcePropertyModel> GetResourcePropertyModel(PropertyBag test, string propertyName,
            string defaultType)
        {
            var value = test.GetProperty(propertyName);
            var referencePropertyValue = value as IReferenceProperty;
            string type = defaultType;
            if (referencePropertyValue != null)
            {
                type = referencePropertyValue.Type;
                value = referencePropertyValue.DisplayName;
            }
            else
            {
                var propBagPropertyValue = value as PropertyBag;
                if (propBagPropertyValue != null)
                {
                    if (propBagPropertyValue.HasProperty(Constants.Type))
                    {
                        type = propBagPropertyValue.GetProperty<string>(Constants.Type);
                    }
                    System.Diagnostics.Trace.TraceInformation("----- GetResourcePropertyModel: type={0}\n", type);
                    return
                        propBagPropertyValue.GetAllProperties()
                            .SelectMany(p => GetResourcePropertyModel(propBagPropertyValue, p, type));
                }
                
                var valueProperty = value as ValueProperty;
                if (valueProperty != null)
                {
                    value = valueProperty.Value + " " + valueProperty.Unit;
                }
            }
            System.Diagnostics.Trace.TraceInformation("----- GetResourcePropertyModel: {0} = {1} ({2})", propertyName, value, type);
            return new[] {new ResourcePropertyModel
            {
                Key = propertyName,
                ResourceType = type,
                Value = value
            }};
        }

        public DisplayInfo DisplayInfo { get; private set; }

        public string DisplayName
        {
            get { return Resources.ImportTest; }
        }

        public ICommand Command { get; private set; }
    }
}
