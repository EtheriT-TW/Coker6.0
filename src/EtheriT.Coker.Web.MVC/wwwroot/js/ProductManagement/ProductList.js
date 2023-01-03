var $btn_display, $name, $name_count, $introduction, $introduction_count, $illustrate, $illustrate_count, $marks, $tag, $price, $stock_number, $alert_number, $min_number, $date, $picker, $permanent
var startDate, endDate, keyId, disp_opt = true, price_tid, temp_psid
var product_list, spec_num = 0, spec_price_num = 0, spec_remove_list = [], modal_price_list = [], techcert_list = [], tag_list = []
var $price_modal, priceModal, $techcert_body, techcertModal;

var techcert_changedBySelectBox, techcert_clearSelectionButton, tag_changedBySelectBox, tag_clearSelectionButton;
var techcert_check_list = [], techcert_text, tag_check_list = [], tag_text

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
            },
            ProdTechCert: function (data) {
                return $.ajax({
                    url: "/api/Product/TechCertAddUp",
                    type: "POST",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: JSON.stringify(data),
                    dataType: "json"
                });
            },
            ProdPrice: function (data) {
                return $.ajax({
                    url: "/api/Product/ProdPriceAddUp",
                    type: "POST",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: JSON.stringify(data),
                    dataType: "json"
                });
            },
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
            ProdSpec: function (id) {
                return $.ajax({
                    url: "/api/Product/GetSpecDetail/",
                    type: "GET",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: { typeid: id },
                });
            },
            ProdTechCert: function (id) {
                return $.ajax({
                    url: "/api/Product/GetTechCertDataAll/",
                    type: "GET",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: { PId: id },
                });
            },
            ProdPrice: function (id) {
                return $.ajax({
                    url: "/api/Product/GetPriceDataAll/",
                    type: "GET",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: { PSId: id },
                });
            },
        },
        Delete: {
            Prod: function (id) {
                return $.ajax({
                    url: "/api/Product/ProdDelete/",
                    type: "GET",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: { Id: id },
                });
            },
            Stock: function (id) {
                return $.ajax({
                    url: "/api/Product/StockDelete/",
                    type: "GET",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: { Id: id },
                });
            },
            Price: function (id) {
                return $.ajax({
                    url: "/api/Product/PriceDelete/",
                    type: "GET",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: { Id: id },
                });
            }
        }
    };

    co.Tag = {
        //AddUp: function (data) {
        //    return $.ajax({
        //        url: "/api/Product/ProductAddUp",
        //        type: "POST",
        //        contentType: 'application/json; charset=utf-8',
        //        headers: _c.Data.Header,
        //        data: JSON.stringify(data),
        //        dataType: "json"
        //    });
        //},
        Get: function (id) {
            return $.ajax({
                url: "/api/Tag/GetProductDataAll/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { PId: id },
            });
        },
        //Delete: function (id) {
        //    return $.ajax({
        //        url: "/api/Product/StockDelete/",
        //        type: "GET",
        //        contentType: 'application/json; charset=utf-8',
        //        headers: _c.Data.Header,
        //        data: { Id: id },
        //    });
        //}
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
                        var price_null = false;
                        var $null_input;
                        $(".input_price").each(function () {
                            if ($(this).val() == "") {
                                price_null = true;
                                $null_input = $(this);
                                return false;
                            }
                        })
                        if (price_null) {
                            co.sweet.error("錯誤", "請確實填寫價格", function () {
                                setTimeout(function () {
                                    $('html, body').animate({ scrollTop: $null_input.offset().top - ($("header").height() * 2) }, 0);
                                }, 500)
                            }, false);
                        } else {
                            Coker.sweet.confirm("即將發布", "發布後將直接顯示於安排的位置", "發布", "取消", function () {
                                AddUp("已成功發布", "發布發生未知錯誤", "List");
                            });
                        }
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
        if ($self.children("span").text() == "expand_less") {
            $self.children("span").text("expand_more")
        } else {
            $self.children("span").text("expand_less")
        }
    })
    $(".btn_spec_add").on("click", function () {
        SpecAdd(null);
    });
    $(".btn_spec_price_add").on("click", function () {
        SpecPriceAdd(null)
    });
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

    $(".btn_techcert_save").on("click", function () {
        if (techcert_check_list.length > 0) {
            techcert_list.forEach(function (item) {
                var index = techcert_check_list.indexOf(item.FK_TCId)
                if (index > -1) {
                    item.Checked = true;
                    techcert_check_list.splice(index, 1)
                } else {
                    item.Checked = false;
                }
            })
            if (techcert_check_list.length > 0) {
                techcert_check_list.forEach(function (item) {
                    var obj = {};
                    obj["Id"] = 0;
                    obj["FK_TCId"] = item;
                    obj["Checked"] = true;
                    techcert_list.push(obj);
                })
            }
        }
        $marks.val(techcert_text);
        techcertModal.hide();
    })

    $(".btn_tag_save").on("click", function () {
        if (tag_check_list.length > 0) {
            tag_list.forEach(function (item) {
                var index = tag_check_list.indexOf(item.FK_TCId)
                if (index > -1) {
                    item.Checked = true;
                    tag_check_list.splice(index, 1)
                } else {
                    item.Checked = false;
                }
            })
            if (tag_check_list.length > 0) {
                tag_check_list.forEach(function (item) {
                    var obj = {};
                    obj["Id"] = 0;
                    obj["FK_TCId"] = item;
                    obj["Checked"] = true;
                    tag_list.push(obj);
                })
            }
        }
        $tag.val(tag_text);
        tagModal.hide();
    })

    $(".btn_to_canvas").on("click", function (event) {
        event.preventDefault()

        Swal.fire({
            icon: 'info',
            title: "前往內容編輯頁",
            html: "是否保存資料?",
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: "　是　",
            cancelButtonText: "　否　",
            reverseButtons: true
        }).then((result) => {
            if (result.isConfirmed) {
                AddUp("已成功發布", "發布發生未知錯誤", "Canvas");
            } else {
                var hash = window.location.hash.replace("#", "") + "-1";
                window.location.hash = hash;
            }
        })
    })
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

    techcertModal = new bootstrap.Modal(document.getElementById('TechCertModal'))
    $("#TechCertModal").on("hidden.bs.modal", function () {
        var temp_list = [];
        techcert_list.forEach(function (item) {
            if (item.Checked) {
                temp_list.push(item.FK_TCId);
            }
        })
        getTechCertListDataGridInstance().selectRows(temp_list);
    })

    tagModal = new bootstrap.Modal(document.getElementById('TagModal'))
    $("#TagModal").on("hidden.bs.modal", function () {
        var temp_list = [];
        techcert_list.forEach(function (item) {
            if (item.Checked) {
                temp_list.push(item.FK_TCId);
            }
        })
        getTagListDataGridInstance().selectRows(temp_list);
    })

    $techcert_body = $("#TechCertModal > .modal-dialog > .modal-content > .modal-body");
    priceModal = new bootstrap.Modal(document.getElementById('PriceModal'))
    $price_modal = $("#PriceModal >.modal-dialog > .modal-content > .modal-body >.price_option");
    document.getElementById('PriceModal').addEventListener('hidden.bs.modal', function (event) {
        $price_modal.children(".frame").each(function () {
            $(this).remove();
            spec_price_num = 0;
        })

        $(".input_price").each(function () {
            var $self = $(this)
            var psid = $self.parents(".frame").data("psid")
            var temppsid = $self.parents(".frame").data("temppsid")
            var index = modal_price_list.findIndex(item => item["PSid"] == psid || (item["TempPSid"] != null && item["TempPSid"] == temppsid))
            if (index > -1) {
                text = "現金：" + modal_price_list[index]["Price"] + " 紅利：" + modal_price_list[index]["Bonus"]
                $self.val(text);
            } else {
                $self.val("");
            }
        })

        $(".alert_text").addClass("d-none");
    })
}

