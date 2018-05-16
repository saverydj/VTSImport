using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using STARS.Applications.Interfaces.Dialogs;
using STARS.Applications.Interfaces.EntityManager;
using STARS.Applications.VETS.Interfaces.Constants;
using STARS.Applications.VETS.Interfaces.Devices;
using STARS.Applications.VETS.Interfaces.Entities;
using STARS.Applications.VETS.Interfaces.Logging;
using STARS.Applications.VETS.Interfaces.TestExecution;
using STARS.Applications.VETS.Interfaces.TestExecution.Activities;
using STARS.Applications.VETS.Interfaces.TestExecution.Activities.Attributes;
using Stars.ApplicationManager;
using Stars.Resources;
using STARS;
using log4net;
using Stars;
using System.Threading;
using System.IO;
using System.Xml.Linq;
using System.Text;

namespace STARS.Applications.VETS.Plugins.VTS.Interface
{
    /// <summary>
    /// Activity to update VETS resources from VTS file
    /// </summary>
    [PartCreationPolicy(CreationPolicy.Shared)]
    [AsyncTestActivity(typeof(IEmissionTestRunContext),
        TriggerState = EmissionTestStates.TestComplete,
        BlockStateExit = EmissionTestStates.PreTestConfiguration)]
    public class OnTestFinish : IAsyncTestActivity<IEmissionTestRunContext>
    {

        #region Imports
        #pragma warning disable 649

        [Import] internal IStarsApplication _starsApplication;

        [Import] internal ILocalResourceSupport _localResourceSupport;

        [Import] internal ITestStatus TestStatus;

        [Import] internal IEntityQuery EntityQuery;

        [Import] internal IDialogService DialogService;

        [Import] internal ISystemLogManager SystemLogManager;

        #pragma warning restore 649
        #endregion

        #region Implementation of IDisposable
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {

        }
        #endregion

        #region Implementation of IAsyncTestActivity
        /// <summary>
        /// Do all test stand updating and re-activate if there have been any changes
        /// </summary>
        /// <param name="runContext">The context to use to run the activity</param>
        /// <param name="activityStatus">Call methods on this object to indicate state and progress of activity</param>
        public void Run(IActivityRunContext<IEmissionTestRunContext> runContext, IActivityStatus activityStatus)
        {
            _runContext = runContext;
            _activityStatus = activityStatus;

            Test vtsTest = GetVetsTest();
            if(vtsTest.ID != TestStatus.RunningTests.Last().RunningTestID)
            {
                return;
            }

            try
            {
                string testID = vtsTest.CustomFieldValues.FirstOrDefault(p => p.CustomFieldID == "TestIDNumber").Value;
                UpdateVTSStatus(testID, "D");
                OnTestFinished();
                _activityStatus.Completed();
            }
            catch (Exception e)
            {
                if (!_aborted)
                    _activityStatus.Failed(e);
            }
        }

        /// <summary>
        /// Abort the execution of this activity. Will only be called if the activity is running.
        /// </summary>
        /// <param name="runContext">The run context</param>
        public void Abort(IActivityRunContext<IEmissionTestRunContext> runContext)
        {
            _aborted = true;
        }

        private void ShowMessage(string message)
        {
            var result = DialogService.PromptUser(
               "Title",
                string.Format(message),
                DialogIcon.Warning,
                DialogButton.Yes,
                DialogButton.Yes, DialogButton.No);
        }


        /// <summary>
        /// An action that will rollback any actions that the activity has taken. This will only be called
        /// if the activity has run and completed.
        /// </summary>
        public Action<IActivityRunContext<IEmissionTestRunContext>, Action<Exception>> Rollback
        {
            get { return null; }
        }
        #endregion

        #region Custom Test Activity Functionality

        protected virtual void OnTestFinished()
        {
            if(TestFinished != null)
            {
                TestFinished(this, EventArgs.Empty);
            }
        }

        private Test GetVetsTest()
        {
            var test =
                    EntityQuery.FirstOrDefault<Test>(
                        x => x.Name == Config.TestResourceName);
            if (test == null)
                throw new NullReferenceException(string.Format("Test {0} does not exist",
                    Config.TestResourceName));
            return test;
        }

        private void UpdateVTSStatus(string testID, string status)
        {
            List<string> idList = new List<string>();
            UpdateVTSSchedule(testID, status, ref idList); //Update the status of the test in the shared VTS schedule xml
            UpdateVTSFeedBack(testID, status, idList);     //Update the status of the test in the shared VTS feedback xml
        }

