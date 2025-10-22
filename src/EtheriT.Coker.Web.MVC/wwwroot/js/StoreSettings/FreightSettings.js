var $set_default, $title, $preserve, $shipping, $freigntStatusType, $freight, $low_con, $d_freight, $pricing_method, $InputProd;
var keyId, disp_opt = true, freight_type
var freight_list
/// <reference path="/wwwroot/js/CokerCore.min.js" />
function PageReady() {
    ElementInit();
    prodListModalInit();
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
        Coker.sweet.confirm("返回運費列表", "資料將不被保存", "確定", "取消", function () {
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
    $freigntStatusType.on("change", function () {
        const type = parseInt($(this).val());
        switch (type) {
            case 2:
                $InputProd.removeAttr("disabled");
                break;
            default:
                $InputProd.attr("disabled", "disabled");
                break;
        }
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
    co.Order.GetFreigntStatusTypEnum().done(function (result) {
        $(result).each(function () {
            var e = this;
            $freigntStatusType.append($("<option>").attr({ value: e.id }).text(e.name));
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
    $freigntStatusType = $("#SelectStatus");
    $freight = $("#InputFreight");
    $low_con = $("#InputLowCon");
    $d_freight = $("#InputDfreight");
    $pricing_method = $("input[name=FreightRadio]");
    $InputProd = $("#InputProd");
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
    $freigntStatusType.val(1);
    $InputProd.attr("disabled", "disabled");

    $pricing_method.each(function () {
        $(this).prop('checked', false)
        FreightRadio();
    })
    ProdDataClear();
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
                $preserve.val(1);
                $freigntStatusType.val(1);
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
    ProdDataSet(result.prodIds);
    $set_default.prop("checked", result.set_Default);
    $title.val(result.title);
    $preserve.val(result.preserveType);
    $shipping.val(result.freigntType);
    $freigntStatusType.val(result.freigntStatusType);
    $freight.val(result.freight);
    $low_con.val(result.low_Con);
    $d_freight.val(result.dis_Freight);
    $pricing_method.each(function () {
        if ($(this).val() == result.freigntType) {
            $(this).prop('checked', true)
            FreightRadio();
        }
    })
    $freigntStatusType.trigger("change");
}

function deleteButtonClicked(e) {
    Coker.sweet.confirm("刪除資料", "刪除後不可返回", "確定刪除", "取消", function () {
        co.Freight.Delete(e.row.key).done(function () {
            Coker.sweet.success("刪除成功", null, true);
            e.component.refresh();
        });
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
        FreigntStatusType: $freigntStatusType.val(),
        Freight: $freight.val(),
        Low_Con: $low_con.val(),
        Dis_Freight: $d_freight.val(),
        Set_Default: $set_default.is(":checked"),
        ProdIds: prod_list
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