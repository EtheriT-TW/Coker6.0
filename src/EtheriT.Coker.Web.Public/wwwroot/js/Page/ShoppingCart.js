var buy_step_swiper;
var gotop_switch = false, isCheckout = false;

var subtotal, ori_freight, low_con, disfreight, freight, total
var shipping = null, payment = null;

var ShippingForms, PaymentForms, OrdererForms, RecipientForms, InvoiceForms;
var OrdererOpen = false, RecipientOpen = false, InvoiceOpen = false;
var shipMethodsChosen = false, payMethodsChosen = false, OrdererFilled = true, RecipientFilled = true, InvoiceFilled = true;
var $Orderer_TWzipcode, $Recipient_TWzipcode, $Invoice_TWzipcode;
var $orderer_name, $orderer_sex, $orderer_email, $orderer_cellphone, $orderer_telphone_area, $orderer_telphone, $orderer_telphone_ext, $orderer_address_city, $orderer_address_town, $orderer_address;
var $recipient_name, $recipient_sex, $recipient_email, $recipient_cellphone, $recipient_telphone_area, $recipient_telphone, $recipient_telphone_ext, $recipient_address_city, $recipient_address_town, $recipient_address, $remark;
var $invoice_recipient, $invoice_title, $invoice_uniformid, $invoice_address_city, $invoice_address_town, $invoice_address;
var $ship_method, $pay_method;

var order_header_data = {}, user_data = {}, order_data = {}, recipient_data = {}, invoice_data = {}, prod_data = {};
var shopping_cart_data = [];
var hasProds = false;

var islogin = false;

var RecipientsList_dxData;

