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
                form._validateCheckboxGroups();
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
    const form = document.getElementById('ContactForm');
    initFilterField(form);
    initLinkedTextRequired(form);
    bindCheckboxGroupChangeBehavior(form);
    relaxNativeRequiredForCheckboxGroups(form);
    bindCheckboxMutualExclusionByStructure(form);
}
/***** helpers：小工具（僅供本檔使用） *****/

// 由一顆 radio/checkbox 找到「對應」的文字欄：優先 data-for，否則取同格 .checkbox_input_text 內第一個 text
function getLinkedTextForTrigger(trigger, form) {
    // A) 明確指定：文字欄 data-for="trigger.id"
    if (trigger.id) {
        const t = form.querySelector(`input[type="text"][data-for="${CSS.escape(trigger.id)}"]`);
        if (t) return t;
    }
    // B) 備援：同一格 .checkbox_input_text 裡的第一個文字欄
    const box = trigger.closest('.checkbox_input_text');
    if (box) {
        const t = box.querySelector('input[type="text"]');
        if (t) return t;
    }
    // ⚠️ 不再猜 name="_th"（避免誤判）
    return null;
}

// 清文字欄：移除 required、清 invalid 樣式和值
function clearTextField(t) {
    if (!t) return;
    t.removeAttribute('required');
    if (typeof t.setCustomValidity === 'function') t.setCustomValidity('');
    if (t.classList) t.classList.remove('is-invalid');
    t.value = '';
}


/***** 1) initFilterField：若你本來有邏輯就放回來；沒有就留這個 no-op *****/
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


/***** 2) 文字欄必填連動（radio/checkbox） *****/
function initLinkedTextRequired(form) {
    if (!form) return;

    // A) radio：同 name 為一組；切換時清「本組所有」文字欄，只對選中的那顆加 required
    const radioGroups = Array.from(new Set(
        [...form.querySelectorAll('input[type="radio"][name]')].map(r => r.name)
    ));
    radioGroups.forEach(group => {
        const radios = [...form.querySelectorAll(`input[type="radio"][name="${group}"]`)];
        // 這組所有可能被使用的文字欄（透過 data-for 或同格容器推得）
        const textsInGroup = [...new Set(
            radios.map(r => getLinkedTextForTrigger(r, form)).filter(Boolean)
        )];
        if (!textsInGroup.length) return;

        const clearAll = () => textsInGroup.forEach(clearTextField);

        const update = () => {
            clearAll();
            const checked = radios.find(r => r.checked);
            if (checked) {
                const t = getLinkedTextForTrigger(checked, form);
                if (t) t.setAttribute('required', 'required');
            }
            if (form.classList.contains('was-validated')) form.checkValidity();
        };

        radios.forEach(r => r.addEventListener('change', update));
        update(); // 初始化同步
    });

    // B) checkbox：逐顆就地處理（多選時互不影響）
    form.querySelectorAll('input[type="checkbox"][name]').forEach(cb => {
        const t = getLinkedTextForTrigger(cb, form);
        const sync = () => {
            if (!t) { if (form.classList.contains('was-validated')) form.checkValidity(); return; }
            if (cb.checked) {
                t.setAttribute('required', 'required');
            } else {
                clearTextField(t); // 只清「這顆」對應的文字
            }
            if (form.classList.contains('was-validated')) form.checkValidity();
        };
        cb.addEventListener('change', sync);
        sync(); // 依初始勾選狀態
    });
}


/***** 3) checkbox 群組（至少勾一個）行為與驗證 *****/
function bindCheckboxGroupChangeBehavior(form) {
    if (!form) return;

    const visible = el =>
        !(el.disabled || el.closest('[hidden]') || el.closest('.d-none') || el.offsetParent === null);

    const validateGroups = () => {
        form.querySelectorAll('.checkbox_father_config').forEach(section => {
            const byName = new Map();
            section.querySelectorAll('input[type="checkbox"][name]').forEach(cb => {
                const arr = byName.get(cb.name) || [];
                arr.push(cb);
                byName.set(cb.name, arr);
            });

            byName.forEach(group => {
                if (group.length < 2) return; // 單顆留給原生 required
                // 只在「本組是必填」時才要求至少一個（依你的規則）
                const groupIsRequired =
                    !!section.querySelector('.title.required') ||
                    section.getAttribute('data-group-required') === 'true' ||
                    group.some(cb => cb.dataset.originalRequired === 'true');

                if (!groupIsRequired) {
                    group[0].setCustomValidity('');
                    group.forEach(cb => cb.classList?.remove('is-invalid'));
                    return;
                }

                const anyChecked = group.some(cb => visible(cb) && cb.checked);
                const anchor = group[0];
                anchor.setCustomValidity(anyChecked ? '' : '請至少勾選一項');
                group.forEach(cb => {
                    if (!visible(cb)) return;
                    cb.classList.toggle('is-invalid', !anyChecked);
                });
            });
        });
    };

    // 即時監聽
    form.querySelectorAll('input[type="checkbox"]').forEach(cb => {
        cb.addEventListener('change', validateGroups);
    });

    // 🔹對外提供一個可重跑的鉤子，讓 submit 前能再同步一次
    form._validateCheckboxGroups = validateGroups;

    // 初始化同步
    validateGroups();
}


