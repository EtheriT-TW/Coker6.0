(function (window, $, co) {
    "use strict";

    const Coker = co || window.Coker || {};
    const MODULE_NAME = "DynamicForm.validation";

    Coker.DynamicForm = Coker.DynamicForm || {};

    const validation = {
        init: init,
        validate: validate,
        relaxNativeRequiredForCheckboxGroups: relaxNativeRequiredForCheckboxGroups
    };

    Coker.DynamicForm.validation = validation;

    function init(form) {
        if (!form) {
            return;
        }

        const targetForm = getFormElement(form);

        if (!targetForm) {
            return;
        }

        relaxNativeRequiredForCheckboxGroups(targetForm);
        bindCheckboxGroupChangeBehavior(targetForm);

        targetForm._validateCheckboxGroups = function (showError) {
            return validate(targetForm, showError);
        };

        validate(targetForm, false);
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

    function bindCheckboxGroupChangeBehavior(form) {
        $(form)
            .off("change.dynamicFormValidation", '.checkbox_father_config input[type="checkbox"]')
            .on("change.dynamicFormValidation", '.checkbox_father_config input[type="checkbox"]', function () {
                validate(form, form.classList.contains("was-validated"));
            });
    }

    function validate(form, showError) {
        if (!form) {
            return true;
        }

        showError = showError === true;

        let isValid = true;

        getCheckboxGroups(form).forEach(function (groupInfo) {
            const group = groupInfo.items;

            if (group.length < 2) {
                return;
            }

            const anchor = group[0];
            const groupRequired = isGroupRequired(groupInfo.section, group);

            if (!groupRequired) {
                anchor.setCustomValidity("");
                group.forEach(function (checkbox) {
                    checkbox.classList.remove("is-invalid");
                });
                return;
            }

            const anyChecked = group.some(function (checkbox) {
                return isVisibleField(checkbox) && checkbox.checked;
            });

            anchor.setCustomValidity(anyChecked ? "" : "請至少勾選一項");

            if (!anyChecked) {
                isValid = false;
            }

            group.forEach(function (checkbox) {
                if (!isVisibleField(checkbox)) {
                    checkbox.classList.remove("is-invalid");
                    return;
                }

                checkbox.classList.toggle("is-invalid", showError && !anyChecked);
            });
        });

        return isValid;
    }

    function getCheckboxGroups(form) {
        const groups = [];

        form.querySelectorAll(".checkbox_father_config").forEach(function (section) {
            const map = new Map();

            section.querySelectorAll('input[type="checkbox"][name]').forEach(function (checkbox) {
                const name = checkbox.getAttribute("name");
                const items = map.get(name) || [];

                items.push(checkbox);
                map.set(name, items);
            });

            map.forEach(function (items) {
                groups.push({
                    section: section,
                    items: items
                });
            });
        });

        return groups;
    }

    function isGroupRequired(section, group) {
        if (!section || !group || !group.length) {
            return false;
        }

        return !!section.querySelector(".title.required") ||
            section.getAttribute("data-group-required") === "true" ||
            group.some(function (checkbox) {
                return checkbox.dataset.originalRequired === "true";
            });
    }

    function relaxNativeRequiredForCheckboxGroups(form) {
        if (!form) {
            return;
        }

        getCheckboxGroups(form).forEach(function (groupInfo) {
            const group = groupInfo.items;

            if (group.length < 2) {
                return;
            }

            group.forEach(function (checkbox) {
                if (checkbox.hasAttribute("required")) {
                    checkbox.dataset.originalRequired = "true";
                }

                checkbox.removeAttribute("required");
            });
        });
    }

    function isVisibleField(field) {
        if (!field) {
            return false;
        }

        return !field.disabled &&
            !field.closest("[hidden]") &&
            !field.closest(".d-none") &&
            field.offsetParent !== null;
    }

})(window, jQuery, window.co || window.Coker);