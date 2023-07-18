
var PageReady = function () {
    if (!!$.cookie("token")) {
        co.User.Check().done(function (result) {
            if (result.success)
                location.href = $.cookie("lastViewPage") || co.Data.DefauleUrl;
        });
    }

    $("#loginBtn").on("click", function (e) {
        e.preventDefault();
        co.User.Login({
            UserName: $("#username").val(),
            Password: $("#password").val()
        }).done(function (result) {
            if (result.success) {
                location.href = co.Data.DefauleUrl;
            } else alert(result.error);
        });
    });
}
var togglePassword = function () {
    var passwordInput = document.getElementById('password');
    var toggleIcon = document.getElementById('togglePassword');

    if (passwordInput.type === 'password') {
        passwordInput.type = 'text';
        toggleIcon.classList.remove('fa-eye-slash');
        toggleIcon.classList.add('fa-eye');
    } else {
        passwordInput.type = 'password';
        toggleIcon.classList.remove('fa-eye');
        toggleIcon.classList.add('fa-eye-slash');
    }
};
