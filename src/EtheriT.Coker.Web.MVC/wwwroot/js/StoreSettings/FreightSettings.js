var $set_default, $name, $preserve, $shipping, $isFree, $freight, $low_con, $d_freight;
var keyId, disp_opt = true
var freight_list

function PageReady() {
    co.Order = {
        GetPreserveTypeEnum: function () {
            return $.ajax({
                url: "/api/Order/GetPreserveTypeEnum",
                type: "POST",
                headers: _c.Data.Header
            });
        },
        GetShippingTypeEnum: function () {
            return $.ajax({
                url: "/api/Order/GetShippingTypeEnum",
                type: "POST",
                headers: _c.Data.Header
            });
        },
        //    Add: function (data) {
        //        return $.ajax({
        //            url: "/api/Marquee/Add",
        //            type: "POST",
        //            contentType: 'application/json; charset=utf-8',
        //            headers: _c.Data.Header,
        //            data: JSON.stringify(data),
        //            dataType: "json"
        //        });
        //    },
        //    Get: function (id) {
        //        return $.ajax({
        //            url: "/api/Marquee/Get/",
        //            type: "GET",
        //            contentType: 'application/json; charset=utf-8',
        //            headers: _c.Data.Header,
        //            data: { id: id },
        //        });
        //    },
        //    Update: function (data) {
        //        return $.ajax({
        //            url: "/api/Marquee/Update",
        //            type: "POST",
        //            contentType: 'application/json; charset=utf-8',
        //            headers: _c.Data.Header,
        //            data: JSON.stringify(data),
        //            dataType: "json"
        //        });
        //    },
        //    Delete: function (id) {
        //        return $.ajax({
        //            url: "/api/Marquee/Delete/",
        //            type: "GET",
        //            contentType: 'application/json; charset=utf-8',
        //            headers: _c.Data.Header,
        //            data: { id: id },
        //        });
        //    }
    };

    ElementInit();

    const forms = $('#FreightForm');
    (() => {
        Array.from(forms).forEach(form => {
            form.addEventListener('submit', event => {
                if (!form.checkValidity()) {
                    event.preventDefault()
                    event.stopPropagation()
                } else {
                    event.preventDefault();
                    keyId > 0 ? Update() : Add();
                }
                form.classList.add('was-validated')
            }, false)
        })
    })()

    $(".btn_back").on("click", function () {
        Coker.sweet.confirm("返回商品列表", "資料將不被保存", "確定", "取消", function () {
            history.back();
        });
    })
    $(".btn_add").on("click", function () {
        FormDataClear();
        window.location.hash = 0;
        HashDataEdit();
    });

    $isFree.on("click", function () {
        if ($isFree.is(":checked")) {
            $freight.val("");
            $freight.attr("disabled", "disabled");
            $low_con.val("");
            $low_con.attr("disabled", "disabled");
            $d_freight.val("");
            $d_freight.attr("disabled", "disabled");
        } else {
            $freight.removeAttr("disabled");
            $low_con.removeAttr("disabled");
            $d_freight.removeAttr("disabled");
        }
    });

    $("input[type='number']").change(function () {
        $(this).val($(this).val() < 0 ? 0 : $(this).val())
    });

    co.Order.GetPreserveTypeEnum().done(function (result) {
        $(result).each(function () {
            var e = this;
            $preserve.append($("<option>").attr({ value: e.value }).text(e.key));
        });
    });

    co.Order.GetShippingTypeEnum().done(function (result) {
        $(result).each(function () {
            var e = this;
            $shipping.append($("<option>").attr({ value: e.value }).text(e.key));
        });
    });

    if ("onhashchange" in window) {
        window.onhashchange = hashChange;
    } else {
        setInterval(hashChange, 1000);
    }
}

function ElementInit() {
    $set_default = $("#CheckDefault");
    $name = $("#InputName");
    $preserve = $("#SelectPreserve");
    $shipping = $("#SelectShipping");
    $isFree = $("#CheckFree");
    $freight = $("#InputFreight");
    $low_con = $("#InputLowCon");
    $d_freight = $("#InputDfreight");
}

function FormDataClear() {
    keyId = 0;
    $set_default.val("");
    $name.val("");
    $preserve.val("");
    $shipping.val("");
    $isFree.val("");
    $freight.val("");
    $low_con.val("");
    $d_freight.val("");
}

function contentReady(e) {
    freight_list = e;
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

    //FormDataSet(set_default, name, preserve, shipping, isFree, freight, low_con, d_freight)
}

function FormDataSet(set_default, name, preserve, shipping, isFree, freight, low_con, d_freight) {
    $set_default.val("");
    $name.val("");
    $preserve.val("");
    $shipping.val("");
    $isFree.val("");
    $freight.val("");
    $low_con.val("");
    $d_freight.val("");
}

//function deleteButtonClicked(e) {
//    Coker.sweet.confirm("刪除資料", "刪除後不可返回", "確定刪除", "取消", function () {
//        co.Marquees.Delete(e.row.key);
//        e.component.refresh();
//    });
//}

function Add() {
    console.log("Default = " + $set_default.is(":checked"))
    console.log("Name = " + $name.val())
    console.log("Preserve = " + $preserve.val())
    console.log("Shipping = " + $shipping.val())
    console.log("IsFree = " + $isFree.is(":checked"))
    console.log("Freight = " + $freight.val())
    console.log("LowCon = " + $low_con.val())
    console.log("Dfreight = " + $d_freight.val())
    //co.Marquees.Add({
    //    WebsiteId: $.cookie('WebSiteId'),
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
    //        freight_list.component.refresh();
    //    }, 1000);
    //}).fail(function () {
    //    Coker.sweet.error("錯誤", error_text, null, true);
    //});
}

function Update() {
    console.log("Default = " + $set_default.is(":checked"))
    console.log("Name = " + $name.val())
    console.log("Preserve = " + $preserve.val())
    console.log("Shipping = " + $shipping.val())
    console.log("IsFree = " + $isFree.is(":checked"))
    console.log("Freight = " + $freight.val())
    console.log("LowCon = " + $low_con.val())
    console.log("Dfreight = " + $d_freight.val())
    //co.Marquees.Update({
    //    id: keyId,
    //    WebsiteId: $.cookie('WebSiteId'),
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
    //        freight_list.component.refresh();
    //    }, 1000);
    //}).fail(function () {
    //    Coker.sweet.error("錯誤", error_text, null, true);
    //});
}

function MoveToContent() {
    $("#FreightForm").removeClass("was-validated");
    $("#FreightList").addClass("d-none");
    $("#FreightContent").removeClass("d-none");
}

function BackToList() {
    $("#FreightList").removeClass("d-none");
    $("#FreightContent").addClass("d-none");
    window.location.hash = ""
}