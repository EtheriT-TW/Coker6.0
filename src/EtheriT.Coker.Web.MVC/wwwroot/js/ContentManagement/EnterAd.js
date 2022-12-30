var $btn_display, $title, $input_sort, $check_sort, $link, $target, $date, $picker, $permanent
var startDate, endDate, keyId, disp_opt = true
var enterAd_list

function PageReady() {

    ElementInit();

    $picker = $("#InputDate");

    co.Picker.Init($picker);

    $picker.on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format('YYYY/MM/DD HH:mm') + ' ~ ' + picker.endDate.format('YYYY/MM/DD HH:mm'));
        startDate = picker.startDate.format("");
        endDate = picker.endDate.format("");
    });

    const forms = $('#AdForm');
    (() => {
        Array.from(forms).forEach(form => {
            form.addEventListener('submit', event => {
                if (!form.checkValidity()) {
                    event.preventDefault()
                    event.stopPropagation()
                } else {
                    event.preventDefault();
                    Coker.sweet.confirm("即將發布", "發布後將直接顯示於安排的位置", "發布", "取消", function () {
                        AddUp(disp_opt, "已成功發布", "發布發生未知錯誤");
                    });
                }
                form.classList.add('was-validated')
                WasValidated();
            }, false)
        })
    })()


    $(".btn_back").on("click", function () {
        Coker.sweet.confirm("返回廣告列表", "資料將不被保存", "確定", "取消", function () {
            history.back();
        });
    })
    $(".btn_add").on("click", function () {
        FormDataClear();
        window.location.hash = 0;
        HashDataEdit();
    });
    $btn_display.on("click", function () {
        if (disp_opt) {
            $btn_display.children("span").text("visibility_off");
            disp_opt = !disp_opt;
        } else {
            $btn_display.children("span").text("visibility");
            disp_opt = !disp_opt;
        }
    })
    $check_sort.on("click", function () {
        if ($check_sort.is(":checked")) {
            $input_sort.removeAttr("disabled");
        } else {
            $input_sort.val('');
            $input_sort.attr("disabled", "disabled");
        }
    })
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
    $title = $("#InputName");
    $input_sort = $("#InputSort");
    $check_sort = $("#SortCheck");
    $link = $("#InputLink");
    $target = $("#TargetCheck");
    $date = $("#InputDate");
    $permanent = $("#PermanentCheck");
}

function contentReady(e) {
    enterAd_list = e;
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
                co.HtmlContent.Get(parseInt(hash)).done(function (result) {
                    if (result != null) {
                        MoveToContent();
                        FormDataSet(result);
                    } else {
                        window.location.hash = ""
                    }
                })
            }
        }
    } else {
        BackToList();
    }
}

function editButtonClicked(e) {
    MoveToContent();
    keyId = e.row.key;
    window.location.hash = keyId;
}

function FormDataSet(result) {
    FormDataClear();
    keyId = result.id;
    startDate = result.startDate;
    endDate = result.endDate;
    $btn_display.children("span").text(result.disp_opt ? "visibility" : "visibility_off");
    disp_opt = result.disp_opt;
    $title.val(result.title);
    if (result.ser_no != 500) {
        $check_sort.prop("checked", true);
        $input_sort.removeAttr("disabled", "disabled");
        $input_sort.val(result.ser_no)
    }
    $link.val(result.link);
    $target.prop("checked", result.target);
    if (result.permanent) {
        $date.val('');
        $date.attr("disabled", "disabled");
        $permanent.prop("checked", true);
    } else {
        startDate != null && $picker.data('daterangepicker').setStartDate(startDate);
        endDate != null && $picker.data('daterangepicker').setEndDate(endDate);
    }
}

function FormDataClear() {
    keyId = 0;
    $btn_display.children("span").text("visibility");
    disp_opt = true;
    $title.val("");
    $input_sort.val("")
    $input_sort.attr("disabled", "disabled");
    $check_sort.prop("checked", false);
    $link.val("https://");
    $target.prop("checked", true);
    $permanent.prop("checked", true);
    $date.val('');
    $date.attr("disabled", "disabled");
    startDate = null;
    endDate = null;
}

function deleteButtonClicked(e) {
    Coker.sweet.confirm("刪除資料", "刪除後不可返回", "確定刪除", "取消", function () {
        co.HtmlContent.Delete(e.row.key).done(function (result) {
            if (result.success) {
                e.component.refresh();
            } else {
                Coker.sweet.error("錯誤", "刪除資料發生錯誤", null, true);
            }
        }).fail(function () {
            Coker.sweet.error("錯誤", "刪除資料發生錯誤", null, true);
        })
    });
}

function AddUp(display, success_text, error_text) {
    co.HtmlContent.AddUp({
        Id: keyId,
        TId: $.cookie('secret'),
        Img: "/images/mu_0.jpg",
        Content: "",
        Type: 8,
        Title: $title.val(),
        ObjectType: 1,
        Disp_opt: display,
        Ser_no: $check_sort.is(":checked") ? $input_sort.val() : 500,
        Link: $link.val(),
        Target: $target.is(":checked"),
        StartDate: startDate,
        EndDate: endDate,
        permanent: $permanent.is(":checked")
    }).done(function (result) {
        if (result.success) {
            Coker.sweet.success(success_text, null, true);
            setTimeout(function () {
                BackToList();
                enterAd_list.component.refresh();
            }, 1000);
        } else {
            Coker.sweet.error("錯誤", error_text, null, true);
        }
    }).fail(function () {
        Coker.sweet.error("錯誤", error_text, null, true);
    })
}

function MoveToContent() {
    UnValidated();
    $("#EnterAdList").addClass("d-none");
    $("#EnterAdContent").removeClass("d-none");
}

function BackToList() {
    $("#EnterAdList").removeClass("d-none");
    $("#EnterAdContent").addClass("d-none");
    window.location.hash = ""
}

function WasValidated() {
    $(".icon_hint").addClass("pe-4");
    $check_sort.parents(".checkbox").first().addClass("pe-4");
    $target.parents(".checkbox").first().addClass("pe-4");
    $permanent.parents(".checkbox").first().addClass("pe-4");
}

function UnValidated() {
    $("#AdForm").removeClass("was-validated");
    $(".icon_hint").removeClass("pe-4");
    $check_sort.parents(".checkbox").first().removeClass("pe-4");
    $target.parents(".checkbox").first().removeClass("pe-4");
    $permanent.parents(".checkbox").first().removeClass("pe-4");
}