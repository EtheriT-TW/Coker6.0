var $CollapseHistory

function SideFloatingInit() {
    Coker.Product = {
        GetHistoryDis: function (id) {
            return $.ajax({
                url: "/api/Product/GetHistoryDisplay",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                data: { TId: id },
            });
        }
    };

    $CollapseHistory = $("#CollapseHistory")

    ProdHistorySet();

    $("#btn_chat").on("click", function () {
        $("#Chatbot_Frame").toggleClass("show");
    });

    $("#btn_gotop").on("click", function () {
        $('html, body').animate({ scrollTop: 0 }, 0);
    });

    $(".history_prod").on("click", function () {
        ClickLog($(this).data("pid"))
    });
}

function ProdHistorySet() {
    $CollapseHistory.addClass("d-none");
    $CollapseHistory.children(".history_list").children(".history_prod").remove();
    if ($.cookie('Token') != null) {
        Coker.Product.GetHistoryDis($.cookie('Token')).done(function (result) {
            if (result.length > 0) {
                $CollapseHistory.removeClass("d-none");
                result.forEach(function (item) {
                    var history_prod = `<li class='history_prod mb-1 border bg-white' data-pid='${item.id}'><a class='pro_link' href='${OrgName}/Toilet/${item.id}'><img class='img-fluid' src='/upload/product/pro_0${item.id}.png' alt='' /></a></li>`;
                    $CollapseHistory.children(".history_list").append(history_prod);
                })
            }
        })
    }
}