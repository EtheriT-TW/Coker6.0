
function ImageUploadModalInit($image_upload, isSingle, needCompress) {
    //console.log("ImageUploadModalInit")
    var img_file = [], img_delete_list = [];

    if (needCompress) {
        //if (isSingle) {
        //    var $btn_img_input = $image_upload.find(".btn_input_pic");
        //    var $btn_img_delete = $image_upload.find(".btn_img_delete");
        //    var $input_pic = $image_upload.find(".input_pic");

        //    $btn_img_input.on("click", function (even) {
        //        even.preventDefault();
        //        $input_pic.click();
        //    });

        //    $input_pic.change(function () {
        //        SingleUploadImage(this.files[0]);
        //    })

        //    $btn_img_delete.on("click", function (even) {
        //        even.preventDefault();
        //        if (typeof ($image_upload.data("Id")) != "undefined") {
        //            img_delete_list.push($image_upload.data("Id"));
        //            $image_upload.removeData("Id");
        //        }
        //        img_file = [];
        //        SingleImageClear();
        //    });

        //} else {

        //}
    } else {
        if (isSingle) {
            $image_upload.find(".btn_input_pic").on("click", function (even) {
                even.preventDefault();
                $(this).parents("div").first().prev(".input_pic").click();
            });

            $image_upload.find(".input_pic").change(function () {
                var $image_upload = $(this).parents(".image_upload").first();
                var file = this.files[0];
                var reader = new FileReader();
                reader.readAsDataURL(file);
                reader.onload = function (e) {
                    var obj = {};
                    obj["Id"] = 0;
                    obj["File"] = file;
                    obj["Name"] = file.name;
                    obj["Link"] = e.target.result
                    if (typeof ($image_upload.data("file")) != "undefined") ImageDelect($image_upload);
                    $image_upload.data("file", obj);
                    ImageSetData($image_upload)
                };
            });

            $image_upload.find(".btn_img_delete").on("click", function (even) {
                even.preventDefault();
                ImageDelect($(this).parents(".image_upload").first());
            });

        } else {

        }
    }
    ImageUploadModalClear($image_upload);
}

function ImageUploadModalClear($select) {
    $select.children("div").each(function (index, data) {
        if (index > 0) {
            data.remove();
        } else {
            $select.data("delectList", null);
            ImageClear($select);
        }
    })
}

function ImageDelect($select) {
    var $img_btn = $select.find(".btn_input_pic");
    if ($img_btn.hasClass("has_image")) {
        if ($select.data("file").Id > 0) {
            var delect_list = [];
            if ($select.data("delectList") != null) {
                delect_list = $select.data("delectList");
            }
            delect_list.push($select.data("file").Id);
            $select.data("delectList", delect_list);
        }
        ImageClear($select);
    }
}

function ImageClear($select) {
    var $img_btn = $select.find(".btn_input_pic");
    if ($img_btn.hasClass("has_image")) {
        var $img_preview = $select.find(".img_preview");
        var $file_name = $select.find(".file_name");
        $img_preview.addClass("d-none");
        $img_preview.removeClass("d-none");
        $img_preview.siblings("span").removeClass("d-none");
        $img_preview.parents("button").first().removeClass("border-0");
        $img_btn.removeClass("has_image");
        $select.data("file", null);
        $img_preview.attr("src", "");
        $img_preview.attr("alt", "");
        $file_name.text("加入照片");
    }
}

function ImageUploadModalDataInsert($select, id, link, name) {
    if (id != "") {
        var obj = {};
        obj["Id"] = id;
        obj["File"] = null;
        obj["Name"] = name;
        obj["Link"] = link;
        $select.data("file", obj);
        ImageSetData($select)
    }
}

function ImageSetData($select) {
    var data = $select.data("file");
    if (typeof ($select.data("file")) == "undefined" || $select.data("file") == null) {
        //新增圖片
    } else {
        var $img_preview = $select.find(".img_preview");
        var $img_btn = $select.find(".btn_input_pic");
        var $file_name = $select.find(".file_name");
        $img_preview.removeClass("d-none");
        $img_preview.siblings("span").addClass("d-none");
        $img_preview.parents("button").first().addClass("border-0");
        $img_btn.addClass("has_image");
        $img_preview.attr("src", data.Link);
        $img_preview.attr("alt", data.Name);
        $file_name.text(data.Name);
    }
}