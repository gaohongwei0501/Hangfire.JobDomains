using Hangfire.JobDomains.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Storage
{

    public interface IDomainStorage
    {
        bool AddService(string connectString);

        Task<bool> ClearServer(string serverName);

        Task<bool> AddOrUpdateServerAsync(ServerDefine server, List<string> pluginNames);

        List<ServerDefine> GetServers();

        ServerDefine GetServer(string server);

        List<string> GetServersByPlugin(string domain);

        List<string> GetServersByQueue(string queue);

        List<QueueDefine> GetCustomerQueues();

        List<QueueDefine> GetCustomerQueues(string server);

        List<PluginDefine> GetAllPlugins();

        List<PluginDefine> GetPlugins(string server);

        List<AssemblyDefine> GetAssemblies(PluginDefine domain);

        List<JobDefine> GetJobs(AssemblyDefine assembly);

        List<ConstructorDefine> GetConstructors(JobDefine job);

        Task<bool> AddPluginAsync(PluginDefine define);

    }

}
