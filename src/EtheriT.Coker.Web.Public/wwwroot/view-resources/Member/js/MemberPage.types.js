/*
 * MemberPage.types.js
 *
 * IntelliSense / JSDoc helper for Member page scripts.
 * Do NOT include this file in bundles.json.
 * Do NOT reference this file from Razor views.
 * This file is only for editor hints and structure documentation.
 */

/* global bootstrap, JQuery */

/**
 * Member page tab names.
 * @typedef {"info"|"bonus"|"order"|"favorites"|"browsing"} MemberPageTabName
 */

/**
 * Shared Member page runtime state.
 * @typedef {Object} MemberPageState
 * @property {MemberPageTabName} tabNow Current active member tab.
 * @property {string} dateNow Current date text, formatted as yyyy-MM-dd.
 * @property {string} oldEmail Original email value loaded from the server.
 * @property {Object|null} loginData Login/token check result passed from Coker.Token.CheckToken().
 * @property {bootstrap.Modal|null} resetEmailModal Bootstrap instance for ResetEmailModal.
 * @property {HTMLElement|null} resetEmailModalElement ResetEmailModal DOM element.
 * @property {JQuery|null} resetEmailCaptchaInput Reset email captcha input jQuery object.
 * @property {JQuery|null} resetEmailCaptchaImage Reset email captcha image jQuery object.
 * @property {JQuery|null} resetEmailForm Reset email form jQuery object.
 * @property {bootstrap.Modal|null} reOrderAlertModal Bootstrap instance for ReOrderAlertModal.
 * @property {HTMLElement|null} reOrderAlertModalElement ReOrderAlertModal DOM element.
 * @property {bootstrap.Modal|null} ecPayModal Bootstrap instance for ECPayModal.
 */

/**
 * Centralized selectors used by Member page scripts.
 * @typedef {Object} MemberPageSelectors
 * @property {string} twZipcode
 * @property {string} userDataForm
 * @property {string} toolList
 * @property {string} tabContent
 * @property {string} infoPane
 * @property {string} orderPane
 * @property {string} favoritePane
 * @property {string} historyPane
 * @property {string} bonusPane
 * @property {string} resetEmailModal
 * @property {string} resetEmailForm
 * @property {string} resetEmailCaptchaInput
 * @property {string} resetEmailCaptchaImage
 * @property {string} reOrderAlertModal
 * @property {string} ecPayModal
 */

/**
 * Common utilities used by Member page modules.
 * @typedef {Object} MemberPageUtils
 * @property {function(): string} todayText Returns current date in yyyy-MM-dd format.
 * @property {function(string=): (number|null)} getHashPage Parses page number from hash text.
 * @property {function(string, string): void} activateTab Activates a tab pane and tab button.
 * @property {function(MemberPageTabName): void} setTabNow Sets MemberPage.State.tabNow.
 * @property {function(JQuery=): boolean} requireRenderer Checks whether DirectoryRenderer.renderItemsByExternalTemplate exists.
 * @property {function(function=): void} loadBarcodeScript Loads JsBarcode and then invokes callback.
 */

/**
 * Pagination helper for tab content.
 * @typedef {Object} MemberPagePagination
 * @property {function(JQuery, number, string): void} init Creates page buttons and binds paging events.
 * @property {function(JQuery, number, number): void} change Updates active page button and ellipsis visibility.
 */

/**
 * Reset Email and reorder alert modal behavior.
 * @typedef {Object} MemberPageModals
 * @property {function(Object=): void} init Initializes modal instances and modal event handlers.
 * @property {function(): void} initResetPasswordUi Enables the old-password area in reset password modal.
 * @property {function(): void} bindResetEmail Binds Reset Email modal events.
 * @property {function(): void} bindReOrderAlert Binds ReOrderAlertModal events.
 * @property {function(): void} submitResetEmail Validates captcha and submits reset email flow.
 * @property {function(): void} resetEmailAction Calls Coker.User.EmailChange and handles result.
 */

/**
 * Member profile tab behavior.
 * @typedef {Object} MemberPageProfile
 * @property {function(): void} init Initializes profile events and loads profile data.
 * @property {function(): void} bindEvents Binds logout, modify, and reset password buttons.
 * @property {function(): void} load Loads member data from Coker.User.GetUser().
 * @property {function(): void} submit Validates and submits member profile data.
 */

