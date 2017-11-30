using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.AppSetting
{
    internal enum SysSettingKey
    {
        [Description("任务包根目录")]
        BasePath,
    }

    internal class SysSetting : AppSettingCache<Dictionary<SysSettingKey, object>>
    {

        public static readonly SysSetting Dictionary = new SysSetting();

        public override string Key { get; } = "cron.json";
 

        protected override Dictionary<SysSettingKey, object> GetDefault()
        {
            var cache = new Dictionary<SysSettingKey, object>();

            cache.Add(SysSettingKey.BasePath, "");

            return cache;
        }

        public T GetValue<T>(SysSettingKey key)
        {
            var cache = GetValue();
            return (T)cache[SysSettingKey.BasePath];
        }


        public void SetValue(SysSettingKey key, object value)
        {
            var cache = GetValue();
            if (cache.ContainsKey(key))
                cache[key] = value;
            else
                cache.Add(key, value);

            CreateOrUpdateSettingFile(() => cache);
        }

    }
}
