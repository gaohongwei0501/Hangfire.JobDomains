﻿using Hangfire.PluginPackets.Models;
using Hangfire.PluginPackets.Storage;
using Hangfire.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Hangfire.PluginPackets.Interface;
using Hangfire.PluginPackets.Command;

namespace Hangfire.PluginPackets.Dashboard.Dispatchers
{

    internal class JobCommandDispatcher : CommandDispatcher<JsonData>
    {

        public override Task<JsonData> Exception(Exception ex)
        {
            return Task.FromResult(new JsonData(ex, null));
        }

        public PluginDefine TheDomain { get; set; }

        public AssemblyDefine TheAssembly { get; set; }

        public JobDefine TheJob { get; set; }

        public Dictionary<string, object> JobData { get; set; }

        public override async Task<JsonData> Invoke()
        {
            var cmd = await GetFromValue("cmd");
            var jobCmd = JobPageCommand.None;
            Enum.TryParse(cmd, out jobCmd);

            var domain = await GetFromValue("domain");
            var assembly = await GetFromValue("assembly");
            var job = await GetFromValue("job");

            var set = StorageService.Provider.GetPluginDefines();
            TheDomain = set.SingleOrDefault(s => s.Title == domain);
            TheAssembly = TheDomain?.GetJobSets().SingleOrDefault(s => s.ShortName == assembly);
            TheJob = TheAssembly?.GetJobs().SingleOrDefault(s => s.Name == job);

            var start = await GetFromValue<DateTime>("start", DateTime.MinValue);
            JobData = await GetDictionaryValue("data");
            var sign = await GetFromValue("sign");

            var paramer = new PluginParamer
            {
                QueueName = (await GetFromValue("queue")).ToLower(),
                PluginName = TheDomain.PathName,
                AssemblyFullName = TheAssembly.FullName,
                AssemblyName = TheAssembly.ShortName,
                JobFullName = TheJob.FullName,
                JobName = TheJob.Name,
                JobParamers = JobData?.Select(s => s.Value).ToArray(),
                JobDelay = start - DateTime.Now,
                JobPeriod = await GetFromValue("period"),
                JobTitle = string.IsNullOrEmpty(sign) ? TheJob.Title : sign
            };

            if (TheJob == null) throw (new Exception("未正确定位到工作任务."));
            if (jobCmd == JobPageCommand.None) throw (new Exception("未正确定位到任务指令."));

            switch (jobCmd)
            {
                case JobPageCommand.Schedule: CreateJobCommand.Schedule(paramer); break;
                case JobPageCommand.Delay:  CreateJobCommand.Delay(paramer); break;
                case JobPageCommand.Immediately:  CreateJobCommand.Immediately(paramer); break;
                case JobPageCommand.Test:  CreateJobCommand.Test(paramer); break;
            }

            return new JsonData
            {
                IsSuccess = true,
                Message = "提交成功.",
                Url = "",
            };

        }

   
    }
}