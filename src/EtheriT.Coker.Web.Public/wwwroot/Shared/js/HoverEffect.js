function HoverEffectInit() {
    $(".hover_mask").each(function () {
        var $self = $(this);

        $self.find(".hover_protrude_scale").each(function () {
            var $child = $(this);
            $child.hover(function () {
                $child.siblings().each(function () {
                    $(this).addClass("opacity-75");
                })
            }, function () {
                $child.siblings().each(function () {
                    $(this).removeClass("opacity-75");
                })
            });
        })
    });
}