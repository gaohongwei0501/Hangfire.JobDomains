using Hangfire.Dashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Microsoft.Owin;
using System.Text.RegularExpressions;

namespace Hangfire.JobDomains.Dashboard.Dispatchers
{
    internal abstract class CommandDispatcher<T> : IDashboardDispatcher
    {
        protected OwinContext Context { get; set; }

        public abstract Task<T> Invoke();

        public async Task<string> GetFromValue(string key)
        {
            var values = await GetFormValuesAsync(key);
            return values == null ? string.Empty : values.FirstOrDefault();
        }

        public async Task<V> GetFromValue<V>(string key, V def)
        {
            var values = await GetFormValuesAsync(key);
            return values == null ? default(V) : values.FirstOrDefault().ConvertTo<V>(def);
        }


        public async Task<IList<string>> GetFormValuesAsync(string key)
        {
            var form = await Context.ReadFormSafeAsync();
            return form.GetValues(key) ?? new List<string>();
        }

        void MatchItem(Dictionary<string, (string Name, string Value, Func<string, object> Convert)> tempDictionary,string Key,string pattern,string input)
        {
            var match = Regex.Match(Key, pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (match.Success)
            {
                if (tempDictionary.ContainsKey(match.Value))
                {
                    var value = tempDictionary[match.Value];
                    value.Name = input;
                    tempDictionary[match.Value] = value;
                }
                else
                {
                    tempDictionary.Add(match.Value, (input, string.Empty, null));
                }
            }
        }


        public async Task<Dictionary<string, object>>GetDictionaryValue(string key)
        {
            var dic = new Dictionary<string, (string Name, string Value,Func<string,object> Convert)>();
            var pattern = $@"{key}\[\d+\]";
            var namePattern = @"\[name\]";
            var valuePattern = @"\[value\]";
            var typePattern = @"\[type\]";

            var form = await Context.ReadFormSafeAsync();
            foreach (var item in form)
            {
             //   MatchItem(dic, item.Key, namePattern, item.Value == null || item.Value.Length == 0 ? string.Empty : item.Value[0]);

                var match = Regex.Match(item.Key, pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                if (match.Success)
                {
                    var nameMatch = Regex.Match(item.Key, namePattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    if (nameMatch.Success) {
                        var itemValue = item.Value == null || item.Value.Length == 0 ? string.Empty : item.Value[0];
                        if (dic.ContainsKey(match.Value))
                        {
                            var value = dic[match.Value];
                            value.Name = itemValue;
                            dic[match.Value] = value;
                        }
                        else
                        {
                            dic.Add(match.Value, (itemValue, string.Empty, null));
                        }
                    }

                    var valueMatch = Regex.Match(item.Key, valuePattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    if (valueMatch.Success)
                    {
                        var itemValue = item.Value == null || item.Value.Length == 0 ? string.Empty : item.Value[0];
                        if (dic.ContainsKey(match.Value))
                        {
                            var value = dic[match.Value];
                            value.Value = itemValue;
                            dic[match.Value] = value;
                        }
                        else
                        {
                            dic.Add(match.Value, (string.Empty, itemValue, null));
                        }
                    }

                    var typeMatch = Regex.Match(item.Key, typePattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    if (typeMatch.Success)
                    {
                        var itemValue = FetchConvert(item.Value == null || item.Value.Length == 0 ? string.Empty : item.Value[0]);
                        if (dic.ContainsKey(match.Value))
                        {
                            var value = dic[match.Value];
                            value.Convert = itemValue;
                            dic[match.Value] = value;
                        }
                        else
                        {
                            dic.Add(match.Value, (string.Empty, string.Empty, itemValue));
                        }
                    }
                }
            }
           
            return dic.ToDictionary(s=>s.Value.Name,s=>s.Value.Convert(s.Value.Value));
        }

        const string StringType = "String";
        const string IntType = "Int";
        const string DateTimeType = "DateTime";

        Func<string, object> FetchConvert(string type)
        {
            if (string.IsNullOrEmpty(type)) return null;
            switch (type)
            {
                case StringType: return input => input;
                case IntType: return input => input.ConvertTo<int>();
                case DateTimeType: return input => input.ConvertTo<DateTime>();
            }
            return null;
        }

        public async Task Dispatch(DashboardContext context)
        {
            var dashboardContext = context as OwinDashboardContext;
            if (dashboardContext == null) throw (new Exception($"{ nameof(OwinDashboardContext) } 未能实例化 "));
            Context = dashboardContext.Environment.GetOwinContext();

           // Context = context;
            var result = await TryInvoke();
            var serialized = JsonConvert.SerializeObject(result);
            Context.Response.ContentType = "application/json";
            await Context.Response.WriteAsync(serialized);
        }

        public virtual async Task<T> Exception(Exception ex)
        {
            return default(T);
        }

        async Task<T> TryInvoke()
        {
            try
            {
                return await Invoke();
            }
            catch(Exception ex)
            {
                return await Exception(ex);
            }
        }
    }
}
