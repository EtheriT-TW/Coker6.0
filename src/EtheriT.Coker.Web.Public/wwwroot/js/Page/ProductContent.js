var $input_quantity
var Pid, s1, s2
var s1_list = [], s2_list = [], spectype_list, spec_list, price_list = []

function PageReady() {

    ElementInit();
    window.CI360.init();

    Pid = $(location).attr('href').substr($(location).attr('href').lastIndexOf("/") + 1);
    PageDefaultSet(Pid);

    var preview_swiper = new Swiper(".PreviewSwiper", {
        slidesPerView: 4,
        loop: false,
        spaceBetween: 10,
        freeMode: true,
        watchSlidesProgress: true,
        scrollbar: {
            el: ".swiper-scrollbar",
        },
        breakpoints: {
            576: {
                slidesPerView: 4,
            },
            768: {
                slidesPerView: 6,
            },
            992: {
                slidesPerView: 8,
            }
        }
    });

    var product_swiper = new Swiper(".ProductSwiper", {
        spaceBetween: 15,
        loop: true,
        navigation: {
            nextEl: ".btn_swiper_next_product",
            prevEl: ".btn_swiper_prev_product",
        },
        breakpoints: {
            768: {
                allowTouchMove: true,
            },
            992: {
                allowTouchMove: false,
            }
        },
        thumbs: {
            swiper: preview_swiper,
        },
    });

    $(".pro_display").on("click", ShowBigPro);
    const proDisplayModal = document.getElementById('ProDisplayModal')
    proDisplayModal.addEventListener('hidden.bs.modal', event => {
        window.CI360.destroy();
        $("#Pro_Youtube").attr("src", "");
    })

    $('#shareBlock').cShare({
        description: 'jQuery plugin - C Share buttons',
        showButtons: ['fb', 'line', 'plurk', 'twitter', 'email']
    });

    $(document).on('click', '.btn_count_plus', function () {
        $input_quantity.val(parseInt($input_quantity.val()) + 1);
    });
    $(document).on('click', '.btn_count_minus', function () {
        $input_quantity.val(parseInt($input_quantity.val()) - 1);
        if ($input_quantity.val() == 0) {
            $input_quantity.val(1);
        }
    });

    var $radio_btn = $('#Product > .content > .options > .radio > .control')
    if ($radio_btn.children().length <= 2) {
        $radio_btn.children('label').toggleClass('pe-none');
    }

    $(".btn_addToCar").on("click", AddToCart);
    $(".btn_certification").on("click", function () {
        $("#ProductDescription").removeClass("active show")
        $("#TechnicalDocuments").addClass("active show")
        $("#pills-description-tab").removeClass("active")
        $("#pills-documents-tab").addClass("active")
        var btn_data = $(this).data("certification")
        $(".badge_directions").each(function () {
            var $self = $(this)
            if ($self.data("certification") == btn_data) {
                $('html, body').animate({ scrollTop: $self.offset().top - ($("header").height() * 2) }, 0);
            }
        })
    })
}

function ElementInit() {
    $input_quantity = $('.input_pro_quantity');

    $prod_content = $("#Product > .content");
    $pro_name = $prod_content.find('.pro_title');
    $pro_introduce = $prod_content.find('.introduce');
    $pro_specification = $prod_content.find('.specification').children("ul");
    $pro_price = $prod_content.find(".ori_price");
    $pro_discount = $prod_content.find(".discount");
    $btn_detailed = $prod_content.find(".btn_detailed");

    $options = $prod_content.find(".options");
}

