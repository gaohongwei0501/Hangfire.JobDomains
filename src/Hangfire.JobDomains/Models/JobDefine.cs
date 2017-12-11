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
    public class JobDefine
    {
        public AssemblyDefine Parent { get; private set; }

        public JobDefine(AssemblyDefine parent,string fullname, string name, string title, string description)
        {
            Parent = parent;
            FullName = fullname;
            Name = name;
            Nameplate = new NameplateAttribute(title, description);
        }

        public JobDefine(AssemblyDefine parent, string fullname, string name, IEnumerable<ConstructorDefine> constructors , NameplateAttribute attr = null)
        {
            if (constructors == null) throw (new Exception("任务类构造函数加载失败！"));
            _constructors = new List<ConstructorDefine>(constructors);

            Parent = parent;
            FullName = fullname;
            Name = name;
            Nameplate = attr;
        }

        /// <summary>
        /// 任务描述属性
        /// </summary>
        NameplateAttribute Nameplate { get;  set; }

        /// <summary>
        /// 完全限定名
        /// </summary>
        public string FullName { get; private set; }

        /// <summary>
        /// 名称 类名
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 构造函数集合
        /// </summary>
        List<ConstructorDefine> _constructors { get;  set; } 

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

        public List<ConstructorDefine> GetConstructors()
        {
            if (_constructors != null) return _constructors;
            return Storage.StorageService.Provider.GetConstructors(this);
        }
    }
}
