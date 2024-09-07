var $btn_display, title, $title_text, $description, $description_text
var keyId, disp_opt = true, DirectoryId = 0, DirectoryType = "n";
let directory_list, editor, permissionDetailsModal;
let DirectoryForms, $DirectoryTags;
let AdvertiseForms, $AdvertiseTags;

function PageReady() {
    DirectoryForms = $('#DirectoryForm');
    AdvertiseForms = $('#AdvertiseForm');
    permissionDetailsModal = new bootstrap.Modal(document.getElementById("PermissionDetailsModal"));
    co.PowerManagement.GetPermission().done(function (permission) {
        if (!permission.CanCreate) $(".btn_add").remove();
    });

    ElementInit();

    $DirectoryTags = $(DirectoryForms).find(".InputTag").TagListModalInit();
    $AdvertiseTags = $(AdvertiseForms).find(".InputTag").TagListModalInit();


    (() => {
        Array.from(AdvertiseForms).forEach(form => {
            form.addEventListener('submit', event => {
                if (!form.checkValidity()) {
                    event.preventDefault()
                    event.stopPropagation()
                } else {
                    event.preventDefault();
                    Coker.sweet.confirm("即將儲存", "儲存後將顯示於廣告列表", "儲存", "取消", function () {
                        AddUpAdvertise("已成功儲存", "儲存發生未知錯誤");
                    });
                }
                form.classList.add('was-validated')
            }, false)
        })
    })();

    (() => {
        Array.from(DirectoryForms).forEach(form => {
            form.addEventListener('submit', event => {
                if (!form.checkValidity()) {
                    event.preventDefault()
                    event.stopPropagation()
                } else {
                    event.preventDefault();
                    if ($("#InputTag").val() == "" || $("#InputTag").val() == "無") {
                        Coker.sweet.error("錯誤", "標籤不可為空", null, false);
                    } else {
                        Coker.sweet.confirm("即將儲存", "儲存後將顯示於安排的位置", "儲存", "取消", function () {
                            AddUp("已成功儲存", "儲存發生未知錯誤");
                        });
                    }
                }
                form.classList.add('was-validated')
            }, false)
        })
    })();

    $("#DirectoryContent .btn_back").on("click", function () {
        Coker.sweet.confirm("返回目錄列表", "資料將不被保存", "確定", "取消", function () {
            directory_list.component.refresh();
            BackToList();
        });
    });
    $("#AdvertiseContent .btn_back").on("click", function () {
        const dir = $("#DirectoryItemps").data("dir");
        Coker.sweet.confirm(`返回${dir.title}廣告列表`, "資料將不被保存", "確定", "取消", function () {
            directoryDatailList.component.refresh();
            location.hash = `Advertise_${dir.id}`
        });
    })

    $("#DirectoryList .btn_add").on("click", function () {
        FormDataClear();
        window.location.hash = 0;
        HashDataEdit();
    });
    $("#DirectoryItemps .btn_add").on("click", function () {
        window.location.hash = `AdvertiseEditor_${DirectoryId}_0`;
    });
    $("#DirectoryItemps .btn_back").off("click").on("click", function () {
        BackToList();
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

    $title_text.on('keyup', function () {
        var $self = $(this);
        $title.children("div").children(".count").text($self.val().length)
    });

    $description_text.on('keyup', function () {
        var $self = $(this);
        $description.children("div").children(".count").text($self.val().length)
    });

    if ("onhashchange" in window) {
        window.onhashchange = hashChange;
    } else {
        setInterval(hashChange, 1000);
    }
}

function ElementInit() {
    $btn_display = $(".btn_display");
    $title = $(".title");
    $title_text = $title.children("textarea");
    $description = $(".description");
    $description_text = $description.children("textarea");
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
            if (!!hash && isNaN(hash)) {
                if (hash.indexOf("Editor") > -1) MoveToItemAdvertise();
                else MoveToItemList();
            } else if (parseInt(hash) == 0) {
                window.location.hash = 0;
                keyId = 0;
                FormDataClear();
                MoveToContent();
            } else {
                MoveToContent();
                co.Directory.Get(parseInt(hash)).done(function (result) {
                    if (result != null) {
                        MoveToContent();
                        FormDataSet(result);
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

function contentReady(e) {
    directory_list = e;
    HashDataEdit();
}
function DirectoryDatailListReady(e) {
    directoryDatailList = e;
}

function editButtonClicked(e) {
    MoveToContent();
    keyId = e.row.key;
    window.location.hash = keyId;
}

function reladataButtonClicked(e) {
    keyId = `Advertise_${e.row.key}`;
    window.location.hash = keyId;
}
function GetDirectoryId() {
    return DirectoryId;
}
function GetDirectoryType() {
    return DirectoryType;
}

function deleteButtonClicked(e) {
    Coker.sweet.confirm("刪除資料", "刪除後不可返回", "確定刪除", "取消", function () {
        co.Directory.Delete(e.row.key).done(function (result) {
            if (result.success) {
                e.component.refresh();
            }
        });
        e.component.refresh();
    });
}

function FormDataClear() {
    $DirectoryTags.TagDataClear();
    keyId = 0;
    $btn_display.children("span").text("visibility");
    $title_text.val("");
    $description_text.val("");
}

function FormDataSet(result) {
    FormDataClear();
    keyId = result.id;
    disp_opt = result.visible;
    if (disp_opt) {
        $btn_display.children("span").text("visibility");
    } else {
        $btn_display.children("span").text("visibility_off");
    }
    $DirectoryTags.TagDataSet(result.tagDatas);
    $title_text.val(result.title);
    $description_text.val(result.description);
}

function AddUp(success_text, error_text) {
    co.Directory.AddUp({
        Id: keyId,
        Title: $title_text.val(),
        Description: $description_text.val(),
        Type: 4,
        Visible: disp_opt,
        TagSelected: $DirectoryTags.data("tagList"),
    }).done(function () {
        Coker.sweet.success(success_text, null, true);
        directory_list.component.refresh();
        BackToList();
    }).fail(function () {
        Coker.sweet.error("錯誤", error_text, null, true);
    });
}
function AddUpAdvertise(success_text, error_text) {
    const data = co.Form.getJson($(AdvertiseForms).attr("id"));
    console.log(data)
    if ($("#ImageUpload .img_input_frame").data("delectList") != null) {
        co.File.DeleteFileById({
            Sid: data.id,
            Type: 6,
            Fid: $("#ImageUpload .img_input_frame").data("delectList")
        });
    }
    co.Advertise.AddUp(data).done((result) => {
        const success = function () {
            Coker.sweet.success(success_text, null, true);
            directoryDatailList.component.refresh();
            location.hash = `Advertise_${DirectoryId}`;
        }

        if ($("#ImageUpload .img_input").data("file") != null && $("#ImageUpload .img_input").data("file").File != null && $("#ImageUpload .img_input").data("file").id == 0) {
            var formData = new FormData();
            formData.append("files", $("#ImageUpload .img_input").data("file").File);
            formData.append("type", 6);
            formData.append("sid", result.message);
            formData.append("serno", 500);
            co.File.Upload(formData).done(function () {
                success();
            });
        } else success();
    });
}

function MoveToContent() {
    $(DirectoryForms).removeClass("was-validated");
    $("#pages>.card,#TopLine").addClass("d-none");
    $("#DirectoryContent").removeClass("d-none");
}

function BackToList() {
    $("#pages>.card,#TopLine").addClass("d-none");
    $("#DirectoryList").removeClass("d-none");
    DirectoryId = 0;
    DirectoryType = "n";
    window.location.hash = ""
}

function MoveToItemList() {
    const para = window.location.hash.replace("#", "").split("_");
    $("#pages>.card,#TopLine").addClass("d-none");
    $("#DirectoryItemps").removeClass("d-none");
    const items = $(`#DirectoryItemps>.${para[0].toLowerCase()}`).removeClass("d-none");
    if (items.length == 0) BackToList();
    else if (para.length > 1 && !isNaN(para[1])) {
        DirectoryId = parseInt(para[1]);
        DirectoryType = para[0];
        switch (DirectoryType) {
            case "Advertise":
                directoryDatailList.component.refresh();
                break
            default:
                BackToList();
                break
        }
    }
}
function MoveToItemAdvertise() {
    const para = window.location.hash.replace("#", "").split("_");
    $("#pages>.card,#TopLine").addClass("d-none");
    $AdvertiseTags.TagDataClear();
    if (para.length > 2 && !isNaN(para[1]) && !isNaN(para[2])) {
        const id = parseInt(para[2]);
        DirectoryId = parseInt(para[1]);
        switch (para[0]) {
            case "AdvertiseEditor":
                const _dfr = $.Deferred();
                co.Directory.Get(DirectoryId).done((result) => {
                    $("#DirectoryItemps").data("dir", result);
                    _dfr.resolve();
                });
                if (id > 0) {
                    co.Advertise.GetDataOne(id).done(function (result) {
                        if (result != null) {
                            console.log(result)
                            result.startEndDate = 0;
                            result.sortCheckbox = 1;
                            result.ImageUpload = 1;
                            co.Form.insertData(result, "#AdvertiseForm");
                            $AdvertiseTags.TagDataSet(result.tagDatas);
                        } else BackToList();
                    })
                } else {
                    co.Form.clear("AdvertiseForm");
                    $("#AdvertiseForm > input[name='id'").val("");
                    _dfr.promise().done(function () {
                        $AdvertiseTags.TagDataSet($("#DirectoryItemps").data("dir").tagDatas);
                    });
                }
                $("#AdvertiseContent").removeClass("d-none");
                break
            default:
                BackToList();
                break
        }

    }
}
function editAdvertiseButtonClicked(e) {
    window.location.hash = `AdvertiseEditor_${DirectoryId}_${e.row.key}`;
}
function groupAdvertiseButtonClicked(e) {
    $("#PermissionDetailsModal").setData({ pageId: e.row.key, title: e.row.data.Title, type: 3 }).modal("show");
}
function deleteAdvertiseButtonClicked(e) {
    Coker.sweet.confirm("刪除資料", "刪除後不可返回", "確定刪除", "取消", function () {
        co.Advertise.Delete(e.row.key).done(function (result) {
            console.log(result)
            if (result.success) {
                e.component.refresh();
            }
        });
    });
}