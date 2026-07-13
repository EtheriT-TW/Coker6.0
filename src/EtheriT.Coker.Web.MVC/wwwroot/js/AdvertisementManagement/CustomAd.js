(function (window, $, co) {
    "use strict";

    const CustomAdPage = {
        rootSelector: "#CustomAdPageRoot",
        directoryFormId: "DirectoryForm",
        advertiseFormId: "AdvertiseForm",

        initialized: false,
        hashPage: null,
        directoryGridEvent: null,
        advertiseGridEvent: null,
        directoryTags: null,
        advertiseTags: null,

        route: {
            version: 0,
            mode: "directory-list",
            directoryId: 0,
            advertiseId: 0
        },

        directoryVisible: true,
        directoryData: null,
        selectedFile: null,
        advertiseFileId: 0,
        mediaPreviewAdvertiseId: 0,
        mediaPreviewNavigationBusy: false,
        mediaPreviewBoundaryDirection: 0,

        init: function () {
            if (this.initialized) {
                if (this.hashPage) this.hashPage.refresh();
                return;
            }

            this.cacheElements();
            if (!this.$root.length) {
                console.warn("CustomAdPage: root not found");
                return;
            }

            this.initTags();
            this.initForms();
            this.bindEvents();
            this.initHashPage();
            this.applyPermission();
            this.initialized = true;
        },

        cacheElements: function () {
            this.$root = $(this.rootSelector);
            this.$topLine = this.$root.find("#TopLine");
            this.$directoryForm = this.$root.find("#" + this.directoryFormId);
            this.$advertiseForm = this.$root.find("#" + this.advertiseFormId);

            this.$directoryVisible = this.$root.find(".btn_display");
            this.$directoryTitle = this.$directoryForm.find(".title textarea");
            this.$directoryDescription = this.$directoryForm.find(".description textarea");
            this.$sortBy = this.$directoryForm.find("#SortBy");

            this.$adType = this.$advertiseForm.find("#AdType");
            this.$adPreview = this.$advertiseForm.find(".ad_preview");
            this.$adLinkGroup = this.$advertiseForm.find(".ad_link");
            this.$adLink = this.$adLinkGroup.find("input[name='Link']");
            this.$imageInput = this.$advertiseForm.find(".input_pic");
            this.$videoInput = this.$advertiseForm.find(".input_video");
            this.$imagePreview = this.$advertiseForm.find(".img_preview");
            this.$videoPreview = this.$advertiseForm.find(".video_preview");
            this.$youtubePreview = this.$advertiseForm.find(".youtube iframe");
            this.$advertiseHeader = this.$root.find("[data-advertise-header]");
            this.$mediaPreviewModal = this.$root.find("#AdvertiseMediaPreviewModal");
            this.$mediaPreviewTitle = this.$mediaPreviewModal.find(".modal-title");
            this.$mediaPreviewImage = this.$mediaPreviewModal.find(".advertise-media-preview-image");
            this.$mediaPreviewVideo = this.$mediaPreviewModal.find(".advertise-media-preview-video");
            this.$mediaPreviewYoutube = this.$mediaPreviewModal.find(".advertise-media-preview-youtube");
            this.$mediaPreviewPrevious = this.$mediaPreviewModal.find("[data-media-preview-previous]");
            this.$mediaPreviewNext = this.$mediaPreviewModal.find("[data-media-preview-next]");
        },

        initTags: function () {
            this.directoryTags = this.$directoryForm.find(".InputTag").TagListModalInit();
            this.advertiseTags = this.$advertiseForm.find(".InputTag").TagListModalInit();
        },

        initForms: function () {
            const self = this;

            co.Form.init(this.directoryFormId, function () {
                if (!self.hasDirectoryTags()) {
                    Coker.sweet.error("錯誤", "標籤不可為空", null, false);
                    return $.Deferred().reject().promise();
                }

                return co.Form.confirmSubmit({
                    title: "即將儲存",
                    text: "儲存後將顯示於安排的位置",
                    confirmButtonText: "儲存",
                    cancelButtonText: "取消",
                    onConfirm: function () {
                        return self.saveDirectory();
                    }
                });
            });

            co.Form.init(this.advertiseFormId, function () {
                return co.Form.confirmSubmit({
                    title: "即將儲存",
                    text: "儲存後將顯示於廣告列表",
                    confirmButtonText: "儲存",
                    cancelButtonText: "取消",
                    onConfirm: function () {
                        return self.saveAdvertise();
                    }
                });
            });
        },

        initHashPage: function () {
            const self = this;

            this.hashPage = Coker.HashPage.create({
                root: this.rootSelector,
                defaultHash: "List",
                listHash: "List",
                newHash: "new",
                listPageKey: "DirectoryList",
                contentPageKey: "DirectoryContent",
                titleSelector: "[data-hash-title]",
                useStack: true,
                parseState: function (hash) {
                    return self.parseRoute(hash);
                },
                onChange: function (route) {
                    self.enterRoute(route);
                }
            });
        },

        parseRoute: function (hash) {
            const value = String(hash || "").trim();
            let match = null;

            if (!value || value.toLowerCase() === "list") {
                return {
                    raw: "List",
                    mode: "directory-list",
                    pageKey: "DirectoryList",
                    title: "目錄管理",
                    directoryId: 0,
                    advertiseId: 0
                };
            }

            if (value.toLowerCase() === "new" || value === "0") {
                return {
                    raw: value,
                    mode: "directory-new",
                    pageKey: "DirectoryContent",
                    title: "新增目錄",
                    directoryId: 0,
                    advertiseId: 0
                };
            }

            if (/^\d+$/.test(value)) {
                return {
                    raw: value,
                    mode: "directory-edit",
                    pageKey: "DirectoryContent",
                    title: "編輯目錄",
                    directoryId: parseInt(value, 10),
                    advertiseId: 0
                };
            }

            match = /^Advertise_(\d+)$/i.exec(value);
            if (match) {
                return {
                    raw: value,
                    mode: "advertise-list",
                    pageKey: "AdvertiseList",
                    title: "廣告列表",
                    directoryId: parseInt(match[1], 10),
                    advertiseId: 0
                };
            }

            match = /^AdvertiseEditor_(\d+)_(\d+)$/i.exec(value);
            if (match) {
                const advertiseId = parseInt(match[2], 10);
                return {
                    raw: value,
                    mode: advertiseId > 0 ? "advertise-edit" : "advertise-new",
                    pageKey: "AdvertiseContent",
                    title: advertiseId > 0 ? "編輯廣告" : "新增廣告",
                    directoryId: parseInt(match[1], 10),
                    advertiseId: advertiseId
                };
            }

            return {
                raw: "List",
                mode: "directory-list",
                pageKey: "DirectoryList",
                title: "目錄管理",
                directoryId: 0,
                advertiseId: 0
            };
        },

        enterRoute: function (route) {
            this.route = {
                version: this.route.version + 1,
                mode: route.mode,
                directoryId: Number(route.directoryId || 0),
                advertiseId: Number(route.advertiseId || 0)
            };

            this.$topLine.toggleClass("d-none", route.mode !== "directory-list");

            switch (route.mode) {
                case "directory-list":
                    this.enterDirectoryList();
                    break;
                case "directory-new":
                    this.enterDirectoryNew();
                    break;
                case "directory-edit":
                    this.enterDirectoryEdit(this.route.directoryId);
                    break;
                case "advertise-list":
                    this.enterAdvertiseList(this.route.directoryId);
                    break;
                case "advertise-new":
                    this.enterAdvertiseEditor(this.route.directoryId, 0);
                    break;
                case "advertise-edit":
                    this.enterAdvertiseEditor(this.route.directoryId, this.route.advertiseId);
                    break;
                default:
                    this.hashPage.goList();
                    break;
            }
        },

        isCurrentRoute: function (version, mode, directoryId, advertiseId) {
            return this.route.version === version &&
                this.route.mode === mode &&
                this.route.directoryId === Number(directoryId || 0) &&
                this.route.advertiseId === Number(advertiseId || 0);
        },

        bindEvents: function () {
            const self = this;

            this.$root.find("#DirectoryList .btn_add")
                .off("click.customAd")
                .on("click.customAd", function (event) {
                    event.preventDefault();
                    self.hashPage.goNew();
                });

            this.$root.find("#DirectoryContent .btn_back")
                .off("click.customAd")
                .on("click.customAd", function (event) {
                    event.preventDefault();
                    self.confirmLeave("返回目錄列表", function () {
                        self.hashPage.goList();
                    });
                });

            this.$root.find("#DirectoryItemps .btn_add")
                .off("click.customAd")
                .on("click.customAd", function (event) {
                    event.preventDefault();
                    self.goAdvertiseEditor(0);
                });

            this.$root.find("#DirectoryItemps .btn_back")
                .off("click.customAd")
                .on("click.customAd", function (event) {
                    event.preventDefault();
                    self.hashPage.goList();
                });

            this.$root.find("#AdvertiseContent .btn_back")
                .off("click.customAd")
                .on("click.customAd", function (event) {
                    event.preventDefault();
                    self.confirmLeave(self.getAdvertiseBackTitle(), function () {
                        self.goAdvertiseList(self.route.directoryId);
                    });
                });

            this.$directoryVisible
                .off("click.customAd")
                .on("click.customAd", function (event) {
                    event.preventDefault();
                    self.setDirectoryVisible(!self.directoryVisible);
                });

            this.$directoryTitle
                .off("input.customAd")
                .on("input.customAd", function () {
                    self.updateTextCount($(this));
                });

            this.$directoryDescription
                .off("input.customAd")
                .on("input.customAd", function () {
                    self.updateTextCount($(this));
                });

            this.$advertiseForm.find("textarea[name='describe']")
                .off("input.customAd")
                .on("input.customAd", function () {
                    self.updateTextCount($(this));
                });

            this.$adType
                .off("change.customAd")
                .on("change.customAd", function () {
                    self.selectedFile = null;
                    self.showAdvertiseType(Number($(this).val() || 0));
                });

            this.$advertiseForm.find(".btn_input_pic")
                .off("click.customAd")
                .on("click.customAd", function (event) {
                    event.preventDefault();
                    self.$imageInput.trigger("click");
                });

            this.$advertiseForm.find(".btn_input_video")
                .off("click.customAd")
                .on("click.customAd", function (event) {
                    event.preventDefault();
                    self.$videoInput.trigger("click");
                });

            this.$imageInput
                .off("change.customAd")
                .on("change.customAd", function (event) {
                    self.readSelectedFile(event.target.files[0], "image");
                });

            this.$videoInput
                .off("change.customAd")
                .on("change.customAd", function (event) {
                    self.readSelectedFile(event.target.files[0], "video");
                });

            this.$advertiseForm.find(".btn_preview")
                .off("click.customAd")
                .on("click.customAd", function (event) {
                    event.preventDefault();
                    self.previewYoutube();
                });

            this.$root
                .off("click.customAdMedia", ".advertise-media-trigger")
                .on("click.customAdMedia", ".advertise-media-trigger", function () {
                    self.showMediaPreviewModal($(this));
                });

            this.$mediaPreviewModal
                .off("hidden.bs.modal.customAd")
                .on("hidden.bs.modal.customAd", function () {
                    self.resetMediaPreviewModal();
                });

            this.$mediaPreviewPrevious
                .off("click.customAd")
                .on("click.customAd", function () {
                    self.navigateMediaPreview(-1);
                });

            this.$mediaPreviewNext
                .off("click.customAd")
                .on("click.customAd", function () {
                    self.navigateMediaPreview(1);
                });
        },

        applyPermission: function () {
            const self = this;

            co.PowerManagement.GetPermission().done(function (permission) {
                if (!permission || permission.CanCreate) return;
                self.$root.find(".btn_add").remove();
            });
        },

        confirmLeave: function (title, onConfirm) {
            Coker.sweet.confirm(title, "資料將不被保存", "確定", "取消", onConfirm);
        },

        updateTextCount: function ($input) {
            $input.closest(".title, .description, .describe")
                .find(".count")
                .text(String($input.val() || "").length);
        },

        enterDirectoryList: function () {
            this.directoryData = null;
            this.refreshGrid(this.directoryGridEvent);
        },

        enterDirectoryNew: function () {
            this.clearDirectoryForm();
        },

        enterDirectoryEdit: function (directoryId) {
            const self = this;
            const version = this.route.version;

            this.clearDirectoryForm();

            co.Directory.Get(directoryId)
                .done(function (result) {
                    if (!self.isCurrentRoute(version, "directory-edit", directoryId, 0)) return;
                    if (!result) {
                        self.hashPage.goList();
                        return;
                    }

                    self.fillDirectoryForm(result);
                })
                .fail(function () {
                    if (self.isCurrentRoute(version, "directory-edit", directoryId, 0)) {
                        Coker.sweet.error("錯誤", "讀取目錄資料失敗", null, false);
                    }
                });
        },

        enterAdvertiseList: function (directoryId) {
            const self = this;
            const version = this.route.version;

            this.directoryData = null;
            this.refreshGrid(this.advertiseGridEvent);

            co.Directory.Get(directoryId).done(function (result) {
                if (!self.isCurrentRoute(version, "advertise-list", directoryId, 0)) return;
                if (!result) {
                    self.hashPage.goList();
                    return;
                }

                self.directoryData = result;
            });
        },

        enterAdvertiseEditor: function (directoryId, advertiseId) {
            const self = this;
            const mode = advertiseId > 0 ? "advertise-edit" : "advertise-new";
            const version = this.route.version;

            this.clearAdvertiseForm(advertiseId);

            const directoryRequest = co.Directory.Get(directoryId);
            directoryRequest.done(function (result) {
                if (!self.isCurrentRoute(version, mode, directoryId, advertiseId)) return;
                if (!result) {
                    self.hashPage.goList();
                    return;
                }

                self.directoryData = result;
                self.$advertiseHeader.text(`返回${result.title || ""}廣告列表`);

                if (advertiseId === 0) {
                    self.advertiseTags.TagDataSet(result.tagDatas || []);
                }
            });

            if (advertiseId === 0) return;

            $.when(
                co.Advertise.GetDataOne(advertiseId),
                co.File.getAdFile(advertiseId, 10)
            ).done(function (advertiseResult, fileResult) {
                if (!self.isCurrentRoute(version, mode, directoryId, advertiseId)) return;

                // $.when 搭配多個 jqXHR 時，每個結果格式為 [data, textStatus, jqXHR]。
                const advertiseData = Array.isArray(advertiseResult) && advertiseResult.length === 3
                    ? advertiseResult[0]
                    : advertiseResult;
                const fileData = Array.isArray(fileResult) && fileResult.length === 3
                    ? fileResult[0]
                    : fileResult;

                if (!advertiseData || Number(advertiseData.id) !== advertiseId) {
                    self.goAdvertiseList(directoryId);
                    return;
                }

                self.fillAdvertiseForm(advertiseData, fileData || [], advertiseId);
            }).fail(function () {
                if (self.isCurrentRoute(version, mode, directoryId, advertiseId)) {
                    Coker.sweet.error("錯誤", "讀取廣告資料失敗", null, false);
                }
            });
        },

        clearDirectoryForm: function () {
            co.Form.clear(this.directoryFormId);
            this.directoryTags.TagDataClear();
            this.setDirectoryVisible(true);
            this.$sortBy.val(0);
            this.$directoryTitle.val("");
            this.$directoryDescription.val("");
            this.updateTextCount(this.$directoryTitle);
            this.updateTextCount(this.$directoryDescription);
        },

        fillDirectoryForm: function (result) {
            this.setDirectoryVisible(!!result.visible);
            this.$sortBy.val(result.sortBy);
            this.$directoryTitle.val(result.title || "");
            this.$directoryDescription.val(result.description || "");
            this.directoryTags.TagDataSet(result.tagDatas || []);
            this.updateTextCount(this.$directoryTitle);
            this.updateTextCount(this.$directoryDescription);
        },

        setDirectoryVisible: function (visible) {
            this.directoryVisible = !!visible;
            this.$directoryVisible.children("span")
                .text(this.directoryVisible ? "visibility" : "visibility_off");
        },

        hasDirectoryTags: function () {
            const tags = this.directoryTags.data("tagList") || [];
            return tags.length > 0;
        },

        saveDirectory: function () {
            const self = this;
            const version = this.route.version;
            const directoryId = this.route.mode === "directory-edit" ? this.route.directoryId : 0;

            return co.Directory.AddUp({
                Id: directoryId,
                Title: this.$directoryTitle.val(),
                Description: this.$directoryDescription.val(),
                Type: 4,
                Visible: this.directoryVisible,
                TagSelected: this.directoryTags.data("tagList") || [],
                SortBy: this.$sortBy.val()
            }).done(function () {
                Coker.sweet.success("已成功儲存", null, true);
                self.refreshGrid(self.directoryGridEvent);

                if (self.route.version === version) {
                    self.hashPage.goList();
                }
            }).fail(function () {
                Coker.sweet.error("錯誤", "儲存發生未知錯誤", null, true);
            });
        },

        clearAdvertiseForm: function (advertiseId) {
            co.Form.clear(this.advertiseFormId);
            this.advertiseTags.TagDataClear();
            this.selectedFile = null;
            this.advertiseFileId = 0;
            this.$adType.val("");
            this.resetAdvertisePreview();
            this.$advertiseForm.find("input[name='id']").val(Number(advertiseId || 0));
            this.$advertiseForm.find("#TargetCheck").prop("checked", false);
            this.$advertiseForm.find("#AdvertiseFormVisible").prop("checked", true);
            this.$advertiseForm.find("#PermanentCheck").prop("checked", true).trigger("change");
            this.$advertiseForm.find("textarea[name='describe']").val("").trigger("input");
        },

        fillAdvertiseForm: function (result, files, expectedId) {
            result.startEndDate = 0;
            result.sortCheckbox = 1;

            co.Form.insertData(result, "#" + this.advertiseFormId);

            // 表單資料只能由目前 route 決定，不接受非同步結果改寫成其他 ID。
            this.$advertiseForm.find("input[name='id']").val(expectedId);
            this.$advertiseForm.find("#TargetCheck").prop("checked", !!result.target);
            this.advertiseTags.TagDataSet(result.tagDatas || []);
            this.$advertiseForm.find("textarea[name='describe']").trigger("input");

            const file = Array.isArray(files) ? files[0] : null;
            if (!file) {
                this.$adType.val("");
                this.resetAdvertisePreview();
                return;
            }

            this.advertiseFileId = Number(file.id || 0);
            this.$adType.val(file.fileType);
            this.showAdvertiseType(Number(file.fileType || 0));

            if (Number(file.fileType) === 1) {
                this.$imagePreview.attr({ src: file.link, alt: file.name || "" }).removeClass("d-none");
                this.$advertiseForm.find(".btn_input_pic > span").addClass("d-none");
                this.$adLink.val(result.link || "");
            } else if (Number(file.fileType) === 2) {
                this.$videoPreview.attr({ type: file.video_Type || "", src: file.link || "" });
            } else if (Number(file.fileType) === 3) {
                this.$adLink.val(file.link || result.link || "");
                this.previewYoutube();
            }
        },

        resetAdvertisePreview: function () {
            this.$adPreview.children("div").addClass("d-none");
            this.$adPreview.children(".preview").removeClass("d-none");
            this.$imagePreview.attr({ src: "", alt: "" }).addClass("d-none");
            this.$advertiseForm.find(".btn_input_pic > span").removeClass("d-none");
            this.$videoPreview.attr({ type: "", src: "" });
            this.$youtubePreview.attr("src", "");
            this.$adLink.val("").removeAttr("required");
            this.$adLinkGroup.addClass("d-none");
            this.$adLinkGroup.find(".checkbox, .btn_preview").addClass("d-none");
        },

        showAdvertiseType: function (type) {
            this.resetAdvertisePreview();

            if (type === 1) {
                this.$adPreview.children(".preview").addClass("d-none");
                this.$adPreview.children(".image").removeClass("d-none");
                this.$adLinkGroup.removeClass("d-none");
                this.$adLink.attr({ required: "required", placeholder: "輸入連結網址" });
                this.$adLinkGroup.find(".checkbox").removeClass("d-none");
            } else if (type === 2) {
                this.$adPreview.children(".preview").addClass("d-none");
                this.$adPreview.children(".video").removeClass("d-none");
            } else if (type === 3) {
                this.$adPreview.children(".preview").addClass("d-none");
                this.$adLinkGroup.removeClass("d-none");
                this.$adLink.attr({ required: "required", placeholder: "https://www.youtube.com/watch?v=" });
                this.$adLinkGroup.find(".btn_preview").removeClass("d-none");
            }
        },

        readSelectedFile: function (file, type) {
            if (!file) return;

            const self = this;
            const version = this.route.version;
            const reader = new FileReader();

            reader.onload = function (event) {
                if (self.route.version !== version) return;

                self.selectedFile = {
                    id: 0,
                    File: file,
                    name: file.name,
                    link: event.target.result
                };

                if (type === "image") {
                    self.$imagePreview.attr({ src: event.target.result, alt: file.name }).removeClass("d-none");
                    self.$advertiseForm.find(".btn_input_pic > span").addClass("d-none");
                } else {
                    self.$videoPreview.attr({ type: file.type, src: event.target.result });
                }
            };

            reader.readAsDataURL(file);
        },

        getYoutubeId: function (value) {
            const raw = String(value || "").trim();
            if (!raw) return "";

            try {
                const url = new URL(raw);
                if (url.hostname.indexOf("youtu.be") >= 0) {
                    return url.pathname.replace(/^\//, "").split("/")[0];
                }

                const queryId = url.searchParams.get("v");
                if (queryId) return queryId;

                const match = /\/(?:embed|shorts)\/([^/?]+)/.exec(url.pathname);
                if (match) return match[1];
            } catch (error) {
                if (/^[\w-]{6,}$/.test(raw)) return raw;
            }

            return "";
        },

        previewYoutube: function () {
            const videoId = this.getYoutubeId(this.$adLink.val());
            if (!videoId) return;

            this.$adPreview.children("div").addClass("d-none");
            this.$adPreview.children(".youtube").removeClass("d-none");
            this.$youtubePreview.attr("src", "https://www.youtube-nocookie.com/embed/" + videoId);
        },

        showMediaPreviewModal: function ($trigger) {
            const item = {
                Id: Number($trigger.attr("data-advertise-id") || 0),
                MediaType: Number($trigger.attr("data-media-type") || 0),
                MainImage: String($trigger.attr("data-media-src") || ""),
                Title: String($trigger.attr("data-media-title") || "廣告預覽")
            };
            if (!this.renderMediaPreview(item)) return;

            bootstrap.Modal.getOrCreateInstance(this.$mediaPreviewModal[0]).show();
        },

        renderMediaPreview: function (item) {
            const type = Number(item && item.MediaType || 0);
            const source = String(item && item.MainImage || "");
            const title = String(item && item.Title || "廣告預覽");
            if (!item || !item.Id || !type || !source) return false;

            this.clearMediaPreviewContent();
            this.mediaPreviewAdvertiseId = Number(item.Id);
            this.mediaPreviewBoundaryDirection = 0;
            this.$mediaPreviewTitle.text(title);

            if (type === 1) {
                this.$mediaPreviewImage.attr({ src: source, alt: title }).removeClass("d-none");
            } else if (type === 2) {
                this.$mediaPreviewVideo.attr("src", source).removeClass("d-none");
                if (this.$mediaPreviewVideo[0]) this.$mediaPreviewVideo[0].load();
            } else if (type === 3) {
                const videoId = this.getYoutubeId(source);
                if (!videoId) return false;
                this.$mediaPreviewYoutube
                    .attr("src", "https://www.youtube-nocookie.com/embed/" + videoId)
                    .removeClass("d-none");
            } else {
                return false;
            }

            this.updateMediaPreviewNavigation();
            return true;
        },

        getAdvertiseGridComponent: function () {
            return this.advertiseGridEvent && this.advertiseGridEvent.component
                ? this.advertiseGridEvent.component
                : null;
        },

        getPageMediaPreviewItems: function (component) {
            if (!component) return [];

            return component.getVisibleRows()
                .filter(function (row) {
                    return row.rowType === "data" && row.data && row.data.MediaType && row.data.MainImage;
                })
                .map(function (row) {
                    return row.data;
                });
        },

        updateMediaPreviewNavigation: function () {
            const component = this.getAdvertiseGridComponent();
            const items = this.getPageMediaPreviewItems(component);
            const currentId = this.mediaPreviewAdvertiseId;
            const currentIndex = items.findIndex(function (item) {
                return Number(item.Id) === currentId;
            });
            const pageIndex = component ? component.pageIndex() : 0;
            const pageCount = component ? component.pageCount() : 0;
            const hasPrevious = this.mediaPreviewBoundaryDirection !== -1 &&
                (currentIndex > 0 || (currentIndex >= 0 && pageIndex > 0));
            const hasNext = currentIndex >= 0 &&
                this.mediaPreviewBoundaryDirection !== 1 &&
                (currentIndex < items.length - 1 || pageIndex < pageCount - 1);

            this.$mediaPreviewPrevious.prop("disabled", this.mediaPreviewNavigationBusy || !hasPrevious);
            this.$mediaPreviewNext.prop("disabled", this.mediaPreviewNavigationBusy || !hasNext);
        },

        navigateMediaPreview: function (direction) {
            if (this.mediaPreviewNavigationBusy || (direction !== -1 && direction !== 1)) return;

            const component = this.getAdvertiseGridComponent();
            if (!component) return;

            const items = this.getPageMediaPreviewItems(component);
            const currentId = this.mediaPreviewAdvertiseId;
            const currentIndex = items.findIndex(function (item) {
                return Number(item.Id) === currentId;
            });
            const adjacentIndex = currentIndex + direction;

            if (currentIndex >= 0 && adjacentIndex >= 0 && adjacentIndex < items.length) {
                this.renderMediaPreview(items[adjacentIndex]);
                return;
            }

            this.mediaPreviewNavigationBusy = true;
            this.updateMediaPreviewNavigation();
            const originPageIndex = component.pageIndex();
            this.loadMediaPreviewPage(component, originPageIndex + direction, direction, originPageIndex);
        },

        loadMediaPreviewPage: function (component, pageIndex, direction, originPageIndex) {
            const self = this;
            const pageCount = component.pageCount();

            if (pageIndex < 0 || pageIndex >= pageCount) {
                $.when(component.pageIndex(originPageIndex)).always(function () {
                    self.mediaPreviewNavigationBusy = false;
                    self.mediaPreviewBoundaryDirection = direction;
                    self.updateMediaPreviewNavigation();
                });
                return;
            }

            $.when(component.pageIndex(pageIndex))
                .done(function () {
                    const items = self.getPageMediaPreviewItems(component);
                    if (!items.length) {
                        self.loadMediaPreviewPage(component, pageIndex + direction, direction, originPageIndex);
                        return;
                    }

                    const item = direction > 0 ? items[0] : items[items.length - 1];
                    self.mediaPreviewNavigationBusy = false;
                    self.renderMediaPreview(item);
                })
                .fail(function () {
                    self.mediaPreviewNavigationBusy = false;
                    self.updateMediaPreviewNavigation();
                });
        },

        clearMediaPreviewContent: function () {
            this.$mediaPreviewModal.find("[data-media-preview]").addClass("d-none");
            this.$mediaPreviewImage.attr({ src: "", alt: "" });
            this.$mediaPreviewYoutube.attr("src", "");

            if (this.$mediaPreviewVideo[0]) {
                this.$mediaPreviewVideo[0].pause();
                this.$mediaPreviewVideo.removeAttr("src");
                this.$mediaPreviewVideo[0].load();
            }
        },

        resetMediaPreviewModal: function () {
            this.clearMediaPreviewContent();
            this.mediaPreviewAdvertiseId = 0;
            this.mediaPreviewNavigationBusy = false;
            this.mediaPreviewBoundaryDirection = 0;
            this.updateMediaPreviewNavigation();
        },

        saveAdvertise: function () {
            const self = this;
            const snapshot = $.extend({}, this.route);
            const payload = co.Form.getJson(this.advertiseFormId);
            const deferred = $.Deferred();

            if (snapshot.mode !== "advertise-edit" && snapshot.mode !== "advertise-new") {
                Coker.sweet.error("錯誤", "目前頁面狀態無法儲存廣告", null, false);
                return deferred.reject().promise();
            }

            // 關鍵保護：儲存 ID 只取自 Hash route，不採用可能被舊回應改寫的 hidden input。
            delete payload.Id;
            payload.id = snapshot.advertiseId;

            co.Advertise.AddUp(payload)
                .done(function (result) {
                    if (!result || result.success === false || !result.message) {
                        deferred.reject(result);
                        return;
                    }

                    const savedId = Number(result.message);
                    self.uploadAdvertiseFile(savedId)
                        .done(function () {
                            Coker.sweet.success("已成功儲存", null, true);
                            self.refreshGrid(self.advertiseGridEvent);

                            if (self.route.version === snapshot.version) {
                                self.goAdvertiseList(snapshot.directoryId);
                            }

                            deferred.resolve(result);
                        })
                        .fail(function (error) {
                            deferred.reject(error);
                        });
                })
                .fail(function (error) {
                    deferred.reject(error);
                });

            deferred.fail(function () {
                Coker.sweet.error("錯誤", "儲存發生未知錯誤", null, true);
            });

            return deferred.promise();
        },

        uploadAdvertiseFile: function (advertiseId) {
            const type = Number(this.$adType.val() || 0);

            if ((type === 1 || type === 2) && this.selectedFile && this.selectedFile.File) {
                const formData = new FormData();
                formData.append("files", this.selectedFile.File);
                formData.append("type", 10);
                formData.append("sid", advertiseId);
                formData.append("serno", 500);
                return co.File.Upload(formData);
            }

            if (type === 3) {
                const youtubeId = this.getYoutubeId(this.$adLink.val());
                if (!youtubeId) return $.Deferred().reject().promise();

                return co.File.UploadYTLink({
                    Id: this.advertiseFileId || 0,
                    SId: advertiseId,
                    File: youtubeId,
                    Type: 10,
                    SerNo: 500
                });
            }

            return $.Deferred().resolve().promise();
        },

        getAdvertiseBackTitle: function () {
            const title = this.directoryData && this.directoryData.title
                ? this.directoryData.title
                : "";
            return `返回${title}廣告列表`;
        },

        goAdvertiseList: function (directoryId) {
            if (!directoryId) {
                this.hashPage.goList();
                return;
            }
            this.hashPage.setHash("Advertise_" + directoryId);
        },

        goAdvertiseEditor: function (advertiseId) {
            if (!this.route.directoryId) return;
            this.hashPage.setHash(`AdvertiseEditor_${this.route.directoryId}_${Number(advertiseId || 0)}`);
        },

        refreshGrid: function (gridEvent) {
            if (gridEvent && gridEvent.component) gridEvent.component.refresh();
        },

        onDirectoryGridReady: function (event) {
            this.directoryGridEvent = event;
        },

        onAdvertiseGridReady: function (event) {
            this.advertiseGridEvent = event;
            if (this.mediaPreviewAdvertiseId && !this.mediaPreviewNavigationBusy) {
                this.updateMediaPreviewNavigation();
            }
        },

        editDirectory: function (event) {
            this.hashPage.goId(event.row.key);
        },

        openAdvertiseList: function (event) {
            this.goAdvertiseList(event.row.key);
        },

        deleteDirectory: function (event) {
            Coker.sweet.confirm("刪除資料", "刪除後不可返回", "確定刪除", "取消", function () {
                co.Directory.Delete(event.row.key).done(function (result) {
                    if (result && result.success) event.component.refresh();
                });
            });
        },

        editAdvertise: function (event) {
            this.goAdvertiseEditor(event.row.key);
        },

        deleteAdvertise: function (event) {
            Coker.sweet.confirm("刪除資料", "刪除後不可返回", "確定刪除", "取消", function () {
                co.Advertise.Delete(event.row.key).done(function (result) {
                    if (result && result.success) event.component.refresh();
                });
            });
        }
    };

    window.CustomAdPage = CustomAdPage;
    window.PageReady = function () { CustomAdPage.init(); };

    // DevExtreme Razor callbacks 保留為薄介面，狀態與行為集中於 CustomAdPage。
    window.contentReady = function (event) { CustomAdPage.onDirectoryGridReady(event); };
    window.DirectoryDatailListReady = function (event) { CustomAdPage.onAdvertiseGridReady(event); };
    window.editButtonClicked = function (event) { CustomAdPage.editDirectory(event); };
    window.reladataButtonClicked = function (event) { CustomAdPage.openAdvertiseList(event); };
    window.deleteButtonClicked = function (event) { CustomAdPage.deleteDirectory(event); };
    window.editAdvertiseButtonClicked = function (event) { CustomAdPage.editAdvertise(event); };
    window.deleteAdvertiseButtonClicked = function (event) { CustomAdPage.deleteAdvertise(event); };
    window.GetDirectoryId = function () { return CustomAdPage.route.directoryId; };
    window.GetDirectoryType = function () { return "Advertise"; };

})(window, window.jQuery, window.co || window.Coker);
