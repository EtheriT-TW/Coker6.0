var $input_quantity
var Pid, s1, s2, bonus
var s1_list = [], s2_list = [], spectype_list, spec_list, price_list = [], img_origin_list
var preview_swiper, product_swiper, $pro_itemNo, $counter_input;
var CanShop;
const showRange = false;

function PageReady() {

    ElementInit();

    if ($('#SwitchPage').length > 0 && $('#SwitchPage').css('display') !== 'none') SwitchPage();

    if ($(".btn_addToCar").length > 0) CanShop = true;
    else CanShop = false;

    /* 讀取使用者剩餘紅利點數 */
    bonus = 0;

    window.CI360.init();
    if (ProdId != null && !isNaN(ProdId)) Pid = ProdId;
    else Pid = location.pathname.substring(location.pathname.lastIndexOf("/") + 1);
    if (isNaN(Pid) && /\/[\d]+\//.test(location.pathname)) {
        const para = location.pathname.match(/\/[\d]+\//g);
        if (para.length > 0) {
            Pid = para[para.length - 1].replace(/\//g, "");
        }
    }
    Product.Log.Click(Pid).done(function () {
        //ProdHistorySet();
    });

    Product.GetOne.ProdMainDisplay(Pid).done(function (result) {
        if (result != null) {
            PageDefaultSet(result);
        } else {
            window.location.href = window.location.pathname.substring(0, window.location.pathname.lastIndexOf("/"));
        }
    });

    $(".pro_display").on("click", ShowBigPro);
    const proDisplayModal = document.getElementById('ProDisplayModal')
    proDisplayModal.addEventListener('hidden.bs.modal', event => {
        window.CI360.destroy();
        $("#Pro_Youtube").attr("src", "");
        $("#Pro_Video").attr("src", "");
    })

    $(".btn_addToCar").on("click", function () {
        if (!$(".btn_addToCar").hasClass("close") && !$(".btn_addToCar").hasClass("bonus_lack")) AddToCart();
    });

    $('#shareBlock').cShare({
        description: 'jQuery plugin - C Share buttons',
        showButtons: ['fb', 'line', 'plurk', 'twitter', 'email']
    });

    $(document).on('click', '.btn_count_plus', function () {
        $input_quantity.val(parseInt($input_quantity.val()) + parseInt($input_quantity.attr("step")));
        $input_quantity.trigger("change");
    });
    $(document).on('click', '.btn_count_minus', function () {
        $input_quantity.val(parseInt($input_quantity.val()) - parseInt($input_quantity.attr("step")));
        if ($input_quantity.val() == 0) {
            $input_quantity.val(parseInt($input_quantity.attr("step")));
        }
        $input_quantity.trigger("change");
    });
    $input_quantity.on("change", function () {
        let v = $(this).val() - $(this).val() % $(this).attr("step");
        if (v > parseInt($(this).attr("max"))) v = parseInt($(this).attr("max"));
        if (v < parseInt($(this).attr("min"))) v = parseInt($(this).attr("min"));
        $(this).val(v);
    });

    var $radio_btn = $('#Product > .content > .options > .radio > .control')
    if ($radio_btn.children().length <= 2) {
        $radio_btn.children('label').toggleClass('pe-none');
    }

    $(".btn_tc").on("click", function () {
        $("#ProductDescription").removeClass("active show")
        $("#TechnicalDocuments").addClass("active show")
        $("#pills-description-tab").removeClass("active")
        $("#pills-documents-tab").addClass("active")
        var btn_data = $(this).data("tcid")
        $(".badge_directions").each(function () {
            var $self = $(this)
            if ($self.data("pro_tc") == btn_data) {
                $('html, body').animate({ scrollTop: $self.offset().top - ($("header").height() * 2) }, 0);
            }
        })
    })

    if ($(".btn_favorites").length > 0) {
        Coker.Favorites.Check(Pid).done(function (check) {
            if (check.success) {
                $(".btn_favorites").data("Fid", check.message);
                $(".btn_favorites").addClass("turn")
                $(".btn_favorites").attr("title", "移除收藏")
            }
        });
        $(".btn_favorites").on("click", function () {
            var $self = $(this);
            if ($self.hasClass("turn")) {
                Coker.Favorites.Delete($(this).data("Fid")).done(function (result) {
                    if (result.success) {
                        $self.removeClass("turn")
                        $self.attr("title", "加入收藏")
                        Coker.sweet.success("已將商品從收藏中移除", null, true);
                    }
                });
            } else {
                Coker.Favorites.Add(Pid).done(function (favorites) {
                    if (favorites.success) {
                        $self.addClass("turn")
                        $self.data("Fid", favorites.message);
                        $self.attr("title", "移除收藏")
                        Coker.sweet.success("成功將商品加入收藏", null, true);
                    }
                });
            }
        })
    }
}
function ElementInit() {
    $input_quantity = $('.input_pro_quantity');
    $counter_input = $(".counter_input");
    $prod_content = $("#Product > .content");
    $pro_name = $prod_content.find('.pro_title');
    $pro_itemNo = $prod_content.find('.pro_itemNo');
    $pro_introduce = $prod_content.find('.introduce');
    $pro_specification = $("#SpecCollapse > ul").first();
    $pro_price = $prod_content.find(".ori_price");
    $pro_discount = $prod_content.find(".discount");
    $btn_detailed = $prod_content.find(".btn_detailed");

    $options = $prod_content.find(".options");
}
function PageDefaultSet(result) {
    if (result.status == 2) {
        CanShop = false;
        $counter_input.addClass("isEmpty");
        $(".btn_addToCar").addClass("close")
        result.stocks.forEach(stock => {
            stock.stock = 0;
        });
    } else if (result.stocks.length == 1 && result.stocks[0].stock <= 0) {
        $(".btn_addToCar").addClass("close")
        $("#Product .content .options").addClass("d-none")
    }

    $pro_name.text(result.title);
    $pro_itemNo.text(result.itemNo);
    $pro_introduce.append("<li>" + result.introduction.replaceAll("\n", "</li><li>") + "</li>")
    $pro_specification.append("<li>" + result.description.replaceAll("\n", "</li><li>") + "</li>")
    var spec_height = 0;
    $pro_specification.children("li").each(function (index) {
        spec_height += $(this).height();
    })
    if (spec_height > 96) {
        $btn_detailed.removeClass("d-none")
    }
    if (result.html != null && result.html.trim() != "") $("#ProductDescription > Content").removeClass("d-none").html($.htmlDecode(result.html));
    else $("#ProductDescription,#btn_tab .description").remove();

    result.techCertDatas.forEach(item => {
        if (item.img_small.length > 0) {
            item.img_small.forEach(function (img) {
                $(".pro_tc > ul").append(`<li class="me-1"><button class="btn_tc bg-transparent border-0" data-tcid="${item.id}"><img src="${img.link}" alt="${img.name}" /></button></li>`);
                $(".pro_tc > ul img").imgCheck();
            })

            item.img_orig.forEach(function (img) {
                $(".pro_tc_content > .techcert_list").append(`<div class="badge_${item.id} row pb-3">
			                                                      <div class="col-12 col-lg-2 col-md-5 text-center verticalAlign">
				                                                      <img class="" src="${img.link}" alt="${img.name}" />
			                                                      </div>
			                                                      <div class="description align-self-center col">${item.description}</div>
		                                                      </div>
		                                                      <hr class="m-1" />`)
                $(".pro_tc_content > .techcert_list img").imgCheck();
            })
        } else {
            $(".pro_tc").addClass("d-none");
        }
    });
    if (result.status != 0) {
        $("#Product>.image").append(`<span class="status status${result.status}">${result.statusName}</span>`);
    }

    if (!$(".pro_tc").hasClass("d-none")) {
        $(".btn_tc").on("click", function () {
            $("#TabContent .tab-pane").removeClass("active show")
            $("#TechnicalDocuments").addClass("active show")
            $("#btn_tab .nav-link").removeClass("active")
            $("#pills-documents-tab").addClass("active")
            var $self_btn = $(this);
            $('html, body').animate({ scrollTop: $(`.badge_${$self_btn.data("tcid")}`).offset().top - $("header > nav").height() * 2 }, 0);
        })
    }
    if (result.techCertDatas.length == 0) $("#btn_tab > .technical,.pro_tc").remove();

    if (result.stocks.length > 1) {
        var obj = {};

        var item1 = $($("#Template_Spec_Radio").html()).clone(), item2 = $($("#Template_Spec_Radio").html()).clone();
        var item1_control = item1.find(".spec_control"),
            item2_control = item2.find(".spec_control");

        item1.data("stype", 1)
        item2.data("stype", 2)

        var maxprice = 0, minprice = 0;

        var hasstock = false;
        result.stocks.forEach(data => {
            if (data.prices.length > 0 && data.prices[0].price != null) {
                var prices = [];
                data.prices.forEach(function (self_data) {
                    var temp_obj = {
                        fK_RId: self_data.fK_RId,
                        priceid: self_data.id,
                        price: self_data.price,
                        oriprice: self_data.oriPrice,
                        bonus: self_data.bonus
                    }
                    prices.push(temp_obj);
                })
                obj = {
                    priceid: data.prices[0].id,
                    s1id: data.fK_S1id,
                    s2id: data.fK_S2id,
                    stock: data.stock,
                    minQty: data.min_Qty,
                    price: data.prices[0].price,
                    prices: prices,
                    suggestprice: data.price,
                };
                price_list.push(obj);
                maxprice = obj["price"] > maxprice ? obj["price"] : maxprice;
                minprice = obj["price"] < minprice || minprice == 0 ? obj["price"] : minprice;
                obj = {}
                var nostock = "";
                if (data.stock <= 0 && CanShop) {
                    nostock = 'disabled="disabled"'
                } else {
                    hasstock = true;
                }

                if (data.fK_S1id > 0) {
                    if (s1_list.indexOf(data.fK_S1id) < 0) {
                        item1_control.append(`<input id="s1_${data.fK_S1id}" type="radio" class="btn-check" name="S1_Radio" autocomplete="off" value="${data.fK_S1id}" ${nostock}>`);
                        item1_control.append('<label class="btn_radio me-2 my-1 px-3 py-1 align-self-center" for="s1_' + data.fK_S1id + '">' + data.s1_Title + '</label>');
                        s1_list.push(data.fK_S1id);
                    } else {
                        if (data.stock > 0) item1_control.find(`#s1_${data.fK_S1id}`).prop("disabled", false);
                    }
                } else {
                    if (!s1 >= 0) {
                        s1 = 0;
                    }
                }

                if (data.fK_S2id > 0) {
                    if (s2_list.indexOf(data.fK_S2id) < 0) {
                        item2_control.append(`<input id="s2_${data.fK_S2id}" type="radio" class="btn-check" name="S2_Radio" autocomplete="off" value="${data.fK_S2id}" ${nostock}>`);
                        item2_control.append('<label class="btn_radio me-2 my-1 px-3 py-1 align-self-center" for="s2_' + data.fK_S2id + '">' + data.s2_Title + '</label>');
                        s2_list.push(data.fK_S2id);
                    }
                } else {
                    if (!s2 >= 0) {
                        s2 = 0;
                    }
                }
            }
        });
        if (!hasstock) {
            $("#Product .content .options").addClass("d-none")
            $(".btn_addToCar").addClass("close")
        }
        else {
            $counter_input.removeClass("isEmpty");
        }

        $options.prepend(item2);
        $options.prepend(item1);

        $radio = $("#Product > .content > .options > .radio");
        $radio.each(function () {
            $input = $(this).children(".spec_control").children("input")
            $input.each(function () {
                $(this).on("change", SpecRadio)
            })
        })
        $(".priceframe").empty();
        var price_temp = $($("#PriceListTemplate").html()).clone();
        if (showRange) {
            if (maxprice == minprice) price_temp.find(".discount").text(minprice.toLocaleString('en-US'));
            else price_temp.find(".discount").text(minprice.toLocaleString('en-US') + " ~ " + maxprice.toLocaleString('en-US'));
        } else price_temp.find(".discount").text(maxprice.toLocaleString('en-US'));
        price_temp.find("input").addClass("d-none");
        $(".priceframe").append(price_temp);
    } else if (result.stocks[0].prices.length > 0 && result.stocks[0].prices[0].price != null) {
        s1 = result.stocks[0].fK_S1id;
        s2 = result.stocks[0].fK_S2id;

        $(".priceframe").empty();
        result.stocks[0].prices.forEach(function (item) {
            var price = item.price;
            var oriprice = item.oriPrice;
            var price_temp = $($("#PriceListTemplate").html()).clone();

            price_temp.find("input").data("priceid", item.id)
            price_temp.find("input").attr("id", `price_${item.id}`);
            price_temp.find("label").attr("for", `price_${item.id}`);

            var price_text = "";
            if (item.bonus > 0) {
                price_text = `${price.toLocaleString('en-US')}+紅利${item.bonus}點`;
                if (CanShop) {
                    if (bonus > item.bonus) {
                        price_temp.find(".discount").removeClass("bonus_lack");
                        price_temp.find("input").prop("disabled", false);
                    }
                    else {
                        price_temp.find(".discount").addClass("bonus_lack");
                        price_temp.find("input").prop("disabled", true);
                    }
                }
            } else price_text = price.toLocaleString('en-US');
            price_temp.find(".discount").text(price_text);
            if (item.fK_RId != 1) price_temp.find(".discount").addClass("mprice");

            if (oriprice > price) {
                price_temp.find(".ori_price").text(oriprice.toLocaleString('en-US'));
                price_temp.find(".ori_price").removeClass("d-none")
            }

            if (!IsLogin && result.stocks[0].suggestPrice > 0 && result.stocks[0].suggestPrice != price) {
                price_temp.find(".discount").empty();
                price_temp.find(".discount").removeClass("price");
                price_temp.find(".discount").append(`<div class="text-body-tertiary text-decoration-line-through fs-5 pe-2">建議售價$${result.stocks[0].suggestPrice.toLocaleString('en-US')}</div><div class="text-danger">折扣後 $${price_text}</div>`);
            }
            else price_temp.find(".discount").text(price_text);

            if ($(".priceframe").children().length == 0 && !price_temp.find("input").prop("disabled")) price_temp.find("input").prop("checked", true);
            $(".priceframe").append(price_temp);
        });

        if (!CanShop || $(".priceframe input").length == 1 || $(".btn_addToCar").hasClass("close")) $(".priceframe input").addClass("d-none");
        else $(".priceframe input").removeClass("d-none");
        if ($(".priceframe input:checked").length == 0) $(".btn_addToCar").addClass("bonus_lack")
        else $(".btn_addToCar").removeClass("bonus_lack")

    } else {
        $(".btn_addToCar").addClass("d-none");
        $(".priceframe").addClass("d-none");
        $(".options").addClass("d-none");
    }
    if (result.stocks.length > 0) {
        $input_quantity.attr({
            min: 0,
            max: result.stocks[0].stock - (result.stocks[0].stock % result.stocks[0].min_Qty),
            step: result.stocks[0].min_Qty
        });
        if (result.stocks[0].stock > 0) {
            $counter_input.removeClass("isEmpty");
            $input_quantity.trigger("change");
        }
    }

    var $product_swiper = $(".ProductSwiper > .swiper-wrapper"), $preview_swiper = $(".PreviewSwiper > .swiper-wrapper");

    if (result.id == 1 && false) {
        var demo_slide = $($("#TemplateDemoSlide").html()).clone();
        var demo_pre_slide = $($("#TemplateDemoPreviewSlide").html()).clone();
        $product_swiper.append(demo_slide);
        $preview_swiper.append(demo_pre_slide);
    } else {
        result.img_Medium.forEach(img_med => {
            var slide = $($("#TemplateImageSlide").html()).clone();
            var slide_image = slide.find(".pro_display");
            slide_image.attr({
                "alt": img_med.name,
                "data-id": img_med.id
            });
            switch (img_med.fileType) {
                case 3:
                    slide = $($("#TemplateVideoSlide").html()).clone();
                    var slide_video = slide.find(".pro_display");
                    slide_video.attr({
                        "alt": img_med.name,
                        "data-id": img_med.id,
                        "src": img_med.link[0],
                    })
                    break;
                case 4:
                    slide.addClass("bg-black")
                    slide_image.attr({
                        "data-display-protype": "youtube",
                        "data-youtube-link": img_med.name,
                        src: `https://img.youtube.com/vi/${img_med.name}/0.jpg`
                    })
                    slide_image.siblings(".schematic_image").replaceWith('<div class="schematic_youtube position-absolute"><i class="fa-brands fa-youtube"></i></div>');
                    break;
                default:
                    slide_image.attr("src", img_med.link[0]);
                    break;
            }
            slide_image.imgCheck();
            $product_swiper.append(slide);
        });

        result.img_Small.forEach(img_small => {
            var pre_slide = $($("#TemplatePreviewSlide").html()).clone();
            var pre_slide_image = pre_slide.find("img");
            pre_slide_image.attr("alt", img_small.name);
            switch (img_small.fileType) {
                case 3:
                    pre_slide_image.attr("src", "/images/videopreview.jpg");
                    break;
                case 4:
                    pre_slide_image.attr({ src: `https://img.youtube.com/vi/${img_small.name}/3.jpg` })
                    break;
                default:
                    pre_slide_image.attr("src", img_small.link[0]);
                    break;
            }
            pre_slide_image.data("id", img_small.id);
            pre_slide_image.imgCheck();
            $preview_swiper.append(pre_slide);
        });
        if (result.img_Small.length == 1) $(".PreviewSwiper,.btn_swiper_prev_product,.btn_swiper_next_product").addClass("d-none");
    }
    if (result.img_Small.length > 1) {
        preview_swiper = new Swiper(".PreviewSwiper", {
            a11y: true,
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

        product_swiper = new Swiper(".ProductSwiper", {
            a11y: true,
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
    }
    $product_swiper.find(".pro_display").on("click", ShowBigPro);
    img_origin_list = result.img_Original;

    if (result.tagDatas.length > 0) {
        result.tagDatas.forEach(item => {
            $(".pro_tag").prepend(`<li><a class="round_tag rounded-pill me-1 px-3 py-1" href="/${OrgName}/Search/Get/3/${item.tag_Name}">${item.tag_Name}</a></li>`);
        })
    } else {
        $(".pro_tag").addClass("d-none");
    }

    if (result.files != null && result.files.length > 0) {
        result.files.forEach(function (file) {
            var link = IsFaPage == true ? file.link : file.link.replace("upload", `upload/${OrgName}`);
            $("#FileDownload>.File_list").append(`
            <div class="file border-bottom">
                <a href="${link}" download="${file.name}" title="${file.name}" class="link_with_icon d-flex text-decoration-none edit_lock">
                    <div draggable="true" class="icon pe-2"></div>
                    <div draggable="true" class="name">${file.name}
                </div></a>
            </div>`)
        });
    } else $("#btn_tab > .files,#FileDownload").remove();
    $("#btn_tab>li>button").first().trigger("click");
    LinkWithIconInit();

    if (!CanShop) $(".counter").addClass("d-none");
}
function SpecRadioSet(stocks, $parent) {
    var item1 = $($("#Template_Spec_Radio").html()).clone(),
        item2 = $($("#Template_Spec_Radio").html()).clone();

    var item1_control = item1.find(".spec_control"),
        item2_control = item2.find(".spec_control");

}
function SpecRadio() {
    $self = $(this);
    $self_p = $self.parents(".radio").first();
    $self_s = $self_p.siblings(".radio");
    $input_quantity.val(1);

    switch ($self_p.data("stype")) {
        case 1:
            s1 = $self.val()
            var temp_list = []
            price_list.forEach(function (item) {
                if (item.s1id == s1) {
                    temp_list.push(item.s2id)
                }
            })
            $self_s.find("input").attr("disabled", "disabled");
            var radioclosenum = $self_s.find("input").length;
            $self_s.find("input").each(function () {
                $radio = $(this)
                if (temp_list.indexOf(parseInt($radio.val())) > -1 && (price_list.find(e => e.s1id == s1 && e.s2id == $radio.val()).stock > 0 || !CanShop)) {
                    $radio.removeAttr("disabled");
                    radioclosenum -= 1;
                }
            })
            if (radioclosenum == $self_s.find("input").length) {
                $(".btn_addToCar").addClass("close")
                $counter_input.addClass("isEmpty");
            }
            if ($self_s.find("input[value='" + parseInt(s2) + "']").attr("disabled") == "disabled") {
                s2 = null;
            }
            var checknow = $self_s.find(`input:checked`);
            if (!(checknow.length > 0 && typeof (checknow.attr("disabled")) == "undefined")) {
                $self_s.find(`input`).prop("checked", false)
                if ($self_s.find(`input:not([disabled])`).length > 0 && $self_s.find(`input:not([disabled]):checked`).length == 0) {
                    $self_s.find(`input:not([disabled])`).first().prop("checked", true).trigger("change");
                }
            }
            break;
        case 2:
            s2 = $self.val()
            var temp_list = [];
            price_list.forEach(function (item) {
                if (item.s2id == s2) {
                    temp_list.push(item.s1id)
                }
            })
            if ($self_s.find("input[value='" + parseInt(s2) + "']").attr("disabled") == "disabled") {
                s1 = null;
            }
            var radioclosenum = 0;
            $self_s.find("input").each(function () {
                $radio = $(this)
                var this_price_list = price_list.find(e => e.s1id == $radio.val() && e.s2id == s2);
                if (typeof (this_price_list) != "undefined" && this_price_list.stock <= 0 && CanShop) {
                    $radio.attr("disabled", "disabled");
                    radioclosenum += 1;
                } else $radio.removeAttr("disabled");
            })
            if (radioclosenum == $self_s.find("input").length) {
                $(".btn_addToCar").addClass("close")
                $counter_input.addClass("isEmpty");
            }
            break;
    }

    if (s1 != null) {
        price_list.forEach(function (item) {
            if (item.s1id == s1 && (item.s2id == 0 || item.s2id == s2)) {
                $(".priceframe").empty();
                item.prices.forEach(function (self_item) {

                    var price = self_item.price;
                    var oriprice = self_item.oriprice;
                    var price_temp = $($("#PriceListTemplate").html()).clone();

                    price_temp.find("input").data("priceid", self_item.priceid)
                    price_temp.find("input").attr("id", `price_${self_item.priceid}`);
                    price_temp.find("label").attr("for", `price_${self_item.priceid}`);

                    var price_text = "";
                    if (self_item.bonus > 0) {
                        price_text = `${price.toLocaleString('en-US')}+紅利${self_item.bonus}點`;
                        if (CanShop) {
                            if (bonus > self_item.bonus) {
                                price_temp.find(".discount").removeClass("bonus_lack");
                                price_temp.find("input").prop("disabled", false);
                            }
                            else {
                                price_temp.find(".discount").addClass("bonus_lack");
                                price_temp.find("input").prop("disabled", true);
                            }
                        }
                    } else price_text = price.toLocaleString('en-US');

                    if (!IsLogin && item.suggestprice > 0 && item.suggestprice != price) {
                        price_temp.find(".discount").empty();
                        price_temp.find(".discount").removeClass("price");
                        price_temp.find(".discount").append(`<div class="text-body-tertiary text-decoration-line-through fs-5 pe-2">建議售價$${item.suggestprice.toLocaleString('en-US')}</div><div class="text-danger">折扣後 $${price_text}</div>`);
                    }
                    else price_temp.find(".discount").text(price_text);

                    if (self_item.fK_RId != 1) price_temp.find(".discount").addClass("mprice");

                    if (oriprice > price) {
                        price_temp.find(".ori_price").text(oriprice.toLocaleString('en-US'));
                        price_temp.find(".ori_price").removeClass("d-none")
                    }

                    if ($(".priceframe").children().length == 0 && !price_temp.find("input").prop("disabled")) price_temp.find("input").prop("checked", true);
                    $(".priceframe").append(price_temp);
                });

                $input_quantity.attr({
                    min: 0,
                    max: item.stock - (item.stock % item.minQty),
                    step: item.minQty
                });
                if (item.stock <= 0) $counter_input.addClass("isEmpty");
                else {
                    $counter_input.removeClass("isEmpty");
                }
                $input_quantity.trigger("change");
            }
        })
    }

    if (s2 == null) s2 = 0
    var this_price_list = price_list.find(e => e.s1id == s1 && e.s2id == s2);
    if (this_price_list.stock <= 0) {
        $(".btn_addToCar").addClass("close")
    }
    else {
        $(".btn_addToCar").removeClass("close")
        $input_quantity.attr("max", this_price_list.stock)
    }

    if (!CanShop || $(".priceframe input").length == 1 || $(".btn_addToCar").hasClass("close")) $(".priceframe input").addClass("d-none");
    else $(".priceframe input").removeClass("d-none");
    if ($(".priceframe input:checked").length == 0) $(".btn_addToCar").addClass("bonus_lack")
    else $(".btn_addToCar").removeClass("bonus_lack")
}
function AddToCart() {
    if (localStorage.getItem('AgreePrivacy') == null) {
        Coker.sweet.warning("請注意", "若要進行商品選購，請先同意隱私權政策", null);
    } else {
        if (s1 != null && s2 != null && $input_quantity.val() != 0) {
            Product.AddUp.Cart({
                FK_Pid: parseInt(Pid),
                FK_PriceId: $(".priceframe input[type='radio']:checked").data("priceid"),
                FK_S1id: s1,
                FK_S2id: s2,
                Quantity: $input_quantity.val(),
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
                    if (price_list.length > 0) {
                        var this_price_list = price_list.find(e => e.s1id == s1 && e.s2id == s2);
                        this_price_list.stock -= $input_quantity.val();
                        $input_quantity.attr("max", this_price_list.stock)
                        if (this_price_list.stock <= 0) {
                            if (!$(".btn_addToCar").hasClass("close")) $(".btn_addToCar").addClass("close")
                            if (!$counter_input.hasClass("isEmpty")) $counter_input.addClass("isEmpty");
                        }
                    } else {
                        var stock = $input_quantity.attr("max") - $input_quantity.val();
                        if (stock <= 0) {
                            if (!$(".btn_addToCar").hasClass("close")) $(".btn_addToCar").addClass("close")
                            if (!$counter_input.hasClass("isEmpty")) $counter_input.addClass("isEmpty");
                        } else {
                            $input_quantity.attr("max", stock)
                        }
                    }
                    $input_quantity.val(1);
                } else {
                    if (result.error == "商品庫存不足") {
                        Coker.sweet.warning(result.error, result.message, function () {
                            location.reload(true);
                        }, false);
                    } else {
                        Coker.sweet.error("商品加入購物車發生錯誤", result.message, null, true);
                    }
                }
            }).fail(function () {
                Coker.sweet.error("錯誤", "商品加入購物車發生錯誤", null, true);
            });
        } else {
            Coker.sweet.warning("請注意", "請確實選擇規格及購買數量", null, false);
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
            $(".modal-dialog").removeClass("ytshow")
            pro_viewModalSpace.children(".pro_img").removeClass("d-none");
            pro_viewModalSpace.children(".pro_video").addClass("d-none");
            addImage(pro_self);
            break;
        case "video":
            $(".modal-dialog").removeClass("ytshow")
            pro_viewModalSpace.children(".pro_img").addClass("d-none");
            pro_viewModalSpace.children(".pro_video").removeClass("d-none");
            addVideo(pro_self);
            break;
        case "youtube":
            $(".modal-dialog").addClass("ytshow")
            pro_viewModalSpace.children(".pro_youtube").removeClass("d-none");
            pro_viewModalSpace.children(".pro_img").addClass("d-none");
            pro_viewModalSpace.children(".pro_video").addClass("d-none");
            addYoutube(pro_self);
            break;
        case "360view":
            if ($(".modal-dialog").hasClass("ytshow")) $(".modal-dialog").removeClass("ytshow")
            pro_viewModalSpace.children(".pro_360view").removeClass("d-none");
            pro_viewModalSpace.children(".pro_video").removeClass("d-none");
            add360View(pro_self);
            break;
    }
}
function addImage(pro_self) {
    var img_data = img_origin_list.find(item => item.id == pro_self.data("id"));

    var pro_filename = img_data.link[0];
    pro_folder = pro_filename.substr(0, pro_filename.lastIndexOf('/') + 1)
    pro_filename = pro_filename.substr(pro_filename.lastIndexOf('/') + 1);

    var proImage_Self = $("#Pro_Image");
    proImage_Self.attr({ "data-filename-x": pro_filename, "data-folder": pro_folder });

    $("#ProDisplayModal").on("shown.bs.modal", function () {
        const proImage = document.getElementById("Pro_Image");
        proImage.classList.add("cloudimage-360");
        window.CI360.add("Pro_Image");
    });
}
function addVideo(pro_self) {
    var img_data = img_origin_list.find(item => item.id == pro_self.data("id"));
    $("#Pro_Video").attr("src", img_data.link[0])
}
function addYoutube(pro_self) {
    var pro_YoutubeLink = pro_self.data("youtube-link");
    $("#Pro_Youtube").attr("src", `https://www.youtube-nocookie.com/embed/${pro_YoutubeLink}?&autoplay=1`);
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
function SwitchPage() {
    var currentUrl = window.location.pathname + window.location.search;
    var catalog = currentUrl.substring(0, currentUrl.indexOf('/product'));
    var productid = (currentUrl.split('/product/')[1]).split('?')[0].split('/')[0];
    var searchtext = decodeURIComponent((currentUrl.split('/product/')[1]).split('?')[0].split('/')[1]);
    var urlParams = new URLSearchParams(window.location.search);
    var dirid = urlParams.get('dirid');
    var diridList = dirid == null ? null : dirid.split(',').map(Number);
    var filter = urlParams.get('filter');

    var routername = catalog.substring(catalog.lastIndexOf("/") + 1)

    if (routername == "search" && sessionStorage.getItem(`product-${searchtext}`)) {
        $("#SwitchPage .btn_list").attr("href", `${catalog}/Get/3/${searchtext}`)
        var plist = JSON.parse(sessionStorage.getItem(`product-${searchtext}`));
        var index = plist.findIndex(p => p.key == productid);
        if (plist.length > 0) {
            if (index > 0) {
                var link = `${catalog}/product/${plist[index - 1].key}/${searchtext}?filter=${filter}`;
                $("#SwitchPage .btn_prev").attr({
                    href: link,
                    title: plist[index - 1].value
                })
                $("#SwitchPage .btn_prev").removeClass("disabled")
            }
            if (index < plist.length - 1) {
                var link = `${catalog}/product/${plist[index + 1].key}/${searchtext}?filter=${filter}`;
                $("#SwitchPage .btn_next").attr({
                    href: link,
                    title: plist[index + 1].value
                })
                $("#SwitchPage .btn_next").removeClass("disabled")
            }
        } else {
            $('#SwitchPage').remove();
        }
    } else {
        $("#SwitchPage .btn_list").attr("href", catalog)
        Directory.SwitchPage({ id: productid, dirids: diridList, routername: routername, searchtext: searchtext, filters: filter, type: 1 }).done(function (result) {
            if (result.length > 0) {
                if (routername == "search") {
                    sessionStorage.setItem(`product-${searchtext}`, JSON.stringify(result));
                    var index = result.findIndex(p => p.key == productid);
                    if (index > 0) {
                        $("#SwitchPage .btn_prev").removeClass("disabled")
                        var link = `${catalog}/product/${result[index - 1].key}/${searchtext}?filter=${filter}`;
                        $("#SwitchPage .btn_prev").attr({
                            href: link,
                            title: result[index - 1].value
                        })
                        $("#SwitchPage .btn_prev").removeClass("disabled")
                    }
                    if (index < result.length - 1) {
                        var link = `${catalog}/product/${result[index + 1].key}/${searchtext}?filter=${filter}`;
                        $("#SwitchPage .btn_next").attr({
                            href: link,
                            title: result[index + 1].value
                        })
                        $("#SwitchPage .btn_next").removeClass("disabled")
                    }
                } else {
                    if (result[0].key != null) {
                        $("#SwitchPage .btn_prev").removeClass("disabled")
                        var link = `${catalog}/product/${result[0].key}`;
                        $("#SwitchPage .btn_prev").attr({
                            href: link,
                            title: result[0].value
                        })
                    }
                    if (result[1].key != null) {
                        $("#SwitchPage .btn_next").removeClass("disabled")
                        var link = `${catalog}/product/${result[1].key}`;
                        $("#SwitchPage .btn_next").attr({
                            href: link,
                            title: result[1].value
                        })
                    }
                }
            } else {
                $('#SwitchPage').remove();
            }
        });
    }
} 