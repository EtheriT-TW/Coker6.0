var $btn_display, $name, $name_count, $introduction, $introduction_count, $illustrate, $illustrate_count, $marks, $tag, $price, $number, $min_number, $date, $picker, $permanent
var startDate, endDate, keyId, disp_opt = true
var product_list

function PageReady() {
    //co.Product = {
    //    Add: function (data) {
    //        return $.ajax({
    //            url: "/api/Marquee/Add",
    //            type: "POST",
    //            contentType: 'application/json; charset=utf-8',
    //            headers: _c.Data.Header,
    //            data: JSON.stringify(data),
    //            dataType: "json"
    //        });
    //    },
    //    Get: function (id) {
    //        return $.ajax({
    //            url: "/api/Marquee/Get/",
    //            type: "GET",
    //            contentType: 'application/json; charset=utf-8',
    //            headers: _c.Data.Header,
    //            data: { id: id },
    //        });
    //    },
    //    Update: function (data) {
    //        return $.ajax({
    //            url: "/api/Marquee/Update",
    //            type: "POST",
    //            contentType: 'application/json; charset=utf-8',
    //            headers: _c.Data.Header,
    //            data: JSON.stringify(data),
    //            dataType: "json"
    //        });
    //    },
    //    Delete: function (id) {
    //        return $.ajax({
    //            url: "/api/Marquee/Delete/",
    //            type: "GET",
    //            contentType: 'application/json; charset=utf-8',
    //            headers: _c.Data.Header,
    //            data: { id: id },
    //        });
    //    }
    //};

    ElementInit();

    $picker = $("#InputDate");

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

    const forms = $('#ProductForm');
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
                            //Update(disp_opt, "已成功發布", "發布發生未知錯誤");
                        } else {
                            //Add(disp_opt, "已成功發布", "發布發生未知錯誤");
                        }
                    });
                }
                form.classList.add('was-validated')
            }, false)
        })
    })()

    $(".btn_back").on("click", function () {
        Coker.sweet.confirm("返回商品列表", "資料將不被保存", "確定", "取消", function () {
            history.back();
        });
    })
    $(".btn_add").on("click", function () {
        FormDataClear();
        window.location.hash = 0;
        HashDataEdit();
    });
    $(".btn_save").on("click", function () {
        disp_opt = false;
        if (keyId > 0) {
            //Update(false, "已存為草稿", "儲存草稿發生未知錯誤");
        } else {
            //Add(false, "已存為草稿", "儲存草稿發生未知錯誤");
        }
    });
    $(".btn_input_pic").on("click", function (event) {
        event.preventDefault();
        $(".input_pic").click();
    })
    $btn_display.on("click", function () {
        if (disp_opt) {
            $btn_display.children("span").text("visibility_off");
            disp_opt = !disp_opt;
        } else {
            $btn_display.children("span").text("visibility");
            disp_opt = !disp_opt;
        }
    })
    $(".btn_expand").on("click", function () {
        var $self = $(this);
        console.log($self)
        if ($self.children("span").text() == "expand_more") {
            $self.children("span").text("expand_less")
        } else {
            $self.children("span").text("expand_more")
        }
    })

    $name.on('keyup', function () {
        $name_count.text($name.val().length);
    });
    $introduction.on('keyup', function () {
        $introduction_count.text($introduction.val().length);
    });
    $illustrate.on('keyup', function () {
        $illustrate_count.text($illustrate.val().length);
    });
    $("input[type='number']").change(function () {
        $(this).val($(this).val() < 0 ? 0 : $(this).val())
    });

    $permanent.on("click", function () {
        if ($permanent.is(":checked")) {
            $date.val('');
            $date.attr("disabled", "disabled");
            startDate = null;
            endDate = null;
        } else {
            $date.removeAttr("disabled");
        }
    })

    if ("onhashchange" in window) {
        window.onhashchange = hashChange;
    } else {
        setInterval(hashChange, 1000);
    }
}

