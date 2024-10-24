function PageReady() {
    Coker.User = {
        GetUser: function (refreshToken) {
            return $.ajax({
                url: "/api/User/GetUserData/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                data: { refreshToken: refreshToken },
            });
        },
        UserEdit: function (data) {
            return $.ajax({
                url: "/api/User/FrontUserEdit",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: {
                    Authorization: 'Bearer ' + localStorage.getItem("token")
                },
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        Logout: function () {
            return $.ajax({
                url: "/api/User/Logout",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: {
                    Authorization: 'Bearer ' + localStorage.getItem("token")
                },
                dataType: "json"
            });
        },
    };

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
    Coker.User.GetUser(data.refreshToken).done(function (result) {
        if (result.success) {
            co.Form.insertData(result.data, "#UserDataForm");

            co.Zipcode.setData({
                el: $("#TWzipcode"),
                addr: result.data.address
            });
        } else {

        }
    });

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
        data.address = `${data.county} ${data.district} ${data.address}`;
        data.WebsiteId = SiteId;
        co.User.UserEdit(data).done(function (result) {
        });
    });
}