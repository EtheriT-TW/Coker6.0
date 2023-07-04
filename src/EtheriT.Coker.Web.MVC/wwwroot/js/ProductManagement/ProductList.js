var $btn_display, $name, $name_count, $introduction, $introduction_count, $illustrate, $illustrate_count, $marks, $price, $stock_number, $alert_number, $min_number, $date, $picker, $permanent
var startDate, endDate, keyId, disp_opt = true, price_tid, temp_psid
var product_list, spec_num = 0, spec_price_num = 0, spec_remove_list = [], modal_price_list = [], spec_pick_list
var $price_modal, priceModal
var file_num = 0, total_files = [];
let importProdPopup = null;
function ImportProd() {
    var formData = new FormData($(`[name="fileUploadForm"]`)[0]);
    co.Product.AddUp.Import(formData).done(function (response) {
        importProdPopup.hide();
        co.sweet.success("檔案上傳成功");
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

function PageReady() {
    ElementInit();
    TechCertListModalInit();
    TagListModalInit();

    /* File Upload */
    $(".btn_upload_add > button").on("click", function (e) {
        e.preventDefault();
        UploadListAdd(null);
    })

    $(window).on("fileUploadWithPreview:imagesAdded", function (event) {
        var cachedFile = upload_file.cachedFileArray;

        $("#ProductForm > .data_upload > ul > li").each(function () {
            var $self = $(this);
            if ($self.data("edit")) {
                switch ($self.data("uploadtype")) {
                    //圖片上傳
                    case 1:
                        var new_file_list = [];
                        var file_name, file_type;
                        cachedFile.forEach(function (file, index) {
                            file_name = file.name.substring(0, file.name.indexOf(":"));
                            file_type = file.type;
                            new_file_list.push(new File(cachedFile.slice(index, index + 1), file_name, { type: file_type }));
                        })
                        var temp_files = [];
                        file_num--;
                        new_file_list.forEach(function (file, index) {
                            var img_file = [];
                            img_file.push(file);
                            var obj = {};
                            obj["TempId"] = $self.data("tempid") + index;
                            obj["Type"] = $self.data("uploadtype");
                            var reader = new FileReader();
                            reader.readAsDataURL(img_file[0]);
                            reader.onload = (function (e) {
                                obj["Link"] = e.target.result;
                            });
                            htmlImageCompress = new HtmlImageCompress(img_file[0], { quality: 0.7, width: 500, height: 500, imageType: img_file[0].type })
                            htmlImageCompress.then(function (result) {
                                img_file.push(new File([result.file], result.origin.name, { type: result.file.type }));

                                htmlImageCompress = new HtmlImageCompress(img_file[0], { quality: 0.3, width: 150, height: 150, imageType: img_file[0].type })
                                htmlImageCompress.then(function (result) {
                                    img_file.push(new File([result.file], result.origin.name, { type: result.file.type }));
                                    obj["File"] = img_file;
                                    obj["IsDelete"] = false;
                                    obj["Name"] = result.origin.name;
                                    total_files.push(obj);
                                    UploadListAdd(obj);
                                }).catch(function (err) {
                                    UploadPreviewFrameClear();
                                    console.log($`發生錯誤：${err}`);
                                    co.sweet.error("資料上傳失敗", "請重新上傳", null, null);
                                })

                            }).catch(function (err) {
                                UploadPreviewFrameClear();
                                console.log($`發生錯誤：${err}`);
                                co.sweet.error("資料上傳失敗", "請重新上傳", null, null);
                            })
                        })

                        break;
                    //360圖片上傳
                    case 2:
                        var new_file_list = [];
                        var file_name, file_type;
                        cachedFile.forEach(function (file, index) {
                            file_name = file.name.substring(0, file.name.indexOf(":"));
                            file_type = file.type;
                            new_file_list.push(new File(cachedFile.slice(index, index + 1), file_name, { type: file_type }));
                        })

                        new_file_list.forEach(function (file, index) {
                            var img_file = [];
                            img_file.push(file);
                            htmlImageCompress = new HtmlImageCompress(img_file[0], { quality: 0.7 })
                            htmlImageCompress.then(function (result) {
                                img_file.push(new File([result.file], result.origin.name, { type: result.file.type }));

                                htmlImageCompress = new HtmlImageCompress(img_file[0], { quality: 0.3 })
                                htmlImageCompress.then(function (result) {
                                    var obj = {};
                                    img_file.push(new File([result.file], result.origin.name, { type: result.file.type }));
                                    obj["File"] = img_file;
                                    obj["TempId"] = $self.data("tempid");
                                    obj["Name"] = result.origin.name;
                                    obj["Type"] = $self.data("uploadtype");
                                    obj["IsDelete"] = false;
                                    total_files.push(obj);
                                }).catch(function (err) {
                                    console.log($`發生錯誤：${err}`);
                                    UploadPreviewFrameClear();
                                    co.sweet.error("資料上傳失敗", "請重新上傳", null, null);
                                })

                            }).catch(function (err) {
                                console.log($`發生錯誤：${err}`);
                                UploadPreviewFrameClear();
                                co.sweet.error("資料上傳失敗", "請重新上傳", null, null);
                            })
                        })

                        $self.find(".title").text(`${new_file_list[0].name}...共${new_file_list.length}張圖`);
                        break;
                    //影片上傳
                    case 3:
                        var new_file_list = [];
                        var file_name, file_type;
                        cachedFile.forEach(function (file, index) {
                            file_name = file.name.substring(0, file.name.indexOf(":"));
                            file_type = file.type;
                            new_file_list.push(new File(cachedFile.slice(index, index + 1), file_name, { type: file_type }));
                        })

                        var temp_files = [];
                        new_file_list.forEach(function (file, index) {
                            var obj = {};
                            obj["File"] = file;
                            obj["TempId"] = $self.data("tempid") + index;
                            obj["Type"] = $self.data("uploadtype");
                            obj["IsDelete"] = false;
                            obj["Name"] = file.name;
                            total_files.push(obj);
                            temp_files.push(obj)
                        })

                        file_num--;
                        temp_files.forEach(function (file) {
                            UploadListAdd(file);
                        })

                        break;
                }
            }
        })
    })

    $(window).on("fileUploadWithPreview:imageDeleted", function (event) {
        //console.log("fileUploadWithPreview:imageDeleted")
        $("#ProductForm > .data_upload > ul > li").each(function () {
            var $self = $(this);
            if ($self.data("edit")) {
                var cachedFile = upload_file.cachedFileArray;
                if (cachedFile.length < 1) {
                    $self.data("file", "");
                    $self.find(".title").text("");
                } else {
                    var new_file_list = [];
                    cachedFile.forEach(function (file, index) {
                        var file_name = file.name.substring(0, file.name.indexOf(":"));
                        var file_type = file.type;
                        new_file_list.push(new File(cachedFile.slice(index, index + 1), file_name, { type: file_type }));
                    })
                    $self.data("file", new_file_list);
                }
            }
        })
    })

    $(window).on("fileUploadWithPreview:clearButtonClicked", function (event) {
        $("#ProductForm > .data_upload > ul > li").each(function () {
            var $self = $(this);
            if ($self.data("edit")) {
                if ($self.data("serno") < file_num) { SortChange("bigger", $self.data("serno"), file_num); }
                if (typeof ($self.data("id")) != "undefined") {
                    total_files.find(item => item["Id"] == $self.data("id"))["IsDelete"] = true;
                } else if (typeof ($self.data("tempid")) != "undefined") {
                    var tempid = $self.data("tempid");
                    var index = total_files.findIndex(item => item["TempId"] == tempid);
                    total_files.splice(index, 1);
                    total_files.forEach(file => {
                        file["TempId"] = file["TempId"] > tempid ? file["TempId"] - 1 : file["TempId"];
                    })
                }
                UploadPreviewFrameClear();
                $self.remove();
                file_num -= 1;
            }
        })
    })

    $("#BtnConnect").on("click", function (e) {
        e.preventDefault();
        var $self_list;
        $("#ProductForm > .data_upload > ul > .upload_list").each(function () {
            var $temp_self = $(this);
            if ($temp_self.data("edit")) { $self_list = $temp_self; }
        })
        var value = $(this).prev().val();
        var index = value.indexOf("watch?v=") + 8;
        $(".youtube_preview").children("*").remove();
        if (value != "" && index > -1 && value.substring(index) != "") {
            var url = "https://www.youtube.com/embed/" + value.substring(index);
            var iframe_html = `<iframe class="yt_preview w-100 h-100" src="${url}" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>`;
            $(".youtube_preview").append(iframe_html);
            $self_list.find(".title").text(value);
            if (typeof (total_files.find(item => item["Id"] == $self_list.data("id"))) != "undefined") {
                if (total_files.find(item => item["Id"] == $self_list.data("id"))["File"] != value.substring(index)) {
                    total_files.find(item => item["Id"] == $self_list.data("id"))["File"] = value.substring(index)
                }
            } else if (typeof (total_files.find(item => item["TempId"] == $self_list.data("tempid"))) != "undefined") {
                if (total_files.find(item => item["TempId"] == $self_list.data("tempid"))["File"] != value.substring(index)) {
                    total_files.find(item => item["TempId"] == $self_list.data("tempid"))["File"] = value.substring(index)
                }
            } else {
                var obj = {};
                obj["File"] = value.substring(index);
                obj["TempId"] = $self_list.data("tempid");
                obj["Type"] = $self_list.data("uploadtype");
                obj["IsDelete"] = false;
                total_files.push(obj);
            }
        } else {
            var error_html = "<div class='w-100 h-100 d-flex justify-content-center align-items-center bg-black bg-opacity-25 fw-bold'>請輸入正確的Youtube連結</div>"
            $(".youtube_preview").append(error_html);
            $self_list.find(".title").text("");
            $self_list.data("file", "");
        }
    })

    $(function () {
        var drap_sy, drap_ey, drap_itemh;
        $("#ProductForm > .data_upload > ul").sortable({
            items: "> .upload_list",
            axis: "y",
            cursor: "move",
            dropOnEmpty: false,
            start: function (event, ui) {
                drap_sy = ui.item.offset().top;
                drap_itemh = ui.item.height() * 1.5
            },
            stop: function (event, ui) {
                drap_ey = ui.item.offset().top;
                var move = Math.trunc((drap_ey - drap_sy) / drap_itemh);
                var $ser_no = ui.item.find(".ser_no");
                if (move > 0) {
                    $ser_no.val(parseInt($ser_no.val()) + move)
                    SortChange("bigger", ui.item.data("serno"), $ser_no.val())
                    ui.item.data("serno", $ser_no.val())
                } else if (move < 0) {
                    $ser_no.val(parseInt($ser_no.val()) + move)
                    SortChange("smaller", $ser_no.val(), ui.item.data("serno"))
                    ui.item.data("serno", $ser_no.val())
                }
            }
        });
    });

    /* 日期選擇 */
    $picker = $("#InputDate");

    co.Picker.Init($picker);

    $picker.on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format('YYYY/MM/DD HH:mm') + ' ~ ' + picker.endDate.format('YYYY/MM/DD HH:mm'));
        startDate = picker.startDate.format("");
        endDate = picker.endDate.format("");
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
            window.location.hash = "";
        });
    })
    $(".btn_add").on("click", function () {
        window.location.hash = 0;
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
                AddUp("已成功發布", "發布發生未知錯誤", "Canvas");
            } else if (result.isDenied) {
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
            spec_price_num = 0;
        })

        $(".input_price").each(function () {
            var $self = $(this)
            var psid = $self.parents(".frame").data("psid")
            var temppsid = $self.parents(".frame").data("temppsid")
            var index = modal_price_list.findIndex(item => item["FK_PSId"] == psid || (item["TempPSid"] != null && item["TempPSid"] == temppsid))
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

function FormDataClear() {
    TechCertDataClear();
    TagDataClear();
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

    modal_price_list = []
    price_tid = 0;
    temp_psid = 0;

    UploadPreviewFrameClear();
    $("#ProductForm > .data_upload > ul").children(".upload_list").remove();
    file_num = 0;
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
                    SpecAdd(null);
                    MoveToContent();
                }
            } else {
                if (hash.includes('-1')) {
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
    //console.log(result)
    //$("#ProductContent .card-header .titile").append(`編輯商品<span class="d-md-flex d-none">－${result.title}</span>`);

    TagDataSet(result.tagDatas);
    TechCertDataSet(result.techCertDatas);

    result.files.forEach(file => {
        UploadListAdd(file);
    })

    result.stocks.forEach(function (stock) {
        stock.prices.forEach(function (price) {
            var price_obj = {};
            price_obj["Id"] = price.id;
            price_obj["Tempid"] = price_tid;
            price_obj["FK_PSId"] = price.fK_PSId;
            price_obj["TempPSid"] = 0;
            price_obj["FK_RId"] = price.fK_RId
            price_obj["Price"] = price.price
            price_obj["Bonus"] = price.bonus
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
    $btn_display.children("span").text(result.disp_Opt ? "visibility" : "visibility_off");

    $name.val(result.title);
    $name_count.text($name.val().length);
    $introduction.val(result.introduction);
    $introduction_count.text($introduction.val().length);
    $illustrate.val(result.description);
    $illustrate_count.text($illustrate.val().length);

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
        item_role.val(result.FK_RId);
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
        obj["FK_PSId"] = $self.parents(".modal-body").first().data("psid");
        obj["TempPSid"] = $self.parents(".modal-body").first().data("temppsid");
        obj["FK_RId"] = $self.find(".select_role").val();
        obj["Price"] = $self.find(".input_cash").val();
        obj["Bonus"] = $self.find(".input_bonus").val();
        obj["IsDelete"] = false;
        if (obj["Price"] == 0 && obj["Bonus"] == 0) {
            $(".alert_text").text("商品現金與紅利不可同時為空")
            $(".alert_text").removeClass("d-none");
            save_success = false
        } else {
            if (temp_list.find(item => item["FK_RId"] == obj["FK_RId"] && item["Price"] == obj["Price"] && item["Bonus"] == obj["Bonus"]) != null) {
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
                    modal_price_list[index]["FK_RId"] = $self.find(".select_role").val();
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

            var temp_spec_list = spec_pick_list.find(item => item.id == item_select_1.val())
            if (temp_spec_list.specs.length > 0) {
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
            if (temp_spec_list.specs.length > 0) {
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
    } else {
        temp_psid += 1;
        item.data("temppsid", temp_psid);
    }

    var index = modal_price_list.findIndex(mitem => mitem["FK_PSId"] == item.data("psid") || (mitem["TempPSid"] != null && mitem["TempPSid"] == item.data("temppsid")))
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
            if (item.FK_PSId == psid || (item.TempPsid != null && item.TempPsid == temppsid)) {
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

        var $spec_input = $self.siblings(".input_spec");

        $spec_input.blur(function () {
            var $option; var id;
            if ($spec_input.val() != "") {
                id = 0;
                $spec_input.each(function () {
                    $self_input = $(this);
                    $self_input.siblings("datalist").children("option").each(function () {
                        $option = $(this);
                        if ($option.val() == $self_input.val()) {
                            id = $option.data("sid");
                        }
                    })
                })
                if (id == 0) {
                    co.Spec.SpecAddUp({ FK_Tid: $spec_input.prev("select").val(), Title: $spec_input.val(), }).done(function (result) {
                        if (result.success) {
                            co.Spec.GetPickSpecList().done(function (pick_result) {
                                spec_pick_list = pick_result;
                                $self_input.siblings("datalist").append(`<option value="${$spec_input.val()}" data-sid="${result.message}"></option>`)
                            });
                        }
                    });
                }
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

/***************
 uploadtype：
    1 = 圖片;
    2 = 360;
    3 = 影片;
    4 = Youtube;
***************/
function UploadFile($self) {
    UploadPreviewFrameClear();
    var $parent = $self.parents(".data_upload").first();
    if ($self.data("edit")) {
        $self.data("edit", false)
        if ($self.hasClass("upload_list") && $self.find(".title").text() == "") {
            $self.remove();
            file_num -= 1;
        }
    } else {
        if ($self.hasClass("upload_list") && $self.find(".title").text() != "") {
            $(".upload_list").each(function () {
                var $li_self = $(this);
                if ($li_self.hasClass("upload_list") && $li_self.find(".title").text() == "") {
                    $li_self.remove();
                    file_num = $self.siblings(".upload_list").length + 1;
                }
            })
        }
        upload_file = null;
        $parent.find(".upload_frame").children("*").remove();
        $(".upload_list").each(function () {
            $(this).data("edit", false);
        })
        $self.data("edit", true)
        $parent.find(".default_frame").removeClass("d-flex");

        switch ($self.data("uploadtype")) {
            case 0:
                var $select_frame = $parent.find(".select_frame")
                $select_frame.addClass("d-flex");
                $select_frame.find("button").each(function () {
                    $(this).on("click", function (e) {
                        e.preventDefault();
                        if ($self.data("uploadtype") == 0) {
                            $self.data("uploadtype", $(this).data("uploadtype"));
                            $self.data("edit", false);
                            UploadFile($self);
                        }
                    })
                })
                break;
            case 1:
                if ($self.find(".title").text() == "") {
                    upload_file = co.File.UploadImageInit("FileUpload");
                    $parent.find(".upload_frame").removeClass("d-none");
                } else {
                    if (typeof ($self.data("id")) != "undefined") {
                        var name = total_files.find(item => item["Id"] == $self.data("id"))["Name"];
                        var file = total_files.find(item => item["Id"] == $self.data("id"))["File"];
                        $parent.find(".media_frame").addClass("d-flex");
                        $parent.find(".media_frame").find("input").val(name);
                        $parent.find(".media_preview > div").children().remove();
                        $parent.find(".media_preview > div").children().remove();
                        $parent.find(".media_preview > div").append(`<img src="${file}" class=""></img>`);
                    } else if (typeof ($self.data("tempid")) != "undefined") {
                        var data = total_files.find(item => item["TempId"] == $self.data("tempid"));
                        if (typeof (data) != "undefined") {
                            $parent.find(".upload_frame").find("span").text(data["File"].name);
                            $parent.find(".media_frame").find("input").val(data["Name"]);
                            $parent.find(".media_preview > div").children().remove();
                            var link = data["Link"];
                            $parent.find(".media_preview > div").append(`<img src="${link}" class=""></img>`);
                        }
                        $parent.find(".media_frame").addClass("d-flex");
                    }
                }
                break;
            case 2:
                upload_file = co.File.Upload360Init("FileUpload");
                if ($self.data("file")) {
                    upload_file.addFiles($self.data("file"));
                    //console.log(upload_file);
                    $parent.find(".upload_frame").find("span").text($self.data("file").length + " 張圖片已選擇");
                }
                $parent.find(".upload_frame").removeClass("d-none");
                break;
            case 3:
                if ($self.find(".title").text() == "") {
                    upload_file = co.File.UploadVideoInit("FileUpload");
                    $parent.find(".upload_frame").removeClass("d-none");
                } else {
                    if (typeof ($self.data("id")) != "undefined") {
                        var name = total_files.find(item => item["Id"] == $self.data("id"))["Name"];
                        var file = total_files.find(item => item["Id"] == $self.data("id"))["File"];
                        $parent.find(".media_frame").addClass("d-flex");
                        $parent.find(".media_frame").find("input").val(name);
                        $parent.find(".media_preview > div").children().remove();
                        $parent.find(".media_preview > div").append(`<video src="${file}" class="h-100 w-100" controls preload="metadata"></video>`);
                    } else if (typeof ($self.data("tempid")) != "undefined") {
                        var data = total_files.find(item => item["TempId"] == $self.data("tempid"));
                        var file;
                        if (typeof (data) != "undefined") {
                            file = total_files.find(item => item["TempId"] == $self.data("tempid"))["File"];
                            $parent.find(".upload_frame").find("span").text(file.name);
                            $parent.find(".media_frame").find("input").val(total_files.find(item => item["TempId"] == $self.data("tempid"))["Name"]);
                        }
                        $parent.find(".media_frame").addClass("d-flex");
                    }
                }
                break;
            case 4:
                if (typeof (total_files.find(item => item["Id"] == $self.data("id"))) != "undefined") {
                    var file = total_files.find(item => item["Id"] == $self.data("id"))["File"];
                    var url = "https://www.youtube.com/watch?v=" + file;
                    $parent.find(".youtube_frame").find("input").val(url);
                    $("#BtnConnect").click();
                } else if (typeof (total_files.find(item => item["TempId"] == $self.data("tempid"))) != "undefined") {
                    var file = total_files.find(item => item["TempId"] == $self.data("tempid"))["File"];
                    var url = "https://www.youtube.com/watch?v=" + file;
                    $parent.find(".youtube_frame").find("input").val(url);
                    $("#BtnConnect").click();
                } else {
                    $parent.find(".youtube_frame").find("input").val("https://www.youtube.com/watch?v=");
                    var error_html = "<div class='w-100 h-100 d-flex justify-content-center align-items-center bg-black bg-opacity-25 fw-bold'>請輸入正確的Youtube連結</div>"
                    $(".youtube_preview").children("*").remove();
                    $(".youtube_preview").append(error_html);
                }
                $parent.find(".youtube_frame").addClass("d-flex");
                break;
        }
    }
}

function UploadListAdd(result) {
    //console.log("UploadListAdd");
    //console.log(result);
    var item = $($("#TemplateUploadList").html()).clone();
    var item_serno = item.find(".ser_no"),
        item_btn_remove = item.find(".btn_remove");

    if (result == null) {
        $("#ProductForm > .data_upload > ul > li").each(function () {
            var $self = $(this);
            if ($self.hasClass("upload_list") && $self.find(".title").text() == "") {
                $self.remove();
                file_num -= 1;
            }
        })

        file_num += 1;
        item.data("tempid", file_num);
        item.data("serno", file_num);
        item_serno.val(file_num);
        item.data("uploadtype", 0);
        item.data("edit", false);
        item.on("click", function () {
            UploadFile($(this));
        })
    } else if (typeof (result.id) == "undefined") {
        file_num += 1;
        item.data("tempid", result.TempId);
        item.data("serno", file_num);
        item_serno.val(file_num);
        item.data("uploadtype", result.Type);
        item.data("edit", false);
        item.find(".title").text(result.Name);

        item.on("click", function () {
            UploadFile($(this));
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
        total_files.push(obj);

        item.on("click", function () {
            UploadFile($(this));
        })
    }

    item_serno.blur(function () {
        var $self = $(this);
        if ($self.val() < 1) {
            $self.val(1);
        } else if ($self.val() > $(".upload_list").length) {
            $self.val($(".upload_list").length);
        }
        if ($self.val() != item.data("serno")) {
            if ($self.val() > item.data("serno")) {
                SortChange("bigger", item.data("serno"), $self.val())
                $("#ProductForm > .data_upload > ul").children("li").eq(parseInt($self.val()) - 1).after(item);
            } else if ($self.val() < item.data("serno")) {
                SortChange("smaller", $self.val(), item.data("serno"))
                $("#ProductForm > .data_upload > ul").children("li").eq(parseInt($self.val()) - 1).before(item);
            }
        }
        item.data("serno", $self.val());
    })

    item_btn_remove.on("click", function (e) {
        e.preventDefault();
        var $self = $(this).parents("li").first();
        if (item.data("serno") < file_num) { SortChange("bigger", item.data("serno"), file_num); }
        if (typeof ($self.data("id")) != "undefined") {
            total_files.find(item => item["Id"] == $self.data("id"))["IsDelete"] = true;
        } else if (typeof ($self.data("tempid")) != "undefined") {
            var tempid = $self.data("tempid");
            var index = total_files.findIndex(item => item["TempId"] == tempid);
            total_files.splice(index, 1);
            total_files.forEach(file => {
                file["TempId"] = file["TempId"] > tempid ? file["TempId"] - 1 : file["TempId"];
            })
        }
        UploadPreviewFrameClear();
        $self.remove();
        file_num -= 1;
    })

    $("#ProductForm > .data_upload > ul > .btn_upload_add").before(item);

    UploadFile(item);
}

function UploadPreviewFrameClear() {
    var $self = $("#ProductForm > .data_upload > .preview_frame");
    $self.find(".default_frame").addClass("d-flex");
    $self.find(".upload_frame").addClass("d-none");
    $self.find(".media_frame").removeClass("d-flex");
    $self.find(".youtube_frame").removeClass("d-flex");
    $self.find(".select_frame").removeClass("d-flex");
}

function SortChange(change, minindex, maxindex) {
    $(".upload_list").each(function () {
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
    var temp_serno = 1;
    $("#Spec_Frame > .frame").each(function () {
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

        obj["Id"] = $self.data("psid") == "" ? 0 : $self.data("psid");
        obj["FK_S1id"] = fk_sid[0];
        obj["FK_S2id"] = fk_sid[1];
        obj["Stock"] = $self.find(".input_stock_number").val();
        obj["Alert_Qty"] = $self.find(".input_alert_number").val();
        obj["Min_Qty"] = $self.find(".input_min_number").val();
        obj["Ser_No"] = temp_serno;
        temp_serno++;

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
        TagSelected: tag_list,
        TechCertSelected: techcert_list,
        Stocks: stock_addup_list
    }).done(function (result) {
        var pid = parseInt(result.message);
        if (result.success) {
            if (total_files.length > 0) {

                $("#ProductForm > .data_upload > ul > li").each(function () {
                    var $self = $(this);

                    if (!$self.hasClass("btn_upload_add")) {
                        var data = [];
                        total_files.forEach(file => {
                            if ((typeof (file["Id"]) != "undefined" && file["Id"] == $self.data("id")) || (typeof (file["TempId"]) != "undefined" && file["TempId"] == $self.data("tempid"))) {
                                data.push(file);
                            }
                        })

                        switch (data[0]["Type"]) {
                            case 1:
                                if (typeof (data[0]["File"]) == "string") {
                                    co.File.fileSortChange({
                                        Id: data[0]["Id"],
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
                                        co.File.Upload(formData);
                                        formData.delete('files');
                                    })
                                }
                                break;
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
                            case 3:
                                if (typeof (data[0]["File"]) == "string") {
                                    co.File.fileSortChange({
                                        Id: data[0]["Id"],
                                        SerNo: $self.find(".ser_no").val(),
                                    });
                                } else {
                                    var formData = new FormData();
                                    formData.append("files", data[0]["File"]);
                                    formData.append("type", 1);
                                    formData.append("sid", pid);
                                    formData.append("serno", $self.find(".ser_no").val());
                                    co.File.Upload(formData);
                                }
                                break;
                            case 4:
                                var Id = typeof (data[0]["Id"]) == "undefined" ? 0 : data[0]["Id"];
                                co.File.UploadYTLink({
                                    Id: Id,
                                    File: data[0]["File"] + "",
                                    SId: pid,
                                    Type: 1,
                                    SerNo: $self.find(".ser_no").val(),
                                });
                                break;
                        }
                    }
                })

                total_files.forEach(file => {
                    if (typeof (file["IsDelete"]) != "undefined" && file["IsDelete"] == true) {
                        switch (file["Type"]) {
                            case 2:
                                break;
                            case 1:
                            case 3:
                            case 4:
                                if (typeof (file["Id"]) != "undefined") {
                                    var deleteid_list = [];
                                    deleteid_list.add(file["Id"]);
                                    co.File.DeleteFileById({
                                        Sid: parseInt(result.message),
                                        Type: 1,
                                        Fid: deleteid_list,
                                    });
                                }
                                break;
                        }
                    }
                });

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

            }
        } else {
            Coker.sweet.error("錯誤", error_text, null, true);
        }
    }).fail(function () {
        Coker.sweet.error("錯誤", error_text, null, true);
    });

    if (spec_remove_list.length > 0) {
        spec_remove_list.forEach(function (item) {
            co.Product.Delete.Stock(item);
        })
    }
}

function MoveToContent() {
    if (keyId == 0) $("#ProductContent .card-header .titile").text("新增商品")
    else $("#ProductContent .card-header .titile").text("編輯商品")
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