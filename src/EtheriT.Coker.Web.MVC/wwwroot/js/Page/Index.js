function PageReady() {
    var editor = grapesInit();

    var menuEditor = new MenuEditor('myEditor',
        {
            textConfirmDelete:"是否確認將<span class='ConfirmKeyWord'>{0}</span>選單刪除?",
            listOptions: {
                placeholderCss: { 'background-color': "#cccccc" }
            },
            iconPicker: {
                searchText: "Buscar...", labelHeader: "{0}/{1}"
            },
            maxLevel: -1, // (Optional) Default is -1 (no level limit)
            element: {
                Form:"#frmEdit",
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
                del: function () {

                },
                add: function (data) {
                    $("#myEditor").removeClass("d-none");
                    $("#myEditor + .emptyList").addClass("d-none");
                    co.WebMesnus.createOrEdit(data).done(function(result){
                        if (!result.Success) co.sweet.error(result.Error);
                    });
                },
                update: function (data) {
                    editor.setComponents("<span>Hi<span>");
                    editor.setStyle("");
                    console.log(data);
                },
                save: function () {

                },
                drop: function () {
                    var str = menuEditor.getString();
                    console.log(str);
                },
                page: function (data) {
                    $("#gjs").data("id", data.id);
                    editor.setComponents("");
                    editor.setStyle("");
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

    var arrayjson = [
        { id:1,"href": "http://home.com", "icon": "fas fa-home", "text": "Home", "target": "_top", "title": "My Home" },
        { id: 2, "icon": "fas fa-chart-bar", "text": "Opcion2" }, { "icon": "fas fa-bell", "text": "Opcion3" },
        { id: 3, "icon": "fas fa-crop", "text": "Opcion4" },
        { id: 4, "icon": "fas fa-flask", "text": "Opcion5" },
        { id: 5, "icon": "fas fa-map-marker", "text": "Opcion6" },
        {
            id: 6, "icon": "fas fa-search", "text": "Opcion7", "children": [
                {
                    id: 7, "icon": "fas fa-plug", "text": "Opcion7-1", "children": [
                        { id: 8, "icon": "fas fa-filter", "text": "Opcion7-1-1" }
                    ]
                }
            ]
        }
    ];

    co.WebMesnus.getAll().done(function (result) {
        if (result.Success) {
            menuEditor.setData(result.Maps);
            $("#myEditor").removeClass("d-none");
        } else {
            menuEditor.setData({});
        }
    });
}