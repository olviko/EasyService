using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyService
{
    /// <summary>
    /// Service logon type.
    /// </summary>
    public enum ServiceAccountType
    {
        /// <summary>
        /// An account that acts as a non-privileged user on the local computer, and
        /// presents anonymous credentials to any remote server.
        /// </summary>
        LocalService = 0,

        /// <summary>
        /// An account that provides extensive local privileges, and presents the computer's
        /// credentials to any remote server.
        /// </summary>
        NetworkService = 1,

        /// <summary>
        /// An account, used by the service control manager, that has extensive privileges
        /// on the local computer and acts as the computer on the network.
        /// </summary>
        LocalSystem = 2,

        /// <summary>
        /// An account defined by a specific user on the network. 
        /// </summary>
        User = 3,
    }

    /// <summary>
    /// Specifies a service's security context.
    /// </summary>
    public sealed class ServiceAccount
    {
        public string Username { get; private set; }
        public string Password { get; private set; }
        public ServiceAccountType AccountType { get; private set; }

        public ServiceAccount(string username, string password)
        {
            if (String.IsNullOrEmpty(username))
                throw new ArgumentNullException("username");

            if (String.IsNullOrEmpty(password))
                throw new ArgumentNullException("password");

            this.AccountType = ServiceAccountType.User;
            this.Username = username;
            this.Password = password;
        }

        public ServiceAccount(ServiceAccountType accountType)
        {
            this.AccountType = accountType;
        }

        public static readonly ServiceAccount LocalService = new ServiceAccount(ServiceAccountType.LocalService);
        public static readonly ServiceAccount NetworkService = new ServiceAccount(ServiceAccountType.NetworkService);
        public static readonly ServiceAccount LocalSystem = new ServiceAccount(ServiceAccountType.LocalSystem);
    }
}

