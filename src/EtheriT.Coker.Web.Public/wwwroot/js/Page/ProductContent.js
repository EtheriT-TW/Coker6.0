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

    $('#shareBlock').cShare({
        description: 'jQuery plugin - C Share buttons',
        showButtons: ['fb', 'line', 'plurk', 'twitter', 'email']
    });

    $(document).on('click', '.btn_count_plus', function () {
        $('.input_count').val(parseInt($('.input_count').val()) + 1);
    });
    $(document).on('click', '.btn_count_minus', function () {
        $('.input_count').val(parseInt($('.input_count').val()) - 1);
        if ($('.input_count').val() == 0) {
            $('.input_count').val(1);
        }
    });

    var $radio_btn = $('#Product > .content > .options > .radio > .control')
    if ($radio_btn.children().length <= 2) {
        $radio_btn.children('label').toggleClass('pe-none');
    }
}
