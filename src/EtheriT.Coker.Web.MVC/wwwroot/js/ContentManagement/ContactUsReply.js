let ContactList, keyId;
function contentReady(e) {
    ContactList = e;
}
function onRowPrepared(e) {
    if (e.rowType === "data") {
        const $row = $(e.rowElement); 
        switch (e.data.Status) {
            case "未處理":
                $row.addClass("status-pending");
                break;
            case "處理中":
                $row.addClass("status-processing");
                break;
            case "已回覆":
                $row.addClass("status-replied");
                break;
            case "已完成":
                $row.addClass("status-closed");
                break;
            case "作廢/忽略":
                $row.addClass("status-ignored");
                break;
        }
    }
}
function editButtonClicked(e) {
    keyId = e.row.key;
    window.location.hash = keyId;
}
function PageReady() {
    const forms = $('#ReplyForm');
    (() => {
        Array.from(forms).forEach(form => {
            form.addEventListener('submit', event => {
                if (!form.checkValidity()) {
                    event.preventDefault()
                    event.stopPropagation()
                } else {
                    event.preventDefault();
                    Reply();
                }
                form.classList.add('was-validated')
            }, false)
        })
    })()

    $(".btn_back").on("click", function () {
        location.hash = "";
        /*
        if ($("#Status").val() == "Processed") {
            history.back();
        } else {
            Coker.sweet.confirm("返回上一頁", "資料將不被保存", "確定", "取消", function () {
                // 存草稿
                history.back();
            });
        }*/
    })

    if ("onhashchange" in window) {
        window.onhashchange = hashChange;
    } else {
        setInterval(hashChange, 1000);
    }
    $(window).trigger('hashchange');
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
            if (parseInt(hash) != 0) {
                co.Contact.GetDataOne(parseInt(hash)).done(function (result) {
                    if (result != null) {
                        keyId = parseInt(hash);
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

function BackToList() {
    $(".page").removeClass("show");
    $("#ArticleList").addClass("show");
}

function FormDataSet(result) {
    co.Form.insertData(result.object, "#ReplyForm");
    $(".page").removeClass("show");
    $("#Form").addClass("show");
    $("#Status").find(`option[value="${result.object.status}"]`).prop("selected", true);
    if (result.object.status == 3 || result.object.status == 9) {
        $("#Form .btn_done,#Status,#InputReply").prop("disabled", true);
    } else {
        $("#Form .btn_done,#Status,#InputReply").prop("disabled", false);
    }
}

function Reply() {
    Coker.sweet.confirm("直接回覆", "回覆後不可取消", "確定", "取消", function () {
        const data = co.Form.getJson("ReplyForm");
        co.Contact.Replay(data).done(function (result) {
            if (result.success) {
                Coker.sweet.success("已成功回覆", null, true);
                location.hash = "";
                ContactList.component.refresh();
            } else {
                Coker.sweet.error(result.message);
            }
        }).fail(function (xhr, status, error) {
            Coker.sweet.error(error);
        });
    });
}