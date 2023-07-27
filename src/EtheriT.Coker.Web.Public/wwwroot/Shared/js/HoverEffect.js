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


