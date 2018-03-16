using Hangfire.PluginPackets.Interface;
using Hangfire.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Dynamic
{
    public abstract class JobExecute
    {
        public void Enqueued(PluginParamer paramer)
        {
            IBackgroundJobClient hangFireClient = new BackgroundJobClient();
            EnqueuedState state = new Hangfire.States.EnqueuedState(paramer.QueueName);
            hangFireClient.Create(() => Execute(paramer), state);
        }

        public bool Test(PluginParamer paramer)
        {
            return Execute<bool>(paramer, PrefabricationActivator.Test, domain => (bool)domain.GetData("result"));
        }

        public void Execute(PluginParamer paramer)
        {
            Execute<bool>(paramer, PrefabricationActivator.Dispatch, domain => true);
        }

        protected abstract T Execute<T>(PluginParamer paramer, Action act, Func<AppDomain, T> GetResult);

    }
}
