using Common.Logging;
using Hangfire.PluginPackets.Command;
using Hangfire.PluginPackets.Storage;
using Hangfire.Server;
using Owin;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;


namespace Hangfire.PluginPackets.Server
{
   
    public  class ServiceStartup
    {
        private static readonly ILog _logger = LogManager.GetLogger<ServiceStartup>();

        private static readonly ConcurrentBag<BackgroundJobServer> Servers = new ConcurrentBag<BackgroundJobServer>();

        public async Task StartAsync<T>(string con, string path = "", int count = 5) where T : IStorage, new()
        {
            var connecting = StorageService.Provider.SetStorage(new T(), con);
            if (connecting == false) throw (new Exception(" HangfireDomain 数据服务连接失败"));
            var Options = await PluginServiceManager.InitServer(path, count);

            PluginServiceManager.LoadDynamic();

            GlobalConfiguration.Configuration.UseSqlServerStorage(con);
            StartHangfireServer(JobStorage.Current, Options);

            await BatchImportCommand.Invoke();
        }

        public void Start<T>(string con, string path = "", int count = 5) where T : IStorage, new()
        {
            var start = StartAsync<T>(con, path, count);
            Task.WaitAll(start);
        }

        public void Stop() {

            StopHangfireServer();
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

        string GetLocalIp()
        {
            string hostname = Dns.GetHostName();//得到本机名   
            IPHostEntry localhost = Dns.GetHostEntry(hostname);
            IPAddress localaddr = localhost.AddressList[0];
            return localaddr.ToString();
        }

    }
}
