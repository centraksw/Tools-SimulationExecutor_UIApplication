using System.ServiceProcess;

namespace DeviceDataSimulatorService
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new SimulatorService()
            };
            ServiceBase.Run(ServicesToRun);
            //SimulatorService ss = new SimulatorService();
            //ss.StartDebug();
        }
    }
}
