function PageReady() {
    var menuEditor;
    const myOffcanvas = new bootstrap.Offcanvas('#offcanvasSite');
    const menuForm = new CokerMenuEditorForm({
        getPageTypes: function () {
            return co.WebMesnus.GetPageTypeList();
        }
    });
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
                else co.sweet.error(result.error);
            });
            return _dfr.promise();
        }
    });
    let editorStting = {
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
            moveEnable: "#moveEnable"
        },
        on: {
            ready: function () {
                menuForm.initialize().fail(function (error) {
                    co.sweet.error(
                        "載入失敗",
                        typeof error === "string" ? error : "無法取得頁面類型"
                    );
                });
            },
            edit: function () {
                menuForm.prepareEdit();
            },
            loadEditData: function (summary) {
                return co.WebMesnus.getEditorDetail(summary.id).then(function (result) {
                    if (!result.success || !result.item) {
                        return $.Deferred()
                            .reject(result.error || "無法取得選單資料")
                            .promise();
                    }

                    return result.item;
                });
            },
            editLoadError: function (error) {
                const message = typeof error === "string"
                    ? error
                    : error?.responseJSON?.error || error?.statusText || "無法取得選單資料";
                co.sweet.error("載入失敗", message);
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
            validate: async function (data) {
                return await menuForm.validate(data);
            },
            add: async function (cEl) {
                var data = cEl.data();
                const check = await this.validate(data);
                if (!check) return;

                $("#myEditor").removeClass("d-none");
                $("#myEditor + .emptyList").addClass("d-none");
                const $selected = $("#myEditor").find("li.selectItem").first();
                if ($selected.length > 0) {
                    const f_data = $selected.data() || {};
                    data.fK_TopNodeId = f_data.id;
                    data.level = (f_data.level || 0) + 1;
                    data.fK_RootNodeId = f_data.fK_RootNodeId || f_data.id;
                }
                co.WebMesnus.createOrEdit(data).done(function (result) {
                    if (!result.success) co.sweet.error(result.error);
                    else {
                        data.id = parseInt(result.message);
                        var ico_success = 0, img_success = 0, overimg_success = 0;

                        var $ico_file = menuForm.getUploadInput("icon");
                        if (typeof ($ico_file.data("file")) != "undefined" && $ico_file.data("file") != null) {
                            var formData = new FormData();
                            formData.append("files", $ico_file.data("file").File);
                            formData.append("type", 9);
                            formData.append("sid", data.id);
                            formData.append("serno", 500);
                            co.File.Upload(formData).done(function (result) {
                                if (result.success) ico_success = 1;
                                else ico_success = -1;
                            });
                        } else ico_success = 1;

                        var $file = menuForm.getUploadInput("image");
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

                        var $over_file = menuForm.getUploadInput("overImage");
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
                            if (ico_success != 0 && img_success != 0 && overimg_success != 0) {
                                menuForm.clearUploads();
                                menuReload(menuEditor, myOffcanvas, function () {
                                    const newId = data.id;
                                    var $target = $("#myEditor").find("li").filter(function () {
                                        var d = $(this).data();
                                        return d.id === newId || d.Id === newId;
                                    }).first();

                                    if ($target.length) {
                                        const $parents = $target.parents("li");
                                        if ($parents.length) {
                                            $parents.each(function () {
                                                const $opener = $(this).find(".sortableListsOpener").first();
                                                if ($(this).hasClass("sortableListsClosed") && $opener.length) {
                                                    $opener.trigger("mousedown");
                                                } 
                                            });
                                        }
                                        $("#myEditor").find("li.selectItem").removeClass("selectItem");
                                        $target.addClass("selectItem");
                                        $target.find(".btnEdit").first().trigger("click");
                                    }
                                });
                                if (!result.success) co.sweet.error(result.error);
                                else {
                                    if (ico_success == -1 || img_success == -1 || overimg_success == -1) co.sweet.erro("圖片上傳失敗");
                                    else co.sweet.success("新增成功");
                                }
                            } else setTimeout(timmer, 100);
                        };
                        setTimeout(timmer, 100);
                    }
                });
            },
            update: async function (data) {
                const check = await this.validate(data);
                if (!check) return;

                co.WebMesnus.createOrEdit(data).done(function (result) {
                    if (!result.success) co.sweet.error(result.error);
                    else {
                        var iconimg_success = 0, img_success = 0, overimg_success = 0, deliconimg_success = 0, delimg_success = 0, deloverimg_success = 0;

                        var $icon_del_list = menuForm.getDeleteList("icon");
                        if ($icon_del_list != null) {
                            co.File.DeleteFileById({
                                sid: data.id,
                                type: 9,
                                fid: $icon_del_list,
                            }).done(function (result) {
                                if (result.success) deliconimg_success = 1
                                else deliconimg_success = -1
                                data.IconUrl = "";
                                data.IconId = "";
                            });
                        } else deliconimg_success = 1

                        var $del_list = menuForm.getDeleteList("image");
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

                        var $over_del_list = menuForm.getDeleteList("overImage");
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
                        const del_timmer = function () {
                            if (deliconimg_success != 0 && delimg_success != 0 && deloverimg_success != 0) {
                                if (deliconimg_success == 1) {
                                    var $file = menuForm.getUploadInput("icon");
                                    if (typeof ($file.data("file")) != "undefined" && $file.data("file") != null && $file.data("file").File != null) {
                                        var formData = new FormData();
                                        formData.append("files", $file.data("file").File);
                                        formData.append("type", 9);
                                        formData.append("sid", data.id);
                                        formData.append("serno", 500);
                                        co.File.Upload(formData).done(function (result) {
                                            if (result.success) iconimg_success = 1;
                                            else iconimg_success = -1;
                                        });
                                    } else iconimg_success = 1;
                                } else iconimg_success = -1;

                                if (delimg_success == 1) {
                                    var $file = menuForm.getUploadInput("image");
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

                                if (deloverimg_success == 1) {
                                    var $over_file = menuForm.getUploadInput("overImage");
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

                                const timmer = function () {
                                    if (iconimg_success == 1 && img_success == 1 && overimg_success == 1) {
                                        menuReload(menuEditor, myOffcanvas);
                                        menuForm.clearUploads();
                                        if (!result.success) co.sweet.error(result.error);
                                        else {
                                            if (iconimg_success == -1 || img_success == -1 || overimg_success == -1) co.sweet.erro("圖片上傳失敗");
                                            else co.sweet.success("儲存成功");
                                        }
                                    } else setTimeout(timmer, 100);
                                }
                                setTimeout(timmer, 100);
                            } else setTimeout(del_timmer, 100);
                        }
                        setTimeout(del_timmer, 100);
                    }
                });
                
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
                    if (s != (index + 1)) {
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
                        co.Grapes.setEditor(editor, html, result.conten.saveCss);
                        co.Grapes.setFile(editor, data.id, 1);
                        $("body").addClass("grapesEdit");
                        $("#TopLine .title").text(data.text);
                        myOffcanvas.hide();
                    } else {
                        co.sweet.error(result.error);
                    }
                });
            },
            updateMenuEditorAddTitle: updateMenuEditorAddTitle
        }, btn: [
            // 1) visible：標題後面的 radio+label
            {
                key: 'visible',
                position: 'title',
                render: function (ctx) {
                    var id = ctx.data.id || ('new_' + ctx.$li.index());
                    var name = 'visible_' + id;
                    var $wrap = $('<span class="menu-visible-toggle ms-2">');
                    var $radio = $('<input type="checkbox">').addClass("selectedItem")
                        .attr({ 'id': name }, 'name', name);
                    var $label = $('<label>')
                        .attr({
                            'for': name,
                            'title': '切換前台顯示'
                        }).append(`<span class="material-symbols-outlined">visibility</span><span class="material-symbols-outlined">visibility_off</span>`);

                    $wrap.append($radio).append($label);
                    return $wrap;
                },
                init: function (ctx) {
                    var visible = ctx.data.visible === true
                        || ctx.data.visible === 1
                        || ctx.data.visible === 'true';

                    var $checkbox = ctx.$button.find('input.selectedItem');
                    $checkbox.prop('checked', visible);
                },
                click: function (ctx) {
                    var $checkbox = ctx.$button.find('input.selectedItem');
                    var newChecked = !$checkbox.prop('checked');
                    $checkbox.prop('checked', newChecked);

                    var newVisible = newChecked;           // 勾選 = 顯示
                    ctx.data.visible = newVisible;
                    ctx.$li.data(ctx.data);
                    co.WebMesnus.SetVisible(ctx.data.id, newVisible).done(function (result) {
                        if (!result.success) co.sweet.error("儲存失敗",result.error);
                    });
                    // 需要的話這裡打 API
                }
            },

            // 2) 後台權限設定：排序群後面、編輯前面
            {
                key: 'setPower',
                position: 'action',
                render: function (ctx) {
                    const hasPermission = ctx.data.hasBackstagePermission === true
                        || ctx.data.hasBackstagePermission === 1
                        || ctx.data.hasBackstagePermission === "true";

                    return $('<a class="btn btn-warning btn-sm permission-state-btn backstage-permission-btn">')
                        .toggleClass("has-permission", hasPermission)
                        .attr({
                            title: hasPermission ? "後台權限設定：已設定" : "後台權限設定：未設定",
                            "aria-label": hasPermission ? "後台權限設定，已設定" : "後台權限設定，未設定"
                        })
                        .append(`
                            <span class="permission-icon-wrap" aria-hidden="true">
                                <i class="fa-solid fa-user-shield permission-main-icon"></i>
                                <i class="fa-solid fa-circle-check permission-check-icon"></i>
                            </span>
                        `);
                },
                click: function (ctx) {
                    const data = ctx.data;
                    $("#PermissionDetailsModal").setData({ pageId: data.id, title: data.text, type: 0 }).modal("show");
                }
            },

            // 3) 前台瀏覽權限設定：跟 setPower 並排
            {
                key: 'setFrontPower',
                position: 'action',
                permission: hasRole,
                render: function (ctx) {
                    const hasPermission = ctx.data.hasFrontPermission === true
                        || ctx.data.hasFrontPermission === 1
                        || ctx.data.hasFrontPermission === "true";

                    return $('<a class="btn btn-info btn-sm permission-state-btn front-permission-btn">')
                        .toggleClass("has-permission", hasPermission)
                        .attr({
                            title: hasPermission ? "前台瀏覽權限：已設定" : "前台瀏覽權限：未設定",
                            "aria-label": hasPermission ? "前台瀏覽權限，已設定" : "前台瀏覽權限，未設定"
                        })
                        .append(`
                            <span class="permission-icon-wrap" aria-hidden="true">
                                <i class="fa-solid fa-user-group permission-main-icon"></i>
                                <i class="fa-solid fa-circle-check permission-check-icon"></i>
                            </span>
                        `);
                },
                click: function (ctx) {
                    const data = ctx.data;
                    $("#RolesDetailsModal").setRolesData({ pageId: data.id, title: data.text, type: 4 }).modal("show");
                }
            }
        ]
    };
    co.PowerManagement.GetPermission().done(function (permission) {
        if (!permission.superManager) delete editorStting.on.setPower;
        menuEditor = new MenuEditor('myEditor', editorStting);
        $("#PermissionDetailsModal, #RolesDetailsModal")
            .off("hidden.bs.modal.permissionStateReload")
            .on("hidden.bs.modal.permissionStateReload", function () {
                if (menuEditor) {
                    menuReload(menuEditor, myOffcanvas);
                }
        });
        $('#offcanvasSite').on('show.bs.offcanvas', function () {
            menuForm.close();
        });
        $('#offcanvasSite').on("click", ".btn-close", function (e) {
            e.preventDefault();
            if ($("#offcanvasSite.offcanvas-lg").length > 0) menuForm.close();
            else myOffcanvas.hide();
        });
        $("#btnExtend").on("click", function () {
            menuForm.prepareAdd();
            $("#myEditor .editItem").removeClass("editItem");
            updateMenuEditorAddTitle();
        });

        menuReload(menuEditor, myOffcanvas);
    });
    /*$(".material-symbols-outlined").each(function () {
        console.log(`"${$(this).text().trim()}"`);
    });*/
    /*$($.iconset_fontawesome_6.icons).each(function () {
        console.log(`"${this.replace(/[-]{3}[\w]{2,4}$/g,"")}"`);
    });*/
}

