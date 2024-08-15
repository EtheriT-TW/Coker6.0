function SitemapInit() {
    var WebMesnus = {
        GetAllById: function (id) {
            return $.ajax({
                url: "/api/WebMenu/GetAll/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                data: { WebsiteID: id },
            });
        },
    };
    if (typeof (SiteId) != "undefined") {
        WebMesnus.GetAllById(SiteId).done(function (result) {
            CreateSitemap(result.maps);
        });
    } else {
        WebMesnus.GetAllById(null).done(function (result) {
            CreateSitemap(result.maps);
        });
    }
}

function CreateSitemap(result) {
    var frame = $(".sitemap_hierarchical_frame");
    const $firstUl = $(`<ul class=""></ul>`);
    frame.append($firstUl);

    var index_f = 1;

    result.forEach(function (data) {
        var first = $($("#Hierarchical_First_Item").html()).clone();
        var first_item = first.find(".first");

        if (data.children != null) {

            first_item.text(`${index_f} ${data.title}`);
            first.append(`<ul class="ps-5 ps-md-0 col-md-8 py-1"></ul>`);
            var second_frame = first.find("ul");
            var index_s = 1;

            data.children.forEach(function (sec_data) {
                var second = $($("#Hierarchical_Second_Item").html()).clone();
                var second_item = second.find(".second");
                second_item.attr("href", sec_data.routerName);
                second_item.attr("alt", sec_data.text);

                if (sec_data.children != null) {
                    second_item.removeClass("ps-4 ps-sm-3");
                    second_item.append(`<span class="d-flex d-md-none material-symbols-outlined position-absolute start-0">expand_more</span><span class="second p-sm-0 ps-2">${index_f}-${index_s} ${sec_data.title}</span><span class="d-none d-md-flex material-symbols-outlined">navigate_next</span>`);

                    second.append(`<ul class="ps-5 ps-md-0 col-md-6"></ul>`);
                    var third_frame = second.find("ul");
                    var index_t = 1;

                    sec_data.children.forEach(function (third_data) {
                        var third = $($("#Hierarchical_Third_Item").html()).clone();
                        var third_item = third.find(".third");
                        third_item.attr("href", `${third_data.routerName}`);
                        third_item.attr("alt", third_data.text);
                        third_item.text(`${index_f}-${index_s}-${index_t} ${third_data.title}`);
                        third_frame.append(third);
                        index_t++;
                    });

                } else {
                    second_item.text(`${index_f}-${index_s} ${sec_data.title}`);
                }

                second_frame.append(second);
                index_s++;

            });
        } else {

            first_item.parent("div").parent("div").html(`<a class="first d-block text-black px-3 rounded-3" href="${data.routerName}" alt="${data.text}">${index_f} ${data.title}</a>`)

        }
        if (index_f == result.length) first.removeClass("border-bottom");
        $firstUl.append(first);
        index_f++;

    })
}