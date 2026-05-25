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