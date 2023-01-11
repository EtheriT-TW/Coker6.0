/*************** 
 obj:{
    autoplay:boolen 是否輪播
 }
 ***************/
function SwiperInit(obj) {
    var config = {
        slidesPerView: 1,
        spaceBetween: 15,
    };

    $(".one_swiper").prop("draggable", true).each(function () {
        var $self = $(this);

        if (!!!$self.data("isInit")) {
            var Id = "#" + $self.attr("id") + " > .swiper"
            var selfConfig = $.extend(config, {
                pagination: {
                    el: Id + " > .swiper_pagination > *",
                    clickable: true,
                },
                navigation: {
                    nextEl: Id + " > .swiper_button_next > button",
                    prevEl: Id + " > .swiper_button_prev > button",
                }
            }, obj.autoplay ? {
                autoplay: {
                    delay: 5000,
                    disableOnInteraction: false,
                },
                loop: true
            } : {});
            var swiper = new Swiper(Id, selfConfig);
            $self.data("isInit", true)
        }
    });

    $(".two_swiper").prop("draggable", true).each(function () {
        var $self = $(this);

        if (!!!$self.data("isInit")) {
            var Id = "#" + $self.attr("id") + " > .swiper"
            var selfConfig = $.extend(config, {
                pagination: {
                    el: Id + " > .swiper_pagination > *",
                    clickable: true,
                    renderBullet: function (index, className) {
                        if ($self.has(".swiper_pagination_water").length) {
                            return '<span class="' + className + ' material-symbols-outlined ms-3">water_drop</span>';
                        } else {
                            return '<span class="' + className + '"></span>';
                        }
                    },
                },
                navigation: {
                    nextEl: Id + " > .swiper_button_next > button",
                    prevEl: Id + " > .swiper_button_prev > button",
                }, breakpoints: {
                    375: {
                        slidesPerView: 2,
                    }
                }
            }, obj.autoplay ? {
                autoplay: {
                    delay: 5000,
                    disableOnInteraction: false,
                },
                loop: true
            } : {});
            var swiper = new Swiper(Id, selfConfig);
            $self.data("isInit", true)
        }
    });

    $(".four_swiper").prop("draggable", true).each(function () {
        var $self = $(this);

        if (!!!$self.data("isInit")) {
            var Id = "#" + $self.attr("id") + " > .swiper"
            var selfConfig = $.extend(config, {
                pagination: {
                    el: Id + " > .swiper_pagination > *",
                    clickable: true,
                },
                navigation: {
                    nextEl: Id + " > .swiper_button_next > button",
                    prevEl: Id + " > .swiper_button_prev > button",
                }, breakpoints: {
                    375: {
                        slidesPerView: 2,
                    },
                    576: {
                        slidesPerView: 3,
                    },
                    769: {
                        slidesPerView: 4,
                    }
                }
            }, obj.autoplay ? {
                autoplay: {
                    delay: 5000,
                    disableOnInteraction: false,
                },
                loop: true
            } : {});
            var swiper = new Swiper(Id, selfConfig);
            $self.data("isInit", true)
        }
    });
}