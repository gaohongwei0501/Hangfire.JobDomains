using Hangfire.PluginPackets.Command;
using Hangfire.PluginPackets.Models;
using Hangfire.PluginPackets.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Dashboard.Dispatchers
{
    internal class ClientCommandDispatcher : CommandDispatcher<JsonData>
    {
        public override  async Task<JsonData> Invoke()
        {
            var cmd = await GetFromValue("cmd");
            var serverCmd = ClientPageCommand.None;
            bool right = Enum.TryParse<ClientPageCommand>(cmd, out serverCmd);
            if (!right || serverCmd == ClientPageCommand.None) throw (new Exception("未知指令"));

            switch (serverCmd)
            {
                case ClientPageCommand.Refresh: return await RefreshClient();
            }

            return new JsonData
            {
                IsSuccess = true,
                Message = "提交成功.",
                Url = "",
            };
        }

        public async Task<JsonData> RefreshClient()
        {
            await PluginPlanImportCommand.CreateExcute();

            return new JsonData
            {
                IsSuccess = true,
                Message = "提交成功.",
                Url = "",
            };
        }
    }
}
