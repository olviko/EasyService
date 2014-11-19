using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyService
{
    /// <summary>
    /// Indicates the service that must be running for this service to run.
    /// </summary>
    public sealed class ServiceDependency
    {
        /// <summary>
        /// Dependency service name.
        /// </summary>
        public string ServiceName { get; private set; }

        public ServiceDependency(string serviceName)
        {
            ServiceName = serviceName;
        }

        public static readonly ServiceDependency Msmq = new ServiceDependency("MSMQ");
        public static readonly ServiceDependency SqlServer = new ServiceDependency("MSSQLSERVER");
        public static readonly ServiceDependency EventLog = new ServiceDependency("Eventlog");
        public static readonly ServiceDependency IIS = new ServiceDependency("W3SVC");
    }
}
