using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Caliburn.PresentationFramework;
using STARS.Applications.Interfaces;
using STARS.Applications.Interfaces.ViewModels;
using STARS.Applications.UI.Common;
using STARS.Applications.VETS.Interfaces;
using STARS.Applications.VETS.Plugins.Enterprise.Common;
using STARS.Applications.VETS.Plugins.Enterprise.Common.Repository;
using STARS.Applications.VETS.Plugins.VTS.UI.Properties;
using STARS.Applications.VETS.Plugins.VTS.Interface;

namespace STARS.Applications.VETS.Plugins.VTS.UI.Command
{
    /// <summary>
    /// Implementation of GetTestList button
    /// </summary>
    internal class GetTestListCommand : ICommandViewModel
    {
        private readonly BindableCollection<SingleTest> _testList;
        private readonly XMLRepository _xmlRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="service"></param>
        /// <param name="repository"></param>
        /// <param name="testList"></param>
        /// <param name="filtersModelFilters"></param>
        public GetTestListCommand(IImageManager imageManager, BindableCollection<SingleTest> testList, XMLRepository xmlRepository)
        {
            _testList = testList;
            _xmlRepository = xmlRepository;

            Command = new RelayCommand(x => DoGetTestList(x));
            DisplayInfo = new DisplayInfo
            {
                Description = Resources.ReloadTestList,
                Image16 = imageManager.GetImage16Path("Restore")
            };
        }

        /// <summary>
        /// Process of GetTestListCommand
        /// </summary>
        private void DoGetTestList(object x)
        {
            bool showInLog;
            try
            {
                showInLog = (bool)x;
            }
            catch
            {
                showInLog = true;
            }
            BindableCollection<SingleTest> testList = new BindableCollection<SingleTest>(_xmlRepository.GetTestList(showInLog));
            _testList.Clear();
            _testList.AddRange(testList);
        }

        public DisplayInfo DisplayInfo { get; private set; }

        public string DisplayName
        {
            get { return Resources.ReloadTestList; }
        }

        public ICommand Command { get; private set; }
    }
}
