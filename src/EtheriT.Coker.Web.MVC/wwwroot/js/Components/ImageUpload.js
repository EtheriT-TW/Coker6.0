var img_file = [], img_delete_list = [], default_width;
var $btn_img_input, $btn_img_delete, $input_pic, $img_preview;

function ImageUploadInit(type, $container) {
    if (type == "single") {
        $image_upload = $container;
        $btn_img_input = $container.find(".btn_input_pic");
        $btn_img_delete = $container.find(".btn_img_delete");
        $input_pic = $container.find(".input_pic");
        $img_preview = $container.find(".img_preview");
        default_width = $img_preview.parents("button").first().css("width");

        $btn_img_input.on("click", function (even) {
            even.preventDefault();
            $input_pic.click();
        });

        $input_pic.change(function () {
            SingleUploadImage(this.files[0]);
        })

        $btn_img_delete.on("click", function (even) {
            even.preventDefault();
            if (typeof ($image_upload.data("Id")) != "undefined") {
                img_delete_list.push($image_upload.data("Id"));
                $image_upload.removeData("Id");
            }
            img_file = [];
            SingleImageClear();
        });

    } else if (type == "multiple") {

    }
}

function SingleImageClear() {
    if (!$img_preview.hasClass("d-none")) {
        $img_preview.addClass("d-none");
    }
    $img_preview.siblings("span").removeClass("d-none");
    $img_preview.siblings("span").text("add_photo_alternate");
    $img_preview.parents("button").first().removeClass("border-0");
    $img_preview.parents("button").first().css("width", default_width);
    $img_preview.attr("src", "");
    $image_upload.removeData("Id");
}

function SingleSetImage(data) {
    if (data != null) {
        $img_preview.removeClass("d-none");
        $img_preview.siblings("span").addClass("d-none");
        $img_preview.parents("button").first().addClass("border-0");
        default_width = $img_preview.parents("button").first().css("width");
        $img_preview.parents("button").first().css("width", "unset");
        $img_preview.attr("src", data.link);
        $img_preview.attr("alt", data.name);
        $image_upload.data("Id", data.id);
    }

    img_file = [];
}

function SingleUploadImage(this_file) {
    img_file = [];
    img_file.push(this_file);

    var htmlImageCompress;

    htmlImageCompress = new HtmlImageCompress(img_file[0], { quality: 0.7 })
    htmlImageCompress.then(function (result) {
        img_file.push(new File([result.file], img_file[0].name));

        htmlImageCompress = new HtmlImageCompress(img_file[0], { quality: 0.3 })
        htmlImageCompress.then(function (result) {
            img_file.push(new File([result.file], img_file[0].name));
            var reader = new FileReader();
            reader.readAsDataURL(img_file[2]);
            reader.onload = (function (e) {
                $img_preview.removeClass("d-none");
                $img_preview.siblings("span").addClass("d-none");
                $img_preview.parents("button").first().addClass("border-0");
                default_width = $img_preview.parents("button").first().css("width");
                $img_preview.parents("button").first().css("width", "unset");
                $img_preview.attr("src", e.target.result);
                $img_preview.attr("alt", img_file[0].name);
            });

            if (typeof ($image_upload.data("Id")) != "undefined") {
                img_delete_list.push(image_upload.data("Id"));
                $image_upload.removeData("Id");
            }

        }).catch(function (err) {
            console.log($`發生錯誤：${err}`);
            $img_preview.addClass("d-none");
            $img_preview.siblings("span").removeClass("d-none");
            $img_preview.siblings("span").text("warning");
            $img_preview.parents("button").first().removeClass("border-0");
            $img_preview.parents("button").first().css("width", default_width);
            $img_preview.attr("src", "");
            img_file = [];
        })

    }).catch(function (err) {
        console.log($`發生錯誤：${err}`);
        $img_preview.siblings("span").text("warning");
        img_file = [];
    })
}