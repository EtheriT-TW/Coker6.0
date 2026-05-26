(function (window, $, co) {
    "use strict";

    const Coker = co || window.Coker || {};

    Coker.DynamicForm = Coker.DynamicForm || {};

    const DEFAULT_OPTIONS = {
        formSelector: ".ContactForm, .DynamicForm",

        captchaInputSelector: '[name="captcha"]',
        captchaImageSelector: ".img-fluid",
        refreshButtonSelector: ".btn_refresh",
        captchaType: "ContactUs",

        senderSelector: '[name="sender"]',
        submitUrl: "/api/Contact/submit",
        antiforgerySelector: 'input:hidden[name="AntiforgeryFieldname"]',
        submittedHash: "submitted",
        clearHashDelay: 1000
    };

    const core = {
        init: init,
        initOne: initOne,
        findForms: findForms,
        options: DEFAULT_OPTIONS
    };

    Coker.DynamicForm.init = init;
    Coker.DynamicForm.initOne = initOne;
    Coker.DynamicForm.findForms = findForms;
    Coker.DynamicForm.options = DEFAULT_OPTIONS;
    Coker.DynamicForm.core = core;

    function init(root, options) {
        const opt = $.extend({}, DEFAULT_OPTIONS, options || {});
        const $forms = findForms(root, opt);

        $forms.each(function () {
            initOne(this, opt);
        });

        return $forms;
    }

    function initOne(form, options) {
        const targetForm = getFormElement(form);

        if (!targetForm) {
            return;
        }

        const $form = $(targetForm);
        const opt = $.extend({}, DEFAULT_OPTIONS, options || {});

        if ($form.data("dynamic-form-inited") === true) {
            return;
        }

        $form.data("dynamic-form-inited", true);

        normalizeFormControlIds(targetForm);

        initFormJson($form);

        if (Coker.DynamicForm.validation && typeof Coker.DynamicForm.validation.init === "function") {
            Coker.DynamicForm.validation.init(targetForm, opt);
        }

        if (Coker.DynamicForm.behavior && typeof Coker.DynamicForm.behavior.init === "function") {
            Coker.DynamicForm.behavior.init(targetForm, opt);
        }

        if (Coker.DynamicForm.captcha && typeof Coker.DynamicForm.captcha.init === "function") {
            Coker.DynamicForm.captcha.init(targetForm, opt);
        }

        if (Coker.DynamicForm.submit && typeof Coker.DynamicForm.submit.init === "function") {
            Coker.DynamicForm.submit.init(targetForm, opt);
        }
    }

    function findForms(root, opt) {
        const $root = toRoot(root);
        const selector = opt.formSelector;

        if ($root.is(selector)) {
            return $root;
        }

        return $root.find(selector);
    }

    function toRoot(root) {
        if (!root) {
            return $(document);
        }

        if (root.jquery) {
            return root;
        }

        return $(root);
    }

    function getFormElement(form) {
        if (!form) {
            return null;
        }

        if (form.jquery) {
            return form.get(0);
        }

        return form;
    }

    function initFormJson($form) {
        if (typeof $form.getFormJson === "function") {
            $form.getFormJson();
        }
    }

    function normalizeFormControlIds(form) {
        if (!form) {
            return;
        }

        const formUid = getOrCreateFormUid(form);
        const controls = Array.prototype.slice.call(
            form.querySelectorAll("input[id], select[id], textarea[id], button[id], img[id]")
        );

        controls.forEach(function (control, index) {
            const oldId = control.getAttribute("id");

            if (!oldId) {
                return;
            }

            if (!shouldRenameId(control, oldId, form)) {
                return;
            }

            const newId = buildScopedId(oldId, formUid, index);

            control.setAttribute("id", newId);
            updateIdReferences(form, oldId, newId, control);
        });
    }

    function getOrCreateFormUid(form) {
        let uid = form.getAttribute("data-dynamic-form-uid");

        if (uid) {
            return uid;
        }

        uid = "df_" + Date.now().toString(36) + "_" + Math.random().toString(36).slice(2, 8);
        form.setAttribute("data-dynamic-form-uid", uid);

        return uid;
    }

    function shouldRenameId(control, oldId, form) {
        const sameIdElements = document.querySelectorAll("#" + cssEscape(oldId));

        if (sameIdElements.length <= 1) {
            return false;
        }

        // 如果重複 id 都在同一個 form 內，也要修。
        // 如果其他 form 或頁面上也有相同 id，也要修。
        return true;
    }

    function buildScopedId(oldId, formUid, index) {
        return oldId + "_" + formUid + "_" + index;
    }

    function updateIdReferences(form, oldId, newId, ownerControl) {
        updateForReferences(form, oldId, newId, ownerControl);
        updateDataForReferences(form, oldId, newId, ownerControl);
        updateAriaIdReferences(form, oldId, newId);
    }

    function updateForReferences(form, oldId, newId, ownerControl) {
        const labels = Array.prototype.slice.call(
            form.querySelectorAll('label[for="' + cssEscape(oldId) + '"]')
        );

        if (!labels.length) {
            return;
        }

        // 優先處理同一個 form-floating / form-check / call 容器內的 label。
        const ownerContainer = ownerControl.closest(".form-floating, .form-check, .call, .checkbox_input_text");

        labels.forEach(function (label) {
            if (!ownerContainer || ownerContainer.contains(label)) {
                label.setAttribute("for", newId);
            }
        });
    }

    function updateDataForReferences(form, oldId, newId, ownerControl) {
        const fields = Array.prototype.slice.call(
            form.querySelectorAll('[data-for="' + cssEscape(oldId) + '"]')
        );

        if (!fields.length) {
            return;
        }

        const ownerContainer = ownerControl.closest(".form-floating, .form-check, .call, .checkbox_input_text");

        fields.forEach(function (field) {
            if (!ownerContainer || ownerContainer.contains(field)) {
                field.setAttribute("data-for", newId);
            }
        });
    }

    function updateAriaIdReferences(form, oldId, newId) {
        [
            "aria-describedby",
            "aria-labelledby",
            "aria-controls",
            "aria-owns"
        ].forEach(function (attrName) {
            form.querySelectorAll("[" + attrName + "]").forEach(function (element) {
                const value = element.getAttribute(attrName);

                if (!value) {
                    return;
                }

                const parts = value.split(/\s+/).map(function (id) {
                    return id === oldId ? newId : id;
                });

                element.setAttribute(attrName, parts.join(" "));
            });
        });
    }

    function cssEscape(value) {
        if (window.CSS && typeof window.CSS.escape === "function") {
            return window.CSS.escape(value);
        }

        return String(value).replace(/["\\]/g, "\\$&");
    }

    $.fn.dynamicFormInit = function (options) {
        return this.each(function () {
            initOne(this, options);
        });
    };

    $.fn.contactInit = function (options) {
        return this.each(function () {
            initOne(this, options);
        });
    };

    window.DynamicFormInit = function (root, options) {
        return init(root, options);
    };

    window.setContact = function (root, options) {
        return init(root, options);
    };

})(window, jQuery, window.co || window.Coker);