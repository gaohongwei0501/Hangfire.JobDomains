﻿using Hangfire.PluginPackets.Interface;
using Hangfire.PluginPackets.Models;
using Hangfire.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Storage
{
    
    public class DynamicClassExtension<T> where T : DynamicBaseClass
    {

        public Action<PluginParamer> GetTestService()
        {
            return paramer =>
            {
                IBackgroundJobClient hangFireClient = new BackgroundJobClient();
                EnqueuedState state = new Hangfire.States.EnqueuedState(paramer.QueueName);
                hangFireClient.Create<T>(service => service.Test(paramer), state);
            };
        }

        public Action<PluginParamer> GetScheduleService()
        {
            return paramer =>
            {
                BackgroundJob.Schedule<T>(service => service.Enqueued(paramer), paramer.JobDelay);
            };
        }

        public Action<PluginParamer> GetEnqueuedService()
        {
            return paramer =>
            {
                IBackgroundJobClient hangFireClient = new BackgroundJobClient();
                EnqueuedState state = new Hangfire.States.EnqueuedState(paramer.QueueName);
                hangFireClient.Create<T>(service => service.Execute(paramer), state);
            };
        }

        public Action<PluginParamer> GetPeriodService()
        {
            return paramer =>
            {
                RecurringJob.AddOrUpdate<T>(paramer.JobTitle, service => service.Execute(paramer), paramer.JobPeriod, queue: paramer.QueueName);
            };
        }

    }
}
