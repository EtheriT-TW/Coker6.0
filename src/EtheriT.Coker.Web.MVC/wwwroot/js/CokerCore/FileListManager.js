(function (window, $) {
    "use strict";

    // Usage:
    // new Coker.FileListManager("#Files", { type: Coker.FileListManager.Types.File });
    // new Coker.FileListManager("#Media", { type: [1, 3, 4], files: initialFiles });
    // A single type opens that uploader directly; multiple types display the selector frame.

    const FileType = Object.freeze({
        Select: 0,
        Image: 1,
        Image360: 2,
        Video: 3,
        ExternalVideo: 4,
        Youtube: 4,
        File: 5
    });

    const ExternalVideoPrefix = "external-video:v1:";
    const ExternalVideoProviders = Object.freeze({
        youtube: { label: "YouTube", icon: "fa-brands fa-youtube", color: "#ff0000", formats: ["youtube.com/watch?v=影片ID", "youtu.be/影片ID", "youtube.com/shorts/影片ID"] },
        facebook: { label: "Facebook", icon: "fa-brands fa-facebook-f", color: "#1877f2", formats: ["facebook.com/watch?v=影片ID", "facebook.com/reel/影片ID", "facebook.com/帳號/videos/影片ID", "fb.watch/短碼"] },
        instagram: { label: "Instagram", icon: "fa-brands fa-instagram", color: "#c13584", formats: ["instagram.com/reel/貼文ID", "instagram.com/reels/貼文ID", "instagram.com/p/貼文ID", "instagram.com/tv/貼文ID"] },
        threads: { label: "Threads", icon: "fa-brands fa-threads", color: "#000000", formats: ["threads.com/@帳號/post/貼文ID", "threads.com/t/貼文ID"] },
        x: { label: "X", icon: "fa-brands fa-x-twitter", color: "#000000", formats: ["x.com/帳號/status/貼文ID", "twitter.com/帳號/status/貼文ID"] }
    });

    function externalVideoDefaultThumbnail(provider) {
        const config = ExternalVideoProviders[provider] || { label: "VIDEO", color: "#5f6368" };
        const label = config.label.toUpperCase();
        const svg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 640 360"><rect width="640" height="360" fill="${config.color}"/><circle cx="320" cy="160" r="74" fill="none" stroke="#fff" stroke-width="10" opacity=".95"/><path d="M300 119l70 41-70 41z" fill="#fff"/><text x="320" y="290" text-anchor="middle" fill="#fff" font-family="Arial,sans-serif" font-size="38" font-weight="700">${label}</text></svg>`;
        return `data:image/svg+xml;charset=UTF-8,${encodeURIComponent(svg)}`;
    }

    function externalVideoFormatMessage(provider) {
        const config = ExternalVideoProviders[provider];
        if (!config) return "請選擇支援的平台";
        return `${config.label} 支援格式：${config.formats.join("、")}`;
    }

    const modes = Object.freeze({
        media: { types: [1, 2, 3, 4], defaultType: 0 },
        image: { types: [1], defaultType: 1 },
        file: { types: [5], defaultType: 5 }
    });

    let instanceSequence = 0;

    function youtubeStartSeconds(value) {
        const text = String(value || "").trim();
        if (/^\d+$/.test(text)) return Number(text);
        const match = text.match(/^(?:(\d+)h)?(?:(\d+)m)?(?:(\d+)s)?$/i);
        return match ? Number(match[1] || 0) * 3600 + Number(match[2] || 0) * 60 + Number(match[3] || 0) : 0;
    }

    function normalizeExternalVideo(provider, value) {
        provider = String(provider || "").toLowerCase();
        if (!ExternalVideoProviders[provider]) return null;

        let url;
        try { url = new URL(String(value || "").trim()); } catch (_) { return null; }
        if (url.protocol !== "https:") return null;

        let canonicalUrl = "";
        let embedUrl = "";
        let thumbnail = externalVideoDefaultThumbnail(provider);
        let externalId = "";

        if (provider === "youtube") {
            const host = url.hostname.toLowerCase().replace(/^www\./, "");
            let videoId = "";
            if (host === "youtu.be") videoId = url.pathname.split("/").filter(Boolean)[0] || "";
            else if (host === "youtube.com" || host === "m.youtube.com") {
                if (url.pathname === "/watch") videoId = url.searchParams.get("v") || "";
                else if (/^\/(shorts|live|embed)\//.test(url.pathname)) videoId = url.pathname.split("/")[2] || "";
            }
            if (!/^[\w-]{6,20}$/.test(videoId)) return null;
            const start = youtubeStartSeconds(url.searchParams.get("t") || url.searchParams.get("start"));
            canonicalUrl = `https://www.youtube.com/watch?v=${videoId}${start ? `&t=${start}s` : ""}`;
            embedUrl = `https://www.youtube-nocookie.com/embed/${videoId}${start ? `?start=${start}` : ""}`;
            thumbnail = `https://img.youtube.com/vi/${videoId}/hqdefault.jpg`;
            externalId = videoId;
        } else if (provider === "facebook") {
            const host = url.hostname.toLowerCase().replace(/^www\./, "");
            if (host !== "facebook.com" && host !== "fb.watch") return null;
            const reelMatch = url.pathname.match(/^\/reel\/(\d+)/i);
            const isWatchPath = /^\/watch\/?$/i.test(url.pathname);
            const watchId = isWatchPath ? url.searchParams.get("v") : "";
            const hasVideoPath = /\/videos\/\d+/.test(url.pathname) || (isWatchPath && /^\d+$/.test(watchId || "")) || host === "fb.watch" || reelMatch;
            if (!hasVideoPath) return null;
            url.hash = "";
            ["__cft__", "__tn__", "mibextid"].forEach(key => url.searchParams.delete(key));
            canonicalUrl = reelMatch
                ? `https://www.facebook.com/reel/${reelMatch[1]}/`
                : watchId
                    ? `https://www.facebook.com/watch?v=${watchId}`
                    : url.toString();
            embedUrl = `https://www.facebook.com/plugins/video.php?href=${encodeURIComponent(canonicalUrl)}&show_text=false&width=${reelMatch ? 420 : 960}`;
            externalId = reelMatch ? reelMatch[1] : "";
        } else if (provider === "instagram") {
            const host = url.hostname.toLowerCase().replace(/^www\./, "");
            const match = url.pathname.match(/^\/(p|reels?|tv)\/([\w-]+)/i);
            if (host !== "instagram.com" || !match) return null;
            const contentType = match[1].toLowerCase() === "reels" ? "reel" : match[1].toLowerCase();
            canonicalUrl = `https://www.instagram.com/${contentType}/${match[2]}/`;
            embedUrl = `${canonicalUrl}embed/`;
            externalId = match[2];
        } else if (provider === "threads") {
            const host = url.hostname.toLowerCase().replace(/^www\./, "");
            const postMatch = url.pathname.match(/^\/@([\w.]+)\/post\/([\w-]+)/i);
            const shortMatch = url.pathname.match(/^\/t\/([\w-]+)/i);
            if ((host !== "threads.com" && host !== "threads.net") || (!postMatch && !shortMatch)) return null;
            canonicalUrl = postMatch
                ? `https://www.threads.com/@${postMatch[1]}/post/${postMatch[2]}`
                : `https://www.threads.com/t/${shortMatch[1]}`;
            externalId = (postMatch && postMatch[2]) || shortMatch[1];
        } else if (provider === "x") {
            const host = url.hostname.toLowerCase().replace(/^www\./, "");
            const match = url.pathname.match(/^\/[^/]+\/status\/(\d+)/i);
            if ((host !== "x.com" && host !== "twitter.com") || !match) return null;
            canonicalUrl = `https://x.com/i/status/${match[1]}`;
            externalId = match[1];
        }

        const storedValue = `${ExternalVideoPrefix}${provider}:${encodeURIComponent(canonicalUrl)}`;
        if (storedValue.length > 200) return null;

        return {
            provider,
            providerLabel: ExternalVideoProviders[provider].label,
            url: canonicalUrl,
            embedUrl,
            thumbnail,
            externalId,
            isReel: (provider === "facebook" || provider === "instagram") && /^\/reel\//i.test(new URL(canonicalUrl).pathname),
            storedValue
        };
    }

    function parseStoredExternalVideo(value) {
        const text = String(value || "").trim();
        if (text.indexOf(ExternalVideoPrefix) === 0) {
            const remainder = text.substring(ExternalVideoPrefix.length);
            const separator = remainder.indexOf(":");
            if (separator <= 0) return null;
            try {
                return normalizeExternalVideo(remainder.substring(0, separator), decodeURIComponent(remainder.substring(separator + 1)));
            } catch (_) {
                return null;
            }
        }

        if (/^https:\/\//i.test(text)) return normalizeExternalVideo("youtube", text);
        const legacy = text.match(/^([\w-]{6,20})(?:&t=(\d+))?$/);
        return legacy
            ? normalizeExternalVideo("youtube", `https://www.youtube.com/watch?v=${legacy[1]}${legacy[2] ? `&t=${legacy[2]}s` : ""}`)
            : null;
    }

    function valueOf(source, camelName, pascalName) {
        if (!source) return undefined;
        return source[camelName] !== undefined ? source[camelName] : source[pascalName];
    }

    function stripGeneratedSuffix(name) {
        const separator = name.lastIndexOf(":");
        return separator > 0 ? name.substring(0, separator) : name;
    }

    function copyFile(file) {
        return new File([file], stripGeneratedSuffix(file.name), { type: file.type });
    }

    function naturalFileCompare(left, right) {
        return String(left && left.name || left || "").localeCompare(
            String(right && right.name || right || ""),
            undefined,
            { numeric: true, sensitivity: "base" }
        );
    }

    function frameName(frame, index) {
        if (frame instanceof File) return frame.name;
        const value = String(frame || "");
        const clean = value.split("?")[0].split("#")[0];
        const name = clean.substring(clean.lastIndexOf("/") + 1);
        try {
            return decodeURIComponent(name) || `影格 ${index + 1}`;
        } catch (_) {
            return name || `影格 ${index + 1}`;
        }
    }

    function frameUrl(frame) {
        if (frame instanceof File) return URL.createObjectURL(frame);
        return String(frame || "");
    }

    function dataUrlOf(file) {
        return new Promise(function (resolve, reject) {
            const reader = new FileReader();
            reader.onload = function (event) { resolve(event.target.result); };
            reader.onerror = reject;
            reader.readAsDataURL(file);
        });
    }

    function imageWidthOf(file) {
        return new Promise(function (resolve, reject) {
            const objectUrl = URL.createObjectURL(file);
            const image = new Image();
            image.onload = function () {
                URL.revokeObjectURL(objectUrl);
                resolve(image.width);
            };
            image.onerror = function (error) {
                URL.revokeObjectURL(objectUrl);
                reject(error);
            };
            image.src = objectUrl;
        });
    }

    async function compressImage(file) {
        const width = await imageWidthOf(file);
        const largeOption = width < 500
            ? { quality: 0.5, width: 500, imageType: file.type }
            : width < 1000
                ? { quality: 0.7, width: 800, imageType: file.type }
                : { quality: 0.8, width: 1000, imageType: file.type };
        const large = await new HtmlImageCompress(file, largeOption);
        const thumb = await new HtmlImageCompress(file, { quality: 0.9, width: 300, imageType: file.type });

        return {
            files: [
                file,
                new File([large.file], large.origin.name, { type: large.file.type }),
                new File([thumb.file], thumb.origin.name, { type: thumb.file.type })
            ],
            link: await dataUrlOf(file),
            name: large.origin.name
        };
    }

    function FileListManager(root, options) {
        this.$root = root && root.jquery ? root : $(root);
        if (this.$root.length !== 1) throw new Error("FileListManager requires exactly one root element.");
        const previous = this.$root.data("file-list-manager");
        if (previous && typeof previous.destroy === "function") previous.destroy();

        this.options = $.extend(true, {
            mode: "media",
            type: null,
            files: [],
            template: "#TemplateUploadList",
            renderItem: null,
            onChange: null,
            onError: null
        }, options || {});

        const mode = modes[this.options.mode];
        if (!mode) throw new Error(`Unknown FileListManager mode: ${this.options.mode}`);

        const configuredTypes = this.options.type !== null
            ? (Array.isArray(this.options.type) ? this.options.type : [this.options.type])
            : (this.options.types || mode.types);
        this.types = configuredTypes.map(Number);
        if (!this.types.length || this.types.some(type => type < FileType.Image || type > FileType.File)) {
            throw new Error("FileListManager requires one or more supported file types.");
        }
        this.defaultType = this.options.defaultType === undefined
            ? (this.options.type !== null ? (this.types.length === 1 ? this.types[0] : FileType.Select) : mode.defaultType)
            : this.options.defaultType;
        this.files = Array.isArray(this.options.files) ? this.options.files : [];
        this.activeItem = null;
        this.uploader = null;
        this.nextTempId = 0;
        this.namespace = `.fileListManager${++instanceSequence}`;

        this._ensureMarkup();
        this.$root.data("file-list-manager", this);
        this._bind();
        this.setFiles(this.files);
    }

    FileListManager.Types = FileType;
    FileListManager.Modes = modes;

    FileListManager.normalize = function (source) {
        if (!source) return null;
        if (source.Type !== undefined || source.TempId !== undefined) return source;

        const type = valueOf(source, "fileType", "FileType");
        const links = valueOf(source, "link", "Link");
        const linkList = Array.isArray(links) ? links.slice() : (links ? [links] : []);
        const frameIds = valueOf(source, "frameIds", "FrameIds");
        const link = linkList[0] || "";
        const name = valueOf(source, "name", "Name") || "";
        const externalVideo = type === FileType.ExternalVideo ? parseStoredExternalVideo(name) : null;
        return Object.assign({}, source, {
            Id: valueOf(source, "id", "Id"),
            Name: externalVideo ? externalVideo.url : name,
            File: type === FileType.ExternalVideo ? name : (type === FileType.Image360 ? linkList : link),
            Type: type,
            Link: externalVideo ? externalVideo.url : link,
            Links: linkList,
            FrameIds: Array.isArray(frameIds) ? frameIds.slice() : [],
            SerNo: valueOf(source, "serNo", "SerNo"),
            Size: valueOf(source, "size", "Size"),
            IsVisible: valueOf(source, "isVisible", "IsVisible"),
            IsEncryption: valueOf(source, "isEncryption", "IsEncryption"),
            AreaKey: valueOf(source, "areakey", "AreaKey"),
            Thumbnail: valueOf(source, "thumbnail", "Thumbnail") || linkList[1] || "",
            AspectRatio: valueOf(source, "aspectRatio", "AspectRatio") || linkList[2] || "auto",
            IsDelete: false
        });
    };

    FileListManager.prototype._bind = function () {
        const self = this;

        this.$root.off(this.namespace)
            .on(`click${this.namespace}`, ".btn_upload_add > button", function (event) {
                event.preventDefault();
                self.add();
            })
            .on(`click${this.namespace}`, ".upload_list", function (event) {
                if ($(event.target).closest("button, a, input, label, select, textarea").length) return;
                self.activate($(this));
            })
            .on(`click${this.namespace}`, ".btn_remove", function (event) {
                event.preventDefault();
                event.stopPropagation();
                self.remove($(this).closest(".upload_list"));
            })
            .on(`click${this.namespace}`, ".btn_preview", function (event) {
                event.preventDefault();
                event.stopPropagation();
                const url = $(this).data("preview-url");
                if (url) window.open(url, "_blank");
            })
            .on(`click${this.namespace}`, ".btn_lock", function (event) {
                event.preventDefault();
                event.stopPropagation();
                const $button = $(this);
                if ($button.data("status") === "locked") co.sweet.warn("操作無效", "已上鎖檔案不可解鎖。");
                else $button.toggleClass("lock");
            })
            .on(`blur${this.namespace}`, ".upload_list .ser_no", function () {
                self.move($(this).closest(".upload_list"), Number($(this).val()));
            })
            .on(`click${this.namespace}`, ".select_frame [data-uploadtype]", function (event) {
                event.preventDefault();
                if (!self.activeItem) return;
                const type = Number($(this).data("uploadtype"));
                if (self.types.indexOf(type) === -1) return;
                self.activeItem.data("uploadtype", type);
                self.activate(self.activeItem, true);
            })
            .on(`click${this.namespace}`, ".external_video_frame .btn_external_video_load", function (event) {
                event.preventDefault();
                const $frame = $(this).closest(".external_video_frame");
                self._loadExternalVideo($frame.find(".external_video_provider").val(), $frame.find(".external_video_url").val());
            })
            .on(`click${this.namespace}`, ".btn_external_video_thumbnail", function (event) {
                event.preventDefault();
                $(this).closest(".external_video_thumbnail_form").find(".external_video_thumbnail_input").trigger("click");
            })
            .on(`change${this.namespace}`, ".external_video_thumbnail_input", function () {
                const file = this.files && this.files[0];
                const item = self.activeItem && self._find(self.activeItem);
                if (!file || !item || !/^image\//i.test(file.type)) return;
                if (file.size > 10 * 1024 * 1024) {
                    if (typeof co !== "undefined" && co.sweet) co.sweet.warn("封面圖片過大", "請選擇 10 MB 以下的圖片");
                    this.value = "";
                    return;
                }
                if (item.ThumbnailObjectUrl) URL.revokeObjectURL(item.ThumbnailObjectUrl);
                item.ThumbnailObjectUrl = URL.createObjectURL(file);
                item.Thumbnail = item.ThumbnailObjectUrl;
                item.ThumbnailFile = file;
                item.RemoveThumbnail = false;
                self.$root.find(".external_video_thumbnail_preview").attr("src", item.Thumbnail);
                self.$root.find(".btn_external_video_thumbnail_reset").removeClass("d-none");
                self._renderLink(self.activeItem, item);
                self._notify("change", item);
                this.value = "";
            })
            .on(`click${this.namespace}`, ".btn_external_video_thumbnail_reset", function (event) {
                event.preventDefault();
                const item = self.activeItem && self._find(self.activeItem);
                if (!item) return;
                if (item.ThumbnailObjectUrl) URL.revokeObjectURL(item.ThumbnailObjectUrl);
                const externalVideo = parseStoredExternalVideo(item.File);
                item.ThumbnailObjectUrl = "";
                item.Thumbnail = "";
                item.ThumbnailFile = null;
                item.RemoveThumbnail = true;
                const fallback = externalVideo ? externalVideo.thumbnail : "/images/defaultImage/video.jpg";
                self.$root.find(".external_video_thumbnail_preview").attr("src", fallback);
                $(this).addClass("d-none");
                self._renderLink(self.activeItem, item);
                self._notify("change", item);
            })
            .on(`change${this.namespace}`, ".external_video_aspect_ratio", function () {
                const item = self.activeItem && self._find(self.activeItem);
                if (!item) return;
                item.AspectRatio = $(this).val() || "auto";
                const externalVideo = parseStoredExternalVideo(item.File);
                if (externalVideo) {
                    self._applyExternalVideoPreviewRatio(
                        self.$root.find(".external_video_preview"),
                        externalVideo,
                        item.AspectRatio
                    );
                }
                self._notify("change", item);
            })
            .on(`change${this.namespace}`, ".image360_replace_input", function () {
                const file = this.files && this.files[0];
                const item = self.activeItem && self._find(self.activeItem);
                if (!file || !item || !Array.isArray(item.File)) return;
                const index = Number($(this).closest(".image360_frame").data("frame-index"));
                item.File[index] = copyFile(file);
                if (Array.isArray(item.Links)) item.Links[index] = "";
                if (!Array.isArray(item.FrameIds)) item.FrameIds = [];
                item.FrameIds[index] = 0;
                item.Name = `360°（${item.File.length} 張）`;
                self._renderItem(self.activeItem, item);
                self._show360(item);
                self._notify("change", item);
            })
            .on(`change${this.namespace}`, ".image360_add_input", function () {
                const item = self.activeItem && self._find(self.activeItem);
                if (!item || !this.files || !this.files.length) return;
                const appended = Array.from(this.files).map(copyFile).sort(naturalFileCompare);
                item.File = (Array.isArray(item.File) ? item.File : []).concat(appended);
                item.FrameIds = (Array.isArray(item.FrameIds) ? item.FrameIds : [])
                    .concat(appended.map(() => 0));
                item.Name = `360°（${item.File.length} 張）`;
                self._renderItem(self.activeItem, item);
                self._show360(item);
                self._notify("change", item);
            })
            .on(`click${this.namespace}`, ".btn_360_remove_frame", function (event) {
                event.preventDefault();
                const item = self.activeItem && self._find(self.activeItem);
                if (!item || !Array.isArray(item.File) || item.File.length <= 1) return;
                const index = Number($(this).closest(".image360_frame").data("frame-index"));
                item.File.splice(index, 1);
                if (Array.isArray(item.Links)) item.Links.splice(index, 1);
                if (Array.isArray(item.FrameIds)) item.FrameIds.splice(index, 1);
                item.Name = `360°（${item.File.length} 張）`;
                self._renderItem(self.activeItem, item);
                self._show360(item);
                self._notify("change", item);
            });

        const uploadId = this._uploadId();
        this._onImagesAdded = function (event) {
            if (!event.detail || event.detail.uploadId !== uploadId || !self.activeItem) return;
            self._acceptFiles(event.detail.cachedFileArray || []);
        };
        this._onImageDeleted = function (event) {
            if (!event.detail || event.detail.uploadId !== uploadId || !self.activeItem) return;
            if (Number(self.activeItem.data("uploadtype")) === FileType.Image360) {
                self.activeItem.data("pending-files", (event.detail.cachedFileArray || []).map(copyFile));
            }
        };
        this._onClear = function (event) {
            if (!event.detail || event.detail.uploadId !== uploadId || !self.activeItem) return;
            self.remove(self.activeItem);
        };

        window.addEventListener("fileUploadWithPreview:imagesAdded", this._onImagesAdded);
        window.addEventListener("fileUploadWithPreview:imageDeleted", this._onImageDeleted);
        window.addEventListener("fileUploadWithPreview:clearButtonClicked", this._onClear);

        const $list = this.$root.children("ul");
        if (typeof $list.sortable === "function") {
            $list.sortable({
                items: "> .upload_list",
                handle: ".serNoTool",
                cursor: "move",
                dropOnEmpty: false,
                placeholder: "sortable-placeholder",
                tolerance: "pointer",
                stop: function () { self._syncOrder(); }
            });
        }
    };

    FileListManager.prototype._uploadId = function () {
        return this.$root.find(".upload_frame").data("upload-id") || `FileUpload${instanceSequence}`;
    };

    FileListManager.prototype._ensureMarkup = function () {
        this.$root.addClass("data_upload");
        if (!this.$root.children("ul").length) {
            this.$root.append('<ul><li class="btn_upload_add"><button type="button" title="新增檔案"><span class="material-symbols-outlined">add</span></button></li></ul>');
        } else if (!this.$root.children("ul").children(".btn_upload_add").length) {
            this.$root.children("ul").append('<li class="btn_upload_add"><button type="button" title="新增檔案"><span class="material-symbols-outlined">add</span></button></li>');
        }

        if (this.$root.children(".preview_frame").length) return;

        const uploadId = `${this.$root.attr("id") || "FileUpload"}${instanceSequence}`;
        const labels = { 1: "圖片", 2: "360", 3: "影片", 4: "外嵌影片", 5: "檔案" };
        const selector = this.types.length > 1
            ? `<div class="select_frame flex-column d-none">${this.types.map(type => `<button type="button" data-uploadtype="${type}">${labels[type]}</button>`).join("")}</div>`
            : "";
        const externalVideo = this.types.indexOf(FileType.ExternalVideo) >= 0
            ? `<div class="external_video_frame d-none flex-column h-100">
                <div class="mb-2">外嵌影片設定</div>
                <div class="external_video_form mb-3">
                    <select class="form-select external_video_provider" aria-label="外嵌影片平台">
                        <option value="youtube">YouTube</option>
                        <option value="facebook">Facebook</option>
                        <option value="instagram">Instagram</option>
                        <option value="threads">Threads</option>
                        <option value="x">X</option>
                    </select>
                    <input class="form-control external_video_url" type="url" placeholder="貼上公開影片或貼文網址">
                    <button class="btn btn-outline-secondary btn_external_video_load" type="button">驗證並載入</button>
                </div>
                <div class="external_video_note text-muted mb-2">僅支援平台官方允許嵌入的公開內容；私人、限制嵌入或已刪除內容無法顯示。</div>
                <div class="external_video_validation text-danger small mb-2 d-none"></div>
                <div class="external_video_settings d-none mb-2">
                    <label class="external_video_aspect_label">前台放大比例
                        <select class="form-select form-select-sm external_video_aspect_ratio">
                            <option value="auto">自動判斷</option><option value="16:9">橫式 16:9</option><option value="9:16">直式 9:16</option><option value="1:1">方形 1:1</option><option value="4:3">傳統 4:3</option>
                        </select>
                    </label>
                </div>
                <div class="external_video_thumbnail_form d-none mb-2">
                    <div class="external_video_thumbnail_heading"><strong>前台輪播封面</strong><span>顯示於商品主圖與下方縮圖，不影響影片內容</span></div>
                    <div class="external_video_thumbnail_body">
                        <img class="external_video_thumbnail_preview" alt="前台輪播封面預覽">
                        <input class="external_video_thumbnail_input d-none" type="file" accept="image/*">
                        <div class="external_video_thumbnail_actions">
                            <button class="btn btn-sm btn-outline-primary btn_external_video_thumbnail" type="button">上傳自訂封面</button>
                            <button class="btn btn-sm btn-outline-secondary btn_external_video_thumbnail_reset d-none" type="button">改用平台預設圖</button>
                            <span class="text-muted small">建議 16:9，最大 10 MB</span>
                        </div>
                    </div>
                </div>
                <div class="external_video_preview_heading">嵌入內容預覽</div>
                <div class="external_video_preview flex-grow-1"></div>
            </div>`
            : "";
        const media = this.types.some(type => type === FileType.Image || type === FileType.Image360 || type === FileType.Video)
            ? '<div class="media_frame d-none flex-column h-100"><div class="mb-2">檔案預覽</div><input class="form-control d-none" type="text" readonly><div class="media_preview flex-grow-1"><div></div></div></div>'
            : "";

        this.$root.append(`<div class="preview_frame"><div class="default_frame h-100 d-flex">點選 + 號新增檔案</div>${selector}<div class="upload_frame custom-file-container d-none" data-upload-id="${uploadId}"></div>${media}${externalVideo}</div>`);
    };

    FileListManager.prototype._notify = function (name, payload) {
        if (typeof this.options.onChange === "function") {
            this.options.onChange.call(this, { type: name, item: payload || null, files: this.files });
        }
    };

    FileListManager.prototype._error = function (error) {
        console.error(error);
        this.clearPreview();
        if (typeof this.options.onError === "function") this.options.onError.call(this, error);
        else if (typeof co !== "undefined" && co.sweet) co.sweet.error("資料上傳失敗", "請重新上傳", null, null);
    };

    FileListManager.prototype._newTempId = function () {
        while (this.files.some(file => file.TempId === this.nextTempId)) this.nextTempId += 1;
        return this.nextTempId++;
    };

    FileListManager.prototype._template = function () {
        const $template = $(this.options.template).first();
        if ($template.length) return $($template.html()).first();
        return $('<li class="upload_list d-flex align-items-center"><div class="serNoTool"><span class="material-symbols-outlined">open_with</span><input class="ser_no" type="number" min="1" step="1"></div><span class="title flex-grow-1"></span><img class="thumb_img" src="/images/noImg.jpg"><a class="btn_link d-none" target="_blank"><span class="material-symbols-outlined">search</span></a><button type="button" class="btn_remove"><span class="material-symbols-outlined">delete</span></button></li>');
    };

    FileListManager.prototype._find = function ($item) {
        const id = $item.data("id");
        const tempId = $item.data("tempid");
        return this.files.find(file => id !== undefined ? file.Id == id : file.TempId == tempId);
    };

    FileListManager.prototype.getItem = function (row) {
        const $row = row && row.jquery ? row : $(row);
        return this._find($row);
    };

    FileListManager.prototype._renderItem = function ($row, item) {
        $row.find(".title").text(item.Name || "");
        const $name = $row.find('input[name="name"]');
        if ($name.length) $name.val(item.Name || "").attr("placeholder", item.Name || "");

        const size = item.File instanceof File ? item.File.size : null;
        const sizeText = item.Size || (size === null ? "" : size < 1024
            ? `${size} B`
            : size < 1048576
                ? `${(size / 1024).toFixed(1)} KB`
                : size < 1073741824
                    ? `${(size / 1048576).toFixed(1)} MB`
                    : `${(size / 1073741824).toFixed(1)} GB`);
        $row.find(".size").text(sizeText);

        const previewUrl = item.Link || (item.File instanceof File ? URL.createObjectURL(item.File) : item.File);
        $row.find(".btn_preview").data("preview-url", previewUrl || "");
        $row.find("label.visible input").prop("checked", item.IsVisible !== false);
        if (item.IsEncryption) {
            $row.find(".btn_lock").addClass("lock").data("status", "locked").attr("title", "已上鎖檔案不可解鎖");
        }
        if (typeof this.options.renderItem === "function") this.options.renderItem.call(this, $row, item);
    };

    FileListManager.prototype._render = function (source) {
        const item = FileListManager.normalize(source);
        const $row = this._template();
        const order = this.$root.find(".upload_list").length + 1;

        $row.data("uploadtype", item.Type);
        $row.data("serno", order);
        $row.find(".ser_no").val(order);
        if (item.Id !== undefined && item.Id !== null) $row.data("id", item.Id);
        else $row.data("tempid", item.TempId);
        $row.data("file-list-placeholder", false);
        this._renderItem($row, item);
        this._renderLink($row, item);
        this.$root.children("ul").children(".btn_upload_add").before($row);
        this.$root.data("file_num", order);
        return $row;
    };

    FileListManager.prototype._renderLink = function ($row, item) {
        const file = item.File;
        const $link = $row.find(".btn_link");
        let thumbnail = "/images/noImg.jpg";
        let href = item.Link || file;

        if (file) {
            if (item.Type === FileType.Image360) thumbnail = "/images/defaultImage/360.jpg";
            else if (item.Type === FileType.Video) thumbnail = "/images/defaultImage/video.jpg";
            else if (item.Type === FileType.ExternalVideo) {
                const externalVideo = parseStoredExternalVideo(file);
                if (externalVideo) {
                    thumbnail = item.Thumbnail || externalVideo.thumbnail;
                    href = externalVideo.url;
                    let $wrap = $row.find(".external_video_thumbnail_wrap");
                    const $image = $row.find(".thumb_img");
                    if (!$wrap.length) {
                        $image.wrap('<span class="external_video_thumbnail_wrap"></span>');
                        $wrap = $image.parent();
                    }
                    let $badge = $wrap.find(".external_video_provider_badge");
                    if (!$badge.length) $badge = $('<span class="external_video_provider_badge"></span>').appendTo($wrap);
                    const provider = ExternalVideoProviders[externalVideo.provider];
                    $badge.css("background-color", provider.color)
                        .attr("title", provider.label)
                        .empty()
                        .append($('<i></i>').attr("class", provider.icon));
                }
            } else if (item.Type === FileType.Image) thumbnail = item.Link || (typeof file === "string" ? file : thumbnail);
        }

        $row.find(".thumb_img").attr("src", thumbnail);
        if (href && typeof href === "string") $link.removeClass("d-none").attr("href", href);
        else $link.addClass("d-none").attr("href", "");
    };

    FileListManager.prototype.add = function (source, activate) {
        if (source) {
            const item = FileListManager.normalize(source);
            if (item.TempId === undefined && (item.Id === undefined || item.Id === null)) item.TempId = this._newTempId();
            if (this.files.indexOf(item) === -1) this.files.push(item);
            const $row = this._render(item);
            if (activate === true) this.activate($row);
            this._notify("add", item);
            return item;
        }

        this.$root.find('.upload_list[data-file-list-placeholder="true"]').remove();

        const placeholder = {
            TempId: this._newTempId(),
            Type: this.defaultType,
            Name: "",
            IsDelete: false
        };
        const $row = this._render(placeholder);
        $row.attr("data-file-list-placeholder", "true").data("file-list-placeholder", true);
        this.activate($row);
        return placeholder;
    };

    FileListManager.prototype.setFiles = function (files) {
        this.files = Array.isArray(files) ? files : [];
        this.$root.data("files", this.files);
        this.activeItem = null;
        this.uploader = null;
        this.nextTempId = 0;
        this.$root.find(".upload_list").remove();
        this.files.filter(file => !file.IsDelete).forEach(file => {
            const normalized = FileListManager.normalize(file);
            if (normalized !== file) Object.assign(file, normalized);
            if (file.TempId === undefined && (file.Id === undefined || file.Id === null)) file.TempId = this._newTempId();
            this._render(file);
        });
        this.clearPreview();
        this._syncOrder(false);
        return this;
    };

    FileListManager.prototype.getFiles = function (includeDeleted) {
        return includeDeleted === false ? this.files.filter(file => !file.IsDelete) : this.files;
    };

    FileListManager.prototype.reset = function () {
        return this.setFiles([]);
    };

    FileListManager.prototype.activate = function ($row, force) {
        if (!$row || !$row.length) return;
        if (!force && this.activeItem && this.activeItem[0] === $row[0]) return;

        const oldItem = this.activeItem;
        if (oldItem && oldItem.length && oldItem.data("file-list-placeholder") === true && oldItem[0] !== $row[0]) oldItem.remove();
        this.activeItem = $row;
        this.clearPreview();
        this.$root.find(".default_frame").removeClass("d-flex").addClass("d-none");

        const type = Number($row.data("uploadtype"));
        const item = this._find($row);
        if (type === FileType.Select) {
            this.$root.find(".select_frame").removeClass("d-none").addClass("d-flex");
            return;
        }

        if (item && item.Name) this._showExisting(item);
        else this._showUploader(type);
    };

    FileListManager.prototype._showUploader = function (type) {
        const $frame = this.$root.find(".upload_frame");
        const uploadId = this._uploadId();
        $frame.empty().removeClass("d-none");

        if (type === FileType.Image) this.uploader = co.File.UploadImageInit(uploadId);
        else if (type === FileType.Image360) this.uploader = co.File.Upload360Init(uploadId);
        else if (type === FileType.Video) this.uploader = co.File.UploadVideoInit(uploadId);
        else if (type === FileType.File) this.uploader = co.File.UploadFileInit(uploadId);
        else if (type === FileType.ExternalVideo) {
            this.$root.find(".external_video_frame").removeClass("d-none").addClass("d-flex");
            this.$root.find(".external_video_provider").val("youtube");
            this.$root.find(".external_video_url").val("");
            this.$root.find(".external_video_validation").addClass("d-none").empty();
            this.$root.find(".external_video_thumbnail_form").removeClass("d-flex").addClass("d-none");
            this.$root.find(".external_video_settings").addClass("d-none");
            return;
        }

        if (type === FileType.File) {
            const input = $frame.find('input[type="file"]')[0];
            if (input) input.click();
        }
    };

    FileListManager.prototype._showExisting = function (item) {
        if (item.Type === FileType.ExternalVideo) {
            const externalVideo = parseStoredExternalVideo(item.File);
            const $frame = this.$root.find(".external_video_frame").removeClass("d-none").addClass("d-flex");
            if (externalVideo) {
                $frame.find(".external_video_provider").val(externalVideo.provider);
                $frame.find(".external_video_url").val(externalVideo.url);
                this._loadExternalVideo(externalVideo.provider, externalVideo.url, false);
            }
            return;
        }

        if (item.Type === FileType.File) return;
        if (item.Type === FileType.Image360) {
            this._show360(item);
            return;
        }
        const $media = this.$root.find(".media_frame").removeClass("d-none").addClass("d-flex");
        $media.children(".mb-2").text("檔案預覽");
        $media.find("input").val(item.Name || "");
        const $preview = $media.find(".media_preview > div")
            .empty()
            .removeClass("image360_editor");
        if (item.Type === FileType.Video) {
            const src = typeof item.File === "string" ? item.File : URL.createObjectURL(item.File);
            $preview.append($("<video>", { class: "h-100 w-100", controls: true, preload: "metadata", src: src }));
        } else {
            $preview.append($("<img>", { src: item.Link || item.File }));
        }
    };

    FileListManager.prototype._show360 = function (item) {
        const self = this;
        const frames = Array.isArray(item.File) ? item.File : [];
        const $media = this.$root.find(".media_frame").removeClass("d-none").addClass("d-flex");
        $media.children(".mb-2").text(`360 影格（共 ${frames.length} 張，拖曳可調整順序）`);
        const $preview = $media.find(".media_preview > div").empty()
            .addClass("image360_editor")
            .append('<div class="image360_toolbar"><label class="btn btn-sm btn-outline-primary mb-0">新增影格<input class="image360_add_input d-none" type="file" accept="image/*" multiple></label></div>');
        const $list = $('<div class="image360_frames"></div>').appendTo($preview);

        frames.forEach(function (frame, index) {
            const url = frameUrl(frame);
            const $frame = $(`<div class="image360_frame" data-frame-index="${index}">
                <div class="image360_frame_header"><span class="image360_drag material-symbols-outlined" title="拖曳調整順序">drag_indicator</span><strong>${index + 1}</strong></div>
                <img alt="360 影格 ${index + 1}">
                <div class="image360_frame_name"></div>
                <div class="image360_frame_actions">
                    <label class="btn btn-sm btn-outline-secondary mb-0" title="替換此影格">替換<input class="image360_replace_input d-none" type="file" accept="image/*"></label>
                    <button type="button" class="btn btn-sm btn-outline-danger btn_360_remove_frame" title="移除此影格"${frames.length <= 1 ? " disabled" : ""}>移除</button>
                </div>
            </div>`);
            $frame.find("img").attr("src", url);
            $frame.find(".image360_frame_name").text(frameName(frame, index)).attr("title", frameName(frame, index));
            $list.append($frame);
        });

        if (typeof $list.sortable === "function") {
            $list.sortable({
                items: "> .image360_frame",
                handle: ".image360_drag",
                cursor: "move",
                tolerance: "pointer",
                stop: function () {
                    const order = $list.children().map(function () { return Number($(this).data("frame-index")); }).get();
                    item.File = order.map(index => frames[index]);
                    if (Array.isArray(item.Links)) item.Links = order.map(index => item.Links[index]);
                    if (Array.isArray(item.FrameIds)) item.FrameIds = order.map(index => item.FrameIds[index] || 0);
                    self._show360(item);
                    self._notify("sort", item);
                }
            });
        }
    };

    FileListManager.prototype._acceptFiles = async function (cachedFiles) {
        const type = Number(this.activeItem.data("uploadtype"));
        const files = cachedFiles.map(copyFile);
        if (!files.length) return;

        try {
            if (type === FileType.Image) {
                const results = await Promise.all(files.map(compressImage));
                results.forEach((result, index) => this._commit(this.activeItem && index === 0 ? this.activeItem : null, {
                    TempId: index === 0 && this.activeItem ? this.activeItem.data("tempid") : this._newTempId(),
                    Type: type,
                    File: result.files,
                    Link: result.link,
                    Name: result.name,
                    IsDelete: false
                }));
            } else if (type === FileType.Image360) {
                files.sort(naturalFileCompare);
                this._commit(this.activeItem, {
                    TempId: this.activeItem.data("tempid"), Type: type, File: files,
                    Links: [], Link: "", FrameIds: files.map(() => 0), Name: `360°（${files.length} 張）`, IsDelete: false
                });
            } else {
                files.forEach((file, index) => this._commit(this.activeItem && index === 0 ? this.activeItem : null, {
                    TempId: index === 0 && this.activeItem ? this.activeItem.data("tempid") : this._newTempId(),
                    Type: type, File: file, Name: file.name, IsDelete: false
                }));
            }
        } catch (error) {
            this._error(error);
        }
    };

    FileListManager.prototype._commit = function ($row, item) {
        this.files.push(item);
        if ($row && $row.length) {
            $row.removeAttr("data-file-list-placeholder")
                .data("file-list-placeholder", false)
                .data("tempid", item.TempId)
                .data("uploadtype", item.Type);
            this._renderItem($row, item);
            this._renderLink($row, item);
        } else this._render(item);
        this.activeItem = null;
        this.clearPreview();
        this._syncOrder();
        this._notify("add", item);
    };

    FileListManager.prototype._renderExternalVideoPreview = function ($preview, externalVideo) {
        $preview.empty();

        if (externalVideo.provider === "facebook") {
            const $embed = $('<div class="fb-video"></div>').attr({
                "data-href": externalVideo.url,
                "data-width": externalVideo.isReel ? "360" : "560",
                "data-show-text": "false",
                "data-allowfullscreen": "true"
            }).append($('<blockquote class="fb-xfbml-parse-ignore"></blockquote>').attr("cite", externalVideo.url)
                .append($('<a target="_blank" rel="noopener noreferrer">在 Facebook 查看影片</a>').attr("href", externalVideo.url)));
            const $facebookPreview = $('<div class="external_video_facebook_preview is-loading d-flex justify-content-center"></div>')
                .append('<div class="external_video_facebook_loading"><i class="fa-brands fa-facebook"></i><span>正在載入 Facebook 影片…</span></div>')
                .append($embed);
            $preview.append($facebookPreview);

            let revealTimer = window.setTimeout(function () {
                $facebookPreview.addClass("is-ready").removeClass("is-loading");
            }, 5000);
            const reveal = function () {
                window.clearTimeout(revealTimer);
                $facebookPreview.addClass("is-ready").removeClass("is-loading");
                observer.disconnect();
            };
            const observer = new MutationObserver(function () {
                const iframe = $facebookPreview.find("iframe").get(0);
                if (iframe && !iframe.dataset.fileListLoadBound) {
                    iframe.dataset.fileListLoadBound = "true";
                    iframe.addEventListener("load", reveal, { once: true });
                }
            });
            observer.observe($facebookPreview.get(0), { childList: true, subtree: true });

            const render = function () {
                if (window.FB && window.FB.XFBML) window.FB.XFBML.parse($preview.get(0));
            };
            if (!document.getElementById("fb-root")) document.body.insertAdjacentHTML("afterbegin", '<div id="fb-root"></div>');
            let script = document.getElementById("facebook-jssdk");
            if (window.FB && window.FB.XFBML) render();
            else if (!script) {
                script = document.createElement("script");
                script.id = "facebook-jssdk";
                script.src = "https://connect.facebook.net/zh_TW/sdk.js#xfbml=1&version=v25.0";
                script.async = true;
                script.defer = true;
                script.crossOrigin = "anonymous";
                document.head.appendChild(script);
                script.addEventListener("load", render, { once: true });
            } else script.addEventListener("load", render, { once: true });
            return;
        }

        if (externalVideo.provider === "threads") {
            const safeUrl = externalVideo.url.replace(/&/g, "&amp;").replace(/"/g, "&quot;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
            const source = `<!doctype html><html><head><meta name="viewport" content="width=device-width,initial-scale=1"></head><body style="margin:0;display:flex;justify-content:center"><blockquote class="text-post-media" data-text-post-permalink="${safeUrl}" data-text-post-version="0"><a href="${safeUrl}" target="_blank" rel="noopener noreferrer">在 Threads 查看貼文</a></blockquote><script async src="https://www.threads.com/embed.js"></script></body></html>`;
            $preview.append($('<iframe class="external_video_iframe w-100 h-100" title="Threads 貼文預覽" frameborder="0"></iframe>')
                .attr("srcdoc", source));
            return;
        }

        if (externalVideo.provider === "x") {
            const $target = $('<div class="external_video_x_preview d-flex justify-content-center"></div>');
            $preview.append($target);
            const render = function () {
                if (window.twttr && window.twttr.widgets && window.twttr.widgets.createTweet) {
                    window.twttr.widgets.createTweet(externalVideo.externalId, $target.get(0), { align: "center", conversation: "none" });
                } else {
                    $target.append($('<a target="_blank" rel="noopener noreferrer">在 X 查看貼文</a>').attr("href", externalVideo.url));
                }
            };

            if (window.twttr && window.twttr.widgets) render();
            else {
                let script = document.querySelector('script[src="https://platform.twitter.com/widgets.js"]');
                if (!script) {
                    script = document.createElement("script");
                    script.src = "https://platform.twitter.com/widgets.js";
                    script.async = true;
                    document.head.appendChild(script);
                }
                script.addEventListener("load", render, { once: true });
            }
            return;
        }

        $preview.append($('<iframe class="external_video_iframe w-100 h-100" title="外嵌影片預覽" frameborder="0" allowfullscreen></iframe>')
            .attr("src", externalVideo.embedUrl));
    };

    FileListManager.prototype._applyExternalVideoPreviewRatio = function ($preview, externalVideo, value) {
        const ratioKey = value && value !== "auto"
            ? value
            : (externalVideo && externalVideo.isReel ? "9:16" : "16:9");
        const ratios = {
            "16:9": [16, 9],
            "9:16": [9, 16],
            "1:1": [1, 1],
            "4:3": [4, 3]
        };
        const ratio = ratios[ratioKey] || ratios["16:9"];
        const parentWidth = Math.max(220, $preview.parent().innerWidth() || 0);
        const maxHeight = 360;
        const width = Math.min(parentWidth, maxHeight * ratio[0] / ratio[1]);
        const height = width * ratio[1] / ratio[0];

        $preview
            .attr("data-aspect-ratio", ratioKey)
            .css({
                width: `${Math.round(width)}px`,
                height: `${Math.round(height)}px`,
                minHeight: `${Math.round(height)}px`,
                maxWidth: "100%"
            });
    };

    FileListManager.prototype._loadExternalVideo = function (provider, value, notify) {
        if (!this.activeItem) return;
        const externalVideo = normalizeExternalVideo(provider, value);
        const $preview = this.$root.find(".external_video_preview").empty();
        const $validation = this.$root.find(".external_video_validation");
        if (!externalVideo) {
            const message = externalVideoFormatMessage(provider);
            $validation.removeClass("d-none").text(message);
            this.$root.find(".external_video_thumbnail_form").removeClass("d-flex").addClass("d-none");
            this.$root.find(".external_video_settings").addClass("d-none");
            $preview.append($('<div class="w-100 h-100 d-flex flex-column gap-2 justify-content-center align-items-center bg-black bg-opacity-10 fw-bold text-center p-3"></div>')
                .append("網址格式不正確，或不是所選平台支援的公開內容網址"));
            return;
        }

        $validation.addClass("d-none").empty();
        this._renderExternalVideoPreview($preview, externalVideo);
        this.$root.find(".external_video_url").val(externalVideo.url);
        this.activeItem.find(".title").text(externalVideo.url);

        let item = this._find(this.activeItem);
        if (!item) {
            item = { TempId: this.activeItem.data("tempid"), Type: FileType.ExternalVideo, IsDelete: false };
            this.files.push(item);
        }
        item.File = externalVideo.storedValue;
        item.Name = externalVideo.url;
        item.Link = externalVideo.url;
        item.Provider = externalVideo.provider;
        const thumbnail = item.Thumbnail || externalVideo.thumbnail;
        this.$root.find(".external_video_settings").removeClass("d-none");
        this.$root.find(".external_video_aspect_ratio").val(item.AspectRatio || "auto");
        this.$root.find(".external_video_thumbnail_form").removeClass("d-none").addClass("d-flex");
        this.$root.find(".external_video_thumbnail_preview").attr("src", thumbnail);
        this.$root.find(".btn_external_video_thumbnail_reset").toggleClass("d-none", !item.Thumbnail);
        this._applyExternalVideoPreviewRatio($preview, externalVideo, item.AspectRatio || "auto");
        this.activeItem.removeAttr("data-file-list-placeholder").data("file-list-placeholder", false);
        this._renderLink(this.activeItem, item);
        if (notify !== false) this._notify("change", item);
    };

    FileListManager.prototype._loadYoutube = function (value, notify) {
        return this._loadExternalVideo("youtube", value, notify);
    };

    FileListManager.prototype.remove = function ($row) {
        if (!$row || !$row.length) return;
        const item = this._find($row);
        if (item) {
            if (item.Id !== undefined && item.Id !== null) item.IsDelete = true;
            else {
                const index = this.files.indexOf(item);
                if (index >= 0) this.files.splice(index, 1);
            }
        }
        if (this.activeItem && this.activeItem[0] === $row[0]) this.activeItem = null;
        $row.remove();
        this.clearPreview();
        this._syncOrder();
        this._notify("remove", item);
    };

    FileListManager.prototype.transferTo = function (row, targetManager) {
        const $row = row && row.jquery ? row : $(row);
        const item = this._find($row);
        if (!item || !(targetManager instanceof FileListManager)) return null;
        const index = this.files.indexOf(item);
        if (index >= 0) this.files.splice(index, 1);
        if (this.activeItem && this.activeItem[0] === $row[0]) this.activeItem = null;
        $row.remove();
        this._syncOrder();
        targetManager.add(item);
        this._notify("transfer", item);
        return item;
    };

    FileListManager.prototype.move = function ($row, targetIndex) {
        const count = this.$root.find(".upload_list").length;
        if (!count) return;
        const next = Math.max(1, Math.min(count, Number(targetIndex) || 1));
        $row.detach();
        const $rows = this.$root.find(".upload_list");
        if (next > $rows.length) this.$root.children("ul").children(".btn_upload_add").before($row);
        else $rows.eq(next - 1).before($row);
        this._syncOrder();
    };

    FileListManager.prototype._syncOrder = function (notify) {
        const ordered = [];
        this.$root.find(".upload_list").each((index, element) => {
            const $row = $(element);
            const item = this._find($row);
            const order = index + 1;
            $row.data("serno", order).find(".ser_no").val(order);
            if (item && ordered.indexOf(item) === -1) {
                item.SerNo = order;
                ordered.push(item);
            }
        });
        this.files.forEach(file => { if (ordered.indexOf(file) === -1) ordered.push(file); });
        this.files.length = 0;
        Array.prototype.push.apply(this.files, ordered);
        this.$root.data("file_num", this.$root.find(".upload_list").length);
        if (notify !== false) this._notify("sort");
    };

    FileListManager.prototype.clearPreview = function () {
        const $preview = this.$root.find(".preview_frame");
        $preview.find(".default_frame").removeClass("d-none").addClass("d-flex");
        $preview.find(".upload_frame").empty().addClass("d-none");
        $preview.find(".media_frame, .external_video_frame, .select_frame").removeClass("d-flex").addClass("d-none");
        $preview.find(".media_frame > .mb-2").text("檔案預覽");
        $preview.find(".external_video_preview").empty();
        $preview.find(".media_preview > div").empty().removeClass("image360_editor");
        this.uploader = null;
    };

    FileListManager.prototype.destroy = function () {
        this.$root.off(this.namespace).removeData("file-list-manager");
        window.removeEventListener("fileUploadWithPreview:imagesAdded", this._onImagesAdded);
        window.removeEventListener("fileUploadWithPreview:imageDeleted", this._onImageDeleted);
        window.removeEventListener("fileUploadWithPreview:clearButtonClicked", this._onClear);
        const $list = this.$root.children("ul");
        if (typeof $list.sortable === "function" && $list.hasClass("ui-sortable")) $list.sortable("destroy");
    };

    const cokerRoot = typeof Coker !== "undefined" ? Coker : (window.Coker || {});
    cokerRoot.FileListManager = FileListManager;
    window.Coker = cokerRoot;
})(window, window.jQuery);
