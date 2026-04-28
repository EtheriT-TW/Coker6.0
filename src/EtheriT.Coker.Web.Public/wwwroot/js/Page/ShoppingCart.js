var buy_step_swiper, ECPayModal;
var gotop_switch = false, isCheckout = false;

var subtotal, ori_freight, low_con, disfreight, freight, total;
var freightType = 0, discountFreightType = null, boxFees = [];
var shipping = null, payment = null;

var ShippingForms, PaymentForms, OrdererForms, RecipientForms, InvoiceForms, InvoicePersonalTypeForms;
var OrdererOpen = false, RecipientOpen = false, InvoiceOpen = false;
var shipMethodsChosen = false, payMethodsChosen = false, OrdererFilled = true, RecipientFilled = true, InvoiceFilled = true, SupportCashOnDelivery;
var $Orderer_TWzipcode, $Recipient_TWzipcode, $Invoice_TWzipcode;
var $orderer_name, $orderer_sex, $orderer_email, $orderer_cellphone, $orderer_telphone_area, $orderer_telphone, $orderer_telphone_ext, $orderer_address_city, $orderer_address_town, $orderer_address;
var $recipient_name, $recipient_sex, $recipient_email, $recipient_cellphone, $recipient_telphone_area, $recipient_telphone, $recipient_telphone_ext, $recipient_address_city, $recipient_address_town, $recipient_address, $remark;
var $invoice_recipient, $invoice_title, $invoice_uniformid, $invoice_address_city, $invoice_address_town, $invoice_address;
var $ship_method, $pay_method;
var step4PaidDateHtml = "";

var order_header_data = {}, user_data = {}, order_data = {}, recipient_data = {}, invoice_data = {}, invoiceType_data = {}, prod_data = {};
var shopping_cart_data = [];
var hasProds = false;

var islogin = false;

var datachange = true, HasECPay = false, HasECPayLogistics = false, ECPayInit = false, ECPayMonitor = false;

var RecipientsList_dxData;

function PageReady() {
    $('#RadioPayment .payment_display').first().addClass("first");
    $('#RadioPayment .payment_display').last().addClass("last");

    $('#RadioPayment .payment_display').on("click", function () {
        var $this_radio = $(this);
        var $parentFormCheck = $this_radio.closest('.form-check');
        var $prevPayment = $parentFormCheck.prevAll('.form-check:not(.d-none)').first().find('.payment_display');
        var $nextPayment = $parentFormCheck.nextAll('.form-check:not(.d-none)').first().find('.payment_display');
        $('#RadioPayment .payment_display').removeClass("checked first last");
        $(`#${$this_radio.data("radioid")}`).prop("checked", true);
        $this_radio.addClass("checked");
        $('#RadioPayment .form-check:not(.d-none) .payment_display')
            .first()
            .addClass("first");
        $('#RadioPayment .form-check:not(.d-none) .payment_display')
            .last()
            .addClass("last");
        $prevPayment.addClass('last');
        $nextPayment.addClass('first');
    });

    // 群組全選（Header）
    $(document).on('change', '.purchase_group .js-group-check', function () {
        const $group = $(this).closest('.purchase_group');
        const checked = this.checked;

        if (checked) clearOtherGroupsExcept($group);     // 互斥：先清其他組
        $group.find('.js-group-check').prop('indeterminate', false); // 讓第一次點擊不會卡在半選

        $group.find('input[name="buyItems"]').prop('checked', checked);

        updateGroupSelectedSubtotal($group);
        TotalCount();
        updateNextStepByBonus();
    });

    // 單一品項
    $(document).on('change', '.purchase_group li.purchase_item input[name="buyItems"]', function () {
        const $group = $(this).closest('.purchase_group');

        if (this.checked) clearOtherGroupsExcept($group); // 互斥：勾任何一個品項就清其他組

        updateGroupSelectedSubtotal($group);
        TotalCount();
        updateNextStepByBonus();
    });


    if ($("#ECPayPayment").length > 0) {
        HasECPay = true;
        ECPayMonitor = true;
        ECPay.initialize($("#ECPayPayment").data("server-type"), 1, function (errMsg) {
            if (errMsg != null) {
                $("#radio_payment_ECPay").addClass("d-none");
                console.log(`Initialize errMsg : ${errMsg}`)
                co.sweet.error("串接綠界發生錯誤");
            } else {
                ECPayInit = true;

                $("#radio_payment_ECPay").prop("checked", true);
                $("#radio_payment_ECPay").closest('.form-check').prevAll('.form-check').first().find('.payment_display').addClass('last');

                $('#RadioPayment .payment_display').on("click", function () {
                    var $this_radio = $(this);
                    var $parentFormCheck = $this_radio.closest('.form-check');
                    var $nextPayment = $parentFormCheck.nextAll('.form-check').first().find('.payment_display');
                    var $ECPayList = $("#ECPayPayment .ecpay-pay-list-wrap .ecpay-pay-list > li");

                    $ECPayList.each(function () {
                        $(this).removeClass("first");
                    })

                    if ($nextPayment.attr("id") == "payment_ECPay") $ECPayList.first().addClass("first");

                    $ECPayList.each(function () {
                        $(this).removeClass("ecpay-pl-act");
                    })
                    buy_step_swiper.update();
                });
            }
        });
        $("#RadioPayment div.form-check").addClass("d-none");
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
        allowTouchMove: false,
        simulateTouch: false,
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
            window.scrollTo(0, $("#BuyStepSwiper").offset().top - $("#Mega_Menu").height() - 90);
        }
    });

    buy_step_swiper.on('slideChange', function () {
        switch (buy_step_swiper.activeIndex) {
            case 1:
                const hasSelected = getSelectedCartIds().length > 0;
                var $errorSelected = $('li.purchase_item.cart-item-error input[name="buyItems"]:checked');

                const { bonus } = computeSelectedSubtotal();
                const memberBonus = Number(totalBonus || 0);
                const bonusNotEnough = Number(bonus || 0) > memberBonus;

                if (!hasProds) {
                    Coker.sweet.warning("錯誤", "無可購買商品。", null, false);
                    buy_step_swiper.slideTo(0);
                } else if (!hasSelected) {
                    Coker.sweet.warning("請注意", "請先勾選要結帳的商品（至少 1 項）。", null);
                    buy_step_swiper.slideTo(0);
                    return;
                } else if ($errorSelected.length > 0) {
                    Coker.sweet.warning(
                        "無法結帳",
                        "您選取的商品中包含已下架或庫存不足的品項，請先調整或移除後再繼續結帳。",
                        null,
                        false
                    );
                    buy_step_swiper.slideTo(0);
                    return;
                } else if (bonusNotEnough) {
                    Coker.sweet.warning(
                        "紅利不足",
                        `目前已選商品需紅利 ${Number(bonus || 0).toLocaleString()} 點，會員可用紅利 ${memberBonus.toLocaleString()} 點，請先調整商品或數量。`,
                        null,
                        false
                    );
                    buy_step_swiper.slideTo(0);
                    return;
                } else {
                    var select_cart_data = shopping_cart_data
                        .filter(item => getSelectedCartIds().includes(item.Id))
                        .reverse();

                    var isdefault = true;

                    for (var i = 0; i < select_cart_data.length; i++) {
                        var $select_input = $('input[data-subtype="' + select_cart_data[i].logisticsSubType + '"]');
                        if ($select_input.length > 0) {
                            $select_input.val(select_cart_data[i].cvsStoreName);
                            var $radio = $select_input.siblings('input[name="RadioShipping"]');
                            $radio.prop('checked', true);
                            $radio.attr({
                                "data-cvsstoreid": select_cart_data[i].cvsStoreID,
                                "data-cvsstorename": select_cart_data[i].cvsStoreName,
                                "data-cvsaddress": select_cart_data[i].cvsAddress,
                                "data-cvstelephone": select_cart_data[i].cvsTelephone,
                                "data-cvsoutside": select_cart_data[i].cvsOutSide,
                            })
                            RadioShipping();
                            isdefault = false;
                            break;
                        }
                    }

                    if (isdefault) $('input[data-isdefault="True"][name="RadioShipping"]').prop('checked', true);

                    enforceFreightVisibility();
                    OrdererFilled = FormCheck(OrdererForms);
                    RecipientFilled = FormCheck(RecipientForms);
                    InvoiceFilled = FormCheck(InvoiceForms);
                    if (!OrdererFilled) {
                        if (OrdererOpen) $("#OrdererForm>form").removeClass('was-validated')
                        OrdererEdit(true);
                        $("#radio_recipient_order").trigger("change");
                        $("#radio_bill_orderer").trigger("change");
                    }
                    if (datachange && HasECPay) {
                        ECPaymentChange();
                        datachange = false;
                    }
                }
                break;
            case 2:
                if (!isCheckout) {
                    if (OrdererOpen) { OrdererFilled = FormCheck(OrdererForms) };
                    if (RecipientOpen) { RecipientFilled = FormCheck(RecipientForms) };
                    if (InvoiceOpen) { InvoiceFilled = FormCheck(InvoiceForms) };
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
                        }
                    }
                    Coker.sweet.warning("未完成結帳流程！", "若資料已確實填寫完畢，請點選下方[確認付款]按鈕進入付款程序", null);
                    setTimeout(function () {
                        buy_step_swiper.slideTo(1);
                    }, 1500);
                }
                break;
        }
    });

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

    if (HasECPay) {
        $(":input").on("blur input change", function () {
            var $self = $(this);
            if ($self.is(':radio')) {
                if ($self.attr("name")?.includes("Sex")) ECPaymentChange();
            } else {
                ECPaymentChange();
            }
        });
    }

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

    /* Step3 Form 檢測 */
    ShippingForms = $('#RadioShipping');
    PaymentForms = $('#RadioPayment');
    OrdererForms = $('#OrdererForm > form');
    RecipientForms = $('#RecipientForm > form');
    InvoiceForms = $('#Form_Invoice');
    InvoicePersonalTypeForms = $('#Form_InvoicePersonalType');

    function getSelectedFreightGroupId() {
        const ids = getSelectedCartIds();     // 你現有的函式：回傳勾選的 scId 陣列
        const selected = shopping_cart_data.filter(e => ids.includes(e.Id));
        // 你的 shopping_cart_data 在 CartListAdd() 會寫入 e.freight（物件或 null）
        const uniq = new Set(
            selected.map(x => (x.freight && x.freight.id) ? Number(x.freight.id) : 0)
        );
        if (uniq.size === 0) return 0;     // 沒選任何商品 → 視為一般
        if (uniq.size > 1) {
            // 理論上不會發生（因為你已做群組互斥），保守取非零優先
            if (uniq.has(0)) uniq.delete(0);
        }
        return [...uniq][0] || 0;
    }

    function enforceFreightVisibility() {
        const fid = getSelectedFreightGroupId(); // 0 = 一般, >0 = 特殊運費 ID
        const $inputs = $('[name="RadioShipping"]');

        if (fid > 0) {
            // 👉 特殊運費情境：只保留該筆運費
            $inputs.each(function () {
                const $input = $(this);
                const id = Number($input.val());
                const $formCheck = $input.closest('.form-check');
                const $describe = $formCheck.next('.freight-describe');
                const isTarget = (id === fid);
                $formCheck.toggleClass('d-none', !isTarget);
                $describe.toggleClass('d-none', !isTarget);
                $input.prop('checked', isTarget);
            });

            const $checked = $('[name="RadioShipping"]:checked');
            if ($checked.length) RadioShipping.call($checked[0]);

        } else {
            // 👉 一般運費情境：隱藏所有特殊運費（freightStatusType = 2）
            $inputs.each(function () {
                const $input = $(this);
                const statusType = Number($input.data('freight-status-type')) || 0;
                const $formCheck = $input.closest('.form-check');
                const $describe = $formCheck.next('.freight-describe');
                const isSpecial = (statusType === 2); // 特殊運費

                $formCheck.toggleClass('d-none', isSpecial);
                $describe.toggleClass('d-none', isSpecial);

                // 若原本選到特殊項目 → 取消選取
                if (isSpecial && $input.is(':checked')) {
                    $input.prop('checked', false);
                }
            });

            // 若沒有任何選取 → 預設勾選第一個可見的一般運費
            const $checked = $('[name="RadioShipping"]:checked');
            if ($checked.length === 0) {
                const $firstVisible = $inputs.filter(function () {
                    return !$(this).closest('.form-check').hasClass('d-none');
                }).first();
                if ($firstVisible.length) {
                    $firstVisible.prop('checked', true);
                    RadioShipping.call($firstVisible[0]);
                }
            }
        }

        buy_step_swiper.update();
    }

    $(".btn_checkout").on("click", function () {
        ECPayMonitor = false;
        Step3Monitor();
        if (!OrdererFilled) {
            if (!OrdererOpen) OrdererEdit(true);
            Coker.sweet.warning("請注意", "請確實填寫訂購人資料！", null);
        } else if (!RecipientFilled) {
            Coker.sweet.warning("請注意", "請確實填寫收件人資料！", null);
        } else if (!InvoiceFilled) {
            Coker.sweet.warning("請注意", "請確實填寫發票寄送資料！", null);
        } else if (!shipMethodsChosen) {
            Coker.sweet.warning("請注意", "請選擇運送方式！", null);
        } else if (!payMethodsChosen && !HasECPay) {
            Coker.sweet.warning("請注意", "請選擇付款方式！", null);
        } else {
            if ($(`[name="RadioPayment"]:checked`).val() == null && HasECPay && (typeof window.Pay === "undefined" || $("#ECPayPayment").children().length == 0)) {
                co.sweet.warning("付款模組尚未載入完成，請稍候再試。", "", null);
            } else {
                Coker.sweet.custom("info", "是否確定結帳？", "點選確認進入付款流程", "是，開始付款", function () {
                    OrderHeaderAdd();
                }, "否", function () {
                    ECPayMonitor = true;
                    console.log("ECPayMonitor Change", ECPayMonitor);
                });
            }
        }
        buy_step_swiper.update();
    });

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
    $('input[type=radio][name=InvoiceType]').on("change", InvoiceTypeRadio);
    $('input[type=radio][name=PersonalInvoiceMode]').on("change", PersonalInvoiceMode);

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
        if (HasECPay) ECPaymentChange();
    })

    const raw = sessionStorage.getItem("orderForm");

    if (raw) {
        const data = JSON.parse(raw);
        const savedAt = data.savedAt;
        const now = Date.now();
        const diffMinutes = (now - savedAt) / 1000 / 60;

        if (diffMinutes < 30) {
            var formData = data.formData;
            var SelectedCartIds = data.SelectedCartIds;

            if (SelectedCartIds.length > 0) {
                co.sweet.loading();
                var timer = setInterval(function () {
                    if ($('input[name="buyItems"]').length > 0) {
                        clearInterval(timer);
                        Swal.close();
                        var $this_group;

                        $('input[name="buyItems"]').prop("checked", false);

                        $('input[name="buyItems"]').each(function () {
                            var $this = $(this);
                            var value = $this.val();
                            if (SelectedCartIds.includes(Number(value))) {
                                $this.prop("checked", true);
                                $this_group = $this.closest('.purchase_group');
                                clearOtherGroupsExcept($this_group);
                            }
                        });

                        if (typeof ($this_group) != "undefined") updateGroupSelectedSubtotal($this_group);
                        TotalCount();
                        updateNextStepByBonus();

                        buy_step_swiper.enable();
                        buy_step_swiper.slideTo(1);

                        OrdererEdit(true)
                        var ordererAddress = formData.ordererAddress;

                        co.Form.insertData(formData, "#Form_Orderer");
                        $("#OrdererInputAddress").val(ordererAddress.substring(ordererAddress.indexOf(" ", ordererAddress.indexOf(" ") + 1)).trim())

                        co.Zipcode.setData({
                            el: $("#Orderer_TWzipcode"),
                            addr: ordererAddress
                        });

                        if (data.RecipientType == "edit") {
                            $("[name='RecipientRadio'][value='edit']").prop("checked", true);
                            RecipientRadio()
                            var recipientAddress = formData.recipientAddress;

                            co.Form.insertData(formData, "#RecipientForm");
                            $("#RecipientInputAddress").val(recipientAddress.substring(recipientAddress.indexOf(" ", recipientAddress.indexOf(" ") + 1)).trim())

                            co.Zipcode.setData({
                                el: $("#Recipient_TWzipcode"),
                                addr: recipientAddress
                            });
                        }

                        if (formData.invoiceType == 2) {
                            $("[name='InvoiceType'][value='company']").prop("checked", true);
                            InvoiceTypeRadio();
                            var invoiceAddress = formData.invoiceAddress;

                            co.Form.insertData(formData, "#Form_Invoice");
                            $("#InvoiceInputAddress").val(invoiceAddress.substring(invoiceAddress.indexOf(" ", invoiceAddress.indexOf(" ") + 1)).trim())

                            co.Zipcode.setData({
                                el: $("#Invoice_TWzipcode"),
                                addr: invoiceAddress
                            });
                        }

                        if (formData.invoiceRecipient == 2) $("[name='InvoiceRadio'][value='order']").prop("checked", true);
                        if (FormCheck(OrdererForms)) OrdererEdit(false);

                        sessionStorage.removeItem("orderForm");
                    }
                }, 50);
            }
        }
    }

    $(".btn_getmap").on("click", function () {
        AllDataGet(false);

        const dataToSave = {
            formData: order_header_data,
            SelectedCartIds: getSelectedCartIds(),
            RecipientType: $(`[name="RecipientRadio"]:checked`).val(),
            savedAt: Date.now()
        };

        sessionStorage.setItem("orderForm", JSON.stringify(dataToSave));
        var $btn = $(this);
        var $radio = $btn.prev('input[name="RadioShipping"]');
        $radio.prop('checked', true);

        var $form = $("form#ecpayLogisticsForm");
        var scids = JSON.stringify(shopping_cart_data.map(c => c.Id));

        $form.find('input[name="LogisticsSubType"]').val($btn.data('subtype'));
        $form.find('input[name="SCIds"]').val(scids);
        $form.find('input[name="IsCollection"]').val($btn.data("support-cash-on-delivery") == "True" ? "Y" : "N");

        $form.submit();
    })
}

