using Hangfire;
using Hangfire.Common;
using Hangfire.Dashboard;
using Hangfire.Dashboard.Pages;
using Hangfire.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Dashboard
{

    internal abstract class HtmlPage : RazorPage
    {
        public Func<string> FetchTitle { get; set; } = () => string.Empty;

        public Func<string> FetchHeader { get; set; } = () => string.Empty;

        public int PageIndex
        {
            get
            {
                var index = Query("from").ConvertTo<int>();
                return index < 0 ? 0 : index;
            }
        }

        public virtual int PageSize { get; } = 10;

        public Func<IEnumerable<Func<RazorPage, MenuItem>>> Sidebar { get; set; }

        public PageResult.ContentResult PageContent { get; set; }

        protected virtual PageResult.ContentResult CreatePageContent() {
            var result = new PageResult.ContentResult(this);
            result.Content = () => Content();
            return result;
        }

        protected abstract bool Content();

        public HtmlPage()
        {
            PageContent = CreatePageContent();
        }

        public void SetLayout(RazorPage layout)
        {
            Layout = layout;
        }

        public void HtmlWrite(string html)
        {
            WriteLiteral(html);
        }

        public void HtmlWrite(RazorPage Partial)
        {
            Write(Html.RenderPartial(Partial));
        }


        public override void Execute()
        {
            PageContent.Output();
        }
   



    }
}
