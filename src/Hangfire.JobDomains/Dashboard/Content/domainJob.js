(function (hangfire) {

    hangfire.DomainJob = (function () {

        function DomainJob() {
            this._initialize();
        }

        DomainJob.prototype._initialize = function () {

            $('.js-domain-job').each(function () {
                var container = this;

                $(this).on('click', '.js-domain-job-commands-schedule',
                    function (e) {
                        var $this = $(this);
                        var cmd = $this.data('cmd');
                        var period = $this.data("schedule");
                        var params = BulidConfirmParam("周期执行任务", cmd, ".schedule_cron", period, container);
                        if (params == null) return;
                        CommandConfirm(container,params);
                    });

                $(this).on('click', '.js-domain-job-commands-immediately',
                    function (e) {
                        var $this = $(this);
                        var cmd = $this.data('cmd');
                        var params = BulidConfirmParam("立即执行任务", cmd, "", "", container);
                        if (params == null) return;
                        CommandConfirm(container,params);
                    });

                $(this).on('click', '.js-domain-job-commands-delay',
                    function (e) {
                        var $this = $(this);
                        var cmd = $this.data('cmd');
                        var params = BulidConfirmParam("周期执行任务", cmd, ".delay_cron", "", container);
                        if (params == null) return;
                        CommandConfirm(container,params);
                    });

                $(this).on('click', '.js-domain-job-commands-test',
                    function (e) {
                        var $this = $(this);
                        var cmd = $this.data('cmd');
                        var params = BulidConfirmParam("任务测试", cmd, "", "", container);
                        if (params == null) return;
                        CommandConfirm(container, params);
                    });

            });

        };

        DomainJob.AlertError = function (panel, message) {
            var heading = $(".panel-heading", panel);
            $('<div class="panel-body panel-error"><div class="alert alert-danger"><a class="close" data-dismiss="alert">×</a><strong>错误! </strong><span>' +
                message + '</span></div></div>').insertAfter(heading);
        }

        DomainJob.AlertInfo = function (panel, message) {
            var heading = $(".panel-heading", panel);
            $('<div class="panel-body panel-info"><div class="alert alert-info"><a class="close" data-dismiss="alert">×</a><span>' +
                message + '</span></div></div>').insertAfter(heading);
        }

        DomainJob.AlertSuccess = function (panel, message) {
            var heading = $(".panel-heading", panel);
            $('<div class="panel-body panel-success"><div class="alert alert-success"><a class="close" data-dismiss="alert">×</a><strong>成功! </strong><span>' +
                message + '</span></div></div>').insertAfter(heading);
        }

        function BulidConfirmParam(title, cmd, cron, period, panel) {
            $(".panel-error", panel).remove();
            $(".panel-info", panel).remove();
            $(".panel-success", panel).remove();

            var funcName = $(".panel-heading", panel).html();
            var cronValue = cron == "" ? "" : $(cron, panel).val();

            if (cron != "" && cronValue == "") {
                DomainJob.AlertError(panel, "执行时间未正确设置");
                return null;
            }

            var job = '<div class="form-group"> <label  class="control-label">执行任务:</label>'
                + '<input type="text" class="form-control disabled" disabled="disabled" data-name="jobname" value="' + funcName + '"></div>'
            var startTime = '<div class="form-group"> <label  class="control-label">执行时间:</label>'
                + '<input type="text" class="form-control disabled" disabled="disabled" data-name="jobstarttime" value="' + cronValue + '"></div>'
            var periodTime = '<div class="form-group"> <label class="control-label">执行周期（分钟）:</label>'
                + '<input type="text" class="form-control disabled" disabled="disabled" data-name="jobperiodtime" value="' + period + '"></div>'

            var boby = job + (cron == "" ? "" : startTime) + (period == "" ? "" : periodTime);
            $(".panel-body", panel).find("input").each(function () {
                var $input = $(this);
                var name = $input.data("name");
                var value = $input.val();
                var type = $input.data("type");
                boby += '<div class="form-group"> <label   class="control-label">' + name
                    + ':</label><input type="text" class="form-control disabled" disabled="disabled" data-name="' + name + '" value="' + value + '" data-type="' + type+'"></div>'
            });
            return { title: title, content: boby, cmd: cmd };
        }

        function CommandConfirm(panel,params) {
            var model = $("#command_confirm_model");
            model.find(".title").html(params.title);
            model.find(".content").html(params.content);
            model.find(".cancel").html("取消");
            model.find(".ok").html("确认提交");

            var options = {};
            $('#command_confirm_model').modal(options);
            var url = $(panel).data("url");
            var domain = $(panel).data("domain");
            var assembly = $(panel).data("assembly");
            var job = $(panel).data("job");
            var data = [];
            var inputs = model.find(".content").find("input");
            var starttime = "";
            var period = 0;
            for (var i = 0; i < inputs.length;i++)
            {
                var $input = $(inputs[i]);
                var name = $input.data("name");
                var value = $input.val();
                if (name == "jobname") {
                    continue;
                }
                if (name == "jobstarttime") {

                    starttime = value;
                    continue;
                }
                if (name == "jobperiodtime") {
                    period = value;
                    continue;
                }
                var type = $input.data("type");
                data.push({ name: name, type: type, value: value });
            }

            var send = { cmd: params.cmd, domain: domain, assembly: assembly, job: job, start: starttime, period: period, data: data };

            //每次都将监听先关闭，防止多次监听发生，确保只有一次监听
            model.find(".cancel").off("click")
            model.find(".ok").off("click")
            model.find(".ok").on("click", function () {
                showModal();
                $.post(url, send, function (result) {
                    hideModal();
                    $('#command_confirm_model').modal('hide');
                    if (result.IsSuccess)
                        DomainJob.AlertSuccess(panel, result.Message);
                    else
                        DomainJob.AlertError(panel, result.Message);
                }).fail(function (xhr, status, error) {
                    hideModal();
                    DomainJob.AlertError(panel, "There was an error. " + error);
                });
            })

            model.find(".cancel").on("click", function () {
                DomainJob.AlertInfo(panel, "任务取消提交");
            })
        }

        function hideModal() {
            $('#LoadingWall').modal('hide');
        }

        function showModal() {
            $('#LoadingWall').modal({ backdrop: 'static', keyboard: false });
        }  

        return DomainJob;

    })();

})(window.Hangfire = window.Hangfire || {});

function jobDoStart() {
    Hangfire.domainJob = new Hangfire.DomainJob();
    var link = document.createElement('link');
    link.setAttribute("rel", "stylesheet");
    link.setAttribute("type", "text/css");
    link.setAttribute("href", 'https://cdnjs.cloudflare.com/ajax/libs/bootstrap-datetimepicker/3.1.4/css/bootstrap-datetimepicker.min.css');
    document.getElementsByTagName("head")[0].appendChild(link);
    var url = "https://cdnjs.cloudflare.com/ajax/libs/bootstrap-datetimepicker/3.1.4/js/bootstrap-datetimepicker.min.js";
    $.getScript(url,
        function() {
            $(function() {
                $("div[id$='_datetimepicker']").each(function() {
                    $(this).datetimepicker();
                });

                $('.date_cron_selector').datetimepicker();
            });
    });
}



