using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace EasyService
{
    internal class ServiceWrapper : System.ServiceProcess.ServiceBase
    {
        public ServiceWrapper(IService service, ServiceHostingSettings serviceOptions)
        {
            this.service = service;
            CanShutdown = true;
            ServiceName = serviceOptions.ServiceName;
        }

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);
            service.OnStart(); 
        }

        protected override void OnStop()
        {
            service.OnStop();
            base.OnStop();
        }

        protected override void OnShutdown()
        {
            Stop();
        }

        private readonly IService service;
    }

}
