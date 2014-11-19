using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyService
{
    /// <summary>
    /// Service interface methods.
    /// </summary>
    public interface IService
    {
        void OnInstall();
        void OnUninstall();
        void OnStart();
        void OnStop();
    }
}
