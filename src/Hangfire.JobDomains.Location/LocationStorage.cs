using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.JobDomains.Models;
using Hangfire.JobDomains.Storage;
using System.Collections.Concurrent;

namespace Hangfire.JobDomains.Storage.Location
{

    public class LocationStorage : IDomainStorage
    {

        #region ServerDefine

        static ServerDefine Server { get; set; }

        Task<bool> IDomainStorage.AddOrUpdateServerAsync(ServerDefine server, List<string> domains)
        {
            Server = server;
            Server.Queues = new List<QueueDefine>();
            QueueDefine def = new QueueDefine { Name = "default", Description = "默认队列", Title = "默认队列" };
            QueueDefine loacl = new QueueDefine { Name = Environment.MachineName.ToLower(), Description = "服务器队列", Title = $"{Environment.MachineName}机器队列" };
            Server.Queues.Add(def);
            Server.Queues.Add(loacl);
            return Task.FromResult(true);
        }

        public List<string> GetServersByDomain(string domain)
        {
            return new List<string> { Server.Name };
        }

        public List<ServerDefine> GetServers()
        {
            return new List<ServerDefine> { Server };
        }

        public ServerDefine GetServer(string server)
        {
            if (server != Server.Name) return null;
            Server.Domains = Storage.Select(s => s.Value).ToList();
            return Server;
        }

        #endregion

        #region QueueDefine

        public List<QueueDefine> GetQueues()
        {
            return Server.Queues;
        }

        public QueueDefine GetQueue(string queueName)
        {
            var queue = Server.Queues.FirstOrDefault(s => s.Name == queueName);
            if (queue.Servers == null) queue.Servers = new List<ServerDefine> { Server };
            return queue;
        }

        public List<QueueDefine> GetQueuesByDomain(string domain)
        {
            return Server.Queues;
        }

        #endregion


        #region DomainDefine

        static ConcurrentDictionary<string, DomainDefine> Storage = new ConcurrentDictionary<string, DomainDefine>();

        public bool AddService(string connectString) => true;

        public bool IsDomainsEmpty() => Storage.IsEmpty;

        Task<bool> IDomainStorage.AddDomainAsync(DomainDefine define)
        {
            var result = Storage.TryAdd(define.PathName, define);
            return Task.FromResult(result);
        }

        public List<DomainDefine> GetAllDomains()
        {
            return Storage.Select(s => s.Value).ToList();
        }


        public List<AssemblyDefine> GetAssemblies(DomainDefine domainDefine)
        {
            if (domainDefine == null) return new List<AssemblyDefine>();

            var domain = Storage.Select(s => s.Value).FirstOrDefault(s => s.Title == domainDefine.Title);
            return domain == null || domain.InnerJobSets == null ? new List<AssemblyDefine>() : domain.InnerJobSets;
        }

        public List<JobDefine> GetJobs(AssemblyDefine assemblyDefine)
        {
            if (assemblyDefine == null) return new List<JobDefine>();
            var domainDefine = assemblyDefine.Parent;
            if (domainDefine == null) return new List<JobDefine>();

            var domain = Storage.Select(s => s.Value).FirstOrDefault(s => s.Title == domainDefine.Title);
            if (domain == null) return new List<JobDefine>();
            var assembly = domain.GetJobSets().FirstOrDefault(s => s.ShortName == assemblyDefine.ShortName);
            return assembly == null || assembly.InnerJobs == null ? new List<JobDefine>() : assembly.InnerJobs;
        }

        public List<ConstructorDefine> GetConstructors(JobDefine jobDefine)
        {
            if (jobDefine == null) return new List<ConstructorDefine>();
            var assemblyDefine = jobDefine.Parent;
            if (assemblyDefine == null) return new List<ConstructorDefine>();
            var domainDefine = assemblyDefine.Parent;
            if (domainDefine == null) return new List<ConstructorDefine>();

            var domain = Storage.Select(s => s.Value).FirstOrDefault(s => s.Title == domainDefine.Title);
            if (domain == null) return new List<ConstructorDefine>();
            var assembly = domain.GetJobSets().FirstOrDefault(s => s.ShortName == assemblyDefine.ShortName);
            if (assembly == null) return new List<ConstructorDefine>();
            var job = assembly.GetJobs().FirstOrDefault(s => s.Name == jobDefine.Name);
            if (job == null || job.InnerConstructors == null) return new List<ConstructorDefine>();

            return job.InnerConstructors;
        }

        #endregion

        #region SysSetting

        static readonly SysSetting sysSetting = new SysSetting();


        public Dictionary<SysSettingKey, string> GetSysSetting()
        {
            return sysSetting.GetValue();
        }

        public bool SetSysSetting(SysSettingKey key, string value)
        {
            return sysSetting.SetValue(key, value);
        }

        #endregion

        #region JobCornSetting


        static readonly JobCornSetting jobCornSetting = new JobCornSetting();

        public Dictionary<int, string> GetJobCornSetting()
        {
            return jobCornSetting.GetValue();
        }

        public bool AddJobCornSetting(int key, string value)
        {
            return jobCornSetting.SetValue(key, value);
        }

        public bool DeleteJobCornSetting(int key)
        {
            return jobCornSetting.DeleteValue(key);
        }



        #endregion

    }

}
