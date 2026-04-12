/**
 * =========================================================
 * Coker Backoffice IntelliSense Declarations (IDE ONLY)
 * =========================================================
 * Source structure based on: CokerCore.min.js (backoffice bundle)
 * - DO NOT include via <script>
 * - DO NOT bundle/minify
 * - NO runtime logic here
 */

/* ---------------------------------------------------------
 * Generic helpers
 * --------------------------------------------------------- */

/**
 * @template T
 * @typedef {Promise<T>} AjaxPromise
 */

/**
 * @typedef {Object<string, any>} AnyObject
 */

/* ---------------------------------------------------------
 * Core: Data / Header / Config
 * --------------------------------------------------------- */

/**
 * @typedef {Object} CokerHeader
 * @property {string|null} Authorization
 * @property {string|null} Secret
 * @property {string} ["x-xsrf-token"]
 */

/**
 * @typedef {Object} CokerTimeConfig
 * @property {number} DataRetentionTime
 * @property {number} DataRetentionLongTime
 * @property {number} ReCheckTime
 */

/**
 * @typedef {Object} CokerData
 * @property {string} DefauleUrl
 * @property {CokerHeader} Header
 * @property {CokerTimeConfig} Time
 * @property {Array<{Id:number,Name:string,value:string}>} Target
 * @property {(html:string)=>string} ReplaceAndSinge
 * @property {(html:string)=>string} HtmlDecode
 * @property {(text:string)=>string} HtmlEncode
 */

/* ---------------------------------------------------------
 * Cookie
 * --------------------------------------------------------- */

/**
 * @typedef {Object} CokerCookie
 * @property {number} EffectiveTime
 * @property {(key:string, value:any)=>void} Add
 * @property {(obj:AnyObject)=>void} AddAll
 * @property {(key:string)=>void} Del
 * @property {(key:string)=>any} Get
 * @property {()=>void} DelAll
 */

/* ---------------------------------------------------------
 * Page / i18n
 * --------------------------------------------------------- */

/**
 * @typedef {Object} CokerPage
 * @property {()=>void} Ready
 */

/**
 * @typedef {Object} CokerI18
 * @property {()=>AjaxPromise<any>} getAll
 */

/* ---------------------------------------------------------
 * String / Object / Array util
 * --------------------------------------------------------- */

/**
 * @typedef {Object} CokerStringUtil
 * @property {(len:number)=>string} generateRandomString
 * @property {(str:any)=>boolean} isNullOrEmpty
 * @property {(i:number)=>string} getWeekNumber
 * @property {(input:any)=>string} thousandSign
 */

/**
 * @typedef {Object} CokerObjectUtil
 * @property {(target:AnyObject, source:AnyObject)=>AnyObject} merge
 * @property {(arr:Array<{key:string,value:any}>)=>AnyObject} arrayToObject
 * @property {(obj:AnyObject)=>Array<{key:string,value:any}>} objectToArray
 */

/**
 * @typedef {Object} CokerArrayUtil
 * @property {(a:any[], b:any[])=>any[]} merge
 * @property {(arr:any[], criteria:AnyObject, id?:any)=>number} Search
 * @property {(arr:any[], criteria:AnyObject)=>void} Delete
 */

/* ---------------------------------------------------------
 * Advertise / Articles / Company / Contact / Directory
 * --------------------------------------------------------- */

/**
 * @typedef {Object} CokerAdvertiseApi
 * @property {(dto:any)=>AjaxPromise<any>} AddUp
 * @property {(id:number)=>AjaxPromise<any>} GetDataOne
 * @property {(id:number)=>AjaxPromise<any>} Delete
 */

/**
 * @typedef {Object} CokerArticlesApi
 * @property {(dto:any)=>AjaxPromise<any>} AddUp
 * @property {(id:number)=>AjaxPromise<any>} GetDataOne
 * @property {(id:number)=>AjaxPromise<any>} Delete
 * @property {(dto:any)=>AjaxPromise<any>} GetConten
 * @property {(dto:any)=>AjaxPromise<any>} SaveConten
 * @property {(dto:any)=>AjaxPromise<any>} ImportConten
 */

