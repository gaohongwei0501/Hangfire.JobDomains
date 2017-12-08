using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.JobDomains.Models;


namespace Hangfire.JobDomains.Storage.Location
{


    internal class SysSetting : AppSettingCache<Dictionary<SysSettingKey, string>>
    {

        public override string Key { get; } = "cron.json";

        protected override Dictionary<SysSettingKey, string> GetDefault()
        {
            var cache = new Dictionary<SysSettingKey, string>();

            //cache.Add(SysSettingKey.BasePath, "");

            return cache;
        }


        public bool SetValue(SysSettingKey key, string value)
        {
            var cache = GetValue();
            if (cache.ContainsKey(key))
                cache[key] = value;
            else
                cache.Add(key, value);

            CreateOrUpdateSettingFile(() => cache);
            return true;
        }


    }
}
