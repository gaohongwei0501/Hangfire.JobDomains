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

        public bool SetStorage(IDomainStorage store,string connectString)
        {
            if (Storage == null) Storage = store;
            return Storage.SetConnectString(connectString);
        }

        public Task<bool> AddOrUpdateServerAsync(ServerDefine server)=> Storage.AddOrUpdateServerAsync(server);

        public Task<bool> UpdateServerDomains(string server,List<string> domains) => Storage.UpdateServerDomainMapAsync(server, domains);

        public List<ServerDefine> GetServers() => Storage.GetServers();

        public ServerDefine GetServer(string server) => Storage.GetServer(server);

        public List<string> GetServersByDomain(string domain) => Storage.GetServersByDomain(domain);


        public List<DomainDefine> GetDomainDefines() => Storage.GetAllDomains();

        public List<AssemblyDefine> GetAssemblies(DomainDefine domain) => Storage.GetAssemblies(domain);

        public List<JobDefine> GetJobs(AssemblyDefine assembly) => Storage.GetJobs(assembly);

        public List<ConstructorDefine> GetConstructors(JobDefine job) => Storage.GetConstructors(job);

        public bool IsDomainsEmpty => Storage.IsDomainsEmpty();

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
