using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Reflection;
using System.Diagnostics;
using STARS.Applications.Interfaces.EntityManager;
using STARS.Applications.VETS.Interfaces.Entities;
using System.ComponentModel.Composition;
using STARS.Applications.Interfaces.EntityProperties.CustomFields;
using STARS.Applications.VETS.Plugins.Enterprise.Common.Resources;
using STARS.Applications.VETS.Plugins.Enterprise.Common;
using STARS.Applications.VETS.Interfaces.Constants;
using STARS.Applications.VETS.Interfaces;
using STARS.Applications.VETS.Interfaces.TestExecution;
using STARS.Applications.VETS.Interfaces.Logging;
using STARS.Applications.VETS.Plugins.VTS.Interface.Properties;
using STARS.Applications.VETS.Interfaces.Events;
using STARS.Applications.Interfaces.ViewModels;
using System.Threading;

namespace STARS.Applications.VETS.Plugins.VTS.Interface
{

    [Export(typeof(XMLRepository)), PartCreationPolicy(CreationPolicy.Shared)]
    public class XMLRepository
    {
        private readonly IEntityQuery _entityQuery;
        private readonly IVETSEntityManagerView _entityManagerView;
        private readonly IEntityCreate _entityCreate;
        private readonly ITestExecution _testExecution;
        private readonly ITestStatus _testStatus;
        private readonly ITestEvents _testEvents;
        private readonly ISystemLogManager _logger;
        private readonly IReportService _resportService;
        private readonly VTS _vts;
        private readonly IApplication _application;
        private readonly IVETSEntityManagerViewModelView _entityManagerViewModelView;

        [ImportingConstructor]
        public XMLRepository(
            IEntityQuery entityQuery,
            IVETSEntityManagerView entityManagerView,
            IEntityCreate entityCreate,
            ITestExecution testExecution,
            ITestStatus testStatus,
            ITestEvents testEvents,
            ISystemLogManager logger,
            IReportService resportService,
            IApplication application,
            IVETSEntityManagerViewModelView entityManagerViewModelView,
            VTS vts
        )
        {
            _entityQuery = entityQuery;
            _entityManagerView = entityManagerView;
            _entityCreate = entityCreate;
            _testExecution = testExecution;
            _testStatus = testStatus;
            _testEvents = testEvents;
            _logger = logger;
            _resportService = resportService;
            _vts = vts;
            _application = application;
            _entityManagerViewModelView = entityManagerViewModelView;
        }

        public IEnumerable<SingleTest> GetTestList(bool showInLog)
        {
            if (showInLog) _logger.AddLogEntry(TraceEventType.Information, SystemLogSources.DataAccess, Properties.Resources.TestListImportStarted);
            _vts.UpdateRepsitory(showInLog);

            List<SingleTest> testList = new List<SingleTest>();
            PropertyInfo[] properties = typeof(SingleTest).GetProperties();

            XDocument xml = XDocument.Load(Config.VtsRepository);
            var vtsTests = xml.Descendants().Where(e => e.Name.LocalName == "VtsTest");
            foreach (XElement vtsTest in vtsTests)
            {
                SingleTest singleTest = new SingleTest();
                foreach (PropertyInfo property in properties)
                {
                    singleTest.GetType().GetProperty(property.Name).SetValue(singleTest, vtsTest.Descendants().Where(e => e.Name.LocalName == property.Name).First().Value, null);
                }
                testList.Add(singleTest);
            }

            if (showInLog) _logger.AddLogEntry(TraceEventType.Information, SystemLogSources.DataAccess, Properties.Resources.TestListImportFinished);
            testList = testList.OrderBy(x => x.TestIDNumber).ToList();
            return testList.OrderBy(x => x.TestCell);
        }

