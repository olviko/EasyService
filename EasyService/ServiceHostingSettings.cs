using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyService
{
    /// <summary>
    /// Service settings for SCM
    /// </summary>
    public sealed class ServiceHostingSettings
    {
        /// <summary>
        /// Service name (required).
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// Service display name.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Service start mode. Defaults to "Automatic".
        /// </summary>
        public ServiceStartMode StartMode { get; set; }

        /// <summary>
        /// Service logon account. Defaults to "LocalSystem".
        /// </summary>
        public ServiceAccount ServiceAccount { get; set; }

        /// <summary>
        /// Service recovery options.
        /// </summary>
        public ServiceRecoveryOptions RecoveryOptions { get; set; }

        /// <summary>
        /// A list of the services that must be running for this service to run.
        /// </summary>
        public List<ServiceDependency> Dependencies { get; set; }

        public ServiceHostingSettings()
        {
            this.StartMode = ServiceStartMode.Automatic;
            this.ServiceAccount = ServiceAccount.LocalSystem;
            this.RecoveryOptions = new ServiceRecoveryOptions();
            this.Dependencies = new List<ServiceDependency>();
        }
    }
}
