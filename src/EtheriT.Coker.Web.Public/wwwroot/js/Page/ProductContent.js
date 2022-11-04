function PageReady() {
    window.CI360.init();

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
        $('.input_pro_quantity').val(parseInt($('.input_pro_quantity').val()) + 1);
    });
    $(document).on('click', '.btn_count_minus', function () {
        $('.input_pro_quantity').val(parseInt($('.input_pro_quantity').val()) - 1);
        if ($('.input_pro_quantity').val() == 0) {
            $('.input_pro_quantity').val(1);
        }
    });

    var $radio_btn = $('#Product > .content > .options > .radio > .control')
    if ($radio_btn.children().length <= 2) {
        $radio_btn.children('label').toggleClass('pe-none');
    }

    $(".btn_addToCar").on("click", AddToCar);
}

function AddToCar() {
    $.cookie('Purchased_Item_Quantity', parseInt($.cookie('Purchased_Item_Quantity')) + parseInt($('.input_pro_quantity').val()), { path: '/' });
    Coker.sweet.success("成功加入購物車！", null, true);
    if ($.cookie('Purchased_Type_Quantity') > 0) {
        CarItemChange();
    } else {
        $.cookie('Purchased_Type_Quantity', 1, { path: '/' });
        CarDropdownReset();
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