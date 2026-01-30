/**
 * =========================================================
 * Coker IntelliSense Declarations (IDE ONLY)
 * =========================================================
 *
 * ⚠️ This file is for IDE IntelliSense only.
 * ⚠️ DO NOT include via <script>.
 * ⚠️ DO NOT bundle or minify.
 * ⚠️ DO NOT write runtime logic here.
 *
 * Purpose:
 * - Provide IntelliSense / autocomplete for `Coker` and `co`
 * - Describe API structure built via Coker.extend(...)
 */

/* =========================================================
 * util.money
 * ========================================================= */

/**
 * @typedef {Object} CokerUtilMoney
 * @property {(input:any)=>boolean} isZeroPriceValue
 */

/**
 * @typedef {Object} CokerUtil
 * @property {CokerUtilMoney} money
 */


/* =========================================================
 * User API
 * ========================================================= */

/**
 * @typedef {Object} CokerUserApi
 * @property {(data:any)=>Promise<any>} AddUser
 * @property {(openId:string)=>Promise<any>} AccountOpening
 * @property {(data:any)=>Promise<any>} AccountReSendOpening
 * @property {(data:any)=>Promise<any>} PasswordForget
 * @property {(forgetId:string)=>Promise<any>} ForgetIdCheck
 * @property {(data:any)=>Promise<any>} PasswordChange
 * @property {(data:any)=>Promise<any>} EmailChange
 * @property {(data:any)=>Promise<any>} Login
 * @property {()=>Promise<any>} GetUser
 * @property {(data:any)=>Promise<any>} UserEdit
 * @property {()=>Promise<any>} Logout
 */


/* =========================================================
 * Order API
 * ========================================================= */

/**
 * @typedef {Object} CokerOrderApi
 * @property {(data:any)=>Promise<any>} AddHeader
 * @property {(data:any)=>Promise<any>} FrontUserUpdate
 * @property {(id:number)=>Promise<any>} GetHeader
 * @property {(id:number)=>Promise<any>} GetDetails
 * @property {(ohid:number, check?:any)=>Promise<any>} GetAllData
 * @property {(ohid:number)=>Promise<any>} GetReorder
 * @property {()=>Promise<any>} GetPaymentTypeEnum
 * @property {(data:any)=>Promise<any>} CheckStock
 * @property {(ohid:number)=>Promise<any>} Reorder
 * @property {(ohid:number, payment?:any)=>Promise<any>} CancelOrder
 */


/* =========================================================
 * Payment API
 * ========================================================= */

/**
 * @typedef {Object} CokerPaymentApi
 * @property {(paytypeid:number)=>Promise<any>} GetPaymentInfo
 */


/* =========================================================
 * Favorites API
 * ========================================================= */

/**
 * @typedef {Object} CokerFavoritesApi
 * @property {(pid:number)=>Promise<any>} Add
 * @property {(page:number)=>Promise<any>} GetDisplay
 * @property {(fid:number)=>Promise<any>} Delete
 * @property {(pid:number)=>Promise<any>} Check
 */


/* =========================================================
 * ThirdParty API
 * ========================================================= */

/**
 * @typedef {Object} CokerThirdPartyApi
 * @property {(ohid:number, paytype:any, support?:any)=>Promise<any>} Request
 * @property {(data:any)=>Promise<any>} ECPayGetToken
 * @property {(data:any)=>Promise<any>} ECPayCreatePayment
 * @property {(data:any)=>Promise<any>} LogisticsGetMap
 */


/* =========================================================
 * API Core
 * ========================================================= */

/**
 * @typedef {Object} CokerApiCore
 * @property {(url:string, data?:any, options?:any)=>Promise<any>} get
 * @property {(url:string, body?:any, options?:any)=>Promise<any>} post
 */


/* =========================================================
 * Root Object
 * ========================================================= */

/**
 * @typedef {Object} CokerRoot
 * @property {CokerUtil} util
 * @property {CokerApiCore} api
 * @property {CokerUserApi} User
 * @property {CokerOrderApi} Order
 * @property {CokerPaymentApi} Payment
 * @property {CokerFavoritesApi} Favorites
 * @property {CokerThirdPartyApi} ThirdParty
 */

/**
 * Global Coker object
 * @type {CokerRoot}
 */
var Coker;

/**
 * Alias of Coker (for convenience)
 * @type {CokerRoot}
 */
var co;
