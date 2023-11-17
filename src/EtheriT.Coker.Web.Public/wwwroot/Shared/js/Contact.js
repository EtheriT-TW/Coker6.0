var id;
var $captcha_input;
var $imgCaptcha;
var form_correct = false;

function setContact() {
    console.log("in");
    $captcha_input = $("#InputCaptcha");
    $imgCaptcha = $('#imgCaptcha');
    NewCaptcha($imgCaptcha, $captcha_input);
    const forms = $('#ContactForm');

    $('.btn_refresh').on('click', NewCaptcha($imgCaptcha, $captcha_input));

    (() => {
        Array.from(forms).forEach(form => {
            form.addEventListener('submit', event => {
                if (!form.checkValidity()) {
                    event.preventDefault()
                    event.stopPropagation()
                    NewCaptcha($imgCaptcha, $captcha_input);
                    Coker.sweet.error("錯誤", "須確實填寫表單資料才可送出", null, true);
                } else {
                    event.preventDefault();
                    form_correct = true;
                    CaptchaVerify($imgCaptcha, $captcha_input, SuccessAction)
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

function SuccessAction() {
    Coker.sweet.success("成功送出表單！", null, true);
    verify_correct = false;
    setTimeout(function () {
        $("#ContactForm").submit();
    }, 1500);
}