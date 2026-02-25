/**
 * =========================================================
 * Coker IntelliSense Declarations (IDE ONLY)
 * =========================================================
 *
 * ⚠️ IDE IntelliSense only.
 * ⚠️ DO NOT include via <script>.
 * ⚠️ DO NOT bundle/minify.
 * ⚠️ DO NOT write runtime logic here.
 *
 * This file describes the runtime surface of Coker.min.js.
 * (Generated from current Coker.min.js)
 */

/* =========================================================
 * Core / Meta
 * ========================================================= */

/**
 * @typedef {Object} CokerMetaExtendLogItem
 * @property {string} path
 * @property {string|null} member
 * @property {boolean} overwrite
 * @property {number} ts
 */

/**
 * @typedef {Object} CokerMeta
 * @property {Object.<string, boolean>} modules
 * @property {CokerMetaExtendLogItem[]} extendLog
 */

/**
 * @typedef {Object} CokerExtendOptions
 * @property {boolean} [overwrite] - default false
 * @property {boolean} [strict] - default true
 */

/**
 * @typedef {(name:string, factory:(Coker:CokerRoot)=>void)=>void} CokerDefineModule
 * @typedef {(source:Object, options?:CokerExtendOptions)=>void} CokerExtend
 * @typedef {(path?:string)=>void} CokerInspect
 */

/* =========================================================
 * loader (dynamic JS/CSS loader) - NEW
 * ========================================================= */

/**
 * @typedef {Object} CokerLoaderScriptOptions
 * @property {boolean} [async] - default true
 * @property {boolean} [defer] - default false
 * @property {string} [crossOrigin]
 * @property {string} [referrerPolicy]
 */

/**
 * @typedef {Object} CokerLoaderCssOptions
 * @property {string} [crossOrigin]
 * @property {string} [referrerPolicy]
 */

/**
 * @typedef {Object} CokerLoaderEnsureAssets
 * @property {string[]} [css]
 * @property {string[]} [js]
 * @property {CokerLoaderScriptOptions} [scriptOptions]
 * @property {CokerLoaderCssOptions} [cssOptions]
 */

/**
 * @typedef {Object} CokerLoader
 * @property {(url:string, options?:CokerLoaderScriptOptions)=>Promise<string>} loadScriptOnce
 * @property {(url:string, options?:CokerLoaderCssOptions)=>Promise<string>} loadCssOnce
 * @property {(assets?:CokerLoaderEnsureAssets)=>Promise<void>} ensure
 * @property {(url:string)=>boolean} isLoaded
 */

/* =========================================================
 * config.timeout
 * ========================================================= */

/**
 * @typedef {Object} CokerConfigTimeout
 * @property {number} time
 */

/**
 * @typedef {Object} CokerConfig
 * @property {CokerConfigTimeout} timeout
 */

/* =========================================================
 * API Core
 * ========================================================= */

/**
 * @typedef {Object} CokerApiAuthHeader
 * @property {string} Authorization
 */

/**
 * @typedef {Object} CokerApiGetPostOptions
 * @property {boolean} [auth] - false => no Authorization header
 * @property {Object.<string,string>} [headers] - extra headers
 * @property {Object} [ajax] - passthrough override for $.ajax options
 */

/**
 * @typedef {Object} CokerApiCore
 * @property {()=>CokerApiAuthHeader|{}} authHeader
 * @property {(url:string, data?:any, options?:CokerApiGetPostOptions)=>JQuery.jqXHR<any>} get
 * @property {(url:string, body?:any, options?:CokerApiGetPostOptions)=>JQuery.jqXHR<any>} post
 */

/* =========================================================
 * util.string
 * ========================================================= */

/**
 * @typedef {Object} CokerUtilString
 * @property {(num:number)=>string} generateRandomString
 * @property {(str:any)=>boolean} isNullOrEmpty
 * @property {(i:number)=>string} getWeekNumber
 * @property {(input:any)=>string} thousandSign
 */

/* =========================================================
 * util.html
 * ========================================================= */

