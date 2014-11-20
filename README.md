EasyService
===========

EasyService is a .NET library intended to simplify development and maintenance of Windows services.

Benefits
--

- Transform any console application into a self-installing Windows service.
- Streamline debugging. It is much easier to debug a console application than a service. No more "Attach to process...".
- Full Windows Service Control Manager (SCM) integration including support for configuring all startup and recovery options.

Example
--

```c#

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


```

1. MiniService.exe -install
2. MiniService.exe -start

Command-line Reference
--

A Windows service created using the library provides command-line arguments which can be used to install, uninstall, start, and stop the service.

service.exe **[command]**

Command|Description
---------------------------------|-----
**help**                         |Displays help
**install** [account] [password] |Installs the service. You can also set log-on account and password.
**uninstall**                    |Uninstalls the service
**start**                        |Starts the service if it is not already running
**stop**                         |Stops the service if it is running




