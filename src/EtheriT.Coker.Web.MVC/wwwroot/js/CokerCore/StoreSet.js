(function (global, $) {
    "use strict";
    const FormType = {
        text: 1,
        textarea: 2,
        radio: 3,
        checkBox: 4,
        select: 5,
        date: 6,
        tel: 7,
        number: 8,
        email: 9,
        password: 10
    };
    function escHtml(s) {
        if (s === null || s === undefined) return "";
        return String(s)
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll('"', "&quot;")
            .replaceAll("'", "&#39;");
    }

    function buildAttrs(field) {
        let attrs = "";
        if (field.pattern) attrs += ` pattern="${escHtml(field.pattern)}"`;
        if (field.maxlength !== null && field.maxlength !== undefined) attrs += ` maxlength="${field.maxlength}"`;
        return attrs;
    }

    function buildHelpText(field) {
        if (!field.memo) return "";
        return `<div class="form-text">${escHtml(field.memo)}</div>`;
    }

    function buildGroupHeader(group) {
        const title = escHtml(group.title || "");
        const desc = escHtml(group.description || "");
        const img = group.image ? `<img src="${escHtml(group.image)}" alt="" class="img-fluid mb-2">` : "";

        return `
            <div class="mb-3">
                <div class="h5 mb-1">${title}</div>
                ${desc ? `<div class="text-muted">${desc}</div>` : ""}
                ${img}
            </div>
        `;
    }
    function resolveDefaultValue(field) {
        if (!Array.isArray(field.storeSetItems)) return null;
        const def = field.storeSetItems.find(i => i.isDefault === true);
        return def ? String(def.key) : null;
    }

    function resolveCurrentValue(field, valuesMap) {
        // valuesMap 代表「已儲存值」(稍後你會把 value 塞進來)
        if (valuesMap && valuesMap[field.key] !== undefined && valuesMap[field.key] !== null) {
            return String(valuesMap[field.key]);
        }
        return null;
    }

    // 最終用於 checked/selected 的有效值
    function resolveEffectiveValue(field, valuesMap) {
        return resolveCurrentValue(field, valuesMap) ?? resolveDefaultValue(field);
    }



    function buildField(field, valuesMap) {
        const key = field.key;                 // ex: "MemberRegister"
        const name = escHtml(field.name || "");
        const memo = buildHelpText(field);
        const attrs = buildAttrs(field);

        // 預設值：若你之後會回 storeSetDetails，可直接塞到 valuesMap
        const effectiveValue = resolveEffectiveValue(field, valuesMap);
        const currentValue = resolveCurrentValue(field, valuesMap);
        // 共用 wrapper（用 bootstrap 的 form-floating 你也可改）
        const idBase = `ss_${key}`;

        // text-like input types
        function input(type) {
            const valAttr = (effectiveValue !== null && effectiveValue !== undefined)
                ? ` value="${escHtml(effectiveValue)}"`
                : "";
            return `
                <div class="mb-3">
                    <label class="form-label" for="${idBase}">${name}</label>
                    <input type="${type}" class="form-control" id="${idBase}" name="${escHtml(key)}"${valAttr}${attrs}>
                    ${memo}
                </div> `;
        }


        switch (field.type) {
            case FormType.text: return input("text");
            case FormType.tel: return input("tel");
            case FormType.number: return input("number");
            case FormType.email: return input("email");
            case FormType.password: return input("password");
            case FormType.date: return input("date");
            case FormType.textarea: {
                const val = (effectiveValue !== null && effectiveValue !== undefined) ? escHtml(effectiveValue) : "";
                return `
                    <div class="mb-3">
                        <label class="form-label" for="${idBase}">${name}</label>
                        <textarea class="form-control" id="${idBase}" name="${escHtml(key)}"${attrs} rows="4">${val}</textarea>
                        ${memo}
                    </div>`;
            }


            case FormType.select: {
                const items = (field.storeSetItems || []).map(it => {
                    const k = String(it.key);
                    const v = escHtml(it.value);
                    const selected = (effectiveValue !== null && String(effectiveValue) === k) ? " selected" : "";
                    return `<option value="${escHtml(k)}"${selected}>${v}</option>`;
                }).join("");

                return `
                    <div class="mb-3">
                        <label class="form-label" for="${idBase}">${name}</label>
                        <select class="form-select" id="${idBase}" name="${escHtml(key)}">
                            ${items}
                        </select>
                        ${memo}
                    </div>
                `;
            }

            case FormType.radio: {
                const items = (field.storeSetItems || []).map((it, idx) => {
                    const k = String(it.key);
                    const v = escHtml(it.value);
                    const id = `${idBase}_${idx}`;
                    const checked = (effectiveValue !== null && String(effectiveValue) === k) ? " checked" : "";
                    return `
                        <div class="form-check">
                            <input class="form-check-input" type="radio" name="${escHtml(key)}" id="${id}" value="${escHtml(k)}"${checked}>
                            <label class="form-check-label" for="${id}">${v}</label>
                        </div>
                    `;
                }).join("");

                return `
                    <div class="mb-3">
                        <div class="form-label">${name}</div>
                        ${items}
                        ${memo}
                    </div>
                `;
            }

            case FormType.checkBox: {
                // checkbox 可能會是多選：用同一個 name="key[]" 收集
                // 若你的後端要的是單值，也可自行改成 name="key"
                const selectedSet = new Set(resolveEffectiveValues(field, valuesMap));
                if (currentValue !== null && currentValue !== undefined) {
                    // 支援 "1,3" 或 ["1","3"]
                    if (Array.isArray(currentValue)) currentValue.forEach(x => selectedSet.add(String(x)));
                    else String(currentValue).split(",").map(x => x.trim()).filter(Boolean).forEach(x => selectedSet.add(x));
                }

                const items = (field.storeSetItems || []).map((it, idx) => {
                    const k = String(it.key);
                    const v = escHtml(it.value);
                    const id = `${idBase}_${idx}`;
                    const checked = selectedSet.has(k) ? " checked" : "";
                    return `
                        <div class="form-check">
                            <input class="form-check-input" type="checkbox" name="${escHtml(key)}[]" id="${id}" value="${escHtml(k)}"${checked}>
                            <label class="form-check-label" for="${id}">${v}</label>
                        </div>
                    `;
                }).join("");

                return `
                    <div class="mb-3">
                        <div class="form-label">${name}</div>
                        ${items}
                        ${memo}
                    </div>
                `;
            }

            default:
                return `
                    <div class="alert alert-warning">
                        Unsupported field type: ${escHtml(field.type)} (key=${escHtml(key)})
                    </div>
                `;
        }
    }

    function renderForm($form, result) {
        const groups = result.storeGroups || [];
        const valuesMap = {};

        // 依你後端慣例：storeSetDetails 內通常是 { key: "...", value: [...] }
        const details = Array.isArray(result.storeSetDetails) ? result.storeSetDetails : [];

        details.forEach(d => {
            if (!d || !d.key) return;

            // 你後端可能會回 [""] 代表未設定（你之前提過）
            // 這裡把 [""] 視為「沒有值」，不寫入 valuesMap，才能保留 defaultValue
            const v = d.value;

            if (v == null) return;

            if (Array.isArray(v)) {
                if (v.length === 0) return;
                if (v.length === 1 && (v[0] === "" || v[0] == null)) return;
                // textarea 通常是 ["整段字"]；select/radio 是 ["1"]；checkbox 是 ["1","3"]
                valuesMap[d.key] = v.length === 1 ? String(v[0]) : v.map(x => String(x));
                return;
            }

            // 若後端回單一字串（防禦性）
            if (typeof v === "string") {
                if (v.trim() === "") return;
                valuesMap[d.key] = v;
                return;
            }

            valuesMap[d.key] = v;
        });

        const isFullRowType = (t) =>
            t === FormType.textarea ||
            t === FormType.radio ||
            t === FormType.checkBox;

        // 你要的「依類別排序」：可自行調整順序
        const TYPE_ORDER = [
            FormType.text,      // 1
            FormType.email,     // 9
            FormType.tel,       // 7
            FormType.number,    // 8
            FormType.date,      // 6
            FormType.password,   // 10
            FormType.select,    // 5
            FormType.textarea,  // 2
            FormType.radio,     // 3
            FormType.checkBox,  // 4
        ];
        const typeRank = (t) => {
            const i = TYPE_ORDER.indexOf(t);
            return i === -1 ? 999 : i;
        };

        let html = "";

        groups.forEach(group => {
            html += `<section class="border rounded p-3 mb-3">`;
            html += buildGroupHeader(group);

            // 先排序，再渲染
            const fields = (group.storeSets || []).slice().sort((a, b) => {
                const ra = typeRank(a.type), rb = typeRank(b.type);
                if (ra !== rb) return ra - rb;

                const na = String(a.name || ""), nb = String(b.name || "");
                const nc = na.localeCompare(nb, "zh-Hant");
                if (nc !== 0) return nc;

                return String(a.key || "").localeCompare(String(b.key || ""));
            });

            let rowOpen = false;

            fields.forEach(field => {
                const fullRow = isFullRowType(field.type);

                // 獨佔列：先收掉一般 row
                if (fullRow) {
                    if (rowOpen) {
                        html += `</div>`;
                        rowOpen = false;
                    }

                    html += `<div class="row g-3">`;
                    html += `<div class="col-12">`;
                    html += buildField(field, valuesMap);
                    html += `</div>`;
                    html += `</div>`;
                    return;
                }

                // 一般欄位：用 .col 自動均分
                if (!rowOpen) {
                    html += `<div class="row g-3">`;
                    rowOpen = true;
                }

                html += `<div class="col">`;
                html += buildField(field, valuesMap);
                html += `</div>`;
            });

            if (rowOpen) html += `</div>`;

            html += `</section>`;
        });

        $form.empty().append(html);
    }
    function resolveDefaultValue(field) {
        // 1) 有選項的欄位（select/radio/checkbox）：優先用 items 的 isDefault
        if (Array.isArray(field.storeSetItems) && field.storeSetItems.length) {
            const def = field.storeSetItems.find(i => i.isDefault === true);
            if (def) return String(def.key);
        }

        // 2) 無選項的欄位（text/textarea/...）：用 StoreSet.defaultValue
        if (field.defaultValue !== null && field.defaultValue !== undefined) {
            const dv = String(field.defaultValue);
            // 如果你希望空字串也算 default，就把這段 trim 判斷拿掉
            if (dv.trim() !== "") return dv;
        }

        return null;
    }


    function resolveCurrentValues(field, valuesMap) {
        if (!valuesMap || valuesMap[field.key] == null) return null;

        const v = valuesMap[field.key];
        if (Array.isArray(v)) return v.map(x => String(x));
        return String(v).split(",").map(x => x.trim()).filter(Boolean);
    }
    function resolveEffectiveValues(field, valuesMap) {
        return resolveCurrentValues(field, valuesMap) ?? resolveDefaultValues(field);
    }

    Coker.extend({
        StoreSet: {
            GetValues: function (data) {
                return $.ajax({
                    url: "/api/StoreSet/getValues/",
                    type: "POST",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: JSON.stringify(data),
                    dataType: "json"
                });
            },
            SaveValues: function (data) {
                return $.ajax({
                    url: "/api/StoreSet/CreateOrUpdate",
                    type: "POST",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: JSON.stringify(data),
                    dataType: "json"
                });
            },
            getGroupStructure: function (data) {
                return $.ajax({
                    url: "/api/StoreSet/getGroupStructure",
                    type: "POST",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: JSON.stringify(data),
                    dataType: "json"
                });
            },
            CreateFrom: function (formId) {
                var _dfr = $.Deferred();
                const $form = $(`#${formId}`);
                const groupId = $form.data("groupid");

                $.when(
                    co.StoreSet.getGroupStructure({ StoreSetGroupId: groupId }),
                    co.StoreSet.GetValues({ StoreSetGroupId: groupId })
                ).done(function (r1, r2) {
                    const structure = r1[0];
                    const values = r2[0];

                    if (!structure || structure.success !== true) {
                        $form.html(`<div class="alert alert-danger">載入設定失敗</div>`);
                        return;
                    }

                    // 把 valuesMap 掛在 structure 上，讓 renderForm 內部吃得到
                    structure.storeSetDetails = values && values.success === true ? (values.storeSetDetails || []) : [];

                    renderForm($form, structure);
                    _dfr.resolve();
                }).fail(function () {
                    $form.html(`<div class="alert alert-danger">載入設定失敗（網路或伺服器錯誤）</div>`);
                });

                return _dfr.promise();
            }
        }
    });
})(window, jQuery);