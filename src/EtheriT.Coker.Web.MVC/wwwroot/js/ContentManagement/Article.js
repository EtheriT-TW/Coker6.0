var $btn_display, $btn_pop_visible, $title, $title_text, $describe, $describe_text, $sort, $sort_input, $sort_checkbox, $picker, $nodeDate, $permanent, $removedFromShelves;
var startDate, endDate, keyId, disp_opt = false, pop_visible = false;
var article_list;
var setPage;

function PageReady() {
    // 啟動
    const editor = grapesInit({
        save: function (html, css) {
            var _dfr = $.Deferred();
            co.Articles.SaveConten({
                Id: $("#gjs").data("id"),
                SaveHtml: html,
                SaveCss: css
            }).done(function (resutlt) {
                if (resutlt.success) _dfr.resolve();
                else co.sweet.error(resutlt.error);
            });
            return _dfr.promise();
        },
        import: function (html, css) {
            var _dfr = $.Deferred();
            co.Articles.ImportConten({
                Id: $("#gjs").data("id"),
                SaveHtml: html,
                SaveCss: css
            }).done(function (resutlt) {
                if (resutlt.success) _dfr.resolve();
                else co.sweet.error(resutlt.error);
            });
            return _dfr.promise();
        },
        getComponer: function () {
            var _dfr = $.Deferred();
            co.HtmlContent.GetAllComponent().done(function (result) {
                if (result.success) _dfr.resolve(result.list);
                else co.sweet.error(resutlt.error);
            });
            return _dfr.promise();
        }
    });

    //設定html資料
    setPage = function (id) {
        $("body").addClass("grapesEdit");
        co.Articles.GetConten({ Id: id }).done(function (result) {
            if (result.success) {
                var html = co.Data.HtmlDecode(result.conten.saveHtml);
                co.Grapes.setEditor(editor, html, result.conten.saveCss);
                co.Grapes.setFile(editor, id, 2);
                if (!!result.title) $("#TopLine .title").text(result.title);
            } else {
                co.sweet.error(result.error);
            }
        });
    }

    ImageUploadModalInit($("#ImageUpload"));
    ElementInit();
    TagListModalInit();

    const forms = $('#ArticletForm');
    (() => {
        Array.from(forms).forEach(form => {
            form.addEventListener('submit', event => {
                if (!form.checkValidity()) {
                    event.preventDefault()
                    event.stopPropagation()
                } else {
                    event.preventDefault();
                    Coker.sweet.confirm("即將儲存", "儲存後將顯示於安排的位置", "儲存", "取消", function () {
                        AddUp("已成功儲存", "儲存發生未知錯誤");
                    });
                }
                form.classList.add('was-validated')
                WasValidated();
            }, false)
        })
    })()

    $(".btn_back").on("click", function () {
        Coker.sweet.confirm("返回文章列表", "資料將不被保存", "確定", "取消", function () {
            article_list.component.refresh();
            BackToList();
        });
    })

    $(".btn_add").on("click", function () {
        FormDataClear();
        window.location.hash = 0;
        HashDataEdit();
    });

    $(".btn_to_canvas").on("click", function (event) {
        event.preventDefault()

        Swal.fire({
            icon: 'info',
            title: "前往文章編輯頁",
            html: "是否保存資料?",
            showCancelButton: true,
            showDenyButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#888888',
            denyButtonColor: '#d33',
            confirmButtonText: "　是　",
            denyButtonText: "　否　",
            cancelButtonText: "　取消　",
            reverseButtons: true
        }).then((result) => {
            if (result.isConfirmed) {
                AddUp("資料已儲存", "儲存發生未知錯誤", "canvas");
            } else if (result.isDenied) {
                var hash = window.location.hash.replace("#", "") + "-1";
                window.location.hash = hash;
                MoveToCanvas();
            }
        })
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

    $btn_pop_visible.on("click", function () {
        if (pop_visible) {
            $btn_pop_visible.children("span").text("group_off");
            pop_visible = !pop_visible;
        } else {
            $btn_pop_visible.children("span").text("group");
            pop_visible = !pop_visible;
        }
    })

    $title_text.on('keyup', function () {
        var $self = $(this);
        $title.children("div").children(".count").text($self.val().length)
    });

    $describe_text.on('keyup', function () {
        var $self = $(this);
        $describe.children("div").children(".count").text($self.val().length)
    });

    $sort_checkbox.on("click", function () {
        var $self = $(this);
        if ($self.is(":checked")) {
            $sort_input.removeAttr("disabled");
        } else {
            $sort_input.val('');
            $sort_input.attr("disabled", "disabled");
        }
    })

    co.Picker.Init($picker);
    co.Picker.Init($nodeDate, { singleDatePicker: true, timePicker: false, locale: { format: 'YYYY/MM/DD' } });
    $picker.on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format('YYYY/MM/DD HH:mm') + ' ~ ' + picker.endDate.format('YYYY/MM/DD HH:mm'));
        startDate = picker.startDate.format("");
        endDate = picker.endDate.format("");
    });
    $permanent.on("click", function () {
        if ($permanent.is(":checked")) {
            $picker.val('');
            $picker.attr("disabled", "disabled");
            startDate = null;
            endDate = null;
        } else {
            $picker.removeAttr("disabled");
        }
    })

    if ("onhashchange" in window) {
        window.onhashchange = hashChange;
    } else {
        setInterval(hashChange, 1000);
    }
}

