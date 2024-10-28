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

    Product.GetAll.History().done(function (result) {
        var data = JSON.stringify(result);
        console.log(data)
        $("#history-tab-pane").append(`<div>${data}</div>`)
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
        if ($("#Name").val() == "") {
            co.sweet.error("輸入資料錯誤", "姓名不可為空", null, false);
        } else if ($("#Email").val() == "") {
            co.sweet.error("輸入資料錯誤", "電子郵件不可為空", null, false);
        } else {
            var data = co.Form.getJson($("#UserDataForm").attr("id"));
            data.address = `${data.county} ${data.district} ${data.address}`;
            //console.log(data);
            co.User.UserEdit(data).done(function (result) {
                co.sweet.success("資料修改完成！", null, true);
            });
        }
    });

    $(".btn_resetPassword").on("click", function () {
        resetModal.show();
    })
}