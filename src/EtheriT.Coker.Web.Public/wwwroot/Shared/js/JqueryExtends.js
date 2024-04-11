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
        }
    });
    $.extend({
        htmlDecode: function (encodedString) {
            var textArea = document.createElement('textarea');
            textArea.innerHTML = encodedString;
            return textArea.value.replace(/\n/g, "<br />");
        }
    });
}
