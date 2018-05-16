using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows.Input;
using Caliburn.PresentationFramework;
using STARS.Applications.Interfaces;
using STARS.Applications.Interfaces.Navigation;
using STARS.Applications.Interfaces.ViewModels;
using STARS.Applications.UI.Common;
using STARS.Applications.UI.Common.AttachableProperties;
using STARS.Applications.VETS.Interfaces;
using STARS.Applications.VETS.Interfaces.Constants;
using STARS.Applications.VETS.Plugins.Enterprise.Common;
using STARS.Applications.VETS.Plugins.Enterprise.Common.Repository;
using STARS.Applications.VETS.Plugins.VTS.UI.Command;
using STARS.Applications.VETS.Plugins.VTS.UI.Properties;
using STARS.Applications.VETS.UI.ViewModels.PropertyEditors;
using STARS.Applications.VETS.Plugins.VTS.Interface;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace STARS.Applications.VETS.Plugins.VTS.UI.ViewModels.Home
{
    /// <summary>
    /// The home view model
    /// </summary>
    [Export("VTS", typeof(HomeViewModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class HomeViewModel : PropertyChangedBase, IReadOnlyNameNavigableItem, ICanSort<SingleTest>, INotifyPropertyChanged
    {
        private readonly IShellViewModel _shellViewModel;

        #region Construction

        /// <summary>
        /// Default constructor
        /// </summary>
        [ImportingConstructor]
        public HomeViewModel(IImageManager imageManager, IShellViewModel shellViewModel, XMLRepository xmlRepository)
        {
            _shellViewModel = shellViewModel;

            TestList = new SortableBindableCollection<SingleTest>();
            SelectedTest = new ObservableValue<SingleTest>
            {
                Changed = _ => { TestProperties4Debug.Clear(); }
            };
            TestProperties4Debug = new BindableCollection<ResourcePropertyModel>();

            SortSettings = new SortSettings("TestCell", ListSortDirection.Ascending);
            SortSettings.Changed += (sender, args) => this.Sort(TestList);
            ColumnNameToSortKey = new Dictionary<string, Func<SingleTest, IComparable>> { };
            ColumnNameToSortKey.Add("TestCell", i => i.TestCell);
            ColumnNameToSortKey.Add("TestID", i => i.TestIDNumber);
            ColumnNameToSortKey.Add("VehicleID", i => i.VehicleID);
            ColumnNameToSortKey.Add("ProjectID", i => i.ProjectID);
            ColumnNameToSortKey.Add("TestTypeCode", i => i.TestTypeCode);
            ColumnNameToSortKey.Add("Priority", i => i.Priority);
            ColumnNameToSortKey.Add("ModificationDate", i => i.ModificationDate);

            GetTestListCommand = new GetTestListCommand(imageManager, TestList, xmlRepository);
            ShowTestPropertiesCommand = new ShowTestPropertiesCommand(imageManager, SelectedTest, TestProperties4Debug, xmlRepository);
            RunTestCommand = new RunTestCommand(imageManager, SelectedTest, ShowTestPropertiesCommand, xmlRepository);
            SaveAsNewResourcesCommand = new SaveAsNewResourcesCommand(imageManager, SelectedTest, this, ShowTestPropertiesCommand, xmlRepository);
            ControlSaveAsCommand = new ControlSaveAsCommand(imageManager, SelectedTest, this);

            var imagePathPattern = "/STARS.Applications.VETS.Plugins.VTS.UI;component/Images/{0}.png";

            DisplayInfo = new ExplorerDisplayInfo
            {
                Description = Resources.VtsVets,
                Image16 = string.Format(imagePathPattern, "green_car_16"),
                Image32 = string.Format(imagePathPattern, "green_car_32"),
                ExplorerImage16 = string.Format(imagePathPattern, "white_car_16"),
            };

            DisplayName = Resources.VtsVets;

            OnTestFinish.TestFinished += OnTestFinished;
        }

        public void OnTestFinished(object sender, EventArgs e)
        {
            GetTestListCommand.Command.Execute(false);
        }

        #endregion

        #region Implementation of IDisplayItem

        /// <summary>
        /// The display info
        /// </summary>
        public DisplayInfo DisplayInfo { get; private set; }

        #endregion

        /// <summary>
        /// The name of the display item
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// Start test command
        /// </summary>
        public ICommandViewModel RunTestCommand { get; private set; }

        /// <summary>
        /// Start test command
        /// </summary>
        public ICommandViewModel GetTestListCommand { get; private set; }

        /// <summary>
        /// Start test command
        /// </summary>
        public ICommandViewModel ShowTestPropertiesCommand { get; private set; }

        /// <summary>
        /// Start test command
        /// </summary>
        public ICommandViewModel SaveAsNewResourcesCommand { get; private set; }

        /// <summary>
        /// Start test command
        /// </summary>
        public ICommandViewModel ControlSaveAsCommand { get; private set; }

        /// <summary>
        /// Enterprise test list
        /// </summary>
        public SortableBindableCollection<SingleTest> TestList { get; set; }

        /// <summary>
        /// Enterprise test names
        /// </summary>
        public SortableBindableCollection<string> TestNames { get; set; }

        /// <summary>
        /// Selected enterprise test
        /// </summary>
        public ObservableValue<SingleTest> SelectedTest { get; set; }

        /// <summary>
        /// Test properties for debug
        /// </summary>
        public BindableCollection<ResourcePropertyModel> TestProperties4Debug { get; private set; }

        public bool IsDebugAreaVisible
        {
            get { return ImportConfig.Instance.IsDebugAreaVisible; }
        }

        private string newTestName;
        public string NewTestName
        {
            get { return newTestName; }
            set { newTestName = value; NotifyPropertyChanged("NewTestName"); }
        }

        private string newFuelName;
        public string NewFuelName
        {
            get { return newFuelName; }
            set { newFuelName = value; NotifyPropertyChanged("NewFuelName"); }
        }
        private string newVehicleName;
        public string NewVehicleName
        {
            get { return newVehicleName; }
            set { newVehicleName = value; NotifyPropertyChanged("NewVehicleName"); }
        }

        private bool newTestEnabled;
        public bool NewTestEnabled
        {
            get { return newTestEnabled; }
            set { newTestEnabled = value; NotifyPropertyChanged("NewTestEnabled"); }
        }

        private bool newFuelEnabled;
        public bool NewFuelEnabled
        {
            get { return newFuelEnabled; }
            set { newFuelEnabled = value; NotifyPropertyChanged("NewFuelEnabled"); }
        }
        private bool newVehicleEnabled;
        public bool NewVehicleEnabled
        {
            get { return newVehicleEnabled; }
            set { newVehicleEnabled = value; NotifyPropertyChanged("NewVehicleEnabled"); }
        }

        private Visibility isOverlayVisible;
        public Visibility IsOverlayVisible
        {
            get { return isOverlayVisible; }
            set { isOverlayVisible = value; NotifyPropertyChanged("IsOverlayVisible"); }
        }

        public ISortSettings SortSettings { get; private set; }

        public Dictionary<string, Func<SingleTest, IComparable>> ColumnNameToSortKey { get; private set; }

        new public virtual event PropertyChangedEventHandler PropertyChanged;
        protected virtual void NotifyPropertyChanged(params string[] propertyNames)
        {
            if (PropertyChanged != null)
            {
                foreach (string propertyName in propertyNames) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                PropertyChanged(this, new PropertyChangedEventArgs("HasError"));
            }
        }

        public void Display()
        {
            IsOverlayVisible = Visibility.Hidden;
            NewTestName = null;
            NewFuelName = null;
            NewVehicleName = null;
            NewTestEnabled = true;
            NewFuelEnabled = true;
            NewVehicleEnabled = true;
            SelectedTest.Value = null;
            GetTestListCommand.Command.Execute(false);
            _shellViewModel.ActivateItem(this);
        }

    }
}
