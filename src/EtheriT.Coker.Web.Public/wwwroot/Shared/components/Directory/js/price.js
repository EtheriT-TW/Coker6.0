(function (w, $) {
    "use strict";

    if (!$) return;

    const DirectoryPrice = (w.DirectoryPrice = w.DirectoryPrice || {});

    function isNullOrEmpty(value) {
        return value === null || value === undefined || value === "";
    }

    function hasMoneyUtil() {
        return !!(w.co && co.util && co.util.money && typeof co.util.money.isZeroPriceValue === "function");
    }

    function normalizeText(value) {
        if (isNullOrEmpty(value)) return "";
        return String(value).trim();
    }

    function normalizeNumber(value) {
        if (isNullOrEmpty(value)) return 0;

        const text = String(value).replace(/,/g, "").trim();
        const num = Number(text);

        return isNaN(num) ? 0 : num;
    }

    function isZeroValue(value) {
        if (isNullOrEmpty(value)) return true;

        if (hasMoneyUtil()) {
            return co.util.money.isZeroPriceValue(value);
        }

        return normalizeNumber(value) === 0;
    }

    function hasDisplayValue(value) {
        return !isNullOrEmpty(value) && !isZeroValue(value);
    }

    function isSameMoney(a, b) {
        return normalizeNumber(a) === normalizeNumber(b);
    }

    function formatMoney(value) {
        const text = normalizeText(value);
        if (!text) return "";

        if (w.CokerCurrency && typeof w.CokerCurrency.format === "function") {
            return w.CokerCurrency.format(normalizeNumber(value));
        }

        return `NT$${text}`;
    }

    function formatBonus(value) {
        const text = normalizeText(value);
        if (!text) return "";
        return `${text}紅利點`;
    }

    function buildOriginPriceLine(label, value, extraClass) {
        if (!hasDisplayValue(value)) return "";
        const safeClass = extraClass ? ` ${extraClass}` : "";
        return `<div class="origin-price text-decoration-line-through${safeClass}">${formatMoney(value)}</div>`;
    }

    function buildLabeledPriceLine(label, value, className) {
        if (!hasDisplayValue(value)) return "";
        const safeClass = className ? ` ${className}` : "";
        return `<div class="sale-price${safeClass}">${label}：${formatMoney(value)}</div>`;
    }

    function buildSalePriceLine(value) {
        if (!hasDisplayValue(value)) return "";
        return `<div class="sale-price">${formatMoney(value)}</div>`;
    }

    function buildBonusOnlyLine(value) {
        if (!hasDisplayValue(value)) return "";
        return `<div class="sale-price">${formatBonus(value)}</div>`;
    }

    function buildComboPriceLine(price, bonus) {
        if (!hasDisplayValue(price) || !hasDisplayValue(bonus)) return "";

        return `
            <div class="sale-price combo-price">
                <span class="cash-price">${formatMoney(price)}</span>
                <span class="plus-sign">+</span>
                <span class="bonus-price">${formatBonus(bonus)}</span>
            </div>
        `;
    }

    function setPriceHtml(content, html) {
        const $content = $(content);

        $content.find(".price-grid")
            .removeClass("price notshow")
            .empty()
            .html(html);

        $content.find(".normal-price")
            .removeClass("price notshow")
            .empty()
            .html(html);

        $content.find(".price").not(".price-grid, .normal-price")
            .removeClass("notshow")
            .empty()
            .html(html);
    }

    function hidePrice(content) {
        const $content = $(content);

        $content.find(".price-grid")
            .removeClass("price notshow")
            .empty();

        $content.find(".normal-price")
            .removeClass("price")
            .addClass("notshow")
            .empty();

        $content.find(".price").not(".price-grid, .normal-price")
            .addClass("notshow")
            .empty();
    }

    /**
     * 規則：
     * 1. priceDisplayText 有值時（時價），優先直接顯示
     * 2. 金額為 0 或空值視為未設定，不顯示
     * 3. 登入前、或登入後沒有會員價（currentRoleName = 非會員）：
     *    price 即非會員價；建議售價 > 非會員價時顯示「劃線建議售價 + 非會員價」
     * 4. 登入後有會員價（currentRoleName 為會員角色）：
     *    顯示「劃線建議售價 + 會員價」，非會員價不顯示；
     *    沒設建議售價時，退用非會員價當劃線價
     * 5. 有紅利時，售價行改為組合：NT$700 + 50紅利點
     * 6. 只有 bonus 時，只顯示 bonus
     */
    function buildHtml(data) {
        if (!data) return "";

        const priceDisplayText = normalizeText(data.priceDisplayText);
        const price = data.price;
        const oriPrice = data.oriPrice;
        const suggestPrice = data.suggestPrice;
        const bonus = data.bonus;
        const currentRoleName = normalizeText(data.currentRoleName);

        if (priceDisplayText) {
            return `<div class="sale-price">${priceDisplayText}</div>`;
        }

        const hasCashPrice = hasDisplayValue(price);
        const hasBonus = hasDisplayValue(bonus);

        if (!hasCashPrice && !hasBonus) {
            return "";
        }

        if (!hasCashPrice && hasBonus) {
            return buildBonusOnlyLine(bonus);
        }

        // 會員價模式：後端選到的是會員層級的價格
        const isMemberPrice =
            !!currentRoleName &&
            currentRoleName !== "非會員";

        // 劃線參考價：建議售價 > 實際售價才顯示；
        // 會員模式下沒有建議售價時，退用非會員價
        let referencePrice = "";
        if (hasDisplayValue(suggestPrice) && normalizeNumber(suggestPrice) > normalizeNumber(price)) {
            referencePrice = suggestPrice;
        } else if (isMemberPrice && hasDisplayValue(oriPrice) && normalizeNumber(oriPrice) > normalizeNumber(price)) {
            referencePrice = oriPrice;
        }

        let html = "";

        if (referencePrice) {
            html += buildOriginPriceLine("", referencePrice);
        }

        // 售價行：有紅利 → 現金 + 紅利組合；否則純現金
        if (hasBonus) {
            html += buildComboPriceLine(price, bonus);
        } else if (isMemberPrice) {
            html += `<div class="sale-price role-current-price">${formatMoney(price)}</div>`;
        } else {
            html += buildSalePriceLine(price);
        }

        return html;
    }

    DirectoryPrice.buildHtml = buildHtml;

    DirectoryPrice.apply = function (content, data) {
        const html = buildHtml(data);

        if (!html) {
            hidePrice(content);
            return;
        }

        setPriceHtml(content, html);
    };

    DirectoryPrice.isZeroValue = isZeroValue;
    DirectoryPrice.hasDisplayValue = hasDisplayValue;
    DirectoryPrice.isSameMoney = isSameMoney;
    DirectoryPrice.formatMoney = formatMoney;
    DirectoryPrice.formatBonus = formatBonus;

})(window, window.jQuery);
