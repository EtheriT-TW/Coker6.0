// wwwroot/view-resources/ShoppingCart/shopping-cart.init.js
(function (cart, $) {
    "use strict";

    var S = cart.State;
    cart.Init = cart.Init || {};

    function PageReady() {
        // Bootstrap Modal 不應留在 Swiper 的 transform 容器內，否則底部按鈕可能被遮罩攔截。
        var $addOnModal = $('#CartProductAddOnModal');
        if ($addOnModal.length && !$addOnModal.parent().is('body')) {
            $addOnModal.appendTo(document.body);
        }

        $('#RadioPayment .payment_display').on("click", function (event) {
            var $formCheck = $(this).closest('.form-check');
            var $radio = $formCheck.find('input[name="RadioPayment"]').first();

            if ($radio.prop("disabled")) {
                event.preventDefault();
                event.stopImmediatePropagation();
                return;
            }

            cart.Payment.Core.updatePaymentRadioUI($formCheck);
            cart.Payment.Core.RadioPayment();
        });

        // 群組全選（Header）
        $(document).on('change', '.purchase_group .js-group-check', function () {
            const $group = $(this).closest('.purchase_group');
            const checked = this.checked;

            const $validItems = $group.find('input[name="buyItems"]:enabled');

            if ($validItems.length === 0) {
                $(this).prop('checked', false);
                return;
            }

            if (checked) cart.Items.clearOtherGroupsExcept($group);

            $group.find('.js-group-check').prop('indeterminate', false);

            $validItems.prop('checked', checked);
            cart.Items.syncAdditionalSelection($group);

            cart.Items.updateGroupSelectedSubtotal($group);
            cart.Pricing.TotalCount();
            cart.Pricing.updateNextStepByBonus();
            cart.Payment.Core.onAmountChanged();
            if (cart.Marketing && typeof cart.Marketing.refreshProductAddOnPrompt === 'function') {
                cart.Marketing.refreshProductAddOnPrompt();
            }
        });

        // 單一品項
        $(document).on('change', '.purchase_group li.purchase_item input[name="buyItems"]', function () {
            const $group = $(this).closest('.purchase_group');

            if ($(this).closest('.purchase_item').hasClass('cart-additional-item')) {
                cart.Items.syncAdditionalSelection($group);
                return;
            }

            if (this.checked) cart.Items.clearOtherGroupsExcept($group); // 互斥：勾任何一個品項就清其他組

            cart.Items.syncAdditionalSelection($group);

            cart.Items.updateGroupSelectedSubtotal($group);
            cart.Pricing.TotalCount();
            cart.Pricing.updateNextStepByBonus();
            cart.Payment.Core.onAmountChanged();
            if (cart.Marketing && typeof cart.Marketing.refreshProductAddOnPrompt === 'function') {
                cart.Marketing.refreshProductAddOnPrompt();
            }
        });

        cart.Payment.Core.initAll();

        $("#btn_car_dropdown").addClass("d-none")

        /* Buy Swiper */
        S.buy_step_swiper = new Swiper("#BuyStepSwiper > .swiper", {
            a11y: true,
            slidesPerView: 1,
            spaceBetween: 15,
            autoHeight: true,
            loop: false,
            enabled: false,
            allowTouchMove: false,
            simulateTouch: false,
            returnFocus: false,
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

        var nextStepButton = document.querySelector('.btn_swiper_next_buystep');
        if (nextStepButton && nextStepButton.dataset.marketingValidationBound !== 'true') {
            nextStepButton.dataset.marketingValidationBound = 'true';
            nextStepButton.addEventListener('click', function (event) {
                if (!S.buy_step_swiper || S.buy_step_swiper.activeIndex !== 0) return;
                if (cart.Marketing && typeof cart.Marketing.validateProductAddOnBeforeNext === 'function' &&
                    !cart.Marketing.validateProductAddOnBeforeNext()) {
                    event.preventDefault();
                    event.stopImmediatePropagation();
                }
            }, true);
        }

        S.buy_step_swiper.on('slideChangeTransitionEnd', function () {
            if (S.gotop_switch) {
                window.scrollTo(0, $("#BuyStepSwiper").offset().top - $("#Mega_Menu").height() - 90);
            }
        });

        S.buy_step_swiper.on('slideChange', function () {
            switch (S.buy_step_swiper.activeIndex) {
                case 1:
                    const hasSelected = cart.Items.getSelectedCartIds().length > 0;
                    var $errorSelected = $('li.purchase_item.cart-item-error input[name="buyItems"]:checked');

                    const { bonus } = cart.Pricing.computeSelectedSubtotal();
                    const memberBonus = Number(totalBonus || 0);
                    const bonusNotEnough = Number(bonus || 0) > memberBonus;

                    if (!S.hasProds) {
                        Coker.sweet.warning("錯誤", "無可購買商品。", null, false);
                        S.buy_step_swiper.slideTo(0);
                    } else if (!hasSelected) {
                        Coker.sweet.warning("請注意", "請先勾選要結帳的商品（至少 1 項）。", null);
                        S.buy_step_swiper.slideTo(0);
                        return;
                    } else if ($errorSelected.length > 0) {
                        Coker.sweet.warning(
                            "無法結帳",
                            "您選取的商品中包含已下架或庫存不足的品項，請先調整或移除後再繼續結帳。",
                            null,
                            false
                        );
                        S.buy_step_swiper.slideTo(0);
                        return;
                    } else if (bonusNotEnough) {
                        Coker.sweet.warning(
                            "紅利不足",
                            `目前已選商品需紅利 ${Number(bonus || 0).toLocaleString()} 點，會員可用紅利 ${memberBonus.toLocaleString()} 點，請先調整商品或數量。`,
                            null,
                            false
                        );
                        S.buy_step_swiper.slideTo(0);
                        return;
                    } else {
                        var select_cart_data = S.shopping_cart_data
                            .filter(item => cart.Items.getSelectedCartIds().includes(item.Id))
                            .reverse();

                        var isdefault = true;

                        for (var i = 0; i < select_cart_data.length; i++) {
                            var $select_input = $('input[data-subtype="' + select_cart_data[i].logisticsSubType + '"]');
                            if ($select_input.length > 0) {
                                var $radio = $select_input
                                    .closest(".shipping-option-row")
                                    .find('input[name="RadioShipping"]');
                                if (!$radio.length) continue;

                                $radio.prop('checked', true);
                                $radio.attr({
                                    "data-cvsstoreid": select_cart_data[i].cvsStoreID,
                                    "data-cvsstorename": select_cart_data[i].cvsStoreName,
                                    "data-cvsaddress": select_cart_data[i].cvsAddress,
                                    "data-cvstelephone": select_cart_data[i].cvsTelephone,
                                    "data-cvsoutside": select_cart_data[i].cvsOutSide,
                                })
                                cart.Shipping.RadioShipping();
                                isdefault = false;
                                break;
                            }
                        }

                        if (isdefault) $('input[data-isdefault="True"][name="RadioShipping"]').prop('checked', true);

                        enforceFreightVisibility();
                        S.OrdererFilled = cart.Forms.FormCheck(S.OrdererForms);
                        S.RecipientFilled = cart.Forms.FormCheck(S.RecipientForms);
                        S.InvoiceFilled = cart.Forms.FormCheck(S.InvoiceForms);
                        if (!S.OrdererFilled) {
                            if (S.OrdererOpen) $("#OrdererForm>form").removeClass('was-validated')
                            cart.Forms.OrdererEdit(true, true);
                            $("#radio_recipient_order").trigger("change");
                            $("#radio_bill_orderer").trigger("change");
                        }
                        //商品數量變更這邊沒有動到 先移除
                        var $checkedShipping = $("[name='RadioShipping']:checked");

                        if ($checkedShipping.length > 0 && cart.Payment.Availability) {
                            cart.Payment.Availability.refresh(
                                cart.Payment.Core.getActivePaymentValue()
                            );

                            // 從綠界超商地圖返回時，restoreOrderForm() 會負責在資料恢復完成後重載金流。
                        }
                    }
                    break;
                case 2:
                    if (!S.isCheckout) {
                        if (S.OrdererOpen) { S.OrdererFilled = cart.Forms.FormCheck(S.OrdererForms) };
                        if (S.RecipientOpen) { S.RecipientFilled = cart.Forms.FormCheck(S.RecipientForms) };
                        if (S.InvoiceOpen) { S.InvoiceFilled = cart.Forms.FormCheck(S.InvoiceForms) };
                        cart.Payment.Core.RadioPayment();
                        if (S.ShippingForms.find(".noshipping").length > 0) {
                            Coker.sweet.warning("請注意", "店家尚未設置運費方式，無法繼續", null);
                            S.buy_step_swiper.slideTo(1);
                        } else if (S.PaymentForms.find(".nopayment").length > 0) {
                            Coker.sweet.warning("請注意", "店家尚未設置付款方式，無法繼續", null);
                            S.buy_step_swiper.slideTo(1);
                        } else {
                            S.shipMethodsChosen = cart.Forms.FormCheck(S.ShippingForms);
                            S.payMethodsChosen = cart.Forms.FormCheck(S.PaymentForms);
                            if (!(S.shipMethodsChosen && S.payMethodsChosen)) {
                                Coker.sweet.warning("請注意", "請確實選擇運送及付款方式！", null);
                                setTimeout(function () {
                                    S.buy_step_swiper.slideTo(1);
                                }, 1500);
                            }
                        }
                        Coker.sweet.warning("未完成結帳流程！", "若資料已確實填寫完畢，請點選下方[確認付款]按鈕進入付款程序", null);
                        setTimeout(function () {
                            S.buy_step_swiper.slideTo(1);
                        }, 1500);
                    }
                    break;
            }
        });

        $('#CollapsePurchase')
            .on('shown.bs.collapse', function () {
                S.buy_step_swiper.update();
                $("body").css("height", "auto");
                $(window).trigger("resize");
            })
            .on('hidden.bs.collapse', function () {
                S.buy_step_swiper.update();
                $("body").css("height", "auto");
                $(window).trigger("resize");
            });

        if ("onhashchange" in window) {
            window.onhashchange = cart.CheckoutResult.hashChange;
        } else {
            setInterval(cart.CheckoutResult.hashChange, 1000);
        }
        cart.CheckoutResult.GetOrderPage();

        Coker.Token.CheckToken().done(function (checkresult) {
            S.islogin = checkresult.isLogin;
            Coker.User.GetUser().done(function (result) {
                if (result.success) {
                    var data_insert = true;
                    S.user_data['orderer'] = result.data.name;
                    S.user_data['ordererSex'] = result.data.sex;
                    S.user_data['ordererEmail'] = result.data.email;

                    if (result.data.cellPhone == null) data_insert = false;
                    S.user_data['ordererCellPhone'] = result.data.cellPhone;

                    if (result.data.telPhone != null) {
                        S.user_data['zone'] = (result.data.telPhone).split('-')[0];
                        S.user_data['ordererTelePhone'] = (result.data.telPhone).split('-')[1];
                        S.user_data['ext'] = (result.data.telPhone).split('-')[2];
                    } else {
                        S.user_data['zone'] = null;
                        S.user_data['ordererTelePhone'] = null;
                        S.user_data['ext'] = null;
                    }

                    if (result.data.address != null) {
                        S.user_data['county'] = (result.data.address).split(' ')[0];
                        S.user_data['district'] = (result.data.address).split(' ')[1];
                        S.user_data['ordererAddress'] = (result.data.address).split(' ')[2];
                    } else {
                        data_insert = false;
                        S.user_data['county'] = null;
                        S.user_data['district'] = null;
                        S.user_data['ordererAddress'] = null;
                    }
                    S.user_data['address'] = result.data.address;

                    if (!data_insert) {
                        cart.Forms.OrdererEdit(true);
                        $('#MemberUpdate').prop('checked', true);
                    }

                    co.Form.insertData(S.user_data, "#Form_Orderer");

                    S.order_data = S.user_data;
                    S.order_data.ordererAddress = S.user_data['address'];
                    cart.Utils.ShoppingCartDataInsert(S.order_data, $("#OrdererForm .default_data"));
                    cart.Forms.RecipientSameOrderer();

                    co.Zipcode.setData({
                        el: $("#Orderer_TWzipcode"),
                        addr: S.order_data.ordererAddress
                    });
                } else S.user_data = null;
            });
        });

        cart.Forms.ElementInit();
        if (cart.Recipients) cart.Recipients.Init();

        $("#OrdererForm :input, #RecipientForm :input, #InvoiceForm :input, #Form_Invoice :input, #Form_InvoicePersonalType :input")
            .not("[name='RadioShipping']")
            .not("[name='RadioPayment']")
            .not("[name='RecipientRadio']")
            .not("[name='InvoiceRadio']")
            .not("[name='InvoiceType']")
            .not("[name='PersonalInvoiceMode']")
            .on("change", function () {
                cart.Payment.Core.onAmountChanged();
                cart.Payment.Core.reloadActiveEmbeddedProvider();
            });

        $(".btn_call_login").on("click", function (event) {
            loginModal.show();
        })

        /* 根據畫面高度判斷切換Swiper是否滑動到上方 */
        S.top_position = $(".swiper").offset().top;

        $(window).scroll(function () {
            var topPosition = $(".swiper").offset().top - $("header").height();
            if (document.body.scrollTop > topPosition || document.documentElement.scrollTop > topPosition) {
                S.gotop_switch = true;
            } else {
                S.gotop_switch = false;
            }
        });

        /* 鍵盤輸入欄位檢測 */
        document.addEventListener("keyup", cart.Forms.AutoSwapInput);

        /* Step3 Form 檢測 */
        S.ShippingForms = $('#RadioShipping');
        S.PaymentForms = $('#RadioPayment');
        S.OrdererForms = $('#OrdererForm > form');
        S.RecipientForms = $('#Form_Recipient');
        S.InvoiceForms = $('#Form_Invoice');
        S.InvoicePersonalTypeForms = $('#Form_InvoicePersonalType');

        function getSelectedFreightGroupId() {
            const ids = cart.Items.getSelectedCartIds();     // 你現有的函式：回傳勾選的 scId 陣列
            const selected = S.shopping_cart_data.filter(e => ids.includes(e.Id));
            // 你的 shopping_cart_data 在 cart.Items.CartListAdd() 會寫入 e.freight（物件或 null）
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
                    const $optionRow = $input.closest('.shipping-option-row');
                    const isTarget = (id === fid);
                    $optionRow.toggleClass('d-none', !isTarget);
                    $input.prop('checked', isTarget);
                });

                const $checked = $('[name="RadioShipping"]:checked');
                if ($checked.length) cart.Shipping.RadioShipping.call($checked[0]);

            } else {
                // 👉 一般運費情境：隱藏所有特殊運費（freightStatusType = 2）
                $inputs.each(function () {
                    const $input = $(this);
                    const statusType = Number($input.data('freight-status-type')) || 0;
                    const $optionRow = $input.closest('.shipping-option-row');
                    const isSpecial = (statusType === 2); // 特殊運費

                    $optionRow.toggleClass('d-none', isSpecial);

                    // 若原本選到特殊項目 → 取消選取
                    if (isSpecial && $input.is(':checked')) {
                        $input.prop('checked', false);
                    }
                });

                // 若沒有任何選取 → 預設勾選第一個可見的一般運費
                const $checked = $('[name="RadioShipping"]:checked');
                if ($checked.length === 0) {
                    const $firstVisible = $inputs.filter(function () {
                        return !$(this).closest('.shipping-option-row').hasClass('d-none');
                    }).first();
                    if ($firstVisible.length) {
                        $firstVisible.prop('checked', true);
                        cart.Shipping.RadioShipping.call($firstVisible[0]);
                    }
                }
            }

            S.buy_step_swiper.update();
        }

        $(".btn_checkout").on("click", function () {
            this.blur();
            cart.Payment.Core.Step3Monitor();
            S.shipMethodsChosen = S.shipMethodsChosen && cart.Shipping.HasSelectedCvsStore();
            if (!S.OrdererFilled) {
                if (!S.OrdererOpen) cart.Forms.OrdererEdit(true);
                Coker.sweet.warning("請注意", "請確實填寫訂購人資料！", null);
            } else if (!S.RecipientFilled) {
                Coker.sweet.warning("請注意", "請確實填寫收件人資料！", null);
            } else if (!S.InvoiceFilled) {
                Coker.sweet.warning("請注意", "請確實填寫發票寄送資料！", null);
            } else if (!S.shipMethodsChosen) {
                if (cart.Shipping.IsCvsShippingSelected() && !cart.Shipping.HasSelectedCvsStore()) {
                    Coker.sweet.warning("請注意", "請先選擇超商取貨門市！", null);
                } else {
                    Coker.sweet.warning("請注意", "請選擇運送方式！", null);
                }
            } else if (!S.payMethodsChosen && !cart.Payment.Core.hasProvidersByType("embedded")) {
                Coker.sweet.warning("請注意", "請選擇付款方式！", null);
            } else {
                var isEmbeddedPayment = cart.Payment.Core.isActiveEmbeddedPayment();
                if (isEmbeddedPayment && !cart.Payment.Core.isActiveEmbeddedLoaded()) {
                    cart.Payment.Core.setProvidersMonitorByType("embedded", true);
                    cart.Payment.Core.reloadActiveEmbeddedProvider();
                    co.sweet.warning("付款模組尚未載入完成，請稍候再試。", "", null);
                } else if (isEmbeddedPayment && !cart.Payment.Core.isActiveEmbeddedReady()) {
                    cart.Payment.Core.setProvidersMonitorByType("embedded", true);
                    cart.Payment.Core.reloadActiveEmbeddedProvider();

                    co.sweet.warning(
                        "付款資訊已更新",
                        "正在重新同步付款資訊，請稍候再點選確認付款。",
                        null
                    );
                } else {
                    cart.Payment.Core.setProvidersMonitorByType("embedded", false);
                    Coker.sweet.custom("info", "是否確定結帳？", "點選確認進入付款流程", "是，開始付款", function () {
                        cart.Order.OrderHeaderAdd();
                    }, "否", function () {
                        cart.Payment.Core.setProvidersMonitorByType("embedded", true);
                    });
                }
            }
            S.buy_step_swiper.update();
        });

        /* Button */
        $(".btn_back_to_check").on("click", function () {
            S.buy_step_swiper.slideTo(0);
        });

        $(".btn_goprev").on("click", function () {
            S.buy_step_swiper.slidePrev();
        });

        $(".btn_edit_data").on("click", function () {
            cart.Forms.OrdererEdit(null)
        });
        $(".btn_delete_recipient").on("click", cart.Forms.DeleteRecipient);

        /* Radio Button */
        $('input[type=radio][name=RadioShipping]').on("change", cart.Shipping.RadioShipping);
        $('input[type=radio][name=RadioPayment]').on("change", cart.Payment.Core.RadioPayment);
        $('input[type=radio][name=RecipientRadio]').on("change", cart.Forms.RecipientRadio);
        $('input[type=radio][name=InvoiceRadio]').on("change", cart.Forms.InvoiceRadio);
        $('input[type=radio][name=InvoiceType]').on("change", cart.Forms.InvoiceTypeRadio);
        $('input[type=radio][name=PersonalInvoiceMode]').on("change", cart.Forms.PersonalInvoiceMode);

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
            if (S.user_data == null) {
                $('#Form_Orderer .gender input[type="radio"]').prop('checked', false);
                co.Zipcode.setData({
                    el: $("#Orderer_TWzipcode"),
                    addr: "縣市"
                });
            } else {
                $('#Form_Orderer .gender input[type="radio"]').prop('checked', false);
                var address = S.user_data.ordererAddress;
                if (address && address.indexOf(" ") > 0) {
                    if (address.split(' ').length >= 3) S.user_data.ordererAddress = address.split(' ')[2];
                    else S.user_data.ordererAddress = "";
                }
                co.Form.insertData(S.user_data, "#Form_Orderer");
                S.user_data.ordererAddress = address;
                co.Zipcode.setData({
                    el: $("#Orderer_TWzipcode"),
                    addr: S.user_data.address
                });
            }
            cart.Payment.Core.onAmountChanged();
            cart.Payment.Core.reloadActiveEmbeddedProvider();
        })

        if (cart.Logistics && cart.Logistics.ECPay) {
            cart.Logistics.ECPay.restoreOrderForm();
            cart.Logistics.ECPay.bindMapButton();
        }

        if (cart.Marketing && typeof cart.Marketing.loadCartMarketingCampaigns === "function") {
            cart.Marketing.loadCartMarketingCampaigns().always(function () {
                cart.Pricing.TotalCount();

                if (S.buy_step_swiper) {
                    S.buy_step_swiper.update();
                }
            });
        }

    }

    Object.assign(cart.Init, {
        PageReady: PageReady
    });
})(window.ShoppingCart, window.jQuery);


// Public compatibility aliases used by existing Razor / DevExtreme callbacks.
window.PageReady = function () { return window.ShoppingCart.Init.PageReady.apply(window.ShoppingCart.Init, arguments); };
window.CardDataGet = function () { return window.ShoppingCart.Items.CardDataGet.apply(window.ShoppingCart.Items, arguments); };
window.RecipientsList_ContentReady = function () { return window.ShoppingCart.Forms.RecipientsList_ContentReady.apply(window.ShoppingCart.Forms, arguments); };
window.RecipientsList_SelectChange = function () { return window.ShoppingCart.Forms.RecipientsList_SelectChange.apply(window.ShoppingCart.Forms, arguments); };
window.RecipientsList_DeleteButtonClicked = function () { return window.ShoppingCart.Forms.RecipientsList_DeleteButtonClicked.apply(window.ShoppingCart.Forms, arguments); };
