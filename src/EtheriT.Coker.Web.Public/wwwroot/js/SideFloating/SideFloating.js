    // 側邊歷史紀錄已移除
    // var $CollapseHistory

function SideFloatingInit() {
    // 側邊歷史紀錄已移除
    //Coker.Product = {
    //    GetHistoryDis: function (id) {
    //        return $.ajax({
    //            url: "/api/Product/GetHistoryDisplay",
    //            type: "GET",
    //            contentType: 'application/json; charset=utf-8',
    //            data: { TId: id },
    //        });
    //    }
    //};

    // 側邊歷史紀錄已移除
    // $CollapseHistory = $("#CollapseHistory")
    // ProdHistorySet();

    $("#btn_chat").on("click", function () {
        $("#Chatbot_Frame").toggleClass("show");
    });

    $("#btn_gotop").on("click", function () {
        $('html, body').animate({ scrollTop: 0 }, 0);
    });

    var adid = $("#Floating_Center > div").data("aid");
    if (adid != "undefined") {
        Advertise.ActivityExposure(adid).done(function (result) {
            //console.log(result)
        })
        $("#Floating_Center > div").on("click", function () {
            Advertise.ActivityClick(adid).done(function (result) {
                //console.log(result)
            })
        });
    }
}

// 側邊歷史紀錄已移除
//function ProdHistorySet() {
//    $CollapseHistory.addClass("d-none");
//    $CollapseHistory.children(".history_list").children(".history_prod").remove();
//    if ($.cookie('Token') != null) {
//        Coker.Product.GetHistoryDis($.cookie('Token')).done(function (result) {
//            if (result.length > 0) {
//                $CollapseHistory.removeClass("d-none");
//                result.forEach(function (item) {
//                    var history_prod = `<li class='history_prod mb-1 border bg-white' data-pid='${item.id}'><a class='pro_link' href='${OrgName}/Toilet/${item.id}'><img class='img-fluid' src='/upload/product/pro_0${item.id}.png' alt='' /></a></li>`;
//                    $CollapseHistory.children(".history_list").append(history_prod);
//                })
//            }
//        })
//    }
//}