var $display, $removedFromShelves, $name, $name_count, $introduction, $introduction_count, $illustrate, $illustrate_count,
    $marks, $price, $subItemNo, $stock_number, $packingPoint_number, $alert_number, $min_number, $date, $picker, $permanent, $itemNo, $itemNo_count;
var startDate, endDate, keyId, price_tid, temp_psid
var product_list, spec_num = 0, spec_price_num = 0, spec_remove_list = [], modal_price_list = [], spec_pick_list, suggest_price_list = []
var $price_modal, priceModal
var total_files = [];
let importProdPopup = null;

function ImportProd() {
    var formData = new FormData($(`[name="fileUploadForm"]`)[0]);
    co.Product.AddUp.Import(formData).done(function (response) {
        importProdPopup.hide();
        co.sweet.success("檔案上傳成功");
        if (product_list != null) product_list.component.refresh();
    }).fail(function () {
        co.sweet.error("檔案格式錯誤，無法解讀。");
    });

}

function showImportProdPopup() {
    importProdPopup = $("#importProdPopup").dxPopup("instance");
    importProdPopup.option("contentTemplate", $("#importProdPopup-template"));
    importProdPopup.option("title", "商品匯入");
    importProdPopup.show();
}

function toolbarPreparing(e) {
    var dataGrid = e.component;

    e.toolbarOptions.items.unshift({
        location: "after",
        widget: "dxButton",
        options: {
            icon: "fa-solid fa-file-excel",
            text: "商品匯入",
            onClick: showImportProdPopup
        },
    });
}

