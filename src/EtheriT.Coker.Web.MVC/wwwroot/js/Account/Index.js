
var PageReady = function () {
    var rule = document.getElementById("rule");
    var newpassword = document.getElementById("newpassword");
    var agnewpassword = document.getElementById("agnewpassword");
    var lowercase = document.getElementById("lowercase");
    var uppercase = document.getElementById("uppercase");
    var number = document.getElementById("number");
    var symbol = document.getElementById("symbol");
    var length = document.getElementById("length");

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

    $("#verification-btn").on("click", function () {
        
    });
    $("#subnewpsw").on("click", function () {
        if (lowercase.classList.contains("valid") && uppercase.classList.contains("valid") &&
            number.classList.contains("valid") && symbol.classList.contains("valid") &&
            length.classList.contains("valid") && agnewpassword.value === newpassword.value) {
            alert("成功");
        } else {
            alert("失敗");        }
    });

    newpassword.onkeyup = function () {
        //小寫
        var lower = /[a-z]/g;
        if (newpassword.value.match(lower)) {
            lowercase.classList.remove("invalid");
            lowercase.classList.add("valid");
        } else {
            lowercase.classList.remove("valid");
            lowercase.classList.add("invalid");
        }
        //大寫
        var upper = /[A-Z]/g;
        if (newpassword.value.match(upper)) {
            uppercase.classList.remove("invalid");
            uppercase.classList.add("valid");
        } else {
            uppercase.classList.remove("valid");
            uppercase.classList.add("invalid");
        }
        //數字
        var num = /[0-9]/g;
        if (newpassword.value.match(num)) {
            number.classList.remove("invalid");
            number.classList.add("valid");
        } else {
            number.classList.remove("valid");
            number.classList.add("invalid");
        }
        //符號
        var sym = /[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/g;
        if (newpassword.value.match(sym)) {
            symbol.classList.remove("invalid");
            symbol.classList.add("valid");
        } else {
            symbol.classList.remove("valid");
            symbol.classList.add("invalid");
        }
        //大於8小於32
        if (newpassword.value.length >= 8 && newpassword.value.length < 32) {
            length.classList.remove("invalid");
            length.classList.add("valid");
        }else {
            length.classList.remove("valid");
            length.classList.add("invalid");
        }
        
    }

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
var togglePassword2 = function () {
    var passwordInput = document.getElementById('newpassword');
    var toggleIcon = document.getElementById('togglePassword2');
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
var togglePassword3 = function () {
    var passwordInput = document.getElementById('agnewpassword');
    var toggleIcon = document.getElementById('togglePassword3');
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