        public PropertyBag ShowTestProperties(string resourceName)
        {
            /// <summary>
            /// To be invoked by UI.dll upon "Get test details" button press.
            /// Test, Fuel, & Vehicle properties are parsed from vtsRepository.xml 
            /// for the VTS resource having "resourceName". Properties are written 
            /// to VETS resources and returned in PropertyBag for display
            /// in Enterprise UI.
            /// </summary>

            string[] propertyBagFilter = { "ProjectID", "VehicleID", "ManufacturerName", "ModelType", "Priority" };

            _logger.AddLogEntry(TraceEventType.Information, SystemLogSources.DataAccess, String.Format(Properties.Resources.TestImportStarted, resourceName));

            Test vetsTest = GetVetsTest(Config.TestResourceName);
            Fuel vetsFuel = GetVetsFuel(Config.FuelResourceName);
            Vehicle vetsVehicle = GetVetsVehicle(Config.VehicleResourceName);
            SamplingConfiguration vetsSamplingConfiguration = GetVetsSamplingConfiguration(Config.SamplingConfigurationResourceName);

            //From vtsRepository.xml
            XElement testProperties = GetProperties(resourceName, Resources.Tests);
            XElement fuelProperties = GetProperties(resourceName, Resources.Fuels);
            XElement vehicleProperties = GetProperties(resourceName, Resources.Vehicles);
            XElement samplingConfigurationProperties = GetProperties(resourceName, Resources.SamplingConfiguration);

            //Shown in Enterprise UI
            CommonPropertyBag masterPropertyBag = new CommonPropertyBag("All Properties");
            CommonPropertyBag testPropertyBag = new CommonPropertyBag(Resources.Tests);
            CommonPropertyBag fuelPropertyBag = new CommonPropertyBag(Resources.Fuels);
            CommonPropertyBag vehiclePropertyBag = new CommonPropertyBag(Resources.Vehicles);
            CommonPropertyBag samplingConfigurationPropertyBag = new CommonPropertyBag(Resources.SamplingConfiguration);

            masterPropertyBag.SetProperty(Constants.Type, "");
            testPropertyBag.SetProperty(Constants.Type, Resources.Tests);
            fuelPropertyBag.SetProperty(Constants.Type, Resources.Fuels);
            vehiclePropertyBag.SetProperty(Constants.Type, Resources.Vehicles);
            samplingConfigurationPropertyBag.SetProperty(Constants.Type, Resources.SamplingConfiguration);

            //Set all custom fields to empty string and doubles to '0'
            //ClearResourceProperties(vetsTest);
            //ClearResourceProperties(vetsFuel);
            //ClearResourceProperties(vetsVehicle);

            //Write VTS properties to VETS resources & enterprise UI
            foreach (XElement property in testProperties.Descendants())
            {
                SetResourceProperty(vetsTest, property.Name.LocalName, property.Value);
                if (propertyBagFilter.Contains(property.Name.LocalName)) testPropertyBag.SetProperty(property.Name.LocalName, property.Value);
            }

            foreach (XElement property in fuelProperties.Descendants())
            {
                SetResourceProperty(vetsFuel, property.Name.LocalName, property.Value);
                if (propertyBagFilter.Contains(property.Name.LocalName)) fuelPropertyBag.SetProperty(property.Name.LocalName, property.Value);
            }

            foreach (XElement property in vehicleProperties.Descendants())
            {
                SetResourceProperty(vetsVehicle, property.Name.LocalName, property.Value);
                if (propertyBagFilter.Contains(property.Name.LocalName)) vehiclePropertyBag.SetProperty(property.Name.LocalName, property.Value);
            }

            foreach (XElement property in samplingConfigurationProperties.Descendants())
            {
                SetResourceProperty(vetsSamplingConfiguration, property.Name.LocalName, property.Value);
                if (propertyBagFilter.Contains(property.Name.LocalName)) samplingConfigurationPropertyBag.SetProperty(property.Name.LocalName, property.Value);
            }

            vetsTest.Modified = DateTime.Now;
            vetsFuel.Modified = DateTime.Now;
            vetsVehicle.Modified = DateTime.Now;
            vetsSamplingConfiguration.Modified = DateTime.Now;

            //Refresh to update fields in VETS UI
            _entityManagerView.Lookup(ResourceType.Test).Value.Update(vetsTest.ID, vetsTest.Properties);
            _entityManagerView.Lookup(ResourceType.Fuel).Value.Update(vetsFuel.ID, vetsFuel.Properties);
            _entityManagerView.Lookup(ResourceType.Vehicle).Value.Update(vetsVehicle.ID, vetsVehicle.Properties);
            _entityManagerView.Lookup(ResourceType.SamplingConfiguration).Value.Update(vetsSamplingConfiguration.ID, vetsSamplingConfiguration.Properties);

            masterPropertyBag.SetProperty("Test Properties", testPropertyBag);
            masterPropertyBag.SetProperty("Fuel Properties", fuelPropertyBag);
            masterPropertyBag.SetProperty("Vehicle Properties", vehiclePropertyBag);
            masterPropertyBag.SetProperty("Sampling Configuration Properties", samplingConfigurationPropertyBag);

            _logger.AddLogEntry(TraceEventType.Information, SystemLogSources.DataAccess, String.Format(Properties.Resources.TestImportFinished, resourceName));

            return masterPropertyBag;
        }

