using Hangfire.JobDomains.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Models
{

    /// <summary>
    /// 任务定义
    /// </summary>
    internal class JobDefine
    {

        public JobDefine(Type type)
        {
            Type = type;
            init();
        }

        void init()
        {
            var attr = Type.ReadReflectionOnlyTypeAttribute<NameplateAttribute>();
            Nameplate = attr;
        }

        /// <summary>
        /// 类型
        /// </summary>
        public Type Type { get;private set; }

        /// <summary>
        /// 任务描述属性
        /// </summary>
        public NameplateAttribute Nameplate { get; private set; }

        /// <summary>
        /// 完全限定名
        /// </summary>
        public string FullName { get { return Type.FullName; } }

        /// <summary>
        /// 名称 类名
        /// </summary>
        public string Name { get { return Type.Name; } }

        /// <summary>
        /// 标题 
        /// </summary>
        public string Title {
            get {
                if (Nameplate == null) return Name;
                return Nameplate.Title;
            }
        }

        /// <summary>
        /// 描述 
        /// </summary>
        public string Description {
            get
            {
                if (Nameplate == null) return string.Empty;
                return Nameplate.Description;
            }
        }

    }
}
