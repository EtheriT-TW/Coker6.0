function MarqueeSwiper(SiteId) {
    var Marquee = {
        getMarqueeData: function (webid, placement) {
            return $.ajax({
                url: "/api/Marquee/GetAll",
                type: "GET",
                data: {
                    webid: webid,
                    placement: placement
                },
                contentType: 'application/json; charset=utf-8',
                dataType: "json"
            });
        }
    }
    $(".marqueeSwiper").each(function () {
        const marqueeContainer = $(this);
        const $marquee = $(this).find(".swiper-wrapper");
        $marquee.find(".swiper-slide").each(function () {
            const slideWidth = $(this).width();
            let $slide = $(this);
            const fontSize = $slide.find(".text").css('font-size');
            let $newSlides = [];
            let nextText = [];
            let text = $slide.text().replace("(current)", "");
            let $tempDiv = $('<div class="temp-div"></div>').appendTo('body');
            $tempDiv.css('width', slideWidth + 'px');
            $tempDiv.css('font-size', fontSize);
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
            $tempDiv.remove();
        });
    });
    var marqueeSwiper = new Swiper(".marqueeSwiper", {
        a11y: true,
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
    const webid = typeof (SiteId) == "undefined" ? 0 : SiteId;
    const placement = "Top";
    Marquee.getMarqueeData(webid, placement).done(function (result) {
        const marqueeContainer = $(".marqueeSwiper");
        const $marquee = marqueeContainer.find(".swiper-wrapper");
        const marqueeModels = result.map(function (item) {
            return {
                title: item.title,
                link: item.link,
                target: item.target
            };
        });

        if (typeof (marqueeContainer) == "undefined") return;
        marqueeContainer.innerHTML = '';
        marqueeModels.forEach(marquee => {
            // 创建 swiper-slide
            const marqueeItem = document.createElement('div');
            marqueeItem.classList.add('swiper-slide');

            let htmlContent;

            // 判断是否有链接，并生成相应的 HTML 结构
            if (!marquee.link) {
                htmlContent = `
                    <div class="overflow-hidden text" data-bs-toggle="tooltip" title="${marquee.title}">
                        ${marquee.title}
                    </div>
                `;
            } else {
                const target = marquee.target ? "_blank" : "_self";
                const title = marquee.target ? `連結至：${marquee.title}(另開視窗)` : `連結至：${marquee.title}`;

                htmlContent = `
                    <a class="overflow-hidden text" data-bs-toggle="tooltip" title='${title}' href='${marquee.link}' target="${target}">
                        ${marquee.title}
                    </a>
                `;
            }

            marqueeItem.innerHTML = htmlContent;

            // 将生成的内容加入到 swiper-wrapper
            $marquee.append(marqueeItem);
        });

        $marquee.find(".swiper-slide").each(function () {
            const slideWidth = $(this).width();
            let $slide = $(this);
            const fontSize = $slide.find(".text").css('font-size');
            let $newSlides = [];
            let nextText = [];
            let text = $slide.text().replace("(current)", "");
            let $tempDiv = $('<div class="temp-div"></div>').appendTo('body');
            $tempDiv.css('width', slideWidth + 'px');
            $tempDiv.css('font-size', fontSize);
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
            $tempDiv.remove();
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
    });
}