/**
 * Bonus tab behavior.
 * @typedef {Object} MemberPageBonus
 * @property {function(number): void} loadPage Loads a bonus page.
 * @property {function(JQuery, Array<Object>): void} render Renders bonus list and bonus usage logs.
 */

/**
 * Favorite and browsing-history product list behavior.
 * @typedef {Object} MemberPageProducts
 * @property {function(number): void} loadFavoritesPage Loads favorite product display page.
 * @property {function(number): void} loadBrowsingHistoryPage Loads browsing-history product display page.
 */

/**
 * Order payment and third-party payment flow behavior.
 * @typedef {Object} MemberPageOrderPayment
 * @property {function({orderHeader:Object}): void} repay Starts order repayment flow.
 * @property {function({orderHeader:Object}): void} requestPayment Requests third-party payment URL or ECPay embedded payment data.
 * @property {function({orderHeader:Object}, Object): void} openECPay Opens embedded ECPay modal.
 * @property {function(): void} submitECPay Gets ECPay token and creates payment.
 * @property {function(Object): void} handleECPayCreateResult Handles ECPay payment creation result.
 * @property {function(Object, number|string): void} showECPayPaymentInfo Shows ECPay ATM/CVS/BARCODE payment info.
 */

/**
 * Order tab behavior.
 * @typedef {Object} MemberPageOrders
 * @property {function(number): void} loadPage Loads order history page.
 * @property {function(Array<Object>): void} render Renders order list.
 * @property {function(JQuery, Object): void} appendActions Appends order action buttons according to action state.
 * @property {function(JQuery, Object): void} appendCancelButton Appends cancel order button.
 * @property {function(JQuery, Object): void} appendRepayButton Appends repay order button.
 * @property {function(JQuery, Object): void} appendRepayCountdown Appends repay countdown text.
 * @property {function(JQuery, Object): void} applyStateStyle Applies order state badge styling.
 * @property {function(JQuery, Object): void} bindCollapse Binds detail collapse button.
 * @property {function(JQuery, Object): void} bindBuyAgain Binds reorder button.
 * @property {function(JQuery, Object): void} bindPaymentInfo Binds payment info button.
 * @property {function(JQuery, Array<Object>): void} renderDetails Renders order detail rows.
 * @property {function(JQuery, Object, Array<Object>): void} renderSummary Renders freight, totals, invoice, shipping, and bonus summary.
 */

/**
 * Hash router for Member page tabs.
 * @typedef {Object} MemberPageRouter
 * @property {function(): void} init Initializes tab click events and hash change handler.
 * @property {function(): void} bindTabButtons Binds tab buttons to hash changes.
 * @property {function(): void} change Routes current hash to the appropriate tab.
 * @property {function(): void} showInfo Shows member info tab.
 * @property {function(string): void} showBonus Shows bonus tab and loads requested page.
 * @property {function(string): void} showOrder Shows order tab and loads requested page.
 * @property {function(string): void} showBrowsing Shows browsing-history tab and loads requested page.
 * @property {function(string): void} showFavorites Shows favorites tab and loads requested page.
 */

/**
 * Member page entry point.
 * @typedef {Object} MemberPageInit
 * @property {function(): void} pageReady Main entry called by global PageReady().
 * @property {function(): void} initZipcode Initializes Taiwan zipcode widget.
 * @property {function(Object): void} start Starts member page after login check.
 */

/**
 * Global MemberPage namespace.
 * @typedef {Object} MemberPageNamespace
 * @property {MemberPageState} State
 * @property {MemberPageSelectors} Selectors
 * @property {MemberPageUtils} Utils
 * @property {MemberPagePagination} Pagination
 * @property {MemberPageModals} Modals
 * @property {MemberPageProfile} Profile
 * @property {MemberPageBonus} Bonus
 * @property {MemberPageProducts} Products
 * @property {MemberPageOrderPayment} OrderPayment
 * @property {MemberPageOrders} Orders
 * @property {MemberPageRouter} Router
 * @property {MemberPageInit} Init
 */

/**
 * IntelliSense declaration only.
 * This variable is initialized by Member.Core.js at runtime.
 * @global
 * @type {MemberPageNamespace}
 */
window.MemberPage;

/**
 * Member page global entry point kept for Razor / legacy bootstrapping.
 * Implemented by Member.Init.js.
 * @global
 * @type {function(): void}
 */
window.PageReady;
