using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STARS.Applications.VETS.Plugins.VTS.Interface
{
    class Precondition
    {
        public string TestResultsName { get; private set; }
        public string TestIDNumber { get; private set; }
        public string TestTypeCode { get; private set; }
        public string VIN { get; private set; }

        public Precondition()
        {
            TestResultsName = "";
            TestIDNumber = "";
            TestTypeCode = "";
            VIN = "";
        }

        public Precondition(string testResultsName, string testIDNumber, string testTypeCode, string vin)
        {
            TestResultsName = testResultsName;
            TestIDNumber = testIDNumber;
            TestTypeCode = testTypeCode;
            VIN = vin;
        }
    }
}
