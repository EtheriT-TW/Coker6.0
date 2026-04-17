(function (w, $) {
    "use strict";

    if (!$) return;

    const DirectoryService = (w.DirectoryService = w.DirectoryService || {});

    /**
     * 取得 catalog 資料
     * @param {jQuery} $item
     * @param {object} option
     */
    DirectoryService.getCatalogData = function ($item, option) {

        if (!w.co || !co.Directory || !co.Directory.getDirectoryData) {
            console.error("co.Directory.getDirectoryData not found");
            return $.Deferred().reject().promise();
        }

        // 🔹 search 特殊處理（沿用你舊邏輯）
        if (option && option.directoryType === "search") {

            const page = option.Page || "1";
            const showNum = option.ShowNum || 12;

            return co.Directory.getDirectoryData(option)
                .then(function (result) {

                    if (!result || !result.releInfos) return result;

                    // 分頁裁切（舊邏輯）
                    const start = (page - 1) * showNum;
                    const end = start + showNum;

                    result.releInfos = result.releInfos.slice(start, end);

                    return result;
                });
        }

        // 🔹 一般目錄
        return co.Directory.getDirectoryData(option);
    };

    /**
     * 取得 menu_directory 資料
     */
    DirectoryService.getMenuData = function (option) {

        if (!w.co || !co.Directory || !co.Directory.getDirectoryMenuData) {
            console.error("co.Directory.getDirectoryMenuData not found");
            return $.Deferred().reject().promise();
        }

        return co.Directory.getDirectoryMenuData(option);
    };

    /**
     * 取得 advertise_directory 資料
     */
    DirectoryService.getAdvertiseData = function (option) {

        if (!w.co || !co.Directory || !co.Directory.getDirectoryAdvertiseData) {
            console.error("co.Directory.getDirectoryAdvertiseData not found");
            return $.Deferred().reject().promise();
        }

        return co.Directory.getDirectoryAdvertiseData(option);
    };

    /**
     * （可選）統一錯誤處理
     */
    DirectoryService.handleError = function (error) {
        console.error("DirectoryService Error:", error);
    };

})(window, window.jQuery);