using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Storage.EntityFrameworkCore.Entities
{
    public class Assembly : SQLiteEntityBase
    {

        public int PluginID { get; set; }

        /// <summary>
        /// 全名
        /// </summary>
        public string FullName { get;  set; }

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get;  set; }

        /// <summary>
        /// 简称 
        /// </summary>
        public string ShortName { get;  set; }

        /// <summary>
        /// 程序集标题 AssemblyTitleAttribute
        /// </summary>
        public string Title { get;  set; }

        /// <summary>
        /// 程序集描述 AssemblyDescription
        /// </summary>
        public string Description { get;  set; }

    }
}
