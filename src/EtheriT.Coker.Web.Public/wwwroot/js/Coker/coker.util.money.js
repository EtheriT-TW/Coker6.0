(function (w) {
    const Coker = (w.Coker = w.Coker || {});
    Coker.defineModule("util-money", function (C) {
        C.util = C.util || {};
        C.util.money = C.util.money || {};

        // 判斷「字串表示的金額」是否為 0（含常見格式：$0、0元、0.00、0,000、空白）
        C.util.money.isZeroPriceValue = function (input) {
            if (input === null || input === undefined) return true;
            const s = String(input).trim();
            if (s === "") return true;

            // 移除常見符號/單位/千分位/空白
            const normalized = s
                .replace(/\s+/g, "")
                .replace(/[$￥¥,]/g, "")
                .replace(/(元|NTD|TWD|NT\$|NT)/gi, "");

            // 空字串 -> 視為 0
            if (normalized === "") return true;

            const n = Number(normalized);
            if (Number.isNaN(n)) return false; // 不是數字就不當作 0
            return n === 0;
        };
    });
})(window);
