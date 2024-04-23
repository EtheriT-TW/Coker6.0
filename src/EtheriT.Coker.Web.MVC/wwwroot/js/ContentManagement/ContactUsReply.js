let ContactList, keyId;
function contentReady(e) {
    ContactList = e;
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
    console.log(result);
}

function Reply() {
    Coker.sweet.confirm("直接回覆", "回覆後不可取消", "確定", "取消", function () {
        Coker.sweet.success("已成功回覆", null, true);
        $("#Status").val("Processed");
        $("#InputReply").attr("disabled", "disabled");
    });
    $(".btn_done").attr("disabled", "disabled");
}