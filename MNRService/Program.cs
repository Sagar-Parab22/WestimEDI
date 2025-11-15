using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace MNRService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            MNREDIService service = new MNREDIService();

            if (Environment.UserInteractive)
            {
                // We are in debug mode
                Console.WriteLine("Service is starting in console/debug mode...");
                Console.WriteLine("Press 'Enter' to stop the service.");

                // Call the OnStart logic directly
                service.DebugOnStart();

                // Wait for 'Enter' to be pressed
                Console.ReadLine();

                // Call the OnStop logic
                service.DebugOnStop();
                Console.WriteLine("Service stopped.");
            }
            else
            {
                // We are in service mode
                // This is the standard way to run the service
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    service
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
