Coker.Object.merge(Coker.File,{
    ListFile:function($self) {
        /***************
         uploadtype：
            1 = 圖片;
            2 = 360;
            3 = 影片;
            4 = Youtube;
            5 = file (no view)
        ***************/
        var $parent = $self.parents(".data_upload").first();
        const $previewFrame = $parent.find(".preview_frame");
        function previewFrameClear() {
            $previewFrame.find(".default_frame").addClass("d-flex");
            $previewFrame.find(".upload_frame").addClass("d-none");
            $previewFrame.find(".media_frame").removeClass("d-flex");
            $previewFrame.find(".youtube_frame").removeClass("d-flex");
            $previewFrame.find(".select_frame").removeClass("d-flex");
        }
        previewFrameClear();
        if ($self.data("edit")) {
            $self.data("edit", false)
            if ($self.hasClass("upload_list") && $self.find(".title").text() == "") {
                $self.remove();
                file_num -= 1;
            }
        } else {
            if ($self.hasClass("upload_list") && $self.find(".title").text() != "") {
                $(".upload_list").each(function () {
                    var $li_self = $(this);
                    if ($li_self.hasClass("upload_list") && $li_self.find(".title").text() == "") {
                        $li_self.remove();
                        file_num = $self.siblings(".upload_list").length + 1;
                    }
                })
            }
            upload_file = null;
            $parent.find(".upload_frame").children("*").remove();
            $(".upload_list").each(function () {
                $(this).data("edit", false);
            })
            $self.data("edit", true)
            $parent.find(".default_frame").removeClass("d-flex");

            switch ($self.data("uploadtype")) {
                case 0:
                    var $select_frame = $parent.find(".select_frame")
                    $select_frame.addClass("d-flex");
                    $select_frame.find("button").each(function () {
                        $(this).on("click", function (e) {
                            e.preventDefault();
                            if ($self.data("uploadtype") == 0) {
                                $self.data("uploadtype", $(this).data("uploadtype"));
                                $self.data("edit", false);
                                UploadFile($self);
                            }
                        })
                    })
                    break;
                case 1:
                    if ($self.find(".title").text() == "") {
                        upload_file = co.File.UploadImageInit("FileUpload");
                        $parent.find(".upload_frame").removeClass("d-none");
                    } else {
                        if (typeof ($self.data("id")) != "undefined") {
                            var name = total_files.find(item => item["Id"] == $self.data("id"))["Name"];
                            var file = total_files.find(item => item["Id"] == $self.data("id"))["File"];
                            $parent.find(".media_frame").addClass("d-flex");
                            $parent.find(".media_frame").find("input").val(name);
                            $parent.find(".media_preview > div").children().remove();
                            $parent.find(".media_preview > div").children().remove();
                            $parent.find(".media_preview > div").append(`<img src="${file}" class=""></img>`);
                        } else if (typeof ($self.data("tempid")) != "undefined") {
                            var data = total_files.find(item => item["TempId"] == $self.data("tempid"));
                            if (typeof (data) != "undefined") {
                                $parent.find(".upload_frame").find("span").text(data["File"].name);
                                $parent.find(".media_frame").find("input").val(data["Name"]);
                                $parent.find(".media_preview > div").children().remove();
                                var link = data["Link"];
                                $parent.find(".media_preview > div").append(`<img src="${link}" class=""></img>`);
                            }
                            $parent.find(".media_frame").addClass("d-flex");
                        }
                    }
                    break;
                 /* ********** *****************
                須重打，360顯示的部分
                ***************************/
                case 2:
                    upload_file = co.File.Upload360Init("FileUpload");
                    if ($self.data("file")) {
                        upload_file.addFiles($self.data("file"));
                        //console.log(upload_file);
                        $parent.find(".upload_frame").find("span").text($self.data("file").length + " 張圖片已選擇");
                    }
                    $parent.find(".upload_frame").removeClass("d-none");
                    break;
                 /* ********** *****************
               須重打，影片顯示的部分
               ***************************/
                case 3:
                    if ($self.find(".title").text() == "") {
                        upload_file = co.File.UploadVideoInit("FileUpload");
                        $parent.find(".upload_frame").removeClass("d-none");
                    } else {
                        if (typeof ($self.data("id")) != "undefined") {
                            var name = total_files.find(item => item["Id"] == $self.data("id"))["Name"];
                            var file = total_files.find(item => item["Id"] == $self.data("id"))["File"];
                            $parent.find(".media_frame").addClass("d-flex");
                            $parent.find(".media_frame").find("input").val(name);
                            $parent.find(".media_preview > div").children().remove();
                            $parent.find(".media_preview > div").append(`<video src="${file}" class="h-100 w-100" controls preload="metadata"></video>`);
                        } else if (typeof ($self.data("tempid")) != "undefined") {
                            var data = total_files.find(item => item["TempId"] == $self.data("tempid"));
                            var file;
                            if (typeof (data) != "undefined") {
                                file = total_files.find(item => item["TempId"] == $self.data("tempid"))["File"];
                                $parent.find(".upload_frame").find("span").text(file.name);
                                $parent.find(".media_frame").find("input").val(total_files.find(item => item["TempId"] == $self.data("tempid"))["Name"]);
                            }
                            $parent.find(".media_frame").addClass("d-flex");
                        }
                    }
                    break;
                case 4:
                    if (typeof (total_files.find(item => item["Id"] == $self.data("id"))) != "undefined") {
                        var file = total_files.find(item => item["Id"] == $self.data("id"))["File"];
                        var url = "https://www.youtube.com/watch?v=" + file;
                        $parent.find(".youtube_frame").find("input").val(url);
                        $("#BtnConnect").click();
                    } else if (typeof (total_files.find(item => item["TempId"] == $self.data("tempid"))) != "undefined") {
                        var file = total_files.find(item => item["TempId"] == $self.data("tempid"))["File"];
                        var url = "https://www.youtube.com/watch?v=" + file;
                        $parent.find(".youtube_frame").find("input").val(url);
                        $("#BtnConnect").click();
                    } else {
                        $parent.find(".youtube_frame").find("input").val("https://www.youtube.com/watch?v=");
                        var error_html = "<div class='w-100 h-100 d-flex justify-content-center align-items-center bg-black bg-opacity-25 fw-bold'>請輸入正確的Youtube連結</div>"
                        $(".youtube_preview").children("*").remove();
                        $(".youtube_preview").append(error_html);
                    }
                    $parent.find(".youtube_frame").addClass("d-flex");
                    break;
            }
        }
    }
});
(function($){
    $.fn.extend({
        
    });
})(jQuery)