/**
 * @typedef {Object} CokerCompanyApi
 * @property {(dto:any)=>AjaxPromise<any>} Save
 */

/**
 * @typedef {Object} CokerContactApi
 * @property {(id:number)=>AjaxPromise<any>} GetDataOne
 * @property {(dto:any)=>AjaxPromise<any>} Replay
 */

/**
 * @typedef {Object} CokerDirectoryApi
 * @property {(dto:any)=>AjaxPromise<any>} AddUp
 * @property {(id:number)=>AjaxPromise<any>} Get
 * @property {(id:number)=>AjaxPromise<any>} Delete
 */

/* ---------------------------------------------------------
 * File
 * --------------------------------------------------------- */

/**
 * @typedef {Object} CokerFileApi
 * @property {(formData:FormData)=>AjaxPromise<any>} Upload
 * @property {(formData:FormData)=>AjaxPromise<any>} Upload360
 * @property {(dto:any)=>AjaxPromise<any>} UploadYTLink
 * @property {(dto:{type:any,id:any})=>AjaxPromise<any>} getFileList
 * @property {(dto:any)=>AjaxPromise<any>} getImgFile
 * @property {(aid:number, type:any)=>AjaxPromise<any>} getAdFile
 * @property {(dto:any)=>AjaxPromise<any>} fileSortChange
 * @property {(key:any)=>AjaxPromise<any>} Delete
 * @property {(dto:any)=>AjaxPromise<any>} DeleteFileById
 *
 * @property {(elementId:string, opt?:any)=>any} UploadImageInit
 * @property {(elementId:string, opt?:any)=>any} UploadFileInit
 * @property {(elementId:string, opt?:any)=>any} Upload360Init
 * @property {(elementId:string, opt?:any)=>any} UploadVideoInit
 * @property {(elementId:string, opt?:any)=>any} UploadVideoPreviewInit
 *
 * // merged in later via Coker.Object.merge(Coker.File, {...})
 * @property {()=>void} ListFileInit
 * @property {()=>void} fileUploadWithPreview
 * @property {(liJq:any)=>void} ListFile
 */

/* ---------------------------------------------------------
 * Form
 * --------------------------------------------------------- */

/**
 * @typedef {Object} CokerFormApi
 * @property {(formId:string, onValid:Function)=>void} set
 * @property {(obj:any, formSelectorOrJq?:any)=>void} insertData
 * @property {(formId:string, isArrayType?:boolean)=>AnyObject} getJson
 * @property {(fieldsetId:string, isArrayType?:boolean)=>AnyObject} getJsonByFieldset
 * @property {(formId:string, fn?:Function)=>void} init
 * @property {(formId:string)=>void} clear
 * @property {(formId:string, type?:number)=>FormData} getFileForm
 */

/* ---------------------------------------------------------
 * Freight / HtmlContent / Member / ObjectType
 * --------------------------------------------------------- */

/**
 * @typedef {Object} CokerFreightApi
 * @property {(dto:any)=>AjaxPromise<any>} AddUp
 * @property {(id:number)=>AjaxPromise<any>} Get
 * @property {(id:number)=>AjaxPromise<any>} Delete
 */

/**
 * @typedef {Object} CokerHtmlContentApi
 * @property {(dto:any)=>AjaxPromise<any>} AddUp
 * @property {(id:number)=>AjaxPromise<any>} Get
 * @property {(id:number|string)=>AjaxPromise<any>} Delete
 * @property {()=>AjaxPromise<any>} GetTypeList
 * @property {()=>AjaxPromise<any>} GetAllComponent
 * @property {(type:any)=>AjaxPromise<any>} GetComponent
 */