        public void RunTest(string resourceName)
        {
            try
            {
                string id = GetVetsTest(Config.TestResourceName).ID;
                if (!CanRunTest(Resources.EntityUriVetsTest, id))
                {
                    _logger.AddLogEntry(TraceEventType.Information, SystemLogSources.DataAccess, String.Format(Properties.Resources.ImpotedTestCannotStart, resourceName));
                    return;
                }
                _testExecution.StartTest(Resources.EntityUriVetsTest, id);
            }
            catch (Exception ex)
            {
                if (ex.Source != Resources.ExecutionSource) throw new System.Exception(ex.Message);
            }
        }

        private bool CanRunTest(string entityUriVetsTest, string id)
        {
            try
            {
                return
                    !_testStatus.TestRunning &&
                    _testExecution.CanStart(Resources.EntityUriVetsTest, GetVetsTest(Config.TestResourceName).ID);
            }
            catch (NullReferenceException e)
            {
                return false;
            }
        }

        public void SaveAsNewResources(string testName, string fuelName, string vehicleName)
        {
            int testCount = _entityManagerView.Tests.Value.Count();
            int fuelCount = _entityManagerView.Fuels.Value.Count();
            int vehicleCount = _entityManagerView.Vehicles.Value.Count();

            if (testName != null)
            {
                Test newTest = new Test();
                newTest.Name = testName;
                _entityManagerView.Tests.Value.Create("EntityUris.VETSTEST.Test", newTest.Properties);
            }

            if (fuelName != null)
            {
                Fuel newFuel = new Fuel();
                newFuel.Name = fuelName;
                _entityManagerView.Fuels.Value.Create("EntityUris.VETSFUEL.Fuel", newFuel.Properties);
            }

            if (vehicleName != null)
            {
                Vehicle newVehicle = new Vehicle();
                newVehicle.Name = vehicleName;
                _entityManagerView.Vehicles.Value.Create("EntityUris.VETSVEHICLE.Vehicle", newVehicle.Properties);
            }
            
            if (testName != null || fuelName != null || vehicleName != null)
            {
                Thread queryResourcesThread = new Thread(() => QueryResourcesOnLoop(testCount, testName, fuelCount, fuelName, vehicleCount, vehicleName));
                queryResourcesThread.IsBackground = true;
                queryResourcesThread.Start();
            }

        }

