document.addEventListener("DOMContentLoaded", () => {
    $.fn.extend({
        contactInit: function () {
            $(this).each(function () {
                $(this).setContact();
            });
        },
        setContact: function () {
            const form = this.get(0);
            const $forms = this;
            const $captcha_input = $forms.find(`[name="captcha"]`);
            const $imgCaptcha = $forms.find('.img-fluid').last();

            $('#ContactForm .btn_refresh').off("click").on('click', () => {
                NewCaptcha($imgCaptcha, $captcha_input, "ContactUs");
            });
            $('#ContactForm .btn_refresh').trigger("click");
            $forms.getFormJson();
            form.addEventListener('submit', event => {
                event.preventDefault();
                event.stopPropagation();
                form.classList.add('was-validated')
                if (!form.checkValidity()) {
                    NewCaptcha($imgCaptcha, $captcha_input, "ContactUs");
                    Coker.sweet.error("錯誤", "須確實填寫表單資料才可送出", null, true);
                } else {
                    event.preventDefault();
                    const sender = { Email: "", Name: "" };
                    const senderFiled = $forms.find(`[name="sender"]`);
                    if (senderFiled.get(0).tagName == "SELECT") {
                        const s = senderFiled.find("option:selected");
                        sender.Email = s.val();
                        sender.Name = s.text();
                    } else {
                        sender.Email = senderFiled.val();
                        sender.Name = senderFiled.data("title");
                    }

                    if (sender.Email == "") {
                        co.sweet.error("資料錯誤", "請選擇寄件人");
                        return;
                    }
                    $.ajax({
                        url: "/api/Contact/submit",
                        type: "POST",
                        contentType: 'application/json; charset=utf-8',
                        data: JSON.stringify({
                            routerName: PageKey,
                            sender: sender,
                            forms: $forms.getFormJson()
                        }),
                        dataType: "json",
                        beforeSend: function (xhr) {
                            xhr.setRequestHeader("requestverificationtoken",
                                $('input:hidden[name="AntiforgeryFieldname"]').val());
                        }
                    }).done(function (result) {
                        if (result.success) {
                            Coker.sweet.success("成功送出表單！", null, true);
                            $forms.removeClass('was-validated');
                            $forms.get(0).reset();
                        } else Coker.sweet.error("發送失敗", result.error, null, true);
                        NewCaptcha($imgCaptcha, $captcha_input, "ContactUs");
                    });
                }
            }, false)

            document.addEventListener("keyup", function (event) {
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
    });
});
function setContact() {
    $('.ContactForm').contactInit();
}