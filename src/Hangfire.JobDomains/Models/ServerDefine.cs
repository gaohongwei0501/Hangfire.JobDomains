using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Models
{

    public class ServerDefine
    {

        public string Name { get; private set; } = Environment.MachineName.ToLower();


        public string PlugPath { get; set; }


        public List<DomainDefine> Domains { get; set; }

    }


}
