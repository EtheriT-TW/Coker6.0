function SitemapInit() {
    var WebMesnus = {
        GetAllById: function () {
            return $.ajax({
                url: "/api/WebMenu/GetAll/",
                type: "GET",
                contentType: 'application/json; charset=utf-8'
            });
        },
    };
    WebMesnus.GetAllById().done(function (result) {
        CreateSitemap(result.maps);
    });
}

function CreateSitemap(result) {
    var frame = $(".sitemap_hierarchical_frame");
    const $firstUl = $(`<ul class=""></ul>`);
    frame.append($firstUl);

    var index_f = 1;

    result.forEach(function (data) {
        var first = $($("#Hierarchical_First_Item").html()).clone();
        var first_item = first.find(".first");

        if (data.linkUrl != "" || data.hasContan) {
            first_item.html(`<a class="first d-block text-black px-3 rounded-3" href="${!!data.routerName ? `/${data.orgName || OrgName}/${data.routerName}` : data.linkUrl}" alt="${data.text}">${index_f} ${data.title}</a>`)
        }else first_item.text(`${index_f} ${data.title}`);


        first.append(`<ul class="ps-5 ps-md-0 col-md-8 py-1"></ul>`);
        var second_frame = first.find("ul");
        var index_s = 1;
        if (data.children != null) {
            data.children.forEach(function (sec_data) {
                var second = $($("#Hierarchical_Second_Item").html()).clone();
                var second_item = second.find(".second");
                if (sec_data.linkUrl != "" || sec_data.hasContan) {
                    console.log(sec_data.title, sec_data.orgName);
                    second_item.attr("href", !!sec_data.routerName ? `/${sec_data.orgName || OrgName}/${sec_data.routerName}` : sec_data.linkUrl);
                    second_item.attr("title", sec_data.text);
                    second_item.append(`<span class="d-flex d-md-none material-symbols-outlined position-absolute start-0">expand_more</span><span class="second p-sm-0 ps-2">${index_f}-${index_s} ${sec_data.title}</span>`);
                } else {
                    second_item = second_item.changeTagName("span").removeAttr("href title");
                    second_item.text(`${index_f}-${index_s} ${sec_data.title}`);
                }
                
                if (sec_data.children != null) {
                    second_item.append(`<span class="d-none d-md-flex material-symbols-outlined">navigate_next</span>`);
                    second_item.removeClass("ps-4 ps-sm-3");
                    second.append(`<ul class="ps-5 ps-md-0 col-md-6"></ul>`);
                    var third_frame = second.find("ul");
                    var index_t = 1;

                    sec_data.children.forEach(function (third_data) {
                        var third = $($("#Hierarchical_Third_Item").html()).clone();
                        var third_item = third.find(".third");
                        third_item.attr("href", !!third_data.routerName ? `/${third_data.orgName || OrgName}/${third_data.routerName}` : third_data.linkUrl);
                        third_item.attr("alt", third_data.text);
                        third_item.text(`${index_f}-${index_s}-${index_t} ${third_data.title}`);
                        third_frame.append(third);
                        index_t++;

                        if ($("#Hierarchical_Fourth_Item").length > 0 && third_data.children != null) {
                            third.append(`<ul class="ps-5 ps-md-5 col-md-6"></ul>`);
                            var fourth_frame = third.find("ul");
                            var index_fo = 1;
                            third_data.children.forEach(function (fourth_data) {
                                var fourth = $($("#Hierarchical_Fourth_Item").html()).clone();
                                var fourth_item = fourth.find(".fourth");
                                fourth_item.attr("href", !!fourth_data.routerName ? `/${fourth_data.orgName || OrgName}/${fourth_data.routerName}` : fourth_data.linkUrl);
                                fourth_item.attr("alt", fourth_data.text);
                                fourth_item.text(`${index_f}-${index_s}-${index_t}-${index_fo} ${fourth_data.title}`);
                                fourth_frame.append(fourth);
                                index_fo++;
                            });
                        }
                    });

                }

                second_frame.append(second);
                index_s++;
            });
        } else {
            first.find(".material-symbols-outlined").remove();
        }

        if (index_f == result.length) first.removeClass("border-bottom");
        $firstUl.append(first);
        index_f++;

    })
}