/***** 4) 放寬原生 required（僅針對「群組」checkbox） *****/
function relaxNativeRequiredForCheckboxGroups(form) {
    if (!form) return;

    // 僅移除「在 .checkbox_father_config 內，且同 name 有 2 顆以上」的 checkbox 的原生 required
    form.querySelectorAll('.checkbox_father_config').forEach(section => {
        const map = new Map();
        section.querySelectorAll('input[type="checkbox"][name]').forEach(cb => {
            const arr = map.get(cb.name) || [];
            arr.push(cb);
            map.set(cb.name, arr);
        });
        map.forEach(group => {
            if (group.length >= 2) {
                group.forEach(cb => cb.removeAttribute('required'));
            }
        });
    });

    // ⚠️ 不動群組以外的單顆（例如同意條款 CheckAgree），保留其原生 required
}


/***** 5) 互斥：只認 data-exclusive（加在 input 或其 .form-check 上） *****/
function bindCheckboxMutualExclusionByStructure(form) {
    if (!form) return;

    // 依群組容器 + 同 name 分組（只處理 checkbox）
    form.querySelectorAll('.checkbox_father_config').forEach(section => {
        const byName = new Map();
        section.querySelectorAll('input[type="checkbox"][name]').forEach(cb => {
            const arr = byName.get(cb.name) || [];
            arr.push(cb);
            byName.set(cb.name, arr);
        });

        byName.forEach(group => {
            if (group.length < 2) return;

            const isExclusive = (el) =>
                el.hasAttribute('data-exclusive') ||
                (el.closest('.form-check') && el.closest('.form-check').hasAttribute('data-exclusive'));

            const exclusives = group.filter(isExclusive);
            if (!exclusives.length) return; // 這組沒有標 data-exclusive 就不處理（不猜）

            const others = group.filter(cb => !isExclusive(cb));

            const cancelOne = (cb) => {
                if (!cb.checked) return;
                cb.checked = false;
                // 若這顆有對應文字欄，同步清除
                const t = getLinkedTextForTrigger(cb, form);
                clearTextField(t);
                // 讓其他聯動（若有）可接到
                cb.dispatchEvent(new Event('change', { bubbles: true }));
            };

            const onChange = (e) => {
                const changed = e.target;
                if (!changed.checked) return;

                if (isExclusive(changed)) {
                    // 勾到獨佔 → 取消其他
                    others.forEach(cancelOne);
                } else {
                    // 勾到非獨佔 → 取消所有獨佔
                    exclusives.forEach(cancelOne);
                }
                if (form.classList.contains('was-validated')) form.checkValidity();
            };

            group.forEach(cb => cb.addEventListener('change', onChange));

            // 初始整理：若一開始就有非獨佔被勾，取消獨佔
            if (others.some(cb => cb.checked)) exclusives.forEach(cancelOne);
        });
    });
}
//***** 6) 提交前驗證 checkbox 群組（被援用，必填checkBox 至少選擇一項） *****/
function validateRequiredCheckboxGroups(form) {
    if (!form) return;

    const visible = el =>
        !(el.disabled || el.closest('[hidden]') || el.closest('.d-none') || el.offsetParent === null);

    form.querySelectorAll('.checkbox_father_config').forEach(section => {
        // 依 name 分組
        const groups = new Map();
        section.querySelectorAll('input[type="checkbox"][name]').forEach(cb => {
            const arr = groups.get(cb.name) || [];
            arr.push(cb);
            groups.set(cb.name, arr);
        });

        groups.forEach(group => {
            // 單顆不當群組，交給原生 required
            if (group.length < 2) return;

            // ✅ 判斷這個群組是否“本來就是必填”的
            const groupIsRequired =
                // 1) 標題上有 .title.required（你的HTML就這樣）
                !!section.querySelector('.title.required') ||
                // 2) 有欄位留下 data-original-required（initFilterField/放寬時可保留）
                group.some(cb => cb.dataset.originalRequired === 'true') ||
                // 3) 或容器顯式標 data-group-required="true"（選用）
                section.getAttribute('data-group-required') === 'true';

            // 若不是必填 → 清乾淨就好，不設錯誤
            if (!groupIsRequired) {
                group[0].setCustomValidity('');
                group.forEach(cb => cb.classList && cb.classList.remove('is-invalid'));
                return;
            }

            // 必填群組：至少一顆“可見且非 disabled”被勾
            const anyChecked = group.some(cb => visible(cb) && cb.checked);

            // 用第一顆當掛點設定/清除錯誤
            const anchor = group[0];
            anchor.setCustomValidity(anyChecked ? '' : '請至少勾選一項');

            // 給 Bootstrap 樣式用
            group.forEach(cb => {
                if (!visible(cb)) return; // 隱藏/disabled 的不加紅框
                cb.classList.toggle('is-invalid', !anyChecked);
            });
        });
    });
}
