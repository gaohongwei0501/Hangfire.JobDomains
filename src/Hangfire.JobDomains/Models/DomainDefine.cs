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
using Hangfire.JobDomains.Storage;

namespace Hangfire.JobDomains.Models
{
   

    public class DomainDefine
    {

        public string PathName { get; private set; }

        public string Title { get; private set; }

        public string Description { get; private set; }

        List<AssemblyDefine> _jobSets = null;

        public DomainDefine(string path)
        {
            PathName = path;
            Title = path;
        }

        public DomainDefine(string path, string name , string description)
        {
            PathName = path;
            Title = string.IsNullOrEmpty(name) ? path : name;
            Description = description;
        }

        public void SetJobSets(IEnumerable<AssemblyDefine> assemblyDefines)
        {
            if (assemblyDefines == null || assemblyDefines.Count() == 0) return;
            _jobSets = new List<AssemblyDefine>(assemblyDefines);
        }

        public List<AssemblyDefine> InnerJobSets { get { return _jobSets; } }

        public List<AssemblyDefine> GetJobSets()
        {
            if (_jobSets != null) return _jobSets;
            return StorageService.Provider.GetAssemblies(this);
        }
        
    }
}
