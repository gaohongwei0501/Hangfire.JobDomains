using Hangfire.PluginPackets.Storage;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Server
{
    public static class OwinExtensions
    {
        /// <summary>
        /// 任务域服务（单机模式）
        /// </summary>
        public static async Task UseHangfirePluginServer<T>(this IAppBuilder app, string path = "",  string connectString = "", int workerCount = 5) where T : IStorage, new()
        {
            await app.InitPluginsAtServer<T>(path, connectString, workerCount);
        }

        static async Task InitPluginsAtServer<T>(this IAppBuilder app, string path = "", string connectString = "", int workerCount = 5) where T : IStorage, new()
        {
            var connecting = StorageService.Provider.SetStorage(new T(), connectString);
            if (connecting == false) throw (new Exception(" HangfireDomain 数据服务连接失败"));
            var Options = await PluginServiceManager.InitServer(path, workerCount);

            PluginServiceManager.LoadDynamic();
            app.UseHangfireServer(Options);
        }

    }
}