/**
 * @typedef {Object} CokerMemberApi
 * @property {(id:any)=>AjaxPromise<any>} Get
 * @property {(id:any)=>AjaxPromise<any>} GetFront
 * @property {(dto:any)=>AjaxPromise<any>} FrontAddUpdate
 * @property {(dto:any)=>AjaxPromise<any>} Update
 * @property {(userId:any)=>AjaxPromise<any>} ForgetPassword
 * @property {()=>AjaxPromise<any>} GetSelf
 * @property {()=>AjaxPromise<any>} GetAllRole
 * @property {(uuid:any)=>AjaxPromise<any>} GetHistoryOrder
 * @property {(userId:any)=>AjaxPromise<any>} ResendFrontUserCreateNoticeMail
 * @property {(password:string)=>boolean} isValidPassword
 */

/**
 * @typedef {Object} CokerObjectTypeApi
 * @property {()=>AjaxPromise<any>} GetAll
 * @property {(dto:any)=>AjaxPromise<any>} createOrEdit
 * @property {(id:any)=>AjaxPromise<any>} delete
 * @property {(list:any)=>AjaxPromise<any>} updateSerNo
 * @property {(id:any)=>AjaxPromise<any>} getConten
 * @property {()=>AjaxPromise<any>} GetNewsletterConten
 * @property {()=>AjaxPromise<any>} GetNewsletterAllConten
 * @property {(dto:any)=>AjaxPromise<any>} SaveConten
 */

/* ---------------------------------------------------------
 * Order / Newsletter / Recipient / Picker / Tag / Spec
 * --------------------------------------------------------- */

/**
 * @typedef {Object} CokerOrderApi
 * @property {(ohids:any)=>AjaxPromise<any>} GetDisplay
 * @property {(id:number)=>AjaxPromise<any>} GetHeaderOld
 * @property {(id:number)=>AjaxPromise<any>} GetDetails
 * @property {(id:number)=>AjaxPromise<any>} SendMail
 * @property {(dto:any)=>AjaxPromise<any>} UpdateStatus
 * @property {()=>AjaxPromise<any>} GetOrderStatusLookup
 * @property {()=>AjaxPromise<any>} GetPreserveTypeEnum
 * @property {()=>AjaxPromise<any>} GetShippingTypeEnum
 * @property {()=>AjaxPromise<any>} GetFreightStatusTypeEnum
 */

/**
 * @typedef {Object} CokerRecipientApi
 * @property {(id:number)=>AjaxPromise<any>} DeleteRecipients
 * @property {()=>AjaxPromise<any>} GetRecipientsTag
 */

/**
 * @typedef {Object} CokerNewsletterApi
 * @property {(id:number)=>AjaxPromise<any>} send
 * @property {(dto:any)=>AjaxPromise<any>} UpdateJson
 * @property {(dto:any)=>AjaxPromise<any>} SaveConten
 */

/**
 * @typedef {Object} CokerPickerApi
 * @property {(jq:any, opt?:any)=>any} Init
 */

/**
 * @typedef {Object} CokerTagApi
 * @property {(dto:any)=>AjaxPromise<any>} AddDelect
 * @property {(pid:number)=>AjaxPromise<any>} Get
 */

/**
 * @typedef {Object} CokerSpecApi
 * @property {(dto:any)=>AjaxPromise<any>} SpecAddUp
 * @property {(id:any)=>AjaxPromise<any>} CheckRelatSpec
 * @property {(id:any)=>AjaxPromise<any>} TypeDelect
 * @property {(id:any)=>AjaxPromise<any>} CheckRelatProd
 * @property {(id:any)=>AjaxPromise<any>} SpecDelect
 * @property {()=>AjaxPromise<any>} GetPickSpecList
 */

/* ---------------------------------------------------------
 * Zipcode / Grapes / Date
 * --------------------------------------------------------- */

/**
 * @typedef {Object} CokerZipcodeApi
 * @property {(selector:any)=>void} init
 * @property {(dto:{el:any, addr:any})=>void} setData
 * @property {(jq:any)=>string} getData
 */