function PageDefaultSet() {
    Product.GetOne.Prod(Pid).done(function (result) {
        $pro_name.text(result.title);
        $pro_introduce.append("<li>" + result.introduction.replaceAll("\n", "</li><li>") + "</li>")
        $pro_specification.append("<li>" + result.description.replaceAll("\n", "</li><li>") + "</li>")
        var spec_height = 0;
        $pro_specification.children("li").each(function () {
            spec_height += $(this).height();
        })
        if (spec_height > $pro_specification.height()) {
            $btn_detailed.removeClass("d-none")
        }
    });

    Product.GetOne.TechCert(Pid).done(function (result) {
        var techcert_list = []
        result.forEach(function (item) {
            techcert_list.push(item.id);
        })

        $(".btn_certification").each(function () {
            var $self = $(this)
            if (techcert_list.indexOf($self.data("certification")) < 0) {
                $self.parents("li").first().remove();
            }
        })

        $(".badge_directions").each(function () {
            var $self = $(this)
            if (techcert_list.indexOf($self.data("certification")) < 0) {
                $self.siblings("hr").first().remove();
                $self.remove();
            }
        })
    })

    Product.GetOne.Stock(Pid).done(function (result) {
        if (result.length > 1) {

            var obj = {};

            var item1 = $($("#Template_Spec_Radio").html()).clone(), item2 = $($("#Template_Spec_Radio").html()).clone();
            var item1_control = item1.find(".spec_control"),
                item2_control = item2.find(".spec_control");

            item1.data("stype", 1)
            item2.data("stype", 2)
            result.forEach(function (spec) {
                obj["s1id"] = spec.fK_S1id;
                obj["s2id"] = spec.fK_S2id;
                obj["price"] = spec.price;
                price_list.push(obj);
                obj = {}

                if (spec.fK_S1id > 0) {
                    if (s1_list.indexOf(spec.fK_S1id) < 0) {
                        item1_control.prepend('<label class="btn_radio me-2 my-1 px-3 py-1 align-self-center" for="s1_' + spec.fK_S1id + '">' + spec.s1_Name + '</label>');
                        item1_control.prepend('<input id="s1_' + spec.fK_S1id + '" type="radio" class="btn-check" name="S1_Radio" autocomplete="off" value="' + spec.fK_S1id + '">');
                        s1_list.push(spec.fK_S1id);
                    }
                } else {
                    if (!s1 >= 0) {
                        s1 = 0;
                    }
                }

                if (spec.fK_S2id > 0) {
                    if (s2_list.indexOf(spec.fK_S2id) < 0) {
                        item2_control.prepend('<label class="btn_radio me-2 my-1 px-3 py-1 align-self-center" for="s2_' + spec.fK_S2id + '">' + spec.s2_Name + '</label>');
                        item2_control.prepend('<input id="s2_' + spec.fK_S2id + '" type="radio" class="btn-check" name="S2_Radio" autocomplete="off" value="' + spec.fK_S2id + '">');
                        s2_list.push(spec.fK_S2id);
                    }
                } else {
                    if (!s2 >= 0) {
                        s2 = 0;
                    }
                }
            })

            $options.prepend(item2);
            $options.prepend(item1);

            $radio = $("#Product > .content > .options > .radio");
            $radio.each(function () {
                $input = $(this).children(".spec_control").children("input")
                $input.each(function () {
                    $(this).on("click", SpecRadio)
                })
            })

            $pro_discount.text(result[0].price.toLocaleString('en-US') + " ~ " + result[result.length - 1].price.toLocaleString('en-US'));
        } else {
            s1 = result[0].fK_S1id;
            s2 = result[0].fK_S2id;
            $pro_discount.text(result[0].price.toLocaleString('en-US'));
        }
    })

    var $product_swiper = $(".ProductSwiper > .swiper-wrapper"), $preview_swiper = $(".PreviewSwiper > .swiper-wrapper")
    if (Pid == 1) {
        var demo_slide = $($("#TemplateDemoSlide").html()).clone();
        var demo_pre_slide = $($("#TemplateDemoPreviewSlide").html()).clone();
        $product_swiper.append(demo_slide);
        $preview_swiper.append(demo_pre_slide);
    } else {
        var slide = $($("#TemplateImageSlide").html()).clone();
        var slide_image = slide.find(".pro_display");
        slide.data("pid", Pid);
        slide_image.attr("src", "/images/product/pro_0" + Pid + ".png")
        $product_swiper.append(slide);

        var pre_slide = $($("#TemplatePreviewSlide").html()).clone();
        var pre_slide_image = pre_slide.find("img");
        pre_slide.data("pid", Pid);
        pre_slide_image.attr("src", "/images/product/pro_0" + Pid + ".png")
        $preview_swiper.append(pre_slide);
    }
}

