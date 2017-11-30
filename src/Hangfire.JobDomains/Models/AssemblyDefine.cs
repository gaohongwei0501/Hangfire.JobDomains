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
    internal class AssemblyDefine
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
        /// 配件箱
        /// </summary>
        public Assembly Define { get; private set; }

        /// <summary>
        /// 配件
        /// </summary>
        public List<JobDefine> Jobs { get; private set; }


        /// <summary>
        /// 构造函数
        /// </summary>
        public AssemblyDefine(Assembly define, List<JobDefine> jobs)
        {
            Define = define;
            Jobs = jobs;
            init();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public AssemblyDefine(string path)
        {
            var info = new FileInfo(path);
            FileName = info.FullName;
            ShortName = info.Name.Replace(".dll", string.Empty);
        }

        void init()
        {
            FileName= Define.Location;
            FullName = Define.FullName;
            ShortName = Define.ManifestModule.Name.Replace(".dll",string.Empty);
            Title = Define.ReadReflectionOnlyAssemblyAttribute<AssemblyTitleAttribute>();
            Description = Define.ReadReflectionOnlyAssemblyAttribute<AssemblyDescriptionAttribute>();
        }

    }
}
