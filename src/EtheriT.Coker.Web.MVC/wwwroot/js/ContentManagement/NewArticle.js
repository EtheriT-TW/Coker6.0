var $placement, $btn_display, $title, $title_count, $input_sort, $check_sort, $link, $target, $date, $picker, $permanent
var startDate, endDate, keyId, disp_opt = true
var article_list

function PageReady() {
    //co.Articles = {
    //    AddUp: function (data) {
    //        return $.ajax({
    //            url: "/api/Article/AddUp",
    //            type: "POST",
    //            contentType: 'application/json; charset=utf-8',
    //            headers: _c.Data.Header,
    //            data: JSON.stringify(data),
    //            dataType: "json"
    //        });
    //    },
    //    Get: function (id) {
    //        return $.ajax({
    //            url: "/api/Article/Get/",
    //            type: "GET",
    //            contentType: 'application/json; charset=utf-8',
    //            headers: _c.Data.Header,
    //            data: { id: id },
    //        });
    //    },
    //    Delete: function (id) {
    //        return $.ajax({
    //            url: "/api/Article/Delete/",
    //            type: "GET",
    //            contentType: 'application/json; charset=utf-8',
    //            headers: _c.Data.Header,
    //            data: { id: id },
    //        });
    //    }
    //};

    ElementInit();

    //const forms = $('#PostForm');
    //(() => {
    //    Array.from(forms).forEach(form => {
    //        form.addEventListener('submit', event => {
    //            if (!form.checkValidity()) {
    //                event.preventDefault()
    //                event.stopPropagation()
    //            } else {
    //                event.preventDefault();
    //                Coker.sweet.confirm("即將發布", "發布後將直接顯示於安排的位置", "發布", "取消", function () {
    //                    AddUp(disp_opt, "已成功發布", "發布發生未知錯誤");
    //                });
    //            }
    //            form.classList.add('was-validated')
    //            WasValidated();
    //        }, false)
    //    })
    //})()


    $(".btn_back").on("click", function () {
        Coker.sweet.confirm("返回文章列表", "資料將不被保存", "確定", "取消", function () {
            history.back();
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
                /*AddUp("已成功發布", "發布發生未知錯誤", "Canvas");*/
            } else if (result.isDenied) {
                var hash = window.location.hash.replace("#", "") + "-1";
                window.location.hash = hash;
            }
        })
    });

    //$btn_display.on("click", function () {
    //    if (disp_opt) {
    //        $btn_display.children("span").text("visibility_off");
    //        disp_opt = !disp_opt;
    //    } else {
    //        $btn_display.children("span").text("visibility");
    //        disp_opt = !disp_opt;
    //    }
    //})
    //$title.on('keyup', function () {
    //    $title_count.text($title.val().length);
    //});
    //$check_sort.on("click", function () {
    //    if ($check_sort.is(":checked")) {
    //        $input_sort.removeAttr("disabled");
    //    } else {
    //        $input_sort.val('');
    //        $input_sort.attr("disabled", "disabled");
    //    }
    //})
    //$permanent.on("click", function () {
    //    if ($permanent.is(":checked")) {
    //        $date.val('');
    //        $date.attr("disabled", "disabled");
    //        startDate = null;
    //        endDate = null;
    //    } else {
    //        $date.removeAttr("disabled");
    //    }
    //})

    if ("onhashchange" in window) {
        window.onhashchange = hashChange;
    } else {
        setInterval(hashChange, 1000);
    }
}

function ElementInit() {
    //$placement = $("#Placement");
    //$btn_display = $("#Btn_Display");
    //$title = $("#InputTitle");
    //$title_count = $("#PostForm > .title .title_count");
    //$input_sort = $("#InputSort");
    //$check_sort = $("#SortCheck");
    //$link = $("#InputLink");
    //$target = $("#TargetCheck");
    //$date = $("#InputDate");
    //$permanent = $("#PermanentCheck");
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
                FormDataClear();
                MoveToContent();
            } else {
                co.Articles.Get(parseInt(hash)).done(function (result) {
                    if (result != null) {
                        MoveToContent();
                        FormDataSet(result);
                    } else {
                        window.location.hash = ""
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

function FormDataSet(result) {
    FormDataClear();
    keyId = result.id;
    //startTime = result.startTime;
    //endTime = result.endTime;
    //$placement.val(result.placement);
    //$btn_display.children("span").text(result.disp_opt ? "visibility" : "visibility_off");
    //disp_opt = result.disp_opt;
    //$title.val(result.title);
    //$title_count.text($title.val().length);
    //if (result.ser_no != 500) {
    //    $check_sort.prop("checked", true);
    //    $input_sort.removeAttr("disabled", "disabled");
    //    $input_sort.val(result.ser_no)
    //}
    //$link.val(result.link);
    //$target.prop("checked", result.target);
    //if (result.permanent) {
    //    $date.val('');
    //    $date.attr("disabled", "disabled");
    //    $permanent.prop("checked", true);
    //} else {
    //    startTime != null && $picker.data('daterangepicker').setStartDate(startTime);
    //    endTime != null && $picker.data('daterangepicker').setEndDate(endTime);
    //}
}

function FormDataClear() {
    keyId = 0;
    //$placement.val("Top");
    //$btn_display.children("span").text("visibility");
    //disp_opt = true;
    //$title.val("");
    //$title_count.text(0);
    //$input_sort.val("")
    //$input_sort.attr("disabled", "disabled");
    //$check_sort.prop("checked", false);
    //$link.val("https://");
    //$target.prop("checked", false);
    //$permanent.prop("checked", false);
    //$date.val("");
    //$date.removeAttr("disabled");
}

function paletteButtonClicked(e) {
    keyId = e.row.key + "-1";
    window.location.hash = keyId;
}

function deleteButtonClicked(e) {
    Coker.sweet.confirm("刪除資料", "刪除後不可返回", "確定刪除", "取消", function () {
        /*co.Article.Delete(e.row.key);*/
        e.component.refresh();
    });
}

function AddUp(display, success_text, error_text) {
    //co.Articles.AddUp({
    //    Id: keyId,
    //    placement: $placement.val(),
    //    title: $title.val(),
    //    disp_opt: display,
    //    ser_no: $check_sort.is(":checked") ? $input_sort.val() : 500,
    //    link: $link.val(),
    //    target: $target.is(":checked"),
    //    StartTime: startDate,
    //    EndTime: endDate,
    //    permanent: $permanent.is(":checked")
    //}).done(function () {
    //    Coker.sweet.success(success_text, null, true);
    //    setTimeout(function () {
    //        BackToList();
    //        article_list.component.refresh();
    //    }, 1000);
    //}).fail(function () {
    //    Coker.sweet.error("錯誤", error_text, null, true);
    //});
}

function MoveToContent() {
    UnValidated();
    $("#ArticleList").addClass("d-none");
    $("#ArticleContent").removeClass("d-none");
}

function BackToList() {
    $("#ArticleList").removeClass("d-none");
    $("#ArticleContent").addClass("d-none");
    window.location.hash = ""
}

function WasValidated() {
    //$check_sort.parents(".checkbox").first().addClass("pe-4");
    //$target.parents(".checkbox").first().addClass("pe-4");
    //$permanent.parents(".checkbox").first().addClass("pe-4");
}

function UnValidated() {
    //$("#PostForm").removeClass("was-validated");
    //$check_sort.parents(".checkbox").first().removeClass("pe-4");
    //$target.parents(".checkbox").first().removeClass("pe-4");
    //$permanent.parents(".checkbox").first().removeClass("pe-4");
}