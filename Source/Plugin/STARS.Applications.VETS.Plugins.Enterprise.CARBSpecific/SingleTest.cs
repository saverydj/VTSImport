using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STARS.Applications.VETS.Plugins.VTS.Interface
{
    public class SingleTest
    {
        public string TestCell { get; private set; }
        public string TestIDNumber { get; private set; }
        public string VehicleID { get; private set; }
        public string ProjectID { get; private set; }
        public string TestTypeCode { get; private set; }
        public string Priority { get; private set; }
        public string ModificationDate { get; private set; }

        public SingleTest()
        {
        }

        public SingleTest(string testCell, string testIDNumber, string vehicleID, string projectID, string testTypeCode, string priority, string modificationDate)
        {
            TestCell = testCell;
            TestIDNumber = testIDNumber;
            VehicleID = vehicleID;
            ProjectID = projectID;
            TestTypeCode = testTypeCode;
            Priority = priority;
            ModificationDate = modificationDate;                     
        }
    }
}
