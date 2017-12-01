using Hangfire.JobDomains.Interface;
using Hangfire.JobDomains.Loader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Server
{
    internal class JobInvoke
    {
        public static void Invoke(string path, string assembly, string job, object[] paramers)
        {
            using (PluginHost host = new PluginHost(path))
            {
                if (host.LoadPlugins())
                {
                    Sponsor<object> objectFromPlugin = host.GetInstance(assembly, job, paramers);
                    if (objectFromPlugin == null) throw (new Exception("任务类构造函数的参数未正确提供不能实例化，或任务类未继承 MarshalByRefObject 而不能被发现"));
                    IPrefabrication instance = objectFromPlugin.Instance as IPrefabrication;
                    instance.Dispatch();
                }
                else
                {
                    throw (new Exception("no load"));
                }
            }
        }

        public static bool Test(string path, string assembly, string job, object[] paramers)
        {
            using (PluginHost host = new PluginHost(path))
            {
                if (host.LoadPlugins())
                {
                    Sponsor<object> objectFromPlugin = host.GetInstance(assembly, job, paramers);
                    if (objectFromPlugin == null) throw (new Exception("任务类构造函数的参数未正确提供不能实例化，或任务类未继承 MarshalByRefObject 而不能被发现"));
                    IPrefabrication instance = objectFromPlugin.Instance as IPrefabrication;
                    return instance.Test();
                }
                else
                {
                    throw (new Exception("no load"));
                }
            }
        }
    }
}
