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

        public static IDomainStorage Storage { get; set; }

        public List<DomainDefine> GetDomainDefines()
        {
            //var list = new List<DomainDefine>();
            //var path = SysSetting.GetValue<string>(SysSettingKey.BasePath);
            //if (string.IsNullOrEmpty(path)) return list;
            return Storage.GetAll();
        }

        public bool IsEmpty => Storage.IsEmpty;

        public bool Add(string path, DomainDefine define) => Storage.Add(path, define);

    }



}
