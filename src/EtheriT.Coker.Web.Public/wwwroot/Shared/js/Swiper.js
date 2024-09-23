/*************** 
 obj:{
    autoplay:boolen 是否輪播
 }
 ***************/
function SwiperInit(obj) {
    var config = {
        slidesPerView: 1,
        spaceBetween: 15,
        keyboard: {
            enabled: true,
        },
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
            const $template = $(Id).find(".swiper-slide").parents(".templatecontent,.template_slide");
            if ($(Id).find(".swiper-slide").length == 1 && $template.length > 0) return false;
            const canNext = $template.length === 0 ? $(Id).find(".swiper-slide").length > 1: $(Id).find(".swiper-slide").length > 2;
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
                        nextEl: "#" + $self.attr("id") + " .swiper_button_next",
                        prevEl: "#" + $self.attr("id") + " .swiper_button_prev",
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
    //單欄輪播+兩欄縮圖
    $(".one_swiper_thumbs").prop("draggable", true).each(function () {
        var $self = $(this);
        const index = $self.index(this);
        $self.find(".six_thumbs > .swiper-wrapper").empty();
        if (!!!$self.data("isInit")) {
            var Id = "#" + $self.attr("id") + " .swiper";
            const canNext = $(Id).find(".swiper-slide").length >= 2;
            var effect = $self.data("effect");
            var speed = $self.data("effect-speed");      
            
            var swiperThumbs = new Swiper(".six_thumbs", {
                loop: false, //改為false阻止thumbs跳過太多張圖
                spaceBetween: 10,
                slidesPerView: 6,
                freeMode: true,
                watchSlidesProgress: true,
            }); 
            if (typeof effect === 'undefined' || effect === false) effect = "slide";
            if (typeof speed === 'undefined' || speed === false) speed = 300;
            else speed = parseInt(speed);
            var autoplay = obj.autoplay ? canNext : false;
            var selfConfig = Object.assign({}, config, {
                pagination: {
                    el: "#" + $self.attr("id") + " .swiper_pagination",
                    clickable: true,
                }, thumbs: {
                    swiper: swiperThumbs,
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
                        nextEl: "#" + $self.attr("id") + " .swiper_button_next",
                        prevEl: "#" + $self.attr("id") + " .swiper_button_prev",
                    },
                    
                } : {});
            if (!canNext) {
                $(`#${$self.attr("id")}`).find(".swiper_button_next,.swiper_button_prev").remove();
            }
            
            if (!$self.find(".swiper").hasClass(".selfThumbs")) { //如果沒有swiper class的元素
                const $images = [];
                $self.find(".swiper-slide img").each(function () { //遍歷所有thumnbs-image底下的img
                    $images.push($(this).attr("src")); //儲存到$images變數
                });
                for (let i = 0; i < $images.length; i++) { //生成Thumbs
                    const newSlide = `<div class="swiper-slide"><img src="${$images[i]}" alt=" " /></div>`;
                    swiperThumbs.appendSlide(newSlide); //放入siwperThumbs
                }
            }
            swiperThumbs.slideTo(index, 0);

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
                    nextEl: "#" + $self.attr("id") + " .swiper_button_next",
                    prevEl: "#" + $self.attr("id") + " .swiper_button_prev"
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
            obj.autoplay && $self.swiperBindEven(swiper);
            $self.prepend($("#" + $self.attr("id") + " .swiper_button_prev"));
            console.log(selfConfig);
        }
    });
    $(".three_swiper").prop("draggable", true).each(function () {
        var $self = $(this);
        if (!!!$self.data("isInit")) {
            var Id = "#" + $self.attr("id") + " > .swiper";
            var selfConfig = Object.assign({}, config, {
                pagination: {
                    el: "#" + $self.attr("id") + " .swiper-pagination",
                    clickable: true,
                },
                navigation: {
                    nextEl: "#" + $self.attr("id") + " .swiper_button_next",
                    prevEl: "#" + $self.attr("id") + " .swiper_button_prev",
                }, breakpoints: {
                    375: {
                        slidesPerView: 1,
                    },
                    576: {
                        slidesPerView: 2,
                    },
                    992: {
                        slidesPerView: 3,
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
            obj.autoplay && $self.swiperBindEven(swiper);
            $self.prepend($("#" + $self.attr("id") + " .swiper_button_prev"));
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
                    nextEl: "#" + $self.attr("id") + " .swiper_button_next",
                    prevEl: "#" + $self.attr("id") + " .swiper_button_prev",
                }, breakpoints: {
                    375: {
                        slidesPerView: 1,
                    },
                    576: {
                        slidesPerView: 3,
                    },
                    992: {
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
            obj.autoplay && $self.swiperBindEven(swiper);
            $self.prepend($("#" + $self.attr("id") + " .swiper_button_prev"));
        }
    });

    $(".five_swiper").prop("draggable", true).each(function () {
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
                    nextEl: "#" + $self.attr("id") + " .swiper_button_next",
                    prevEl: "#" + $self.attr("id") + " .swiper_button_prev",
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
                        slidesPerView: 5,
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
            obj.autoplay && $self.swiperBindEven(swiper);
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
                    nextEl: "#" + $self.attr("id") + " .swiper_button_next",
                    prevEl: "#" + $self.attr("id") + " .swiper_button_prev",
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
            obj.autoplay && $self.swiperBindEven(swiper);
            $self.prepend($("#" + $self.attr("id") + " .swiper_button_prev"));
        }
    });

    if ($(".picture-category").length > 0 && $("#SwiperModal").length > 0) {
        const imgCount = $(".picture-category .templatecontent img").length - 2;
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
                    slidesPerView: 6, //modal顯示縮圖的數量

                }
            },
            loop: false, //改為false阻止點選最後一張圖連跳太多張
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
            //Siwper多會複製兩張圖所以$images.length - 2
            for (let i = 0; i < $images.length - 2; i++) {
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