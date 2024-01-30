$.fn.extend({
    ImageUploadModalClear: function () {
        var $select = $(this);
        $select.find(".img_input_frame").data("delectList", null);
        $select.find(".img_input_frame").children().each(function () {
            var $self = $(this);
            if (!$self.is("template")) $self.remove();
        })
        ImageSetData($select, null);
    }
});
function ImageUploadModalClear($select) {
    $select.find(".img_input_frame").data("delectList", null);
    $select.find(".img_input_frame").children().each(function () {
        var $self = $(this);
        if (!$self.is("template")) $self.remove();
    });
    ImageSetData($select, null);
}
var ImageUploadModalInit = ImageUploadModalClear;

function ImageDelect($select) {
    var $img_btn = $select.find(".btn_input_pic");
    if ($img_btn.hasClass("has_image")) {
        if ($select.data("file").id > 0) {
            var delect_list = [];
            if (typeof ($select.parent(".img_input_frame").data("delectList")) != "undefined" && $select.parent(".img_input_frame").data("delectList") != null) delect_list = $select.parent(".img_input_frame").data("delectList");
            delect_list.push($select.data("file").id);
            $select.parent(".img_input_frame").data("delectList", delect_list);
        }
        ImageClear($select);
    }
}
function ImageClear($select) {
    var $img_btn = $select.find(".btn_input_pic");
    var $parent_frame = $select.parents(".img_input_frame")
    if ($img_btn.hasClass("has_image")) {
        if (!$parent_frame.parent().first().data("issinge")) $select.remove();
        else {
            $img_btn.removeClass("has_image");
            $select.data("file", null);

            var $img_preview = $select.find(".img_preview");
            $img_preview.addClass("d-none");
            $img_preview.removeClass("d-none");
            $img_preview.siblings("span").removeClass("d-none");
            $img_preview.parents("button").first().removeClass("border-0");
            $img_preview.attr("src", "");
            $img_preview.attr("alt", "");

            var $file_name = $select.find(".file_name");
            $select.find(".btn_img_delete").addClass("d-none");
            $file_name.text("加入照片");
        }
    }
}

function ImageUploadModalDataInsert($select, id, link, name) {
    if (id != "") {
        var obj = {};
        obj["id"] = id;
        obj["File"] = null;
        obj["name"] = name;
        obj["link"] = link;
        ImageSetData($select, obj);
    }
}

function ImageSetData($select, file) {
    var isSingle = $select.data("issinge");
    var needCompress = $select.data("needcompress");

    if (file != null && isSingle) var input_frame = $select.find(".img_input");
    else if ($select.hasClass("img_input")) var input_frame = $select;
    else input_frame = $($("#Template_Image_Preview").html()).clone();
    if (file != null) {
        input_frame.find(".btn_img_delete").removeClass("d-none");
        if (typeof (input_frame.data("file")) != "undefined" && input_frame.data("file") != null) {
            var $parent_frame = input_frame.parent(".img_input_frame")
            var temp_delect_list = typeof ($parent_frame.data("delectList")) == "undefined" || $parent_frame.data("delectList") == null ? [] : $parent_frame.data("delectList");
            temp_delect_list.push(input_frame.data("file").id)
            $parent_frame.data("delectList", temp_delect_list);
        }
        input_frame.data("file", file);

        var $img_preview = input_frame.find(".img_preview");
        $img_preview.removeClass("d-none");
        $img_preview.siblings("span").addClass("d-none");
        $img_preview.parents("button").first().addClass("border-0");
        $img_preview.attr("src", !!file.link ? file.link: file.path);
        $img_preview.attr("alt", file.name);

        var $img_btn = input_frame.find(".btn_input_pic");
        $img_btn.addClass("has_image");

        input_frame.find(".file_name").text(file.name);
    }

    if (!isSingle || file == null) {
        if (!isSingle && file == null) input_frame.find(".input_pic").each(function () { $(this).attr("multiple", "multiple") })
        input_frame.find(".btn_input_pic").on("click", function (even) {
            even.preventDefault();
            $(this).parents("div").first().prev(".input_pic").click();
        });
        input_frame.find(".btn_img_delete").on("click", function (even) {
            even.preventDefault();
            ImageDelect($(this).parents(".img_input").first());
        });
        input_frame.find(".input_pic").change(function () {
            var $select_input = $(this).parent(".img_input");
            if (typeof ($select_input.data("file")) != "undefined") $select = $select_input;
            $.each(this.files, function (index, file) {
                if (needCompress) {
                    var reader = new FileReader();
                    reader.readAsDataURL(file);
                    reader.onload = function (e) {
                        var img_file = [];
                        img_file.push(file);
                        htmlImageCompress = new HtmlImageCompress(img_file[0], { quality: 0.7, width: 500, height: 500, imageType: img_file[0].type })
                        htmlImageCompress.then(function (result) {
                            img_file.push(new File([result.file], result.origin.name, { type: result.file.type }));

                            htmlImageCompress = new HtmlImageCompress(img_file[0], { quality: 0.3, width: 500, height: 500, imageType: img_file[0].type })
                            htmlImageCompress.then(function (result) {
                                img_file.push(new File([result.file], result.origin.name, { type: result.file.type }));
                                var obj = {};
                                obj["id"] = 0;
                                obj["File"] = img_file;
                                obj["name"] = file.name;
                                obj["link"] = e.target.result
                                ImageSetData($select, obj)
                            }).catch(function (err) {
                                console.log($`發生錯誤：${err}`);
                                UploadPreviewFrameClear();
                                co.sweet.error("資料上傳失敗", "請重新上傳", null, null);
                            })
                        }).catch(function (err) {
                            console.log($`發生錯誤：${err}`);
                            UploadPreviewFrameClear();
                            co.sweet.error("資料上傳失敗", "請重新上傳", null, null);
                        })
                    };
                } else {
                    var reader = new FileReader();
                    reader.readAsDataURL(file);
                    reader.onload = function (e) {
                        var obj = {};
                        obj["id"] = 0;
                        obj["File"] = file;
                        obj["name"] = file.name;
                        obj["link"] = e.target.result
                        ImageSetData($select, obj)
                    };
                }
            });
        });
        $select.find(".img_input_frame").prepend(input_frame);
    }
}