using Hangfire.JobDomains.Models;
using Hangfire.JobDomains.Storage.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Storage.EntityFrameworkCore.Sqlite
{
    public class SQLiteStorage : EFCoreStorage
    {
        public override EFCoreDBContext GetContext() => new SQLiteDBContext();

        public override bool TransactionEnable => true;

        public override bool AddService(string connectString)
        {
            SQLiteDBContext.ConnectionString = connectString;
            return SQLiteDBContext.CanService();
        }
    }
}
