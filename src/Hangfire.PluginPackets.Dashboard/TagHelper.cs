using Hangfire.Dashboard;
using Hangfire.PluginPackets.Models;
using Hangfire.PluginPackets.Storage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Dashboard
{
    internal class TagHelper
    {

        public TagHelper()
        {
        }

        public string ScriptPlug(string scriptFile,string cssFile,params string[] scripts)
        {
            if (scripts == null) return string.Empty;
            return scriptFile + @" 
                    <script> 
                        function SysLoad(){ 
                            " + scripts.Aggregate((a, b) => a + b) + @"
                        } 
                            " + cssFile + @"
                        if (typeof (SysLoad) === 'function') {
                            if (window.attachEvent) {
                                window.attachEvent('onload', SysLoad);
                            } else {
                                if (window.onload) {
                                    var curronload = window.onload;
                                    var newonload = function (evt) {
                                        curronload(evt);
                                        SysLoad(evt);
                                    };
                                    window.onload = newonload;
                                } else {
                                    window.onload = SysLoad;
                                }
                            }
                        }
                    </script>
                    ";
        }

        public string Panel(string heading, string description, string content = "", string footer = "",string panelClass=""  ,string customAttr="")
        {
            return Panel(heading, description, new List<string> { content }, new List<string> { footer }, panelClass, customAttr);

            //content = string.IsNullOrEmpty(content) ? string.Empty : $@"<div class=""panel-body""> {content} </div>";
            //footer = string.IsNullOrEmpty(footer) ? string.Empty : $@"<div class=""panel-footer clearfix "">  <div class=""pull-right""> { footer}  </div></div>";
            //return $@"<div class=""panel panel-info { panelClass }"" { customAttr }>
            //                  <div class=""panel-heading"">{ heading }</div>
            //                  { description }
            //                  { content  }
            //                  { footer }
            //            </div>";
        }

        public string Panel(string heading, string description, IEnumerable<string> contents, string footer, string panelClass = "", string customAttr = "")
        {
            return Panel(heading, description, contents, new List<string> { footer },  panelClass, customAttr);
        }

        public string Panel(string heading, string description, string content, IEnumerable<string> footers, string panelClass = "", string customAttr = "")
        {
            return Panel(heading, description,new List<string> { content }, footers, panelClass, customAttr);
        }

        public string Panel(string heading, string description,IEnumerable<string> contents, IEnumerable<string> footers, string panelClass = "", string customAttr = "")
        {
            var bulider = new StringBuilder();


            bulider.Append($@"<div class=""panel panel-info { panelClass }"" { customAttr }>
                              <div class=""panel-heading"">{ heading }</div>
                            ");

            bulider.Append(string.IsNullOrEmpty(description) ? string.Empty : $@"<div class=""panel-body""><p>{ description }</p></div>");

            if (contents != null)
            {
                foreach (var content in contents)
                {
                    bulider.Append(string.IsNullOrEmpty(content) ? string.Empty : $@"<div class=""panel-body""> {content} </div>");
                }
            }

            if (footers != null)
            {
                foreach (var footer in footers)
                {
                    bulider.Append(string.IsNullOrEmpty(footer) ? string.Empty : $@"<div class=""panel-footer clearfix""> {footer} </div>");
                }
            }


            bulider.Append(@" </div>");

            return bulider.ToString();
        }


        public string List(params string[] Items)
        {
            return List(Items as IEnumerable<string>);
        }

        public string List(IEnumerable<string> Items)
        {
            var bulider = new StringBuilder();
            if (Items != null)
            {
                foreach (var item in Items)
                {
                    bulider.Append(item);
                }
            }
            return $@"<div class=""list-group"">  {bulider}  </div>";
        }

        public string ListLink<T>(IEnumerable<T> source, Func<T, (string Name, string Link)> CreateRoute)
        {
            return List<T>(source, one => {
                var (Name, Link) = CreateRoute(one);
                return ListLinkItem(Name, Link);
            });
        }

        public string List<T>(IEnumerable<T> source, Func<T,string> CreateItem)
        {
            var bulider = new StringBuilder();
            if (source != null&& CreateItem != null)
            {
                foreach (var one in source)
                {
                    var item = CreateItem(one);
                    bulider.Append(item);
                }
            }
            return $@"<div class=""list-group"">  {bulider}  </div>";
        }
       
        public string ListItem(string content,string badge="" ,string data="")
        {
            badge = string.IsNullOrEmpty(badge) ? string.Empty:$@"<span class=""badge"">{badge}</span>";
            return $@" <div href="""" class=""list-group-item "" {data} >{badge}{content}</div> ";
        }

     
        public string ListLinkItem(string content, string url, string badge = "", string data = "", bool active = false)
        {
            badge = string.IsNullOrEmpty(badge) ? string.Empty : $@"<span class=""badge"">{badge}</span>";
            var activeStr = active ? "active" : "";
            return $@"<a href=""{ url }"" class=""list-group-item {activeStr}""  {data}>{badge}{content}</a> ";
        }

        protected string Input(string id, string labelText, string placeholderText, string inputtype,string dataTag="")
        {
            return $@"
                    <div class=""form-group"">
                        <label for=""{ id }"" class=""control-label"">{ labelText }</label>
                        <input type=""{ inputtype }"" class=""form-control"" placeholder=""{ placeholderText }"" id=""{ id }"" { dataTag } >
                    </div>
                    ";
        }

        public string InputTextbox(string id, string labelText, string placeholderText, string dataTag = "")
        {
            return Input(id, labelText, placeholderText, "text", dataTag);
        }

        public string InputNumberbox(string id, string labelText, string placeholderText, string dataTag = "")
        {
            return Input(id, labelText, placeholderText, "number", dataTag);
        }

        public string InputDatebox(string id, string labelText, string placeholderText, string className="", string dataTag = "")
        {
            return $@"
                    <div class=""form-group"">
                        <label for=""{ id }"" class=""control-label"">{ labelText }</label>
                        <div class='input-group date' id='{ id }_datetimepicker'>
                            <input type='text' class=""form-control { className } "" placeholder=""{ placeholderText }"" id=""{id}""  { dataTag }/>
                            <span class=""input-group-addon"">
                                <span class=""glyphicon glyphicon-calendar""></span>
                            </span>
                        </div>
                    </div>";

        }

        public string InputCheckbox(string id, string labelText, string placeholderText, string dataTag = "")
        {
            return $@"
                       <div class=""form-group"">
                            <div class=""checkbox"">
                              <label>
                                <input type=""checkbox"" id=""{id}"" { dataTag }  >
                                {labelText}
                              </label>                             
                            </div>
                        </div>
                    ";
        }

    }

    internal static class TagExtension
    {

        public static string CreateLoadingWall(this TagHelper Tag, string message)
        {
            return $@"<div id=""LoadingWall"" class=""modal fade"" data-keyboard=""false"" data-backdrop = ""static"" data-role = ""dialog""
                         aria-labelledby = ""myModalLabel"" aria-hidden = ""true"" >
                         <div id = ""LoadingWallBody"" class=""loading"">{ message }</div>
                    </div> ";
        }

        public static string CreateCommandComfirmBox(this TagHelper Tag)
        {
            return $@"
                     <div id=""command_confirm_model"" class=""modal"">
                        <div class=""modal-dialog modal-sm"">
                            <div class=""modal-content"">
                                <div class=""modal-header"">
                                    <button type = ""button"" class=""close"" data-dismiss=""modal""><span aria-hidden=""true"">&times;</span><span class=""sr-only"">Close</span></button>
                                    <h5 class=""modal-title""><i class=""fa fa-exclamation-circle""></i> <span class=""title""></span></h5>
                                </div>
                                <div class=""modal-body small content"">
                                   
                                </div>
                                <div class=""modal-footer"" >
                                    <button type = ""button"" class=""btn btn-primary ok"" data-dismiss=""modal"">确认</button>
                                    <button type = ""button"" class=""btn btn-default cancel"" data-dismiss=""modal"">取消</button>
                                </div>
                            </div>
                        </div>
                    </div>";
        }


        public static string CreateJobQueues(this TagHelper Tag, IEnumerable<string> queues)
        {
            var bulider = new StringBuilder();

            bulider.Append($@"<div class=""form-group"">
                               <div class=""input-group  "" >
                                <input type=""text"" class=""form-control schedule_queue""  placeholder=""任务工作执行队列"">
                                <span class=""input-group-btn"">
                                    <button type=""button"" class=""btn btn-info dropdown-toggle"" data-toggle=""dropdown"" aria-haspopup=""true"" aria-expanded=""false"">
                                         推荐队列 &nbsp; <span class=""caret""></span>
                                    </button>
                                    <ul class=""dropdown-menu"">");


            foreach (var item in queues)
            {
                bulider.Append($@"<li><a href=""#""  class=""js-job-queue""  data-name=""{ item }""  >{ item }</a></li>");
            }

            bulider.Append(@" </ul></span></div></div>");

            return bulider.ToString();
        }


        public static string CreateJobTestButton(this TagHelper Tag, string loading = "Loading...")
        {
            var bulider = new StringBuilder();

            bulider.Append($@"<div class=""form-group"">
                                <div class=""btn-group "" role=""group"" aria-label=""..."">
                                  
                                      <button class=""js-domain-job-commands-create btn btn-default btn-sm btn-danger"" 
                                         data-cmd=""{ JobPageCommand.Test }"" data-loading-text=""{ loading }"" > 
                                        <span class=""glyphicon glyphicon-info-sign""></span> &nbsp;测试
                                    </button>
                                    </div>
                                </div>
                          ");

            return bulider.ToString();
        }

        public static string CreateJobScheduleButton(this TagHelper Tag, string loading = "Loading...")
        {
            var id= Guid.NewGuid().ToString();
            var bulider = new StringBuilder();
            bulider.Append($@"  <div class=""form-group"">
                            <label  class=""control-label"">排期执行时间</label>
                        <div class='input-group date' id=""{ id }_schedule_date_datetimepicker"" >
                            <input type='text' class=""form-control  schedule_date "" placeholder=""执行时间 "" id=""{ id }_schedule_date"" />
                            <span class=""input-group-addon"">
                                <span class=""glyphicon glyphicon-calendar""></span>
                            </span>
                        </div>
                    </div>");


            bulider.Append($@"<div class=""form-group"">
                            <div class=""btn-group "" role=""group"" aria-label=""..."">
                                   <button class=""js-domain-job-commands-create  btn btn-default btn-sm btn-warning"" type=""button"" input-id=""{ id }"" 
                                        data-cmd=""{ JobPageCommand.Delay }"" data-loading-text=""{ loading }"">
                                        <span class=""glyphicon  glyphicon-play-circle ""></span> &nbsp; 排期执行
                                    </button>
                                  </div>
                            </div>");
            return bulider.ToString();
        }


        public static string CreateJobPeriodButton(this TagHelper Tag,  string sign, string loading = "Loading...")
        {
            var bulider = new StringBuilder();

            bulider.Append($@" <div class=""form-group"">
                            <label  class=""control-label"">周期任务标识</label>
                            <input type='text' class=""form-control  schedule_sign disabled"" disabled=""disabled"" placeholder=""任务标识 ""   value=""{ sign }"" />
                    </div>");

            bulider.Append($@" <div class=""form-group"">
                            <label  class=""control-label"">周期任务周期（5段 Cron 值）<a href ='http://cron.qqe2.com/' target=""_blank"" >参考</a></label>
                            <input type='text' class=""form-control  schedule_period"" placeholder=""任务周期 ""  value="""" />
                    </div>");


            bulider.Append($@"<div class=""form-group"">
                            <div class=""btn-group "" role=""group"" aria-label=""..."">
                                    <button class=""js-domain-job-commands-create  btn btn-default btn-sm btn-success"" type=""button""
                                        data-cmd=""{ JobPageCommand.Schedule }"" data-loading-text=""{ loading }"">
                                        <span class=""glyphicon  glyphicon-repeat ""></span> &nbsp; 周期执行
                                    </button>
                                  </div>
                           </div>");


            return bulider.ToString();
        }


        public static string CreateDescription(this TagHelper Tag, string description) {

            return description;
        }

        public static string CreateServerList(this TagHelper Tag, ServerDefine define)
        {
            var bulider = new StringBuilder();
            bulider.Append(Tag.ListItem($" 服务名器：{define.Name}"));
            bulider.Append(Tag.ListItem($" 插件位置：{define.PlugPath}",  $"<span class='js-server-path-set cmd-link' data-cmd='{ServerPageCommand.EditPath}' >编辑</span>"));
            return Tag.List(bulider.ToString());
        }

        public static string CreateJobHistoryList(this TagHelper Tag, PluginDefine plugin, AssemblyDefine assembly, JobDefine job)
        {
            var bulider = new StringBuilder();

            var path = DynamicFactory.DynamicPath;
            var files = Directory.GetFiles(path);

            var className = job.Title.Replace(" ", "_").Replace("-", "_").Replace(".", "_").Replace("\"", "_").Replace("'", "_");
            var file_pro = $"{ plugin.PathName }.{ assembly.ShortName }.{ job.Name }";

            foreach (var one in files)
            {
                var file = new FileInfo(one);
                if (file.Name.StartsWith(file_pro) == false) continue;

                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                var dynamicAssembly = assemblies.FirstOrDefault(s => s.FullName.Contains(file.Name.Replace(".dll","")));

                bulider.Append(Tag.ListItem($" Assembly：{file.Name}"));
            }

            return Tag.List(bulider.ToString());
        }


        public static string CreateCronDeleteButtons(this TagHelper Tag, int value)
        {
            return $@"<a href=""javascript:void(0);"" class=""Cron-Delete"" data-value=""{value}"">删除</a>";
        }

        public static string CreateCronAddButtons(this TagHelper Tag)
        {
            var url = "";
            return $@" 
                        <div class=""col-sm-2 pull-right"">
                            <div class=""input-group input-group-sm"">
                                <span class=""input-group-btn "">
                                    <button class=""btn btn-default btn-sm btn-primary js-domain-job-input-commands"" type=""button""    data-url=""{ url }"" >
                                        <span class=""glyphicon glyphicon-ok""></span> &nbsp; 添加
                                    </button>
                                </span>
                            </div>
                        </div>
                       
                        <div class=""col-sm-4 pull-right"">
                            <div class=""input-group"">
                                <span class=""input-group-addon"" >时间值</span>
                                <input type = ""text"" class=""form-control new_cron_value"" placeholder=""时间值（分钟）"" >
                            </div>
                        </div>

                        <div class=""col-sm-4 pull-right"">
                            <div class=""input-group"">
                                <span class=""input-group-addon"" >名称</span>
                                <input type = ""text"" class=""form-control new_cron_name"" placeholder=""名称"" >
                            </div>
                        </div>
                   
                ";
        }
    }
}