function PageReady() {
    Coker.Order = {
        AddHeader: function (data) {
            return $.ajax({
                url: "/api/Order/AddHeader",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: {
                    Authorization: 'Bearer ' + localStorage.getItem("token")
                },
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        FrontUserUpdate: function (data) {
            return $.ajax({
                url: "/api/Order/FrontUserUpdate",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: {
                    Authorization: 'Bearer ' + localStorage.getItem("token")
                },
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        GetHeader: function (id) {
            return $.ajax({
                url: "/api/Order/GetHeaderOne/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: {
                    Authorization: 'Bearer ' + localStorage.getItem("token")
                },
                data: { id: id },
            });
        },
        GetDetails: function (id) {
            return $.ajax({
                url: "/api/Order/GetOrderDetails/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: {
                    Authorization: 'Bearer ' + localStorage.getItem("token")
                },
                data: { id: id },
            });
        },
        GetAllData: function (ohid, check) {
            return $.ajax({
                url: "/api/Order/GetOrderDisplay",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: {
                    Authorization: 'Bearer ' + localStorage.getItem("token")
                },
                data: { ohid: ohid, check: check },
            });
        },
        GetReorder: function (ohid) {
            return $.ajax({
                url: "/api/Order/ReorderDisplay",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: {
                    Authorization: 'Bearer ' + localStorage.getItem("token")
                },
                data: { ohid: ohid },
            });
        },
        GetPaymentTypeEnum: function () {
            return $.ajax({
                url: "/api/Order/GetPaymentTypeEnum",
                type: "POST",
            });
        },
        CheckStock: function (data) {
            return $.ajax({
                url: "/api/Order/CheckStock",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: {
                    Authorization: 'Bearer ' + localStorage.getItem("token")
                },
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        Reorder: function (ohid) {
            return $.ajax({
                url: "/api/Order/Reorder/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: {
                    Authorization: 'Bearer ' + localStorage.getItem("token")
                },
                data: { ohid: ohid },
            });
        },
    };

    Coker.Payment = {
        GetPaymentInfo: function (paytypeid) {
            return $.ajax({
                url: "/api/ShoppingCart/GetPaymentInfo/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: {
                    Authorization: 'Bearer ' + localStorage.getItem("token")
                },
                data: { paytypeid: paytypeid },
            });
        }
    }

    $("#btn_car_dropdown").addClass("d-none")

    /* Buy Swiper */
    buy_step_swiper = new Swiper("#BuyStepSwiper > .swiper", {
        a11y: true,
        slidesPerView: 1,
        spaceBetween: 15,
        autoHeight: true,
        loop: false,
        enabled: false,
        pagination: {
            el: ".swiper_pagination > .swiper_pagination_buystep",
            clickable: true,
            renderBullet: function (index, className) {
                return `<span class="${className}">${index + 1}</span>`;
            },
        },
        navigation: {
            nextEl: ".btn_swiper_next_buystep",
            prevEl: ".btn_swiper_prev_buystep",
        }
    });

    buy_step_swiper.on('slideChangeTransitionEnd', function () {
        if (gotop_switch) {
            window.scrollTo(0, $(".swiper").offset().top - $("header").height());
        }
    });

    buy_step_swiper.on('slideChange', function () {
        switch (buy_step_swiper.activeIndex) {
            case 1:
                if (!hasProds) {
                    Coker.sweet.warning("錯誤", "無可購買商品。", null, false);
                    buy_step_swiper.slideTo(0);
                }
                break;
            case 2:
                RadioPayment();
                if (ShippingForms.find(".noshipping").length > 0) {
                    Coker.sweet.warning("請注意", "店家尚未設置運費方式，無法繼續", null);
                    buy_step_swiper.slideTo(1);
                } else if (PaymentForms.find(".nopayment").length > 0) {
                    Coker.sweet.warning("請注意", "店家尚未設置付款方式，無法繼續", null);
                    buy_step_swiper.slideTo(1);
                } else {
                    shipMethodsChosen = FormCheck(ShippingForms);
                    payMethodsChosen = FormCheck(PaymentForms);
                    if (!(shipMethodsChosen && payMethodsChosen)) {
                        Coker.sweet.warning("請注意", "請確實選擇運送及付款方式！", null);
                        setTimeout(function () {
                            buy_step_swiper.slideTo(1);
                        }, 1500);
                    } else {
                        OrdererFilled = FormCheck(OrdererForms);
                        RecipientFilled = FormCheck(RecipientForms);
                        InvoiceFilled = FormCheck(InvoiceForms);
                        if (!OrdererFilled) {
                            if (OrdererOpen) $("#OrdererForm>form").removeClass('was-validated')
                            OrdererEdit(true);
                            $("#radio_recipient_order").trigger("change");
                            $("#radio_bill_orderer").trigger("change");
                        }
                    }
                }
                break;
            case 3:
                if (!isCheckout) {
                    if (OrdererOpen) { OrdererFilled = FormCheck(OrdererForms) };
                    if (RecipientOpen) { RecipientFilled = FormCheck(RecipientForms) };
                    if (InvoiceOpen) { InvoiceFilled = FormCheck(InvoiceForms) };
                    Coker.sweet.warning("未完成結帳流程！", "若資料已確實填寫完畢，請點選下方[確認付款]按鈕進入付款程序", null);
                    setTimeout(function () {
                        buy_step_swiper.slideTo(2);
                    }, 1500);
                }
                break;
        }
    });

    //ECPay.initialize("Stage", 1, function (errMsg) {
    //    console.log("errMsg", errMsg)
    //});

    $('#CollapsePurchase')
        .on('shown.bs.collapse', function () {
            buy_step_swiper.update();
            $("body").css("height", "auto");
            $(window).trigger("resize");
        })
        .on('hidden.bs.collapse', function () {
            buy_step_swiper.update();
            $("body").css("height", "auto");
            $(window).trigger("resize");
        });

    if ("onhashchange" in window) {
        window.onhashchange = hashChange;
    } else {
        setInterval(hashChange, 1000);
    }
    GetOrderPage();

    Coker.Token.CheckToken().done(function (checkresult) {
        islogin = checkresult.isLogin;
        Coker.User.GetUser().done(function (result) {
            if (result.success) {
                var data_insert = true;
                user_data['orderer'] = result.data.name;
                user_data['ordererSex'] = result.data.sex;
                user_data['ordererEmail'] = result.data.email;

                if (result.data.cellPhone == null) data_insert = false;
                user_data['ordererCellPhone'] = result.data.cellPhone;

                if (result.data.telPhone != null) {
                    user_data['zone'] = (result.data.telPhone).split('-')[0];
                    user_data['ordererTelePhone'] = (result.data.telPhone).split('-')[1];
                    user_data['ext'] = (result.data.telPhone).split('-')[2];
                } else {
                    user_data['zone'] = null;
                    user_data['ordererTelePhone'] = null;
                    user_data['ext'] = null;
                }

                if (result.data.address != null) {
                    user_data['county'] = (result.data.address).split(' ')[0];
                    user_data['district'] = (result.data.address).split(' ')[1];
                    user_data['ordererAddress'] = (result.data.address).split(' ')[2];
                } else {
                    data_insert = false;
                    user_data['county'] = null;
                    user_data['district'] = null;
                    user_data['ordererAddress'] = null;
                }
                user_data['address'] = result.data.address;

                if (!data_insert) {
                    OrdererEdit(true);
                    $('#MemberUpdate').prop('checked', true);
                }

                co.Form.insertData(user_data, "#Form_Orderer");

                order_data = user_data;
                order_data.ordererAddress = user_data['address'];
                ShoppingCartDataInsert(order_data, $("#OrdererForm .default_data"));
                RecipientSameOrderer();

                co.Zipcode.setData({
                    el: $("#Orderer_TWzipcode"),
                    addr: order_data.ordererAddress
                });
            } else user_data = null;
        });
    });

    ElementInit();

    $(".btn_call_login").on("click", function (event) {
        loginModal.show();
    })

    /* 根據畫面高度判斷切換Swiper是否滑動到上方 */
    top_position = $(".swiper").offset().top;

    $(window).scroll(function () {
        var topPosition = $(".swiper").offset().top - $("header").height();
        if (document.body.scrollTop > topPosition || document.documentElement.scrollTop > topPosition) {
            gotop_switch = true;
        } else {
            gotop_switch = false;
        }
    });

    /* 鍵盤輸入欄位檢測 */
    document.addEventListener("keyup", AutoSwapInput);

    /* 選按Input與label不拖動到swiper */
    $("input,label,button").mousedown(function (e) {
        buy_step_swiper.allowTouchMove = false;
    })

    $("input,label,button").mouseup(function (e) {
        buy_step_swiper.allowTouchMove = true;
    })

    /* Step2 Form 檢測 */
    ShippingForms = $('#RadioShipping');
    PaymentForms = $('#RadioPayment');

    $(".btn_swiper_next_step3").on("click", Step2Monitor);

    /* Step3 Form 檢測 */
    OrdererForms = $('#OrdererForm > form');
    RecipientForms = $('#RecipientForm > form');
    InvoiceForms = $('#InvoiceForm > form');

    $(".btn_checkout").on("click", Step3Monitor);

    /* Button */
    $(".btn_back_to_check").on("click", function () {
        buy_step_swiper.slideTo(0);
    });

    $(".btn_goprev").on("click", function () {
        buy_step_swiper.slidePrev();
    });

    $(".btn_edit_data").on("click", function () {
        OrdererEdit(null)
    });
    $(".btn_delete_recipient").on("click", DeleteRecipient);

    /* Radio Button */
    $('input[type=radio][name=RadioShipping]').on("change", RadioShipping);
    $('input[type=radio][name=RadioPayment]').on("change", RadioPayment);
    $('input[type=radio][name=RecipientRadio]').on("change", RecipientRadio);
    $('input[type=radio][name=InvoiceRadio]').on("change", InvoiceRadio);

    $(".btn_backshop").each(function () {
        var $this = $(this);
        if ($this.attr("href") == "") $this.attr("title", "繼續購物：返回上一頁");
    })
    $(".btn_backshop").on("click", function (event) {
        var $this = $(this);
        if ($this.attr("href") == "") {
            history.back();
            return false;
        }
    });


    $(".btn_inituser").on("click", function () {
        var oricheck = $('#MemberUpdate').prop('checked');
        co.Form.clear("Form_Orderer");
        $('#MemberUpdate').prop('checked', oricheck);
        if (user_data == null) {
            $('#Form_Orderer .gender input[type="radio"]').prop('checked', false);
            co.Zipcode.setData({
                el: $("#Orderer_TWzipcode"),
                addr: "縣市"
            });
        } else {
            $('#Form_Orderer .gender input[type="radio"]').prop('checked', false);
            var address = user_data.ordererAddress;
            if (address && address.indexOf(" ") > 0) {
                if (address.split(' ').length >= 3) user_data.ordererAddress = address.split(' ')[2];
                else user_data.ordererAddress = "";
            }
            co.Form.insertData(user_data, "#Form_Orderer");
            user_data.ordererAddress = address;
            co.Zipcode.setData({
                el: $("#Orderer_TWzipcode"),
                addr: user_data.address
            });
        }
    })
}
function hashChange(e) {
    if (!!e) {
        e.preventDefault();
        GetOrderPage();
    } else {
        console.log("HashChange錯誤")
    }
}
function GetOrderPage() {
    if ($.isNumeric(window.location.search.substring(1))) {
        isCheckout = true;
        var ohid = parseInt(window.location.search.substring(1));
        Coker.Order.GetAllData(ohid, true).done(function (results) {
            if (results.length > 0) {
                var result = results[0];
                $("#Step4 > .card-header > .order_number").text(window.location.search.substring(1));
                $("#Step4 > .card-body .pruchase_content .order_time").text(`訂單成立時間：${result.orderHeader.creationTime}`);
                switch (result.orderHeader.stateStr) {
                    case "待確認":
                        $("#Step4 > .card-body > .pruchase_content > .status_alert").text("訂單已成立，謝謝您的訂購！");
                        break;
                    case "已付款":
                        $("#Step4 > .card-body > .pruchase_content > .status_alert").text("訂單已成立並完成付款，謝謝您的訂購！");
                        break;
                    case "已取消":
                        $("#Step4 > .card-body > .pruchase_content > .status_alert").text("訂單已取消。");
                        break;
                    case "付款失敗":
                        $("#Step4 > .card-body > .pruchase_content > .status_alert").text("訂單付款失敗！");
                        if ($('.buyagain_text').length > 0 && !IsLogin) {
                            $('.buyagain_text').removeClass("d-none");
                            $('.buyagain_text span').on("click", function () {
                                var ohid = parseInt($("#Step4 .card-header .order_number").text());
                                Coker.Order.Reorder(ohid).done(function (result) {
                                    if (result.success) {
                                        var ohidstr = `000000000${result.message}`.substring(result.message.length);
                                        window.location.href = `/${OrgName}/ShoppingCar?reorder${ohidstr}`;
                                    } else {
                                        Coker.sweet.error("錯誤", result.message)
                                    }
                                });
                            });
                        }
                        break;
                    case "待付款":
                        $("#Step4 > .card-body > .pruchase_content > .status_alert").text("訂單已成立，待商家確認付款資訊，謝謝您的訂購！");
                        break;
                }
                SuccessPageDataInsert(result);
            } else {
                if (islogin) {
                    $("#Step4 > .card-body > .pruchase_content > .status_alert").text("查無訂單資訊或期限已過，請至會員管理歷史訂單中確認");
                } else {
                    $("#Step4 > .card-body > .pruchase_content > .status_alert").text("查無訂單資訊");
                }
            }
            buy_step_swiper.enable();
            buy_step_swiper.slideTo(4);
            buy_step_swiper.disable();
        });
    } else if (window.location.search.substring(1).startsWith("reorder")) {
        var ohid = parseInt(window.location.search.substring("reorder".length + 1));
        Coker.Order.GetReorder(ohid).done(function (result) {
            if (result.success && result.orderHeader != null && result.orderDetails != null) {
                $("#Step1 > .card-body").removeClass("d-none");
                buy_step_swiper.enable();
                $("#Purchase_Null").addClass("d-none");
                CartInit(result.orderDetails)
            } else {
                window.location.href = `/${OrgName}/ShoppingCar`;
            }
        })
    }
}
function SuccessPageDataInsert(data) {
    var header = data.orderHeader;
    var details = data.orderDetails;
    ShoppingCartDataInsert(header, $("#Step4 .card-body"))
    TemplateDataInsert($("#Purchase"), $("#CollapsePurchase"), $("#Template_Purchase_Details"), details)
}
/* 元素初始化 */
function ElementInit() {
    /* TWzipcode 初始化 */
    $Orderer_TWzipcode = $('#Orderer_TWzipcode');
    $Recipient_TWzipcode = $('#Recipient_TWzipcode');
    $Invoice_TWzipcode = $('#Invoice_TWzipcode');
    TWZipCodeInit();

    /* 寄件者資訊 */
    $orderer_name = $("#OrdererInputName");
    $orderer_sex = $("input[name=OrdererRadioGender]");
    $orderer_email = $("#OrdererInputMail");
    $orderer_cellphone = $("#OrdererInputCellPhone");
    $orderer_telphone_area = $("#OrdererInputTelPhoneArea");
    $orderer_telphone = $("#OrdererInputTelPhone");
    $orderer_telphone_ext = $("#OrdererInputTelPhoneExt");
    $orderer_address_city = $Orderer_TWzipcode.children('.county').children("select");
    $orderer_address_town = $Orderer_TWzipcode.children('.district').children("select");
    $orderer_address = $("#OrdererInputAddress");

    /* 收件者資訊 */
    $recipient_radio = $("input[name=RecipientRadio]");
    $recipient_name = $("#RecipientInputName");
    $recipient_sex = $("input[name=RecipientRadioGender]");
    $recipient_email = $("#RecipientInputMail");
    $recipient_cellphone = $("#RecipientInputCellPhone");
    $recipient_telphone_area = $("#RecipientInputTelPhoneArea");
    $recipient_telphone = $("#RecipientInputTelPhone");
    $recipient_telphone_ext = $("#RecipientInputTelPhoneExt");
    $recipient_address_city = $Recipient_TWzipcode.children('.county').children("select");
    $recipient_address_town = $Recipient_TWzipcode.children('.district').children("select");
    $recipient_address = $("#RecipientInputAddress");
    $remark = $("#TextareaRemark");

    /* 發票 */
    $invoice_recipient = $("input[name=InvoiceRadio]");
    $invoice_title = $("#InvoiceInputTitle");
    $invoice_uniformid = $("#InvoiceInputUniformId");
    $invoice_address_city = $Invoice_TWzipcode.children('.county').children("select");
    $invoice_address_town = $Invoice_TWzipcode.children('.district').children("select");
    $invoice_address = $("#InvoiceInputAddress");

    /* 運送、付款方式 */
    $ship_method = $("input[name=RadioShipping]");
    $ship_method.each(function () {
        if ($(this).is(":checked")) {
            shipping = $(this).val();
            ori_freight = $(this).data("freight");
            low_con = $(this).data("lowcon");
            disfreight = $(this).data("disfreight");
            freight = ori_freight
        } else {
            freight = 0;
        }
    })
    $pay_method = $("input[name=RadioPayment]");
}
function CardDataGet() {
    Product.GetAll.Cart().done(function (result) {
        if (result.length > 0) {
            CartInit(result)
        }
    });
}
function CartInit(result) {
    $("#Step1 > .card-body").removeClass("d-none");
    for (var i = 0; i < result.length; i++) {
        CartListAdd(result[i])
    }
    $("#Purchase_Null").addClass("d-none");
    buy_step_swiper.enable();

    var popoverTriggerList = Array.prototype.slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'))
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl)
    })

    buy_step_swiper.update();
    TotalCount();
    PaymentHideShow();
}
function PaymentHideShow() {
    $("#RadioPayment > div").each(function () {
        var $self = $(this);
        var $self_input = $self.find("input");
        $self.removeClass("d-none");
        if (subtotal < $self_input.data("minamount")) $self.addClass("d-none");
        if ($self_input.data("maxamount") != null && $self_input.data("maxamount") != "") {
            if (subtotal > $self_input.data("maxamount")) $self.addClass("d-none");
        }
    })
}
function CartListAdd(data) {
    if (data.quantity > 0) {
        if (shopping_cart_data.find(e => e.Id == data.scId) != null) {
            data.price = shopping_cart_data.find(e => e.Id == data.scId).Price;
        } else {
            var obj = {};
            obj['Id'] = data.scId;
            obj['Price'] = data.price;
            obj['Quantity'] = data.quantity;
            shopping_cart_data.push(obj);
            hasProds = true;
        }
    }

    var max_quantity = data.quantity + data.stock;

    var item_list_ul = $("#Step1 > .card-body > .purchase_list");
    var $template = $($("#Template_Cart_Details").html()).clone();
    $template.data("scId", data.scId);

    $template = CartListInsert($template, data);
    $template.find(".btn_remove_pro").on("click", function () {
        var $self = $(this).parents("li").first();
        Coker.sweet.confirm("確定將商品從購物車移除？", "該商品將會從購物車中移除，且不可復原。", "確認移除", "取消", function () {
            CartDelete($self, $self.data("scId"), "成功移除商品", "移除商品發生未知錯誤")
        });
    });
    $template.find(".btn_count_plus").on("click", function () {
        var $self_bro = $(this).siblings(".pro_quantity");
        if ($self_bro.val() < max_quantity) {
            $self_bro.val(parseInt($self_bro.val()) + 1)
            CartQuantityUpdate($template.find(".pro_subtotal"), data.price, $template.data("scId"), $self_bro.val());
        }
    });
    $template.find(".btn_count_minus").on("click", function () {
        var $self_bro = $(this).siblings(".pro_quantity");
        if ($self_bro.val() > 1) {
            $self_bro.val(parseInt($self_bro.val()) - 1)
            CartQuantityUpdate($template.find(".pro_subtotal"), data.price, $template.data("scId"), $self_bro.val());
        }
    });
    $template.find(".pro_quantity").on("change", function () {
        var $self = $(this);
        if ($self.val() < 1) $self.val(1);
        if ($self.val() > max_quantity) $self.val(max_quantity)
        CartQuantityUpdate($template.find(".pro_subtotal"), data.price, $template.data("scId"), $self.val());
    });

    if ($template.find(".btn_move_to_favorites").length > 0) {
        if (islogin) {
            var $btn_favorites = $template.find(".btn_move_to_favorites");
            $btn_favorites.parent("span").removeClass("d-none");

            Coker.Favorites.Check(data.pId).done(function (check) {
                if (check.success) {
                    $btn_favorites.data("Fid", check.message);
                    $btn_favorites.find("i").addClass("fa-solid")
                    $btn_favorites.find("i").removeClass("fa-regular")
                }
            });

            $btn_favorites.on("click", function () {
                var $self = $(this).find("i");
                if ($self.hasClass("fa-regular")) {
                    Coker.Favorites.Add(data.pId).done(function (favorites) {
                        if (favorites.success) {
                            $btn_favorites.data("Fid", favorites.message);
                            $self.addClass("fa-solid")
                            $self.removeClass("fa-regular")
                            Coker.sweet.success("成功將商品加入收藏", null, true);
                        } else {
                            Coker.sweet.error("商品加入收藏發生錯誤", favorites.message, null, true);
                        }
                    });
                } else {
                    if (typeof ($btn_favorites.data("Fid")) != "undefined" && typeof ($btn_favorites.data("Fid")) != "") {
                        Coker.Favorites.Delete($btn_favorites.data("Fid")).done(function (favorites) {
                            if (favorites.success) {
                                $btn_favorites.data("Fid", "");
                                $self.addClass("fa-regular")
                                $self.removeClass("fa-solid")
                                Coker.sweet.success("已將商品從收藏中移除", null, true);
                            } else {
                                Coker.sweet.error("商品移除收藏發生錯誤", favorites.message, null, true);
                            }
                        });
                    }
                }
            })
        }
    }

    item_list_ul.append($template);
}
function CartListInsert($frame, data) {
    $frame.find("*").each(function () {
        var $self = $(this);
        if (typeof ($self.data("key")) != "undefined") {
            var key = $self.data("key");
            switch (key) {
                case "title":
                    if (data[key] != data['oldTitle'] && data['oldTitle'] != null) $self.addClass("text-danger");
                    $self.text(data[key]);
                    break;
                case "link":
                    $self.attr({
                        href: `/${OrgName}/home/product/${data['pId']}`,
                        title: `連結至：${data['title']}(另開新視窗)`
                    });
                    break;
                case "spec":
                    $self.append(data['s1Title'] == "" ? "" : `<span class="border px-1 me-1">${data['s1Title']}</span>`)
                    $self.append(data['s2Title'] == "" ? "" : `<span class="border px-1 me-1">${data['s2Title']}</span>`)
                    break;
                case "imagePath":
                    data[key] = data[key].replaceAll(`/${OrgName}/`, '/');
                    $self.attr({
                        src: data[key],
                        alt: `${data['title']}的圖片`
                    });
                    break;
                case "oldQuantity":
                    if (data[key] != data['quantity']) $self.removeClass("d-none");
                    $self.text(data[key]);
                    break;
                case "oldPrice":
                    if (data[key] != data['price'] && data[key] > 0) {
                        $self.removeClass("d-none");
                        $self.text(data[key]);
                        $self.siblings("div[data-key='price']").addClass("red_text");
                    }
                    break;
                case "subtotal":
                    $self.text(data['price'] * data['quantity']);
                    $self.data("subtotal", data['price'] * data['quantity'])
                    CartQuantityUpdate($self, data['price'], $frame.data("scId"), data['quantity']);
                    break;
                case "quantity":
                    $self.val(data[key]);
                    break;
                default:
                    $self.text(data[key]);
                    break;
            }

            if (data['quantity'] == 0) {
                $frame.find(".nostock").removeClass("d-none");
                $frame.find(".content").addClass("d-none");
                $frame.find(".btn_side_icon").addClass("d-none");
            }

            var type = $self.data("type");
            switch (type) {
                case "price":
                    $self.text(parseInt($self.text()).toLocaleString())
                    break;
            };
        }
    });
    return $frame;
}
function CartQuantityUpdate(self, price, scid, quantity) {
    if (quantity > 0) {
        Product.Update.Cart({
            Id: scid,
            Quantity: quantity,
        }).done(function (result) {
            if (result.success) {
                Product.GetOne.Cart(result.message).done(function (result) {
                    shopping_cart_data.find(e => e.Id == result.scId).Price = price;
                    shopping_cart_data.find(e => e.Id == result.scId).Quantity = quantity;
                    self.data("subtotal", price * quantity)
                    self.text((price * quantity).toLocaleString())
                    TotalCount();
                    CartDropReset(scid, quantity)

                });
            } else {
                if (result.error == "商品庫存不足") {
                    Coker.sweet.warning(result.error, result.message, function () {
                        location.reload(true);
                    }, false);
                } else {
                    Coker.sweet.error("商品更改數量發生錯誤", result.message, null, true);
                }
            }
        }).fail(function () {
            Coker.sweet.error("錯誤", "商品數量修改發生錯誤", null, true);
        });
    }
}
function TotalCount() {
    subtotal = 0;
    $('.purchase_list').children("li").each(function () {
        subtotal += $(this).children(".content").children(".pro_subtotal").data("subtotal");
        if (subtotal > low_con) {
            freight = disfreight;
        } else {
            freight = ori_freight;
        }
    })
    $(".subtotal").each(function () {
        $(this).text(subtotal.toLocaleString())
    })
    $(".shipping_fee").each(function () {
        $(this).text(((freight == null || freight == "") || freight == "") ? 0 : freight.toLocaleString())
    })
    $(".total_amount").each(function () {
        total = (freight == null || freight == "") ? subtotal : subtotal + freight;
        $(this).text(parseInt(total).toLocaleString())
    })
    PaymentHideShow()
}
function CartDelete(self, id, success, error) {
    self.remove();
    Product.Delete.Cart(id).done(function () {
        Coker.sweet.success(success, null, true);
        var index = shopping_cart_data.findIndex(e => e.Id == id);
        if (index !== -1) {
            shopping_cart_data.splice(index, 1);
        }
        CartDropReset(id, 0)
        TotalCount();
        if (parseInt($("#Car_Badge").text()) == 0) {
            DetailsClear();
        }
    }).fail(function () {
        Coker.sweet.error("錯誤", error, null, true);
    })
}
function Step2Monitor() {
    shipMethodsChosen = FormCheck(ShippingForms);
    payMethodsChosen = FormCheck(PaymentForms);

    if (!(shipMethodsChosen && payMethodsChosen)) {
        Coker.sweet.warning("請注意", "請確實選擇運送及付款方式！", null);
    } else {
        buy_step_swiper.slideNext();
    }

    buy_step_swiper.update();
}
function RadioShipping() {
    ori_freight = $(this).data("freight");
    low_con = $(this).data("lowcon");
    disfreight = $(this).data("disfreight");
    freight = ori_freight
    TotalCount();
}
function RadioPayment() {
    var $pay_text = $(".payment_method");
    $pay_text.addClass("fs-2 fw-bold px-3");
    $pay_method.each(function () {
        var $this = $(this);
        if ($this.is(":checked")) {
            var val = $this.val();
            if (val == 1) {
                $(".pay_info").removeClass("d-none");
            } else {
                $(".pay_info").addClass("d-none");
            }
            $pay_text.text($this.siblings("label").text());
        }
    })
    buy_step_swiper.update();
}
function Step3Monitor() {

    OrdererFilled = FormCheck(OrdererForms)

    if (RecipientOpen) {
        RecipientFilled = FormCheck(RecipientForms);
    } else {
        switch ($(`[name="RecipientRadio"]:checked`).val()) {
            case "order":
                RecipientSameOrderer();
                RecipientFilled = true;
                break;
        }
    }
    if (InvoiceOpen) {
        InvoiceFilled = FormCheck(InvoiceForms)
    } else {
        switch ($(`[name="InvoiceRadio"]:checked`).val()) {
            case "order":
            case "recipient":
                InvoiceFilled = true;
                break;
        }
    }

    if (!OrdererFilled) {
        if (!OrdererOpen) OrdererEdit(true);
        Coker.sweet.warning("請注意", "請確實填寫訂購人資料！", null);
    } else if (!RecipientFilled) {
        Coker.sweet.warning("請注意", "請確實填寫收件人資料！", null);
    } else if (!InvoiceFilled) {
        Coker.sweet.warning("請注意", "請確實填寫發票寄送資料！", null);
    } else {
        Coker.sweet.confirm("是否確定結帳？", "點選確認進入付款流程", "是，開始付款", "否", function () {
            OrderHeaderAdd();
        });
    }

    buy_step_swiper.update();
}
function OrdererEdit(isopen) {
    if (isopen == null) {
        isopen = !OrdererOpen;
        OrdererEdit(isopen)
    }

    if (!OrdererOpen && isopen) {
        $("#OrdererForm > .default_data").addClass("d-none");
        $("#OrdererForm > form").removeClass("d-none");
        OrdererOpen = true;
        OrdererFilled = false;
    } else if (OrdererOpen && !isopen) {
        var data = co.Form.getJson($("#Form_Orderer").attr("id"), $("#OrdererForm .default_data"));
        data.ordererAddress = `${data.county}${data.district}${data.ordererAddress}`;
        ShoppingCartDataInsert(data, $("#OrdererForm .default_data"));
        $("#OrdererForm > .default_data").removeClass("d-none");
        $("#OrdererForm > form").addClass("d-none");
        OrdererOpen = false;
        OrdererFilled = true;
    }
    buy_step_swiper.update();
}
function RecipientRadio() {
    var $self = $(this)
    recipient_data = {};
    if ($self.val() == "edit") {
        $("#RecipientForm > .default_data").addClass("d-none");
        $("#RecipientForm > form").removeClass("d-none");
        RecipientOpen = true;
        RecipientFilled = false;
        RecipientFormClear();
    } else if ($self.val() == "order") {
        $("#RecipientForm > .default_data").addClass("d-none");
        $("#RecipientForm > form").addClass("d-none");
        RecipientOpen = false;
        RecipientFilled = true;
        RecipientSameOrderer();
    }
    else {
        $("#RecipientForm > .default_data").addClass("d-none");
        $("#RecipientForm > form").addClass("d-none");
        RecipientOpen = false;
        RecipientFilled = true;
    }
    buy_step_swiper.update();
}
function RecipientFormClear() {
    $recipient_name.val("");
    $recipient_sex.val("");
    $recipient_sex.each(function () {
        $(this).removeAttr("checked");
    })
    $recipient_email.val("");
    $recipient_cellphone.val("");
    $recipient_telphone_area.val("");
    $recipient_telphone.val("");
    $recipient_telphone_ext.val("");
    $recipient_address_city.val("");
    $recipient_address_town.val("");
    $recipient_address.val("");
    $remark.val("");
}
function RecipientSameOrderer() {
    for (var key in order_data) {
        if (key.startsWith("orderer") > 0) recipient_data[key.replace("orderer", "recipient")] = order_data[key]
    }
}
function InvoiceRadio() {
    invoice_data = {}
    switch (this.value) {
        case "order":
            $("#InvoiceForm > .default_data").addClass("d-none");
            $("#InvoiceForm > form").addClass("d-none");
            InvoiceOpen = false;
            InvoiceFilled = true;
            break;
        case "recipient":
            $("#InvoiceForm > .default_data").addClass("d-none");
            $("#InvoiceForm > form").addClass("d-none");
            InvoiceOpen = false;
            InvoiceFilled = true;
            break;
        case "company":
            $("#InvoiceForm > .default_data").addClass("d-none");
            $("#InvoiceForm > form").removeClass("d-none");
            InvoiceOpen = true;
            InvoiceFilled = false;
            InvoiceFormClear();
            break;
    }
    buy_step_swiper.update();
}
function InvoiceFormClear() {
    $invoice_title.val("");
    $invoice_uniformid.val("");
    $invoice_address_city.val("");
    $invoice_address_town.val("");
    $invoice_address.val("");
}
function InvoiceFormSet(title, uniformid, address_city, address_town, address) {
    $invoice_title.val(title);
    $invoice_uniformid.val(uniformid);
    $Invoice_TWzipcode.twzipcode('set', {
        'county': address_city,
        'district': address_town,
    });
    $invoice_address.val(address);
}
/* 表單驗證 */
function FormCheck(Forms) {
    var Check = false;
    Array.from(Forms).forEach(form => {
        if (form.checkValidity()) {
            Check = true;
        }
        form.classList.add('was-validated')
    })
    return Check;
}
function DetailsClear() {
    $("#Step1 > .card-body").addClass("d-none");
    $("#Purchase_Null").removeClass("d-none");
    buy_step_swiper.disable();
}
function DeleteRecipient() {
    var $this_parent = $(this).parents("tr");
    $this_parent.remove();
}
function OrderHeaderAdd() {

    Coker.Order.CheckStock(shopping_cart_data).done(function (result) {
        if (result.success) {
            var checksuccess = true;

            order_data = co.Form.getJson($("#Form_Orderer").attr("id"));
            order_data.ordererAddress = `${order_data.county} ${order_data.district} ${order_data.ordererAddress}`;

            if (order_data.zone == "" ^ order_data.ordererTelePhone == "") {
                Coker.sweet.warning("資料填寫錯誤", "如要提供訂購人電話資訊，請確實填寫區碼與聯絡電話。", null);
                checksuccess = false;
            } else if (order_data.zone == "" && order_data.ordererTelePhone == "") {
                order_data.ordererTelePhone = null;
            } else {
                order_data.ordererTelePhone = `${order_data.zone}-${order_data.ordererTelePhone}` + (order_data.ext == "" ? "" : `-${order_data.ext}`);
            }

            for (var key in order_data) {
                if (key.startsWith("orderer") > 0) order_header_data[key] = order_data[key]
            }

            switch ($(`[name="RecipientRadio"]:checked`).val()) {
                case "order":
                    RecipientSameOrderer();
                    break;
                case "edit":
                    recipient_data = co.Form.getJson($("#Form_Recipient").attr("id"));
                    recipient_data.recipientAddress = `${recipient_data.county} ${recipient_data.district} ${recipient_data.recipientAddress}`;

                    if (recipient_data.zone == "" ^ recipient_data.recipientTelePhone == "") {
                        Coker.sweet.warning("資料填寫錯誤", "如要提供收件人電話資訊，請確實填寫區碼與聯絡電話。", null);
                        checksuccess = false;
                    } else if (recipient_data.zone == "" && recipient_data.recipientTelePhone == "") {
                        recipient_data.recipientTelePhone = null;
                    } else {
                        recipient_data.recipientTelePhone = `${recipient_data.zone}-${recipient_data.recipientTelePhone}` + (recipient_data.ext == "" ? "" : `-${recipient_data.ext}`);
                    }
                    break;
            }

            for (var key in recipient_data) {
                if (key.startsWith("recipient") > 0) order_header_data[key] = recipient_data[key]
            }
            if (typeof (recipient_data['remark']) == "undefined" || recipient_data['remark'] == "") {
                order_header_data['remark'] = "無";
            } else {
                order_header_data['remark'] = recipient_data['remark']
            }

            switch ($(`[name="InvoiceRadio"]:checked`).val()) {
                case "order":
                    invoice_data['invoiceRecipient'] = 1;
                    invoice_data['invoiceAddress'] = order_data['ordererAddress'];
                    break;
                case "recipient":
                    invoice_data['invoiceRecipient'] = 2;
                    invoice_data['invoiceAddress'] = recipient_data['recipientAddress'];
                    break;
                case "company":
                    invoice_data = co.Form.getJson($("#Form_Invoice").attr("id"));
                    invoice_data.invoiceAddress = `${invoice_data.county} ${invoice_data.district} ${invoice_data.invoiceAddress}`;
                    invoice_data['invoiceRecipient'] = 3;
                    order_header_data.uniformId = invoice_data.uniformId;
                    break;
            }
            for (var key in invoice_data) {
                if (key.startsWith("invoice") > 0) order_header_data[key] = invoice_data[key]
            }

            order_header_data.shipping = $(`[name="RadioShipping"]:checked`).val();
            order_header_data.payment = $(`[name="RadioPayment"]:checked`).val();
            order_header_data.state = 1;
            order_header_data.subtotal = subtotal;
            order_header_data.discount = 0;
            order_header_data.bonus = 0;
            order_header_data.couponId = 0;
            order_header_data.freight = freight == "" ? 0 : freight;
            order_header_data.Service_Charge = 0;
            order_header_data.OrderDetails = shopping_cart_data;

            if (checksuccess) {
                var memberUpdateFailMessage = "";
                if ($(".memberUpdate").length > 0 ? $("#MemberUpdate").is(":checked") : false) {
                    Coker.Order.FrontUserUpdate(order_header_data).done(function (result) {
                        if (!result.success) {
                            memberUpdateFailMessage = `<br/>${result.message}`;
                        }
                    });
                }

                Coker.sweet.loading();
                Coker.Order.AddHeader(order_header_data).done(function (result) {
                    if (result.success) {
                        Coker.sweet.success(`謝謝您的訂購！${memberUpdateFailMessage}<br />訂單處理中，若有錯誤請修正後重送訂單。請勿按[回上頁]按鈕，以免重複下單，或發生其他不可預期的錯誤！`, function () {
                            OrderSuccess(result);
                            setTimeout(function () {
                                isCheckout = true;
                                var paymenttype = result.message.split(",")[0];
                                switch (paymenttype) {
                                    case "LinePay":
                                    case "PCHomePay":
                                        //case "ECPay":
                                        Coker.sweet.loading();
                                        Coker.ThirdParty.Request(result.message.split(",")[1], paymenttype).done(function (result) {
                                            Swal.close();
                                            if (result.success) {
                                                localStorage.setItem("lastSaveTime", new Date().toISOString())
                                                localStorage.setItem("lastSaveToken", localStorage.getItem("token"));
                                                $("#Step4 > .card-body > .pruchase_content > .status_alert").text("訂單已成立，即將進入付款流程。");
                                                setTimeout(function () {
                                                    buy_step_swiper.slideNext();
                                                    buy_step_swiper.disable();
                                                }, 300);
                                                $("#Step4 > .card-body .thirdpay_link a").attr("href", result.message);
                                                $("#Step4 > .card-body .thirdpay_link").removeClass("d-none");
                                                window.open(result.message, "_blank");
                                            } else {
                                                $("#Step4 > .card-body > .pruchase_content > .status_alert").text("付款流程發生未知錯誤，請稍後重新嘗試，或直接聯繫客服人員。");
                                                setTimeout(function () {
                                                    buy_step_swiper.slideNext();
                                                    buy_step_swiper.disable();
                                                }, 300);
                                            }
                                        });
                                        break;
                                    case "Default":
                                        setTimeout(function () {
                                            buy_step_swiper.slideNext();
                                            buy_step_swiper.disable();
                                        }, 300);
                                        break;
                                }
                            }, 300);
                        })
                    } else {
                        console.log(result);
                        Coker.sweet.error("錯誤", result.error, null, true);
                    }
                }).fail(function (result) {
                    console.log(result);
                    Coker.sweet.error("錯誤", result.error, null, true);
                });
            }
        } else {
            Coker.sweet.error("錯誤", result.message, null, false);
            $("#Step1 > .card-body > .purchase_list > li").remove();
            CardDataGet();
            buy_step_swiper.slideTo(0);
        }
    });
}
function OrderSuccess(result) {
    var message = result.message.split(",")
    var order_header_id = message[1];

    CartClear();

    $("#Step4 > .card-header > .order_number").text(("000000000" + order_header_id).substring(order_header_id.length));
    $("#Step4 > .card-body .pruchase_content .order_time").text(`訂單成立時間：${message[2]}`);

    $("#Step4 > .card-body > .pruchase_content > .status_alert").text("訂單已成立，謝謝您的訂購！");

    if ($(".storememo").text() != "") {
        Swal.fire({
            title: "小提醒",
            icon: "info",
            html: $(".storememo").text().replaceAll("\n", "<br/>"),
            focusConfirm: false,
            confirmButtonText: "確認",
        }).then((confirm) => {
            if (result.error != null) {
                if (!islogin) {
                    Coker.sweet.warning("信件發送失敗", "訂購信件發送失敗，請註冊會員以查看詳細訂單，或將訂單完成頁面截圖。", null)
                } else {
                    Coker.sweet.warning("信件發送失敗", "訂購信件發送失敗，訂單詳細可於會員管理歷史訂單中查看。", null)
                }
            }
        });
    } else {
        if (result.error != null) {
            if (!islogin) {
                Coker.sweet.warning("信件發送失敗", "訂購信件發送失敗，請註冊會員以查看詳細訂單，或將訂單完成頁面截圖。", null)
            } else {
                Coker.sweet.warning("信件發送失敗", "訂購信件發送失敗，訂單詳細可於會員管理歷史訂單中查看。", null)
            }
        }
    }

    $(".storememo").empty();

    ShoppingCartDataInsert(order_data, $("#Step4 .orderer_data"));
    //HiddenCode($("#Step4 .orderer_data"))
    ShoppingCartDataInsert(recipient_data, $("#Step4 .recipient_data"));
    //HiddenCode($("#Step4 .recipient_data"))
    switch (invoice_data.invoiceRecipient) {
        case 1:
            ShoppingCartDataInsert(order_data, $("#Step4 .invoice_data .orderer"));
            //HiddenCode($("#Step4 .invoice_data .orderer"))
            $("#Step4 .invoice_data .orderer").removeClass("d-none");
            break;
        case 2:
            ShoppingCartDataInsert(recipient_data, $("#Step4 .invoice_data .recipient"));
            //HiddenCode($("#Step4 .invoice_data .recipient"))
            $("#Step4 .invoice_data .recipient").removeClass("d-none");
            break;
        case 3:
            ShoppingCartDataInsert(invoice_data, $("#Step4 .invoice_data .company"));
            //HiddenCode($("#Step4 .invoice_data .company"))
            $("#Step4 .invoice_data .company").removeClass("d-none");
            break;
    }

    $("#PaymentData .pay_info .paid_date").append(message[3]);
    var tempmail = order_header_data.ordererEmail;
    $("#PaymentData .pay_mail").append(`如因交易條件有誤、商品缺貨或價格物刊或有其他本公司無法接受訂單之情形,本公司保留商品出貨與否的權利。<br />．隨後我們也會將轉帳的資料mail到您指定的電子信箱:${tempmail.substr(0, 1)}******${tempmail.substr(tempmail.indexOf("@") - 1)}`);

    Coker.Order.GetDetails(order_header_id).done(function (message) {
        if (message.length > 0) {
            if (message.length > 1) {
                $(".btn_view_list").removeClass("d-none")
            }
            for (var i = 0; i < message.length; i++) {
                if (i == 0) {
                    PurchaseAdd(message[i], $("#Step4 > .card-body > .pruchase_content > .purchase_list").first())
                } else {
                    PurchaseAdd(message[i], $("#Step4 > .card-body > .pruchase_content > .purchase_list.collapse"))
                }
            }
        }
    })

    Coker.Payment.GetPaymentInfo(order_header_data.payment).done(function (message) {
        //console.log(message)
        if (message != null && message.length > 0) {
            var html = "";
            $.each(message, function (index, value) {
                html += `<div class="mb-2 row">
                                        <div class="col-auto col-sm-2 py-0 text-end">${value.title}：</div>
                                        <div class="col ps-0">${value.value}</div>
                                    </div>`;
            })
            $(".pay_info > div").prepend(html);
        }
    });
}
function PurchaseAdd(result, item_list_ul) {
    var item = $($("#Template_Purchase_Details").html()).clone();
    var item_link = item.find(".pro_link"),
        item_image = item.find(".pro_image"),
        item_name = item.find(".pro_name"),
        item_specification = item.find(".pro_specification"),
        item_unit = item.find(".pro_unit"),
        item_quantity = item.find(".pro_quantity"),
        item_subtotal = item.find(".pro_subtotal");

    item_link.attr("href", `/${OrgName}/Home/product/` + result.pId);
    item_link.attr("title", `連結至：${result.title}(另開新視窗)`);
    item_image.attr("src", result.imagePath.replace(`upload/${OrgName}/`, "upload/"));
    item_name.text(result.title);
    item_specification.append(result.s1Title == "" ? "" : '<span class="border px-1 me-1">' + result.s1Title + '</span>')
    item_specification.append(result.s2Title == "" ? "" : '<span class="border px-1">' + result.s2Title + '</span>')
    item_unit.text(`$${(result.price).toLocaleString('en-US')}`)
    item_quantity.text(result.quantity);
    item_subtotal.text((result.price * result.quantity).toLocaleString('en-US'))

    item_list_ul.append(item);
}
function HiddenCode($self) {
    $self.find("*").each(function () {
        var $this = $(this);
        var key = $this.data("key");
        if (typeof ($this.data("key")) != "undefined") {
            switch ($this.data("type")) {
                case "name":
                    var name = $this.text();
                    $this.text(`${name.substr(0, 1)}○${name.substr(name.length - 1)}`)
                    break;
                case "email":
                    var email = $this.text();
                    $this.text(`${email.substr(0, 3)}**********`)
                    break;
                case "phone":
                    var phone = $this.text();
                    $this.text(`${phone.substr(0, 3)}****${phone.substr(phone.length - 3)}`)
                    break;
                case "address":
                    var address = $this.text();
                    address = address.split(' ')[0] + address.split(' ')[1] + address.split(' ')[2];
                    $this.text(`${address.substr(0, 9)}*****`)
                    break;
                case "uniformId":
                    var uniformId = $this.text();
                    $this.text(`${uniformId.substr(0, 3)}*****`)
                    break;
            }
        }
    });
}
function ShoppingCartDataInsert(data, $self) {
    ShoppingCartDataClear($self);
    if (typeof (data.invoiceRecipient) != "undefined") {
        switch (parseInt(data.invoiceRecipient)) {
            case 1:
                $(".invoice_data .orderer").removeClass("d-none");
                break;
            case 2:
                $(".invoice_data .recipient").removeClass("d-none");
                break;
            case 3:
                $(".invoice_data .company").removeClass("d-none");
                break;
        }
    }
    $self.find("*").each(function () {
        var $this = $(this);
        var key = $this.data("key");
        if (typeof ($this.data("key")) != "undefined") {
            if ($this.hasClass("price")) {
                $this.text(data[key].toLocaleString());
            }
            else $this.text(data[key]);
        }
    });
}
function ShoppingCartDataClear($self) {
    $self.find("*").each(function () {
        var $this = $(this);
        var key = $this.data("key");
        if (typeof ($this.data("key")) != "undefined") {
            $this.text("");
        }
    });
}
function TemplateDataInsert($Frame, $CollapseFrame, $Template, datas) {
    $.each(datas, function (index, data) {
        var $html = $($Template.html()).clone();
        $html.find("*").each(function () {
            var $this = $(this);
            var key = $this.data("key");
            if (typeof ($this.data("key")) != "undefined") {
                switch (key) {
                    case "link":
                        $this.attr({
                            href: `/${OrgName}/Home/product/${data['prodId']}`,
                            title: `連結至：${data['title']}(另開新視窗)`
                        });
                        break;
                    case "imagePath":
                        data[key] = data[key].replace(`/${OrgName}/`, '/');
                        $this.attr({
                            src: data[key],
                            alt: data['title']
                        });
                        break;
                    case "spec":
                        $this.append(data['s1Title'] == "" ? "" : `<span class="border px-1 me-1">${data['s1Title']}</span>`)
                        $this.append(data['s2Title'] == "" ? "" : `<span class="border px-1 me-1">${data['s2Title']}</span>`)
                        break;
                    default:
                        if ($this.hasClass("price") && !$this.hasClass("pro_unit")) {
                            $this.text(data[key].toLocaleString());
                        }
                        else $this.text(data[key]);
                        break;
                }
            }
        });
        if (index == 0) {
            $Frame.append($html);
        } else {
            $(".btn_view_list").removeClass("d-none")
            $CollapseFrame.append($html);
        }
    });
}
/* Input輸入自動切換 */
function AutoSwapInput() {
    var target = event.target

    if (target.nodeName == "INPUT" && target.className.indexOf("pro_quantity") < 0) {
        if (target.value.length == target.maxLength) {
            var elements = $(target).parents("form").first().find("input");
            for (let i = 0; i < elements.length; i++) {
                if (elements[i] == target) {
                    if (elements[i + 1]) {
                        elements[i + 1].focus();
                    }
                    return;
                }
            }
        }
    }
}
/* 地址選單初始化 */
function TWZipCodeInit() {
    //$Orderer_TWzipcode.twzipcode({
    //    'zipcodeIntoDistrict': true,
    //    'countySel': '高雄市',
    //    'districtSel': '前鎮區'
    //});
    $Orderer_TWzipcode.twzipcode({ 'zipcodeIntoDistrict': true });
    $Recipient_TWzipcode.twzipcode({ 'zipcodeIntoDistrict': true });
    $Invoice_TWzipcode.twzipcode({ 'zipcodeIntoDistrict': true });

    var $county, $district;

    $county = $Orderer_TWzipcode.children('.county');
    $district = $Orderer_TWzipcode.children('.district');

    $county.children('select').attr({
        id: "OrdererSelectCity",
        class: "orderer_city form-select",
        required: "required"
    });
    $county.append("<label class='px-4 required' for='OrdererSelectCity'>縣市</label>");
    var $county_first_option = $county.children('select').children('option').first();
    $county_first_option.text("請選擇縣市");
    $county_first_option.attr('disabled', 'disabled');

    $district.children('select').attr({
        id: "OrdererSelectTown",
        class: "orderer_town form-select",
        required: "required"
    });
    $district.append("<label class='px-4 required' for='OrdererSelectCity'>鄉鎮</label>");
    var $district_first_option = $district.children('select').children('option').first();
    $district_first_option.text("請選擇鄉鎮");
    $district_first_option.attr('disabled', 'disabled');

    $county = $Recipient_TWzipcode.children('.county');
    $district = $Recipient_TWzipcode.children('.district');

    $county.children('select').attr({
        id: "RecipientSelectCity",
        class: "recipient_city form-select",
        required: "required"
    });
    $county.append("<label class='px-4 required' for='RecipientSelectCity'>縣市</label>");
    var $county_first_option = $county.children('select').children('option').first();
    $county_first_option.text("請選擇縣市");
    $county_first_option.attr('disabled', 'disabled');

    $district.children('select').attr({
        id: "RecipientSelectTown",
        class: "recipient_town form-select",
        required: "required"
    });
    $district.append("<label class='px-4 required' for='RecipientSelectCity'>鄉鎮</label>");
    var $district_first_option = $district.children('select').children('option').first();
    $district_first_option.text("請選擇鄉鎮");
    $district_first_option.attr('disabled', 'disabled');


    $county = $Invoice_TWzipcode.children('.county');
    $district = $Invoice_TWzipcode.children('.district');

    $county.children('select').attr({
        id: "InvoiceSelectCity",
        class: "bill_city form-select",
        required: "required"
    });
    $county.append("<label class='px-4 required' for='InvoiceSelectCity'>縣市</label>");
    var $county_first_option = $county.children('select').children('option').first();
    $county_first_option.text("請選擇縣市");
    $county_first_option.attr('disabled', 'disabled');

    $district.children('select').attr({
        id: "InvoiceSelectTown",
        class: "bill_town form-select",
        required: "required"
    });
    $district.append("<label class='px-4 required' for='InvoiceSelectCity'>鄉鎮</label>");
    var $district_first_option = $district.children('select').children('option').first();
    $district_first_option.text("請選擇鄉鎮");
    $district_first_option.attr('disabled', 'disabled');
}
function RecipientsList_ContentReady(e) {
    RecipientsList_dxData = $("#RecipientsList").dxDataGrid("instance");
    console.log("RecipientsList_dxData", RecipientsList_dxData)
}

function RecipientsList_SelectChange(selectedItems) {
    var data = selectedItems.selectedRowsData;

    console.log("Select", data)
}

function RecipientsList_DeleteButtonClicked(e) {
    console.log(e.row.key)
    co.sweet.confirm("刪除收件人", "確定刪除？資料刪除後不可復原", "確　定", "取　消", function () {
        //co.Tag.TagDelete(e.row.key).done(function () {
        //    RecipientsList_dxData.refresh();
        //})
    })
}