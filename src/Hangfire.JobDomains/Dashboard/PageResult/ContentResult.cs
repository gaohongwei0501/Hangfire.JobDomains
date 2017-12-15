using Hangfire.Dashboard;
using Hangfire.Dashboard.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Dashboard.PageResult
{
    internal class ContentResult
    {
        protected HtmlPage Page { get; set; }

        public TagHelper Tag { get; set; } = new TagHelper();

        public Action Content { get; set; } = () => { };

        public ContentResult(HtmlPage page)
        {
            Page = page;
        }

        void TryRender()
        {
            try
            {
                Content();
            }
            catch (Exception ex)
            {
                Page.HtmlWrite($@"<div class=""alert alert-danger"" role=""alert""> { nameof(ContentResult) } 渲染遇见异常：{ ex.Message }.</div>");
            }
        }

        public virtual bool Output()
        {
            var pageTitle = Page.FetchTitle();
            var layout= new LayoutPage(pageTitle);
            Page.SetLayout(layout);

            var pageHeader = Page.FetchHeader();

            Page.HtmlWrite("<div class=\"row\">\r\n");
            Page.HtmlWrite("<div class=\"col-md-3\">\r\n");

            var sideMenu = Page.Sidebar == null ? new List<Func<RazorPage, MenuItem>>() : Page.Sidebar();
            Page.HtmlWrite(new SidebarPartial(sideMenu));

            Page.HtmlWrite("</div>\r\n");
            Page.HtmlWrite("<div class=\"col-md-9\">\r\n");
            Page.HtmlWrite("<h1 class=\"page-header\">\r\n");
            Page.HtmlWrite(pageHeader);
            Page.HtmlWrite("</h1>\r\n");

            TryRender();

            Page.HtmlWrite("\r\n</div>\r\n");
            Page.HtmlWrite("\r\n</div>\r\n");

            WriteStyleFile(Page.Url.To("/cssex/jobdomain"));

            var loadingMessage = "加载中...";
            var loading = Tag.CreateLoadingWall(loadingMessage);
            Page.HtmlWrite(loading);
            var Script = Tag.ScriptPlug(ScriptFileBuilder.ToString(),Cssbulider.ToString(), ScriptBuilder.ToString());
            Page.HtmlWrite(Script);
            return false;
        }

        StringBuilder Cssbulider = new StringBuilder();
        StringBuilder ScriptBuilder = new StringBuilder();
        StringBuilder ScriptFileBuilder = new StringBuilder();

        public void WriteStyleFile(string cssSrc)
        {
            var linkID = Guid.NewGuid().ToString("N");
            Cssbulider.AppendLine($"var link_{ linkID } = document.createElement('link');");
            Cssbulider.AppendLine($"link_{ linkID }.setAttribute('rel', 'stylesheet');");
            Cssbulider.AppendLine($"link_{ linkID }.setAttribute('type', 'text/css'); ");
            Cssbulider.AppendLine($"link_{ linkID }.setAttribute('href', '{ cssSrc }');");
            Cssbulider.AppendLine($"document.getElementsByTagName('head')[0].appendChild(link_{ linkID });");
        }

        public void WriteScript(string script)
        {
            ScriptBuilder.AppendLine(script);
        }

        public void WriteScriptFile(string scriptSrc)
        {
            ScriptFileBuilder.AppendLine($"\r\n<script src=\"{ scriptSrc }\"></script>\r\n");
        }

        public void WritePagerPanel<T>(string title, IEnumerable<T> data, int index, int size, Func<T, (string Name, string Link)> createItem)
        {
            if (data == null) return;

            var list = data.Skip(index).Take(size);

            var items = list.Select(d => {
                var (Name, Link) = createItem(d);
                return Tag.ListLinkItem(Name, Link);
            });

            var content = Tag.List(items);

            var pager = new Pager(index, size, data.Count());

            var part = Page.Html.Paginator(pager);

            var footer = part.ToString();

            var panel = Tag.Panel(title, string.Empty, content, footer);

            Page.HtmlWrite(panel);
        }

    }
}