function ElementInit() {
    $btn_display = $(".btn_display");
    $btn_pop_visible = $(".btn_pop_visible");
    $title = $(".title");
    $title_text = $title.children("textarea");
    $describe = $(".describe");
    $describe_text = $describe.children("textarea");
    $sort = $(".sort");
    $sort_input = $sort.children("input");
    $sort_checkbox = $sort.children(".checkbox").children("input");
    $picker = $("#InputDate");
    $nodeDate = $("#NodeDate");
    $permanent = $("#PermanentCheck");
    $removedFromShelves = $("#RemovedFromShelves");
}

function contentReady(e) {
    article_list = e;
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
                window.location.hash = 0;
                keyId = 0;
                FormDataClear();
                MoveToContent();
            } else {
                co.Articles.GetDataOne(parseInt(hash)).done(function (result) {
                    if (result != null) {
                        keyId = parseInt(hash);
                        if (hash.indexOf("-") > 0) {
                            MoveToCanvas();
                        } else {
                            MoveToContent();
                            FormDataSet(result);
                        }
                    } else {
                        window.location.hash = ""
                        keyId = "";
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

function FormDataClear() {
    TagDataClear();
    ImageUploadModalClear($("#ImageUpload"));
    keyId = 0;
    $btn_display.children("span").text("visibility_off");
    $btn_pop_visible.children("span").text("group_off");
    $title_text.val("");
    $title.children("div").children(".count").text(0);
    $describe_text.val("");
    $describe.children("div").children(".count").text(0);
    $sort_input.val("");
    $sort_input.attr("disabled", "disabled");
    $sort_checkbox.prop("checked", false);
    $permanent.prop("checked", true);
    $removedFromShelves.prop("checked", true);
    pop_visible = false;
    disp_opt = false;
}

function FormDataSet(result) {
    FormDataClear();
    co.File.getImgFile({ Sid: result.id, Type: 6, Size: 3 }).done(function (file) {
        if (file.length>0)
            ImageUploadModalDataInsert($("#ImageUpload"), file[0].id, file[0].link, file[0].name)
    });
    keyId = result.id;

    if (result.visible) {
        $btn_display.children("span").text("visibility");
    }

    if (result.popularVisible) {
        $btn_pop_visible.children("span").text("group");
    }

    $title_text.val(result.title);
    $title.children("div").children(".count").text(result.title.length);
    $describe_text.val(result.description);
    $describe.children("div").children(".count").text(result.description.length);
    $nodeDate.val(result.nodeDate);
    startDate = result.startTime;
    endDate = result.endTime;
    pop_visible = result.popularVisible;
    disp_opt = result.visible;
    $removedFromShelves.prop("checked", result.removedFromShelves);
    

    if (result.serNO != 500) {
        $sort_input.val(result.serNO);
        $sort_input.removeAttr("disabled");
        $sort_checkbox.prop("checked", true);
    }

    if (result.permanent) {
        $picker.val('');
        $picker.attr("disabled", "disabled");
        $permanent.prop("checked", true);
    } else {
        startDate != null && $picker.data('daterangepicker').setStartDate(startDate);
        endDate != null && $picker.data('daterangepicker').setEndDate(endDate);
        $permanent.prop("checked", false);
    }

    if (result.nodeDate != null) {
        $nodeDate.data('daterangepicker').setStartDate(result.nodeDate);
        $nodeDate.data('daterangepicker').setEndDate(result.nodeDate);
    }

    TagDataSet(result.tagDatas);

}

function paletteButtonClicked(e) {
    keyId = e.row.key;
    window.location.hash = keyId + "-1";
}

function deleteButtonClicked(e) {
    Coker.sweet.confirm("刪除資料", "刪除後不可返回", "確定刪除", "取消", function () {
        co.Articles.Delete(e.row.key).done(function (result) {
            if (result.success) {
                e.component.refresh();
            }
        });
    });
}

function AddUp(success_text, error_text, place) {
    if ($("#ImageUpload .img_input_frame").data("delectList") != null) {
        co.File.DeleteFileById({
            Sid: keyId,
            Type: 6,
            Fid: $("#ImageUpload").data("delectList")
        });
    }

    co.Articles.AddUp({
        Id: keyId,
        Title: $title_text.val(),
        Description: $describe_text.val(),
        Visible: disp_opt,
        SerNO: $sort_checkbox.is(":checked") ? $sort_input.val() : 500,
        PopularVisible: pop_visible,
        TagSelected: tag_list,
        permanent: $permanent.is(":checked"),
        StartTime: startDate,
        EndTime: endDate,
        NodeDate: $nodeDate.val(),
        RemovedFromShelves:$removedFromShelves.is(":checked")
    }).done(function (result) {
        if (result.success) {
            if ($("#ImageUpload .img_input").data("file") != null && $("#ImageUpload .img_input").data("file").File != null && $("#ImageUpload .img_input").data("file").id == 0) {
                var formData = new FormData();
                formData.append("files", $("#ImageUpload .img_input").data("file").File);
                formData.append("type", 6);
                formData.append("sid", result.message);
                formData.append("serno", 500);
                co.File.Upload(formData).done(function (response) {
                    if (response.success) {
                        Coker.sweet.success(success_text, null, true);
                        setTimeout(function () {
                            if (place == "canvas") {
                                setTimeout(function () {
                                    if (keyId == 0) window.location.hash = `${result.message}-1`;
                                    else window.location.hash += "-1";
                                }, 1000);
                            } else {
                                setTimeout(function () {
                                    article_list.component.refresh();
                                    BackToList();
                                }, 1000);
                            }
                        }, 1000);
                    }
                });
            } else {
                Coker.sweet.success(success_text, null, true);
                setTimeout(function () {
                    if (place == "canvas") {
                        setTimeout(function () {
                            if (keyId == 0) window.location.hash = `${result.message}-1`;
                            else window.location.hash += "-1";
                        }, 1000);
                    } else {
                        setTimeout(function () {
                            article_list.component.refresh();
                            BackToList();
                        }, 1000);
                    }
                }, 1000);
            }
        } else {
            Coker.sweet.error("錯誤", error_text, null, true);
        }
    }).fail(function () {
        Coker.sweet.error("錯誤", error_text, null, true);
    });
}

function MoveToContent() {
    UnValidated();
    /*if (keyId == 0) {
        $(".btn_to_canvas").addClass("text-dark");
        $(".btn_to_canvas").attr('disabled', '');
    } else {
        $(".btn_to_canvas").removeClass("text-dark");
        $(".btn_to_canvas").removeAttr('disabled');
    }*/
    $("#ArticleList").addClass("d-none");
    $("#ArticleContent").removeClass("d-none");
    $("#ArticleCanvas").addClass("d-none");
    $("body").removeClass("grapesEdit");
}

function MoveToCanvas() {
    UnValidated();
    $("#gjs").data("id", keyId);
    setPage(keyId);
    $("#TopLine > div > a").removeClass("d-none");
    $("#ArticleList").addClass("d-none");
    $("#ArticleContent").addClass("d-none");
    $("#ArticleCanvas").removeClass("d-none");
}

function BackToList() {
    $("#TopLine > div > a").addClass("d-none");
    $("#ArticleList").removeClass("d-none");
    $("#ArticleContent").addClass("d-none");
    $("#ArticleCanvas").addClass("d-none");
    $("body").removeClass("grapesEdit");
    $("#TopLine .title").text("文章管理");
    window.location.hash = ""
}

function WasValidated() {
    $sort.children(".checkbox").addClass("pe-4");
}

function UnValidated() {
    $("#ArticletForm").removeClass("was-validated");
    $sort.children(".checkbox").removeClass("pe-4");
}