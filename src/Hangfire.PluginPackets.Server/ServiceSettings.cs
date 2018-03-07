using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Server
{
    public class ServiceSettings : SettingCache<ServiceStruct>
    {

        public static readonly ServiceSettings Value = new ServiceSettings();

        public ServiceSettings()
        {

        }

        public override string Key { get; } = "service.json";

        protected override ServiceStruct GetDefault()
        {
            var value = new ServiceStruct
            {
                ServiceName = "Hangfire.PluginPackets.Service",
                ServiceDescription = "Hangfire.PluginPackets.Service",
                ServiceDisplayName = "Hangfire.PluginPackets.Service"
            };
            return value;
        }


    }
}
