/*************** 
 obj:{
    autoplay:boolen 是否輪播
 }
 ***************/
function SwiperInit() {
    var config = {
        slidesPerView: 1,
        spaceBetween: 15,
        loop: true,
    };

    $(".one_swiper").each(function (obj) {
        var $self = $(this);

        if (!!!$self.data("isInit")) {
            var Id = "#" + $self.attr("id") + " > .swiper"
            var selfConfig = $.extend(config, {
                pagination: {
                    el: Id + " > .swiper_pagination > *",
                    clickable: true,
                },
                navigation: {
                    nextEl: Id + " > .swiper_button_prev > button",
                    prevEl: Id + " > .swiper_button_next > button",
                }
            }, obj.autoplay ? {} : {
                autoplay: {
                    delay: 5000,
                    disableOnInteraction: false,
                }
            });
            var swiper = new Swiper(Id, selfConfig);
            $self.data("isInit", true)
        }
    });

    $(".two_swiper").each(function (obj) {
        var $self = $(this);

        if (!!!$self.data("isInit")) {
            var Id = "#" + $self.attr("id") + " > .swiper"
            var selfConfig = $.extend(config, {
                pagination: {
                    el: Id + " > .swiper_pagination > *",
                    clickable: true,
                },
                navigation: {
                    nextEl: Id + " > .swiper_button_prev > button",
                    prevEl: Id + " > .swiper_button_next > button",
                }, breakpoints: {
                    375: {
                        slidesPerView: 2,
                    }
                }
            }, obj.autoplay ? {} : {
                autoplay: {
                    delay: 5000,
                    disableOnInteraction: false,
                }
            });
            var swiper = new Swiper(Id, selfConfig);
            $self.data("isInit", true)
        }
    });

    $(".four_swiper").each(function (obj) {
        var $self = $(this);

        if (!!!$self.data("isInit")) {
            var Id = "#" + $self.attr("id") + " > .swiper"
            var selfConfig = $.extend(config, {
                pagination: {
                    el: Id + " > .swiper_pagination > *",
                    clickable: true,
                },
                navigation: {
                    nextEl: Id + " > .swiper_button_prev > button",
                    prevEl: Id + " > .swiper_button_next > button",
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
            }, obj.autoplay ? {} : {
                autoplay: {
                    delay: 5000,
                    disableOnInteraction: false,
                }
            });
            var swiper = new Swiper(Id, selfConfig);
            $self.data("isInit", true)
        }
    });
}