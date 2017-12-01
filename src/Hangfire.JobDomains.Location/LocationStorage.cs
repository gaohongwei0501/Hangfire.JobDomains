using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.JobDomains.Models;
using Hangfire.JobDomains.Storage;
using System.Collections.Concurrent;

namespace Hangfire.JobDomains.Location
{

    public class LocationStorage : IDomainStorage
    {

        static ConcurrentDictionary<string, DomainDefine> Storage = new ConcurrentDictionary<string, DomainDefine>();

        public bool SetConnectString(string connectString) => true;

        public bool IsEmpty { get { return Storage.IsEmpty; } }

        public bool Add(string key, DomainDefine define)
        {
           return Storage.TryAdd(key, define);
        }

        public List<DomainDefine> GetAll()
        {
            return Storage.Select(s => s.Value).ToList();
        }

      
    }

}
