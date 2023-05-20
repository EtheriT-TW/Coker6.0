var img_file = [], img_delete_list = [];
var $btn_img_input, $btn_img_delete, $input_pic, $img_preview;

function ImageUploadInit(type, $container) {
    if (type == "single") {
        $image_upload = $container;
        $btn_img_input = $container.find(".btn_input_pic");
        $btn_img_delete = $container.find(".btn_img_delete");
        $input_pic = $container.find(".input_pic");
        $img_preview = $container.find(".img_preview");

        $btn_img_input.on("click", function (even) {
            even.preventDefault();
            $input_pic.click();
        });

        $input_pic.change(function () {
            SingleUploadImage(this.files[0]);
        })

    } else if (type == "multiple") {

    }
}

function SingleUploadImage(this_file) {
    img_file = [];
    img_file.push(this_file);

    var htmlImageCompress;
    htmlImageCompress = new HtmlImageCompress(img_file[0], { quality: 0.7 })
    htmlImageCompress.then(function (result) {
        img_file.push(new File([result.file], img_file[0].name));
    }).catch(function (err) {
        console.log($`發生錯誤：${err}`);
    })
    htmlImageCompress = new HtmlImageCompress(img_file[0], { quality: 0.3 })
    htmlImageCompress.then(function (result) {
        img_file.push(new File([result.file], img_file[0].name));
        var reader = new FileReader();
        reader.readAsDataURL(img_file[2]);
        reader.onload = (function (e) {
            $img_preview.removeClass("d-none");
            $img_preview.siblings("span").addClass("d-none");
            $img_preview.parents("button").first().addClass("border-0");
            $img_preview.parents("button").first().css("width", "unset");
            $img_preview.attr("src", e.target.result);
        });
    }).catch(function (err) {
        console.log($`發生錯誤：${err}`);
    })

    if (typeof ($image_upload.data("Id")) != "undefined") {
        img_delete_list.push(image_upload.data("Id"));
        $image_upload.removeData("Id");
    }
}