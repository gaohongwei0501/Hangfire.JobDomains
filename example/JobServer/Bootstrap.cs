using Common.Logging;
using Hangfire.PluginPackets.Storage.EntityFrameworkCore.SqlServer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace Hangfire.PluginPackets.Server
{
    public class Bootstrap : ServiceControl
    {
        private static readonly ILog _logger = LogManager.GetLogger<Bootstrap>();

        public ServiceStartup Provider = new ServiceStartup();

        public Bootstrap()
        {
        }

        public bool Start(HostControl hostControl)
        {
            try
            {
                Provider.Start<SqlServerStorage>("ConnectionString");

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error("Topshelf starting occured errors.", ex);
                return false;
            }

        }

        public bool Stop(HostControl hostControl)
        {
            try
            {
               // StopHangfireServer();
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Topshelf stopping occured errors.", ex);
                return false;
            }
        }

      
     
    }

}
