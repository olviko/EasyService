using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.ServiceProcess;
using System.Threading;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Reflection;
using System.Security.Principal;

namespace EasyService
{
    /// <summary>
    /// SCM communication class
    /// </summary>
    public static class ScmController
    {
        #region Service Management

        /// <summary>
        /// Checks if service is installed.
        /// </summary>
        public static bool IsInstalled(string serviceName)
        {
            return ServiceController
                .GetServices()
                .Any(service => service.ServiceName == serviceName);
        }

        /// <summary>
        /// Starts the service if it is not running.
        /// </summary>
        public static void Start(string serviceName)
        {
            using (var sc = new ServiceController(serviceName))
            {
                ServiceControllerStatus status = sc.Status;

                if (status == ServiceControllerStatus.Running)
                {
                    return;
                }

                if (status == ServiceControllerStatus.StartPending)
                {
                    return;
                }

                if (!IsAdministrator())
                {
                    if (RerunAsAdministrator())
                        return;

                    throw new InvalidOperationException("The service can only be started by administrator");
                }

                sc.Start();
                sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));

                sc.Refresh();
                status = sc.Status;

                if (status != ServiceControllerStatus.Running)
                {
                    throw new InvalidOperationException("Service failed to start in timely manner.");
                }
            }
        }

        /// <summary>
        /// Stops the service if it is running.
        /// </summary>
        public static void Stop(string serviceName)
        {
            using (var sc = new ServiceController(serviceName))
            {
                ServiceControllerStatus status = sc.Status;

                if (status == ServiceControllerStatus.Stopped)
                {
                    return;
                }

                if (status == ServiceControllerStatus.StopPending)
                {
                    return;
                }

                if (IsAdministrator())
                {
                    if (RerunAsAdministrator())
                        return;
                    throw new InvalidOperationException("The service can only be stopped by administrator");
                }

                sc.Stop();
                sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));

                sc.Refresh();
                status = sc.Status;

                if (status != ServiceControllerStatus.Stopped)
                {
                    throw new InvalidOperationException("Service failed to stop in timely manner.");
                }
            }
        }
        #endregion

        #region Installer

        private static Installer CreateInstaller(
                string serviceName,
                string serviceDisplayName,
                string serviceDescription,
                ServiceStartMode startMode,
                ServiceAccount serviceAccount,
                IEnumerable<ServiceDependency> dependencies,
                ServiceRecoveryOptions recoveryOptions
            )
        {
            ServiceProcessInstaller spi = new ServiceProcessInstaller();

            spi.Account = (System.ServiceProcess.ServiceAccount)(int)serviceAccount.AccountType;

            if (serviceAccount.AccountType == ServiceAccountType.User)
            {
                spi.Username = serviceAccount.Username;
                spi.Password = serviceAccount.Password;
            }

            string[] servicesDependedOn;

            if (dependencies == null)
            {
                servicesDependedOn = new string[0];
            }
            else
            {
                servicesDependedOn = dependencies.Select(d => d.ServiceName).ToArray();
            }

            ServiceInstaller si = new ServiceInstaller
                                      {
                                          ServiceName = serviceName,
                                          DisplayName = serviceDisplayName,
                                          Description = serviceDescription,
                                          ServicesDependedOn = servicesDependedOn,
                                          StartType = (System.ServiceProcess.ServiceStartMode)(int)startMode
                                      };

            var installer = new Installer();
            installer.Installers.Add(spi);
            installer.Installers.Add(si);

            installer.Committed += delegate 
                                       {
                                           ScmController.SetServiceRecoveryOptions(serviceName, recoveryOptions);
                                       };

            string path = String.Format("/assemblypath={0}", Assembly.GetEntryAssembly().Location);
            string[] cmdline = { path, "/LogtoConsole=false" }; //disable installer output

            TransactedInstaller ti = new TransactedInstaller();
            ti.Installers.Add(installer);
            ti.Context = new InstallContext(null, cmdline);

            return ti;
        }

        /// <summary>
        /// Registers service in SCM.
        /// </summary>
        public static void Install(
                string serviceName,
                string serviceDisplayName,
                string serviceDescription,
                ServiceStartMode startMode,
                ServiceAccount serviceAccount,
                IEnumerable<ServiceDependency> dependencies,
                ServiceRecoveryOptions recoveryOptions
            )
        {
            using (Installer ti = CreateInstaller(serviceName, serviceDisplayName, serviceDescription, startMode, serviceAccount, dependencies, recoveryOptions))
            {
                if (IsInstalled(serviceName))
                {
                    throw new InvalidOperationException("The service is already installed.");
                }

                if (!IsAdministrator())
                {
                    if (RerunAsAdministrator())
                        return;

                    throw new InvalidOperationException("The service can only be installed by administrator");
                }
                
                ti.Install(new System.Collections.Hashtable());
            }
        }

        /// <summary>
        /// Removes service from SCM.
        /// </summary>
        public static void Uninstall(
                string serviceName,
                string serviceDisplayName,
                string serviceDescription,
                ServiceStartMode startMode,
                ServiceAccount serviceAccount,
                IEnumerable<ServiceDependency> dependencies,
                ServiceRecoveryOptions recoveryOptions
            )
        {
            using (Installer ti = CreateInstaller(serviceName, serviceDisplayName, serviceDescription, startMode, serviceAccount, dependencies, recoveryOptions))
            {
                if (!IsInstalled(serviceName))
                {
                    throw new InvalidOperationException("The service is not installed.");
                }

                if (!IsAdministrator())
                {
                    if (RerunAsAdministrator())
                        return;

                    throw new InvalidOperationException("The service can only be uninstalled by administrator");
                }

                ti.Uninstall(null);
            }
        }

        #endregion

        #region User Account Elevation

        private static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();

            if (null != identity)
            {
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }

            return false;
        }

        private static bool RerunAsAdministrator()
        {
            if (Environment.OSVersion.Version.Major == 6)
            {
                var startInfo = new ProcessStartInfo
                {
                    Verb = "runas",
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    FileName = Assembly.GetEntryAssembly().Location,
                    Arguments = string.Join(" ", Environment.GetCommandLineArgs().Skip(1)),
                    WorkingDirectory = Environment.CurrentDirectory,
                };

                try
                {
                    Process process = Process.Start(startInfo);
                    process.WaitForExit();

                    return true;
                }
                catch (Win32Exception ex)
                {
                    Console.WriteLine("Process Start Exception", ex);
                }
            }

            return false;
        }


        #endregion


        #region Service Recovery Options

        [StructLayout(LayoutKind.Sequential)]
        struct LUID_AND_ATTRIBUTES
        {
            [MarshalAs(UnmanagedType.U4)]
            public UInt32 Attributes;
            public long Luid;
        }


        [StructLayout(LayoutKind.Sequential)]
        struct SC_ACTION
        {
            public SC_ACTION_TYPE Type;
            public uint Delay;
        }

        enum SC_ACTION_TYPE
        {
            None = 0,
            RestartService = 1,
            RebootComputer = 2,
            RunCommand = 3
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SERVICE_FAILURE_ACTIONS
        {
            [MarshalAs(UnmanagedType.U4)]
            public int dwResetPeriod;

            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpRebootMsg;

            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpCommand;

            [MarshalAs(UnmanagedType.U4)]
            public UInt32 cActions;

            public IntPtr lpsaActions;
        }


        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct TOKEN_PRIVILEGES
        {
            public int PrivilegeCount;
            public LUID_AND_ATTRIBUTES Privileges;
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ChangeServiceConfig2(
            IntPtr hService,
            int dwInfoLevel,
            IntPtr lpInfo);

        [DllImport("advapi32.dll")]
        static extern bool
            AdjustTokenPrivileges(IntPtr TokenHandle, bool DisableAllPrivileges,
                                  [MarshalAs(UnmanagedType.Struct)] ref TOKEN_PRIVILEGES NewState, int BufferLength,
                                  IntPtr PreviousState, ref int ReturnLength);

        [DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
        static extern bool
            LookupPrivilegeValue(string lpSystemName, string lpName, ref long lpLuid);

        [DllImport("advapi32.dll")]
        static extern bool
            OpenProcessToken(IntPtr ProcessHandle, int DesiredAccess, ref IntPtr TokenHandle);

        const int SERVICE_CONFIG_FAILURE_ACTIONS = 2;
        const int SE_PRIVILEGE_ENABLED = 2;
        const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
        const int TOKEN_ADJUST_PRIVILEGES = 32;
        const int TOKEN_QUERY = 8;

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public static void SetServiceRecoveryOptions(
            string serviceName, ServiceRecoveryOptions recoveryOptions)
        {
            if (recoveryOptions == null)
                throw new ArgumentNullException("recoveryOptions");

            bool requiresShutdownPriveleges =
                recoveryOptions.FirstFailureAction == ServiceRecoveryAction.RestartComputer ||
                recoveryOptions.SecondFailureAction == ServiceRecoveryAction.RestartComputer ||
                recoveryOptions.SubsequentFailureAction == ServiceRecoveryAction.RestartComputer;
            if (requiresShutdownPriveleges)
                GrantShutdownPrivileges();

            int actionCount = 3;
            var restartServiceAfter = (uint)TimeSpan.FromMinutes(
                                                                 recoveryOptions.RestartServiceWaitMinutes).TotalMilliseconds;

            IntPtr failureActionsPointer = IntPtr.Zero;
            IntPtr actionPointer = IntPtr.Zero;

            ServiceController controller = null;
            try
            {
                // Open the service
                controller = new ServiceController(serviceName);

                // Set up the failure actions
                var failureActions = new SERVICE_FAILURE_ACTIONS();
                failureActions.dwResetPeriod = (int)TimeSpan.FromDays(recoveryOptions.ResetFailureCountWaitDays).TotalSeconds;
                failureActions.cActions = (uint)actionCount;
                failureActions.lpRebootMsg = recoveryOptions.RestartSystemMessage;

                // allocate memory for the individual actions
                actionPointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SC_ACTION))*actionCount);
                ServiceRecoveryAction[] actions = {
                                                    recoveryOptions.FirstFailureAction,
                                                    recoveryOptions.SecondFailureAction,
                                                    recoveryOptions.SubsequentFailureAction
                                                  };
                for (int i = 0; i < actions.Length; i++)
                {
                    ServiceRecoveryAction action = actions[i];
                    SC_ACTION scAction = GetScAction(action, restartServiceAfter);
                    Marshal.StructureToPtr(scAction, (IntPtr)((Int64)actionPointer + (Marshal.SizeOf(typeof(SC_ACTION)))*i), false);
                }
                failureActions.lpsaActions = actionPointer;

                string command = recoveryOptions.RunProgramCommand;
                if (command != null)
                    failureActions.lpCommand = command;

                failureActionsPointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SERVICE_FAILURE_ACTIONS)));
                Marshal.StructureToPtr(failureActions, failureActionsPointer, false);

                // Make the change
                bool success = ChangeServiceConfig2(controller.ServiceHandle.DangerousGetHandle(),
                                                    SERVICE_CONFIG_FAILURE_ACTIONS,
                                                    failureActionsPointer);

                // Check that the change occurred
                if (!success)
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "Unable to change the Service configuration.");
            }
            finally
            {
                if (failureActionsPointer != IntPtr.Zero)
                    Marshal.FreeHGlobal(failureActionsPointer);

                if (actionPointer != IntPtr.Zero)
                    Marshal.FreeHGlobal(actionPointer);

                if (controller != null)
                {
                    controller.Dispose();
                }

                //log.Debug(m => m("Done setting service recovery options."));
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        static void GrantShutdownPrivileges()
        {
            //log.Debug(m => m("Granting shutdown privileges to process user..."));

            IntPtr tokenHandle = IntPtr.Zero;

            var tkp = new TOKEN_PRIVILEGES();

            long luid = 0;
            int retLen = 0;

            try
            {
                IntPtr processHandle = Process.GetCurrentProcess().Handle;
                bool success = OpenProcessToken(processHandle, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref tokenHandle);
                if (!success)
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "Unable to open process token.");

                LookupPrivilegeValue(null, SE_SHUTDOWN_NAME, ref luid);

                tkp.PrivilegeCount = 1;
                tkp.Privileges.Luid = luid;
                tkp.Privileges.Attributes = SE_PRIVILEGE_ENABLED;

                success = AdjustTokenPrivileges(tokenHandle, false, ref tkp, 0, IntPtr.Zero, ref retLen);
                if (!success)
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "Unable to adjust shutdown priveleges.");
            }
            finally
            {
                if (tokenHandle != IntPtr.Zero)
                    Marshal.FreeHGlobal(tokenHandle);
                //log.Debug(m => m("Done granting shutdown privileges to process user."));
            }
        }

        static SC_ACTION GetScAction(ServiceRecoveryAction action,
                                     uint restartServiceAfter)
        {
            var scAction = new SC_ACTION();
            SC_ACTION_TYPE actionType = default(SC_ACTION_TYPE);
            switch (action)
            {
                case ServiceRecoveryAction.TakeNoAction:
                    actionType = SC_ACTION_TYPE.None;
                    break;
                case ServiceRecoveryAction.RestartService:
                    actionType = SC_ACTION_TYPE.RestartService;
                    break;
                case ServiceRecoveryAction.RestartComputer:
                    actionType = SC_ACTION_TYPE.RebootComputer;
                    break;
                case ServiceRecoveryAction.RunProgram:
                    actionType = SC_ACTION_TYPE.RunCommand;
                    break;
            }
            scAction.Type = actionType;
            scAction.Delay = restartServiceAfter;
            return scAction;
        }


        #endregion
    }
}