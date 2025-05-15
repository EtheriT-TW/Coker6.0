function PageReady() {
    const sliders = [];
    let isDragging = false;
    function addSlider(data = {}) {
        const slider = {
            desktopImage: data.desktopImage || '/images/noImg.jpg',
            mobileImage: data.mobileImage || '/images/noImg.jpg',
            desktopFile: data.desktopFile || null,
            mobileFile: data.mobileFile || null,
            desktopDeleted: data.desktopDeleted || false,
            mobileDeleted: data.mobileDeleted || false,
            title: data.title || '',
            subtitle: data.subtitle || '',
            link: data.link || '',
            enabled: data.enabled ?? true
        };
        sliders.push(slider);
        renderSliders(); // ❗ 讓 UI 刷新
    }

    function renderSliders() {
        $('#sliderList .slider-item').not('.add-new').remove();

        sliders.forEach((slider, index) => {
            const $item = $(`
                <div class="slider-item" data-bs-toggle="tooltip" title="點擊編輯 Banner">
                    <div class="slider-preview-wrapper">
                        <img src="${slider.desktopImage}" alt="desktop-preview" class="slider-preview">
                    </div>
                    <div class="delete-icon material-symbols-outlined" title="刪除">delete</div>
                </div>
            `);
            $item.attr('data-index', index);

            $item.tooltip({ boundary: 'window' });

            $item.find(".slider-preview-wrapper").on("click", function () {
                if (!isDragging) {
                    openEdit(index);
                }
            });

            $item.find(".delete-icon").on("click", function () {
                deleteSlider(index);
            });

            $item.insertBefore('.slider-item.add-new');
        });

        refreshSortable();
    }

    function refreshSortable() {
        const $list = $('#sliderList');
        if ($list.data('ui-sortable')) {
            $list.sortable('destroy');
        }
        $list.sortable({
            axis: 'x',
            items: '.slider-item:not(.add-new)',
            helper: 'clone',
            placeholder: 'slider-placeholder',
            start: function (e, ui) {
                isDragging = true;
                if (ui.helper) ui.helper.css('cursor', 'grabbing');
            },
            stop: function (e, ui) {
                if (ui.helper) ui.helper.css('cursor', 'grab');
                setTimeout(() => isDragging = false, 100);
            },
            update: function () {
                const newOrder = [];
                $list.find('.slider-item').not('.add-new').each(function () {
                    const idx = $(this).data('index');
                    newOrder.push(sliders[idx]);
                });
                sliders.splice(0, sliders.length, ...newOrder);
                renderSliders();
            }
        });
    }

    function openEdit(index) {
        const s = sliders[index];
        $('#editIndex').val(index);
        $('#editTitle').val(s.title);
        $('#editSubtitle').val(s.subtitle);
        $('#editLink').val(s.link);
        $('#editEnabled').prop('checked', s.enabled);

        updateImagePreview('desktop', s.desktopFile || s.desktopImage);
        updateImagePreview('mobile', s.mobileFile || s.mobileImage);

        $('#editDesktopImageInput').val('').hide();
        $('#editMobileImageInput').val('').hide();

        new bootstrap.Modal(document.getElementById('editModal')).show();
    }
    function clearImage(type) {
        const index = $('#editIndex').val();
        const s = sliders[index];
        updateImagePreview(type, null);
        $(`#edit${capitalize(type)}ImageInput`).val('');
        s[`${type}Deleted`] = true;
    }
    function previewImage(input, type) {
        const file = input.files[0];
        if (file) {
            updateImagePreview(type, file);
            const i = $('#editIndex').val();
            sliders[i][`${type}Deleted`] = false;
        }
    }

    function updateImagePreview(type, value) {
        const index = $('#editIndex').val();
        const s = sliders[index];
        let url = '';
        if (value instanceof File) {
            url = URL.createObjectURL(value);
            if (type === 'desktop') {
                s.desktopFile = value;
                s.desktopImage = url;
            } else {
                s.mobileFile = value;
                s.mobileImage = url;
            }
            s[`${type}Deleted`] = false;
        } else {
            url = value || '/images/noImg.jpg';
            if (type === 'desktop') {
                s.desktopFile = null;
                s.desktopImage = url;
            } else {
                s.mobileFile = null;
                s.mobileImage = url;
            }
        }

        $(`#edit${capitalize(type)}ImagePreview`).attr('src', url);
    }
    function capitalize(str) {
        return str.charAt(0).toUpperCase() + str.slice(1);
    }
    function deleteSlider(index) {
        co.sweet.confirm("確定要刪除這個 Banner 嗎？", "", "確定", "取消", function () {
            sliders.splice(index, 1);
            renderSliders();
        });
    }

    function saveEdit() {
        const i = $('#editIndex').val();
        const s = sliders[i];

        s.title = $('#editTitle').val();
        s.subtitle = $('#editSubtitle').val();
        s.link = $('#editLink').val();
        s.enabled = $('#editEnabled').prop('checked');

        const desktopFile = $('#editDesktopImageInput')[0].files[0];
        const mobileFile = $('#editMobileImageInput')[0].files[0];

        if (desktopFile instanceof File && !s.desktopImage.startsWith('http')) {
            s.desktopFile = desktopFile;
            s.desktopImage = URL.createObjectURL(desktopFile);
        }

        if (mobileFile instanceof File && !s.mobileImage.startsWith('http')) {
            s.mobileFile = mobileFile;
            s.mobileImage = URL.createObjectURL(mobileFile);
        }

        bootstrap.Modal.getInstance(document.getElementById('editModal')).hide();
        renderSliders();
    }

    function loadTemplateSetting(data) {
        const config = data.contentConfig || {};
        $('#showMarquee').prop('checked', config.showMarquee);
        $('#showPagePath').prop('checked', config.showPagePath);
        $(`input[name="headType"][value="${data.headType}"]`).prop('checked', true);
        sliders.length = 0;
        (config.sliders || []).forEach(addSlider); // ✅ 只呼叫 addSlider，統一預設邏輯

        renderSliders();
    }

    function getTemplateSetting() {
        return {
            headType: parseInt($('input[name="headType"]:checked').val()),
            contentConfig: {
                showMarquee: $('#showMarquee').prop('checked'),
                showPagePath: $('#showPagePath').prop('checked'),
                sliders: sliders.map(s => ({
                    title: s.title,
                    subtitle: s.subtitle,
                    link: s.link,
                    enabled: s.enabled,
                    desktopImage: s.desktopDeleted ? null : s.desktopImage,
                    mobileImage: s.mobileDeleted ? null : s.mobileImage
                }))
            }
        };
    }

    //$('#LogoImageUpload').ImageUploadModalClear();
    //$('#LogoImageUpload').data('init', true);

    $("#sliderList .add-new").on("click", addSlider);

    $('#editDesktopImagePreview').on("click", function () {
        $('#editDesktopImageInput').trigger('click');
    });

    $('#editMobileImagePreview').on("click", function () {
        $('#editMobileImageInput').trigger('click');
    });

    $('#editDesktopImagePreview + .image-remove-icon').on("click", function () {
        clearImage("desktop");
    });

    $('#editMobileImagePreview + .image-remove-icon').on("click", function () {
        clearImage("mobile");
    });

    $('#editDesktopImageInput').on("change", function () {
        previewImage(this, 'desktop');
    });

    $('#editMobileImageInput').on("change", function () {
        previewImage(this, 'mobile');
    });

    $("#editModal .modal-footer .btn-primary").on("click", saveEdit);

    function uploadImages() {
        const imageList = [];
        /*const logoForm = co.Form.getFileForm("LogoImageUpload",12);
        // 上傳 Logo 圖片
        imageList.push(co.File.Upload(logoForm).done(function (result) {
            var _dfr = $.Deferred()
            if (result.success && result.files.length > 0) {
                const file = result.files[0];
                $("#LogoImageUpload").ImageUploadModalDataInsert(file.id, file.path, file.name);
                _dfr.resolve();
            } else _dfr.reject();
            return _dfr.promise();
        }));*/
        for (let i = 0; i < sliders.length; i++) {
            const slider = sliders[i];

            // 上傳桌機圖片
            if (slider.desktopFile instanceof File) {
                const formData = new FormData();
                formData.append('files', slider.desktopFile);
                formData.append('type', 0);
                imageList.push(co.File.Upload(formData).done(function (result) {
                    var _dfr = $.Deferred()
                    if (result.success && result.files.length > 0) {
                        const file = result.files[0];
                        slider.desktopImage = file.path; // ✅ 替換 blob 成實際路徑
                        slider.desktopFile = null;        // ✅ 上傳完成後清空 file
                        _dfr.resolve();
                    } else _dfr.reject();
                    return _dfr.promise();
                }));
            }
            // 上傳手機圖片
            if (slider.mobileFile instanceof File) {
                const formData = new FormData();
                formData.append('files', slider.mobileFile);
                formData.append('type', 0);
                imageList.push(co.File.Upload(formData).done(function (result) {
                    var _dfr = $.Deferred()
                    if (result.success && result.files.length > 0) {
                        const file = result.files[0];
                        slider.mobileImage = file.path;
                        slider.mobileFile = null;
                        _dfr.resolve();
                    } else _dfr.reject();
                    return _dfr.promise();
                }));
            }
        }
        return $.when.apply(null, imageList);
    }

    document.getElementById("headerSetting").addEventListener("submit", function (e) {
        e.preventDefault();
        uploadImages().done(function () {
            var data = getTemplateSetting();
            co.Templates.saveDefaultHeader(data).then(result => {
                if (result.success) {
                    co.sweet.success("儲存成功");
                } else co.sweet.error("儲存失敗",result.error);
            });
        });
    }, false);

    co.Templates.getDefaultHeader().then(result => {
        if (result.success) {
            loadTemplateSetting(result.object);
        }
    });

    const $box = $('#customLinkBox');
    let currentTarget = null;

    $('.image-link-icon').on('click', function (e) {
        const $target = $(e.currentTarget);
        const type = $target.data('type');
        const targetOffset = $target.offset(); // 絕對位置
        const containerOffset = $target.closest('.modal-body').offset(); // modal 內的參考座標

        currentTarget = $target;
        $('#customLinkBox')
            .removeClass('d-none')
            .css({
                top: targetOffset.top - containerOffset.top + $target.outerHeight() + 4,
                left: targetOffset.left - containerOffset.left,
                position: 'absolute'
            })
            .attr('data-type', type)
            .find('input')
            .val('')
            .focus();
    });

    // 點擊確認按鈕
    $('#customLinkBox .btn-primary').on('click', function (e) {
        e.preventDefault();
        e.stopPropagation();
        const url = $('#customLinkBox input').val().trim();
        if (url && isValidUrl(url)) {
            const type = $('#customLinkBox').attr('data-type');
            const index = $('#editIndex').val();
            const s = sliders[index];

            if (type === 'desktop') {
                $('#editDesktopImagePreview').attr('src', url);
                s.desktopFile = null;
                s.desktopImage = url;
                s.desktopDeleted = false;
            } else if (type === 'mobile') {
                $('#editMobileImagePreview').attr('src', url);
                s.mobileFile = null;
                s.mobileImage = url;
                s.mobileDeleted = false;
            }

            $('#customLinkBox').addClass('d-none');
        } else {
            co.sweet.error("請輸入有效的圖片網址");
        }
    });

    // 點擊外部隱藏
    $(document).on('click', function (e) {
        if (!$(e.target).closest('.url-input-box, .image-link-icon').length) {
            $box.addClass('d-none');
        }
    });

    function isValidUrl(str) {
        try {
            new URL(str);
            return true;
        } catch {
            return false;
        }
    }

    
    refreshSortable();
}
