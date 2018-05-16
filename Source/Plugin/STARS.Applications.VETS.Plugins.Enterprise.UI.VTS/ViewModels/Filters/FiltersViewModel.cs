using System;
using System.Linq;
using System.Windows.Input;
using Caliburn.PresentationFramework;
using STARS.Applications.Interfaces.ViewModels.PropertyEditors;
using STARS.Applications.UI.Common;
using STARS.Applications.VETS.Plugins.Enterprise.Common;
using STARS.Applications.VETS.Plugins.Enterprise.VetoRepository;
using STARS.Applications.VETS.UI.ViewModels.PropertyEditors;

namespace STARS.Applications.VETS.Plugins.Enterprise.UI.VTS.ViewModels.Filters
{
    internal class FiltersViewModel : PropertyChangedBase
    {
        public FiltersViewModel(IImportManager service)
        {
            //var orders = service.GetOrders();

            Filters =
                new BindableCollection<FilterModel>(
                    service.DefaultFilters.Conditions.OfType<WhereCondition>()
                        .Select(c => new FilterModel(c.Property, c.Value)));

            if (Filters.Count != service.DefaultFilters.Conditions.Count())
            {
                throw new ArgumentOutOfRangeException("Only WhereConditions are suppoerted");
            }

            SelectedFilter = new ObservableValue<FilterModel>();

            AddFilterCommand = new RelayCommand(_ => AddFilter());
            DeleteFilterCommand = new RelayCommand(_ => DeleteFilter(), _ => SelectedFilter.Value != null);
        }

        public BindableCollection<FilterModel> Filters { get; private set; }


        public ICommand AddFilterCommand { get; private set; }

        public ICommand DeleteFilterCommand { get; private set; }

        public IObservableValue<FilterModel> SelectedFilter { get; private set; }


        public void AddFilter()
        {
            Filters.Add(new FilterModel(string.Empty, string.Empty));
        }

        public void DeleteFilter()
        {
            if (SelectedFilter.Value == null)
                return;

            Filters.Remove(SelectedFilter.Value);
            SelectedFilter.Value = null;
        }
    }
}
