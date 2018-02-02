using Common.Logging;
using Hangfire.PluginPackets.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestDLL
{

    [Nameplate("测试任务", "这是一个测试任务")]
    public class HelloWorld :IPrefabrication
    {
        static ILog loger = LogManager.GetLogger<HelloWorld>();

        private string People { get; set; }

        private DateTime Time { get; set; } = DateTime.MinValue;

        public HelloWorld() : this(" I ") { }

        public HelloWorld(string people) : this(people,DateTime.Now) { }

        public HelloWorld(string people, DateTime time) : this(time, people) { }

        public HelloWorld(DateTime time,string people)
        {
            People = people;
            Time = time;
        }

        string SayStyle
        {
            get
            {
                if (Time < DateTime.Now)
                {
                    return " said ";
                }
                else {
                    return " will say ";
                }
            }
        }

        public bool Test()
        {
            loger.Info($"测试任务|Test :On { Time } I { SayStyle } :HelloWorld ,{ People } !");
            return true;
        }

        public void Dispatch()
        {
            loger.Info($"测试任务|Dispatch :On { Time } I { SayStyle } :HelloWorld ,{ People } !");
        }
    }

}
