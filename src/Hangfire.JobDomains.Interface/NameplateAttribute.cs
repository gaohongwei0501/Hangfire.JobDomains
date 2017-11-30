using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Interface
{
    public class NameplateAttribute : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public NameplateAttribute(string title = "", string description = "", string queue = "default")
        {
            Title = title;
            Description = description;
            Queue = queue;
        }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 队列
        /// </summary>
        public string Queue { get; set; }

    }


}
