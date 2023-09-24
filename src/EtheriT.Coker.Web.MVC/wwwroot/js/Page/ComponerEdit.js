function PageReady() {
    const myOffcanvas = new bootstrap.Offcanvas('#offcanvasSite');
    var editor = grapesInit({
        save: null,
        import: function (html, css) {
            var _dfr = $.Deferred();
            co.ObjectType.SaveConten({
                Id: $("#gjs").data("id"),
                Html: html,
                Css: css
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
            textConfirmDelete: "是否確認將<span class='ConfirmKeyWord'>{0}</span>分類刪除?",
            listOptions: {
                placeholderCss: { 'background-color': "#cccccc" }
            },
            iconPicker: {
                searchText: "Buscar...", labelHeader: "{0}/{1}"
            },
            maxLevel: 1, // (Optional) Default is -1 (no level limit)
            levelChang:false,
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
                },
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
                    co.ObjectType.delete(data.id).done(function (result) {
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
                            co.sweet.success("新增成功");
                        }
                    });
                },
                update: function (data) {
                    co.ObjectType.createOrEdit(data).done(function (result) {
                        if (!result.success) co.sweet.error(result.error);
                        else co.sweet.success("更新成功");
                    });
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
                    console.log(cEl);
                    ul.children("li").each(function (index, element) {
                        var s = $(element).data("serNO");
                        if (s != (index + 1)) {
                            s = index + 1;
                            $(element).data("serNO", s);
                            if ($(element).data("id") != cEl.data("id")) saveList.push($(element).data());
                            else if (!isAdd) saveList.push($(element).data());
                        }
                    });
                    co.ObjectType.updateSerNo(saveList).done(function (result) {
                        if (!result.success) co.sweet.error(result.error);
                    });
                },
                page: function (data) {
                    $("#gjs").data("id", data.id);
                    $("#gjs").removeClass("d-none");
                    $("#gjs + .emptyList").addClass("d-none");
                    co.ObjectType.getConten(data.id).done(function (result) {
                        if (result.success) {
                            var html = co.Data.HtmlDecode(result.conten.html);
                            editor.setStyle(result.conten.css);
                            editor.setComponents(html);
                            $("body").addClass("grapesEdit");
                            $("#TopLine .title").text(data.text);
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
        openEditForm();
        $('#frmEdit [name="id"]').val(0);
        $("#btnRefresh,#btnAdd").removeClass("d-none");
        $("#btnUpdate").addClass("d-none");
        $("#btnRefresh").trigger("click");
    });

    co.ObjectType.GetAll().done(function (result) {
        if (result.success) {
            menuEditor.setData(result.list);
            $("#myEditor").removeClass("d-none");
            if (result.list.length > 0) $("#myEditor + .emptyList").addClass("d-none");
            else $("#myEditor").addClass("d-none");
            myOffcanvas.show();
        } else {
            menuEditor.setData([]);
        }
    });
}