/**
 * @typedef {Object} CokerGrapesApi
 * @property {(editor:any, html:any, css:any)=>void} setEditor
 * @property {(editor:any, id:any, type:any)=>void} setFile
 */

/**
 * @typedef {Object} CokerDateApi
 * @property {(value:any)=>string} GetDateTimeStr
 */

/* ---------------------------------------------------------
 * PowerManagement
 * --------------------------------------------------------- */

/**
 * @typedef {Object} CokerPowerManagementApi
 * @property {()=>AjaxPromise<any>} GetAll
 * @property {()=>AjaxPromise<any>} GetPermission
 * @property {(dto:any)=>AjaxPromise<any>} GetPagePermission
 * @property {(dto:any)=>AjaxPromise<any>} SavePagePermission
 * @property {()=>AjaxPromise<any>} getAllUsers
 * @property {(id:any)=>AjaxPromise<any>} GetUser
 * @property {(dto:any)=>AjaxPromise<any>} AddUser
 * @property {(id:any)=>AjaxPromise<any>} RemoveMappingUserAndWebsite
 * @property {(dto:any)=>AjaxPromise<any>} MappingUserAndWebsite
 * @property {(dto:any)=>AjaxPromise<any>} AddRole
 * @property {(dto:any)=>AjaxPromise<any>} AddUserToRole
 * @property {(dto:any)=>AjaxPromise<any>} RemoveUserToRole
 * @property {(dto:any)=>AjaxPromise<any>} EditRole
 * @property {(id:any)=>AjaxPromise<any>} DeleteRole
 * @property {(dto:any)=>AjaxPromise<any>} GetPermissions
 * @property {(dto:any)=>AjaxPromise<any>} SavePermissions
 */

/* ---------------------------------------------------------
 * Product (nested)
 * --------------------------------------------------------- */

/**
 * @typedef {Object} CokerProductAddUpApi
 * @property {(dto:any)=>AjaxPromise<any>} Product
 * @property {(dto:any)=>AjaxPromise<any>} ProdTechCert
 * @property {(dto:any)=>AjaxPromise<any>} ProdPrice
 * @property {(formData:FormData)=>AjaxPromise<any>} Import
 */

/**
 * @typedef {Object} CokerProductGetApi
 * @property {(id:any)=>AjaxPromise<any>} ProdOne
 * @property {(pid:any)=>AjaxPromise<any>} ProdStock
 * @property {(pid:any)=>AjaxPromise<any>} ProdTechCert
 * @property {(psid:any)=>AjaxPromise<any>} ProdPrice
 */

/**
 * @typedef {Object} CokerProductDeleteApi
 * @property {(id:any)=>AjaxPromise<any>} Prod
 * @property {(id:any)=>AjaxPromise<any>} Stock
 * @property {(id:any)=>AjaxPromise<any>} Price
 */

/**
 * @typedef {Object} CokerProductContentApi
 * @property {(dto:any)=>AjaxPromise<any>} GetConten
 * @property {(dto:any)=>AjaxPromise<any>} SaveConten
 * @property {(dto:any)=>AjaxPromise<any>} ImportConten
 */

/**
 * @typedef {Object} CokerProductThirdPartyApi
 * @property {(dto:any)=>AjaxPromise<any>} save
 */

/**
 * @typedef {Object} CokerProductSpecApi
 * @property {()=>void} ListInit
 */

/**
 * @typedef {Object} CokerProductStockApi
 * @property {(dto:any)=>AjaxPromise<any>} BatchSet
 */

/**
 * @typedef {Object} CokerProductApi
 * @property {CokerProductAddUpApi} AddUp
 * @property {CokerProductGetApi} Get
 * @property {CokerProductDeleteApi} Delete
 * @property {CokerProductContentApi} Content
 * @property {CokerProductThirdPartyApi} ThirdParty
 * @property {CokerProductSpecApi} Spec
 * @property {CokerProductStockApi} Stock
 */

