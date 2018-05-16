using System;

namespace STARS.Applications.VETS.Plugins.Enterprise.Common.Exceptions
{
    public class RepositoryException : Exception
    {
        private string _message;
        private string _myStackTrace;

        public override string Message
        {
            get { return _message; }
        }

        public override string StackTrace { get { return _myStackTrace; } }

        public bool IsRepositoryException { get; set; }

        public void SetMessage(string message)
        {
            _message = message;
        }

        public void SetStackTrace(string stackTrace)
        {
            _myStackTrace = stackTrace;
        }
    }
}