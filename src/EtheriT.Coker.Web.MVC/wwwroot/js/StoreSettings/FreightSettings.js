var $set_default, $title, $preserve, $shipping, $freight, $low_con, $d_freight, $pricing_method;
var keyId, disp_opt = true, freight_type
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
        }
    };
    co.Freight = {
        AddUp: function (data) {
            return $.ajax({
                url: "/api/Freight/AddUp",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        Get: function (id) {
            return $.ajax({
                url: "/api/Freight/GetOne/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { id: id },
            });
        },
        Delete: function (id) {
            return $.ajax({
                url: "/api/Freight/Delete/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { id: id },
            });
        }
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
                    AddUp();
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

    $('input[type=radio][name=FreightRadio]').change(FreightRadio);

    if ("onhashchange" in window) {
        window.onhashchange = hashChange;
    } else {
        setInterval(hashChange, 1000);
    }
}

function ElementInit() {
    $set_default = $("#CheckDefault");
    $title = $("#InputName");
    $preserve = $("#SelectPreserve");
    $shipping = $("#SelectShipping");
    $freight = $("#InputFreight");
    $low_con = $("#InputLowCon");
    $d_freight = $("#InputDfreight");
    $pricing_method = $("input[name=FreightRadio]");
}

function FormDataClear() {
    keyId = 0;
    $set_default.val("");
    $title.val("");
    $preserve.val("");
    $shipping.val("");
    $freight.val("");
    $freight.attr("disabled", "disabled");
    $low_con.val("");
    $low_con.attr("disabled", "disabled");
    $d_freight.val("");
    $d_freight.attr("disabled", "disabled");

    $pricing_method.each(function () {
        $(this).prop('checked', false)
        FreightRadio();
    })
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
                co.Freight.Get(parseInt(hash)).done(function (result) {
                    if (result != null) {
                        keyId = result.id;
                        FormDataSet(result);
                        MoveToContent();
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
    window.location.hash = keyId
}

function FormDataSet(result) {
    $set_default.prop("checked", result.set_Default);
    $title.val(result.title);
    $preserve.val(result.preserveType);
    $shipping.val(result.freigntType);
    $freight.val(result.freight);
    $low_con.val(result.low_Con);
    $d_freight.val(result.dis_Freight);
    $pricing_method.each(function () {
        if ($(this).val() == result.freigntType) {
            $(this).prop('checked', true)
            FreightRadio();
        }
    })
}

function deleteButtonClicked(e) {
    Coker.sweet.confirm("刪除資料", "刪除後不可返回", "確定刪除", "取消", function () {
        co.Freight.Delete(e.row.key);
        e.component.refresh();
    });
}

function FreightRadio() {
    $pricing_method.each(function () {
        if ($(this).is(":checked")) {
            freight_type = $(this).val();
            switch (parseInt($(this).val())) {
                case 1:
                    $freight.val("");
                    $freight.attr("disabled", "disabled");
                    $low_con.val("");
                    $low_con.attr("disabled", "disabled");
                    $d_freight.val("");
                    $d_freight.attr("disabled", "disabled");
                    break;
                case 2:
                    $freight.removeAttr("disabled");
                    $low_con.removeAttr("disabled");
                    $d_freight.removeAttr("disabled");
                    break;
            }
        }
    })
}

function AddUp() {
    co.Freight.AddUp({
        Id: keyId,
        Title: $title.val(),
        PreserveType: $preserve.val(),
        LogisticsType: $shipping.val(),
        FreigntType: freight_type,
        Freight: $freight.val(),
        Low_Con: $low_con.val(),
        Dis_Freight: $d_freight.val(),
        Set_Default: $set_default.is(":checked")
    }).done(function () {
        Coker.sweet.success("運費設定儲存成功", null, true);
        setTimeout(function () {
            BackToList();
            freight_list.component.refresh();
        }, 1000);
    }).fail(function () {
        Coker.sweet.error("錯誤", "儲存運費設定發生錯誤", null, true);
    });
}

function MoveToContent() {
    $("#FreightForm").removeClass("was-validated");
    $("#FreightList").addClass("d-none");
    $("#FreightContent").removeClass("d-none");
}

function BackToList() {
    $("#FreightList").removeClass("d-none");
    $("#FreightContent").addClass("d-none");
    FormDataClear();
    window.location.hash = ""
}