/* ---------------------------------------------------------
 * Role / Remote / sweet / StoreSet / ThirdParty / TechCert
 * --------------------------------------------------------- */

/**
 * @typedef {Object} CokerRoleApi
 * @property {(id:any)=>AjaxPromise<any>} Delete
 */

/**
 * @typedef {Object} CokerRemoteApi
 * @property {(dto:any)=>AjaxPromise<any>} GetRemoteCount
 */

/**
 * @typedef {Object} CokerSweetConfig
 * @property {number} timeout
 */

/**
 * @typedef {Object} CokerSweetApi
 * @property {CokerSweetConfig} config
 * @property {()=>void} loading
 * @property {(html:string, onOk?:Function, autoClose?:boolean)=>void} success
 * @property {(title:string, html:string, onOk?:Function, autoClose?:boolean)=>void} error
 * @property {(title:string, html:string, okText:string, cancelText:string, onOk?:Function, onCancel?:Function)=>void} confirm
 * @property {(title:string, html:string, okText:string, cancelText:string)=>Promise<boolean>} confirmAsync
 * @property {(title:string, html:string, onOk?:Function)=>void} warn
 * @property {(template:string, keyword:string)=>string} TitleHilight
 */

/**
 * @typedef {Object} CokerStoreSetApi
 * @property {(dto:any)=>AjaxPromise<any>} GetValues
 * @property {(dto:any)=>AjaxPromise<any>} SaveValues
 * @property {(dto:any)=>AjaxPromise<any>} getGroupStructure
 * @property {(elementId:string)=>AjaxPromise<any>} CreateFrom
 */

/**
 * @typedef {Object} CokerThirdPartyApi
 * @property {(dto:any)=>AjaxPromise<any>} HandleThirdPartyPayment
 * @property {(payment:any, ohid:any, refund:any)=>AjaxPromise<any>} PayRefund
 * @property {(payment:any, transactionId:any)=>AjaxPromise<any>} CheckRefund
 * @property {(ohid:any, thirdparty:any)=>AjaxPromise<any>} CheckPaymentStatus
 * @property {{ Confirm:(ohid:any)=>AjaxPromise<any>, PayVoid:(ohid:any)=>AjaxPromise<any> }} Line
 */

/**
 * @typedef {Object} CokerTechnicalCertificateApi
 * @property {(dto:any)=>AjaxPromise<any>} AddUp
 * @property {(id:any)=>AjaxPromise<any>} Get
 * @property {(id:any)=>AjaxPromise<any>} Delete
 * @property {(dto:any)=>AjaxPromise<any>} GetConten
 * @property {(dto:any)=>AjaxPromise<any>} SaveConten
 */

/* ---------------------------------------------------------
 * Templates (fetch-based)
 * --------------------------------------------------------- */

/**
 * @typedef {Object} CokerTemplatesApi
 * @property {()=>Promise<any>} getDefaultFooter
 * @property {(dto:any)=>Promise<any>} saveDefaultFooter
 * @property {(dto:any)=>Promise<any>} importDefaultFooter
 * @property {()=>Promise<any>} getDefaultHeader
 * @property {(dto:any)=>Promise<any>} saveDefaultHeader
 */

/* ---------------------------------------------------------
 * User / UserHabits / WebMenus / Website
 * --------------------------------------------------------- */

/**
 * @typedef {Object} CokerUserApi
 * @property {(dto:any)=>AjaxPromise<any>} Login
 * @property {()=>AjaxPromise<any>} Logout
 * @property {()=>AjaxPromise<any>} Check
 * @property {(dto:any)=>AjaxPromise<any>} UpdatePassword
 */

/**
 * @typedef {Object} CokerUserHabitsApi
 * @property {(dto:any)=>AjaxPromise<any>} AddUpUserGroup
 * @property {(id:any)=>AjaxPromise<any>} DeleteUserGroup
 * @property {(id:any)=>AjaxPromise<any>} GetUserGroupOne
 */

