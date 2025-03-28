Coker.Object.merge(Coker.File, {
    ListFileInit: function () {
        co.File.fileUploadWithPreview();
        $(".btn_upload_add > button").on("click", function (e) {
            e.preventDefault();
            var $self = $(this).parents(".data_upload");
            UploadListAdd(null, $self);
            if ($self.data("uploadtype") == 5) {
                $self.find(".preview_frame>.upload_frame input").trigger("click");
            }
        })

        $(".youtube_frame .btn").on("click", function (e) {
            e.preventDefault();
            var $self_list;
            var $self = $(this).parents(".data_upload");
            $self.find("ul > .upload_list").each(function () {
                var $temp_self = $(this);
                if ($temp_self.data("edit")) { $self_list = $temp_self; }
            })
            if ($self_list.data("uploadtype") != 4) return;
            var value = $(this).prev().val();
            var videoId = "";
            var starttime = "";
            var urlParams = new URLSearchParams(value.split('?')[1]);
            // 處理不同格式的 YouTube 網址
            if (value.startsWith('https://www.youtube.com/watch?')) {
                videoId = urlParams.get('v');
                starttime = urlParams.get('t');
            } else if (value.startsWith('https://youtu.be/') ||
                value.startsWith("https://www.youtube.com/shorts/") ||
                value.startsWith("https://youtube.com/shorts/") ||
                value.startsWith('https://www.youtube.com/live/')) {
                var urlParts = value.split('/');
                videoId = urlParts[urlParts.length - 1].split('?')[0];
                starttime = urlParams.get('t') == null ? urlParams.get('start') : urlParams.get('t');
            }
            var index = value.indexOf("watch?v=") + 8;
            $self.find(".youtube_preview").children("*").remove();
            if (value != "" && index > -1 && videoId != "") {
                var url = "https://www.youtube-nocookie.com/embed/" + videoId;
                if (starttime != null) url += `?start=${starttime}`;
                var iframe_html = `<iframe class="yt_preview w-100 h-100" src="${url}" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>`;
                $self.find(".youtube_preview").append(iframe_html);
                $self_list.find(".title").text(value);
                var videofile = starttime == null ? videoId : `${videoId}&t=${starttime}`;
                var obj = {
                    File: videofile,
                };
                if (typeof (total_files.find(item => typeof (item["Id"]) != "undefined" && item["Id"] == $self_list.data("id"))) != "undefined") {
                    if (total_files.find(item => item["Id"] == $self_list.data("id"))["File"].split("?")[0] != videoId) {
                        total_files.find(item => item["Id"] == $self_list.data("id"))["File"] = obj["File"]
                    }
                } else if (typeof (total_files.find(item => item["TempId"] == $self_list.data("tempid"))) != "undefined") {
                    if (total_files.find(item => item["TempId"] == $self_list.data("tempid"))["File"].split("?")[0] != videoId) {
                        total_files.find(item => item["TempId"] == $self_list.data("tempid"))["File"] = obj["File"]
                    }
                } else {
                    obj["TempId"] = $self_list.data("tempid");
                    obj["Type"] = $self_list.data("uploadtype");
                    obj["IsDelete"] = false;
                    total_files.push(obj);
                }
                $self_list.find(".thumb_img").attr("src", `https://img.youtube.com/vi/${videoId}/hqdefault.jpg`);
            } else {
                var error_html = "<div class='w-100 h-100 d-flex justify-content-center align-items-center bg-black bg-opacity-25 fw-bold'>請輸入正確的Youtube連結</div>"
                $self.find(".youtube_preview").append(error_html);
                $self_list.find(".title").text("");
                $self_list.data("file", "");
            }
        })

        $(function () {
            var drap_sy, drap_ey, drap_itemh;
            $(".data_upload > ul").each(function (index, element) {
                $(element).sortable({
                    items: "> .upload_list",
                    handle: ".serNoTool",
                    cursor: "move",
                    dropOnEmpty: false,
                    placeholder: "sortable-placeholder",
                    tolerance: "pointer",
                    start: function (event, ui) {
                        ui.item.data("startIndex", ui.item.index() + 1);
                        drap_sy = ui.item.offset().top;
                        drap_itemh = ui.item.height() * 1.5
                    },
                    stop: function (event, ui) {
                        drap_ey = ui.item.offset().top;
                        var $uploadList = ui.item.parents(".data_upload").find(".upload_list");
                        var $ser_no = ui.item.find(".ser_no");
                        var startIndex = ui.item.data("startIndex");
                        var newIndex = ui.item.index() + 1;
                        var move = startIndex - newIndex;
                        $ser_no.val(newIndex);
                        if (move < 0) {
                            SortChange($uploadList, "bigger", ui.item.data("serno"), $ser_no.val());
                        } else if (move >= 0) {
                            SortChange($uploadList, "smaller", $ser_no.val(), ui.item.data("serno"));
                        }
                        ui.item.data("serno", $ser_no.val());
                        ui.item.data("startIndex", newIndex)
                    }
                });
            });
        });
    },
    fileUploadWithPreview: function () {
        $(window).on("fileUploadWithPreview:imagesAdded", function (event) {
            var cachedFile = upload_file.cachedFileArray;
            $(".data_upload > ul > li").each(function () {
                var $self = $(this);
                let $root = $self.parents(".data_upload");
                let file_num = $root.data("file_num");
                if ($self.data("edit")) {
                    switch ($self.data("uploadtype")) {
                        //圖片上傳
                        case 1:
                            var new_file_list = [];
                            var file_name, file_type;
                            cachedFile.forEach(function (file, index) {
                                file_name = file.name.substring(0, file.name.indexOf(":"));
                                file_type = file.type;
                                new_file_list.push(new File(cachedFile.slice(index, index + 1), file_name, { type: file_type }));
                            })
                            var temp_files = [];
                            file_num--;
                            new_file_list.forEach(function (file, index) {
                                var img_file = [];
                                img_file.push(file);
                                var obj = {};
                                obj["TempId"] = $self.data("tempid") + index;
                                obj["Type"] = $self.data("uploadtype");
                                var reader = new FileReader();
                                var _URL = window.URL || window.webkitURL;
                                var image = new Image();
                                var _dfr = $.Deferred();
                                _dfr.promise();
                                image.src = _URL.createObjectURL(img_file[0]);
                                reader.readAsDataURL(img_file[0]);
                                image.onload = function () {
                                    _dfr.resolve(this.width);
                                };
                                reader.onload = (function (e) {
                                    obj["Link"] = e.target.result;
                                });
                                _dfr.pipe(function (width) {
                                    switch (true) {
                                        case width < 500:
                                            htmlImageCompress = new HtmlImageCompress(img_file[0], { quality: 0.5, width: 500, imageType: img_file[0].type });
                                            break;
                                        case width < 1000:
                                            htmlImageCompress = new HtmlImageCompress(img_file[0], { quality: 0.7, width: 800, imageType: img_file[0].type });
                                            break;
                                        default:
                                            htmlImageCompress = new HtmlImageCompress(img_file[0], { quality: 0.8, width: 1000, imageType: img_file[0].type });
                                            break;
                                    }
                                    return htmlImageCompress;
                                }).pipe(function () {
                                    htmlImageCompress.then(function (result) {
                                        img_file.push(new File([result.file], result.origin.name, { type: result.file.type }));
                                        htmlImageCompress = new HtmlImageCompress(img_file[0], { quality: 0.9, width: 300, imageType: img_file[0].type })
                                        htmlImageCompress.then(function (result) {
                                            image.src = _URL.createObjectURL(result.file);
                                            img_file.push(new File([result.file], result.origin.name, { type: result.file.type }));
                                            obj["File"] = img_file;
                                            obj["IsDelete"] = false;
                                            obj["Name"] = result.origin.name;
                                            total_files.push(obj);
                                            UploadListAdd(obj, $root);
                                        }).catch(function (err) {
                                            UploadPreviewFrameClear($root);
                                            console.log($`發生錯誤：${err}`);
                                            co.sweet.error("資料上傳失敗", "請重新上傳", null, null);
                                        })

                                    }).catch(function (err) {
                                        UploadPreviewFrameClear($root);
                                        console.log($`發生錯誤：${err}`);
                                        co.sweet.error("資料上傳失敗", "請重新上傳", null, null);
                                    })
                                });
                            })

                            break;
                        //360圖片上傳
                        /* ********** ***************** 
                        須重打
                        ***************************/
                        case 2:
                            var new_file_list = [];
                            var file_name, file_type;
                            cachedFile.forEach(function (file, index) {
                                file_name = file.name.substring(0, file.name.indexOf(":"));
                                file_type = file.type;
                                new_file_list.push(new File(cachedFile.slice(index, index + 1), file_name, { type: file_type }));
                            })

                            new_file_list.forEach(function (file, index) {
                                var img_file = [];
                                img_file.push(file);
                                htmlImageCompress = new HtmlImageCompress(img_file[0], { quality: 0.7 })
                                htmlImageCompress.then(function (result) {
                                    img_file.push(new File([result.file], result.origin.name, { type: result.file.type }));

                                    htmlImageCompress = new HtmlImageCompress(img_file[0], { quality: 0.3 })
                                    htmlImageCompress.then(function (result) {
                                        var obj = {};
                                        img_file.push(new File([result.file], result.origin.name, { type: result.file.type }));
                                        obj["File"] = img_file;
                                        obj["TempId"] = $self.data("tempid");
                                        obj["Name"] = result.origin.name;
                                        obj["Type"] = $self.data("uploadtype");
                                        obj["IsDelete"] = false;
                                        total_files.push(obj);
                                    }).catch(function (err) {
                                        console.log($`發生錯誤：${err}`);
                                        UploadPreviewFrameClear($root);
                                        co.sweet.error("資料上傳失敗", "請重新上傳", null, null);
                                    })

                                }).catch(function (err) {
                                    console.log($`發生錯誤：${err}`);
                                    UploadPreviewFrameClear($root);
                                    co.sweet.error("資料上傳失敗", "請重新上傳", null, null);
                                })
                            })

                            $self.find(".title").text(`${new_file_list[0].name}...共${new_file_list.length}張圖`);
                            break;
                        //影片上傳
                        /* ********** *****************
                        須重打，上傳有問題，尚未查詢
                        ***************************/
                        case 3:
                            var new_file_list = [];
                            var file_name, file_type;
                            cachedFile.forEach(function (file, index) {
                                file_name = file.name.substring(0, file.name.indexOf(":"));
                                file_type = file.type;
                                new_file_list.push(new File(cachedFile.slice(index, index + 1), file_name, { type: file_type }));
                            })

                            var temp_files = [];
                            new_file_list.forEach(function (file, index) {
                                var obj = {};
                                obj["File"] = file;
                                obj["TempId"] = $self.data("tempid") + index;
                                obj["Type"] = $self.data("uploadtype");
                                obj["IsDelete"] = false;
                                obj["Name"] = file.name;
                                total_files.push(obj);
                                temp_files.push(obj)
                            })

                            file_num--;
                            temp_files.forEach(function (file) {
                                UploadListAdd(file, $root);
                            })
                            break;
                        case 5:
                            var new_file_list = [];
                            var file_name, file_type;
                            cachedFile.forEach(function (file, index) {
                                file_name = file.name.substring(0, file.name.indexOf(":"));
                                file_type = file.type;
                                new_file_list.push(new File(cachedFile.slice(index, index + 1), file_name, { type: file_type }));
                            })

                            var temp_files = [];
                            new_file_list.forEach(function (file, index) {
                                var obj = {};
                                obj["File"] = file;
                                obj["TempId"] = $self.data("tempid") + index;
                                obj["Type"] = $self.data("uploadtype");
                                obj["IsDelete"] = false;
                                obj["Name"] = file.name;
                                total_files.push(obj);
                                temp_files.push(obj)
                            })

                            file_num--;
                            temp_files.forEach(function (file) {
                                UploadListAdd(file, $root);
                            })
                            break;
                    }
                    $root.data("file_num", file_num);
                }
            })
        })

        $(window).on("fileUploadWithPreview:imageDeleted", function (event) {
            //console.log("fileUploadWithPreview:imageDeleted")
            $(".data_upload > ul > li").each(function () {
                var $self = $(this);
                if ($self.data("edit")) {
                    var cachedFile = upload_file.cachedFileArray;
                    if (cachedFile.length < 1) {
                        $self.data("file", "");
                        $self.find(".title").text("");
                    } else {
                        var new_file_list = [];
                        cachedFile.forEach(function (file, index) {
                            var file_name = file.name.substring(0, file.name.indexOf(":"));
                            var file_type = file.type;
                            new_file_list.push(new File(cachedFile.slice(index, index + 1), file_name, { type: file_type }));
                        })
                        $self.data("file", new_file_list);
                    }
                }
            })
        })

        $(window).on("fileUploadWithPreview:clearButtonClicked", function (event) {
            $(".data_upload > ul > li").each(function () {
                var $self = $(this);
                var $uploadList = $self.parents(".data_upload").find(".upload_list");
                var $root = $(this).parents(".data_upload");
                let file_num = $root.data("file_num");
                if ($self.data("edit")) {
                    if ($self.data("serno") < file_num) { SortChange($uploadList, "bigger", $self.data("serno"), file_num); }
                    if (typeof ($self.data("id")) != "undefined") {
                        total_files.find(item => item["Id"] == $self.data("id"))["IsDelete"] = true;
                    } else if (typeof ($self.data("tempid")) != "undefined") {
                        var tempid = $self.data("tempid");
                        var index = total_files.findIndex(item => item["TempId"] == tempid);
                        total_files.splice(index, 1);
                        total_files.forEach(file => {
                            file["TempId"] = file["TempId"] > tempid ? file["TempId"] - 1 : file["TempId"];
                        })
                    }
                    UploadPreviewFrameClear($root);
                    $self.remove();
                    file_num -= 1;
                    $root.data("file_num", file_num);
                }
            })
        })
    },
    ListFile: function ($self) {
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
        let file_num = $parent.data("file_num");
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
                $parent.find(".upload_list").each(function () {
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
            });
            $self.data("edit", true)
            $parent.find(".default_frame").removeClass("d-flex");
            $parent.find(".youtube_preview").empty();
            $parent.find(".media_preview > div").children().remove();
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
                                co.File.ListFile($self);
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
                                if (typeof (file) != "undefined") {
                                    const objectUrl = URL.createObjectURL(file);
                                    var video = $("<video>", {
                                        class: "h-100 w-100",
                                        controls: true,
                                        preload: "metadata",
                                        src: objectUrl
                                    });
                                    $parent.find(".media_frame").addClass("d-flex");
                                    $parent.find(".media_preview > div").append(video);
                                }
                                $parent.find(".upload_frame").find("span").text(file.name);
                                $parent.find(".media_frame").find("input").val(total_files.find(item => item["TempId"] == $self.data("tempid"))["Name"]);
                            }
                            $parent.find(".media_frame").addClass("d-flex");
                        }
                    }
                    break;
                case 4:
                    if (typeof (total_files.find(item => typeof (item["Id"]) != "undefined" && item["Id"] == $self.data("id"))) != "undefined") {
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
                case 5:
                    if ($self.find(".title").text() == "") {
                        upload_file = co.File.UploadFileInit("ProdFile");
                        $parent.find(".upload_frame").removeClass("d-none");
                        $parent.find(".image-preview").addClass("d-none");
                    } else $parent.find(".default_frame").addClass("d-flex");
                    break;
            }
        }
        $parent.data("file_num", file_num);
    }
});