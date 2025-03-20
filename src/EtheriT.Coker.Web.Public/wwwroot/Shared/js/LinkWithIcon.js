function LinkWithIconInit() {
    $(".link_with_icon").each(function () {
        var $self = $(this)
        var data_url = $self.attr("href");
        if (typeof ($self.data("old_href")) != "undefined" && $self.data("old_href") == data_url) return true;
        $self.find(".icon").empty();
        if (typeof (data_url) != "undefined") {
            if (data_url.indexOf("?v=") > -1) {
                data_url = data_url.substring(0, data_url.indexOf("?v="));
                $self.attr("href", data_url)
            }
            var type = data_url.substring(data_url.lastIndexOf('.') + 1, data_url.length);
            switch (type) {
                case "jpg":
                case "jpeg":
                case "png":
                case "gif":
                case "avif":
                    $self.find(".icon").append('<i class="fa-solid fa-file-image font-yellow"></i>');
                    break;
                case "pdf":
                    $self.find(".icon").append('<i class="fa-solid fa-file-pdf font-rad"></i>');
                    break;
                case "doc":
                case "docx":
                case "odt":
                    $self.find(".icon").append('<i class="fa-solid fa-file-word font-blue"></i>');
                    break;
                case "ppt":
                case "pptx":
                case "odp":
                    $self.find(".icon").append('<i class="fa-solid fa-file-powerpoint font-orange"></i>');
                    break;
                case "xls":
                case "xlsx":
                case "ods":
                    $self.find(".icon").append('<i class="fa-solid fa-file-excel font-green"></i>');
                    break;
                case "zip":
                case "rar":
                    $self.find(".icon").append('<i class="fa-solid fa-file-zipper"></i>');
                    break;
                default:
                    $self.find(".icon").append('<i class="fa-solid fa-file font-gray"></i>');
                    break;
            }
        } else {
            $self.find(".icon").append('<i class="fa-solid fa-file font-gray"></i>');
        }
        if (typeof ($self.attr("download")) == "undefined") {
            $self.attr("download", local.UnnamedFile);
        }
        $self.attr("title", local.LinkToAndBlank.format($self.attr("download")));
        $self.find(".name").text($self.attr("download").replace(`.${type}`, ""));
        if (type == "pdf")
            $self.attr({ target: "_blank" }).removeAttr("download");
        else if (!(new RegExp(`[\.]{1}${type}$`, "gi")).test($self.attr("download"))) $self.attr("download", `${$self.attr("download")}.${type}`);
        else $self.attr("download", `${$self.attr("download")}`);
        $self.data("old_href", data_url);
    })
}