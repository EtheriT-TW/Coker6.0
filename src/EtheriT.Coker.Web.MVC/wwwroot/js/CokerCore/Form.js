Coker.extend({
    Form: {
        insertData: function (obj, $self) {
            if (typeof ($self) == "undefined" || $self == null) $self = $("form").first();
            else if (typeof ($self) == "string") {
                $self = /^#/.test($self) ? $($self) : $(`#${$self}`);
            }

            const formTypeSet = (type, $e, value) => {
                switch (type) {
                    case "zipcode":
                        co.Zipcode.setData({
                            el: $e,
                            addr: value
                        });
                        break;

                    case "date_range":
                        if (!!!$e.data('daterangepicker')) _c.Picker.Init($e);
                        if (!!obj[$e.data("start")] || !!obj[$e.data("end")]) {
                            $e.data('daterangepicker').setStartDate(obj[$e.data("start")]);
                            $e.data('daterangepicker').setEndDate(obj[$e.data("end")]);
                        } else {
                            $e.val("");
                        }
                        break;

                    case "date":
                        if (!!!$e.data('daterangepicker')) {
                            _c.Picker.Init($e, {
                                singleDatePicker: true,
                                timePicker: false,
                                locale: { format: 'YYYY/MM/DD' }
                            });
                        }
                        $e.data('daterangepicker').setStartDate(value || Date.now());
                        break;

                    case "disabled":
                        $e.on("change", function () {
                            const checked = $(this).data("direct") == "reverse"
                                ? !$(this).prop("checked")
                                : $(this).prop("checked");

                            if (checked) {
                                $(`#${$(this).data("target")}`).attr("disabled", "disabled").val("");
                            } else {
                                $(`#${$(this).data("target")}`).removeAttr("disabled");
                            }
                        });

                        if (!!$e.data("value")) {
                            let _v = $(`#${$e.data("target")}`).val();
                            if (typeof ($e.data("value")) == "number") _v = parseInt(_v);
                            else if (typeof ($e.data("value")) == "string") _v = _v.toString();

                            value = $e.data("direct") == "reverse"
                                ? !($e.data("value") == _v)
                                : $e.data("value") == _v;
                        }

                        $e.prop("checked", value);
                        $e.trigger("change");
                        break;

                    case "images":
                        if (!!!$e.data("init")) {
                            $e.ImageUploadModalClear();
                            $e.data("init", true);
                        }
                        co.File.getImgFile({
                            Sid: obj[$e.data("target")],
                            Type: $e.data("image-type"),
                            Size: $e.data("image-size")
                        }).done(function (file) {
                            if (file.length > 0) {
                                ImageUploadModalDataInsert($e, file[0].id, file[0].link, file[0].name);
                            }
                        });
                        break;

                    case "html":
                        $e.empty().html($("<div>").html(value).html());
                        break;

                    case "custom":
                        const setterName = $e.data("setter");
                        if (setterName && typeof window[setterName] === "function") {
                            window[setterName](value, $e, obj);
                        }
                        break;

                    default:
                        $e.val(_c.Form.formatElementValue($e, value));
                        break;
                }
            };

            for (const key in obj) {
                const $e = $self.find(`[name="${key}"]`);
                if ($e.length > 0) {
                    if (!!$e.data("form-type")) {
                        formTypeSet($e.data("form-type"), $e, obj[key]);
                    } else {
                        switch ($e[0].tagName) {
                            case "INPUT":
                                switch (($e.attr("type") || "").toLowerCase()) {
                                    case "radio":
                                        $self.find(`[name="${key}"][value="${obj[key]}"]`).prop("checked", true);
                                        break;

                                    case "checkbox":
                                        if (Array.isArray(obj[key])) {
                                            $(obj[key]).each(function (index, element) {
                                                $self.find(`[name="${key}"][value="${element}"]`).prop("checked", true);
                                            });
                                        } else {
                                            $self.find(`[name="${key}"][value="${obj[key]}"]`).prop("checked", true);
                                        }
                                        break;

                                    case "datetime-local":
                                        $e.val(co.Date.GetDateTimeStr(obj[key]));
                                        break;

                                    default:
                                        $e.val(_c.Form.formatElementValue($e, obj[key]));
                                        break;
                                }
                                break;

                            default:
                                $e.val(_c.Form.formatElementValue($e, obj[key]));
                                break;
                        }
                    }
                }
            }
        },

        normalizeElementValue: function (elementOrJq, value) {
            const $e = elementOrJq instanceof jQuery ? elementOrJq : $(elementOrJq);

            if (!$e.length) return value;
            if (value === null || value === undefined) return value;

            const inputType = String($e.attr("type") || "").toLowerCase();
            const originType = String($e.attr("data-origin-type") || "").toLowerCase();
            const formType = String($e.attr("data-form-type") || "").toLowerCase();

            if (Array.isArray(value)) {
                return value.map(v => _c.Form.normalizeElementValue($e, v));
            }

            if (formType === "boolean") {
                if (inputType === "checkbox") {
                    return value === true || value === "true" || value === "on" || value === "1";
                }

                if (inputType === "radio") {
                    if (value === "true") return true;
                    if (value === "false") return false;
                }
            }

            if (
                originType === "number" ||
                inputType === "number" ||
                formType === "number" ||
                formType === "number-format"
            ) {
                const raw = String(value).replace(/,/g, "").trim();
                if (raw === "") return null;
                return Number(raw);
            }

            return value;
        },

        formatElementValue: function (elementOrJq, value) {
            const $e = elementOrJq instanceof jQuery ? elementOrJq : $(elementOrJq);

            if (!$e.length) return value;
            if (value === null || value === undefined) return "";

            const inputType = String($e.attr("type") || "").toLowerCase();
            const originType = String($e.attr("data-origin-type") || "").toLowerCase();
            const formType = String($e.attr("data-form-type") || "").toLowerCase();

            if (
                originType === "number" ||
                inputType === "number" ||
                formType === "number" ||
                formType === "number-format"
            ) {
                const raw = String(value).replace(/,/g, "").trim();
                if (raw === "" || isNaN(raw)) return "";
                return Number(raw).toLocaleString();
            }

            return value;
        },

        bindNumberFormatter: function (elementOrJq) {
            const $input = elementOrJq instanceof jQuery ? elementOrJq : $(elementOrJq);
            if (!$input.length) return;

            const parseNumber = (val) => {
                if (val === null || val === undefined || val === "") return "";
                return String(val).replace(/,/g, "").trim();
            };

            $input.each(function () {
                const $e = $(this);

                if ($e.data("number-format-init")) return;
                $e.data("number-format-init", true);

                if (!$e.attr("data-origin-type")) {
                    $e.attr("data-origin-type", "number");
                }

                if (($e.attr("type") || "").toLowerCase() === "number") {
                    $e.attr("type", "text");
                }

                if (!$e.attr("inputmode")) {
                    $e.attr("inputmode", "numeric");
                }

                if (!$e.attr("data-form-type")) {
                    $e.attr("data-form-type", "number-format");
                }

                $e.on("focus.numberFormat", function () {
                    $e.val(parseNumber($e.val()));
                });

                $e.on("blur.numberFormat", function () {
                    $e.val(_c.Form.formatElementValue($e, $e.val()));
                });

                $e.on("input.numberFormat", function () {
                    let val = $e.val();
                    val = String(val).replace(/[^\d]/g, "");
                    $e.val(val);
                });

                const initVal = $e.val();
                if (initVal !== null && initVal !== undefined && initVal !== "") {
                    $e.val(_c.Form.formatElementValue($e, initVal));
                }
            });
        },

        initNumberFormatter: function (scope) {
            let $scope = null;

            if (!scope) $scope = $(document);
            else if (scope instanceof jQuery) $scope = scope;
            else $scope = $(scope);

            if (!$scope.length) return;

            _c.Form.bindNumberFormatter(
                $scope.find('input[type="number"], input[data-form-type="number"], input[data-form-type="number-format"]')
            );
        },

        getJson: function (id, isArrayType) {
            const form = document.getElementById(id);
            const $form = $(`#${id}`);
            const formFields = new FormData(form);
            let isArray = typeof (isArrayType) == "undefined" ? false : isArrayType;

            let formDataObject = Object.fromEntries(Array.from(formFields.keys(), key => {
                const values = formFields.getAll(key);
                const $field = $(`#${id}`).find(`[name="${key}"]`).first();

                return [
                    key,
                    (isArray || values.length > 1)
                        ? values.map(v => _c.Form.normalizeElementValue($field, v))
                        : _c.Form.normalizeElementValue($field, values.pop())
                ];
            }));

            let exItems = $(`#${id}`).find(`[name][data-form-type]`);
            exItems.each(function () {
                const $e = $(this);
                switch ($e.data("form-type")) {
                    case "zipcode":
                        formDataObject[$e.attr("name")] = co.Zipcode.getData($e);
                        break;

                    case "tags":
                        formDataObject[$e.attr("name")] = $e.find(".InputTag").data("tagList");
                        break;

                    case "custom":
                        const getterName = $e.data("getter");
                        if (getterName && typeof window[getterName] === "function") {
                            formDataObject[$e.attr("name")] = window[getterName]($e, formDataObject);
                        }
                        break;
                }
            });

            if (formDataObject.startEndDate) {
                const d = formDataObject.startEndDate.split("~");
                formDataObject.StartTime = d[0].trim();
                if (d.length > 1) formDataObject.EndTime = d[1].trim();
            }

            const checkboxNames = [...new Set(
                $form.find('input[type="checkbox"][name]')
                    .map(function () { return this.name; })
                    .get()
            )];

            checkboxNames.forEach(function (name) {
                const $items = $form.find(`input[type="checkbox"][name="${name}"]`);
                if ($items.length === 0) return;

                const formType = String($items.first().attr("data-form-type") || "").toLowerCase();

                if (formType === "boolean") {
                    if (typeof formDataObject[name] === "undefined") {
                        formDataObject[name] = false;
                    }
                    return;
                }

                if (typeof formDataObject[name] === "undefined") {
                    formDataObject[name] = [];
                }
            });

            return formDataObject;
        },

        getJsonByFieldset: function (id, isArrayType) {
            const fieldset = document.getElementById(id);
            const elements = fieldset.querySelectorAll('input, select, textarea');
            const fieldsetData = {};

            elements.forEach(element => {
                switch (element.type) {
                    case "checkbox":
                        fieldsetData[element.name] = element.checked;
                        break;
                    case "select-multiple":
                        fieldsetData[element.name] = Array.from(element.selectedOptions).map(option => option.value);
                        break;
                    default:
                        fieldsetData[element.name] = _c.Form.normalizeElementValue(element, element.value);
                        break;
                }
            });

            return fieldsetData;
        },

        init: function (id, fun) {
            const form = document.getElementById(id);
            if (!form) return;

            const parseNumber = (val) => {
                if (val === null || val === undefined || val === "") return "";
                return String(val).replace(/,/g, "").trim();
            };

            _c.Form.initNumberFormatter(form);

            form.addEventListener('submit', function (event) {
                event.preventDefault();

                if (form.dataset.submitting === "true") {
                    event.stopImmediatePropagation();
                    return;
                }

                const customValidity = $(form).find(".customValidity").get(0);
                if (customValidity) customValidity.setCustomValidity("");

                $(form).find('[data-origin-type="number"]').each(function () {
                    const $input = $(this);
                    $input.val(parseNumber($input.val()));
                });

                if (!form.checkValidity()) {
                    event.stopPropagation();
                    form.classList.add('was-validated');
                    return;
                }

                form.dataset.submitting = "true";
                form.classList.add('was-validated');

                let result = null;

                try {
                    result = fun && fun(id);
                } catch (error) {
                    form.dataset.submitting = "false";
                    throw error;
                }

                if (result && typeof result.always === "function") {
                    result.always(function () {
                        form.dataset.submitting = "false";
                    });
                    return;
                }

                if (result && typeof result.finally === "function") {
                    result.finally(function () {
                        form.dataset.submitting = "false";
                    });
                    return;
                }

                form.dataset.submitting = "false";
            }, false);
        },

        clear: function (id) {
            const form = document.getElementById(id);
            const $items = $(`[data-form-type]`);

            _c.Form.insertData(_c.Form.getJson(id), `#${id}`);

            $items.each(function (i, e) {
                const $e = $(e);
                switch ($e.data("form-type")) {
                    case "images":
                        if (!!!$e.data("init")) {
                            $e.ImageUploadModalClear();
                            $e.data("init", true);
                        } else {
                            $e.ImageUploadModalClear();
                        }
                        break;

                    case "date":
                        $e.data('daterangepicker').setStartDate(_c.Date.GetDateTimeStr(Date.now()));
                        $e.data('daterangepicker').setEndDate(null);
                        break;
                }
            });

            form.reset();
            if ($(form).find("[name='id']").length > 0) {
                $(form).find("[name='id']").val(0);
            }

            _c.Form.initNumberFormatter(form);
        },

        getFileForm: function (id, type = 0) {
            var formData = new FormData();
            formData.append("files", $(`#${id} .img_input`).data("file").File);
            formData.append("type", type);
            return formData;
        }
    }
});