// 同步 header 的勾選/半選狀態與「已選件數」
function syncHeaderCheckbox($group) {
    const $checks = $group.find('input[name="buyItems"]');
    const total = $checks.length;
    const selected = $checks.filter(':checked').length;
    const $header = $group.find('.js-group-check');

    $header.prop('indeterminate', false);
    if (selected === 0) $header.prop('checked', false);
    else if (selected === total) $header.prop('checked', true);
    else $header.prop({ checked: false, indeterminate: true });

    $group.find('.js-selected-count').text(selected);
}
// 計算本組「已選」的小計 → 更新 footer
function updateGroupSelectedSubtotal($group) {
    let sum = 0;
    $group.find('li.purchase_item').each(function () {
        const $li = $(this);
        if ($li.find('input[name="buyItems"]').is(':checked')) {
            sum += Number($li.find('[data-key="subtotal"]').data('subtotal') || 0);
        }
    });
    $group.find('.js-group-subtotal').attr('data-subtotal', sum).text(`$${sum.toLocaleString()}`);
    syncHeaderCheckbox($group);
}
// 清掉「其他群組」的選取與小計（互斥的關鍵）
function clearOtherGroupsExcept($group) {
    $('.purchase_group').not($group).each(function () {
        const $g = $(this);
        $g.find('.js-group-check').prop({ checked: false, indeterminate: false });
        $g.find('input[name="buyItems"]').prop('checked', false);
        $g.find('.js-group-subtotal').attr('data-subtotal', 0).text('$0');
        $g.find('.js-selected-count').text(0);
    });
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
    console.log("GetOrderPage", window.location.search);
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
            if (result.success && result.object != null) {
                $("#Step1 > .card-body").removeClass("d-none");
                buy_step_swiper.enable();
                $("#Purchase_Null").addClass("d-none");
                CartInit(result.object)
            } else {
                window.location.href = `/${OrgName}/ShoppingCar`;
            }
        })
    } else if (window.location.search.substring(1).startsWith("ECPayError")) {
        co.sweet.confirm("訂單付款發生錯誤", "", "確認", "", null);
    }
}
function SuccessPageDataInsert(data) {
    var header = data.orderHeader || {};
    var details = data.orderDetails || [];

    console.log(header);
    console.log(details);

    ShoppingCartDataInsert(header, $("#Step4 .card-body"));

    toggleStep4EndlineDisplay(header);
    toggleStep4InvoiceDisplay(header);
    toggleStep4PaymentDisplay(header);

    TemplateDataInsert($("#Purchase"), $("#CollapsePurchase"), $("#Template_Purchase_Details"), details);

    buy_step_swiper.update();
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
            ori_freight = Number($(this).data("freight") || 0);
            low_con = Number($(this).data("lowcon") || 0);
            disfreight = Number($(this).data("disfreight") || 0);
            freightType = Number($(this).data("freight-type") || 0);

            var rawDiscountFreightType = $(this).attr("data-discount-freight-type");
            discountFreightType = rawDiscountFreightType === "" || rawDiscountFreightType == null
                ? null
                : Number(rawDiscountFreightType);

            var rawBoxFees = $(this).attr("data-boxfees");
            try {
                boxFees = rawBoxFees ? JSON.parse(rawBoxFees) : [];
            } catch (err) {
                console.error("init data-boxfees parse failed", err, rawBoxFees);
                boxFees = [];
            }

            freight = ori_freight;
        } else {
            freight = 0;
        }
    });
    $pay_method = $("input[name=RadioPayment]");
}
function CardDataGet() {
    Product.GetAll.Cart().done(function (result) {
        if (result.length > 0) {
            CartInit(result)
        }
    });
}
// 依 freight.id 分群（null/缺值 = 0：一般配送）
function groupByFreight(list) {
    return list.reduce((acc, it) => {
        const key = (it.freight && it.freight.id) ? it.freight.id : 0;
        (acc[key] ||= []).push(it);
        return acc;
    }, {});
}
// 建群組標頭 DOM
function createGroupHeader(meta) {
    const $tpl = $($('#Template_Cart_GroupHeader').html().trim());
    $tpl.attr('data-group-id', meta.id);
    $tpl.find('[data-field="title"]').text(meta.title || '一般配送');
    if (meta.describe) $tpl.find('[data-field="describe"]').text(meta.describe).show();
    else $tpl.find('[data-field="describe"]').hide();
    $tpl.find('[data-field="count"]').text(`${meta.count} 件`);
    $tpl.find('.btn-checkout-group').attr('data-group-id', meta.id);
    return $tpl;
}
// ✅ 用你原本的 CartListAdd 來插入每一筆
function renderCartGroups(result) {
    const $ul = $("#Step1 > .card-body > .purchase_list");
    // 只移除舊的群組，不要 .empty() 以免不小心砍到 template
    $ul.children("li.purchase_group").remove();

    // 分群
    const groups = groupByFreight(result);
    const orderedKeys = Object.keys(groups).sort((a, b) => groups[b].length - groups[a].length);

    orderedKeys.forEach(key => {
        const items = groups[key];
        const meta = {
            id: Number(key),
            title: items[0].freight?.title || '一般配送',
            describe: items[0].freight?.describe || '',
            count: items.length
        };

        // 建群組容器
        const $group = $($('#Template_Cart_Group').html().trim());
        $group.attr('data-group-id', meta.id);
        $group.find('[data-field="title"]').text(meta.title);
        $group.find('[data-field="count"]').text(`${meta.count} 件`);
        if (meta.describe) $group.find('[data-field="describe"]').text(meta.describe);
        else $group.find('[data-field="describe"]').remove();

        const $groupItems = $group.find('.group_items');

        // 塞入該組 items（沿用你原有 CartListAdd，但改讓它可以指定容器）
        items.forEach(row => {
            var originalPrice = row.oldPrice != null ? row.oldPrice : row.price;
            var currentPrice = row.price != null ? row.price : 0;

            // 存一份「加入購物車當時的價」到自訂欄位，給後面需要的人用
            row.originalPriceInCart = originalPrice;
            row.priceChangeFlag = null;  // 'up' = 調漲, 'down' = 降價

            if (originalPrice > 0 && currentPrice > 0 && originalPrice !== currentPrice) {
                row.priceChangeFlag = (currentPrice > originalPrice) ? 'up' : 'down';
            }
            // === 原本的程式 ===
            row.__groupId = meta.id;
            CartListAdd(row, $groupItems);
        });


        // 初始化本組已選小計/件數
        $group.find('.js-group-subtotal').attr('data-subtotal', 0).text('$0');
        $group.find('.js-group-check').prop('indeterminate', false);
        $group.find('.js-selected-count').text(0);

        $ul.append($group);
    });

    const hasItems = result.length > 0;
    $("#Step1 > .card-body").toggleClass("d-none", !hasItems);
    $("#Purchase_Null").toggleClass("d-none", hasItems);

    buy_step_swiper.update();
}
function updateOverallSubtotal() {
    let sum = 0;
    $('#Step1 .purchase_list .purchase_group_header [data-field="subtotal"]').each(function () {
        sum += Number($(this).attr('data-subtotal') || 0);
    });
    $('#Step1 [data-key="subtotal"].subtotal').text(`$${sum.toLocaleString()}`);
    $('#Step1 [data-key="total"].subtotal').text(`$${sum.toLocaleString()}`);
}
function CartInit(result) {
    $("#Step1 > .card-body").removeClass("d-none");
    renderCartGroups(result);

    $("#Purchase_Null").addClass("d-none");
    buy_step_swiper.enable();

    var popoverTriggerList = Array.prototype.slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'))
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl)
    })

    buy_step_swiper.update();
    TotalCount();
    updateNextStepByBonus();
    PaymentHideShow();
    ValidateCartOnInit();

    const $firstGroup = $('.purchase_group').first();
    if ($firstGroup.length) {
        // 勾選整組
        $firstGroup.find('.js-group-check').prop('checked', true);
        $firstGroup.find('input[name="buyItems"]').prop('checked', true);

        // 計算小計與合計
        updateGroupSelectedSubtotal($firstGroup);
        TotalCount();
        updateNextStepByBonus();
    }
}
function PaymentHideShow() {
    if (!HasECPay || $(".ecpayWarning").hasClass("d-none")) {
        $("#RadioPayment > div").each(function () {
            var $self = $(this);
            var $self_input = $self.find("input");
            $self.removeClass("d-none");
            if (subtotal < $self_input.data("minamount")) $self.addClass("d-none");
            if ($self_input.data("maxamount") != null && $self_input.data("maxamount") != "") {
                if (subtotal > $self_input.data("maxamount")) $self.addClass("d-none");
            }
        })
        return true;
    } else return false;
}
function CartListAdd(data, $container) {
    if (data.quantity > 0) {
        var exists = shopping_cart_data.find(e => e.Id == data.scId);

        if (exists != null) {
            // 已經存在就沿用目前的結帳單價（避免重複 push）
            data.price = exists.Price;
        } else {
            var obj = {};
            obj['Id'] = data.scId;
            obj['Price'] = data.price;
            obj['OriginalPrice'] = data.originalPriceInCart ?? data.price;
            obj['Quantity'] = data.quantity;
            obj['Bonus'] = data.bonus;
            obj['PackingPoint'] = Number(data.packingPoint || data.PackingPoint || 0);
            obj['freight'] = data.freight;
            obj['cvsStoreID'] = data.cvsStoreID;
            obj['cvsStoreName'] = data.cvsStoreName;
            obj['cvsAddress'] = data.cvsAddress;
            obj['cvsTelephone'] = data.cvsTelephone;
            obj['cvsOutSide'] = data.cvsOutSide;
            obj['logisticsSubType'] = data.logisticsSubType;
            shopping_cart_data.push(obj);
            refreshHasProds();
        }
    }

    var max_quantity = data.quantity + data.stock;

    var item_list_ul = $container || $("#Step1 > .card-body > .purchase_list");
    var $template = $($("#Template_Cart_Details").html()).clone();
    var groupId = (data.freight && data.freight.id) ? data.freight.id : 0;

    $template.data("scId", data.scId);
    $template.attr('data-group-id', groupId);
    $template = CartListInsert($template, data);
    $template.find(".btn_remove_pro").on("click", function () {
        var $self = $(this).parents("li").first();
        Coker.sweet.confirm("確定將商品從購物車移除？", "該商品將會從購物車中移除，且不可復原。", "確認移除", "取消", function () {
            const $group = $self.closest('li.purchase_group');
            CartDelete($self, $self.data("scId"), "成功移除商品", "移除商品發生未知錯誤")

            if ($group.length) {
                if ($group.find('li.purchase_item').length === 0) {
                    $group.remove();
                } else {
                    // 還有其他商品 → 更新群組小計/勾選狀態
                    updateGroupSelectedSubtotal($group);
                    TotalCount();
                }
            }
        });
    });
    $template.find(".btn_count_plus").on("click", function () {
        var $self_bro = $(this).siblings(".pro_quantity");
        const $group = $template.closest('.purchase_group');

        if ($self_bro.val() < max_quantity) {
            $self_bro.val(parseInt($self_bro.val()) + parseInt($self_bro.attr("step")));
            CartQuantityUpdate(
                $template.find(".pro_subtotal"),
                data.price,
                data.bonus,
                $template.data("scId"),
                Number($self_bro.val()),
                $group
            );
        }
    });

    $template.find(".btn_count_minus").on("click", function () {
        var $self_bro = $(this).siblings(".pro_quantity");
        const $group = $template.closest('.purchase_group');

        const currentQty = Number($self_bro.val() || 0);
        const step = Number($self_bro.attr("step") || 1);
        const nextQty = currentQty - step;

        // 按鈕減量：下一次數量低於最小購買量時，走既有刪除流程
        if (nextQty < step) {
            $template.find(".btn_remove_pro").trigger("click");
            return;
        }

        $self_bro.val(nextQty);

        CartQuantityUpdate(
            $template.find(".pro_subtotal"),
            data.price,
            data.bonus,
            $template.data("scId"),
            nextQty,
            $group
        );
    });

    $template.find(".pro_quantity").on("change", function () {
        var $self = $(this);
        const $group = $template.closest('.purchase_group');

        if ($self.val() < parseInt($self.attr("step"))) {
            $self.val(parseInt($self.attr("step")));
        } else if ($self.val() > max_quantity) {
            $self.val(max_quantity - (max_quantity % parseInt($self.attr("step"))));
        } else {
            $self.val($self.val() - ($self.val() % parseInt($self.attr("step"))));
        }

        CartQuantityUpdate(
            $template.find(".pro_subtotal"),
            data.price,
            data.bonus,
            $template.data("scId"),
            Number($self.val()),
            $group
        );
    });

    if ($template.find(".btn_move_to_favorites").length > 0) {
        if (islogin) {
            var $btn_favorites = $template.find(".btn_move_to_favorites");
            if (data.quantity) $btn_favorites.parent("span").removeClass("d-none");

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
function refreshHasProds() {
    hasProds = shopping_cart_data && shopping_cart_data.length > 0;
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
                case "oldPrice": {
                    var original = Number(data.oldPrice || 0);
                    var current = Number(data.price || 0);

                    var oldBonus = Number(data.oldBonus || 0);
                    var bonus = Number(data.bonus || 0);

                    var $priceDiv = $self.siblings("div[data-key='price']");

                    // 先清乾淨，避免殘留上一筆狀態
                    $self.addClass("d-none").text("");
                    $priceDiv.removeClass("price-up price-down price-changed text-danger text-success red_text");
                    $priceDiv.removeData("price-display-mode");

                    var priceChanged = original > 0 && current > 0 && original !== current;
                    var bonusChanged = oldBonus !== bonus;

                    // 價格與紅利都沒變：
                    // 不顯示舊價，且目前價格改用 block 顯示
                    if (!priceChanged && !bonusChanged) {
                        $priceDiv.data("price-display-mode", "block");
                        break;
                    }

                    let oldText = "";

                    if (oldBonus > 0) {
                        if (original > 0) {
                            oldText = `$${original.toLocaleString()} + 紅利${oldBonus.toLocaleString()}`;
                        } else {
                            oldText = `紅利${oldBonus.toLocaleString()}`;
                        }
                    } else {
                        oldText = `$${original.toLocaleString()}`;
                    }

                    $self.removeClass("d-none");
                    $self.text(oldText);

                    if (priceChanged) {
                        if (current > original) {
                            $priceDiv.addClass("price-up text-danger red_text");
                        } else {
                            $priceDiv.addClass("price-down text-success");
                        }
                    }

                    if (bonusChanged) {
                        if (bonus > oldBonus) {
                            $priceDiv.addClass("price-up text-danger red_text");
                        } else {
                            $priceDiv.addClass("price-down text-success");
                        }
                    }

                    $priceDiv.addClass("price-changed");
                    break;
                }
                case "price": {
                    var unitPrice = Number(data.price || 0);
                    var bonus = Number(data.bonus || 0);

                    var cashText = unitPrice > 0
                        ? `$${unitPrice.toLocaleString()}`
                        : "";

                    if (data.priceLabel != null && cashText) {
                        cashText = `${data.priceLabel} ${cashText}`;
                    }

                    var mode = $self.data("price-display-mode") || "inline";

                    setCartPriceBlock($self, cashText, bonus, mode);
                    break;
                }
                case "subtotal": {
                    var unitPrice = Number(data.price || 0);
                    var qty = Number(data.quantity || 0);
                    var sub_price = unitPrice * qty;
                    var sub_bonus = Number(data.bonus || 0) * qty;

                    var cashText = sub_price > 0
                        ? `$${sub_price.toLocaleString()}`
                        : "";

                    setCartPriceBlock($self, cashText, sub_bonus, "block");

                    $self.data("subtotal", sub_price);
                    $self.data("subtotal_bonus", sub_bonus);
                    break;
                }

                case "quantity":
                    $self.val(data[key]);
                    $self.attr({ step: data.step })
                    break;
                case "freight":
                    if (data[key] == null) $self.remove();
                    else {
                        $self.data("freight", data[key]);
                        $self.text(data[key].title);
                    }
                    break;
                default:
                    $self.text(data[key]);
                    break;
            }
            const item = $self.closest(".image");
            const checkItem = item.find(`[name="buyItems"]`).attr("id", `prod${data.scId}`).val(data.scId);
            checkItem.next("label").attr("for", `prod${data.scId}`);
            if (data['quantity'] == 0) {
                $frame.find(".nostock").removeClass("d-none");
                $frame.find(".content").addClass("d-none");
                $frame.find(".btn_side_icon .favorites").addClass("d-none");
            }
        }
    });
    return $frame;
}
function CartQuantityUpdate(self, price, bonus, scid, quantity, $group) {
    if (quantity <= 0) return;

    var entry = shopping_cart_data.find(function (e) { return e.Id == scid; });
    var oldQty = entry ? entry.Quantity : quantity;

    function updateSubtotalAndDisplay(qty) {
        var sub_price = Number(price || 0) * Number(qty || 0);
        var sub_bonus = Number(bonus || 0) * Number(qty || 0);

        self.data("subtotal", sub_price);
        self.data("subtotal_bonus", sub_bonus);

        var cashText = sub_price > 0
            ? `$${sub_price.toLocaleString()}`
            : "";

        setCartPriceBlock(self, cashText, sub_bonus);
    }

    function syncGroupAndTotal() {
        if ($group && $group.length) {
            updateGroupSelectedSubtotal($group);
        }
        TotalCount();
        updateNextStepByBonus();
    }

    function handleUpdateError(title, message, rollbackOnly) {
        var $li = self.closest('li.purchase_item');
        var $qty = $li.find('.pro_quantity');

        $qty.val(oldQty);
        updateSubtotalAndDisplay(oldQty);

        if (!rollbackOnly && title !== "") {
            Coker.sweet.error(title, message, null, true);
        }

        if (!rollbackOnly) {
            $li.addClass('cart-item-error');

            var $itemCheckbox = $li.find('input[name="buyItems"]');
            if ($itemCheckbox.length) {
                $itemCheckbox.prop('checked', false);
                $itemCheckbox.prop('disabled', true);
            }

            $li.find('.js-stock-error').remove();
            var $content = $li.find('.content');
            if (!$content.length) $content = $li;

            var $msgDiv = $('<div class="js-stock-error text-danger small mt-1"></div>');
            $msgDiv.text(message || '此商品目前無法購買，請調整或移除。');
            $content.append($msgDiv);
        }

        syncGroupAndTotal();
    }

    Product.Update.Cart({
        Id: scid,
        Quantity: quantity
    }).done(function (result) {
        if (result.success) {
            var $li = self.closest('li.purchase_item');

            if (entry) {
                entry.Price = price;
                entry.Bonus = bonus;
                entry.Quantity = quantity;
                entry.PackingPoint = Number(entry.PackingPoint || 0);
            }

            $li.removeClass('cart-item-error');
            $li.find('.js-stock-error').remove();

            var $itemCheckbox = $li.find('input[name="buyItems"]');
            if ($itemCheckbox.length) {
                $itemCheckbox.prop('disabled', false);
            }

            if (result.object?.items?.[0] && !result.object.items[0].success) {
                handleUpdateError("", result.object.items[0].message, false);
                return;
            }

            updateSubtotalAndDisplay(quantity);
            syncGroupAndTotal();
            CartDropReset(scid, quantity);
            return;
        }

        var msg = result.message || "商品數量修改發生錯誤，請稍後再試。";
        handleUpdateError("商品更改數量發生錯誤", msg, false);

    }).fail(function () {
        handleUpdateError("錯誤", "商品數量修改發生錯誤，請稍後再試。", false);
    });
}
function computeSelectedSubtotal() {
    let sum = 0, bonus = 0;
    $('.purchase_group li.purchase_item input[name="buyItems"]:checked').each(function () {
        const $li = $(this).closest('li.purchase_item');
        const $sub = $li.find('[data-key="subtotal"]');
        sum += Number($sub.data('subtotal') || 0);
        bonus += Number($sub.data('subtotal_bonus') || 0);
    });
    return { sum, bonus };
}
function TotalCount() {
    const { sum, bonus } = computeSelectedSubtotal();

    // 商品小計
    subtotal = Number(sum || 0);
    order_data.bonus = Number(bonus || 0);

    // 依運費型態計算
    var freightResult = calculateFreight(subtotal);
    freight = Number(freightResult.freight || 0);

    // 同步目前 shipping meta 到全域
    var shippingMeta = getSelectedShippingMeta();
    ori_freight = Number(shippingMeta.freight || 0);
    low_con = Number(shippingMeta.lowCon || 0);
    disfreight = Number(shippingMeta.disFreight || 0);
    freightType = Number(shippingMeta.freightType || 0);
    discountFreightType = shippingMeta.discountFreightType == null ? null : Number(shippingMeta.discountFreightType);
    boxFees = shippingMeta.boxFees || [];

    // 商品金額
    $(".subtotal").text(subtotal.toLocaleString());

    // 商品紅利（作為附註，不放進主計算列）
    const $bonusParts = $(".dual-price .bonus-part");
    const $priceBonus = $(".priceline .bonus");

    if ((bonus || 0) > 0) {
        $priceBonus.text(bonus.toLocaleString());
        $bonusParts.removeClass("d-none");
        $(".dual-price .plus-sign").removeClass("d-none");
    } else {
        $priceBonus.text("");
        $bonusParts.addClass("d-none");
        $(".dual-price .plus-sign").addClass("d-none");
    }

    // ===== 紅利折抵規則 / 本次折抵 =====
    const $bonusDisconLine = $(".bonusDiscionLine");
    const $bonusRuleLine = $(".bonusRuleLine");
    const $redeemRuleText = $(".bonusRedeemRuleText");

    const redeemEnabled = (MinOrderForRedemption > 0 && MaxRedemptionPercent > 0);

    let allBonus = Number(bonus || 0);
    let redeemAmount = 0;
    let payableSubtotal = subtotal;

    // 預設全部清空（避免殘留）
    $bonusDisconLine.addClass("d-none");
    $bonusDisconLine.find(".summary-label").text("紅利折抵");
    $bonusDisconLine.find(".bonusDiscion").text("");

    $bonusRuleLine.addClass("d-none");
    $redeemRuleText.text("");

    // ===== 狀態判斷 =====
    if (redeemEnabled) {

        // 未達門檻
        if (subtotal < MinOrderForRedemption) {

            const diff = MinOrderForRedemption - subtotal;

            $redeemRuleText.text(
                `再消費 $${diff.toLocaleString()} 可使用紅利折抵（最高 ${MaxRedemptionPercent}%）`
            );

            $bonusRuleLine.removeClass("d-none");
        }

        // 已達門檻
        else {

            const cap = Math.floor(subtotal * MaxRedemptionPercent / 100);
            const memberBonusAmount = Math.max(0, (totalBonus || 0) - bonus);

            redeemAmount = Math.min(cap, memberBonusAmount);

            // ===== 有可折抵 =====
            if (redeemAmount > 0) {

                $bonusDisconLine.removeClass("d-none");

                // label 覆蓋
                $bonusDisconLine.find(".summary-label")
                    .text(`本單可使用紅利折抵（最高 ${MaxRedemptionPercent}%）`);

                // 金額
                $bonusDisconLine.find(".bonusDiscion")
                    .text(`${redeemAmount.toLocaleString()} 點`);

                allBonus += redeemAmount;
                payableSubtotal = Math.max(0, subtotal - redeemAmount);

                // 👉 若「被紅利上限卡住」才顯示提示
                if (memberBonusAmount < cap) {
                    $redeemRuleText.text(
                        `目前剩餘紅利 ${memberBonusAmount.toLocaleString()} 點，已全數折抵`
                    );
                    $bonusRuleLine.removeClass("d-none");
                }
            }

            // ===== 可折抵但實際=0（紅利不足）=====
            else {

                $redeemRuleText.text(
                    `目前剩餘紅利 ${memberBonusAmount.toLocaleString()} 點`
                );

                $bonusRuleLine.removeClass("d-none");
            }
        }
    }

    // Step1 小計（折抵後，不含運費）
    $(".payable_subtotal").text(parseInt(payableSubtotal, 10).toLocaleString());

    // ===== 紅利回饋提示 =====
    const $rewardRow = $(".bonusRedeemHintLine");
    const $earnText = $(".bonusEarnHintText");

    const earnEnabled = (MinOrderForEarnPoints > 0 && RewardRatePercent > 0);

    if (!earnEnabled) {
        $rewardRow.addClass("d-none");
        $earnText.text("");
    } else if (subtotal < MinOrderForEarnPoints) {
        const diff = MinOrderForEarnPoints - subtotal;
        $rewardRow.removeClass("d-none");
        $earnText.text(`再消費 $${diff.toLocaleString()} 可獲得紅利回饋`);
    } else {
        const earnPoints = Math.floor(subtotal * RewardRatePercent / 100);
        if (earnPoints > 0) {
            $rewardRow.removeClass("d-none");
            $earnText.text(`本單預計獲得紅利 ${earnPoints.toLocaleString()} 點`);
        } else {
            $rewardRow.addClass("d-none");
            $earnText.text("");
        }
    }

    // ===== 運費 =====
    $(".shipping_fee").text(freight.toLocaleString());

    // 運費提醒：單筆 / 箱型都支援，只要有門檻且未達成就顯示
    if (shouldShowShippingShortage(freightResult)) {
        $(".shipping_memo .price").text(Number(freightResult.shortage).toLocaleString());
        $(".shipping_memo").removeClass("d-none");
    } else {
        $(".shipping_memo").addClass("d-none");
        $(".shipping_memo .price").text("");
    }

    // ===== 箱型資訊 =====
    const $boxMemo = $(".shipping_box_memo");
    if ($boxMemo.length) {
        if (freightResult.mode === 3 && freightResult.boxResult && freightResult.boxResult.boxes.length > 0) {
            var boxText = freightResult.boxResult.boxes
                .map(function (x) {
                    return `${x.name} × ${x.count}`;
                })
                .join("、");

            $boxMemo.text(`(本次配箱：${boxText})`);
            $boxMemo.removeClass("d-none");
        } else {
            $boxMemo.text("");
            $boxMemo.addClass("d-none");
        }
    }

    // ===== 全單紅利使用總計 =====
    if (allBonus > 0) {
        $(".bonusUseTotalLine").removeClass("d-none");
        $(".bonusUseTotalValue").text(`${allBonus.toLocaleString()} 點`);
    } else {
        $(".bonusUseTotalLine").addClass("d-none");
        $(".bonusUseTotalValue").text("");
    }

    // ===== Step3 小計（折抵後 + 運費）=====
    total = payableSubtotal + freight;
    $(".total_amount").text(parseInt(total, 10).toLocaleString());

    PaymentHideShow();
}
function setCartPriceBlock($target, cashText, bonusValue, mode) {
    const bonus = Number(bonusValue || 0);

    const $main = $target.find(".cart-price-main");
    const $bonus = $target.find(".cart-price-bonus");

    // ⭐ 預設 fallback：Step1（單行）
    if (!mode) mode = "inline";

    if (mode === "block") {
        // ===== Step4：上下分行 =====
        if ($main.length && $bonus.length) {
            $main.text(cashText || "");

            if (bonus > 0) {
                $bonus.text(`紅利：${bonus.toLocaleString()}`);
                $bonus.removeClass("d-none");
            } else {
                $bonus.text("");
                $bonus.addClass("d-none");
            }
        } else {
            // fallback
            if (bonus > 0) {
                $target.html(`${cashText}<br/>紅利：${bonus.toLocaleString()}`);
            } else {
                $target.text(cashText || "");
            }
        }
    } else {
        // ===== Step1：單行 =====
        if (bonus > 0) {
            $target.text(
                cashText
                    ? `${cashText} + 紅利${bonus.toLocaleString()}`
                    : `紅利${bonus.toLocaleString()}`
            );
        } else {
            $target.text(cashText || "");
        }
    }
}
function toNumberValue(value) {
    if (value == null || value === "") return 0;
    return Number(String(value).replaceAll(",", "")) || 0;
}
function getValueIgnoreCase(data, key) {
    if (!data || !key) return undefined;

    if (Object.prototype.hasOwnProperty.call(data, key)) {
        return data[key];
    }

    var lowerCamelKey = key.charAt(0).toLowerCase() + key.slice(1);
    if (Object.prototype.hasOwnProperty.call(data, lowerCamelKey)) {
        return data[lowerCamelKey];
    }

    var pascalKey = key.charAt(0).toUpperCase() + key.slice(1);
    if (Object.prototype.hasOwnProperty.call(data, pascalKey)) {
        return data[pascalKey];
    }

    return undefined;
}
function toggleStep4EndlineDisplay(header) {
    header = header || {};

    var productBonus = toNumberValue(getValueIgnoreCase(header, "productBonus"));
    var redeemBonus = toNumberValue(getValueIgnoreCase(header, "redeemBonus"));
    var totalBonus = toNumberValue(getValueIgnoreCase(header, "bonus"));

    $("#Step4 .productBonusLine").toggleClass("d-none", productBonus <= 0);
    $("#Step4 .bonusDiscionLine").toggleClass("d-none", redeemBonus <= 0);
    $("#Step4 .bonusUseTotalLine").toggleClass("d-none", totalBonus <= 0);

    // Step4 運費是費用列，0 元也要顯示
    var freightText = String(getValueIgnoreCase(header, "freight") ?? "");
    if (freightText !== "") {
        $("#Step4 .shipping_fee").text(toNumberValue(freightText).toLocaleString());
    }
}

