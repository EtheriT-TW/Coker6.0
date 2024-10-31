function PageReady() {
    Coker.Member = {
        GetOrderHistory: function () {
            return $.ajax({
                url: "/api/Order/GetHistoryOrder/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: {
                    Authorization: 'Bearer ' + localStorage.getItem("token")
                },
            });
        },
    }

    let addr = $("#TWzipcode .address").val()
    co.Zipcode.init("#TWzipcode");
    co.Zipcode.setData({
        el: $("#TWzipcode"),
        addr: addr
    });

    Coker.Token.CheckToken().done(function (resule) {
        if (!resule.isLogin) {
            co.sweet.warning("尚未登入", "請登入後再重新操作，將引導至首頁。", function () {
                location.href = "/";
            })
        } else Member(resule);
    });
}
function Member(data) {
    $("#ResetForm .reset_old_pass").removeClass("d-none");
    $("#ResetForm .reset_old_pass input").removeAttr("disabled");
    $("#ResetOldPassFeedBack").removeClass("d-none");
    $("#ResetModal .btn_resetforget").removeClass("d-none");

    SetMemberData();
    SetHistoryOrderData();
    SetFavoriteData();
    SetBrowsingHistoryData();

    $(".btn_logout").on("click", function () {
        co.User.Logout().done(function (result) {
            if (result.success) {
                co.sweet.success("登出成功");
                setTimeout(e => {
                    location.href = "/";
                }, 1000);
            }
        });
    });

    $(".btn_modifi").on("click", function () {
        if ($("#Name").val() == "") {
            co.sweet.error("輸入資料錯誤", "姓名不可為空", null, false);
        } else if ($("#Email").val() == "") {
            co.sweet.error("輸入資料錯誤", "電子郵件不可為空", null, false);
        } else {
            var data = co.Form.getJson($("#UserDataForm").attr("id"));
            data.address = `${data.county} ${data.district} ${data.address}`;
            data.telPhone = `${data.zone}-${data.telPhone}-${data.ext}`;
            co.User.UserEdit(data).done(function (result) {
                co.sweet.success("資料修改完成！", null, true);
            });
        }
    });

    $(".btn_resetPassword").on("click", function () {
        resetModal.show();
    })
}

