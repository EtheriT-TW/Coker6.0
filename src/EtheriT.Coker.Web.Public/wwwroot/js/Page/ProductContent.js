function PageReady() {

    var preview_swiper = new Swiper(".PreviewSwiper", {
        slidesPerView: 4,
        loop: false,
        spaceBetween: 10,
        freeMode: true,
        watchSlidesProgress: true,
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
        thumbs: {
            swiper: preview_swiper,
        },
    });

    $(".pro_display").on("click", Show3DPro);

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
    $.cookie('Purchased_Item_Quantity', parseInt($.cookie('Purchased_Item_Quantity')) + parseInt($('.input_pro_quantity').val()));
    console.log($('.input_pro_quantity').val());
    Coker.sweet.success("成功加入購物車！", null);
    if ($.cookie('Purchased_Type_Quantity') == 0) {
        $.cookie('Purchased_Type_Quantity', 1);
        CarDropdownReset();
    } else {
        CarItemAdd();
    }
}

function Show3DPro() {
    var pro_360view = $(this);
    var pro_viewModalSpace = $("#ProDisplayModal > .modal-dialog > .modal-content > .modal-body");
    pro_viewModalSpace.children(".pro_img").addClass("d-none");
    pro_viewModalSpace.children(".pro_youtube").addClass("d-none");
    pro_viewModalSpace.children(".pro_360view").addClass("d-none");
    switch (pro_360view.data("display-protype")) {
        case "image":
            pro_viewModalSpace.children(".pro_img").removeClass("d-none");
            pro_viewModalSpace.children(".pro_img").attr("src", ".." + $(this).attr("src"));
            break;
        case "youtube":
            pro_viewModalSpace.children(".pro_youtube").removeClass("d-none");
            pro_viewModalSpace.children(".pro_youtube").attr("src", "https://www.youtube.com/embed/JGEj2nhPvDs");
            break;
        case "360view":
            pro_viewModalSpace.children(".pro_360view").removeClass("d-none");
            add360View(pro_360view);
            break;
    }
}

function add360View(pro_self) {
    var pro360View_Self = $("#Pro_360View");
    pro360View_Self.attr("data-filename-x", pro_self.data("filename-x"));
    pro360View_Self.attr("data-amount-x", pro_self.data("amount-x"));

    window.CI360.init();
    $("#ProDisplayModal").on("shown.bs.modal", function () {
        console.log("click");
        const pro360View = document.getElementById("Pro_360View");
        pro360View.classList.add("cloudimage-360");
        window.CI360.add("Pro_360View");
    });
}