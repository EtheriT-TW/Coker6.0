var $input_quantity
var Pid

function PageReady() {
    ElementInit();
    window.CI360.init();

    Pid = $(location).attr('href').substr($(location).attr('href').lastIndexOf("/") + 1);

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
        console.log("Click")
        console.log($(this))
        console.log($(this).data("certification"))
    })
}

function ElementInit() {
    $input_quantity = $('.input_pro_quantity');
}

function AddToCart() {
    Product.AddUp.Cart({
        FK_Tid: $.cookie("Token"),
        FK_Pid: Pid,
        FK_S1id: 1,
        FK_S2id: 4,
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