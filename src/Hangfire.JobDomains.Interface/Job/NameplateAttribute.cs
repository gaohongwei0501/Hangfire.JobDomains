using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Interface
{
    public class NameplateAttribute : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public NameplateAttribute(string title = "", string description = "")
        {
            Title = title;
            Description = description;
        }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
   
    }


}
