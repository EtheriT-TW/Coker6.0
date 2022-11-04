function PageReady() {

    var $imgCaptcha = $('#imgCaptcha')

    $('.btn_refresh').on('click', function () {
        console.log("btn_refresh Click")
        $imgCaptcha.attr('src', '/Page/Conta/Captcha?id=12345&time=' + new Date().getTime())
        console.log($imgCaptcha.attr('src'))
    })

    $('#btnValidate').on('click', function () {
        var code = $('#InputCaptcha').val()
        console.log("Code = " + code)
        $.ajax('/Page/Validate?id=12345&code=' + code, {
            dataType: "JSON",
            success: function (result) {
                if (result.success) {
                    console.log('驗證碼輸入正確')
                } else {
                    console.log('驗證碼輸入錯誤')
                }
            }
        })
    })

    const forms = $('#ContactForm');

    (() => {
        Array.from(forms).forEach(form => {
            form.addEventListener('submit', event => {
                if (!form.checkValidity()) {
                    event.preventDefault()
                    event.stopPropagation()
                } else {
                    event.preventDefault();
                    Coker.sweet.success("成功送出表單！", null, true);
                    setTimeout(function () {
                        $("#ContactForm").submit();
                    }, 1500);
                }
                form.classList.add('was-validated')
            }, false)
        })
    })()


    document.addEventListener("keyup", function () {
        var target = event.target

        if (target.nodeName == "INPUT") {
            if (target.value.length == target.maxLength) {
                var elements = $(target).parents("form").first().find("input");
                for (let i = 0; i < elements.length; i++) {
                    if (elements[i] == target) {
                        if (elements[i + 1]) {
                            elements[i + 1].focus();
                        }
                        return;
                    }
                }
            }
        }
    });
}