function getTechCertListDataGridInstance() {
    return $("#TechCertList").dxDataGrid("instance");
}

function TechCertList_ClearBtnInit(e) {
    techcert_clearSelectionButton = e.component;
}

function TechCertList_ClearBtnClick() {
    if (techcert_list.length > 0) {
        techcert_list.forEach(function (item) {
            item.Checked = false;
        })
    }
    getTechCertListDataGridInstance().clearSelection();
}

function TechCertList_SelectChange(selectedItems) {
    var data = selectedItems.selectedRowsData;

    techcert_check_list = [];
    if (data.length > 0) {
        techcert_text = data.map((value) => `${value.Title}`).join("、");

        data.forEach(function (item) {
            techcert_check_list.push(item.Id)
        })
    } else {
        techcert_text = "無";
    }

    techcert_changedBySelectBox = false;
    techcert_clearSelectionButton.option('disabled', !data.length);
}

function getTagListDataGridInstance() {
    return $("#TagList").dxDataGrid("instance");
}

function TagList_ClearBtnInit(e) {
    tag_clearSelectionButton = e.component;
}

function TagList_ClearBtnClick() {
    if (tag_list.length > 0) {
        tag_list.forEach(function (item) {
            item.Checked = false;
        })
    }
    getTagListDataGridInstance().clearSelection();
}

