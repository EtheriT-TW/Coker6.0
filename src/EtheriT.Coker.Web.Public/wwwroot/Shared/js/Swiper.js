/*************** 
 obj:{
    autoplay:boolen 是否輪播
 }
 ***************/
//# sourceURL=Swiper.js
function SwiperInit(obj) {
    var config = {
        slidesPerView: 1,
        spaceBetween: 15,
        keyboard: {
            enabled: true,
        },
        lazy: {
            loadPrevNext: true,
        },
        on: {
            init: function () {
                const swiper = this;
                const setShow = function (event) {
                    event.preventDefault();
                    $(event.currentTarget).parents(".swiper").find(".outside-item").removeClass("show");
                    $(event.currentTarget).parents(".swiper-slide").find(".outside-item").addClass("show");
                }
                const hideShow = function (event) {
                    $(".swiper-slide .outside-item").removeClass("show");
                }
                $(this.el).find(".hover-outside").off("click").on("click", setShow);
                $(this.el).find(".hover-outside").off("mouseover").on("mouseover", setShow);
                $(this.el).find(".hover-outside").off("keydown").on("keydown", function (event) {
                    if (event.key === 'Enter' || event.key === ' ') { // 檢查 Enter 或空白鍵
                        setShow(event);
                    }
                });
                $(this.el).find(".outside-item").off("mouseover").on("mouseover", setShow);
                $(this.el).find(".hover-outside").off("mouseout").on("mouseout", hideShow);
                $(this.el).find(".outside-item").off("mouseout").on("mouseout", hideShow);
                $("body").off("click.disableoutsideItem").off("click.disableoutsideItem").on("click.disableoutsideItem", hideShow)
                this.isLoopActive = this.params.loop;
            },
            slideChange: function () {
                const swiper = this;
                if (swiper.params.loop) {
                    const $swiper = $(swiper.el);
                    const $sliders = $swiper.find(".swiper-slide");
                    const $focusedSlide = $swiper.find(":focus").parents(".swiper-slide");
                    const focusIndex = $sliders.index($focusedSlide);

                    if (typeof (focusIndex) != "undefined" && focusIndex >= 0 && swiper.isLoopActive) {
                        swiper.isLoopActive = false;
                        swiper.loopDestroy();
                    } else if (typeof (focusIndex) == "undefined" && !swiper.isLoopActive) {
                        swiper.isLoopActive = true;
                        swiper.loopCreate();
                    }
                }
            }
        }
    };
    $.fn.extend({
        swiperBindEven: function (swiper, canNext) {

            const checkSlides = function () {
                if (Array.isArray(swiper)) swiper = swiper[swiper.length-1];
                if (swiper == null) return;
                const totalSlides = swiper.slides.length;
                const slidesPerView = swiper.params.slidesPerView;
                // 檢查導航元素
                const nextEl = swiper.navigation.nextEl ? swiper.navigation.nextEl : null;
                const prevEl = swiper.navigation.prevEl ? swiper.navigation.prevEl : null;
                const paginationEl = swiper.pagination.el;
                if (totalSlides <= slidesPerView) {
                    // 停止自動輪播
                    swiper.autoplay.stop();
                    // 隱藏左右箭頭
                    if (!!nextEl && !Array.isArray(nextEl)) nextEl.classList.add("d-none");
                    if (!!prevEl && !Array.isArray(prevEl)) prevEl.classList.add("d-none");
                    if (!!paginationEl) paginationEl.classList.add("d-none");
                } else {
                    // 確保箭頭可見
                    if (!!nextEl && !Array.isArray(nextEl)) nextEl.classList.remove("d-none");
                    if (!!prevEl && !Array.isArray(prevEl)) prevEl.classList.remove("d-none");
                    if (!!paginationEl) paginationEl.classList.remove("d-none");
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
            const thisSwiper = $(this);
            $(this).off("mouseover").on("mouseover", stop);
            $(this).find(".swiper-slide a,.swiper-slide").off("focus").on("focus", function () {
                const $item = $(this).hasClass("swiper-slide") ? $(this) : $(this).parents(".swiper-slide");
                const activeIndex = $item.attr("aria-label").split(" / ")[0];
                swiper.slideTo(activeIndex - 1, 300);
                stop();
            });
            $(this).off("mouseout").on("mouseout", start);
            $(this).find("a").on("blob", start);
            $(this).find("button").prop("disabled", false);
            $(window).off('resize.swiper').on('resize.swiper', checkSlides);
            $(window).trigger("resize.swiper");
            swiper.update();
        }
    });

    $(".one_swiper").each(function () {
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
            if (typeof ($self.attr("id")) == "undefined") $self.attr("id", `id-${Math.random().toString(36).substring(2, 9)}-${Date.now()}`)
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
            let siwerTimer;
            var selfConfig = Object.assign({}, config, {
                pagination: {
                    el: "#" + $self.attr("id") + " .swiper_pagination",
                    clickable: true,
                }, on: {
                    slideChangeTransitionStart: function () {
                        // 暫停所有 video
                        var videos = $(swiper.wrapperEl).find('video');
                        videos.each(function () {
                            this.pause();
                        });

                        // 暫停所有 iframe
                        var iframes = $(swiper.wrapperEl).find('iframe');
                        iframes.each(function () {
                            var $iframe = $(this);
                            if ($iframe.attr('src')) {
                                let src = $iframe.attr('src');
                                if ($iframe.data("start_time") && src.includes("youtube") && src.includes("autoplay") && !src.includes("start=")) 
                                    src += `&start=${$iframe.data("start_time")}`; //如果有設定開始時間，則加上參數
                                $iframe.data('src', src);
                                $iframe.attr('src', '');
                            }
                        });
                    },
                    slideChangeTransitionEnd: function () {
                        var activeSlide = $(swiper.wrapperEl).find('.swiper-slide').eq(swiper.activeIndex);
                        var $video = $(activeSlide).find('video');
                        var $iframe = activeSlide.find('iframe');
                        var keepTime = $(activeSlide).data("keep_time") || 0;
                        clearTimeout(siwerTimer);
                        if (autoplay) swiper.autoplay.start();
                        if (keepTime) {
                            if (autoplay) {
                                swiper.autoplay.stop();
                                siwerTimer = setTimeout(() => {
                                    swiper.autoplay.start();
                                }, keepTime)
                            }
                        } else {
                            if ($video.length > 0) {
                                var videoEl = $video.get(0);
                                function getVideoDuration(videoEl) {
                                    return new Promise((resolve) => {
                                        if (videoEl.readyState >= 1 && !isNaN(videoEl.duration)) {
                                            resolve(videoEl.duration);
                                        } else {
                                            videoEl.addEventListener("loadedmetadata", function onLoaded() {
                                                videoEl.removeEventListener("loadedmetadata", onLoaded);
                                                resolve(videoEl.duration);
                                            });
                                        }
                                    });
                                }
                                var startTime = parseFloat($video.data('starttime')) || 0;
                                videoEl.currentTime = startTime;
                                videoEl.play();
                                if (autoplay) {
                                    swiper.autoplay.stop();
                                    getVideoDuration(videoEl).then((duration) => {
                                        const totalDuration = (duration - startTime) * 1000;
                                        siwerTimer = setTimeout(() => {
                                            swiper.autoplay.start();
                                        }, totalDuration);
                                    });
                                }
                            } else if ($iframe.length > 0 && $iframe.data('src')) {
                                $iframe.attr('src', $iframe.data('src'));
                            }
                        }
                    }
                },
                navigation: {
                    nextEl: "#" + $self.attr("id") + " .swiper_button_next",
                    prevEl: "#" + $self.attr("id") + " .swiper_button_prev",
                },
                effect: effect,
                speed: speed
            }, autoplay ? {
                autoplay: {
                    delay: 5000,
                    disableOnInteraction: false,
                },
                loop: true
            } : {});
            var swiper = new Swiper(Id, selfConfig);
            selfConfig.on.slideChangeTransitionEnd.call(swiper);
            $self.data("isInit", true)
            $self.swiperBindEven(swiper);
        }
    });
    //單欄輪播+兩欄縮圖
    $(".one_swiper_thumbs").each(function () {
        var $self = $(this);
        const index = $self.index(this);
        $self.find(".six_thumbs > .swiper-wrapper").empty();
        if (!!!$self.data("isInit")) {
            if (typeof ($self.attr("id")) == "undefined") $self.attr("id", `id-${Math.random().toString(36).substring(2, 9)}-${Date.now()}`)
            var Id = "#" + $self.attr("id") + " .swiper";
            const canNext = $(Id).find(".swiper-slide").length >= 2;
            var effect = $self.data("effect");
            var speed = $self.data("effect-speed");

            var swiperThumbs = new Swiper(".six_thumbs", {
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
                navigation: {
                    nextEl: "#" + $self.attr("id") + " .swiper_button_next",
                    prevEl: "#" + $self.attr("id") + " .swiper_button_prev",
                },
                effect: effect,
                speed: speed
            }, autoplay ? {
                autoplay: {
                    delay: 5000,
                    disableOnInteraction: false,
                },
                loop: true
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
            if (autoplay) PauseOnMouseEnter(swiper, $self.find(".swiper"))
        }
    });
    $(".two_swiper").each(function () {
        var $self = $(this);
        if (!!!$self.data("isInit")) {
            if (typeof ($self.attr("id")) == "undefined") $self.attr("id", `id-${Math.random().toString(36).substring(2, 9)}-${Date.now()}`)
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
    $(".three_swiper").each(function () {
        var $self = $(this);
        if (!!!$self.data("isInit")) {
            if (typeof ($self.attr("id")) == "undefined") $self.attr("id", `id-${Math.random().toString(36).substring(2, 9)}-${Date.now()}`)
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
    $(".four_swiper").each(function () {
        var $self = $(this);
        if (!!!$self.data("isInit")) {
            if (typeof ($self.attr("id")) == "undefined") $self.attr("id", `id-${Math.random().toString(36).substring(2, 9)}-${Date.now()}`)
            var Id = "#" + $self.attr("id") + " > .swiper";
            const $swiper = $(Id);
            const freeModeAttr = $swiper.data('freeMode');
            const speedAttr = $swiper.data('speed');
            const delayAttr = $swiper.data('delay');
            const freeMode = freeModeAttr === undefined ? false : freeModeAttr;
            const speed = Number.isFinite(parseInt(speedAttr)) ? parseInt(speedAttr) : 300;
            const delay = (delayAttr === undefined ? 5000 : parseInt(delayAttr));

            var selfConfig = Object.assign({}, config, {
                speed: speed,
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
                    delay: delay,
                    disableOnInteraction: false,
                },
                loop: true
            } : {}, freeMode ? {
                freeMode: {
                    enabled: true,
                    momentum: false,
                    sticky: false,
                }
            } : {});
            var swiper = new Swiper(Id, selfConfig);
            $self.data("isInit", true)
            obj.autoplay && $self.swiperBindEven(swiper);
            $self.prepend($("#" + $self.attr("id") + " .swiper_button_prev"));
        }
    });
    $(".five_swiper").each(function () {
        var $self = $(this);
        if (!!!$self.data("isInit")) {
            if (typeof ($self.attr("id")) == "undefined") $self.attr("id", `id-${Math.random().toString(36).substring(2, 9)}-${Date.now()}`)
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
    $(".six_swiper").each(function () {
        var $self = $(this);
        if (!!!$self.data("isInit")) {
            if (typeof ($self.attr("id")) == "undefined") $self.attr("id", `id-${Math.random().toString(36).substring(2, 9)}-${Date.now()}`)
            var Id = "#" + $self.attr("id") + " > .swiper";
            const $swiper = $(Id);
            const freeModeAttr = $swiper.data('freeMode');
            const speedAttr = $swiper.data('speed');
            const delayAttr = $swiper.data('delay');
            const freeMode = freeModeAttr === undefined ? false : freeModeAttr;
            const speed = Number.isFinite(parseInt(speedAttr)) ? parseInt(speedAttr) : 300;
            const delay = (delayAttr === undefined ? 5000 : parseInt(delayAttr));

            var selfConfig = Object.assign({}, config, {
                speed: speed,
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
                    delay: delay,
                    disableOnInteraction: false,
                },
                loop: true
            } : {}, freeMode ? {
                freeMode: {
                    enabled: true,
                    momentum: false,
                    sticky: false,
                }
            } : {});
            var swiper = new Swiper(Id, selfConfig);
            $self.data("isInit", true)
            obj.autoplay && $self.swiperBindEven(swiper);
            $self.prepend($("#" + $self.attr("id") + " .swiper_button_prev"));
        }
    });
    if ($(".picture-category").length > 0 && $("#SwiperModal").length > 0) {
        if (!!!$(this).data("isinit")) {
            const $header_text = $("#SwiperModal .modal-header .imgalt");
            $header_text.text("");
            let pictureSwiperThumbsOptions = {
                spaceBetween: 10,
                slidesPerView: 'auto',
                freeMode: true,
                centeredSlides: false,
                centeredSlidesBounds: false,
                watchSlidesProgress: true,
                watchOverflow: true
            };
            let pictureSwiperThumbs = new Swiper("#pictureSwiperThumbs", pictureSwiperThumbsOptions);
            let pictureSwiperOptions = {
                centeredSlides: true,
                spaceBetween: 10,
                loop: false,
                loopAdditionalSlides: 0,
                zoom: { maxRatio: 5 },
                navigation: {
                    nextEl: "#pictureSwiper .swiper-button-next",
                    prevEl: "#pictureSwiper .swiper-button-prev",
                },
                pagination: {
                    el: "#pictureSwiper .swiper-pagination",
                },
                thumbs: {
                    swiper: pictureSwiperThumbs,
                },
                autoplay: {
                    delay: 5000,
                    disableOnInteraction: false,
                }, on: {
                    slideChange: function () {
                        var activeSlide = $(pictureSwiper.wrapperEl).find('.swiper-slide').eq(pictureSwiper.activeIndex);
                        $header_text.text(activeSlide.find("img").attr("alt"));
                    },
                    slideChangeTransitionStart: function () {
                        // 暫停所有 video
                        var videos = $(pictureSwiper.wrapperEl).find('video');
                        videos.each(function () {
                            this.pause();
                        });

                        // 暫停所有 iframe
                        var iframes = $(pictureSwiper.wrapperEl).find('iframe');
                        iframes.each(function () {
                            var $iframe = $(this);
                            if ($iframe.attr('src')) {
                                $iframe.data('src', $iframe.attr('src'));
                                $iframe.attr('src', '');
                            }
                        });
                    },
                    slideChangeTransitionEnd: function () {
                        // 恢復當前 slide iframe
                        var activeSlide = $(pictureSwiper.wrapperEl).find('.swiper-slide').eq(pictureSwiper.activeIndex);
                        var $video = $(activeSlide).find('video');
                        var $iframe = activeSlide.find('iframe');

                        if ($video.length > 0) {
                            var startTime = parseFloat($video.data('starttime')) || 0;
                            $video[0].currentTime = startTime;
                            $video[0].play();
                        } else if ($iframe.length > 0 && $iframe.data('src')) {
                            $iframe.attr('src', $iframe.data('src'));
                        }
                    }
                }
            };
            let pictureSwiper = new Swiper("#pictureSwiper", pictureSwiperOptions);

            $(".picture-category a").attr("href", "#SwiperModal").on("click", function () {
                pictureSwiper.removeAllSlides();
                pictureSwiperThumbs.removeAllSlides();
                pictureSwiper.update();
                pictureSwiperThumbs.update();
                const self = this;
                var $self = $(this).parents(".picture-category");
                var index = $self.find("a").index(this);
                var $items = [];
                var $images = [];
                $self.find(".templatecontent img,.swiper-slide img").each(function () {
                    var obj = {};
                    var $a = $(this).parents("a");
                    var link = $a.data("link") || "";
                    var ratio = $a.data("ratio") || "9x16";
                    var start_time = $a.data("start_time") || 0;
                    var keep_time = $(this).data("keep_time") || 5;
                    keep_time = keep_time * 1000;
                    obj['src'] = $(this).attr("src");
                    obj['alt'] = typeof ($(this).attr("alt")) == "undefined" ? "" : $(this).attr("alt");
                    $images.push(obj);
                    if (link.startsWith("https://www.youtube.com") || link.startsWith("https://www.facebook.com")) $items.push({ type: "iframe", src: link, ratio: ratio, startTime: start_time, keepTime: keep_time });
                    else if (isVideoFile(link)) $items.push({ type: "video", src: link, ratio: ratio, startTime: start_time, keepTime: keep_time });
                    else $items.push({ type: "image", src: obj['src'], keepTime: keep_time, alt: obj['alt'] });
                });
                $header_text.text($images[index]['alt']);

                for (let i = 0; i < $items.length; i++) {
                    var item = $items[i];
                    var newSlide = "";
                    var newSlideThumbs = "";
                    if (item.type === "image") {
                        newSlide = `<div class="swiper-slide" data-swiper-autoplay="${item.keepTime}">
                            <div class="swiper-zoom-container">
                                <img src="${item.src}" alt="${item.alt}" />
                            </div>
                        </div>`;
                    } else if (item.type === "video") {
                        newSlide = `<div class="swiper-slide" data-swiper-autoplay="${item.keepTime}">
                            <div class="video-content video-${item.ratio}">
                                <video controls preload="metadata" poster="${$images[i].src}" data-startTime="${item.startTime}">
                                    <source src="${item.src}" type="video/mp4">
                                    Your browser does not support the video tag.
                                </video>
                            </div>
                        </div>`;
                    } else if (item.type === "iframe") {
                        newSlide = `<div class="swiper-slide" data-swiper-autoplay="${item.keepTime}">
                            <div class="video-content video-${item.ratio}">
                                <iframe src="${item.src}" frameborder="0" allowfullscreen allow="autoplay; encrypted-media"></iframe>
                            </div>
                        </div>`;
                    }
                    newSlideThumbs = `<div class="swiper-slide align-content-center ms-1 me-2"><img src="${$images[i].src}" data-keepTime="${item.keepTime}" alt="${$images[i].alt}" /></div>`;
                    pictureSwiper.appendSlide(newSlide);
                    pictureSwiperThumbs.appendSlide(newSlideThumbs);
                }
                pictureSwiper.autoplay.stop();

                if ($items.length > 1) {
                    PauseOnMouseEnter(pictureSwiper, $("#pictureSwiper"));
                    pictureSwiper.autoplay.start();
                }

                const images = document.querySelectorAll('#SwiperModal .swiper-slide-active img');
                $('#SwiperModal').modal('show');
                //let loadedCount = 0;
                //images.forEach(img => {
                //    img.onload = () => {
                //        loadedCount++;
                //        if (loadedCount === images.length) {
                //            // 確保全部圖片都載入後再測量
                //            $('#SwiperModal').modal('show');
                //        }
                //    };
                //});
                $('#SwiperModal').off("shown.bs.modal").on("shown.bs.modal", function () {
                    const wrapper = document.querySelector('#pictureSwiperThumbs .swiper-wrapper');
                    const container = document.querySelector('#pictureSwiperThumbs');
                    const wrapperWidth = wrapper.scrollWidth;
                    const containerWidth = container.clientWidth;
                    if (wrapperWidth > containerWidth) {
                        pictureSwiperThumbs.destroy(false, false);
                        pictureSwiperThumbsOptions.centeredSlides = true;
                        pictureSwiperThumbsOptions.centeredSlidesBounds = true;
                        pictureSwiperThumbs = new Swiper("#pictureSwiperThumbs", pictureSwiperThumbsOptions);
                        pictureSwiperThumbs.update();
                        pictureSwiperOptions.thumbs.swiper = pictureSwiperThumbs;
                        pictureSwiper.destroy(false, false);
                        pictureSwiper = new Swiper("#pictureSwiper", pictureSwiperOptions);
                        pictureSwiper.update();
                        pictureSwiper.on('slideChange', function () {
                            const index = pictureSwiper.realIndex;
                            pictureSwiperThumbs.slideTo(index - 1); // 預留一格避免卡邊
                        });
                        $('#pictureSwiper').swiperBindEven(pictureSwiper);
                    }
                    pictureSwiper.slideToLoop(index, 0,false);
                    pictureSwiperThumbs.slideTo(index, 0,false);
                });
                $('#SwiperModal').off("hide.bs.modal").on("hide.bs.modal", function () {
                    document.activeElement.blur();
                });
                $('#SwiperModal').off("hidden.bs.modal").on("hidden.bs.modal", function () {
                    self.focus();
                });
                return false;
            });
            $(this).data("isinit", true);
        }
    }
    $(".three_two_grid_swiper").each(function () {
        var $self = $(this);
        if (!!!$self.data("isInit")) {
            if (typeof ($self.attr("id")) == "undefined") $self.attr("id", `id-${Math.random().toString(36).substring(2, 9)}-${Date.now()}`)
            var Id = "#" + $self.attr("id") + " > .swiper"
            const $template = $(Id).find(".swiper-slide").parents(".templatecontent,.template_slide");
            const length = $template.length === 0 ? $(Id).find(".swiper-slide").length : $(Id).find(".swiper-slide").length - 1;
            const canNext = length > 6;
            var autoplay = obj.autoplay ? canNext : false;
            const selfConfig = Object.assign({}, config, {
                breakpoints: {
                    375: {
                        slidesPerView: 1,
                        grid: { rows: 1, fill: "row" },
                    },
                    576: {
                        slidesPerView: 2,
                        grid: { rows: 1, fill: "row" },
                    },
                    992: {
                        slidesPerView: 3,
                        grid: { rows: 2, fill: "column" },
                    },
                },
                pagination: {
                    el: "#" + $self.attr("id") + " .swiper-pagination",
                    clickable: true,
                },
                navigation: {
                    nextEl: "#" + $self.attr("id") + " .swiper_button_next",
                    prevEl: "#" + $self.attr("id") + " .swiper_button_prev",
                },
            }, obj.autoplay ? {
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
            autoplay && $self.swiperBindEven(swiper);
            $self.prepend($("#" + $self.attr("id") + " .swiper_button_prev"));
        }
    });

    $(".vertical_swiper_thumbs").each(function () {
        var $self = $(this);
        if (!!!$self.data("isInit")) {
            if (typeof ($self.attr("id")) == "undefined") $self.attr("id", `id-${Math.random().toString(36).substring(2, 9)}-${Date.now()}`)
            var Id = "#" + $self.attr("id") + " .swiper"
            const $template = $(Id).find(".swiper-slide").parents(".templatecontent,.template_slide");
            const length = $template.length === 0 ? $(Id).find(".swiper-slide").length : $(Id).find(".swiper-slide").length - 1;
            const canNext = length > 3;
            var autoplay = obj.autoplay ? canNext : false;
            var swiperThumbs = new Swiper(`#${$self.attr("id")} .swiper_thumbs`, {
                slidesPerView: 1,
                direction: "horizontal",
                loop: true,
                breakpoints: {
                    768: {
                        direction: "vertical",
                        watchSlidesProgress: true,
                        loop: canNext,
                    },
                },
                navigation: {
                    nextEl: "#" + $self.attr("id") + " .swiper_thumbs .swiper_button_next",
                    prevEl: "#" + $self.attr("id") + " .swiper_thumbs .swiper_button_prev",
                },
            });
            const selfConfig = Object.assign({}, config, {
                loop: true,
                slidesPerView: 1,
                direction: "horizontal",
                spaceBetween: 0,
                breakpoints: {
                    768: {
                        direction: "vertical",
                        loop: canNext,
                        slidesPerView: 3,
                    },
                },
                centeredSlides: true,
                effect: "coverflow",
                coverflowEffect: {
                    rotate: 0,
                    stretch: 0,
                    depth: 100,
                    modifier: 1,
                    slideShadows: true,
                },
                thumbs: {
                    swiper: swiperThumbs,
                },
                navigation: {
                    nextEl: "#" + $self.attr("id") + " .swiper .swiper_button_next",
                    prevEl: "#" + $self.attr("id") + " .swiper .swiper_button_prev",
                },
            }, obj.autoplay ? {
                autoplay: {
                    delay: 5000,
                    disableOnInteraction: false,
                }
            } : {});
            var swiper = new Swiper(Id, selfConfig);
            swiper.on('slideChange', function () {
                var slides = $(swiper.slides);
                slides.each(function (index, slide) {
                    if (index < swiper.activeIndex - 1 || index > swiper.activeIndex + 1) {
                        $(slide).addClass('hidden-slide');
                    } else {
                        $(slide).removeClass('hidden-slide');
                    }
                });
            });
            $self.data("isInit", true)
            autoplay && $self.swiperBindEven(swiper);
            swiper.slideTo(0);
            swiper.loopCreate();
            swiper.update();
        }
    });

    $(".swiper_same_height").each(function () {
        var $self = $(this);
        SameHeight($self);
    });

    $(window).resize(function () {
        $(".swiper_same_height").each(function () {
            var $self = $(this);
            SameHeight($self);
        });
    });
}

/*滑鼠覆蓋暫停輪播 pauseOnMouseEnter: true 無作用時使用*/
function PauseOnMouseEnter(swiper, $container) {
    $container.on("mouseenter", function () {
        swiper.autoplay.stop();
    })
    $container.on("mouseleave", function () {
        swiper.autoplay.start();
    })
}

/* 強制高度設置 */
function SameHeight($swiper) {
    var height = $swiper.find(".swiper-wrapper").css("height");
    $swiper.find(".swiper-wrapper .swiper-slide").each(function () {
        var $slide = $(this);
        $slide.css("min-height", height);
    });
}

const isVideoFile = function (url) {
    const videoExtensions = [".mp4", ".webm", ".ogg", ".mov"];
    return videoExtensions.some(ext => url.toLowerCase().endsWith(ext));
}