function toggleStep4InvoiceDisplay(header) {
    header = header || {};

    var invoiceRecipient = Number(getValueIgnoreCase(header, "invoiceRecipient") || 0);

    $("#Step4 .invoice_type .person").addClass("d-none");
    $("#Step4 .invoice_type .company").addClass("d-none");
    $("#Step4 .invoice_type .mobileCarrier").addClass("d-none");

    $("#Step4 .invoice_data .orderer").addClass("d-none");
    $("#Step4 .invoice_data .recipient").addClass("d-none");

    var invoiceTypeTitle = String(getValueIgnoreCase(header, "invoiceTypeTitle") || "");
    var personalInvoiceTypeTitle = String(getValueIgnoreCase(header, "personalInvoiceTypeTitle") || "");
    var carrier = String(getValueIgnoreCase(header, "carrier") || "");

    if (invoiceTypeTitle.indexOf("公司") >= 0) {
        $("#Step4 .invoice_type .company").removeClass("d-none");
    } else {
        $("#Step4 .invoice_type .person").removeClass("d-none");

        if (carrier !== "" || personalInvoiceTypeTitle.indexOf("載具") >= 0 || personalInvoiceTypeTitle.indexOf("手機") >= 0) {
            $("#Step4 .invoice_type .mobileCarrier").removeClass("d-none");
        }
    }

    switch (invoiceRecipient) {
        case 1:
            $("#Step4 .invoice_data .orderer").removeClass("d-none");
            break;
        case 2:
            $("#Step4 .invoice_data .recipient").removeClass("d-none");
            break;
    }
}

