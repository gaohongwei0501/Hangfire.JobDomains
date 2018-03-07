using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Models
{
    public class JsonData
    {
        public JsonData() { }

        public JsonData(Exception ex, Func<(string Name, string Link)> CreateRoute)
        {
            IsSuccess = false;
            Message = ex.Message;
            try
            {
                var route = CreateRoute == null ? (Name: "", Link: "#") : CreateRoute();
                Url = route.Link;
            }
            catch
            {
                Url = "#";
            }
        }

        public bool IsSuccess { get; set; }

        public string Url { get; set; }

        public string Message { get; set; }
    }

    internal class JsonData<T> : JsonData
    {
        public JsonData() : base() { }

        public T Data { get; set; }
    }
}