function updateMenuEditorAddTitle() {
    const $editItem = $("#myEditor").find("li.editItem").first();
    const $selected = $("#myEditor").find("li.selectItem").first();
    let titleText = "";
    if ($editItem.length !== 0) return;
    if ($selected.length === 0) {
        // 沒有選任何選單 → 新增主選單
        titleText = "新增主選單";
    } else {
        const data = $selected.data() || {};
        // 先用 data.text，沒有再退回去抓畫面文字
        const menuTitle =
            (data.text && data.text.toString().trim()) ||
            $selected.find(".txt").first().text().trim() ||
            "選單";

        titleText = `新增「${menuTitle}」的子選單`;
    }

    $("#MenuEditorForm>.card-header>.title").text(titleText);
}

function menuReload(menuEditor, myOffcanvas, afterReload) {
    co.WebMesnus.getAll().done(function (result) {
        if (result.success) {
            //console.log(result.maps)
            (menuEditor.setDataPreserve || menuEditor.setData).call(menuEditor, result.maps);
            //menuEditor.setData(result.maps);
            $("#myEditor").removeClass("d-none");
            if (result.maps.length > 0) $("#myEditor + .emptyList").addClass("d-none");
            else $("#myEditor").addClass("d-none");
            myOffcanvas.show();
            typeof afterReload === "function" && afterReload();
        } else {
            menuEditor.setData([]);
        }
    });
}
