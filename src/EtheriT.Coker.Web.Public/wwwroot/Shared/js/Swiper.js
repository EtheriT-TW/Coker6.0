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
        swiperBindEven: function (swiper, canNext) {

            const checkSlides = function () {
                const totalSlides = swiper.slides.length;
                const slidesPerView = swiper.params.slidesPerView;
                // 檢查導航元素
                const nextEl = swiper.navigation.nextEl ? swiper.navigation.nextEl[0] : null;
                const prevEl = swiper.navigation.prevEl ? swiper.navigation.prevEl[0] : null;
                const paginationEl = swiper.pagination.el;

                if (totalSlides <= slidesPerView) {
                    // 停止自動輪播
                    swiper.autoplay.stop();
                    // 隱藏左右箭頭
                    if (nextEl) swiper.navigation.nextEl.style.display = 'none';
                    if (prevEl) swiper.navigation.prevEl.style.display = 'none';
                    if (paginationEl) paginationEl.style.display = 'none';
                } else {
                    // 確保箭頭可見
                    if (nextEl) swiper.navigation.nextEl.style.display = '';
                    if (prevEl) swiper.navigation.prevEl.style.display = '';
                    if (paginationEl) paginationEl.style.display = '';
                }
            }
            const stop = function () {
                var activeIndex = swiper.activeIndex;   // 当前活动滑块的索引
                var realIndex = swiper.realIndex;       // 如果使用了循环模式，获取真实的滑块索引
                var activeSlide = swiper.slides[activeIndex]; // 获取当前活动的滑块元素
                if ($(activeSlide).find("video").length > 0) {
                    return;
                }
                swiper.autoplay.stop()
            }
            const start = function () {
                var activeIndex = swiper.activeIndex;   // 当前活动滑块的索引
                var realIndex = swiper.realIndex;       // 如果使用了循环模式，获取真实的滑块索引
                var activeSlide = swiper.slides[activeIndex]; // 获取当前活动的滑块元素
                if ($(activeSlide).find("video").length > 0) {
                    return;
                }
                swiper.autoplay.start()
            }
            thisSwiper = $(this);
            $(this).off("mouseover").on("mouseover", stop);
            $(this).find("a").on("focus", stop);
            $(this).off("mouseout").on("mouseout", start);
            $(this).find("a").on("blob", start);
            $(this).find("button").prop("disabled", false);
            if (typeof (swiper.slide) != "undefined") {
                setTimeout(function () {
                    swiper.slideTo(0);
                }, 100);
                if (swiper.slides.length > 1) swiper.slideTo(1);
            }
            $(window).off('resize.swiper').on('resize.swiper', checkSlides);
            $(window).trigger("resize.swiper");
        }
    });

    $(".one_swiper").prop("draggable", true).each(function () {
        var $self = $(this);
        if (!!!$self.data("isInit")) {
            $self.find('video').each(function () {
                var $video = $(this);
                if ($video.attr('controls')) {
                    $video.removeAttr('controls');
                    $video.prop('muted', true);
                    $video.prop('playsinline', true);
                }
            });
            var Id = "#" + $self.attr("id") + " > .swiper";
            const $template = $(Id).find(".swiper-slide").parents(".templatecontent,.template_slide");
            if ($(Id).find(".swiper-slide").length == 1 && $template.length > 0) return false;
            const canNext = $template.length === 0 ? $(Id).find(".swiper-slide").length > 1 : $(Id).find(".swiper-slide").length > 2;
            var effect = $self.data("effect");
            var speed = $self.data("effect-speed");

            if (typeof effect === 'undefined' || effect === false) effect = "slide";
            if (typeof speed === 'undefined' || speed === false) speed = 300;
            else speed = parseInt(speed);
            var autoplay = obj.autoplay ? canNext : false;
            var selfConfig = Object.assign({}, config, {
                pagination: {
                    el: "#" + $self.attr("id") + " .swiper_pagination",
                    clickable: true,
                }, on: {
                    //以下有Bug
                    slideChangeTransitionEnd: function () {
                        const slide = this;
                        const totalSlides = this.slides.length;
                        const previousSlideIndex = this.previousIndex;
                        //const previousSlideIndex = (this.realIndex - 1 + totalSlides) % totalSlides;
                        const $previousSlide = $(this.slides[previousSlideIndex]);
                        const videoAction = $(this.slides[this.activeIndex]).find('video');
                        const videoElement = videoAction.get(0);
                        var videoManager = () => {
                            slide.autoplay.stop();
                            videoElement.onended = function () {
                                thisSwiper.on("mouseover");
                                thisSwiper.on("mouseout");
                                slide.autoplay.start();
                            };
                        };
                        if (videoAction.length > 0) {
                            thisSwiper.off("mouseover");
                            thisSwiper.off("mouseout");
                            videoManager();
                        }
                        $self.find(".swiper-slide").each(function () {
                            if (parseInt($(this).attr("data-swiper-slide-index")) != this.realIndex) {
                                var html;
                                var reset = function (element, tager) {
                                    $(element).empty();
                                    $(element).append(tager);
                                };
                                if ($(this).find("iframe").length > 0) {
                                    html = $(this).html();
                                } else if ($(this).find("video").length > 0) {
                                    var video = $(this).find('video').get(0);
                                    video.pause();
                                    video.currentTime = 0;
                                    video.play();
                                }
                                if (html !== undefined && html !== null) {
                                    reset($(this), html);
                                }
                            }
                        });
                    }
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
            var swiper = new Swiper(Id, selfConfig);
            $self.data("isInit", true)
            $self.swiperBindEven(swiper);
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
                const $alts = [];
                const $class = [];
                $self.find(".swiper-slide img").each(function () { //遍歷所有thumnbs-image底下的img
                    $images.push($(this).attr("src")); //儲存到$images變數
                    $alts.push($(this).attr("alt"));
                    if ($(this).closest('.swiper-slide').hasClass('backstageType')) {
                        $class.push('backstageType');
                    } else {
                        $class.push("");
                    }
                });
                if ($images.length > 1) {
                    for (let i = 0; i < $images.length; i++) { //生成Thumbs
                        const newSlide = `<div class="swiper-slide ${$class[i]}"><img src="${$images[i]}" alt="${$alts[i]}" /></div>`;
                        swiperThumbs.appendSlide(newSlide); //放入siwperThumbs
                    }
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
            const $template = $(Id).find(".swiper-slide").parents(".templatecontent,.template_slide");
            const length = $template.length === 0 ? $(Id).find(".swiper-slide").length : $(Id).find(".swiper-slide").length - 1;
            const canNext = length > 2;
            var autoplay = obj.autoplay ? canNext : false;
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
            }, autoplay ? {
                autoplay: {
                    delay: 5000,
                    disableOnInteraction: false,
                },
                loop: true
            } : {});
            if (!canNext && length > 0) {
                $(`#${$self.attr("id")}`).find(".swiper_button_next,.swiper_button_prev").addClass("d-none");
            } else {
                $(`#${$self.attr("id")}`).find(".swiper_button_next,.swiper_button_prev").removeClass("d-none");
            }
            var swiper = new Swiper(Id, selfConfig);
            $self.data("isInit", true)
            obj.autoplay && $self.swiperBindEven(swiper);
            $self.prepend($("#" + $self.attr("id") + " .swiper_button_prev"));
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
            if (typeof ($self.attr("id")) == "undefined") $self.attr("id", Math.random().toString(36).substring(2, 9) + Date.now())
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
        const $header_text = $("#SwiperModal .modal-header .imgalt");
        $header_text.text("");
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
            const index = $(".picture-category a").index(this);// > $(".picture-category a").length - 2 ? $(".picture-category a").length - $(".picture-category a").index(this) - 1 : $(".picture-category a").index(this) - 1;
            const $images = [];
            $self.find(".templatecontent img").each(function () {
                var obj = {};
                obj['src'] = $(this).attr("src");
                obj['alt'] = typeof ($(this).attr("alt")) == "undefined" ? "" : $(this).attr("alt");
                $images.push(obj);
            });
            pictureSwiper.removeAllSlides();
            pictureSwiperThumbs.removeAllSlides();
            $header_text.text($images[0]['alt']);
            if ($images.length == 1) {
                const newSlide = `<div class="swiper-slide"><img src="${$images[0]['src']}" alt="${$images[0]['alt']}" /></div>`;
                pictureSwiper.appendSlide(newSlide);
            } else {
                for (let i = 0; i < $images.length; i++) {
                    const newSlide = `<div class="swiper-slide"><img src="${$images[i]['src']}" alt="${$images[i]['alt']}" /></div>`;
                    pictureSwiper.appendSlide(newSlide);
                    const newSlideThumbs = `<div class="swiper-slide align-content-center mx-2"><img class="" src="${$images[i]['src']}" alt="${$images[i]['alt']}" /></div>`;
                    pictureSwiperThumbs.appendSlide(newSlideThumbs);
                }
            }
            pictureSwiper.on('slideChange', function () {
                var activeSlide = $(pictureSwiper.wrapperEl).find('.swiper-slide').eq(pictureSwiper.activeIndex);
                $header_text.text(activeSlide.find("img").attr("alt"));
            });
            pictureSwiper.slideTo(index, 0);
            pictureSwiperThumbs.slideTo(index, 0);
            $('#SwiperModal').modal('show');
            return false;
        });
    }
}