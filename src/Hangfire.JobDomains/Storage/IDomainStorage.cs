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

        Task<bool> AddOrUpdateServerAsync(ServerDefine server);

        Task<bool> UpdateServerDomainMapAsync(string server, List<string> domains);

        List<ServerDefine> GetServers();

        ServerDefine GetServer(string server);

        List<string> GetServersByDomain(string domain);

        bool IsDomainsEmpty();

        bool SetConnectString(string connectString);

        List<DomainDefine> GetAllDomains();

        Task<bool> AddDomainAsync(DomainDefine define);

        Dictionary<SysSettingKey, string> GetSysSetting();

        bool SetSysSetting(SysSettingKey key, string value);

        Dictionary<int, string> GetJobCornSetting();

        bool AddJobCornSetting(int key, string value);

        bool DeleteJobCornSetting(int key);


    }

}
