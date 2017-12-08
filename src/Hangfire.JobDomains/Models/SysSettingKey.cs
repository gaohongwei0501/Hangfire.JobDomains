using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Models
{
    public enum SysSettingKey
    {
        [Description("任务包根目录")]
        _BasePath,
    }
}
