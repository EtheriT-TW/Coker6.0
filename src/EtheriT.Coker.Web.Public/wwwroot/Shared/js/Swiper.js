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
    $.fn.extend({
        swiperBindEven: function (swiper) {
            const stop = function () {
                console.log("stop");
                swiper.autoplay.stop()
            }
            const start = function () {
                console.log("start");
                swiper.autoplay.start()
            }
            $(this).find(".swiper").prepend($(this).find(".swiper_button_prev"));
            $(this).on("mouseover", stop);
            $(this).find("a").on("focus", stop);
            $(this).on("mouseout", start);
            $(this).find("a").on("blob", start);
        }
    });

    $(".one_swiper").prop("draggable", true).each(function () {
        var $self = $(this);

        if (!!!$self.data("isInit")) {
            var Id = "#" + $self.attr("id") + " > .swiper"
            var selfConfig = Object.assign({}, config,{
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
            $self.swiperBindEven(swiper);
        }
    });

    $(".two_swiper").prop("draggable", true).each(function () {
        var $self = $(this);

        if (!!!$self.data("isInit")) {
            var Id = "#" + $self.attr("id") + " > .swiper"
            var selfConfig = Object.assign({},config, {
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
                    400: {
                        slidesPerView: 1,
                    },
                    1024: {
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
            $self.swiperBindEven(swiper);
        }
    });

    $(".four_swiper").prop("draggable", true).each(function () {
        var $self = $(this);
        if (!!!$self.data("isInit")) {
            var Id = "#" + $self.attr("id") + " > .swiper";
            var selfConfig = Object.assign({}, config, {
                pagination: {
                    el: Id + " > .swiper_pagination > *",
                    clickable: true,
                },
                navigation: {
                    nextEl: Id + " > .swiper_button_next > button",
                    prevEl: Id + " > .swiper_button_prev > button",
                }, breakpoints: {
                    375: {
                        slidesPerView: 1,
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
            $self.swiperBindEven(swiper);
        }
    });


    $(".six_swiper").prop("draggable", true).each(function () {
        var $self = $(this);

        if (!!!$self.data("isInit")) {
            var Id = "#" + $self.attr("id") + " > .swiper";
            var selfConfig = Object.assign({}, config, {
                navigation: {
                    nextEl: Id + " > .swiper_button_next > button",
                    prevEl: Id + " > .swiper_button_prev > button",
                },breakpoints: {
                    375: {
                        slidesPerView: 2,
                    },
                    576: {
                        slidesPerView: 3,
                    },
                    769: {
                        slidesPerView: 4,
                    },
                    1024: {
                        slidesPerView: 6,
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
            $self.swiperBindEven(swiper);
        }
    });
}