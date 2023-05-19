var $btn_display, $bind_type, $tag_input
var keyId, disp_opt = true
var directory_list;

function PageReady() {
    console.log("Directory");

    co.Directory = {
        AddUp: function (data) {
            return $.ajax({
                /*url: "/api/Marquee/AddUp",*/
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        Get: function (id) {
            return $.ajax({
                /*url: "/api/Marquee/Get/",*/
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { id: id },
            });
        },
        Delete: function (id) {
            return $.ajax({
                /*url: "/api/Marquee/Delete/",*/
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { id: id },
            });
        }
    };


    ElementInit();

    $(".btn_back").on("click", function () {
        Coker.sweet.confirm("返回目錄列表", "資料將不被保存", "確定", "取消", function () {
            directory_list.component.refresh();
            BackToList();
        });
    })

    $(".btn_add").on("click", function () {
        FormDataClear();
        window.location.hash = 0;
        HashDataEdit();
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

    if ("onhashchange" in window) {
        window.onhashchange = hashChange;
    } else {
        setInterval(hashChange, 1000);
    }
}

function ElementInit() {
    $btn_display = $(".btn_display");
    $tag_input = $(".tag > input");
    $bind_type = $("#BindType");
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
                MoveToContent();
                FormDataSet({ id: hash });
                //co.Directorys.GetSimple(parseInt(hash)).done(function (result) {
                //    if (result != null) {
                //        MoveToContent();
                //        FormDataSet(result);
                //    } else {
                //        window.location.hash = ""
                //        keyId = "";
                //    }
                //})
            }
        }
    } else {
        BackToList();
    }
}

function contentReady(e) {
    directory_list = e;
    HashDataEdit();
}

function editButtonClicked(e) {
    MoveToContent();
    keyId = e.row.key;
    window.location.hash = keyId;
}

function deleteButtonClicked(e) {
    Coker.sweet.confirm("刪除資料", "刪除後不可返回", "確定刪除", "取消", function () {
        //co.Directorys.Delete(e.row.key).done(function (result) {
        //    if (result.success) {
        //        e.component.refresh();
        //    }
        //});
        e.component.refresh();
    });
}

function FormDataClear() {
    keyId = 0;
    $btn_display.children("span").text("visibility");
    $bind_type[0].selectedIndex = 0;
    $tag_input.val("");
}

function FormDataSet(result) {
    FormDataClear();
    keyId = result.id;
    disp_opt = false;
    if (disp_opt) {
        $btn_display.children("span").text("visibility");
    } else {
        $btn_display.children("span").text("visibility_off");
    }
    $bind_type.val("");
    $tag_input.val("")

}

function AddUp(success_text, error_text) {
    co.Directorys.AddUp({

    }).done(function () {
        Coker.sweet.success(success_text, null, true);
        directory_list.component.refresh();
        BackToList();
    }).fail(function () {
        Coker.sweet.error("錯誤", error_text, null, true);
    });
}

function MoveToContent() {
    UnValidated();
    if (keyId == 0) {
        $(".btn_to_canvas").addClass("text-dark");
        $(".btn_to_canvas").attr('disabled', '');
    } else {
        $(".btn_to_canvas").removeClass("text-dark");
        $(".btn_to_canvas").removeAttr('disabled');
    }
    $("#DirectoryList").addClass("d-none");
    $("#DirectoryContent").removeClass("d-none");
    $("#DirectoryCanvas").addClass("d-none");
}

function BackToList() {
    $("#TopLine > a").addClass("d-none");
    $("#DirectoryList").removeClass("d-none");
    $("#DirectoryContent").addClass("d-none");
    $("#DirectoryCanvas").addClass("d-none");
    window.location.hash = ""
}

function UnValidated() {
    $("#DirectoryForm").removeClass("was-validated");
}