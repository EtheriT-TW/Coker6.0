function MarqueeSwiper() {
    $(".marqueeSwiper").each(function () {
        const $marquee = $(this).find(".swiper-wrapper");
        $marquee.find(".swiper-slide").each(function () {
            const slideWidth = $(this).width();
            let $slide = $(this);
            let $newSlides = [];
            let nextText = [];
            let text = $slide.text().replace("(current)", "");
            let $tempDiv = $('<div class="temp-div"></div>').appendTo('body');
            $tempDiv.css('width', slideWidth + 'px');
            let previousHeight = $tempDiv.height();
            for (let i = 0; i < text.length; i++) {
                $tempDiv.append(text[i]);
                let currentHeight = $tempDiv.height();
                if (currentHeight > previousHeight) {
                    nextText.push(i);
                    previousHeight = currentHeight;
                }
            }
            for (let i = 0; i < nextText.length; i++) {
                let startIdx = nextText[i];
                let endIdx = i === nextText.length - 1 ? text.length : nextText[i + 1];
                let $newSlide = $slide.clone();
                $newSlide.find(".text").text(text.substring(startIdx, endIdx));
                $slide.before($newSlide);
            }
            $slide.remove();
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
}