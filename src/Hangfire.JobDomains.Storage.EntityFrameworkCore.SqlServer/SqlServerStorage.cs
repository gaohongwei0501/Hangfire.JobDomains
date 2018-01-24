using Hangfire.JobDomains.Models;
using Hangfire.JobDomains.Storage.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Storage.EntityFrameworkCore.SqlServer
{

    public class SqlServerStorage : EFCoreStorage
    {
        public override bool TransactionEnable => true;

        public override EFCoreDBContext GetContext()=> new SqlServerDBContext();

        public override bool AddService(string nameOrConnectionString)
        {
            if (nameOrConnectionString == null) throw new ArgumentNullException(nameof(nameOrConnectionString));
            SqlServerDBContext.ConnectionString = GetConnectionString(nameOrConnectionString) ;
            return SqlServerDBContext.CanService();
        }
       
    }
}