async function PageReady() {
    ElementInit();
    TechCertListModalInit();
    TagListModalInit();
    try {
        const LogisticsBoxRequires = await co.LogisticsBox.Requires();
        if (!LogisticsBoxRequires.object) throw new Error("不需要物流箱");
    } catch (error) {
        $("#Spec_Frame").addClass("no-logistics-box");
    }
    
    // 啟動
    const editor = grapesInit({
        save: function (html, css) {
            var _dfr = $.Deferred();
            co.Product.Content.SaveConten({
                Id: $("#gjs").data("id"),
                SaveHtml: html,
                SaveCss: css
            }).done(function (resutlt) {
                if (resutlt.success) _dfr.resolve();
                else co.sweet.error(resutlt.error);
            });
            return _dfr.promise();
        },
        import: function (html, css) {
            var _dfr = $.Deferred();
            co.Product.Content.ImportConten({
                Id: $("#gjs").data("id"),
                SaveHtml: html,
                SaveCss: css
            }).done(function (resutlt) {
                if (resutlt.success) _dfr.resolve();
                else co.sweet.error(resutlt.error);
            });
            return _dfr.promise();
        },
        getComponer: function () {
            var _dfr = $.Deferred();
            co.HtmlContent.GetAllComponent().done(function (result) {
                if (result.success) _dfr.resolve(result.list);
                else co.sweet.error(resutlt.error);
            });
            return _dfr.promise();
        }
    });

    //設定html資料
    setPage = function (id) {
        co.Product.Content.GetConten({ Id: id }).done(function (result) {
            if (result.success) {
                var html = co.Data.HtmlDecode(result.conten.saveHtml);
                co.Grapes.setEditor(editor, html, result.conten.saveCss);
                co.Grapes.setFile(editor, id, 3);
            } else {
                co.sweet.error(result.error);
            }
        });
    }

    /* File Upload */
    co.File.ListFileInit();

    /* Spec List */
    co.Product.Spec.ListInit();

    /* 日期選擇 */
    $picker = $("#InputDate");

    co.Picker.Init($picker);

    $picker.on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format('YYYY/MM/DD HH:mm') + ' ~ ' + picker.endDate.format('YYYY/MM/DD HH:mm'));
        startDate = picker.startDate.format("");
        endDate = picker.endDate.format("");
    });

    $(document).on("wheel", "input[type=number]", function (event) {
        event.preventDefault();
    });

    $(document).on('blur', 'input[type="number"]', function () {
        var $self = $(this);
        var value = $self.val().trim();

        if (/^0+\d/.test(value)) value = value.replace(/^0+/, '');
        if ($self.attr("step") == "1" && value.includes(".")) value = value.substring(0, value.indexOf("."));
        if (parseFloat(value) > 100000000) value = "100000000";

        $self.val(value);
    });

    /*Form觸發*/
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
                                AddUp("已成功發布", "發布發生未知錯誤", "item");
                            });
                        }
                    }
                }
                form.classList.add('was-validated');
            }, false)
        })
    })()

    $(".btn_back").on("click", function () {
        Coker.sweet.confirm("返回商品列表", "資料將不被保存", "確定", "取消", function () {
            BackToList(true);
        });
    })
    $(".btn_add").on("click", function () {
        window.location.hash = 0;
    });
    $(".btn_input_pic").on("click", function (event) {
        event.preventDefault();
        $(".input_pic").click();
    })

    $(".btn_expand_out").on("click", function () {
        var $self = $(this);
        if ($self.children("span").text() == "expand_less") {
            $self.children("span").text("expand_more")
        } else {
            $self.children("span").text("expand_less")
        }
    })
    $(".btn_spec_price_add").on("click", function () {
        SpecPriceAdd(null)
    });
    $(".btn_price_save").on("click", SpecPriceSave);
    $("#TimePrice").on("change",function () {
        if ($(this).prop("checked")) $(".priceSetting").addClass("d-none");
        else $(".priceSetting").removeClass("d-none");
    });

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

    if ("onhashchange" in window) { window.onhashchange = hashChange; } else { setInterval(hashChange, 1000); }

    $(".btn_to_canvas").on("click", function (event) {
        event.preventDefault()

        Swal.fire({
            icon: 'info',
            title: "前往內容編輯頁",
            html: "是否保存資料?",
            showCancelButton: true,
            showDenyButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#888888',
            denyButtonColor: '#d33',
            confirmButtonText: "　是　",
            cancelButtonText: "　取消　",
            denyButtonText: "　否　",
            reverseButtons: true
        }).then((result) => {
            if (result.isConfirmed) {
                Array.from(forms).forEach(form => {
                    if (form.checkValidity()) {
                        if (ISpecRepect()) {
                            if ($removedFromShelves.is(":checked")) {
                                $removedFromShelves.prop("checked", false);
                                AddUp("已成功儲存，資料尚有缺漏或格式錯誤，未上架", "儲存發生未知錯誤", "Canvas");
                            } else {
                                AddUp("已成功儲存", "儲存發生未知錯誤", "Canvas");
                            }
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
                                if ($removedFromShelves.is(":checked")) {
                                    $removedFromShelves.prop("checked", false);
                                    AddUp("已成功儲存，資料尚有缺漏或格式錯誤，未上架", "儲存發生未知錯誤", "Canvas");
                                } else {
                                    AddUp("已成功儲存", "儲存發生未知錯誤", "Canvas");
                                }
                            } else {
                                AddUp("已成功發布", "發布發生未知錯誤", "Canvas");
                            }
                        }
                    } else {
                        if ($removedFromShelves.is(":checked")) {
                            $removedFromShelves.prop("checked", false);
                            AddUp("已成功儲存，資料尚有缺漏或格式錯誤，未上架", "儲存發生未知錯誤", "Canvas");
                        } else {
                            AddUp("已成功儲存", "儲存發生未知錯誤", "Canvas");
                        }
                    }
                });
            } else if (result.isDenied) {
                window.location.hash = `${keyId}-1`;
            }
        })
    })
}
function ElementInit() {
    $name = $("#InputName");
    $name_count = $("#ProductForm .name .name_count");
    $introduction = $("#InputIntroduction");
    $introduction_count = $("#ProductForm .introduction .introduction_count");
    $illustrate = $("#InputIllustrate");
    $illustrate_count = $("#ProductForm .illustrate .illustrate_count");
    $marks = $("#InputMarks");
    $spec_select = $(".spec_select")
    $price = $(".input_price");
    $subItemNo = $(".input_subItemNo");
    $stock_number = $(".input_stock_number");
    $packingPoint_number = $(".input_packingPoint_number");
    $min_number = $(".input_min_number");
    $alert_number = $(".input_alert_number");
    $date = $("#InputDate");
    $permanent = $("#PermanentCheck");
    $itemNo = $("#InputItemNo");
    $itemNo_count = $("#ProductForm .itemNo .itemNo_count");
    $display = $(`#ProductForm [name="Visible"]`);
    $removedFromShelves = $(`#ProductForm [name="RemovedFromShelves"]`);

    priceModal = new bootstrap.Modal(document.getElementById('PriceModal'))
    $price_modal = $("#PriceModal >.modal-dialog > .modal-content > .modal-body > .priceSetting >.price_option");
    $("#SortCheck").on("change", function () {
        const $items = $(`[name="serNo"]`);
        if ($(this).prop("checked")) $items.removeAttr("disabled");
        else $items.attr({ disabled: "disabled" });
    });
    document.getElementById('PriceModal').addEventListener('hidden.bs.modal', function (event) {
        $price_modal.children(".frame").each(function () {
            $(this).remove();
            spec_price_num = 0;
        });

        $(".input_price").each(function () {
            var $self = $(this)
            var psid = $self.parents(".spec_list").data("psid")
            var temppsid = $self.parents(".spec_list").data("temppsid")
            var timePrice = $self.parents(".spec_list").data("timeprice")
            var count = $self.parents(".price").find(".count");
            var text = "";
            var filter = modal_price_list.filter(item => !item.IsDelete && (item["FK_PSId"] == psid || item["TempPSid"] == temppsid));
            $self.removeClass("multi-price");
            if (timePrice) {
                $self.val("時價");
                count.addClass("d-none")
            } else {
                if (filter.length > 1) {
                    count.removeClass("d-none").text(filter.length);
                    $self.addClass("multi-price");
                } else count.addClass("d-none");
                filter.forEach(item => {
                    if (text != "") text += "\n";
                    text += "現金：" + co.String.thousandSign(item["Price"]);
                    if (parseInt(item["Bonus"]) !== 0) text += " 紅利：" + co.String.thousandSign(item["Bonus"]);
                });
                $self.val(filter.length ? text : "");
            }
        })

        $(".alert_text").addClass("d-none");
    })
}
function FormDataClear() {
    TechCertDataClear();
    TagDataClear();
    $("#Spec_Frame .spec_list").each(function () {
        $(this).remove();
    })
    spec_num = 0;
    keyId = 0;
    $removedFromShelves.prop("checked", false);
    $display.prop("checked", false);
    $name.val("");
    $name_count.text(0);
    $itemNo.val("");
    $itemNo_count.text(0);
    $introduction.val("");
    $introduction_count.text(0);
    $illustrate.val("");
    $illustrate_count.text(0);
    $marks.val("");
    $price.val("");
    $subItemNo.val("");
    $stock_number.val(0);
    $packingPoint_number.val(1);
    $alert_number.val("");
    $min_number.val(1);
    $permanent.prop("checked", false);
    $date.val("");
    $date.removeAttr("disabled");
    startDate = null;
    endDate = null;
    spec_remove_list = [];

    modal_price_list = [];
    suggest_price_list = [];
    price_tid = 0;
    temp_psid = 0;
    $(".data_upload").each(function () {
        UploadPreviewFrameClear($(this));
    });
    $(".data_upload > ul > .upload_list").remove();
    total_files = [];
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
    FormDataClear();
    if (window.location.hash != "") {
        if (window.currentHash != window.location.hash) {
            var hash = window.location.hash.replace("#", "");
            if (parseInt(hash) == 0) {
                if (hash.includes('-1')) {
                    MoveToCanvas();
                } else {
                    co.Spec.GetPickSpecList().done(function (pick_result) {
                        spec_pick_list = pick_result;
                        SpecAdd(null);
                        MoveToContent();
                    });
                }
            } else {
                if (hash.includes('-1')) {
                    keyId = parseInt(hash);
                    MoveToCanvas();
                } else {
                    co.Product.Get.ProdOne(parseInt(hash)).done(function (result) {
                        if (result != null) {
                            co.Spec.GetPickSpecList().done(function (pick_result) {
                                spec_pick_list = pick_result;
                                FormDataSet(result);
                                MoveToContent();
                            });
                        } else {
                            BackToList(false);
                        }
                    })
                }
            }
        }
    } else {
        BackToList(false);
    }
}
function editButtonClicked(e) {
    MoveToContent();
    keyId = e.row.key;
    window.location.hash = keyId
}
function paletteButtonClicked(e) {
    $("#gjs").data("id", e.row.key);
    setPage(e.row.key);
    keyId = e.row.key + "-1";
    window.location.hash = keyId;
}
function FormDataSet(result) {
    //console.log(result)
    //$("#ProductContent .card-header .titile").append(`編輯商品<span class="d-md-flex d-none">－${result.title}</span>`);

    TagDataSet(result.tagDatas);
    TechCertDataSet(result.techCertDatas);

    result.multimedia.forEach(media => {
        UploadListAdd(media, $("#ProdMedia"));
    })

    result.files.forEach(file => {
        UploadListAdd(file, $("#ProdFiles"));
    })
    $("#ProdMedia > ul > li:first-child").trigger("click");

    result.stocks.forEach(function (stock) {
        var suggest_price_obj = {};
        suggest_price_obj["FK_PSId"] = stock.id;
        suggest_price_obj["TempPSid"] = 0;
        suggest_price_obj["Price"] = stock.price
        suggest_price_list.push(suggest_price_obj);

        stock.prices.forEach(function (price) {
            var price_obj = {};
            price_obj["Id"] = price.id;
            price_obj["Tempid"] = price_tid;
            price_obj["FK_PSId"] = price.fK_PSId;
            price_obj["TempPSid"] = 0;
            price_obj["FK_RId"] = price.fK_RId
            price_obj["Price"] = price.price
            price_obj["Bonus"] = price.bonus
            price_obj["SubItemNo"] = price.subItemNo
            price_obj["IsDelete"] = false;
            price_tid += 1;
            modal_price_list.push(price_obj);
        });
        SpecAdd(stock);
    });

    startDate = result.startTime;
    endDate = result.endTime;
    keyId = result.id;
    disp_opt = result.disp_Opt;
    $removedFromShelves.prop("checked", !result.removedFromShelves)
    $display.prop("checked", result.visible);
    $name.val(result.title);
    $name_count.text($name.val().length);
    $itemNo.val(result.itemNo);
    $itemNo_count.text($itemNo.val().length);
    $introduction.val(result.introduction);
    $introduction_count.text($introduction.val().length);
    $illustrate.val(result.description);
    $illustrate_count.text($illustrate.val().length);
    $(`[name="ProdStatus"] > option[value="${result.status}"]`).prop("selected", true);
    $date = $("#InputDate");
    $(".linkToF").attr("href", `${defaultUrl}/${OrgName}/search/product/${result.id}`);

    $("#SortCheck").prop("checked", result.ser_No != 500);
    $(`[name="serNo"]`).val(result.ser_No);
    $("#SortCheck").trigger("change");

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
        item_role.val(result.FK_RId);
        item_cash.val(result.Price);
        item_bonus.val(result.Bonus);
    }

    item_btn_delete.on("click", function () {
        var $self_p = $(this).parents(".modal_price").first();
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

    $("#PriceModal > .modal-dialog > .modal-content > .modal-body .price_option").append(item);

    $("input[type='number']").on("input", function () {
        var $self = $(this);
        var value = $self.val();
        if (value !== "" && parseFloat(value) < 0) $self.val("0");
    });
}
function SpecPriceSave() {
    var temp_list = []
    var save_success = true
    var psid = $price_modal.parents(".modal-body").first().data("psid");
    var temppsid = $price_modal.parents(".modal-body").first().data("temppsid");
    let index;
    if (psid != "") {
        index = suggest_price_list.findIndex(item => item["FK_PSId"] == psid)
    } else {
        index = suggest_price_list.findIndex(item => item["TempPSid"] == temppsid)
    }
    suggest_price_list[index]["Price"] = $("#PriceModal .suggest_price input").val();
    $(".spec_list").each(function () {
        if ($(this).data("psid") == psid || $(this).data("temppsid") == temppsid)
            $(this).data("timeprice", $("#TimePrice").prop("checked"));
    });
    if (!$("#TimePrice").prop("checked")) {
        $price_modal.children(".frame").each(function () {
            $self = $(this);
            var obj = {};
            obj["Id"] = $self.data("ppid");
            obj["Tempid"] = price_tid;
            obj["FK_PSId"] = psid;
            obj["TempPSid"] = temppsid;
            obj["FK_RId"] = $self.find(".select_role").val();
            obj["Price"] = $self.find(".input_cash").val();
            obj["Bonus"] = $self.find(".input_bonus").val();
            obj["IsDelete"] = false;
            if (obj["Price"] == 0 && obj["Bonus"] == 0) {
                co.sweet.error("商品現金與紅利不可同時為空", null, true)
                $(".alert_text").text("商品現金與紅利不可同時為空")
                $(".alert_text").removeClass("d-none");
                save_success = false
            } else {
                if (temp_list.find(item => item["FK_RId"] == obj["FK_RId"] && (item["Price"] == obj["Price"] || item["Bonus"] == obj["Bonus"])) != null) {
                    co.sweet.error("商品現金或紅利不可重複", null, true)
                    $(".alert_text").removeClass("d-none");
                    $(".alert_text").text("同個會員等級下商品現金或紅利不可重複");
                    save_success = false
                } else {
                    temp_list.push(obj)
                    $(".alert_text").addClass("d-none");
                    if ($self.data("tempid") < 0) {
                        modal_price_list.push(obj)
                        price_tid += 1;
                    } else {
                        var index = modal_price_list.findIndex(item => item["Tempid"] == $self.data("tempid"))
                        modal_price_list[index]["FK_RId"] = $self.find(".select_role").val();
                        modal_price_list[index]["Price"] = $self.find(".input_cash").val();
                        modal_price_list[index]["Bonus"] = $self.find(".input_bonus").val();
                    }
                }
            }
        })
    }
    if (save_success) {
        priceModal.hide();
    }
}
function SpecAdd(result) {
    spec_num += 1;
    $("#Spec_Frame").data("spec_num", spec_num)
    var item = $($("#TemplateSpecification").html()).clone();
    var item_select_input_1 = item.find(".input_spec").first(),
        item_select_input_2 = item.find(".input_spec").last(),
        item_select_list_1 = item.find("datalist").first(),
        item_select_list_2 = item.find("datalist").last(),
        item_price = item.find(".input_price"),
        item_price_count = item.find(".price > .count"),
        item_subItemNo = item.find(".input_subItemNo"),
        item_min = item.find(".input_min_number"),
        item_stock = item.find(".input_stock_number"),
        item_packingPoint = item.find(".input_packingPoint_number"),
        item_alert = item.find(".input_alert_number"),
        item_collapse = item.find(".collapse"),
        item_btn_expand = item.find(".btn_expand"),
        item_btn_delete = item.find(".btn_remove");

    if (result != null) {
        item.find(".ser_no").val(result.ser_No);
        item.data("serno", result.ser_No);
        item.data("timeprice", result.timePrice);
    } else {
        item.find(".ser_no").val(spec_num);
        item.data("serno", spec_num);
        item.data("timeprice", false);
    }

    item.find(".ser_no").on("blur", function () {
        var $self = $(this);
        if ($self.val() < 1) {
            $self.val(1);
        } else if ($self.val() > $(".spec_list").length) {
            $self.val($(".spec_list").length);
        }
        if ($self.val() != item.data("serno")) {
            if ($self.val() > item.data("serno")) {
                SortChange($(".spec_list"), "bigger", item.data("serno"), $self.val())
                $("#Spec_Frame > ul.specList").children("li").eq(parseInt($self.val()) - 1).after(item);
            } else if ($self.val() < item.data("serno")) {
                SortChange($(".spec_list"), "smaller", $self.val(), item.data("serno"))
                $("#Spec_Frame > ul.specList").children("li").eq(parseInt($self.val()) - 1).before(item);
            }
        }
        item.data("serno", $self.val());
    })
    item.find(".spec").each(function () {
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

            var temp_spec_list = spec_pick_list.find(item => item.id == item_select_1.val());
            if (!!temp_spec_list && temp_spec_list.specs.length > 0) {
                temp_spec_list.specs.forEach(item => {
                    item_select_list_1.append(`<option value="${item.title}" data-sid="${item.id}"></option>`)
                    if (item.id == result.fK_S1id) {
                        item_select_input_1.val(item.title);
                        item_select_input_1.data("id", item.id);
                    }
                })
            }
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

            var temp_spec_list = spec_pick_list.find(item => item.id == item_select_2.val())
            if (temp_spec_list && temp_spec_list.specs && temp_spec_list.specs.length > 0) {
                temp_spec_list.specs.forEach(item => {
                    item_select_list_2.append(`<option value="${item.title}" data-sid="${item.id}"></option>`)
                    if (item.id == result.fK_S2id) {
                        item_select_input_2.val(item.title);
                        item_select_input_2.data("id", item.id);
                    }
                })
            }
        }
    }
    if (result != null) {
        item.data("psid", result.id);
        item.data("oldstock", result.stock);
    } else {
        temp_psid += 1;
        item.data("temppsid", temp_psid);
        item.data("oldstock", null);

        var suggest_price_obj = {};
        suggest_price_obj["FK_PSId"] = "";
        suggest_price_obj["TempPSid"] = temp_psid;
        suggest_price_obj["Price"] = 0;
        suggest_price_list.push(suggest_price_obj);
    }
    
    if (item.data("timeprice")) {
        item_price.val("時價");
        item_price_count.addClass("d-none");
    } else {
        var text = "";
        var filter = modal_price_list.filter(mitem => !mitem.IsDelete && (mitem["FK_PSId"] == item.data("psid")));
        item_price.removeClass("multi-price");
        if (filter.length > 1) {
            item_price.addClass("multi-price");
            item_price_count.removeClass("d-none").text(filter.length);
        } else item_price_count.addClass("d-none");
        filter.map(item => {
            if (text != "") text += "\n";
            if (parseInt(item["Price"]) > 0) text += "現金：" + co.String.thousandSign(item["Price"]);
            if (parseInt(item["Bonus"]) > 0) text += " 紅利：" + co.String.thousandSign(item["Bonus"]);
        });
        if (filter.length > 0) {
            item_price.val(text);
        } else {
            item_price.val("");
        }
    }
    
    item_subItemNo.val(result != null ? result.subItemNo : "");
    item_min.val(result != null ? result.min_Qty ?? 1 : 1);
    item_min.on("change", function () {
        var $self = $(this);
        if ($self.val() < 1 || $self.val() == "") $self.val(1);
    });
    item_stock.val(result != null ? (result.stock ?? 0) + result.orderStock : 0);
    item_packingPoint.val(result != null ? result.packingPoint ?? 1 : 1);
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
        var psid = $self.parents(".spec_list").data("psid")
        var temppsid = $self.parents(".spec_list").data("temppsid")
        var timePrice = $self.parents(".spec_list").data("timeprice")
        $price_modal.parents(".modal-body").first().data("psid", psid != null ? psid : "")
        $price_modal.parents(".modal-body").first().data("temppsid", temppsid != null ? temppsid : "")
        $("#TimePrice").prop("checked", timePrice);
        $("#TimePrice").trigger("change");
        if (!!psid) {
            var index = suggest_price_list.findIndex(item => item["FK_PSId"] == psid)
            $("#PriceModal .suggest_price input").val(suggest_price_list[index]["Price"]);
        } else {
            var index = suggest_price_list.findIndex(item => item["TempPSid"] == temppsid)
            $("#PriceModal .suggest_price input").val(suggest_price_list[index]["Price"]);
        }

        modal_price_list.forEach(function (item) {
            if (!item.IsDelete && (item.FK_PSId == psid || (item.TempPSid != null && item.TempPSid == temppsid))) {
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

    item_btn_delete.on("click", function (e) {
        e.preventDefault();
        var $self = $(this);
        var $self_p = $self.parents('.spec_list');
        if (spec_num == 1) {
            co.sweet.error("商品至少需有一種規格", null, false);
        } else {
            co.sweet.confirm("移除規格", "確定要移除此項規格嗎?", "　是　", "　否　", function () {
                spec_remove_list.push($self_p.data("psid"));
                spec_num -= 1;
                if (item.data("serno") < $("#Spec_Frame").data("spec_num")) { SortChange($(".spec_list"), "bigger", item.data("serno"), $("#Spec_Frame").data("spec_num")); }
                $self_p.remove();
                $("#Spec_Frame").data("spec_num", spec_num)
            })
        }
    })

    item.find(".spec_select").each(function () {
        $self = $(this);
        var $spec_input = $self.siblings(".input_spec");
        $self.on("change", function () {
            var $spec_type = $(this);
            var $spec_bro = $spec_type.parents(".spec").first().siblings(".spec");
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
                var temp_spec_list = spec_pick_list.find(item => item.id == $spec_type.val())
                if (temp_spec_list.specs.length > 0) {
                    temp_spec_list.specs.forEach(item => {
                        $spec_list.append(`<option value="${item.title}" data-sid="${item.id}"></option>`)
                    })
                }
            }
        })
        $self.siblings(".input_spec").blur(function () {
            SpecBlurFunction($(this));
        })
    })

    $("#Spec_Frame ul .btn_spec_add").before(item);

    $price = $(".input_price");
    $stock_number = $(".input_stock_number");
    $packingPoint_number = $(".input_packingPoint_number");
    $min_number = $(".input_min_number");
    $alert_number = $(".input_alert_number");

    $("input[type='number']").on("input", function () {
        var $self = $(this);
        var value = $self.val();
        if (value !== "" && parseFloat(value) < 0) $self.val("0");
    });
}
function SpecBlurFunction($spec) {
    var $option; var id;
    if ($spec.val() != "") {
        id = 0;
        $spec.each(function () {
            $self_input = $(this);
            $self_input.siblings("datalist").children("option").each(function () {
                $option = $(this);
                if ($option.val() == $self_input.val()) {
                    id = $option.data("sid");
                }
            })
        })
        if (id == 0) {
            co.Spec.SpecAddUp({ FK_Tid: $spec.prev("select").val(), Title: $spec.val() }).done(function (result) {
                if (result.success) {
                    co.Spec.GetPickSpecList().done(function (pick_result) {
                        spec_pick_list = pick_result;
                        $self_input.siblings("datalist").append(`<option value="${$spec.val()}" data-sid="${result.message}"></option>`)
                    });
                }
            });
        }
    }
}
function ISpecRepect() {
    var obj = []
    var temp_list = []
    var isRepect = false;
    $("#Spec_Frame .spec_list").each(function () {
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
function UploadListAdd(result, $target) {
    var item = $($("#TemplateUploadList").html()).clone();
    var item_serno = item.find(".ser_no"),
        item_btn_remove = item.find(".btn_remove");
    var file_num = $target.find("ul > li").length - 1;
    var tempId = total_files.length;
    if (typeof (file_num) == "undefined") file_num = 0;
    if (result == null) {
        $target.find("ul > li").each(function () {
            var $self = $(this);
            if ($self.hasClass("upload_list") && $self.find(".title").text() == "") {
                $self.remove();
                file_num -= 1;
            }
        })

        file_num += 1;
        item.data("tempid", tempId);
        item.data("serno", file_num);
        item_serno.val(file_num);
        if ($target.find(".select_frame").length == 0 && typeof ($target.data("uploadtype")) != "undefined")
            item.data("uploadtype", $target.data("uploadtype"));
        else
            item.data("uploadtype", 0);
        item.data("edit", false);
        item.on("click", function () {
            co.File.ListFile($(this));
        })
    } else if (typeof (result.id) == "undefined") {
        item.data("tempid", result.TempId);
        item.data("serno", file_num);
        item_serno.val(file_num);
        item.data("uploadtype", result.Type);
        item.data("edit", false);
        item.find(".title").text(result.Name);
        if (!!result.Link) {
            item.find(".thumb_img").attr("src", result.Link);
        } else if (result.Type == 2)
            item.find(".thumb_img").attr("src", `/images/defaultImage/360.jpg`);
        else if (result.Type == 3)
            item.find(".thumb_img").attr("src", `/images/defaultImage/video.jpg`);
        item.on("click", function () {
            co.File.ListFile($(this));
        })
    } else {
        file_num += 1;

        item.data("id", result.id);
        item.data("serno", file_num);
        item_serno.val(file_num);
        item.data("uploadtype", result.fileType);
        item.data("edit", false);
        item.find(".title").text(result.name);

        var obj = {};
        obj["Id"] = result.id;
        obj["Name"] = result.name;
        var link = result.link[0];
        if (result.fileType == 4) {
            obj["File"] = result.name;
        } else {
            obj["File"] = link;
        }
        obj["Type"] = result.fileType;
        obj["IsDelete"] = false;
        if (!!obj["File"]) {
            switch (obj.Type) {
                case 2:
                    item.find(".thumb_img").attr("src", `/images/defaultImage/360.jpg`);
                    break;
                case 3:
                    item.find(".thumb_img").attr("src", `/images/defaultImage/video.jpg`);
                    break;
                case 4:
                    item.find(".thumb_img").attr("src", `https://img.youtube.com/vi/${obj["File"]}/hqdefault.jpg`);
                    break;
                default:
                    item.find(".thumb_img").attr("src", obj["File"]);
                    break;
            }
            item.find(".btn_link").removeClass("d-none").attr("href", obj["File"]);
        } else item.find(".btn_link").addClass("d-none");
        total_files.push(obj);

        item.on("click", function () {
            co.File.ListFile($(this));
        })
    }
    $target.data("file_num", file_num);
    item_serno.on("blur", function () {
        var $self = $(this);
        var $uploadList = $target.find(".upload_list");
        if ($self.val() < 1) {
            $self.val(1);
        } else if ($self.val() > $uploadList.length) {
            $self.val($uploadList.length);
        }
        if ($self.val() != item.data("serno")) {
            if ($self.val() > item.data("serno")) {
                SortChange($uploadList, "bigger", item.data("serno"), $self.val())
                $("#ProductForm > .data_upload > ul").children("li").eq(parseInt($self.val()) - 1).after(item);
            } else if ($self.val() < item.data("serno")) {
                SortChange($uploadList, "smaller", $self.val(), item.data("serno"))
                $("#ProductForm > .data_upload > ul").children("li").eq(parseInt($self.val()) - 1).before(item);
            }
        }
        item.data("serno", $self.val());
    })

    item_btn_remove.on("click", function (e) {
        e.preventDefault();
        var $self = $(this).parents("li").first();
        var $uploadList = $target.find(".upload_list");
        if (item.data("serno") < $target.data("file_num")) {
            SortChange($uploadList, "bigger", item.data("serno"), $target.data("file_num"));
        }
        if (typeof ($self.data("id")) != "undefined") {
            total_files.find(item => item["Id"] == $self.data("id"))["IsDelete"] = true;
        } else if (typeof ($self.data("tempid")) != "undefined") {
            var tempid = $self.data("tempid");
            var index = total_files.findIndex(item => item["TempId"] == tempid);
            if (index >= 0) {
                total_files.splice(index, 1);
                total_files.forEach(file => {
                    file["TempId"] = file["TempId"] > tempid ? file["TempId"] - 1 : file["TempId"];
                })
            }
        }
        UploadPreviewFrameClear($target);
        $self.remove();
        $target.data("file_num", $target.data("file_num") - 1);
    })

    $target.find("ul > .btn_upload_add").before(item);
    co.File.ListFile(item);
}
function UploadPreviewFrameClear($target) {
    var $self = $target.find(".preview_frame");
    $self.find(".default_frame").addClass("d-flex");
    $self.find(".upload_frame").addClass("d-none");
    $self.find(".media_frame").removeClass("d-flex");
    $self.find(".youtube_frame").removeClass("d-flex");
    $self.find(".select_frame").removeClass("d-flex");
    $self.find(".youtube_preview").empty();
    $self.find(".media_preview > div").empty();
}
/* ********** *****************
排序 沒有資料的情況下依舊可以拖動 需修改
***************************/
function SortChange($self, change, minindex, maxindex) {
    $self.each(function () {
        var $li_self = $(this)
        if (change == "bigger") {
            if ($li_self.data("serno") > minindex && $li_self.data("serno") <= maxindex) {
                $li_self.find(".ser_no").val(parseInt($li_self.data("serno")) - 1);
                $li_self.data("serno", $li_self.find(".ser_no").val());
            }
        } else if (change == "smaller") {
            if ($li_self.data("serno") >= minindex && $li_self.data("serno") < maxindex) {
                $li_self.find(".ser_no").val(parseInt($li_self.data("serno")) + 1);
                $li_self.data("serno", $li_self.find(".ser_no").val());
            }
        }
    })
}
function AddUp(success_text, error_text, target) {
    var stock_addup_list = []
    var status = parseInt($(`[name="ProdStatus"] > option:selected`).val() || 0);
    var updateStock = false;
    $("#Spec_Frame ul li.spec_list").each(function () {
        var $self = $(this);
        var obj = {};
        var fk_sid = [];
        $self.find(".input_spec").each(function () {
            var id = 0;
            $self_input = $(this);
            $self_input.siblings("datalist").children("option").each(function () {
                var $option = $(this);
                if ($option.val() == $self_input.val()) {
                    id = $option.data("sid");
                }
            })
            fk_sid.push(id)
        })
        obj["Id"] = $self.data("psid") == "" || typeof ($self.data("psid")) == "undefined" ? 0 : $self.data("psid");
        if (obj["Id"] != 0) {
            var index = suggest_price_list.findIndex(item => item["FK_PSId"] == obj["Id"])
            obj["Price"] = suggest_price_list[index]["Price"];
        } else {
            var index = suggest_price_list.findIndex(item => item["TempPSid"] == $self.data("temppsid"))
            obj["Price"] = suggest_price_list[index]["Price"];
        }
        obj["TimePrice"] = $self.data("timeprice");
        obj["Price"] = isNaN(obj["Price"]) ? 0 : Number(obj["Price"]);
        obj["FK_S1id"] = fk_sid[0];
        obj["FK_S2id"] = fk_sid[1];
        obj["PackingPoint"] = $self.find(".input_packingPoint_number").val() ?? 1;
        obj["Stock"] = $self.find(".input_stock_number").val();
        obj["Alert_Qty"] = $self.find(".input_alert_number").val();
        obj["Min_Qty"] = $self.find(".input_min_number").val();
        obj["Ser_No"] = $self.find(".ser_no").val();
        obj['OldStock'] = $self.data("oldstock");
        obj['SubItemNo'] = $self.find(".input_subItemNo").val();
        updateStock = updateStock || parseInt(obj["Stock"] || 0) > parseInt(obj['OldStock'] || 0);
        var price_list = [];
        modal_price_list.forEach(function (item) {
            if (item.FK_PSId == $self.data("psid") || item.TempPSid == $self.data("temppsid")) {
                var price_object = {};
                price_object["Id"] = item.Id;
                price_object["FK_PSId"] = item.FK_PSId;
                price_object["FK_RId"] = item.FK_RId;
                price_object["Price"] = item.Price;
                price_object["Bonus"] = item.Bonus;
                price_object["IsDelete"] = item.IsDelete;
                price_list.push(price_object)
            }
        })
        obj["Prices"] = price_list;
        stock_addup_list.push(obj);
    })

    const update = function () {
        co.Product.AddUp.Product({
            Id: keyId,
            Title: $name.val(),
            ItemNo: $itemNo.val(),
            Visible: $display.is(":checked"),
            RemovedFromShelves: !$removedFromShelves.is(":checked"),
            Ser_No: $("#SortCheck").is(":checked") ? $(`[name="serNo"]`).val() : 500,
            Introduction: $introduction.val(),
            Description: $illustrate.val(),
            StartTime: startDate,
            EndTime: endDate,
            Permanent: $permanent.is(":checked"),
            TagSelected: tag_list,
            TechCertSelected: techcert_list,
            Stocks: stock_addup_list,
            Status: status
        }).done(function (result) {
            var pid = parseInt(result.message);
            if (result.success) {
                Coker.sweet.success(success_text, null, true);
                var fileListSave = [];
                if (total_files.length > 0) {
                    $("#ProductForm .data_upload > ul > li").each(function () {
                        var $self = $(this);
                        if (!$self.hasClass("btn_upload_add")) {
                            var data = [];
                            total_files.forEach(file => {
                                if ((typeof (file["Id"]) != "undefined" && file["Id"] == $self.data("id")) || (typeof (file["TempId"]) != "undefined" && file["TempId"] == $self.data("tempid"))) {
                                    data.push(file);
                                }
                            })
                            if (data.length > 0) {
                                switch (data[0]["Type"]) {
                                    case 1:
                                        if (typeof (data[0]["File"]) == "string") {
                                            co.File.fileSortChange({
                                                Id: data[0]["Id"],
                                                Sid: pid,
                                                SerNo: $self.find(".ser_no").val(),
                                            });
                                        } else {
                                            var formData = new FormData();
                                            formData.append("type", 1);
                                            formData.append("sid", pid);
                                            formData.append("serno", $self.find(".ser_no").val());
                                            data.forEach(item => {
                                                for (var i = 0; i < item["File"].length; i++) {
                                                    formData.append("files", item["File"][i]);
                                                }
                                                fileListSave.push(
                                                    co.File.Upload(formData).done(function (result) {
                                                        var _dfr = $.Deferred()
                                                        if (result.success) {
                                                            for (let n = 0; n < data.length; n++) {
                                                                data[n].Id = result.files[n].id;
                                                                data[n].File = result.files[n].path;
                                                            }
                                                            return _dfr.resolve();
                                                        } else return _dfr.reject();
                                                        return _dfr.promise();
                                                    })
                                                );
                                                formData.delete('files');
                                            })
                                        }
                                        break;
                                    /* ********** *****************
                                  360 上傳資料庫，須重打
                                   ***************************/
                                    case 2:
                                        var formData = new FormData();
                                        formData.append("type", 1);
                                        formData.append("sid", pid);
                                        formData.append("serno", $self.find(".ser_no").val());
                                        for (var i = 0; i < data.length; i += 3) {
                                            for (var j = i; j < i + 3; j++) {
                                                formData.append('files', data[j]);
                                            }
                                            formData.delete('files');
                                        }
                                        break;
                                    /* ********** *****************
                                       影片上傳資料庫，不確定錯誤是否在這
                                        ***************************/
                                    case 3:
                                        if (typeof (data[0]["File"]) == "string") {
                                            co.File.fileSortChange({
                                                Id: data[0]["Id"],
                                                sid: pid,
                                                SerNo: $self.find(".ser_no").val(),
                                            });
                                        } else {
                                            var formData = new FormData();
                                            formData.append("files", data[0]["File"]);
                                            formData.append("type", 1);
                                            formData.append("sid", pid);
                                            formData.append("serno", $self.find(".ser_no").val());
                                            fileListSave.push(
                                                co.File.Upload(formData).done(function (result) {
                                                    if (result.success) {
                                                        data[0].Id = result.files[0].id;
                                                        data[0].File = result.files[0].path;
                                                    }
                                                })
                                            );
                                        }
                                        break;
                                    case 4:
                                        var Id = typeof (data[0]["Id"]) == "undefined" ? 0 : data[0]["Id"];
                                        fileListSave.push(
                                            co.File.UploadYTLink({
                                                Id: Id,
                                                File: data[0]["File"] + "",
                                                SId: pid,
                                                Type: 1,
                                                SerNo: $self.find(".ser_no").val(),
                                            }).done(function (result) {
                                                var _dfr = $.Deferred();
                                                if (result.success && typeof (result.files) != "undefined") {
                                                    data[0].Id = result.files[0].id;
                                                    return _dfr.resolve();
                                                } else return _dfr.reject();
                                                return _dfr.promise();
                                            })
                                        );
                                        break;
                                    case 5:
                                        if (typeof (data[0]["File"]) == "string") {
                                            co.File.fileSortChange({
                                                Id: data[0]["Id"],
                                                sid: pid,
                                                SerNo: $self.find(".ser_no").val(),
                                            });
                                        } else {
                                            var formData = new FormData();
                                            formData.append("files", data[0]["File"]);
                                            formData.append("type", 8);
                                            formData.append("sid", pid);
                                            formData.append("serno", $self.find(".ser_no").val());
                                            fileListSave.push(
                                                co.File.Upload(formData).done(function (result) {
                                                    var _dfr = $.Deferred();
                                                    if (result.success) {
                                                        data[0].Id = result.files[0].id;
                                                        data[0].File = result.files[0].path;
                                                        return _dfr.resolve();
                                                    } else return _dfr.reject();
                                                    return _dfr.promise();
                                                })
                                            );
                                        }
                                }
                            }
                        }
                    })

                    total_files.forEach(file => {
                        if (typeof (file["IsDelete"]) != "undefined" && file["IsDelete"] == true) {
                            switch (file["Type"]) {
                                /* ********** *****************
                               360檔案刪除未處理
                                ***************************/
                                case 2:
                                    break;
                                case 1:
                                case 3:
                                case 4:
                                case 5:
                                    if (typeof (file["Id"]) != "undefined") {
                                        var deleteid_list = [];
                                        deleteid_list.push(file["Id"]);
                                        fileListSave.push(
                                            co.File.DeleteFileById({
                                                Sid: parseInt(result.message),
                                                Type: (file["Type"] == 5 ? 8 : 1),
                                                Fid: deleteid_list,
                                            })
                                        );
                                    }
                                    break;
                            }
                        }
                    });

                    switch (target) {
                        case "List":
                            setTimeout(function () {
                                BackToList(true);
                            }, 1000);
                            break;
                        case "Canvas":
                            setTimeout(function () {
                                window.location.hash = `${pid}-1`;
                            }, 1000);
                            break;
                    }
                } else {
                    switch (target) {
                        case "List":
                            setTimeout(function () {
                                BackToList(true);
                            }, 1000);
                            break;
                        case "Canvas":
                            setTimeout(function () {
                                window.location.hash = `${pid}-1`;
                            }, 1000);
                            break;
                    }
                }
                $.when.apply(null, fileListSave).done(function () {
                    HashDataEdit();
                });
            } else {
                Coker.sweet.error("錯誤", error_text, null, true);
            }
        }).fail(function () {
            Coker.sweet.error("錯誤", error_text, null, true);
        });
    }
    if (status == 2 && updateStock) {
        co.sweet.confirm("商品狀態即將變更", "您加大了商品可銷售量，商品售完狀態將被變更！", "確認", "取消", update);
    } else update()
    if (spec_remove_list.length > 0) {
        spec_remove_list.forEach(function (item) {
            co.Product.Delete.Stock(item);
        })
    }
}
function setTotalFile(obj) {
    total_files.forEach((index, item) => {
        obj.data.forEach((index2, item2) => {
            if (typeof (item.TempId) != "") {

            }
        });
    });
}
function MoveToContent() {
    if (keyId == 0) $("#ProductContent .card-header .titile").text("新增商品")
    else $("#ProductContent .card-header .titile").text("編輯商品")
    $("#ProductForm").removeClass("was-validated");
    $("#ProductList").addClass("d-none");
    $("#ProductCanvas").addClass("d-none");
    $("#ProductContent").removeClass("d-none");
    tagContentRefresh();
}
function MoveToCanvas() {
    $("#gjs").data("id", keyId);
    setPage(keyId);
    $("#TopLine > a").removeClass("d-none");
    $("#ProductList").addClass("d-none");
    $("#ProductContent").addClass("d-none");
    $("#ProductCanvas").removeClass("d-none");
}
function BackToList(refresh) {
    $("#TopLine > a").addClass("d-none");
    $("#ProductList").removeClass("d-none");
    $("#ProductCanvas").addClass("d-none");
    $("#ProductContent").addClass("d-none");
    if (refresh) {
        window.location.hash = "";
        product_list.component.refresh();
    }
}