function TagList_SelectChange(selectedItems) {
    var data = selectedItems.selectedRowsData;

    tag_check_list = [];
    if (data.length > 0) {
        tag_text = data.map((value) => `${value.Title}`).join("、");

        data.forEach(function (item) {
            tag_check_list.push(item.Id)
        })
    } else {
        tag_text = "無";
    }

    tag_changedBySelectBox = false;
    tag_clearSelectionButton.option('disabled', !data.length);
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

    techcert_list = []
    $techcert_body.children("div[class=form-check]").each(function () {
        var $input = $(this).children("input");
        $input.prop("checked", false);
        $input.data("ptcid", 0);
    })
    modal_price_list = []
    price_tid = 0;
    temp_psid = 0;

    techcert_text = "";
    TechCertList_ClearBtnClick();
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
                if (hash.includes('-1')) {
                    MoveToCanvas();
                } else {
                    FormDataClear();
                    SpecAdd(null);
                    MoveToContent();
                }
            } else {
                if (hash.includes('-1')) {
                    MoveToCanvas();
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

function paletteButtonClicked(e) {
    keyId = e.row.key + "-1";
    window.location.hash = keyId;
}

function FormDataSet(result) {
    FormDataClear();
    co.Product.Get.ProdStock(result.id).done(function (all_result) {
        all_result.forEach(function (result) {
            co.Product.Get.ProdPrice(result.id).done(function (all_result) {
                all_result.forEach(function (result) {
                    var obj = {};
                    obj["Id"] = result.id;
                    obj["Tempid"] = price_tid;
                    obj["PSid"] = result.fK_PSId;
                    obj["TempPSid"] = 0;
                    obj["Rid"] = result.fK_RId
                    obj["Price"] = result.price
                    obj["Bonus"] = result.bonus
                    obj["IsDelete"] = false;
                    price_tid += 1;
                    modal_price_list.push(obj);
                })
                SpecAdd(result);
            })
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

    var text = ""
    co.Product.Get.ProdTechCert(result.id).done(function (result) {
        if (result != null && result.length > 0) {
            var temp_list = [];
            result.forEach(function (item) {
                var obj = {};
                obj["Id"] = item.id;
                obj["FK_TCId"] = item.fK_TCId;
                obj["Checked"] = item.isChecked;
                if (item.isChecked) {
                    temp_list.push(item.fK_TCId);
                    text = text == "" ? item.title : text + "、" + item.title;
                }
                techcert_list.push(obj)
            })
            getTechCertListDataGridInstance().selectRows(temp_list);
        }
        $marks.val(text == "" ? "無" : text);
    })

    text = ""
    co.Tag.Get(result.id).done(function (result) {
        console.log(result)
        //if (result != null && result.length > 0) {
        //    var temp_list = [];
        //    result.forEach(function (item) {
        //        var obj = {};
        //        obj["Id"] = item.id;
        //        obj["FK_TCId"] = item.fK_TCId;
        //        obj["Checked"] = item.isChecked;
        //        if (item.isChecked) {
        //            temp_list.push(item.fK_TCId);
        //            text = text == "" ? item.title : text + "、" + item.title;
        //        }
        //        tag_list.push(obj)
        //    })
        //    getTagListDataGridInstance().selectRows(temp_list);
        //}
        $tag.val(text == "" ? "無" : text);
    })

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
        co.Product.Delete.Prod(e.row.key).done(function () {
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
        item_btn_delete = item.find(".btn_price_delete");

    item.data("ppid", result == null ? 0 : result.Id);
    item.data("tempid", result == null ? -1 : result.Tempid);
    if (result != null) {
        item_role.val(result.Rid);
        item_cash.val(result.Price);
        item_bonus.val(result.Bonus);
    }

    item_btn_delete.on("click", function () {
        var $self_p = $(this).parents(".frame").first();
        if (spec_price_num == 1) {
            co.sweet.error("商品至少需有一種價格", null, false);
        } else {
            co.sweet.confirm("移除價格", "確定要移除此項價格嗎?", "　是　", "　否　", function () {
                if ($self_p.data("ppid") == 0) {
                    if ($self_p.data("tempid") > -1) {
                        var index = modal_price_list.findIndex(item => item["Tempid"] == $self_p.data("tempid"))
                        modal_price_list[index]["IsDelete"] = true;
                    }
                } else {
                    var index = modal_price_list.findIndex(item => item["Id"] == $self_p.data("ppid"))
                    modal_price_list[index]["IsDelete"] = true;
                }
                spec_price_num -= 1;
                $self_p.remove();
            })
        }
    })

    $("#PriceModal > .modal-dialog > .modal-content > .modal-body > .price_option").append(item);

    $("input[type='number']").on("input", function () {
        $(this).val($(this).val() < 0 ? 0 : $(this).val())
    });
}

function SpecPriceSave() {
    var temp_list = []
    var save_success = true
    $price_modal.children(".frame").each(function () {
        $self = $(this);
        var obj = {};
        obj["Id"] = $self.data("ppid");
        obj["Tempid"] = price_tid;
        obj["PSid"] = $self.parents(".modal-body").first().data("psid");
        obj["TempPSid"] = $self.parents(".modal-body").first().data("temppsid");
        obj["Rid"] = $self.find(".select_role").val();
        obj["Price"] = $self.find(".input_cash").val();
        obj["Bonus"] = $self.find(".input_bonus").val();
        obj["IsDelete"] = false;
        if (obj["Price"] == 0 && obj["Bonus"] == 0) {
            $(".alert_text").text("商品現金與紅利不可同時為空")
            $(".alert_text").removeClass("d-none");
            save_success = false
        } else {
            if (temp_list.find(item => item["Rid"] == obj["Rid"] && item["Price"] == obj["Price"] && item["Bonus"] == obj["Bonus"]) != null) {
                $(".alert_text").removeClass("d-none");
                $(".alert_text").text("價格不可重複");
                save_success = false
            } else {
                temp_list.push(obj)
                $(".alert_text").addClass("d-none");
                if ($self.data("tempid") < 0) {
                    modal_price_list.push(obj)
                    price_tid += 1;
                } else {
                    var index = modal_price_list.findIndex(item => item["Tempid"] == $self.data("tempid"))
                    modal_price_list[index]["Rid"] = $self.find(".select_role").val();
                    modal_price_list[index]["Price"] = $self.find(".input_cash").val();
                    modal_price_list[index]["Bonus"] = $self.find(".input_bonus").val();
                }
            }
        }
    })
    if (save_success) {
        priceModal.hide();
    }
}

function SpecAdd(result) {
    spec_num += 1;
    var item = $($("#TemplateSpecification").html()).clone();
    var item_topline = item.find(".topline"),
        item_select_input_1 = item.find(".input_spec").first(),
        item_select_input_2 = item.find(".input_spec").last(),
        item_select_list_1 = item.find("datalist").first(),
        item_select_list_2 = item.find("datalist").last(),
        item_price = item.find(".input_price"),
        item_min = item.find(".input_min_number"),
        item_stock = item.find(".input_stock_number"),
        item_alert = item.find(".input_alert_number"),
        item_collapse = item.find(".collapse"),
        item_btn_expand = item.find(".btn_expand"),
        item_btn_delete = item.find(".btn_delete");

    item_topline.children(".spec").each(function () {
        var spectype = $($("#TemplateSpecType").html()).clone();
        $(this).prepend(spectype);
    })

    var item_select_1 = item.find(".spec_select").first(),
        item_select_2 = item.find(".spec_select").last();

    if (result != null && result.fK_ST1id != null) {
        item_select_1.val(result.fK_ST1id);
        if (result.fK_ST1id > 0) {
            var $spec1_bro = item_select_1.parents(".spec").first().siblings(".spec");

            $spec1_bro.children(".spec_select").children("option").each(function () {
                var child = $(this)
                if (child.val() == item_select_1.val()) {
                    child.attr("disabled", "disabled");
                    child.addClass("bg-secondary-light25");
                }
            })
            item_select_input_1.removeAttr("disabled")
            co.Product.Get.ProdSpec(item_select_1.val()).done(function (spec1_result) {
                if (spec1_result != null) {
                    spec1_result.forEach(function (spec1) {
                        item_select_list_1.append('<option value="' + spec1.title + '" data-sid="' + spec1.id + '"></option>')
                        if (spec1.id == result.fK_S1id) {
                            item_select_input_1.val(spec1.title);
                        }
                    })
                }
            });
        }
    }

    if (result != null && result.fK_ST2id != null) {
        item_select_2.val(result.fK_ST2id);
        if (result.fK_ST2id > 0) {
            var $spec2_bro = item_select_2.parents(".spec").first().siblings(".spec");

            $spec2_bro.children(".spec_select").children("option").each(function () {
                var child = $(this)
                if (child.val() == item_select_2.val()) {
                    child.attr("disabled", "disabled");
                    child.addClass("bg-secondary-light25");
                }
            })
            item_select_input_2.removeAttr("disabled")
            co.Product.Get.ProdSpec(item_select_2.val()).done(function (spec2_result) {
                if (spec2_result != null) {
                    spec2_result.forEach(function (spec2) {
                        item_select_list_2.append('<option value="' + spec2.title + '" data-sid="' + spec2.id + '"></option>')
                        if (spec2.id == result.fK_S2id) {
                            item_select_input_2.val(spec2.title);
                        }
                    })
                }
            });
        }
    }

    if (result != null) {
        item.data("psid", result.id);
    } else {
        temp_psid += 1;
        item.data("temppsid", temp_psid);
    }

    var index = modal_price_list.findIndex(mitem => mitem["PSid"] == item.data("psid") || (mitem["TempPSid"] != null && mitem["TempPSid"] == item.data("temppsid")))
    if (index > -1) {
        text = "現金：" + modal_price_list[index]["Price"] + " 紅利：" + modal_price_list[index]["Bonus"]
        item_price.val(text);
    } else {
        item_price.val("");
    }
    item_min.val(result != null ? result.min_Qty : "");
    item_stock.val(result != null ? result.stock : "");
    item_alert.val("");
    item_alert.val(result != null ? result.alert_Qty : "");
    item_collapse.attr("id", "CollapseDetail" + spec_num);
    item_btn_expand.attr("data-bs-target", "#CollapseDetail" + spec_num);
    item_btn_expand.attr("aria-controls", "CollapseDetail" + spec_num);
    item_select_input_1.attr("list", "SpecListOpt" + spec_num + "-1");
    item_select_input_2.attr("list", "SpecListOpt" + spec_num + "-2");
    item_select_list_1.attr("id", "SpecListOpt" + spec_num + "-1");
    item_select_list_2.attr("id", "SpecListOpt" + spec_num + "-2");

    item_price.on("click", function () {
        var isnull = true;
        var $self = $(this)
        var psid = $self.parents(".frame").data("psid")
        var temppsid = $self.parents(".frame").data("temppsid")
        $price_modal.parents(".modal-body").first().data("psid", psid != null ? psid : "")
        $price_modal.parents(".modal-body").first().data("temppsid", temppsid != null ? temppsid : "")
        modal_price_list.forEach(function (item) {
            if (item.PSid == psid || (item.TempPsid != null && item.TempPsid == temppsid)) {
                SpecPriceAdd(item)
                isnull = false;
            }
        })
        if (isnull) {
            SpecPriceAdd(null)
        }
        priceModal.show();
    })

    if (result != null) {
        item_btn_expand.children("span").text("expand_more");
        item_btn_expand.parents("div").first().prev().removeClass("show")

    }

    item_btn_expand.on("click", function () {
        var $self = $(this);
        if ($self.children("span").text() == "expand_less") {
            $self.children("span").text("expand_more")
        } else {
            $self.children("span").text("expand_less")
        }
    })

    item_btn_delete.on("click", function () {
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
    $spec_select.each(function () {
        $self = $(this);
        $self.on("change", function () {
            var $spec_type = $(this);
            var $spec_bro = $spec_type.parents(".spec").first().siblings(".spec");
            var $spec_input = $spec_type.siblings(".input_spec");
            var $spec_list = $spec_type.siblings("datalist");

            $spec_input.val("");
            $spec_list.children("option").each(function () {
                $(this).remove();
            })
            $spec_bro.children(".spec_select").children("option").each(function () {
                var child = $(this)
                child.removeAttr("disabled");
                child.removeClass("bg-secondary-light25");
            })

            if ($spec_type.val() == 0) {
                $spec_input.attr("disabled", "disabled")
            } else {
                $spec_bro.children(".spec_select").children("option").each(function () {
                    var child = $(this)
                    if (child.val() == $spec_type.val()) {
                        child.attr("disabled", "disabled");
                        child.addClass("bg-secondary-light25");
                    }
                })
                $spec_input.removeAttr("disabled")
                co.Product.Get.ProdSpec($spec_type.val()).done(function (spec_result) {
                    if (spec_result != null) {
                        spec_result.forEach(function (spec) {
                            $spec_list.append('<option value="' + spec.title + '" data-sid="' + spec.id + '"></option>')
                        })
                    }
                });
            }
        })
    })
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
        $self.find(".input_spec").each(function () {
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

function AddUp(success_text, error_text, target) {
    if (spec_remove_list.length > 0) {
        spec_remove_list.forEach(function (item) {
            co.Product.Delete.Stock(item);
        })
    }

    co.Product.AddUp.Product({
        Id: keyId,
        Title: $name.val(),
        Disp_Opt: disp_opt,
        Ser_No: 500,
        Introduction: $introduction.val(),
        Description: $illustrate.val(),
        StartTime: startDate,
        EndTime: endDate,
        Permanent: $permanent.is(":checked"),
    }).done(function (result) {
        if (result.success) {
            keyId = result.message == null ? keyId : result.message;

            var stock_addup_list = []
            $("#Spec_Frame > .frame").each(function () {
                var $self = $(this);

                var obj = {};
                var fk_sid = [];
                $self.find(".input_spec").each(function () {
                    var id = -1;
                    $self_input = $(this);
                    $self_input.siblings("datalist").children("option").each(function () {
                        var $option = $(this);
                        if ($option.val() == $self_input.val()) {
                            id = $option.data("sid");
                        }
                    })
                    fk_sid.push(id > -1 ? id : 0)
                })

                obj["Id"] = $self.data("psid") == "" ? 0 : $self.data("psid");
                obj["Pid"] = keyId;
                obj["FK_S1id"] = fk_sid[0];
                obj["FK_S2id"] = fk_sid[1];
                obj["Stock"] = $self.find(".input_stock_number").val();
                obj["Min_Qty"] = $self.find(".input_min_number").val();
                obj["Alert_Qty"] = $self.find(".input_alert_number").val();

                stock_addup_list.push(obj);
            })

            co.Product.AddUp.Stock(stock_addup_list).done(function (result) {
                if (result.success) {
                    var new_index = result.message.split(",");
                    var price_addup_list = [];
                    modal_price_list.forEach(function (item) {
                        if (!item.IsDelete) {
                            var obj = {};
                            obj["Id"] = item.Id;
                            if (item.PSid != null) {
                                obj["FK_PSId"] = item.PSid;
                            } else {
                                obj["FK_PSId"] = new_index[item.TempPSid - 1];
                            }
                            obj["FK_RId"] = item.Rid;
                            obj["Price"] = item.Price;
                            obj["Bonus"] = item.Bonus;
                            price_addup_list.push(obj)
                        } else {
                            if (item.PSid != null && item.Id != null) {
                                co.Product.Delete.Price(item.Id);
                            }
                        }
                    })

                    co.Product.AddUp.ProdPrice(price_addup_list).done(function () {
                        if (result.success) {
                            var techcert_addup_list = [];
                            if (techcert_list != null) {
                                techcert_list.forEach(function (item) {
                                    var obj = {};
                                    obj["Id"] = item.Id;
                                    obj["FK_PId"] = keyId;
                                    obj["FK_TCId"] = item.FK_TCId;
                                    obj["IsChecked"] = item.Checked;
                                    techcert_addup_list.push(obj);
                                })
                            }
                            co.Product.AddUp.ProdTechCert(techcert_addup_list).done(function (result) {
                                if (result.success) {
                                    if (target == "List") {
                                        Coker.sweet.success(success_text, null, true);
                                        setTimeout(function () {
                                            BackToList();
                                            product_list.component.refresh();
                                        }, 1000);
                                    } else if (target == "Canvas") {
                                        Coker.sweet.success(success_text, null, true);
                                        setTimeout(function () {
                                            var hash = window.location.hash.replace("#", "") + "-1";
                                            window.location.hash = hash;
                                        }, 1000);
                                    }
                                } else {
                                    Coker.sweet.error("錯誤", error_text, null, true);
                                }
                            }).fail(function () {
                                Coker.sweet.error("錯誤", error_text, null, true);
                            })
                        } else {
                            Coker.sweet.error("錯誤", error_text, null, true);
                        }
                    }).fail(function () {
                        Coker.sweet.error("錯誤", error_text, null, true);
                    })
                } else {
                    Coker.sweet.error("錯誤", error_text, null, true);
                }
            }).fail(function () {
                Coker.sweet.error("錯誤", error_text, null, true);
            })
        } else {
            Coker.sweet.error("錯誤", error_text, null, true);
        }
    }).fail(function () {
        Coker.sweet.error("錯誤", error_text, null, true);
    });
}

function MoveToContent() {
    $("#ProductForm").removeClass("was-validated");
    $("#ProductList").addClass("d-none");
    $("#ProductCanvas").addClass("d-none");
    $("#ProductContent").removeClass("d-none");
}

function MoveToCanvas() {
    $("#ProductList").addClass("d-none");
    $("#ProductContent").addClass("d-none");
    $("#ProductCanvas").removeClass("d-none");
}

function BackToList() {
    $("#ProductList").removeClass("d-none");
    $("#ProductCanvas").addClass("d-none");
    $("#ProductContent").addClass("d-none");
    window.location.hash = ""
}