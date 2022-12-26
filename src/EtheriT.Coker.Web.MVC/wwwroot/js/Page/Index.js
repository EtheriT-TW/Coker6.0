function PageReady() {
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
        saveComponer: function () {

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
                edit: function () {
                    openEditForm();
                    $("#btnUpdate").removeClass("d-none");
                    $("#btnRefresh,#btnAdd").addClass("d-none");
                },
                del: function (data) {
                    if ($("#myEditor>li").length == 0) {
                        $("#myEditor").addClass("d-none");
                        $("#myEditor + .emptyList").removeClass("d-none");
                    }
                    co.WebMesnus.delete(data.id).done(function (result) {
                        if (result.success) co.sweet.success("已成功刪除");
                        else co.sweet.error(result.error);
                        /*console.log(data);*/
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
                            co.sweet.success("新增成功");
                        }
                    });
                },
                update: function (data) {
                    co.WebMesnus.createOrEdit(data).done(function (result) {
                        if (!result.success) co.sweet.error(result.error);
                        else co.sweet.success("更新成功");
                    });
                    //editor.setComponents("<span>Hi<span>");
                    //editor.setStyle("");
                    /*console.log(data);*/
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
                            editor.setComponents(result.conten.saveHtml);
                            editor.setStyle(result.conten.saveCss);
                        } else {
                            co.sweet.error(result.error);
                        }
                    });
                }
            }
        });

    var openEditForm = function () {
        if ($('#frmEdit [name="id"]').val()==0) $("#btnClear").addClass("d-none");
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
        openEditForm();
        $('#frmEdit [name="id"]').val(0);
        $("#btnRefresh,#btnAdd").removeClass("d-none");
        $("#btnUpdate").addClass("d-none");
        $("#btnRefresh").trigger("click");
    });

    co.WebMesnus.getAll().done(function (result) {
        if (result.success) {
            menuEditor.setData(result.maps);
            $("#myEditor").removeClass("d-none");
            if (result.maps.length > 0) $("#myEditor + .emptyList").addClass("d-none");
            else $("#myEditor").addClass("d-none");
        } else {
            menuEditor.setData([]);
        }
    });
}