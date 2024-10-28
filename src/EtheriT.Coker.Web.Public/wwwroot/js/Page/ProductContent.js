var $input_quantity
var Pid, s1, s2
var s1_list = [], s2_list = [], spectype_list, spec_list, price_list = [], img_origin_list
var preview_swiper, product_swiper, $pro_itemNo, $counter_input;
const showRange = false;

function PageReady() {

    ElementInit();
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
    })

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

    $(".btn_addToCar").on("click", AddToCart);
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

}

function ElementInit() {
    $input_quantity = $('.input_pro_quantity');
    $counter_input = $(".counter_input");
    $prod_content = $("#Product > .content");
    $pro_name = $prod_content.find('.pro_title');
    $pro_itemNo = $prod_content.find('.pro_itemNo');
    $pro_introduce = $prod_content.find('.introduce');
    $pro_specification = $prod_content.find('.specification').children("ul");
    $pro_price = $prod_content.find(".ori_price");
    $pro_discount = $prod_content.find(".discount");
    $btn_detailed = $prod_content.find(".btn_detailed");

    $options = $prod_content.find(".options");
}

function PageDefaultSet(result) {
    $pro_name.text(result.title);
    $pro_itemNo.text(result.itemNo);
    $pro_introduce.append("<li>" + result.introduction.replaceAll("\n", "</li><li>") + "</li>")
    $pro_specification.append("<li>" + result.description.replaceAll("\n", "</li><li>") + "</li>")
    var spec_height = 0;
    $pro_specification.children("li").each(function () {
        spec_height += $(this).height();
    })
    if (spec_height > $pro_specification.height()) {
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

    var roleid = 1;
    if (result.stocks.length > 1) {
        var obj = {};

        var item1 = $($("#Template_Spec_Radio").html()).clone(), item2 = $($("#Template_Spec_Radio").html()).clone();
        var item1_control = item1.find(".spec_control"),
            item2_control = item2.find(".spec_control");

        item1.data("stype", 1)
        item2.data("stype", 2)

        var maxprice = 0, minprice = 0;

        result.stocks.forEach(data => {
            obj = {
                s1id: data.fK_S1id,
                s2id: data.fK_S2id,
                stock: data.stock,
                minQty: data.min_Qty,
                price: data.prices.find(e => e.fK_RId == roleid).price
            };
            price_list.push(obj);
            maxprice = obj["price"] > maxprice ? obj["price"] : maxprice;
            minprice = obj["price"] < minprice || minprice == 0 ? obj["price"] : minprice;
            obj = {}

            if (data.fK_S1id > 0) {
                if (s1_list.indexOf(data.fK_S1id) < 0) {
                    item1_control.append('<input id="s1_' + data.fK_S1id + '" type="radio" class="btn-check" name="S1_Radio" autocomplete="off" value="' + data.fK_S1id + '">');
                    item1_control.append('<label class="btn_radio me-2 my-1 px-3 py-1 align-self-center" for="s1_' + data.fK_S1id + '">' + data.s1_Title + '</label>');
                    s1_list.push(data.fK_S1id);
                }
            } else {
                if (!s1 >= 0) {
                    s1 = 0;
                }
            }

            if (data.fK_S2id > 0) {
                if (s2_list.indexOf(data.fK_S2id) < 0) {
                    item2_control.append('<input id="s2_' + data.fK_S2id + '" type="radio" class="btn-check" name="S2_Radio" autocomplete="off" value="' + data.fK_S2id + '">');
                    item2_control.append('<label class="btn_radio me-2 my-1 px-3 py-1 align-self-center" for="s2_' + data.fK_S2id + '">' + data.s2_Title + '</label>');
                    s2_list.push(data.fK_S2id);
                }
            } else {
                if (!s2 >= 0) {
                    s2 = 0;
                }
            }

        });

        $options.prepend(item2);
        $options.prepend(item1);

        $radio = $("#Product > .content > .options > .radio");
        $radio.each(function () {
            $input = $(this).children(".spec_control").children("input")
            $input.each(function () {
                $(this).on("change", SpecRadio)
            })
        })
        if (showRange) {
            if (maxprice == minprice) {
                $pro_discount.text(minprice.toLocaleString('en-US'));
            } else {
                $pro_discount.text(minprice.toLocaleString('en-US') + " ~ " + maxprice.toLocaleString('en-US'));
            }
        } else $pro_discount.text(maxprice.toLocaleString('en-US'));
    } else {
        s1 = result.stocks[0].fK_S1id;
        s2 = result.stocks[0].fK_S2id;

        var price = result.stocks[0].prices.find(e => e.fK_RId == roleid).price;
        $pro_discount.text(price.toLocaleString('en-US'));
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
                case 4:
                    slide_image.attr({
                        "data-display-protype": "youtube",
                        "data-youtube-link": img_med.name,
                        src: `https://img.youtube.com/vi/${img_med.name}/0.jpg`
                    })
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
}

function SpecRadio() {
    $self = $(this);
    $self_p = $self.parents(".radio").first();
    $self_s = $self_p.siblings(".radio");

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
            $self_s.find("input").each(function () {
                $radio = $(this)
                if (temp_list.indexOf(parseInt($radio.val())) > -1) {
                    $radio.removeAttr("disabled");
                }
            })
            if ($self_s.find("input[value='" + parseInt(s2) + "']").attr("disabled") == "disabled") {
                s2 = null;
            }
            $self_s.find(`input`).prop("checked", false)
            if ($self_s.find(`input:not([disabled])`).length > 0 && $self_s.find(`input:not([disabled]):checked`).length == 0) {
                $self_s.find(`input:not([disabled])`).first().prop("checked", true).trigger("change");
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
            break;
    }

    if (s1 != null) {
        price_list.forEach(function (item) {
            if (item.s1id == s1 && (item.s2id == 0 || item.s2id == s2)) {
                $pro_discount.text(item.price.toLocaleString('en-US'));
                $input_quantity.attr({
                    min: 0,
                    max: item.stock - (item.stock % item.minQty),
                    step: item.minQty
                });
                if (item.stock == 0) $counter_input.addClass("isEmpty");
                else $counter_input.removeClass("isEmpty");
                $input_quantity.trigger("change");
            }
        })
    }
}

function AddToCart() {
    if ($.cookie('cookie') == null || $.cookie('cookie') == 'reject') {
        Coker.sweet.error("錯誤", "若要進行商品選購，請先同意隱私權政策", null, false);
    } else {
        if (s1 != null && s2 != null && $input_quantity.val() != 0) {
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
            Coker.sweet.error("錯誤", "請確實選擇規格及購買數量", null, false);
        }
    }
}

function ShowBigPro() {
    var pro_self = $(this);
    var pro_viewModalSpace = $("#ProDisplayModal > .modal-dialog > .modal-content > .modal-body");
    pro_viewModalSpace.children(".pro_img").addClass("d-none");
    pro_viewModalSpace.children(".pro_youtube").addClass("d-none");
    pro_viewModalSpace.children(".pro_360view").addClass("d-none");
    console.log(pro_self.data("display-protype"));
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

function addYoutube(pro_self) {
    var pro_YoutubeLink = pro_self.data("youtube-link");
    $("#Pro_Youtube").attr("src", "https://www.youtube-nocookie.com/embed/" + pro_YoutubeLink);
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