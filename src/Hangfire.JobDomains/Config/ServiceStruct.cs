using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Config
{
    public class ServiceStruct
    {
        public string ServiceName { get; set; }
        public string ServiceDisplayName { get; set; }
        public string ServiceDescription { get; set; }
    }

    public class ServiceSettings : SettingCache<ServiceStruct>
    {
        public override string Key { get; } = "service.json";

        public override string BasePath { get; set; }

        ServiceStruct defaultValue { get; set; }

        public ServiceSettings(string path, string defaultName)
        {
            BasePath = path;
            defaultValue = new ServiceStruct
            {
                ServiceName = defaultName,
                ServiceDescription = defaultName,
                ServiceDisplayName = defaultName
            };
        }

        protected override ServiceStruct GetDefault()
        {
            return defaultValue;
        }
    }
}