/**
 * @typedef {Object} CokerUtilHtml
 * @property {(str:any)=>string} replaceAndSinge
 * @property {(text:string)=>string} htmlEncode
 * @property {()=>string} getPageTitle
 */

/* =========================================================
 * util.money
 * ========================================================= */

/**
 * @typedef {Object} CokerUtilMoney
 * @property {(input:any)=>boolean} isZeroPriceValue
 */

/* =========================================================
 * util.device
 * ========================================================= */

/**
 * @typedef {Object} CokerUtilDevice
 * @property {()=>boolean} isMobileDevice
 */

/* =========================================================
 * util.mail
 * ========================================================= */

/**
 * @typedef {Object} CokerUtilMailOpenOptions
 * @property {string} [to]
 * @property {string} [subject]
 * @property {string} [body]
 * @property {Object} [local]
 * @property {boolean} [desktopConfirm] - default true
 * @property {boolean} [mobileHint] - default true
 * @property {string} [confirmTitleKey]
 * @property {string} [confirmBodyKey]
 * @property {string} [confirmOkKey]
 * @property {string} [confirmCancelKey]
 * @property {string} [mobileTitleKey]
 * @property {string} [mobileBodyKey]
 */

/**
 * @typedef {Object} CokerUtilMail
 * @property {(opt?:CokerUtilMailOpenOptions)=>void} open
 */

/**
 * @typedef {Object} CokerUtil
 * @property {CokerUtilString} string
 * @property {CokerUtilHtml} html
 * @property {CokerUtilMoney} money
 * @property {CokerUtilDevice} device
 * @property {CokerUtilMail} mail
 */

/* =========================================================
 * dom.zipcode
 * ========================================================= */

/**
 * @typedef {Object} CokerDomZipcodeSetDataOptions
 * @property {JQuery} el
 * @property {string} addr
 */

/**
 * @typedef {Object} CokerDomZipcode
 * @property {(id:string|HTMLElement|JQuery)=>void} init
 * @property {(obj:CokerDomZipcodeSetDataOptions)=>void} setData
 * @property {($e:JQuery)=>string} getData
 */

/* =========================================================
 * dom.form
 * ========================================================= */

/**
 * @typedef {Object} CokerDomForm
 * @property {(id:string, method:Function)=>void} set
 * @property {(obj:Object, $self?:string|JQuery)=>void} insertData
 * @property {(id:string, isArrayType?:boolean)=>any} getJson
 * @property {(id:string, isArrayType?:boolean)=>any} getJsonByFieldset
 * @property {(id:string, fun?:Function)=>void} init
 * @property {(id:string)=>void} clear
 */

/**
 * @typedef {Object} CokerDom
 * @property {CokerDomZipcode} zipcode
 * @property {CokerDomForm} form
 */

/* =========================================================
 * ui.sweet
 * ========================================================= */

/**
 * @typedef {Object} CokerUiSweet
 * @property {()=>void} loading
 * @property {(icon:string, title:string, text:string, confirmtext?:string, confirmaction?:Function, canceltext?:string, canceltextaction?:Function)=>void} custom
 * @property {(text:string, action?:Function, autoclose?:boolean)=>void} success
 * @property {(title:string, text:string, action?:Function, autoclose?:boolean)=>void} error
 * @property {(title:string, text:string, confirmtexet:string, cancanceltext:string, action:Function)=>void} confirm
 * @property {(title:string, action?:Function)=>void} info
 * @property {(title:string, text:string, action?:Function)=>void} warning
 */

/**
 * @typedef {Object} CokerUi
 * @property {CokerUiSweet} sweet
 */

/* =========================================================
 * feature.search
 * ========================================================= */

/**
 * @typedef {Object} CokerFeatureSearch
 * @property {(id:string|HTMLElement|JQuery)=>void} Init
 */

/**
 * @typedef {Object} CokerFeature
 * @property {{search:CokerFeatureSearch}} search
 */

/* =========================================================
 * Activity API
 * ========================================================= */

/**
 * @typedef {Object} CokerActivityApi
 * @property {(FK_Aid:number)=>JQuery.jqXHR<any>} Click
 * @property {(FK_Aid:number)=>JQuery.jqXHR<any>} Exposure
 */

