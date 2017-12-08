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

        public bool AddOrUpdateServerAsync(ServerDefine server)
        {
            Server = server;
            return true;
        }

        public bool UpdateServerDomains(string server, List<string> domains)
        {
            return true;
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

        #region DomainDefine

        static ConcurrentDictionary<string, DomainDefine> Storage = new ConcurrentDictionary<string, DomainDefine>();

        public bool SetConnectString(string connectString) => true;

        public bool IsDomainsEmpty () => Storage.IsEmpty;

        public bool AddDomainAsync(DomainDefine define)
        {
            return Storage.TryAdd(define.BasePath, define);
        }

        public List<DomainDefine> GetAllDomains()
        {
            return Storage.Select(s => s.Value).ToList();
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
