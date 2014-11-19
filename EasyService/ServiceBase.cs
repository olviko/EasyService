using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyService
{
    /// <summary>
    /// Base service class used to execute service logic on a dedicated thread.
    /// </summary>
    public abstract class ServiceBase : IService
    {
        public ServiceBase()
        {
            cancellationSource = new CancellationTokenSource();
            mainThread = new Thread(() => MainLoop(cancellationSource.Token));
        }

        public virtual void OnInstall()
        {

        }

        public virtual void OnUninstall()
        {

        }

        public void OnStart()
        {
            mainThread.Start();
        }

        public void OnStop()
        {
            cancellationSource.Cancel();    

            if (mainThread.ThreadState != ThreadState.Unstarted)
            {
                mainThread.Join();
            }

            cancellationSource.Dispose();
        }

        protected abstract void MainLoop(CancellationToken stopEvent);

        private readonly CancellationTokenSource cancellationSource;
        private readonly Thread mainThread;
    }
}