/* =========================================================
 * User API
 * ========================================================= */

/**
 * @typedef {Object} CokerUserApi
 * @property {(data:any)=>JQuery.jqXHR<any>} AddUser
 * @property {(OpenId:string)=>JQuery.jqXHR<any>} AccountOpening
 * @property {(data:any)=>JQuery.jqXHR<any>} AccountReSendOpening
 * @property {(data:any)=>JQuery.jqXHR<any>} PasswordForget
 * @property {(ForgetId:string)=>JQuery.jqXHR<any>} ForgetIdCheck
 * @property {(data:any)=>JQuery.jqXHR<any>} PasswordChange
 * @property {(data:any)=>JQuery.jqXHR<any>} EmailChange
 * @property {(data:any)=>JQuery.jqXHR<any>} Login
 * @property {()=>JQuery.jqXHR<any>} GetUser
 * @property {(data:any)=>JQuery.jqXHR<any>} UserEdit
 * @property {()=>JQuery.jqXHR<any>} Logout
 */

/* =========================================================
 * File API
 * ========================================================= */

/**
 * @typedef {Object} CokerFileApi
 * @property {(fid:number)=>JQuery.jqXHR<any>} DownloadEncryptedFile
 */

/* =========================================================
 * ThirdParty API
 * ========================================================= */

/**
 * @typedef {Object} CokerThirdPartyApi
 * @property {(ohid:number, paytype:any, support?:any)=>JQuery.jqXHR<any>} Request
 * @property {(data?:any)=>JQuery.jqXHR<any>} ECPayGetToken
 * @property {(data:any)=>JQuery.jqXHR<any>} ECPayCreatePayment
 * @property {(data:any)=>JQuery.jqXHR<any>} LogisticsGetMap
 */

/* =========================================================
 * Favorites API
 * ========================================================= */

/**
 * @typedef {Object} CokerFavoritesApi
 * @property {(Pid:number)=>JQuery.jqXHR<any>} Add
 * @property {(page:number)=>JQuery.jqXHR<any>} GetDisplay
 * @property {(Fid:number)=>JQuery.jqXHR<any>} Delete
 * @property {(Pid:number)=>JQuery.jqXHR<any>} Check
 */

/* =========================================================
 * Order API
 * ========================================================= */

/**
 * @typedef {Object} CokerOrderApi
 * @property {(data:any)=>JQuery.jqXHR<any>} AddHeader
 * @property {(data:any)=>JQuery.jqXHR<any>} FrontUserUpdate
 * @property {(id:number)=>JQuery.jqXHR<any>} GetHeader
 * @property {(id:number)=>JQuery.jqXHR<any>} GetDetails
 * @property {(ohid:number, check?:any)=>JQuery.jqXHR<any>} GetAllData
 * @property {(ohid:number)=>JQuery.jqXHR<any>} GetReorder
 * @property {()=>JQuery.jqXHR<any>} GetPaymentTypeEnum
 * @property {(data:any)=>JQuery.jqXHR<any>} CheckStock
 * @property {(ohid:number)=>JQuery.jqXHR<any>} Reorder
 * @property {(ohid:number, payment?:any)=>JQuery.jqXHR<any>} CancelOrder
 */

/* =========================================================
 * Payment API
 * ========================================================= */

/**
 * @typedef {Object} CokerPaymentApi
 * @property {(paytypeid:number)=>JQuery.jqXHR<any>} GetPaymentInfo
 */

/* =========================================================
 * Directory API
 * ========================================================= */

/**
 * @typedef {Object} CokerDirectoryApi
 * @property {(data:any)=>JQuery.jqXHR<any>} getDirectoryData
 * @property {(data:any)=>JQuery.jqXHR<any>} getDirectoryMenuData
 * @property {(data:any)=>JQuery.jqXHR<any>} getDirectoryAdvertiseData
 * @property {(data:any)=>JQuery.jqXHR<any>} SwitchPage
 * @property {(data:any)=>JQuery.jqXHR<any>} getFacet
 */

/* =========================================================
 * Token API
 * ========================================================= */