/**
 * @typedef {Object} CokerWebMesnusApi
 * @property {()=>AjaxPromise<any>} getAll
 * @property {()=>AjaxPromise<any>} getAllList
 * @property {(dto:any)=>AjaxPromise<any>} createOrEdit
 * @property {(id:any)=>AjaxPromise<any>} getConten
 * @property {(dto:any)=>AjaxPromise<any>} saveConten
 * @property {(dto:any)=>AjaxPromise<any>} importConten
 * @property {(id:any)=>AjaxPromise<any>} delete
 * @property {(list:any)=>AjaxPromise<any>} updateLevelAndSerNo
 * @property {(id:any, isVisible:boolean)=>AjaxPromise<any>} SetVisible
 * @property {()=>AjaxPromise<any>} GetPageTypeList
 */

/**
 * @typedef {Object} CokerWebSiteApi
 * @property {(page:any)=>AjaxPromise<any>} getPageAll
 * @property {(id:any)=>AjaxPromise<any>} exchange
 * @property {()=>AjaxPromise<any>} getPrivacyAndTerms
 * @property {(dto:any)=>AjaxPromise<any>} Save
 * @property {()=>AjaxPromise<any>} LoadFrameCss
 * @property {(css:any)=>AjaxPromise<any>} SettingCss
 */

/* ---------------------------------------------------------
 * Root Object: Coker / co / _c
 * --------------------------------------------------------- */

/**
 * @typedef {Object} CokerBackofficeRoot
 * @property {(source:AnyObject)=>void} extend
 * @property {CokerData} Data
 * @property {CokerCookie} Cookie
 * @property {CokerPage} Page
 * @property {CokerI18} i18
 * @property {CokerStringUtil} String
 * @property {CokerObjectUtil} Object
 * @property {CokerArrayUtil} Array
 *
 * @property {CokerAdvertiseApi} Advertise
 * @property {CokerArticlesApi} Articles
 * @property {CokerCompanyApi} Company
 * @property {CokerContactApi} Contact
 * @property {CokerDirectoryApi} Directory
 * @property {CokerFileApi} File
 * @property {CokerFormApi} Form
 * @property {CokerFreightApi} Freight
 * @property {CokerHtmlContentApi} HtmlContent
 * @property {CokerMemberApi} Member
 * @property {CokerObjectTypeApi} ObjectType
 * @property {CokerOrderApi} Order
 * @property {CokerRecipientApi} Recipient
 * @property {CokerNewsletterApi} Newsletter
 * @property {CokerPickerApi} Picker
 * @property {CokerTagApi} Tag
 * @property {CokerSpecApi} Spec
 * @property {CokerZipcodeApi} Zipcode
 * @property {CokerGrapesApi} Grapes
 * @property {CokerDateApi} Date
 * @property {CokerPowerManagementApi} PowerManagement
 * @property {CokerProductApi} Product
 * @property {CokerRoleApi} Role
 * @property {CokerRemoteApi} Remote
 * @property {CokerSweetApi} sweet
 * @property {CokerStoreSetApi} StoreSet
 * @property {CokerThirdPartyApi} ThirdParty
 * @property {CokerTechnicalCertificateApi} TechnicalCertificate
 * @property {CokerTemplatesApi} Templates
 * @property {CokerUserApi} User
 * @property {CokerUserHabitsApi} UserHabits
 * @property {CokerWebMesnusApi} WebMesnus
 * @property {CokerWebSiteApi} WebSite
 */

/**
 * Global Coker object (backoffice)
 * @type {CokerBackofficeRoot}
 */
var Coker;

/**
 * Alias: co === Coker (backoffice)
 * @type {CokerBackofficeRoot}
 */
var co;

/**
 * Alias: _c === Coker (backoffice, internal)
 * @type {CokerBackofficeRoot}
 */
var _c;