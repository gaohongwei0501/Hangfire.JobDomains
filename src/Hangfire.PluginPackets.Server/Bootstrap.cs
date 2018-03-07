using Common.Logging;
using Hangfire.PluginPackets.Server;
using Hangfire.PluginPackets.Storage;
using Hangfire.Server;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace Hangfire.PluginPackets.Service
{
    public class Bootstrap : ServiceControl
    {
        private static readonly ILog _logger = LogManager.GetLogger<Bootstrap>();

        private static readonly ConcurrentBag<BackgroundJobServer> Servers = new ConcurrentBag<BackgroundJobServer>();

        public IStorage Storage { get; set; }

        public string Path { get; set; }

        public int Count { get; set; }

        public Bootstrap(IStorage storage,string path,int count)
        {
            Storage = storage;
            Path = path;
            Count = count;
        }

        public bool Start(HostControl hostControl)
        {
            try
            {
                var con = "ConnectionString";

                //var connecting = StorageService.Provider.SetStorage(Storage, con);
                //if (connecting == false) throw (new Exception(" HangfireDomain 数据服务连接失败"));
                //var fetchOptions = PluginServiceManager.InitServer(Path, Count);

                //Task.WaitAll(fetchOptions);
                //var Options = fetchOptions.Result;
                //PluginServiceManager.LoadDynamic();

                //GlobalConfiguration.Configuration.UseSqlServerStorage(con);
                //StartHangfireServer(JobStorage.Current, Options);

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
                StopHangfireServer();
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Topshelf stopping occured errors.", ex);
                return false;
            }
        }

        void StartHangfireServer(JobStorage storage, BackgroundJobServerOptions options, params IBackgroundProcess[] additionalProcesses)
        {
            if (storage == null) throw new ArgumentNullException(nameof(storage));
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (additionalProcesses == null) throw new ArgumentNullException(nameof(additionalProcesses));

            var server = new BackgroundJobServer(options, storage, additionalProcesses);
            Servers.Add(server);
        }

        void StopHangfireServer()
        {
            foreach (var server in Servers)
            {
                server.SendStop();
            }
        }

     
    }

}
