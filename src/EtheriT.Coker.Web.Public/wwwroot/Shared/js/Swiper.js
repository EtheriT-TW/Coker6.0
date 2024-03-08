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
                swiper.autoplay.stop()
            }
            const start = function () {
                swiper.autoplay.start()
            }
            $(this).find(".swiper").prepend($(this).find(".swiper_button_prev"));
            $(this).on("mouseover", stop);
            $(this).find("a").on("focus", stop);
            $(this).on("mouseout", start);
            $(this).find("a").on("blob", start);
            $(this).find("button").prop("disabled",false);
        }
    });

    $(".one_swiper").prop("draggable", true).each(function () {
        var $self = $(this);

        if (!!!$self.data("isInit")) {
            var Id = "#" + $self.attr("id") + " > .swiper";
            const canNext = $(Id).find(".swiper-slide").length > 2;
            var effect = $self.data("effect");
            var speed = $self.data("effect-speed")
            if (typeof effect === 'undefined' || effect === false) effect = "slide";
            if (typeof speed === 'undefined' || speed === false) speed = 300;
            else speed = parseInt(speed);
            var autoplay = obj.autoplay ? canNext : false;
            var selfConfig = Object.assign({}, config, {
                pagination: {
                    el: "#" + $self.attr("id") + " .swiper_pagination",
                    clickable: true,
                },
                effect: effect,
                speed: speed
            }, autoplay ? {
                autoplay: {
                    delay: 5000,
                    disableOnInteraction: false,
                },
                loop: true
            } : {},
                canNext ? {
                    navigation: {
                        nextEl: "#" + $self.attr("id") + " .swiper_button_next > button",
                        prevEl: "#" + $self.attr("id") + " .swiper_button_prev > button",
                    }
                } : {});
            if (!canNext) {
                $(`#${$self.attr("id")}`).find(".swiper_button_next,.swiper_button_prev").remove();
            }
            var swiper = new Swiper(Id, selfConfig);
            $self.data("isInit", true)
            if (autoplay && swiper.slides.length - 2 > 1) {
                $self.swiperBindEven(swiper);
            }
        }
    });

    $(".two_swiper").prop("draggable", true).each(function () {
        var $self = $(this);

        if (!!!$self.data("isInit")) {
            var Id = "#" + $self.attr("id") + " > .swiper"
            var selfConfig = Object.assign({}, config, {
                pagination: {
                    el: "#" + $self.attr("id") + " .swiper_pagination",
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
                    nextEl: "#" + $self.attr("id") + " .swiper_button_next > button",
                    prevEl: "#" + $self.attr("id") + " .swiper_button_prev > button"
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
            obj.autoplay &&$self.swiperBindEven(swiper);
            $self.prepend($("#" + $self.attr("id") + " .swiper_button_prev"));
            console.log(selfConfig);
        }
    });

    $(".four_swiper").prop("draggable", true).each(function () {
        var $self = $(this);
        if (!!!$self.data("isInit")) {
            var Id = "#" + $self.attr("id") + " > .swiper";
            var selfConfig = Object.assign({}, config, {
                pagination: {
                    el: "#" + $self.attr("id") + " .swiper-pagination",
                    clickable: true,
                },
                navigation: {
                    nextEl: "#" + $self.attr("id") + " .swiper_button_next > button",
                    prevEl: "#" + $self.attr("id") + " .swiper_button_prev > button",
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
            obj.autoplay&&$self.swiperBindEven(swiper);
            $self.prepend($("#" + $self.attr("id") + " .swiper_button_prev"));
        }
    });


    $(".six_swiper").prop("draggable", true).each(function () {
        var $self = $(this);
        if (!!!$self.data("isInit")) {
            var Id = "#" + $self.attr("id") + " > .swiper";
            var selfConfig = Object.assign({}, config, {
                pagination: {
                    el: "#" + $self.attr("id") + " .swiper_pagination",
                    clickable: true,
                },
                scrollbar: {
                    el: '.swiper-scrollbar',
                    draggable: true,
                },
                navigation: {
                    nextEl: "#" + $self.attr("id") + " .swiper_button_next > button",
                    prevEl: "#" + $self.attr("id") + " .swiper_button_prev > button",
                }, breakpoints: {
                    320: {
                        slidesPerView: 2,
                    },
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
            obj.autoplay &&$self.swiperBindEven(swiper);
            $self.prepend($("#" + $self.attr("id") + " .swiper_button_prev"));
        }
    });

    if ($(".picture-category").length > 0 && $("#SwiperModal").length > 0) {
        const pictureSwiperThumbs = new Swiper("#pictureSwiperThumbs", {
            spaceBetween: 10,
            breakpoints: {
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
            },
            freeMode: true,
            watchSlidesProgress: true,
        });
        const pictureSwiper = new Swiper("#pictureSwiper", {
            centeredSlides: true,
            spaceBetween: 10,
            navigation: {
                nextEl: "#pictureSwiper .swiper-button-next",
                prevEl: "#pictureSwiper .swiper-button-prev",
            },
            pagination: {
                el: "#pictureSwiper .swiper-pagination",
            },
            thumbs: {
                swiper: pictureSwiperThumbs,
            }
        });
        $(".picture-category a").attr("href", "#SwiperModal").on("click", function () {
            const $self = $(this).parents(".picture-category");
            const index = $(".picture-category a").index(this);
            const $images = [];
            $self.find(".templatecontent img").each(function () {
                $images.push($(this).attr("src"));
            });
            pictureSwiper.removeAllSlides();
            pictureSwiperThumbs.removeAllSlides();
            for (let i = 0; i < $images.length; i++) {
                const newSlide = `<div class="swiper-slide"><img src="${$images[i]}" alt=" " /></div>`;
                pictureSwiper.appendSlide(newSlide);
                pictureSwiperThumbs.appendSlide(newSlide);
            }
            pictureSwiper.slideTo(index, 0);
            pictureSwiperThumbs.slideTo(index, 0);
            $('#SwiperModal').modal('show'); 
            return false;
        });
    }
}