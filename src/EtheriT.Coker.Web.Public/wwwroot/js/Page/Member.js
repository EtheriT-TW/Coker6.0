function PageReady() {

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
    Coker.User.GetUser(data.refreshToken).done(function (result) {
        //console.log(result.data)
        if (result.success) {
            co.Form.insertData(result.data, "#UserDataForm");
            $("#ResetForm").data("Email", result.data.email);
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
        //console.log(data);
        co.User.UserEdit(data).done(function (result) {
            co.sweet.success("資料修改完成！", null, true);
        });
    });

    $(".btn_resetPassword").on("click", function () {
        resetModal.show();
    })
}