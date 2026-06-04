(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});
    if (typeof Coker.defineModule === "function") Coker.defineModule("member-pagination", factory);
    else factory(Coker);

    function factory() {
        var MemberPage = (w.MemberPage = w.MemberPage || {});

        MemberPage.Pagination = {
            init: function ($self, pageTotal, hashName) {
                $self.removeClass("d-none");

                for (var i = 1; i <= pageTotal; i++) {
                    var html = "";

                    if (i === pageTotal && pageTotal > 7) {
                        html += "<li class='page-item btn_page endhide'>" +
                            "<button class='d-none' title='...' disabled='disabled'>...</button>" +
                            "</li>";
                    }

                    html += "<li class='page-item btn_page'>" +
                        "<button class='d-none' data-page='" + i + "' title='切換至第" + i + "頁'>" + i + "</button>" +
                        "</li>";

                    if (i === 1 && pageTotal > 7) {
                        html += "<li class='page-item btn_page starthide'>" +
                            "<button class='d-none' title='...' disabled='disabled'>...</button>" +
                            "</li>";
                    }

                    $self.find(".btn_next").before(html);
                }

                $self.data("init", true);

                $self.find(".btn_prev button").on("click.memberPagination", function () {
                    $("html, body").animate({ scrollTop: 0 }, 0);

                    var pageNow = w.location.hash.indexOf("-") < 0
                        ? 1
                        : parseInt(w.location.hash.substring(w.location.hash.indexOf("-") + 1), 10);

                    if (pageNow > 1) pageNow -= 1;
                    MemberPage.Pagination.change($(this).closest("ul"), pageNow, pageTotal);
                    w.location.hash = "#" + hashName + "-" + pageNow;
                });

                $self.find(".btn_next button").on("click.memberPagination", function () {
                    $("html, body").animate({ scrollTop: 0 }, 0);

                    var pageNow = w.location.hash.indexOf("-") < 0
                        ? 1
                        : parseInt(w.location.hash.substring(w.location.hash.indexOf("-") + 1), 10);

                    if (pageNow < pageTotal) pageNow += 1;
                    MemberPage.Pagination.change($(this).closest("ul"), pageNow, pageTotal);
                    w.location.hash = "#" + hashName + "-" + pageNow;
                });

                $self.find(".btn_page button").on("click.memberPagination", function () {
                    $("html, body").animate({ scrollTop: 0 }, 0);

                    var $btn = $(this);
                    MemberPage.Pagination.change($btn.closest("ul"), $btn.data("page"), pageTotal);
                    w.location.hash = "#" + hashName + "-" + $btn.data("page");
                });
            },

            change: function ($self, page, pageTotal) {
                $self.find("li").each(function () {
                    var $btn = $(this).find("button");

                    if ($btn.data("page") == page) {
                        $btn.addClass("focus").attr("disabled", "disabled");
                    } else {
                        $btn.removeClass("focus").removeAttr("disabled");
                    }
                });

                if (pageTotal > 7) {
                    if (page < 4) {
                        $self.find("li.btn_page").each(function () {
                            var $btn = $(this).find("button");
                            if ($btn.data("page") <= 5 || $btn.data("page") == pageTotal) $btn.removeClass("d-none");
                            else $btn.addClass("d-none");
                        });
                    } else if (page > pageTotal - 3) {
                        $self.find("li.btn_page").each(function () {
                            var $btn = $(this).find("button");
                            if ($btn.data("page") >= pageTotal - 4 || $btn.data("page") == 1) $btn.removeClass("d-none");
                            else $btn.addClass("d-none");
                        });
                    } else {
                        $self.find("li.btn_page").each(function () {
                            var $btn = $(this).find("button");
                            if ((parseInt(page, 10) + 2 >= $btn.data("page") && $btn.data("page") >= parseInt(page, 10) - 2) || $btn.data("page") == 1 || $btn.data("page") == pageTotal) {
                                $btn.removeClass("d-none");
                            } else {
                                $btn.addClass("d-none");
                            }
                        });
                    }

                    if ($self.find("li button[data-page=2]").hasClass("d-none")) $self.find("li.starthide button").removeClass("d-none");
                    if ($self.find("li button[data-page=" + (pageTotal - 1) + "]").hasClass("d-none")) $self.find("li.endhide button").removeClass("d-none");
                } else {
                    $self.find("li button").removeClass("d-none");
                }
            }
        };
    }
})(window);
