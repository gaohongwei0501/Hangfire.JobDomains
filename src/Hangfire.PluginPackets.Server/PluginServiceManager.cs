using Common.Logging;
using Hangfire.PluginPackets.Command;
using Hangfire.PluginPackets.Dynamic;
using Hangfire.PluginPackets.Interface;
using Hangfire.PluginPackets.Models;
using Hangfire.PluginPackets.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Server
{
    internal class PluginServiceManager
    {

        public static string ServerName => Environment.MachineName.ToLower();

        static ILog loger = LogManager.GetLogger<PluginServiceManager>();

        public static async Task<BackgroundJobServerOptions> InitServer(string path, int workerCount)
        {
            if (string.IsNullOrEmpty(path))
            {
                var server = StorageService.Provider.GetServer(ServerName);
                if (server != null) path = server.PlugPath;
            }

            await UpdateServer(path);

            var queues = StorageService.Provider.GetQueues(null, ServerName);
            var options = new BackgroundJobServerOptions
            {
                WorkerCount = workerCount,
                Queues = queues.Select(s => s.Name).ToArray()
            };
            return options;
        }

        public static async Task UpdateServer(string path)
        {
            await ServerRefreshCommad.Invoke(path);
            await BatchImportCommand.ScanBatches(path);
        }

        public static void LoadDynamic()
        {
            var plugins = StorageService.Provider.GetPluginDefines();
            foreach (var plugin in plugins)
            {
                var sets = plugin.GetJobSets();
                foreach (var ass in sets)
                {
                    var jobs = ass.GetJobs();
                    foreach (var job in jobs)
                    {
                        TypeFactory.Create<JobExecute>(plugin.PathName, ass.ShortName, job.Name, job.Title);
                    }
                }
            }

            AppDomain.CurrentDomain.SetupInformation.PrivateBinPath = TypeFactory.DynamicPath;
        }

    }

}
