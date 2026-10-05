var FileApi = {
    insertNotFondFile: function (data) {
        data.from = location.href;
        return $.ajax({
            url: "/api/File/insertNotFondFile",
            type: "POST",
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(data),
            dataType: "json"
        });
    }
}
// Per-page diagnostics only: do not create extra image/HEAD requests.
var imageCheckReported = new Set();
var imageCheckSucceeded = new Set();
function rememberImageCheck(cache, source) {
    if (cache.has(source)) return;
    if (cache.size >= 500) cache.delete(cache.values().next().value);
    cache.add(source);
}
function jqueryExtend() {
    $.fn.extend({
        imgCheck: function () {
            var $self = $(this);
            $self.each(function (i, item) {
                var $image = $(item);
                var previous = $image.data("cokerImageCheck");
                if (previous) clearTimeout(previous.timer);
                var state = { timer: null, pending: null };
                $image.data("cokerImageCheck", state);
                function sourceKey(source) {
                    if (!source || /^(data|blob):/i.test(source)) return "";
                    try {
                        var url = new URL(source, document.baseURI);
                        if (!/^https?:$/.test(url.protocol) ||
                            /\/(noimg\.jpg|directory-loading\.svg)$/i.test(url.pathname)) return "";
                        return url.href;
                    } catch (_) { return ""; }
                }
                function cancel() {
                    clearTimeout(state.timer);
                    state.timer = null;
                    state.pending = null;
                }
                function schedule(source, placeholder) {
                    var key = sourceKey(source);
                    cancel();
                    if (!key || imageCheckReported.has(key) || imageCheckSucceeded.has(key)) return;
                    var pending = { key: key, source: source, placeholder: placeholder };
                    state.pending = pending;
                    state.timer = setTimeout(function () {
                        state.timer = null;
                        if (state.pending !== pending || !item.isConnected || imageCheckSucceeded.has(key)) return;
                        var current = sourceKey(item.currentSrc || item.getAttribute("src"));
                        var displayed = item.getAttribute("src") || "";
                        var isLazyFallback = placeholder != null && displayed === placeholder;
                        if (current !== key && !isLazyFallback) return;
                        if (current === key && item.naturalWidth > 0) return;
                        if (!isLazyFallback && !item.complete) return;
                        if (imageCheckReported.has(key)) return;
                        rememberImageCheck(imageCheckReported, key);
                        FileApi.insertNotFondFile({ Url: source, FK_WebsiteID: typeof (SiteId) == "undefined" ? 0 : SiteId });
                        state.pending = null;
                        if (!isLazyFallback && displayed === source) item.setAttribute("src", "/images/noImg.jpg");
                    }, 3000);
                }
                // Namespaced handlers make repeated imgCheck calls idempotent.
                $image.off(".cokerImageCheck")
                    .on("coker:image-reset.cokerImageCheck", cancel)
                    .on("coker:image-error.cokerImageCheck", function (event) {
                        var failure = event.originalEvent.detail;
                        schedule(failure.source, failure.placeholder);
                    })
                    .on("load.cokerImageCheck", function () {
                        var key = sourceKey(item.currentSrc || item.getAttribute("src"));
                        if (!key || item.naturalWidth <= 0) return;
                        rememberImageCheck(imageCheckSucceeded, key);
                        if (state.pending && state.pending.key === key) cancel();
                    })
                    .on("error.cokerImageCheck", function () {
                        if (item.cokerLazyImageManaged) return;
                        var declared = item.getAttribute("src") || "";
                        var actual = item.currentSrc || declared;
                        schedule(sourceKey(actual) === sourceKey(declared) ? declared : actual, null);
                    });
                if (item.complete && item.naturalWidth > 0) {
                    var loaded = sourceKey(item.currentSrc || item.getAttribute("src"));
                    if (loaded) rememberImageCheck(imageCheckSucceeded, loaded);
                }
            });
            return $self;
        }, changeTagName: function (newTag) {
            let newElements = [];
            this.each(function () {
                let $oldElement = $(this);
                let $newElement = $(`<${newTag}>`);

                // 複製所有屬性
                $.each(this.attributes, function () {
                    $newElement.attr(this.name, this.value);
                });

                // 複製內容
                $newElement.html($oldElement.html());

                // 替換舊的元素
                $oldElement.replaceWith($newElement);

                // 保存新元素以便返回
                newElements.push($newElement[0]);
            });

            // 返回新元素的 jQuery 物件
            return $(newElements);
        },
        getFormJson: function () {
            const form = $(this);
            const excludedNames = [];
            const handledNames = [];
            const formDataObject = $(form).serializeArray();

            $(formDataObject).each(function () {
                const obj = this;
                const field = $(form).find(`[name="${obj.name}"]`);
                const getTitleNearby = function(field) {
                    let wrapper = field.closest('.d-flex');

                    while (wrapper.length) {
                        const title = wrapper.prevAll('.title').first().text().trim();
                        if (title) return title;
                        wrapper = wrapper.parent().closest('.d-flex');
                    }

                    return '';
                }
                // 排除在隱藏容器（data-show-on + .d-none）中的欄位
                const hiddenContainer = field.closest('[data-show-on].d-none');
                if (hiddenContainer.length || !obj.name || excludedNames.includes(obj.name) || handledNames.includes(obj.name)) {
                    obj.ignore = true;
                    return;
                }
                obj.title = field.closest('.form-floating').find('.title').first().text().trim() ||
                    field.closest('.form-floating').find('label').first().text().trim() || "";
                // 控制 title 與 value
                switch (field.prop("tagName")) {
                    case "SELECT":
                        obj.value = field.find("option:selected").text().trim();
                        break;
                    case "INPUT":
                        switch (field.attr("type")) {
                            case "radio":
                                const checked = $(form).find(`input[name="${obj.name}"]:checked`);
                                const label = $(form).find(`label[for="${checked.attr("id")}"]`).text().trim();
                                obj.title = getTitleNearby(checked) || "";
                                obj.value = label;

                                // 若有補充輸入框（如 text 緊跟在 radio 後）
                                const extraInput = checked.closest('.form-check, .d-flex').find('input[type="text"]');
                                if (extraInput.length && extraInput.val().trim()) {
                                    obj.value += `：${extraInput.val().trim()}`;
                                    excludedNames.push(extraInput.attr("name"));
                                }
                                break;
                            case "checkbox":
                                obj.title = field.closest('.d-flex').prevAll('.title').first().text().trim() ||
                                    field.closest('.d-flex').prevAll('label').first().text().trim() || "";
                                obj.value = '';

                                $(form).find(`[name="${obj.name}"]:checked`).each(function () {
                                    const $checked = $(this);
                                    let lbl = $checked.nextAll("label").first().text().trim();
                                    const extraInput = $checked.closest('.form-check, .checkbox_input_text').find('input[type="text"]');

                                    if (extraInput.length && extraInput.val().trim()) {
                                        lbl += `：${extraInput.val().trim()}`;
                                        excludedNames.push(extraInput.attr("name"));
                                    }

                                    obj.value += lbl + " ,";
                                });

                                obj.value = obj.value.replace(/ ,$/, '');
                                break;

                            default:
                                obj.value = field.val().trim();
                                break;
                        }
                        break;
                    default:
                        obj.value = field.val().trim();
                        break;
                }
                if (obj.value === "captcha") obj.title = "";
                handledNames.push(obj.name);
            });
            return formDataObject.filter(f => !f.ignore);
        }
    });
    $.extend({
        htmlDecode: function (encodedString) {
            var textArea = document.createElement('textarea');
            textArea.innerHTML = encodedString;
            const isHtml = /<[^>/]+>/.test(textArea.value.replace(`<div class="container">`, ""));
            return isHtml ? textArea.value : textArea.value.replace(/\n/g, "<br />");
        },
        loadCss: function (src) {
            const _dfr = $.Deferred();
            let head = document.getElementsByTagName('HEAD')[0];

            // Create new link Element
            let link = document.createElement('link');

            // set the attributes for link element
            link.rel = 'stylesheet';

            link.type = 'text/css';

            link.href = src;
            link.onload = function () {
                _dfr.resolve();
            };
            // Append link element to HTML head
            head.appendChild(link);
            return _dfr.promise();
        },
        LoadJs: function (src) {
            const _dfr = $.Deferred();
            let head = document.getElementsByTagName('HEAD')[0];
            let link = document.createElement('script');
            link.type = 'text/javascript';
            link.src = src;

            if (/.mjs$/.test(src)) {
                link.type = "module";
            }
            link.onload = function () {
                _dfr.resolve();
            };
            link.onerror = function () {
                _dfr.reject(new Error(`Failed to load script: ${src}`));
            };
            // Append link element to HTML head
            head.appendChild(link);
            return _dfr.promise();
        }
    });
}

if (!String.prototype.format) {
    String.prototype.format = function () {
        var args = arguments;
        return this.replace(/{(\d+)}/g, function (match, number) {
            return typeof args[number] != 'undefined'
                ? args[number]
                : match
                ;
        });
    };
}
if (!Storage.prototype.isNullOrEmpty) {
    Storage.prototype.isNullOrEmpty = function (key) {
        const value = this.getItem(key);
        return value === null || value === "";
    };
}

if (window.jQuery && typeof jqueryExtend === "function") {
    jqueryExtend();
}
