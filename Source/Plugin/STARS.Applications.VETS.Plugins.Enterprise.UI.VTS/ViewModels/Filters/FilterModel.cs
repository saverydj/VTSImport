using Caliburn.PresentationFramework;
using STARS.Applications.Interfaces.ViewModels.PropertyEditors;
using STARS.Applications.VETS.UI.ViewModels.PropertyEditors;

namespace STARS.Applications.VETS.Plugins.Enterprise.UI.VTS.ViewModels.Filters
{
    internal class FilterModel : PropertyChangedBase
    {
        public FilterModel(string property, string value)
        {
            Property = new ObservableValue<string>(property);
            Value = new ObservableValue<string>(value);
        }

        public IObservableValue<string> Property { get; private set; }

        public IObservableValue<string> Value { get; private set; }

        public bool IsEmpty { get
        {
            return string.IsNullOrEmpty(Property.Value) || string.IsNullOrEmpty(Value.Value);
        } }
    }
}
