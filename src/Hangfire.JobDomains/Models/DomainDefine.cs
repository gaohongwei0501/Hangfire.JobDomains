using Hangfire.JobDomains.Loader;
using Hangfire.JobDomains.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Models
{
    internal class DomainDefine
    {

        public string Name { get; private set; }

        public string BasePath { get; private set; }

        public string Description { get; private set; }

        public List<AssemblyDefine> JobSets { get; private set; } = new List<AssemblyDefine>();

        public DomainDefine(string path,IEnumerable<AssemblyDefine> sets)
        {
            BasePath = path;
            var index = path.LastIndexOf("\\");
            Name = path.Substring(index + 1);
            JobSets.AddRange(sets);
        }

        public List<AssemblyDefine> GetJobSets()
        {
            return JobSets.Where(s=>s.Define!=null).ToList();
        }
        
    }
}
