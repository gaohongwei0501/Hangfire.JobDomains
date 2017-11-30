using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.AppSetting
{
    internal class JobCornSetting : AppSettingCache<Dictionary<int, string>>
    {

        public static readonly JobCornSetting Dictionary = new JobCornSetting();

        public override string Key { get; } = "cron.json";

        protected override Dictionary<int, string> GetDefault()
        {
            var value = new Dictionary<int, string>();

            value.Add(1, "一分钟");
            value.Add(5, "五分钟");
            value.Add(10, "十分钟");

            return value;
        }

        public void SetValue(int key, string value)
        {
            var cache = GetValue();
            if (cache.ContainsKey(key))
                cache[key] = value;
            else
                cache.Add(key, value);

            CreateOrUpdateSettingFile(() => cache);
        }

        public bool Contains(int key)
        {
            var cache = GetValue();
            return cache.ContainsKey(key);
        }

    }

}
