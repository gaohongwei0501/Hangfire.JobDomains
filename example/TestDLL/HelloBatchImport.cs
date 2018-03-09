using Hangfire.PluginPackets.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDLL
{
    public class HelloBatchImport : IBatchImport
    {
        public List<IPeriodBatch> GetPeriodBatch()
        {
            PeriodBatch<HelloTest> testJob = new PeriodBatch<HelloTest>("HelloTest 计划", "0 2 * * *");
            PeriodBatch<HelloWorld> worldJob = new PeriodBatch<HelloWorld>("HelloWorld 计划", "0 3 * * *", args: "daly");

            return new List<IPeriodBatch> { testJob, worldJob };
        }
    }
}
