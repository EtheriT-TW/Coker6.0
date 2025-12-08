function PageReady() {
    var menuEditor;
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
                co.WebMesnus.GetPageTypeList().done(function (result) {
                    if (result.success) {
                        const $s = $("#pageType");
                        const PageTypes = result.type
                        $(PageTypes).each(function () {
                            $s.append(`<option value="${this.value}">${this.key}</option>`);
                        });
                        $s.on("change", function () {
                            const $self = $(this);
                            const $RouterNameBlock = $("#RouterNameBlock");
                            const $RouterNameInput = $RouterNameBlock.find("input");
                            $RouterNameBlock.addClass("d-none")
                            let o = { key: $self.find(":selected").text(), value: parseInt($self.val()) };
                            o = PageTypes[co.Array.Search(PageTypes, o)];
                            if ([1, 6].includes(o.value)) {
                                if (co.Array.Search(PageTypes, { enName: $RouterNameInput.val() }) > 0) $RouterNameInput.val("");
                                $RouterNameBlock.removeClass("d-none");
                            } else {
                                $RouterNameInput.val(o.enName);
                            }
                        })
                    }
                });
                $("#IconImageUpload").ImageUploadModalClear();
                $("#ImageUpload").ImageUploadModalClear();
                $("#OverImageUpload").ImageUploadModalClear();
            },
            edit: function () {
                openEditForm();
                $("#btnUpdate").removeClass("d-none");
                $("#btnRefresh,#btnAdd").addClass("d-none");
                $("#IconImageUpload").ImageUploadModalClear();
                ImageUploadModalDataInsert($("#IconImageUpload"), $("#IconImageUpload").siblings("#iconId").val(), $("#IconImageUpload").siblings("#iconUrl").val(), "");
                $("#ImageUpload").ImageUploadModalClear();
                ImageUploadModalDataInsert($("#ImageUpload"), $("#ImageUpload").siblings("#imgId").val(), $("#ImageUpload").siblings("#imgUrl").val(), $("#ImageUpload").siblings("#imgName").val())
                $("#OverImageUpload").ImageUploadModalClear();
                ImageUploadModalDataInsert($("#OverImageUpload"), $("#OverImageUpload").siblings("#overImgId").val(), $("#OverImageUpload").siblings("#overImgUrl").val(), $("#OverImageUpload").siblings("#overImgName").val())
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
                if (!data.linkUrl && !data.routerName) {
                    let msg = "【路徑名稱】與【連結】<span class='text-danger font-weight-bold'>必須</span>填寫其中之一";
                    co.sweet.error("資料錯誤", msg);
                    return false;
                }
                if (data.linkUrl && data.routerName) {
                    let msg = "您同時輸入【路徑名稱】與【連結】。" +
                        "<br/>儲存後此選單將無法顯示頁面內容，只會直接<span class='text-danger font-weight-bold'>跳轉</span>到指定的連結。<br/>" +
                        "是否確認要這樣設定？";

                    const ok = await co.sweet.confirmAsync(
                        "跳頁設定",
                        msg,
                        "仍要儲存",
                        "取消"
                    );

                    if (!ok) {
                        // 使用者按取消 → 不繼續儲存
                        return false;
                    }
                }

                return true;
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

                        var $ico_file = $("#IconImageUpload .img_input_frame > .img_input");
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
                            if (ico_success != 0 && img_success != 0 && overimg_success != 0) {
                                $("#IconImageUpload").ImageUploadModalClear();
                                $("#ImageUpload").ImageUploadModalClear();
                                $("#OverImageUpload").ImageUploadModalClear();
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

                        var $icon_del_list = $("#IconImageUpload .img_input_frame").data("delectList");
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
                        const del_timmer = function () {
                            if (deliconimg_success != 0 && delimg_success != 0 && deloverimg_success != 0) {
                                if (deliconimg_success == 1) {
                                    var $file = $("#IconImageUpload .img_input_frame > .img_input");
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

                                if (deloverimg_success == 1) {
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

                                const timmer = function () {
                                    if (iconimg_success == 1 && img_success == 1 && overimg_success == 1) {
                                        menuReload(menuEditor, myOffcanvas);
                                        $("#IconImageUpload").ImageUploadModalClear();
                                        $("#ImageUpload").ImageUploadModalClear();
                                        $("#OverImageUpload").ImageUploadModalClear();
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
                position: 'action', // ← 會插在 .btnEdit 前
                render: function (ctx) {
                    return $('<a class="btn btn-warning btn-sm" title="後台權限設定">')
                        .append('<i class="fa-solid fa-user-shield"></i>');
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
                    return $('<a class="btn btn-info btn-sm" title="前台瀏覽權限">')
                        .append('<i class="fa-solid fa-user-group"></i>');
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
        $('#offcanvasSite').on('show.bs.offcanvas', function () {
            closeEdit();
        });
        $('#offcanvasSite').on("click", ".btn-close", function (e) {
            e.preventDefault();
            if ($("#offcanvasSite.offcanvas-lg").length > 0) closeEdit();
            else myOffcanvas.hide();
        });
        $("#btnExtend").on("click", function () {
            $("#IconImageUpload").ImageUploadModalClear();
            $("#ImageUpload").ImageUploadModalClear();
            $("#OverImageUpload").ImageUploadModalClear();
            openEditForm();
            $('#frmEdit [name="id"]').val(0);
            $("#btnRefresh,#btnAdd").removeClass("d-none");
            $("#btnUpdate").addClass("d-none");
            $("#btnRefresh").trigger("click");
            $("#myEditor .editItem").removeClass("editItem");
            updateMenuEditorAddTitle();
            $("#MenuEditorForm>.card-header>a").addClass("d-none");
        });

        menuReload(menuEditor, myOffcanvas);
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