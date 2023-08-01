function LinkWithIconInit() {
    $(".link_with_icon").each(function () {
        var $self = $(this)
        var data_url = $self.attr("href");
        $self.find(".icon").empty();
        if (typeof (data_url) != "undefined") {
            var type = data_url.substring(data_url.lastIndexOf('.') + 1, data_url.length);
            switch (type) {
                case "jpg":
                case "jpeg":
                case "png":
                case "gif":
                    $self.find(".icon").append('<i class="fa-solid fa-file-image" style="color: #ff9500;"></i>');
                    break;
                case "pdf":
                    $self.find(".icon").append('<i class="fa-solid fa-file-pdf" style="color: #b30b00;"></i>');
                    break;
                case "doc":
                case "docx":
                case "odt":
                    $self.find(".icon").append('<i class="fa-solid fa-file-word" style="color: #2c599d;"></i>');
                    break;
                case "ppt":
                case "pptx":
                case "odp":
                    $self.find(".icon").append('<i class="fa-solid fa-file-powerpoint" style="color: #D04323;"></i>');
                case "xls":
                case "xlsx":
                case "ods":
                    $self.find(".icon").append('<i class="fa-solid fa-file-excel" style="color: #037945;"></i>');
                    break;
                case "zip":
                case "rar":
                    $self.find(".icon").append('<i class="fa-solid fa-file-zipper"></i>');
                    break;
                default:
                    $self.find(".icon").append('<i class="fa-solid fa-file" style="color: #999999;"></i>');
                    break;
            }
        }
        if (typeof ($self.attr("download")) == "undefined") {
            $self.attr("download", "未命名");
        }
        $self.find(".name").text($self.attr("download"));
        if (!(new RegExp(`${type}$`,"gi")).test($self.attr("download"))) $self.attr("download", `${$self.attr("download")}.${type}`);
    })
}