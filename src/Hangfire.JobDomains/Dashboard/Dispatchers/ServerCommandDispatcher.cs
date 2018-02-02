using Hangfire.PluginPackets.Models;
using Hangfire.PluginPackets.Server;
using Hangfire.PluginPackets.Storage;
using Hangfire.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Dashboard.Dispatchers
{
    internal class ServerCommandDispatcher : CommandDispatcher<JsonData>
    {
        public override Task<JsonData> Exception(Exception ex)
        {
            return Task.FromResult(new JsonData(ex, null));
        }

        public ServerDefine TheServer { get; private set; }


        public override async Task<JsonData> Invoke()
        {
            var cmd = await GetFromValue("cmd");
            var serverCmd = ServerPageCommand.None;
            bool right= Enum.TryParse<ServerPageCommand>(cmd, out serverCmd);
            if (!right|| serverCmd == ServerPageCommand.None) throw (new Exception("未知指令"));

            var serverName = await GetFromValue("server");
            TheServer = StorageService.Provider.GetServer(serverName);
            if(TheServer==null) throw (new Exception("未知服务器"));

            switch (serverCmd)
            {
                case  ServerPageCommand.EditPath:return await EditPath(); 
                
            }

            //var start = await GetFromValue<DateTime>("start", DateTime.MinValue);
            //var period = await GetFromValue<int>("period", 0);
            //var queue = (await GetFromValue("queue")).ToLower();
            //var jobSign = await GetFromValue("sign");

            //JobData = await GetDictionaryValue("data");
            //var paramers = JobData?.Select(s => s.Value).ToArray();

            //var set = StorageService.Provider.GetDomainDefines();
            //TheDomain = set.SingleOrDefault(s => s.Title == domain);
            //TheAssembly = TheDomain?.GetJobSets().SingleOrDefault(s => s.ShortName == assembly);
            //TheJob = TheAssembly?.GetJobs().SingleOrDefault(s => s.Name == job);

            //if (TheJob == null) throw (new Exception("未正确定位到工作任务."));
            //if (jobCmd == JobPageCommand.None) throw (new Exception("未正确定位到任务指令."));

            //switch (jobCmd)
            //{
            //    case JobPageCommand.Schedule: Schedule(queue, start, period, jobSign, paramers); break;
            //    case JobPageCommand.Delay: Delay(queue, start, paramers); break;
            //    case JobPageCommand.Immediately: JobTest(queue, paramers); break;
            //    case JobPageCommand.Test: JobTest(queue, paramers); break;
            //}

            return new JsonData
            {
                IsSuccess = true,
                Message = "提交成功.",
                Url = "",
            };
        }

        public  async Task<JsonData> EditPath()
        {
            var path= await GetFromValue("path");
            if (TheServer.PlugPath == path) throw (new Exception("路径地址并没有变化"));

            var queue = StorageService.Provider.GetSelfQueue(TheServer.Name);
            IBackgroundJobClient hangFireClient = new BackgroundJobClient();
            EnqueuedState state = new Hangfire.States.EnqueuedState(queue.Name);
            hangFireClient.Create(()=> PluginServiceManager.Restart(path), state);

            return new JsonData
            {
                IsSuccess = true,
                Message = "任务已经设置,请等待任务完成。",
                Url = "",
            };
        }



    }
}
