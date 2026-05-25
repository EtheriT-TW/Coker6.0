(function (window, $, co) {
    "use strict";

    const Coker = co || window.Coker || {};

    Coker.DynamicForm = Coker.DynamicForm || {};

    const DEFAULT_OPTIONS = {
        captchaInputSelector: '[name="captcha"]',
        captchaImageSelector: ".img-fluid",
        refreshButtonSelector: ".btn_refresh",
        captchaType: "ContactUs"
    };

    const captcha = {
        init: init,
        refresh: refresh,
        getElements: getElements
    };

    Coker.DynamicForm.captcha = captcha;

    function init(form, options) {
        const targetForm = getFormElement(form);

        if (!targetForm) {
            return;
        }

        const opt = $.extend({}, DEFAULT_OPTIONS, options || {});
        const elements = getElements(targetForm, opt);

        bindRefreshButton(targetForm, elements, opt);

        refresh(targetForm, opt);
    }

    function bindRefreshButton(form, elements, opt) {
        if (!elements.$refreshButton.length) {
            return;
        }

        elements.$refreshButton
            .off("click.dynamicFormCaptcha")
            .on("click.dynamicFormCaptcha", function (event) {
                event.preventDefault();
                refresh(form, opt);
            });
    }

    function refresh(form, options) {
        const targetForm = getFormElement(form);

        if (!targetForm) {
            return;
        }

        const opt = $.extend({}, DEFAULT_OPTIONS, options || {});
        const elements = getElements(targetForm, opt);

        if (!elements.$captchaImage.length || !elements.$captchaInput.length) {
            return;
        }

        elements.$captchaInput.val("");
        elements.$captchaInput.removeClass("is-invalid");

        if (typeof elements.$captchaInput.get(0).setCustomValidity === "function") {
            elements.$captchaInput.get(0).setCustomValidity("");
        }

        if (typeof window.NewCaptcha === "function") {
            window.NewCaptcha(
                elements.$captchaImage,
                elements.$captchaInput,
                opt.captchaType
            );
        }
    }

    function getElements(form, opt) {
        const $form = $(form);

        return {
            $captchaInput: $form.find(opt.captchaInputSelector).first(),
            $captchaImage: $form.find(opt.captchaImageSelector).last(),
            $refreshButton: $form.find(opt.refreshButtonSelector)
        };
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