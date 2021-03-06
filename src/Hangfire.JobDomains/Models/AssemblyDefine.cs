﻿using Hangfire.JobDomains.Interface;
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
        public List<JobDefine> Jobs { get; private set; } = new List<JobDefine>();


        public void AddJob(JobDefine job)
        {
            if (job != null) Jobs.Add(job);
        }

        public void AddJob(IEnumerable<JobDefine> jobs)
        {
            if (jobs != null) Jobs.AddRange(jobs);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public AssemblyDefine(string file, string fullname, string shortname, string title, string description, IEnumerable<JobDefine> jobs = null)
        {
            if(jobs!=null) Jobs.AddRange(jobs);
            FileName = file;
            FullName = fullname;
            ShortName = shortname;
            Title = title;
            Description = description;
        }

     

    }
}
