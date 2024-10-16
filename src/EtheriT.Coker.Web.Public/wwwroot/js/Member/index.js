function PageReady() {
    Coker.Token.CheckToken().done(function (resule) {
        if (!resule.isLogin) {
            co.sweet.warning("尚未登入", "請登入後再重新操作，將引導至首頁。", function () {
                location.href = "/";
            })
        } else Member();
    });
}
function Member() {
    $(".logout").on("click", function () {
        co.User.Logout().done(function (result) {
            if (result.success) {
                co.sweet.success("登出成功");
                setTimeout(e => {
                    location.href = "/";
                }, 1000);
            }
        });
    });
}