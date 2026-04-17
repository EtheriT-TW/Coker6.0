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
        return `$${text}元`;
    }

    function formatBonus(value) {
        const text = normalizeText(value);
        if (!text) return "";
        return `${text}紅利點`;
    }

    function buildOriginPriceLine(label, value, extraClass) {
        if (!hasDisplayValue(value)) return "";
        const safeClass = extraClass ? ` ${extraClass}` : "";
        return `<div class="origin-price text-decoration-line-through${safeClass}">${label}：${formatMoney(value)}</div>`;
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
     * 1. priceDisplayText 有值時，優先直接顯示
     * 2. oriPrice / suggestPrice = 0 視為未設定，不顯示
     * 3. oriPrice / suggestPrice 只有在有效且 != price 時才顯示
     * 4. 不預設 price 是會員價，統一以「售價」概念處理
     * 5. cash + bonus 顯示成同一組：$700元 + 50點
     * 6. 只有 bonus 時，只顯示 bonus
     */
    function buildHtml(data) {
        if (!data) return "";

        const priceDisplayText = normalizeText(data.priceDisplayText);
        const price = data.price;
        const oriPrice = data.oriPrice;
        const suggestPrice = data.suggestPrice;
        const bonus = data.bonus;
        const baseRoleName = normalizeText(data.baseRoleName);
        const currentRoleName = normalizeText(data.currentRoleName);

        const hasCashPrice = hasDisplayValue(price);
        const hasBonus = hasDisplayValue(bonus);

        const hasSuggestPrice =
            hasDisplayValue(suggestPrice) &&
            !isSameMoney(suggestPrice, price);

        const hasOriPrice =
            hasDisplayValue(oriPrice) &&
            !isSameMoney(oriPrice, price);

        const isMemberRole =
            !!currentRoleName &&
            currentRoleName !== "非會員";

        if (priceDisplayText) {
            return `<div class="sale-price">${priceDisplayText}</div>`;
        }

        if (!hasCashPrice && !hasBonus) {
            return "";
        }

        if (!hasCashPrice && hasBonus) {
            return buildBonusOnlyLine(bonus);
        }

        let html = "";

        // 第一行：建議售價
        if (hasSuggestPrice) {
            html += buildOriginPriceLine("建議售價", suggestPrice);
        }

        // bonus 場景先沿用舊邏輯
        if (hasCashPrice && hasBonus) {
            html += buildComboPriceLine(price, bonus);
            return html;
        }

        // 會員三行模式：
        // 1. 當前角色不是非會員
        // 2. oriPrice 有值
        // 3. oriPrice 與 price 不同
        if (isMemberRole && hasOriPrice) {
            if (baseRoleName) {
                html += buildOriginPriceLine(baseRoleName, oriPrice);
            } else {
                html += buildOriginPriceLine("非會員", oriPrice);
            }

            if (currentRoleName) {
                html += `<div class="sale-price role-current-price">${currentRoleName}：${formatMoney(price)}</div>`;
            } else {
                html += buildSalePriceLine(price);
            }

            return html;
        }

        // 其餘情況：非會員 / 會員但比較價不存在或相同
        html += buildSalePriceLine(price);

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