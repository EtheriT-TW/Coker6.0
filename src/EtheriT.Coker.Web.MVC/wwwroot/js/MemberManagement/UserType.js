let keyId,article_list,$from;

function PageReady() {
    ElementInit();
    TagListModalInit();
    const forms = $('#PageForm');
    $Tags = $(forms).find(".InputTag").TagListModalInit();

    
    (() => {
        Array.from(forms).forEach(form => {
            form.addEventListener('submit', event => {
                if (!form.checkValidity()) {
                    event.preventDefault()
                    event.stopPropagation()
                } else {
                    event.preventDefault();
                    Coker.sweet.confirm("即將儲存", "儲存後將顯示於安排的位置", "儲存", "取消", function () {
                        AddUp("已成功儲存", "儲存發生未知錯誤");
                    });
                }
                form.classList.add('was-validated')
                WasValidated();
            }, false)
        })
    })()

    $(".btn_back").on("click", function () {
        Coker.sweet.confirm("返回文章列表", "資料將不被保存", "確定", "取消", function () {
            article_list.component.refresh();
            BackToList();
        });
    })

    $(".btn_add").on("click", function () {
        FormDataClear();
        window.location.hash = 0;
        HashDataEdit();
    });

    if ("onhashchange" in window) {
        window.onhashchange = hashChange;
    } else {
        setInterval(hashChange, 1000);
    }
}

function ElementInit() {
    $from = $("#PageForm");
}

function contentReady(e) {
    article_list = e;
    HashDataEdit();
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
                co.UserHabits.GetUserGroupOne(parseInt(hash)).done(function (result) {
                    if (result != null) {
                        keyId = parseInt(hash);
                        if (hash.indexOf("-") > 0) {
                            MoveToCanvas();
                        } else {
                            MoveToContent();
                            FormDataSet(result);
                        }
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

function editButtonClicked(e) {
    MoveToContent();
    keyId = e.row.key;
    window.location.hash = keyId;
}

function FormDataClear() {
    $Tags.TagDataClear();
    keyId = 0;
    co.Form.clear("PageForm");
}

function FormDataSet(result) {
    FormDataClear();
    keyId = result.id;
    co.Form.insertData(result.object, "PageForm");
    $Tags.TagDataSet(result.object.tags);
}

function paletteButtonClicked(e) {
    keyId = e.row.key;
    window.location.hash = keyId + "-1";
}

function deleteButtonClicked(e) {
    Coker.sweet.confirm("刪除資料", "刪除後不可返回", "確定刪除", "取消", function () {
        co.UserHabits.DeleteUserGroup(e.row.key).done(function (result) {
            if (result.success) {
                e.component.refresh();
            }
        });
    });
}

function AddUp(success_text, error_text, place) {
    let data = co.Form.getJson("PageForm");
    data.tags = $Tags.data("tagList");
    co.UserHabits.AddUpUserGroup(data).done(function (result) {
        if (result.success) {
            Coker.sweet.success(success_text, null, true);
            setTimeout(function () {
                article_list.component.refresh();
                BackToList();
            }, 1000);
        } else {
            Coker.sweet.error("錯誤", error_text, null, true);
        }
    }).fail(function () {
        Coker.sweet.error("錯誤", error_text, null, true);
    });
}

function MoveToContent() {
    UnValidated();
    $("#PageList").addClass("d-none");
    $("#PageContent").removeClass("d-none");
    $("body").removeClass("grapesEdit");
}

function BackToList() {
    $("#TopLine > div > a").addClass("d-none");
    $("#PageList").removeClass("d-none");
    $("#PageContent").addClass("d-none");
    $("body").removeClass("grapesEdit");
    $("#TopLine .title").text("文章管理");
    window.location.hash = ""
}

function WasValidated() {
}

function UnValidated() {
    $("#PageForm").removeClass("was-validated");
}