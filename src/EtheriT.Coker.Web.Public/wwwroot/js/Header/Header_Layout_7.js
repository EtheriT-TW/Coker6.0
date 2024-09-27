var time = 0;
function HeaderInit() {
    const $main = $("<div class='header'>") //高峰重複的container
    const $contain = $("body:not(.home) #main > *");
    $("#breadcrumb").prependTo($main);
    $contain.appendTo($main);
    $("#main").append($main);
    $(".marqueeSwiper").each(function () {
        const $marquee = $(this).find(".swiper-wrapper"); 
        console.log("網站的開設讓大眾能方便的瞭解與本公司相關的資訊，歡迎有需要買賣剪床、折床的客戶，".length);
        $marquee.find(".swiper-slide").each(function () {
            let maxLen = 100;
            let $slide = $(this);
            let txt = $slide.text().replace("(current)", "");
            if ($(window).width() < 576) { 
                maxLen = 15;
            } else if ($(window).width() < 768) {
                maxLen = 21;
            } else if ($(window).width() < 992) {
                maxLen = 38;
            }
            const count = Math.floor(txt.length / maxLen) - (txt.length % maxLen > 0 ? 0 : 1);
            if (count > 0) $slide.find(".text").text(txt.substring(0, maxLen));
            for (let i = 1; i < count; i++) {
                let $newSlide = $slide.clone();
                $newSlide.find(".text").text(txt.substring((i * maxLen), ((i+1)* maxLen)));
                console.log($newSlide.find(".text").text(), i , maxLen);
                $slide.after($newSlide);
            }
        });
    });
    var marqueeSwiper = new Swiper(".marqueeSwiper", {
        direction: "vertical",
        allowTouchMove: false,
        keyboard: {
            enabled: true,
        },
        autoplay: {
            delay: 3000,
        },
    });
    const checkFunction = function () {
        if (typeof ($(".marqueeSwiper").swiperBindEven) == "function")
            $(".marqueeSwiper").swiperBindEven(marqueeSwiper);
        else
            setTimeout(checkFunction, 300);
    }
    checkFunction();

    const clickBtnMenu = function () {
        var icon = document.getElementById("menuIcon");
        if (icon.classList.contains("fa-bars")) {
            icon.classList.remove("fa-bars");
            icon.classList.add("fa-times-square"); // 切換成叉叉
        } else {
            icon.classList.remove("fa-times-square");
            icon.classList.add("fa-bars"); // 切換回漢堡
        }
    }

    document.getElementById("btnMenu").addEventListener("click", clickBtnMenu);

    /* ThreeSwiper */
    var threeSwiper = new Swiper(".threeSwiper", {
        slidesPerView: 1,
        loop: true,
        allowTouchMove: true,
        navigation: {
            nextEl: ".threeSwiper>.swiper-button-next",
            prevEl: ".threeSwiper>.swiper-button-prev",
        },
        keyboard: {
            enabled: true,
        },
        breakpoints: {
            768: {
                slidesPerView: 3,
                spaceBetween: 50,
                allowTouchMove: false,
            },
        },
    });

    /*Planning Swiper */
    var planningSwiper = new Swiper(".planningSwiper", {
        loop: true,
        slidesPerView: 1,
        breakpoints: {
            768: {
                direction: "vertical",
                watchSlidesProgress: true,
                allowTouchMove: false,
            },
        },
        keyboard: {
            enabled: true,
        },
        navigation: {
            nextEl: ".planningSwiper>.swiper-button-next",
            prevEl: ".planningSwiper>.swiper-button-prev",
        },
    });

    var planningThumbsSwiper = new Swiper(".planningThumbsSwiper", {
        allowTouchMove: true,
        direction: "vertical",
        effect: "coverflow",
        loop: true,
        slidesPerView: 2,
        centeredSlides: true,
        coverflowEffect: {
            rotate: 0,
            stretch: 0,
            depth: 150,
            scale: 0.9,
            modifier: 1,
            slideShadows: false,
        },
        keyboard: {
            enabled: true,
        },
        navigation: {
            nextEl: ".planning>.swiper-button-next",
            prevEl: ".planning>.swiper-button-prev",
        },
        thumbs: {
            swiper: planningSwiper,
        },
    });

    /*Outcome Swiper */
    var outcomeswiper = new Swiper(".outcomeSwiper", {
        slidesPerView: 2,
        grid: {
            rows: 2,
        },
        breakpoints: {
            768: {
                slidesPerView: 4,
            },
        },
        keyboard: {
            enabled: true,
        },
        pagination: {
            el: ".outcomeSwiper .swiper-pagination",
        },
        navigation: {
            nextEl: ".outcomeSwiper>.swiper-button-next",
            prevEl: ".outcomeSwiper>.swiper-button-prev",
        },
    });

    $(window).scroll(function () {
        $('.hideme:not(.show)').each(function (i) {
            var $self = $(this);
            var bottom_of_object = $self.offset().top + $self.outerHeight();
            var bottom_of_window = $(window).scrollTop() + $(window).height();
            if (bottom_of_window > bottom_of_object) {
                $self.addClass("show");
                time = $self.data("swiper-slide-index") * 500;
                $self.delay(time).animate({ 'opacity': '1' }, 500);
            }
        });
    });
}