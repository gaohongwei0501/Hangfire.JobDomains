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

        Task<bool> AddOrUpdateServerAsync(ServerDefine server, List<string> domains);

        List<ServerDefine> GetServers();

        ServerDefine GetServer(string server);

        List<string> GetServersByDomain(string domain);

        List<string> GetServersByQueue(string queue);

        List<QueueDefine> GetCustomerQueues();

        List<QueueDefine> GetCustomerQueues(string server);

        List<DomainDefine> GetAllDomains();

        List<AssemblyDefine> GetAssemblies(DomainDefine domain);

        List<JobDefine> GetJobs(AssemblyDefine assembly);

        List<ConstructorDefine> GetConstructors(JobDefine job);

        Task<bool> AddDomainAsync(DomainDefine define);

        Dictionary<SysSettingKey, string> GetSysSetting();

        bool SetSysSetting(SysSettingKey key, string value);

        Dictionary<int, string> GetJobCornSetting();

        bool AddJobCornSetting(int key, string value);

        bool DeleteJobCornSetting(int key);


    }

}
