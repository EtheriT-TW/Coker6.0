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

    $('.black-bg .close').click(function () {
        $(".black-bg").fadeOut(500);
    });
    $('#swiper1, #swiper2, #swiper3, #swiper4, #swiper5, #swiper6, #swiper7, #swiper8').click(function () {
        $(".black-bg").fadeIn(500);
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


