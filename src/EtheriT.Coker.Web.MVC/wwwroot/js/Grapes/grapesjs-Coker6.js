grapesjs.plugins.add('grapesjs-Coker6', (editor, options) => {
    let settings = {
        save: function () { return false; },
        import: function () { return false; },
        getComponer: function () { return false; },
        asset: [],
        iconPickerOpt: { cols: 4, rows: 4, footer: false, iconset: "GoogleMaterialSymbolsOutlined" }
    };
    $.extend(true, settings, options);
    editor.settings = settings;
    const AssetManager = editor.AssetManager;
    const categories = editor.BlockManager.getCategories();
    const BlockManager = editor.BlockManager;
    const panelManager = editor.Panels;

    //設定UI文字
    editor.I18n.setMessages({ tw: tw });

    /*檔案管理*/
    AssetManager.addType('image', {
        view: {
            attributes: { 'Guid': 'custom-value' }
        }
    });

    // Wait for CKEDITOR load
    setInterval(() => {
        if (typeof (CKEDITOR) != "undefined") {
            CKEDITOR.dtd.$editable.a = 1;
            CKEDITOR.dtd.$editable.p = 1;
            CKEDITOR.dtd.$editable.span = 1;
            CKEDITOR.dtd.$editable.li = 1;
            CKEDITOR.dtd.$editable.strong = 1;
            CKEDITOR.dtd.$editable.div = 1;
            CKEDITOR.dtd.$removeEmpty.span = 0;
            CKEDITOR.dtd.$removeEmpty.i = 0;
        }
    }, 200);
    editor.on("run:modal-open:modalckeditor", function () {
        console.log("in");
    });

    editor.on('asset:add', (option) => {
        //console.log(option);
    });

    editor.on('run:open-assets', () => {
        const modal = editor.Modal;
        const modalBody = modal.getContentEl();
        const uploader = modalBody.querySelector('.gjs-am-file-uploader');
        const assetsHeader = modalBody.querySelector('.gjs-am-assets-header');
        const assetsBody = modalBody.querySelector('.gjs-am-assets-cont');
        const assetsFilter = modalBody.querySelector('.gjs-am-assets-filter');
        if (!!!assetsFilter) {
            const filter = $(`
                <div class="input-group mb-3 gjs-am-assets-filter">
                    <input type="text" class="form-control" placeholder="搜尋檔案名稱" aria-label="搜尋檔案名稱" aria-describedby="button-addon2">
                    <button class="btn btn-outline-secondary" type="button" id="button-addon2">
                        <span class="material-symbols-outlined">search</span>
                    </button>
                </div>
            `);
            filter.find(".btn").on("click", () => {
                const search = filter.children('[type="text"]').val();
                if (search == "") $(".gjs-am-asset-image").removeClass("d-none");
                else {
                    $(".gjs-am-asset-image").each(function () {
                        const $image = $(this);
                        const imageName = $image.find(".gjs-am-dimensions").text();
                        if (imageName.indexOf(search) < 0) $image.addClass("d-none");
                        else $image.removeClass("d-none");
                    });
                }
            });
            assetsHeader.style.display = 'none'
            assetsBody.insertBefore(filter[0], assetsHeader);
        }
    });

    //檔案刪除
    editor.on("asset:remove", (asset) => {
        const guid = asset.get('guid');
        co.File.Delete(guid).done(function (result) {
            console.log(result);
        });
    });

    //元件參數設定
    /***********************************************************************
     * 注意事項
     * 元件名稱需為小寫，否則欄位會無法對應
     * ********************************************************************* */
    /*元件浮動控制項*/
    const commands = editor.Commands;
    commands.add('tlb-settime', editor => {
        myModal.show();
    });

    editor.on('component:selected', () => {

        // whenever a component is selected in the editor

        // set your command and icon here
        const commandToAdd = 'tlb-settime';
        const commandIcon = 'fa fa-star';

        // get the selected componnet and its default toolbar
        const selectedComponent = editor.getSelected();
        const defaultToolbar = selectedComponent.get('toolbar');

        // check if this command already exists on this component toolbar
        const commandExists = defaultToolbar.some(item => item.command === commandToAdd);

        // if it doesn't already exist, add it
        if (!commandExists) {
            selectedComponent.set({
                toolbar: [...defaultToolbar, { attributes: { class: commandIcon }, command: commandToAdd }]
            });
        }

    });

    /*連結 */
    editor.DomComponents.addType('連結', {
        isComponent: el => el.tagName == 'A',
        model: {
            defaults: {
                traits: [
                    // Strings are automatically converted to text types
                    { name: 'title', type: 'text', label: '名稱', placeholder: '請輸入連結名稱' },
                    { name: 'data-text', type: 'text', label: '顯示文字', placeholder: '請輸入顯示文字' },
                    { name: 'href', type: 'text', label: '超連結', placeholder: '請輸入連結位子' },
                    {
                        name: 'file', type: 'button',
                        text: "選擇檔案",
                        command: editor => {
                            AssetManager.open();
                            AssetManager.onSelect((resule) => {
                                editor.getSelected().set("attributes", { "href": resule.id });
                                AssetManager.close();
                            });
                        },
                    },
                    {
                        name: 'target', type: 'select', label: '開啟方式',
                        options: [
                            { id: '_self', name: '直接連結' },
                            { id: '_blank', name: '另開視窗', label: '另開視窗' }
                        ]
                    }
                ]
            }, init() {
                this.on('change:attributes:data-text', function (component) {
                    if (typeof (component.getEl()) != "undefined") {
                        component.find(".name")[0].components(component.getAttributes()["data-text"]);
                    }
                });
            }
        },
    });
    /*檔案下載*/
    editor.DomComponents.addType('檔案下載', {
        isComponent: el => el.classList?.contains('link_with_icon'),
        model: {
            defaults: {
                traits: [
                    // Strings are automatically converted to text types
                    { name: 'download', type: 'text', label: '檔案名稱', placeholder: '請輸入檔案名稱' },
                    {
                        name: 'file', type: 'button',
                        text: "選擇檔案",
                        command: editor => {
                            AssetManager.open();
                            AssetManager.onSelect((resule) => {
                                var LinkWithIconInit = $(".gjs-frame")[0].contentWindow.LinkWithIconInit;
                                editor.getSelected().set("attributes", { "href": resule.id });
                                LinkWithIconInit();
                                AssetManager.close();
                            });
                        },
                    }
                ],
            }, init() {
                this.on('change:attributes:download', function (component) {
                    if (typeof (component.getEl()) != "undefined")
                        component.find(".name")[0].components(component.getAttributes().download);
                });
            }
        },
        view: {
            init() {
                this.setLink();
            }, setLink: function () {
                setTimeout(function () {
                    var LinkWithIconInit = $(".gjs-frame")[0].contentWindow.LinkWithIconInit;
                    LinkWithIconInit();
                }, 100);
            }
        }
    });
    //輪播
    editor.DomComponents.addType('輪播', {
        isComponent: el => el.classList?.contains('one_swiper') || el.classList?.contains('one_swiper_thumbs') || el.classList?.contains('two_swiper') || el.classList?.contains('three_swiper') || el.classList?.contains('four_swiper') || el.classList?.contains('six_swiper'),
        model: {
            defaults: {
                traits: [
                    {
                        type: 'button', text: "開啟編輯",
                        command: editor => {
                            const selectedComponent = editor.getSelected();
                            var $selected = $(selectedComponent.getEl());
                            if ($selected.find(".swiper-slide").length > 0 && $("#SwiperModal").length < 1) {
                                $(`<div class="modal fade" id="SwiperModal" tabindex="-1" aria-labelledby="SwiperModalLabel" aria-hidden="true">
                                        <div class="modal-dialog modal-lg">
                                        <div class="modal-content">
                                            <div class="modal-header">
                                            <h1 class="modal-title fs-5" id="SwiperModalLabel">輪播編輯</h1>
                                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                                            </div>
                                            <div class="modal-body">
                                                <div id="scroll" class="px-0 w-50">
                                                <ul id="SwiperList" class="px-0 p-1"></ul>
                                                <template id="TemplateSwiperList">
                                                    <li>
                                                        <input type="radio" name="label" checked="checked">
                                                        <label class="d-flex mb-3 border p-2 border-dark rounded position-relative isScroll">
                                                            <img class="me-2 update-img isPointer" src="" alt="" />
                                                            <div class="align-self-center">
                                                                <div class="img_alt d-none"></div>
                                                                <p class="setting d-none h3">正在編輯</>
                                                                <div class="a_href d-none"></div>
                                                                <div class="a_title d-none"></div>
                                                                <div class="a_target d-none"></div>
                                                                <div class="synopsis_title d-none"></div>
                                                                <div class="synopsis_caption d-none"></div>
                                                                <div class="eyes">
                                                                    <span class="material-symbols-outlined">visibility</span>
                                                                    <span class="material-symbols-outlined d-none">visibility_off</span>
                                                                </div>
                                                            </div>

                                                            <div class="align-items-center d-flex position-absolute top-50 end-0 translate-middle-y mr-1">
                                                                <a class="mr-1 show-form isPointer" title="編輯內容">
                                                                    <span class="material-symbols-outlined text-black button">border_color</span>
                                                                </a>
                                                                <a class="mr-1 delete-slide isPointer" title="刪除">
                                                                    <span class="material-symbols-outlined text-black button">delete</span>
                                                                </a>
                                                            </div>
                                                        </label>
                                                    </li>
                                                </template>
                                                </div>
                                                <button id="add" type="button" class="btn-add-column">新增一欄</button>
                                                <div class="w-50 ps-3 set-caption">
                                                  <h5>相關設定</h5>
                                                  <form id="EditContentForm">
                                                    <div id=set-title class="mb-3 d-none">
                                                      <label for="slideTitle" class="form-label">標題</label>
                                                      <input type="text" class="form-control" id="slideTitle" placeholder="輸入名稱" />
                                                    </div>
                                                    <div id="set-content" class="mb-3">
                                                      <label for="slideAlt" class="form-label">內文</label>
                                                      <input class="form-control" id="slideAlt" rows="3" placeholder="輸入內文"></textarea>
                                                    </div>
                                                    <div id="set-link" class="mb-4">
                                                      <label for="slideHref" class="form-label">連結網址</label>
                                                      <input class="form-check-input ms-4" type="checkbox" value="" id="CheckOpenWindow">
                                                      <label class="form-check-label ms-3 ps-4" for="CheckOpenWindow">另開新視窗</label>
                                                      <input type="text" class="form-control" id="slideHref" placeholder="輸入連結" />
                                                    </div>
                                                    <div id="img-hidden" class="ms-3">
                                                        <input class="form-check-input" type="checkbox" value="" id="CheckHidden">
                                                        <label class="form-check-label" for="CheckHidden">隱藏</label>
                                                    </div>
                                                  </form>
                                                </div>
                                            </div>
                                            <div class="modal-footer">
                                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">取消</button>
                                            <button type="button" class="btn btn-primary sava">完成編輯</button>
                                            </div>
                                        </div>
                                        </div>
                                    </div>
                                `).appendTo("body");
                            }
                            var $body = $("#SwiperList");
                            var datas = [];
                            $selected.find(".swiper .swiper-slide").each(function () {
                                var $self = $(this);
                                if (!$self.parent().hasClass("template_slide")) {
                                    var obj = {
                                        "href": $self.find("a").attr("href"),
                                        "title": $self.find("a").attr("title"),
                                        "src": $self.find("img").attr("src"),
                                        "alt": $self.find("img").attr("alt"),
                                        "a_tag": $self.find("a").length > 0 ? true : flase,
                                        "target": $self.find("a").attr("target"),
                                        "synopsis_title": $self.find('.synopsis_title').text(), //文章標題
                                        "synopsis_caption": $self.find('.synopsis_caption').text(), //文章內容
                                        "visible": $self.hasClass("backstageType")
                                    };
                                    datas.push(obj);
                                }
                            });
                            $body.empty();

                            $("#EditContentForm input").off("change").on("change", function () {
                                //相關設定存檔
                                // 獲取當前選中的 li
                                const $li = $(`#SwiperList [name="label"]:checked`).closest('li'); // 獲取顯示的 setting 所在的 li
                                // 獲取標題、內文、連結
                                const title = $('#slideTitle').val();
                                const content = $('#slideAlt').val();
                                const link = $('#slideHref').val();
                                const target = $("#CheckOpenWindow").prop("checked") ? "_blank" : "_self";
                                const visible = $("#CheckHidden").prop("checked") ? true : false;
                                $li.data({
                                    alt: title,
                                    synopsis_title: title,
                                    synopsis_caption: content,
                                    href: link,
                                    title: title,
                                    target: target,
                                    visible: visible
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
                                $li.find('.a_visible').html(visible);//是否隱藏
                            });
                            const newLi = function (index, data) {
                                var o = co.Object.merge({
                                    src: "/images/UploadImg.png",
                                    alt: "",
                                    href: $selected.find("a").attr("href") === "#SwiperModal" ? "#SwiperModal" : "",
                                    title: "",
                                    synopsis_title: "",
                                    synopsis_caption: "",
                                    visible:false,
                                    a_tag: true
                                }, data);
                                var content = $($("#TemplateSwiperList").html());
                                content.data(o);
                                content.find("[name='label']").attr("id", `selectSwiper${index}`);
                                content.find("label").attr("for", `selectSwiper${index}`);
                                content.find("img").attr({ "src": o.src, "alt": o.alt });
                                content.find(".img_alt").text(o.alt);
                                content.find(".a_href").text(o.href);
                                content.find(".synopsis_caption").text(o.synopsis_caption);
                                if (data.visible) {
                                    content.find(".eyes > span:first-child").addClass("d-none");
                                    content.find(".eyes > span:last-child").removeClass("d-none");
                                } else {
                                    content.find(".eyes > span:first-child").removeClass("d-none");
                                    content.find(".eyes > span:last-child").addClass("d-none");
                                }
                                content.data("order", index);
                                content.find("label").on("click", function () {
                                    $("#EditContentForm input:focus").trigger("change");
                                    const $li = $(this).closest('li');
                                    const $caption = $('.set-caption');
                                    const $setting = $li.find('.setting');
                                    const $setTitle = $caption.find('#slideTitle');
                                    const $setContent = $caption.find('#slideAlt');
                                    const $setLink = $caption.find('#slideHref');
                                    const $formSetting = [$("#set-title"), $("#set-content"), $("#set-link")];
                                    $caption.find('*:not(.a_target)').removeClass('d-none');
                                    if ($li.data("href") === "#SwiperModal" || !$li.data("a_tag")) {
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
                                });

                                content.find(".update-img").on("click", function () {
                                    const $imgElement = $(this).closest("li").find("img"); // 找到對應的 img 標籤
                                    AssetManager.open();

                                    AssetManager.onSelect((result) => {
                                        // 使用選擇的圖片更新 img 的 src 屬性
                                        if (result && result.id) {const imgName = result.attributes.name.split(".");
                                            let newName = "";
                                            for (let i = 0; i < imgName.length; i++) {
                                                if (i == imgName.length - 1) {
                                                    break;
                                                }
                                                newName+= imgName[i];
                                            }
                                            $("#slideTitle").val(newName).trigger("change");//消除副檔名
                                            $imgElement.attr("src", result.id); // 假設 result.id 是圖片的 URL
                                        }
                                        AssetManager.close();
                                    });
                                });

                                content.find(".delete-slide").on("click", function () {
                                    var isConfirmed = window.confirm("確定要刪除此欄位嗎?");
                                    if (isConfirmed) {
                                        $(this).closest("li").remove();
                                    }
                                });

                                $body.append(content)
                            }
                            $.each(datas, newLi);
                            $("#SwiperList").sortable();
                            var SwiperModal = new bootstrap.Modal('#SwiperModal');
                            SwiperModal.show();
                            $("#SwiperList li:first").find("label").trigger("click");

                            $("#SwiperModal .btn-add-column").off("click").on("click", function () {
                                newLi($("#SwiperList>li").length, {});
                            });

                            $("#SwiperModal .sava").off("click").on("click", function () {
                                const $s = $selected.clone();
                                const $slides = $s.find(".swiper .swiper-wrapper>.swiper-slide").clone();
                                const $b = $s.find(".swiper .swiper-wrapper");
                                $b.empty();
                                $("#SwiperList li").each(function (index, element) {
                                    const newImgSrc = $(element).find("img").attr("src");
                                    const newTitle = $(element).data("alt");
                                    const newLink = $(element).data("href");
                                    const newTarget = $(element).data("target");
                                    const isVisible = $(element).data("visible");
                                    const newCaption = $(element).data("synopsis_caption");
                                    // 更新slides中的圖片
                                    const order = $(element).data("order");
                                    let $new_slide = $slides[order];
                                    const existingTitle = $($new_slide).find('h2').text().trim();
                                    if ($new_slide) {
                                        $($new_slide).find('img').attr('src', newImgSrc);
                                        $($new_slide).find('img').attr('alt', newTitle);
                                        if (newLink) {
                                            $($new_slide).find('a').attr('href', newLink);
                                        }
                                        $($new_slide).find('a').attr('title', newTitle);
                                        $($new_slide).find('a').attr('target', newTarget);
                                        $($new_slide).find('.synopsis_title').text(newTitle);
                                        $($new_slide).find('.synopsis_caption').text(newCaption);
                                        if (isVisible) {
                                            $($new_slide).addClass('backstageType');
                                        } else {
                                            $($new_slide).removeClass('backstageType');
                                        }
                                        $b.append($new_slide);
                                        
                                    } else {
                                        var $selected = editor.getSelected();
                                        var swiper = $selected.find(".swiper")[0].getEl().swiper;
                                        const have_template = $selected.find(".template_slide>.swiper-slide")[0];
                                        if (have_template) {
                                            $new_slide = $($selected.find(".template_slide>.swiper-slide")[0].toHTML());
                                            $new_slide.find('a').attr('target', newTarget);
                                            $new_slide.find('.synopsis_title').text(newTitle);
                                            $new_slide.find('.synopsis_caption').text(newCaption);
                                        } else {
                                            $new_slide = $("<div>").append($($selected.find(".swiper-slide")[0].toHTML())).html();
                                            $new_slide = $($new_slide);
                                            $new_slide.find('img').attr('src', newImgSrc);
                                            $new_slide.find('img').attr('alt', newTitle);
                                            if (newLink) { 
                                                 $new_slide.find('a').attr('href', newLink);
                                            }
                                            $new_slide.find('a').attr('title', newTitle);
                                            $new_slide.find('a').attr('target', newTarget);
                                            $new_slide.find('.synopsis_title').text(newTitle);
                                            $new_slide.find('.synopsis_caption').text(newCaption);
                                        }
                                        if (isVisible) {
                                            $($new_slide).addClass('backstageType');
                                        } else {
                                            $($new_slide).removeClass('backstageType');
                                        }
                                        $b.append($new_slide);
                                    }
                                });
                                $s.find(".six_thumbs .swiper-wrapper").empty();
                                selectedComponent.components([]);
                                $s.children().each(function () {
                                    selectedComponent.append($(this).prop('outerHTML'));
                                });
                                //editor.getSelected().addComponents($s.html());
                                $(".gjs-frame")[0].contentWindow.$(`#${$selected.attr("id")}`).data("isInit", false);
                                $(".gjs-frame")[0].contentWindow.SwiperInit({ autoplay: false });
                                SwiperModal.hide();
                            });
                        },
                    }
                ],
            }
        },
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
    //QA元件
    editor.DomComponents.addType('QA元件', {
        isComponent: el => el.classList?.contains('qa'),
        model: {
            init() {
                const self = this;
                const ccid = self.ccid;
                const c = $(".gjs-frame")[0].contentWindow.$;
                window.setTimeout(function () {
                    const a = self.find("a.btn")[0];
                    const collapse = self.find(".collapse")[0];
                    a.addAttributes({ "href": `#${ccid}_content`, "Title": "展開QA", "data-bs-toggle": "collapse" })
                    collapse.addAttributes({ "id": `${ccid}_content` });
                    c(`#${ccid} a`).attr({ "data-bs-toggle": `` });
                }, 200)
            }
        },
    });
    editor.DomComponents.addType('QA元件鎖定版型', {
        isComponent: el => {
            const fa = $(el).parents(".qa");
            return $(fa).length > 0 && (el.classList?.contains('collapse') || el.classList?.contains('card') || el.classList?.contains('fas') || el.classList?.contains('qa-bg'));
        },
        model: {
            defaults: {
                hoverable: false,
                selectable: false,
                droppable: false,
                copyable: false,
                removable: false,
                editable: false,
            }
        }
    });
    //名片
    editor.DomComponents.addType('名片介紹', {
        isComponent: el => el.classList?.contains('frame_type_2'),
        model: {
            defaults: {
                droppable: false,
                copyable: false
            },
            init() {

            }
        },
    });
    //名片介紹
    editor.DomComponents.addType('名片介紹', {
        isComponent: el => el.classList?.contains('frame_type_2'),
        model: {
            defaults: {
                droppable: false,
                copyable: false
            },
            init() {

            }
        },
    });
    //活動
    editor.DomComponents.addType('活動列表', {
        isComponent: el => el.classList?.contains('articletype'),
        model: {
            defaults: {
                droppable: false,
                copyable: false,
                traits: [
                    { name: 'data-daterange', type: 'date-range', label: '起訖日期' },
                    { name: 'data-location', type: 'text', label: '地點', placeholder: '請輸入地點' },
                    { name: 'data-addr', type: 'text', label: '地址', placeholder: '請輸入地址' },
                    { name: 'data-link', type: 'text', label: '連結', placeholder: '請輸入連結' },
                    { name: 'data-organizer', type: 'text', label: '主辦單位', placeholder: '請輸入主辦單位' },
                    { name: 'data-a-organizer', type: 'text', label: '協辦單位', placeholder: '請輸入協辦單位' },
                    { name: 'data-r-organizer', type: 'text', label: '執行單位', placeholder: '請輸入執行單位' },
                    { name: 'data-tel', type: 'text', label: '電話', placeholder: '請輸入電話' }
                ]
            },
            init() {
                const self = this;
                const setting = function () {
                    setTimeout(() => {
                        var content = $(".gjs-frame")[0].contentWindow.date_input_change;
                        if (typeof (content) == "undefined") setting();
                        else {
                            const html = content(self.getId());
                            self.components(html);
                        }
                    }, 200);
                }
                self.on(`change:attributes`, setting);
                setting();
            }
        }
    });
    //日期區間型態
    editor.TraitManager.addType("date-range", {

        createInput({ trait }) {
            const self = this;
            const el = document.createElement('div');
            el.innerHTML = `
              <div class="date-range-inputs">
                <input type="text" class="date-range" pattern="[1-9]\d{3}/\d{2}/\d{2} \d{2}:\d{2} ~ [1-9]\d{3}/\d{2}/\d{2} \d{2}:\d{2}" />
              </div>
            `;
            const $d = $(el).find(".date-range");
            const timeStr = self.model.getValue();
            co.Picker.Init($d);
            try {
                const array = timeStr.split(" ~ ");
                $d.data('daterangepicker').setStartDate(array[0]);
                $d.data('daterangepicker').setEndDate(array[1]);
            } catch {
                $d.data('daterangepicker').setStartDate(Date.now);
            }
            $d.on("change", function () {
                editor.getSelected().addAttributes({ "data-daterange": this.value })
            });
            return el;
        }
    });
    //子頁內容區
    editor.DomComponents.addType('子頁內容', {
        isComponent: el => el.classList?.contains('subpage_content'),
        model: {
            defaults: {
                droppable: false,
                editable: false,
                copyable: false,
            }
        },
    });

    var PopupDirectory = null;
    //目錄
    editor.DomComponents.addType('目錄', {
        isComponent: el => el.classList?.contains('menu_directory') || el.classList?.contains('catalog_frame') || el.classList?.contains('advertise_directory'),
        model: {
            defaults: {
                droppable: false,
                editable: false,
                copyable: false,
                traits: [
                    { name: 'id', type: 'text', label: 'ID', placeholder: '元件ID名稱' },
                    { name: 'data-diridname', type: 'text', label: '目錄名稱', placeholder: '尚未關聯目錄' },
                    { name: 'data-dirpath', type: 'text', label: '路徑', placeholder: '設定連結路徑' },
                    {
                        name: 'data-dirid', type: 'button',
                        text: "設置目錄",
                        command: editor => {
                            var data = null;
                            if (!!!PopupDirectory) {
                                PopupDirectory = $("#PopupDirectory").dxPopup("instance");
                                PopupDirectory.option("contentTemplate", $("#PopupDirectory-template"));
                                PopupDirectory.option("title", "設置目錄");
                                window.DirectoryList_SelectChange = function (selectedItems) {
                                    data = selectedItems.selectedRowsData;
                                }
                                window.setTimeout(function () {
                                    $("#PopupDirectory .cancel").on("click", function () {
                                        PopupDirectory.hide();
                                    });
                                    $("#PopupDirectory .Sure").on("click", function () {
                                        editor.getSelected().set("attributes", {
                                            "data-dirid": data.map(function (item) {
                                                return item['Id'];
                                            }),
                                            "data-diridname": data.map(function (item) {
                                                return item['Title'];
                                            })
                                        });
                                        PopupDirectory.hide();
                                        $(".gjs-frame")[0].contentWindow.DirectoryGetDataInit();
                                    });
                                }, 200);
                            }
                            PopupDirectory.show();
                        }
                    },
                    { name: 'data-shownum', type: 'text', label: '單頁筆數', placeholder: '一個分頁要抓幾筆資料' },
                    { name: 'data-maxlen', type: 'text', label: '最大筆數', placeholder: '該目錄僅抓幾筆資料' }
                ],
            },
            init() {
                var self = this;
                self.on(`change:attributes:data-shownum`, component => {
                    setTimeout(() => {
                        const fWindow = $(".gjs-frame")[0].contentWindow;
                        let attr = component.getAttributes();
                        fWindow.$(`#${component.getId()}`).data({
                            "prevdirid": attr["data-prevdirid"],
                            "shownum": attr["data-shownum"]
                        });
                        fWindow.DirectoryGetDataInit();
                    }, 200);
                });
                self.on(`change:attributes:data-maxlen`, component => {
                    setTimeout(() => {
                        const fWindow = $(".gjs-frame")[0].contentWindow;
                        let attr = component.getAttributes();
                        fWindow.$(`#${component.getId()}`).data({
                            "prevdirid": attr["data-prevdirid"],
                            "maxlen": attr["data-maxlen"]
                        });
                        fWindow.DirectoryGetDataInit();
                    }, 200);
                });
            }
        },
    });
    //目錄切換控制
    editor.DomComponents.addType('格列切換控制', {
        isComponent: el => el.classList?.contains('switch_control'),
        model: {
            defaults: {
                droppable: false,
                copyable: false,
                traits: [
                    {
                        type: 'checkbox',
                        label: '文字',
                        name: 'btn_text',
                        valueTrue: "1",
                        valueFalse: "0"
                    }, {
                        type: 'checkbox',
                        label: '圖片',
                        name: 'btn_grid',
                        valueTrue: "1",
                        valueFalse: "0"
                    }, {
                        type: 'checkbox',
                        label: '圖文',
                        name: 'btn_list',
                        valueTrue: "1",
                        valueFalse: "0"
                    }
                ]
            },
            init() {

                var self = this;

                var list = ["btn_text", "btn_grid", "btn_list"];
                for (var i = 0; i < list.length; i++) {
                    const myClass = list[i];

                    self.on(`change:attributes:${myClass}`, () => {
                        self.components().models.forEach(function (item) {
                            if (item.getClasses().indexOf(myClass) >= 0) {

                                if (item.getClasses().indexOf('d-none') >= 0) {
                                    item.removeClass("d-none");
                                    setTimeout(() => {
                                        var content = $(".gjs-frame")[0].contentWindow.namecontrol;
                                        content(self.getId());
                                    }, 200);
                                }
                                else {
                                    item.addClass("d-none");
                                    setTimeout(() => {
                                        var content = $(".gjs-frame")[0].contentWindow.namecontrol;
                                        content(self.getId());
                                    }, 200);
                                }
                            }
                        });

                    });
                }
            }
        },
    });
    const textType = editor.DomComponents.getType("text");
    //文字
    editor.DomComponents.addType('span', {
        // Define the Model
        model: textType.model.extend({},
            {
                isComponent(el) {
                    if (el.tagName === 'SPAN') {
                        return {
                            type: 'text',
                            src: el.src,
                            tagName: el.tagName.toLowerCase(),
                            content: el.innerHTML,
                            editable: true

                        }
                    }
                }
            }),
        view: textType.view
    });
    //BGCanvas 動畫
    editor.DomComponents.addType('動畫背景', {
        isComponent: el => el.classList?.contains('.BGCanvas'),
        model: {
            defaults: {
                droppable: false,
                copyable: false
            },
            init() {
                const self = this;
                const ccid = self.ccid;
                const c = $(".gjs-frame")[0].contentWindow.$;

                window.setTimeout(function () {
                    console.log("in");
                    c(`#${ccid}`).BGCanvas();
                }, 200)
            }
        },
    });
    //關閉所有元件分類夾，僅開啟一個
    var blockControl = function () {
        $(categories.models).each(function (index, category) {
            category.set('open', index == 0).on('change:open', function (opened) {
                opened.get('open') && categories.each(category => {
                    if (category !== opened) {
                        category.set('open', false)
                    }
                })
            });
        });
    }
    //檢驗元件是否已載入，若載入成功則設定關閉控制
    editor.on('load', () => {
        blockControl();
    })

    //載入儲存的元件
    settings.getComponer().done(function (result) {
        $(result).each(function () {
            const html = co.Data.HtmlDecode(this.html);
            const elementHtmlCss = `${html}<style>${this.css}</style>`;
            let blockId = 'customBlockTemplate_' + this.id;
            let iconText = this.icon.replace("material-symbols-outlined", "").trim();
            let media = "";
            if (/^fa/.test(this.icon)) {
                media = `<i class="${this.icon} fa-5x"></i>`;
            } else if (/material-symbols-outlined/.test(this.icon)) {
                media = `<i class="material-symbols-outlined fa-5x">${iconText}</i>`;
            }
            appendBlock(blockId, {
                category: this.typeName,
                attributes: { custom_block_template: true },
                label: `${this.title}`,
                media: media,
                content: elementHtmlCss,
            });
        });
    });

    //產生元件
    const getCssRules = (selected, myRule, top) => {
        const id = selected.getId();
        const itemClass = selected.getClasses();
        const tagName = selected.get('tagName');
        const cssComposer = editor.CssComposer;
        const rules = cssComposer.getAll();
        const relatedIdRules = rules.filter(rule => {
            const ruleStr = rule.toCSS();
            const hasIdOrTagName =
                (itemClass.some(cls => ruleStr.includes(`.${cls}`)) && top.map(e => ruleStr.includes(e))) ||
                ruleStr.includes(`#${id}`) ||
                (ruleStr.includes(tagName) && top.map(e => ruleStr.includes(e)));
            return hasIdOrTagName;
        });
        co.Array.merge(myRule, relatedIdRules);
    }

    const findComponentStyles = function (selected, myRule, top) {
        if (selected) {
            const childModel = selected.components().models
            if (typeof (top) == "undefined") top = [];
            if (childModel) {
                top.push(...selected.get('tagName'));
                for (const model of childModel) {
                    findComponentStyles(model, myRule, top)
                }
            }
            getCssRules(selected, myRule, top);
        }
    }
    const createBlockTemplate = function (selected, name_blockId) {
        const blockId = name_blockId.blockId;
        const name = name_blockId.name;
        const relatedRules = [];
        let elementHTML = $(selected.getEl().outerHTML).removeClass("gjs-selected")[0].outerHTML;
        let first_partHtml = elementHTML.substring(0, elementHTML.indexOf(' '));
        let second_partHtml = elementHTML.substring(elementHTML.indexOf(' ') + 1);
        first_partHtml += ` custom_block_template=true block_id="${blockId}" `;
        let finalHtml = first_partHtml + second_partHtml;
        let icon = $('#NewBlockicon').val();
        findComponentStyles(selected, relatedRules);
        let blockCss = "";
        relatedRules.forEach(rule => {
            blockCss += rule.toCSS();
        });
        const css = `<style>${blockCss}</style>`;
        const elementHtmlCss = finalHtml + css;
        const category = $('#ComponerTypeList>option:selected').text();
        const object = {
            id: name_blockId.id,
            Title: name,
            icon: icon,
            type: $('#ComponerTypeList>option:selected').val(),
            Html: $('<div/>').text(finalHtml).html(),
            css: blockCss,
            disp_opt: true
        }
        co.HtmlContent.AddUp(object).done(function (result) {
            if (result.success) {
                let iconText = "";
                if (/material-symbols-outlined/.test(icon)) iconText = icon.replace("material-symbols-outlined", "").trim();
                appendBlock(blockId, {
                    category: category,
                    attributes: { custom_block_template: true },
                    label: `${name}`,
                    media: `<i class="${icon} fa-5x">${iconText}</i>`,
                    content: elementHtmlCss,
                })
            } else co.sweet.error(result.error);
        });

    }
    let actionBlockId = null;
    const ContextMenu = function (options) {
        // 唯一实例
        let instance;

        // 创建实例方法
        function createMenu() {
            // todo
            const ul = document.createElement("ul");
            ul.classList.add("custom-context-menu");
            const { menus } = options;
            if (menus && menus.length > 0) {
                for (let menu of menus) {
                    const li = document.createElement("li");
                    li.textContent = menu.name;
                    li.onclick = menu.onClick;
                    ul.appendChild(li);
                }
            }
            const body = document.querySelector("body");
            body.appendChild(ul);
            return ul;
        }

        return {
            // 获取实例的唯一方式
            getInstance: function () {
                if (!instance) {
                    instance = createMenu();
                }
                return instance;
            },
        };
    };
    const menuSinglton = ContextMenu({
        menus: [
            {
                name: "加入至頁面",
                onClick: function (e) {
                    const block = BlockManager.get(actionBlockId);
                    editor.addComponents(block.attributes.content);
                },
            },
            {
                name: "刪除元件",
                onClick: function (e) {
                    removeBlock();
                },
            }
        ],
    });
    function showMenu(e) {
        const menus = menuSinglton.getInstance();
        menus.style.top = `${e.clientY}px`;
        menus.style.left = `${e.clientX}px`;
        menus.style.display = "block";
    }
    function hideMenu(e) {
        const menus = menuSinglton.getInstance();
        menus.style.display = "none";
    }
    function removeBlock() {
        const l = actionBlockId.split("_");
        const id = l[l.length - 1];
        console.log(id);
        co.HtmlContent.Delete(id).done(function (result) {
            if (result.success) {
                BlockManager.remove(actionBlockId);
                co.sweet.success("成功", null, true);
            } else co.sweet.error(result.error);
        });
    }

    document.addEventListener("click", hideMenu);
    const appendBlock = function (id, obj) {
        const blockId = `customBlockTemplate_${id}`;
        var mySetting = {
            type: "default",
            render: ({ model, el }) => {
                el.addEventListener('dblclick', () => {
                    co.sweet.confirm('即將刪除', co.sweet.TitleHilight(`是否要確認將{0}刪除?`, obj.label), '確認', "取消", function () {
                        actionBlockId = blockId;
                        removeBlock();
                    });
                }), el.addEventListener('contextmenu', (e) => {
                    e.preventDefault();
                    actionBlockId = blockId;
                    showMenu(e);
                })
            }
        }
        $.extend(true, mySetting, obj);
        BlockManager.add(blockId, mySetting);
    }
    const createBlockTemplateConfirmation = function () {
        const selected = editor.getSelected();
        const self = this;
        let name = $("#NewBlockName").val() || "新增";

        co.HtmlContent.AddUp({
            Title: name,
            Type: $('#ComponerTypeList>option:selected').val()
        }).done(function (result) {
            if (result.success) {
                let blockId = 'customBlockTemplate_' + result.message.split(' ').join('_')
                let name_blockId = {
                    'id': result.message,
                    'name': name,
                    'blockId': blockId
                }
                createBlockTemplate(selected, name_blockId);
                document.getElementById("ComFrm").reset();
            } else co.sweet.error(result.error);
        });
    }
    $(`<div id="setComponents" class="modal" tabindex="-1">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Modal title</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
          </div>
          <div class="modal-body">
            <form id="ComFrm" class="form-horizontal needs-validation" novalidate>
                <div class="form-floating mb-3 input-group">
                    <input type="hidden" name="icon">
                    <input type="text" class="form-control" id="NewBlockName" placeholder="請輸入新元件的名稱">
                    <label for="floatingInput">元件名稱</label>
                    <div class="input-group-append">
                        <button type="button" id="NewBlockicon" class="btn btn-outline-secondary"></button>
                    </div>
                </div>
                <div class="d-none">
                    <select name="ComponerTypeList" id="ComponerTypeList" class="form-select item-menu" aria-label="類別">
                        <option value="" disabled="disabled">類別</option>
                    </select>
                    <div class="invalid-feedback">類別必填</div>
                </div>
            </form>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
            <button type="button" class="btn btn-primary btn-save">Save changes</button>
          </div>
        </div>
      </div>
    </div>`).appendTo("body");
    const myModal = new bootstrap.Modal('#setComponents');
    const iconPicker = $('#NewBlockicon').iconpicker(settings.iconPickerOpt);
    co.HtmlContent.GetTypeList().done(function (result) {
        if (result.success) {
            var $s = $("#ComponerTypeList");
            $(result.type).each(function () {
                $s.append(`<option value="${this.value}">${this.key}</option>`);
            });
            if (result.type.length > 1) $s.parents(".d-none").removeClass("d-none");
        }
    });
    $('#setComponents').find(".btn-save").on("click", function () {
        createBlockTemplateConfirmation();
        myModal.hide();
        co.sweet.success("加入我的最愛");
    });
    iconPicker.on('change', function (e) {
        $('#NewBlockicon').val(e.icon);
    });


    //畫布內容儲存及發布
    if (settings.save != null) {
        panelManager.addButton('options', {
            id: 'panelSave',
            className: 'someClass',
            label: '<i title="儲存" class="fa fa-download"></i>',
            command: function (editor) {
                settings.save(editor.getHtml(), editor.getCss()).done(function () {
                    co.sweet.success("已儲存草稿");
                });
            },
            attributes: { title: 'save' },
            active: false,
        });
    }

    if (settings.import != null) {
        panelManager.addButton('options', {
            id: 'panelImport',
            className: 'someClass',
            label: '<i title="發布" class="fa fa-cloud-arrow-up""></i>',
            command: function (editor) {
                settings.import(editor.getHtml(), editor.getCss()).done(function () {
                    co.sweet.success("已儲存並發布");
                });
            },
            attributes: { title: 'save' },
            active: false,
        });
    }

    // 複製事件監聽
    editor.on('component:clone', (obj) => {
        const iframe = document.getElementsByClassName("gjs-frame")[0].contentWindow;
        const classList = obj.getClasses();
        obj.setAttributes({ id: 'id', 'data-key': '' });

        if (classList.indexOf("anchor_title") > -1) {
            var cont = iframe.document.getElementsByClassName("anchor_title").length;
            const timmer = function () {
                if (iframe.document.getElementsByClassName("anchor_title").length != cont) iframe.AnchorPointInit();
                else setTimeout(timmer, 100);
            }
            setTimeout(timmer, 100);
        } else if (classList.indexOf("swiper-slide") > -1) {
            var swiper = editor.getSelected().parent().parent().getEl().swiper;
            if (typeof (swiper) != "undefined") {
                var cont = iframe.document.getElementsByClassName("swiper-slide").length;
                const timmer = function () {
                    if (iframe.document.getElementsByClassName("swiper-slide").length != cont) swiper.update();
                    else setTimeout(timmer, 100);
                }
                setTimeout(timmer, 100);
            }
        } else if (classList.indexOf("one_swiper") > -1 || classList.indexOf("one_swiper_thumbs") > -1 || classList.indexOf("two_swiper") > -1 || classList.indexOf("four_swiper") > -1 || classList.indexOf("six_swiper") > -1) {
            var cont = iframe.document.getElementsByClassName("swiper").length;
            const timmer = function () {
                if (iframe.document.getElementsByClassName("swiper").length != cont) iframe.SwiperInit({ autoplay: false });
                else setTimeout(timmer, 100);
            }
            setTimeout(timmer, 100);
        }
    });

    // 刪除事件監聽
    editor.on('component:remove', (obj) => {
        const iframe = document.getElementsByClassName("gjs-frame")[0].contentWindow;
        const classList = obj.getClasses();
        obj.setAttributes({ id: 'id', 'data-key': '' });

        if (classList.indexOf("anchor_title") > -1) {
            var cont = iframe.document.getElementsByClassName("anchor_title").length;
            const timmer = function () {
                if (iframe.document.getElementsByClassName("anchor_title").length != cont) iframe.AnchorPointInit();
                else setTimeout(timmer, 100);
            }
            setTimeout(timmer, 100);
        } else if (classList.indexOf("swiper-slide") > -1) {
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

    // 挪動事件監聽

    editor.on('component:drag:end', (obj) => {
        const classList = obj.target.getClasses();
        const iframe = document.getElementsByClassName("gjs-frame")[0].contentWindow;
        if (classList.indexOf("anchor_title") > -1) iframe.AnchorPointInit();
        else if (classList.indexOf("swiper-slide") > -1) {
            var swiper = obj.target.parent().parent().getEl().swiper;
            swiper.update();
        } else if (classList.indexOf("one_swiper") > -1 || classList.indexOf("one_swiper_thumbs") > -1 || classList.indexOf("two_swiper") > -1 || classList.indexOf("four_swiper") > -1 || classList.indexOf("six_swiper") > -1) iframe.SwiperInit({ autoplay: false });
    });

    /*editor.on('selector:add', selector => {
        selector.set({
            // Can't be seen by the style manager, therefore even by the user
            private: true,
        })
    });*/
    /**************
     * 指令參考
     * ***********/
    //editor.addComponents('<div id="yui" class="cls">New component</div>');
});