function SetMemberData() {
    Coker.User.GetUser().done(function (result) {
        if (result.success) {
            result.data['zone'] = (result.data.telPhone).split('-')[0];
            result.data['telPhone'] = (result.data.telPhone).split('-')[1];
            result.data['ext'] = (result.data.telPhone).split('-')[2];

            co.Form.insertData(result.data, "#UserDataForm");

            co.Zipcode.setData({
                el: $("#TWzipcode"),
                addr: result.data.address
            });

            $("#ResetForm").data("Email", result.data.email);

            var now = new Date();
            var month = (now.getMonth() + 1).toString();
            if (month.length == 1) month = '0' + month;
            var day = now.getDate().toString();
            if (day.length == 1) day = '0' + day;
            var date_now = `${now.getFullYear()}-${month}-${day}`

            $("#Birthday").attr("max", date_now);

            $("#Birthday").on("keydown", function (e) {
                e.preventDefault();
            });

        } else {

        }
    });
}
function SetHistoryOrderData() {
    Coker.Member.GetOrderHistory().done(function (result) {
        if (result.success && result.orderData != null) {
            $.each(result.orderData, function (index, data) {
                var order_header = data.orderHeader;
                var order_details = data.orderDetails;

                var frame = $($("#Template_Order_List").html()).clone();
                frame.find(".number").text(("000000000" + order_header.id).substr(order_header.id.length));
                frame.find(".date").text(((order_header.creationTime).substr(0, 10).replaceAll("-", "/")));
                frame.find(".amount").text((order_header.total).toLocaleString());

                switch (order_header.state) {
                    case 1:
                        frame.find(".state").text("待確認");
                        break;
                    case 2:
                        frame.find(".state").text("已付款");
                        break;
                    case 3:
                        frame.find(".state").text("已出貨");
                        break;
                    case 4:
                        frame.find(".state").text("已取消");
                        break;
                    case 5:
                        frame.find(".state").text("付款失敗");
                        break;
                    case 6:
                        frame.find(".state").text("待付款");
                        break;
                    case 7:
                        frame.find(".state").text("已完成");
                        break;
                }

                $("#profile-tab-pane").append(frame);

                frame.find(".collapse").addClass(`collapse_${order_header.id}`);
                frame.find(".btn_collapse").attr("data-bs-target", `.collapse_${order_header.id}`);

                frame.find(".btn_collapse").on("click", function () {
                    if ($(this).hasClass("collapsed")) $(this).text("點擊查看訂單詳細");
                    else $(this).text("點擊關閉訂單詳細");
                })

                $.each(order_details, function (index, detail) {
                    var list_frame = $($("#Template_Order_Details_List").html()).clone();
                    if (detail != null) {
                        list_frame.find("a").attr("href", `/${OrgName}/Member/product/${detail.pId}`);
                        list_frame.find("a").attr("title", `連結至：${detail.title}`);
                        list_frame.find("img").attr("src", detail.imagePath);
                        list_frame.find("img").attr("alt", `${detail.title}的主要圖片`);
                        list_frame.find(".title").text(detail.title);
                        list_frame.find(".price").text((detail.price).toLocaleString());
                        list_frame.find(".quantity").text(detail.quantity);
                        list_frame.find(".subtotal").text(((parseInt(detail.price)) * (parseInt(detail.quantity))).toLocaleString());
                        frame.find(".list-group").append(list_frame);
                    }
                })

                frame.find(".collapse .header_subtotal").text((order_header.subtotal).toLocaleString());
                frame.find(".collapse .header_freight").text((order_header.freight).toLocaleString());
                frame.find(".collapse .header_total").text((order_header.total).toLocaleString());

            })
        } else {
            $("#profile-tab-pane .nodata").removeClass("d-none");
        }
    });
}
function SetFavoriteData() {
    var result = {};
    //Product.GetAll.History().done(function (result) {
    //console.log(result)
    if (result.length > 0) {
        $.each(result, function (index, data) {
            var frame = $($("#Template_Favorite_List").html()).clone();
            frame.find("*").each(function () {
                var $self = $(this);
                if (typeof ($self.data("key")) != "undefined") {
                    var key = $self.data("key");
                    switch (key) {
                        case "link":
                            $self.attr("href", `/${OrgName}/Member${data['link']}`);
                            $self.attr("title", `連結至：${data['title']}`);
                            break;
                        case "image":
                            $self.attr("src", data['image']);
                            $self.attr("alt", `${data['title']}的主要照片`);
                            break;
                        default:
                            $self.text(data[key]);
                            break;
                    }
                }
            });
            frame.find(".shareBlock").hover(function () {
                $(this).addClass("show");
            }, function () {
                $(this).removeClass("show");
            })

            $("#history-tab-pane").append(frame);
        })

        $('.shareBlock').cShare({
            description: 'jQuery plugin - C Share buttons',
            showButtons: ['fb', 'line', 'plurk', 'twitter', 'email']
        });
    } else {
        $("#favorite-tab-pane .nodata").removeClass("d-none");
    }
    //});
}
function SetBrowsingHistoryData() {
    Product.GetAll.History().done(function (result) {
        //console.log(result)
        if (result.length > 0) {
            $.each(result, function (index, data) {
                var frame = $($("#Template_Prod_List").html()).clone();
                frame.find("*").each(function () {
                    var $self = $(this);
                    if (typeof ($self.data("key")) != "undefined") {
                        var key = $self.data("key");
                        switch (key) {
                            case "link":
                                $self.attr("href", `/${OrgName}/Member${data['link']}`);
                                $self.attr("title", `連結至：${data['title']}`);
                                break;
                            case "image":
                                $self.attr("src", data['image']);
                                $self.attr("alt", `${data['title']}的主要照片`);
                                break;
                            default:
                                $self.text(data[key]);
                                break;
                        }
                    }
                });
                frame.find(".shareBlock").hover(function () {
                    $(this).addClass("show");
                }, function () {
                    $(this).removeClass("show");
                })
                frame.find(".btn_favorite").on("click", function () {
                    $self = $(this).find("i");
                    if ($self.hasClass("fa-regular")) {
                        $self.addClass("fa-solid")
                        $self.removeClass("fa-regular")
                        $self.attr("title", "移除收藏")
                    } else {
                        $self.addClass("fa-regular")
                        $self.removeClass("fa-solid")
                        $self.attr("title", "加入收藏")
                    }
                })

                $("#history-tab-pane").append(frame);
            })

            $('.shareBlock').cShare({
                description: 'jQuery plugin - C Share buttons',
                showButtons: ['fb', 'line', 'plurk', 'twitter', 'email']
            });
        } else {
            $("#history-tab-pane .nodata").removeClass("d-none");
        }
    });
}