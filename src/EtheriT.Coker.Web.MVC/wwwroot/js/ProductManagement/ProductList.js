var $btn_display, $name, $name_count, $introduction, $introduction_count, $illustrate, $illustrate_count, $marks, $tag, $price, $stock_number, $alert_number, $min_number, $date, $picker, $permanent
var startDate, endDate, keyId, disp_opt = true
var product_list, spec_num = 0, spec_price_num = 0, spec_remove_list = [], spec_price_remove_list = [], modal_price_list = []
var $price_modal, priceModal;

function PageReady() {
    co.Product = {
        AddUp: {
            Product: function (data) {
                return $.ajax({
                    url: "/api/Product/ProductAddUp",
                    type: "POST",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: JSON.stringify(data),
                    dataType: "json"
                });
            },
            Stock: function (data) {
                return $.ajax({
                    url: "/api/Product/StockAddUp",
                    type: "POST",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: JSON.stringify(data),
                    dataType: "json"
                });
            }
        },
        Get: {
            ProdOne: function (id) {
                return $.ajax({
                    url: "/api/Product/GetProdDataOne/",
                    type: "GET",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: { id: id },
                });
            },
            ProdStock: function (id) {
                return $.ajax({
                    url: "/api/Product/GetStockDataAll/",
                    type: "GET",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: { PId: id },
                });
            },
            SpecType: function () {
                return $.ajax({
                    url: "/api/Product/GetSpecType/",
                    type: "GET",
                    headers: _c.Data.Header,
                });
            },
            Spec: function (id) {
                return $.ajax({
                    url: "/api/Product/GetSpecDetail/",
                    type: "GET",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: { typeid: id },
                });
            }
        },
        Delect: {
            Prod: function (data) {
                return $.ajax({
                    url: "/api/Product/ProdDelete",
                    type: "POST",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: JSON.stringify(data),
                    dataType: "json"
                });
            },
            Stock: function (data) {
                return $.ajax({
                    url: "/api/Product/StockDelete",
                    type: "POST",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: JSON.stringify(data),
                    dataType: "json"
                });
            }
        }
    };

    ElementInit();

    $picker = $("#InputDate");

    co.Picker.Init($picker);

    $picker.on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format('YYYY/MM/DD HH:mm') + ' ~ ' + picker.endDate.format('YYYY/MM/DD HH:mm'));
        startDate = picker.startDate.format("");
        endDate = picker.endDate.format("");
    });

    const forms = $('#ProductForm');
    (() => {
        Array.from(forms).forEach(form => {
            form.addEventListener('submit', event => {
                if (!form.checkValidity()) {
                    event.preventDefault()
                    event.stopPropagation()
                } else {
                    event.preventDefault();
                    if (ISpecRepect()) {
                        co.sweet.error("錯誤", "商品規格不可重複", null, false);
                    } else {
                        Coker.sweet.confirm("即將發布", "發布後將直接顯示於安排的位置", "發布", "取消", function () {
                            AddUp(disp_opt, "已成功發布", "發布發生未知錯誤");
                        });
                    }
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
        window.location.hash = 0;
        HashDataEdit();
    });
    $(".btn_save").on("click", function () {
        disp_opt = false;
        $btn_display.children("span").text("visibility_off");
        AddUp(false, "已存為草稿", "儲存草稿發生未知錯誤");
    });
    $(".btn_input_pic").on("click", function (event) {
        event.preventDefault();
        $(".input_pic").click();
    })
    $btn_display.on("click", function () {
        if (disp_opt) {
            $btn_display.children("span").text("visibility_off");
            disp_opt = !disp_opt;
        } else {
            $btn_display.children("span").text("visibility");
            disp_opt = !disp_opt;
        }
    })
    $(".btn_expand_out").on("click", function () {
        var $self = $(this);
        if ($self.children("span").text() == "expand_more") {
            $self.children("span").text("expand_less")
        } else {
            $self.children("span").text("expand_more")
        }
    })
    $(".btn_spec_add").on("click", SpecAdd);
    $(".btn_spec_price_add").on("click", SpecPriceAdd);
    $(".btn_price_save").on("click", SpecPriceSave);

    $name.on('keyup', function () {
        $name_count.text($name.val().length);
    });
    $introduction.on('keyup', function () {
        $introduction_count.text($introduction.val().length);
    });
    $illustrate.on('keyup', function () {
        $illustrate_count.text($illustrate.val().length);
    });

    $permanent.on("click", function () {
        if ($permanent.is(":checked")) {
            $date.val('');
            $date.attr("disabled", "disabled");
            startDate = null;
            endDate = null;
        } else {
            $date.removeAttr("disabled");
        }
    })

    if ("onhashchange" in window) {
        window.onhashchange = hashChange;
    } else {
        setInterval(hashChange, 1000);
    }
}

function ElementInit() {
    $btn_display = $("#Btn_Display");
    $name = $("#InputName");
    $name_count = $("#ProductForm > .name .name_count");
    $introduction = $("#InputIntroduction");
    $introduction_count = $("#ProductForm > .introduction .introduction_count");
    $illustrate = $("#InputIllustrate");
    $illustrate_count = $("#ProductForm > .illustrate .illustrate_count");
    $marks = $("#InputMarks");
    $tag = $("#InputTag");
    $spec_select = $(".spec_select")
    $price = $(".input_price");
    $stock_number = $(".input_stock_number");
    $min_number = $(".input_min_number");
    $alert_number = $(".input_alert_number");
    $date = $("#InputDate");
    $permanent = $("#PermanentCheck");

    priceModal = new bootstrap.Modal(document.getElementById('PriceModal'))
    $price_modal = $("#PriceModal >.modal-dialog > .modal-content > .modal-body >.price_option");
    document.getElementById('PriceModal').addEventListener('hidden.bs.modal', function (event) {
        $price_modal.children(".frame").each(function () {
            $(this).remove();
            modal_price_list = [];
            spec_price_num = 0;
        })
    })
}

function FormDataClear() {
    $("#Spec_Frame > .frame").each(function () {
        $(this).remove();
    })
    spec_num = 0;
    keyId = 0;
    $btn_display.children("span").text("visibility");
    disp_opt = true;
    $name.val("");
    $name_count.text(0);
    $introduction.val("");
    $introduction_count.text(0);
    $illustrate.val("");
    $illustrate_count.text(0);
    $marks.val("");
    $tag.val("");
    $price.val("");
    $stock_number.val("");
    $alert_number.val("");
    $min_number.val("");
    $permanent.prop("checked", false);
    $date.val("");
    $date.removeAttr("disabled");
    startDate = null;
    endDate = null;
    spec_remove_list = [];
}

function contentReady(e) {
    product_list = e;
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
                SpecAdd(null);
                MoveToContent();
            } else {
                co.Product.Get.ProdOne(parseInt(hash)).done(function (result) {
                    if (result != null) {
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
    FormDataClear();
    co.Product.Get.ProdStock(result.id).done(function (all_result) {
        all_result.forEach(function (result) {
            SpecAdd(result);
        })
    })
    startDate = result.startTime;
    endDate = result.endTime;
    keyId = result.id;
    disp_opt = result.disp_Opt;
    $btn_display.children("span").text(result.disp_Opt ? "visibility" : "visibility_off");

    $name.val(result.title);
    $name_count.text($name.val().length);
    $introduction.val(result.introduction);
    $introduction_count.text($introduction.val().length);
    $illustrate.val(result.description);
    $illustrate_count.text($illustrate.val().length);

    $marks.val("");
    $tag.val("");
    $date = $("#InputDate");
    if (result.permanent) {
        $date.val('');
        $date.attr("disabled", "disabled");
        $permanent.prop("checked", true);
    } else {
        startDate != null && $picker.data('daterangepicker').setStartDate(startDate);
        endDate != null && $picker.data('daterangepicker').setEndDate(endDate);
    }
}

function deleteButtonClicked(e) {
    Coker.sweet.confirm("刪除資料", "刪除後不可返回", "確定刪除", "取消", function () {
        co.Product.Delect.Prod({
            Id: e.row.key,
            TId: $.cookie('secret')
        }).done(function () {
            product_list.component.refresh();
        }).fail(function () {
            Coker.sweet.error("錯誤", "刪除資料發生錯誤", null, true);
        });
    });
}

function SpecPriceAdd(result) {
    spec_price_num += 1;
    var item = $($("#ModalTemplatePrice").html()).clone();
    var item_role = item.find(".select_role"),
        item_cash = item.find(".input_cash"),
        item_bonus = item.find(".input_bonus"),
        item_btn_delect = item.find(".btn_price_delect");

    //co.Product.Get.SpecType().done(function (type_result) {
    //    if (type_result != null) {
    //        type_result.forEach(function (type) {
    //            var spec = $($("#TemplateSpecSelect").html()).clone();
    //            var spec_select = spec.find("select");
    //            co.Product.Get.Spec(type.id).done(function (spec_result) {
    //                spec_select.attr("aria-label", type.title);
    //                if (result != null) {
    //                    spec_select.append('<option value="" disabled="disabled">' + type.title + '</option>')
    //                    spec_select.append('<option value="0">無</option>')
    //                    spec_result.forEach(function (spec) {
    //                        if (spec.id == result.fK_S1id || spec.id == result.fK_S2id) {
    //                            spec_select.append('<option selected value="' + spec.id + '">' + spec.title + '</option>')
    //                        } else {
    //                            spec_select.append('<option value="' + spec.id + '">' + spec.title + '</option>')
    //                        }
    //                    })
    //                } else {
    //                    spec_select.append('<option selected value="" disabled="disabled">' + type.title + '</option>')
    //                    spec_select.append('<option value="0">無</option>')
    //                    spec_result.forEach(function (spec) {
    //                        spec_select.append('<option value="' + spec.id + '">' + spec.title + '</option>')
    //                    })
    //                }
    //            });
    //            item_select.append(spec);
    //        })
    //    }
    //});

    //item.data("psid", result != null ? result.id : "")
    //item_price.val(result != null ? result.price : "");
    //item_min.val(result != null ? result.min_Qty : "");
    //item_stock.val(result != null ? result.stock : "");
    //item_alert.val("");
    //item_alert.val(result != null ? result.alert_Qty : "");

    item_btn_delect.on("click", function () {
        $self_p = $(this).parents(".frame").first();
        if (spec_price_num == 1) {
            co.sweet.error("商品至少需有一種價格", null, false);
        } else {
            co.sweet.confirm("移除價格", "確定要移除此項價格嗎?", "　是　", "　否　", function () {
                spec_price_remove_list.push($self_p.data("ppid"));
                spec_price_num -= 1;
                $self_p.remove();
            })
        }
    })

    $("#PriceModal > .modal-dialog > .modal-content > .modal-body > .price_option").append(item);

    //$spec_select = $(".spec_select")
    //$price = $(".input_price");
    //$stock_number = $(".input_stock_number");
    //$min_number = $(".input_min_number");
    //$alert_number = $(".input_alert_number");

    $("input[type='number']").on("input", function () {
        $(this).val($(this).val() < 0 ? 0 : $(this).val())
    });
}

function SpecPriceSave() {
    var save_success = true;
    var obj = []
    modal_price_list = []
    $price_modal.children(".frame").each(function () {
        $self = $(this);
        obj["PSid"] = $self.find(".select_role").val()
        obj["Rid"] = $self.find(".select_role").val()
        obj["Price"] = $self.find(".input_cash").val()
        obj["Bonus"] = $self.find(".input_bonus").val()
        if (obj["Price"] == "" && obj["Bonus"] == "") {
            co.sweet.error("錯誤", "商品現金與紅利不可同時為空", null, false);
            save_success = false;
            return false;
        } else {
            if (modal_price_list.find(item => item[0] == obj[0] && item[1] == obj[1] && item[2] == obj[2]) != null) {
                co.sweet.error("錯誤", "價格不可重複", null, false);
                save_success = false;
                return false;
            } else {
                modal_price_list.push(obj);
                obj = [];
            }
        }
        //if (modal_price_list.find(item => item["Rid"] == obj["Rid"] && item["Price"] == obj["Price"] && item["Bonus"] == obj["Bonus"]) != null) {
        //    isRepect = true;
        //} else {
        //    modal_price_list.push(obj);
        //    obj = [];
        //}
    })
    if (save_success) {
        priceModal.hide();
        console.log(modal_price_list)
        console.log($price_modal.data("psid"))
    }
}

function SpecAdd(result) {
    spec_num += 1;
    var item = $($("#TemplateSpecification").html()).clone();
    var item_select = item.find(".select"),
        item_price = item.find(".input_price"),
        item_min = item.find(".input_min_number"),
        item_stock = item.find(".input_stock_number"),
        item_alert = item.find(".input_alert_number"),
        item_collapse = item.find(".collapse"),
        item_btn_expand = item.find(".btn_expand"),
        item_btn_delect = item.find(".btn_delect");

    co.Product.Get.SpecType().done(function (type_result) {
        if (type_result != null) {
            type_result.forEach(function (type) {
                var spec = $($("#TemplateSpecSelect").html()).clone();
                var spec_select = spec.find("select");
                co.Product.Get.Spec(type.id).done(function (spec_result) {
                    spec_select.attr("aria-label", type.title);
                    if (result != null) {
                        spec_select.append('<option value="" disabled="disabled">' + type.title + '</option>')
                        spec_select.append('<option value="0">無</option>')
                        spec_result.forEach(function (spec) {
                            if (spec.id == result.fK_S1id || spec.id == result.fK_S2id) {
                                spec_select.append('<option selected value="' + spec.id + '">' + spec.title + '</option>')
                            } else {
                                spec_select.append('<option value="' + spec.id + '">' + spec.title + '</option>')
                            }
                        })
                    } else {
                        spec_select.append('<option selected value="" disabled="disabled">' + type.title + '</option>')
                        spec_select.append('<option value="0">無</option>')
                        spec_result.forEach(function (spec) {
                            spec_select.append('<option value="' + spec.id + '">' + spec.title + '</option>')
                        })
                    }
                });
                item_select.append(spec);
            })
        }
    });

    item.data("psid", result != null ? result.id : "")
    item_price.val(result != null ? result.price : "");
    item_min.val(result != null ? result.min_Qty : "");
    item_stock.val(result != null ? result.stock : "");
    item_alert.val("");
    item_alert.val(result != null ? result.alert_Qty : "");
    item_collapse.attr("id", "CollapseDetail" + spec_num);
    item_btn_expand.attr("data-bs-target", "#CollapseDetail" + spec_num);
    item_btn_expand.attr("aria-controls", "CollapseDetail" + spec_num);

    //item_price.on("click", function () {
    //    SpecPriceAdd(null);
    //    var psid = $(this).parents(".frame").data("psid")
    //    $price_modal.data("psid", psid != null ? psid : "")
    //    priceModal.show()
    //})

    item_btn_expand.on("click", function () {
        var $self = $(this);
        if ($self.children("span").text() == "expand_more") {
            $self.children("span").text("expand_less")
        } else {
            $self.children("span").text("expand_more")
        }
    })

    item_btn_delect.on("click", function () {
        $self_p = $(this).parents(".frame").first();
        if (spec_num == 1) {
            co.sweet.error("商品至少需有一種規格", null, false);
        } else {
            co.sweet.confirm("移除規格", "確定要移除此項規格嗎?", "　是　", "　否　", function () {
                spec_remove_list.push($self_p.data("psid"));
                spec_num -= 1;
                $self_p.remove();
            })
        }
    })

    $("#Spec_Frame").append(item);

    $spec_select = $(".spec_select")
    $price = $(".input_price");
    $stock_number = $(".input_stock_number");
    $min_number = $(".input_min_number");
    $alert_number = $(".input_alert_number");

    $("input[type='number']").on("input", function () {
        $(this).val($(this).val() < 0 ? 0 : $(this).val())
    });
}

function ISpecRepect() {
    var obj = []
    var temp_list = []
    var isRepect = false;
    $("#Spec_Frame > .frame").each(function () {
        $self = $(this);
        $self.find(".spec_select").each(function () {
            obj.push($(this).val());
        })
        if (temp_list.find(item => item[0] == obj[0] && item[1] == obj[1]) != null) {
            isRepect = true;
        } else {
            temp_list.push(obj);
            obj = [];
        }
    })
    return isRepect;
}

function AddUp(display, success_text, error_text) {
    if (spec_remove_list.length > 0) {
        spec_remove_list.forEach(function (item) {
            co.Product.Delect.Stock({
                Id: item,
                TId: $.cookie('secret')
            });
        })
    }

    co.Product.AddUp.Product({
        TId: $.cookie('secret'),
        Id: keyId,
        Title: $name.val(),
        Disp_Opt: display,
        Ser_No: 500,
        Introduction: $introduction.val(),
        Description: $illustrate.val(),
        StartTime: startDate,
        EndTime: endDate,
        Permanent: $permanent.is(":checked"),
    }).done(function (result) {
        keyId = result.message == null ? keyId : result.message;
        $("#Spec_Frame > .frame").each(function () {
            $self = $(this);
            var fk_sid = []
            $self.find(".spec_select").each(function () {
                fk_sid.push($(this).val())
            })
            co.Product.AddUp.Stock({
                TId: $.cookie('secret'),
                Id: $self.data("psid") == "" ? 0 : $self.data("psid"),
                Pid: keyId,
                FK_S1id: fk_sid[0],
                FK_S2id: fk_sid[1],
                Price: $self.find(".input_price").val(),
                Stock: $self.find(".input_stock_number").val(),
                Min_Qty: $self.find(".input_min_number").val(),
                Alert_Qty: $self.find(".input_alert_number").val()
            }).done(function () {
                Coker.sweet.success(success_text, null, true);
                setTimeout(function () {
                    BackToList();
                    product_list.component.refresh();
                }, 1000);
            }).fail(function () {
                Coker.sweet.error("錯誤", error_text, null, true);
            });
        })
    }).fail(function () {
        Coker.sweet.error("錯誤", error_text, null, true);
    });
}

function MoveToContent() {
    $("#ProductForm").removeClass("was-validated");
    $("#ProductList").addClass("d-none");
    $("#ProductContent").removeClass("d-none");
}

function BackToList() {
    $("#ProductList").removeClass("d-none");
    $("#ProductContent").addClass("d-none");
    window.location.hash = ""
}