        private void QueryResourcesOnLoop(int testCount, string testName, int fuelCount, string fuelName, int vehicleCount, string vehicleName)
        {
            string[] uniqueProperties = { "Name", "ID", "Properties", "Created", "SoftwareVersion" };
            string newTestName = null;
            string newFuelName = null;
            string newVehicleName = null;

            for (int i = 0; i < 100; i++)
            {
                if (testName != null && _entityQuery.Where<Test>().Count() != testCount && (fuelName == null || newFuelName != null) && (vehicleName == null || newVehicleName != null))
                {
                    Test vtsTest = GetVetsTest(Config.TestResourceName);
                    newTestName = _entityQuery.Where<Test>().OrderByDescending(x => x.Created).First().Name;
                    Test newTest = GetVetsTest(newTestName);
                    foreach(PropertyInfo property in vtsTest.GetType().GetProperties())
                    {
                        if(!uniqueProperties.Contains(property.Name))
                        {
                            newTest.GetType().GetProperty(property.Name).SetValue(newTest, property.GetValue(vtsTest, null), null);
                        }
                    }
                    if(fuelName != null) newTest.FuelName = newFuelName;
                    if(vehicleName != null) newTest.VehicleName = newVehicleName;
                    _entityManagerView.Lookup(ResourceType.Test).Value.Update(newTest.ID, newTest.Properties);
                    _logger.AddLogEntry(TraceEventType.Information, SystemLogSources.DataAccess, String.Format("Test resource '{0}' was successfully created.", newTestName));
                    return;
                }

                if (fuelName != null && newFuelName == null && _entityQuery.Where<Fuel>().Count() != fuelCount)
                {
                    Fuel vtsFuel = GetVetsFuel(Config.FuelResourceName);
                    string newName = _entityQuery.Where<Fuel>().OrderByDescending(x => x.Created).First().Name;
                    Fuel newFuel = GetVetsFuel(newName);
                    foreach (PropertyInfo property in vtsFuel.GetType().GetProperties())
                    {
                        if (!uniqueProperties.Contains(property.Name))
                        {
                            newFuel.GetType().GetProperty(property.Name).SetValue(newFuel, property.GetValue(vtsFuel, null), null);
                        }
                    }
                    _entityManagerView.Lookup(ResourceType.Fuel).Value.Update(newFuel.ID, newFuel.Properties);
                    _logger.AddLogEntry(TraceEventType.Information, SystemLogSources.DataAccess, String.Format("Fuel resource '{0}' was successfully created.", newName));
                    newFuelName = newName;
                    if((testName == null && vehicleName == null) || (testName == null && newVehicleName != null)) return;
                }

                if (vehicleName != null && newVehicleName == null && _entityQuery.Where<Vehicle>().Count() != vehicleCount)
                {
                    Vehicle vtsVehicle = GetVetsVehicle(Config.VehicleResourceName);
                    string newName = _entityQuery.Where<Vehicle>().OrderByDescending(x => x.Created).First().Name;
                    Vehicle newVehicle = GetVetsVehicle(newName);
                    foreach (PropertyInfo property in vtsVehicle.GetType().GetProperties())
                    {
                        if (!uniqueProperties.Contains(property.Name))
                        {
                            newVehicle.GetType().GetProperty(property.Name).SetValue(newVehicle, property.GetValue(vtsVehicle, null), null);
                        }
                    }
                    _entityManagerView.Lookup(ResourceType.Vehicle).Value.Update(newVehicle.ID, newVehicle.Properties);
                    _logger.AddLogEntry(TraceEventType.Information, SystemLogSources.DataAccess, String.Format("Vehicle resource '{0}' was successfully created.", newName));
                    newVehicleName = newName;
                    if ((testName == null && fuelName == null) || (testName == null && newFuelName != null)) return;
                }

                Thread.Sleep(100);
            }
        }

        private void QueryFuelsOnLoop(int fuelCount)
        {
            string[] uniqueProperties = { "Name", "ID", "Properties", "Created", "SoftwareVersion" };
            for (int i = 0; i < 100; i++)
            {
                
                Thread.Sleep(100);
            }
        }

        private void QueryVehiclesOnLoop(int vehicleCount)
        {
            string[] uniqueProperties = { "Name", "ID", "Properties", "Created", "SoftwareVersion" };
            for (int i = 0; i < 100; i++)
            {

                Thread.Sleep(100);
            }
        }

        private Test GetVetsTest(string resourceName)
        {         
            var test =
                    _entityQuery.FirstOrDefault<Test>(
                        x => x.Name == resourceName);
            if (test == null)
                throw new NullReferenceException(string.Format("Test {0} does not exist",
                    resourceName));
            return test;
        }

        private Fuel GetVetsFuel(string resourceName)
        {
            var fuel =
                    _entityQuery.FirstOrDefault<Fuel>(
                        x => x.Name == resourceName);
            if (fuel == null)
                throw new NullReferenceException(string.Format("Fuel {0} does not exist",
                    resourceName));
            return fuel;
        }

        private Vehicle GetVetsVehicle(string resourceName)
        {
            var vehicle =
                    _entityQuery.FirstOrDefault<Vehicle>(
                        x => x.Name == resourceName);
            if (vehicle == null)
                throw new NullReferenceException(string.Format("Vehicle {0} does not exist",
                    Config.VehicleResourceName));
            return vehicle;
        }

        private SamplingConfiguration GetVetsSamplingConfiguration(string resourceName)
        {
            var samplingConfiguration =
                    _entityQuery.FirstOrDefault<SamplingConfiguration>(
                        x => x.Name == resourceName);
            if (samplingConfiguration == null)
                throw new NullReferenceException(string.Format("Sampling Configuration {0} does not exist",
                    resourceName));
            return samplingConfiguration;
        }