function ElementInit() {
    $btn_display = $("#Btn_Display");
    $name = $("#InputName");
    $name_count = $("#ProductForm > .name .name_count");
    $introduction = $("#InputIntroduction");
    $introduction_count = $("#ProductForm > .introduction .introduction_count");
    $illustrate = $("#InputIllustrate");
    $illustrate_count = $("#ProductForm > .illustrate .illustrate_count");
    $marks = $("#InputMarks");
    $tag = $("#InputTag");
    $price = $("#InputPrice");
    $number = $("#InputNumber");
    $min_number = $("#InputMinNumber");
    $date = $("#InputDate");
    $permanent = $("#PermanentCheck");
}

function FormDataClear() {
    keyId = 0;
    $btn_display.children("span").text("visibility");
    disp_opt = true;
    $name.val("");
    $name_count.text(0);
    $introduction.val("");
    $introduction_count.text(0);
    $illustrate.val("");
    $illustrate_count.text(0);
    $marks.val("");
    $tag.val("");
    $price.val("");
    $number.val("");
    $min_number.val("");
    $permanent.prop("checked", false);
    $date.val("");
    $date.removeAttr("disabled");
}

function contentReady(e) {
    product_list = e;
    HashDataEdit();
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
                FormDataClear();
                MoveToContent();
            } else {
                //co.Marquees.Get(parseInt(hash)).done(function (result) {
                //    if (result != null) {
                //        MoveToContent();
                //        keyId = result.id;
                //        FormDataSet(result.placement, result.disp_opt, result.title, result.ser_no, result.link, result.target, result.permanent, result.startTime, result.endTime);
                //    } else {
                //        window.location.hash = ""
                //    }
                //})
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

    //FormDataSet(data.placement, data.disp_opt, data.title, data.ser_no, data.link, data.target, data.permanent, data.StartTime, data.EndTime)
}

function FormDataSet(disp, name, introduction, illustrate, marks, tag, price, number, min_number, permanent, startTime, endTime) {
    startDate = startTime;
    endDate = endTime;
    FormDataClear();
    $btn_display.children("span").text(disp ? "visibility" : "visibility_off");
    disp_opt = disp;

    $name.val(name);
    $name_count.text($name.val().length);
    $introduction.val(introduction);
    $introduction_count.text($introduction.val().length);
    $illustrate.val(illustrate);
    $illustrate_count.text($illustrate.val().length);

    $marks.val(marks);
    $tag.val(tag);
    $price.val(price);
    $number.val(number);
    $min_number.val(min_number);
    $date = $("#InputDate");
    if (permanent) {
        $date.val('');
        $date.attr("disabled", "disabled");
        $permanent.prop("checked", true);
    } else {
        startTime != null && $picker.data('daterangepicker').setStartDate(startTime);
        endTime != null && $picker.data('daterangepicker').setEndDate(endTime);
    }
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
        placement: $placement.val(),
        title: $title.val(),
        disp_opt: display,
        ser_no: $check_sort.is(":checked") ? $input_sort.val() : 500,
        link: $link.val(),
        target: $target.is(":checked"),
        StartTime: startDate,
        EndTime: endDate,
        permanent: $permanent.is(":checked")
    }).done(function () {
        Coker.sweet.success(success_text, null, true);
        setTimeout(function () {
            BackToList();
            product_list.component.refresh();
        }, 1000);
    }).fail(function () {
        Coker.sweet.error("錯誤", error_text, null, true);
    });
}

function Update(display, success_text, error_text) {
    co.Marquees.Update({
        id: keyId,
        WebsiteId: $.cookie('WebSiteId'),
        placement: $placement.val(),
        title: $title.val(),
        disp_opt: display,
        ser_no: $check_sort.is(":checked") ? $input_sort.val() : 500,
        link: $link.val(),
        target: $target.is(":checked"),
        StartTime: startDate,
        EndTime: endDate,
        permanent: $permanent.is(":checked")
    }).done(function () {
        Coker.sweet.success(success_text, null, true);
        setTimeout(function () {
            BackToList();
            product_list.component.refresh();
        }, 1000);
    }).fail(function () {
        Coker.sweet.error("錯誤", error_text, null, true);
    });
}

function MoveToContent() {
    $("#ProductForm").removeClass("was-validated");
    $("#ProductList").addClass("d-none");
    $("#ProductContent").removeClass("d-none");
}

function BackToList() {
    $("#ProductList").removeClass("d-none");
    $("#ProductContent").addClass("d-none");
    window.location.hash = ""
}