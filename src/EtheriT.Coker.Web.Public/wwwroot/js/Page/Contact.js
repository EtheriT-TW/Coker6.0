var id;
var $captcha_input;
var $imgCaptcha;
var form_correct = false;

function PageReady() {

    $captcha_input = $("#InputCaptcha");
    $imgCaptcha = $('#imgCaptcha');
    NewCaptcha();
    const forms = $('#ContactForm');


    $('.btn_refresh').on('click', NewCaptcha);


    (() => {
        Array.from(forms).forEach(form => {
            form.addEventListener('submit', event => {
                if (!form.checkValidity()) {
                    event.preventDefault()
                    event.stopPropagation()
                    form_correct = false;
                    CaptchaVerify();
                    NewCaptcha();
                } else {
                    event.preventDefault();
                    form_correct = true;
                    CaptchaVerify();
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

function CaptchaVerify() {
    var code = $captcha_input.val();
    if (!code == "") {
        $.ajax('/api/Captcha/Validate?id=' + id + '&code=' + code, {
            dataType: "JSON",
            success: function (result) {
                if (result.success) {
                    $captcha_input.removeClass('is-invalid');
                    if (form_correct) {
                        Coker.sweet.success("成功送出表單！", null, true);
                        verify_correct = false;
                        setTimeout(function () {
                            $("#ContactForm").submit();
                        }, 1500);
                    }
                } else {
                    $captcha_input.addClass('is-invalid');
                    $captcha_input.val("");
                }
            }
        })
    }
    return false;
}

function NewCaptcha() {
    id = Math.floor(Math.random() * 10000);
    $imgCaptcha.attr('src', '/api/Captcha/index?id=' + id);
    $captcha_input.val("");
}