var $btn_input_pic, $btn_img_delete, $btn_display, $title, $illustrate, $illustrate_count, $input_sort, $check_sort, $date, $permanent
var $input_pic, $img_preview;
var startDate, endDate, keyId, disp_opt = true;
var img_file = [], img_start_index = null, img_delete_list = [];
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
                } else if (img_file == undefined) {
                    Coker.sweet.error("尚未上傳證照圖片", "請上傳一張圖片");
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

    $btn_input_pic.on("click", function (even) {
        even.preventDefault();
        var $self = $(this);
        if ($self.data("index") != undefined) {
            img_start_index = (parseInt($self.data("index")) - 1) * 3;
        } else {
            img_start_index = null;
        }
        $self.prev(".input_pic").click();
    })

    $input_pic.change(function () {
        if (img_start_index == null) {
            uploadImage(this.files[0]);
        } else {
            reuploadImage(this.files[0]);
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
    $btn_input_pic = $(".btn_input_pic");
    $btn_img_delete = $(".btn_img_delete");
    $input_pic = $(".input_pic");
    $img_preview = $(".img_preview");
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

    if (img_result != null) {
        for (var i = 0; i < img_result.length; i++) {
            var item = $($("#Template_Image_Preview").html()).clone();
            var item_btn_add = item.find(".btn_input_pic"),
                item_btn_del = item.find(".btn_img_delete"),
                item_input = item.find(".input_pic"),
                item_img = item.find("img"),
                item_name = item.find(".img_name");

            item_btn_add.data("id", img_result[i].id);
            item_name.text(img_result[i].name);
            item_img.attr("src", img_result[i].link);

            $("#TechnicalCertificateForm > div > .img_input_frame").prepend(item);

            item_btn_add.on("click", function (even) {
                even.preventDefault();
                var $self = $(this);
                if ($self.data("id") != 0) {
                    img_delete_list.push(parseInt($self.data("id")));
                    $self.data("id", 0);
                }
                if ($self.data("index") != undefined && $self.data("index") != "") {
                    img_start_index = (parseInt($self.data("index")) - 1) * 3;
                } else {
                    $self.data("index", img_file.length / 3 + 1)
                    img_start_index = (parseInt($self.data("index")) - 1) * 3;
                }
                $self.parents("div").first().siblings(".input_pic").click();
            })

            item_btn_del.on("click", function (even) {
                even.preventDefault();
                var $self_bro = $(this).siblings(".btn_input_pic");
                img_delete_list.push(parseInt($self_bro.data("id")));
                $(this).parents("div").first().parents("div").first().remove();
            })

            item_input.change(function () {
                if (img_start_index == null) {
                    uploadImage(this.files[0]);
                } else {
                    reuploadImage(this.files[0], $(this));
                }
            })
        }
    }
}

function FormDataClear() {
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

    $("#TechnicalCertificateForm > div > .img_input_frame").children("div").each(function () {
        if ($(this).siblings().length > 1) {
            $(this).remove();
        }
    });
    img_file = [];
    img_start_index = null;

}

function uploadImage(this_file) {
    img_file.push(this_file);

    var item = $($("#Template_Image_Preview").html()).clone();
    var item_btn_add = item.find(".btn_input_pic"),
        item_btn_del = item.find(".btn_img_delete"),
        item_input = item.find(".input_pic"),
        item_img = item.find("img"),
        item_name = item.find(".img_name");
    img_start_index = img_file.length - 1;
    item_name.text(img_file[img_start_index].name);
    item_btn_add.data("index", img_start_index / 3 + 1);

    var htmlImageCompress;
    htmlImageCompress = new HtmlImageCompress(img_file[img_start_index], { quality: 0.7 })
    htmlImageCompress.then(function (result) {
        img_file.push(new File([result.file], img_file[img_start_index].name));
    }).catch(function (err) {
        console.log($`發生錯誤：${err}`);
    })
    htmlImageCompress = new HtmlImageCompress(img_file[img_start_index], { quality: 0.3 })
    htmlImageCompress.then(function (result) {
        img_file.push(new File([result.file], img_file[img_start_index].name));
        var reader = new FileReader();
        reader.readAsDataURL(img_file[img_start_index + 2]);
        reader.onload = (function (e) {
            item_img.attr("src", e.target.result);
        });
    }).catch(function (err) {
        console.log($`發生錯誤：${err}`);
    })

    $("#TechnicalCertificateForm > div > .img_input_frame").prepend(item);

    item_btn_add.on("click", function (even) {
        even.preventDefault();
        var $self = $(this);
        if ($self.data("index") != undefined) {
            img_start_index = (parseInt($self.data("index")) - 1) * 3;
        } else {
            img_start_index = null;
        }
        $self.prev(".input_pic").click();
    })

    item_btn_del.on("click", function (even) {
        even.preventDefault();
        var $self_bro = $(this).siblings(".btn_input_pic");
        img_delete_list.push(parseInt($self_bro.data("id")));
        $(this).parents("div").first().parents("div").first().remove();
    })

    item_input.change(function () {
        if (img_start_index == null) {
            uploadImage(this.files[0]);
        } else {
            reuploadImage(this.files[0], $(this));
        }
    })
}

function reuploadImage(this_file, this_input) {
    img_file[img_start_index] = this_file;
    this_input.siblings(".img_name").text(img_file[img_start_index].name);

    var htmlImageCompress;
    htmlImageCompress = new HtmlImageCompress(img_file[img_start_index], { quality: 0.7 })
    htmlImageCompress.then(function (result) {
        img_file[img_start_index + 1] = new File([result.file], img_file[img_start_index].name);
    }).catch(function (err) {
        console.log($`發生錯誤：${err}`);
    })
    htmlImageCompress = new HtmlImageCompress(img_file[img_start_index], { quality: 0.3 })
    htmlImageCompress.then(function (result) {
        img_file[img_start_index + 2] = new File([result.file], img_file[img_start_index].name);
        var reader = new FileReader();
        reader.readAsDataURL(img_file[img_start_index + 2]);
        reader.onload = (function (e) {
            this_input.next("div").children(".btn_input_pic").children("img").attr("src", e.target.result);
        });
    }).catch(function (err) {
        console.log($`發生錯誤：${err}`);
    })

}

function DeleteImage() {

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

            if (img_delete_list.length > 0) {
                img_delete_list.forEach(function (imgid) {
                    co.File.DeleteImgByImgId(imgid).done(function (result) {
                    });
                })
            }

            if (img_file.length > 0) {
                var formData = new FormData();
                formData.append("type", 4);
                formData.append("sid", result.message);
                for (var i = 0; i < img_file.length; i += 3) {
                    for (var j = i; j < i + 3; j++) {
                        formData.append('files', img_file[j]);
                    }
                    co.File.Upload(formData).done(function (result) {
                        if (result.success) {
                            Coker.sweet.success(success_text, null, true);
                            setTimeout(function () {
                                BackToList();
                                FormDataClear();
                                technicalCertificate_list.component.refresh();
                            }, 1000);
                        } else {
                            Coker.sweet.error("錯誤", error_text, null, true);
                        }
                    });
                    formData.delete('files');
                }
            } else {
                Coker.sweet.success(success_text, null, true);
                setTimeout(function () {
                    BackToList();
                    FormDataClear();
                    technicalCertificate_list.component.refresh();
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