/**
 * @typedef {Object} CokerTokenApi
 * @property {()=>JQuery.jqXHR<any>} GetToken
 * @property {()=>JQuery.jqXHR<any>} CheckToken
 * @property {()=>JQuery.jqXHR<any>} AgreePrivacy
 */

/* =========================================================
 * Legacy aliases (for IntelliSense only)
 * ========================================================= */

/**
 * @typedef {Object} CokerLegacyString
 * @property {(num:number)=>string} generateRandomString
 * @property {(str:any)=>boolean} isNullOrEmpty
 * @property {(i:number)=>string} getWeekNumber
 * @property {(input:any)=>string} thousandSign
 */

/**
 * @typedef {Object} CokerLegacyStringManager
 * @property {(str:any)=>string} ReplaceAndSinge
 * @property {(text:string)=>string} htmlEncode
 */

/**
 * @typedef {Object} CokerLegacyZipcode
 * @property {(id:string|HTMLElement|JQuery)=>void} init
 * @property {(obj:CokerDomZipcodeSetDataOptions)=>void} setData
 * @property {($e:JQuery)=>string} getData
 */

/**
 * @typedef {Object} CokerLegacyForm
 * @property {(id:string, method:Function)=>void} set
 * @property {(obj:Object, $self?:string|JQuery)=>void} insertData
 * @property {(id:string, isArrayType?:boolean)=>any} getJson
 * @property {(id:string, isArrayType?:boolean)=>any} getJsonByFieldset
 * @property {(id:string, fun?:Function)=>void} init
 * @property {(id:string)=>void} clear
 */

/**
 * @typedef {Object} CokerLegacySweet
 * @property {()=>void} loading
 * @property {(icon:string, title:string, text:string, confirmtext?:string, confirmaction?:Function, canceltext?:string, canceltextaction?:Function)=>void} custom
 * @property {(text:string, action?:Function, autoclose?:boolean)=>void} success
 * @property {(title:string, text:string, action?:Function, autoclose?:boolean)=>void} error
 * @property {(title:string, text:string, confirmtexet:string, cancanceltext:string, action:Function)=>void} confirm
 * @property {(title:string, action?:Function)=>void} info
 * @property {(title:string, text:string, action?:Function)=>void} warning
 */

/**
 * @typedef {Object} CokerLegacySearch
 * @property {(id:string|HTMLElement|JQuery)=>void} Init
 */

/* =========================================================
 * Root Object
 * ========================================================= */

/**
 * @typedef {Object} CokerRoot
 * @property {CokerMeta} _meta
 * @property {CokerDefineModule} defineModule
 * @property {CokerExtend} extend
 * @property {CokerInspect} inspect
 *
 * @property {CokerConfig} config
 * @property {CokerConfigTimeout} timeout  - legacy alias
 *
 * @property {CokerLoader} loader
 *
 * @property {CokerApiCore} api
 *
 * @property {CokerUtil} util
 * @property {CokerDom} dom
 * @property {CokerUi} ui
 * @property {CokerFeature} feature
 *
 * @property {CokerActivityApi} Activity
 * @property {CokerUserApi} User
 * @property {CokerThirdPartyApi} ThirdParty
 * @property {CokerFavoritesApi} Favorites
 * @property {CokerOrderApi} Order
 * @property {CokerPaymentApi} Payment
 * @property {CokerDirectoryApi} Directory
 * @property {CokerTokenApi} Token
 *
 * @property {CokerLegacyString} String - legacy alias
 * @property {CokerLegacyStringManager} stringManager - legacy alias
 * @property {CokerLegacyZipcode} Zipcode - legacy alias
 * @property {CokerLegacyForm} Form - legacy alias
 * @property {CokerLegacySweet} sweet - legacy alias
 * @property {CokerLegacySearch} Search - legacy alias
 * 
 *  @property {CokerFileApi} File
 * 
 */

/**
 * Global Coker object
 * @type {CokerRoot}
 */
var Coker;

/**
 * Alias of Coker (getter on window.co)
 * @type {CokerRoot}
 */
var co;

/**
 * Alias of Coker (getter on window._c)
 * @type {CokerRoot}
 */
var _c;