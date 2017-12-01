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
        bool IsEmpty { get; }

        bool SetConnectString(string connectString);

        List<DomainDefine> GetAll();

        bool Add(string path, DomainDefine define);
    }

}