        private void UpdateVTSSchedule(string testID, string status, ref List<string> idList)
        {
            int rowNode = 0;
            string[] lines = File.ReadAllLines(Config.VtsSchedule, Encoding.GetEncoding(1252));
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("<ROW>")) rowNode = i;
                if (lines[i].Contains("<TEST_ID_NUM>") && lines[i].Contains("</TEST_ID_NUM>"))
                {
                    idList.Add(lines[i].Split(new string[] { "<TEST_ID_NUM>" }, StringSplitOptions.None)[1].Split(new string[] { "</TEST_ID_NUM>" }, StringSplitOptions.None)[0]);
                }
                if (lines[i].Contains(testID))
                {
                    while (!lines[rowNode].Contains("</ROW>") && rowNode < lines.Length)
                    {
                        if (lines[rowNode].Contains("<TEST_STATUS>") && lines[rowNode].Contains("</TEST_STATUS>"))
                        {
                            lines[rowNode] = lines[rowNode].Split('<')[0] + "<TEST_STATUS>" + status + "</TEST_STATUS>";
                        }
                        rowNode++;
                    }
                }
            }
            File.WriteAllLines(Config.VtsSchedule, lines, Encoding.GetEncoding(1252));
        }

        private void UpdateVTSFeedBack(string testID, string status, List<string> idList)
        {
            XDocument vtsFeedBack;
            if (!File.Exists(Config.VtsFeedBack))
            {
                vtsFeedBack = new XDocument(new XDeclaration("1.0", "utf-16", null), new XElement("Tests"));
            }
            else
            {
                vtsFeedBack = XDocument.Load(Config.VtsFeedBack);
            }
            IEnumerable<XElement> allTests = vtsFeedBack.Descendants().Where(e => e.Name.LocalName == "Test");
            if (allTests.Count() > 0)
            {
                foreach (XElement existingTest in allTests)
                {
                    try
                    {
                        XElement existingTestID = existingTest.Descendants().Where(e => e.Name.LocalName == "TEST_ID_NUM").FirstOrDefault();
                        if (existingTestID.Value == testID)
                        {
                            XElement existingTestStatus = existingTest.Descendants().Where(e => e.Name.LocalName == "TEST_STATUS").FirstOrDefault();
                            existingTestStatus.Value = status;
                            CleanVTSFeedBack(vtsFeedBack, idList);
                            vtsFeedBack.Save(Config.VtsFeedBack);
                            return;
                        }
                    }
                    catch { }
                }
            }
            XElement tests = vtsFeedBack.Descendants().Where(e => e.Name.LocalName == "Tests").FirstOrDefault();
            XElement newTest = new XElement("Test");
            newTest.Add(new XElement("TEST_ID_NUM", testID));
            newTest.Add(new XElement("TEST_STATUS", status));
            tests.Add(newTest);
            CleanVTSFeedBack(vtsFeedBack, idList);
            vtsFeedBack.Save(Config.VtsFeedBack);
        }

        private void CleanVTSFeedBack(XDocument vtsFeedBack, List<string> idList)
        {
            var allTests = vtsFeedBack.Descendants().Where(e => e.Name.LocalName == "Test").ToList();
            foreach (var existingTest in allTests)
            {
                try
                {
                    XElement existingTestID = existingTest.Descendants().Where(e => e.Name.LocalName == "TEST_ID_NUM").FirstOrDefault();
                    if (!idList.Contains(existingTestID.Value))
                    {
                        existingTest.Remove();
                    }
                }
                catch { }
            }
        }

        private string GetXMLField(XDocument text, string tag, string parentTag)
        {
            var nodes = text.Descendants().Where(e => e.Name.LocalName == tag);
            if (nodes.Count() == 0)
            {
                nodes = text.Descendants().Where(e => e.Name.LocalName == "CustomFieldID");
                if (nodes.Count() > 0)
                {
                    foreach (var node in nodes)
                    {
                        if (node.Value.ToString() == tag)
                        {
                            XElement valueNode = (XElement)node.NextNode;
                            return valueNode.Value.ToString();
                        }
                    }
                }
            }
            else if (nodes.Count() == 1)
            {
                foreach (var node in nodes)
                {
                    return node.Value.ToString();
                }
            }
            else if (nodes.Count() > 1)
            {
                foreach (var node in nodes)
                {
                    if (parentTag == "" || node.Parent.Name.LocalName == parentTag)
                    {
                        return node.Value.ToString();
                    }
                }
            }
            return null;
        }

        private void ReplaceXMLField(ref XDocument text, string tag, string parentTag, string field)
        {
            var nodes = text.Descendants().Where(e => e.Name.LocalName == tag);
            if (nodes.Count() == 0)
            {
                nodes = text.Descendants().Where(e => e.Name.LocalName == "CustomFieldID");
                if (nodes.Count() > 0)
                {
                    foreach (var node in nodes)
                    {
                        if (node.Value.ToString() == tag)
                        {
                            XElement valueNode = (XElement)node.NextNode;
                            valueNode.SetValue(field);
                        }
                    }
                }
            }
            else if (nodes.Count() == 1)
            {
                foreach (var node in nodes)
                {
                    node.SetValue(field);
                }
            }
            else if (nodes.Count() > 1)
            {
                foreach (var node in nodes)
                {
                    if (parentTag == "" || node.Parent.Name.LocalName == parentTag)
                    {
                        node.SetValue(field);
                    }
                }
            }
        }

        #endregion 

        #region Fields

        protected static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IActivityRunContext<IEmissionTestRunContext> _runContext;

        private IActivityStatus _activityStatus;

        private bool _aborted;

        private readonly ActionExecutor _actionExecutor = new ActionExecutor();

        public delegate void TestFinishedEventHandler(object sender, EventArgs e);

        public static event TestFinishedEventHandler TestFinished;

        #endregion
    }
}
