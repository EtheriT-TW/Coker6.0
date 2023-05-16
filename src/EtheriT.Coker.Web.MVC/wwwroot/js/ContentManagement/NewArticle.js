var $btn_display, $btn_pop_visible, $title, $title_text, $describe, $describe_text, $sort, $sort_input, $sort_checkbox
var keyId, disp_opt = true, pop_visible = true;
var article_list

function PageReady() {
    co.Articles = {
        AddUp: function (data) {
            return $.ajax({
                url: "/api/Article/AddUp_Simple",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        GetSimple: function (Id) {
            return $.ajax({
                url: "/api/Article/GetSimple/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { Id: Id },
            });
        },
        Delete: function (Id) {
            return $.ajax({
                url: "/api/Article/Delete/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { Id: Id },
            });
        }
    };

    ElementInit();

    const forms = $('#ArticletForm');
    (() => {
        Array.from(forms).forEach(form => {
            form.addEventListener('submit', event => {
                if (!form.checkValidity()) {
                    event.preventDefault()
                    event.stopPropagation()
                } else {
                    event.preventDefault();
                    Coker.sweet.confirm("即將發布", "發布後將直接顯示於安排的位置", "發布", "取消", function () {
                        AddUp("已成功發布", "發布發生未知錯誤");
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

    $(".btn_to_canvas").on("click", function (event) {
        event.preventDefault()

        Swal.fire({
            icon: 'info',
            title: "前往文章編輯頁",
            html: "是否保存資料?",
            showCancelButton: true,
            showDenyButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#888888',
            denyButtonColor: '#d33',
            confirmButtonText: "　是　",
            denyButtonText: "　否　",
            cancelButtonText: "　取消　",
            reverseButtons: true
        }).then((result) => {
            if (result.isConfirmed) {
                AddUp("已成功發布", "發布發生未知錯誤", "canvas");
            } else if (result.isDenied) {
                var hash = window.location.hash.replace("#", "") + "-1";
                window.location.hash = hash;
                MoveToCanvas();
            }
        })
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

    $btn_pop_visible.on("click", function () {
        if (pop_visible) {
            $btn_pop_visible.children("span").text("group_off");
            pop_visible = !pop_visible;
        } else {
            $btn_pop_visible.children("span").text("group");
            pop_visible = !pop_visible;
        }
    })

    $title_text.on('keyup', function () {
        var $self = $(this);
        $title.children("div").children(".count").text($self.val().length)
    });

    $describe_text.on('keyup', function () {
        var $self = $(this);
        $describe.children("div").children(".count").text($self.val().length)
    });

    $sort_checkbox.on("click", function () {
        var $self = $(this);
        if ($self.is(":checked")) {
            $sort_input.removeAttr("disabled");
        } else {
            $sort_input.val('');
            $sort_input.attr("disabled", "disabled");
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
    $btn_pop_visible = $(".btn_pop_visible");
    $title = $(".title");
    $title_text = $title.children("textarea");
    $describe = $(".describe");
    $describe_text = $describe.children("textarea");
    $sort = $(".sort");
    $sort_input = $sort.children("input");
    $sort_checkbox = $sort.children(".checkbox").children("input");
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
            if (hash.indexOf("-") == 1) {
                MoveToCanvas();
            } else {
                if (parseInt(hash) == 0) {
                    FormDataClear();
                    MoveToContent();
                } else {
                    co.Articles.GetSimple(parseInt(hash)).done(function (result) {
                        if (result != null) {
                            MoveToContent();
                            FormDataSet(result);
                        } else {
                            window.location.hash = ""
                        }
                    })
                }
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

function FormDataSet(result) {
    FormDataClear();
    keyId = result.id;

    if (!result.visible) {
        $btn_display.children("span").text("visibility_off");
    }

    if (!result.popularVisible) {
        $btn_pop_visible.children("span").text("group_off");
    }

    $title_text.val(result.title);
    $title.children("div").children(".count").text(result.title.length);
    $describe_text.val(result.description);
    $describe.children("div").children(".count").text(result.description.length);

    if (result.serNO != 500) {
        $sort_input.val(result.serNO);
        $sort_input.removeAttr("disabled");
        $sort_checkbox.prop("checked", true);
    }

}

function FormDataClear() {
    keyId = 0;
    $btn_display.children("span").text("visibility");
    $btn_pop_visible.children("span").text("group");
    $title_text.val("");
    $title.children("div").children(".count").text(0);
    $describe_text.val("");
    $describe.children("div").children(".count").text(0);
    $sort_input.val("");
    $sort_input.attr("disabled", "disabled");
    $sort_checkbox.prop("checked", false);
}

function paletteButtonClicked(e) {
    keyId = e.row.key + "-1";
    window.location.hash = keyId;
    MoveToCanvas();
}

function deleteButtonClicked(e) {
    Coker.sweet.confirm("刪除資料", "刪除後不可返回", "確定刪除", "取消", function () {
        co.Articles.Delete(e.row.key).done(function (result) {
            if (result.success) {
                e.component.refresh();
            }
        });
    });
}

function AddUp(success_text, error_text, place) {
    co.Articles.AddUp({
        Id: keyId,
        Title: $title_text.val(),
        Description: $describe_text.val(),
        Visible: disp_opt,
        SerNO: $sort_checkbox.is(":checked") ? $sort_input.val() : 500,
        PopularVisible: pop_visible,
    }).done(function () {
        Coker.sweet.success(success_text, null, true);
        if (place == "canvas") {
            setTimeout(function () {
                window.location.hash += "-1";
                MoveToCanvas();
            }, 1000);
        } else {
            setTimeout(function () {
                article_list.component.refresh();
                BackToList();
            }, 1000);
        }
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
    $("#ArticleList").addClass("d-none");
    $("#ArticleContent").removeClass("d-none");
    $("#ArticleCanvas").addClass("d-none");
}

function MoveToCanvas() {
    UnValidated();
    $("#ArticleList").addClass("d-none");
    $("#ArticleContent").addClass("d-none");
    $("#ArticleCanvas").removeClass("d-none");
}

function BackToList() {
    $("#ArticleList").removeClass("d-none");
    $("#ArticleContent").addClass("d-none");
    $("#ArticleCanvas").addClass("d-none");
    window.location.hash = ""
}

function WasValidated() {
    $sort.children(".checkbox").addClass("pe-4");
}

function UnValidated() {
    $("#ArticletForm").removeClass("was-validated");
    $sort.children(".checkbox").removeClass("pe-4");
}