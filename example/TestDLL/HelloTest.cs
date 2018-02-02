using Hangfire.PluginPackets.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDLL
{

    [Nameplate("测试任务", "这是一个测试任务")]
    public class HelloTest : IPrefabrication
    {

        public bool Test()
        {
            return true;
        }

        public void Dispatch()
        {
            
        }

        [Nameplate("测试方法", "这是一个测试任务")]
        public void TestDispatch()
        {

        }

    }
}
