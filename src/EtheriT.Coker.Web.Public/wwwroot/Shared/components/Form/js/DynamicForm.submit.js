(function (window, $, co) {
    "use strict";

    const Coker = co || window.Coker || {};

    Coker.DynamicForm = Coker.DynamicForm || {};

    const DEFAULT_OPTIONS = {
        senderSelector: '[name="sender"]',
        submittedHash: "submitted",
        clearHashDelay: 1000,

        // 表單來源 metadata 外層容器。
        // 例如：
        // <div class="js-form-source"
        //      data-form-title="..."
        //      data-source-type="4"
        //      data-source-id="123">
        //     <form class="DynamicForm">...</form>
        // </div>
        formSourceSelector: ".js-form-source"
    };

    const submit = {
        init: init,
        bind: bind,
        submitForm: submitForm,
        getSender: getSender,
        getFormSourceMeta: getFormSourceMeta
    };

    Coker.DynamicForm.submit = submit;

    function init(form, options) {
        bind(form, options);
    }

    function bind(form, options) {
        const targetForm = getFormElement(form);

        if (!targetForm) {
            return;
        }

        const opt = $.extend({}, DEFAULT_OPTIONS, options || {});
        const $form = $(targetForm);

        $form
            .off("submit.dynamicForm")
            .on("submit.dynamicForm", function (event) {
                event.preventDefault();
                event.stopPropagation();

                handleSubmit(targetForm, opt);
            });
    }

    function handleSubmit(form, opt) {
        form.classList.add("was-validated");

        if (!validateBeforeSubmit(form)) {
            refreshCaptcha(form);
            co.sweet.error(local.Error, local.FormSubmitMessage, null, true);
            return;
        }

        const sender = getSender(form, opt);

        if (!sender.Email) {
            co.sweet.error(local.InformationError, local.NoSelectSender);
            return;
        }

        submitForm(form, sender, opt);
    }

    function validateBeforeSubmit(form) {
        let checkboxGroupValid = true;

        if (typeof form._validateCheckboxGroups === "function") {
            checkboxGroupValid = form._validateCheckboxGroups(true);
        } else if (
            Coker.DynamicForm &&
            Coker.DynamicForm.validation &&
            typeof Coker.DynamicForm.validation.validate === "function"
        ) {
            checkboxGroupValid = Coker.DynamicForm.validation.validate(form, true);
        }

        return form.checkValidity() && checkboxGroupValid;
    }

    function submitForm(form, sender, opt) {
        const $form = $(form);
        const meta = getFormSourceMeta($form, opt);

        const data = {
            routerName: window.PageKey,
            formTitle: meta.formTitle,
            sourceType: meta.sourceType,
            sourceId: meta.sourceId,
            sender: sender,
            forms: getFormJson($form)
        };

        Coker.DynamicFormApi.Submit(data)
            .done(function (result) {
                if (result.success) {
                    handleSubmitSuccess(form, opt);
                } else {
                    co.sweet.error(local.FailedToSend, result.error, null, true);
                }

                refreshCaptcha(form);
            })
            .fail(function () {
                co.sweet.error(local.FailedToSend, local.FormSubmitMessage, null, true);
                refreshCaptcha(form);
            });
    }

    function getFormSourceMeta($form, opt) {
        const option = $.extend({}, DEFAULT_OPTIONS, opt || {});
        const $source = getNearestFormSource($form, option);

        const formTitle = normalizeText(
            readDataValue($form, "form-title") ||
            readDataValue($source, "form-title")
        );

        const sourceType = normalizeNullableInt(
            readDataValue($form, "source-type") ||
            readDataValue($source, "source-type")
        );

        const sourceId = normalizeNullableInt(
            readDataValue($form, "source-id") ||
            readDataValue($source, "source-id")
        );

        return {
            formTitle: formTitle || null,
            sourceType: sourceType,
            sourceId: sourceId
        };
    }

    function getNearestFormSource($form, opt) {
        if (!$form || !$form.length) {
            return $();
        }

        const selector = opt.formSourceSelector || DEFAULT_OPTIONS.formSourceSelector;

        if ($form.is(selector)) {
            return $form;
        }

        return $form.closest(selector).first();
    }

    function readDataValue($element, dataName) {
        if (!$element || !$element.length || !dataName) {
            return "";
        }

        const attrName = "data-" + dataName;

        // 優先讀 attr，避免 jQuery .data() 快取舊值。
        const attrValue = $element.attr(attrName);

        if (attrValue !== undefined && attrValue !== null) {
            return String(attrValue);
        }

        const camelName = dataName.replace(/-([a-z])/g, function (_, letter) {
            return letter.toUpperCase();
        });

        const dataValue = $element.data(camelName);

        if (dataValue === undefined || dataValue === null) {
            return "";
        }

        return String(dataValue);
    }

    function normalizeText(value) {
        if (value === undefined || value === null) {
            return "";
        }

        return String(value).trim();
    }

    function normalizeNullableInt(value) {
        const text = normalizeText(value);

        if (!text) {
            return null;
        }

        const number = parseInt(text, 10);

        return Number.isFinite(number) ? number : null;
    }

    function handleSubmitSuccess(form, opt) {
        const $form = $(form);

        co.sweet.success(local.SentSuccessfully, null, true);

        $form.removeClass("was-validated");
        form.reset();

        clearValidationState(form);

        if (opt.submittedHash) {
            window.location.hash = opt.submittedHash;

            setTimeout(function () {
                history.replaceState(null, null, " ");
            }, opt.clearHashDelay);
        }
    }

    function clearValidationState(form) {
        form.querySelectorAll(".is-invalid").forEach(function (field) {
            field.classList.remove("is-invalid");
        });

        form.querySelectorAll("input, select, textarea").forEach(function (field) {
            if (typeof field.setCustomValidity === "function") {
                field.setCustomValidity("");
            }
        });

        if (
            Coker.DynamicForm &&
            Coker.DynamicForm.validation &&
            typeof Coker.DynamicForm.validation.validate === "function"
        ) {
            Coker.DynamicForm.validation.validate(form, false);
        }
    }

    function getSender(form, opt) {
        const sender = {
            Email: "",
            Name: ""
        };

        const $senderField = $(form).find(opt.senderSelector).first();

        if (!$senderField.length) {
            return sender;
        }

        const senderElement = $senderField.get(0);

        if (senderElement.tagName === "SELECT") {
            const $selected = $senderField.find("option:selected");

            sender.Email = $selected.val() || "";
            sender.Name = $selected.text() || "";
        } else {
            sender.Email = $senderField.val() || "";
            sender.Name = $senderField.data("title") || "";
        }

        return sender;
    }

    function getFormJson($form) {
        if (typeof $form.getFormJson === "function") {
            return $form.getFormJson();
        }

        return {};
    }

    function refreshCaptcha(form) {
        if (
            Coker.DynamicForm &&
            Coker.DynamicForm.captcha &&
            typeof Coker.DynamicForm.captcha.refresh === "function"
        ) {
            Coker.DynamicForm.captcha.refresh(form);
        }
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

})(window, jQuery, window.co || window.Coker);