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