        private XElement GetProperties(string resourceName, string propertyType)
        {
            XDocument xml = XDocument.Load(Config.VtsRepository);
            var vtsTests = xml.Descendants().Where(e => e.Name.LocalName == "VtsTest");
            foreach (XElement vtsTest in vtsTests)
            {
                XElement vtsResourceName = vtsTest.Descendants().Where(e => e.Name.LocalName == "TestIDNumber").FirstOrDefault();
                if (vtsResourceName.Value == resourceName)
                {
                    return vtsTest.Descendants().Where(e => e.Name.LocalName == propertyType).FirstOrDefault(); ;
                }
            }
            return null;
        }

        private void SetResourceProperty(object resource, string propertyName, string propertyValue)
        {
            if (propertyValue == Config.KeepOldValueKey) return;
            if (propertyName.Contains('.')) { GetNestedResource(ref resource, ref propertyName); }

            //Try for normal property
            PropertyInfo property = resource.GetType().GetProperty(propertyName);
            if (property != null)
            {
                Type type = property.PropertyType;
                try
                {
                    //Perform type conversion using property type
                    if (type.BaseType == typeof(Enum)) property.SetValue(resource, Enum.Parse(type, propertyValue), null);
                    else if (type == typeof(bool)) property.SetValue(resource, TypeCast.ToBool(propertyValue), null);
                    else if (type == typeof(int)) property.SetValue(resource, TypeCast.ToInt(propertyValue), null);
                    else if (type == typeof(double)) property.SetValue(resource, TypeCast.ToDouble(propertyValue), null);
                    else property.SetValue(resource, propertyValue, null);
                    return;
                }
                catch
                {
                    throw new System.Exception(String.Format("Imported property \"{0}\" has invalid value \"{1}\". Property must be of type \"{2}\".", propertyName, propertyValue, type.Name));
                }
            }

            //Try for special properties (i.e. custom device properties)
            property = resource.GetType().GetProperty("Properties");
            if (property != null)
            {
                IDictionary<string, object> properties = property.GetValue(resource, null) as IDictionary<string, object>;
                if (properties.ContainsKey(propertyName))
                {
                    Type type = properties[propertyName].GetType();
                    try
                    {
                        //Perform type conversion using property type
                        if (type.BaseType == typeof(Enum)) properties[propertyName] = Enum.Parse(type, propertyValue);
                        else if (type == typeof(bool)) properties[propertyName] = TypeCast.ToBool(propertyValue);
                        else if (type == typeof(int)) properties[propertyName] = TypeCast.ToInt(propertyValue);
                        else if (type == typeof(double)) properties[propertyName] = TypeCast.ToDouble(propertyValue);
                        else properties[propertyName] = propertyValue;
                        return;
                    }
                    catch
                    {
                        throw new System.Exception(String.Format("Imported property \"{0}\" has invalid value \"{1}\". Property must be of type \"{2}\".", propertyName, propertyValue, type.Name));
                    }
                }
            }

            //Try for custom fields
            property =  resource.GetType().GetProperty(Resources.CustomFieldValues);
            if (property != null)
            {
                CustomFieldValue[] customFields = property.GetValue(resource, null) as CustomFieldValue[];
                if (customFields.Any(p => p.CustomFieldID == propertyName))
                {
                    customFields.FirstOrDefault(p => p.CustomFieldID == propertyName).Value = propertyValue;
                    return;
                }
            }
        }

        private void ClearResourceProperties(object resource)
        {
            PropertyInfo[] properties = resource.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (property.PropertyType == typeof(double))
                {
                    property.SetValue(resource, 0, null);
                }
            }
            CustomFieldValue[] customFields = resource.GetType().GetProperty(Resources.CustomFieldValues).GetValue(resource, null) as CustomFieldValue[];
            foreach(CustomFieldValue customField in customFields)
            {
                customField.Value = String.Empty;
            }
        }

        private void GetNestedResource(ref object resource, ref string propertyName)
        {
            //Set resource context to bottom level and propertyName to lowest child
            string[] tree = propertyName.Split('.');
            for (int i = 0; i < tree.Length - 1; i++)
            {
                resource = resource.GetType().GetProperty(tree[i]).GetValue(resource, null);
            }
            propertyName = tree[tree.Length - 1];
        }

    }
}
