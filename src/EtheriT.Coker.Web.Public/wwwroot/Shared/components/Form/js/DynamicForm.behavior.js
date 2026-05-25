(function (window, $, co) {
    "use strict";

    const Coker = co || window.Coker || {};
    const MODULE_NAME = "DynamicForm.behavior";

    Coker.DynamicForm = Coker.DynamicForm || {};

    const behavior = {
        init: init,
        initFilterField: initFilterField,
        initLinkedTextRequired: initLinkedTextRequired,
        bindCheckboxMutualExclusion: bindCheckboxMutualExclusion,
        bindAutoNext: bindAutoNext
    };

    Coker.DynamicForm.behavior = behavior;

    function init(form) {
        const targetForm = getFormElement(form);

        if (!targetForm) {
            return;
        }

        initFilterField(targetForm);
        initLinkedTextRequired(targetForm);
        bindCheckboxMutualExclusion(targetForm);
        bindAutoNext(targetForm);
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

    function initFilterField(form) {
        if (!form) {
            return;
        }

        $(form)
            .off("change.dynamicFormFilter", ".filterCheck")
            .on("change.dynamicFormFilter", ".filterCheck", function () {
                updateFilterVisibility(form, this);
            });

        form.querySelectorAll(".filterCheck").forEach(function (select) {
            updateFilterVisibility(form, select);
        });
    }

    function updateFilterVisibility(form, select) {
        if (!form || !select) {
            return;
        }

        const selected = select.selectedOptions && select.selectedOptions.length
            ? select.selectedOptions[0]
            : null;

        const filterId = selected && selected.dataset
            ? selected.dataset.filterid || ""
            : "";

        const activeIds = filterId
            .split(",")
            .map(function (value) {
                return value.trim();
            })
            .filter(function (value) {
                return value.length > 0;
            });

        form.querySelectorAll("[data-show-on]").forEach(function (element) {
            const showIds = (element.dataset.showOn || "")
                .split(",")
                .map(function (value) {
                    return value.trim();
                })
                .filter(function (value) {
                    return value.length > 0;
                });

            const shouldShow = showIds.some(function (id) {
                return activeIds.indexOf(id) >= 0;
            });

            element.classList.toggle("d-none", !shouldShow);

            syncRequiredByVisibility(element, shouldShow);

            if (!shouldShow) {
                clearInvalidState(element);
            }
        });

        refreshCheckboxGroupValidation(form);
    }

    function syncRequiredByVisibility(container, visible) {
        if (!container) {
            return;
        }

        container.querySelectorAll("[required],[data-original-required]").forEach(function (field) {
            if (!visible) {
                if (field.hasAttribute("required")) {
                    field.setAttribute("data-original-required", "true");
                }

                field.removeAttribute("required");

                if (typeof field.setCustomValidity === "function") {
                    field.setCustomValidity("");
                }

                return;
            }

            if (field.dataset.originalRequired === "true") {
                field.setAttribute("required", "required");
            }
        });
    }

    function clearInvalidState(container) {
        if (!container) {
            return;
        }

        container.querySelectorAll(".is-invalid").forEach(function (field) {
            field.classList.remove("is-invalid");
        });
    }

    function initLinkedTextRequired(form) {
        if (!form) {
            return;
        }

        bindRadioLinkedTextRequired(form);
        bindCheckboxLinkedTextRequired(form);
    }

    function bindRadioLinkedTextRequired(form) {
        const radioNames = Array.from(new Set(
            Array.prototype.slice.call(form.querySelectorAll('input[type="radio"][name]'))
                .map(function (radio) {
                    return radio.name;
                })
        ));

        radioNames.forEach(function (name) {
            const radios = Array.prototype.slice.call(
                form.querySelectorAll('input[type="radio"][name="' + cssEscape(name) + '"]')
            );

            if (!radios.length) {
                return;
            }

            const linkedTexts = getLinkedTextsInGroup(radios, form);

            if (!linkedTexts.length) {
                return;
            }

            const sync = function (changedRadio) {
                const checkedRadio = radios.find(function (radio) {
                    return radio.checked;
                });

                linkedTexts.forEach(function (textField) {
                    const owner = getTriggerByLinkedText(radios, textField, form);
                    const shouldBeRequired = checkedRadio && owner === checkedRadio;

                    if (shouldBeRequired) {
                        textField.setAttribute("required", "required");
                    } else {
                        clearTextField(textField, true);
                    }
                });

                refreshFormValidity(form);
            };

            $(form)
                .off("change.dynamicFormRadioText." + safeEventKey(name), 'input[type="radio"][name="' + cssEscape(name) + '"]')
                .on("change.dynamicFormRadioText." + safeEventKey(name), 'input[type="radio"][name="' + cssEscape(name) + '"]', function () {
                    sync(this);
                });

            sync(null);
        });
    }

    function bindCheckboxLinkedTextRequired(form) {
        $(form)
            .off("change.dynamicFormCheckboxText", 'input[type="checkbox"][name]')
            .on("change.dynamicFormCheckboxText", 'input[type="checkbox"][name]', function () {
                syncCheckboxLinkedText(form, this);
            });

        form.querySelectorAll('input[type="checkbox"][name]').forEach(function (checkbox) {
            syncCheckboxLinkedText(form, checkbox);
        });
    }

    function syncCheckboxLinkedText(form, checkbox) {
        const textField = getLinkedTextForTrigger(checkbox, form);

        if (!textField) {
            refreshFormValidity(form);
            return;
        }

        if (checkbox.checked) {
            textField.setAttribute("required", "required");
        } else {
            clearTextField(textField, true);
        }

        refreshFormValidity(form);
    }

    function getLinkedTextsInGroup(triggers, form) {
        const result = [];

        triggers.forEach(function (trigger) {
            const textField = getLinkedTextForTrigger(trigger, form);

            if (textField && result.indexOf(textField) < 0) {
                result.push(textField);
            }
        });

        return result;
    }

    function getTriggerByLinkedText(triggers, textField, form) {
        for (let i = 0; i < triggers.length; i++) {
            const linked = getLinkedTextForTrigger(triggers[i], form);

            if (linked === textField) {
                return triggers[i];
            }
        }

        return null;
    }

    function getLinkedTextForTrigger(trigger, form) {
        if (!trigger || !form) {
            return null;
        }

        if (trigger.id) {
            const explicit = form.querySelector('input[type="text"][data-for="' + cssEscape(trigger.id) + '"]');

            if (explicit) {
                return explicit;
            }
        }

        const box = trigger.closest(".checkbox_input_text");

        if (box) {
            return box.querySelector('input[type="text"]');
        }

        return null;
    }

    function clearTextField(textField, clearValue) {
        if (!textField) {
            return;
        }

        textField.removeAttribute("required");

        if (typeof textField.setCustomValidity === "function") {
            textField.setCustomValidity("");
        }

        textField.classList.remove("is-invalid");

        if (clearValue === true) {
            textField.value = "";
        }
    }

    function bindCheckboxMutualExclusion(form) {
        if (!form) {
            return;
        }

        $(form)
            .off("change.dynamicFormExclusive", '.checkbox_father_config input[type="checkbox"][name]')
            .on("change.dynamicFormExclusive", '.checkbox_father_config input[type="checkbox"][name]', function () {
                handleExclusiveCheckboxChange(form, this);
            });

        normalizeInitialExclusiveState(form);
    }

    function handleExclusiveCheckboxChange(form, changed) {
        if (!changed || !changed.checked) {
            refreshCheckboxGroupValidation(form);
            return;
        }

        const section = changed.closest(".checkbox_father_config");

        if (!section) {
            return;
        }

        const group = Array.prototype.slice.call(
            section.querySelectorAll('input[type="checkbox"][name="' + cssEscape(changed.name) + '"]')
        );

        if (group.length < 2) {
            return;
        }

        const exclusives = group.filter(isExclusiveCheckbox);

        if (!exclusives.length) {
            return;
        }

        const others = group.filter(function (checkbox) {
            return !isExclusiveCheckbox(checkbox);
        });

        if (isExclusiveCheckbox(changed)) {
            others.forEach(function (checkbox) {
                cancelCheckbox(form, checkbox);
            });
        } else {
            exclusives.forEach(function (checkbox) {
                cancelCheckbox(form, checkbox);
            });
        }

        refreshCheckboxGroupValidation(form);
    }

    function normalizeInitialExclusiveState(form) {
        form.querySelectorAll(".checkbox_father_config").forEach(function (section) {
            const groupMap = new Map();

            section.querySelectorAll('input[type="checkbox"][name]').forEach(function (checkbox) {
                const list = groupMap.get(checkbox.name) || [];

                list.push(checkbox);
                groupMap.set(checkbox.name, list);
            });

            groupMap.forEach(function (group) {
                if (group.length < 2) {
                    return;
                }

                const exclusives = group.filter(isExclusiveCheckbox);

                if (!exclusives.length) {
                    return;
                }

                const others = group.filter(function (checkbox) {
                    return !isExclusiveCheckbox(checkbox);
                });

                if (others.some(function (checkbox) {
                    return checkbox.checked;
                })) {
                    exclusives.forEach(function (checkbox) {
                        cancelCheckbox(form, checkbox);
                    });
                }
            });
        });

        refreshCheckboxGroupValidation(form);
    }

    function isExclusiveCheckbox(checkbox) {
        if (!checkbox) {
            return false;
        }

        return checkbox.hasAttribute("data-exclusive") ||
            !!checkbox.closest(".form-check[data-exclusive]");
    }

    function cancelCheckbox(form, checkbox) {
        if (!checkbox || !checkbox.checked) {
            return;
        }

        checkbox.checked = false;

        const textField = getLinkedTextForTrigger(checkbox, form);
        clearTextField(textField, true);

        $(checkbox).trigger("change");
    }

    function bindAutoNext(form) {
        if (!form) {
            return;
        }

        $(form)
            .off("keyup.dynamicFormAutoNext", "input")
            .on("keyup.dynamicFormAutoNext", "input", function () {
                const target = this;

                if (!target.maxLength || target.maxLength <= 0) {
                    return;
                }

                if (target.value.length !== target.maxLength) {
                    return;
                }

                const inputs = $(form).find("input").toArray();
                const index = inputs.indexOf(target);

                if (index >= 0 && inputs[index + 1]) {
                    inputs[index + 1].focus();
                }
            });
    }

    function refreshFormValidity(form) {
        if (!form || !form.classList.contains("was-validated")) {
            return;
        }

        form.checkValidity();
        refreshCheckboxGroupValidation(form);
    }

    function refreshCheckboxGroupValidation(form) {
        if (!form || !Coker.DynamicForm || !Coker.DynamicForm.validation) {
            return;
        }

        if (typeof Coker.DynamicForm.validation.validate === "function") {
            Coker.DynamicForm.validation.validate(
                form,
                form.classList.contains("was-validated")
            );
        }
    }

    function cssEscape(value) {
        if (window.CSS && typeof window.CSS.escape === "function") {
            return window.CSS.escape(value);
        }

        return String(value).replace(/["\\]/g, "\\$&");
    }

    function safeEventKey(value) {
        return String(value || "")
            .replace(/[^a-zA-Z0-9_-]/g, "_");
    }

})(window, jQuery, window.co || window.Coker);