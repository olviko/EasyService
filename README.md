EasyService
===========

EasyService is a NET library aimed to simplify development and maintenance of Windows services.

Benefits
--

- Streamline deployment by creating self-installing services.
- Simplify debugging. No more "Attach to process..." nightmares.
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
        var service = new MiniService();

        var settings = new ServiceHostingSettings
        {
            ServiceName = "MiniService",
            DisplayName = "Minimal Service",
            Description = "Minimal service description",
        };

        ServiceHost.Run(service, settings, args);
    }
}


```

1. MiniService.exe -install
2. MiniService.exe -start

