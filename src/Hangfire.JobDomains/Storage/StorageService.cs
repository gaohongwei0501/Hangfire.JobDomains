using Hangfire.JobDomains.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Storage
{
    internal class StorageService
    {

        public readonly static StorageService Provider = new StorageService();

        static IDomainStorage Storage { get; set; }

        public Task<bool> ClearServer(string serverName)=> Storage.ClearServer(serverName);

        public bool SetStorage(IDomainStorage store, string connectString)
        {
            if (Storage == null) Storage = store;
            return Storage.AddService(connectString);
        }

        public Task<bool> AddOrUpdateServerAsync(ServerDefine server, List<string> pluginNames) => Storage.AddOrUpdateServerAsync(server, pluginNames);

        public List<ServerDefine> GetServers(Hangfire.JobStorage hangfireStorage)
        {
            var monitor = hangfireStorage.GetMonitoringApi();
            var activeServers = monitor.Servers();
            var actives = activeServers.Select(s => SubServerName(s.Name)).ToList();

            var servers = Storage.GetServers();
            return servers.Where(s=> actives.Contains(s.Name)).ToList();
        }

        string SubServerName(string server)
        {
            var index = server.IndexOf(":");
            return index > 0 ? server.Substring(0, index) : server;
        }

        public List<string> GetServersByQueue(Hangfire.JobStorage hangfireStorage, string queue)
        {
            var monitor = hangfireStorage.GetMonitoringApi();
            var servers = monitor.Servers();
            var serverNames = servers.Select(s=>SubServerName(s.Name)).Distinct();
            if (queue == "default") return new List<string>();
            if (serverNames.Contains(queue)) return new List<string> { queue };
            return Storage.GetServersByQueue(queue);
        }


        public ServerDefine GetServer(Hangfire.JobStorage hangfireStorage, string serverName)
        {
            var Server = Storage.GetServer(serverName);
            Server.Queues = GetQueues(hangfireStorage, serverName);
            return Server;
        }

        public ServerDefine GetServer( string serverName)
        {
            return Storage.GetServer(serverName);
        }

        public List<string> GetServersByDomain(string domain) => Storage.GetServersByDomain(domain);

        public List<QueueDefine> GetQueues(Hangfire.JobStorage hangfireStorage)
        {
            var queues = GetActiveServerQueues(hangfireStorage).ToList();
            var customerQueues = Storage.GetCustomerQueues();
            queues.AddRange(customerQueues);
            return queues;
        }

        public QueueDefine GetSelfQueue(string serverName)
        {
            QueueDefine loacl = new QueueDefine { Name = $"{serverName}_SelfQueue".ToLower(), Description = "服务器队列" };
            return loacl;
        }

        public List<QueueDefine> GetQueues(Hangfire.JobStorage hangfireStorage, string serverName)
        {
            QueueDefine loacl = GetSelfQueue(serverName);
            var queues = Storage.GetCustomerQueues(serverName);
            queues.Add(QueueDefine.defaultValue);
            queues.Add(loacl);
            if (hangfireStorage != null) {
                var actives = GetActiveServerQueues(hangfireStorage, serverName);
                foreach (var queue in queues)
                {
                    var one = actives.FirstOrDefault(s=>s.Name== queue.Name);
                    if (one != null) { queue.IsActive = true; }
                }
            }
            return queues;
        }

        IEnumerable<QueueDefine> GetActiveServerQueues(Hangfire.JobStorage hangfireStorage, string serverName)
        {
            var monitor = hangfireStorage.GetMonitoringApi();
            var server = monitor.Servers().FirstOrDefault(s => SubServerName(s.Name) == serverName);
            if (server == null) return new List<QueueDefine>();
            return server.Queues.Where(s => s != "default").Select(s => new QueueDefine { Name = s, Description = "服务器队列", IsActive = true });
        }

        IEnumerable<QueueDefine> GetActiveServerQueues(Hangfire.JobStorage hangfireStorage)
        {
            var monitor = hangfireStorage.GetMonitoringApi();
            var servers = monitor.Servers().ToList();
            var list = new List<string>();
            servers.ForEach(s => list.AddRange(s.Queues));
            return list.Distinct().Where(s => s != "default").Select(s => new QueueDefine { Name = s, Description = "服务器队列" });
        }

        public IEnumerable<string> GetQueuesByDomain(Hangfire.JobStorage hangfireStorage, string domainName)
        {
            var domainServers = GetServersByDomain(domainName);
            var monitor = hangfireStorage.GetMonitoringApi();
            var activeServers = monitor.Servers();

            #region 激活的【任务服务器】（可以执行指定插件的任务服务器）的队列集

            var activeDomainServers = activeServers.Where(s => domainServers.Contains(SubServerName(s.Name))).ToList();
            var domainQueues = new List<string>();
            activeDomainServers.ForEach(s => domainQueues.AddRange(s.Queues));
            var domainDistincts = domainQueues.Distinct().Where(s => s != "default");

            #endregion

            #region 激活的非【任务服务器】的队列集

            var otherActiveDomainServers = activeServers.Where(s => domainServers.Contains(SubServerName(s.Name)) == false).ToList();
            var otherQueues = new List<string>();
            otherActiveDomainServers.ForEach(s => otherQueues.AddRange(s.Queues));
            var otherDistincts = otherQueues.Distinct();

            #endregion

            return domainDistincts.Where(s => otherDistincts.Contains(s) == false);
        }

        public QueueDefine GetQueue(Hangfire.JobStorage hangfireStorage, string queueName)
        {
            if (queueName == "default") return QueueDefine.defaultValue;
            var queues = GetActiveServerQueues(hangfireStorage);
            var queue = queues.FirstOrDefault(s => s.Name == queueName);
            if (queue == null) queue = new QueueDefine { Name = queueName, Description = "自定义队列" };
            return queue;
        }

        public List<DomainDefine> GetDomainDefines() => Storage.GetAllDomains();

        public List<AssemblyDefine> GetAssemblies(DomainDefine domain) => Storage.GetAssemblies(domain);

        public List<JobDefine> GetJobs(AssemblyDefine assembly) => Storage.GetJobs(assembly);

        public List<ConstructorDefine> GetConstructors(JobDefine job) => Storage.GetConstructors(job);

        public Task<bool> AddDomainAsync(DomainDefine define) => Storage.AddDomainAsync(define);

        public Dictionary<SysSettingKey, string> GetSysSetting()
        {
            return Storage.GetSysSetting();
        }

        public bool SetSysSetting(SysSettingKey key,string value)
        {
            return Storage.SetSysSetting(key, value);
        }

        public Dictionary<int, string> GetJobCornSetting()
        {
            return Storage.GetJobCornSetting();
        }

        public bool AddJobCornSetting(int key, string value)
        {
            return Storage.AddJobCornSetting(key, value);
        }

        public bool DeleteJobCornSetting(int key)
        {
            return Storage.DeleteJobCornSetting(key);
        }

    }



}
