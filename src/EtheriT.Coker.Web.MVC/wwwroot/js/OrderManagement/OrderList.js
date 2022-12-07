var keyId
var order_list

function PageReady() {
    co.Order = {
        Add: function (data) {
            return $.ajax({
                url: "/api/Order/Add",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        //Get: function (id) {
        //    return $.ajax({
        //        url: "/api/Marquee/Get/",
        //        type: "GET",
        //        contentType: 'application/json; charset=utf-8',
        //        headers: _c.Data.Header,
        //        data: { id: id },
        //    });
        //},
        //Update: function (data) {
        //    return $.ajax({
        //        url: "/api/Marquee/Update",
        //        type: "POST",
        //        contentType: 'application/json; charset=utf-8',
        //        headers: _c.Data.Header,
        //        data: JSON.stringify(data),
        //        dataType: "json"
        //    });
        //},
        Delete: function (id) {
            return $.ajax({
                url: "/api/Order/Delete/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { id: id },
            });
        }
    };

    OrderDataCollapse();
    $(window).resize(OrderDataCollapse);

    ElementInit();

    $(".btn_back").on("click", function () {
        Coker.sweet.confirm("返回訂單列表", "資料將不被保存", "確定", "取消", function () {
            history.back();
        });
    })
    //$(".btn_add").on("click", function () {
    //    FormDataClear();
    //    window.location.hash = 0;
    //    HashDataEdit();
    //});

    if ("onhashchange" in window) {
        window.onhashchange = hashChange;
    } else {
        setInterval(hashChange, 1000);
    }
}

function ElementInit() {

}

function FormDataClear() {
    keyId = 0;
}

function contentReady(e) {
    order_list = e;
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
                //co.Marquees.Get(parseInt(hash)).done(function (result) {
                //    if (result != null) {
                //        MoveToContent();
                //        keyId = result.id;
                //        FormDataSet(result.placement, result.disp_opt, result.title, result.ser_no, result.link, result.target, result.permanent, result.startTime, result.endTime);
                //    } else {
                //        window.location.hash = ""
                //    }
                //})
            }
        }
    } else {
        BackToList();
    }
}

function editButtonClicked(e) {
    MoveToContent();

    var data = e.row.data;
    keyId = e.row.key;
    window.location.hash = keyId

    //FormDataSet(data.placement, data.disp_opt, data.title, data.ser_no, data.link, data.target, data.permanent, data.StartTime, data.EndTime)
}

function FormDataSet(disp, name, introduction, illustrate, marks, tag, price, number, min_number, permanent, startTime, endTime) {
    startDate = startTime;
    endDate = endTime;
    FormDataClear();
    $btn_display.children("span").text(disp ? "visibility" : "visibility_off");
    disp_opt = disp;

    $name.val(name);
    $name_count.text($name.val().length);
    $introduction.val(introduction);
    $introduction_count.text($introduction.val().length);
    $illustrate.val(illustrate);
    $illustrate_count.text($illustrate.val().length);

    $marks.val(marks);
    $tag.val(tag);
    $price.val(price);
    $number.val(number);
    $min_number.val(min_number);
    $date = $("#InputDate");
    if (permanent) {
        $date.val('');
        $date.attr("disabled", "disabled");
        $permanent.prop("checked", true);
    } else {
        startTime != null && $picker.data('daterangepicker').setStartDate(startTime);
        endTime != null && $picker.data('daterangepicker').setEndDate(endTime);
    }
}

function deleteButtonClicked(e) {
    Coker.sweet.confirm("刪除資料", "刪除後不可返回", "確定刪除", "取消", function () {
        co.Marquees.Delete(e.row.key);
        e.component.refresh();
    });
}

function Add(display, success_text, error_text) {
    co.Marquees.Add({
        WebsiteId: $.cookie('WebSiteId'),
        placement: $placement.val(),
        title: $title.val(),
        disp_opt: display,
        ser_no: $check_sort.is(":checked") ? $input_sort.val() : 500,
        link: $link.val(),
        target: $target.is(":checked"),
        StartTime: startDate,
        EndTime: endDate,
        permanent: $permanent.is(":checked")
    }).done(function () {
        Coker.sweet.success(success_text, null, true);
        setTimeout(function () {
            BackToList();
            product_list.component.refresh();
        }, 1000);
    }).fail(function () {
        Coker.sweet.error("錯誤", error_text, null, true);
    });
}

function Update(display, success_text, error_text) {
    co.Marquees.Update({
        id: keyId,
        WebsiteId: $.cookie('WebSiteId'),
        placement: $placement.val(),
        title: $title.val(),
        disp_opt: display,
        ser_no: $check_sort.is(":checked") ? $input_sort.val() : 500,
        link: $link.val(),
        target: $target.is(":checked"),
        StartTime: startDate,
        EndTime: endDate,
        permanent: $permanent.is(":checked")
    }).done(function () {
        Coker.sweet.success(success_text, null, true);
        setTimeout(function () {
            BackToList();
            product_list.component.refresh();
        }, 1000);
    }).fail(function () {
        Coker.sweet.error("錯誤", error_text, null, true);
    });
}

function MoveToContent() {
    $("#OrderList").addClass("d-none");
    $("#OrderContent").removeClass("d-none");
}

function BackToList() {
    $("#OrderList").removeClass("d-none");
    $("#OrderContent").addClass("d-none");
    window.location.hash = ""
}

function OrderDataCollapse() {
    $this_body = $("body > .wrapper > .content-area > .content-wrapper");
    $OrderDetails = $("#OrderDetails");
    $OrderData = $("#OrderData");

    if ($this_body.width() >= 1024) {
        $("#Btn_Side_Collapse").addClass("d-none");
        $OrderDetails.removeClass("col-12");
        $OrderData.addClass("col-3");
        $OrderData.removeClass("offcanvas offcanvas-end visible");
        $OrderData.css('visibility', '');
    } else {
        $("#Btn_Side_Collapse").removeClass("d-none");
        $OrderDetails.addClass("col-12");
        $OrderData.addClass("offcanvas offcanvas-end visible");
        $OrderData.removeClass("col-3");
    }
}