using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace SocketSvc_LA_SIHOT_GMC
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
			{ 
				new SocketSvc_LA_SIHOT_GMC() 
			};
            ServiceBase.Run(ServicesToRun);
        }
    }
}
