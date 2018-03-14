using Hangfire.PluginPackets.Dynamic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Dashboard
{
    public class ClientManager
    {
        static AppDomain DynamicDomain { get; set; }

        public static void LoadDynamic()
        {
            if (DynamicDomain != null) {
                AppDomain.Unload(DynamicDomain);
                DynamicDomain = null;
            }

            var path = TypeFactory.DynamicPath;
            AppDomainSetup setup = new AppDomainSetup
            {
                ApplicationBase = Path.GetDirectoryName(path),
                ConfigurationFile = $"{path}\\App.config",
                PrivateBinPath = path,
                DisallowApplicationBaseProbing = false,
                DisallowBindingRedirects = false
            };
            DynamicDomain = AppDomain.CreateDomain($"Client AppDomain { Guid.NewGuid() } ", null, setup);
            var assemblies = DynamicDomain.GetAssemblies();

        }


    }
}
