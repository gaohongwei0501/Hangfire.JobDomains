using Hangfire.Dashboard;
using Hangfire.JobDomains.AppSetting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Dashboard
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
            content = string.IsNullOrEmpty(content) ? string.Empty : $@"<div class=""panel-body""> {content} </div>";
            footer = string.IsNullOrEmpty(footer) ? string.Empty : $@"<div class=""panel-footer clearfix "">  <div class=""pull-right""> { footer}  </div></div>";
            description = string.IsNullOrEmpty(description) ? string.Empty : $@"<div class=""panel-body""><p>{ description }</p></div>";
            return $@"<div class=""panel panel-info { panelClass }"" { customAttr }>
                              <div class=""panel-heading"">{ heading }</div>
                              { description }
                              { content  }
                              { footer }
                        </div>";
        }

        public string List(Func<IEnumerable<string>> fetchItems)
        {
            var Items = fetchItems();
            return List(Items);
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

        public string List(IEnumerable<Func<string>> fetchItems)
        {
            var bulider = new StringBuilder();
            if (fetchItems != null)
            {
                foreach (var fetch in fetchItems)
                {
                    var item = fetch();
                    bulider.Append(item);
                }
            }
            return $@"<div class=""list-group"">  {bulider}  </div>";
        }

        public string List(params string[] Items)
        {
            return List(Items as IEnumerable<string>);
        }

        public string List(params Func<string>[] fetchItems)
        {
            return List(fetchItems as IEnumerable<Func<string>>);
        }

        public string ListItem(string content,string badge="")
        {
            badge = string.IsNullOrEmpty(badge) ? string.Empty:$@"<span class=""badge"">{badge}</span>";
            return $@" <div href="""" class=""list-group-item "" >{badge}{content}</div> ";
        }

        public string ListLinkItem(string content, string url, string badge = "", bool active = false)
        {
            badge = string.IsNullOrEmpty(badge) ? string.Empty : $@"<span class=""badge"">{badge}</span>";
            var activeStr = active ? "active" : "";
            return $@"<a href=""{ url }"" class=""list-group-item {activeStr}"" >{badge}{content}</a> ";
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

        public string InputDatebox(string id, string labelText, string placeholderText, string dataTag = "")
        {
            return $@"
                    <div class=""form-group"">
                        <label for=""{id}"" class=""control-label"">{labelText}</label>
                        <div class='input-group date' id='{id}_datetimepicker'>
                            <input type='text' class=""form-control"" placeholder=""{placeholderText}"" id=""{id}""  { dataTag }/>
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

        public static string CreateLoadingWall(this TagHelper Tag,string message)
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
       
        public static string CreateJobScheduleButtons(this TagHelper Tag , string id)
        {
            var jobCorns = JobCornSetting.Dictionary.GetValue();
            var bulider = new StringBuilder();
            var loadingText = "Loading...";

            bulider.Append($@"
                        <div class=""col-sm-2 pull-right"">
                            <button class=""js-domain-job-commands-test btn btn-sm btn-danger"" 
                                 data-cmd=""{ JobPageCommand.Test }"" data-loading-text=""{ loadingText }"" input-id=""{ id }""> 
                                <span class=""glyphicon glyphicon-play-circle""></span> &nbsp;测试
                            </button>
                        </div>
                      ");



            bulider.Append(@"<div class=""col-sm-4 pull-right"">
                               <div class=""input-group  input-group-sm"" >
                                <input type='text' class=""form-control date_cron_selector schedule_cron"" placeholder=""首次执行时间""   />
                                <span class=""input-group-btn"">
                                    <button type=""button"" class=""btn btn-info btn-sm dropdown-toggle"" data-toggle=""dropdown"" aria-haspopup=""true"" aria-expanded=""false"">
                                         周期执行 &nbsp; <span class=""caret""></span>
                                    </button>
                                    <ul class=""dropdown-menu"">");


            foreach (var item in jobCorns) {
                bulider.Append($@"<li><a href=""#"" class=""js-domain-job-commands-schedule"" input-id=""{ id }"" data-schedule=""{ item.Key }""         
                                        data-cmd=""{ JobPageCommand.Schedule }"" data-loading-text=""{ loadingText }"">{ item.Value }</a></li>");
            }

            bulider.Append(@" </ul></span></div></div>");
            bulider.Append($@"
                        <div class=""col-sm-2 pull-right"">
                            <button class=""js-domain-job-commands-immediately btn btn-sm btn-success"" 
                                 data-cmd=""{ JobPageCommand.Immediately }"" data-loading-text=""{ loadingText }"" input-id=""{ id }""> 
                                <span class=""glyphicon glyphicon-play-circle""></span> &nbsp;立即执行
                            </button>
                        </div>
                        <div class=""col-sm-4 pull-right"">
                            <div class=""input-group input-group-sm"">
                                <input type=""text"" class=""form-control date_cron_selector delay_cron"" placeholder=""执行时间"" >
                                <span class=""input-group-btn "">
                                    <button class=""js-domain-job-commands-delay  btn btn-default btn-sm btn-warning"" type=""button"" input-id=""{ id }"" 
                                        data-cmd=""{ JobPageCommand.Delay }"" data-loading-text=""{ loadingText }"">
                                        <span class=""glyphicon glyphicon-repeat""></span> &nbsp; 排期执行
                                    </button>
                                </span>
                            </div>
                        </div>");
            return bulider.ToString();
        }

        static Dictionary<SysSettingKey, string> SysSettingDescriptions = new Dictionary<SysSettingKey, string>();

        public static string CreateSysList(this  TagHelper Tag)
        {
            var bulider = new StringBuilder();
            if (SysSettingDescriptions.Count == 0) ReadDescriptions();
            Array arrays = Enum.GetValues(typeof(SysSettingKey));
            var cache = SysSetting.Dictionary.GetValue();
            for (int i = 0; i < arrays.LongLength; i++)
            {
                var key = (SysSettingKey)arrays.GetValue(i);
                var value = cache[key];
                var name = SysSettingDescriptions[key];
                bulider.Append(Tag.ListItem($"{name}：{value}", "编辑"));
            }
            return Tag.List(bulider.ToString());
        }

        static void ReadDescriptions()
        {
            Type SysSettingType = typeof(SysSettingKey);
            Array arrays = Enum.GetValues(SysSettingType);
            for (int i = 0; i < arrays.LongLength; i++)
            {
                var value = (SysSettingKey)arrays.GetValue(i);
                FieldInfo fieldInfo = SysSettingType.GetField(value.ToString());
                var attr= fieldInfo.GetCustomAttribute<DescriptionAttribute>(false);
                SysSettingDescriptions.Add(value, attr.Description);
            }
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
