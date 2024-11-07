
var resetEmailModal, ResetEmailModal, $InputResetEmailVCode, $ResetEmailImgCaptcha, ResetEmailForms
var old_email
var TabNow = "info", date_now = "";

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
        CanceOrder: function (ohid) {
            return $.ajax({
                url: "/api/Order/CanceOrder/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                data: { ohid: ohid },
            });
        }
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

    var now = new Date();
    var month = (now.getMonth() + 1).toString();
    if (month.length == 1) month = '0' + month;
    var day = now.getDate().toString();
    if (day.length == 1) day = '0' + day;
    date_now = `${now.getFullYear()}-${month}-${day}`

    resetEmailModal = $("#ResetEmailModal").length > 0 ? new bootstrap.Modal($("#ResetEmailModal")) : null;
    $InputResetEmailVCode = $("#InputNewMailVCode");
    $ResetEmailImgCaptcha = $('#NewMailImgCaptcha');
    ResetEmailForms = $('#ResetEmailForm');

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
        var data = co.Form.getJson($("#UserDataForm").attr("id"));
        if (data.name == "") {
            co.sweet.error("輸入資料錯誤", "姓名不可為空", null, false);
        } else if ($("#Email").val() == "") {
            co.sweet.error("輸入資料錯誤", "電子郵件不可為空", null, false);
        } else if (data.zone == "" ^ data.telPhone == "") {
            co.sweet.error("輸入資料錯誤", "如要填入電話資訊，請確實填寫區碼與聯絡電話", null, false);
        } else if (((data.county == "") ^ (data.address == ""))) {
            co.sweet.error("輸入資料錯誤", "如要填入地址資訊，請確實填寫縣市、鄉鎮與地址", null, false);
        }
        else {
            var datacheck = true;
            if (data.birthday == "") data.birthday = null;
            data.address = `${data.county} ${data.district} ${data.address}`;
            if (data.cellPhone != "" && (!$.isNumeric(data.cellPhone) || data.cellPhone.length != 10)) {
                co.sweet.error("輸入資料錯誤", "手機格式不正確，請重新輸入", null, false);
                datacheck = false;
            }
            if (data.telPhone != "") {
                if ($.isNumeric(data.zone) && $.isNumeric(data.telPhone) && ((data.ext != "" && $.isNumeric(data.ext)) || data.ext == "")) {
                    data.telPhone = `${data.zone}-${data.telPhone}-${data.ext}`;
                } else {
                    co.sweet.error("輸入資料錯誤", "電話格式不正確，請重新輸入", null, false);
                    datacheck = false;
                }
            }
            //console.log(data)
            if (datacheck) {
                co.User.UserEdit(data).done(function (result) {
                    co.sweet.success("資料修改完成！", null, true);
                });
            }
        }
    });

    $(".btn_resetPassword").on("click", function () {
        resetModal.show();
    })

    $(".btn_resetEmail").on("click", function () {
        resetEmailModal.show()
    });

    $("#ResetEmailModal .btn_resetforget").on("click", function () {
        $("#ResetModal .btn_resetforget").click();
    })

    $('#ResetEmailModal .btn_refresh').on('click', function (event) {
        event.preventDefault();
        NewCaptcha($ResetEmailImgCaptcha, $InputResetEmailVCode);
    });

    ResetEmailModal = document.getElementById('ResetEmailModal')
    if (ResetEmailModal != null) {
        ResetEmailModal.addEventListener('show.bs.modal', function (event) {
            NewCaptcha($ResetEmailImgCaptcha, $InputResetEmailVCode);
        })
        ResetEmailModal.addEventListener('hidden.bs.modal', function (event) {
            FormClear(ResetEmailForms, $InputResetEmailVCode)
        })
    }
    $("#ResetEmailForm input").on("keypress", function (event) {
        if (event.which == 13) {
            event.preventDefault();
            $("#ResetEmailModal .btn_resetmail").click();
        }
    });
    $(".btn_resetmail").on("click", function () {
        console.log("btn_resetmail")
        if (SiteFormCheck(ResetEmailForms, $InputResetEmailVCode)) {
            CaptchaVerify($ResetEmailImgCaptcha, $InputResetEmailVCode, function () {
                ResetmailAction(data);
            })
        } else {
            $InputResetEmailVCode.addClass('is-invalid');
            $InputResetEmailVCode.siblings("div").addClass("me-4 pe-2");
            NewCaptcha($ResetEmailImgCaptcha, $InputResetEmailVCode)
            $InputResetEmailVCode.val("");
            Coker.sweet.error("錯誤", "請確實填寫資料", null, true);
        }
    });
}
function SetMemberData() {
    Coker.User.GetUser().done(function (result) {
        if (result.success) {
            if (result.data.telPhone != null && result.data.telPhone != "") {
                result.data['zone'] = (result.data.telPhone).split('-')[0];
                result.data['telPhone'] = (result.data.telPhone).split('-')[1];
                if ((result.data.telPhone).split('-').length == 2) result.data['ext'] = (result.data.telPhone).split('-')[2];
            }

            co.Form.insertData(result.data, "#UserDataForm");

            old_email = result.data.email;

            co.Zipcode.setData({
                el: $("#TWzipcode"),
                addr: result.data.address
            });

            $("#ResetForm").data("Email", result.data.email);

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
        //console.log(result)
        if (result.success && result.orderData != null && result.orderData.length > 0) {
            $.each(result.orderData, function (index, data) {
                var order_header = data.orderHeader;
                var order_details = data.orderDetails;
                var frame = $($("#Template_Order_List").html()).clone();
                frame.find(".number").text(("000000000" + order_header.id).substr(order_header.id.length));
                frame.find(".date").text(((order_header.creationTime).substr(0, 10).replaceAll("-", "/")));
                frame.find(".amount").text((order_header.total).toLocaleString());
                if ((order_header.creationTime.split(' ')[0] == date_now) && order_header.state == 1) {
                    frame.find(".state").prepend(`${order_header.stateStr}<button data-ohid="${order_header.id}" class="btn_canceOrder bg-transparent border-0 text-decoration-underline text-primary" title="取消此筆訂單">取消訂單</button>`)
                    frame.find(".state .btn_canceOrder").on("click", function () {
                        var $this = $(this);
                        Coker.sweet.confirm("確定取消訂單？", "", "是", "否", function () {
                            Coker.sweet.loading();
                            Coker.Member.CanceOrder($this.data("ohid")).done(function (result) {
                                if (result.success) {
                                    $this.parent(".state").addClass("text-danger fw-bold");
                                    $this.parent(".state").text("已取消");
                                    Coker.sweet.success("已取消訂單", null, false);
                                } else {
                                    console.log(result.message);
                                }
                            })
                        })
                    });
                }
                else {
                    frame.find(".state").text(order_header.stateStr);
                    if (order_header.state == 4 || order_header.state == 5) {
                        frame.find(".state").addClass("text-danger fw-bold");
                    }
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

    $("#favorite-tab-pane").children().not(".nodata").not("template").remove();
    Coker.Favorites.GetDisplay().done(function (result) {
        //console.log(result)
        if (result != null && result.length > 0) {
            $.each(result, function (index, data) {
                var frame = $($("#Template_Favorite_List").html()).clone();
                frame.data("Pid", data.pId);
                frame.find(".btn_favorite").data("Fid", data.fId)
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
                            case "price":
                                var prices = data['price'];
                                if (prices.length > 1) $self.text(`${prices[0].toLocaleString()}~$${[prices[prices.length - 1].toLocaleString()]}`)
                                else $self.text(`${prices[0].toLocaleString()}`)
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
                    Coker.Favorites.Delete($(this).data("Fid")).done(function (result) {
                        if (result.success) {
                            SetBrowsingHistoryData();
                            Coker.sweet.success("已將商品從收藏中移除", null, true);
                            frame.remove();
                        }
                    });
                })

                $("#favorite-tab-pane").append(frame);
                if (!$("#favorite-tab-pane .nodata").hasClass("d-none")) $("#favorite-tab-pane .nodata").addClass("d-none");
            })

            $('.shareBlock').cShare({
                description: 'jQuery plugin - C Share buttons',
                showButtons: ['fb', 'line', 'plurk', 'twitter', 'email']
            });
        } else {
            $("#favorite-tab-pane .nodata").removeClass("d-none");
        }
    });
}
function SetBrowsingHistoryData() {
    $("#history-tab-pane").children().not(".nodata").not("template").remove();
    Product.GetAll.History().done(function (result) {
        //console.log(result)
        if (result != null && result.length > 0) {
            $.each(result, function (index, data) {
                var frame = $($("#Template_Prod_List").html()).clone();
                frame.data("Pid", data.id);
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
                            case "price":
                                var prices = data['price'];
                                if (prices.length > 1) $self.text(`${prices[0].toLocaleString()}~$${[prices[prices.length - 1].toLocaleString()]}`)
                                else $self.text(`${prices[0].toLocaleString()}`)
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

                var $btn_favorites = frame.find(".btn_favorite");

                Coker.Favorites.Check(frame.data("Pid")).done(function (check) {
                    if (check.success) {
                        $btn_favorites.data("Fid", check.message);
                        $btn_favorites.find("i").addClass("fa-solid")
                        $btn_favorites.find("i").removeClass("fa-regular")
                        $btn_favorites.attr("title", "移除收藏")
                    }
                });
                $btn_favorites.on("click", function () {
                    $self = $(this).find("i");
                    if ($self.hasClass("fa-regular")) {
                        Coker.Favorites.Add(frame.data("Pid")).done(function (result) {
                            if (result.success) {
                                $btn_favorites.data("Fid", result.message);
                                $self.addClass("fa-solid")
                                $self.removeClass("fa-regular")
                                $btn_favorites.attr("title", "移除收藏")
                                SetFavoriteData();
                                Coker.sweet.success("成功將商品加入收藏", null, true);
                            } else {
                                console.log(result.message)
                            }
                        });
                    } else {
                        if (typeof ($btn_favorites.data("Fid")) != "undefined" && typeof ($btn_favorites.data("Fid")) != "") {
                            Coker.Favorites.Delete($btn_favorites.data("Fid")).done(function (result) {
                                if (result.success) {
                                    $btn_favorites.data("Fid", "");
                                    $self.addClass("fa-regular")
                                    $self.removeClass("fa-solid")
                                    $btn_favorites.attr("title", "加入收藏")
                                    SetFavoriteData();
                                    Coker.sweet.success("已將商品從收藏中移除", null, true);
                                } else {
                                    console.log(result.message)
                                }
                            });
                            console.log("非收藏商品")
                        }
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
function ResetmailAction(data) {
    var input_data = co.Form.getJson($("#ResetEmailForm").attr("id"));
    if (input_data.email == old_email) {
        Coker.sweet.info("新的電子郵件與舊的相同，不進行變更。", null)
        resetEmailModal.hide()
    } else {
        Coker.sweet.loading();
        Coker.User.EmailChange(input_data).done(function (result) {
            if (result.success) {
                Coker.sweet.success("電子郵件修改成功", function () {
                    location.reload()
                }, false);
            } else {
                NewCaptcha($ResetEmailImgCaptcha, $InputResetEmailVCode);
                switch (result.error) {
                    case "密碼錯誤":
                        Coker.sweet.confirm("密碼輸入錯誤", result.message, "忘記密碼?", "確定", function () {
                            $("#ResetModal .btn_resetforget").click();
                        })
                        break;
                    case "錯誤三次":
                        Coker.sweet.error(result.error, result.message, null, false);
                        break;
                    default:
                        Coker.sweet.error(result.error, result.message, null, false);
                        break;
                }
            }
        });
    }
}