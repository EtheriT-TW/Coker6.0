function HoverEffectInit() {
    $(".hover_mask").each(function () {
        $(this).find(".hover_protrude_scale").each(function () {
            var $child = $(this);
            $child.hover(function () {
                $child.siblings().each(function () {
                    $(this).addClass("opacity-50");
                })
            }, function () {
                $child.siblings().each(function () {
                    $(this).removeClass("opacity-50");
                })
            });
        })
    });


}

function withebg() {
    $('.qual-width-images').mouseover(function () {
        var imgNum = $(this).attr('id').substr(3);
        $('.image-overlay').not('#overlay' + imgNum).css('display', 'block');
    });
    $('.qual-width-images').mouseout(function () {
        $('.image-overlay').css('display', 'none');
    });
};

$(document).ready(function () {
    $('.qual-width-images').click(function () {
        $(".black-bg").fadeIn(500);
    });

    $('.black-bg .close').click(function () {
        $(".black-bg").fadeOut(500);
    });
    var currentMainSlideIndex = 0;
    function updateMainSwiper(index) {
        currentMainSlideIndex = index;
        $('.one_swiper').hide();
        $('.one_swiper').eq(currentMainSlideIndex).show();
        var myMainSwiper = new Swiper('.one_swiper:visible .swiper-container', {
            loop: true,
            pagination: {
                el: '.swiper-pagination',
                clickable: true,
            },
        });
        $('.one_swiper:visible .swiper_button_next').click(function () {
            currentMainSlideIndex = (currentMainSlideIndex + 1) % 8;
        });
        $('.one_swiper:visible .swiper_button_prev').click(function () {
            currentMainSlideIndex = (currentMainSlideIndex - 1 + 8) % 8;
        });
    }

});
