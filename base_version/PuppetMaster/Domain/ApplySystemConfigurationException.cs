using System;
using System.Collections.Generic;
using System.Text;

namespace PuppetMaster.Domain
{
    [Serializable]
    public class ApplySystemConfigurationException : Exception
    {
        public ApplySystemConfigurationException()
        { }

        public ApplySystemConfigurationException(string message)
            : base(message)
        { }

        public ApplySystemConfigurationException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
