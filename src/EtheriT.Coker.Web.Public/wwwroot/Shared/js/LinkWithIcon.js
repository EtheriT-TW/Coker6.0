function LinkWithIconInit() {
    $(".link_with_icon").each(function () {
        var $self = $(this)
        var data_url = $self.attr("href");
        if (typeof ($self.data("old_href")) != "undefined" && $self.data("old_href") == data_url) return true;
        if (typeof (data_url) != "undefined") {
            if (data_url.indexOf("?v=") > -1) {
                data_url = data_url.substring(0, data_url.indexOf("?v="));
                $self.attr("href", data_url)
            }

            var type = typeof ($self.data("extension")) == "undefined" ? data_url.substring(data_url.lastIndexOf('.') + 1, data_url.length) : $self.data("extension");
            type = String(type || "").split(/[?#]/)[0].toLowerCase();
            //if (type == "" && typeof ($self.data("extension")) != "undefined") type = $self.data("extension");
            var normalizedHref = $self.attr("data-file-normalized-href");
            var normalizedExtension = $self.attr("data-file-icon-extension") || "file";
            var expectedIconClass = GetLinkWithIconClass(type);
            if (normalizedHref === data_url &&
                normalizedExtension === (type || "file") &&
                $self.find(".icon i").hasClass(expectedIconClass)) {
                $self.data("old_href", data_url);
                return true;
            }

            $self.find(".icon").empty();
            switch (type) {
                case "jpg":
                case "jpeg":
                case "png":
                case "gif":
                case "avif":
                    $self.find(".icon").append('<i class="fa-solid fa-file-image"></i>');
                    break;
                case "pdf":
                    $self.find(".icon").append('<i class="fa-solid fa-file-pdf"></i>');
                    break;
                case "doc":
                case "docx":
                case "odt":
                    $self.find(".icon").append('<i class="fa-solid fa-file-word"></i>');
                    break;
                case "ppt":
                case "pptx":
                case "odp":
                    $self.find(".icon").append('<i class="fa-solid fa-file-powerpoint"></i>');
                    break;
                case "xls":
                case "xlsx":
                case "ods":
                    $self.find(".icon").append('<i class="fa-solid fa-file-excel"></i>');
                    break;
                case "zip":
                case "rar":
                    $self.find(".icon").append('<i class="fa-solid fa-file-zipper"></i>');
                    break;
                default:
                    $self.find(".icon").append('<i class="fa-solid fa-file"></i>');
                    break;
            }
        } else {
            $self.find(".icon").empty();
            $self.find(".icon").append('<i class="fa-solid fa-file"></i>');
        }
        if (typeof ($self.attr("download")) == "undefined") {
            $self.attr("download", $self.attr("data-file-name") || local.UnnamedFile);
        }
        $self.attr("title", local.LinkToAndBlank.format($self.attr("download")));
        if (!$self.hasClass("do_not_rename")) {
            $self.find(".name").text($self.attr("download").replace(`.${type}`, ""));
            if (type == "pdf")
                $self.attr({ target: "_blank", rel: "noopener noreferrer" }).removeAttr("download");
            else if (!(new RegExp(`[\.]{1}${type}$`, "gi")).test($self.attr("download"))) $self.attr("download", `${$self.attr("download")}.${type}`);
            else $self.attr("download", `${$self.attr("download")}`);
        }
        $self.data("old_href", data_url);
    })
}

function GetLinkWithIconClass(type) {
    if (["jpg", "jpeg", "png", "gif", "avif"].indexOf(type) > -1) return "fa-file-image";
    if (type == "pdf") return "fa-file-pdf";
    if (["doc", "docx", "odt"].indexOf(type) > -1) return "fa-file-word";
    if (["ppt", "pptx", "odp"].indexOf(type) > -1) return "fa-file-powerpoint";
    if (["xls", "xlsx", "ods"].indexOf(type) > -1) return "fa-file-excel";
    if (["zip", "rar"].indexOf(type) > -1) return "fa-file-zipper";
    return "fa-file";
}
