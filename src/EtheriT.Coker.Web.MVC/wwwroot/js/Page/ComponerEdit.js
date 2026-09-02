function PageReady() {
    const myOffcanvas = new bootstrap.Offcanvas('#offcanvasSite');
    const customObjectTypeId = 999;
    const componentImageFileType = 17;
    let purposeOptions = [];
    let selectedPurposeIdList = [];

    function findComponentItem(id) {
        const targetId = String(id);
        return $("#myEditor").find("li").filter(function () {
            const itemId = $(this).data("id") ?? $(this).data("Id");
            return String(itemId) === targetId;
        }).first();
    }

    function openComponentState(state) {
        const $target = findComponentItem(state.id);
        const buttonSelector = state.mode === "canvas" ? ".btnPage" : ".btnEdit";
        const $button = $target.find(buttonSelector).first();
        if (!$target.length || !$button.length) {
            pageState.clear();
            myOffcanvas.show();
            return;
        }

        $target.parents("li").each(function () {
            const $parent = $(this);
            const $opener = $parent.find(".sortableListsOpener").first();
            if ($parent.hasClass("sortableListsClosed") && $opener.length) {
                $opener.trigger("mousedown");
            }
        });

        $("#myEditor").find("li.selectItem").removeClass("selectItem");
        $target.addClass("selectItem");
        $button.trigger("click");
    }

    const pageState = Coker.HashPage.createEditorState({
        onRestore: openComponentState
    });

    function selectedPurposeIds() {
        return selectedPurposeIdList.slice();
    }

    function setPurposeOptions(items) {
        purposeOptions = items || [];
        const grid = $("#ComponentPurposeGrid").dxDataGrid("instance");
        if (grid) grid.option("dataSource", purposeOptions);
        renderPurposeSummary();
    }

    function setSelectedPurposeIds(ids) {
        const validIds = new Set(purposeOptions.map(function (purpose) { return Number(purpose.id); }));
        selectedPurposeIdList = (ids || [])
            .map(Number)
            .filter(function (id, index, all) {
                return validIds.has(id) && all.indexOf(id) === index;
            });
        renderPurposeSummary();
    }

    function renderPurposeSummary() {
        const names = purposeOptions
            .filter(function (purpose) { return selectedPurposeIdList.includes(Number(purpose.id)); })
            .map(function (purpose) { return purpose.name; });
        const $summary = $("#PurposeSummary").empty();
        if (!names.length) {
            $summary.text("尚未設定用途").addClass("is-empty");
            return;
        }
        $summary.removeClass("is-empty");
        names.forEach(function (name) {
            $("<span>").addClass("badge rounded-pill me-1 mb-1").text(name).appendTo($summary);
        });
    }

    function initPurposeGrid() {
        $("#ComponentPurposeGrid").dxDataGrid({
            dataSource: purposeOptions,
            keyExpr: "id",
            showBorders: true,
            rowAlternationEnabled: true,
            hoverStateEnabled: true,
            selection: {
                mode: "multiple",
                showCheckBoxesMode: "always",
                selectAllMode: "allPages"
            },
            searchPanel: {
                visible: true,
                width: 260,
                placeholder: "搜尋元件用途"
            },
            filterRow: {
                visible: true,
                applyFilter: "auto"
            },
            paging: { pageSize: 10 },
            pager: {
                visible: true,
                showPageSizeSelector: true,
                allowedPageSizes: [10, 20, 50],
                showInfo: true
            },
            columns: [
                { dataField: "name", caption: "用途名稱" }
            ],
            noDataText: "目前沒有可選擇的元件用途"
        });
    }

    function openPurposeModal() {
        const grid = $("#ComponentPurposeGrid").dxDataGrid("instance");
        if (!grid) return;
        grid.option("dataSource", purposeOptions);
        grid.selectRows(selectedPurposeIdList, false);
        bootstrap.Modal.getOrCreateInstance("#ComponentPurposeModal").show();
    }

    function updatePurposeVisibility() {
        const isCustom = Number($("#classType").val()) === customObjectTypeId;
        $("#PurposeBlock").toggleClass("d-none", isCustom);
        if (isCustom) setSelectedPurposeIds([]);
    }

    function clearComponentIcon() {
        $("#myEditor_icon").iconpicker("setIcon", "empty");
        $('#frmEdit [name="icon"]').val("");
    }

    function clearComponentImage() {
        const $image = $("#ComponentImageUpload .img_input_frame > .img_input").first();
        if ($image.length) ImageDelect($image);
    }

    function resetComponentOptions() {
        $("#ComponentImageUpload").ImageUploadModalClear();
        setSelectedPurposeIds([]);
        updateImageUploadVisibility();
        updatePurposeVisibility();
    }

    function loadComponentImageFromForm() {
        $("#ComponentImageUpload").ImageUploadModalClear();
        ImageUploadModalDataInsert(
            $("#ComponentImageUpload"),
            $("#componentImgId").val(),
            $("#componentImgUrl").val(),
            $("#componentImgName").val()
        );
        updateImageUploadVisibility();
    }

    function updateImageUploadVisibility() {
        const icon = ($('#frmEdit [name="icon"]').val() || "").trim();
        const hasIcon = !!icon && icon !== "empty";
        const hasImage = !!$("#ComponentImageUpload .img_input_frame > .img_input").first().data("file");
        $("#myEditor_icon").closest(".input-group-append").toggleClass("d-none", hasImage);
        $("#ComponentImageBlock").toggleClass("d-none", hasIcon);
    }

    function observeComponentImageState() {
        const frame = $("#ComponentImageUpload .img_input_frame").get(0);
        if (!frame || $(frame).data("componentImageObserver")) return;

        const observer = new MutationObserver(updateImageUploadVisibility);
        observer.observe(frame, {
            subtree: true,
            childList: true,
            attributes: true,
            attributeFilter: ["class", "src"]
        });
        $(frame).data("componentImageObserver", observer);
    }

    function prepareComponentData(data) {
        data.purposeIds = Number(data.fK_TopNodeId) === customObjectTypeId
            ? []
            : selectedPurposeIds();
        if (data.icon === "empty") data.icon = "";

        const imageData = $("#ComponentImageUpload .img_input_frame > .img_input").first().data("file");
        if (imageData) data.icon = "";
        return data;
    }

    async function saveComponentImage(componentId) {
        const $frame = $("#ComponentImageUpload .img_input_frame");
        const deleteIds = $frame.data("delectList") || [];
        if (deleteIds.length > 0) {
            const deleteResult = await co.File.DeleteFileById({
                sid: componentId,
                type: componentImageFileType,
                fid: deleteIds
            });
            if (!deleteResult.success) throw new Error(deleteResult.error || "圖片刪除失敗");
        }

        const imageData = $frame.children(".img_input").first().data("file");
        if (!imageData || !imageData.File) return;

        const formData = new FormData();
        formData.append("files", imageData.File);
        formData.append("type", componentImageFileType);
        formData.append("sid", componentId);
        formData.append("serno", 500);
        const uploadResult = await co.File.Upload(formData);
        if (!uploadResult.success) throw new Error(uploadResult.error || "圖片上傳失敗");
    }

    function renderComponentListImages($scope) {
        ($scope || $("#myEditor")).find("li").each(function () {
            const $item = $(this);
            const data = $item.data() || {};
            const $title = $item.children("div").first().find(".d-flex.align-items-center").first();
            const $icon = $title.children("i").first();
            $title.children(".component-menu-image").remove();
            $icon.toggleClass("d-none", !!data.imgUrl);
            if (!data.imgUrl) return;

            $("<img>")
                .addClass("component-menu-image me-1 rounded")
                .attr({ src: data.imgUrl, alt: data.title || "" })
                .css({ width: "28px", height: "28px", objectFit: "cover" })
                .insertBefore($icon);
        });
    }

    async function refreshComponentImageData(componentId, $item) {
        const images = await co.File.getImgFile({
            sid: componentId,
            type: componentImageFileType,
            size: 1
        });
        const image = (images || [])[0];
        const imageData = {
            imgId: image ? image.id : null,
            imgUrl: image ? image.link : "",
            imgName: image ? image.name : ""
        };
        $item.data(imageData);
        $("#componentImgId").val(imageData.imgId || "");
        $("#componentImgUrl").val(imageData.imgUrl);
        $("#componentImgName").val(imageData.imgName);
        renderComponentListImages($item);
    }

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
                moveEnable: "#moveEnable"
            },
            on: {
                ready: function () {
                    $("#ComponentImageUpload").ImageUploadModalClear();
                    observeComponentImageState();
                    initPurposeGrid();
                    $("#classType").off("change.componentPurpose").on("change.componentPurpose", updatePurposeVisibility);
                    $("#ComponentImageUpload").off("change.componentIcon", ".input_pic")
                        .on("change.componentIcon", ".input_pic", function () {
                            clearComponentIcon();
                            updateImageUploadVisibility();
                        });
                    $("#ComponentImageUpload").off("click.componentIcon", ".btn_img_delete")
                        .on("click.componentIcon", ".btn_img_delete", updateImageUploadVisibility);
                    $("#myEditor_icon").off("change.componentImage").on("change.componentImage", function () {
                        const icon = $('#frmEdit [name="icon"]').val();
                        if (icon && icon !== "empty") clearComponentImage();
                        updateImageUploadVisibility();
                    });
                    $("#btnSelectPurpose").off("click.componentPurpose").on("click.componentPurpose", openPurposeModal);
                    $("#btnConfirmPurpose").off("click.componentPurpose").on("click.componentPurpose", function () {
                        const grid = $("#ComponentPurposeGrid").dxDataGrid("instance");
                        setSelectedPurposeIds(grid ? grid.getSelectedRowKeys() : []);
                        bootstrap.Modal.getOrCreateInstance("#ComponentPurposeModal").hide();
                    });
                    $("#btnRefresh").off("click.componentOptions").on("click.componentOptions", resetComponentOptions);
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
                edit: function (data) {
                    openEditForm();
                    $("#btnUpdate").removeClass("d-none");
                    $("#btnRefresh,#btnAdd").addClass("d-none");
                    const itemData = $("#myEditor li.editItem").first().data() || {};
                    setSelectedPurposeIds(itemData.purposeIds || []);
                    loadComponentImageFromForm();
                    updatePurposeVisibility();
                    pageState.replace("edit", data.id);
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
                add: async function (cEl) {
                    var data = prepareComponentData(cEl.data());
                    $("#myEditor").removeClass("d-none");
                    $("#myEditor + .emptyList").addClass("d-none");
                    try {
                        const result = await co.ObjectType.createOrEdit(data);
                        if (!result.success) throw new Error(result.error || result.message || "新增失敗");
                        data.id = parseInt(result.message);
                        cEl.data(data);
                        await saveComponentImage(data.id);
                        await refreshComponentImageData(data.id, cEl);
                        resetComponentOptions();
                        co.sweet.success("新增成功");
                    } catch (error) {
                        co.sweet.error(error.message || "新增失敗");
                    }
                },
                update: async function (data) {
                    data = prepareComponentData(data);
                    try {
                        const result = await co.ObjectType.createOrEdit(data);
                        if (!result.success) throw new Error(result.error || result.message || "更新失敗");
                        await saveComponentImage(data.id);
                        const $item = $("#myEditor li.editItem").first().data(data);
                        await refreshComponentImageData(data.id, $item);
                        co.sweet.success("更新成功");
                    } catch (error) {
                        co.sweet.error(error.message || "更新失敗");
                    }
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
                            co.Grapes.setEditor(editor, html, result.conten.css);
                            $("body").addClass("grapesEdit");
                            $("#TopLine .title").text(data.text);
                            pageState.replace("canvas", data.id);
                            myOffcanvas.hide();
                        } else {
                            pageState.clear();
                            myOffcanvas.show();
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
    $('#offcanvasSite').on("click", ".btn-close", function (e) {
        e.preventDefault();
        if ($("#offcanvasSite.offcanvas-lg").length > 0) {
            closeEdit();
            pageState.clear();
        }
        else myOffcanvas.hide();
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
            setPurposeOptions(result.purposes);
            (result.list || []).forEach(function (rootItem) {
                // 父層是元件分類，沒有畫布與刪除功能；編輯能力仍由 canEdit 控制。
                rootItem.canView = false;
                rootItem.canDel = false;
            });
            $(result.list).each((i, e) => {
                const $s = $("#classType");
                $s.append(`<option value="${e.id}">${e.title}</option>`);
            });
            menuEditor.setData(result.list);
            renderComponentListImages();
            $("#myEditor").removeClass("d-none");
            if (result.list.length > 0) $("#myEditor + .emptyList").addClass("d-none");
            else $("#myEditor").addClass("d-none");
            const initialState = pageState.getState();
            if (!initialState || initialState.mode !== "canvas") myOffcanvas.show();
            updatePurposeVisibility();
            pageState.restore();
        } else {
            menuEditor.setData([]);
        }
    });
}
