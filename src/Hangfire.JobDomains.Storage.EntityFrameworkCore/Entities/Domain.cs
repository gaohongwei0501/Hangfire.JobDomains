using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Storage.EntityFrameworkCore.Entities
{
    public class Domain : SQLiteEntityBase
    {
        public string PathName { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

    }
}
