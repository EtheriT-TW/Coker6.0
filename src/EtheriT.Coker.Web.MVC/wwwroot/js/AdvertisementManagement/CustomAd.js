var $btn_display, title, $title_text, $description, $description_text, $ad_type
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
    /*ImageUploadModalInit($("#ImageUpload"));*/
    $(".btn_input_pic").on("click", function (even) {
        even.preventDefault();
        $(".btn_input_pic").prev(".input_pic").click();
    });

    $(".input_pic").on("change", function (e) {
        $ad_type.data("fileid", 0);
        var file = e.target.files[0];
        var reader = new FileReader();
        reader.readAsDataURL(file);
        reader.onload = function (e) {
            var obj = {};
            obj["id"] = 0;
            obj["File"] = file;
            obj["name"] = file.name;
            obj["link"] = e.target.result
            $(".img_preview").attr("src", e.target.result);
            $(".btn_input_pic > span").addClass("d-none");
            $(".img_preview").removeClass("d-none");
            $ad_type.data("file", obj);
        };
    })

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
            $ad_type.data("fileid", 0);
            $ad_type.val(null);
            $ad_type.change();
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

    $(".btn_preview").off("click").on("click", function () {
        $(".ad_preview > div").each(function (i) {
            $(this).addClass("d-none");
        });
        $(".ad_preview > .youtube").removeClass("d-none");
        var ytlink = $(".btn_preview").prev().val();
        var file = ytlink.substr(ytlink.indexOf("v=") + 2);
        $ad_type.data("file", file);
        var videostring = "https://www.youtube.com/embed/" + file;
        $(".ad_preview > .youtube > iframe").attr("src", videostring);
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
    $ad_type.on("change", function () {
        $(".ad_preview > div").each(function (i) {
            $(this).addClass("d-none");
        });
        $ad_type.data("fileid", 0);
        $(".ad_link > input").val("");
        $(".ad_preview > .youtube > iframe").attr("src", "");
        $(".btn_input_pic > span").removeClass("d-none");
        $(".img_preview").addClass("d-none");
        $(".img_preview").attr("src", "");
        $ad_type.data("file", "");
        if ($ad_type.val() == null) {
            $(".ad_link").addClass("d-none");
            $(".ad_link > input").removeAttr("name");
            $(".ad_link > input").removeAttr("required");
            $(".ad_preview > .preview").removeClass("d-none");
        } else {
            switch (parseInt($ad_type.val())) {
                case 1:
                    $(".ad_preview > .image").removeClass("d-none");
                    $(".ad_link > input").attr("placeholder", "輸入連結網址");
                    $(".ad_link > input").attr("name", "link");
                    $(".ad_link > input").attr("required", "required");
                    $(".ad_link").removeClass("d-none");
                    $(".ad_link > .checkbox").removeClass("d-none");
                    $(".ad_link > button").addClass("d-none");
                    break;
                case 2:
                    $(".ad_link").addClass("d-none");
                    $(".ad_link > input").removeAttr("name");
                    $(".ad_link > input").removeAttr("required");
                    break;
                case 3:
                    $(".ad_link > input").attr("placeholder", "https://www.youtube.com/watch?v=");
                    $(".ad_link > input").removeAttr("name");
                    $(".ad_link > input").attr("required", "required");
                    $(".ad_link").removeClass("d-none");
                    $(".ad_link > .checkbox").addClass("d-none");
                    $(".ad_link > button").removeClass("d-none");
                    break;
            }
        }
    });

    if ("onhashchange" in window) {
        window.onhashchange = hashChange;
    } else {
        setInterval(hashChange, 1000);
    }
}

function ElementInit() {
    $ad_type = $("#AdType");
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
                else {
                    let t = null;
                    const f = function () {
                        clearTimeout(t);
                        if (directoryDatailList != null) {
                            MoveToItemList();
                        } else t = setTimeout(f, 100);
                    }
                    f();
                }
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
        console.log($ad_type.data("fileid"))
        if ($ad_type.data("fileid") == 0) {
            switch (parseInt($ad_type.val())) {
                case 1:
                    console.log($ad_type.data("file").File)
                    var formData = new FormData();
                    formData.append("files", $ad_type.data("file").File);
                    formData.append("type", 10);
                    formData.append("sid", result.message);
                    formData.append("serno", 500);
                    co.File.Upload(formData).done(function () {
                        success();
                    });
                    break;
                case 2:
                    console.log("上傳影片")
                    break;
                case 3:
                    var ytlink = $(".btn_preview").prev().val();
                    var file = ytlink.substr(ytlink.indexOf("v=") + 2);
                    $ad_type.data("file", file);
                    if (typeof (result.message) != "undefined") {
                        co.File.UploadYTLink({
                            Id: typeof ($ad_type.data("fileid")) == "undefined" ? 0 : $ad_type.data("fileid"),
                            SId: result.message,
                            File: $ad_type.data("file"),
                            Type: 10,
                            SerNo: 500,
                        }).done(function () {
                            success();
                        })
                    }
                    break;
            }
        } else {
            success();
        }
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
                $(AdvertiseForms).removeClass("was-validated");
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
                            result.startEndDate = 0;
                            result.sortCheckbox = 1;
                            result.ImageUpload = 1;
                            co.File.getAdFile(result.id).done(function (Fresult) {
                                $ad_type.val(Fresult.fileType);
                                $ad_type.trigger("change");
                                co.Form.insertData(result, "#AdvertiseForm");
                                $ad_type.data("fileid", Fresult.id);
                                switch (parseInt($ad_type.val())) {
                                    case 1:
                                        $(".img_preview").attr("src", Fresult.link);
                                        $(".img_preview").attr("alt", Fresult.name);
                                        $(".btn_input_pic > span").addClass("d-none");
                                        $(".img_preview").removeClass("d-none");
                                        break;
                                    case 2:
                                        break;
                                    case 3:
                                        $(".ad_link > input").val(Fresult.link);
                                        $(".btn_preview").trigger("click");
                                        break;
                                }
                                $AdvertiseTags.TagDataSet(result.tagDatas);
                            });
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