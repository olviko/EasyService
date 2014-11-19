namespace EasyService
{
    public enum ServiceRecoveryAction
    {
        TakeNoAction = 0,
        RestartService = 1,
        RestartComputer = 2,
        RunProgram = 3,
    }

    public class ServiceRecoveryOptions
    {
        public ServiceRecoveryOptions()
        {
            FirstFailureAction = ServiceRecoveryAction.TakeNoAction;
            SecondFailureAction = ServiceRecoveryAction.TakeNoAction;
            SubsequentFailureAction = ServiceRecoveryAction.TakeNoAction;
            ResetFailureCountWaitDays = 0;
            RestartServiceWaitMinutes = 1;
            RestartSystemWaitMinutes = 1;
        }

        public ServiceRecoveryAction FirstFailureAction { get; set; }
        public ServiceRecoveryAction SecondFailureAction { get; set; }
        public ServiceRecoveryAction SubsequentFailureAction { get; set; }
        public int ResetFailureCountWaitDays { get; set; }
        public int RestartServiceWaitMinutes { get; set; }
        public int RestartSystemWaitMinutes { get; set; }
        public string RestartSystemMessage { get; set; }
        public string RunProgramCommand { get; set; }
        public string RunProgramParameters { get; set; }
    }
}