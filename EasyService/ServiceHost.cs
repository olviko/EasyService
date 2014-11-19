using System;
using System.IO;

namespace EasyService
{
    /// <summary>
    /// Manages lifetime of a service.
    /// </summary>
    public static class ServiceHost
    {
        public static void Run(IService service, ServiceHostingSettings serviceOptions, string[] cmdArgs = null)
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            if (cmdArgs == null || cmdArgs.Length == 0)
            {
                if (Environment.UserInteractive)
                {
                    service.OnStart();
                    Console.WriteLine("Service is running. Press ENTER to stop...");
                    Console.ReadLine();
                    service.OnStop();
                }
                else
                {
                    var svc = new ServiceWrapper(service, serviceOptions);
                    System.ServiceProcess.ServiceBase.Run(svc);
                }
                return;
            }

            switch (cmdArgs[0])
            {
                case "start":
                    ScmController.Start(serviceOptions.ServiceName);
                    break;

                case "stop":
                    ScmController.Stop(serviceOptions.ServiceName);
                    break;

                case "install":

                    if (cmdArgs.Length > 2)
                    {
                        serviceOptions.ServiceAccount = new ServiceAccount(cmdArgs[1], cmdArgs[2]);
                    }
                    else if (cmdArgs.Length > 1)
                    {
                        ServiceAccountType serviceAccountType;

                        if (!Enum.TryParse(cmdArgs[1], out serviceAccountType))
                        {
                            Console.WriteLine("Invalid service account '{0}'", cmdArgs[1]);
                            break;
                        }
                        serviceOptions.ServiceAccount = new ServiceAccount(serviceAccountType);
                    }

                    ScmController.Install(
                        serviceOptions.ServiceName,
                        serviceOptions.DisplayName,
                        serviceOptions.Description,
                        serviceOptions.StartMode,
                        serviceOptions.ServiceAccount,
                        serviceOptions.Dependencies,
                        serviceOptions.RecoveryOptions);

                        service.OnInstall();

                    break;

                case "uninstall":

                    service.OnUninstall();

                    ScmController.Uninstall(
                        serviceOptions.ServiceName,
                        serviceOptions.DisplayName,
                        serviceOptions.Description,
                        serviceOptions.StartMode,
                        serviceOptions.ServiceAccount,
                        serviceOptions.Dependencies,
                        serviceOptions.RecoveryOptions);

                    break;

                default:
                    PrintInstruction();
                    break;
            }
        }

        private static void PrintInstruction()
        {
            string appName = Environment.GetCommandLineArgs()[0];

            Console.WriteLine();
            Console.WriteLine(appName + @" start - start service");
            Console.WriteLine(appName + @" stop - stop service");
            Console.WriteLine(appName + @" install [<LocalSystem|LocalService|NetworkService|domain\username>] [<password>] - install under specific service or user account. If no account is specified the service will run under LocalSystem. Password is required for user accounts.");
            Console.WriteLine(appName + @" uninstall - uninstall service");
            Console.WriteLine(appName + @" help - display this instruction");
            Console.WriteLine();
        }
    }
}
