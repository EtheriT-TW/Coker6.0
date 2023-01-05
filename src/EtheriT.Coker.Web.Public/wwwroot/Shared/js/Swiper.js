function SwiperInit() {
    $(".one_swiper").each(function () {
        var $self = $(this);
        var Id = "#" + $self.attr("id") + " > .swiper"
        var swiper = new Swiper(Id, {
            slidesPerView: 1,
            spaceBetween: 15,
            loop: true,
            autoplay: {
                delay: 5000,
                disableOnInteraction: false,
            },
            pagination: {
                el: Id + " > .swiper_pagination > *",
                clickable: true,
            },
            navigation: {
                nextEl: Id + " > .swiper_button_prev > button",
                prevEl: Id + " > .swiper_button_next > button",
            },
        });
    });

    $(".two_swiper").each(function () {
        var $self = $(this);
        var Id = "#" + $self.attr("id") + " > .swiper"
        var swiper = new Swiper(Id, {
            slidesPerView: 1,
            spaceBetween: 15,
            loop: true,
            autoplay: {
                delay: 5000,
                disableOnInteraction: false,
            },
            pagination: {
                el: Id + " > .swiper_pagination > *",
                clickable: true,
            },
            navigation: {
                nextEl: Id + " > .swiper_button_prev > button",
                prevEl: Id + " > .swiper_button_next > button",
            },
            breakpoints: {
                375: {
                    slidesPerView: 2,
                }
            }
        });
    });

    $(".four_swiper").each(function () {
        var $self = $(this);
        var Id = "#" + $self.attr("id") + " > .swiper"
        var swiper = new Swiper(Id, {
            slidesPerView: 1,
            spaceBetween: 15,
            loop: true,
            autoplay: {
                delay: 5000,
                disableOnInteraction: false,
            },
            pagination: {
                el: Id + " > .swiper_pagination > *",
                clickable: true,
            },
            navigation: {
                nextEl: Id + " > .swiper_button_prev > button",
                prevEl: Id + " > .swiper_button_next > button",
            },
            breakpoints: {
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
        });
    });
}