function SpecRadio() {
    $self = $(this);
    $self_p = $self.parents(".radio").first();
    $self_s = $self_p.siblings(".radio");

    $self_p.find("input").each(function () {
        $radio = $(this)
        $radio.removeAttr("disabled");
    })
    $self_s.find("input").each(function () {
        $radio = $(this)
        $radio.removeAttr("disabled");
    })

    switch ($self_p.data("stype")) {
        case 1:
            s1 = $self.val()
            var temp_list = []
            //console.log(price_list)
            //console.log(s1)
            price_list.forEach(function (item) {
                if (item.s1id == s1) {
                    temp_list.push(item.s2id)
                }
            })
            $self_s.find("input").attr("disabled", "disabled");
            $self_s.find("input").each(function () {
                $radio = $(this)
                if (temp_list.indexOf(parseInt($radio.val())) > -1) {
                    $radio.removeAttr("disabled");
                }
            })
            if ($self_s.find("input[value='" + parseInt(s2) + "']").attr("disabled") == "disabled") {
                s2 = null;
            }
            break;
        case 2:
            s2 = $self.val()
            var temp_list = []
            price_list.forEach(function (item) {
                if (item.s2id == s2) {
                    temp_list.push(item.s1id)
                }
            })
            $self_s.find("input").attr("disabled", "disabled");
            $self_s.find("input").each(function () {
                $radio = $(this)
                if (temp_list.indexOf(parseInt($radio.val())) > -1) {
                    $radio.removeAttr("disabled");
                }
            })
            if ($self_s.find("input[value='" + parseInt(s2) + "']").attr("disabled") == "disabled") {
                s1 = null;
            }
            break;
    }

    if (s1 != null && s2 != null) {
        price_list.forEach(function (item) {
            if (item.s1id == s1 && item.s2id == s2) {
                $pro_discount.text(item.price.toLocaleString('en-US'));
            }
        })
    }
}

function AddToCart() {
    if ($.cookie('cookie') == null || $.cookie('cookie') == 'reject') {
        Coker.sweet.error("錯誤", "若要進行商品選購，請先同意隱私權政策", null, false);
    } else {
        if (s1 != null && s2 != null) {
            Product.AddUp.Cart({
                FK_Tid: $.cookie("Token"),
                FK_Pid: parseInt(Pid),
                FK_S1id: s1,
                FK_S2id: s2,
                Quantity: $input_quantity.val(),
                Discont: 0,
                Bonus: 0,
                PriceType: 0,
                IsAdditional: false,
                Ser_No: 500,
            }).done(function (result) {
                if (result.success) {
                    Coker.sweet.success("商品已成功加入購物車", null, true);
                    var type = (result.message).substr(0, 1);
                    var id = (result.message).substr(1);
                    Product.GetOne.Cart(id).done(function (result) {
                        if (type == 'N') {
                            CartDropAdd(result);
                        } else {
                            CartDropUpdate(result);
                        }
                    });
                } else {
                    Coker.sweet.error("錯誤", "商品加入購物車發生錯誤", null, true);
                }
            }).fail(function () {
                Coker.sweet.error("錯誤", "商品加入購物車發生錯誤", null, true);
            });
        } else {
            Coker.sweet.error("錯誤", "請確實選擇規格", null, false);
        }

    }
}

function ShowBigPro() {
    var pro_self = $(this);
    var pro_viewModalSpace = $("#ProDisplayModal > .modal-dialog > .modal-content > .modal-body");
    pro_viewModalSpace.children(".pro_img").addClass("d-none");
    pro_viewModalSpace.children(".pro_youtube").addClass("d-none");
    pro_viewModalSpace.children(".pro_360view").addClass("d-none");
    switch (pro_self.data("display-protype")) {
        case "image":
            pro_viewModalSpace.children(".pro_img").removeClass("d-none");
            addImage(pro_self);
            break;
        case "youtube":
            pro_viewModalSpace.children(".pro_youtube").removeClass("d-none");
            addYoutube(pro_self);
            break;
        case "360view":
            pro_viewModalSpace.children(".pro_360view").removeClass("d-none");
            add360View(pro_self);
            break;
    }
}

function addImage(pro_self) {
    var pro_filename = pro_self.attr("src");
    while (pro_filename.indexOf('/') >= 0) {
        pro_filename = pro_filename.substr(pro_filename.indexOf('/') + 1);
    }

    var proImage_Self = $("#Pro_Image");
    proImage_Self.attr("data-filename-x", pro_filename);

    $("#ProDisplayModal").on("shown.bs.modal", function () {
        const proImage = document.getElementById("Pro_Image");
        proImage.classList.add("cloudimage-360");
        window.CI360.add("Pro_Image");
    });
}

function addYoutube(pro_self) {
    var pro_YoutubeLink = pro_self.data("youtube-link");
    $("#Pro_Youtube").attr("src", "https://www.youtube.com/embed/" + pro_YoutubeLink);
}

function add360View(pro_self) {
    var pro360View_Self = $("#Pro_360View");
    pro360View_Self.attr("data-filename-x", pro_self.data("filename-x"));
    pro360View_Self.attr("data-amount-x", pro_self.data("amount-x"));

    $("#ProDisplayModal").on("shown.bs.modal", function () {
        const pro360View = document.getElementById("Pro_360View");
        pro360View.classList.add("cloudimage-360");
        window.CI360.add("Pro_360View");
    });
}