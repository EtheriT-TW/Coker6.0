(function (global, $) {
    "use strict";

    function CokerMenuEditorForm(options) {
        const settings = $.extend(true, {
            root: "#MenuEditorForm",
            offcanvas: "#offcanvasSite",
            form: "#frmEdit",
            pageType: "#pageType",
            pageTypeDescription: "#PageTypeDescription",
            routerNameBlock: "#RouterNameBlock",
            routerName: "#routerName",
            linkUrlBlock: "#LinkUrlBlock",
            linkUrl: "#linkUrl",
            popular: "#popular",
            popularDisplay: "#popularDisplay",
            buttons: {
                update: "#btnUpdate",
                add: "#btnAdd",
                refresh: "#btnRefresh"
            },
            uploads: {
                icon: "#IconImageUpload",
                image: "#ImageUpload",
                overImage: "#OverImageUpload"
            },
            getPageTypes: null
        }, options || {});

        const $root = $(settings.root);
        const $form = $root.find(settings.form);
        let pageTypes = [];
        let initialized = false;

        function toBool(value) {
            return value === true || value === 1 || value === "true";
        }

        function find(selector) {
            return $root.find(selector);
        }

        function getPageTypeOption(value) {
            const pageType = parseInt(value ?? find(settings.pageType).val(), 10);
            return pageTypes.find(function (item) {
                return parseInt(item.value, 10) === pageType;
            }) || null;
        }

        function getUploadElement(slot) {
            const selector = settings.uploads[slot];
            return selector ? find(selector) : $();
        }

        function syncToggle($toggle) {
            const $input = $toggle.find('input[type="hidden"].item-menu').first();
            const enabled = toBool($input.val());
            const $button = $toggle.find(".menu-editor-toggle__button");
            const icon = enabled ? $toggle.data("icon-on") : $toggle.data("icon-off");
            const stateText = enabled
                ? ($toggle.attr("data-state-on") || "是")
                : ($toggle.attr("data-state-off") || "否");
            const labelText = $toggle.find(".menu-editor-toggle__label").text().trim();

            $toggle.toggleClass("is-active", enabled);
            $button
                .attr("aria-pressed", enabled.toString())
                .attr("title", `${labelText}：${stateText}`);
            $toggle.find("[data-menu-toggle-icon]").text(icon || (enabled ? "check" : "close"));
            $toggle.find("[data-menu-toggle-state]").text(stateText);
        }

        function bindToggles() {
            $root
                .off("click.menuEditorFormToggle", "[data-menu-toggle] .menu-editor-toggle__button")
                .on("click.menuEditorFormToggle", "[data-menu-toggle] .menu-editor-toggle__button", function () {
                    const $toggle = $(this).closest("[data-menu-toggle]");
                    const $input = $toggle.find('input[type="hidden"].item-menu').first();
                    $input.val((!toBool($input.val())).toString()).trigger("change");
                    syncToggle($toggle);
                });
        }

        this.initialize = function () {
            if (initialized) {
                return $.Deferred().resolve(pageTypes).promise();
            }

            initialized = true;
            this.clearUploads();
            this.syncPopularDisplay();
            bindToggles();
            this.syncToggles();

            if (typeof settings.getPageTypes !== "function") {
                return $.Deferred().resolve(pageTypes).promise();
            }

            const self = this;
            let request;
            try {
                request = settings.getPageTypes();
            } catch (error) {
                initialized = false;
                return $.Deferred().reject(error).promise();
            }

            const loading = request.then(function (result) {
                if (!result.success) {
                    return $.Deferred()
                        .reject(result.error || "無法取得頁面類型")
                        .promise();
                }

                pageTypes = result.type || [];
                const $select = find(settings.pageType);
                $select.empty();

                $(pageTypes).each(function () {
                    $select.append($("<option>", {
                        value: this.value,
                        text: this.key
                    }));
                });

                $select
                    .off("change.menuEditorForm")
                    .on("change.menuEditorForm", function () {
                        self.updatePageTypeUi();
                    });

                self.updatePageTypeUi();
                return pageTypes;
            });

            loading.fail(function () {
                initialized = false;
            });
            return loading;
        };

        this.updatePageTypeUi = function () {
            const option = getPageTypeOption();
            if (!option) return;

            const showRouterName = toBool(option.showRouterName);
            const showLinkUrl = toBool(option.showLinkUrl);
            const $routerNameBlock = find(settings.routerNameBlock);
            const $routerNameInput = find(settings.routerName);
            const $linkUrlBlock = find(settings.linkUrlBlock);
            const $linkUrlInput = find(settings.linkUrl);
            const $description = find(settings.pageTypeDescription);

            $description.text(option.description || "");
            $description.toggleClass("d-none", !option.description);
            $routerNameBlock.toggleClass("d-none", !showRouterName);
            $linkUrlBlock.toggleClass("d-none", !showLinkUrl);

            if (!showRouterName) {
                $routerNameInput.val(option.routerName || "");
            } else {
                const currentRouterName = ($routerNameInput.val() || "").trim();
                const isSystemRouterName = pageTypes.some(function (item) {
                    return item.routerName && item.routerName === currentRouterName;
                });

                if (isSystemRouterName) {
                    $routerNameInput.val("");
                }
            }

            if (!showLinkUrl) {
                $linkUrlInput.val("");
            }
        };

        this.open = function () {
            $(settings.offcanvas).addClass("offcanvas-lg");
            $root.removeClass("d-none");
        };

        this.close = function () {
            $(settings.offcanvas).removeClass("offcanvas-lg");
            $root.addClass("d-none");
        };

        this.prepareEdit = function () {
            this.open();
            find(settings.buttons.update).removeClass("d-none");
            find(settings.buttons.refresh + "," + settings.buttons.add).addClass("d-none");
            this.loadImagesFromForm();
            this.syncToggles();
            this.syncPopularDisplay();
            this.updatePageTypeUi();
        };

        this.prepareAdd = function () {
            this.clearUploads();
            this.open();
            $form.find('[name="id"]').val(0);
            find(settings.buttons.refresh + "," + settings.buttons.add).removeClass("d-none");
            find(settings.buttons.update).addClass("d-none");
            find(settings.buttons.refresh).trigger("click");
            find(settings.popular).val(0);
            this.syncToggles();
            this.syncPopularDisplay();
            this.updatePageTypeUi();
            $root.find(".card-header>a").addClass("d-none");
        };

        this.clearUploads = function () {
            Object.keys(settings.uploads).forEach(function (slot) {
                const $upload = getUploadElement(slot);
                if ($upload.length && typeof $upload.ImageUploadModalClear === "function") {
                    $upload.ImageUploadModalClear();
                }
            });
        };

        this.loadImagesFromForm = function () {
            this.clearUploads();

            const imageBindings = [
                { slot: "icon", id: "#iconId", url: "#iconUrl", name: null },
                { slot: "image", id: "#imgId", url: "#imgUrl", name: "#imgName" },
                { slot: "overImage", id: "#overImgId", url: "#overImgUrl", name: "#overImgName" }
            ];

            imageBindings.forEach(function (binding) {
                const $upload = getUploadElement(binding.slot);
                if (!$upload.length || typeof global.ImageUploadModalDataInsert !== "function") return;

                global.ImageUploadModalDataInsert(
                    $upload,
                    find(binding.id).val(),
                    find(binding.url).val(),
                    binding.name ? find(binding.name).val() : ""
                );
            });
        };

        this.getUploadInput = function (slot) {
            return getUploadElement(slot).find(".img_input_frame > .img_input");
        };

        this.getDeleteList = function (slot) {
            return getUploadElement(slot).find(".img_input_frame").data("delectList");
        };

        this.syncToggles = function () {
            $root.find("[data-menu-toggle]").each(function () {
                syncToggle($(this));
            });
        };

        this.syncPopularDisplay = function () {
            const value = Number(find(settings.popular).val() || 0);
            find(settings.popularDisplay).text(
                Number.isFinite(value) ? value.toLocaleString("zh-TW") : "0"
            );
        };

        this.validate = async function (data) {
            const option = getPageTypeOption(data.pageType);

            if (!option) {
                co.sweet.error("資料錯誤", "無法取得頁面類型設定，請重新整理後再試。");
                return false;
            }

            const showRouterName = toBool(option.showRouterName);
            const showLinkUrl = toBool(option.showLinkUrl);

            if (!showRouterName) {
                data.routerName = option.routerName || "";
            }
            if (!showLinkUrl) {
                data.linkUrl = "";
            }
            if (!showRouterName && !showLinkUrl) {
                return true;
            }
            if (!data.linkUrl && !data.routerName) {
                co.sweet.error(
                    "資料錯誤",
                    "【路徑名稱】與【連結】<span class='text-danger font-weight-bold'>必須</span>填寫其中之一"
                );
                return false;
            }
            if (data.linkUrl && data.routerName) {
                const message = "您同時輸入【路徑名稱】與【連結】。" +
                    "<br/>儲存後此選單將無法顯示頁面內容，只會直接<span class='text-danger font-weight-bold'>跳轉</span>到指定的連結。<br/>" +
                    "是否確認要這樣設定？";
                return await co.sweet.confirmAsync(
                    "跳頁設定",
                    message,
                    "仍要儲存",
                    "取消"
                );
            }

            return true;
        };
    }

    global.CokerMenuEditorForm = CokerMenuEditorForm;
})(window, jQuery);
