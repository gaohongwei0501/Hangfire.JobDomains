﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Dashboard
{

    internal enum JobPageCommand
    {
        /// <summary>
        /// 默认值
        /// </summary>
        [DescriptionAttribute("未知预制指令")]
        None,
        /// <summary>
        /// 任务执行
        /// </summary>
        [DescriptionAttribute("任务立即执行指令")]
        Immediately,
        [DescriptionAttribute("任务排期执行指令")]
        Delay,
        [DescriptionAttribute("任务周期执行指令")]
        Schedule
    }

    internal enum DomainPageCommand
    {
        [DescriptionAttribute("未知预制指令")]
        None,
        [DescriptionAttribute("未知预制指令")]
        Load,
        [DescriptionAttribute("未知预制指令")]
        Unload,
    }

}
