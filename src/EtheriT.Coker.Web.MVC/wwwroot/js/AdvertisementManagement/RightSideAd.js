var $check_sort, $target, $date, $picker, $permanent, $Visible
var startDate, endDate, keyId, disp_opt = true
var enterAd_list
let AdForm

function PageReady() {
    ImageUploadModalInit($("#ImageUpload"));
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

                    if (typeof ($("#ImageUpload .img_input_frame > .img_input").data("file")) == "undefined" || $("#ImageUpload .img_input_frame > .img_input").data("file") == null) {
                        co.sweet.error("資料有誤", "圖示不可為空", null, false);
                    } else {
                        Coker.sweet.confirm("即將發布", "發布後將直接顯示於安排的位置", "發布", "取消", function () {
                            AddUp("已成功發布", "發布發生未知錯誤");
                        });
                    }
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
    $Visible.on("click", function () {
        $Visible.val($Visible.prop("checked"));
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
    AdForm = $('#AdForm');
    $Visible = $("#AdFormVisible");
    $check_sort = $("#AdvertiseSortCheck");
    $input_sort = $("#AdvertiseSerNO");
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
                co.Advertise.GetDataOne(parseInt(hash)).done(function (result) {
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
    console.log(result)
    co.File.getImgFile({ Sid: result.id, Type: 5, Size: 1, }).done(function (file) {
        if (file.length > 0) ImageUploadModalDataInsert($("#ImageUpload"), file[0].id, file[0].link, file[0].name)
    })
    keyId = result.id;
    result.startEndDate = 0;
    result.sortCheckbox = 1;
    co.Form.insertData(result, "#AdForm");
    $Visible.val(result.visible);
    $Visible.prop("checked", result.visible);
}

function FormDataClear() {
    ImageUploadModalClear($("#ImageUpload"));
    co.Form.clear("AdForm");
    $("#AdForm > input[name='id'").val("");
    $Visible.prop("checked", true);
    $Visible.val(true);
    $check_sort.prop("checked", false);
    $input_sort.prop("disabled", true);
    $input_sort.val("")
}

function deleteButtonClicked(e) {
    Coker.sweet.confirm("刪除資料", "刪除後不可返回", "確定刪除", "取消", function () {
        co.Advertise.Delete(e.row.key).done(function (result) {
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

function AddUp(success_text, error_text) {
    const data = co.Form.getJson($(AdForm).attr("id"));
    data.type = 2;
    var delectList = $("#ImageUpload").find(".img_input_frame").data("delectList")
    if (typeof (delectList) != "undefined" && delectList != null) {
        co.File.DeleteFileById({
            sid: keyId,
            type: 5,
            fid: delectList
        });
    }


    co.Advertise.AddUp(data).done(function (result) {
        if (result.success) {
            var file = $("#ImageUpload .img_input_frame > .img_input").data("file").File;
            if (file != null) {
                var formData = new FormData();
                formData.append("files", file);
                formData.append("type", 5);
                formData.append("sid", result.message);
                formData.append("serno", 500);
                co.File.Upload(formData).done(function (result) {
                    if (result.success) {
                        Coker.sweet.success(success_text, null, true);
                        setTimeout(function () {
                            BackToList();
                            FormDataClear();
                            enterAd_list.component.refresh();
                        }, 1000);
                    }
                });
            } else {
                Coker.sweet.success(success_text, null, true);
                setTimeout(function () {
                    BackToList();
                    FormDataClear();
                    enterAd_list.component.refresh();
                }, 1000);
            }
        } else {
            Coker.sweet.error("錯誤", error_text, null, true);
        }
    }).fail(function () {
        Coker.sweet.error("錯誤", error_text, null, true);
    })
}

function MoveToContent() {
    UnValidated();
    $("#RightSideAdList").addClass("d-none");
    $("#RightSideAdContent").removeClass("d-none");
}

function BackToList() {
    $("#RightSideAdList").removeClass("d-none");
    $("#RightSideAdContent").addClass("d-none");
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