using Hangfire.JobDomains.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestDLL
{

    [Nameplate("测试任务", "这是一个测试任务")]
    public class HelloWorld : MarshalByRefObject,IPrefabrication
    {
        private string Owner { get; set; }

        private DateTime Time { get; set; } = DateTime.MinValue;

        public HelloWorld() : this(" I ") { }

        public HelloWorld(string owner) : this(owner,DateTime.Now) { }

        public HelloWorld(string owner, DateTime time) : this(time, owner) { }

        public HelloWorld(DateTime time,string owner)
        {
            Owner = owner;
            Time = time;
        }

        public string Invoke(string name)
        {
            return $"{Time}| {Owner} say :HelloWorld ,{ name }!";
        }

        public bool Test()
        {
            return true;
        }

        public void Dispatch()
        {
            Console.WriteLine($" {Time}| I say :HelloWorld ,{Owner}!");
        }
    }

}
