function PageReady() {

    $.fn.extend({
        loadShipmentsCar: function () {
            var $self = $(this);
            /*
            var list = [
                $.getStoreInfo(),
                $.getShoppingCar()
            ];
             */
            var data = null;

            $.when.apply(null, list).pipe(function (result, result2) {
                var _dfr = $.Deferred();
                data = result2[0].OrderData;
                _dfr.promise();
                $self.data({
                    "ShoppingCar": data,
                    "Store": result[0].Store
                });
                switch (result[0].Store.RFQType) {
                    case "N":
                        $(".printingButton,.inquiryButton").remove();
                        break;
                    case "P":
                        $(".printingButton").removeClass("hide");
                        $(".inquiryButton").remove();
                        break;
                    case "S":
                        $(".inquiryButton").removeClass("hide");
                        $(".printingButton").remove();
                        break;
                }
                if (data.ShoppingCar.length == 0) {
                    $("#ShoppingCarIsEmpty").removeClass("hide");
                    $self.addClass("hide");
                    $("#moduleCont").removeClass("opacity");
                    _dfr.reject();
                } else {
                    _dfr.resolve();
                }
                return _dfr;
            }).pipe(function () {
                if (data.ShoppingCar.length == 1) {
                    var $first = $("#ShipmentsCar,#stepprogressbar>li:first-child")
                    $("#ShipmentsCar").find(".next").trigger("click");
                    if ($first.data("translation") == "step0") $first.remove();
                    $self.find("fieldset.active").updateShipmentsCar(data.ShoppingCar[0].theLogisticsID);
                } else {
                    var $ShipmentsCar = $("#ShipmentsCar>.mainPage>.shippingbutton");
                    var _html = $ShipmentsCar.data("html") || $ShipmentsCar.html();
                    var $now = null;
                    $ShipmentsCar.data("html", _html).empty();
                    $("#stepprogressbar").addClass("li5");
                    $(data.ShoppingCar).each(function () {
                        var car = this;
                        var $btn = $(_html).on("click", function (e) {
                            e.preventDefault();
                            location.hash = car.theLogisticsID;
                            $("#ShipmentsCar").find(".next").trigger("click");
                            $self.find("fieldset.active").updateShipmentsCar(car.theLogisticsID);
                        }).appendTo($ShipmentsCar);
                        $btn.find(".shippingMethodBtn").text(car.Title || getWord("NormalProd"));
                        if (location.hash == "#" + car.theLogisticsID) $now = $btn;
                    });
                    $("#ShipmentsCar .previous").on("click", function (e) {
                        e.preventDefault();
                        window.location.href = firstProdList;
                    });
                    if ($now != null) $now.trigger("click");
                }
                $("#moduleCont").removeClass("opacity");
            });
        }, ShoppingCar: function () {
            var $self = $(this);
            var current_fs, next_fs, previous_fs; //fieldsets
            var left, opacity, scale; //fieldset properties which we will animate
            var animating; //flag to prevent quick multi-click glitches
            $self.find("fieldset").data("root", $self);
            /*
             * if (!memLogin) {
                $self.find("#memberLogin").addClass("hide");
            } else {
                $self.find(".orderGetBonus").addClass("hide");
                $self.find("#memberLogin>.memberLogin").on("click", function (e) {
                    e.preventDefault();
                    document.memAdd.submit();
                });
            }
             */
            $self.find(".btn_next").data("lock", false).off().on("click", function (e) {
                e.preventDefault();
                if ($(this).data("lock")) return false;
                current_fs = $(this).parents("fieldset");
                next_fs = current_fs.next();
                var index = $self.find("fieldset").index(next_fs);
                //activate next step on progressbar using the index of next_fs
                $("#stepprogressbar>li").eq(index).addClass("active now");
                $("#stepprogressbar>li").eq(index - 1).removeClass("now");

                //show the next fieldset
                next_fs.addClass("active");
                console.log(next_fs);
                //hide the current fieldset with style
                current_fs.removeClass("active");
                console.log(current_fs);
                $("html,body").scrollTop($("#stepform").offset().top);
                switch ($(current_fs).attr("class")) {
                    case "step0":
                        break;
                    case "step1":
                        $(next_fs).loadPayment(current_fs);
                        break;
                }
            });

            $self.find(".btn_prev").off().on("click", function (e) {
                e.preventDefault();
                current_fs = $(this).parents("fieldset");
                previous_fs = current_fs.prev();
                var index = $self.find("fieldset").index(current_fs);
                if (index == 0) window.history.back();
                else {
                    //de-activate current step on progressbar
                    $("#stepprogressbar li").eq(index).removeClass("active now");
                    $("#stepprogressbar li").eq(index - 1).addClass("now");

                    //show the previous fieldset
                    previous_fs.addClass("active");
                    //hide the current fieldset with style
                    current_fs.removeClass("active");
                    $("html,body").scrollTop($("#stepform").offset().top);
                    switch ($(previous_fs).attr("class")) {
                        case "step1":
                            $(previous_fs).updateShipmentsCar($(previous_fs).data("ShoppingCar").theLogisticsID);
                            break;
                    }
                }
            });
            $self.find(".printingOrder").on("click", function (event) {
                event.preventDefault();
                var pageFloder = languageSet[$("html").attr("lang") || "zh-Hant-TW"];
                var $head = $("<div>").append($("#head").html());
                var $body = $("<div>").append($(this).parents("fieldset").html());
                var $title = $("<div>").addClass("titleBox");
                $title.append("<h2 id='logoTitle'>" + $("#logo_link").text() + "</h2>");
                $title.append("<h3 id='InquiryForm'>" + $("body").data("translation").InquiryForm + "</h3>")
                $head.find("button,#languageBar,#bannerCont,#topmenu,#belowmenu,#aumenu").remove();
                $head.find("#menuType").append($title);
                $body.find(".shop_tbh,.shoppingBox,.selectbutton").remove();
                $body.find(".cockerTitleFont").text($("body").data("translation").InquiryForm);
                $body.find("form").prepend($("#sub_cont").html());
                var html =
                    "<!DOCTYPE HTML>\
                        <html lang='" + $("html").attr("lang") + "'>\
                            <head>\
                            "+ $("head").html() + "\
                            <style>\
                                #head,#head .navbar-inner{height:auto !important;position: relative;}\
                                #menuType{overflow: hidden;font-size: 0;margin: auto;text-align: left;}\
                                #menuType>*{float:none; display:inline-block}\
                                #menuType>.titleBox{width: 65%; padding-left: 5%;}\
                                #menuType>#logo{width: 30%;}\
                                #hd_logo {width:100%;}\
                                #InquiryForm{top: 3rem;}\
                                .CartProdImg img{max-width: 3.5rem;}\
                                .buyCartTable .error .limit{display:inline-block;font-size:0.8rem;font-weight:bold;color:#d60000;}\
                                .CartProdImg>.span4{max-width: 3.5rem;margin-right: 1rem;float:left;}\
                                .CartProdImg>.check_buy_link{position: absolute;top: 1rem;left: 5rem;}\
                                .priceCol,.delCel,.buyCartTable .limit,.localSum,.sumBox{display:none}\
                                input[type='text']{border:none;box-shadow: none;}\
                                .buyCartTable{width:100%;};\
                                .buyCartTable td{width:10rem;}\
                                .buy_prod_title{width:50%;position: relative;}\
                                .buy_prod_title.nn{width:80%;}\
                                .buy_prod_title.n{width:70%;}\
                                .colorTitle,.sizeTitle{width:20%;}\
                            </style >\
                            </head >\
                            <body>\
                                <div id='head'><!--start #head-->"+ $head.html() + "<!--end #head--></div>\
                                <div class='container'>" + $body.html() + "</div>\
                            </body >\
                        </html>";

                var $ifrm = $("<iframe id='filelink' style='display:none'>");
                $ifrm.appendTo("body");
                function postToIframe(target) {
                    $('body').append('<form action="' + '/' + pageFloder + '/htmlToPDF/htmlToPDF.aspx' + '" method="post" target="' + target + '" id="postToIframe"></form>');
                    $('#postToIframe').append('<input type="hidden" name="htmlCode" value="' + encodeURIComponent(html) + '" />');
                    $('#postToIframe').submit().remove();
                }
                postToIframe("filelink");
            });

            $self.find(".submit").click(function () {
                return false;
            })
            /*$self.loadShipmentsCar();*/
        }
    });

    (function () {
        $("#stepform").ShoppingCar();
    })();
}