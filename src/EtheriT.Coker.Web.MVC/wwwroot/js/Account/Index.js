var Login = function (para) {
    return $.ajax({
        url: "Account/Login",
        type: "POST",
        contentType: 'application/x-www-form-urlencoded; charset=utf-8',
        data: para,
        dataType: "json"
    });
}
var PageReady = function () {
    $("#loginBtn").on("click", function (e) {
        e.preventDefault();
        Login({
            UserName: $("#username").val(),
            Password: $("#password").val()
        }).done(function (result) {
            if (result.success) location.href = "Dashboard/index";
            else alert(result.error);
            console.log(result);
        });
    });
}