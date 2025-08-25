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
function jqueryExtend() {
    $.fn.extend({
        imgCheck: function () {
            var $self = $(this);
            $self.each(function (i, item) {
                $(item).on("error", function () {
                    FileApi.insertNotFondFile({ Url: $(item).attr("src"), FK_WebsiteID: typeof (SiteId) == "undefined" ? 0 : SiteId });
                    $(item).attr("src", "/images/noImg.jpg");
                })
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
                                    const lbl = $(this).nextAll("label").text().trim();
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
