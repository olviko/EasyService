using System;
using System.Threading;
using EasyService;

namespace ServiceSample
{
    class MiniService : ServiceBase
    {
        protected override void MainLoop(CancellationToken stopEvent)
        {
            try
            {
                // do some useful work here {

                while (true)
                {
                    try
                    {
                        // pretend to work hard :-)
                        stopEvent.WaitHandle.WaitOne(5000);

                        // periodically check stop event
                        stopEvent.ThrowIfCancellationRequested();
                    }
                    catch (Exception)
                    {
                        // recover from errors or just sleep for some time
                        if (stopEvent.WaitHandle.WaitOne(30000))
                            break;
                    }
                    finally
                    {

                    }
                }

                // } 
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Initialize your service
            var service = new MiniService();

            // Configure service hosting
            var settings = new ServiceHostingSettings
            {
                ServiceName = "MiniService",
                DisplayName = "Minimal Service",
                Description = "Minimal service description",
                StartMode = ServiceStartMode.Automatic,
                ServiceAccount = ServiceAccount.LocalSystem,
                RecoveryOptions = new ServiceRecoveryOptions
                {
                    FirstFailureAction = ServiceRecoveryAction.RestartService,
                    SecondFailureAction = ServiceRecoveryAction.RestartService,
                    SubsequentFailureAction = ServiceRecoveryAction.RestartComputer,
                    ResetFailureCountWaitDays = 1,
                    RestartServiceWaitMinutes = 3,
                    RestartSystemWaitMinutes = 3
                }
            };

            // Parse command-line options and execute
            ServiceHost.Run(service, settings, args);
        }
    }
}
