var $btn_display, $title, $illustrate, $illustrate_count, $input_sort, $check_sort, $date, $permanent
var startDate, endDate, keyId, disp_opt = true;
var technicalCertificate_list

function PageReady() {
    co.TechnicalCertificate = {
        AddUp: function (data) {
            return $.ajax({
                url: "/api/TechnicalCertificate/AddUp",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        Get: function (id) {
            return $.ajax({
                url: "/api/TechnicalCertificate/GetOne/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { id: id },
            });
        },
        Delete: function (id) {
            return $.ajax({
                url: "/api/TechnicalCertificate/Delete/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { Id: id },
            });
        },
    };

    ImageUploadModalInit($("#ImageUpload"));
    ElementInit();

    co.Picker.Init($picker);

    $picker.on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format('YYYY/MM/DD HH:mm') + ' ~ ' + picker.endDate.format('YYYY/MM/DD HH:mm'));
        startDate = picker.startDate.format("");
        endDate = picker.endDate.format("");
    });

    const forms = $('#TechnicalCertificateForm');
    (() => {
        Array.from(forms).forEach(form => {
            form.addEventListener('submit', event => {
                if (!form.checkValidity()) {
                    event.preventDefault()
                    event.stopPropagation()
                }
                event.preventDefault();
                if ($("#ImageUpload").find(".img_input").length <= 1) co.sweet.error("資料有誤", "至少需上傳一張圖片", null, false);
                else {
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
        Coker.sweet.confirm("返回技術證照列表", "資料將不被保存", "確定", "取消", function () {
            history.back();
        });
    })
    $(".btn_add").on("click", function () {
        FormDataClear();
        window.location.hash = 0;
        HashDataEdit();
    });
    $(".btn_expand").on("click", function () {
        var $self = $(this);
        if ($self.children("span").text() == "expand_more") {
            $self.children("span").text("expand_less")
        } else {
            $self.children("span").text("expand_more")
        }
    })

    $btn_display.on("click", function () {
        if (disp_opt) {
            $btn_display.children("span").text("visibility_off");
            disp_opt = !disp_opt;
        } else {
            $btn_display.children("span").text("visibility");
            disp_opt = !disp_opt;
        }
    });
    $illustrate.on('keyup', function () {
        $illustrate_count.text($illustrate.val().length);
    });
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
    $picker = $("#InputDate");
    $btn_display = $("#Btn_Display");
    $title = $("#InputTitle");
    $illustrate = $("#InputIllustrate");
    $illustrate_count = $("#TechnicalCertificateForm > .illustrate .illustrate_count");
    $input_sort = $("#InputSort");
    $check_sort = $("#SortCheck");
    $date = $("#InputDate");
    $permanent = $("#PermanentCheck");
}

function contentReady(e) {
    technicalCertificate_list = e;
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
                co.TechnicalCertificate.Get(parseInt(hash)).done(function (result) {
                    MoveToContent();
                    if (result != null) {
                        co.File.getImgFile({ Sid: result.id, Type: 4, Size: 3 }).done(function (img_result) {
                            FormDataSet(result, img_result);
                        });
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

function FormDataSet(result, img_result) {
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
    $illustrate.val(result.description);
    $illustrate_count.text($illustrate.val().length);
    if (result.permanent) {
        $date.val('');
        $date.attr("disabled", "disabled");
        $permanent.prop("checked", true);
    } else {
        startDate != null && $picker.data('daterangepicker').setStartDate(startDate);
        endDate != null && $picker.data('daterangepicker').setEndDate(endDate);
    }

    for (var i = 0; i < img_result.length; i++) {
        ImageUploadModalDataInsert($("#ImageUpload"), img_result[i].id, img_result[i].link, img_result[i].name)
    }
}

function FormDataClear() {
    ImageUploadModalClear($("#ImageUpload"));
    keyId = 0;
    $btn_display.children("span").text("visibility");
    disp_opt = true;
    $title.val("");
    $illustrate.val("");
    $illustrate_count.text(0);
    $input_sort.val("")
    $input_sort.attr("disabled", "disabled");
    $check_sort.prop("checked", false);
    $permanent.prop("checked", true);
    $date.val('');
    $date.attr("disabled", "disabled");
    startDate = null;
    endDate = null;
}

function deleteButtonClicked(e) {
    Coker.sweet.confirm("刪除資料", "刪除後不可返回", "確定刪除", "取消", function () {
        co.TechnicalCertificate.Delete(e.row.key).done(function (result) {
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
    if (typeof ($("#ImageUpload").find(".img_input_frame").data("delectList")) != "undefined" && $("#ImageUpload").find(".img_input_frame").data("delectList") != null) {
        co.File.DeleteFileById({
            sid: keyId,
            type: 4,
            fid: $("#ImageUpload").find(".img_input_frame").data("delectList")
        });
    }

    co.TechnicalCertificate.AddUp({
        Id: keyId,
        TId: $.cookie('secret'),
        Disp_opt: display,
        Img: "",
        Title: $title.val(),
        Description: $illustrate.val(),
        Ser_no: $check_sort.is(":checked") ? $input_sort.val() : 500,
        StartDate: startDate,
        EndDate: endDate,
        permanent: $permanent.is(":checked")
    }).done(function (result) {
        if (result.success) {
            var file_num = 0, success_file_num = 0, error_file_num = 0;
            $("#ImageUpload .img_input_frame > .img_input").each(function () {
                var $item = $(this);
                if (typeof ($item.data("file")) != "undefined" && $item.data("file").id == 0) {
                    file_num++;
                    var formData = new FormData();
                    formData.append("type", 4);
                    formData.append("sid", result.message);
                    formData.append("serno", 500);
                    formData.append("files", $item.data("file").File[0]);
                    formData.append("files", $item.data("file").File[1]);
                    formData.append("files", $item.data("file").File[2]);
                    co.File.Upload(formData).done(function (result) {
                        if (result.success) success_file_num++;
                        else error_file_num++;
                    });
                }
            })
            const upload_timmer = function () {
                if (file_num == (success_file_num + error_file_num)) {
                    if (error_file_num > 0) {
                        Coker.sweet.error("錯誤", error_text, null, true);
                    } else {
                        Coker.sweet.success(success_text, null, true);
                        setTimeout(function () {
                            BackToList();
                            FormDataClear();
                            technicalCertificate_list.component.refresh();
                        }, 1000);
                    }
                }
                else setTimeout(upload_timmer, 100);
            }
            setTimeout(upload_timmer, 100);
        } else {
            Coker.sweet.error("錯誤", error_text, null, true);
        }
    }).fail(function () {
        Coker.sweet.error("錯誤", error_text, null, true);
    })

}

function MoveToContent() {
    UnValidated();
    $("#TechnicalCertificateList").addClass("d-none");
    $("#TechnicalCertificateContent").removeClass("d-none");
}

function BackToList() {
    $("#TechnicalCertificateList").removeClass("d-none");
    $("#TechnicalCertificateContent").addClass("d-none");
    window.location.hash = ""
}

function WasValidated() {
    $(".icon_hint").addClass("pe-4");
    $check_sort.parents(".checkbox").first().addClass("pe-4");
    $permanent.parents(".checkbox").first().addClass("pe-4");
}

function UnValidated() {
    $("#TechnicalCertificateForm").removeClass("was-validated");
    $(".icon_hint").removeClass("pe-4");
    $check_sort.parents(".checkbox").first().removeClass("pe-4");
    $permanent.parents(".checkbox").first().removeClass("pe-4");
}