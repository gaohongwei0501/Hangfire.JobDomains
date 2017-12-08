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
   

    public class DomainDefine
    {

        public string Name { get; private set; }

        public string BasePath { get; private set; }

        public string Description { get; private set; }

        List<AssemblyDefine> _jobSets = null;

        public DomainDefine(string path)
        {
            BasePath = path;
            var index = path.LastIndexOf("\\");
            Name = path.Substring(index + 1);
        }

        public DomainDefine(string path, string name , string description)
        {
            BasePath = path;
            Name = name;
            Description = description;
        }

        public DomainDefine(string path, IEnumerable<AssemblyDefine> sets,string name="",string description="") : this(path)
        {
            _jobSets = new List<AssemblyDefine>();
            _jobSets.AddRange(sets);
            if (string.IsNullOrEmpty(name) == false) Name = name;
            Description = description;
        }

        public List<AssemblyDefine> GetJobSets()
        {
            if (_jobSets != null) return _jobSets;


            return null;
        }
        
    }
}
