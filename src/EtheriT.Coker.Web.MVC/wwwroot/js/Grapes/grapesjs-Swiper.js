var $gjs_select = null;
grapesjs.plugins.add('grapesjs-Swiper', (editor, options) => {
    const AssetManager = editor.AssetManager;
    //輪播
    editor.DomComponents.addType('輪播', {
        isComponent: el => el.classList?.contains('one_swiper') || el.classList?.contains('one_swiper_thumbs') || el.classList?.contains('two_swiper') || el.classList?.contains('three_swiper') || el.classList?.contains('four_swiper') || el.classList?.contains('six_swiper') || el.classList?.contains('three_two_grid_swiper') || el.classList?.contains('vertical_swiper_thumbs'),
        model: {
            defaults: {
                tagName: 'div',
                traits: [
                    {
                        type: 'button', text: "開啟編輯",
                        command: editor => {
                            const selectedComponent = editor.getSelected();
                            var $selected = $(selectedComponent.getEl());
                            if ($selected.find(".swiper-slide").length > 0 && !$("#SwiperModal").hasClass("ready")) {
                                $(".set-caption .selectVideo").on("click", function (e) {
                                    e.preventDefault();
                                    const assets = AssetManager.getAll().models.map(m => m.attributes);
                                    AssetManager.open();

                                    AssetManager.onSelect((selected) => {
                                        if (selected && selected.id) {
                                            if (isVideoFile(selected.id)) {
                                                $(".set-caption #ytSrc").val(selected.id);
                                                $(".set-caption #ytSrc").trigger("change");
                                            } else co.sweet.error("請選擇影片檔案！");
                                        }
                                        AssetManager.close();
                                    });
                                    $("#SwiperModal").addClass("ready")
                                });
                            }
                            var $body = $("#SwiperList");
                            const $caption = $('.set-caption');
                            var datas = [];
                            $selected.find(".swiper .swiper-slide").each(function () {
                                var $self = $(this);
                                if (!$self.parent().hasClass("template_slide")) {
                                    var obj = {
                                        "href": $self.find("a").attr("href"),
                                        "title": $self.find("a").attr("title") || $self.find(".title").text(),
                                        "src": $self.find("img").attr("src"),
                                        "alt": $self.find("img").attr("alt"),
                                        "img_update": $self.find("img").length > 0 ? true : false,
                                        "a_tag": $self.find("video,iframe").length == 0 && ($self.find("a").length == 0 || typeof $self.find("a").attr("href") == "undefined" || !$self.find("a").attr("href").startsWith("#SwiperModal")) ? true : false,
                                        "target": $self.find("a").attr("target"),
                                        "yt_src": $self.find("iframe").length > 0 ? $self.find("iframe").attr("src") :
                                            $self.find("video").length > 0 ? $self.find("video").attr("src") :
                                                $self.find(`[href="#SwiperModal"]`).data("link"),
                                        "video_title": $self.find("iframe").length ? $self.find("iframe").attr("title") :
                                            $self.find("video").length > 0 ? $self.find("video").attr("title") :
                                                $self.find("img").attr("alt"),
                                        "start_time": $self.find("[data-start_time]").length ? $self.find("[data-start_time]").data("start_time") : "",
                                        "keep_time": $self.find("[data-keep_time]").length ? $self.find("[data-keep_time]").data("keep_time") : "",
                                        "synopsis_title": $self.find('.synopsis_title').text(), //文章標題
                                        "synopsis_caption": $self.find('.synopsis_caption').text().trim(), //文章內容
                                        "visible": $self.hasClass("backstageType"),
                                        "ratio": $self.find("a").data("ratio") || "16x9"
                                    };
                                    datas.push(obj);
                                }
                            });
                            $body.empty();
                            $("#EditContentForm input,#EditContentForm select").off("change").on("change", function () {
                                //相關設定存檔
                                // 獲取當前選中的 li
                                const $li = $(`#SwiperList [name="label"]:checked`).closest('li'); // 獲取顯示的 setting 所在的 li
                                // 獲取標題、內文、連結
                                const title = $('#slideTitle').val();
                                const content = $('#slideAlt').val();
                                const link = $('#slideHref').val();
                                const target = $("#CheckOpenWindow").prop("checked") ? "_blank" : "_self";
                                let yt_src = $('#ytSrc').val();
                                const start_time = $('#startTime').val() ? $('#startTime').val() : "";
                                const keep_time = $('#keepTime').val() ? $('#keepTime').val() : 5;
                                const visible = $("#CheckHidden").prop("checked") ? true : false;
                                const ratio = $('#ratio').val() || "16x9";
                                if (yt_src != "") {
                                    const $img = $li.find("img");
                                    if ($img.attr("src").startsWith("/images/")) {
                                        const video = getVideoUrl(yt_src);
                                        if (video.img != "") {
                                            $img.attr("src", video.img);
                                            yt_src = video.url;
                                        }
                                    }
                                }
                                $li.data({
                                    alt: title,
                                    synopsis_title: title,
                                    synopsis_caption: content,
                                    href: link,
                                    title: title,
                                    target: target,
                                    yt_src: yt_src,
                                    video_title: title,
                                    start_time: start_time,
                                    keep_time: keep_time,
                                    visible: visible,
                                    ratio: ratio
                                });
                                if (visible) {
                                    $li.find('.eyes>span:first-child').addClass('d-none');
                                    $li.find('.eyes>span:last-child').removeClass('d-none');
                                }
                                else {
                                    $li.find('.eyes>span:last-child').addClass('d-none');
                                    $li.find('.eyes>span:first-child').removeClass('d-none');
                                }
                                // 將生成的 HTML 注入到 li 內
                                $li.find('.img_alt').html(title);//更新圖片名稱
                                $li.find('.synopsis_title').html(title); // 更新標題
                                $li.find('.synopsis_caption').html(content); // 更新內文
                                $li.find('.a_href').html(link);//更新連結
                                $li.find('.a_title').html(title);//更新連結名稱
                                $li.find('.a_target').html(target);//是否另開連結
                                $li.find('.yt_src').html(yt_src);//更新YT連結
                                $li.find('.video_title').html(title);//更新影片標題
                                $li.find('.start_time').html(start_time);
                                $li.find('.keep_time').html(keep_time);
                                $li.find('.a_visible').html(visible);//是否隱藏
                            });
                            const newLi = function (index, data) {
                                var o = co.Object.merge({
                                    src: "/images/UploadImg.png",
                                    alt: "",
                                    href: $selected.find("a").attr("href") === "#SwiperModal" ? "#SwiperModal" : "",
                                    title: "",
                                    yt_src: "",
                                    video_title: "",
                                    start_time: 0,
                                    keep_time: 5,
                                    synopsis_title: "",
                                    synopsis_caption: "",
                                    visible: false,
                                    a_tag: true,
                                    img_update: true,
                                    ratio: "16x9"
                                }, data);
                                if (typeof (o.src) == "undefined" || o.src == "") {
                                    if (o.yt_src != "" && typeof (o.yt_src) != "undefined") {
                                        o.src = getVideoUrl(o.yt_src).img;
                                        if (o.src == "") o.src = "/images/defaultImage/video.jpg";
                                    } else o.src = "/images/UploadImg.png";
                                }
                                var content = $($("#TemplateSwiperList").html()); // 使用模板生成新的 li 元素
                                content.data(o);
                                content.find("[name='label']").attr("id", `selectSwiper${index}`);
                                content.find("label").attr("for", `selectSwiper${index}`);
                                content.find("img").attr({ "src": o.src, "alt": o.alt });
                                content.find(".img_alt").text(o.alt);
                                content.find(".a_href").text(o.href);
                                content.find(".yt_src").text(o.yt_src);
                                content.find(".start_time").text(o.start_time);
                                content.find(".keep_time").text(o.keep_time);
                                content.find(".synopsis_caption").text(o.synopsis_caption);
                                if (o.href != "#SwiperModal" && o.yt_src != "" && typeof (o.yt_src) != "undefined") {
                                    content.find("img").removeClass("update-img isPointer");
                                }
                                if (data.visible) {
                                    content.find(".eyes > span:first-child").addClass("d-none");
                                    content.find(".eyes > span:last-child").removeClass("d-none");
                                } else {
                                    content.find(".eyes > span:first-child").removeClass("d-none");
                                    content.find(".eyes > span:last-child").addClass("d-none");
                                }
                                /*if (!data.img_update) {
                                    content.find(".update-img").removeClass("update-img");
                                }*/
                                content.data("order", index);
                                content.find("label").on("click", function () {
                                    const $li = $(this).closest('li');
                                    const $setting = $li.find('.setting');
                                    const $setTitle = $caption.find('#slideTitle');
                                    const $setContent = $caption.find('#slideAlt');
                                    const $setLink = $caption.find('#slideHref');
                                    const $setYtSrc = $caption.find('#ytSrc');
                                    const $setStartTime = $caption.find('#startTime');
                                    const $setKeepTime = $caption.find('#keepTime');
                                    const $formSetting = [$("#set-title"), $("#set-content"), $("#set-link"), $("#YT-link"), $("#start-time"), $("#Ratio")];
                                    $caption.find('*:not(.a_target, #YT-link)').removeClass('d-none');
                                    if (
                                        !$li.data("a_tag") && (!$li.data("img_update") || ($li.data("yt_src") != "" && typeof ($li.data("yt_src")) != "undefined"))
                                    ) {
                                        $formSetting[3].removeClass("d-none");
                                        $formSetting[4].removeClass("d-none");
                                        $formSetting[5].removeClass("d-none");
                                    } else {
                                        $formSetting[3].addClass("d-none");
                                        $formSetting[4].addClass("d-none");
                                        $formSetting[5].addClass("d-none");
                                    }
                                    if ($li.data("yt_src") || $li.data("href") === "#SwiperModal" || !$li.data("a_tag")) {
                                        $formSetting[2].addClass('d-none');
                                    }
                                    if (!$li.data("synopsis_caption")) {
                                        $formSetting[1].addClass('d-none');
                                    }
                                    $("#CheckHidden").prop("checked", $li.data("visible"));
                                    $("#CheckOpenWindow").prop("checked", $li.data("target") == "_blank");
                                    $("#SwiperList").find('.setting').addClass('d-none');
                                    $setting.removeClass('d-none');
                                    $caption.removeClass('d-none');
                                    $("#SwiperList").find("[name='label']").prop("checked", false);
                                    $li.find("[name='label']").prop("checked", true);
                                    //注入已有資料到form的input
                                    $setTitle.val($li.data().alt); //圖片alt
                                    //$setTitle.val($li.data().title); //連結title
                                    //$setTitle.val($li.data().synopsis_title); //文章標題
                                    $setContent.val($li.data().synopsis_caption); //內文
                                    $setLink.val($li.data().href); //連結
                                    $setYtSrc.val($li.data().yt_src);//youtube
                                    $caption.find(`#ratio>option[value="${$li.data().ratio}"]`).prop("selected", true);
                                    $setStartTime.val($li.data().start_time);
                                    $setKeepTime.val($li.data().keep_time);
                                });

                                content.find(".update-img").on("click", function () {
                                    const $imgElement = $(this).closest("li").find("img"); // 找到對應的 img 標籤
                                    AssetManager.open();

                                    AssetManager.onSelect((result) => {
                                        // 使用選擇的圖片更新 img 的 src 屬性
                                        if (result && result.id) {
                                            const imgName = result.attributes.name.split(".");
                                            let newName = "";
                                            for (let i = 0; i < imgName.length; i++) {
                                                if (i == imgName.length - 1) {
                                                    break;
                                                }
                                                newName += imgName[i];
                                            }
                                            $(this).closest("li").find('.img_alt').text() ? $("#slideTitle").val : $("#slideTitle").val(newName).trigger("change");//消除副檔名
                                            $imgElement.attr("src", result.id); // 假設 result.id 是圖片的 URL
                                        }
                                        AssetManager.close();
                                    });
                                });

                                content.find(".delete-slide").on("click", function () {
                                    var $self = $(this);
                                    co.sweet.confirm("刪除欄位", "確定要刪除此欄位嗎?", "刪除", "取消", function () {
                                        $self.closest("li").remove();
                                    });
                                });

                                $body.append(content)
                                return content;
                            }
                            $.each(datas, newLi);
                            $("#SwiperList").sortable();
                            var SwiperModal = new bootstrap.Modal('#SwiperModal');
                            SwiperModal.show();
                            $("#SwiperList li:first").find("label").trigger("click");

                            $("#SwiperModal .btn-add-column").off("click").on("click", function () {
                                const $l = newLi($("#SwiperList>li").length, { a_tag: true, img_update: true });
                                $l.find(`[name="label"]`).prop("checked", true);
                                $l.find("label").trigger("click");
                                $('#scroll').scrollTop($('#scroll')[0].scrollHeight);
                            });


                            var $videoBotton = $("#SwiperModal .btn-add-YT");
                            $videoBotton.off("click").on("click", function () {
                                const $l = newLi($("#SwiperList>li").length, { a_tag: false, img_update: false });
                                $l.data("slide-type","video");
                                $l.find(`[name="label"]`).prop("checked", true);
                                $l.find("label").trigger("click");
                                $('#scroll').scrollTop($('#scroll')[0].scrollHeight);
                            });

                            const setIframe = function (VideoLink, startTime, Title) {
                                return $("<iframe>").attr({
                                    src: VideoLink,
                                    title: Title,
                                    width: "100%",
                                    height: "500",
                                    frameborder: "0",
                                    allow: "accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture",
                                    allowfullscreen: true,
                                    "data-start_time": startTime,
                                });
                            }
                            const isVideoFile = function (url) {
                                const videoExtensions = [".mp4", ".webm", ".ogg", ".mov"];
                                return videoExtensions.some(ext => url.toLowerCase().endsWith(ext));
                            }
                            const addAndUpdateSwiper = function ($slide, obj) {
                                if ($slide == null || $slide.length == 0) {
                                    var $selected = editor.getSelected();
                                    const have_template = $selected.find(".template_slide>.swiper-slide")[0];
                                    if (have_template) {
                                        $slide = $($selected.find(".template_slide>.swiper-slide")[0].toHTML());
                                    } else {
                                        $slide = $($("<div>").append($($selected.find(".swiper-slide")[0].toHTML())).html());
                                    }
                                }
                                if (obj.VideoLink != "" && typeof (obj.VideoLink) != "undefined") {
                                    if ($($slide).find('iframe').length > 0) {
                                        $($slide).find('iframe').remove();
                                    }
                                    const $item = $($slide).find(`[href="#SwiperModal"]`);
                                    if ($item.length > 0) {
                                        const video = getVideoUrl(obj.VideoLink, obj.startTime);
                                        if (video.url != "") {
                                            $item.attr({
                                                'data-link': video.url,
                                                'data-ratio': obj.ratio || "16x9",
                                                'data-start_time': obj.startTime || 0,
                                            });
                                            $item.find("img").attr({ 'data-keep_time': obj.keepTime || 5 });
                                        }
                                        if (obj.ImgSrc.startsWith("/images/")) {
                                            if (video.img != "") {
                                                obj.ImgSrc = video.img;
                                            } else if (video.img == "") {
                                                obj.ImgSrc = "/images/defaultImage/video.jpg";
                                            }
                                        }
                                    } else if (isVideoFile(obj.VideoLink)) {
                                        const $video = $('<video>', {
                                            src: obj.VideoLink,
                                            controls: true,
                                            preload: 'metadata',
                                            poster: obj.ImgSrc || "/images/defaultImage/video.jpg",
                                            'data-start_time': obj.startTime || 0,
                                            'data-keep_time': obj.keepTime || 5,
                                        });
                                        if (obj.VideoLink == "") obj.ImgSrc = "/images/defaultImage/video.jpg";
                                        $($slide).append($video);
                                    } else {
                                        const video = getVideoUrl(obj.VideoLink, obj.startTime);
                                        const $iframe = setIframe(video.url, obj.startTime, obj.Title);
                                        if (obj.keepTime) {
                                            $iframe.attr("data-keep_time", obj.keepTime);
                                            $($slide).attr("data-swiper-autoplay", obj.keepTime * 1000);
                                        }
                                        $($slide).append($iframe.appendTo("<div>"));
                                    }
                                    if ($($slide).find("video,iframe").length > 0 && $($slide).find("img").length == 1) {
                                        $($slide).find("img").remove(); // 移除舊的圖片容器
                                    }
                                }
                                $($slide).find('img').attr('src', obj.ImgSrc);
                                $($slide).find('img').attr('alt', obj.Title);
                                if (obj.Link) {
                                    $($slide).find('a').attr('href', obj.Link);
                                }
                                $($slide).find('a').attr('title', obj.Title);
                                $($slide).find('a').attr('target', obj.Target);
                                $($slide).find('.synopsis_title').text(obj.Title);
                                $($slide).find('.synopsis_caption').text(obj.Caption);
                                if (obj.isVisible) {
                                    $($slide).addClass('backstageType');
                                } else {
                                    $($slide).removeClass('backstageType');
                                }
                                return $slide;
                            }

                            $("#SwiperModal .sava").off("click").on("click", function () {
                                const s = selectedComponent.toHTML();
                                const $slides = $(s).find('.swiper .swiper-wrapper .swiper-slide').clone();
                                const $b = $(s).find('.swiper .swiper-wrapper');
                                $b.empty();
                                $("#SwiperList li").each(function (index, element) {
                                    const $img = $(element).find("img");
                                    let data = {
                                        ImgSrc: $img.length > 0 ? $(element).find("img").attr("src") : "",
                                        Title: $(element).data("alt") ? $(element).data("alt") : $(element).data("video_title"),
                                        Link: $(element).data("href"),
                                        Target: $(element).data("target"),
                                        VideoLink: $(element).data("yt_src"),
                                        isVisible: $(element).data("visible"),
                                        Caption: $(element).data("synopsis_caption"),
                                        startTime: $(element).data("start_time"),
                                        keepTime: $(element).data("keep_time"),
                                        ratio: $(element).data("ratio") || "16x9"
                                    };
                                    if (data.ImgSrc == "") {
                                        if (obj.VideoLink == "" || typeof (obj.VideoLink) == "undefined") {
                                            data.ImgSrc = "/images/UploadImg.png";
                                        }
                                    }
                                    // 更新slides中的圖片
                                    const order = $(element).data("order");
                                    let $slide = $slides[order];
                                    const existingTitle = $($slide).find('h2').text().trim();

                                    $b.append(addAndUpdateSwiper($slide, data));
                                });
                                const wrapper = selectedComponent.find(".swiper:not(.six_thumbs) .swiper-wrapper")[0];
                                $(s).find(".six_thumbs .swiper-wrapper").empty();
                                wrapper.components([]);
                                $b.children().each(function () {
                                    wrapper.append($(this).prop('outerHTML'));
                                });
                                //editor.getSelected().addComponents($s.html());
                                const $swiper = $(".gjs-frame")[0].contentWindow.$(`#${$selected.attr("id")}`);
                                $swiper.data("isInit", false);
                                typeof ($swiper.find(".swiper")[0].swiper) !== "undefined" && $swiper.find(".swiper")[0].swiper.destroy(true, true);
                                $(".gjs-frame")[0].contentWindow.SwiperInit({ autoplay: false });
                                SwiperModal.hide();
                            });
                        },
                    }
                ],
            }
        }, view: {
            events: {
                'dblclick': function (e) {
                    const traits = this.model.get('traits');
                    const editTrait = traits?.find(tr => tr.get('text') === '開啟編輯');
                    if (editTrait) {
                        const cmd = editTrait.get('command');
                        if (cmd) {
                            if (typeof cmd === 'function') {
                                cmd(this.em);
                            } else if (typeof cmd === 'string') {
                                this.em.runCommand(cmd);
                            }
                        }
                    }
                }
            }
        }
    });
    editor.DomComponents.addType('輪播容器', {
        isComponent: el => el.classList?.contains('image_link_slide'),
        model: {
            defaults: {
                removable: true,
                editable: false,
            }
        }
    });
    editor.on('component:clone', (obj) => {
        const iframe = document.getElementsByClassName("gjs-frame")[0].contentWindow;
        const classList = obj.getClasses();

        if (classList.indexOf("swiper-slide") > -1) {
            var swiper = editor.getSelected().parent().getEl().swiper;
            if (typeof (swiper) != "undefined") {
                var cont = iframe.document.getElementsByClassName("swiper-slide").length;
                const timmer = function () {
                    if (iframe.document.getElementsByClassName("swiper-slide").length != cont) swiper.update();
                    else setTimeout(timmer, 100);
                }
                setTimeout(timmer, 100);
            }
        } else if (classList.indexOf("one_swiper") > -1 || classList.indexOf("one_swiper_thumbs") > -1 || classList.indexOf("two_swiper") > -1 || classList.indexOf("four_swiper") > -1 || classList.indexOf("six_swiper") > -1 || classList.indexOf("three_two_grid_swiper") > -1 || classList.indexOf("vertical_swiper_thumbs") > -1) {
            var cont = iframe.document.getElementsByClassName("swiper").length;
            const timmer = function () {
                if (iframe.document.getElementsByClassName("swiper").length != cont) iframe.SwiperInit({ autoplay: false });
                else setTimeout(timmer, 100);
            }
            setTimeout(timmer, 100);
        }
    });
    editor.on('component:remove', (obj) => {
        const iframe = document.getElementsByClassName("gjs-frame")[0].contentWindow;
        const classList = obj.getClasses();
        if (classList.indexOf("swiper-slide") > -1) {
            if (typeof (editor.getSelected()) != "undefined") {
                var swiper = editor.getSelected().getEl().swiper;
                if (typeof (swiper) != "undefined") {
                    var cont = iframe.document.getElementsByClassName("swiper-slide").length;
                    const timmer = function () {
                        if (iframe.document.getElementsByClassName("swiper-slide").length != cont) swiper.update();
                        else setTimeout(timmer, 100);
                    }
                    setTimeout(timmer, 100);
                }
            }
        }
    });
    editor.on('component:drag:end', (obj) => {
        if (classList.indexOf("swiper-slide") > -1) {
            var swiper = obj.target.parent().parent().getEl().swiper;
            swiper.update();
        } else if (classList.indexOf("one_swiper") > -1 || classList.indexOf("one_swiper_thumbs") > -1 || classList.indexOf("two_swiper") > -1 || classList.indexOf("four_swiper") > -1 || classList.indexOf("six_swiper") > -1 || classList.indexOf("three_two_grid_swiper") > -1 || classList.indexOf("vertical_swiper_thumbs") > -1) iframe.SwiperInit({ autoplay: false });
    });
});