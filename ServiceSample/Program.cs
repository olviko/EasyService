using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasyService;

namespace ServiceSample
{
    class MiniService : ServiceBase
    {
        protected override void MainLoop(CancellationToken stopEvent)
        {
            try
            {
                // do useful work here {

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
}
