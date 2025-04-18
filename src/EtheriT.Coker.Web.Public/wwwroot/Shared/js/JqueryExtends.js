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
