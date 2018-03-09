using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Interface
{
    public interface IBatchImport
    {
        List<IPeriodBatch> GetPeriodBatch();
    }
}
