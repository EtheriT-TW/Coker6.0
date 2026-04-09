(function (window) {
    "use strict";

    const Form = {};

    // =============================
    // 工具：欄位名稱標準化（忽略大小寫 / _ / -）
    // =============================
    function normalizeKey(key) {
        return String(key || "")
            .toLowerCase()
            .replace(/[_-]/g, "");
    }

    // =============================
    // 工具：建立 key 對應表
    // =============================
    function buildKeyMap(data) {
        const map = {};
        Object.keys(data || {}).forEach(k => {
            map[normalizeKey(k)] = k;
        });
        return map;
    }

    // =============================
    // insertData（核心）
    // =============================
    Form.insertData = function (data, formSelector) {
        if (!data) return;

        const $form = $(formSelector);
        const keyMap = buildKeyMap(data);

        $form.find("[name]").each(function () {
            const $el = $(this);
            const name = $el.attr("name");
            const normalized = normalizeKey(name);

            if (!keyMap[normalized]) return;

            const value = data[keyMap[normalized]];

            // checkbox（單一 boolean）
            if ($el.is(":checkbox") && !$el.attr("value")) {
                $el.prop("checked", !!value);
                return;
            }

            // checkbox（group）
            if ($el.is(":checkbox") && $el.attr("value")) {
                if (Array.isArray(value)) {
                    $el.prop("checked", value.includes($el.val()));
                }
                return;
            }

            // radio
            if ($el.is(":radio")) {
                $el.prop("checked", String($el.val()) === String(value));
                return;
            }

            // select
            if ($el.is("select")) {
                $el.val(value).trigger("change");
                return;
            }

            // number（顯示千分位）
            if ($el.attr("type") === "number" || $el.data("type") === "number") {
                if (value !== null && value !== undefined && value !== "") {
                    $el.val(Number(value).toLocaleString());
                } else {
                    $el.val("");
                }
                return;
            }

            // custom setter
            if ($el.data("formType") === "custom") {
                const setter = $el.data("setter");
                if (typeof setter === "function") {
                    setter($el, value);
                }
                return;
            }

            // default
            $el.val(value ?? "");
        });
    };

    // =============================
    // clear（重寫）
    // =============================
    Form.clear = function (formSelector) {
        const $form = $(formSelector);

        $form.find("[name]").each(function () {
            const $el = $(this);

            if ($el.is(":checkbox") || $el.is(":radio")) {
                $el.prop("checked", false);
                return;
            }

            if ($el.is("select")) {
                $el.val("").trigger("change");
                return;
            }

            if ($el.data("formType") === "custom") {
                const setter = $el.data("setter");
                if (typeof setter === "function") {
                    setter($el, null);
                }
                return;
            }

            $el.val("");
        });
    };

    // =============================
    // number parse
    // =============================
    function parseNumber(val) {
        if (val === null || val === undefined) return null;
        const cleaned = String(val).replace(/,/g, "");
        if (cleaned === "") return null;
        return Number(cleaned);
    }

    // =============================
    // getJson
    // =============================
    Form.getJson = function (formSelector) {
        const $form = $(formSelector);
        const result = {};

        $form.find("[name]").each(function () {
            const $el = $(this);
            const name = $el.attr("name");

            // checkbox（group）
            if ($el.is(":checkbox") && $el.attr("value")) {
                if (!result[name]) result[name] = [];

                if ($el.prop("checked")) {
                    result[name].push($el.val());
                }
                return;
            }

            // checkbox（boolean）
            if ($el.is(":checkbox")) {
                result[name] = $el.prop("checked");
                return;
            }

            // radio
            if ($el.is(":radio")) {
                if ($el.prop("checked")) {
                    result[name] = $el.val();
                }
                return;
            }

            // custom getter
            if ($el.data("formType") === "custom") {
                const getter = $el.data("getter");
                if (typeof getter === "function") {
                    result[name] = getter($el);
                }
                return;
            }

            let val = $el.val();

            // number parse
            if ($el.attr("type") === "number" || $el.data("type") === "number") {
                val = parseNumber(val);
            }

            result[name] = val;
        });

        return result;
    };

    // =============================
    // init（避免重複 submit 綁定）
    // =============================
    Form.init = function (formSelector, onSubmit) {
        const $form = $(formSelector);

        $form.off("submit.form").on("submit.form", function (e) {
            e.preventDefault();

            if (typeof onSubmit === "function") {
                onSubmit(Form.getJson(formSelector));
            }
        });
    };

    // =============================
    // export
    // =============================
    window._c = window._c || {};
    window._c.Form = Form;

})(window);