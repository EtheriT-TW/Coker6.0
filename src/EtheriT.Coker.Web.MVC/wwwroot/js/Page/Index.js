function PageReady() {
    const myOffcanvas = new bootstrap.Offcanvas('#offcanvasSite');
    var editor = grapesInit({
        save: function (html, css) {
            var _dfr = $.Deferred();
            co.WebMesnus.saveConten({
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
            co.WebMesnus.importConten({
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

    var menuEditor = new MenuEditor('myEditor',
        {
            textConfirmDelete: "是否確認將<span class='ConfirmKeyWord'>{0}</span>選單刪除?",
            listOptions: {
                placeholderCss: { 'background-color': "#cccccc" }
            },
            iconPicker: {
                searchText: "Buscar...", labelHeader: "{0}/{1}"
            },
            maxLevel: -1, // (Optional) Default is -1 (no level limit)
            element: {
                Form: "#frmEdit",
                Update: "#btnUpdate",
                Add: '#btnAdd',
                Refresh: '#btnRefresh',
            },
            on: {
                ready: function () {
                    co.WebMesnus.GetPageTypeList().done(function (result) {
                        if (result.success) {
                            const $s = $("#PageType");
                            $(result.type).each(function () {
                                $s.append(`<option value="${this.value}">${this.key}</option>`);
                            });
                            $s.on("change", function () {
                                const $self = $(this);
                                if ($self.val() == 2) {
                                    $("#RouterNameBlock").addClass("d-none").val("Home");
                                } else {
                                    $("#RouterNameBlock").removeClass("d-none").val("");
                                }
                            })
                        }
                    });
                    ImageUploadModalInit($("#ImageUpload"), true, false);
                    ImageUploadModalInit($("#OverImageUpload"), true, false);
                },
                edit: function () {
                    openEditForm();
                    $("#btnUpdate").removeClass("d-none");
                    $("#btnRefresh,#btnAdd").addClass("d-none");
                    ImageUploadModalClear($("#ImageUpload"));
                    ImageUploadModalDataInsert($("#ImageUpload"), $("#ImageUpload").siblings("#imgId").val(), $("#ImageUpload").siblings("#imgUrl").val(), $("#ImageUpload").siblings("#imgName").val())
                    ImageUploadModalClear($("#OverImageUpload"));
                    ImageUploadModalDataInsert($("#OverImageUpload"), $("#OverImageUpload").siblings("#overImgId").val(), $("#OverImageUpload").siblings("#overImgUrl").val(), $("#ImageUpload").siblings("#overImgName").val())
                },
                del: function (data) {
                    if ($("#myEditor>li").length == 0) {
                        $("#myEditor").addClass("d-none");
                        $("#myEditor + .emptyList").removeClass("d-none");
                    }
                    co.WebMesnus.delete(data.id).done(function (result) {
                        if (result.success) co.sweet.success("已成功刪除");
                        else co.sweet.error(result.error);
                    });
                },
                add: function (cEl) {
                    var data = cEl.data();
                    $("#myEditor").removeClass("d-none");
                    $("#myEditor + .emptyList").addClass("d-none");
                    co.WebMesnus.createOrEdit(data).done(function (result) {
                        if (!result.success) co.sweet.error(result.error);
                        else {
                            data.id = parseInt(result.message);
                            var img_success = 0, overimg_success = 0;

                            var $file = $("#ImageUpload .img_input_frame > .img_input");
                            if (typeof ($file.data("file")) != "undefined" && $file.data("file") != null) {
                                var formData = new FormData();
                                formData.append("files", $file.data("file").File);
                                formData.append("type", 2);
                                formData.append("sid", data.id);
                                formData.append("serno", 500);
                                co.File.Upload(formData).done(function (result) {
                                    if (result.success) img_success = 1;
                                    else img_success = -1;
                                });
                            } else img_success = 1;

                            var $over_file = $("#OverImageUpload .img_input_frame > .img_input");
                            if (typeof ($over_file.data("file")) != "undefined" && $over_file.data("file") != null) {
                                var formData = new FormData();
                                formData.append("files", $over_file.data("file").File);
                                formData.append("type", 3);
                                formData.append("sid", data.id);
                                formData.append("serno", 500);
                                co.File.Upload(formData).done(function (result) {
                                    if (result.success) overimg_success = 1;
                                    else overimg_success = -1;
                                });
                            } else overimg_success = 1;

                            const timmer = function () {
                                if (img_success != 0 && overimg_success != 0) {
                                    ImageUploadModalClear($("#ImageUpload"));
                                    ImageUploadModalClear($("#OverImageUpload"));
                                    menuReload(menuEditor, myOffcanvas);
                                    if (!result.success) co.sweet.error(result.error);
                                    else {
                                        if (img_success == -1 || overimg_success == -1) co.sweet.erro("圖片上傳失敗");
                                        else co.sweet.success("新增成功");
                                    }
                                } else setTimeout(timmer, 100);
                            };
                            setTimeout(timmer, 100);
                        }
                    });
                },
                update: function (data) {
                    var img_success = 0, overimg_success = 0, delimg_success = 0, deloverimg_success = 0;

                    var $del_list = $("#ImageUpload .img_input_frame").data("delectList");
                    if ($del_list != null) {
                        co.File.DeleteFileById({
                            sid: data.id,
                            type: 2,
                            fid: $del_list,
                        }).done(function (result) {
                            if (result.success) delimg_success = 1
                            else delimg_success = -1
                        });
                    } else delimg_success = 1

                    var $over_del_list = $("#OverImageUpload .img_input_frame").data("delectList");
                    if ($over_del_list != null) {
                        co.File.DeleteFileById({
                            sid: data.id,
                            type: 3,
                            fid: $over_del_list,
                        }).done(function (result) {
                            if (result.success) deloverimg_success = 1
                            else deloverimg_success = -1
                        });
                    } else deloverimg_success = 1

                    co.WebMesnus.createOrEdit(data).done(function (result) {
                        const imgdel_timmer = function () {
                            if (delimg_success != 0) {
                                if (delimg_success == 1) {
                                    var $file = $("#ImageUpload .img_input_frame > .img_input");
                                    if (typeof ($file.data("file")) != "undefined" && $file.data("file") != null && $file.data("file").File != null) {
                                        var formData = new FormData();
                                        formData.append("files", $file.data("file").File);
                                        formData.append("type", 2);
                                        formData.append("sid", data.id);
                                        formData.append("serno", 500);
                                        co.File.Upload(formData).done(function (result) {
                                            if (result.success) img_success = 1;
                                            else img_success = -1;
                                        });
                                    } else img_success = 1;
                                } else img_success = -1;
                            } else setTimeout(imgdel_timmer, 100);
                        }
                        setTimeout(imgdel_timmer, 100);

                        const overdel_timmer = function () {
                            if (delimg_success != 0) {
                                if (delimg_success == 1) {
                                    var $over_file = $("#OverImageUpload .img_input_frame > .img_input");
                                    if (typeof ($over_file.data("file")) != "undefined" && $over_file.data("file") != null && $over_file.data("file").File != null) {
                                        var formData = new FormData();
                                        formData.append("files", $over_file.data("file").File);
                                        formData.append("type", 3);
                                        formData.append("sid", data.id);
                                        formData.append("serno", 500);
                                        co.File.Upload(formData).done(function (result) {
                                            if (result.success) overimg_success = 1;
                                            else overimg_success = -1;
                                        });
                                    } else overimg_success = 1;
                                } else overimg_success = -1;
                            } else setTimeout(overdel_timmer, 100);
                        }
                        setTimeout(overdel_timmer, 100);

                        const timmer = function () {
                            if (img_success == 1 && overimg_success == 1) {
                                menuReload(menuEditor, myOffcanvas);
                                ImageUploadModalClear($("#ImageUpload"));
                                ImageUploadModalClear($("#OverImageUpload"));
                                if (!result.success) co.sweet.error(result.error);
                                else {
                                    if (img_success == -1 || overimg_success == -1) co.sweet.erro("圖片上傳失敗");
                                    else co.sweet.success("新增成功");
                                }
                            } else setTimeout(timmer, 100);
                        }
                        setTimeout(timmer, 100);
                    });
                    //editor.setComponents("<span>Hi<span>");
                    //editor.setStyle("");
                },
                save: function () {

                },
                drop: function (cEl) {
                    let saveList = [];
                    let ps = cEl.parents('li');
                    let root = ps.last();
                    let fa = ps.first();
                    let ul = cEl.parents('ul');
                    let fK_TopNodeId, fK_RootNodeId;
                    let isAdd = false;
                    if (fa.length == 0) {
                        fK_TopNodeId = null;
                    } else {
                        fK_TopNodeId = fa.data("id");
                    }
                    if (root.length == 0) {
                        fK_RootNodeId = null;
                    } else {
                        fK_RootNodeId = root.data("id");
                    }
                    if (cEl.data("fK_TopNodeId") != fK_TopNodeId || cEl.data("fK_RootNodeId") != fK_RootNodeId) {
                        cEl.data({
                            "fK_TopNodeId": fK_TopNodeId,
                            "fK_RootNodeId": fK_RootNodeId
                        });
                        isAdd = true;
                        saveList.push($(cEl).data());
                    }

                    ul.children("li").each(function (index, element) {
                        var s = $(element).data("serNO");
                        console.log(s, index + 1, element);
                        if (s != (index + 1)) {
                            /*console.log(element, s, (index + 1));*/
                            s = index + 1;
                            $(element).data("serNO", s);
                            if ($(element).data("id") != cEl.data("id")) saveList.push($(element).data());
                            else if (!isAdd) saveList.push($(element).data());
                        }
                    });
                    co.WebMesnus.updateLevelAndSerNo(saveList).done(function (result) {
                        if (!result.success) co.sweet.error(result.error);
                    });
                },
                page: function (data) {
                    $("#gjs").data("id", data.id);
                    $("#gjs").removeClass("d-none");
                    $("#gjs + .emptyList").addClass("d-none");
                    co.WebMesnus.getConten(data.id).done(function (result) {
                        if (result.success) {
                            var html = co.Data.HtmlDecode(result.conten.saveHtml);
                            editor.setComponents(html);
                            editor.setStyle(result.conten.saveCss);
                            myOffcanvas.hide();
                        } else {
                            co.sweet.error(result.error);
                        }
                    });
                }
            }
        });

    var openEditForm = function () {
        if ($('#frmEdit [name="id"]').val() == 0) $("#btnClear").addClass("d-none");
        $("#offcanvasSite").addClass("offcanvas-lg");
        $("#MenuEditorForm").removeClass("d-none");
    }
    var closeEdit = function () {
        $("#offcanvasSite").removeClass("offcanvas-lg");
        $("#MenuEditorForm").addClass("d-none");
    }
    $('#offcanvasSite').on('show.bs.offcanvas', function () {
        closeEdit();
    });
    $("#btnExtend").on("click", function () {
        ImageUploadModalClear($("#ImageUpload"));
        ImageUploadModalClear($("#OverImageUpload"));
        openEditForm();
        $('#frmEdit [name="id"]').val(0);
        $("#btnRefresh,#btnAdd").removeClass("d-none");
        $("#btnUpdate").addClass("d-none");
        $("#btnRefresh").trigger("click");
    });

    menuReload(menuEditor, myOffcanvas);
    /*$(".material-symbols-outlined").each(function () {
        console.log(`"${$(this).text().trim()}"`);
    });*/
    /*$($.iconset_fontawesome_6.icons).each(function () {
        console.log(`"${this.replace(/[-]{3}[\w]{2,4}$/g,"")}"`);
    });*/
}

function menuReload(menuEditor, myOffcanvas) {
    co.WebMesnus.getAll().done(function (result) {
        if (result.success) {
            console.log(result.maps)
            menuEditor.setData(result.maps);
            $("#myEditor").removeClass("d-none");
            if (result.maps.length > 0) $("#myEditor + .emptyList").addClass("d-none");
            else $("#myEditor").addClass("d-none");
            myOffcanvas.show();
        } else {
            menuEditor.setData([]);
        }
    });
}