function toggleStep4PaymentDisplay(header) {
    header = header || {};

    var payment = String(getValueIgnoreCase(header, "payment") || "");
    var total = String(getValueIgnoreCase(header, "total") || "");
    var memo = String(getValueIgnoreCase(header, "memo") || "");

    if (payment !== "") {
        $("#PaymentData").removeClass("d-none");
        $("#PaymentData .payment_method").text(payment);
    } else {
        $("#PaymentData").addClass("d-none");
    }

    if (total !== "") {
        $("#PaymentData .pay_info .total_amount").text(total);
    }

    $("#PaymentData .pay_info .paid_date").empty();

    if (step4PaidDateHtml !== "") {
        $("#PaymentData .pay_info .paid_date").append(step4PaidDateHtml);
        $("#PaymentData .pay_info").removeClass("d-none");
    } else if (memo !== "") {
        $("#PaymentData .pay_info .paid_date").html(memo);
        $("#PaymentData .pay_info").removeClass("d-none");
    }
}
function updateNextStepByBonus() {
    const { bonus } = computeSelectedSubtotal();
    const memberBonus = Number(totalBonus || 0);
    const shortage = Math.max(0, Number(bonus || 0) - memberBonus);
    const isEnough = shortage <= 0;
    console.log(totalBonus);
    const $nextBtn = $(".btn_swiper_next_buystep");
    const $hint = $(".js-bonus-nextstep-hint");
    const $hintText = $(".js-bonus-nextstep-text");

    // 按鈕控制
    $nextBtn.prop("disabled", !isEnough);
    $nextBtn.toggleClass("disabled", !isEnough);

    // 提示控制
    if ($hint.length && $hintText.length) {
        if (isEnough) {
            $hint.addClass("d-none");
            $hintText.text("");
        } else {
            $hint.removeClass("d-none");
            $hintText.text(
                `目前已選商品需紅利 ${Number(bonus || 0).toLocaleString()} 點，` +
                `會員可用紅利 ${memberBonus.toLocaleString()} 點，` +
                `尚差 ${shortage.toLocaleString()} 點。`
            );
        }
    }
}
function CartDelete(self, id, success, error) {
    self.remove();
    datachange = true;
    Product.Delete.Cart(id).done(function () {
        Coker.sweet.success(success, null, true);
        var index = shopping_cart_data.findIndex(e => e.Id == id);
        if (index !== -1) {
            shopping_cart_data.splice(index, 1);
            refreshHasProds();
        }
        CartDropReset(id, 0)
        TotalCount();
        updateNextStepByBonus();
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
    var $this = $("[name='RadioShipping']:checked");

    ori_freight = Number($this.data("freight") || 0);
    low_con = Number($this.data("lowcon") || 0);
    disfreight = Number($this.data("disfreight") || 0);
    freightType = Number($this.data("freight-type") || 0);

    var rawDiscountFreightType = $this.attr("data-discount-freight-type");
    discountFreightType = rawDiscountFreightType === "" || rawDiscountFreightType == null
        ? null
        : Number(rawDiscountFreightType);

    var rawBoxFees = $this.attr("data-boxfees");
    try {
        boxFees = rawBoxFees ? JSON.parse(rawBoxFees) : [];
    } catch (err) {
        console.error("RadioShipping data-boxfees parse failed", err, rawBoxFees);
        boxFees = [];
    }

    freight = ori_freight;

    TotalCount();

    if (HasECPay) ECPaymentChange();

    var this_SupportCashOnDelivery = $this.data("support-cash-on-delivery") == "True";
    if (SupportCashOnDelivery != this_SupportCashOnDelivery) {
        UpdatePaymentVisibility({
            isCOD: this_SupportCashOnDelivery,
            isECPayMode: ECPayMonitor
        })
        SupportCashOnDelivery = this_SupportCashOnDelivery;
    }
}
// Toggle Payment By SupportCashOnDelivery && ECPay
function UpdatePaymentVisibility({ isCOD, isECPayMode, $clickElement }) {
    if (PaymentHideShow()) {
        $("#RadioPayment .form-check").each(function () {
            var $self = $(this);
            var $input = $self.find("input");

            var val = Number($input.val());
            var isECPay = $input.attr("id") === "radio_payment_ECPay";

            var shouldShow = true;

            if (isCOD) shouldShow = (val === 28);
            else if (val === 28) shouldShow = false;

            if (isECPayMode && isECPay) shouldShow = false;

            $self.toggleClass("d-none", !shouldShow);
        });

        var $items = $("#RadioPayment .form-check:not(.d-none)");
        var $first = $items.first();

        if ($first.length > 0) {
            $("#RadioPayment input").prop("checked", false);
            $("#RadioPayment .payment_display").removeClass("checked");

            var $input = $first.find("input");
            var $display = $first.find(".payment_display");

            $input.prop("checked", true);
            $display.addClass("checked");
            $input.trigger("change");
            $input.closest('.form-check').nextAll('.form-check:not(.d-none)').first().find('.payment_display').addClass("first");
        }
    }
}
function RadioPayment() {
    var $pay_text = $(".payment_method");
    $pay_text.addClass("fs-2 fw-bold px-3");
    var $checked = $pay_method.filter(':checked');
    if ($checked.length) {
        var val = $checked.val();
        if (val == 1) {
            $('.pay_info').removeClass('d-none');
        } else {
            $('.pay_info').addClass('d-none');
        }
        $pay_text.text($checked.data('title'));
    }
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
    } else if ($(`[name="InvoiceRadio"]`).length == 0) {
        InvoiceFilled = true;
    } else {
        switch ($(`[name="InvoiceRadio"]:checked`).val()) {
            case "order":
            case "recipient":
                InvoiceFilled = true;
                break;
        }
    }

    shipMethodsChosen = FormCheck(ShippingForms);
    payMethodsChosen = FormCheck(PaymentForms);
}
function ECLogisticsChange() {
    if (AllDataGet(false)) {
        ShippingForms.removeClass("d-none");
        $(".ecpayLogisticsWarning").addClass("d-none")
    }
    else {
        ShippingForms.addClass("d-none");
        $(".ecpayLogisticsWarning").removeClass("d-none")
    }
}
function ECPaymentChange() {
    if (!ECPayMonitor) return;

    Step3Monitor();

    $(".ecpayWarning").removeClass("d-none");
    $("#ECPayPayment").empty();

    if (AllDataGet(false)) {
        $(".ecpayWarning").addClass("d-none");
        $(".ecpay_loading").removeClass("d-none");
        $("#radio_payment_ECPay").prop("checked", true);
        $("input[name='RadioPayment']").prop("disabled", true);

        UpdatePaymentVisibility({
            isCOD: SupportCashOnDelivery,
            isECPayMode: true
        });

        var timeout = 0;
        var checkInterval = setInterval(function () {
            if (ECPayInit === true) {
                clearInterval(checkInterval);
                Coker.ThirdParty.ECPayGetToken(order_header_data).done(function (result) {
                    if (result.success) {
                        var message = result.message.split(",");
                        order_header_data.orderId = message[0];

                        ECPay.createPayment(message[1], ECPay.Language.zhTW, function (errMsg) {
                            if (errMsg != null) {
                                $(".ecpay_loading").text(`串接綠界發生錯誤(${errMsg})`);
                            } else {
                                var $ECPayList = $("#ECPayPayment .ecpay-pay-list-wrap .ecpay-pay-list > li");
                                $ECPayList.first().next('li').addClass("first");
                                $ECPayList.last().addClass("last");

                                $("#ECPayPayment").on("click", function () {
                                    var $this_radio = $("#radio_payment_ECPay");
                                    var $parentFormCheck = $this_radio.closest('.form-check');
                                    var $prevPayment = $parentFormCheck.prevAll('.form-check').first().find('.payment_display');
                                    $('#RadioPayment .payment_display').removeClass("checked first last");
                                    $this_radio.prop("checked", true);
                                    $('#RadioPayment .payment_display').first().addClass("first");
                                    $prevPayment.addClass('last');

                                    if ($(".ecpay_loading").hasClass("d-none")) {
                                        $ECPayList.removeClass("first last")
                                        var $activeLi = $ECPayList.filter('.ecpay-pl-act');

                                        if ($activeLi.prev('li').length == 0) {
                                            if ($('#RadioPayment .payment_display').length > 1) $prevPayment.addClass('last');
                                        }
                                        else {
                                            if ($('#RadioPayment .payment_display').length == 1) $ECPayList.first().addClass("first");
                                            $prevPayment.removeClass("last");
                                            $activeLi.prev('li').addClass("last");
                                        }
                                        $activeLi.addClass("first last")
                                        $activeLi.next('li').addClass("first");
                                        $ECPayList.last().addClass("last");
                                    }

                                    buy_step_swiper.update();
                                })

                                var checkPayExist = setInterval(function () {
                                    if (typeof window.Pay !== "undefined") {
                                        clearInterval(checkPayExist);
                                        $(".ecpay_loading").addClass("d-none");
                                        $("input[name='RadioPayment']").prop("disabled", false);
                                        buy_step_swiper.update();
                                    }
                                }, 1000);
                            }
                        }, 'V2');
                    } else {
                        $(".ecpay_loading").text(`串接綠界發生錯誤，請稍後嘗試`);
                        console.log(result.message)
                    }
                });
            } else {
                timeout += 100;
                if (timeout >= 10000) { // 最多等 10 秒
                    clearInterval(checkInterval);
                    $(".ecpay_loading").text(`串接綠界發生錯誤(初始化失敗-逾時)`);
                }
            }
        }, 100);
    } else {
        $(".ecpayWarning").removeClass("d-none");
        $("#RadioPayment div.form-check").addClass("d-none");
    }
}
function GetECPayType() {
    var $ECPayList = $("#ECPayPayment .ecpay-pay-list-wrap .ecpay-pay-list > li");
    var $activeLi = $ECPayList.filter('.ecpay-pl-act');
    $("#Step4 .payment_method").text($activeLi.find(".ecpay-pl-intro .ecpay-pl-type").text());
    switch ($activeLi.attr("id")) {
        case "CreditCard":
            order_header_data.payment = 16;
            break;
        case "CreditInstallment":
            var stage = $activeLi.find("select.ecpay-Installment").val();
            switch (stage) {
                case 3:
                    order_header_data.payment = 18;
                    $("#Step4 .payment_method").text("信用卡付款 (3期)");
                    break;
                case 6:
                    order_header_data.payment = 19;
                    $("#Step4 .payment_method").text("信用卡付款 (6期)");
                    break;
                case 12:
                    order_header_data.payment = 20;
                    $("#Step4 .payment_method").text("信用卡付款 (12期)");
                    break;
            }
            break;
        case "UnionPay":
            order_header_data.payment = 17;
            break;
        case "ATM":
            order_header_data.payment = 21;
            break;
        case "CVS":
            order_header_data.payment = 23;
            break;
        case "Barcode":
            order_header_data.payment = 22;
            break;
        case "ApplePay":
            order_header_data.payment = 27;
            break;
    }
}
function OrdererEdit(isopen) {
    if (isopen == null) {
        isopen = !OrdererOpen;
        OrdererEdit(isopen)
    }

    if (!OrdererOpen && isopen) {
        $("#OrdererForm > .default_data").addClass("d-none");
        $("#OrdererForm > form#Form_Orderer").removeClass("d-none");
        OrdererOpen = true;
        OrdererFilled = false;
    } else if (OrdererOpen && !isopen) {
        OrdererFilled = FormCheck(OrdererForms);
        if (!OrdererFilled) Coker.sweet.warning("請注意", "請確實填寫訂購人資料！", null);
        else {
            var data = co.Form.getJson($("#Form_Orderer").attr("id"), $("#OrdererForm .default_data"));
            data.ordererAddress = `${data.county}${data.district}${data.ordererAddress}`;
            ShoppingCartDataInsert(data, $("#OrdererForm .default_data"));
            $("#OrdererForm > .default_data").removeClass("d-none");
            $("#OrdererForm > form#Form_Orderer").addClass("d-none");
            OrdererOpen = false;
        }
    }
    if (HasECPay) ECPaymentChange();
    buy_step_swiper.update();
}
function RecipientRadio() {
    const value = $("[name='RecipientRadio']:checked").val();
    recipient_data = {};

    if (value == "edit") {
        $("#RecipientForm > .default_data").addClass("d-none");
        $("#RecipientForm > form").removeClass("d-none");
        RecipientOpen = true;
        RecipientFilled = false;
        RecipientFormClear();
    } else if (value == "order") {
        $("#RecipientForm > .default_data").addClass("d-none");
        $("#RecipientForm > form").addClass("d-none");
        RecipientOpen = false;
        RecipientFilled = true;
        RecipientSameOrderer();
    } else {
        $("#RecipientForm > .default_data").addClass("d-none");
        $("#RecipientForm > form").addClass("d-none");
        RecipientOpen = false;
        RecipientFilled = true;
    }
    if (HasECPay) ECPaymentChange();
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
    }
    if (HasECPay) ECPaymentChange();
    buy_step_swiper.update();
}
function PersonalInvoiceMode() {
    $(`#invoiceType .invoice-row`).addClass("d-none");
    switch (this.value) {
        case "mobile":
            $(`.invoice-row[data-personal="mobile"]`).removeClass("d-none");
            $(InvoicePersonalTypeForms).find("input").prop("required", true)
            $("#InvoiceForm").addClass("d-none");
            break;
        case "paper":
            $(`.invoice-row[data-personal="paper"]`).removeClass("d-none");
            $("#InvoiceForm").removeClass("d-none");
            $(InvoicePersonalTypeForms).find("input").prop("required", false)
            break;
    }
}
function InvoiceTypeRadio() {
    const value = $("[name='InvoiceType']:checked").val();
    $(`#invoiceType .invoice-block`).addClass("d-none");
    switch (value) {
        case "personal":
            $("#InvoiceInputPersonal").removeClass("d-none");
            $("#InvoiceForm").removeClass("d-none");
            break
        case "company":
            $("#Form_Invoice").removeClass("d-none");
            $("#InvoiceForm").removeClass("d-none");
            break
        case "none":
            $("#InvoiceInputNone").removeClass("d-none");
            $("#InvoiceForm").addClass("d-none");
            break
    }
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
    return Array.from(Forms).every(form => {
        const valid = form.checkValidity();
        form.classList.add('was-validated');
        return valid;
    });
}
function DetailsClear() {
    $("#Step1 > .card-body").addClass("d-none");
    $("#Purchase_Null").removeClass("d-none");
    buy_step_swiper.disable();
    refreshHasProds();
}
function DeleteRecipient() {
    var $this_parent = $(this).parents("tr");
    $this_parent.remove();
}
// 訂購人資料寫入 order_header_data
function OrderDataGet() {
    var shipping_radio = $(`[name="RadioShipping"]:checked`);
    order_header_data.shipping = shipping_radio.val();
    order_header_data.CVSStoreID = shipping_radio.attr("data-cvsstoreid") ?? null;
    order_header_data.CVSStoreName = shipping_radio.attr("data-cvsstorename") ?? null;
    order_header_data.CVSAddress = shipping_radio.attr("data-cvsaddress") ?? null;
    order_header_data.CVSTelephone = shipping_radio.attr("data-cvstelephone") ?? null;
    order_header_data.CVSOutSide = shipping_radio.attr("data-cvsoutside") ?? null;

    if (typeof (order_header_data.payment) != "undefined" && (!((order_header_data.payment >= 16 && order_header_data.payment <= 23) || order_header_data.payment === 27))) {
        order_header_data.payment = $(`[name="RadioPayment"]:checked`).val();
    }

    order_header_data.state = 1;
    order_header_data.subtotal = subtotal;
    order_header_data.discount = 0;
    order_header_data.bonus = 0;
    order_header_data.couponId = 0;
    order_header_data.freight = freight == "" ? 0 : freight;
    order_header_data.Service_Charge = 0;
    order_header_data.OrderDetails = shopping_cart_data;

    if (HasECPay) {
        if (window.ApplePaySession && typeof ApplePaySession.canMakePayments == "function") {
            order_header_data.SupportApplePay = true;
        }
    }
}
function OrdererDataGet() {
    order_data = co.Form.getJson($("#Form_Orderer").attr("id"));
    var country = order_data.county ? `${order_data.county} ` : "";
    var district = order_data.district ? `${order_data.district} ` : "";
    order_data.ordererZipCode = order_data.zipcode ? `${order_data.zipcode}` : "";
    order_data.ordererAddress = `${country}${district}${order_data.ordererAddress}`;
    if (order_data.ordererTelePhone != "" && order_data.zone != "") {
        order_data.ordererTelePhone = `${order_data.zone}-${order_data.ordererTelePhone}` + (order_data.ext == "" ? "" : `-${order_data.ext}`);
    }

    for (var key in order_data) {
        if (key.startsWith("orderer") > 0) order_header_data[key] = order_data[key]
    }

    if (!FormCheck(OrdererForms)) return false;
    return true;
}
// 收件人資料寫入 order_header_data
function RecipientDataGet() {
    var checkform = false;

    switch ($(`[name="RecipientRadio"]:checked`).val()) {
        case "order":
            RecipientSameOrderer();
            break;
        case "edit":
            recipient_data = co.Form.getJson($("#Form_Recipient").attr("id"));
            var country = recipient_data.county ? `${recipient_data.county} ` : "";
            var district = recipient_data.district ? `${recipient_data.district} ` : "";
            recipient_data.recipientZipCode = recipient_data.zipcode ? `${recipient_data.zipcode}` : "";
            recipient_data.recipientAddress = `${country}${district}${recipient_data.recipientAddress}`;
            recipient_data.recipientTelePhone = "";
            if (recipient_data.recipientTelePhone != "" && recipient_data.zone != "") {
                recipient_data.recipientTelePhone = `${recipient_data.zone}-${recipient_data.recipientTelePhone}` + (recipient_data.ext == "" ? "" : `-${recipient_data.ext}`);
            }
            checkform = true;
            break;
    }

    for (var key in recipient_data) {
        if (key.startsWith("recipient") > 0) order_header_data[key] = recipient_data[key]
    }

    if (checkform && !FormCheck(RecipientForms)) return false;
    return true;
}
// 發票資料寫入 order_header_data
function InvoiceDataGet() {
    var checkform = false;
    switch ($(`[name="InvoiceType"]:checked`).val()) {
        case "personal":
            if (InvoicePersonalTypeForms.length && !FormCheck(InvoicePersonalTypeForms)) return false;
            order_header_data.invoiceType = 1;
            switch ($(`[name="PersonalInvoiceMode"]:checked`).val()) {
                case "paper":
                    order_header_data.PersonalInvoiceType = 1;
                    break;
                case "mobile":
                    order_header_data.PersonalInvoiceType = 2;
                    invoiceType_data = co.Form.getJson("Form_InvoicePersonalType");
                    order_header_data.Carrier = invoiceType_data["MobileCarrier"];
                    break;
            }
            invoiceType_data.PersonalInvoiceType = order_header_data.PersonalInvoiceType;
            break;
        case "company":
            checkform = true
            invoice_data = co.Form.getJson($("#Form_Invoice").attr("id"));
            var country = invoice_data.county ? `${invoice_data.county} ` : "";
            var district = invoice_data.district ? `${invoice_data.district} ` : "";
            invoice_data.invoiceAddress = `${country}${district}${invoice_data.invoiceAddress}`;
            order_header_data.invoiceType = 2;
            break;
    }
    switch ($(`[name="InvoiceRadio"]:checked`).val()) {
        case "order":
            invoice_data['invoiceRecipient'] = 1;
            break;
        case "recipient":
            invoice_data['invoiceRecipient'] = 2;
            break;
    }
    for (var key in invoice_data) {
        order_header_data[key] = invoice_data[key]
    }
    if (checkform && !FormCheck(InvoiceForms)) return false;
    return true;
}
function AllDataGet(EnableWarning) {
    var checksuccess = true;

    RadioPayment();
    OrderDataGet();

    if (!OrdererDataGet()) {
        checksuccess = false;
        if (EnableWarning) Coker.sweet.warning("請注意", "請確實填寫訂購人資料！", null);
    }
    else {
        if (order_data.zone == "" ^ order_data.ordererTelePhone == "") {
            if (EnableWarning) Coker.sweet.warning("資料填寫錯誤", "如要提供訂購人電話資訊，請確實填寫區碼與聯絡電話。", null);
            checksuccess = false;
        }
    }

    if (!RecipientDataGet()) {
        checksuccess = false;
        if (EnableWarning) Coker.sweet.warning("請注意", "請確實填寫收件人資料！", null);
    }
    else {
        if ($(`[name="RecipientRadio"]:checked`).val() == "edit") {
            if (recipient_data.zone == "" ^ recipient_data.recipientTelePhone == "") {
                if (EnableWarning) Coker.sweet.warning("資料填寫錯誤", "如要提供收件人電話資訊，請確實填寫區碼與聯絡電話。", null);
                checksuccess = false;
            }
        }
    }

    if (!InvoiceDataGet()) {
        checksuccess = false;
        if (EnableWarning) Coker.sweet.warning("請注意", "請確實填寫發票資料！", null);
    }

    return checksuccess;
}
function getSelectedCartIds() {
    const ids = [];
    $('.purchase_group li.purchase_item input[name="buyItems"]:checked').each(function () {
        const $li = $(this).closest('li.purchase_item');
        const scid = $li.data('scId');            // ← 你已有：$template.data("scId", data.scId);
        if (scid != null) ids.push(Number(scid));
    });
    return ids;
}
async function OrderHeaderAdd() {
    var checksuccess = true;
    var paymentInfo = null;
    var ids = getSelectedCartIds();
    var data = shopping_cart_data.filter(e => ids.includes(e.Id));

    if (($("#RadioPayment > .form-check").length > 1 && $("#radio_payment_ECPay").length > 0 && $("#radio_payment_ECPay").prop("checked")) || ($(".ecpay_loading").is(":hidden") && $("#ECPayPayment").length > 0)) {
        GetECPayType();

        if (order_header_data.payment != 27) {
            co.sweet.loading();

            var validate = await ValidateECPayPayment();
            var validate_success = false
            var validate_result = "";
            if (validate.length > 1) {
                validate_success = validate.substring(0, 1) == 1 ? true : false;
                validate_result = validate.substring(1);
            }

            if (validate_success) {
                Swal.close();
                paymentInfo = JSON.parse(validate_result);
            } else checksuccess = false;
        }
    }

    if (typeof (order_header_data.payment) == "undefined") order_header_data.payment = $(`[name="RadioPayment"]:checked`).val();

    Coker.Order.CheckStock(data).done(function (result) {
        if (result.success) {
            var checksuccess = AllDataGet(true)

            var shipping_radio = $(`[name="RadioShipping"]:checked`);
            order_header_data.shipping = shipping_radio.val();
            order_header_data.CVSStoreID = shipping_radio.attr("data-cvsstoreid") ?? null;

            var shipping_radio = $(`[name="RadioShipping"][value="${order_header_data.shipping}"]`);
            var hasBtnGetMap = shipping_radio.siblings('.btn_getmap').length > 0;
            if (hasBtnGetMap && order_header_data.CVSStoreID == null) {
                checksuccess = false;
                if (EnableWarning) Coker.sweet.warning("請注意", "請選擇取貨門市！", null);
            }

            if (checksuccess) {
                var memberUpdateFailMessage = "";
                if ($(".memberUpdate").length > 0 ? $("#MemberUpdate").is(":checked") : false) {
                    Coker.Order.FrontUserUpdate(order_header_data).done(function (result) {
                        if (!result.success) {
                            memberUpdateFailMessage = `<br/>${result.message}`;
                        }
                    });
                }
                order_header_data.OrderDetails = order_header_data.OrderDetails.filter(e => ids.includes(e.Id));
                order_header_data.remark = $remark.val();
                if (typeof (order_header_data.remark) == "undefined" || order_header_data.remark == "") {
                    order_header_data.remark = "無";
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
                                        Coker.sweet.loading();
                                        Coker.ThirdParty.Request(result.message.split(",")[1], paymenttype, null).done(function (result) {
                                            Swal.close();
                                            if (result.success) {
                                                localStorage.setItem("lastSaveTime", new Date().toISOString())
                                                localStorage.setItem("lastSaveToken", localStorage.getItem("token"));
                                                $("#Step4 > .card-body > .pruchase_content > .status_alert").text("訂單已成立，即將進入付款流程。");
                                                setTimeout(function () {
                                                    buy_step_swiper.slideNext();
                                                    buy_step_swiper.update();
                                                    buy_step_swiper.disable();
                                                }, 300);
                                                $("#Step4 > .card-body .thirdpay_link a").attr("href", result.message);
                                                $("#Step4 > .card-body .thirdpay_link").removeClass("d-none");
                                                window.open(result.message, "_blank");
                                            } else {
                                                $("#Step4 > .card-body > .pruchase_content > .status_alert").text("付款流程發生未知錯誤，請稍後重新嘗試，或直接聯繫客服人員。");
                                                setTimeout(function () {
                                                    buy_step_swiper.slideNext();
                                                    buy_step_swiper.update();
                                                    buy_step_swiper.disable();
                                                }, 300);
                                            }
                                        });
                                        break;
                                    case "ECPay":
                                        co.sweet.loading();
                                        if (paymentInfo != null) {
                                            co.ThirdParty.ECPayCreatePayment(paymentInfo).done(function (result) {
                                                if (result.success) {
                                                    var result_obj = JSON.parse(result.message);
                                                    var SwalClose = false;
                                                    switch (result_obj.OrderInfo.PaymentType) {
                                                        case null:
                                                        case "Credit":
                                                        case "UnionPay":
                                                            localStorage.setItem("lastSaveTime", new Date().toISOString())
                                                            localStorage.setItem("lastSaveToken", localStorage.getItem("token"));
                                                            var VerifyURL = result_obj.ThreeDInfo?.ThreeDURL ?? result_obj.UnionPayInfo?.UnionPayURL;
                                                            $("#Step4 > .card-body > .pruchase_content > .status_alert").text("訂單已成立，即將進入驗證流程。");
                                                            $("#Step4 > .card-body .thirdpay_link a").attr({
                                                                href: VerifyURL,
                                                                title: "連結至：驗證頁面(開新視窗)",
                                                            });
                                                            $("#Step4 > .card-body .thirdpay_link").removeClass("d-none");
                                                            SwalClose = true;
                                                            window.open(VerifyURL, "_blank");
                                                            break;
                                                        case "ATM":
                                                            var ATMInfo = result_obj.ATMInfo;
                                                            $("#Step4 > .card-body > .pruchase_content > .status_alert").text(`訂單已成立，請於${ATMInfo.ExpireDate}前完成付款。`);
                                                            co.sweet.confirm("訂單付款資訊", `<div class="text-start">繳費銀行代碼：${ATMInfo.BankCode}<br>繳費虛擬帳號：${ATMInfo.vAccount}<br><br>請將此付款資訊截圖保存，並於繳費期限<span class="text-danger fw-bold">${ATMInfo.ExpireDate}</span>前完成繳費，感謝您的訂購。</div>`, "確定", "", null);
                                                            break;
                                                        case "CVS":
                                                            var CVSInfo = result_obj.CVSInfo;
                                                            $("#Step4 > .card-body > .pruchase_content > .status_alert").text(`訂單已成立，請於${CVSInfo.ExpireDate}前完成付款。`);
                                                            co.sweet.confirm("訂單付款資訊", `<div class="text-start">繳費代碼：${CVSInfo.PaymentNo}<br>或點此<a class="fw-bold text-primary px-1" href="${CVSInfo.PaymentURL}" target="_blank" title="連結至：繳費條碼(開新分頁)">連結</a>取得繳費條碼<br><br>請將此付款資訊截圖保存，並於繳費期限<span class="text-danger fw-bold">${CVSInfo.ExpireDate}</span>前完成繳費，感謝您的訂購。</div>`, "確定", "", null);
                                                            break;
                                                        case "BARCODE":
                                                            var BarcodeInfo = result_obj.BarcodeInfo;
                                                            $("#Step4 > .card-body > .pruchase_content > .status_alert").text(`訂單已成立，請於${BarcodeInfo.ExpireDate}前完成付款。`);
                                                            co.sweet.confirm("訂單付款資訊", `<div class="text-start"><svg id="barcode1" class="barcode_svg w-100"></svg><svg id="barcode2" class="barcode_svg w-100"></svg><svg id="barcode3" class="barcode_svg w-100"></svg><br><br>請將此付款資訊截圖保存，並於繳費期限<span class="text-danger fw-bold">${BarcodeInfo.ExpireDate}</span>前完成繳費，感謝您的訂購。<br><br>條碼載入需要一段時間，請耐心等候</div>`, "確定", "", null);
                                                            $.getScript("https://cdn.jsdelivr.net/npm/jsbarcode@3.11.5/dist/JsBarcode.all.min.js", function () {
                                                                JsBarcode("#barcode1", BarcodeInfo.Barcode1, { format: "CODE39", displayValue: true });
                                                                JsBarcode("#barcode2", BarcodeInfo.Barcode2, { format: "CODE39", displayValue: true });
                                                                JsBarcode("#barcode3", BarcodeInfo.Barcode3, { format: "CODE39", displayValue: true });
                                                            });
                                                            break;
                                                        default:
                                                            co.sweet.confirm(`回傳的PaymentType：${result_obj.OrderInfo.PaymentType}`, "此為測試訊息", "確認", "", null)
                                                            break;
                                                    }
                                                    setTimeout(function () {
                                                        buy_step_swiper.slideNext();
                                                        buy_step_swiper.update();
                                                        buy_step_swiper.disable();
                                                        if (SwalClose) Swal.close();
                                                    }, 300);
                                                } else {
                                                    $("#Step4 > .card-body > .pruchase_content > .status_alert").html(`<div>付款流程發生錯誤，${result.message + '<br>'}請稍後重新嘗試，或直接聯繫客服人員。</div>`);
                                                    setTimeout(function () {
                                                        buy_step_swiper.slideNext();
                                                        buy_step_swiper.update();
                                                        buy_step_swiper.disable();
                                                        Swal.close();
                                                    }, 300);
                                                }
                                            })
                                        } else if (order_header_data.payment == 27) {
                                            $("#Step4 > .card-body > .pruchase_content > .status_alert").text(`訂單已成立，謝謝您的訂購！。`);
                                            setTimeout(function () {
                                                buy_step_swiper.slideNext();
                                                buy_step_swiper.update();
                                                buy_step_swiper.disable();
                                                Swal.close();
                                            }, 300);
                                        } else {
                                            $("#Step4 > .card-body > .pruchase_content > .status_alert").html(`<div>付款流程發生錯誤，請稍後重新嘗試，或直接聯繫客服人員。</div>`);
                                            setTimeout(function () {
                                                buy_step_swiper.slideNext();
                                                buy_step_swiper.update();
                                                buy_step_swiper.disable();
                                                Swal.close();
                                            }, 300);
                                        }
                                        break;
                                    case "Default":
                                        setTimeout(function () {
                                            buy_step_swiper.slideNext();
                                            buy_step_swiper.update();
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
function ValidateECPayPayment() {
    return new Promise((resolve) => {
        ECPay.getPayToken(function (paymentInfo, errMsg) {
            if (errMsg != null) {
                co.sweet.warning("請確實填寫付款資料", errMsg, null);
                resolve(`2${errMsg}`);
            } else {
                resolve(`1${JSON.stringify(paymentInfo)}`);
            }
        });
    });
}
function OrderSuccess(result) {
    var message = result.message.split(",");
    var order_header_id = message[1];

    step4PaidDateHtml = message.length > 3 ? (message[3] || "") : "";

    CartClear();

    $("#Step4 > .card-header > .order_number")
        .text(("000000000" + order_header_id).substring(order_header_id.length));

    $("#Step4 > .card-body .pruchase_content .order_time")
        .text(`訂單成立時間：${message[2]}`);

    $("#Step4 > .card-body > .pruchase_content > .status_alert")
        .text("訂單已成立，謝謝您的訂購！");

    Coker.Order.GetAllData(order_header_id, true).done(function (results) {
        if (results && results.length > 0) {
            SuccessPageDataInsert(results[0]);
        }
    });

    if ($(".storememo").text() != "") {
        Swal.fire({
            title: "小提醒",
            icon: "info",
            html: $(".storememo").text().replaceAll("\n", "<br/>"),
            focusConfirm: false,
            confirmButtonText: "確認",
        }).then(function () {
            if (result.error != null) {
                if (!islogin) {
                    Coker.sweet.warning("信件發送失敗", "訂購信件發送失敗，請註冊會員以查看詳細訂單，或將訂單完成頁面截圖。", null);
                } else {
                    Coker.sweet.warning("信件發送失敗", "訂購信件發送失敗，訂單詳細可於會員管理歷史訂單中查看。", null);
                }
            }
        });
    } else {
        if (result.error != null) {
            if (!islogin) {
                Coker.sweet.warning("信件發送失敗", "訂購信件發送失敗，請註冊會員以查看詳細訂單，或將訂單完成頁面截圖。", null);
            } else {
                Coker.sweet.warning("信件發送失敗", "訂購信件發送失敗，訂單詳細可於會員管理歷史訂單中查看。", null);
            }
        }
    }
}
function PurchaseAdd(result, item_list_ul) {
    var item = $($("#Template_Purchase_Details").html()).clone();
    var item_link = item.find(".pro_link"),
        item_image = item.find(".pro_image"),
        item_name = item.find(".pro_name"),
        item_specification = item.find(".pro_specification"),
        item_unit = item.find(".pro_unit"),
        item_unitBonus = item.find(".pro_bonus"),
        item_quantity = item.find(".pro_quantity"),
        item_subtotal = item.find(`.pro_subtotal > [data-key="subtotal"]`),
        item_subtotal_bonus = item.find(`.pro_subtotal > [data-key="subtotalBonus"]`);

    item_link.attr("href", `/${OrgName}/Home/product/` + result.pId);
    item_link.attr("title", `連結至：${result.title}(另開新視窗)`);
    item_image.attr("src", result.imagePath.replace(`upload/${OrgName}/`, "upload/"));
    item_name.text(result.title);
    item_specification.append(result.s1Title == "" ? "" : '<span class="border px-1 me-1">' + result.s1Title + '</span>')
    item_specification.append(result.s2Title == "" ? "" : '<span class="border px-1">' + result.s2Title + '</span>')
    if (result.price > 0)
        item_unit.text(`$${(result.price).toLocaleString('en-US')}`)
    if (result.bonusPrice > 0)
        item_unitBonus.text(`紅利：${(result.bonusPrice).toLocaleString('en-US')}`)
    item_quantity.text(result.quantity);
    if (result.price > 0)
        item_subtotal.text((result.price * result.quantity).toLocaleString('en-US'))
    if (result.bonusPrice > 0)
        item_subtotal_bonus.text("紅利：" + (result.bonusPrice * result.quantity).toLocaleString('en-US'))

    item_list_ul.append(item);
}
function HiddenCode($self) {
    $self.find("*").each(function () {
        var $this = $(this);
        var key = $this.data("key");
        if (typeof (key) != "undefined") {
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

    data = data || {};

    $self.find("[data-key]").each(function () {
        var $this = $(this);
        var key = $this.data("key");

        if (typeof key === "undefined" || !key) return;

        var value = getValueIgnoreCase(data, key);

        if (value == null) {
            value = "";
        }

        if ($this.hasClass("price")) {
            var num = toNumberValue(value);
            $this.text(num > 0 ? num.toLocaleString() : "");
        } else {
            $this.text(value);
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
    $Frame.empty();
    $CollapseFrame.empty();

    $.each(datas || [], function (index, data) {
        var $html = $($Template.html()).clone();

        $html.find("[data-key]").each(function () {
            var $this = $(this);
            var key = $this.data("key");

            if (typeof key === "undefined" || !key) return;

            switch (key) {
                case "link":
                    $this.attr({
                        href: `/${OrgName}/Home/product/${getValueIgnoreCase(data, "prodId")}`,
                        title: `連結至：${getValueIgnoreCase(data, "title") || ""}(另開新視窗)`
                    });
                    break;

                case "imagePath": {
                    var imagePath = String(getValueIgnoreCase(data, "imagePath") || "/images/noImg.jpg");
                    imagePath = imagePath.replace(`/${OrgName}/`, "/");
                    $this.attr({
                        src: imagePath,
                        alt: getValueIgnoreCase(data, "title") || ""
                    });
                    break;
                }

                case "spec": {
                    var s1 = getValueIgnoreCase(data, "s1Title") || "";
                    var s2 = getValueIgnoreCase(data, "s2Title") || "";

                    $this.empty();
                    if (s1 !== "") $this.append(`<span class="border px-1 me-1">${s1}</span>`);
                    if (s2 !== "") $this.append(`<span class="border px-1 me-1">${s2}</span>`);
                    break;
                }

                case "price": {
                    var price = toNumberValue(getValueIgnoreCase(data, "price"));
                    $this.text(price > 0 ? `$${price.toLocaleString()}` : "");
                    break;
                }

                case "bonus": {
                    var bonus = toNumberValue(getValueIgnoreCase(data, "bonus"));
                    if (bonus > 0) {
                        $this.text(`紅利：${bonus.toLocaleString()}`);
                        $this.removeClass("d-none");
                    } else {
                        $this.text("");
                        $this.addClass("d-none");
                    }
                    break;
                }

                case "subtotal": {
                    var subtotal = toNumberValue(getValueIgnoreCase(data, "subtotal"));
                    $this.text(subtotal > 0 ? `$${subtotal.toLocaleString()}` : "");
                    break;
                }

                case "subtotalBonus": {
                    var subtotalBonus = toNumberValue(getValueIgnoreCase(data, "subtotalBonus"));
                    if (subtotalBonus > 0) {
                        $this.text(`紅利：${subtotalBonus.toLocaleString()}`);
                        $this.removeClass("d-none");
                    } else {
                        $this.text("");
                        $this.addClass("d-none");
                    }
                    break;
                }

                case "quantity": {
                    var qty = toNumberValue(getValueIgnoreCase(data, "quantity"));
                    $this.text(`× ${qty.toLocaleString()}`);
                    break;
                }

                default: {
                    var value = getValueIgnoreCase(data, key);
                    if (value == null) value = "";

                    if ($this.hasClass("price")) {
                        var num = toNumberValue(value);
                        $this.text(num > 0 ? num.toLocaleString() : "");
                    } else {
                        $this.text(value);
                    }
                    break;
                }
            }
        });

        if (index === 0) {
            $Frame.append($html);
        } else {
            $(".btn_view_list").removeClass("d-none");
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
        class: "bill_city form-select"
    });
    $county.append("<label class='px-4' for='InvoiceSelectCity'>縣市</label>");
    var $county_first_option = $county.children('select').children('option').first();
    $county_first_option.text("請選擇縣市");
    $county_first_option.attr('disabled', 'disabled');

    $district.children('select').attr({
        id: "InvoiceSelectTown",
        class: "bill_town form-select"
    });
    $district.append("<label class='px-4' for='InvoiceSelectCity'>鄉鎮</label>");
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
    co.sweet.confirm("刪除收件人", "確定刪除？資料刪除後不可復原", "確　定", "取　消", function () {
        //co.Tag.TagDelete(e.row.key).done(function () {
        //    RecipientsList_dxData.refresh();
        //})
    })
}
function ValidateCartOnInit() {
    if (!shopping_cart_data || shopping_cart_data.length === 0) return;

    var payload = shopping_cart_data
        .filter(function (x) { return x.Quantity > 0; })
        .map(function (x) {
            return { Id: x.Id, Quantity: x.Quantity };
        });

    if (payload.length === 0) return;

    Product.Update.MultiCart(payload).done(function (result) {
        var batch = result.object || result.Object;
        if (!batch) return;

        var items = batch.items || batch.Items || [];
        if (!items || !items.length) return;

        items.forEach(function (item) {
            var cartId = item.cartId || item.CartId;
            var success = (item.success === true || item.Success === true);
            var removed = (item.removed === true || item.Removed === true);
            var msg = item.message || item.Message || '';
            var errorCode = item.error || item.Error || '';

            if (!cartId) return;

            var $li = $('li.purchase_item').filter(function () {
                return $(this).data('scId') === cartId;
            });
            if (!$li.length) return;

            // 先清掉舊狀態
            $li.removeClass('cart-item-error');
            $li.find('.js-stock-error').remove();

            var $itemCheckbox = $li.find('input[name="buyItems"]');
            // 預設先解鎖，避免舊狀態殘留
            $itemCheckbox.prop('disabled', false);

            // ✅ 成功 & 未被後端標記移除 → 不處理
            if (success && !removed) return;

            // ❌ 有錯誤的品項：加上錯誤標記、取消勾選並禁用
            $li.addClass('cart-item-error');

            if ($itemCheckbox.length) {
                $itemCheckbox.prop('checked', false);
                $itemCheckbox.prop('disabled', true);
            }

            var $content = $li.find('.content');
            if (!$content.length) $content = $li;
            var $msgDiv = $('<div class="js-stock-error text-danger small mt-1"></div>');
            $msgDiv.text(msg || '此商品目前無法購買，請調整或移除。');
            $content.append($msgDiv);
        });

        TotalCount();
    }).fail(function () {
        // 驗證失敗就當沒發生，不影響使用者操作
    });
}

function getSelectedShippingMeta() {
    var $selected = $("[name='RadioShipping']:checked");
    if (!$selected.length) {
        return {
            id: 0,
            freightType: 0,
            discountFreightType: null,
            freight: 0,
            lowCon: 0,
            disFreight: 0,
            boxFees: []
        };
    }

    var rawBoxFees = $selected.attr("data-boxfees");
    var parsedBoxFees = [];

    if (rawBoxFees) {
        try {
            parsedBoxFees = JSON.parse(rawBoxFees);
        } catch (err) {
            console.error("data-boxfees parse failed", err, rawBoxFees);
            parsedBoxFees = [];
        }
    }

    var rawDiscountFreightType = $selected.attr("data-discount-freight-type");

    return {
        id: Number($selected.val() || 0),
        freightType: Number($selected.data("freight-type") || 0),
        discountFreightType: rawDiscountFreightType === "" || rawDiscountFreightType == null
            ? null
            : Number(rawDiscountFreightType),
        freight: Number($selected.data("freight") || 0),
        lowCon: Number($selected.data("lowcon") || 0),
        disFreight: Number($selected.data("disfreight") || 0),
        boxFees: Array.isArray(parsedBoxFees) ? parsedBoxFees : []
    };
}
function calculateDiscountTargetFreight(baseFreight, shippingMeta) {
    var originFreight = Number(baseFreight || 0);
    var disFreight = Number(shippingMeta.disFreight || 0);
    var discountType = shippingMeta.discountFreightType == null
        ? null
        : Number(shippingMeta.discountFreightType);

    if (discountType == null) {
        return originFreight;
    }

    // 1 = 指定折抵後運費：Dis_Freight 本身就是最終運費
    if (discountType === 1) {
        return Math.max(0, disFreight);
    }

    // 2 = 折抵固定運費金額：Dis_Freight 是折抵金額，所以要從原運費扣掉
    if (discountType === 2) {
        return Math.max(0, originFreight - Math.max(0, disFreight));
    }

    return originFreight;
}
function shouldShowShippingShortage(freightResult) {
    return Number(freightResult.shortage || 0) > 0;
}
function applyDiscountFreight(baseFreight, productSubtotal, shippingMeta) {
    var originFreight = Number(baseFreight || 0);
    var freeThreshold = Number(shippingMeta.lowCon || 0);
    var disFreight = Number(shippingMeta.disFreight || 0);
    var discountType = shippingMeta.discountFreightType == null
        ? null
        : Number(shippingMeta.discountFreightType);

    // 沒有門檻 / 未達門檻 / 未設定折抵方式 => 原運費
    if (freeThreshold <= 0 || productSubtotal < freeThreshold || discountType == null) {
        return originFreight;
    }

    // 1 = 指定折抵後運費
    if (discountType === 1) {
        return Math.max(0, disFreight);
    }

    // 2 = 折抵固定運費金額
    if (discountType === 2) {
        return Math.max(0, originFreight - Math.max(0, disFreight));
    }

    return originFreight;
}
function getSelectedCartItems() {
    var ids = getSelectedCartIds();
    return shopping_cart_data.filter(function (x) {
        return ids.includes(Number(x.Id));
    });
}

function getSelectedPackingPoint() {
    var items = getSelectedCartItems();
    return items.reduce(function (sum, item) {
        var point = Number(item.PackingPoint || 0);
        var qty = Number(item.Quantity || 0);
        return sum + (point * qty);
    }, 0);
}

function calculateNormalFreight(productSubtotal, shippingMeta) {
    var originFreight = Number(shippingMeta.freight || 0);
    var freeThreshold = Number(shippingMeta.lowCon || 0);

    if (freeThreshold > 0 && productSubtotal >= freeThreshold) {
        return calculateDiscountTargetFreight(originFreight, shippingMeta);
    }

    return originFreight;
}

function calculateBoxFreightByTotalCapacity(totalPackingPoint, fees) {
    var list = (fees || [])
        .map(function (x) {
            return {
                logisticsBoxId: Number(x.LogisticsBoxId || x.logisticsBoxId || 0),
                name: x.Name || x.name || "",
                capacityPoint: Number(
                    x.CapacityPoint ??
                    x.capacityPoint ??
                    0
                ),
                fee: Number(
                    x.Fee ??
                    x.fee ??
                    0
                )
            };
        })
        .filter(function (x) {
            return x.capacityPoint > 0 && x.fee >= 0;
        })
        .sort(function (a, b) {
            if (a.capacityPoint !== b.capacityPoint) return a.capacityPoint - b.capacityPoint;
            return a.fee - b.fee;
        });

    if (!list.length || totalPackingPoint <= 0) {
        return {
            freight: 0,
            boxes: [],
            totalPackingPoint: totalPackingPoint || 0
        };
    }

    // 單位容量成本最低的箱型優先
    var bestUnitBox = list[0];
    var bestUnitCost = bestUnitBox.fee / bestUnitBox.capacityPoint;

    for (var i = 1; i < list.length; i++) {
        var unitCost = list[i].fee / list[i].capacityPoint;
        if (unitCost < bestUnitCost) {
            bestUnitCost = unitCost;
            bestUnitBox = list[i];
        }
    }

    var remaining = totalPackingPoint;
    var usedBoxes = [];
    var totalFreight = 0;

    if (remaining > bestUnitBox.capacityPoint) {
        var count = Math.floor(remaining / bestUnitBox.capacityPoint);
        if (count > 0) {
            usedBoxes.push({
                logisticsBoxId: bestUnitBox.logisticsBoxId,
                name: bestUnitBox.name,
                capacityPoint: bestUnitBox.capacityPoint,
                fee: bestUnitBox.fee,
                count: count
            });
            totalFreight += count * bestUnitBox.fee;
            remaining -= count * bestUnitBox.capacityPoint;
        }
    }

    if (remaining > 0) {
        var fitBox = list.find(function (x) {
            return x.capacityPoint >= remaining;
        });

        if (!fitBox) {
            fitBox = list[list.length - 1];
            var extraCount = Math.ceil(remaining / fitBox.capacityPoint);
            totalFreight += extraCount * fitBox.fee;
            usedBoxes.push({
                logisticsBoxId: fitBox.logisticsBoxId,
                name: fitBox.name,
                capacityPoint: fitBox.capacityPoint,
                fee: fitBox.fee,
                count: extraCount
            });
        } else {
            totalFreight += fitBox.fee;
            usedBoxes.push({
                logisticsBoxId: fitBox.logisticsBoxId,
                name: fitBox.name,
                capacityPoint: fitBox.capacityPoint,
                fee: fitBox.fee,
                count: 1
            });
        }
    }

    var merged = [];
    usedBoxes.forEach(function (box) {
        var exists = merged.find(function (x) {
            return x.logisticsBoxId === box.logisticsBoxId;
        });
        if (exists) {
            exists.count += box.count;
        } else {
            merged.push({
                logisticsBoxId: box.logisticsBoxId,
                name: box.name,
                capacityPoint: box.capacityPoint,
                fee: box.fee,
                count: box.count
            });
        }
    });

    return {
        freight: totalFreight,
        boxes: merged,
        totalPackingPoint: totalPackingPoint
    };
}

function calculateFreight(productSubtotal) {
    var shippingMeta = getSelectedShippingMeta();
    var shortage = 0;
    var lowConValue = Number(shippingMeta.lowCon || 0);

    if (lowConValue > 0 && productSubtotal < lowConValue) {
        shortage = lowConValue - productSubtotal;
    }

    switch (shippingMeta.freightType) {
        case 1: // 免運費
            return {
                freight: 0,
                mode: 1,
                shortage: 0,
                boxResult: null,
                originFreight: 0,
                targetFreight: 0
            };

        case 3: { // 依箱計費
            var totalPackingPoint = getSelectedPackingPoint();
            var boxResult = calculateBoxFreightByTotalCapacity(totalPackingPoint, shippingMeta.boxFees);

            // 箱型原始運費必須用實際配箱結果，不可用 radio data-freight
            var boxOriginFreight = Number(boxResult.freight || 0);

            // targetFreight 一律代表「達門檻後的最終運費」
            var targetFreight = calculateDiscountTargetFreight(boxOriginFreight, shippingMeta);

            var finalFreight = shortage > 0 ? boxOriginFreight : targetFreight;

            return {
                freight: Number(finalFreight || 0),
                mode: 3,
                shortage: shortage,
                boxResult: boxResult,
                originFreight: boxOriginFreight,
                targetFreight: targetFreight
            };
        }

        case 2: // 單筆計算
        default: {
            var originFreight = Number(shippingMeta.freight || 0);

            // targetFreight 一律代表「達門檻後的最終運費」
            var targetFreight = calculateDiscountTargetFreight(originFreight, shippingMeta);

            var finalFreight = shortage > 0 ? originFreight : targetFreight;

            return {
                freight: Number(finalFreight || 0),
                mode: 2,
                shortage: shortage,
                boxResult: null,
                originFreight: originFreight,
                targetFreight: targetFreight
            };
        }
    }
}