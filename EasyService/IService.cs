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
