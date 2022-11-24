var $title, $link, $target, $disp_opt, $date, $permanent, $input_number
var startDate, endDate, keyId, keyArray
var $picker, marquee_list

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
        },
        Delete: function (id) {
            return $.ajax({
                url: "/api/Marquee/Delete/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { id: id },
            });
        },
        GetAllKey: function () {
            return $.ajax({
                url: "/api/Marquee/GetAllKey",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
            });
        }
    };

    ElementInit();

    $picker = $("#Datepicker");

    $picker.daterangepicker({
        timePicker: true,
        timePicker24Hour: true,
        autoUpdateInput: true,
        locale: {
            format: 'YYYY/M/DD HH:mm',
            separator: " ~ ",
            applyLabel: "　確認　",
            cancelLabel: "　取消　",
            daysOfWeek: ["日", "一", "二", "三", "四", "五", "六"],
            monthNames: ["一月", "二月", "三月", "四月", "五月", "六月", "七月", "八月", "九月", "十月", "十一月", "十二月"]
        }
    });

    $picker.on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format('YYYY/M/DD HH:mm') + ' ~ ' + picker.endDate.format('YYYY/M/DD HH:mm'));
        startDate = picker.startDate.format("");
        endDate = picker.endDate.format("");
    });
    $picker.on('cancel.daterangepicker', function (ev, picker) {
        $(this).val("");
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
                        if (keyId > 0) {
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

    $(".btn_add").on("click", function () {
        FormDataClear();
        window.location.hash = 0;
        HashDataEdit();
    });
    $(".btn_save").on("click", function () {
        $disp_opt.text("visibility_off");
        if (keyId > 0) {
            Update(false, "已存為草稿", "儲存草稿發生未知錯誤");
        } else {
            Add(false, "已存為草稿", "儲存草稿發生未知錯誤");
        }
    });

    $disp_opt.on("click", function () {
        if ($disp_opt.text() == "visibility") {
            $disp_opt.text("visibility_off");
        } else if ($disp_opt.text() == "visibility_off") {
            $disp_opt.text("visibility");
        }
    })
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

    if ("onhashchange" in window) {
        window.onhashchange = hashChange;
    } else {
        setInterval(hashChange, 1000);
    }
}

function ElementInit() {
    var $CardBody = $("#MarqueeContent > .card-body")
    $disp_opt = $CardBody.children(".select_placement").children("button").children("span");
    $title = $CardBody.children(".input_text").children("textarea");
    $link = $CardBody.children(".input_link").children(".input_link");
    $target = $CardBody.children(".input_link").children(".checkbox_link").children("input");
    $date = $CardBody.children(".input_date").children("input");
    $permanent = $CardBody.children(".input_date").children(".checkbox_permanent").children("input");
    $input_number = $CardBody.children(".input_text").children("div").children(".input_number");
}

function contentReady(e) {
    co.Marquees.GetAllKey().done(function (result) {
        keyArray = result
        marquee_list = e;
        HashDataEdit();
    });
}

function hashChange(e) {
    if (!!e) {
        HashDataEdit();
        e.preventDefault();
    } else {
        console.log("HashChange錯誤")
    }
}

function HashDataEdit() {
    if (window.location.hash != "") {
        if (window.currentHash != window.location.hash) {
            var hash = window.location.hash.replace("#", "");
            if (parseInt(hash) == 0) {
                keyId = 0;
                MoveToContent();
            } else if (keyArray.indexOf(parseInt(hash)) > -1) {
                MoveToContent();
                co.Marquees.Get(parseInt(hash)).done(function (result) {
                    keyId = result.id;
                    FormDataSet(result.title, result.disp_opt, result.link, result.target, result.permanent, result.startTime, result.endTime);
                })
            } else {
                window.location.hash = ""
            }
        }
    } else {
        BackToList();
    }
}

function editButtonClicked(e) {
    MoveToContent();

    var data = e.row.data;
    keyId = e.row.key;
    window.location.hash = keyId

    FormDataSet(data.title, data.disp_opt, data.link, data.target, data.permanent, data.StartTime, data.EndTime)
}

function FormDataSet(title, disp, link, target, permanent, startTime, endTime) {
    startDate = startTime;
    endDate = endTime;
    FormDataClear();
    $title.val(title);
    $input_number.text($title.val().length);
    $disp_opt.text(disp ? "visibility" : "visibility_off");
    $link.val(link);
    $target.prop("checked", target);
    $permanent.prop("checked", permanent);
    if (permanent) {
        $date.val('');
        $date.attr("disabled", "disabled");
        $date.siblings("span").removeClass("bg-transparent");
    } else {
        $date.removeAttr("disabled");
        $date.siblings("span").addClass("bg-transparent");
        startTime != null && $picker.data('daterangepicker').setStartDate(startTime);
        endTime != null && $picker.data('daterangepicker').setEndDate(endTime);
    }
}

function FormDataClear() {
    $title.val("");
    $input_number.text(0);
    $disp_opt.text("visibility");
    $link.val("https://");
    $target.prop("checked", false);
    $permanent.prop("checked", false);
    $date.val("");
    console.log($date.val())
    $date.removeAttr("disabled");
    $date.siblings("span").addClass("bg-transparent");
}

function deleteButtonClicked(e) {
    Coker.sweet.confirm("刪除資料", "刪除後不可返回", "確定刪除", "取消", function () {
        co.Marquees.Delete(e.row.key);
        e.component.refresh();
    });
}

function Add(display, success_text, error_text) {
    co.Marquees.Add({
        WebsiteId: $.cookie('WebSiteId'),
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
        setTimeout(function () {
            BackToList();
            marquee_list.component.refresh();
        }, 1000);
    }).fail(function () {
        Coker.sweet.error("錯誤", error_text, null, true);
    });
}

function Update(display, success_text, error_text) {
    co.Marquees.Update({
        id: keyId,
        WebsiteId: $.cookie('WebSiteId'),
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
        setTimeout(function () {
            BackToList();
            marquee_list.component.refresh();
        }, 1000);
    }).fail(function () {
        Coker.sweet.error("錯誤", error_text, null, true);
    });
}

function MoveToContent() {
    $("#PostForm").removeClass("was-validated");
    $("#MarqueeList").addClass("d-none");
    $("#MarqueeContent").removeClass("d-none");
}

function BackToList() {
    $("#MarqueeList").removeClass("d-none");
    $("#MarqueeContent").addClass("d-none");
    window.location.hash = ""
}