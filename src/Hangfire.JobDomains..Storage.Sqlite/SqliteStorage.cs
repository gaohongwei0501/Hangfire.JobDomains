using Hangfire.JobDomains.Models;
using Hangfire.JobDomains.Storage.Sqlite.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Storage.Sqlite
{
    internal static class EntityMapper
    {
        public static Domain Convert(this DomainDefine define, int ServerID)
        {
            return new Domain
            {
                BasePath = define.BasePath,
                Name = define.Name,
                Description = define.Description,
                CreatedAt = DateTime.Now,
                ServerGroupId = ServerID,
            };
        }
    }


    public class SQLiteStorage : IDomainStorage
    {

        static int GroupID { get; set; }

        static int ServerID { get; set; }


        public bool SetConnectString(string connectString)
        {
            SQLiteDBContext.ConnectionString = connectString;
            return SQLiteDBContext.CanService();
        }

        public bool IsDomainsEmpty
        {
            get
            {
                using (var context = new SQLiteDBContext())
                {
                    var model = context.ServerGroups.Take(100).ToList();
                    return context.Domains.Where(s => s.ServerGroupId == GroupID).Count() == 0;
                }
            }
        }

        public bool AddDomain(DomainDefine define)
        {
            using (var context = new SQLiteDBContext())
            {
                
                 


                 var model = context.ServerGroups.Take(100).ToList();
                // 业务代码
            }
            return true;
        }



        public List<DomainDefine> GetAllDomains()
        {
            throw new NotImplementedException();
        }


        public Dictionary<SysSettingKey, string> GetSysSetting()
        {
            throw new NotImplementedException();
        }

        public bool SetSysSetting(SysSettingKey key, string value)
        {
            throw new NotImplementedException();
        }


        public Dictionary<int, string> GetJobCornSetting()
        {
            throw new NotImplementedException();
        }

        public bool AddJobCornSetting(int key, string value)
        {
            throw new NotImplementedException();
        }

        public bool DeleteJobCornSetting(int key)
        {
            throw new NotImplementedException();
        }


    }
}
