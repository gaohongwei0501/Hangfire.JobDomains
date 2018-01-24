using Hangfire.JobDomains.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Models
{

    /// <summary>
    /// 程序集定义
    /// </summary>
    public class AssemblyDefine
    {
        public PluginDefine Parent { get; private set; }

        /// <summary>
        /// 全名
        /// </summary>
        public string FullName { get; private set; }

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// 简称 
        /// </summary>
        public string ShortName { get; private set; }

        /// <summary>
        /// 程序集标题 AssemblyTitleAttribute
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// 程序集描述 AssemblyDescription
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// 配件
        /// </summary>
        List<JobDefine> _jobs { get;  set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public AssemblyDefine(PluginDefine parent, string file, string fullname, string shortname, string title, string description)
        {
            Parent = parent;
            FileName = file;
            FullName = fullname;
            ShortName = shortname;
            Title = title;
            Description = description;
        }

        public void SetJobs(IEnumerable<JobDefine> jobs)
        {
            if (jobs == null || jobs.Count() == 0) return;
            _jobs = new List<JobDefine>(jobs);
        }

        public List<JobDefine> InnerJobs { get { return _jobs; } }

        public List<JobDefine> GetJobs()
        {
            if (_jobs != null) return _jobs;
            return Storage.StorageService.Provider.GetJobs(this);
        }

      
    }
}
