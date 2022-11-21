var $space, $title, $link, $target, $disp_opt, $permanent
var Dataid, startDate, endDate

function PageReady() {
    co.Marquees = {
        Add: function (data) {
            return $.ajax({
                url: "/api/Marquee/Add",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        Get: function (id) {
            return $.ajax({
                url: "/api/Marquee/Get/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { id: id },
            });
        },
        Update: function (data) {
            return $.ajax({
                url: "/api/Marquee/Update",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        }
    };

    if (location.href.indexOf('?id=') != -1) {
        Dataid = (location.href.split('?id='))[1];
    }

    $CardBody = $("#CreatePost > .card-body")
    $space = $CardBody.children(".select_placement").children("div").children("select");
    $disp_opt = $CardBody.children(".select_placement").children("button").children("span");
    $title = $CardBody.children(".input_text").children("textarea");
    $link = $CardBody.children(".input_link").children(".input_link");
    $target = $CardBody.children(".input_link").children(".checkbox_link").children("input");
    $date = $CardBody.children(".input_date").children("input");
    $permanent = $CardBody.children(".input_date").children(".checkbox_permanent").children("input");

    var $picker = $("#Datepicker");

    $picker.daterangepicker({
        timePicker: true,
        timePicker24Hour: true,
        autoUpdateInput: Dataid > 0 ? true : false,
        locale: {
            format: 'YYYY/M/DD HH:mm'
        }
    });

    if (Dataid > 0) {
        co.Marquees.Get(Dataid).done(function (result) {
            $title.text(result.title);
            $disp_opt.text(result.disp_opt ? "visibility" : "visibility_off");
            $link.val(result.link);
            $target.prop("checked", result.target);
            $permanent.prop("checked", result.permanent);
            if (result.permanent) {
                $date.val('');
                $date.attr("disabled", "disabled");
                $date.siblings("span").removeClass("bg-transparent");
                startDate = null;
                endDate = null;
            } else {
                $date.siblings("span").addClass("bg-transparent");
                $picker.data('daterangepicker').setStartDate(result.startTime);
                $picker.data('daterangepicker').setEndDate(result.endTime);
            }
        });
    }

    $picker.on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format('YYYY/M/DD HH:mm') + ' ~ ' + picker.endDate.format('YYYY/M/DD HH:mm'));
        startDate = picker.startDate.format("");
        endDate = picker.startDate.format("");
    });

    $picker.on('cancel.daterangepicker', function (ev, picker) {
        $(this).val('');
    });

    const forms = $('#PostForm');
    (() => {
        Array.from(forms).forEach(form => {
            form.addEventListener('submit', event => {
                if (!form.checkValidity()) {
                    event.preventDefault()
                    event.stopPropagation()
                } else {
                    event.preventDefault();
                    Coker.sweet.confirm("即將發布", "發布後將直接顯示於安排的位置", "發布", "取消", function () {
                        if (Dataid > 0) {
                            Update($disp_opt.text() == "visibility" ? true : false, "已成功發布", "發布發生未知錯誤");
                        } else {
                            Add($disp_opt.text() == "visibility" ? true : false, "已成功發布", "發布發生未知錯誤");
                        }
                    });
                }
                form.classList.add('was-validated')
            }, false)
        })
    })()

    $(".btn_save").on("click", function () {
        $disp_opt.text("visibility_off");
        if (Dataid > 0) {
            Update(false, "已存為草稿", "儲存草稿發生未知錯誤");
        } else {
            Add(false, "已存為草稿", "儲存草稿發生未知錯誤");
        }
    })

    $disp_opt.on("click", function () {
        if ($disp_opt.text() == "visibility") {
            $disp_opt.text("visibility_off");
        } else if ($disp_opt.text() == "visibility_off") {
            $disp_opt.text("visibility");
        }
    })

    $input_number = $CardBody.children(".input_text").children("div").children(".input_number");
    $title.on('keyup', function () {
        $input_number.text($title.val().length);
    });

    $permanent.on("click", function () {
        if ($permanent.is(":checked")) {
            $date.val('');
            $date.attr("disabled", "disabled");
            $date.siblings("span").removeClass("bg-transparent");
            startDate = null;
            endDate = null;
        } else {
            $date.removeAttr("disabled");
            $date.siblings("span").addClass("bg-transparent");
        }
    })

}

function Add(display, success_text, error_text) {
    co.Marquees.Add({
        WebsiteId: 1,
        title: $title.val(),
        disp_opt: display,
        ser_no: 1,
        link: $link.val(),
        target: $target.is(":checked"),
        StartTime: startDate,
        EndTime: endDate,
        permanent: $permanent.is(":checked")
    }).done(function () {
        Coker.sweet.success(success_text, null, true);
        setTimeout(function () { history.back() }, 1000);
    }).fail(function () {
        Coker.sweet.error("錯誤", error_text, null, true);
    });
}

function Update(display, success_text, error_text) {
    co.Marquees.Update({
        id: Dataid,
        WebsiteId: 1,
        title: $title.val(),
        disp_opt: display,
        ser_no: 1,
        link: $link.val(),
        target: $target.is(":checked"),
        StartTime: startDate,
        EndTime: endDate,
        permanent: $permanent.is(":checked")
    }).done(function () {
        Coker.sweet.success(success_text, null, true);
        setTimeout(function () { history.back() }, 1000);
    }).fail(function () {
        Coker.sweet.error("錯誤", error_text, null, true);
    });
}