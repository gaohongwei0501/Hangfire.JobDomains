using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Storage.Sqlite.Entities
{
    internal class Job : SQLiteEntityBase
    {

        public int DomainID { get; set; }

        public int AssemblyID { get; set; }

        /// <summary>
        /// 完全限定名
        /// </summary>
        public string FullName { get;  set; }

        /// <summary>
        /// 名称 类名
        /// </summary>
        public string Name { get;  set; }

        /// <summary>
        /// 标题 
        /// </summary>
        public string Title { get;  set; }

        /// <summary>
        /// 描述 
        /// </summary>
        public string Description { get;  set; }

    }
}