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
            const $captcha_input = $forms.find(`[name="captcha"]`);//獲取驗證碼輸入框
            const $imgCaptcha = $forms.find('.img-fluid').last();//獲取有.img-fluid的圖片
            //重整驗證碼，找尋id是ContactForm的form去執行btn_refresh的按鈕事件
            $('#ContactForm .btn_refresh').off("click").on('click', () => {

                NewCaptcha($imgCaptcha, $captcha_input, "ContactUs");
                
            });
            //點擊驗證碼刷新 (用trigger在網頁重整時直接觸發click事件)
            $('#ContactForm .btn_refresh').trigger("click");
            $forms.getFormJson();//初始化表單的Json方法

            //表單提交
            form.addEventListener('submit', event => {
                event.preventDefault(); //阻止默認提交
                event.stopPropagation();
                form.classList.add('was-validated') //添加驗證樣式
                if (!form.checkValidity()) { //檢查表單
                    NewCaptcha($imgCaptcha, $captcha_input, "ContactUs"); //刷新驗證碼
                    Coker.sweet.error(local.Error, local.FormSubmitMessage, null, true); //顯示錯誤訊息
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
                        co.sweet.error(local.InformationError, local.NoSelectSender);
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
                            Coker.sweet.success(local.SentSuccessfully, null, true);
                            $forms.removeClass('was-validated');
                            $forms.get(0).reset();
                            window.location.hash = 'submitted';
                            setTimeout(() => {
                                history.replaceState(null, null, ' '); // 清除 hash，不影響瀏覽器歷史記錄
                            }, 1000);
                        } else Coker.sweet.error(local.FailedToSend, result.error, null, true);
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
    initFilterField(document.querySelector(".ContactForm"));
    initRadioConditionalRequired(document.querySelector(".ContactForm"));
}
function initFilterField(scope = document) {
    scope.querySelectorAll('.filterCheck').forEach(select => {
        const form = select.closest('form') || scope;

        const updateVisibility = () => {
            const selected = select.selectedOptions[0];
            const filterId = selected?.dataset.filterid || '';
            const activeIds = filterId.split(',').map(s => s.trim()).filter(Boolean);

            form.querySelectorAll('[data-show-on]').forEach(el => {
                const showIds = (el.dataset.showOn || '').split(',').map(s => s.trim());
                const shouldShow = showIds.some(id => activeIds.includes(id));
                el.classList.toggle('d-none', !shouldShow);

                el.querySelectorAll('[required],[data-original-required]').forEach(field => {
                    if (!shouldShow) {
                        field.setAttribute('data-original-required', 'true');
                        field.removeAttribute('required');
                    } else if (field.dataset.originalRequired === 'true') {
                        field.setAttribute('required', 'required');
                    }
                });
            });
        };

        select.addEventListener('change', updateVisibility);
        updateVisibility();
    });
}
function initRadioConditionalRequired(scope = document) {
    // 找所有有補充欄位的 radio group（假設補充欄位是 text 並與 radio 同組）
    scope.querySelectorAll('input[type="radio"]').forEach(radio => {
        const groupName = radio.name;

        // 只處理有補充 input 的 radio 組（避免過度處理）
        const wrapper = radio.closest('.form-check, .d-flex');
        if (!wrapper) return;

        const textInput = wrapper.querySelector('input[type="text"]');
        if (!textInput) return;

        // 設定監聽：在 radio 切換時，根據值決定是否 required
        const updateRequired = () => {
            const checked = scope.querySelector(`input[name="${groupName}"]:checked`);
            if (!checked) return;

            const checkedWrapper = checked.closest('.form-check, .d-flex');
            const checkedTextInput = checkedWrapper?.querySelector('input[type="text"]');

            // 若有補充 input（ex: 是 → 有欄位），就 required；否則移除
            scope.querySelectorAll(`input[name="${groupName}_th"]`).forEach(input => {
                if (input === checkedTextInput) {
                    input.setAttribute('required', 'required');
                } else {
                    input.removeAttribute('required');
                }
            });
        };

        // 監聽所有同組 radio
        const radios = scope.querySelectorAll(`input[type="radio"][name="${groupName}"]`);
        radios.forEach(r => r.addEventListener('change', updateRequired));

        // 初始化
        updateRequired();
    });
}