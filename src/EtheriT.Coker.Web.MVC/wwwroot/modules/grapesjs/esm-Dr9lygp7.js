import { n as __exportAll } from "./rolldown-runtime-B-1-B7_t.js";
//#region node_modules/jodit/esm/core/constants.js
var constants_exports = /* @__PURE__ */ __exportAll({
	ACCURACY: () => 10,
	APP_VERSION: () => APP_VERSION,
	BASE_PATH: () => BASE_PATH,
	BASE_PATH_IS_MIN: () => BASE_PATH_IS_MIN,
	BR: () => "br",
	CLIPBOARD_ID: () => CLIPBOARD_ID,
	COMMAND_KEYS: () => COMMAND_KEYS,
	EMULATE_DBLCLICK_TIMEOUT: () => 300,
	ES: () => ES,
	FAT_MODE: () => true,
	HOMEPAGE: () => HOMEPAGE,
	INSEPARABLE_TAGS: () => INSEPARABLE_TAGS,
	INSERT_AS_HTML: () => INSERT_AS_HTML,
	INSERT_AS_TEXT: () => INSERT_AS_TEXT,
	INSERT_CLEAR_HTML: () => INSERT_CLEAR_HTML,
	INSERT_ONLY_TEXT: () => INSERT_ONLY_TEXT,
	INVISIBLE_SPACE: () => "﻿",
	INVISIBLE_SPACE_REG_EXP: () => INVISIBLE_SPACE_REG_EXP,
	INVISIBLE_SPACE_REG_EXP_END: () => INVISIBLE_SPACE_REG_EXP_END,
	INVISIBLE_SPACE_REG_EXP_START: () => INVISIBLE_SPACE_REG_EXP_START,
	IS_BLOCK: () => IS_BLOCK,
	IS_ES_MODERN: () => true,
	IS_ES_NEXT: () => true,
	IS_IE: () => IS_IE,
	IS_INLINE: () => IS_INLINE,
	IS_MAC: () => IS_MAC,
	IS_PROD: () => true,
	IS_TEST: () => IS_TEST,
	KEY_ALIASES: () => KEY_ALIASES,
	KEY_ALT: () => "Alt",
	KEY_BACKSPACE: () => KEY_BACKSPACE,
	KEY_DELETE: () => KEY_DELETE,
	KEY_DOWN: () => KEY_DOWN,
	KEY_ENTER: () => KEY_ENTER,
	KEY_ESC: () => KEY_ESC,
	KEY_F3: () => "F3",
	KEY_LEFT: () => KEY_LEFT,
	KEY_META: () => KEY_META,
	KEY_RIGHT: () => KEY_RIGHT,
	KEY_SPACE: () => KEY_SPACE,
	KEY_TAB: () => "Tab",
	KEY_UP: () => KEY_UP,
	LIST_TAGS: () => LIST_TAGS,
	MARKER_CLASS: () => MARKER_CLASS,
	MODE_SOURCE: () => 2,
	MODE_SPLIT: () => 3,
	MODE_WYSIWYG: () => 1,
	NBSP_SPACE: () => "\xA0",
	NEARBY: () => 5,
	NO_EMPTY_TAGS: () => NO_EMPTY_TAGS,
	PARAGRAPH: () => "p",
	PASSIVE_EVENTS: () => PASSIVE_EVENTS,
	SAFE_COUNT_CHANGE_CALL: () => 10,
	SET_TEST: () => SET_TEST,
	SOURCE_CONSUMER: () => SOURCE_CONSUMER,
	SPACE_REG_EXP: () => SPACE_REG_EXP,
	SPACE_REG_EXP_END: () => SPACE_REG_EXP_END,
	SPACE_REG_EXP_START: () => SPACE_REG_EXP_START,
	TEMP_ATTR: () => TEMP_ATTR,
	TEXT_HTML: () => TEXT_HTML,
	TEXT_PLAIN: () => TEXT_PLAIN,
	TEXT_RTF: () => TEXT_RTF,
	TOKENS: () => TOKENS,
	globalDocument: () => globalDocument,
	globalWindow: () => globalWindow,
	lang: () => lang
});
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var APP_VERSION = "4.14.2";
var ES = "es2020";
var IS_TEST = false;
var HOMEPAGE = "https://xdsoft.net/jodit/";
var SET_TEST = () => IS_TEST = true;
var TOKENS = {};
var INVISIBLE_SPACE_REG_EXP = () => /[\uFEFF]/g;
var INVISIBLE_SPACE_REG_EXP_END = () => /[\uFEFF]+$/g;
var INVISIBLE_SPACE_REG_EXP_START = () => /^[\uFEFF]+/g;
var SPACE_REG_EXP = () => /[\s\n\t\r\uFEFF\u200b]+/g;
var SPACE_REG_EXP_START = () => /^[\s\n\t\r\uFEFF\u200b]+/g;
var SPACE_REG_EXP_END = () => /[\s\n\t\r\uFEFF\u200b]+$/g;
var globalWindow = typeof window !== "undefined" ? window : void 0;
var globalDocument = typeof document !== "undefined" ? document : void 0;
var IS_BLOCK = /^(ADDRESS|ARTICLE|ASIDE|BLOCKQUOTE|CANVAS|DD|DFN|DIV|DL|DT|FIELDSET|FIGCAPTION|FIGURE|FOOTER|FORM|H[1-6]|HEADER|HGROUP|HR|LI|MAIN|NAV|NOSCRIPT|OUTPUT|P|PRE|RUBY|SCRIPT|STYLE|OBJECT|OL|SECTION|IFRAME|JODIT|JODIT-MEDIA|UL|TR|TD|TH|TBODY|THEAD|TFOOT|TABLE|BODY|HTML|VIDEO)$/i;
var IS_INLINE = /^(STRONG|SPAN|I|EM|B|SUP|SUB|A|U)$/i;
var LIST_TAGS = /* @__PURE__ */ new Set(["ul", "ol"]);
var __UNSEPARABLE_TAGS = [
	"img",
	"video",
	"svg",
	"iframe",
	"script",
	"input",
	"textarea",
	"link",
	"jodit",
	"jodit-media"
];
var INSEPARABLE_TAGS = /* @__PURE__ */ new Set([
	...__UNSEPARABLE_TAGS,
	"br",
	"hr"
]);
var NO_EMPTY_TAGS = new Set(__UNSEPARABLE_TAGS);
var KEY_META = "Meta";
var KEY_BACKSPACE = "Backspace";
var KEY_ENTER = "Enter";
var KEY_ESC = "Escape";
var KEY_LEFT = "ArrowLeft";
var KEY_UP = "ArrowUp";
var KEY_RIGHT = "ArrowRight";
var KEY_DOWN = "ArrowDown";
var KEY_SPACE = "Space";
var KEY_DELETE = "Delete";
var COMMAND_KEYS = [
	KEY_META,
	KEY_BACKSPACE,
	KEY_DELETE,
	KEY_UP,
	KEY_DOWN,
	KEY_RIGHT,
	KEY_LEFT,
	KEY_ENTER,
	KEY_ESC,
	"F3",
	"Tab"
];
/**
* Is Internet Explorer
*/
var IS_IE = typeof navigator !== "undefined" && (navigator.userAgent.indexOf("MSIE") !== -1 || /rv:11.0/i.test(navigator.userAgent));
/**
* For IE11 it will be 'text'. Need for dataTransfer.setData
*/
var TEXT_PLAIN = IS_IE ? "text" : "text/plain";
var TEXT_HTML = IS_IE ? "html" : "text/html";
var TEXT_RTF = IS_IE ? "rtf" : "text/rtf";
var MARKER_CLASS = "jodit-selection_marker";
/**
* Paste the copied text as HTML, all content will be pasted exactly as it was on the clipboard.
* So how would you copy its code directly into the source document.
* ```
* <h1 style="color:red">test</h1>
* ```
* Will be inserted into the document as
* ```
* <h1 style="color:red">test</h1>
* ```
*/
var INSERT_AS_HTML = "insert_as_html";
/**
* Same as [[INSERT_AS_HTML]], but content will be stripped of extra styles and empty tags
* ```html
* <h1 style="color:red">test</h1>
* ```
* Will be inserted into the document as
* ```html
* <h1>test</h1>
* ```
*/
var INSERT_CLEAR_HTML = "insert_clear_html";
/**
* The contents of the clipboard will be pasted into the document as plain text, i.e. all tags will be displayed as text.
* ```html
* <h1>test</h1>
* ```
* Will be inserted into the document as
* ```html
* &gt;&lt;h1&gt;test&lt;/h1&gt;
* ```
*/
var INSERT_AS_TEXT = "insert_as_text";
/**
* All tags will be stripped:
* ```html
* <h1>test</h1>
* ```
* Will be inserted into the document as
* ```html
* test
* ```
*/
var INSERT_ONLY_TEXT = "insert_only_text";
var IS_MAC = typeof globalWindow !== "undefined" && /Mac|iPod|iPhone|iPad/.test(globalWindow.navigator.platform);
var KEY_ALIASES = {
	add: "+",
	break: "pause",
	cmd: "meta",
	command: "meta",
	ctl: "control",
	ctrl: "control",
	del: "delete",
	down: "arrowdown",
	esc: "escape",
	ins: "insert",
	left: "arrowleft",
	mod: IS_MAC ? "meta" : "control",
	opt: "alt",
	option: "alt",
	return: "enter",
	right: "arrowright",
	space: "space",
	spacebar: "space",
	up: "arrowup",
	win: "meta",
	windows: "meta"
};
var removeScriptName = (src) => {
	const parts = src.split("/");
	const isMin = false;
	if (/\.js/.test(parts[parts.length - 1])) return {
		basePath: parts.slice(0, parts.length - 1).join("/") + "/",
		isMin
	};
	return {
		basePath: src,
		isMin
	};
};
var { basePath, isMin } = (() => {
	if (typeof document === "undefined") return {
		basePath: "",
		isMin: Boolean(false)
	};
	const script = globalDocument.currentScript;
	if (script) return removeScriptName(script.src);
	const scripts = globalDocument.querySelectorAll("script[src]");
	if (scripts && scripts.length) return removeScriptName(scripts[scripts.length - 1].src);
	return removeScriptName(globalWindow.location.href);
})();
/**
* Path to the current script
*/
var BASE_PATH = basePath;
/**
* Current script is minified
*/
var BASE_PATH_IS_MIN = isMin;
var TEMP_ATTR = "data-jodit-temp";
var lang = {};
var CLIPBOARD_ID = "clipboard";
var SOURCE_CONSUMER = "source-consumer";
var PASSIVE_EVENTS = /* @__PURE__ */ new Set([
	"touchstart",
	"touchend",
	"scroll",
	"mousewheel",
	"mousemove",
	"touchmove"
]);
//#endregion
//#region node_modules/jodit/esm/core/helpers/checker/is-function.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/checker
*/
/**
* Check value is Function
*/
function isFunction(value) {
	return typeof value === "function";
}
//#endregion
//#region node_modules/jodit/esm/core/decorators/autobind/autobind.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* [[include:core/decorators/autobind/README.md]]
* @packageDocumentation
* @module decorators/autobind
*/
/**
* Decorator that automatically binds a method to its class instance.
* This is useful when passing methods as callbacks to preserve the correct `this` context.
*
* @example
* ```typescript
* class MyComponent {
*   @autobind
*   handleClick() {
*     console.log(this); // Always refers to MyComponent instance
*   }
* }
*
* const component = new MyComponent();
* const button = document.createElement('button');
* button.addEventListener('click', component.handleClick); // `this` is correctly bound
* ```
*/
function autobind(_target, propertyKey, descriptor) {
	if (!isFunction(descriptor.value)) throw new TypeError(`@autobind can only be applied to methods, but "${propertyKey}" is not a function`);
	const originalMethod = descriptor.value;
	return {
		configurable: true,
		get() {
			const boundMethod = originalMethod.bind(this);
			Object.defineProperty(this, propertyKey, {
				value: boundMethod,
				configurable: true,
				writable: true
			});
			return boundMethod;
		}
	};
}
//#endregion
//#region node_modules/jodit/esm/core/component/statuses.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module component
*/
var STATUSES = {
	beforeInit: "beforeInit",
	ready: "ready",
	beforeDestruct: "beforeDestruct",
	destructed: "destructed"
};
//#endregion
//#region node_modules/jodit/esm/core/helpers/checker/is-native-function.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/checker
*/
/**
* Check if function or method was not replaced on some custom implementation
*/
function isNativeFunction(f) {
	return Boolean(f) && (typeof f).toLowerCase() === "function" && (f === Function.prototype || /^\s*function\s*(\b[a-z$_][a-z0-9$_]*\b)*\s*\((|([a-z$_][a-z0-9$_]*)(\s*,[a-z$_][a-z0-9$_]*)*)\)\s*{\s*\[native code]\s*}\s*$/i.test(String(f)));
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/checker/is-array.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/checker
*/
/**
* Check if element is array
*/
function isArray(elm) {
	return Array.isArray(elm);
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/checker/is-string.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/checker
*/
/**
* Check value is String
*/
function isString(value) {
	return typeof value === "string";
}
/**
* Check value is Array of String
*/
function isStringArray(value) {
	return isArray(value) && isString(value[0]);
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/checker/is-void.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/checker
*/
/**
* Check value is undefined or null
*/
function isVoid(value) {
	return value === void 0 || value === null;
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/utils/get.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Safe access in tree object
*
* @example
* ```js
* const obj = {
*   a: {
*     b: {
*       c: {
*         e: false
*       }
*     }
*   }
* };
*
* console.log(Jodit.modules.Helpers.get('a.b.c.d.e', obj) === false); // true
* console.log(Jodit.modules.Helpers.get('a.b.a.d.e', obj) === null); // false
* ```
*/
function get$1(chain, obj) {
	if (!isString(chain) || !chain.length) return null;
	const parts = chain.split(".");
	let result = obj;
	try {
		for (const part of parts) {
			if (isVoid(result[part])) return null;
			result = result[part];
		}
	} catch (_a) {
		return null;
	}
	if (isVoid(result)) return null;
	return result;
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/utils/reset.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var map$1 = {};
/**
* Reset Vanilla JS native function
* @example
* ```js
* reset('Array.from')(Set([1,2,3])) // [1, 2, 3]
* ```
* You must use the function derived from the method immediately as its iframe is being removed
*/
function reset(key) {
	var _a, _b;
	if (!(key in map$1)) {
		const iframe = globalDocument.createElement("iframe");
		try {
			iframe.src = "about:blank";
			globalDocument.body.appendChild(iframe);
			if (!iframe.contentWindow) return null;
			const func = get$1(key, iframe.contentWindow), bind = get$1(key.split(".").slice(0, -1).join("."), iframe.contentWindow);
			if (isFunction(func)) map$1[key] = func.bind(bind);
		} catch (e) {} finally {
			(_a = iframe.parentNode) === null || _a === void 0 || _a.removeChild(iframe);
		}
	}
	return (_b = map$1[key]) !== null && _b !== void 0 ? _b : null;
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/array/to-array.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/array
*/
/**
* Always return Array. It's a safe polyfill for [Array.from](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Array/from) method
* In certain scenarios (such as with Joomla Mootools), Array.from may be substituted with a less optimal implementation
* ```javascript
* Jodit.modules.Helpers.toArray('123') // ['1', '2', '3']
* Jodit.modules.Helpers.toArray(['test']) // ['test']
* Jodit.modules.Helpers.toArray(1) // []
* ```
*/
var toArray = function toArray(...args) {
	var _a;
	return (isNativeFunction(Array.from) ? Array.from : (_a = reset("Array.from")) !== null && _a !== void 0 ? _a : Array.from).apply(Array, args);
};
//#endregion
//#region node_modules/jodit/esm/core/helpers/checker/is-html.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/checker
*/
/**
* Check if a string is html or not
*/
var isHTML = (str) => isString(str) && /<([A-Za-z][A-Za-z0-9]*)\b[^>]*>(.*?)<\/\1>/m.test(str.replace(/[\r\n]/g, ""));
//#endregion
//#region node_modules/jodit/esm/core/helpers/checker/is-set.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/checker
*/
/**
* Check if element is set
*/
function isSet(elm) {
	return Boolean(elm) && isFunction(elm.has) && isFunction(elm.add) && isFunction(elm.delete);
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/string/trim.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/string
*/
/**
* It clears the line of all auxiliary invisible characters , from the spaces and line breaks , tabs
* from the beginning and end of the line
*/
function trim(value) {
	return value.replace(SPACE_REG_EXP_END(), "").replace(SPACE_REG_EXP_START(), "");
}
function trimChars(value, chars) {
	return value.replace(RegExp(`[${chars}]+$`), "").replace(RegExp(`^[${chars}]+`), "");
}
/**
* Trim only invisible chars
*/
function trimInv(value) {
	return value.replace(INVISIBLE_SPACE_REG_EXP_END(), "").replace(INVISIBLE_SPACE_REG_EXP_START(), "");
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/utils/assert.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/utils
*/
var AssertionError = class extends Error {
	constructor(message) {
		super(message);
		this.name = "AssertionError";
	}
};
/** Asserts that condition is truthy (or evaluates to true). */
function assert(condition, message) {
	if (!condition) throw new AssertionError(`Assertion failed: ${message}`);
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/checker/is-window.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/checker
*/
function isWindow(obj) {
	return obj != null && obj === obj.window;
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/checker/is-plain-object.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Check if element is simple plaint object
*/
function isPlainObject(obj) {
	if (!obj || typeof obj !== "object" || obj.nodeType || isWindow(obj)) return false;
	return !(obj.constructor && !{}.hasOwnProperty.call(obj.constructor.prototype, "isPrototypeOf"));
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/string/kebab-case.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/string
*/
var kebabCase = (key) => {
	return key.replace(/([A-Z])([A-Z])([a-z])/g, "$1-$2$3").replace(/([a-z])([A-Z])/g, "$1-$2").replace(/[\s_]+/g, "-").toLowerCase();
};
var CamelCaseToKebabCase = (key) => {
	return key.replace(/([A-Z])([A-Z])([a-z])/g, "$1-$2$3").replace(/([a-z])([A-Z])/g, "$1-$2").toLowerCase();
};
//#endregion
//#region node_modules/jodit/esm/core/helpers/checker/is-boolean.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/checker
*/
function isBoolean(elm) {
	return typeof elm === "boolean";
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/checker/is-numeric.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/checker
*/
/**
* Check value has numeric format
*/
function isNumeric(value) {
	if (isString(value)) {
		if (!value.match(/^([+-])?[0-9]+(\.?)([0-9]+)?(e[0-9]+)?$/)) return false;
		value = parseFloat(value);
	}
	return typeof value === "number" && !isNaN(value) && isFinite(value);
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/color/color-to-hex.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/color
*/
/**
* Converts rgba text representation of color in hex
*
* @param color - string like rgba(red, green, blue, alpha) or rgb(red, green, blue)
* @returns hex color view, NaN - for transparent color
* @example
* ```javascript
* var p = document.createElement('p');
* p.style.color = '#ffffff';
* console.log(p.getAttribute('style')); // color: rgb(255, 255, 255);
* console.log(colorTohex(p.style.color)); // #ffffff
* ```
*/
var colorToHex = (color) => {
	if (color === "rgba(0, 0, 0, 0)" || color === "") return false;
	if (!color) return "#000000";
	if (color.substr(0, 1) === "#") return color;
	const digits = /([\s\n\t\r]*?)rgb\((\d+), (\d+), (\d+)\)/.exec(color) || /([\s\n\t\r]*?)rgba\((\d+), (\d+), (\d+), ([\d.]+)\)/.exec(color);
	if (!digits) return "#000000";
	const red = parseInt(digits[2], 10), green = parseInt(digits[3], 10);
	let hex = (parseInt(digits[4], 10) | green << 8 | red << 16).toString(16).toUpperCase();
	while (hex.length < 6) hex = "0" + hex;
	return digits[1] + "#" + hex;
};
//#endregion
//#region node_modules/jodit/esm/core/helpers/normalize/normalize-css-value.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/normalize
*/
var NUMBER_FIELDS_REG = /^(left|top|bottom|right|width|min|max|height|margin|padding|fontsize|font-size)/i;
function normalizeCssNumericValue(key, value) {
	if (!isVoid(value) && NUMBER_FIELDS_REG.test(key) && isNumeric(value.toString())) return parseInt(value.toString(), 10) + "px";
	return value;
}
function normalizeCssValue(key, value) {
	switch (kebabCase(key)) {
		case "font-weight":
			switch (value.toString().toLowerCase()) {
				case "700":
				case "bold": return 700;
				case "400":
				case "normal": return 400;
				case "900":
				case "heavy": return 900;
			}
			return isNumeric(value) ? Number(value) : value;
	}
	if (/color/i.test(key) && /^rgb/i.test(value.toString())) return colorToHex(value.toString()) || value;
	return value;
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/string/camel-case.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/string
*/
/**
* Convert (kebab-case or snake_case) to camelCase
*/
var camelCase = (key) => {
	return key.replace(/([-_])(.)/g, (m, code, letter) => {
		return letter.toUpperCase();
	});
};
//#endregion
//#region node_modules/jodit/esm/core/helpers/utils/css.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Get the value of a computed style property for the first element in the set of matched elements or set one or
* more CSS properties for every matched element
*
* @param element - HTML element
* @param key - An object of property-value pairs to set. A CSS property name.
* @param value - A value to set for the property.
* @param onlyStyleMode - Get value from style attribute, without calculating
*/
function css(element, key, value, onlyStyleMode = false) {
	if (isBoolean(value)) {
		onlyStyleMode = value;
		value = void 0;
	}
	if (isPlainObject(key) || value !== void 0) {
		const setValue = (elm, _key, _value) => {
			_value = normalizeCssNumericValue(_key, _value);
			if (_value !== void 0 && (_value == null || css(elm, _key, true) !== normalizeCssValue(_key, _value))) elm.style[_key] = _value;
		};
		if (isPlainObject(key)) {
			const keys = Object.keys(key);
			for (let j = 0; j < keys.length; j += 1) setValue(element, camelCase(keys[j]), key[keys[j]]);
		} else setValue(element, camelCase(key), value);
		return "";
	}
	const key2 = kebabCase(key);
	const doc = element.ownerDocument || document;
	const win = doc ? doc.defaultView || doc.parentWindow : false;
	const currentValue = element.style[key];
	let result = "";
	if (currentValue !== void 0 && currentValue !== "") result = currentValue;
	else if (win && !onlyStyleMode) result = win.getComputedStyle(element).getPropertyValue(key2);
	if (NUMBER_FIELDS_REG.test(key) && /^[-+]?[0-9.]+px$/.test(result.toString())) result = parseInt(result.toString(), 10);
	return normalizeCssValue(key, result);
}
/**
* Clear center align
*/
/**
* Read the exact inline value of a CSS property (the `style` attribute).
* Unlike `css()` it never touches computed styles (no layout access) and does
* not normalize the result, so it is safe for exact string comparisons and
* save/restore. Returns an empty string when the property is not set inline.
*
* @example
* ```js
* cssInline(elm, 'zIndex'); // '' or '100'
* cssInline(elm, 'width'); // '0px' (css(elm, 'width', true) would return 0)
* ```
*/
function cssInline(element, key) {
	return element.style.getPropertyValue(kebabCase(key));
}
var clearCenterAlign = (image) => {
	if (css(image, "display") === "block") css(image, "display", "");
	const { style } = image;
	if (style.marginLeft === "auto" && style.marginRight === "auto") {
		style.marginLeft = "";
		style.marginRight = "";
	}
};
//#endregion
//#region node_modules/jodit/esm/core/helpers/utils/attr.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Alias for `elm.getAttribute` but if set second argument `-{key}`
* it will also check `data-{key}` attribute
* if set `value` it is alias for setAttribute with the same logic
*/
function attr(elm, keyOrAttributes, value) {
	if (!elm || !isFunction(elm.getAttribute)) return null;
	if (!isString(keyOrAttributes)) {
		Object.keys(keyOrAttributes).forEach((key) => {
			const value = keyOrAttributes[key];
			if (isPlainObject(value) && key === "style") css(elm, value);
			else {
				if (key === "className") key = "class";
				attr(elm, key, value);
			}
		});
		return null;
	}
	let key = CamelCaseToKebabCase(keyOrAttributes);
	if (/^-/.test(key)) {
		const res = attr(elm, `data${key}`);
		if (res) return res;
		key = key.substr(1);
	}
	if (value !== void 0) if (value == null) elm.hasAttribute(key) && elm.removeAttribute(key);
	else {
		let replaceValue = value.toString();
		if (elm.nodeName === "IMG" && (key === "width" || key === "height")) replaceValue = replaceValue.replace("px", "");
		elm.setAttribute(key, replaceValue);
		return replaceValue;
	}
	return elm.getAttribute(key);
}
/**
* Exact-name attribute access: no camelCase → kebab-case conversion, no
* `data-` fallback and no `px` stripping. Use it where the attribute name must
* be taken verbatim — sanitizers and attribute comparison (`onLoad`, `viewBox`,
* `xlink:href`). `null` removes the attribute.
*/
function attrRaw(elm, name, value) {
	if (value === void 0) return elm.getAttribute(name);
	if (value == null) {
		if (elm.hasAttribute(name)) elm.removeAttribute(name);
		return;
	}
	elm.setAttribute(name, value);
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/checker/is-view-object.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Check if an element is instance of View
*/
function isViewObject(jodit) {
	return Boolean(jodit && jodit instanceof Object && isFunction(jodit.constructor) && jodit.isView);
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/utils/data-bind.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var store = /* @__PURE__ */ new WeakMap();
var dataBind = (elm, key, value) => {
	let itemStore = store.get(elm);
	if (!itemStore) {
		itemStore = {};
		store.set(elm, itemStore);
		let e = null;
		if (isViewObject(elm.j)) e = elm.j.e;
		if (isViewObject(elm)) e = elm.e;
		e && e.on("beforeDestruct", () => {
			store.delete(elm);
		});
	}
	if (value === void 0) return itemStore[key];
	itemStore[key] = value;
	return value;
};
//#endregion
//#region node_modules/jodit/esm/core/helpers/utils/error/errors/abort-error.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
function abort(message = "Aborted") {
	return new DOMException(message, "AbortError");
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/utils/error/errors/connection-error.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/utils
*/
var ConnectionError = class ConnectionError extends Error {
	constructor(m) {
		super(m);
		Object.setPrototypeOf(this, ConnectionError.prototype);
	}
};
//#endregion
//#region node_modules/jodit/esm/core/helpers/utils/error/errors/options-error.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/utils
*/
var OptionsError = class OptionsError extends TypeError {
	constructor(m) {
		super(m);
		Object.setPrototypeOf(this, OptionsError.prototype);
	}
};
//#endregion
//#region node_modules/jodit/esm/core/helpers/utils/error/error.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/utils
*/
/**
* Helper for create Error object
*/
function error(message) {
	return new TypeError(message);
}
function connection(message) {
	return new ConnectionError(message);
}
function options(message) {
	return new OptionsError(message);
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/checker/is-promise.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/checker
*/
function isPromise(val) {
	return val && typeof val.then === "function";
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/utils/utils.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Call function with parameters
*
* @example
* ```js
* const f = Math.random();
* Jodit.modules.Helpers.call(f > 0.5 ? Math.ceil : Math.floor, f);
* ```
*/
function call(func, ...args) {
	return func(...args);
}
/**
* Call function with parameters
*
* @example
* ```js
* const f = Math.random();
* Jodit.modules.Helpers.call(f > 0.5 ? Math.ceil : Math.floor, f);
* ```
*/
function callThis(func, thisArg, ...args) {
	return func.apply(thisArg, args);
}
/**
* Mark element for debugging
*/
function markOwner(jodit, elm) {
	attr(elm, "data-editor_id", jodit.id);
	!elm.component && Object.defineProperty(elm, "jodit", { value: jodit });
}
function callPromise(condition, callback) {
	if (isPromise(condition)) return condition.then((resp) => resp, () => null).finally(callback);
	return callback === null || callback === void 0 ? void 0 : callback();
}
/**
* Allow load image in promise
*/
var loadImage = (src, jodit) => jodit.async.promise((res, rej) => {
	const image = new Image(), onError = () => {
		jodit.e.off(image);
		rej === null || rej === void 0 || rej();
	}, onSuccess = () => {
		jodit.e.off(image);
		res(image);
	};
	jodit.e.one(image, "load", onSuccess).one(image, "error", onError).one(image, "abort", onError);
	image.src = src;
	if (image.complete) onSuccess();
});
var keys = (obj, own = true) => {
	if (own) return Object.keys(obj);
	const props = [];
	for (const key in obj) props.push(key);
	return props;
};
/**
* Memorize last user chose
*/
var memorizeExec = (editor, _, { control }, preProcessValue) => {
	var _a;
	const key = `button${control.command}`;
	let value = (_a = control.args && control.args[0]) !== null && _a !== void 0 ? _a : dataBind(editor, key);
	if (isVoid(value)) return false;
	dataBind(editor, key, value);
	if (preProcessValue) value = preProcessValue(value);
	editor.execCommand(control.command, false, value !== null && value !== void 0 ? value : void 0);
};
/**
* Get DataTransfer from different event types
*/
var getDataTransfer = (event) => {
	if (event.clipboardData) return event.clipboardData;
	try {
		return event.dataTransfer || new DataTransfer();
	} catch (_a) {
		return null;
	}
};
function getPropertyDescriptor(obj, prop) {
	let desc;
	do {
		desc = Object.getOwnPropertyDescriptor(obj, prop);
		obj = Object.getPrototypeOf(obj);
	} while (!desc && obj);
	return desc;
}
//#endregion
//#region node_modules/jodit/esm/core/dom/dom.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var TEMP_ELEMENT_REG_EXP = new RegExp(`<([a-z]+)[^>]+${TEMP_ATTR}[^>]*>(.+?)</\\1>`, "gi");
/**
* Module for working with DOM
*/
var Dom = class Dom {
	constructor() {
		throw new Error("Dom is static module");
	}
	/**
	* Remove all content from element
	*/
	static detach(node) {
		while (node && node.firstChild) node.removeChild(node.firstChild);
	}
	/**
	* Wrap all inline next siblings
	*/
	static wrapNextInline(current, tag, editor) {
		let tmp;
		const first = current;
		let last = current;
		editor.s.save();
		let needFindNext = false;
		do {
			needFindNext = false;
			tmp = last.nextSibling;
			if (tmp && !Dom.isBlock(tmp) && !Dom.isTag(tmp, "br")) {
				needFindNext = true;
				last = tmp;
			}
		} while (needFindNext);
		return Dom.__wrapElements(tag, editor, first, last);
	}
	static __wrapElements(tag, editor, first, last) {
		const wrapper = isString(tag) ? editor.createInside.element(tag) : tag;
		if (first.parentNode) first.parentNode.insertBefore(wrapper, first);
		let next = first;
		while (next) {
			next = first.nextSibling;
			wrapper.appendChild(first);
			if (first === last || !next) break;
			first = next;
		}
		editor.s.restore();
		return wrapper;
	}
	/**
	* Wrap all inline siblings
	*/
	static wrapInline(current, tag, editor) {
		let tmp;
		let first = current;
		let last = current;
		editor.s.save();
		let needFindNext = false;
		do {
			needFindNext = false;
			tmp = first.previousSibling;
			if (tmp && !Dom.isBlock(tmp)) {
				needFindNext = true;
				first = tmp;
			}
		} while (needFindNext);
		do {
			needFindNext = false;
			tmp = last.nextSibling;
			if (tmp && !Dom.isBlock(tmp)) {
				needFindNext = true;
				last = tmp;
			}
		} while (needFindNext);
		return Dom.__wrapElements(tag, editor, first, last);
	}
	/**
	* Wrap node inside another node
	*/
	static wrap(current, tag, create) {
		const wrapper = isString(tag) ? create.element(tag) : tag;
		if (Dom.isNode(current)) {
			if (!current.parentNode) throw error("Element should be in DOM");
			current.parentNode.insertBefore(wrapper, current);
			wrapper.appendChild(current);
		} else {
			const fragment = current.extractContents();
			current.insertNode(wrapper);
			wrapper.appendChild(fragment);
		}
		return wrapper;
	}
	/**
	* Remove parent of node and insert this node instead that parent
	*/
	static unwrap(node) {
		const parent = node.parentNode;
		if (parent) {
			while (node.firstChild) parent.insertBefore(node.firstChild, node);
			Dom.safeRemove(node);
		}
	}
	/**
	* Call callback for all nodes between `start` and `end` in document order
	* (`start` and `end` are not included). Iteration stops when the callback returns `true`.
	*
	* `end` must be positioned after `start` in the document (e.g. selection markers),
	* otherwise iteration will stop only at the end of the tree.
	*/
	static between(start, end, callback) {
		let next = start;
		while (next && next !== end) {
			if (start !== next && callback(next)) break;
			let step = next.firstChild || next.nextSibling;
			if (!step) {
				while (next && !next.nextSibling) {
					next = next.parentNode;
					if (next === end) return;
				}
				step = next === null || next === void 0 ? void 0 : next.nextSibling;
			}
			next = step;
		}
	}
	static replace(elm, newTagName, create, withAttributes = false, notMoveContent = false) {
		if (isHTML(newTagName)) {
			assert(create, "Need create instance for new tag");
			newTagName = create.fromHTML(newTagName);
		}
		let tag;
		if (isString(newTagName)) {
			assert(create, "Need create instance for new tag");
			tag = create.element(newTagName);
		} else tag = newTagName;
		if (!notMoveContent) while (elm.firstChild) tag.appendChild(elm.firstChild);
		if (withAttributes && Dom.isElement(elm) && Dom.isElement(tag)) toArray(elm.attributes).forEach((attr) => {
			tag.setAttribute(attr.name, attr.value);
		});
		if (elm.parentNode) elm.parentNode.replaceChild(tag, elm);
		return tag;
	}
	/**
	* Checks whether the Node text and blank (in this case it may contain invisible auxiliary characters ,
	* it is also empty )
	*
	* @param node - The element of wood to be checked
	*/
	static isEmptyTextNode(node) {
		return Dom.isText(node) && (!node.nodeValue || node.nodeValue.replace(INVISIBLE_SPACE_REG_EXP(), "").trim().length === 0);
	}
	static isEmptyContent(node) {
		return Dom.each(node, (elm) => Dom.isEmptyTextNode(elm));
	}
	/**
	* The node is editable
	*/
	static isContentEditable(node, root) {
		return Dom.isNode(node) && !Dom.closest(node, (elm) => Dom.isElement(elm) && elm.getAttribute("contenteditable") === "false", root);
	}
	static isEmpty(node, condNoEmptyElement = NO_EMPTY_TAGS) {
		if (!node) return true;
		let cond;
		if (!isFunction(condNoEmptyElement)) cond = (elm) => condNoEmptyElement.has(elm.nodeName.toLowerCase());
		else cond = condNoEmptyElement;
		const emptyText = (node) => node.nodeValue == null || trim(node.nodeValue).length === 0;
		if (Dom.isText(node)) return emptyText(node);
		return !(Dom.isElement(node) && cond(node)) && Dom.each(node, (elm) => {
			if (Dom.isText(elm) && !emptyText(elm) || Dom.isElement(elm) && cond(elm)) return false;
		});
	}
	/**
	* Returns true if it is a DOM node
	*/
	static isNode(object) {
		return Boolean(object && isString(object.nodeName) && typeof object.nodeType === "number" && object.childNodes && isFunction(object.appendChild));
	}
	/**
	*  Check if element is table cell
	*/
	static isCell(elm) {
		return Dom.isNode(elm) && (elm.nodeName === "TD" || elm.nodeName === "TH");
	}
	/**
	* Check if element is a list element UL or OL
	*/
	static isList(elm) {
		return Dom.isTag(elm, LIST_TAGS);
	}
	/**
	* Check if element is a part of list	element LI
	*/
	static isLeaf(elm) {
		return Dom.isTag(elm, "li");
	}
	/**
	* Check is element is Image element
	*/
	static isImage(elm) {
		return Dom.isNode(elm) && /^(img|svg|picture|canvas)$/i.test(elm.nodeName);
	}
	/**
	* Check the `node` is a block element
	* @param node - Object to check
	*/
	static isBlock(node) {
		return !isVoid(node) && typeof node === "object" && Dom.isNode(node) && IS_BLOCK.test(node.nodeName);
	}
	/**
	* Check if element is text node
	*/
	static isText(node) {
		return Boolean(node && node.nodeType === Node.TEXT_NODE);
	}
	/**
	* Check if element is comment node
	*/
	static isComment(node) {
		return Boolean(node && node.nodeType === Node.COMMENT_NODE);
	}
	/**
	* Check if element is element node
	*/
	static isElement(node) {
		if (!Dom.isNode(node)) return false;
		return node.nodeType === Node.ELEMENT_NODE;
	}
	/**
	* Check if element is document fragment
	*/
	static isFragment(node) {
		if (!Dom.isNode(node)) return false;
		return node.nodeType === Node.DOCUMENT_FRAGMENT_NODE;
	}
	/**
	* Check if element is HTMLElement node
	*/
	static isHTMLElement(node) {
		var _a;
		if (!Dom.isNode(node)) return false;
		const win = (_a = node.ownerDocument) === null || _a === void 0 ? void 0 : _a.defaultView;
		return node instanceof (win ? win.HTMLElement : HTMLElement);
	}
	/**
	* Check element is inline block
	*/
	static isInlineBlock(node) {
		return Dom.isElement(node) && !/^(BR|HR)$/i.test(node.tagName) && ["inline", "inline-block"].indexOf(css(node, "display").toString()) !== -1;
	}
	/**
	* It's block and it can be split
	*/
	static canSplitBlock(node) {
		return !isVoid(node) && Dom.isHTMLElement(node) && Dom.isBlock(node) && !/^(TD|TH|CAPTION|FORM)$/.test(node.nodeName) && node.style !== void 0 && !/^(fixed|absolute)/i.test(node.style.position);
	}
	/**
	* Get first matched node inside root
	*/
	static first(root, condition) {
		return Dom.__deepMost(root, condition, false);
	}
	/**
	* Get last matched node inside root
	*/
	static last(root, condition) {
		return Dom.__deepMost(root, condition, true);
	}
	/**
	* Depth-first search for the first matched node inside root:
	* in document order (`reverse = false`) or in reverse order
	*/
	static __deepMost(root, condition, reverse) {
		const child = reverse ? "lastChild" : "firstChild", sibling = reverse ? "previousSibling" : "nextSibling";
		let current = root === null || root === void 0 ? void 0 : root[child];
		if (!current) return null;
		do {
			if (condition(current)) return current;
			let next = current[child] || current[sibling];
			if (!next && current.parentNode !== root) {
				do
					current = current.parentNode;
				while (current && !current[sibling] && current.parentNode !== root);
				next = current === null || current === void 0 ? void 0 : current[sibling];
			}
			current = next;
		} while (current);
		return null;
	}
	/**
	* Find previous node
	*/
	static prev(node, condition, root, withChild = true) {
		return Dom.find(node, condition, root, false, withChild);
	}
	/**
	* Find next node what `condition(next) === true`
	*/
	static next(node, condition, root, withChild = true) {
		return Dom.find(node, condition, root, true, withChild);
	}
	static prevWithClass(node, className) {
		return Dom.__siblingWithClass(node, className, true);
	}
	static nextWithClass(node, className) {
		return Dom.__siblingWithClass(node, className, false);
	}
	static __siblingWithClass(node, className, left) {
		const condition = (elm) => Dom.isElement(elm) && elm.classList.contains(className);
		const parent = node.parentNode;
		return left ? Dom.prev(node, condition, parent) : Dom.next(node, condition, parent);
	}
	/**
	* Find next/prev node what `condition(next) === true`
	*/
	static find(node, condition, root, leftToRight = true, withChild = true) {
		const gen = this.nextGen(node, root, leftToRight, withChild);
		let item = gen.next();
		while (!item.done) {
			if (condition(item.value)) return item.value;
			item = gen.next();
		}
		return null;
	}
	/**
	* Lazily iterate over all nodes what follow after `start` (in document order
	* for `leftToRight = true`, in reverse order otherwise) inside `root`.
	* Ancestors of `start` are not yielded.
	*/
	static *nextGen(start, root, leftToRight = true, withChild = true) {
		const stack = [];
		let currentNode = start;
		do {
			let next = leftToRight ? currentNode.nextSibling : currentNode.previousSibling;
			while (next) {
				stack.push(next);
				next = leftToRight ? next.nextSibling : next.previousSibling;
			}
			stack.reverse();
			yield* this.runInStack(start, stack, leftToRight, withChild);
			currentNode = currentNode.parentNode;
		} while (currentNode && currentNode !== root);
		return null;
	}
	/**
	* It goes through all the internal elements of the node, causing a callback function
	*
	* @param elm - the element whose children and descendants you want to iterate over
	* @param callback - It called for each item found
	* @example
	* ```javascript
	* Jodit.modules.Dom.each(editor.s.current(), function (node) {
	*  if (node.nodeType === Node.TEXT_NODE) {
	*      node.nodeValue = node.nodeValue.replace(Jodit.INVISIBLE_SPACE_REG_EXP(), '') // remove all of the text element codes invisible character
	*  }
	* });
	* ```
	*/
	static each(elm, callback, leftToRight = true) {
		const gen = this.eachGen(elm, leftToRight);
		let item = gen.next();
		while (!item.done) {
			if (callback(item.value) === false) return false;
			item = gen.next();
		}
		return true;
	}
	static eachGen(root, leftToRight = true) {
		return this.runInStack(root, [root], leftToRight);
	}
	static *runInStack(start, stack, leftToRight, withChild = true) {
		while (stack.length) {
			const item = stack.pop();
			if (withChild) {
				let child = leftToRight ? item.lastChild : item.firstChild;
				while (child) {
					stack.push(child);
					child = leftToRight ? child.previousSibling : child.nextSibling;
				}
			}
			if (start !== item) yield item;
		}
	}
	/**
	* Find next/prev node what `condition(next) === true`
	*/
	static findWithCurrent(node, condition, root, sibling = "nextSibling", child = "firstChild") {
		let next = node;
		do {
			if (condition(next)) return next || null;
			if (child && next && next[child]) {
				const nextOne = Dom.findWithCurrent(next[child], condition, next, sibling, child);
				if (nextOne) return nextOne;
			}
			while (next && !next[sibling] && next !== root) next = next.parentNode;
			if (next && next[sibling] && next !== root) next = next[sibling];
		} while (next && next !== root);
		return null;
	}
	/**
	* Get not empty sibling
	*/
	static findSibling(node, left = true, cond = (n) => !Dom.isEmptyTextNode(n)) {
		let sibling = Dom.sibling(node, left);
		while (sibling && !cond(sibling)) sibling = Dom.sibling(sibling, left);
		return sibling && cond(sibling) ? sibling : null;
	}
	/**
	* Returns the nearest non-empty sibling
	*/
	static findNotEmptySibling(node, left) {
		return Dom.findSibling(node, left, (n) => {
			var _a;
			return !Dom.isEmptyTextNode(n) && Boolean(!Dom.isText(n) || ((_a = n.nodeValue) === null || _a === void 0 ? void 0 : _a.length) && trim(n.nodeValue));
		});
	}
	/**
	* Returns the nearest non-empty neighbor
	*/
	static findNotEmptyNeighbor(node, left, root) {
		return call(left ? Dom.prev : Dom.next, node, (n) => Boolean(n && (!(Dom.isText(n) || Dom.isComment(n)) || trim((n === null || n === void 0 ? void 0 : n.nodeValue) || "").length)), root);
	}
	static sibling(node, left) {
		return left ? node.previousSibling : node.nextSibling;
	}
	/**
	* It goes through all the elements in ascending order, and checks to see if they meet the predetermined condition
	*
	* The condition is checked for the `node` itself too. The `root` reached
	* while ascending is checked only when `checkRoot = true`
	* (but if `node === root` it is checked in any case).
	*/
	static up(node, condition, root, checkRoot = false) {
		let start = node;
		if (!start) return null;
		do {
			if (condition(start)) return start;
			if (start === root || !start.parentNode) break;
			start = start.parentNode;
		} while (start && start !== root);
		if (start === root && checkRoot && condition(start)) return start;
		return null;
	}
	static closest(node, tagsOrCondition, root) {
		let condition;
		const lc = (s) => s.toLowerCase();
		if (isFunction(tagsOrCondition)) condition = tagsOrCondition;
		else if (isArray(tagsOrCondition) || isSet(tagsOrCondition)) {
			const set = isSet(tagsOrCondition) ? tagsOrCondition : new Set(tagsOrCondition.map(lc));
			condition = (tag) => Boolean(tag && set.has(lc(tag.nodeName)));
		} else condition = (tag) => Boolean(tag && lc(tagsOrCondition) === lc(tag.nodeName));
		return Dom.up(node, condition, root);
	}
	/**
	* Furthest parent node matching condition
	*/
	static furthest(node, condition, root) {
		let matchedParent = null, current = node === null || node === void 0 ? void 0 : node.parentElement;
		while (current && current !== root) {
			if (condition(current)) matchedParent = current;
			current = current === null || current === void 0 ? void 0 : current.parentElement;
		}
		return matchedParent;
	}
	/**
	* Append new element in the start of root
	*/
	static appendChildFirst(root, newElement) {
		if (root.firstChild !== newElement) Dom.prepend(root, newElement);
	}
	/**
	* Insert newElement after element
	*/
	static after(elm, newElement) {
		var _a;
		(_a = elm.parentNode) === null || _a === void 0 || _a.insertBefore(newElement, elm.nextSibling);
	}
	/**
	* Insert newElement before element
	*/
	static before(elm, newElement) {
		var _a;
		(_a = elm.parentNode) === null || _a === void 0 || _a.insertBefore(newElement, elm);
	}
	/**
	* Insert newElement as first child inside element
	*/
	static prepend(root, newElement) {
		root.insertBefore(newElement, root.firstChild);
	}
	static append(root, newElement) {
		if (isArray(newElement)) newElement.forEach((node) => {
			this.append(root, node);
		});
		else root.appendChild(newElement);
	}
	static moveContent(from, to, inStart = false, filter = () => true) {
		const fragment = (from.ownerDocument || globalDocument).createDocumentFragment();
		toArray(from.childNodes).filter((elm) => {
			if (filter(elm)) return true;
			Dom.safeRemove(elm);
			return false;
		}).forEach((node) => {
			fragment.appendChild(node);
		});
		to.insertBefore(fragment, inStart ? to.firstChild : null);
	}
	/**
	* Check root contains child or equal child
	*/
	static isOrContains(root, child, onlyContains = false) {
		if (root === child) return !onlyContains;
		return Boolean(child && root && this.up(child, (nd) => nd === root, root, true));
	}
	/**
	* Safe remove element from DOM
	*/
	static safeRemove(...nodes) {
		nodes.forEach((node) => Dom.isNode(node) && node.parentNode && node.parentNode.removeChild(node));
	}
	/**
	* Insert a node into the range and collapse the range to the start of
	* the inserted content. Unlike the native `Range.insertNode` it does not
	* split inseparable elements (BR, HR, IMG etc.) and removes empty text
	* nodes produced by the split of a text container.
	*/
	static safeInsertNode(range, node) {
		range.collapsed || range.deleteContents();
		const child = Dom.isFragment(node) ? node.lastChild : node;
		if (range.startContainer === range.endContainer && range.collapsed && Dom.isTag(range.startContainer, INSEPARABLE_TAGS)) Dom.after(range.startContainer, node);
		else {
			range.insertNode(node);
			child && range.setStartBefore(child);
		}
		range.collapse(true);
		[node.nextSibling, node.previousSibling].forEach((n) => Dom.isText(n) && !n.nodeValue && Dom.safeRemove(n));
	}
	/**
	* Hide element
	*/
	static hide(node) {
		if (!node) return;
		dataBind(node, "__old_display", node.style.getPropertyValue("display"));
		node.style.setProperty("display", "none");
	}
	/**
	* Show element
	*/
	static show(node) {
		if (!node) return;
		const display = dataBind(node, "__old_display");
		if (node.style.getPropertyValue("display") === "none") node.style.setProperty("display", display || null);
	}
	static isTag(node, tagNames) {
		if (Array.isArray(tagNames)) throw new TypeError("Dom.isTag does not support array");
		if (!this.isElement(node)) return false;
		const nameL = node.tagName.toLowerCase();
		if (tagNames instanceof Set) return tagNames.has(nameL) || tagNames.has(node.tagName.toUpperCase());
		return nameL === tagNames || node.tagName.toUpperCase() === tagNames;
	}
	/**
	* Marks an item as temporary
	*/
	static markTemporary(element, attributes) {
		attributes && attr(element, attributes);
		attr(element, TEMP_ATTR, true);
		return element;
	}
	/**
	* Check if element is temporary
	*/
	static isTemporary(element) {
		if (!Dom.isElement(element)) return false;
		return Dom.isMarker(element) || attr(element, "data-jodit-temp") === "true";
	}
	/**
	* Define element is selection helper
	*/
	static isMarker(elm) {
		return Dom.isNode(elm) && Dom.isTag(elm, "span") && elm.hasAttribute("data-jodit-selection_marker");
	}
	/**
	* Unwrap temporary elements inside a HTML string (keeps their content)
	*/
	static replaceTemporaryFromString(value) {
		return value.replace(TEMP_ELEMENT_REG_EXP, "$2");
	}
	/**
	* Collect the temporary elements (`data-jodit-temp`) inside the root.
	* A plain tree walk on purpose: no selector engine, works on VNode.
	*/
	static temporaryList(root) {
		const result = [];
		Dom.each(root, (node) => {
			if (Dom.isHTMLElement(node) && node.hasAttribute("data-jodit-temp")) result.push(node);
		});
		return result;
	}
};
//#endregion
//#region node_modules/jodit/esm/core/helpers/checker/has-browser-color-picker.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/checker
*/
/**
* Check if browser has a color picker (a new HTML5 attribute for input tag)
*/
function hasBrowserColorPicker() {
	let supportsColor = true;
	try {
		const a = globalDocument.createElement("input");
		a.type = "color";
		a.value = "!";
		supportsColor = a.type === "color" && a.value !== "!";
	} catch (e) {
		supportsColor = false;
	}
	return supportsColor;
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/checker/is-abort-error.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
function isAbortError(error) {
	return Boolean(error) && error instanceof DOMException && error.name === "AbortError";
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/string/stringify.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Safe stringify circular object
*/
function stringify(value, options = {}) {
	if (typeof value !== "object") return String(value);
	const excludeKeys = new Set(options.excludeKeys);
	const map = /* @__PURE__ */ new WeakMap();
	const r = (k, v) => {
		if (excludeKeys.has(k)) return;
		if (typeof v === "object" && v != null) {
			if (map.get(v)) return "[refObject]";
			map.set(v, true);
		}
		return v;
	};
	return JSON.stringify(value, r, options.prettify);
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/checker/is-equal.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/checker
*/
/**
* Check two element are equal
*/
function isEqual(a, b) {
	return a === b || stringify(a) === stringify(b);
}
function isFastEqual(a, b) {
	return a === b;
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/checker/is-html-from-word.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/checker
*/
/**
* Detect if string is HTML from MS Word, Excel or LibreOffice/OpenOffice
*/
function isHtmlFromWord(data) {
	return data.search(/<meta.*?Microsoft Excel\s[\d].*?>/) !== -1 || data.search(/<meta.*?Microsoft Word\s[\d].*?>/) !== -1 || data.search(/<meta[^>]*?ProgId[^>]*?(Word|Excel)\./i) !== -1 || data.search(/<meta[^>]*?(LibreOffice|OpenOffice)/i) !== -1 || data.search(/urn:schemas-microsoft-com:office:(word|excel)/) !== -1 || data.search(/<\w[^>]*\sclass=("|')?Mso/) !== -1 || data.search(/style='[^']*mso-/) !== -1 || data.search(/style="[^"]*mso-/) !== -1 && data.search(/<font/) !== -1;
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/checker/is-imp-interface.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Check value has method init
*/
function isInitable(value) {
	return !isVoid(value) && isFunction(value.init);
}
/**
* Check value has method destruct
*/
function isDestructable(value) {
	return !isVoid(value) && isFunction(value.destruct);
}
/**
* Check value is instant that implements IContainer
*/
function hasContainer(value) {
	return !isVoid(value) && Dom.isElement(value.container);
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/checker/is-int.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/checker
*/
/**
* Check value is Int
*/
function isInt(value) {
	if (isString(value) && isNumeric(value)) value = parseFloat(value);
	return typeof value === "number" && Number.isFinite(value) && !(value % 1);
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/checker/is-jodit-object.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Check if element is instance of Jodit
*/
function isJoditObject(jodit) {
	return Boolean(jodit && jodit instanceof Object && isFunction(jodit.constructor) && (typeof Jodit !== "undefined" && jodit instanceof Jodit || jodit.isJodit));
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/checker/is-license.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/checker
*/
var isLicense = (license) => isString(license) && license.length === 23 && /^[a-z0-9]{5}-[a-z0-9]{5}-[a-z0-9]{5}-[a-z0-9]{5}$/i.test(license);
//#endregion
//#region node_modules/jodit/esm/core/helpers/checker/is-marker.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Define element is selection helper
* @deprecated use Dom.isMarker instead
*/
function isMarker(elm) {
	return Dom.isMarker(elm);
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/checker/is-number.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/checker
*/
/**
* Check value is a number
*/
function isNumber(value) {
	return typeof value === "number" && !isNaN(value) && isFinite(value);
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/checker/is-url.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/checker
*/
/**
* Check if a string is an url
*/
function isURL(str) {
	if (str.includes(" ")) return false;
	if (typeof URL !== "undefined") try {
		const url = new URL(str);
		return [
			"https:",
			"http:",
			"ftp:",
			"file:",
			"rtmp:"
		].includes(url.protocol);
	} catch (e) {
		return false;
	}
	const a = globalDocument.createElement("a");
	a.href = str;
	return Boolean(a.hostname);
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/checker/is-valid-name.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/checker
*/
/**
* Check if name has a normal format
*/
function isValidName(name) {
	if (!name.length) return false;
	return !/[^0-9A-Za-zа-яА-ЯЁё\w\-_. ]/.test(name) && name.trim().length > 0;
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/checker/index.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
//#endregion
//#region node_modules/jodit/esm/core/decorators/cache/cache.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Retrieves a cached property value from an object if it exists; otherwise, returns `null`.
*
* This utility is particularly useful when working with properties that are lazily initialized
* or dynamically created, such as getters or cached computations. It ensures you can safely
* access the value without triggering initialization or creating a new instance.
*
* ### Usage Example:
* ```typescript
* import type { IUIElement } from "jodit";
*
* const { component, cache, cached } = Jodit.decorators;
* const { UIElement } = Jodit.modules;
*
* @component
* class SomeComponent extends UIElement {
*   @cache
*   get someElement(): IUIElement {
*     return new UIElement(this.jodit);
*   }
*
*   destruct() {
*     // Use the cached utility to clean up only if the property is initialized
*     cached(this, 'someElement')?.destruct();
*     super.destruct();
*   }
* }
* ```
*
* @param object - The object containing the property to check.
* @param property - The name of the property to retrieve from the cache.
* @returns The cached value of the property if it exists; otherwise, `null`.
*
* ### Notes:
* - If the property is defined as a getter, the function will return `null`
*   instead of invoking the getter.
* - This function is non-destructive and does not alter the object's state.
*/
function cached(object, property) {
	const descriptor = Object.getOwnPropertyDescriptor(object, property);
	if (!descriptor || isFunction(descriptor.get)) return null;
	return descriptor.value;
}
/**
* A decorator that caches the result of a getter method. Once the getter is accessed for the first time,
* its computed value is stored as a property of the object. Subsequent accesses return the cached value
* without recalculating it, improving performance and avoiding redundant computations.
*
* ### Key Features:
* - **Lazy Initialization**: The original getter is invoked only once, the first time the property is accessed.
* - **Immutability**: After caching, the value is stored as a non-writable, non-configurable property, preventing accidental modifications.
* - **Conditional Caching**: If the returned value has a property `noCache` set to `true`, the caching mechanism is bypassed, and the getter is invoked each time.
*
* ### Usage Example 1: Basic Caching
* ```typescript
* import { cache } from './decorators';
*
* class Example {
*   private counter = 0;
*
*   @cache
*   get expensiveComputation(): number {
*     console.log('Calculating...');
*     return ++this.counter;
*   }
* }
*
* const instance = new Example();
* console.log(instance.expensiveComputation); // Logs "Calculating..." and returns 1
* console.log(instance.expensiveComputation); // Returns 1 (cached value, no calculation)
* ```
*
* ### Usage Example 2: Integration with Cached Utilities
* ```typescript
* import { cache, cached } from './decorators';
* import type { IUIElement } from "jodit";
*
* const { component } = Jodit.decorators;
* const { UIElement } = Jodit.modules;
*
* @component
* class SomeComponent extends UIElement {
*   @cache
*   get someElement(): IUIElement {
*     return new UIElement(this.jodit);
*   }
*
*   destruct() {
*     // Use the cached utility to clean up only if the property is initialized
*     cached(this, 'someElement')?.destruct();
*     super.destruct();
*   }
* }
* ```
*
* @param _ - The target object (not used directly).
* @param name - The name of the property to decorate.
* @param descriptor - The property descriptor, which must include a getter method.
* @throws Will throw an error if the descriptor does not include a getter.
*
* ### Notes:
* - **Performance**: Ideal for properties that are computationally expensive and do not change after the initial computation.
* - **Flexibility**: Supports conditional caching via the `noCache` property in the returned value.
* - **Compatibility**: Designed to work seamlessly with objects and classes in TypeScript or JavaScript.
*/
function cache(_, name, descriptor) {
	const getter = descriptor.get;
	if (!getter) throw error("Getter property descriptor expected");
	descriptor.get = function() {
		const value = getter.call(this);
		if (value && value.noCache === true) return value;
		Object.defineProperty(this, name, {
			configurable: descriptor.configurable,
			enumerable: descriptor.enumerable,
			writable: false,
			value
		});
		return value;
	};
}
function getOwnerDocument(component) {
	const od = component.od;
	return od !== null && od !== void 0 ? od : document;
}
function cacheHTML(target, _, descriptor) {
	const fn = descriptor.value;
	if (!isFunction(fn)) throw error("Handler must be a Function");
	let useCache = true;
	/**
	* The cache is keyed by the owner document first and only then by the
	* component class. A page can create editors inside different documents
	* (iframes) and destroy those documents later: a template cached from a
	* dead document would otherwise be cloned for every subsequent editor,
	* whose buttons then never react to clicks.
	*/
	const cached = /* @__PURE__ */ new WeakMap();
	descriptor.value = function(...attrs) {
		var _a;
		const doc = getOwnerDocument(this);
		const docCache = cached.get(doc);
		if (useCache && (docCache === null || docCache === void 0 ? void 0 : docCache.has(this.constructor))) return (_a = docCache.get(this.constructor)) === null || _a === void 0 ? void 0 : _a.cloneNode(true);
		const value = fn.apply(this, attrs);
		if (useCache && Dom.isElement(value)) if (docCache) docCache.set(this.constructor, value);
		else cached.set(doc, new WeakMap([[this.constructor, value]]));
		return useCache ? value.cloneNode(true) : value;
	};
	target.hookStatus(STATUSES.ready, (component) => {
		const view = isViewObject(component) ? component : component.jodit;
		useCache = Boolean(view.options.cache);
	});
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/utils/get-class-name.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var keepNames = /* @__PURE__ */ new Map();
var getClassName = (obj) => {
	var _a;
	if (isFunction(obj.className)) return obj.className();
	const constructor = ((_a = obj.constructor) === null || _a === void 0 ? void 0 : _a.originalConstructor) || obj.constructor;
	if (keepNames.has(constructor)) return keepNames.get(constructor);
	if (constructor.name) return constructor.name;
	const regex = /* @__PURE__ */ new RegExp(/^\s*function\s*(\S*)\s*\(/);
	const res = constructor.toString().match(regex);
	return res ? res[1] : "";
};
//#endregion
//#region node_modules/jodit/esm/core/decorators/component/component.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var componentRegistry = /* @__PURE__ */ new Map();
/**
* Decorate components and set status isReady after constructor
* @param constructorFunction - Component constructor class
*/
function component(constructorFunction) {
	class newConstructorFunction extends constructorFunction {
		constructor(...args) {
			super(...args);
			if (this.constructor === newConstructorFunction) {
				if (!(this instanceof newConstructorFunction)) Object.setPrototypeOf(this, newConstructorFunction.prototype);
				this.setStatus("ready");
			}
		}
	}
	const name = getClassName(constructorFunction.prototype);
	if (componentRegistry.has(name) && false);
	componentRegistry.set(name, newConstructorFunction);
	return newConstructorFunction;
}
function getComponentClass(name) {
	return componentRegistry.get(name);
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/async/set-timeout.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/async
*/
/**
* Create async callback if set timeout value - else call function immediately
*/
function setTimeout(callback, timeout, ...args) {
	if (!timeout) callback.call(null, ...args);
	else return globalWindow.setTimeout(callback, timeout, ...args);
	return 0;
}
/**
* Clear timeout
*/
function clearTimeout(timer) {
	globalWindow.clearTimeout(timer);
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/async/index.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
//#endregion
//#region node_modules/jodit/esm/core/async/async.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var Async = class {
	constructor() {
		var _a, _b, _c, _d, _e;
		this.timers = /* @__PURE__ */ new Map();
		this.__callbacks = /* @__PURE__ */ new Map();
		this.__queueMicrotaskNative = (_a = queueMicrotask === null || queueMicrotask === void 0 ? void 0 : queueMicrotask.bind(window)) !== null && _a !== void 0 ? _a : Promise.resolve().then.bind(Promise.resolve());
		this.promisesRejections = /* @__PURE__ */ new Set();
		this.__requestsIdle = /* @__PURE__ */ new Set();
		this.__controllers = /* @__PURE__ */ new Set();
		this.__requestsRaf = /* @__PURE__ */ new Set();
		this.__requestIdleCallbackNative = (_c = (_b = window["requestIdleCallback"]) === null || _b === void 0 ? void 0 : _b.bind(window)) !== null && _c !== void 0 ? _c : ((callback, options) => {
			var _a;
			const start = Date.now();
			return this.setTimeout(() => {
				callback({
					didTimeout: false,
					timeRemaining: () => Math.max(0, 50 - (Date.now() - start))
				});
			}, (_a = options === null || options === void 0 ? void 0 : options.timeout) !== null && _a !== void 0 ? _a : 1);
		});
		this.__cancelIdleCallbackNative = (_e = (_d = window["cancelIdleCallback"]) === null || _d === void 0 ? void 0 : _d.bind(window)) !== null && _e !== void 0 ? _e : ((request) => {
			this.clearTimeout(request);
		});
		this.isDestructed = false;
	}
	delay(timeout) {
		return this.promise((resolve) => this.setTimeout(resolve, timeout));
	}
	setTimeout(callback, timeout, ...args) {
		if (this.isDestructed) return 0;
		let options = {};
		if (isVoid(timeout)) timeout = 0;
		if (!isNumber(timeout)) {
			options = timeout;
			timeout = options.timeout || 0;
		}
		if (options.label) this.clearLabel(options.label);
		const timer = setTimeout(callback, timeout, ...args);
		const key = options.label || timer;
		this.timers.set(key, timer);
		this.__callbacks.set(key, callback);
		return timer;
	}
	updateTimeout(label, timeout) {
		assert(label && this.timers.has(label), "Label does not exist");
		if (!label || !this.timers.has(label)) return null;
		const callback = this.__callbacks.get(label);
		assert(isFunction(callback), "Callback is not a function");
		return this.setTimeout(callback, {
			label,
			timeout
		});
	}
	clearLabel(label) {
		if (label && this.timers.has(label)) {
			clearTimeout(this.timers.get(label));
			this.timers.delete(label);
			this.__callbacks.delete(label);
		}
	}
	clearTimeout(timerOrLabel) {
		if (isString(timerOrLabel)) return this.clearLabel(timerOrLabel);
		clearTimeout(timerOrLabel);
		this.timers.delete(timerOrLabel);
		this.__callbacks.delete(timerOrLabel);
	}
	/**
	* Debouncing enforces that a function not be called again until a certain amount of time has passed without
	* it being called. As in "execute this function only if 100 milliseconds have passed without it being called."
	*
	* @example
	* ```javascript
	* var jodit = Jodit.make('.editor');
	* jodit.e.on('mousemove', jodit.async.debounce(() => {
	* 	// Do expensive things
	* }, 100));
	* ```
	*/
	debounce(fn, timeout, firstCallImmediately = false) {
		let timer = 0, fired = false;
		const promises = [];
		const callFn = (...args) => {
			if (!fired) {
				timer = 0;
				const res = fn(...args);
				fired = true;
				if (promises.length) {
					const runPromises = () => {
						promises.forEach((res) => res());
						promises.length = 0;
					};
					isPromise(res) ? res.finally(runPromises) : runPromises();
				}
			}
		};
		const onFire = (...args) => {
			fired = false;
			if (!timeout) callFn(...args);
			else {
				if (!timer && firstCallImmediately) callFn(...args);
				clearTimeout(timer);
				timer = this.setTimeout(() => callFn(...args), isFunction(timeout) ? timeout() : timeout);
				this.timers.set(fn, timer);
			}
		};
		return isPlainObject(timeout) && timeout.promisify ? (...args) => {
			const promise = this.promise((res) => {
				promises.push(res);
			}).catch((e) => {
				if (isAbortError(e)) return null;
				throw e;
			});
			onFire(...args);
			return promise;
		} : onFire;
	}
	microDebounce(fn, firstCallImmediately = false) {
		let scheduled = false;
		let needCall = true;
		let savedArgs;
		return ((...args) => {
			savedArgs = args;
			if (scheduled) {
				needCall = true;
				return;
			}
			needCall = true;
			if (firstCallImmediately) {
				needCall = false;
				fn(...savedArgs);
			}
			scheduled = true;
			this.__queueMicrotaskNative(() => {
				scheduled = false;
				if (this.isDestructed) return;
				needCall && fn(...savedArgs);
			});
		});
	}
	/**
	* Throttling enforces a maximum number of times a function can be called over time.
	* As in "execute this function at most once every 100 milliseconds."
	*
	* @example
	* ```javascript
	* var jodit = Jodit.make('.editor');
	* jodit.e.on(document.body, 'scroll', jodit.async.throttle(function() {
	* 	// Do expensive things
	* }, 100));
	* ```
	*/
	throttle(fn, timeout, ignore = false) {
		let timer = null, needInvoke, callee, lastArgs;
		return (...args) => {
			needInvoke = true;
			lastArgs = args;
			if (!timeout) {
				fn(...lastArgs);
				return;
			}
			if (!timer) {
				callee = () => {
					if (needInvoke) {
						fn(...lastArgs);
						needInvoke = false;
						timer = this.setTimeout(callee, isFunction(timeout) ? timeout() : timeout);
						this.timers.set(callee, timer);
					} else timer = null;
				};
				callee();
			}
		};
	}
	promise(executor) {
		let rejectCallback = () => {};
		const promise = new Promise((resolve, reject) => {
			rejectCallback = () => reject(abort("Abort async"));
			this.promisesRejections.add(rejectCallback);
			executor(resolve, reject);
		});
		if (!promise.finally && typeof process !== "undefined" && false);
		promise.finally(() => {
			this.promisesRejections.delete(rejectCallback);
		}).catch(() => null);
		promise.rejectCallback = rejectCallback;
		return promise;
	}
	/**
	* Get Promise status
	*/
	promiseState(p) {
		if (p.status) return p.status;
		if (!Promise.race) return new Promise((resolve) => {
			p.then((v) => {
				resolve("fulfilled");
				return v;
			}, (e) => {
				resolve("rejected");
				throw e;
			});
			this.setTimeout(() => {
				resolve("pending");
			}, 100);
		});
		const t = {};
		return Promise.race([p, t]).then((v) => v === t ? "pending" : "fulfilled", () => "rejected");
	}
	requestIdleCallback(callback, options = { timeout: 100 }) {
		const request = this.__requestIdleCallbackNative(callback, options);
		this.__requestsIdle.add(request);
		return request;
	}
	requestIdlePromise(options) {
		return this.promise((res) => {
			const request = this.requestIdleCallback(() => res(request), options);
		});
	}
	/**
	* Try to use scheduler.postTask if it is available https://wicg.github.io/scheduling-apis/
	*/
	schedulerPostTask(task, options = {
		delay: 0,
		priority: "user-visible"
	}) {
		const controller = new AbortController();
		if (options.signal) options.signal.addEventListener("abort", () => controller.abort());
		this.__controllers.add(controller);
		if (typeof globalThis.scheduler !== "undefined") {
			const promise = globalThis.scheduler.postTask(task, {
				...options,
				signal: controller.signal
			});
			promise.finally(() => {
				this.__controllers.delete(controller);
			}).catch(() => null);
			return promise;
		}
		return this.promise((resolve, reject) => {
			const timeout = this.setTimeout(() => {
				try {
					resolve(task());
				} catch (e) {
					reject(e);
				}
				this.__controllers.delete(controller);
			}, options.delay || 1);
			controller.signal.addEventListener("abort", () => {
				this.clearTimeout(timeout);
				this.__controllers.delete(controller);
				reject(abort());
			});
		});
	}
	schedulerYield() {
		return this.schedulerPostTask(() => {}, { priority: "user-visible" });
	}
	cancelIdleCallback(request) {
		this.__requestsIdle.delete(request);
		return this.__cancelIdleCallbackNative(request);
	}
	requestAnimationFrame(callback) {
		const request = requestAnimationFrame(callback);
		this.__requestsRaf.add(request);
		return request;
	}
	cancelAnimationFrame(request) {
		this.__requestsRaf.delete(request);
		cancelAnimationFrame(request);
	}
	clear() {
		this.__requestsIdle.forEach((key) => this.cancelIdleCallback(key));
		this.__requestsRaf.forEach((key) => this.cancelAnimationFrame(key));
		this.__controllers.forEach((controller) => controller.abort());
		this.timers.forEach((key) => clearTimeout(this.timers.get(key)));
		this.timers.clear();
		this.promisesRejections.forEach((reject) => reject());
		this.promisesRejections.clear();
	}
	destruct() {
		this.clear();
		this.isDestructed = true;
	}
};
//#endregion
//#region node_modules/jodit/esm/core/async/index.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
//#endregion
//#region node_modules/jodit/esm/core/helpers/array/split-array.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Return an array from string or array
* ```javascript
* Jodit.modules.Helpers.splitArray('1,2,3') // ['1', '2', '3']
* Jodit.modules.Helpers.splitArray(['1', '2', '3']) // ['1', '2', '3']
* ```
*/
function splitArray(a) {
	return Array.isArray(a) ? a : a.split(/[,\s]+/);
}
//#endregion
//#region node_modules/jodit/esm/core/event-emitter/store.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var defaultNameSpace = "JoditEventDefaultNamespace";
var EventHandlersStore = class {
	constructor() {
		this.__store = /* @__PURE__ */ new Map();
	}
	get(event, namespace) {
		if (this.__store.has(namespace)) {
			const ns = this.__store.get(namespace);
			assert(ns, "-");
			return ns[event];
		}
	}
	indexOf(event, namespace, originalCallback) {
		const blocks = this.get(event, namespace);
		if (blocks) {
			for (let i = 0; i < blocks.length; i += 1) if (blocks[i].originalCallback === originalCallback) return i;
		}
		return false;
	}
	namespaces(withoutDefault = false) {
		const nss = toArray(this.__store.keys());
		return withoutDefault ? nss.filter((ns) => ns !== defaultNameSpace) : nss;
	}
	events(namespace) {
		const ns = this.__store.get(namespace);
		return ns ? Object.keys(ns) : [];
	}
	set(event, namespace, data, onTop = false) {
		let ns = this.__store.get(namespace);
		if (!ns) {
			ns = {};
			this.__store.set(namespace, ns);
		}
		if (ns[event] === void 0) ns[event] = [];
		if (!onTop) ns[event].push(data);
		else ns[event].unshift(data);
	}
	clear() {
		this.__store.clear();
	}
	clearEvents(namespace, event) {
		const ns = this.__store.get(namespace);
		if (ns && ns[event]) {
			delete ns[event];
			if (!Object.keys(ns).length) this.__store.delete(namespace);
		}
	}
	isEmpty() {
		return this.__store.size === 0;
	}
};
//#endregion
//#region node_modules/jodit/esm/core/event-emitter/event-emitter.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* The module editor's event manager
*/
var EventEmitter = class {
	mute(event) {
		this.__mutedEvents.add(event !== null && event !== void 0 ? event : "*");
		return this;
	}
	isMuted(event) {
		if (event && this.__mutedEvents.has(event)) return true;
		return this.__mutedEvents.has("*");
	}
	unmute(event) {
		this.__mutedEvents.delete(event !== null && event !== void 0 ? event : "*");
		return this;
	}
	__eachEvent(events, callback) {
		splitArray(events).map((e) => e.trim()).forEach((eventNameSpace) => {
			const eventAndNameSpace = eventNameSpace.split(".");
			const namespace = eventAndNameSpace[1] || "JoditEventDefaultNamespace";
			callback.call(this, eventAndNameSpace[0], namespace);
		});
	}
	__getStore(subject) {
		if (!subject) throw error("Need subject");
		if (subject[this.__key] === void 0) {
			const store = new EventHandlersStore();
			Object.defineProperty(subject, this.__key, {
				enumerable: false,
				configurable: true,
				writable: true,
				value: store
			});
		}
		return subject[this.__key];
	}
	__removeStoreFromSubject(subject) {
		if (subject[this.__key] !== void 0) delete subject[this.__key];
	}
	__triggerNativeEvent(element, event) {
		const evt = this.__doc.createEvent("HTMLEvents");
		if (isString(event)) evt.initEvent(event, true, true);
		else {
			evt.initEvent(event.type, event.bubbles, event.cancelable);
			[
				"screenX",
				"screenY",
				"clientX",
				"clientY",
				"target",
				"srcElement",
				"currentTarget",
				"timeStamp",
				"which",
				"keyCode"
			].forEach((property) => {
				Object.defineProperty(evt, property, {
					value: event[property],
					enumerable: true
				});
			});
			Object.defineProperty(evt, "originalEvent", {
				value: event,
				enumerable: true
			});
		}
		element.dispatchEvent(evt);
	}
	/**
	* Get current event name
	*
	* @example
	* ```javascript
	* parent.e.on('openDialog closeDialog', function () {
	*     if (parent.e.current === 'closeDialog') {
	*         alert('Dialog was closed');
	*     } else {
	*         alert('Dialog was opened');
	*     }
	* });
	* ```
	*/
	get current() {
		return this.currents[this.currents.length - 1];
	}
	on(eventsOrSubjects, callbackOrEvents, optionsOrCallback, opts) {
		let subjects;
		let events;
		let callback;
		let options;
		if (isString(eventsOrSubjects) || isStringArray(eventsOrSubjects)) {
			subjects = this;
			events = eventsOrSubjects;
			callback = callbackOrEvents;
			options = optionsOrCallback;
		} else {
			subjects = eventsOrSubjects;
			events = callbackOrEvents;
			callback = optionsOrCallback;
			options = opts;
		}
		if (!(isString(events) || isStringArray(events)) || events.length === 0) throw error("Need events names");
		if (!isFunction(callback)) throw error("Need event handler");
		if (isArray(subjects)) {
			subjects.forEach((subj) => {
				this.on(subj, events, callback, options);
			});
			return this;
		}
		const subject = subjects;
		const store = this.__getStore(subject);
		const self = this;
		let syntheticCallback = function(event, ...args) {
			if (self.isMuted(event)) return;
			return callback && callback.call(this, ...args);
		};
		if (isDOMElement(subject)) syntheticCallback = function(event) {
			if (self.isMuted(event.type)) return;
			self.__prepareEvent(event);
			if (callback && callback.call(this, event) === false) {
				event.preventDefault();
				event.stopImmediatePropagation();
				return false;
			}
		};
		this.__eachEvent(events, (event, namespace) => {
			var _a, _b;
			if (event.length === 0) throw error("Need event name");
			if (store.indexOf(event, namespace, callback) === false) {
				const block = {
					event,
					originalCallback: callback,
					syntheticCallback
				};
				store.set(event, namespace, block, options === null || options === void 0 ? void 0 : options.top);
				if (isDOMElement(subject)) {
					const eOpts = PASSIVE_EVENTS.has(event) ? {
						passive: true,
						capture: (_a = options === null || options === void 0 ? void 0 : options.capture) !== null && _a !== void 0 ? _a : false
					} : (_b = options === null || options === void 0 ? void 0 : options.capture) !== null && _b !== void 0 ? _b : false;
					syntheticCallback.options = eOpts;
					subject.addEventListener(event, syntheticCallback, eOpts);
					this.__memoryDOMSubjectToHandler(subject, syntheticCallback);
				}
			}
		});
		return this;
	}
	__memoryDOMSubjectToHandler(subject, syntheticCallback) {
		const callbackStore = this.__domEventsMap.get(subject) || /* @__PURE__ */ new Set();
		callbackStore.add(syntheticCallback);
		this.__domEventsMap.set(subject, callbackStore);
	}
	__unmemoryDOMSubjectToHandler(subject, syntheticCallback) {
		const m = this.__domEventsMap;
		const callbackStore = m.get(subject) || /* @__PURE__ */ new Set();
		callbackStore.delete(syntheticCallback);
		if (callbackStore.size) m.set(subject, callbackStore);
		else m.delete(subject);
	}
	one(eventsOrSubjects, callbackOrEvents, optionsOrCallback, opts) {
		let subjects;
		let events;
		let callback;
		let options;
		if (isString(eventsOrSubjects) || isStringArray(eventsOrSubjects)) {
			subjects = this;
			events = eventsOrSubjects;
			callback = callbackOrEvents;
			options = optionsOrCallback;
		} else {
			subjects = eventsOrSubjects;
			events = callbackOrEvents;
			callback = optionsOrCallback;
			options = opts;
		}
		const newCallback = (...args) => {
			this.off(subjects, events, newCallback);
			return callback(...args);
		};
		this.on(subjects, events, newCallback, options);
		return this;
	}
	off(eventsOrSubjects, callbackOrEvents, handler) {
		let subjects;
		let events;
		let callback;
		if (isString(eventsOrSubjects) || isStringArray(eventsOrSubjects)) {
			subjects = this;
			events = eventsOrSubjects;
			callback = callbackOrEvents;
		} else {
			subjects = eventsOrSubjects;
			events = callbackOrEvents;
			callback = handler;
		}
		if (isArray(subjects)) {
			subjects.forEach((subj) => {
				this.off(subj, events, callback);
			});
			return this;
		}
		const subject = subjects;
		const store = this.__getStore(subject);
		if (!(isString(events) || isStringArray(events)) || events.length === 0) {
			store.namespaces().forEach((namespace) => {
				this.off(subject, "." + namespace);
			});
			this.__removeStoreFromSubject(subject);
			return this;
		}
		const removeEventListener = (block) => {
			var _a;
			if (isDOMElement(subject)) {
				subject.removeEventListener(block.event, block.syntheticCallback, (_a = block.syntheticCallback.options) !== null && _a !== void 0 ? _a : false);
				this.__unmemoryDOMSubjectToHandler(subject, block.syntheticCallback);
			}
		}, removeCallbackFromNameSpace = (event, namespace) => {
			if (event === "") {
				store.events(namespace).forEach((eventName) => {
					if (eventName !== "") removeCallbackFromNameSpace(eventName, namespace);
				});
				return;
			}
			const blocks = store.get(event, namespace);
			if (!blocks || !blocks.length) return;
			if (!isFunction(callback)) {
				blocks.forEach(removeEventListener);
				blocks.length = 0;
				store.clearEvents(namespace, event);
			} else {
				const index = store.indexOf(event, namespace, callback);
				if (index !== false) {
					removeEventListener(blocks[index]);
					blocks.splice(index, 1);
					if (!blocks.length) store.clearEvents(namespace, event);
				}
			}
		};
		this.__eachEvent(events, (event, namespace) => {
			if (namespace === "JoditEventDefaultNamespace") store.namespaces().forEach((namespace) => {
				removeCallbackFromNameSpace(event, namespace);
			});
			else removeCallbackFromNameSpace(event, namespace);
		});
		if (store.isEmpty()) this.__removeStoreFromSubject(subject);
		return this;
	}
	stopPropagation(subjectOrEvents, eventsList) {
		const subject = isString(subjectOrEvents) ? this : subjectOrEvents;
		const events = isString(subjectOrEvents) ? subjectOrEvents : eventsList;
		if (typeof events !== "string") throw error("Need event names");
		const store = this.__getStore(subject);
		this.__eachEvent(events, (event, namespace) => {
			const blocks = store.get(event, namespace);
			if (blocks) this.__stopped.push(blocks);
			if (namespace === "JoditEventDefaultNamespace") store.namespaces(true).forEach((ns) => this.stopPropagation(subject, event + "." + ns));
		});
	}
	__removeStop(currentBlocks) {
		if (currentBlocks) {
			const index = this.__stopped.indexOf(currentBlocks);
			index !== -1 && this.__stopped.splice(0, index + 1);
		}
	}
	__isStopped(currentBlocks) {
		return currentBlocks !== void 0 && this.__stopped.indexOf(currentBlocks) !== -1;
	}
	fire(subjectOrEvents, eventsList, ...args) {
		let result, result_value;
		const subject = isString(subjectOrEvents) ? this : subjectOrEvents;
		const events = isString(subjectOrEvents) ? subjectOrEvents : eventsList;
		const argumentsList = isString(subjectOrEvents) ? [eventsList, ...args] : args;
		if (!isDOMElement(subject) && !isString(events)) throw error("Need events names");
		const store = this.__getStore(subject);
		if (!isString(events) && isDOMElement(subject)) this.__triggerNativeEvent(subject, eventsList);
		else this.__eachEvent(events, (event, namespace) => {
			if (isDOMElement(subject)) this.__triggerNativeEvent(subject, event);
			else {
				const blocks = store.get(event, namespace);
				if (blocks) try {
					[...blocks].every((block) => {
						if (this.__isStopped(blocks)) return false;
						this.currents.push(event);
						result_value = block.syntheticCallback.call(subject, event, ...argumentsList);
						this.currents.pop();
						if (result_value !== void 0) result = result_value;
						return true;
					});
				} finally {
					this.__removeStop(blocks);
				}
				if (namespace === "JoditEventDefaultNamespace" && !isDOMElement(subject)) store.namespaces().filter((ns) => ns !== namespace).forEach((ns) => {
					const result_second = this.fire.apply(this, [
						subject,
						event + "." + ns,
						...argumentsList
					]);
					if (result_second !== void 0) result = result_second;
				});
			}
		});
		return result;
	}
	constructor(doc) {
		this.__domEventsMap = /* @__PURE__ */ new Map();
		this.__mutedEvents = /* @__PURE__ */ new Set();
		this.__key = "__JoditEventEmitterNamespaces";
		this.__doc = globalDocument;
		this.__prepareEvent = (e) => {
			if (e.cancelBubble) return;
			if (e.composed && isFunction(e.composedPath) && e.composedPath()[0]) Object.defineProperty(e, "target", {
				value: e.composedPath()[0],
				configurable: true,
				enumerable: true
			});
			if (e.type.match(/^touch/) && e.changedTouches && e.changedTouches.length) [
				"clientX",
				"clientY",
				"pageX",
				"pageY"
			].forEach((key) => {
				Object.defineProperty(e, key, {
					value: e.changedTouches[0][key],
					configurable: true,
					enumerable: true
				});
			});
			if (!e.originalEvent) e.originalEvent = e;
			if (e.type === "paste" && e.clipboardData === void 0 && this.__doc.defaultView.clipboardData) Object.defineProperty(e, "clipboardData", {
				get: () => {
					return this.__doc.defaultView.clipboardData;
				},
				configurable: true,
				enumerable: true
			});
		};
		this.currents = [];
		this.__stopped = [];
		this.__isDestructed = false;
		if (doc) this.__doc = doc;
		this.__key += (/* @__PURE__ */ new Date()).getTime();
	}
	destruct() {
		if (this.__isDestructed) return;
		this.__isDestructed = true;
		this.__domEventsMap.forEach((set, elm) => {
			this.off(elm);
		});
		this.__domEventsMap.clear();
		this.__mutedEvents.clear();
		this.currents.length = 0;
		this.__stopped.length = 0;
		this.off(this);
		this.__getStore(this).clear();
		this.__removeStoreFromSubject(this);
	}
};
function isDOMElement(subject) {
	return subject && isFunction(subject.addEventListener);
}
//#endregion
//#region node_modules/jodit/esm/core/event-emitter/global.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module event-emitter
*/
var eventEmitter$1 = new EventEmitter();
//#endregion
//#region node_modules/jodit/esm/core/helpers/utils/complete-url.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/utils
*/
var completeUrl = (url) => {
	if (globalWindow.location.protocol === "file:" && /^\/\//.test(url)) url = "https:" + url;
	return url;
};
//#endregion
//#region node_modules/jodit/esm/core/helpers/utils/append-script.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var alreadyLoadedList = /* @__PURE__ */ new Map();
var cacheLoaders = (loader) => {
	return (jodit, url) => {
		if (alreadyLoadedList.has(url)) return alreadyLoadedList.get(url);
		const promise = loader(jodit, url);
		alreadyLoadedList.set(url, promise);
		return promise;
	};
};
/**
* Load script and return promise
*/
var appendScriptAsync = cacheLoaders((jodit, url) => {
	return jodit.async.promise((resolve, reject) => {
		if (jodit.isInDestruct) return reject();
		const script = jodit.c.element("script", {
			type: "text/javascript",
			crossorigin: "anonymous",
			referrerpolicy: "no-referrer",
			async: true,
			src: completeUrl(url)
		});
		if (jodit.o.nonce) attr(script, "nonce", jodit.o.nonce);
		jodit.e.one(script, "error", reject).one(script, "load", resolve);
		Dom.append(jodit.od.body, script);
	});
});
/**
* Download CSS style script
*/
var appendStyleAsync = cacheLoaders((jodit, url) => {
	return jodit.async.promise((resolve, reject) => {
		if (jodit.isInDestruct) return reject();
		const link = jodit.c.element("link", {
			rel: "stylesheet",
			media: "all",
			crossorigin: "anonymous"
		});
		if (jodit.o.nonce) attr(link, "nonce", jodit.o.nonce);
		const callback = () => resolve(link);
		!jodit.isInDestruct && jodit.e.on(link, "load", callback).on(link, "error", reject);
		attr(link, "href", completeUrl(url));
		if (jodit.o.shadowRoot) Dom.append(jodit.o.shadowRoot, link);
		else Dom.append(jodit.od.body, link);
	});
});
function loadNext(jodit, urls, i = 0) {
	if (!isString(urls[i])) return Promise.resolve();
	return appendScriptAsync(jodit, urls[i]).then(() => loadNext(jodit, urls, i + 1));
}
function loadNextStyle(jodit, urls, i = 0) {
	if (!isString(urls[i])) return Promise.resolve();
	return appendStyleAsync(jodit, urls[i]).then(() => loadNextStyle(jodit, urls, i + 1));
}
//#endregion
//#region node_modules/jodit/esm/core/plugin/helpers/utils.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module plugin
*/
/**
* @private
*/
function normalizeName(name) {
	return kebabCase(name).toLowerCase();
}
//#endregion
//#region node_modules/jodit/esm/core/plugin/helpers/load.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var styles = /* @__PURE__ */ new Set();
/**
* @private
*/
async function loadStyle(jodit, pluginName) {
	const url = getFullUrl(jodit, pluginName, false);
	if (styles.has(url)) return;
	styles.add(url);
	return appendStyleAsync(jodit, url);
}
/**
* Call full url to the script or style file
* @private
*/
function getFullUrl(jodit, name, js) {
	name = kebabCase(name);
	const min = jodit.minified ? ".min" : "";
	return jodit.basePath + "plugins/" + name + "/" + name + min + "." + (js ? "js" : "css");
}
/**
* @private
*/
function loadExtras(items, jodit, extraList, callback) {
	try {
		const needLoadExtras = extraList.filter((extra) => !items.has(normalizeName(extra.name)));
		if (needLoadExtras.length) load(jodit, needLoadExtras, callback);
	} catch (e) {}
}
/**
* Download plugins
* @private
*/
function load(jodit, pluginList, callback) {
	pluginList.map((extra) => {
		return appendScriptAsync(jodit, extra.url || getFullUrl(jodit, extra.name, true)).then(callback).catch(() => null);
	});
}
//#endregion
//#region node_modules/jodit/esm/core/plugin/helpers/init-instance.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Init plugin if it has no dependencies, in another case wait requires plugins will be init
* @private
*/
function init(jodit, pluginName, plugin, instance, doneList, waitingList, getContainer) {
	if (isInitable(instance)) try {
		instance.init(jodit);
	} catch (e) {
		console.error(e);
	}
	doneList.set(pluginName, instance);
	waitingList.delete(pluginName);
	if (instance.hasStyle) loadStyle(jodit, pluginName).catch((e) => {});
	if (instance.styles) {
		const style = getContainer(jodit, pluginName, "style");
		style.innerHTML = instance.styles;
	}
}
//#endregion
//#region node_modules/jodit/esm/core/plugin/helpers/make-instance.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Create instance of plugin
* @private
*/
function makeInstance(jodit, plugin) {
	try {
		try {
			return isFunction(plugin) ? new plugin(jodit) : plugin;
		} catch (e) {
			if (isFunction(plugin) && !plugin.prototype) return plugin(jodit);
		}
	} catch (e) {
		console.error(e);
	}
	return null;
}
//#endregion
//#region node_modules/jodit/esm/core/plugin/plugin-system.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Jodit plugin system
* @example
* ```js
* Jodit.plugins.add('emoji2', {
* 	init() {
*  	alert('emoji Inited2')
* 	},
*	destruct() {}
* });
* ```
*/
var PluginSystem = class {
	constructor(opts) {
		this.opts = opts;
		this.__items = /* @__PURE__ */ new Map();
	}
	get size() {
		return this.__items.size;
	}
	/**
	* Add plugin in store
	*/
	add(name, plugin) {
		this.__items.set(normalizeName(name), plugin);
		eventEmitter$1.fire(`plugin:${name}:ready`);
	}
	/**
	* Get plugin from store
	*/
	get(name) {
		return this.__items.get(normalizeName(name));
	}
	/**
	* Remove plugin from store
	*/
	remove(name) {
		this.__items.delete(normalizeName(name));
	}
	__getFullPluginsList(filter) {
		const results = [];
		this.__items.forEach((plugin, name) => {
			if (!filter || filter.has(name)) results.push([name, plugin]);
		});
		return results;
	}
	/**
	* Public method for async init all plugins
	*/
	__init(jodit) {
		const { extraList, disableList, filter } = getSpecialLists(jodit);
		const doneList = /* @__PURE__ */ new Map();
		const pluginsMap = {};
		const waitingList = /* @__PURE__ */ new Set();
		jodit.__plugins = pluginsMap;
		const initPlugins = () => {
			if (jodit.isInDestruct) return;
			let commit = false;
			this.__getFullPluginsList(filter).forEach(([name, plugin]) => {
				if (disableList.has(name) || doneList.has(name)) return;
				const requires = plugin === null || plugin === void 0 ? void 0 : plugin.requires;
				if (requires && isArray(requires) && requires.length) {
					if (requires.some((req) => disableList.has(req))) return;
					if (!requires.every((name) => doneList.has(name))) {
						waitingList.add(name);
						return;
					}
				}
				commit = true;
				const instance = makeInstance(jodit, plugin);
				if (!instance) {
					doneList.set(name, null);
					waitingList.delete(name);
					return;
				}
				init(jodit, name, plugin, instance, doneList, waitingList, this.opts.getContainer);
				pluginsMap[name] = instance;
			});
			if (commit) {
				jodit.e.fire("updatePlugins");
				initPlugins();
			}
		};
		if (extraList && extraList.length) loadExtras(this.__items, jodit, extraList, initPlugins);
		initPlugins();
		bindOnBeforeDestruct(jodit, pluginsMap);
	}
	/**
	* Returns the promise to wait for the plugin to load.
	*/
	wait(name) {
		return new Promise((resolve) => {
			if (this.get(name)) return resolve();
			const onReady = () => {
				resolve();
				eventEmitter$1.off(`plugin:${name}:ready`, onReady);
			};
			eventEmitter$1.on(`plugin:${name}:ready`, onReady);
		});
	}
};
/**
* Destroy all plugins before - Jodit will be destroyed
*/
function bindOnBeforeDestruct(jodit, plugins) {
	jodit.e.on("beforeDestruct", () => {
		Object.keys(plugins).forEach((name) => {
			const instance = plugins[name];
			if (isDestructable(instance)) instance.destruct(jodit);
			delete plugins[name];
		});
		delete jodit.__plugins;
	});
}
function getSpecialLists(jodit) {
	return {
		extraList: jodit.o.extraPlugins.map((s) => isString(s) ? { name: s } : s),
		disableList: new Set(splitArray(jodit.o.disablePlugins).map(normalizeName)),
		filter: jodit.o.safeMode ? new Set(jodit.o.safePluginsList) : null
	};
}
//#endregion
//#region node_modules/jodit/esm/core/event-emitter/eventify.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Class for adding event handling capability
*
* ```ts
* class SomeClass extends Eventify<{ start: (node: Node) => boolean; }> {
* 	constructor() {
* 		super();
* 		setTimeout(() => {
* 			if (this.emit('start', document.body)) {
* 				console.log('yes');
* 			};
* 		}, 100);
* 	}
* }
*
* const sm = new SomeClass();
* sm.on('start', (node) => {
* 	console.log(node);
* 	return true;
* })
* ```
*/
var Eventify = class {
	constructor() {
		this.__map = /* @__PURE__ */ new Map();
	}
	on(name, func) {
		var _a;
		if (!this.__map.has(name)) this.__map.set(name, /* @__PURE__ */ new Set());
		(_a = this.__map.get(name)) === null || _a === void 0 || _a.add(func);
		return this;
	}
	off(name, func) {
		var _a;
		if (this.__map.has(name)) (_a = this.__map.get(name)) === null || _a === void 0 || _a.delete(func);
		return this;
	}
	destruct() {
		this.__map.clear();
	}
	emit(name, ...args) {
		var _a;
		let result;
		if (this.__map.has(name)) (_a = this.__map.get(name)) === null || _a === void 0 || _a.forEach((cb) => {
			result = cb(...args);
		});
		return result;
	}
};
//#endregion
//#region node_modules/jodit/esm/core/dom/lazy-walker.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$45 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
/**
* Walks the DOM tree in small chunks between browser tasks so that even huge
* documents do not block the main thread.
*
* Events:
* - `visit` - is called for every node; return `true` to mark the pass as
*   "had effect" (it will be passed into the `end` event)
* - `break` - the pass was interrupted via [[LazyWalker.break]]
* - `end` - the pass is finished, receives `true` if some `visit` handler returned `true`
*/
var LazyWalker = class extends Eventify {
	/**
	* Starts a new pass over the `root` tree.
	* If a previous pass is still running it will be stopped first.
	*/
	setWork(root) {
		if (this.isWorked) this.break();
		else this.stop();
		this.workNodes = Dom.eachGen(root, !this.options.reverse);
		this.isFinished = false;
		this.hadAffect = false;
		this._requestStarting();
		return this;
	}
	constructor(async, options = {}) {
		super();
		this.async = async;
		this.options = options;
		this.workNodes = null;
		this.hadAffect = false;
		this.isWorked = false;
		this.isFinished = false;
		this.__schedulerController = null;
	}
	_requestStarting() {
		this.__schedulerController = new AbortController();
		this.async.schedulerPostTask(this.__workPerform, {
			delay: this.options.timeout,
			signal: this.__schedulerController.signal
		}).catch(() => null);
	}
	break(reason) {
		if (this.isWorked) {
			this.stop();
			this.emit("break", reason);
		}
	}
	end() {
		if (this.isWorked) {
			this.stop();
			this.emit("end", this.hadAffect);
			this.hadAffect = false;
		}
	}
	stop() {
		var _a;
		this.isWorked = false;
		this.isFinished = true;
		this.workNodes = null;
		(_a = this.__schedulerController) === null || _a === void 0 || _a.abort();
		this.__schedulerController = null;
	}
	destruct() {
		super.destruct();
		this.stop();
	}
	__workPerform() {
		var _a;
		if (this.workNodes) {
			this.isWorked = true;
			let count = 0;
			const chunkSize = (_a = this.options.timeoutChunkSize) !== null && _a !== void 0 ? _a : 50;
			while (!this.isFinished && count < chunkSize) {
				const item = this.workNodes.next();
				count += 1;
				if (this.visitNode(item.value)) this.hadAffect = true;
				if (item.done) {
					this.end();
					return;
				}
			}
		} else this.end();
		if (!this.isFinished) this._requestStarting();
	}
	visitNode(nodeElm) {
		var _a;
		if (!nodeElm || this.options.whatToShow !== void 0 && nodeElm.nodeType !== this.options.whatToShow) return false;
		return (_a = this.emit("visit", nodeElm)) !== null && _a !== void 0 ? _a : false;
	}
};
__decorate$45([autobind], LazyWalker.prototype, "__workPerform", null);
//#endregion
//#region node_modules/jodit/esm/core/dom/index.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
//#endregion
//#region node_modules/jodit/esm/core/global.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var instances = {};
var counter = 1;
var uuids = /* @__PURE__ */ new Set();
/**
* Generate global unique uid
*/
function uniqueUid() {
	function gen() {
		counter += 10 * (Math.random() + 1);
		return Math.round(counter).toString(16);
	}
	let uid = gen();
	while (uuids.has(uid)) uid = gen();
	uuids.add(uid);
	return uid;
}
var pluginSystem = new PluginSystem({ getContainer });
var modules = {};
var boxes = /* @__PURE__ */ new WeakMap();
/**
* Create unique box(HTMLCotainer) and remove it after destroy
*/
function getContainer(jodit, classFunc, tag = "div", createInsideEditor = false) {
	const name = isString(classFunc) ? classFunc : classFunc ? getClassName(classFunc.prototype) : "jodit-utils";
	const data = boxes.get(jodit) || {}, key = name + tag;
	const view = isViewObject(jodit) ? jodit : jodit.j;
	let body = null;
	if (!data[key]) {
		let c = view.c;
		body = getPopupViewRoot(view.o, view.container, jodit.od.body);
		if (createInsideEditor && isJoditObject(jodit) && jodit.od !== jodit.ed) {
			c = jodit.createInside;
			const place = tag === "style" ? jodit.ed.head : jodit.ed.body;
			body = isJoditObject(jodit) && jodit.o.shadowRoot ? jodit.o.shadowRoot : place;
		}
		const box = c.element(tag, { className: `jodit jodit-${kebabCase(name)}-container jodit-box` });
		if (view.o.nonce && (tag === "style" || tag === "script")) attr(box, "nonce", view.o.nonce);
		box.classList.add(`jodit_theme_${view.o.theme || "default"}`);
		Dom.append(body, box);
		data[key] = box;
		jodit.hookStatus("beforeDestruct", () => {
			view.events.off(box);
			Dom.safeRemove(box);
			delete data[key];
			if (Object.keys(data).length) boxes.delete(jodit);
		});
		boxes.set(jodit, data);
		view.events.fire("getContainer", box);
	}
	data[key].classList.remove("jodit_theme_default", "jodit_theme_dark");
	data[key].classList.add(`jodit_theme_${view.o.theme || "default"}`);
	return data[key];
}
/**
* Get root element for view
* @internal
*/
function getPopupViewRoot(o, container, defaultRoot) {
	var _a, _b, _c;
	return (_c = (_b = (_a = o.popupRoot) !== null && _a !== void 0 ? _a : o.shadowRoot) !== null && _b !== void 0 ? _b : Dom.closest(container, (parentElement) => Dom.isHTMLElement(parentElement) && (Dom.isTag(parentElement, "dialog") || ["fixed", "absolute"].includes(css(parentElement, "position"))), defaultRoot)) !== null && _c !== void 0 ? _c : defaultRoot;
}
/**
* Global event emitter
* @deprecated use `import { eventEmitter } from 'jodit/core/event-emitter/global';`
*/
var eventEmitter = eventEmitter$1;
//#endregion
//#region node_modules/jodit/esm/core/helpers/array/as-array.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/array
*/
/**
* Always return Array
* ```javascript
* Jodit.modules.Helpers.asArray('test') // ['test']
* Jodit.modules.Helpers.asArray(['test']) // ['test']
* Jodit.modules.Helpers.asArray(1) // [1]
* ```
*/
var asArray = (a) => isArray(a) ? a : [a];
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @deprecated Do not use it in new code
*/
function $$(selector, root) {
	let result;
	result = root.querySelectorAll(selector);
	return [].slice.call(result);
}
/**
* Calculate XPath selector
*/
var getXPathByElement = (element, root) => {
	if (!element || element.nodeType !== Node.ELEMENT_NODE) return "";
	if (!element.parentNode || root === element) return "";
	if (element.id) return "//*[@id='" + element.id + "']";
	const sames = [].filter.call(element.parentNode.childNodes, (x) => x.nodeName === element.nodeName);
	return getXPathByElement(element.parentNode, root) + "/" + element.nodeName.toLowerCase() + (sames.length > 1 ? "[" + (toArray(sames).indexOf(element) + 1) + "]" : "");
};
/**
* Find all `ref` or `data-ref` elements inside HTMLElement
*/
var refs = (root) => {
	if ("container" in root) root = root.container;
	const def = {};
	Dom.each(root, (child) => {
		if (Dom.isElement(child) && (child.hasAttribute("ref") || child.hasAttribute("data-ref"))) {
			const key = attr(child, "-ref");
			if (key && isString(key)) {
				def[camelCase(key)] = child;
				def[key] = child;
			}
		}
	});
	return def;
};
/**
* Calculate full CSS selector
*/
var cssPath = (el) => {
	if (!Dom.isElement(el)) return null;
	const path = [];
	let start = el;
	while (start && start.nodeType === Node.ELEMENT_NODE) {
		let selector = start.nodeName.toLowerCase();
		if (start.id) {
			selector += "#" + start.id;
			path.unshift(selector);
			break;
		} else {
			let sib = start, nth = 1;
			do {
				sib = sib.previousElementSibling;
				if (sib && sib.nodeName.toLowerCase() === selector) nth++;
			} while (sib);
			selector += ":nth-of-type(" + nth + ")";
		}
		path.unshift(selector);
		start = start.parentNode;
	}
	return path.join(" > ");
};
/**
* Try to find element by selector
*/
function resolveElement(element, od) {
	let resolved = element;
	if (isString(element)) try {
		resolved = od.querySelector(element);
	} catch (_a) {
		throw error("String \"" + element + "\" should be valid HTML selector");
	}
	if (!resolved || typeof resolved !== "object" || !Dom.isElement(resolved) || !resolved.cloneNode) throw error("Element \"" + element + "\" should be string or HTMLElement instance");
	return resolved;
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/html/apply-styles.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/html
*/
function normalizeCSS(s) {
	return s.replace(/mso-[a-z-]+:[\s]*[^;]+;/gi, "").replace(/mso-[a-z-]+:[\s]*[^";']+$/gi, "").replace(/border[a-z-]*:[\s]*[^;]+;/gi, "").replace(/([0-9.]+)(pt|cm)/gi, (match, units, metrics) => {
		switch (metrics.toLowerCase()) {
			case "pt": return (parseFloat(units) * 1.328).toFixed(0) + "px";
			case "cm": return (parseFloat(units) * .02645833).toFixed(0) + "px";
		}
		return match;
	});
}
/**
* If the HTML has CSS rules with selectors,
* it applies them to the selectors in the HTML itself
* and then removes the selector styles, leaving only the inline ones.
*/
function applyStyles(html) {
	const openMatch = /<html(?:\s[^>]*)?>/i.exec(html);
	if (!openMatch) return html;
	html = html.substring(openMatch.index);
	const closeIndex = html.toLowerCase().lastIndexOf("</html>");
	if (closeIndex !== -1) html = html.substring(0, closeIndex + 7);
	const iframe = globalDocument.createElement("iframe");
	css(iframe, "display", "none");
	Dom.append(globalDocument.body, iframe);
	let convertedString = "", collection = [];
	try {
		const iframeDoc = iframe.contentDocument || (iframe.contentWindow ? iframe.contentWindow.document : null);
		if (iframeDoc) {
			iframeDoc.open();
			iframeDoc.write(html);
			iframeDoc.close();
			Dom.each(iframeDoc.body, (node) => {
				if (Dom.isElement(node) && /mso-list:\s*ignore/i.test(attr(node, "style") || "")) Dom.safeRemove(node);
			});
			try {
				for (let i = 0; i < iframeDoc.styleSheets.length; i += 1) {
					const rules = iframeDoc.styleSheets[i].cssRules;
					for (let idx = 0; idx < rules.length; idx += 1) {
						if (rules[idx].selectorText === "") continue;
						collection = $$(rules[idx].selectorText, iframeDoc.body);
						collection.forEach((elm) => {
							elm.style.cssText = normalizeCSS(rules[idx].style.cssText + ";" + elm.style.cssText);
						});
					}
				}
			} catch (e) {}
			Dom.each(iframeDoc.body, (node) => {
				if (Dom.isElement(node)) {
					const cssText = attr(node, "style");
					if (cssText) node.style.cssText = normalizeCSS(cssText);
					if (!attr(node, "style")) attr(node, "style", null);
				}
			});
			convertedString = iframeDoc.firstChild ? trim(iframeDoc.body.innerHTML) : "";
		}
	} catch (_a) {} finally {
		Dom.safeRemove(iframe);
	}
	if (convertedString) html = convertedString;
	return trim(html.replace(/<(\/)?(html|colgroup|col|o:p)[^>]*>/g, "").replace(/<!--[^>]*>/g, ""));
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/html/clean-from-word.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/html
*/
/**
* The method automatically cleans up content from Microsoft Word and other HTML sources to ensure clean, compliant
* content that matches the look and feel of the site.
*/
function cleanFromWord(html) {
	if (html.indexOf("<html ") !== -1) {
		html = html.substring(html.indexOf("<html "), html.length);
		html = html.substring(0, html.lastIndexOf("</html>") + 7);
	}
	let convertedString = "";
	try {
		const div = globalDocument.createElement("div");
		div.innerHTML = html;
		const marks = [];
		if (div.firstChild) Dom.each(div, (node) => {
			if (!node) return;
			switch (node.nodeType) {
				case Node.ELEMENT_NODE:
					switch (node.nodeName) {
						case "STYLE":
						case "LINK":
						case "META":
							marks.push(node);
							break;
						case "W:SDT":
						case "W:SDTPR":
						case "FONT":
							Dom.unwrap(node);
							break;
						default:
							if (/mso-list:\s*ignore/i.test(attr(node, "style") || "")) {
								marks.push(node);
								break;
							}
							toArray(node.attributes).forEach((attribute) => {
								if ([
									"src",
									"href",
									"rel",
									"content"
								].indexOf(attribute.name.toLowerCase()) === -1) attrRaw(node, attribute.name, null);
							});
					}
					break;
				case Node.TEXT_NODE: break;
				default: marks.push(node);
			}
		});
		Dom.safeRemove.apply(null, marks);
		convertedString = div.innerHTML;
	} catch (e) {}
	if (convertedString) html = convertedString;
	html = html.split(/(\n)/).filter(trim).join("\n");
	return html.replace(/<(\/)?(html|colgroup|col|o:p)[^>]*>/g, "").replace(/<!--[^>]*>/g, "");
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/html/htmlspecialchars.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/html
*/
/**
* Convert special characters to HTML entities
*/
function htmlspecialchars(html) {
	const tmp = globalDocument.createElement("div");
	tmp.textContent = html;
	return tmp.innerHTML;
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/html/nl2br.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/html
*/
/**
*  Inserts HTML line breaks before all newlines in a string
*/
function nl2br(html) {
	return html.replace(/\r\n|\r|\n/g, "<br/>");
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/utils/align.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Align image
*/
function hAlignElement(image, align) {
	if (align && align !== "normal") if (align !== "center") {
		css(image, "float", align);
		clearCenterAlign(image);
	} else css(image, {
		float: "",
		display: "block",
		marginLeft: "auto",
		marginRight: "auto"
	});
	else {
		if (css(image, "float") && ["right", "left"].indexOf(css(image, "float").toString().toLowerCase()) !== -1) css(image, "float", "");
		clearCenterAlign(image);
	}
}
/**
* Remove text-align style for all selected children
*/
function clearAlign(node) {
	Dom.each(node, (elm) => {
		if (Dom.isHTMLElement(elm)) {
			if (cssInline(elm, "textAlign")) {
				css(elm, "textAlign", "");
				if (!(attr(elm, "style") || "").trim().length) attr(elm, "style", null);
			}
		}
	});
}
/**
* Apply align for element
*/
var ALIGN_BY_COMMAND = /* @__PURE__ */ new Map([
	["justifyfull", "justify"],
	["justifyright", "right"],
	["justifyleft", "left"],
	["justifycenter", "center"]
]);
function alignElement(command, box) {
	if (Dom.isNode(box) && Dom.isElement(box)) {
		clearAlign(box);
		const align = ALIGN_BY_COMMAND.get(command.toLowerCase());
		if (align) css(box, "textAlign", align);
	}
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/utils/browser.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/utils
*/
/**
* Module returns method that is used to determine the browser
* @example
* ```javascript
* console.log(Jodit.modules.Helpers.browser('mse'));
* console.log(Jodit.modules.Helpers.browser('chrome'));
* console.log($Jodit.modules.Helpers.browser('opera'));
* console.log(Jodit.modules.Helpers.browser('firefox'));
* console.log(Jodit.modules.Helpers.browser('mse') && Jodit.modules.Helpers.browser('version') > 10);
* ```
*/
var browser = (browser) => {
	const ua = navigator.userAgent.toLowerCase(), match = /(firefox)[\s/]([\w.]+)/.exec(ua) || /(chrome)[\s/]([\w.]+)/.exec(ua) || /(webkit)[\s/]([\w.]+)/.exec(ua) || /(opera)(?:.*version)[\s/]([\w.]+)/.exec(ua) || /(msie)[\s]([\w.]+)/.exec(ua) || /(trident)\/([\w.]+)/.exec(ua) || ua.indexOf("compatible") < 0 || [];
	if (browser === "version") return match[2];
	if (browser === "webkit") return match[1] === "chrome" || match[1] === "webkit";
	if (browser === "ff") return match[1] === "firefox";
	if (browser === "msie") return match[1] === "trident" || match[1] === "msie";
	return match[1] === browser;
};
//#endregion
//#region node_modules/jodit/esm/core/helpers/utils/build-query.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Build query string
*/
var buildQuery = (data, prefix) => {
	const str = [];
	const enc = encodeURIComponent;
	for (const dataKey in data) if (Object.prototype.hasOwnProperty.call(data, dataKey)) {
		const k = prefix ? prefix + "[" + dataKey + "]" : dataKey;
		const v = data[dataKey];
		str.push(isPlainObject(v) ? buildQuery(v, k) : enc(k) + "=" + enc(v));
	}
	return str.join("&");
};
//#endregion
//#region node_modules/jodit/esm/config.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Editor options. These can be configured upon the creation of the editor.
* ```javascript
* const editor = Jodit.make('#editor', {
* 	 toolbar: true,
* 	 buttons: ['bold', 'italic', 'underline']
* 	 // other options
* 	 // ...
* });
* ```
* @packageDocumentation
* @module config
*/
var ConfigPrototype = {};
/**
* Default Editor's Configuration.
*
* This class holds all default option values for the Jodit editor.
* It uses a **private constructor** and a **lazy singleton** pattern — the single instance
* is created on the first access to {@link Config.defaultOptions} (also available as `Jodit.defaultOptions`).
*
* ## How options are resolved
*
* When you create an editor with `Jodit.make('#editor', userOptions)`, the library
* calls {@link ConfigProto}(userOptions, Config.defaultOptions). `ConfigProto` does
* **not** deep-clone the defaults. Instead it creates a new object whose JavaScript
* prototype is `Config.defaultOptions`:
*
* ```
* userOptions  ──[[Prototype]]──►  Config.defaultOptions
* ```
*
* Any key present in `userOptions` shadows the default;
* any key **not** present falls through to `Config.defaultOptions` via the prototype chain.
* Nested plain objects are recursively prototyped in the same way, so partial overrides
* of nested options work automatically:
*
* ```js
* // Only override `dialogWidth`; all other `image.*` defaults are still available
* Jodit.make('#editor', {
*   image: { dialogWidth: 500 }
* });
* ```
*
* ## How plugins extend the config
*
* Each plugin adds its own defaults by assigning to `Config.prototype` and augmenting
* the TypeScript type with `declare module`:
*
* ```ts
* // 1. Type augmentation (compile-time)
* declare module 'jodit/config' {
*   interface Config {
*     toolbarSticky: boolean;
*   }
* }
*
* // 2. Runtime default
* Config.prototype.toolbarSticky = true;
* ```
*
* Because the constructor runs `Object.assign(this, ConfigPrototype)` (where
* `ConfigPrototype` is captured as `Config.prototype` after the class definition),
* all prototype-level values — including those added by plugins — are materialized
* as own properties on the singleton. This means `Config.defaultOptions` always
* contains every registered option as an own, enumerable property.
*
* ## Changing global defaults
*
* You can modify `Jodit.defaultOptions` **before** creating editors to change
* defaults globally:
*
* ```js
* Jodit.defaultOptions.language = 'de';
* Jodit.defaultOptions.theme = 'dark';
*
* // Both editors inherit the new defaults
* Jodit.make('#editor1');
* Jodit.make('#editor2');
* ```
*
* ## `Jodit.atom` — preventing deep merge
*
* By default, `ConfigProto` deep-merges nested plain objects and arrays.
* Wrap a value with `Jodit.atom(value)` to make it **atomic** — it will completely
* replace the default instead of being merged:
*
* ```js
* Jodit.make('#editor', {
*   controls: {
*     fontsize: {
*       // Replace the entire list rather than merging with the default one
*       list: Jodit.atom([8, 9, 10])
*     }
*   }
* });
* ```
*
* `Jodit.atom` calls {@link markAsAtomic}, which sets a non-enumerable
* `isAtom` flag on the object. `ConfigProto` checks this flag and skips
* recursive merging when it is present. Note: top-level arrays (depth 0)
* are always treated as atomic — they replace rather than merge.
*
* @see {@link ConfigProto} for the full merge algorithm
* @see {@link markAsAtomic} / {@link isAtom} for the atom marker implementation
*/
var Config = class Config {
	constructor() {
		/**
		* When enabled, the editor caches the results of expensive computations (e.g. toolbar rebuilds)
		* to improve performance. Disable for debugging or when options change frequently at runtime.
		*/
		this.cache = true;
		/**
		* Timeout of all asynchronous methods
		*/
		this.defaultTimeout = 100;
		/**
		* Prefix used for CSS class names and local-storage keys to avoid collisions
		* when multiple editor instances or applications share the same page.
		*/
		this.namespace = "";
		/**
		* Editor loads completely without plugins. Useful when debugging your own plugin.
		*/
		this.safeMode = false;
		/**
		* Editor's width
		*
		* ```javascript
		* Jodit.make('.editor', {
		*    width: '100%',
		* })
		* ```
		* ```javascript
		* Jodit.make('.editor', {
		*    width: 600, // equivalent for '600px'
		* })
		* ```
		* ```javascript
		* Jodit.make('.editor', {
		*    width: 'auto', // autosize
		* })
		* ```
		*/
		this.width = "auto";
		/**
		* Editor's height
		*
		* ```javascript
		* Jodit.make('.editor', {
		*    height: '100%',
		* })
		* ```
		* ```javascript
		* Jodit.make('.editor', {
		*    height: 600, // equivalent for '600px'
		* })
		* ```
		* ```javascript
		* Jodit.make('.editor', {
		*    height: 'auto', // default - autosize
		* })
		* ```
		*/
		this.height = "auto";
		/**
		* List of plugins that will be initialized in safe mode.
		*
		* ```js
		* Jodit.make('#editor', {
		* 	safeMode: true,
		* 	safePluginsList: ['about'],
		* 	extraPlugins: ['yourPluginDev']
		* });
		* ```
		*/
		this.safePluginsList = [
			"about",
			"enter",
			"backspace",
			"size",
			"bold",
			"hotkeys"
		];
		/**
		* Reserved for the paid version of the editor
		*/
		this.license = "";
		/**
		* The name of the preset that will be used to initialize the editor.
		* The list of available presets can be found here Jodit.defaultOptions.presets
		* ```javascript
		* Jodit.make('.editor', {
		* 	preset: 'inline'
		* });
		* ```
		*/
		this.preset = "custom";
		/**
		* Dictionary of named configuration presets. Each key is a preset name and the value
		* is a partial options object that will be merged into the editor config when
		* {@link Config.preset} matches the key.
		*
		* ```javascript
		* // Use a built-in preset
		* Jodit.make('#editor', {
		*     preset: 'inline'
		* });
		* ```
		*
		* ```javascript
		* // Define and use a custom preset
		* Jodit.defaultOptions.presets.myCompact = {
		*     toolbarButtonSize: 'small',
		*     showCharsCounter: false,
		*     showWordsCounter: false,
		*     showXPathInStatusbar: false
		* };
		*
		* Jodit.make('#editor', {
		*     preset: 'myCompact'
		* });
		* ```
		*/
		this.presets = { inline: {
			inline: true,
			toolbar: false,
			toolbarInline: true,
			toolbarInlineForSelection: true,
			showXPathInStatusbar: false,
			showCharsCounter: false,
			showWordsCounter: false,
			showPlaceholder: false
		} };
		/**
		* The Document object the editor operates within. Defaults to the current `document`.
		* Override when the editor is created inside an iframe or a different browsing context.
		*/
		this.ownerDocument = globalDocument;
		/**
		* Allows you to specify the window in which the editor will be created. Default - window
		* This is necessary if you are creating the editor inside an iframe but the code is running in the parent window
		*/
		this.ownerWindow = globalWindow;
		/**
		* Shadow root if Jodit was created in it
		*
		* ```html
		* <div id="editor"></div>
		* ```
		*
		* ```js
		* const app = document.getElementById('editor');
		* app.attachShadow({ mode: 'open' });
		* const root = app.shadowRoot;
		*
		* root.innerHTML = `
		* <link rel="stylesheet" href="./build/jodit.css"/>
		* <h1>Jodit example in Shadow DOM</h1>
		* <div id="edit"></div>
		* `;
		*
		* const editor = Jodit.make(root.getElementById('edit'), {
		* 	globalFullSize: false,
		* 	shadowRoot: root
		* });
		* editor.value = '<p>start</p>';
		* ```
		*/
		this.shadowRoot = null;
		/**
		* CSP nonce applied to every `<style>`, `<script>` and `<link>` element
		* Jodit injects at runtime (plugin styles, CDN scripts for ACE/beautify,
		* downloaded stylesheets). Set it to the same nonce your server puts in the
		* `Content-Security-Policy` header so a strict `style-src`/`script-src`
		* policy does not block the editor.
		*
		* ```js
		* Jodit.make('#editor', {
		* 	nonce: 'r4nd0m'
		* });
		* ```
		*/
		this.nonce = "";
		/**
		* Base CSS `z-index` for the editor UI (toolbar, popups, dialogs).
		* Set to a higher value when other page elements overlap the editor.
		* `0` means no explicit z-index is applied.
		*/
		this.zIndex = 0;
		/**
		* Change the read-only state of the editor
		*/
		this.readonly = false;
		/**
		* Change the disabled state of the editor
		*/
		this.disabled = false;
		/**
		* In readOnly mode, some buttons can still be useful, for example, the button to view source code or print
		*/
		this.activeButtonsInReadOnly = [
			"source",
			"fullsize",
			"print",
			"about",
			"dots",
			"selectall"
		];
		/**
		* When the editor is in read-only mode, some commands can still be executed:
		* ```javascript
		* const editor = Jodit.make('.editor', {
		* 	 allowCommandsInReadOnly: ['selectall', 'preview', 'print']
		* 	 readonly: true
		* });
		* editor.execCommand('selectall');// will be selected all content
		* editor.execCommand('delete');// but content will not be deleted
		* ```
		*/
		this.allowCommandsInReadOnly = [
			"selectall",
			"preview",
			"print"
		];
		/**
		* Size of icons in the toolbar (can be "small", "middle", "large")
		*
		* ```javascript
		* const editor = Jodit.make(".dark_editor", {
		*      toolbarButtonSize: "small"
		* });
		* ```
		*/
		this.toolbarButtonSize = "middle";
		/**
		* Allow navigation in the toolbar of the editor by Tab key
		*/
		this.allowTabNavigation = false;
		/**
		* When enabled, the editor renders without its own container chrome (toolbar, borders, statusbar).
		* The editable area becomes the element itself. Typically combined with
		* `toolbarInline: true` so a floating toolbar appears on selection.
		*/
		this.inline = false;
		/**
		* Theme (can be "dark")
		*
		* ```javascript
		* const editor = Jodit.make(".dark_editor", {
		*      theme: "dark"
		* });
		* ```
		*/
		this.theme = "default";
		/**
		* if set true, then the current mode is saved in a cookie, and is restored after a reload of the page
		*/
		this.saveModeInStorage = false;
		/**
		* Configure the provider that backs {@link IViewBased.asyncStorage}.
		*
		* By default the editor's `asyncStorage` uses persistent `IndexedDB` (with an
		* in-memory fallback when it is unavailable). Set `defaultProvider` to override it:
		* - `'local'` — persist in `localStorage`;
		* - `'memory'` — keep everything in memory (nothing survives a reload);
		* - a custom {@link IAsyncStorage} implementation — plug in your own backend.
		*
		* ```javascript
		* Jodit.make('#editor', {
		*    asyncStorage: { defaultProvider: 'local' }
		* });
		*
		* // or a fully custom backend
		* Jodit.make('#editor', {
		*    asyncStorage: { defaultProvider: myAsyncStorage }
		* });
		* ```
		*/
		this.asyncStorage = {};
		/**
		* Class name that can be appended to the editable area
		*
		* @see {@link Config.iframeCSSLinks}
		* @see {@link Config.iframeStyle}
		*
		* ```javascript
		* Jodit.make('#editor', {
		*    editorClassName: 'some_my_class'
		* });
		* ```
		* ```html
		* <style>
		* .some_my_class p{
		*    line-height: 16px;
		* }
		* </style>
		* ```
		*/
		this.editorClassName = false;
		/**
		* Class name that can be appended to the main editor container
		*
		* ```javascript
		* const jodit = Jodit.make('#editor', {
		*    className: 'some_my_class'
		* });
		*
		* console.log(jodit.container.classList.contains('some_my_class')); // true
		* ```
		* ```html
		* <style>
		* .some_my_class {
		*    max-width: 600px;
		*    margin: 0 auto;
		* }
		* </style>
		* ```
		*/
		this.className = false;
		/**
		* The internal styles of the editable area. They are intended to change
		* not the appearance of the editor, but to change the appearance of the content.
		*
		* ```javascript
		* Jodit.make('#editor', {
		* 		style: {
		* 		 font: '12px Arial',
		* 		 color: '#0c0c0c'
		* 		}
		* });
		* ```
		*/
		this.style = false;
		/**
		* Inline CSS styles applied to the outer editor container element.
		* Use this to style the editor wrapper (borders, background, etc.) without affecting content.
		*
		* ```javascript
		* Jodit.make('#editor', {
		* 		containerStyle: {
		* 		 border: '1px solid #ccc',
		* 		 background: '#f9f9f9'
		* 		}
		* });
		* ```
		*/
		this.containerStyle = false;
		/**
		* Dictionary of variable values in css, a complete list can be found here
		* https://github.com/xdan/jodit/blob/main/src/styles/variables.less#L25
		*
		* ```js
		* const editor = Jodit.make('#editor', {
		*   styleValues: {
		*		'color-text': 'red',
		*		colorBorder: 'black',
		*		'color-panel': 'blue'
		*   }
		* });
		* ```
		*/
		this.styleValues = {};
		/**
		* When enabled, the editor dispatches a native `change` event on the original
		* `<textarea>` element whenever the content changes, so standard DOM listeners work.
		*
		* ```javascript
		* const editor = Jodit.make('#editor');
		* document.getElementById('editor').addEventListener('change', function () {
		*      console.log(this.value);
		* })
		* ```
		*/
		this.triggerChangeEvent = true;
		/**
		* The writing direction of the language which is used to create editor content. Allowed values are: ''
		* (an empty string) – Indicates that content direction will be the same as either the editor UI direction or
		* the page element direction. 'ltr' – Indicates a Left-To-Right text direction (like in English).
		* 'rtl' – Indicates a Right-To-Left text direction (like in Arabic).
		*
		* ```javascript
		* Jodit.make('.editor', {
		*    direction: 'rtl'
		* })
		* ```
		*/
		this.direction = "";
		/**
		* Language by default. if `auto` language set by document.documentElement.lang ||
		* (navigator.language && navigator.language.substr(0, 2)) ||
		* (navigator.browserLanguage && navigator.browserLanguage.substr(0, 2)) || 'en'
		*
		* ```html
		* <!-- include in you page lang file -->
		* <script src="jodit/lang/de.js"><\/script>
		* <script>
		* var editor = Jodit.make('.editor', {
		*    language: 'de'
		* });
		* <\/script>
		* ```
		*/
		this.language = "auto";
		/**
		* if true all Lang.i18n(key) return `{key}`
		*
		* ```html
		* <script>
		* var editor = Jodit.make('.editor', {
		*    debugLanguage: true
		* });
		*
		* console.log(editor.i18n("Test")); // {Test}
		* <\/script>
		* ```
		*/
		this.debugLanguage = false;
		/**
		* Collection of language pack data `{en: {'Type something': 'Type something', ...}}`
		*
		* ```javascript
		* const editor = Jodit.make('#editor', {
		*     language: 'ru',
		*     i18n: {
		*         ru: {
		*            'Type something': 'Начните что-либо вводить'
		*         }
		*     }
		* });
		* console.log(editor.i18n('Type something')) //Начните что-либо вводить
		* ```
		*/
		this.i18n = false;
		/**
		* The tabindex global attribute is an integer indicating if the element can take
		* input focus (is focusable), if it should participate to sequential keyboard navigation,
		* and if so, at what position. It can take several values
		*/
		this.tabIndex = -1;
		/**
		* Boolean, whether the toolbar should be shown.
		* Alternatively, a valid css-selector-string to use an element as toolbar container.
		*/
		this.toolbar = true;
		/**
		* Boolean, whether the statusbar should be shown.
		*/
		this.statusbar = true;
		/**
		* Show tooltip after mouse enter on the button
		*/
		this.showTooltip = true;
		/**
		* Delay before show tooltip
		*/
		this.showTooltipDelay = 200;
		/**
		* Instead of creating a custom tooltip, use the browser's native title tooltips
		*/
		this.useNativeTooltip = false;
		/**
		* How pasted content is inserted into the editor by default.
		* Possible values: `insert_as_html`, `insert_as_text`, `insert_only_text`, `insert_clear_html`.
		*/
		this.defaultActionOnPaste = INSERT_AS_HTML;
		/**
		* Element that will be created when you press Enter
		*/
		this.enter = "p";
		/**
		* When this option is enabled, the editor's content will be placed in an iframe and isolated from the rest of the page.
		*
		* ```javascript
		* Jodit.make('#editor', {
		*    iframe: true,
		*    iframeStyle: 'html{margin: 0px;}body{padding:10px;background:transparent;color:#000;position:relative;z-index:2;\
		*    user-select:auto;margin:0px;overflow:hidden;}body:after{content:"";clear:both;display:block}';
		* });
		* ```
		*/
		this.iframe = false;
		/**
		* Allow editing the entire HTML document(html, head)
		* \> Works together with the iframe option.
		*
		* ```js
		* const editor = Jodit.make('#editor', {
		*   iframe: true,
		*   editHTMLDocumentMode: true
		* });
		* editor.value = '<!DOCTYPE html><html lang="en" style="overflow-y:hidden">' +
		* 	'<head><title>Jodit Editor</title></head>' +
		* 	'<body spellcheck="false"><p>Some text</p><p> a </p></body>' +
		* 	'</html>';
		* ```
		*/
		this.editHTMLDocumentMode = false;
		/**
		* Use when you need to insert new block element
		* use enter option if not set
		*/
		this.enterBlock = this.enter !== "br" ? this.enter : "p";
		/**
		* Jodit.MODE_WYSIWYG The HTML editor allows you to write like MSWord,
		* Jodit.MODE_SOURCE syntax highlighting source editor
		*
		* ```javascript
		* var editor = Jodit.make('#editor', {
		*     defaultMode: Jodit.MODE_SPLIT
		* });
		* console.log(editor.getRealMode())
		* ```
		*/
		this.defaultMode = 1;
		/**
		* When enabled, the editor displays both the WYSIWYG view and the source-code view side by side.
		*/
		this.useSplitMode = false;
		/**
		* The colors in HEX representation to select a color for the background and for the text in colorpicker
		*
		* ```javascript
		*  Jodit.make('#editor', {
		*     colors: ['#ff0000', '#00ff00', '#0000ff']
		* })
		* ```
		*/
		this.colors = {
			greyscale: [
				"#000000",
				"#434343",
				"#666666",
				"#999999",
				"#B7B7B7",
				"#CCCCCC",
				"#D9D9D9",
				"#EFEFEF",
				"#F3F3F3",
				"#FFFFFF"
			],
			palette: [
				"#980000",
				"#FF0000",
				"#FF9900",
				"#FFFF00",
				"#00F0F0",
				"#00FFFF",
				"#4A86E8",
				"#0000FF",
				"#9900FF",
				"#FF00FF"
			],
			full: [
				"#E6B8AF",
				"#F4CCCC",
				"#FCE5CD",
				"#FFF2CC",
				"#D9EAD3",
				"#D0E0E3",
				"#C9DAF8",
				"#CFE2F3",
				"#D9D2E9",
				"#EAD1DC",
				"#DD7E6B",
				"#EA9999",
				"#F9CB9C",
				"#FFE599",
				"#B6D7A8",
				"#A2C4C9",
				"#A4C2F4",
				"#9FC5E8",
				"#B4A7D6",
				"#D5A6BD",
				"#CC4125",
				"#E06666",
				"#F6B26B",
				"#FFD966",
				"#93C47D",
				"#76A5AF",
				"#6D9EEB",
				"#6FA8DC",
				"#8E7CC3",
				"#C27BA0",
				"#A61C00",
				"#CC0000",
				"#E69138",
				"#F1C232",
				"#6AA84F",
				"#45818E",
				"#3C78D8",
				"#3D85C6",
				"#674EA7",
				"#A64D79",
				"#85200C",
				"#990000",
				"#B45F06",
				"#BF9000",
				"#38761D",
				"#134F5C",
				"#1155CC",
				"#0B5394",
				"#351C75",
				"#733554",
				"#5B0F00",
				"#660000",
				"#783F04",
				"#7F6000",
				"#274E13",
				"#0C343D",
				"#1C4587",
				"#073763",
				"#20124D",
				"#4C1130"
			]
		};
		/**
		* The default tab color picker
		*
		* ```javascript
		* Jodit.make('#editor2', {
		*     colorPickerDefaultTab: 'color'
		* })
		* ```
		*/
		this.colorPickerDefaultTab = "background";
		/**
		* Default width (in pixels) applied to images inserted into the editor
		*/
		this.imageDefaultWidth = 300;
		/**
		* Do not display these buttons that are on the list
		*
		* ```javascript
		* Jodit.make('#editor2', {
		*     removeButtons: ['hr', 'source']
		* });
		* ```
		*/
		this.removeButtons = [];
		/**
		* Do not init these plugins
		*
		* ```typescript
		* var editor = Jodit.make('.editor', {
		*    disablePlugins: 'table,iframe'
		* });
		* //or
		* var editor = Jodit.make('.editor', {
		*    disablePlugins: ['table', 'iframe']
		* });
		* ```
		*/
		this.disablePlugins = [];
		/**
		* Init and download extra plugins that are **not** already bundled/registered.
		*
		* For every name in this list that is not found in the plugin registry, Jodit
		* loads it **at runtime over the network** from:
		*
		* ```text
		* <basePath>plugins/<name>/<name>(.min).js
		* ```
		*
		* (see {@link Config.basePath} and {@link Config.minified}). If the plugin is
		* already registered — e.g. you imported it statically, or you use a bundle
		* that ships it (such as the `jodit-pro` / `jodit-pro-react` "all plugins"
		* build) — it is **skipped** and no request is made; in that case you don't
		* need `extraPlugins` at all, just add the plugin's button.
		*
		* ```typescript
		* // Dynamic loading: fetches <basePath>plugins/emoji/emoji.js
		* const editor = Jodit.make('.editor', {
		*    extraPlugins: ['emoji']
		* });
		* ```
		*
		* You can also pass an explicit URL to bypass the `basePath` convention:
		*
		* ```typescript
		* const editor = Jodit.make('.editor', {
		*    extraPlugins: [{ name: 'emoji', url: 'https://cdn.example.com/emoji.js' }]
		* });
		* ```
		*
		* Note: if you see a request to a malformed URL (e.g. `.../src/main.tsx?t=...plugins/emoji/emoji.js`),
		* it means `basePath` was auto-detected incorrectly under your bundler — set
		* {@link Config.basePath} explicitly. See the Plugin System docs for details.
		*/
		this.extraPlugins = [];
		/**
		* Additional buttons appended to the {@link Config.buttons} list
		*/
		this.extraButtons = [];
		/**
		* By default, you can only install an icon from the Jodit suite.
		* You can add your icon to the set using the `Jodit.modules.Icon.set (name, svg Code)` method.
		* But for a declarative declaration, you can use this option.
		*
		* ```js
		* Jodit.modules.Icon.set('someIcon', '<svg><path.../></svg>');
		* const editor = Jodit.make({
		*   extraButtons: [{
		*     name: 'someButton',
		*     icon: 'someIcon'
		*   }]
		* });
		* ```
		*
		* ```js
		* const editor = Jodit.make({
		*   extraIcons: {
		*     someIcon: '<svg><path.../></svg>'
		*   },
		*   extraButtons: [{
		*     name: 'someButton',
		*     icon: 'someIcon'
		*   }]
		* });
		* ```
		*
		* ```js
		* const editor = Jodit.make({
		*   extraButtons: [{
		*     name: 'someButton',
		*     icon: '<svg><path.../></svg>'
		*   }]
		* });
		* ```
		*/
		this.extraIcons = {};
		/**
		* Default attributes for created inside editor elements
		*
		* ```js
		* const editor2 = Jodit.make('#editor', {
		* 	createAttributes: {
		* 		div: {
		* 			class: 'test'
		* 		},
		* 		ul: function (ul) {
		* 			ul.classList.add('ui-test');
		* 		}
		* 	}
		* });
		*
		* const div2 = editor2.createInside.div();
		* expect(div2.className).equals('test');
		*
		* const ul = editor2.createInside.element('ul');
		* expect(ul.className).equals('ui-test');
		* ```
		* Or JSX in React
		*
		* ```jsx
		* import React, {useState, useRef} from 'react';
		* import JoditEditor from "jodit-react";
		*
		* const config = {
		* 	createAttributes: {
		* 		div: {
		* 			class: 'align-center'
		* 		}
		* 	}
		* };
		*
		* <JoditEditor config={config}/>
		* ```
		*/
		this.createAttributes = { table: { style: "border-collapse:collapse;width: 100%;" } };
		/**
		* The width of the editor, accepted as the biggest. Used to the responsive version of the editor
		*/
		this.sizeLG = 900;
		/**
		* The width of the editor, accepted as the medium. Used to the responsive version of the editor
		*/
		this.sizeMD = 700;
		/**
		* The width of the editor, accepted as the small. Used to the responsive version of the editor
		*/
		this.sizeSM = 400;
		/**
		* The list of buttons that appear in the editor's toolbar on large places (≥ options.sizeLG).
		* Note - this is not the width of the device, the width of the editor
		*
		* ```javascript
		* Jodit.make('#editor', {
		*     buttons: ['bold', 'italic', 'source'],
		*     buttonsMD: ['bold', 'italic'],
		*     buttonsXS: ['bold', 'fullsize'],
		* });
		* ```
		*
		* ```javascript
		* Jodit.make('#editor2', {
		*     buttons: [{
		*         name: 'empty',
		*         icon: 'source',
		*         exec: function (editor) {
		*             const dialog = new Jodit.modules.Dialog({}),
		*                 text = editor.c.element('textarea');
		*
		*             dialog.setHeader('Source code');
		*             dialog.setContent(text);
		*             dialog.setSize(400, 300);
		*
		*             Jodit.modules.Helpers.css(elm, {
		*                 width: '100%',
		*                 height: '100%'
		*             })
		
		*             dialog.open();
		*         }
		*     }]
		* });
		* ```
		*
		* ```javascript
		* Jodit.make('#editor2', {
		*     buttons: Jodit.defaultOptions.buttons.concat([{
		*        name: 'listsss',
		*        iconURL: 'stuf/dummy.png',
		*        list: {
		*            h1: 'insert Header 1',
		*            h2: 'insert Header 2',
		*            clear: 'Empty editor',
		*        },
		*        exec: ({originalEvent, control, btn}) => {
		*             var key = control.args[0],
		*                value = control.args[1];
		*             if (key === 'clear') {
		*                 this.val('');
		*                 return;
		*             }
		*             this.s.insertNode(this.c.element(key, ''));
		*             this.message.info('Was inserted ' + value);
		*        },
		*        template: function (key, value) {
		*            return '<div>' + value + '</div>';
		*        }
		*  });
		* ```
		*/
		this.buttons = [
			{
				group: "font-style",
				buttons: []
			},
			{
				group: "list",
				buttons: []
			},
			{
				group: "font",
				buttons: []
			},
			"---",
			{
				group: "script",
				buttons: []
			},
			{
				group: "media",
				buttons: []
			},
			"\n",
			{
				group: "state",
				buttons: []
			},
			{
				group: "clipboard",
				buttons: []
			},
			{
				group: "insert",
				buttons: []
			},
			{
				group: "indent",
				buttons: []
			},
			{
				group: "color",
				buttons: []
			},
			{
				group: "form",
				buttons: []
			},
			"---",
			{
				group: "history",
				buttons: []
			},
			{
				group: "search",
				buttons: []
			},
			{
				group: "source",
				buttons: []
			},
			{
				group: "other",
				buttons: []
			},
			{
				group: "info",
				buttons: []
			}
		];
		/**
		* Some events are called when the editor is initialized, for example, the `afterInit` event.
		* So this code won't work:
		* ```javascript
		* const editor = Jodit.make('#editor');
		* editor.events.on('afterInit', () => console.log('afterInit'));
		* ```
		* You need to do this:
		* ```javascript
		* Jodit.make('#editor', {
		* 		events: {
		* 	  	afterInit: () => console.log('afterInit')
		* 		}
		* });
		* ```
		* The option can use any Jodit events, for example:
		* ```javascript
		* const editor = Jodit.make('#editor', {
		* 		events: {
		* 			hello: (name) => console.log('Hello', name)
		* 		}
		* });
		* editor.e.fire('hello', 'Mike');
		* ```
		*/
		this.events = {};
		/**
		* Buttons in toolbar without SVG - only texts
		*/
		this.textIcons = false;
		/**
		* Element for dialog container
		*/
		this.popupRoot = null;
		/**
		* shows a INPUT[type=color] to open the browser color picker, on the right bottom of widget color picker
		*/
		this.showBrowserColorPicker = true;
		Object.assign(this, ConfigPrototype);
	}
	static get defaultOptions() {
		if (!Config.__defaultOptions) Config.__defaultOptions = new Config();
		return Config.__defaultOptions;
	}
};
ConfigPrototype = Config.prototype;
Config.prototype.controls = {};
//#endregion
//#region node_modules/jodit/esm/core/helpers/utils/extend.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/utils
*/
function isAtom(obj) {
	return obj && obj.isAtom;
}
function markAsAtomic(obj) {
	Object.defineProperty(obj, "isAtom", {
		enumerable: false,
		value: true,
		configurable: false
	});
	return obj;
}
function fastClone(object) {
	return JSON.parse(stringify(object));
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/utils/is-unsafe-proto-key.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/utils
*/
/**
* Keys that must never be written from a (potentially untrusted) source —
* assigning them while walking/merging an object can reach and mutate
* `Object.prototype` (prototype pollution, CWE-1321).
*/
var UNSAFE_PROTO_KEYS = [
	"__proto__",
	"constructor",
	"prototype"
];
/**
* Check whether a key can be used to pollute the prototype chain.
*/
function isUnsafeProtoKey(key) {
	return UNSAFE_PROTO_KEYS.indexOf(key) !== -1;
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/utils/config-proto.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @example
* ```js
* const defaultConfig = {
*   a: {
*     b: {
*       c: 2
*     },
*     e: 5
*   },
*   d: {
*     g: 7
*   }
* };
*
* const options = ConfigProto({a: {
*   b: {
*     c: 1
*   }
* }}, defaultConfig);
*
* console.log(options.a.b.c); // 1
* console.log(options.a.e); // 5
* console.log(options.d.g); // 7
*
* defaultConfig.d.g  = 8;
* console.log(options.d.g); // 8
*
* ```
*/
function ConfigProto(options, proto, deep = 0) {
	if (Object.getPrototypeOf(options) !== Object.prototype) return options;
	const def = Config.defaultOptions;
	if (isString(options.preset)) {
		if (def.presets[options.preset] !== void 0) {
			const preset = def.presets[options.preset];
			Object.keys(preset).forEach((subKey) => {
				if (isVoid(options[subKey])) options[subKey] = preset[subKey];
			});
		}
		delete options.preset;
	}
	const newOpt = {};
	Object.keys(options).forEach((key) => {
		if (isUnsafeProtoKey(key)) return;
		const opt = options[key], protoKey = proto ? proto[key] : null;
		if (isPlainObject(opt) && isPlainObject(protoKey) && !isAtom(opt)) {
			newOpt[key] = ConfigProto(opt, protoKey, deep + 1);
			return;
		}
		if (deep !== 0 && isArray(opt) && !isAtom(opt) && isArray(protoKey)) {
			newOpt[key] = [...opt, ...protoKey.slice(opt.length)];
			return;
		}
		newOpt[key] = opt;
	});
	Object.setPrototypeOf(newOpt, proto);
	return newOpt;
}
function ConfigFlatten(obj) {
	return keys(obj, false).reduce((app, key) => {
		app[key] = obj[key];
		return app;
	}, {});
}
/**
* Returns a plain object from a prototype-based object.
* ```typescript
* const editor = Jodit.make('#editor', {
*   image: {
*     dialogWidth: 500
*   }
* });
*
* console.log(editor.o.image.openOnDblClick) // true
* // But you can't get all options in plain object
* console.log(JSON.stringify(editor.o.image)); // {"dialogWidth":500}
*
* const plain = Jodit.modules.Helpers.ConfigDeepFlatten(editor.o.image);
* console.log(JSON.stringify(plain)); // {"dialogWidth":500, "openOnDblClick": true, "editSrc": true, ...}
* ```
*/
/**
* Deep-merges `source` into `target` in-place.
* Uses the same merge semantics as {@link ConfigProto}:
* - Nested plain objects are merged recursively
* - {@link isAtom | Atomic} values replace the target entirely
* - Everything else (primitives, arrays, class instances) replaces the target value
*
* Designed for patching `Config.defaultOptions` without losing existing keys:
*
* ```js
* Jodit.configure({
*   controls: {
*     someButton: { group: 'custom' }
*   }
* });
* // Only `controls.someButton` is touched — all other controls remain intact.
* ```
*
* @see {@link ConfigProto} for the prototype-chain variant used at editor creation time
*/
function ConfigMerge(target, source) {
	Object.keys(source).forEach((key) => {
		if (isUnsafeProtoKey(key)) return;
		const srcVal = source[key];
		const tgtVal = target[key];
		if (isPlainObject(srcVal) && isPlainObject(tgtVal) && !isAtom(srcVal)) ConfigMerge(tgtVal, srcVal);
		else target[key] = srcVal;
	});
}
function ConfigDeepFlatten(obj) {
	return keys(obj, false).reduce((app, key) => {
		app[key] = isPlainObject(obj[key]) ? ConfigDeepFlatten(obj[key]) : obj[key];
		return app;
	}, {});
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/utils/parse-query.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Parse query string
*/
var parseQuery = (queryString) => {
	const query = {}, a = queryString.substring(1).split("&");
	for (let i = 0; i < a.length; i += 1) {
		const keyValue = a[i].split("=");
		query[decodeURIComponent(keyValue[0])] = decodeURIComponent(keyValue[1] || "");
	}
	return query;
};
//#endregion
//#region node_modules/jodit/esm/core/helpers/utils/convert-media-url-to-video-embed.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/utils
*/
/**
* Javascript url pattern converter replace youtube/vimeo url in embed code.
*/
var convertMediaUrlToVideoEmbed = (url, { width = 400, height = 345 } = {}) => {
	if (!isURL(url)) return url;
	const parser = globalDocument.createElement("a");
	parser.href = url;
	if (!width) width = 400;
	if (!height) height = 345;
	const protocol = parser.protocol || "";
	switch (parser.hostname) {
		case "www.vimeo.com":
		case "vimeo.com": {
			const segments = parser.pathname.split("/").filter(Boolean);
			const idIndex = segments.findIndex((s) => /^\d+$/.test(s));
			if (idIndex === -1) return url;
			let path = segments[idIndex];
			const hash = segments[idIndex + 1];
			if (hash && idIndex === 0) path += "/" + hash;
			return "<iframe width=\"" + width + "\" height=\"" + height + "\" src=\"" + protocol + "//player.vimeo.com/video/" + path + "\" frameborder=\"0\" allowfullscreen></iframe>";
		}
		case "youtube.com":
		case "www.youtube.com":
		case "m.youtube.com":
		case "music.youtube.com":
		case "youtu.be":
		case "www.youtu.be": {
			let v = (parser.search ? parseQuery(parser.search) : {}).v || parser.pathname.substring(1);
			v = v.replace(/^(watch|embed|shorts|live|v)\//, "").replace(/\/$/, "");
			return v ? "<iframe width=\"" + width + "\" height=\"" + height + "\" src=\"" + protocol + "//www.youtube.com/embed/" + v + "\" frameborder=\"0\" allowfullscreen></iframe>" : url;
		}
	}
	return url;
};
//#endregion
//#region node_modules/jodit/esm/core/helpers/utils/ctrl-key.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/utils
*/
/**
* CTRL pressed
* @returns true ctrl key was pressed
*/
var ctrlKey = (e) => {
	if (typeof navigator !== "undefined" && navigator.userAgent.indexOf("Mac OS X") !== -1) {
		if (e.metaKey && !e.altKey) return true;
	} else if (e.ctrlKey && !e.altKey) return true;
	return false;
};
//#endregion
//#region node_modules/jodit/esm/core/helpers/utils/default-language.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/utils
*/
/**
* Try define user language
*/
var defaultLanguage = (language, defaultLanguage = "en") => {
	if (language !== "auto" && isString(language)) return language;
	if (globalDocument.documentElement && globalDocument.documentElement.lang) return globalDocument.documentElement.lang;
	if (navigator.language) return navigator.language.substring(0, 2);
	return defaultLanguage;
};
//#endregion
//#region node_modules/jodit/esm/core/helpers/utils/human-size-to-bytes.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/utils
*/
/**
* Converts from human readable file size (kb,mb,gb,tb) to bytes
* @param human - readable file size. Example 1gb or 11.2mb
*/
var humanSizeToBytes = (human) => {
	if (/^[0-9.]+$/.test(human.toString())) return parseFloat(human);
	const format = human.substr(-2, 2).toUpperCase(), formats = [
		"KB",
		"MB",
		"GB",
		"TB"
	], number = parseFloat(human.substr(0, human.length - 2));
	return formats.indexOf(format) !== -1 ? number * Math.pow(1024, formats.indexOf(format) + 1) : parseInt(human, 10);
};
//#endregion
//#region node_modules/jodit/esm/core/helpers/utils/mark-deprecated.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/utils
*/
/**
* By default, terser will remove all `console.*` but
* if you use this object it will not be
*/
var cns = console;
/**
* Mark function as deprecated
*/
function markDeprecated(method, names = [""], ctx = null) {
	return (...args) => {
		cns.warn(`Method "${names[0]}" deprecated.` + (names[1] ? ` Use "${names[1]}" instead` : ""));
		return method.call(ctx, ...args);
	};
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/utils/print.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Fixes image sizes and sets absolute paths to images
*/
function fixedAssetsSizeAndAbsoluteLinks(editor, points) {
	const restoreAttributes = [];
	const images = [];
	Dom.each(editor.editor, (node) => {
		Dom.isTag(node, "img") && images.push(node);
	});
	try {
		images.forEach((item) => {
			const previousAttrs = [
				attr(item, "width"),
				attr(item, "height"),
				attr(item, "src")
			];
			attr(item, {
				width: item.offsetWidth + points,
				height: item.offsetHeight + points
			});
			const a = editor.createInside.a();
			Dom.append(editor.ed.body, a);
			attr(a, "href", attr(item, "src") || "");
			attr(item, "src", a.href);
			Dom.safeRemove(a);
			restoreAttributes.push(() => {
				attr(item, {
					src: previousAttrs[2],
					width: previousAttrs[0] || null,
					height: previousAttrs[1] || null
				});
			});
		});
	} catch (e) {
		restoreAttributes.forEach((c) => c());
		restoreAttributes.length = 0;
		throw e;
	}
	return restoreAttributes;
}
/**
* Generates a copy of an HTML document, resizes images, executes JS
*
* @event beforePreviewBox(string | undefined, 'pt' | 'px' | '')
* @event afterPreviewBox(HTMLElement)
*/
function previewBox(editor, defaultValue, points = "px", container = null) {
	const onDestruct = [];
	const restoreAttributes = fixedAssetsSizeAndAbsoluteLinks(editor, points);
	try {
		const res = editor.e.fire("beforePreviewBox", defaultValue, points);
		if (res != null) return res;
		let div = editor.c.div("jodit__preview-box jodit-context");
		if (container) Dom.append(container, div);
		css(div, {
			position: "relative",
			padding: 16
		});
		const value = editor.value || `<div style='position: absolute;left:50%;top:50%;transform: translateX(-50%) translateY(-50%);color:#ccc;'>${editor.i18n("Empty")}</div>`;
		if (editor.iframe) {
			const iframe = editor.create.element("iframe");
			css(iframe, {
				minWidth: 800,
				minHeight: 600,
				border: 0
			});
			Dom.append(div, iframe);
			const myWindow = iframe.contentWindow;
			if (myWindow) {
				editor.e.fire("generateDocumentStructure.iframe", myWindow.document, editor);
				div = myWindow.document.body;
				if (typeof ResizeObserver === "function") {
					let destructed = false;
					const elm = myWindow.document.body;
					const resizeObserver = new ResizeObserver(editor.async.debounce(() => {
						resizeObserver.unobserve(elm);
						css(iframe, "height", elm.offsetHeight + 20);
						editor.async.requestAnimationFrame(() => {
							!destructed && resizeObserver.observe(elm);
						});
					}, 100));
					const beforeDestruct = () => {
						destructed = true;
						resizeObserver.unobserve(elm);
						resizeObserver.disconnect();
						editor.e.off("beforeDestruct", beforeDestruct);
					};
					onDestruct.push(beforeDestruct);
					editor.e.on("beforeDestruct", beforeDestruct);
				}
			}
		} else css(div, {
			minWidth: 1024,
			minHeight: 600,
			border: 0
		});
		const setHTML = (box, value) => {
			const dv = isString(value) ? editor.c.div() : value;
			if (isString(value)) dv.innerHTML = value;
			for (let i = 0; i < dv.childNodes.length; i += 1) {
				const c = dv.childNodes[i];
				if (Dom.isElement(c)) {
					const newNode = box.ownerDocument.createElement(c.nodeName);
					for (let j = 0; j < c.attributes.length; j += 1) attr(newNode, c.attributes[j].name, c.attributes[j].value);
					if (c.childNodes.length === 0 || Dom.isTag(c, "table")) switch (c.nodeName) {
						case "SCRIPT":
							if (c.textContent) newNode.textContent = c.textContent;
							break;
						default:
							if (c.innerHTML) newNode.innerHTML = c.innerHTML;
							break;
					}
					else setHTML(newNode, c);
					try {
						Dom.append(box, newNode);
					} catch (_a) {}
				} else try {
					Dom.append(box, c.cloneNode(true));
				} catch (_b) {}
			}
		};
		setHTML(div, value);
		editor.e.fire("afterPreviewBox", div);
		return [div, () => {
			onDestruct.forEach((cb) => cb());
		}];
	} finally {
		restoreAttributes.forEach((clb) => clb());
	}
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/utils/scroll-into-view.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/utils
*/
/**
* `getBoundingClientRect` returns fractional values while `scrollTop`,
* `offsetTop` and `clientHeight` are rounded to integers, so a container
* scrolled right up to its limit can still appear to clip the element by a
* fraction of a pixel. Treat anything clipped by less than a pixel as
* visible — otherwise `scrollIntoViewIfNeeded` falls through to
* `elm.scrollIntoView()` and needlessly scrolls the whole page.
*/
var SUBPIXEL_TOLERANCE = 1;
/**
* Check if element is in view
*/
function inView(elm, root, doc) {
	let rect = elm.getBoundingClientRect(), el = elm;
	const top = rect.top, height = rect.height;
	while (el && el !== root && el.parentNode) {
		el = el.parentNode;
		rect = el.getBoundingClientRect();
		if (top + height > rect.bottom + SUBPIXEL_TOLERANCE && top > rect.top) return false;
		if (top + height <= rect.top) return false;
	}
	const clientHeight = doc.documentElement && doc.documentElement.clientHeight || 0;
	return (top + height <= clientHeight + SUBPIXEL_TOLERANCE || top <= 0) && top + height >= 0;
}
/**
* Scroll element into view if it is not in view
*/
function scrollIntoViewIfNeeded(elm, root, doc) {
	if (Dom.isHTMLElement(elm) && !inView(elm, root, doc)) {
		if (root.clientHeight !== root.scrollHeight) {
			const alignBottom = elm.offsetTop + elm.offsetHeight - root.clientHeight;
			root.scrollTop = root.scrollTop < alignBottom ? alignBottom : elm.offsetTop;
		}
		if (!inView(elm, root, doc)) elm.scrollIntoView({ block: "nearest" });
	}
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/utils/set.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Safe access in tree object
*
* @example
* ```js
* const a = {}, b = {};
* Jodit.modules.Helpers.set('a.b.c.d.e', 1, a);
* console.log(a);// {a: {b: {c: {d: {e: 1}}}}}
*
* Jodit.modules.Helpers.set('a.0.e', 1, b);
* console.log(b);// {a: [{e: 1}]}
* ```
*/
function set(chain, value, obj) {
	if (!isString(chain) || !chain.length) return;
	const parts = chain.split(".");
	if (parts.some(isUnsafeProtoKey)) return;
	let result = obj, key = parts[0];
	for (let i = 0; i < parts.length - 1; i += 1) {
		key = parts[i];
		if (!isArray(result[key]) && !isPlainObject(result[key])) result[key] = isNumeric(parts[i + 1]) ? [] : {};
		result = result[key];
	}
	if (result) result[parts[parts.length - 1]] = value;
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/utils/stack.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var LimitedStack = class {
	constructor(limit) {
		this.limit = limit;
		this.stack = [];
	}
	push(item) {
		this.stack.push(item);
		if (this.stack.length > this.limit) this.stack.shift();
		return this;
	}
	pop() {
		return this.stack.pop();
	}
	find(clb) {
		return this.stack.find(clb);
	}
};
//#endregion
//#region node_modules/jodit/esm/core/helpers/utils/index.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
//#endregion
//#region node_modules/jodit/esm/core/helpers/html/safe-html.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var HTML_NAMESPACE = "http://www.w3.org/1999/xhtml";
/**
* Integration points where HTML is legitimately allowed inside MathML/SVG foreign content.
*/
var HTML_INTEGRATION_POINTS = /* @__PURE__ */ new Set([
	"foreignobject",
	"annotation-xml",
	"desc",
	"title"
]);
/**
* True for an HTML element the parser placed inside MathML/SVG outside an integration point - smuggled
* HTML (e.g. `mglyph` / `style` under `<math>`) that a reparse can hoist into a live node. Legitimate
* MathML/SVG children and HTML below an integration point are kept.
*/
var isMathOrSvg = (node) => Boolean(node && /^(math|svg)$/i.test(node.nodeName));
function isSmuggledForeignHtml(elm) {
	if (elm.namespaceURI !== HTML_NAMESPACE || Dom.up(elm, isMathOrSvg) == null) return false;
	for (let parent = elm.parentElement; parent; parent = parent.parentElement) {
		const name = parent.nodeName.toLowerCase();
		if (name === "math" || name === "svg") break;
		if (HTML_INTEGRATION_POINTS.has(name)) return false;
	}
	return true;
}
/**
* Removes dangerous constructs from HTML
*/
function safeHTML(box, options) {
	var _a;
	if (!Dom.isElement(box) && !Dom.isFragment(box)) return;
	const foreign = [];
	Dom.each(box, (node) => {
		if (Dom.isElement(node) && Dom.up(node.parentNode, isMathOrSvg)) foreign.push(node);
	});
	for (const elm of foreign) if (Dom.isOrContains(box, elm) && isSmuggledForeignHtml(elm)) Dom.safeRemove(elm);
	const removeEvents = (_a = options.removeEventAttributes) !== null && _a !== void 0 ? _a : options.removeOnError;
	const process = (node) => {
		if (!Dom.isElement(node)) return;
		if (removeEvents) removeAllEventAttributes(node);
		sanitizeHTMLElement(node, options);
		if (options.safeLinksTarget && node.nodeName === "A" && attr(node, "target") === "_blank") {
			const parts = (attr(node, "rel") || "").split(/\s+/).filter(Boolean);
			if (!parts.includes("noopener")) parts.push("noopener");
			if (!parts.includes("noreferrer")) parts.push("noreferrer");
			attr(node, "rel", parts.join(" "));
		}
	};
	process(box);
	Dom.each(box, process);
}
/**
* Remove all on* event handler attributes from an element
*/
function removeAllEventAttributes(elm) {
	if (!Dom.isElement(elm)) return false;
	let effected = false;
	const toRemove = [];
	for (let i = 0; i < elm.attributes.length; i++) if (elm.attributes[i].name.toLowerCase().startsWith("on")) toRemove.push(elm.attributes[i].name);
	for (const name of toRemove) {
		attrRaw(elm, name, null);
		effected = true;
	}
	return effected;
}
/**
* URL-bearing attributes (besides `href`) that can load or execute content.
*/
var URL_ATTRIBUTES = [
	"src",
	"data",
	"action",
	"formaction",
	"poster",
	"background",
	"xlink:href"
];
/**
* Tags that load their URL as a *document* (scripts inside run). An SVG data
* URL is only an XSS vector here — as an `<img>` source it renders inertly.
*/
var DOCUMENT_EMBED_TAGS = /* @__PURE__ */ new Set([
	"iframe",
	"frame",
	"object",
	"embed"
]);
/**
* Detects executable / script-bearing URL schemes. The attribute value is
* already HTML-entity-decoded by `getAttribute`, so only whitespace and
* control characters (which browsers ignore inside a scheme) need stripping.
*/
function isDangerousUrl(value, tagName) {
	const normalized = value.replace(/[\u0000-\u0020]+/g, "").toLowerCase();
	if (/^(?:javascript|vbscript|livescript|mocha):/.test(normalized)) return true;
	if (/^data:(?:text\/html|application\/xhtml)/.test(normalized)) return true;
	return /^data:image\/svg/.test(normalized) && DOCUMENT_EMBED_TAGS.has(tagName);
}
function sanitizeHTMLElement(elm, { safeJavaScriptLink, removeOnError } = {
	safeJavaScriptLink: true,
	removeOnError: true
}) {
	if (!Dom.isElement(elm)) return false;
	let effected = false;
	if (removeOnError && elm.hasAttribute("onerror")) {
		attr(elm, "onerror", null);
		effected = true;
	}
	const tagName = elm.nodeName.toLowerCase();
	const href = attr(elm, "href");
	if (safeJavaScriptLink && href && isDangerousUrl(href, tagName)) {
		attr(elm, "href", (typeof location !== "undefined" ? location.protocol : "http:") + "//" + href);
		effected = true;
	}
	if (safeJavaScriptLink) {
		if (elm.hasAttribute("srcdoc")) {
			attr(elm, "srcdoc", null);
			effected = true;
		}
		for (const name of URL_ATTRIBUTES) {
			const value = attrRaw(elm, name);
			if (value && isDangerousUrl(value, tagName)) {
				attr(elm, name, null);
				effected = true;
			}
		}
	}
	return effected;
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/html/strip-tags.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var NEW_LINE_TAGS = /* @__PURE__ */ new Set([
	"div",
	"p",
	"br",
	"h1",
	"h2",
	"h3",
	"h4",
	"h5",
	"h6",
	"hr"
]);
var INVISIBLE_TAGS = /* @__PURE__ */ new Set(["script", "style"]);
var ALONE_TAGS = /* @__PURE__ */ new Set([
	"br",
	"hr",
	"input"
]);
/**
* Extract plain text from HTML text
*/
function stripTags(html, doc = document, exclude = null, blockBr = false) {
	const tmp = doc.createElement("div");
	if (isString(html)) tmp.innerHTML = html;
	else Dom.append(tmp, html);
	const all = [];
	Dom.each(tmp, (node) => {
		Dom.isElement(node) && all.push(node);
	});
	all.forEach((p) => {
		if (!p.parentNode) return;
		if (exclude && Dom.isTag(p, exclude)) {
			const tag = p.nodeName.toLowerCase();
			const text = !Dom.isTag(p, ALONE_TAGS) ? `%%%jodit-${tag}%%%${stripTags(p.innerHTML, doc, exclude, blockBr)}%%%/jodit-${tag}%%%` : `%%%jodit-single-${tag}%%%`;
			Dom.before(p, doc.createTextNode(text));
			Dom.safeRemove(p);
			return;
		}
		if (Dom.isTag(p, INVISIBLE_TAGS)) {
			Dom.safeRemove(p);
			return;
		}
		if (!Dom.isTag(p, NEW_LINE_TAGS)) return;
		const nx = p.nextSibling;
		if (Dom.isText(nx) && /^\s/.test(nx.nodeValue || "")) return;
		if (nx) Dom.before(nx, doc.createTextNode(blockBr ? "%%%jodit-single-br%%%" : " "));
	});
	return restoreTags(trim(tmp.innerText));
}
function restoreTags(content) {
	return content.replace(/%%%(\/)?jodit(-single)?-([\w\n]+)%%%/g, (_, isClosed, isSingle, tag) => `<${isClosed ? "/" : ""}${tag}>`);
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/html/index.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
//#endregion
//#region node_modules/jodit/esm/core/helpers/normalize/normalize-color.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/normalize
*/
/**
* Convert rgba and short HEX color to Full text color. #fff to #FFFFFF
*
* @param colorInput - string like rgba(red, green, blue, alpha) or rgb(red, green, blue) or #fff or #ffffff
* @returns HEX color, false - for transparent color
*/
var normalizeColor = (colorInput) => {
	const newcolor = ["#"];
	let color = colorToHex(colorInput);
	if (!color) return false;
	color = trim(color.toUpperCase());
	color = color.substring(1);
	if (color.length === 3) {
		for (let i = 0; i < 3; i += 1) {
			newcolor.push(color[i]);
			newcolor.push(color[i]);
		}
		return newcolor.join("");
	}
	if (color.length > 6) color = color.slice(0, 6);
	return "#" + color;
};
//#endregion
//#region node_modules/jodit/esm/core/helpers/normalize/normalize-key-aliases.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Normalize keys to some standard name
*/
function normalizeKeyAliases(keys) {
	const memory = {};
	const order = {
		meta: 1,
		ctrl: 2,
		control: 2,
		alt: 3,
		shift: 4,
		space: 5
	};
	return keys.replace(/\+\+/g, "+add").split(/[\s]*\+[\s]*/).map((key) => trim(key.toLowerCase())).map((key) => KEY_ALIASES[key] || key).sort((a, b) => {
		if (order[a] && !order[b]) return -1;
		if (!order[a] && order[b]) return 1;
		if (order[a] && order[b]) return order[a] - order[b];
		return a > b ? 1 : -1;
	}).filter((key) => !memory[key] && key !== "" && (memory[key] = true)).join("+");
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/normalize/normalize-license.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/normalize
*/
var normalizeLicense = (license, count = 8) => {
	const parts = [];
	while (license.length) {
		parts.push(license.substr(0, count));
		license = license.substr(count);
	}
	parts[1] = parts[1].replace(/./g, "*");
	parts[2] = parts[2].replace(/./g, "*");
	return parts.join("-");
};
//#endregion
//#region node_modules/jodit/esm/core/helpers/normalize/normalize-path.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/normalize
*/
/**
* Replaces back slashes and correctly concatenates several parts of the path.
*/
var normalizePath = (...path) => {
	return path.filter((part) => trim(part).length).map((part, index) => {
		part = part.replace(/([^:])[\\/]+/g, "$1/");
		if (index) part = part.replace(/^\//, "");
		if (index !== path.length - 1) part = part.replace(/\/$/, "");
		return part;
	}).join("/");
};
//#endregion
//#region node_modules/jodit/esm/core/helpers/normalize/normalize-relative-path.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/normalize
*/
var normalizeRelativePath = (path) => {
	return path.split("/").reduce((builder, section) => {
		switch (section) {
			case "": break;
			case ".": break;
			case "..":
				builder.pop();
				break;
			default:
				builder.push(section);
				break;
		}
		return builder;
	}, []).join("/") + (path.endsWith("/") ? "/" : "");
};
//#endregion
//#region node_modules/jodit/esm/core/helpers/normalize/normalize-size.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/normalize
*/
/**
* Normalize value to CSS meters
*/
var normalizeSize = (value, units) => {
	if (/^[0-9]+$/.test(value.toString())) return value + units;
	return value.toString();
};
//#endregion
//#region node_modules/jodit/esm/core/helpers/normalize/normalize-url.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/normalize
*/
var normalizeUrl = (...urls) => {
	return urls.filter((url) => url.length).map((url) => url.replace(/\/$/, "")).join("/").replace(/([^:])[\\/]+/g, "$1/");
};
//#endregion
//#region node_modules/jodit/esm/core/helpers/normalize/index.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
//#endregion
//#region node_modules/jodit/esm/core/helpers/size/get-content-width.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/size
*/
var getContentWidth = (element, win) => {
	const pi = (value) => parseInt(value, 10), style = win.getComputedStyle(element), width = element.offsetWidth, paddingLeft = pi(style.getPropertyValue("padding-left") || "0"), paddingRight = pi(style.getPropertyValue("padding-right") || "0");
	return width - paddingLeft - paddingRight;
};
//#endregion
//#region node_modules/jodit/esm/core/helpers/size/get-fixed-position-offset.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Whether the element establishes a containing block for its
* `position: fixed` descendants, so their coordinates become relative to it
* instead of the viewport.
*/
function isContainingBlockForFixed(style) {
	return style.transform !== "" && style.transform !== "none" || style.perspective !== "" && style.perspective !== "none" || style.filter !== "" && style.filter !== "none" || style.willChange === "transform" || style.willChange === "perspective" || style.willChange === "filter" || style.contain === "paint" || style.contain === "layout" || style.contain === "strict" || style.contain === "content";
}
/**
* Returns the viewport offset of the containing block of a `position: fixed`
* descendant of `elm`. A fixed element is normally positioned relative to the
* viewport, but an ancestor with `transform`, `filter`, `perspective`, etc.
* establishes a new containing block, shifting the fixed element by that
* ancestor's top-left corner. The returned offset should be subtracted from
* the desired viewport coordinates before applying them.
*
* Returns `{ x: 0, y: 0 }` when no such ancestor exists (the common case), so
* call sites keep their previous behaviour unchanged.
*/
function getFixedPositionOffset(elm) {
	var _a;
	const win = (_a = elm.ownerDocument) === null || _a === void 0 ? void 0 : _a.defaultView;
	let node = elm.parentElement;
	while (win && node) {
		if (isContainingBlockForFixed(win.getComputedStyle(node))) {
			const rect = node.getBoundingClientRect();
			return {
				x: rect.left,
				y: rect.top
			};
		}
		node = node.parentElement;
	}
	return {
		x: 0,
		y: 0
	};
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/size/get-scroll-parent.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
function getScrollParent(node) {
	if (!node) return null;
	const isElement = Dom.isHTMLElement(node);
	const overflowY = isElement && css(node, "overflowY");
	if (isElement && overflowY !== "visible" && overflowY !== "hidden" && node.scrollHeight >= node.clientHeight) return node;
	return getScrollParent(node.parentNode) || globalDocument.scrollingElement || globalDocument.body;
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/size/inner-width.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/size
*/
var innerWidth = (element, win) => {
	const computedStyle = win.getComputedStyle(element);
	let elementWidth = element.clientWidth;
	elementWidth -= parseFloat(computedStyle.paddingLeft || "0") + parseFloat(computedStyle.paddingRight || "0");
	return elementWidth;
};
//#endregion
//#region node_modules/jodit/esm/core/helpers/size/object-size.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
function size$1(subject) {
	if (isString(subject) || isArray(subject)) return subject.length;
	if (isPlainObject(subject)) return Object.keys(subject).length;
	return 0;
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/size/offset.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var offset = (elm, jodit, doc, recurse = false) => {
	let rect;
	try {
		rect = elm.getBoundingClientRect();
	} catch (e) {
		rect = {
			top: 0,
			bottom: 0,
			left: 0,
			right: 0,
			width: 0,
			height: 0
		};
	}
	const body = doc.body, docElem = doc.documentElement || {
		clientTop: 0,
		clientLeft: 0,
		scrollTop: 0,
		scrollLeft: 0
	}, win = doc.defaultView || doc.parentWindow, scrollTop = win.pageYOffset || docElem.scrollTop || body.scrollTop, scrollLeft = win.pageXOffset || docElem.scrollLeft || body.scrollLeft, clientTop = docElem.clientTop || body.clientTop || 0, clientLeft = docElem.clientLeft || body.clientLeft || 0;
	let topValue, leftValue;
	const iframe = jodit.iframe;
	if (!recurse && jodit && jodit.options && jodit.o.iframe && iframe) {
		const { top, left } = offset(iframe, jodit, jodit.od, true);
		topValue = rect.top + top;
		leftValue = rect.left + left;
	} else {
		topValue = rect.top + scrollTop - clientTop;
		leftValue = rect.left + scrollLeft - clientLeft;
	}
	return {
		top: Math.round(topValue),
		left: Math.round(leftValue),
		width: rect.width,
		height: rect.height
	};
};
//#endregion
//#region node_modules/jodit/esm/core/helpers/size/position.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Calculate screen element position
*/
function position(elm, jodit, recurse = false) {
	var _a, _b;
	const rect = elm.getBoundingClientRect();
	let xPos = rect.left, yPos = rect.top;
	if (isJoditObject(jodit) && jodit.iframe && jodit.ed.body.contains(elm) && !recurse) {
		const { left, top } = position(jodit.iframe, jodit, true);
		xPos += left;
		yPos += top;
	}
	return {
		left: Math.round(xPos),
		top: Math.round(yPos),
		width: Math.round((_a = elm.offsetWidth) !== null && _a !== void 0 ? _a : rect.width),
		height: Math.round((_b = elm.offsetHeight) !== null && _b !== void 0 ? _b : rect.height)
	};
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/size/index.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
//#endregion
//#region node_modules/jodit/esm/core/helpers/string/fuzzy-search-index.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/string
*/
/**
* Finds the position of the substring in the string, if any, and returns the length of the found subsequence.
* Unlike `indexOf` ignores INVISIBLE_SPACE and may fail at `maxDistance` characters
*/
function fuzzySearchIndex(needle, haystack, offset = 0, maxDistance = 1) {
	let i = 0, j = 0, startIndex = -1, len = 0, errorDistance = 0;
	for (j = offset; i < needle.length && j < haystack.length;) {
		if (needle[i].toLowerCase() === haystack[j].toLowerCase()) {
			i++;
			len++;
			errorDistance = 0;
			if (startIndex === -1) startIndex = j;
		} else if (i > 0) if (errorDistance >= maxDistance && haystack[j] !== "﻿") {
			i = 0;
			startIndex = -1;
			len = 0;
			errorDistance = 0;
			j--;
		} else {
			errorDistance++;
			len++;
		}
		j++;
	}
	return i === needle.length ? [startIndex, len] : [-1, 0];
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/string/ucfirst.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module helpers/string
*/
/**
* Make a string's first character uppercase
*/
function ucfirst(value) {
	if (!value.length) return "";
	return value[0].toUpperCase() + value.substring(1);
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/string/i18n.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Simple variant sprintf function
*/
var sprintf = (str, args) => {
	if (!args || !args.length) return str;
	const reg = /%([sd])/g;
	let fnd = reg.exec(str);
	let res = str, i = 0;
	while (fnd && args[i] !== void 0) {
		res = res.replace(fnd[0], args[i].toString());
		i += 1;
		fnd = reg.exec(str);
	}
	return res;
};
/**
* Internationalization method. Uses Jodit.lang object
* @example
* ```javascript
* var editor = Jodit.make("#redactor", {
*      language: 'ru'
* });
* console.log(editor.i18n('Cancel')) //Отмена;
*
* Jodit.defaultOptions.language = 'ru';
* console.log(Jodit.prototype.i18n('Cancel')) //Отмена
*
* Jodit.lang.cs = {
*    Cancel: 'Zrušit'
* };
* Jodit.defaultOptions.language = 'cs';
* console.log(Jodit.prototype.i18n('Cancel')) //Zrušit
*
* Jodit.lang.cs = {
*    'Hello world': 'Hello \s Good \s'
* };
* Jodit.defaultOptions.language = 'cs';
* console.log(Jodit.prototype.i18n('Hello world', 'mr.Perkins', 'day')) //Hello mr.Perkins Good day
* ```
*/
function i18n(key, params, options) {
	if (!isString(key)) throw error("i18n: Need string in first argument");
	if (!key.length) return key;
	const debug = Boolean(options === null || options === void 0 ? void 0 : options.debugLanguage);
	let store = {};
	const parse = (value) => params && params.length ? sprintf(value, params) : value, defaultLanguage$1 = defaultLanguage(Config.defaultOptions.language, Config.defaultOptions.language), language = defaultLanguage(options === null || options === void 0 ? void 0 : options.language, defaultLanguage$1), tryGet = (store) => {
		if (!store) return;
		if (isString(store[key])) return parse(store[key]);
		const lcKey = key.toLowerCase();
		if (isString(store[lcKey])) return parse(store[lcKey]);
		const ucfKey = ucfirst(key);
		if (isString(store[ucfKey])) return parse(store[ucfKey]);
	};
	if (lang[language] !== void 0) store = lang[language];
	else if (!debug) if (lang[defaultLanguage$1] !== void 0) store = lang[defaultLanguage$1];
	else store = lang.en;
	const i18nOvr = options === null || options === void 0 ? void 0 : options.i18n;
	if (i18nOvr && i18nOvr[language]) {
		const result = tryGet(i18nOvr[language]);
		if (result) return result;
	}
	const result = tryGet(store);
	if (result) return result;
	if (!debug && lang.en && isString(lang.en[key]) && lang.en[key]) return parse(lang.en[key]);
	if (debug) return "{" + key + "}";
	return parse(key);
}
//#endregion
//#region node_modules/jodit/esm/core/helpers/string/index.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
//#endregion
//#region node_modules/jodit/esm/core/helpers/index.js
var helpers_exports = /* @__PURE__ */ __exportAll({
	$$: () => $$,
	CamelCaseToKebabCase: () => CamelCaseToKebabCase,
	ConfigDeepFlatten: () => ConfigDeepFlatten,
	ConfigFlatten: () => ConfigFlatten,
	ConfigMerge: () => ConfigMerge,
	ConfigProto: () => ConfigProto,
	ConnectionError: () => ConnectionError,
	LimitedStack: () => LimitedStack,
	NUMBER_FIELDS_REG: () => NUMBER_FIELDS_REG,
	OptionsError: () => OptionsError,
	UNSAFE_PROTO_KEYS: () => UNSAFE_PROTO_KEYS,
	abort: () => abort,
	alignElement: () => alignElement,
	alreadyLoadedList: () => alreadyLoadedList,
	appendScriptAsync: () => appendScriptAsync,
	appendStyleAsync: () => appendStyleAsync,
	applyStyles: () => applyStyles,
	asArray: () => asArray,
	assert: () => assert,
	attr: () => attr,
	attrRaw: () => attrRaw,
	browser: () => browser,
	buildQuery: () => buildQuery,
	call: () => call,
	callPromise: () => callPromise,
	callThis: () => callThis,
	camelCase: () => camelCase,
	cleanFromWord: () => cleanFromWord,
	clearAlign: () => clearAlign,
	clearCenterAlign: () => clearCenterAlign,
	clearTimeout: () => clearTimeout,
	cns: () => cns,
	colorToHex: () => colorToHex,
	completeUrl: () => completeUrl,
	connection: () => connection,
	convertMediaUrlToVideoEmbed: () => convertMediaUrlToVideoEmbed,
	css: () => css,
	cssInline: () => cssInline,
	cssPath: () => cssPath,
	ctrlKey: () => ctrlKey,
	dataBind: () => dataBind,
	defaultLanguage: () => defaultLanguage,
	error: () => error,
	fastClone: () => fastClone,
	fuzzySearchIndex: () => fuzzySearchIndex,
	get: () => get$1,
	getClassName: () => getClassName,
	getContentWidth: () => getContentWidth,
	getDataTransfer: () => getDataTransfer,
	getFixedPositionOffset: () => getFixedPositionOffset,
	getPropertyDescriptor: () => getPropertyDescriptor,
	getScrollParent: () => getScrollParent,
	getXPathByElement: () => getXPathByElement,
	hAlignElement: () => hAlignElement,
	hasBrowserColorPicker: () => hasBrowserColorPicker,
	hasContainer: () => hasContainer,
	htmlspecialchars: () => htmlspecialchars,
	humanSizeToBytes: () => humanSizeToBytes,
	i18n: () => i18n,
	inView: () => inView,
	innerWidth: () => innerWidth,
	isAbortError: () => isAbortError,
	isArray: () => isArray,
	isAtom: () => isAtom,
	isBoolean: () => isBoolean,
	isDestructable: () => isDestructable,
	isEqual: () => isEqual,
	isFastEqual: () => isFastEqual,
	isFunction: () => isFunction,
	isHTML: () => isHTML,
	isHtmlFromWord: () => isHtmlFromWord,
	isInitable: () => isInitable,
	isInt: () => isInt,
	isJoditObject: () => isJoditObject,
	isLicense: () => isLicense,
	isMarker: () => isMarker,
	isNativeFunction: () => isNativeFunction,
	isNumber: () => isNumber,
	isNumeric: () => isNumeric,
	isPlainObject: () => isPlainObject,
	isPromise: () => isPromise,
	isSet: () => isSet,
	isString: () => isString,
	isStringArray: () => isStringArray,
	isURL: () => isURL,
	isUnsafeProtoKey: () => isUnsafeProtoKey,
	isValidName: () => isValidName,
	isViewObject: () => isViewObject,
	isVoid: () => isVoid,
	isWindow: () => isWindow,
	kebabCase: () => kebabCase,
	keepNames: () => keepNames,
	keys: () => keys,
	loadImage: () => loadImage,
	loadNext: () => loadNext,
	loadNextStyle: () => loadNextStyle,
	markAsAtomic: () => markAsAtomic,
	markDeprecated: () => markDeprecated,
	markOwner: () => markOwner,
	memorizeExec: () => memorizeExec,
	nl2br: () => nl2br,
	normalizeColor: () => normalizeColor,
	normalizeCssNumericValue: () => normalizeCssNumericValue,
	normalizeCssValue: () => normalizeCssValue,
	normalizeKeyAliases: () => normalizeKeyAliases,
	normalizeLicense: () => normalizeLicense,
	normalizePath: () => normalizePath,
	normalizeRelativePath: () => normalizeRelativePath,
	normalizeSize: () => normalizeSize,
	normalizeUrl: () => normalizeUrl,
	offset: () => offset,
	options: () => options,
	parseQuery: () => parseQuery,
	position: () => position,
	previewBox: () => previewBox,
	refs: () => refs,
	reset: () => reset,
	resolveElement: () => resolveElement,
	safeHTML: () => safeHTML,
	sanitizeHTMLElement: () => sanitizeHTMLElement,
	scrollIntoViewIfNeeded: () => scrollIntoViewIfNeeded,
	set: () => set,
	setTimeout: () => setTimeout,
	size: () => size$1,
	splitArray: () => splitArray,
	sprintf: () => sprintf,
	stringify: () => stringify,
	stripTags: () => stripTags,
	toArray: () => toArray,
	trim: () => trim,
	trimChars: () => trimChars,
	trimInv: () => trimInv,
	ucfirst: () => ucfirst
});
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
//#endregion
//#region node_modules/jodit/esm/core/component/component.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var StatusListHandlers = /* @__PURE__ */ new Map();
/**
* The base class of all Jodit UI components. Provides work with a life cycle.
*/
var Component = class {
	get componentName() {
		if (!this.__componentName) this.__componentName = "jodit-" + kebabCase((isFunction(this.className) ? this.className() : "") || getClassName(this));
		return this.__componentName;
	}
	getFullElName(elementName, mod, modValue) {
		const result = [this.componentName];
		if (elementName) {
			elementName = elementName.replace(/[^a-z0-9-]/gi, "-");
			result.push(`__${elementName}`);
		}
		if (mod) {
			result.push("_", mod);
			result.push("_", isVoid(modValue) ? "true" : modValue.toString());
		}
		return result.join("");
	}
	/**
	* The document in which jodit was created
	*/
	get ownerDocument() {
		return this.ow.document;
	}
	/**
	* Shortcut for `this.ownerDocument`
	*/
	get od() {
		return this.ownerDocument;
	}
	get ow() {
		return this.ownerWindow;
	}
	/**
	* Safe get any field
	* @example
	* ```js
	* private a = {
	* 	b: {
	* 		c: {
	* 			e: {
	* 				g: {
	* 					color: 'red'
	* 				}
	* 			}
	* 		}
	* 	}
	* }
	*
	* this.get('a.b.c.e.g.color'); // Safe access to color
	* // instead using optionsl chaining
	* this?.a?.b?.c?.e?.g?.color
	* ```
	*
	* @param chain - the path to be traversed in the obj object
	* @param obj - the object in which the value is searched
	*/
	get(chain, obj) {
		return get$1(chain, obj || this);
	}
	/**
	* Component is ready for work
	*/
	get isReady() {
		return this.componentStatus === STATUSES.ready;
	}
	/**
	* Component was destructed
	*/
	get isDestructed() {
		return this.componentStatus === STATUSES.destructed;
	}
	/**
	* The component is currently undergoing destructuring or has already been destroyed.
	* Those. you should not the app froze new events on him now or do anything else with him.
	*/
	get isInDestruct() {
		return STATUSES.beforeDestruct === this.componentStatus || STATUSES.destructed === this.componentStatus;
	}
	/**
	* Bind destructor to some View
	*/
	bindDestruct(component) {
		component.hookStatus(STATUSES.beforeDestruct, () => !this.isInDestruct && this.destruct());
		return this;
	}
	constructor() {
		this.async = new Async();
		/**
		* The window in which jodit was created
		*/
		this.ownerWindow = window;
		this.__componentStatus = STATUSES.beforeInit;
		this.uid = "jodit-uid-" + uniqueUid();
	}
	/**
	* Destruct component method
	*/
	destruct() {
		this.setStatus(STATUSES.destructed);
		if (this.async) {
			this.async.destruct();
			this.async = void 0;
		}
		if (StatusListHandlers.get(this)) StatusListHandlers.delete(this);
		this.ownerWindow = void 0;
	}
	/**
	* Current component status
	*/
	get componentStatus() {
		return this.__componentStatus;
	}
	/**
	* Setter for current component status
	*/
	set componentStatus(componentStatus) {
		this.setStatus(componentStatus);
	}
	/**
	* Set component status
	* @param componentStatus - component status
	* @see ComponentStatus
	*/
	setStatus(componentStatus) {
		return this.setStatusComponent(componentStatus, this);
	}
	/**
	* Set status recursively on all parents
	*/
	setStatusComponent(componentStatus, component) {
		if (componentStatus === this.__componentStatus) return;
		if (component === this) this.__componentStatus = componentStatus;
		const proto = Object.getPrototypeOf(this);
		if (proto && isFunction(proto.setStatusComponent)) proto.setStatusComponent(componentStatus, component);
		const statuses = StatusListHandlers.get(this), list = statuses === null || statuses === void 0 ? void 0 : statuses[componentStatus];
		if (list && list.length) list.forEach((cb) => cb(component));
	}
	/**
	* Adds a handler for changing the component's status
	*
	* @param status - the status at which the callback is triggered
	* @param callback - a function that will be called when the status is `status`
	*/
	hookStatus(status, callback) {
		let list = StatusListHandlers.get(this);
		if (!list) {
			list = {};
			StatusListHandlers.set(this, list);
		}
		if (!list[status]) list[status] = [];
		list[status].push(callback);
	}
	static isInstanceOf(c, constructorFunc) {
		return c instanceof constructorFunc;
	}
};
Component.STATUSES = STATUSES;
//#endregion
//#region node_modules/jodit/esm/core/component/view-component.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var ViewComponent = class extends Component {
	/**
	* Shortcut for `this.jodit`
	*/
	get j() {
		return this.jodit;
	}
	get defaultTimeout() {
		return this.j.defaultTimeout;
	}
	i18n(text, ...params) {
		return this.j.i18n(text, ...params);
	}
	/**
	* Attach component to View
	*/
	setParentView(jodit) {
		this.jodit = jodit;
		if (jodit.ow) this.ownerWindow = jodit.ow;
		jodit.components.add(this);
		return this;
	}
	constructor(jodit) {
		super();
		this.setParentView(jodit);
	}
	/** @override */
	destruct() {
		this.j.components.delete(this);
		return super.destruct();
	}
};
//#endregion
//#region node_modules/jodit/esm/core/component/index.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
//#endregion
//#region node_modules/jodit/esm/core/decorators/debounce/debounce.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
function debounce(timeout, firstCallImmediately = false, method = "debounce") {
	return (target, propertyKey) => {
		const fn = target[propertyKey];
		if (!isFunction(fn)) throw error("Handler must be a Function");
		target.hookStatus(STATUSES.ready, (component) => {
			const { async } = component;
			assert(async != null, `Component ${component.componentName || component.constructor.name} should have "async:IAsync" field`);
			const propTimeout = isFunction(timeout) ? timeout(component) : timeout;
			const realTimout = isNumber(propTimeout) || isPlainObject(propTimeout) ? propTimeout : component.defaultTimeout;
			Object.defineProperty(component, propertyKey, {
				configurable: true,
				value: async[method](component[propertyKey].bind(component), realTimout, firstCallImmediately)
			});
		});
		return {
			configurable: true,
			get() {
				return fn.bind(this);
			}
		};
	};
}
/**
* Wrap function in throttle wrapper
*/
function throttle(timeout, firstCallImmediately = false) {
	return debounce(timeout, firstCallImmediately, "throttle");
}
//#endregion
//#region node_modules/jodit/esm/core/decorators/derive/derive.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* [[include:core/decorators/derive/README.md]]
* @packageDocumentation
* @module decorators/derive
*/
function derive(...traits) {
	return (target) => {
		const origin = target.prototype;
		for (let i = 0; i < traits.length; i++) {
			const trait = traits[i];
			const keys = Object.getOwnPropertyNames(trait.prototype);
			for (let j = 0; j < keys.length; j++) {
				const key = keys[j], method = Object.getOwnPropertyDescriptor(trait.prototype, key);
				if (method != null && isFunction(method.value) && !isFunction(origin[key])) Object.defineProperty(origin, key, {
					enumerable: true,
					configurable: true,
					writable: true,
					value: function(...args) {
						return method.value.call(this, ...args);
					}
				});
			}
		}
	};
}
//#endregion
//#region node_modules/jodit/esm/core/decorators/hook/hook.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Call on some component status
*/
function hook(status) {
	return (target, propertyKey) => {
		if (!isFunction(target[propertyKey])) throw error("Handler must be a Function");
		target.hookStatus(status, (component) => {
			component[propertyKey].call(component);
		});
	};
}
//#endregion
//#region node_modules/jodit/esm/core/decorators/idle/idle.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
function idle() {
	return (target, propertyKey) => {
		if (!isFunction(target[propertyKey])) throw error("Handler must be a Function");
		target.hookStatus(STATUSES.ready, (component) => {
			const { async } = component;
			const originalMethod = component[propertyKey];
			component[propertyKey] = (...args) => async.requestIdleCallback(originalMethod.bind(component, ...args));
		});
	};
}
//#endregion
//#region node_modules/jodit/esm/core/decorators/nonenumerable/nonenumerable.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* [[include:core/decorators/nonenumerable/README.md]]
* @packageDocumentation
* @module decorators/nonenumerable
*/
var nonenumerable = (target, propertyKey) => {
	if ((Object.getOwnPropertyDescriptor(target, propertyKey) || {}).enumerable !== false) Object.defineProperty(target, propertyKey, {
		enumerable: false,
		set(value) {
			Object.defineProperty(this, propertyKey, {
				enumerable: false,
				writable: true,
				value
			});
		}
	});
};
//#endregion
//#region node_modules/jodit/esm/core/decorators/persistent/persistent.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
function persistent(target, propertyKey) {
	target.hookStatus(STATUSES.ready, (component) => {
		const jodit = isViewObject(component) ? component : component.jodit, storageKey = `${jodit.options.namespace}${component.componentName}_prop_${propertyKey}`, initialValue = component[propertyKey];
		Object.defineProperty(component, propertyKey, {
			get() {
				var _a;
				return (_a = jodit.storage.get(storageKey)) !== null && _a !== void 0 ? _a : initialValue;
			},
			set(value) {
				jodit.storage.set(storageKey, value);
			}
		});
	});
}
//#endregion
//#region node_modules/jodit/esm/core/decorators/wait/wait.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
function wait(condition) {
	return (target, propertyKey) => {
		const fn = target[propertyKey];
		if (!isFunction(fn)) throw error("Handler must be a Function");
		target.hookStatus(STATUSES.ready, (component) => {
			const { async } = component;
			const realMethod = component[propertyKey];
			let timeout = 0;
			Object.defineProperty(component, propertyKey, {
				configurable: true,
				value: function callProxy(...args) {
					async.clearTimeout(timeout);
					if (condition(component)) realMethod.apply(component, args);
					else timeout = async.setTimeout(() => callProxy(...args), 10);
				}
			});
		});
	};
}
//#endregion
//#region node_modules/jodit/esm/core/event-emitter/observable.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var OBSERVABLE_OBJECT = Symbol("observable-object");
function isObservableObject(obj) {
	return obj[OBSERVABLE_OBJECT] !== void 0;
}
/**
* Makes any object an observable object
* @example
* ```js
* const obj = {
*   a: 1,
*   b: {
*     c: 5
*   }
* }
*
* const obsObj = Jodit.modules.observable(obj);
* console.log(obj === obsObj); // true
* obsObj.on('change', () => {
*   console.log('Object changed');
* });
* obsObj.on('change.a', () => {
*   console.log('Key a changed');
* });
* obsObj.on('change.b.c', () => {
*   console.log('Key b.c changed');
* });
*
* obj.a = 6;
* // Object changed
* // Key a changed
*
* obj.b = {c: 6}
* // Object changed
*
* obj.b.c = 8
* // Object changed
* // Key b.c changed
* ```
*/
function observable(obj) {
	if (isObservableObject(obj)) return obj;
	const __lockEvent = {};
	const __onEvents = {};
	const on = (event, callback) => {
		if (isArray(event)) {
			event.map((e) => on(e, callback));
			return obj;
		}
		if (!__onEvents[event]) __onEvents[event] = [];
		__onEvents[event].push(callback);
		return obj;
	};
	const fire = (event, ...attr) => {
		if (isArray(event)) {
			event.map((e) => fire(e, ...attr));
			return;
		}
		try {
			if (!__lockEvent[event] && __onEvents[event]) {
				__lockEvent[event] = true;
				__onEvents[event].forEach((clb) => clb.call(obj, ...attr));
			}
		} finally {
			__lockEvent[event] = false;
		}
	};
	const initAccessors = (dict, prefixes = []) => {
		const store = {};
		if (isObservableObject(dict)) return;
		Object.defineProperty(dict, OBSERVABLE_OBJECT, {
			enumerable: false,
			value: true
		});
		Object.keys(dict).forEach((_key) => {
			const key = _key;
			const prefix = prefixes.concat(key).filter((a) => a.length);
			store[key] = dict[key];
			const descriptor = getPropertyDescriptor(dict, key);
			Object.defineProperty(dict, key, {
				set: (value) => {
					const oldValue = store[key];
					if (!isFastEqual(store[key], value)) {
						fire(["beforeChange", `beforeChange.${prefix.join(".")}`], key, value);
						if (isPlainObject(value)) initAccessors(value, prefix);
						if (descriptor && descriptor.set) descriptor.set.call(obj, value);
						else store[key] = value;
						const sum = [];
						fire(["change", ...prefix.reduce((rs, p) => {
							sum.push(p);
							rs.push(`change.${sum.join(".")}`);
							return rs;
						}, [])], prefix.join("."), oldValue, (value === null || value === void 0 ? void 0 : value.valueOf) ? value.valueOf() : value);
					}
				},
				get: () => {
					if (descriptor && descriptor.get) return descriptor.get.call(obj);
					return store[key];
				},
				enumerable: true,
				configurable: true
			});
			if (isPlainObject(store[key])) initAccessors(store[key], prefix);
		});
		Object.defineProperty(obj, "on", { value: on });
	};
	initAccessors(obj);
	return obj;
}
//#endregion
//#region node_modules/jodit/esm/core/decorators/watch/watch.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Watch decorator. Added observer for some change in field value
*/
function watch(observeFields, opts) {
	return (target, propertyKey) => {
		var _a;
		if (!isFunction(target[propertyKey])) throw error("Handler must be a Function");
		const immediately = (_a = opts === null || opts === void 0 ? void 0 : opts.immediately) !== null && _a !== void 0 ? _a : true;
		const context = opts === null || opts === void 0 ? void 0 : opts.context;
		const process = (component) => {
			const view = isViewObject(component) ? component : component.jodit;
			let callback = (key, ...args) => {
				if (component.isInDestruct) return;
				return component[propertyKey](key, ...args);
			};
			if (!immediately) callback = component.async.microDebounce(callback, true);
			splitArray(observeFields).forEach((field) => {
				if (/:/.test(field)) {
					const [objectPath, eventName] = field.split(":");
					let ctx = context;
					if (objectPath.length) ctx = component.get(objectPath);
					if (isFunction(ctx)) ctx = ctx(component);
					view.events.on(ctx || component, eventName, callback);
					if (!ctx) view.events.on(eventName, callback);
					component.hookStatus("beforeDestruct", () => {
						view.events.off(ctx || component, eventName, callback).off(eventName, callback);
					});
					return;
				}
				const parts = field.split("."), [key] = parts, teil = parts.slice(1);
				let value = component[key];
				if (isPlainObject(value)) observable(value).on(`change.${teil.join(".")}`, callback);
				const descriptor = getPropertyDescriptor(target, key);
				Object.defineProperty(component, key, {
					configurable: true,
					set(v) {
						const oldValue = value;
						if (oldValue === v) return;
						value = v;
						if (descriptor && descriptor.set) descriptor.set.call(component, v);
						if (isPlainObject(value)) {
							value = observable(value);
							value.on(`change.${teil.join(".")}`, callback);
						}
						callback(key, oldValue, value);
					},
					get() {
						if (descriptor && descriptor.get) return descriptor.get.call(component);
						return value;
					}
				});
			});
		};
		if (isFunction(target.hookStatus)) target.hookStatus(STATUSES.ready, process);
		else process(target);
	};
}
//#endregion
//#region node_modules/jodit/esm/core/decorators/index.js
var decorators_exports = /* @__PURE__ */ __exportAll({
	autobind: () => autobind,
	cache: () => cache,
	cacheHTML: () => cacheHTML,
	cached: () => cached,
	component: () => component,
	debounce: () => debounce,
	derive: () => derive,
	getComponentClass: () => getComponentClass,
	hook: () => hook,
	idle: () => idle,
	nonenumerable: () => nonenumerable,
	persistent: () => persistent,
	throttle: () => throttle,
	wait: () => wait,
	watch: () => watch
});
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
//#endregion
//#region node_modules/jodit/esm/core/request/config.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
Config.prototype.defaultAjaxOptions = {
	successStatuses: [
		200,
		201,
		202
	],
	method: "GET",
	url: "",
	data: null,
	contentType: "application/x-www-form-urlencoded; charset=UTF-8",
	headers: { "X-REQUESTED-WITH": "XMLHttpRequest" },
	withCredentials: false,
	xhr() {
		return new XMLHttpRequest();
	}
};
//#endregion
//#region node_modules/jodit/esm/core/request/response.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var Response = class {
	get url() {
		return this.request.url;
	}
	constructor(request, status, statusText, body) {
		this.request = request;
		this.status = status;
		this.statusText = statusText;
		this.body = body;
	}
	async json() {
		return JSON.parse(this.body);
	}
	text() {
		return Promise.resolve(this.body);
	}
	async blob() {
		return this.body;
	}
};
//#endregion
//#region node_modules/jodit/esm/core/request/ajax.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$44 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var Ajax = class Ajax {
	className() {
		return "Ajax";
	}
	constructor(options, defaultAjaxOptions = Config.prototype.defaultAjaxOptions) {
		this.__async = new Async();
		this.__isFulfilled = false;
		this.__activated = false;
		this.__isDestructed = false;
		this.options = ConfigProto(options || {}, defaultAjaxOptions);
		this.xhr = this.o.xhr ? this.o.xhr() : new XMLHttpRequest();
	}
	__buildParams(obj, prefix) {
		if (isPlainObject(obj) && this.options.contentType && this.options.contentType.includes("application/json")) return JSON.stringify(obj);
		if (isFunction(this.o.queryBuild)) return this.o.queryBuild.call(this, obj, prefix);
		if (isString(obj) || obj instanceof globalWindow.FormData || typeof obj === "object" && obj != null && isFunction(obj.append)) return obj;
		return buildQuery(obj);
	}
	get o() {
		return this.options;
	}
	abort() {
		if (this.__isFulfilled) return this;
		try {
			this.__isFulfilled = true;
			this.xhr.abort();
		} catch (_a) {}
		return this;
	}
	send() {
		this.__activated = true;
		const { xhr, o } = this;
		const request = this.prepareRequest();
		return this.__async.promise(async (resolve, reject) => {
			var _a;
			const onReject = () => {
				this.__isFulfilled = true;
				reject(connection("Connection error"));
			};
			const onResolve = () => {
				this.__isFulfilled = true;
				resolve(new Response(request, xhr.status, xhr.statusText, !xhr.responseType ? xhr.responseText : xhr.response));
			};
			xhr.onload = onResolve;
			xhr.onabort = () => {
				this.__isFulfilled = true;
				reject(abort("Abort connection"));
			};
			xhr.onerror = onReject;
			xhr.ontimeout = onReject;
			if (o.responseType) xhr.responseType = o.responseType;
			xhr.onprogress = (e) => {
				var _a, _b;
				let percentComplete = 0;
				if (e.lengthComputable) percentComplete = e.loaded / e.total * 100;
				(_b = (_a = this.options).onProgress) === null || _b === void 0 || _b.call(_a, percentComplete);
			};
			xhr.onreadystatechange = () => {
				var _a, _b;
				(_b = (_a = this.options).onProgress) === null || _b === void 0 || _b.call(_a, 10);
				if (xhr.readyState === XMLHttpRequest.DONE) {
					if (o.successStatuses.includes(xhr.status)) onResolve();
					else if (xhr.statusText) {
						this.__isFulfilled = true;
						reject(connection(xhr.statusText));
					}
				}
			};
			xhr.withCredentials = (_a = o.withCredentials) !== null && _a !== void 0 ? _a : false;
			const { url, data, method } = request;
			xhr.open(method, url, true);
			if (o.contentType && xhr.setRequestHeader) xhr.setRequestHeader("Content-type", o.contentType);
			let { headers } = o;
			if (isFunction(headers)) headers = await headers.call(this);
			if (headers && xhr.setRequestHeader) Object.keys(headers).forEach((key) => {
				xhr.setRequestHeader(key, headers[key]);
			});
			this.__async.setTimeout(() => {
				xhr.send(data ? this.__buildParams(data) : void 0);
			}, 0);
		});
	}
	async *stream() {
		var _a;
		this.__activated = true;
		const { xhr, o } = this;
		const request = this.prepareRequest();
		let lastIndex = 0;
		let buffer = "";
		const queue = [];
		let waitResolve = null;
		let done = false;
		let streamError = null;
		const notify = () => {
			if (waitResolve) {
				const r = waitResolve;
				waitResolve = null;
				r();
			}
		};
		const processChunk = (final) => {
			var _a;
			const text = xhr.responseText;
			buffer += text.substring(lastIndex);
			lastIndex = text.length;
			const events = buffer.split("\n\n");
			buffer = final ? "" : (_a = events.pop()) !== null && _a !== void 0 ? _a : "";
			for (const event of events) {
				const trimmed = event.trim();
				if (!trimmed) continue;
				const dataLines = [];
				for (const line of trimmed.split("\n")) if (line.startsWith("data: ")) dataLines.push(line.slice(6));
				else if (line.startsWith("data:")) dataLines.push(line.slice(5));
				if (dataLines.length) queue.push(dataLines.join("\n"));
			}
			notify();
		};
		xhr.onprogress = () => processChunk(false);
		xhr.onload = () => {
			processChunk(true);
			done = true;
			this.__isFulfilled = true;
			notify();
		};
		xhr.onerror = () => {
			streamError = connection("Connection error");
			done = true;
			this.__isFulfilled = true;
			notify();
		};
		xhr.onabort = () => {
			streamError = abort("Abort connection");
			done = true;
			this.__isFulfilled = true;
			notify();
		};
		xhr.withCredentials = (_a = o.withCredentials) !== null && _a !== void 0 ? _a : false;
		const { url, data, method } = request;
		xhr.open(method, url, true);
		if (o.contentType && xhr.setRequestHeader) xhr.setRequestHeader("Content-type", o.contentType);
		let { headers } = o;
		if (isFunction(headers)) headers = await headers.call(this);
		if (headers && xhr.setRequestHeader) Object.keys(headers).forEach((key) => {
			xhr.setRequestHeader(key, headers[key]);
		});
		xhr.send(data ? this.__buildParams(data) : void 0);
		try {
			while (true) if (queue.length > 0) yield queue.shift();
			else if (done) break;
			else await new Promise((r) => {
				waitResolve = r;
			});
		} finally {
			if (!this.__isFulfilled) this.abort();
		}
		if (streamError) throw streamError;
	}
	prepareRequest() {
		if (!this.o.url) throw error("Need URL for AJAX request");
		let url = this.o.url;
		const data = this.o.data;
		const method = (this.o.method || "get").toLowerCase();
		if (method === "get" && data && isPlainObject(data)) {
			const qIndex = url.indexOf("?");
			if (qIndex !== -1) {
				const urlData = parseQuery(url.substring(qIndex));
				url = url.substring(0, qIndex) + "?" + buildQuery({
					...urlData,
					...data
				});
			} else url += "?" + buildQuery(this.o.data);
		}
		const request = {
			url,
			method,
			data
		};
		Ajax.log.splice(100);
		Ajax.log.push(request);
		return request;
	}
	destruct() {
		if (!this.__isDestructed) {
			this.__isDestructed = true;
			if (this.__activated && !this.__isFulfilled) {
				this.abort();
				this.__isFulfilled = true;
			}
			this.__async.destruct();
		}
	}
};
Ajax.log = [];
__decorate$44([autobind], Ajax.prototype, "destruct", null);
//#endregion
//#region node_modules/jodit/esm/core/request/index.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
//#endregion
//#region node_modules/jodit/esm/core/traits/elms.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var Elms = class {
	/**
	* Return element with BEM class name
	*/
	getElm(elementName) {
		const className = this.getFullElName(elementName);
		return Dom.first(this.container, (node) => Dom.isHTMLElement(node) && node.classList.contains(className));
	}
	/**
	* Return elements with BEM class name
	*/
	getElms(elementName) {
		const className = this.getFullElName(elementName);
		const result = [];
		Dom.each(this.container, (node) => {
			if (Dom.isHTMLElement(node) && node.classList.contains(className)) result.push(node);
		});
		return result;
	}
};
//#endregion
//#region node_modules/jodit/esm/core/traits/mods.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var Mods = class {
	afterSetMod(name, value) {}
	/**
	* Set/remove BEM class modification
	*
	* @param value - if null, mod will be removed
	*/
	setMod(name, value, container) {
		name = name.toLowerCase();
		const oldValue = this.mods[name];
		if (oldValue === value) return this;
		const mod = `${this.componentName}_${name}_`, cl = (container || this.container).classList;
		if (oldValue != null) cl.remove(`${mod}${oldValue.toString().toLowerCase()}`);
		!isVoid(value) && value !== "" && cl.add(`${mod}${value.toString().toLowerCase()}`);
		this.mods[name] = value;
		this.afterSetMod(name, value);
		return this;
	}
	/**
	* Get BEM class modification value
	*/
	getMod(name) {
		var _a;
		return (_a = this.mods[name]) !== null && _a !== void 0 ? _a : null;
	}
};
//#endregion
//#region node_modules/jodit/esm/core/ui/icon.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var Icon = class Icon {
	static getIcon(name) {
		if (/<svg/i.test(name)) return name;
		return Icon.icons[name] || Icon.icons[name.replace(/-/g, "_")] || Icon.icons[name.replace(/_/g, "-")] || Icon.icons[camelCase(name)] || Icon.icons[kebabCase(name)] || Icon.icons[name.toLowerCase()];
	}
	/**
	* Check if icon exist in store
	*/
	static exists(name) {
		return this.getIcon(name) !== void 0;
	}
	/**
	* Return SVG icon
	*/
	static get(name, defaultValue = "<span></span>") {
		return this.getIcon(name) || defaultValue;
	}
	/**
	* Set SVG in store
	*/
	static set(name, value) {
		this.icons[name.replace("_", "-")] = value;
		return this;
	}
	/**
	* Make icon html element
	*/
	static makeIcon(jodit, icon) {
		var _a, _b, _c, _d;
		if (!icon) return;
		let iconElement;
		const { name, iconURL, fill, scale } = icon;
		const clearName = name.replace(/[^a-zA-Z0-9]/g, "_");
		let iconFromEvent;
		if (!/<svg/.test(name)) iconFromEvent = (_b = (_a = jodit.o).getIcon) === null || _b === void 0 ? void 0 : _b.call(_a, name, clearName);
		const cacheKey = `${name}${iconURL}${fill}${scale !== null && scale !== void 0 ? scale : ""}${iconFromEvent !== null && iconFromEvent !== void 0 ? iconFromEvent : ""}`;
		if (jodit.o.cache && this.__cache.has(cacheKey)) return (_c = this.__cache.get(cacheKey)) === null || _c === void 0 ? void 0 : _c.cloneNode(true);
		if (iconURL) {
			iconElement = jodit.c.span();
			css(iconElement, "backgroundImage", "url(" + iconURL.replace("{basePath}", (jodit === null || jodit === void 0 ? void 0 : jodit.basePath) || "") + ")");
		} else {
			const svg = iconFromEvent || Icon.get(name, "") || ((_d = jodit.o.extraIcons) === null || _d === void 0 ? void 0 : _d[name]);
			if (svg) {
				iconElement = Icon.toIconElement(jodit, svg.trim());
				if (!/^<svg/i.test(name)) iconElement.classList.add("jodit-icon_" + clearName);
			}
		}
		if (iconElement) {
			iconElement.classList.add("jodit-icon");
			css(iconElement, "fill", fill);
			if (scale != null) css(iconElement, "transform", `scale(${scale})`);
			jodit.o.cache && this.__cache.set(cacheKey, iconElement.cloneNode(true));
		}
		return iconElement;
	}
	/**
	* Turn a raw icon string into an element with a `classList`.
	*
	* A plain-text icon (e.g. an emoji/text glyph) makes `fromHTML` return a
	* Text node, which has no `classList`; wrap it in a span so classes/styles
	* can be applied and `makeIcon` never crashes on `iconElement.classList`.
	* Note: SVG icons are `SVGElement` (not `HTMLElement`) but are still Element
	* nodes with a `classList`, so we check `isElement`, not `isHTMLElement`.
	*/
	static toIconElement(jodit, svg) {
		const node = jodit.c.fromHTML(svg);
		return Dom.isElement(node) ? node : jodit.c.span("jodit-icon_text", node);
	}
};
Icon.icons = {};
Icon.__cache = /* @__PURE__ */ new Map();
//#endregion
//#region node_modules/jodit/esm/core/ui/element.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$43 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var UIElement_1;
var UIElement = UIElement_1 = class UIElement extends ViewComponent {
	getRole() {
		var _a;
		return ((_a = this.options) === null || _a === void 0 ? void 0 : _a.role) || "";
	}
	get parentElement() {
		return this.__parentElement;
	}
	set parentElement(parentElement) {
		this.__parentElement = parentElement;
		if (parentElement) parentElement.hookStatus("beforeDestruct", () => this.destruct());
		this.updateParentElement(this);
	}
	bubble(callback) {
		let parent = this.parentElement;
		while (parent) {
			callback(parent);
			parent = parent.parentElement;
		}
		return this;
	}
	updateParentElement(target) {
		var _a;
		(_a = this.__parentElement) === null || _a === void 0 || _a.updateParentElement(target);
		return this;
	}
	/** @override */
	get(chain, obj) {
		return super.get(chain, obj) || this.getElm(chain);
	}
	/**
	* Find match parent
	*/
	closest(type) {
		const c = typeof type === "object" ? (pe) => pe === type : (pe) => Component.isInstanceOf(pe, type);
		let pe = this.__parentElement;
		while (pe) {
			if (c(pe)) return pe;
			if (!pe.parentElement && pe.container.parentElement) pe = UIElement_1.closestElement(pe.container.parentElement, UIElement_1);
			else pe = pe.parentElement;
		}
		return null;
	}
	/**
	* Find closest UIElement in DOM
	*/
	static closestElement(node, type) {
		const elm = Dom.up(node, (elm) => {
			if (elm) {
				const { component } = elm;
				return component && Component.isInstanceOf(component, type);
			}
			return false;
		});
		return elm ? elm === null || elm === void 0 ? void 0 : elm.component : null;
	}
	/**
	* Update UI from state
	*/
	update() {}
	/**
	* Append container to element
	*/
	appendTo(element) {
		Dom.append(element, this.container);
		return this;
	}
	/**
	* Valid name only with valid chars
	*/
	clearName(name) {
		return name.replace(/[^a-zA-Z0-9]/g, "_");
	}
	/**
	* Method create only box
	*/
	render(options) {
		return this.j.c.div(this.componentName);
	}
	/**
	* Create main HTML container
	*/
	createContainer(options) {
		const result = this.render(options);
		if (isString(result)) {
			const elm = this.parseTemplate(result);
			elm.classList.add(this.componentName);
			return elm;
		}
		return result;
	}
	parseTemplate(result) {
		return this.j.c.fromHTML(result.replace(/\*([^*]+?)\*/g, (_, name) => Icon.get(name) || "").replace(/&_/g, this.componentName + "_").replace(/~([^~]+?)~/g, (_, s) => this.i18n(s)));
	}
	constructor(jodit, options) {
		super(jodit);
		this.name = "";
		this.__parentElement = null;
		this.mods = {};
		this.options = options;
		this.container = this.createContainer(options);
		const role = this.getRole();
		role && attr(this.container, "role", role);
		Object.defineProperty(this.container, "component", {
			value: this,
			configurable: true
		});
	}
	/** @override */
	destruct() {
		Dom.safeRemove(this.container);
		this.parentElement = null;
		return super.destruct();
	}
};
UIElement = UIElement_1 = __decorate$43([derive(Mods, Elms)], UIElement);
//#endregion
//#region node_modules/jodit/esm/core/ui/button/button/button.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$42 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var UIButtonState = () => ({
	size: "middle",
	type: "button",
	role: "button",
	name: "",
	value: "",
	variant: "initial",
	disabled: false,
	activated: false,
	icon: {
		name: "empty",
		fill: "",
		iconURL: "",
		scale: void 0
	},
	tooltip: "",
	text: "",
	tabIndex: void 0
});
var UIButton = class UIButton extends UIElement {
	/** @override */
	className() {
		return "UIButton";
	}
	/**
	* Set state
	*/
	setState(state) {
		Object.assign(this.state, state);
		return this;
	}
	/**
	* DOM container for text content
	*/
	get text() {
		const text = this.getElm("text");
		assert(text, "Text element not found");
		return text;
	}
	/**
	* DOM container for icon
	*/
	get icon() {
		const icon = this.getElm("icon");
		assert(icon, "Icon element not found");
		return icon;
	}
	onChangeSize() {
		this.setMod("size", this.state.size);
	}
	onChangeType() {
		attr(this.container, "type", this.state.type);
	}
	onChangeRole() {
		attr(this.container, "role", this.state.role);
	}
	/**
	* Set size from a parent list
	*/
	updateSize() {
		const UIList = getComponentClass("UIList");
		const pe = this.closest(UIList);
		if (pe) {
			this.state.size = pe.buttonSize;
			return;
		}
	}
	onChangeStatus() {
		this.setMod("variant", this.state.variant);
	}
	onChangeText() {
		this.text.textContent = this.jodit.i18n(this.state.text);
		this.updateAriaLabel();
	}
	onChangeTextSetMode() {
		this.setMod("text-icons", Boolean(this.state.text.trim().length));
	}
	onChangeDisabled() {
		attr(this.container, "disabled", this.state.disabled || null);
	}
	onChangeActivated() {
		attr(this.container, "aria-pressed", this.state.activated);
	}
	onChangeName() {
		this.container.classList.add(`${this.componentName}_${this.clearName(this.state.name)}`);
		this.name = this.state.name;
		attr(this.container, "data-ref", this.state.name);
		attr(this.container, "ref", this.state.name);
	}
	onChangeTooltip() {
		const i8nTooltip = this.state.tooltip ? this.jodit.i18n(this.state.tooltip) : null;
		if (this.get("j.o.useNativeTooltip")) attr(this.container, "title", i8nTooltip);
		attr(this.container, "aria-label", i8nTooltip);
		this.updateAriaLabel();
	}
	updateAriaLabel() {
		const hasText = this.state.text.trim().length > 0;
		const i8nTooltip = this.state.tooltip ? this.jodit.i18n(this.state.tooltip) : null;
		attr(this.container, "aria-label", i8nTooltip);
		attr(this.button, "aria-label", !hasText ? i8nTooltip : null);
	}
	onChangeTabIndex() {
		attr(this.container, "tabindex", this.state.tabIndex);
	}
	onChangeIcon() {
		const textIcons = this.get("j.o.textIcons");
		if (textIcons === true || isFunction(textIcons) && textIcons(this.state.name)) return;
		Dom.detach(this.icon);
		const iconElement = Icon.makeIcon(this.j, this.state.icon);
		iconElement && Dom.append(this.icon, iconElement);
	}
	/**
	* Set focus on an element
	*/
	focus() {
		this.container.focus();
	}
	/**
	* Element has focus
	*/
	isFocused() {
		const { activeElement } = this.od;
		return Boolean(activeElement && Dom.isOrContains(this.container, activeElement));
	}
	/** @override */
	createContainer() {
		const cn = this.componentName;
		const button = this.j.c.element("button", {
			class: cn,
			type: "button",
			role: "button",
			ariaPressed: false
		});
		const icon = this.j.c.span(cn + "__icon");
		const text = this.j.c.span(cn + "__text");
		Dom.append(button, icon);
		Dom.append(button, text);
		return button;
	}
	constructor(jodit, state) {
		super(jodit);
		/**
		* Marker for buttons
		*/
		this.isButton = true;
		this.state = UIButtonState();
		this.actionHandlers = [];
		this.button = this.container;
		this.updateSize();
		this.onChangeSize();
		this.onChangeStatus();
		if (state) this.hookStatus(STATUSES.ready, () => {
			this.setState(state);
		});
	}
	destruct() {
		this.j.e.off(this);
		this.j.e.off(this.container);
		return super.destruct();
	}
	/**
	* Add action handler
	*/
	onAction(callback) {
		this.actionHandlers.push(callback);
		return this;
	}
	/**
	* Fire all click handlers
	*/
	__onActionFire(e) {
		e.buffer = { actionTrigger: this };
		this.actionHandlers.forEach((callback) => callback.call(this, e));
		this.j.e.fire(this, "click", e);
	}
};
__decorate$42([cache], UIButton.prototype, "text", null);
__decorate$42([cache], UIButton.prototype, "icon", null);
__decorate$42([watch("state.size", { immediately: false })], UIButton.prototype, "onChangeSize", null);
__decorate$42([watch("state.type", { immediately: false })], UIButton.prototype, "onChangeType", null);
__decorate$42([watch("state.role", { immediately: false })], UIButton.prototype, "onChangeRole", null);
__decorate$42([watch("parentElement")], UIButton.prototype, "updateSize", null);
__decorate$42([watch("state.variant", { immediately: false })], UIButton.prototype, "onChangeStatus", null);
__decorate$42([watch("state.text", { immediately: false })], UIButton.prototype, "onChangeText", null);
__decorate$42([watch("state.text", { immediately: false })], UIButton.prototype, "onChangeTextSetMode", null);
__decorate$42([watch("state.disabled")], UIButton.prototype, "onChangeDisabled", null);
__decorate$42([watch("state.activated")], UIButton.prototype, "onChangeActivated", null);
__decorate$42([watch("state.name", { immediately: false })], UIButton.prototype, "onChangeName", null);
__decorate$42([watch("state.tooltip", { immediately: false })], UIButton.prototype, "onChangeTooltip", null);
__decorate$42([watch("state.tabIndex", { immediately: false })], UIButton.prototype, "onChangeTabIndex", null);
__decorate$42([watch("state.icon", { immediately: false })], UIButton.prototype, "onChangeIcon", null);
__decorate$42([cacheHTML], UIButton.prototype, "createContainer", null);
__decorate$42([watch("button:click")], UIButton.prototype, "__onActionFire", null);
UIButton = __decorate$42([component], UIButton);
function Button(jodit, stateOrText, text, variant) {
	const button = new UIButton(jodit);
	button.state.tabIndex = jodit.o.allowTabNavigation ? 0 : -1;
	if (isString(stateOrText)) {
		button.state.icon.name = stateOrText;
		button.state.name = stateOrText;
		if (variant) button.state.variant = variant;
		if (text) button.state.text = text;
	} else button.setState(stateOrText);
	return button;
}
//#endregion
//#region node_modules/jodit/esm/core/ui/group/group.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$41 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var UIGroup_1;
var UIGroup = UIGroup_1 = class UIGroup extends UIElement {
	className() {
		return "UIGroup";
	}
	getRole() {
		var _a;
		return ((_a = this.options) === null || _a === void 0 ? void 0 : _a.role) || "list";
	}
	/**
	* All group children
	*/
	get allChildren() {
		const result = [];
		const stack = [...this.elements];
		while (stack.length) {
			const elm = stack.shift();
			if (isArray(elm)) stack.push(...elm);
			else if (Component.isInstanceOf(elm, UIGroup_1)) stack.push(...elm.elements);
			else elm && result.push(elm);
		}
		return result;
	}
	/**
	* Update all children
	*/
	update() {
		this.elements.forEach((elm) => elm.update());
		this.setMod("size", this.buttonSize);
	}
	append(elms, distElementOrIndex) {
		if (isArray(elms)) {
			if (typeof distElementOrIndex === "number") throw new Error("You can not use index when append array of elements");
			elms.forEach((item) => this.append(item, distElementOrIndex));
			return this;
		}
		const elm = elms;
		let index = void 0;
		if (typeof distElementOrIndex === "number") {
			index = Math.min(Math.max(0, distElementOrIndex), this.elements.length);
			this.elements.splice(index, 0, elm);
		} else this.elements.push(elm);
		if (elm.name) elm.container.classList.add(this.getFullElName(elm.name));
		if (distElementOrIndex && typeof distElementOrIndex === "string") {
			const distElm = this.getElm(distElementOrIndex);
			if (distElm == null) throw new Error("Element does not exist");
			Dom.append(distElm, elm.container);
		} else this.appendChildToContainer(elm.container, index);
		elm.parentElement = this;
		return this;
	}
	/** @override */
	afterSetMod(name, value) {
		if (this.syncMod) this.elements.forEach((elm) => elm.setMod(name, value));
	}
	/**
	* Allow set another container for the box of all children
	*/
	appendChildToContainer(childContainer, index) {
		const ref = index === void 0 || index < 0 || index > this.elements.length - 1 ? null : this.container.children[index];
		if (ref == null) Dom.append(this.container, childContainer);
		else Dom.before(ref, childContainer);
	}
	/**
	* Remove element from group
	*/
	remove(elm) {
		const index = this.elements.indexOf(elm);
		if (index !== -1) {
			this.elements.splice(index, 1);
			Dom.safeRemove(elm.container);
			elm.parentElement = null;
		}
		return this;
	}
	/**
	* Clear group
	*/
	clear() {
		this.elements.forEach((elm) => elm.destruct());
		this.elements.length = 0;
		return this;
	}
	/**
	* @param elements - Items of group
	*/
	constructor(jodit, elements, options) {
		super(jodit, options);
		this.options = options;
		/**
		* Synchronize mods to all children
		*/
		this.syncMod = false;
		this.elements = [];
		this.buttonSize = "middle";
		elements === null || elements === void 0 || elements.forEach((elm) => elm && this.append(elm));
		if (options === null || options === void 0 ? void 0 : options.name) this.name = options.name;
	}
	setParentView(view) {
		var _a;
		(_a = this.elements) === null || _a === void 0 || _a.forEach((elm) => elm.setParentView(view));
		return super.setParentView(view);
	}
	/** @override */
	destruct() {
		this.clear();
		return super.destruct();
	}
};
__decorate$41([watch("buttonSize")], UIGroup.prototype, "update", null);
UIGroup = UIGroup_1 = __decorate$41([component], UIGroup);
//#endregion
//#region node_modules/jodit/esm/core/ui/button/group/group.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$40 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var UIButtonGroup = class UIButtonGroup extends UIGroup {
	/** @override */
	className() {
		return "UIButtonGroup";
	}
	/** @override */
	render(options) {
		return `<div>
			<div class="&__label">~${options.label}~</div>
			<div class="&__options"></div>
		</div>`;
	}
	/** @override */
	appendChildToContainer(childContainer) {
		const options = this.getElm("options");
		assert(options != null, "Options does not exist");
		Dom.append(options, childContainer);
	}
	constructor(jodit, options = { radio: true }) {
		var _a, _b;
		super(jodit, (_a = options.options) === null || _a === void 0 ? void 0 : _a.map((opt) => {
			const btn = new UIButton(jodit, {
				text: opt.text,
				value: opt.value,
				variant: "primary"
			});
			btn.onAction(() => {
				this.select(opt.value);
			});
			return btn;
		}), options);
		this.options = options;
		this.select((_b = options.value) !== null && _b !== void 0 ? _b : 0);
	}
	select(indexOrValue) {
		var _a, _b;
		this.elements.forEach((elm, index) => {
			if (index === indexOrValue || elm.state.value === indexOrValue) elm.state.activated = true;
			else if (this.options.radio) elm.state.activated = false;
		});
		const result = this.elements.filter((elm) => elm.state.activated).map((elm) => ({
			text: elm.state.text,
			value: elm.state.value
		}));
		this.jodit.e.fire(this, "select", result);
		(_b = (_a = this.options).onChange) === null || _b === void 0 || _b.call(_a, result);
	}
};
UIButtonGroup = __decorate$40([component], UIButtonGroup);
//#endregion
//#region node_modules/jodit/esm/core/ui/button/tooltip/tooltip.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$39 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var UITooltip_1;
var WINDOW_EVENTS_ON_HIDE = [
	"scroll.tooltip",
	"joditCloseDialog",
	"mouseleave.tooltip"
];
var JODIT_EVENTS_ON_HIDE = [
	"escape.tooltip",
	"change.tooltip",
	"changePlace.tooltip",
	"afterOpenPopup.tooltip",
	"hidePopup.tooltip",
	"beforePopupClose.tooltip",
	"closeAllPopups.tooltip"
];
var UITooltip = UITooltip_1 = class UITooltip extends UIElement {
	className() {
		return "UITooltip";
	}
	render() {
		return "<div><div class=\"&__content\"></div></div>";
	}
	constructor(view) {
		super(view);
		this.__isOpened = false;
		this.__attachedContainers = /* @__PURE__ */ new Set();
		this.__listenClose = false;
		this.__currentTarget = null;
		this.__delayShowTimeout = 0;
		this.__hideTimeout = 0;
		if (!view.o.textIcons && view.o.showTooltip && !view.o.useNativeTooltip) {
			this.j.e.on("getContainer", (box) => {
				this.__onAttach(box);
			});
			view.hookStatus(STATUSES.ready, () => {
				this.__onAttach(this.j.container);
			});
		}
	}
	__onAttach(container) {
		Dom.append(getContainer(this.j, UITooltip_1), this.container);
		this.__attachedContainers.add(container);
		this.__attachedContainers.add(this.j.container);
		this.j.e.on(container, "mouseenter.tooltip", this.__onMouseEnter, { capture: true }).on(container, "mouseleave.tooltip", this.__onMouseLeave, { capture: true }).on(this.j.container, "mouseleave.tooltip", this.__onMouseLeave, { capture: true });
	}
	__addListenersOnEnter() {
		if (this.__listenClose) return;
		this.__listenClose = true;
		const view = this.j;
		view.e.on(view.ow, WINDOW_EVENTS_ON_HIDE, this.__hide).on(JODIT_EVENTS_ON_HIDE, this.__hide);
	}
	__removeListenersOnLeave() {
		if (!this.__listenClose) return;
		this.__listenClose = false;
		const view = this.j;
		view.e.off(view.ow, WINDOW_EVENTS_ON_HIDE, this.__hide).off(JODIT_EVENTS_ON_HIDE, this.__hide);
	}
	__onMouseLeave(e) {
		if (this.__currentTarget === e.target) {
			this.__hideDelay();
			this.__currentTarget = null;
		}
	}
	__onMouseEnter(e) {
		if (!Dom.isHTMLElement(e.target)) return;
		const tooltip = attr(e.target, "aria-label");
		if (!tooltip) return;
		if (Boolean(attr(e.target, "disabled"))) return;
		if (!e.target.className.includes("jodit")) return;
		this.__currentTarget = e.target;
		const target = e.target;
		this.__open(() => {
			const pos = position(target);
			return {
				x: pos.left + pos.width / 2,
				y: pos.top + pos.height
			};
		}, tooltip);
	}
	__open(getPoint, content) {
		this.__addListenersOnEnter();
		this.__isOpened = true;
		this.j.async.clearTimeout(this.__hideTimeout);
		this.j.async.clearTimeout(this.__delayShowTimeout);
		const to = this.j.o.showTooltipDelay || this.j.defaultTimeout;
		if (!to) {
			this.__show(getPoint, content);
			return;
		}
		this.__delayShowTimeout = this.j.async.setTimeout(() => this.__show(getPoint, content), to);
	}
	__show(getPoint, content) {
		this.setMod("visible", true);
		this.setMod("above", false);
		this.getElm("content").innerHTML = content;
		const point = getPoint();
		const offset = getFixedPositionOffset(this.container);
		css(this.container, {
			left: point.x - offset.x,
			top: point.y - offset.y
		});
		const tooltipPos = position(this.container);
		const viewHeight = this.j.ow.innerHeight;
		if (tooltipPos.top + tooltipPos.height > viewHeight) {
			const targetPos = position(this.__currentTarget);
			this.setMod("above", true);
			css(this.container, { top: targetPos.top - tooltipPos.height - offset.y });
		}
	}
	__hide() {
		this.j.async.clearTimeout(this.__delayShowTimeout);
		this.j.async.clearTimeout(this.__hideTimeout);
		this.__removeListenersOnLeave();
		if (this.__isOpened) {
			this.__isOpened = false;
			this.setMod("visible", false);
			Dom.detach(this.getElm("content"));
			css(this.container, { left: -5e3 });
		}
	}
	__hideDelay() {
		this.j.async.clearTimeout(this.__delayShowTimeout);
		this.j.async.clearTimeout(this.__hideTimeout);
		if (!this.__isOpened) return;
		this.__hideTimeout = this.async.setTimeout(this.__hide, this.j.defaultTimeout);
	}
	destruct() {
		this.__attachedContainers.forEach((container) => {
			this.j.e.off(container, "mouseenter.tooltip", this.__onMouseEnter).off(container, "mouseleave.tooltip", this.__onMouseLeave);
		});
		this.__hide();
		super.destruct();
	}
};
__decorate$39([autobind], UITooltip.prototype, "__onMouseLeave", null);
__decorate$39([autobind], UITooltip.prototype, "__onMouseEnter", null);
__decorate$39([autobind], UITooltip.prototype, "__hide", null);
__decorate$39([autobind], UITooltip.prototype, "__hideDelay", null);
UITooltip = UITooltip_1 = __decorate$39([component], UITooltip);
//#endregion
//#region node_modules/jodit/esm/core/ui/button/index.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
//#endregion
//#region node_modules/jodit/esm/core/ui/form/block/block.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$38 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var UIBlock = class UIBlock extends UIGroup {
	/** @override */
	className() {
		return "UIBlock";
	}
	constructor(jodit, elements, options = { align: "left" }) {
		super(jodit, elements);
		this.options = options;
		this.setMod("align", this.options.align || "left");
		this.setMod("width", this.options.width || "");
		this.options.mod && this.setMod(this.options.mod, true);
		this.options.className && this.container.classList.add(this.options.className);
		attr(this.container, "data-ref", options.ref);
		attr(this.container, "ref", options.ref);
	}
};
UIBlock = __decorate$38([component], UIBlock);
//#endregion
//#region node_modules/jodit/esm/core/ui/form/validators/input.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var input_exports = /* @__PURE__ */ __exportAll({
	required: () => required$1,
	url: () => url
});
/**
* Input is required
*/
var required$1 = function(input) {
	if (!trim(input.value).length) {
		input.error = "Please fill out this field";
		return false;
	}
	return true;
};
/**
* Input value should be valid URL
*/
var url = function(input) {
	if (!isURL(trim(input.value))) {
		input.error = "Please enter a web address";
		return false;
	}
	return true;
};
//#endregion
//#region node_modules/jodit/esm/core/ui/form/validators/select.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Select is required
*/
var required = function(select) {
	if (!trim(select.value).length) {
		select.error = "Please fill out this field";
		return false;
	}
	return true;
};
//#endregion
//#region node_modules/jodit/esm/core/ui/form/validators/index.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
//#endregion
//#region node_modules/jodit/esm/core/ui/form/inputs/input/input.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$37 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var UIInput_1;
var UIInput = UIInput_1 = class UIInput extends UIElement {
	/** @override */
	className() {
		return "UIInput";
	}
	onChangeClear() {
		if (this.state.clearButton) Dom.after(this.nativeInput, this.clearButton);
		else Dom.safeRemove(this.clearButton);
	}
	onChangeClassName(ignore, oldClassName) {
		oldClassName && this.container.classList.remove(oldClassName);
		this.state.className && this.container.classList.add(this.state.className);
	}
	onChangeState() {
		this.name = this.state.name;
		const input = this.nativeInput, { name, icon, type, ref, required, placeholder, autocomplete, label } = this.state;
		attr(input, "name", name);
		attr(input, "type", type);
		attr(input, "data-ref", ref || name);
		attr(input, "ref", ref || name);
		attr(input, "required", required || null);
		attr(input, "autocomplete", !autocomplete ? "off" : null);
		attr(input, "placeholder", placeholder ? this.j.i18n(placeholder) : "");
		if (icon && Icon.exists(icon)) {
			Dom.before(input, this.icon);
			this.icon.innerHTML = Icon.get(icon);
		} else Dom.safeRemove(this.icon);
		if (label) {
			Dom.before(this.wrapper, this.label);
			this.label.innerText = this.j.i18n(label);
		} else Dom.safeRemove(this.label);
		this.updateValidators();
	}
	updateValidators() {
		var _a;
		this.validators.clear();
		if (this.state.required) this.validators.add(required$1);
		(_a = this.state.validators) === null || _a === void 0 || _a.forEach((name) => {
			const validator = input_exports[name];
			validator && this.validators.add(validator);
		});
	}
	set error(value) {
		this.setMod("has-error", Boolean(value));
		if (!value) Dom.safeRemove(this.__errorBox);
		else {
			this.__errorBox.innerText = this.j.i18n(value, this.j.i18n(this.state.label || ""));
			Dom.append(this.container, this.__errorBox);
		}
	}
	get value() {
		return this.nativeInput.value;
	}
	set value(value) {
		if (this.value !== value) {
			this.nativeInput.value = value;
			this.onChangeValue();
		}
	}
	/**
	* Call on every state value changed
	*/
	onChangeStateValue() {
		const value = this.state.value.toString();
		if (value !== this.value) this.value = value;
	}
	/**
	* Call on every native value changed
	*/
	onChangeValue() {
		var _a, _b;
		const { value } = this;
		if (this.state.value !== value) {
			this.state.value = value;
			this.j.e.fire(this, "change", value);
			(_b = (_a = this.state).onChange) === null || _b === void 0 || _b.call(_a, value);
		}
	}
	validate() {
		this.error = "";
		const validate = toArray(this.validators).every((validator) => validator(this));
		this.__markInputInvalid();
		return validate;
	}
	__markInputInvalid() {
		var _a, _b, _c, _d;
		if (this.error) {
			attr(this.nativeInput, "aria-invalid", "true");
			(_b = (_a = this.nativeInput).setCustomValidity) === null || _b === void 0 || _b.call(_a, this.error);
		} else {
			attr(this.nativeInput, "aria-invalid", null);
			(_d = (_c = this.nativeInput).setCustomValidity) === null || _d === void 0 || _d.call(_c, "");
		}
	}
	/** @override **/
	createContainer(options) {
		const container = super.createContainer();
		this.wrapper = this.j.c.div(this.getFullElName("wrapper"));
		if (!this.nativeInput) this.nativeInput = this.createNativeInput();
		const { nativeInput } = this;
		nativeInput.classList.add(this.getFullElName("input"));
		Dom.append(this.wrapper, nativeInput);
		Dom.append(container, this.wrapper);
		attr(nativeInput, "dir", this.j.o.direction || "auto");
		return container;
	}
	/**
	* Create native input element
	*/
	createNativeInput(options) {
		return this.j.create.element("input");
	}
	/** @override **/
	constructor(jodit, options) {
		super(jodit, options);
		this.label = this.j.c.span(this.getFullElName("label"));
		this.icon = this.j.c.span(this.getFullElName("icon"));
		this.clearButton = this.j.c.span(this.getFullElName("clear"), Icon.get("cancel"));
		this.state = { ...UIInput_1.defaultState };
		this.__errorBox = this.j.c.span(this.getFullElName("error"));
		this.validators = /* @__PURE__ */ new Set([]);
		if ((options === null || options === void 0 ? void 0 : options.value) !== void 0) options.value = options.value.toString();
		Object.assign(this.state, options);
		if (this.state.clearButton !== void 0) {
			this.j.e.on(this.clearButton, "click", (e) => {
				e.preventDefault();
				this.nativeInput.value = "";
				this.j.e.fire(this.nativeInput, "input");
				this.focus();
			}).on(this.nativeInput, "input", () => {
				this.state.clearButton = Boolean(this.value.length);
			});
			this.state.clearButton = Boolean(this.value.length);
		}
		this.j.e.on(this.nativeInput, "focus blur", () => {
			this.onChangeFocus();
		}).on(this.nativeInput, "input change", this.onChangeValue);
		this.onChangeState();
		this.onChangeClassName();
		this.onChangeStateValue();
	}
	focus() {
		this.nativeInput.focus();
	}
	get isFocused() {
		return this.nativeInput === this.j.od.activeElement;
	}
	/**
	* Set `focused` mod on change focus
	*/
	onChangeFocus() {
		this.setMod("focused", this.isFocused);
	}
};
UIInput.defaultState = {
	className: "",
	autocomplete: true,
	name: "",
	value: "",
	icon: "",
	label: "",
	ref: "",
	type: "text",
	placeholder: "",
	required: false,
	validators: []
};
__decorate$37([watch("state.clearButton")], UIInput.prototype, "onChangeClear", null);
__decorate$37([watch("state.className")], UIInput.prototype, "onChangeClassName", null);
__decorate$37([watch([
	"state.name",
	"state.type",
	"state.label",
	"state.placeholder",
	"state.autocomplete",
	"state.icon"
], { immediately: false }), debounce()], UIInput.prototype, "onChangeState", null);
__decorate$37([watch("state.value")], UIInput.prototype, "onChangeStateValue", null);
__decorate$37([autobind], UIInput.prototype, "onChangeValue", null);
UIInput = UIInput_1 = __decorate$37([component], UIInput);
//#endregion
//#region node_modules/jodit/esm/core/ui/form/inputs/select/select.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$36 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var UISelect_1;
var UISelect = UISelect_1 = class UISelect extends UIInput {
	/** @override */
	className() {
		return "UISelect";
	}
	/** @override **/
	createContainer(state) {
		var _a;
		const container = super.createContainer(state);
		const { j } = this, { nativeInput } = this;
		const opt = () => j.create.element("option");
		if (state.placeholder !== void 0) {
			const option = opt();
			option.value = "";
			option.text = j.i18n(state.placeholder);
			nativeInput.add(option);
		}
		(_a = state.options) === null || _a === void 0 || _a.forEach((element) => {
			const option = opt();
			option.value = element.value.toString();
			option.text = j.i18n(element.text);
			nativeInput.add(option);
		});
		if (state.size && state.size > 0) attr(nativeInput, "size", state.size);
		if (state.multiple) attr(nativeInput, "multiple", "");
		return container;
	}
	/** @override **/
	createNativeInput() {
		return this.j.create.element("select");
	}
	/** @override **/
	updateValidators() {
		super.updateValidators();
		if (this.state.required) {
			this.validators.delete(required$1);
			this.validators.add(required);
		}
	}
	constructor(jodit, state) {
		super(jodit, state);
		/** @override */
		this.state = { ...UISelect_1.defaultState };
		Object.assign(this.state, state);
	}
};
/** @override */
UISelect.defaultState = {
	...UIInput.defaultState,
	options: [],
	size: 1,
	multiple: false
};
UISelect = UISelect_1 = __decorate$36([component], UISelect);
//#endregion
//#region node_modules/jodit/esm/core/ui/form/form.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$35 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var UIForm = class UIForm extends UIGroup {
	/** @override */
	className() {
		return "UIForm";
	}
	submit() {
		this.j.e.fire(this.container, "submit");
	}
	validate() {
		const inputs = this.allChildren.filter((elm) => Component.isInstanceOf(elm, UIInput));
		for (const input of inputs) if (!input.validate()) return false;
		const selects = this.allChildren.filter((elm) => Component.isInstanceOf(elm, UISelect));
		for (const select of selects) if (!select.validate()) return false;
		return true;
	}
	onSubmit(handler) {
		this.j.e.on(this.container, "submit", () => {
			const inputs = this.allChildren.filter((elm) => Component.isInstanceOf(elm, UIInput));
			if (!this.validate()) return false;
			handler(inputs.reduce((res, item) => {
				res[item.state.name] = item.value;
				return res;
			}, {}));
			return false;
		});
		return this;
	}
	/** @override */
	createContainer() {
		const form = this.j.c.element("form");
		form.classList.add(this.componentName);
		attr(form, "dir", this.j.o.direction || "auto");
		attr(form, "novalidate", "");
		return form;
	}
	constructor(...args) {
		var _a, _b;
		super(...args);
		if ((_a = this.options) === null || _a === void 0 ? void 0 : _a.className) this.container.classList.add((_b = this.options) === null || _b === void 0 ? void 0 : _b.className);
	}
};
UIForm = __decorate$35([component], UIForm);
//#endregion
//#region node_modules/jodit/esm/core/ui/form/inputs/area/area.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$34 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var UITextArea_1;
var UITextArea = UITextArea_1 = class UITextArea extends UIInput {
	/** @override */
	className() {
		return "UITextArea";
	}
	createNativeInput(options) {
		return this.j.create.element("textarea");
	}
	constructor(jodit, state) {
		super(jodit, state);
		/** @override */
		this.state = { ...UITextArea_1.defaultState };
		Object.assign(this.state, state);
		if (this.state.resizable === false) css(this.nativeInput, "resize", "none");
	}
	onChangeStateSize() {
		const { size, resizable } = this.state;
		css(this.nativeInput, "resize", resizable ? "auto" : "none");
		this.nativeInput.rows = size !== null && size !== void 0 ? size : 5;
	}
};
/** @override */
UITextArea.defaultState = {
	...UIInput.defaultState,
	size: 5,
	resizable: true
};
__decorate$34([watch(["state.size", "state.resizable"])], UITextArea.prototype, "onChangeStateSize", null);
UITextArea = UITextArea_1 = __decorate$34([component], UITextArea);
//#endregion
//#region node_modules/jodit/esm/core/ui/form/inputs/checkbox/checkbox.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$33 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var UICheckbox_1;
var UICheckbox = UICheckbox_1 = class UICheckbox extends UIInput {
	/** @override */
	className() {
		return "UICheckbox";
	}
	/** @override */
	render() {
		return this.j.c.element("label", { className: this.componentName });
	}
	/** @override **/
	constructor(jodit, options) {
		super(jodit, {
			...options,
			type: "checkbox"
		});
		/** @override */
		this.state = { ...UICheckbox_1.defaultState };
		Object.assign(this.state, options);
	}
	onChangeChecked() {
		this.value = this.state.checked.toString();
		this.nativeInput.checked = this.state.checked;
		this.setMod("checked", this.state.checked);
	}
	onChangeNativeCheckBox() {
		this.state.checked = this.nativeInput.checked;
	}
	onChangeSwitch() {
		this.setMod("switch", this.state.switch);
		let slider = this.getElm("switch-slider");
		if (this.state.switch) {
			if (!slider) slider = this.j.c.div(this.getFullElName("switch-slider"));
			Dom.after(this.nativeInput, slider);
		} else Dom.safeRemove(slider);
	}
};
/** @override */
UICheckbox.defaultState = {
	...UIInput.defaultState,
	checked: false,
	switch: false
};
__decorate$33([watch("state.checked"), hook("ready")], UICheckbox.prototype, "onChangeChecked", null);
__decorate$33([watch("nativeInput:change")], UICheckbox.prototype, "onChangeNativeCheckBox", null);
__decorate$33([watch("state.switch"), hook("ready")], UICheckbox.prototype, "onChangeSwitch", null);
UICheckbox = UICheckbox_1 = __decorate$33([component], UICheckbox);
//#endregion
//#region node_modules/jodit/esm/core/ui/form/inputs/file/file.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$32 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var UIFileInput = class UIFileInput extends UIInput {
	/** @override */
	className() {
		return "UIFileInput";
	}
	createContainer(options) {
		this.button = new UIButton(this.j, {
			tooltip: options.tooltip,
			icon: { name: "plus" }
		});
		const { container } = this.button;
		if (!this.nativeInput) this.nativeInput = this.createNativeInput(options);
		const { nativeInput } = this;
		nativeInput.classList.add(this.getFullElName("input"));
		container.classList.add(this.componentName);
		Dom.append(container, nativeInput);
		return container;
	}
	createNativeInput(options) {
		return this.j.create.fromHTML(`<input
			type="file"
			accept="${options.onlyImages ? "image/*" : "*"}"
			tabindex="-1"
			dir="auto"
			multiple=""
		/>`);
	}
	constructor(jodit, options) {
		super(jodit, {
			type: "file",
			...options
		});
		this.state = {
			...UIInput.defaultState,
			type: "file",
			onlyImages: true
		};
	}
};
UIFileInput = __decorate$32([component], UIFileInput);
//#endregion
//#region node_modules/jodit/esm/core/ui/form/inputs/index.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
//#endregion
//#region node_modules/jodit/esm/core/ui/form/index.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
//#endregion
//#region node_modules/jodit/esm/core/ui/group/separator.js
/**
* @module ui/group
*/
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$31 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var UISeparator = class UISeparator extends UIElement {
	className() {
		return "UISeparator";
	}
};
UISeparator = __decorate$31([component], UISeparator);
//#endregion
//#region node_modules/jodit/esm/core/ui/group/spacer.js
/**
* @module ui/group
*/
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$30 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var UISpacer = class UISpacer extends UIElement {
	className() {
		return "UISpacer";
	}
};
UISpacer = __decorate$30([component], UISpacer);
//#endregion
//#region node_modules/jodit/esm/core/ui/helpers/buttons.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @private
*/
var isButtonGroup = (item) => {
	return isArray(item.buttons);
};
//#endregion
//#region node_modules/jodit/esm/core/ui/helpers/get-control-type.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Get control for button name
* @private
*/
function getControlType(button, controls) {
	let buttonControl;
	if (!controls) controls = Config.defaultOptions.controls;
	if (!isString(button)) {
		buttonControl = {
			name: "empty",
			...ConfigFlatten(button)
		};
		if (controls[buttonControl.name] !== void 0) buttonControl = {
			...ConfigFlatten(controls[buttonControl.name]),
			...ConfigFlatten(buttonControl)
		};
	} else buttonControl = findControlType(button, controls) || {
		name: button,
		command: button,
		tooltip: button
	};
	return buttonControl;
}
/**
* @private
*/
function findControlType(path, controls) {
	let [namespaceOrKey, key] = path.split(/\./);
	let store = controls;
	if (key != null) {
		if (controls[namespaceOrKey] !== void 0) store = controls[namespaceOrKey];
	} else key = namespaceOrKey;
	return store[key] ? {
		name: key,
		...ConfigFlatten(store[key])
	} : void 0;
}
//#endregion
//#region node_modules/jodit/esm/core/ui/helpers/get-strong-control-types.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @private
*/
function getStrongControlTypes(items, controls) {
	return (isArray(items) ? items : keys(items, false).map((key) => {
		const value = items[key] || {};
		return ConfigProto({ name: key }, value);
	})).map((item) => getControlType(item, controls || Config.defaultOptions.controls));
}
//#endregion
//#region node_modules/jodit/esm/core/ui/group/list.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$29 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var UIList = class UIList extends UIGroup {
	constructor() {
		super(...arguments);
		this.mode = "horizontal";
		this.removeButtons = [];
	}
	className() {
		return "UIList";
	}
	getRole() {
		return "list";
	}
	__onChangeMode() {
		this.setMod("mode", this.mode);
	}
	/**
	* Make new group and append it in list of elements
	*/
	makeGroup(role) {
		return new UIGroup(this.jodit, [], role ? { role } : void 0);
	}
	/**
	* All buttons from list
	*/
	get buttons() {
		return this.allChildren.filter((elm) => Component.isInstanceOf(elm, UIButton));
	}
	/**
	* Helper for getting full plain button list
	*/
	getButtonsNames() {
		return this.buttons.map((a) => a instanceof UIButton && a.state.name || "").filter((a) => a !== "");
	}
	setRemoveButtons(removeButtons) {
		this.removeButtons = removeButtons || [];
		return this;
	}
	build(items, target = null) {
		items = splitArray(items);
		this.clear();
		let lastBtnSeparator = false;
		let line = this.makeGroup("group");
		this.append(line);
		line.setMod("line", true);
		let group;
		const addButton = (control) => {
			let elm = null;
			switch (control.name) {
				case "\n":
					line = this.makeGroup("group");
					line.setMod("line", true);
					group = this.makeGroup();
					line.append(group);
					this.append(line);
					break;
				case "|":
					if (!lastBtnSeparator) {
						lastBtnSeparator = true;
						elm = new UISeparator(this.j);
					}
					break;
				case "---": {
					group.setMod("before-spacer", true);
					const space = new UISpacer(this.j);
					line.append(space);
					group = this.makeGroup();
					line.append(group);
					lastBtnSeparator = false;
					break;
				}
				default:
					lastBtnSeparator = false;
					switch (control.component) {
						case "select":
							elm = this.makeSelect(control, target);
							break;
						default: elm = this.makeButton(control, target);
					}
			}
			if (elm) {
				if (!group) {
					group = this.makeGroup();
					line.append(group);
				}
				group.append(elm);
			}
		};
		const isNotRemoved = (b) => {
			var _a;
			return !this.removeButtons.includes(b.name) && (!b.isVisible || ((_a = b.isVisible) === null || _a === void 0 ? void 0 : _a.call(b, this.j, b)));
		};
		items.forEach((item) => {
			if (isButtonGroup(item)) {
				const buttons = item.buttons.filter((b) => b);
				if (buttons.length) {
					group = this.makeGroup();
					group.setMod("separated", true).setMod("group", item.group);
					line.append(group);
					getStrongControlTypes(buttons, this.j.o.controls).filter(isNotRemoved).forEach(addButton);
				}
			} else {
				if (!group) {
					group = this.makeGroup();
					line.append(group);
				}
				const control = getControlType(item, this.j.o.controls);
				isNotRemoved(control) && addButton(control);
			}
		});
		this.update();
		return this;
	}
	makeSelect(control, target) {
		throw new Error("Not implemented behaviour");
	}
	/**
	* Create button instance
	*/
	makeButton(control, target) {
		return new UIButton(this.j, { name: control.name });
	}
};
__decorate$29([watch("mode"), hook("ready")], UIList.prototype, "__onChangeMode", null);
UIList = __decorate$29([component], UIList);
//#endregion
//#region node_modules/jodit/esm/core/ui/group/index.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
//#endregion
//#region node_modules/jodit/esm/core/ui/popup/popup.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$28 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var EVENTS_FOR_AUTOCLOSE = [
	"escape",
	"cut",
	"delete",
	"backSpaceAfterDelete",
	"beforeCommandDelete"
];
var Popup = class Popup extends UIGroup {
	className() {
		return "Popup";
	}
	appendChildToContainer(childContainer) {
		const content = this.getElm("content");
		assert(content, "Content element should exist");
		Dom.append(content, childContainer);
	}
	updateParentElement(target) {
		if (target !== this && Component.isInstanceOf(target, Popup)) {
			this.__childrenPopups.forEach((popup) => {
				if (!target.closest(popup) && popup.isOpened) popup.close();
			});
			if (!this.__childrenPopups.has(target)) this.j.e.on(target, "beforeClose", () => {
				this.__childrenPopups.delete(target);
			});
			this.__childrenPopups.add(target);
		}
		return super.updateParentElement(target);
	}
	/**
	* Set popup content
	*/
	setContent(content) {
		if (this.allChildren.length) throw new Error("Remove children");
		if (Component.isInstanceOf(content, UIElement)) this.append(content);
		else {
			const elm = isString(content) ? this.j.c.fromHTML(content) : content;
			this.appendChildToContainer(elm);
		}
		this.updatePosition();
		return this;
	}
	/**
	* Open popup near with some bound
	*/
	open(getBound, keepPosition = false, parentContainer) {
		markOwner(this.jodit, this.container);
		this.container.classList.add(`jodit_theme_${this.jodit.o.theme}`);
		this.__calculateZIndex();
		this.isOpened = true;
		this.__addGlobalListeners();
		this.__targetBound = !keepPosition ? getBound : this.getKeepBound(getBound);
		if (parentContainer) Dom.append(parentContainer, this.container);
		else {
			const popupContainer = getContainer(this.jodit, Popup);
			if (parentContainer !== this.container.parentElement) Dom.append(popupContainer, this.container);
		}
		this.updatePosition();
		this.j.e.fire(this, "afterOpen");
		this.j.e.fire("afterOpenPopup", this);
		return this;
	}
	__calculateZIndex() {
		if (cssInline(this.container, "zIndex")) return;
		const checkView = (view) => {
			const zIndex = cssInline(view.container, "zIndex") || view.o.zIndex;
			if (zIndex) {
				this.setZIndex(1 + parseInt(zIndex.toString(), 10));
				return true;
			}
			return false;
		};
		const { j } = this;
		if (checkView(j)) return;
		let pe = this.parentElement;
		while (pe) {
			if (checkView(pe.j)) return;
			const parentZIndex = cssInline(pe.container, "zIndex");
			if (parentZIndex) {
				this.setZIndex(1 + parseInt(parentZIndex.toString(), 10));
				return;
			}
			if (!pe.parentElement && pe.container.parentElement) {
				const elm = UIElement.closestElement(pe.container.parentElement, UIElement);
				if (elm) {
					pe = elm;
					continue;
				}
			}
			pe = pe.parentElement;
		}
	}
	/**
	* Calculate static bound for point
	*/
	getKeepBound(getBound) {
		var _a;
		const oldBound = getBound();
		const elmUnderCursor = ((_a = this.j.o.shadowRoot) !== null && _a !== void 0 ? _a : this.od).elementFromPoint(oldBound.left, oldBound.top);
		if (!elmUnderCursor) return getBound;
		const element = Dom.isHTMLElement(elmUnderCursor) ? elmUnderCursor : elmUnderCursor.parentElement;
		const oldPos = position(element, this.j);
		return () => {
			const bound = getBound();
			const newPos = position(element, this.j);
			return {
				...bound,
				top: bound.top + (newPos.top - oldPos.top),
				left: bound.left + (newPos.left - oldPos.left)
			};
		};
	}
	/**
	* Update container position
	*/
	updatePosition() {
		if (!this.isOpened) return this;
		const [pos, strategy] = this.__calculatePosition(this.__targetBound(), this.viewBound(), position(this.container, this.j));
		this.setMod("strategy", strategy);
		const offset = getFixedPositionOffset(this.container);
		css(this.container, {
			left: pos.left - offset.x,
			top: pos.top - offset.y
		});
		this.__childrenPopups.forEach((popup) => popup.updatePosition());
		return this;
	}
	__throttleUpdatePosition() {
		this.updatePosition();
	}
	/**
	* Calculate start point
	*/
	__calculatePosition(target, view, container, defaultStrategy = this.strategy) {
		const x = {
			left: target.left,
			right: target.left - (container.width - target.width)
		}, y = {
			bottom: target.top + target.height,
			top: target.top - container.height
		};
		const list = Object.keys(x).reduce((keys, xKey) => keys.concat(Object.keys(y).map((yKey) => `${xKey}${ucfirst(yKey)}`)), []);
		const getPointByStrategy = (strategy) => {
			const [xKey, yKey] = kebabCase(strategy).split("-");
			return {
				left: x[xKey],
				top: y[yKey],
				width: container.width,
				height: container.height
			};
		};
		const getMatchStrategy = (inBox) => {
			let strategy = null;
			if (Popup.boxInView(getPointByStrategy(defaultStrategy), inBox)) strategy = defaultStrategy;
			else strategy = list.find((key) => {
				if (Popup.boxInView(getPointByStrategy(key), inBox)) return key;
			}) || null;
			return strategy;
		};
		let strategy = getMatchStrategy(position(this.j.container, this.j));
		if (!strategy || !Popup.boxInView(getPointByStrategy(strategy), view)) strategy = getMatchStrategy(view) || strategy || defaultStrategy;
		return [getPointByStrategy(strategy), strategy];
	}
	/**
	* Check if one box is inside second
	*/
	static boxInView(box, view) {
		return box.top - view.top >= -2 && box.left - view.left >= -2 && view.top + view.height - (box.top + box.height) >= -2 && view.left + view.width - (box.left + box.width) >= -2;
	}
	/**
	* Close popup
	*/
	close() {
		if (!this.isOpened) return this;
		this.isOpened = false;
		this.__childrenPopups.forEach((popup) => popup.close());
		this.j.e.fire(this, "beforeClose");
		this.j.e.fire("beforePopupClose", this);
		this.__removeGlobalListeners();
		const { activeElement } = this.od;
		if (activeElement && activeElement !== this.container && Dom.isOrContains(this.container, activeElement) && isFunction(activeElement.blur)) activeElement.blur();
		Dom.safeRemove(this.container);
		return this;
	}
	/**
	* Close popup if click was in outside
	*/
	__closeOnOutsideClick(e) {
		if (!this.isOpened || this.isOwnClick(e)) return;
		this.close();
	}
	isOwnClick(e) {
		if (!e.target) return false;
		const box = UIElement.closestElement(e.target, Popup);
		return Boolean(box && (this === box || box.closest(this)));
	}
	__addGlobalListeners() {
		const up = this.__throttleUpdatePosition, ow = this.ow;
		eventEmitter.on("closeAllPopups", this.close);
		if (this.smart) this.j.e.on(EVENTS_FOR_AUTOCLOSE, this.close).on("mousedown touchstart", this.__closeOnOutsideClick).on(ow, "mousedown touchstart", this.__closeOnOutsideClick);
		this.j.e.on("closeAllPopups", this.close).on("resize", up).on(this.container, "scroll mousewheel", up).on(ow, "scroll", up).on(ow, "resize", up);
		Dom.up(this.j.container, (box) => {
			box && this.j.e.on(box, "scroll mousewheel", up);
		});
	}
	__removeGlobalListeners() {
		const up = this.__throttleUpdatePosition, ow = this.ow;
		eventEmitter.off("closeAllPopups", this.close);
		if (this.smart) this.j.e.off(EVENTS_FOR_AUTOCLOSE, this.close).off("mousedown touchstart", this.__closeOnOutsideClick).off(ow, "mousedown touchstart", this.__closeOnOutsideClick);
		this.j.e.off("closeAllPopups", this.close).off("resize", up).off(this.container, "scroll mousewheel", up).off(ow, "scroll", up).off(ow, "resize", up);
		if (this.j.container.isConnected) Dom.up(this.j.container, (box) => {
			box && this.j.e.off(box, "scroll mousewheel", up);
		});
	}
	/**
	* Set ZIndex
	*/
	setZIndex(index) {
		css(this.container, "zIndex", index.toString());
	}
	constructor(jodit, smart = true) {
		super(jodit);
		this.smart = smart;
		this.isOpened = false;
		this.strategy = "leftBottom";
		this.viewBound = () => ({
			left: 0,
			top: 0,
			width: this.ow.innerWidth,
			height: this.ow.innerHeight
		});
		this.__childrenPopups = /* @__PURE__ */ new Set();
		attr(this.container, "role", "popup");
	}
	render() {
		return `<div>
			<div class="&__content"></div>
		</div>`;
	}
	/** @override **/
	destruct() {
		this.close();
		return super.destruct();
	}
};
__decorate$28([autobind], Popup.prototype, "updatePosition", null);
__decorate$28([throttle(10), autobind], Popup.prototype, "__throttleUpdatePosition", null);
__decorate$28([autobind], Popup.prototype, "close", null);
__decorate$28([autobind], Popup.prototype, "__closeOnOutsideClick", null);
//#endregion
//#region node_modules/jodit/esm/core/ui/popup/index.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
//#endregion
//#region node_modules/jodit/esm/core/ui/progress-bar/progress-bar.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var ProgressBar = class ProgressBar extends UIElement {
	constructor() {
		super(...arguments);
		this.__animationElement = null;
	}
	/** @override */
	className() {
		return "ProgressBar";
	}
	/** @override */
	render() {
		return "<div><div></div></div>";
	}
	/**
	* Show progress bar
	*/
	show() {
		const container = this.j.workplace || this.j.container;
		Dom.append(container, this.container);
		return this;
	}
	hide() {
		Dom.safeRemove(this.container);
		return this;
	}
	progress(percentage) {
		css(this.container, "width", percentage.toFixed(2) + "%");
		return this;
	}
	showFileUploadAnimation(from, to) {
		this.__cleanUpAnimation();
		const box = getContainer(this.j, ProgressBar);
		const pos = position(this.j.container, this.j);
		const el = this.j.c.div(this.getFullElName("file-animation"));
		const iconSvg = Icon.get("file", "");
		if (iconSvg) el.innerHTML = iconSvg;
		const start = from !== null && from !== void 0 ? from : {
			x: pos.width / 2,
			y: 0
		};
		const end = to !== null && to !== void 0 ? to : {
			x: start.x + 60,
			y: start.y - 80
		};
		css(el, {
			left: pos.left + start.x,
			top: pos.top + start.y
		});
		Dom.append(box, el);
		this.__animationElement = el;
		el.offsetWidth;
		css(el, {
			left: pos.left + end.x,
			top: pos.top + end.y,
			opacity: 0,
			transform: "scale(0.4)"
		});
		const onEnd = () => {
			el.removeEventListener("transitionend", onEnd);
			this.__cleanUpAnimation();
		};
		el.addEventListener("transitionend", onEnd);
	}
	__cleanUpAnimation() {
		if (this.__animationElement) {
			Dom.safeRemove(this.__animationElement);
			this.__animationElement = null;
		}
	}
	destruct() {
		this.__cleanUpAnimation();
		this.hide();
		return super.destruct();
	}
};
//#endregion
//#region node_modules/jodit/esm/core/ui/index.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
//#endregion
//#region node_modules/jodit/esm/core/create/create.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var Create = class {
	get doc() {
		return isFunction(this.document) ? this.document() : this.document;
	}
	constructor(document, createAttributes) {
		this.document = document;
		this.createAttributes = createAttributes;
	}
	element(tagName, childrenOrAttributes, children) {
		const elm = this.doc.createElement(tagName.toLowerCase());
		this.applyCreateAttributes(elm);
		if (childrenOrAttributes) if (isPlainObject(childrenOrAttributes)) attr(elm, childrenOrAttributes);
		else children = childrenOrAttributes;
		if (children) asArray(children).forEach((child) => Dom.append(elm, isString(child) ? this.fromHTML(child) : child));
		return elm;
	}
	div(className, childrenOrAttributes, children) {
		const div = this.element("div", childrenOrAttributes, children);
		if (className) div.className = className;
		return div;
	}
	sandbox() {
		var _a;
		const iframe = this.element("iframe", { sandbox: "allow-same-origin" });
		Dom.append(this.doc.body, iframe);
		const doc = (_a = iframe.contentWindow) === null || _a === void 0 ? void 0 : _a.document;
		assert(doc, "iframe.contentWindow.document");
		if (!doc) throw Error("Iframe error");
		doc.open();
		doc.write("<!DOCTYPE html><html><head></head><body></body></html>");
		doc.close();
		return [doc.body, iframe];
	}
	span(className, childrenOrAttributes, children) {
		const span = this.element("span", childrenOrAttributes, children);
		if (className) span.className = className;
		return span;
	}
	a(className, childrenOrAttributes, children) {
		const a = this.element("a", childrenOrAttributes, children);
		if (className) a.className = className;
		return a;
	}
	/**
	* Create text node
	*/
	text(value) {
		return this.doc.createTextNode(value);
	}
	/**
	* Create invisible text node
	*/
	fake() {
		return this.text("﻿");
	}
	/**
	* Create HTML Document fragment element
	*/
	fragment() {
		return this.doc.createDocumentFragment();
	}
	/**
	* Create a DOM element from HTML text
	*
	// eslint-disable-next-line tsdoc/syntax
	* @param refsToggleElement - State dictionary in which you can set the visibility of some of the elements
	* ```js
	* const editor = Jodit.make('#editor');
	* editor.createInside.fromHTML(`<div>
	*   <input name="name" ref="name"/>
	*   <input name="email" ref="email"/>
	* </div>`, {
	*   name: true,
	*   email: false
	* });
	* ```
	*/
	fromHTML(html, refsToggleElement) {
		const div = this.div();
		div.innerHTML = html.toString();
		const child = div.firstChild !== div.lastChild || !div.firstChild ? div : div.firstChild;
		Dom.safeRemove(child);
		if (refsToggleElement) {
			const refElements = refs(child);
			Object.keys(refsToggleElement).forEach((key) => {
				const elm = refElements[key];
				if (elm && refsToggleElement[key] === false) Dom.hide(elm);
			});
		}
		return child;
	}
	/**
	* Apply to element `createAttributes` options
	*/
	applyCreateAttributes(elm) {
		if (this.createAttributes) {
			const ca = this.createAttributes;
			if (ca && ca[elm.tagName.toLowerCase()]) {
				const attrsOpt = ca[elm.tagName.toLowerCase()];
				if (isFunction(attrsOpt)) attrsOpt(elm);
				else if (isPlainObject(attrsOpt)) attr(elm, attrsOpt);
			}
		}
	}
};
//#endregion
//#region node_modules/jodit/esm/core/event-emitter/index.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
//#endregion
//#region node_modules/jodit/esm/core/storage/engines/indexed-db-provider.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Persistent storage using IndexedDB
*/
var IndexedDBProvider = class {
	constructor(dbName = "JoditDB", storeName = "keyValueStore") {
		this.dbName = dbName;
		this.dbPromise = null;
		this.DB_VERSION = 1;
		this.storeName = storeName;
	}
	/**
	* Initialize or get the database connection
	*/
	getDB() {
		if (this.dbPromise) return this.dbPromise;
		this.dbPromise = new Promise((resolve, reject) => {
			const request = indexedDB.open(this.dbName, this.DB_VERSION);
			request.onerror = () => {
				reject(request.error);
			};
			request.onsuccess = () => {
				resolve(request.result);
			};
			request.onupgradeneeded = (event) => {
				const db = event.target.result;
				if (!db.objectStoreNames.contains(this.storeName)) db.createObjectStore(this.storeName);
			};
		});
		return this.dbPromise;
	}
	/**
	* Perform a transaction on the store
	*/
	async performTransaction(mode, callback) {
		try {
			const request = callback((await this.getDB()).transaction([this.storeName], mode).objectStore(this.storeName));
			return new Promise((resolve, reject) => {
				request.onsuccess = () => {
					resolve(request.result);
				};
				request.onerror = () => {
					reject(request.error);
				};
			});
		} catch (error) {
			return Promise.reject(error);
		}
	}
	async set(key, value) {
		try {
			await this.performTransaction("readwrite", (store) => store.put(value, key));
		} catch (e) {}
		return this;
	}
	async delete(key) {
		try {
			await this.performTransaction("readwrite", (store) => store.delete(key));
		} catch (_a) {}
		return this;
	}
	async get(key) {
		try {
			return await this.performTransaction("readonly", (store) => store.get(key));
		} catch (_a) {
			return;
		}
	}
	async exists(key) {
		try {
			return await this.performTransaction("readonly", (store) => store.get(key)) !== void 0;
		} catch (_a) {
			return false;
		}
	}
	async clear() {
		try {
			await this.performTransaction("readwrite", (store) => store.clear());
		} catch (_a) {}
		return this;
	}
	/**
	* Close the database connection
	*/
	async close() {
		if (this.dbPromise) {
			try {
				(await this.dbPromise).close();
			} catch (_a) {}
			this.dbPromise = null;
		}
	}
	/**
	* Get all keys in the store
	*/
	async keys() {
		try {
			let result = await this.performTransaction("readonly", (store) => store.getAllKeys());
			if (result && typeof result === "object" && "then" in result) result = await result;
			return result.map((k) => String(k));
		} catch (_a) {
			return [];
		}
	}
	/**
	* Get all values in the store
	*/
	async values() {
		try {
			let result = await this.performTransaction("readonly", (store) => store.getAll());
			if (result && typeof result === "object" && "then" in result) result = await result;
			return result;
		} catch (_a) {
			return [];
		}
	}
	/**
	* Get all entries (key-value pairs) in the store
	*/
	async entries() {
		try {
			const [keys, values] = await Promise.all([this.keys(), this.values()]);
			return keys.map((key, index) => [key, values[index]]);
		} catch (_a) {
			return [];
		}
	}
};
var cachedResult = null;
function clearUseIndexedDBCache() {
	cachedResult = null;
}
/**
* Check if IndexedDB is available
*/
async function canUseIndexedDB() {
	if (cachedResult != null) return cachedResult;
	try {
		if (typeof indexedDB === "undefined") {
			cachedResult = false;
			return false;
		}
		const tmpKey = "___Jodit___" + Math.random().toString();
		const request = indexedDB.open(tmpKey);
		cachedResult = await new Promise((resolve) => {
			request.onerror = () => {
				resolve(false);
			};
			request.onsuccess = () => {
				indexedDB.deleteDatabase(tmpKey);
				resolve(true);
			};
		});
		return cachedResult;
	} catch (_a) {
		cachedResult = false;
		return false;
	}
}
//#endregion
//#region node_modules/jodit/esm/core/storage/engines/local-storage-provider.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Check if user disable local storages/cookie etc.
*/
var canUsePersistentStorage = (strategy = "localStorage") => {
	const cache = /* @__PURE__ */ new Map();
	return (() => {
		if (cache.has(strategy)) return cache.get(strategy);
		const tmpKey = "___Jodit___" + Math.random().toString();
		const storage = strategy === "sessionStorage" ? sessionStorage : localStorage;
		try {
			storage.setItem(tmpKey, "1");
			const result = storage.getItem(tmpKey) === "1";
			storage.removeItem(tmpKey);
			cache.set(strategy, result);
			return result;
		} catch (_a) {}
		cache.set(strategy, false);
		return false;
	})();
};
/**
* Persistent storage in localStorage or sessionStorage
*/
var LocalStorageProvider = class {
	get storage() {
		return this.strategy === "sessionStorage" ? sessionStorage : localStorage;
	}
	set(key, value) {
		try {
			const buffer = this.storage.getItem(this.rootKey);
			const json = buffer ? JSON.parse(buffer) : {};
			json[key] = value;
			this.storage.setItem(this.rootKey, JSON.stringify(json));
		} catch (_a) {}
		return this;
	}
	delete(key) {
		try {
			const buffer = this.storage.getItem(this.rootKey);
			if (buffer) {
				const json = JSON.parse(buffer);
				delete json[key];
				this.storage.setItem(this.rootKey, JSON.stringify(json));
			}
		} catch (_a) {}
		return this;
	}
	get(key) {
		try {
			const buffer = this.storage.getItem(this.rootKey);
			const json = buffer ? JSON.parse(buffer) : {};
			return json[key] !== void 0 ? json[key] : void 0;
		} catch (_a) {}
	}
	exists(key) {
		return this.get(key) != null;
	}
	constructor(rootKey, strategy = "localStorage") {
		this.rootKey = rootKey;
		this.strategy = strategy;
	}
	clear() {
		try {
			this.storage.removeItem(this.rootKey);
		} catch (_a) {}
		return this;
	}
};
//#endregion
//#region node_modules/jodit/esm/core/storage/engines/memory-storage-provider.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var MemoryStorageProvider = class {
	constructor() {
		this.data = /* @__PURE__ */ new Map();
	}
	set(key, value) {
		this.data.set(key, value);
		return this;
	}
	delete(key) {
		this.data.delete(key);
		return this;
	}
	get(key) {
		return this.data.get(key);
	}
	exists(key) {
		return this.data.has(key);
	}
	clear() {
		this.data.clear();
		return this;
	}
};
//#endregion
//#region node_modules/jodit/esm/core/storage/storage.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var StorageKey = "Jodit_";
var Storage = class Storage {
	set(key, value) {
		this.provider.set(camelCase(this.prefix + key), value);
		return this;
	}
	delete(key) {
		this.provider.delete(camelCase(this.prefix + key));
		return this;
	}
	get(key) {
		return this.provider.get(camelCase(this.prefix + key));
	}
	exists(key) {
		return this.provider.exists(camelCase(this.prefix + key));
	}
	clear() {
		this.provider.clear();
		return this;
	}
	constructor(provider, suffix) {
		this.provider = provider;
		this.prefix = StorageKey;
		if (suffix) this.prefix += suffix;
	}
	static makeStorage(persistentOrStrategy = false, suffix) {
		let provider;
		if (persistentOrStrategy === "localStorage" || persistentOrStrategy === "sessionStorage") {
			if (canUsePersistentStorage(persistentOrStrategy)) provider = new LocalStorageProvider(StorageKey + (suffix || ""), persistentOrStrategy);
		} else if (persistentOrStrategy === true) {
			if (canUsePersistentStorage("localStorage")) provider = new LocalStorageProvider(StorageKey + (suffix || ""));
		}
		if (!provider) provider = new MemoryStorageProvider();
		return new Storage(provider, suffix);
	}
};
//#endregion
//#region node_modules/jodit/esm/core/storage/async-storage.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var AsyncStorage = class AsyncStorage {
	constructor(provider, suffix) {
		this.provider = provider;
		this.prefix = StorageKey;
		if (suffix) this.prefix += suffix;
	}
	async set(key, value) {
		await (await this.provider).set(camelCase(this.prefix + key), value);
		return this;
	}
	async delete(key) {
		await (await this.provider).delete(camelCase(this.prefix + key));
		return this;
	}
	async get(key) {
		return (await this.provider).get(camelCase(this.prefix + key));
	}
	async exists(key) {
		return (await this.provider).exists(camelCase(this.prefix + key));
	}
	async clear() {
		await (await this.provider).clear();
		return this;
	}
	async close() {
		const provider = await this.provider;
		if ("close" in provider && typeof provider.close === "function") await provider.close();
	}
	static makeStorage(persistentOrStrategy = false, suffix, options) {
		let provider = void 0;
		let storage = null;
		const defaultProvider = options === null || options === void 0 ? void 0 : options.defaultProvider;
		if (defaultProvider != null) {
			if (defaultProvider === "local") provider = canUsePersistentStorage("localStorage") ? new LocalStorageProvider(StorageKey + (suffix || "")) : new MemoryStorageProvider();
			else if (defaultProvider === "memory") provider = new MemoryStorageProvider();
			else provider = defaultProvider;
			return new AsyncStorage(Promise.resolve(provider), suffix);
		}
		if (persistentOrStrategy === "localStorage" || persistentOrStrategy === "sessionStorage") {
			if (canUsePersistentStorage(persistentOrStrategy)) provider = new LocalStorageProvider(StorageKey + (suffix || ""), persistentOrStrategy);
		} else if (persistentOrStrategy === "indexedDB" || persistentOrStrategy === true) provider = canUseIndexedDB().then((canUse) => canUse ? new IndexedDBProvider(StorageKey + (suffix || ""), "keyValueStore") : new MemoryStorageProvider());
		if (!provider) provider = new MemoryStorageProvider();
		storage = new AsyncStorage(Promise.resolve(provider), suffix);
		return storage;
	}
};
//#endregion
//#region node_modules/jodit/esm/modules/messages/message.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$27 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var UIMessage = class UIMessage extends UIElement {
	className() {
		return "UIMessage";
	}
	constructor(jodit, options) {
		super(jodit);
		this.setMod("active", true);
		this.setMod("variant", options.variant);
		this.container.textContent = options.text;
	}
};
UIMessage = __decorate$27([component], UIMessage);
//#endregion
//#region node_modules/jodit/esm/modules/messages/messages.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$26 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
/**
* Plugin display pop-up messages in the lower right corner of the editor
*/
var UIMessages = class UIMessages extends UIGroup {
	className() {
		return "UIMessages";
	}
	constructor(jodit, __box, options = {
		defaultTimeout: 3e3,
		defaultOffset: 5
	}) {
		super(jodit);
		this.__box = __box;
		this.options = options;
		this.__messages = /* @__PURE__ */ new Set();
	}
	/**
	* Show popup info message in the lower right corner of the container
	* ```js
	* const jodit = Jodit.make('#editor');
	* jodit.info('Hello world', 3000);
	* ```
	*/
	info(text, timeout) {
		this.__message(text, "info", timeout);
	}
	/**
	* Show popup success message in the lower right corner of the container
	* ```js
	* const jodit = Jodit.make('#editor');
	* jodit.success('Hello world', 3000);
	* ```
	*/
	success(text, timeout) {
		this.__message(text, "success", timeout);
	}
	/**
	* Show popup error message in the lower right corner of the container
	* ```js
	* const jodit = Jodit.make('#editor');
	* jodit.error('Hello world', 3000);
	* ```
	*/
	error(text, timeout) {
		this.__message(text, "error", timeout);
	}
	/**
	* Show popup message in the lower right corner of the container
	* ```js
	* const jodit = Jodit.make('#editor');
	* jodit.message('Hello world', 'info', 3000);
	* ```
	*/
	message(text, variant, timeout) {
		this.__message(text, variant, timeout);
	}
	__message(text, variant = "info", timeout) {
		const key = text + ":" + variant;
		if (this.__messages.has(key)) {
			this.async.updateTimeout(key, timeout || this.options.defaultTimeout);
			return;
		}
		if (!this.__box) throw new Error("Container is not defined: " + key);
		Dom.append(this.__box, this.container);
		const msg = new UIMessage(this.j, {
			text,
			variant
		});
		this.append(msg);
		this.__calcOffsets();
		this.__messages.add(key);
		const remove = this.__getRemoveCallback(msg, key);
		this.j.e.on(msg.container, "pointerdown", remove);
		this.async.setTimeout(remove, {
			label: key,
			timeout: timeout || this.options.defaultTimeout
		});
	}
	__getRemoveCallback(msg, key) {
		const remove = (e) => {
			e && e.preventDefault();
			if (msg.isInDestruct) return;
			this.async.clearTimeout(key);
			this.j.e.off(msg.container, "pointerdown", remove);
			this.__messages.delete(key);
			msg.setMod("active", false);
			this.async.setTimeout(() => {
				this.remove(msg);
				msg.destruct();
				this.__calcOffsets();
			}, 300);
		};
		return remove;
	}
	__calcOffsets() {
		let height = 5;
		this.elements.forEach((elm) => {
			css(elm.container, "bottom", height + "px");
			height += elm.container.offsetHeight + this.options.defaultOffset;
		});
	}
};
UIMessages = __decorate$26([component], UIMessages);
//#endregion
//#region node_modules/jodit/esm/core/view/view.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$25 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var View_1;
var View = View_1 = class View extends Component {
	constructor(options, isJodit = false) {
		var _a;
		super();
		this.isJodit = isJodit;
		this.isView = true;
		this.parent = null;
		this.mods = {};
		/**
		* All created ViewComponent inside this view
		*/
		this.components = /* @__PURE__ */ new Set();
		this.OPTIONS = View_1.defaultOptions;
		this.__isFullSize = false;
		this.__whoLocked = "";
		this.isLockedNotBy = (name) => this.isLocked && this.__whoLocked !== name;
		this.__modulesInstances = /* @__PURE__ */ new Map();
		this.id = (/* @__PURE__ */ new Date()).getTime().toString();
		this.initOptions(options);
		this.initOwners();
		this.events = (_a = options === null || options === void 0 ? void 0 : options.eventEmmiter) !== null && _a !== void 0 ? _a : new EventEmitter(this.od);
		this.create = new Create(this.od);
		this.__container = this.c.div(`jodit ${this.componentName}`);
	}
	/**
	* Get a path for loading extra staff
	*/
	get basePath() {
		if (this.o.basePath) return this.o.basePath;
		return BASE_PATH;
	}
	/**
	* Plugin autoloader should load minified version of the file
	*/
	get minified() {
		if (this.o.minified !== void 0) return this.o.minified;
		return BASE_PATH_IS_MIN;
	}
	/**
	* Return a default timeout period in milliseconds for some debounce or throttle functions.
	* By default, `{history.timeout}` options
	*/
	get defaultTimeout() {
		return isVoid(this.o.defaultTimeout) ? 100 : this.o.defaultTimeout;
	}
	/**
	* Some extra data inside editor
	* @see copyformat plugin
	*/
	get buffer() {
		return Storage.makeStorage();
	}
	get message() {
		return this.getMessageModule(this.container);
	}
	getMessageModule(container) {
		return new UIMessages(this, container);
	}
	/**
	* Container for persistent set/get value
	* @deprecated Use asyncStorage instead
	*/
	get storage() {
		return Storage.makeStorage(true, this.id);
	}
	/**
	* Container for persistent set/get value
	*/
	get asyncStorage() {
		return AsyncStorage.makeStorage(true, this.id, this.o.asyncStorage);
	}
	/**
	* Short alias for `create`
	*/
	get c() {
		return this.create;
	}
	get container() {
		return this.__container;
	}
	set container(container) {
		this.__container = container;
	}
	/**
	* Short alias for `events`
	*/
	get e() {
		return this.events;
	}
	/**
	* progress_bar Progress bar
	*/
	get progressbar() {
		return new ProgressBar(this);
	}
	get options() {
		return this.__options;
	}
	set options(options) {
		this.__options = options;
	}
	/**
	* Short alias for options
	*/
	get o() {
		return this.options;
	}
	/**
	* Internationalization method. Uses Jodit.lang object
	*/
	i18n(text, ...params) {
		return i18n(text, params, this.options);
	}
	toggleFullSize(isFullSize) {
		if (isFullSize === void 0) isFullSize = !this.__isFullSize;
		if (isFullSize === this.__isFullSize) return;
		this.__isFullSize = isFullSize;
		this.e.fire("toggleFullSize", isFullSize);
	}
	/**
	* View is locked
	*/
	get isLocked() {
		return this.__whoLocked !== "";
	}
	/**
	* Disable selecting
	*/
	lock(name = "any") {
		if (!this.isLocked) {
			this.__whoLocked = name;
			return true;
		}
		return false;
	}
	/**
	* Enable selecting
	*/
	unlock() {
		if (this.isLocked) {
			this.__whoLocked = "";
			return true;
		}
		return false;
	}
	/**
	* View is in fullSize
	*/
	get isFullSize() {
		return this.__isFullSize;
	}
	/**
	* Return current version
	*/
	getVersion() {
		return View_1.version;
	}
	static getVersion() {
		return View_1.version;
	}
	/** @override */
	initOptions(options) {
		this.options = ConfigProto(options || {}, ConfigProto(this.options || {}, View_1.defaultOptions));
	}
	/**
	* Can change ownerWindow here
	*/
	initOwners() {
		var _a;
		this.ownerWindow = (_a = this.o.ownerWindow) !== null && _a !== void 0 ? _a : window;
	}
	/**
	* Add option's event handlers in emitter
	*/
	attachEvents(options) {
		if (!options) return;
		const e = options === null || options === void 0 ? void 0 : options.events;
		e && Object.keys(e).forEach((key) => this.e.on(key, e[key]));
	}
	getInstance(moduleNameOrConstructor, options) {
		const moduleName = isFunction(moduleNameOrConstructor) ? moduleNameOrConstructor.prototype.className() : moduleNameOrConstructor;
		const instance = this.e.fire(camelCase("getInstance_" + moduleName), options);
		if (instance) return instance;
		const module = isFunction(moduleNameOrConstructor) ? moduleNameOrConstructor : modules[moduleName], mi = this.__modulesInstances;
		if (!isFunction(module)) throw error("Need real module name");
		if (!mi.has(moduleName)) {
			const instance = module.prototype instanceof ViewComponent ? new module(this, options) : new module(options);
			this.components.add(instance);
			mi.set(moduleName, instance);
		}
		return mi.get(moduleName);
	}
	/** Add some element to box */
	addDisclaimer(elm) {
		Dom.append(this.container, elm);
	}
	/**
	* Call before destruct
	*/
	beforeDestruct() {
		this.e.fire(STATUSES.beforeDestruct, this);
		this.components.forEach((component) => {
			if (isDestructable(component) && !component.isInDestruct) component.destruct();
		});
		this.components.clear();
	}
	/** @override */
	destruct() {
		var _a, _b, _c, _d;
		if (this.isDestructed) return;
		(_a = cached(this, "progressbar")) === null || _a === void 0 || _a.destruct();
		(_b = cached(this, "message")) === null || _b === void 0 || _b.destruct();
		(_c = cached(this, "asyncStorage")) === null || _c === void 0 || _c.close();
		if (this.events) {
			this.events.destruct();
			this.events = void 0;
		}
		(_d = cached(this, "buffer")) === null || _d === void 0 || _d.clear();
		Dom.safeRemove(this.container);
		super.destruct();
	}
};
View.ES = ES;
View.version = APP_VERSION;
View.esNext = true;
View.esModern = true;
__decorate$25([cache], View.prototype, "buffer", null);
__decorate$25([cache], View.prototype, "message", null);
__decorate$25([cache], View.prototype, "storage", null);
__decorate$25([cache], View.prototype, "asyncStorage", null);
__decorate$25([cache], View.prototype, "c", null);
__decorate$25([cache], View.prototype, "e", null);
__decorate$25([cache], View.prototype, "progressbar", null);
__decorate$25([hook(STATUSES.beforeDestruct)], View.prototype, "beforeDestruct", null);
View = View_1 = __decorate$25([derive(Mods, Elms)], View);
View.defaultOptions = {
	extraButtons: [],
	cache: true,
	textIcons: false,
	namespace: "",
	removeButtons: [],
	zIndex: 100002,
	defaultTimeout: 100,
	fullsize: false,
	showTooltip: true,
	useNativeTooltip: false,
	buttons: [],
	globalFullSize: true,
	language: "auto"
};
//#endregion
//#region node_modules/jodit/esm/modules/toolbar/factory.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Collection factory
*/
function makeCollection(jodit, parentElement) {
	const ToolbarCollection = getComponentClass("ToolbarCollection");
	const ToolbarEditorCollection = getComponentClass("ToolbarEditorCollection");
	const collection = isJoditObject(jodit) ? new ToolbarEditorCollection(jodit) : new ToolbarCollection(jodit);
	if (jodit.o.textIcons) collection.container.classList.add("jodit_text_icons");
	if (parentElement) collection.parentElement = parentElement;
	if (jodit.o.toolbarButtonSize) collection.buttonSize = jodit.o.toolbarButtonSize;
	return collection;
}
/**
* Button factory
*/
function makeButton(jodit, control, target = null) {
	if (isFunction(control.getContent)) return new (getComponentClass("ToolbarContent"))(jodit, control, target);
	const button = new (getComponentClass("ToolbarButton"))(jodit, control, target);
	button.state.tabIndex = jodit.o.allowTabNavigation ? 0 : -1;
	return button;
}
function makeSelect(view, control, target = null) {
	return new (getComponentClass("ToolbarSelect"))(view, control, target);
}
//#endregion
//#region node_modules/jodit/esm/core/view/view-with-toolbar.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$24 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var ViewWithToolbar = class extends View {
	/**
	* Container for toolbar
	*/
	get toolbarContainer() {
		if (!this.o.fullsize && (isString(this.o.toolbar) || Dom.isHTMLElement(this.o.toolbar))) return resolveElement(this.o.toolbar, this.o.shadowRoot || this.od);
		this.o.toolbar && this.__appendToolbarBox();
		return this.__defaultToolbarContainer;
	}
	/**
	* Keep the toolbar box as the first child of the container, except that
	* children flagged with `data-jodit-above-toolbar` (e.g. the `above`
	* workplace slot used for presence bars and banners) stay above it.
	*/
	__appendToolbarBox() {
		const box = this.__defaultToolbarContainer;
		let anchor = this.container.firstElementChild;
		while (anchor && anchor !== box && anchor.hasAttribute("data-jodit-above-toolbar")) anchor = anchor.nextElementSibling;
		if (anchor !== box) anchor ? Dom.before(anchor, box) : Dom.append(this.container, box);
	}
	/**
	* Change panel container
	*/
	setPanel(element) {
		this.o.toolbar = element;
		this.buildToolbar();
	}
	/**
	* Helper for appended toolbar in its place
	*/
	buildToolbar() {
		var _a;
		if (!this.o.toolbar) return;
		const buttons = this.o.buttons ? splitArray(this.o.buttons) : [];
		(_a = this.toolbar) === null || _a === void 0 || _a.setRemoveButtons(this.o.removeButtons).build(buttons.concat(this.o.extraButtons || [])).appendTo(this.toolbarContainer);
	}
	getRegisteredButtonGroups() {
		return this.groupToButtons;
	}
	/**
	* Register button for a group
	*/
	registerButton(btn) {
		var _a;
		this.registeredButtons.add(btn);
		const group = (_a = btn.group) !== null && _a !== void 0 ? _a : "other";
		if (!this.groupToButtons[group]) this.groupToButtons[group] = [];
		if (btn.position != null) this.groupToButtons[group][btn.position] = btn.name;
		else this.groupToButtons[group].push(btn.name);
		return this;
	}
	/**
	* Remove button from a group
	*/
	unregisterButton(btn) {
		var _a;
		this.registeredButtons.delete(btn);
		const groupName = (_a = btn.group) !== null && _a !== void 0 ? _a : "other", group = this.groupToButtons[groupName];
		if (group) {
			const index = group.indexOf(btn.name);
			if (index !== -1) group.splice(index, 1);
			if (group.length === 0) delete this.groupToButtons[groupName];
		}
		return this;
	}
	/**
	* Prepare toolbar items and append buttons in groups
	*/
	beforeToolbarBuild(items) {
		if (Object.keys(this.groupToButtons).length) return items.map((item) => {
			if (isButtonGroup(item) && item.group && this.groupToButtons[item.group]) return {
				group: item.group,
				buttons: [...item.buttons, ...this.groupToButtons[item.group]]
			};
			return item;
		});
	}
	/** @override **/
	constructor(options, isJodit = false) {
		super(options, isJodit);
		this.toolbar = makeCollection(this);
		this.__defaultToolbarContainer = this.c.div("jodit-toolbar__box");
		this.registeredButtons = /* @__PURE__ */ new Set();
		this.groupToButtons = {};
		this.isJodit = false;
		this.__tooltip = new UITooltip(this);
		this.isJodit = isJodit;
		this.e.on("beforeToolbarBuild", this.beforeToolbarBuild);
	}
	destruct() {
		if (this.isDestructed) return;
		this.setStatus(STATUSES.beforeDestruct);
		this.e.off("beforeToolbarBuild", this.beforeToolbarBuild);
		this.__tooltip.destruct();
		this.toolbar.destruct();
		this.toolbar = void 0;
		super.destruct();
	}
};
__decorate$24([watch(":rebuildToolbar")], ViewWithToolbar.prototype, "buildToolbar", null);
__decorate$24([autobind], ViewWithToolbar.prototype, "beforeToolbarBuild", null);
//#endregion
//#region node_modules/jodit/esm/modules/dialog/dialog.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$23 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var Dialog_1;
Config.prototype.dialog = {
	namespace: "",
	extraButtons: [],
	/**
	* This dialog can resize by trigger
	*/
	resizable: true,
	/**
	* This dialog can move by header
	*/
	draggable: true,
	buttons: ["dialog.close"],
	removeButtons: [],
	toolbarButtonSize: "middle",
	zIndex: "inherit"
};
Config.prototype.controls.dialog = { close: {
	icon: "cancel",
	exec: (dialog) => {
		dialog.close();
	}
} };
/**
* Module to generate dialog windows
*/
var Dialog = Dialog_1 = class Dialog extends ViewWithToolbar {
	/** @override */
	className() {
		return "Dialog";
	}
	get destination() {
		const { popupRoot, shadowRoot } = this.o;
		if (popupRoot) return popupRoot;
		if (shadowRoot) return shadowRoot;
		return this.od.body;
	}
	setElements(root, elements) {
		const elements_list = [];
		asArray(elements).forEach((elm) => {
			if (isArray(elm)) {
				const div = this.c.div(this.getFullElName("column"));
				elements_list.push(div);
				Dom.append(root, div);
				return this.setElements(div, elm);
			}
			let element;
			if (isString(elm)) element = this.c.fromHTML(elm);
			else element = hasContainer(elm) ? elm.container : elm;
			elements_list.push(element);
			if (element.parentNode !== root) Dom.append(root, element);
		});
		toArray(root.childNodes).forEach((elm) => {
			if (elements_list.indexOf(elm) === -1) Dom.safeRemove(elm);
		});
	}
	__onMouseUp() {
		if (this.draggable || this.resizable) {
			this.__removeGlobalResizeListeners();
			this.draggable = false;
			this.resizable = false;
			this.unlockSelect();
			if (this.e) {
				this.__removeGlobalResizeListeners();
				/**
				* Fired when the dialog box is finished to resizing
				*/
				this.e.fire(this, "endResize endMove");
			}
		}
	}
	/**
	*
	*/
	__onHeaderMouseDown(e) {
		const target = e.target;
		if (!this.o.draggable || target && target.nodeName.match(/^(INPUT|SELECT)$/)) return;
		this.draggable = true;
		this.startX = e.clientX;
		this.startY = e.clientY;
		this.startPoint.x = css(this.dialog, "left");
		this.startPoint.y = css(this.dialog, "top");
		this.setMaxZIndex();
		if (e.cancelable) e.preventDefault();
		this.lockSelect();
		this.__addGlobalResizeListeners();
		if (this.e) {
			/**
			* Fired when dialog box is started moving
			*/
			this.e.fire(this, "startMove");
			this.e.fire("closeAllPopups");
		}
	}
	__onMouseMove(e) {
		if (this.draggable && this.o.draggable) {
			this.setPosition(this.startPoint.x + e.clientX - this.startX, this.startPoint.y + e.clientY - this.startY);
			if (this.e)
 /**
			* Fired when dialog box is moved
			*/
			this.e.fire(this, "move", e.clientX - this.startX, e.clientY - this.startY);
			e.stopImmediatePropagation();
		}
		if (this.resizable && this.o.resizable) {
			this.setSize(Math.max(this.startPoint.w + e.clientX - this.startX, this.minSize.w), Math.max(this.startPoint.h + e.clientY - this.startY, this.minSize.h));
			if (this.e)
 /**
			* Fired when dialog box is resized
			*/
			this.e.fire(this, "resizeDialog", e.clientX - this.startX, e.clientY - this.startY);
		}
	}
	__onEsc(e) {
		if (!this.o.closeOnEsc) return;
		if (this.isOpened && e.key === "Escape" && this.getMod("static") !== true) {
			const me = this.getMaxZIndexDialog();
			if (me) me.close();
			else this.close();
			e.stopImmediatePropagation();
		}
	}
	__onResizerMouseDown(e) {
		var _a, _b, _c;
		this.resizable = true;
		this.startX = e.clientX;
		this.startY = e.clientY;
		this.startPoint.w = this.dialog.offsetWidth;
		this.startPoint.h = this.dialog.offsetHeight;
		const header = this.getElm("header");
		const footer = this.getElm("footer");
		const content = this.getElm("content");
		const contentMinHeight = content ? parseFloat(this.ow.getComputedStyle(content).minHeight) || 0 : 0;
		this.minSize.w = isNumber(this.o.minWidth) ? this.o.minWidth : Math.max(100, (_a = footer === null || footer === void 0 ? void 0 : footer.scrollWidth) !== null && _a !== void 0 ? _a : 0);
		this.minSize.h = isNumber(this.o.minHeight) ? this.o.minHeight : ((_b = header === null || header === void 0 ? void 0 : header.offsetHeight) !== null && _b !== void 0 ? _b : 0) + ((_c = footer === null || footer === void 0 ? void 0 : footer.offsetHeight) !== null && _c !== void 0 ? _c : 0) + contentMinHeight + this.resizer.offsetHeight;
		this.lockSelect();
		this.__addGlobalResizeListeners();
		if (this.e)
 /**
		* Fired when dialog box is started resizing
		*/
		this.e.fire(this, "startResize");
	}
	__addGlobalResizeListeners() {
		const self = this;
		self.e.on(self.ow, "pointermove touchmove", self.__onMouseMove).on(self.ow, "pointerup touchend", self.__onMouseUp);
	}
	__removeGlobalResizeListeners() {
		const self = this;
		self.e.off(self.ow, "mousemove pointermove", self.__onMouseMove).off(self.ow, "mouseup pointerup", self.__onMouseUp);
	}
	/**
	* Specifies the size of the window
	*
	* @param w - The width of the window
	* @param h - The height of the window
	*/
	setSize(w, h) {
		if (w == null) w = this.dialog.offsetWidth;
		if (h == null) h = this.dialog.offsetHeight;
		css(this.dialog, {
			width: w,
			height: h
		});
		return this;
	}
	/**
	* Recalculate auto sizes
	*/
	calcAutoSize() {
		this.setSize("auto", "auto");
		this.setSize();
		return this;
	}
	/**
	* Specifies the position of the upper left corner of the window . If x and y are specified,
	* the window is centered on the center of the screen
	*
	* @param x - Position px Horizontal
	* @param y - Position px Vertical
	*/
	setPosition(x, y) {
		const w = this.ow.innerWidth, h = this.ow.innerHeight;
		let left = w / 2 - this.dialog.offsetWidth / 2, top = h / 2 - this.dialog.offsetHeight / 2;
		if (left < 0) left = 0;
		if (top < 0) top = 0;
		if (x !== void 0 && y !== void 0) {
			this.offsetX = x;
			this.offsetY = y;
			this.moved = Math.abs(x - left) > 100 || Math.abs(y - top) > 100;
		}
		css(this.dialog, {
			left: (x || left) + "px",
			top: (y || top) + "px"
		});
		return this;
	}
	/**
	* Specifies the dialog box title . It can take a string and an array of objects
	*
	* @param content - A string or an HTML element ,
	* or an array of strings and elements
	* @example
	* ```javascript
	* var dialog = new Jodi.modules.Dialog(parent);
	* dialog.setHeader('Hello world');
	* dialog.setHeader(['Hello world', '<button>OK</button>', $('<div>some</div>')]);
	* dialog.open();
	* ```
	*/
	setHeader(content) {
		this.setElements(this.dialogbox_header, content);
		return this;
	}
	/**
	* It specifies the contents of the dialog box. It can take a string and an array of objects
	*
	* @param content - A string or an HTML element ,
	* or an array of strings and elements
	* @example
	* ```javascript
	* var dialog = new Jodi.modules.Dialog(parent);
	* dialog.setHeader('Hello world');
	* dialog.setContent('<form onsubmit="alert(1);"><input type="text" /></form>');
	* dialog.open();
	* ```
	*/
	setContent(content) {
		this.setElements(this.dialogbox_content, content);
		return this;
	}
	/**
	* Sets the bottom of the dialog. It can take a string and an array of objects
	*
	* @param content - A string or an HTML element ,
	* or an array of strings and elements
	* @example
	* ```javascript
	* var dialog = new Jodi.modules.Dialog(parent);
	* dialog.setHeader('Hello world');
	* dialog.setContent('<form><input id="someText" type="text" /></form>');
	* dialog.setFooter([
	*  $('<a class="jodit-button">OK</a>').click(function () {
	*      alert($('someText').val())
	*      dialog.close();
	*  })
	* ]);
	* dialog.open();
	* ```
	*/
	setFooter(content) {
		this.setElements(this.dialogbox_footer, content);
		this.setMod("footer", Boolean(content));
		return this;
	}
	__openedSiblings() {
		const result = [];
		Dialog_1.__opened.forEach((dialog) => {
			if (dialog.destination === this.destination) result.push(dialog);
		});
		return result;
	}
	/**
	* Get zIndex from dialog
	*/
	getZIndex() {
		return parseInt(css(this.container, "zIndex"), 10) || 0;
	}
	/**
	* Get dialog instance with maximum z-index displaying it on top of all the dialog boxes
	*/
	getMaxZIndexDialog() {
		let maxZi = 0, res = this;
		this.__openedSiblings().forEach((dialog) => {
			const zIndex = dialog.getZIndex();
			if (zIndex > maxZi) {
				res = dialog;
				maxZi = zIndex;
			}
		});
		return res;
	}
	/**
	* Sets the maximum z-index dialog box, displaying it on top of all the dialog boxes
	*/
	setMaxZIndex() {
		if (this.getMod("static")) return;
		let maxZIndex = 20000004;
		this.__openedSiblings().forEach((dialog) => {
			maxZIndex = Math.max(dialog.getZIndex(), maxZIndex);
		});
		css(this.container, "zIndex", (maxZIndex + 1).toString());
	}
	/**
	* Expands the dialog on full browser window
	*/
	toggleFullSize(isFullSize) {
		if (isVoid(isFullSize)) isFullSize = !this.getMod("fullsize");
		this.setMod("fullsize", isFullSize);
		super.toggleFullSize(isFullSize);
	}
	/**
	* It opens a dialog box to center it, and causes the two event.
	*
	* @param contentOrClose - specifies the contents of the dialog box.
	* Can be false or undefined. see [[Dialog.setContent]]
	* @param title - specifies the title of the dialog box, @see setHeader
	* @param destroyAfterClose - true - After closing the window , the destructor will be called.
	* @param modal - true window will be opened in modal mode
	*/
	open(contentOrClose, titleOrModal, destroyAfterClose, modal) {
		eventEmitter.fire("closeAllPopups hideHelpers");
		/**
		* Called before the opening of the dialog box
		*/
		if (this.e.fire(this, "beforeOpen") === false) return this;
		if (isBoolean(contentOrClose)) destroyAfterClose = contentOrClose;
		if (isBoolean(titleOrModal)) modal = titleOrModal;
		this.destroyAfterClose = destroyAfterClose === true;
		const content = isBoolean(contentOrClose) ? void 0 : contentOrClose;
		const title = isBoolean(titleOrModal) ? void 0 : titleOrModal;
		if (title !== void 0) this.setHeader(title);
		if (content) this.setContent(content);
		this.setMod("active", true);
		this.isOpened = true;
		Dialog_1.__opened.add(this);
		this.setModal(modal);
		Dom.append(this.destination, this.container);
		if (this.getMod("static") !== true) {
			this.setPosition(this.offsetX, this.offsetY);
			this.setMaxZIndex();
		} else css(this.container, "zIndex", null);
		if (this.o.fullsize) this.toggleFullSize(true);
		/**
		* Called after the opening of the dialog box
		*/
		this.e.fire("afterOpen", this);
		return this;
	}
	/**
	* Set modal mode
	*/
	setModal(modal) {
		this.isModal = Boolean(modal);
		this.setMod("modal", this.isModal);
		return this;
	}
	/****
	* Closes the dialog box , if you want to call the method `destruct`
	*
	* @see destroy
	* @example
	* ```javascript
	* //You can close dialog two ways
	* var dialog = new Jodit.modules.Dialog();
	* dialog.open('Hello world!', 'Title');
	* var $close = dialog.create.fromHTML('<a href="#" style="float:left;" class="jodit-button">
	*     <i class="icon icon-check"></i>&nbsp;' + Jodit.prototype.i18n('Ok') + '</a>');
	* $close.addEventListener('click', function () {
	*     dialog.close();
	* });
	* dialog.setFooter($close);
	* // and second way, you can close dialog from content
	* dialog.open('<a onclick="var event = doc.createEvent('HTMLEvents'); event.initEvent('close_dialog', true, true);
	* this.dispatchEvent(event)">Close</a>', 'Title');
	* ```
	*/
	close() {
		if (this.isDestructed || !this.isOpened || this.getMod("static") === true) return this;
		const { e } = this;
		/**
		* Called up to close the window
		*/
		if (e.fire(this, "beforeClose") === false || e.fire("beforeClose", this) === false) return this;
		this.setMod("active", false);
		this.isOpened = false;
		Dialog_1.__opened.delete(this);
		if (this.isFullSize) this.toggleFullSize(false);
		Dom.safeRemove(this.container);
		this.__removeGlobalResizeListeners();
		/**
		* It called after the window is closed
		*/
		e.fire(this, "afterClose");
		if (!this.isInDestruct) e.fire(this.ow, "joditCloseDialog");
		if (this.destroyAfterClose) this.destruct();
		return this;
	}
	constructor(options = {}) {
		super(options);
		this.destroyAfterClose = false;
		this.moved = false;
		this.resizable = false;
		this.draggable = false;
		this.startX = 0;
		this.startY = 0;
		this.startPoint = {
			x: 0,
			y: 0,
			w: 0,
			h: 0
		};
		this.lockSelect = () => {
			this.setMod("moved", true);
		};
		this.unlockSelect = () => {
			this.setMod("moved", false);
		};
		this.__onResize = () => {
			if (this.options && this.o.resizable && !this.moved && this.isOpened && !this.offsetX && !this.offsetY) this.setPosition();
		};
		/**
		* Minimal size the dialog can be resized to — the header and the footer
		* (with its buttons) must always stay inside the panel
		*/
		this.minSize = {
			w: 0,
			h: 0
		};
		this.isModal = false;
		/**
		* True, if dialog was opened
		*/
		this.isOpened = false;
		const self = this;
		self.options = ConfigProto(options, ConfigProto(Config.prototype.dialog, Dialog_1.defaultOptions));
		self.attachEvents(self.options);
		Dom.safeRemove(self.container);
		const n = this.getFullElName.bind(this);
		self.container = this.c.fromHTML(`<div class="jodit jodit-dialog ${this.componentName}">
				<div class="${n("overlay")}"></div>
				<div class="${this.getFullElName("panel")}">
					<div class="${n("header")}">
						<div class="${n("header-title")}"></div>
						<div class="${n("header-toolbar")}"></div>
					</div>
					<div class="${n("content")}"></div>
					<div class="${n("footer")}"></div>
					<div class="${n("resizer")}">${Icon.get("resize_handler")}</div>
				</div>
			</div>`);
		if (self.options.direction === "rtl") {
			css(self.container, "direction", "rtl");
			attr(self.container, "dir", "rtl");
		}
		if (this.o.zIndex) css(this.container, "zIndex", this.o.zIndex.toString());
		attr(self.container, "role", "dialog");
		Object.defineProperty(self.container, "component", { value: this });
		self.setMod("theme", self.o.theme || "default").setMod("resizable", Boolean(self.o.resizable));
		const dialog = self.getElm("panel");
		assert(dialog != null, "Panel element does not exist");
		const resizer = self.getElm("resizer");
		assert(resizer != null, "Resizer element does not exist");
		const dialogbox_header = self.getElm("header-title");
		assert(dialogbox_header != null, "header-title element does not exist");
		const dialogbox_content = self.getElm("content");
		assert(dialogbox_content != null, "Content element does not exist");
		const dialogbox_footer = self.getElm("footer");
		assert(dialogbox_footer != null, "Footer element does not exist");
		const dialogbox_toolbar = self.getElm("header-toolbar");
		assert(dialogbox_toolbar != null, "header-toolbar element does not exist");
		this.dialog = dialog;
		this.resizer = resizer;
		this.dialogbox_header = dialogbox_header;
		this.dialogbox_content = dialogbox_content;
		this.dialogbox_footer = dialogbox_footer;
		this.dialogbox_toolbar = dialogbox_toolbar;
		css(self.dialog, {
			maxWidth: self.options.maxWidth,
			minHeight: self.options.minHeight,
			minWidth: self.options.minWidth
		});
		const headerBox = self.getElm("header");
		headerBox && self.e.on(headerBox, "pointerdown touchstart", self.__onHeaderMouseDown);
		self.e.on(self.resizer, "mousedown touchstart", self.__onResizerMouseDown);
		const fullSize = pluginSystem.get("fullsize");
		isFunction(fullSize) && fullSize(self);
		this.e.on(self.container, "close_dialog", self.close).on(this.ow, "keydown", this.__onEsc).on(this.ow, "resize", this.__onResize);
		if (this.o.closeOnClickOverlay) {
			const overlay = self.getElm("overlay");
			assert(overlay != null, "Overlay element does not exist");
			this.e.on(overlay, "click", self.close);
		}
	}
	/**
	* Build toolbar after ready
	*/
	buildToolbar() {
		this.o.buttons && this.toolbar.build(splitArray(this.o.buttons)).setMod("mode", "header").appendTo(this.dialogbox_toolbar);
	}
	/**
	* It destroys all objects created for the windows and also includes all the handlers for the window object
	*/
	destruct() {
		if (this.isInDestruct) return;
		this.setStatus(STATUSES.beforeDestruct);
		if (this.isOpened) this.close();
		Dialog_1.__opened.delete(this);
		if (this.events) {
			this.__removeGlobalResizeListeners();
			this.events.off(this.container, "close_dialog", self.close).off(this.ow, "keydown", this.__onEsc).off(this.ow, "resize", this.__onResize);
		}
		super.destruct();
	}
};
/**
* Registry of the currently opened dialogs. Replaces a DOM scan of the
* whole `destination` (usually `document.body`) for every z-index update.
*/
Dialog.__opened = /* @__PURE__ */ new Set();
Dialog.defaultOptions = {
	...View.defaultOptions,
	closeOnClickOverlay: false,
	closeOnEsc: true
};
__decorate$23([autobind], Dialog.prototype, "__onMouseUp", null);
__decorate$23([autobind], Dialog.prototype, "__onHeaderMouseDown", null);
__decorate$23([autobind], Dialog.prototype, "__onMouseMove", null);
__decorate$23([autobind], Dialog.prototype, "__onEsc", null);
__decorate$23([autobind], Dialog.prototype, "__onResizerMouseDown", null);
__decorate$23([autobind], Dialog.prototype, "close", null);
__decorate$23([hook("ready")], Dialog.prototype, "buildToolbar", null);
Dialog = Dialog_1 = __decorate$23([component], Dialog);
//#endregion
//#region node_modules/jodit/esm/modules/dialog/alert.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Show `alert` dialog. Work without Jodit object
* @example
* ```javascript
* Jodit.Alert("File was uploaded");
* Jodit.Alert("File was uploaded", "Message");
* Jodit.Alert("File was uploaded", function() {
*    $('form').hide();
* });
* Jodit.Alert("File wasn't uploaded", "Error", function() {
*    $('form').hide();
* });
* ```
*/
function Alert(msg, title, callback, className = "jodit-dialog_alert") {
	if (isFunction(title)) {
		callback = title;
		title = void 0;
	}
	const dialog = this instanceof Dialog ? this : new Dialog({ closeOnClickOverlay: true }), container = dialog.c.div(className), okButton = Button(dialog, "ok", "Ok");
	asArray(msg).forEach((oneMessage) => {
		Dom.append(container, Dom.isNode(oneMessage) ? oneMessage : dialog.c.fromHTML(oneMessage));
	});
	okButton.onAction(() => {
		if (!callback || !isFunction(callback) || callback(dialog) !== false) dialog.close();
	});
	dialog.setFooter([okButton]);
	dialog.open(container, title || "&nbsp;", true, true);
	okButton.focus();
	return dialog;
}
//#endregion
//#region node_modules/jodit/esm/modules/dialog/confirm.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Show `confirm` dialog. Work without Jodit object
*
* @param title - Title or callback
* @param callback - callback. The first argument is the value entered
* @example
* ```javascript
* Jodit.Confirm("Are you sure?", "Confirm Dialog", function (yes) {
*     if (yes) {
*         // do something
*     }
* });
* ```
*/
function Confirm(msg, title, callback) {
	const dialog = this instanceof Dialog ? this : new Dialog({ closeOnClickOverlay: true }), $div = dialog.c.fromHTML("<form class=\"jodit-dialog_prompt\"></form>"), $label = dialog.c.element("label");
	if (isFunction(title)) {
		callback = title;
		title = void 0;
	}
	Dom.append($label, dialog.c.fromHTML(msg));
	Dom.append($div, $label);
	const action = (yes) => () => {
		if (!callback || callback(yes) !== false) dialog.close();
	};
	const $cancel = Button(dialog, "cancel", "Cancel");
	const $ok = Button(dialog, "ok", "Yes");
	$cancel.onAction(action(false));
	$ok.onAction(action(true));
	dialog.e.on($div, "submit", () => {
		action(true)();
		return false;
	});
	dialog.setFooter([$ok, $cancel]);
	dialog.open($div, title || "&nbsp;", true, true);
	$ok.focus();
	return dialog;
}
//#endregion
//#region node_modules/jodit/esm/modules/dialog/prompt.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Show `Prompt` dialog. Work without Jodit object
*
* @param msg - Dialog content
* @param title - Title or callback
* @param callback - callback. The first argument is the value entered
* @param placeholder - Placeholder for input
* @example
* ```javascript
* Jodit.Prompt("Enter your name", "Prompt Dialog", function (name) {
*     if (name.length < 3) {
*         Jodit.Alert("The name must be at least 3 letters");
*         return false;
*     }
*     // do something
* });
* ```
*/
function Prompt(msg, title, callback, placeholder, defaultValue) {
	const dialog = this instanceof Dialog ? this : new Dialog({ closeOnClickOverlay: true }), cancelButton = Button(dialog, "cancel", "Cancel"), okButton = Button(dialog, "ok", "Ok"), form = dialog.c.element("form", { class: "jodit-dialog_prompt" }), inputElement = dialog.c.element("input", {
		autofocus: true,
		class: "jodit-input"
	}), labelElement = dialog.c.element("label");
	if (isFunction(title)) {
		callback = title;
		title = void 0;
	}
	if (placeholder) attr(inputElement, "placeholder", placeholder);
	Dom.append(labelElement, dialog.c.text(msg));
	Dom.append(form, labelElement);
	Dom.append(form, inputElement);
	cancelButton.onAction(dialog.close);
	const onclick = () => {
		if (!callback || !isFunction(callback) || callback(inputElement.value) !== false) dialog.close();
	};
	okButton.onAction(onclick);
	dialog.e.on(form, "submit", () => {
		onclick();
		return false;
	});
	dialog.setFooter([okButton, cancelButton]);
	dialog.open(form, title || "&nbsp;", true, true);
	inputElement.focus();
	if (defaultValue !== void 0 && defaultValue.length) {
		inputElement.value = defaultValue;
		inputElement.select();
	}
	return dialog;
}
//#endregion
//#region node_modules/jodit/esm/modules/dialog/index.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
//#endregion
//#region node_modules/jodit/esm/core/traits/dlgs.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var Dlgs = class {
	dlg(options) {
		const popupRoot = getPopupViewRoot(this.o, this.container, this.od.body);
		const dialog = new Dialog({
			language: this.o.language,
			shadowRoot: this.o.shadowRoot,
			popupRoot,
			ownerWindow: this.o.ownerWindow,
			defaultTimeout: this.o.defaultTimeout,
			direction: this.o.direction,
			theme: this.o.theme,
			globalFullSize: this.o.globalFullSize,
			...options
		});
		markOwner(this, dialog.container);
		dialog.parent = this;
		return dialog.bindDestruct(this);
	}
	confirm(msg, title, callback) {
		msg = processTitle(msg, this);
		title = processTitle(title, this);
		return Confirm.call(this.dlg({ closeOnClickOverlay: true }), msg, title, callback);
	}
	prompt(msg, title, callback, placeholder, defaultValue) {
		msg = processTitle(msg, this);
		title = processTitle(title, this);
		placeholder = processTitle(placeholder, this);
		return Prompt.call(this.dlg({ closeOnClickOverlay: true }), msg, title, callback, placeholder, defaultValue);
	}
	alert(msg, title, callback, className) {
		msg = processTitle(msg, this);
		title = processTitle(title, this);
		return Alert.call(this.dlg({ closeOnClickOverlay: true }), msg, title, callback, className);
	}
};
function processTitle(title, self) {
	if (isString(title) && !isHTML(title)) title = self.i18n(title);
	return title;
}
//#endregion
//#region node_modules/jodit/esm/modules/context-menu/context-menu.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$22 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
/**
* Module to generate context menu
*/
var ContextMenu = class ContextMenu extends Popup {
	/** @override */
	className() {
		return "ContextMenu";
	}
	/**
	* Generate and show context menu
	*
	* @param x - Global coordinate by X
	* @param y - Global coordinate by Y
	* @param actions - Array with plain objects `{icon: 'bin', title: 'Delete', exec: function () {}}`
	* @example
	* ```javascript
	* parent.show(e.clientX, e.clientY, [{icon: 'bin', title: 'Delete', exec: function () { alert(1) }}]);
	* ```
	*/
	show(x, y, actions) {
		const self = this;
		self.clear();
		if (!isArray(actions)) return;
		actions.forEach((item) => {
			if (!item) return;
			const action = Button(this.jodit, item.icon || "empty", item.title);
			this.jodit && action.setParentView(this.jodit);
			action.setMod("context", "menu");
			action.onAction((e) => {
				var _a;
				(_a = item.exec) === null || _a === void 0 || _a.call(self, e);
				self.clear();
				self.close();
				return false;
			});
			this.append(action);
		});
		this.open(() => ({
			left: x,
			top: y,
			width: 0,
			height: 0
		}), true);
	}
};
ContextMenu = __decorate$22([component], ContextMenu);
//#endregion
//#region node_modules/jodit/esm/core/storage/index.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
//#endregion
//#region node_modules/jodit/esm/modules/file-browser/config.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
Config.prototype.filebrowser = {
	namespace: "",
	cache: true,
	extraButtons: [],
	filter(item, search) {
		search = search.toLowerCase();
		if (isString(item)) return item.toLowerCase().indexOf(search) !== -1;
		if (isString(item.name)) return item.name.toLowerCase().indexOf(search) !== -1;
		if (isString(item.file)) return item.file.toLowerCase().indexOf(search) !== -1;
		return true;
	},
	sortBy: "changed-desc",
	sort(a, b, sortBy) {
		const [sortAttr, arrow] = sortBy.toLowerCase().split("-"), asc = arrow === "asc";
		const compareStr = (f, s) => {
			if (f < s) return asc ? -1 : 1;
			if (f > s) return asc ? 1 : -1;
			return 0;
		};
		if (isString(a)) return compareStr(a.toLowerCase(), b.toLowerCase());
		if (a[sortAttr] === void 0 || sortAttr === "name") {
			if (isString(a.name)) return compareStr(a.name.toLowerCase(), b.name.toLowerCase());
			if (isString(a.file)) return compareStr(a.file.toLowerCase(), b.file.toLowerCase());
			return 0;
		}
		switch (sortAttr) {
			case "changed": {
				const f = new Date(a.changed).getTime(), s = new Date(b.changed).getTime();
				return asc ? f - s : s - f;
			}
			case "size": {
				const f = humanSizeToBytes(a.size), s = humanSizeToBytes(b.size);
				return asc ? f - s : s - f;
			}
		}
		return 0;
	},
	editImage: true,
	preview: true,
	showPreviewNavigation: true,
	showSelectButtonInPreview: true,
	contextMenu: true,
	howLongShowMsg: 3e3,
	createNewFolder: true,
	deleteFolder: true,
	renameFolder: true,
	moveFolder: true,
	moveFile: true,
	permissionsPresets: {
		allowFileDownload: void 0,
		allowFileMove: void 0,
		allowFileRemove: void 0,
		allowFileRename: void 0,
		allowFileUpload: void 0,
		allowFileUploadRemote: void 0,
		allowFiles: void 0,
		allowFolderCreate: void 0,
		allowFolderMove: void 0,
		allowFolderRemove: void 0,
		allowFolderRename: void 0,
		allowFolderTree: void 0,
		allowFolders: void 0,
		allowGeneratePdf: void 0,
		allowImageCrop: void 0,
		allowImageResize: void 0
	},
	showFoldersPanel: true,
	storeLastOpenedFolder: true,
	width: 859,
	height: 400,
	buttons: [
		"filebrowser.upload",
		"filebrowser.remove",
		"filebrowser.update",
		"filebrowser.select",
		"filebrowser.edit",
		"|",
		"filebrowser.tiles",
		"filebrowser.list",
		"|",
		"filebrowser.filter",
		"|",
		"filebrowser.sort"
	],
	removeButtons: [],
	fullsize: false,
	showTooltip: true,
	view: null,
	isSuccess(resp) {
		return resp.success;
	},
	getMessage(resp) {
		return resp.data.messages !== void 0 && isArray(resp.data.messages) ? resp.data.messages.join(" ") : "";
	},
	showFileName: true,
	showFileSize: true,
	showFileChangeTime: true,
	saveStateInStorage: {
		storeLastOpenedFolder: true,
		storeView: true,
		storeSortBy: true
	},
	pixelOffsetLoadNewChunk: 200,
	getThumbTemplate(item, source, source_name) {
		const opt = this.options, IC = this.files.getFullElName("item"), showName = opt.showFileName, showSize = opt.showFileSize && item.size, showTime = opt.showFileChangeTime && item.time;
		let name = "";
		if (item.file !== void 0) name = item.file;
		const info = `<div class="${IC}-info">${showName ? `<span class="${IC}-info-filename">${name}</span>` : ""}${showSize ? `<span class="${IC}-info-filesize">${item.size}</span>` : ""}${showTime ? `<span class="${IC}-info-filechanged">${showTime}</span>` : ""}</div>`;
		return `<a
			data-jodit-file-browser-item="true"
			data-is-file="${item.isImage ? 0 : 1}"
			draggable="true"
			class="${IC}"
			href="${item.fileURL}"
			data-source="${source_name}"
			data-path="${item.path}"
			data-name="${name}"
			title="${name}"
			data-url="${item.fileURL}">
				<img
					data-is-file="${item.isImage ? 0 : 1}"
					data-src="${item.fileURL}"
					src="${item.imageURL}"
					alt="${name}"
					loading="lazy"
				/>
				${showName || showSize || showTime ? info : ""}
			</a>`;
	},
	ajax: {
		...Config.prototype.defaultAjaxOptions,
		url: "",
		data: {},
		cache: true,
		contentType: "application/x-www-form-urlencoded; charset=UTF-8",
		method: "POST",
		processData: true,
		headers: {},
		prepareData(data) {
			return data;
		},
		process(resp) {
			return resp;
		}
	},
	create: { data: { action: "folderCreate" } },
	getLocalFileByUrl: { data: { action: "getLocalFileByUrl" } },
	resize: { data: { action: "imageResize" } },
	crop: { data: { action: "imageCrop" } },
	fileMove: { data: { action: "fileMove" } },
	folderMove: { data: { action: "folderMove" } },
	fileRename: { data: { action: "fileRename" } },
	folderRename: { data: { action: "folderRename" } },
	fileRemove: { data: { action: "fileRemove" } },
	folderRemove: { data: { action: "folderRemove" } },
	items: { data: { action: "files" } },
	folder: { data: { action: "folders" } },
	permissions: { data: { action: "permissions" } }
};
Config.prototype.controls.filebrowser = {
	upload: {
		icon: "plus",
		tooltip: "Upload file",
		isInput: true,
		isDisabled: (browser) => !browser.dataProvider.canI("FileUpload"),
		getContent: (filebrowser, btnInt) => {
			const btn = new UIFileInput(filebrowser, {
				tooltip: btnInt.control.tooltip,
				onlyImages: filebrowser.state.onlyImages
			});
			filebrowser.e.fire("bindUploader.filebrowser", btn.container);
			return btn.container;
		}
	},
	remove: {
		icon: "bin",
		tooltip: "Remove file",
		isDisabled: (browser) => {
			return !browser.state.activeElements.length || !browser.dataProvider.canI("FileRemove");
		},
		exec: (editor) => {
			editor.e.fire("fileRemove.filebrowser");
		}
	},
	update: {
		tooltip: "Update file list",
		exec: (editor) => {
			editor.e.fire("update.filebrowser");
		}
	},
	select: {
		tooltip: "Select file",
		icon: "check",
		isDisabled: (browser) => !browser.state.activeElements.length,
		exec: (editor) => {
			editor.e.fire("select.filebrowser");
		}
	},
	edit: {
		tooltip: "Edit image",
		icon: "pencil",
		isDisabled: (browser) => {
			const selected = browser.state.activeElements;
			return selected.length !== 1 || !selected[0].isImage || !(browser.dataProvider.canI("ImageCrop") || browser.dataProvider.canI("ImageResize"));
		},
		exec: (editor) => {
			editor.e.fire("edit.filebrowser");
		}
	},
	tiles: {
		tooltip: "Tiles view",
		icon: "th",
		isActive: (filebrowser) => filebrowser.state.view === "tiles",
		exec: (filebrowser) => {
			filebrowser.e.fire("view.filebrowser", "tiles");
		}
	},
	list: {
		tooltip: "List view",
		icon: "th-list",
		isActive: (filebrowser) => filebrowser.state.view === "list",
		exec: (filebrowser) => {
			filebrowser.e.fire("view.filebrowser", "list");
		}
	},
	filter: {
		isInput: true,
		getContent: (filebrowser, b) => {
			const oldInput = Dom.first(b.container, (node) => Dom.isHTMLElement(node) && node.classList.contains("jodit-input"));
			if (oldInput) return oldInput;
			const input = filebrowser.c.element("input", {
				class: "jodit-input",
				placeholder: filebrowser.i18n("Filter")
			});
			input.value = filebrowser.state.filterWord;
			filebrowser.e.on(input, "keydown mousedown", filebrowser.async.debounce(() => {
				filebrowser.e.fire("filter.filebrowser", input.value);
			}, filebrowser.defaultTimeout));
			return input;
		}
	},
	sort: {
		isInput: true,
		getContent: (fb) => {
			const select = fb.c.fromHTML(`<select class="jodit-input jodit-select"><option value="changed-asc">${fb.i18n("Sort by changed")} (⬆)</option><option value="changed-desc">${fb.i18n("Sort by changed")} (⬇)</option><option value="name-asc">${fb.i18n("Sort by name")} (⬆)</option><option value="name-desc">${fb.i18n("Sort by name")} (⬇)</option><option value="size-asc">${fb.i18n("Sort by size")} (⬆)</option><option value="size-desc">${fb.i18n("Sort by size")} (⬇)</option></select>`);
			select.value = fb.state.sortBy;
			fb.e.on("sort.filebrowser", (value) => {
				if (select.value !== value) select.value = value;
			}).on(select, "change", () => {
				fb.e.fire("sort.filebrowser", select.value);
			});
			return select;
		}
	}
};
//#endregion
//#region node_modules/jodit/esm/modules/file-browser/fetch/load-items.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Loads a list of files and adds them to the state
* @private
*/
function loadItems(fb) {
	fb.files.setMod("active", true);
	fb.files.setMod("loading", true);
	return fb.dataProvider.items(fb.state.currentPath, fb.state.currentSource, {
		sortBy: fb.state.sortBy,
		onlyImages: fb.state.onlyImages,
		filterWord: fb.state.filterWord
	}).then((resp) => {
		if (resp) {
			fb.state.elements = resp;
			fb.state.activeElements = [];
		}
	}).catch(fb.status).finally(() => fb.files.setMod("loading", false));
}
//#endregion
//#region node_modules/jodit/esm/modules/file-browser/fetch/load-tree.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Loads a list of directories
* @private
*/
async function loadTree(fb) {
	fb.tree.setMod("active", true);
	Dom.detach(fb.tree.container);
	const items = loadItems(fb);
	if (fb.o.showFoldersPanel) {
		fb.tree.setMod("loading", true);
		const tree = fb.dataProvider.tree(fb.state.currentPath, fb.state.currentSource).then((resp) => {
			fb.state.sources = resp;
		}).catch(fb.status).finally(() => fb.tree.setMod("loading", false));
		return Promise.all([tree, items]);
	}
	fb.tree.setMod("active", false);
	return items;
}
//#endregion
//#region node_modules/jodit/esm/modules/file-browser/builders/elements-map.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var map = /* @__PURE__ */ new WeakMap();
/**
* Returns a map of the file's key correspondence to its DOM element in the file browser
* @private
*/
var elementsMap = (view) => {
	let result = map.get(view);
	if (!result) {
		result = {};
		map.set(view, result);
	}
	return result;
};
//#endregion
//#region node_modules/jodit/esm/modules/file-browser/builders/utils.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @private
*/
var getItem = (node, root, tag = "a") => Dom.closest(node, (elm) => Dom.isTag(elm, tag), root);
/**
* @private
*/
var elementToItem = (elm, elementsMap) => {
	const { key } = elm.dataset, { item } = elementsMap[key || ""];
	return item;
};
//#endregion
//#region node_modules/jodit/esm/modules/file-browser/builders/item.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var FileBrowserItem = class FileBrowserItem {
	constructor(data) {
		this.data = data;
		Object.keys(data).forEach((key) => {
			this[key] = data[key];
		});
	}
	static create(data) {
		if (data instanceof FileBrowserItem) return data;
		return new FileBrowserItem(data);
	}
	get path() {
		return normalizePath(this.data.source.path ? this.data.source.path + "/" : "/");
	}
	get imageURL() {
		const timestamp = this.time || (/* @__PURE__ */ new Date()).getTime().toString(), { thumbIsAbsolute, source, thumb, file } = this.data, path = thumb || file;
		return thumbIsAbsolute && path ? path : normalizeUrl(source.baseurl, source.path, path || "") + "?_tmst=" + encodeURIComponent(timestamp);
	}
	get fileURL() {
		let { name } = this.data;
		const { file, fileIsAbsolute, source } = this.data;
		if (file !== void 0) name = file;
		return fileIsAbsolute && name ? name : normalizeUrl(source.baseurl, source.path, name || "");
	}
	get time() {
		const { changed } = this.data;
		return changed && (typeof changed === "number" ? new Date(changed).toLocaleString() : changed) || "";
	}
	get uniqueHashKey() {
		const data = this.data;
		let key = [
			data.sourceName,
			data.name,
			data.file,
			this.time,
			data.thumb
		].join("_");
		key = key.toLowerCase().replace(/[^0-9a-z\-.]/g, "-");
		return key;
	}
	toJSON() {
		return this.data;
	}
};
//#endregion
//#region node_modules/jodit/esm/modules/file-browser/data-provider.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$21 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var DEFAULT_SOURCE_NAME$1 = "default";
var DataProvider = class {
	constructor(parent, options) {
		this.parent = parent;
		this.options = options;
		this.__currentPermissions = null;
		this.__ajaxInstances = /* @__PURE__ */ new Map();
		this.progressHandler = (ignore) => {};
	}
	/**
	* Alias for options
	*/
	get o() {
		return this.options;
	}
	get(name) {
		const instances = this.__ajaxInstances;
		if (instances.has(name)) {
			const ajax = instances.get(name);
			ajax === null || ajax === void 0 || ajax.abort();
			instances.delete(name);
		}
		const opts = ConfigProto(this.options[name] !== void 0 ? this.options[name] : {}, ConfigProto({ onProgress: this.progressHandler }, this.o.ajax));
		if (opts.prepareData) opts.data = opts.prepareData.call(this, opts.data);
		const ajax = new Ajax(opts);
		instances.set(name, ajax);
		const promise = ajax.send();
		promise.finally(() => {
			ajax.destruct();
			instances.delete(name);
			this.progressHandler(100);
		}).catch(() => null);
		return promise.then((resp) => resp.json()).then((resp) => {
			if (resp && !this.isSuccess(resp)) throw new Error(this.getMessage(resp));
			return resp;
		});
	}
	onProgress(callback) {
		this.progressHandler = callback;
	}
	/**
	* Load permissions for path and source
	*/
	async permissions(path, source) {
		if (!this.o.permissions) return null;
		this.o.permissions.data.path = path;
		this.o.permissions.data.source = source;
		if (this.o.permissions.url) return this.get("permissions").then((resp) => {
			if (this.parent.isInDestruct) throw abort();
			let process = this.o.permissions.process;
			if (!process) process = this.o.ajax.process;
			if (process) {
				const respData = process.call(self, resp);
				if (respData.data.permissions) {
					this.parent.events.fire(this, "changePermissions", this.__currentPermissions, respData.data.permissions);
					this.__currentPermissions = respData.data.permissions;
				}
			}
			return this.__currentPermissions;
		});
		return null;
	}
	canI(action) {
		const rule = "allow" + action;
		const presetValue = this.o.permissionsPresets[rule];
		if (presetValue !== void 0) return presetValue;
		return this.__currentPermissions == null || this.__currentPermissions[rule] === void 0 || this.__currentPermissions[rule];
	}
	__items(path, source, mods, onResult) {
		const opt = this.options;
		if (!opt.items) return Promise.reject(Error("Set Items api options"));
		opt.items.data.path = path;
		opt.items.data.source = source;
		opt.items.data.mods = mods;
		return this.get("items").then((resp) => {
			let process = this.o.items.process;
			if (!process) process = this.o.ajax.process;
			if (process) resp = process.call(self, resp);
			return onResult(resp);
		});
	}
	/**
	* Load items list by path and source
	*/
	items(path, source, mods = {}) {
		return this.__items(path, source, mods, (resp) => this.__generateItemsList(resp.data.sources, mods));
	}
	/**
	* Load items list by path and source
	*/
	itemsEx(path, source, mods = {}) {
		const calcTotal = (sources) => sources.reduce((acc, source) => acc + source.files.length, 0);
		return this.__items(path, source, mods, (resp) => ({
			items: this.__generateItemsList(resp.data.sources, mods),
			loadedTotal: calcTotal(resp.data.sources)
		}));
	}
	__generateItemsList(sources, mods = {}) {
		const elements = [];
		const canBeFile = (item) => item.type === "folder" || !mods.onlyImages || item.isImage === void 0 || item.isImage;
		const inFilter = (item) => {
			var _a;
			return !((_a = mods.filterWord) === null || _a === void 0 ? void 0 : _a.length) || this.o.filter === void 0 || this.o.filter(item, mods.filterWord);
		};
		sources.forEach((source) => {
			if (source.files && source.files.length) {
				const { sort } = this.o;
				if (isFunction(sort) && mods.sortBy) source.files.sort((a, b) => sort(a, b, mods.sortBy));
				source.files.forEach((item) => {
					if (inFilter(item) && canBeFile(item)) elements.push(FileBrowserItem.create({
						...item,
						sourceName: source.name,
						source
					}));
				});
			}
		});
		return elements;
	}
	async tree(path, source) {
		path = normalizeRelativePath(path);
		if (!this.o.folder) return Promise.reject(Error("Set Folder Api options"));
		await this.permissions(path, source);
		this.o.folder.data.path = path;
		this.o.folder.data.source = source;
		return this.get("folder").then((resp) => {
			let process = this.o.folder.process;
			if (!process) process = this.o.ajax.process;
			if (process) resp = process.call(self, resp);
			return resp.data.sources;
		});
	}
	/**
	* Get path by url. You can use this method in another modules
	*/
	getPathByUrl(url) {
		set("options.getLocalFileByUrl.data.url", url, this);
		return this.get("getLocalFileByUrl").then((resp) => {
			if (this.isSuccess(resp)) return resp.data;
			throw error(this.getMessage(resp));
		});
	}
	/**
	* Create a directory on the server
	*
	* @param name - Name the new folder
	* @param path - Relative directory in which you want create a folder
	* @param source - Server source key
	*/
	createFolder(name, path, source) {
		const { create } = this.o;
		if (!create) throw error("Set Create api options");
		create.data.source = source;
		create.data.path = path;
		create.data.name = name;
		return this.get("create").then((resp) => {
			if (this.isSuccess(resp)) return true;
			throw error(this.getMessage(resp));
		});
	}
	/**
	* Move a file / directory on the server
	*
	* @param filepath - The relative path to the file / folder source
	* @param path - Relative to the directory where you want to move the file / folder
	*/
	move(filepath, path, source, isFile) {
		const mode = isFile ? "fileMove" : "folderMove";
		const option = this.options[mode];
		if (!option) throw error("Set Move api options");
		option.data.from = filepath;
		option.data.path = path;
		option.data.source = source;
		return this.get(mode).then((resp) => {
			if (this.isSuccess(resp)) return true;
			throw error(this.getMessage(resp));
		});
	}
	/**
	* Deleting item
	*
	* @param path - Relative path
	* @param file - The filename
	* @param source - Source
	*/
	__remove(action, path, file, source) {
		const fr = this.o[action];
		if (!fr) throw error(`Set "${action}" api options`);
		fr.data.path = path;
		fr.data.name = file;
		fr.data.source = source;
		return this.get(action).then((resp) => {
			if (fr.process) resp = fr.process.call(this, resp);
			return this.getMessage(resp);
		});
	}
	/**
	* Deleting a file
	*
	* @param path - Relative path
	* @param file - The filename
	* @param source - Source
	*/
	fileRemove(path, file, source) {
		return this.__remove("fileRemove", path, file, source);
	}
	/**
	* Deleting a folder
	*
	* @param path - Relative path
	* @param file - The filename
	* @param source - Source
	*/
	folderRemove(path, file, source) {
		return this.__remove("folderRemove", path, file, source);
	}
	/**
	* Rename action
	*
	* @param path - Relative path
	* @param name - Old name
	* @param newname - New name
	* @param source - Source
	*/
	__rename(action, path, name, newname, source) {
		const fr = this.o[action];
		if (!fr) throw error(`Set "${action}" api options`);
		fr.data.path = path;
		fr.data.name = name;
		fr.data.newname = newname;
		fr.data.source = source;
		return this.get(action).then((resp) => {
			if (fr.process) resp = fr.process.call(self, resp);
			return this.getMessage(resp);
		});
	}
	/**
	* Rename folder
	*/
	folderRename(path, name, newname, source) {
		return this.__rename("folderRename", path, name, newname, source);
	}
	/**
	* Rename file
	*/
	fileRename(path, name, newname, source) {
		return this.__rename("fileRename", path, name, newname, source);
	}
	__changeImage(type, path, source, name, newname, box) {
		if (!this.o[type]) this.o[type] = { data: {} };
		const query = this.o[type];
		if (query.data === void 0) query.data = { action: type };
		query.data.newname = newname || name;
		if (box) query.data.box = box;
		query.data.path = path;
		query.data.name = name;
		query.data.source = source;
		return this.get(type).then((resp) => {
			return resp.data.newPath || true;
		});
	}
	/**
	* Send command to server to crop image
	*/
	crop(path, source, name, newname, box) {
		return this.__changeImage("crop", path, source, name, newname, box);
	}
	/**
	* Send command to server to resize image
	*/
	resize(path, source, name, newname, box) {
		return this.__changeImage("resize", path, source, name, newname, box);
	}
	getMessage(resp) {
		return this.options.getMessage(resp);
	}
	isSuccess(resp) {
		return this.options.isSuccess(resp);
	}
	destruct() {
		this.__ajaxInstances.forEach((a) => a.destruct());
		this.__ajaxInstances.clear();
	}
};
__decorate$21([autobind], DataProvider.prototype, "onProgress", null);
__decorate$21([autobind], DataProvider.prototype, "permissions", null);
__decorate$21([autobind], DataProvider.prototype, "canI", null);
__decorate$21([autobind], DataProvider.prototype, "items", null);
__decorate$21([autobind], DataProvider.prototype, "itemsEx", null);
__decorate$21([autobind], DataProvider.prototype, "tree", null);
__decorate$21([autobind], DataProvider.prototype, "getPathByUrl", null);
__decorate$21([autobind], DataProvider.prototype, "createFolder", null);
__decorate$21([autobind], DataProvider.prototype, "move", null);
__decorate$21([autobind], DataProvider.prototype, "fileRemove", null);
__decorate$21([autobind], DataProvider.prototype, "folderRemove", null);
__decorate$21([autobind], DataProvider.prototype, "folderRename", null);
__decorate$21([autobind], DataProvider.prototype, "fileRename", null);
__decorate$21([autobind], DataProvider.prototype, "crop", null);
__decorate$21([autobind], DataProvider.prototype, "resize", null);
__decorate$21([autobind], DataProvider.prototype, "getMessage", null);
__decorate$21([autobind], DataProvider.prototype, "isSuccess", null);
__decorate$21([autobind], DataProvider.prototype, "destruct", null);
//#endregion
//#region node_modules/jodit/esm/modules/file-browser/factories.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
function makeDataProvider(parent, options) {
	return new DataProvider(parent, options);
}
function makeContextMenu(parent) {
	return new ContextMenu(parent);
}
//#endregion
//#region node_modules/jodit/esm/modules/file-browser/fetch/delete-file.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Removes a file from the server
* @private
*/
function deleteFile(fb, name, source) {
	return fb.dataProvider.fileRemove(fb.state.currentPath, name, source).then((message) => {
		fb.status(message || fb.i18n("File \"%s\" was deleted", name), true);
	}).catch(fb.status);
}
//#endregion
//#region node_modules/jodit/esm/modules/image-editor/icons/crop.svg.js
var crop_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"> <path d=\"M621 1280h595v-595zm-45-45l595-595h-595v595zm1152 77v192q0 14-9 23t-23 9h-224v224q0 14-9 23t-23 9h-192q-14 0-23-9t-9-23v-224h-864q-14 0-23-9t-9-23v-864h-224q-14 0-23-9t-9-23v-192q0-14 9-23t23-9h224v-224q0-14 9-23t23-9h192q14 0 23 9t9 23v224h851l246-247q10-9 23-9t23 9q9 10 9 23t-9 23l-247 246v851h224q14 0 23 9t9 23z\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/modules/image-editor/icons/resize.svg.js
var resize_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 24 24\"> <g transform=\"translate(-251.000000, -443.000000)\"> <g transform=\"translate(215.000000, 119.000000)\"/> <path d=\"M252,448 L256,448 L256,444 L252,444 L252,448 Z M257,448 L269,448 L269,446 L257,446 L257,448 Z M257,464 L269,464 L269,462 L257,462 L257,464 Z M270,444 L270,448 L274,448 L274,444 L270,444 Z M252,462 L252,466 L256,466 L256,462 L252,462 Z M270,462 L270,466 L274,466 L274,462 L270,462 Z M254,461 L256,461 L256,449 L254,449 L254,461 Z M270,461 L272,461 L272,449 L270,449 L270,461 Z\"/> </g> </svg> ";
//#endregion
//#region node_modules/jodit/esm/modules/image-editor/config.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
Config.prototype.imageeditor = {
	min_width: 20,
	min_height: 20,
	closeAfterSave: false,
	width: "85%",
	height: "85%",
	crop: true,
	resize: true,
	resizeUseRatio: true,
	resizeMinWidth: 20,
	resizeMinHeight: 20,
	cropUseRatio: true,
	cropDefaultWidth: "70%",
	cropDefaultHeight: "70%"
};
Icon.set("crop", crop_svg_default).set("resize", resize_svg_default);
//#endregion
//#region node_modules/jodit/esm/modules/image-editor/templates/form.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var jie$1 = "jodit-image-editor";
var gi = Icon.get.bind(Icon);
var act = (el, className = "jodti-image-editor_active") => el ? className : "";
var form = (editor, o) => {
	const i = editor.i18n.bind(editor);
	const switcher = (label, ref, active = true) => `<div class="jodit-form__group">
			<label class="jodit-switcher-wrapper">
				<span class='jodit-switcher'>
					<input ${act(active, "checked")} data-ref="${ref}" type="checkbox"/>
					<span class="jodit-switcher__slider"></span>
				</span>
				<span>${i(label)}</span>
			</label>
	</div>`;
	return editor.create.fromHTML(`<form class="${jie$1} jodit-properties">
		<div class="jodit-grid jodit-grid_xs-column">
			<div class="jodit_col-lg-3-4 jodit_col-sm-5-5">
			${o.resize ? `<div class="${jie$1}__area ${jie$1}__area_resize ${jie$1}_active">
							<div data-ref="resizeBox" class="${jie$1}__box"></div>
							<div class="${jie$1}__resizer">
								<i class="jodit_bottomright"></i>
							</div>
						</div>` : ""}
			${o.crop ? `<div class="${jie$1}__area ${jie$1}__area_crop ${act(!o.resize)}">
							<div data-ref="cropBox" class="${jie$1}__box">
								<div class="${jie$1}__croper">
									<i class="jodit_bottomright"></i>
									<i class="${jie$1}__sizes"></i>
								</div>
							</div>
						</div>` : ""}
			</div>
			<div class="jodit_col-lg-1-4 jodit_col-sm-5-5">
			${o.resize ? `<div data-area="resize" class="${jie$1}__slider ${jie$1}_active">
							<div class="${jie$1}__slider-title">
								${gi("resize")}
								${i("Resize")}
							</div>
							<div class="${jie$1}__slider-content">
								<div class="jodit-form__group">
									<label>
										${i("Width")}
									</label>
									<input type="number" data-ref="widthInput" class="jodit-input"/>
								</div>
								<div class="jodit-form__group">
									<label>
										${i("Height")}
									</label>
									<input type="number" data-ref="heightInput" class="jodit-input"/>
								</div>
								${switcher("Keep Aspect Ratio", "keepAspectRatioResize")}
							</div>
						</div>` : ""}
			${o.crop ? `<div data-area="crop" class="${jie$1}__slider ${act(!o.resize)}'">
							<div class="${jie$1}__slider-title">
								${gi("crop")}
								${i("Crop")}
							</div>
							<div class="${jie$1}__slider-content">
								${switcher("Keep Aspect Ratio", "keepAspectRatioCrop")}
							</div>
						</div>` : ""}
			</div>
		</div>
	</form>`);
};
//#endregion
//#region node_modules/jodit/esm/modules/image-editor/image-editor.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$20 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var ImageEditor_1;
var jie = "jodit-image-editor";
var hasClass = (className) => (elm) => elm.classList.contains(className);
function findFirst(root, cond) {
	return Dom.first(root, (n) => Dom.isHTMLElement(n) && cond(n));
}
function findAll(root, cond) {
	const result = [];
	Dom.each(root, (n) => {
		Dom.isHTMLElement(n) && cond(n) && result.push(n);
	});
	return result;
}
var TABS = {
	resize: "resize",
	crop: "crop"
};
/**
* The module allows you to edit the image: resize or cut any part of it
*
*/
var ImageEditor = ImageEditor_1 = class ImageEditor extends ViewComponent {
	/** @override */
	className() {
		return "ImageEditor";
	}
	get o() {
		return this.options;
	}
	/**
	* Hide image editor
	*/
	hide() {
		this._dialog.close();
	}
	/**
	* Open image editor
	* @example
	* ```javascript
	* const jodit = Jodit.make('.editor', {
	*		 imageeditor: {
	*				 crop: false,
	*				 closeAfterSave: true,
	*				 width: 500
	*		 }
	* });
	* jodit.imageeditor.open('https://xdsoft.net/jodit/images/test.png', function (name, data, success, failed) {
	*		 var img = jodit.node.c('img');
	*		 img.setAttribute('src', 'https://xdsoft.net/jodit/images/test.png');
	*		 if (box.action !== 'resize') {
	*					return failed('Sorry it is work only in resize mode. For croping use FileBrowser');
	*		 }
	*		 img.style.width = data.w;
	*		 img.style.height = data.h;
	*		 jodit.s.insertNode(img);
	*		 success();
	* });
	* ```
	*/
	open(url, save) {
		return this.j.async.promise((resolve) => {
			const timestamp = (/* @__PURE__ */ new Date()).getTime();
			this.image = this.j.c.element("img");
			const isTrash = (elm) => Dom.isTag(elm, "img") || hasClass("jodit-icon_loader")(elm);
			findAll(this.resize_box, isTrash).forEach(Dom.safeRemove);
			findAll(this.crop_box, isTrash).forEach(Dom.safeRemove);
			css(this.cropHandler, "background", "transparent");
			this.onSave = save;
			Dom.append(this.resize_box, this.j.c.element("i", { class: "jodit-icon_loader" }));
			Dom.append(this.crop_box, this.j.c.element("i", { class: "jodit-icon_loader" }));
			if (/\?/.test(url)) url += "&_tst=" + timestamp;
			else url += "?_tst=" + timestamp;
			attr(this.image, "src", url);
			this._dialog.open();
			const { widthInput, heightInput } = refs(this.editor);
			const onload = () => {
				if (this.isDestructed) return;
				this.image.removeEventListener("load", onload);
				this.naturalWidth = this.image.naturalWidth;
				this.naturalHeight = this.image.naturalHeight;
				widthInput.value = this.naturalWidth.toString();
				heightInput.value = this.naturalHeight.toString();
				this.ratio = this.naturalWidth / this.naturalHeight;
				Dom.append(this.resize_box, this.image);
				this.cropImage = this.image.cloneNode(true);
				Dom.append(this.crop_box, this.cropImage);
				Dom.safeRemove.apply(null, findAll(this.editor, hasClass("jodit-icon_loader")));
				if (this.activeTab === TABS.crop) this.showCrop();
				this.j.e.fire(this.resizeHandler, "updatesize");
				this.j.e.fire(this.cropHandler, "updatesize");
				this._dialog.setPosition();
				this.j.e.fire("afterImageEditor");
				resolve(this._dialog);
			};
			this.image.addEventListener("load", onload);
			if (this.image.complete) onload();
		});
	}
	onTitleModeClick(e) {
		const self = this, title = e.target;
		const slide = title === null || title === void 0 ? void 0 : title.parentElement;
		if (!slide) return;
		findAll(self.editor, (elm) => hasClass(`${jie}__slider`)(elm) || hasClass(`${jie}__area`)(elm)).forEach((elm) => elm.classList.remove(`${jie}_active`));
		slide.classList.add(`${jie}_active`);
		this.activeTab = attr(slide, "-area") || TABS.resize;
		const tab = findFirst(self.editor, (elm) => hasClass(`${jie}__area`)(elm) && hasClass(`${jie}__area_` + self.activeTab)(elm));
		if (tab) tab.classList.add(`${jie}_active`);
		if (self.activeTab === TABS.crop) self.showCrop();
	}
	onChangeSizeInput(e) {
		const self = this, input = e.target, { widthInput, heightInput } = refs(this.editor), isWidth = attr(input, "data-ref") === "widthInput", x = parseInt(input.value, 10), minX = isWidth ? self.o.min_width : self.o.min_height, minY = !isWidth ? self.o.min_width : self.o.min_height;
		let y;
		if (x > minX) {
			css(self.image, isWidth ? "width" : "height", x);
			if (self.resizeUseRatio) {
				y = isWidth ? Math.round(x / self.ratio) : Math.round(x * self.ratio);
				if (y > minY) {
					css(self.image, !isWidth ? "width" : "height", y);
					if (isWidth) heightInput.value = y.toString();
					else widthInput.value = y.toString();
				}
			}
		}
		this.j.e.fire(self.resizeHandler, "updatesize");
	}
	onResizeHandleMouseDown(e) {
		const self = this;
		self.target = e.target;
		e.preventDefault();
		e.stopImmediatePropagation();
		self.clicked = true;
		self.start_x = e.clientX;
		self.start_y = e.clientY;
		if (self.activeTab === TABS.crop) {
			self.top_x = css(self.cropHandler, "left");
			self.top_y = css(self.cropHandler, "top");
			self.width = self.cropHandler.offsetWidth;
			self.height = self.cropHandler.offsetHeight;
		} else {
			self.width = self.image.offsetWidth;
			self.height = self.image.offsetHeight;
		}
		self.j.e.on(this.j.ow, "mousemove", this.onGlobalMouseMove).one(this.j.ow, "mouseup", this.onGlobalMouseUp);
	}
	onGlobalMouseUp(e) {
		if (this.clicked) {
			this.clicked = false;
			e.stopImmediatePropagation();
			this.j.e.off(this.j.ow, "mousemove", this.onGlobalMouseMove);
		}
	}
	onGlobalMouseMove(e) {
		const self = this;
		if (!self.clicked) return;
		const { widthInput, heightInput } = refs(this.editor);
		self.diff_x = e.clientX - self.start_x;
		self.diff_y = e.clientY - self.start_y;
		if (self.activeTab === TABS.resize && self.resizeUseRatio || self.activeTab === TABS.crop && self.cropUseRatio) if (self.diff_x) {
			self.new_w = self.width + self.diff_x;
			self.new_h = Math.round(self.new_w / self.ratio);
		} else {
			self.new_h = self.height + self.diff_y;
			self.new_w = Math.round(self.new_h * self.ratio);
		}
		else {
			self.new_w = self.width + self.diff_x;
			self.new_h = self.height + self.diff_y;
		}
		if (self.activeTab === TABS.resize) {
			if (self.new_w > self.o.resizeMinWidth) {
				css(self.image, "width", self.new_w + "px");
				widthInput.value = self.new_w.toString();
			}
			if (self.new_h > self.o.resizeMinHeight) {
				css(self.image, "height", self.new_h + "px");
				heightInput.value = self.new_h.toString();
			}
			this.j.e.fire(self.resizeHandler, "updatesize");
		} else {
			if (self.target !== self.cropHandler) {
				if (self.top_x + self.new_w > self.cropImage.offsetWidth) self.new_w = self.cropImage.offsetWidth - self.top_x;
				if (self.top_y + self.new_h > self.cropImage.offsetHeight) self.new_h = self.cropImage.offsetHeight - self.top_y;
				css(self.cropHandler, {
					width: self.new_w,
					height: self.new_h
				});
			} else {
				if (self.top_x + self.diff_x + self.cropHandler.offsetWidth > self.cropImage.offsetWidth) self.diff_x = self.cropImage.offsetWidth - self.top_x - self.cropHandler.offsetWidth;
				css(self.cropHandler, "left", self.top_x + self.diff_x);
				if (self.top_y + self.diff_y + self.cropHandler.offsetHeight > self.cropImage.offsetHeight) self.diff_y = self.cropImage.offsetHeight - self.top_y - self.cropHandler.offsetHeight;
				css(self.cropHandler, "top", self.top_y + self.diff_y);
			}
			this.j.e.fire(self.cropHandler, "updatesize");
		}
	}
	constructor(editor) {
		super(editor);
		this.resizeUseRatio = true;
		this.cropUseRatio = true;
		this.clicked = false;
		this.start_x = 0;
		this.start_y = 0;
		this.top_x = 0;
		this.top_y = 0;
		this.width = 0;
		this.height = 0;
		this.activeTab = TABS.resize;
		this.naturalWidth = 0;
		this.naturalHeight = 0;
		this.ratio = 0;
		this.new_h = 0;
		this.new_w = 0;
		this.diff_x = 0;
		this.diff_y = 0;
		this.cropBox = {
			x: 0,
			y: 0,
			w: 0,
			h: 0
		};
		this.resizeBox = {
			w: 0,
			h: 0
		};
		this.calcCropBox = () => {
			const node = this.crop_box.parentNode, w = node.offsetWidth * .8, h = node.offsetHeight * .8;
			let wn = w, hn = h;
			const { naturalWidth: nw, naturalHeight: nh } = this;
			if (w > nw && h > nh) {
				wn = nw;
				hn = nh;
			} else if (this.ratio > w / h) {
				wn = w;
				hn = nh * (w / nw);
			} else {
				wn = nw * (h / nh);
				hn = h;
			}
			css(this.crop_box, {
				width: wn,
				height: hn
			});
		};
		this.showCrop = () => {
			if (!this.cropImage) return;
			this.calcCropBox();
			const w = this.cropImage.offsetWidth || this.image.offsetWidth || this.image.naturalWidth;
			this.new_w = ImageEditor_1.calcValueByPercent(w, this.o.cropDefaultWidth);
			const h = this.cropImage.offsetHeight || this.image.offsetHeight || this.image.naturalHeight;
			if (this.cropUseRatio) this.new_h = this.new_w / this.ratio;
			else this.new_h = ImageEditor_1.calcValueByPercent(h, this.o.cropDefaultHeight);
			css(this.cropHandler, {
				backgroundImage: "url(" + attr(this.cropImage, "src") + ")",
				width: this.new_w,
				height: this.new_h,
				left: w / 2 - this.new_w / 2,
				top: h / 2 - this.new_h / 2
			});
			this.j.e.fire(this.cropHandler, "updatesize");
		};
		this.updateCropBox = () => {
			if (!this.cropImage) return;
			const ratioX = this.cropImage.offsetWidth / this.naturalWidth, ratioY = this.cropImage.offsetHeight / this.naturalHeight;
			this.cropBox.x = css(this.cropHandler, "left") / ratioX;
			this.cropBox.y = css(this.cropHandler, "top") / ratioY;
			this.cropBox.w = this.cropHandler.offsetWidth / ratioX;
			this.cropBox.h = this.cropHandler.offsetHeight / ratioY;
			this.sizes.textContent = this.cropBox.w.toFixed(0) + "x" + this.cropBox.h.toFixed(0);
		};
		this.updateResizeBox = () => {
			this.resizeBox.w = this.image.offsetWidth || this.naturalWidth;
			this.resizeBox.h = this.image.offsetHeight || this.naturalHeight;
		};
		this.setHandlers = () => {
			const self = this;
			const { widthInput, heightInput } = refs(this.editor);
			self.j.e.on([findFirst(self.editor, hasClass("jodit_bottomright")), self.cropHandler], `mousedown.${jie}`, this.onResizeHandleMouseDown).on(this.j.ow, `resize.${jie}`, () => {
				this.j.e.fire(self.resizeHandler, "updatesize");
				self.showCrop();
				this.j.e.fire(self.cropHandler, "updatesize");
			});
			self.j.e.on(findAll(this.editor, hasClass(`${jie}__slider-title`)), "click", this.onTitleModeClick).on([widthInput, heightInput], "input", this.onChangeSizeInput);
			const { keepAspectRatioResize, keepAspectRatioCrop } = refs(this.editor);
			if (keepAspectRatioResize) keepAspectRatioResize.addEventListener("change", () => {
				this.resizeUseRatio = keepAspectRatioResize.checked;
			});
			if (keepAspectRatioCrop) keepAspectRatioCrop.addEventListener("change", () => {
				this.cropUseRatio = keepAspectRatioCrop.checked;
			});
			self.j.e.on(self.resizeHandler, "updatesize", () => {
				css(self.resizeHandler, {
					top: 0,
					left: 0,
					width: self.image.offsetWidth || self.naturalWidth,
					height: self.image.offsetHeight || self.naturalHeight
				});
				this.updateResizeBox();
			}).on(self.cropHandler, "updatesize", () => {
				if (!self.cropImage) return;
				let new_x = css(self.cropHandler, "left"), new_y = css(self.cropHandler, "top"), new_width = self.cropHandler.offsetWidth, new_height = self.cropHandler.offsetHeight;
				if (new_x < 0) new_x = 0;
				if (new_y < 0) new_y = 0;
				if (new_x + new_width > self.cropImage.offsetWidth) {
					new_width = self.cropImage.offsetWidth - new_x;
					if (self.cropUseRatio) new_height = new_width / self.ratio;
				}
				if (new_y + new_height > self.cropImage.offsetHeight) {
					new_height = self.cropImage.offsetHeight - new_y;
					if (self.cropUseRatio) new_width = new_height * self.ratio;
				}
				css(self.cropHandler, {
					width: new_width,
					height: new_height,
					left: new_x,
					top: new_y,
					backgroundPosition: -new_x - 1 + "px " + (-new_y - 1) + "px",
					backgroundSize: self.cropImage.offsetWidth + "px " + self.cropImage.offsetHeight + "px"
				});
				self.updateCropBox();
			});
			Object.values(self.buttons).forEach((button) => {
				button.onAction(() => {
					const data = {
						action: self.activeTab,
						box: self.activeTab === TABS.resize ? self.resizeBox : self.cropBox
					};
					switch (button) {
						case self.buttons.saveas:
							self.j.prompt("Enter new name", "Save in new file", (name) => {
								if (!trim(name)) {
									self.j.alert("The name should not be empty");
									return false;
								}
								self.j.e.fire("afterImageEditorSave", data, name);
								self.onSave(name, data, self.hide, (e) => {
									self.j.alert(e.message);
								});
							});
							break;
						case self.buttons.save:
							self.j.e.fire("afterImageEditorSave", data);
							self.onSave(void 0, data, self.hide, (e) => {
								self.j.alert(e.message);
							});
							break;
						case self.buttons.reset:
							if (self.activeTab === TABS.resize) {
								css(self.image, {
									width: null,
									height: null
								});
								widthInput.value = self.naturalWidth.toString();
								heightInput.value = self.naturalHeight.toString();
								self.j.e.fire(self.resizeHandler, "updatesize");
							} else self.showCrop();
							break;
					}
				});
			});
		};
		this.options = editor && editor.o && editor.o.imageeditor ? editor.o.imageeditor : Config.defaultOptions.imageeditor;
		const o = this.options;
		this.resizeUseRatio = o.resizeUseRatio;
		this.cropUseRatio = o.cropUseRatio;
		this.buttons = {
			reset: Button(this.j, "update", "Reset"),
			save: Button(this.j, "save", "Save"),
			saveas: Button(this.j, {
				icon: { name: "save" },
				name: "save-as",
				text: "Save as ..."
			})
		};
		this.activeTab = o.resize ? TABS.resize : TABS.crop;
		this.editor = form(this.j, this.options);
		const { resizeBox, cropBox } = refs(this.editor);
		this.resize_box = resizeBox;
		this.crop_box = cropBox;
		this.sizes = findFirst(this.editor, (elm) => hasClass(`${jie}__sizes`)(elm) && Boolean(Dom.closest(elm, (p) => Dom.isHTMLElement(p) && hasClass(`${jie}__area_crop`)(p), this.editor)));
		this.resizeHandler = findFirst(this.editor, hasClass(`${jie}__resizer`));
		this.cropHandler = findFirst(this.editor, hasClass(`${jie}__croper`));
		this._dialog = this.j.dlg({ buttons: ["fullsize", "dialog.close"] });
		this._dialog.setContent(this.editor);
		this._dialog.setSize(this.o.width, this.o.height);
		this._dialog.setHeader([
			this.buttons.reset,
			this.buttons.save,
			this.buttons.saveas
		]);
		this.setHandlers();
	}
	/** @override */
	destruct() {
		if (this.isDestructed) return;
		if (this._dialog && !this._dialog.isInDestruct) this._dialog.destruct();
		Dom.safeRemove(this.editor);
		if (this.j.e) this.j.e.off(this.j.ow, "mousemove", this.onGlobalMouseMove).off(this.j.ow, "mouseup", this.onGlobalMouseUp).off(this.ow, `.${jie}`).off(`.${jie}`);
		super.destruct();
	}
};
ImageEditor.calcValueByPercent = (value, percent) => {
	const percentStr = percent.toString();
	const valueNbr = parseFloat(value.toString());
	let match;
	match = /^[-+]?[0-9]+(px)?$/.exec(percentStr);
	if (match) return parseInt(percentStr, 10);
	match = /^([-+]?[0-9.]+)%$/.exec(percentStr);
	if (match) return Math.round(valueNbr * (parseFloat(match[1]) / 100));
	return valueNbr || 0;
};
__decorate$20([autobind], ImageEditor.prototype, "hide", null);
__decorate$20([autobind], ImageEditor.prototype, "open", null);
__decorate$20([autobind], ImageEditor.prototype, "onTitleModeClick", null);
__decorate$20([debounce(), autobind], ImageEditor.prototype, "onChangeSizeInput", null);
__decorate$20([autobind], ImageEditor.prototype, "onResizeHandleMouseDown", null);
__decorate$20([autobind], ImageEditor.prototype, "onGlobalMouseUp", null);
__decorate$20([throttle(10)], ImageEditor.prototype, "onGlobalMouseMove", null);
ImageEditor = ImageEditor_1 = __decorate$20([component], ImageEditor);
/**
* Open Image Editor
*/
function openImageEditor(href, name, path, source, onSuccess, onFailed) {
	return this.getInstance("ImageEditor", this.o).open(href, (newname, box, success, failed) => {
		if (box.action === "saved") {
			success();
			if (onSuccess) onSuccess(box.newPath);
			return;
		}
		call(box.action === "resize" ? this.dataProvider.resize : this.dataProvider.crop, path, source, name, newname, box.box).then((result) => {
			if (result) {
				success();
				if (onSuccess) onSuccess(typeof result === "string" ? result : void 0);
			}
		}).catch((error) => {
			failed(error);
			if (onFailed) onFailed(error);
		});
	});
}
//#endregion
//#region node_modules/jodit/esm/modules/file-browser/builders/context-menu.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var CLASS_PREVIEW = "jodit-file-browser-preview";
var preview_tpl_next = (next = "next", right = "right") => `<div class="${CLASS_PREVIEW}__navigation ${CLASS_PREVIEW}__navigation_arrow_${next}">` + Icon.get("angle-" + right) + "</a>";
var context_menu_default = (self) => {
	if (!self.o.contextMenu) return () => {};
	const contextmenu = makeContextMenu(self);
	return (e) => {
		const a = getItem(e.target, self.container);
		if (!a) return;
		let item = a;
		const opt = self.options, ga = (key) => attr(item, key) || "";
		self.async.setTimeout(() => {
			const selectedItem = elementToItem(a, elementsMap(self));
			if (!selectedItem) return;
			self.state.activeElements = [selectedItem];
			contextmenu.show(e.clientX, e.clientY, [
				ga("data-is-file") !== "1" && opt.editImage && (self.dataProvider.canI("ImageResize") || self.dataProvider.canI("ImageCrop")) ? {
					icon: "pencil",
					title: "Edit",
					exec: () => openImageEditor.call(self, ga("href"), ga("data-name"), ga("data-path"), ga("data-source"))
				} : false,
				self.dataProvider.canI("FileRename") ? {
					icon: "italic",
					title: "Rename",
					exec: () => {
						self.e.fire("fileRename.filebrowser", ga("data-name"), ga("data-path"), ga("data-source"));
					}
				} : false,
				self.dataProvider.canI("FileRemove") ? {
					icon: "bin",
					title: "Delete",
					exec: async () => {
						try {
							await deleteFile(self, ga("data-name"), ga("data-source"));
						} catch (e) {
							return self.status(e);
						}
						self.state.activeElements = [];
						return loadTree(self).catch(self.status);
					}
				} : false,
				opt.preview ? {
					icon: "eye",
					title: "Preview",
					exec: () => {
						const preview = self.dlg({ buttons: ["fullsize", "dialog.close"] }), temp_content = self.c.div(CLASS_PREVIEW, "<div class=\"jodit-icon_loader\"></div>"), preview_box = self.c.div("jodit-file-browser-preview__box"), next = self.c.fromHTML(preview_tpl_next()), prev = self.c.fromHTML(preview_tpl_next("prev", "left")), addLoadHandler = (src) => {
							const image = self.c.element("img");
							attr(image, "src", src);
							const onload = () => {
								var _a;
								if (self.isInDestruct) return;
								self.e.off(image, "load");
								Dom.detach(temp_content);
								if (opt.showPreviewNavigation) {
									if (Dom.prevWithClass(item, self.files.getFullElName("item"))) Dom.append(temp_content, prev);
									if (Dom.nextWithClass(item, self.files.getFullElName("item"))) Dom.append(temp_content, next);
								}
								Dom.append(temp_content, preview_box);
								Dom.append(preview_box, image);
								preview.setPosition();
								(_a = self === null || self === void 0 ? void 0 : self.events) === null || _a === void 0 || _a.fire("previewOpenedAndLoaded");
							};
							self.e.on(image, "load", onload);
							if (image.complete) onload();
						};
						self.e.on([next, prev], "click", function() {
							if (this === next) item = Dom.nextWithClass(item, self.files.getFullElName("item"));
							else item = Dom.prevWithClass(item, self.files.getFullElName("item"));
							if (!item) throw error("Need element");
							Dom.detach(temp_content);
							Dom.detach(preview_box);
							temp_content.innerHTML = "<div class=\"jodit-icon_loader\"></div>";
							addLoadHandler(ga("href"));
						});
						self.e.on("beforeDestruct", () => {
							preview.destruct();
						});
						preview.container.classList.add("jodit-file-browser-preview__dialog");
						preview.setContent(temp_content);
						preview.setPosition();
						preview.open();
						addLoadHandler(ga("href"));
						self.events.on("beforeDestruct", () => {
							preview.destruct();
						}).fire("previewOpened");
					}
				} : false,
				{
					icon: "upload",
					title: "Download",
					exec: () => {
						const url = ga("href");
						if (url) self.ow.open(url);
					}
				}
			]);
		}, self.defaultTimeout);
		self.e.on("beforeClose", () => {
			contextmenu.close();
		}).on("beforeDestruct", () => contextmenu.destruct());
		e.stopPropagation();
		e.preventDefault();
		return false;
	};
};
//#endregion
//#region node_modules/jodit/esm/modules/file-browser/listeners/native-listeners.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @private
*/
function nativeListeners() {
	let dragElement = false;
	const elmMap = elementsMap(this);
	const self = this;
	self.e.on(self.tree.container, "dragstart", (e) => {
		const a = getItem(e.target, self.container);
		if (!a) return;
		if (self.o.moveFolder) dragElement = a;
	}).on(self.tree.container, "drop", (e) => {
		if ((self.o.moveFile || self.o.moveFolder) && dragElement) {
			let path = attr(dragElement, "-path") || "";
			if (!self.o.moveFolder && dragElement.classList.contains(this.tree.getFullElName("item"))) return false;
			if (dragElement.classList.contains(this.files.getFullElName("item"))) {
				path += attr(dragElement, "-name");
				if (!self.o.moveFile) return false;
			}
			const a = getItem(e.target, self.container);
			if (!a) return;
			self.dataProvider.move(path, attr(a, "-path") || "", attr(a, "-source") || "", dragElement.classList.contains(this.files.getFullElName("item"))).then(() => loadTree(this)).catch(self.status);
			dragElement = false;
		}
	}).on(self.files.container, "contextmenu", context_menu_default(self)).on(self.files.container, "click", (e) => {
		if (!ctrlKey(e)) this.state.activeElements = [];
	}).on(self.files.container, "click", (e) => {
		const a = getItem(e.target, self.container);
		if (!a) return;
		const item = elementToItem(a, elmMap);
		if (!item) return;
		if (!ctrlKey(e)) self.state.activeElements = [item];
		else self.state.activeElements = [...self.state.activeElements, item];
		e.stopPropagation();
		return false;
	}).on(self.files.container, "dragstart", (e) => {
		if (self.o.moveFile) {
			const a = getItem(e.target, self.container);
			if (!a) return;
			dragElement = a;
		}
	}).on(self.container, "drop", (e) => e.preventDefault());
}
//#endregion
//#region node_modules/jodit/esm/modules/file-browser/listeners/self-listeners.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @private
*/
function selfListeners() {
	const state = this.state, dp = this.dataProvider, self = this;
	self.e.on("view.filebrowser", (view) => {
		if (view !== state.view) state.view = view;
	}).on("sort.filebrowser", (value) => {
		if (value !== state.sortBy) {
			state.sortBy = value;
			loadItems(self);
		}
	}).on("filter.filebrowser", (value) => {
		if (value !== state.filterWord) {
			state.filterWord = value;
			loadItems(self);
		}
	}).on("openFolder.filebrowser", (data) => {
		let path;
		if (data.name === "..") path = data.path.split("/").filter((p) => p.length).slice(0, -1).join("/");
		else path = normalizePath(data.path, data.name);
		self.state.currentPath = path;
		self.state.currentSource = data.name === "." ? DEFAULT_SOURCE_NAME$1 : data.source;
	}).on("removeFolder.filebrowser", (data) => {
		self.confirm("Are you sure?", "Delete", (yes) => {
			if (yes) dp.folderRemove(data.path, data.name, data.source).then((message) => {
				self.status(message, true);
				return loadTree(self);
			}).catch(self.status);
		});
	}).on("renameFolder.filebrowser", (data) => {
		self.prompt("Enter new name", "Rename", (newName) => {
			if (!isValidName(newName)) {
				self.status(self.i18n("Enter new name"));
				return false;
			}
			dp.folderRename(data.path, data.name, newName, data.source).then((message) => {
				self.state.activeElements = [];
				self.status(message, true);
				return loadTree(self);
			}).catch(self.status);
		}, "type name", data.name);
	}).on("addFolder.filebrowser", (data) => {
		self.prompt("Enter Directory name", "Create directory", (name) => {
			dp.createFolder(name, data.path, data.source).then(() => loadTree(self)).catch(self.status);
		}, "type dir name");
	}).on("fileRemove.filebrowser", () => {
		if (self.state.activeElements.length) self.confirm("Are you sure?", "", (yes) => {
			if (yes) {
				const promises = [];
				self.state.activeElements.forEach((item) => {
					promises.push(deleteFile(self, item.file || item.name || "", item.sourceName));
				});
				self.state.activeElements = [];
				Promise.all(promises).then(() => loadTree(self).catch(self.status), self.status);
			}
		});
	}).on("edit.filebrowser", () => {
		if (self.state.activeElements.length === 1) {
			const [file] = this.state.activeElements;
			openImageEditor.call(self, file.fileURL, file.file || "", file.path, file.sourceName);
		}
	}).on("fileRename.filebrowser", (name, path, source) => {
		if (self.state.activeElements.length === 1) self.prompt("Enter new name", "Rename", (newName) => {
			if (!isValidName(newName)) {
				self.status(self.i18n("Enter new name"));
				return false;
			}
			dp.fileRename(path, name, newName, source).then((message) => {
				self.state.activeElements = [];
				self.status(message, true);
				loadItems(self);
			}).catch(self.status);
		}, "type name", name);
	}).on("update.filebrowser", () => {
		loadTree(this).then(this.status, this.status);
	});
}
//#endregion
//#region node_modules/jodit/esm/modules/file-browser/listeners/state-listeners.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var DEFAULT_SOURCE_NAME = "default";
/**
* Convert state to view
* @private
*/
function stateListeners() {
	const elmMap = elementsMap(this);
	const { state, files, create, options } = this, getDomElement = (item) => {
		const key = item.uniqueHashKey;
		if (elmMap[key]) return elmMap[key].elm;
		const elm = create.fromHTML(options.getThumbTemplate.call(this, item, item.source, item.sourceName.toString()));
		elm.dataset.key = key;
		elmMap[key] = {
			item,
			elm
		};
		return elmMap[key].elm;
	};
	state.on(["change.currentPath", "change.currentSource"], this.async.debounce(() => {
		if (this.o.saveStateInStorage && this.o.saveStateInStorage.storeLastOpenedFolder) this.storage.set("currentPath", this.state.currentPath).set("currentSource", this.state.currentSource);
		loadTree(this).catch(this.status);
	}, this.defaultTimeout)).on("beforeChange.activeElements", () => {
		state.activeElements.forEach((item) => {
			const key = item.uniqueHashKey, { elm } = elmMap[key];
			elm && elm.classList.remove(files.getFullElName("item", "active", true));
		});
	}).on("change.activeElements", () => {
		this.e.fire("changeSelection");
		state.activeElements.forEach((item) => {
			const key = item.uniqueHashKey, { elm } = elmMap[key];
			elm && elm.classList.add(files.getFullElName("item", "active", true));
		});
	}).on("change.view", () => {
		files.setMod("view", state.view);
		if (this.o.saveStateInStorage && this.o.saveStateInStorage.storeView) this.storage.set("view", state.view);
	}).on("change.sortBy", () => {
		if (this.o.saveStateInStorage && this.o.saveStateInStorage.storeSortBy) this.storage.set("sortBy", state.sortBy);
	}).on("change.elements", this.async.debounce(() => {
		Dom.detach(files.container);
		if (state.elements.length) state.elements.forEach((item) => {
			Dom.append(this.files.container, getDomElement(item));
		});
		else Dom.append(files.container, create.div(this.componentName + "_no-files_true", this.i18n("There are no files")));
	}, this.defaultTimeout)).on("change.sources", this.async.debounce(() => {
		Dom.detach(this.tree.container);
		state.sources.forEach((source) => {
			const sourceName = source.name;
			if (sourceName && sourceName !== DEFAULT_SOURCE_NAME) Dom.append(this.tree.container, create.div(this.tree.getFullElName("source-title"), sourceName));
			source.folders.forEach((name) => {
				const folderElm = create.a(this.tree.getFullElName("item"), {
					draggable: "draggable",
					href: "#",
					"data-path": normalizePath(source.path, name + "/"),
					"data-name": name,
					"data-source": sourceName,
					"data-source-path": source.path
				}, create.span(this.tree.getFullElName("item-title"), name));
				const action = (actionName) => (e) => {
					this.e.fire(`${actionName}.filebrowser`, {
						name,
						path: normalizePath(source.path + "/"),
						source: sourceName
					});
					e.stopPropagation();
					e.preventDefault();
				};
				this.e.on(folderElm, "click", action("openFolder"));
				Dom.append(this.tree.container, folderElm);
				if (name === ".." || name === ".") return;
				if (options.renameFolder && this.dataProvider.canI("FolderRename")) {
					const btn = Button(this, {
						icon: { name: "pencil" },
						name: "rename",
						tooltip: "Rename",
						size: "tiny"
					});
					btn.onAction(action("renameFolder"));
					Dom.append(folderElm, btn.container);
				}
				if (options.deleteFolder && this.dataProvider.canI("FolderRemove")) {
					const btn = Button(this, {
						icon: { name: "cancel" },
						name: "remove",
						tooltip: "Delete",
						size: "tiny"
					});
					btn.onAction(action("removeFolder"));
					Dom.append(folderElm, btn.container);
				}
			});
			if (options.createNewFolder && this.dataProvider.canI("FolderCreate")) {
				const button = Button(this, "plus", "Add folder", "secondary");
				button.onAction(() => {
					this.e.fire("addFolder", {
						path: normalizePath(source.path + "/"),
						source: sourceName
					});
				});
				this.tree.append(button);
			}
		});
	}, this.defaultTimeout));
}
//#endregion
//#region node_modules/jodit/esm/modules/file-browser/ui/files/files.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var FileBrowserFiles = class extends UIGroup {
	className() {
		return "FileBrowserFiles";
	}
};
//#endregion
//#region node_modules/jodit/esm/modules/file-browser/ui/tree/tree.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var FileBrowserTree = class extends UIGroup {
	className() {
		return "FileBrowserTree";
	}
};
//#endregion
//#region node_modules/jodit/esm/modules/file-browser/ui/index.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
//#endregion
//#region node_modules/jodit/esm/modules/file-browser/file-browser.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$19 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var FileBrowser = class FileBrowser extends ViewWithToolbar {
	/** @override */
	className() {
		return "FileBrowser";
	}
	get dataProvider() {
		return makeDataProvider(this, this.options);
	}
	onSelect(callback) {
		return () => {
			if (this.state.activeElements.length) {
				const files = [];
				const isImages = [];
				this.state.activeElements.forEach((elm) => {
					const url = elm.fileURL;
					if (url) {
						files.push(url);
						isImages.push(elm.isImage || false);
					}
				});
				this.close();
				const data = {
					baseurl: "",
					files,
					isImages
				};
				if (isFunction(callback)) callback(data);
				this.close();
			}
			return false;
		};
	}
	get _dialog() {
		var _a;
		const dialog = this.dlg({
			minWidth: Math.min(700, screen.width),
			minHeight: 300,
			buttons: (_a = this.o.headerButtons) !== null && _a !== void 0 ? _a : ["fullsize", "dialog.close"]
		});
		[
			"beforeClose",
			"afterClose",
			"beforeOpen"
		].forEach((proxyEvent) => dialog.events.on(dialog, proxyEvent, () => this.e.fire(proxyEvent)));
		dialog.setSize(this.o.width, this.o.height);
		return dialog;
	}
	/**
	* Container for set/get value
	*/
	get storage() {
		return Storage.makeStorage(Boolean(this.o.saveStateInStorage), this.componentName);
	}
	get isOpened() {
		return this._dialog.isOpened && cssInline(this.browser, "display") !== "none";
	}
	/**
	* It displays a message in the status bar of filebrowser
	*
	* @param message - The message that will be displayed
	* @param success - true It will be shown a message light . If no option is specified ,
	* ßan error will be shown the red
	* @example
	* ```javascript
	* parent.filebrowser.status('There was an error uploading file', false);
	* ```
	*/
	status(message, success) {
		if (!message || isAbortError(message)) return;
		if (!isString(message)) message = message.message;
		if (!isString(message) || !trim(message).length) return;
		this.message.message(message, success ? "success" : "error", this.o.howLongShowMsg);
	}
	/**
	* It opens a web browser window
	*
	* @param callback - The function that will be called after the file selection in the browser
	* @param onlyImages - Show only images
	* @example
	* ```javascript
	* var fb = new Jodit.modules.FileBrowser(parent);
	* fb.open(function (data) {
	*     var i;
	*     for (i = 0;i < data.files.length; i += 1) {
	*         parent.s.insertImage(data.baseurl + data.files[i]);
	*     }
	* });
	* ```
	*/
	open(callback = this.o.defaultCallback, onlyImages = false) {
		this.state.onlyImages = onlyImages;
		return this.async.promise((resolve, reject) => {
			var _a;
			if (!this.o.items || !this.o.items.url) throw error("Need set options.filebrowser.ajax.url");
			let localTimeout = 0;
			this.e.off(this.files.container, "dblclick").on(this.files.container, "dblclick", this.onSelect(callback)).on(this.files.container, "touchstart", () => {
				const now = (/* @__PURE__ */ new Date()).getTime();
				if (now - localTimeout < 300) this.onSelect(callback)();
				localTimeout = now;
			}).off("select.filebrowser").on("select.filebrowser", this.onSelect(callback));
			const header = this.c.div();
			(_a = this.toolbar) === null || _a === void 0 || _a.appendTo(header);
			this.__updateToolbarButtons();
			this._dialog.open(this.browser, header);
			this.e.fire("sort.filebrowser", this.state.sortBy);
			loadTree(this).then(resolve, reject).finally(() => {
				var _a;
				if (this.isInDestruct) return;
				(_a = this === null || this === void 0 ? void 0 : this.e) === null || _a === void 0 || _a.fire("fileBrowserReady.filebrowser");
			});
		}).catch((e) => {
			if (!isAbortError(e) && false);
		});
	}
	__getButtons() {
		var _a;
		return ((_a = this.o.buttons) !== null && _a !== void 0 ? _a : []).filter((btn) => {
			if (!isString(btn)) return true;
			switch (btn) {
				case "filebrowser.upload": return this.dataProvider.canI("FileUpload");
				case "filebrowser.edit": return this.dataProvider.canI("ImageResize") || this.dataProvider.canI("ImageCrop");
				case "filebrowser.remove": return this.dataProvider.canI("FileRemove");
			}
			return true;
		});
	}
	initUploader(editor) {
		var _a;
		const self = this, uploaderOptions = ConfigProto(((_a = editor === null || editor === void 0 ? void 0 : editor.options) === null || _a === void 0 ? void 0 : _a.uploader) || {}, Config.defaultOptions.uploader);
		const uploadHandler = () => loadItems(this);
		self.uploader = self.getInstance("Uploader", uploaderOptions);
		self.uploader.setPath(self.state.currentPath).setSource(self.state.currentSource).bind(self.browser, uploadHandler, self.errorHandler);
		this.state.on(["change.currentPath", "change.currentSource"], () => {
			this.uploader.setPath(this.state.currentPath).setSource(this.state.currentSource);
		});
		self.e.on("bindUploader.filebrowser", (button) => {
			self.uploader.bind(button, uploadHandler, self.errorHandler);
		});
	}
	constructor(options) {
		super(options);
		this.browser = this.c.div(this.componentName);
		this.status_line = this.c.div(this.getFullElName("status"));
		this.tree = new FileBrowserTree(this);
		this.files = new FileBrowserFiles(this);
		this.state = observable({
			currentPath: "",
			currentSource: DEFAULT_SOURCE_NAME$1,
			currentBaseUrl: "",
			activeElements: [],
			elements: [],
			sources: [],
			view: "tiles",
			sortBy: "changed-desc",
			filterWord: "",
			onlyImages: false
		});
		this.errorHandler = (resp) => {
			if (isAbortError(resp)) return;
			if (resp instanceof Error) this.status(this.i18n(resp.message));
			else this.status(this.dataProvider.getMessage(resp));
		};
		/**
		* Close dialog
		*/
		this.close = () => {
			this._dialog.close();
		};
		this.__prevButtons = [];
		this.attachEvents(options);
		const self = this;
		self.options = ConfigProto(options || {}, Config.defaultOptions.filebrowser);
		self.browser.component = this;
		self.container = self.browser;
		if (self.o.showFoldersPanel) Dom.append(self.browser, self.tree.container);
		Dom.append(self.browser, self.files.container);
		Dom.append(self.browser, self.status_line);
		selfListeners.call(self);
		nativeListeners.call(self);
		stateListeners.call(self);
		[
			"getLocalFileByUrl",
			"crop",
			"resize",
			"create",
			"fileMove",
			"folderMove",
			"fileRename",
			"folderRename",
			"fileRemove",
			"folderRemove",
			"folder",
			"items",
			"permissions"
		].forEach((key) => {
			if (this.options[key] != null) this.options[key] = ConfigProto(this.options[key], this.o.ajax);
		});
		const { storeView, storeSortBy, storeLastOpenedFolder } = this.o.saveStateInStorage || {
			storeLastOpenedFolder: false,
			storeView: false,
			storeSortBy: false
		};
		const view = storeView && this.storage.get("view");
		if (view && this.o.view == null) self.state.view = view === "list" ? "list" : "tiles";
		else self.state.view = self.o.view === "list" ? "list" : "tiles";
		self.files.setMod("view", self.state.view);
		const sortBy = storeSortBy && self.storage.get("sortBy");
		if (sortBy) {
			const parts = sortBy.split("-");
			self.state.sortBy = [
				"changed",
				"name",
				"size"
			].includes(parts[0]) ? sortBy : "changed-desc";
		} else self.state.sortBy = self.o.sortBy || "changed-desc";
		if (storeLastOpenedFolder) {
			const currentPath = self.storage.get("currentPath"), currentSource = self.storage.get("currentSource");
			self.state.currentPath = currentPath !== null && currentPath !== void 0 ? currentPath : "";
			self.state.currentSource = currentSource !== null && currentSource !== void 0 ? currentSource : "";
		}
		self.initUploader(self);
		self.setStatus(STATUSES.ready);
	}
	destruct() {
		var _a;
		if (this.isInDestruct) return;
		(_a = cached(this, "_dialog")) === null || _a === void 0 || _a.destruct();
		super.destruct();
		this.events && this.e.off(".filebrowser");
		this.uploader && this.uploader.destruct();
	}
	__updateToolbarButtons() {
		var _a;
		const buttons = this.__getButtons();
		if (isEqualButtonList(this.__prevButtons, buttons)) return;
		this.__prevButtons = buttons;
		(_a = this.toolbar) === null || _a === void 0 || _a.build(buttons);
	}
};
__decorate$19([cache], FileBrowser.prototype, "dataProvider", null);
__decorate$19([cache], FileBrowser.prototype, "_dialog", null);
__decorate$19([cache], FileBrowser.prototype, "storage", null);
__decorate$19([autobind], FileBrowser.prototype, "status", null);
__decorate$19([autobind], FileBrowser.prototype, "open", null);
__decorate$19([watch("dataProvider:changePermissions")], FileBrowser.prototype, "__updateToolbarButtons", null);
FileBrowser = __decorate$19([derive(Dlgs)], FileBrowser);
function isEqualButtonList(prevButtons, buttons) {
	if (prevButtons.length !== buttons.length) return false;
	for (let i = 0; i < prevButtons.length; i++) if (prevButtons[i] !== buttons[i]) return false;
	return true;
}
//#endregion
//#region node_modules/jodit/esm/modules/file-browser/index.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
//#endregion
//#region node_modules/jodit/esm/core/create/index.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
//#endregion
//#region node_modules/jodit/esm/core/plugin/plugin.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$18 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var Plugin = class extends ViewComponent {
	/** @override */
	className() {
		return "Plugin";
	}
	constructor(jodit) {
		super(jodit);
		/** @override */
		this.buttons = [];
		/**
		* Plugin have CSS style and it should be loaded
		*/
		this.hasStyle = false;
		this.__inited = false;
		jodit.e.on("afterPluginSystemInit", this.__afterPluginSystemInit).on("afterInit", this.__afterInit).on("beforeDestruct", this.__beforeDestruct);
	}
	__afterPluginSystemInit() {
		const { j, buttons } = this;
		if (buttons && isJoditObject(j)) buttons.forEach((btn) => {
			j.registerButton(btn);
		});
	}
	__afterInit() {
		this.__inited = true;
		this.setStatus(STATUSES.ready);
		this.afterInit(this.jodit);
	}
	init(jodit) {
		if (this.jodit.isReady) {
			this.afterInit(this.jodit);
			this.__afterPluginSystemInit();
			this.jodit.e.fire("rebuildToolbar");
		}
	}
	__beforeDestruct() {
		var _a;
		if (this.isInDestruct) return;
		const { j } = this;
		j.e.off("afterPluginSystemInit", this.__afterPluginSystemInit).off("afterInit", this.__afterInit).off("beforeDestruct", this.destruct);
		this.setStatus(STATUSES.beforeDestruct);
		if (!this.__inited) return super.destruct();
		if (isJoditObject(j)) (_a = this.buttons) === null || _a === void 0 || _a.forEach((btn) => {
			j === null || j === void 0 || j.unregisterButton(btn);
		});
		this.beforeDestruct(this.j);
		super.destruct();
	}
};
Plugin.requires = [];
__decorate$18([autobind], Plugin.prototype, "__afterPluginSystemInit", null);
__decorate$18([autobind], Plugin.prototype, "__afterInit", null);
__decorate$18([autobind], Plugin.prototype, "__beforeDestruct", null);
//#endregion
//#region node_modules/jodit/esm/core/plugin/index.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
//#endregion
//#region node_modules/jodit/esm/modules/history/command.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var Command = class {
	undo() {
		this.history.snapshot.restore(this.oldValue);
	}
	redo() {
		this.history.snapshot.restore(this.newValue);
	}
	constructor(oldValue, newValue, history, tick) {
		this.oldValue = oldValue;
		this.newValue = newValue;
		this.history = history;
		this.tick = tick;
	}
};
//#endregion
//#region node_modules/jodit/esm/modules/history/snapshot.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Module for creating snapshot of editor which includes html content and the current selection
*/
var Snapshot = class Snapshot extends ViewComponent {
	constructor() {
		super(...arguments);
		this.__isBlocked = false;
		this.__levelOfTransaction = 0;
	}
	/** @override */
	className() {
		return "Snapshot";
	}
	/**
	* Compare two snapshotes, if and htmls and selections match, then return true
	*
	* @param first - the first snapshote
	* @param second - second shot
	*/
	static equal(first, second) {
		return first.html === second.html && JSON.stringify(first.range) === JSON.stringify(second.range);
	}
	/**
	* Calc count element before some node in parentNode. All text nodes are joined
	*/
	static countNodesBeforeInParent(elm) {
		if (!elm.parentNode) return 0;
		const elms = elm.parentNode.childNodes;
		let count = 0, previous = null;
		for (let j = 0; j < elms.length; j += 1) {
			if (previous && !this.isIgnoredNode(elms[j]) && !(Dom.isText(previous) && Dom.isText(elms[j]))) count += 1;
			if (elms[j] === elm) return count;
			previous = elms[j];
		}
		return 0;
	}
	/**
	* Calc normal offset in joined text nodes
	*/
	static strokeOffset(elm, offset) {
		while (Dom.isText(elm)) {
			elm = elm.previousSibling;
			if (Dom.isText(elm) && elm.nodeValue) offset += elm.nodeValue.length;
		}
		return offset;
	}
	/**
	* Calc whole hierarchy path before some element in editor's tree
	*/
	calcHierarchyLadder(elm) {
		const counts = [];
		if (!elm || !elm.parentNode || !Dom.isOrContains(this.j.editor, elm)) return [];
		while (elm && elm !== this.j.editor) {
			if (elm && !Snapshot.isIgnoredNode(elm)) counts.push(Snapshot.countNodesBeforeInParent(elm));
			elm = elm.parentNode;
		}
		return counts.reverse();
	}
	getElementByLadder(ladder) {
		let n = this.j.editor, i;
		for (i = 0; n && i < ladder.length; i += 1) n = n.childNodes[ladder[i]];
		return n;
	}
	get isBlocked() {
		return this.__isBlocked;
	}
	__block(enable) {
		this.__isBlocked = enable;
	}
	transaction(changes) {
		this.__block(true);
		this.__levelOfTransaction += 1;
		try {
			changes();
		} catch (e) {} finally {
			this.__levelOfTransaction -= 1;
			if (this.__levelOfTransaction === 0) this.__block(false);
		}
	}
	/**
	* Creates object a snapshot of editor: html and the current selection. Current selection calculate by
	* offset by start document
	* \{html: string, range: \{startContainer: int, startOffset: int, endContainer: int, endOffset: int\}\} or
	* \{html: string\} without selection
	*/
	make() {
		const snapshot = {
			html: "",
			range: {
				startContainer: [],
				startOffset: 0,
				endContainer: [],
				endOffset: 0
			}
		};
		snapshot.html = this.__getCleanedEditorValue(this.j.editor);
		const sel = this.j.s.sel;
		if (sel && sel.rangeCount) {
			const range = sel.getRangeAt(0);
			const startContainer = this.calcHierarchyLadder(range.startContainer);
			const endContainer = this.calcHierarchyLadder(range.endContainer);
			let startOffset = Snapshot.strokeOffset(range.startContainer, range.startOffset), endOffset = Snapshot.strokeOffset(range.endContainer, range.endOffset);
			if (!startContainer.length && range.startContainer !== this.j.editor) startOffset = 0;
			if (!endContainer.length && range.endContainer !== this.j.editor) endOffset = 0;
			snapshot.range = {
				startContainer,
				startOffset,
				endContainer,
				endOffset
			};
		}
		return snapshot;
	}
	/**
	* Restores the state of the editor of the snapshot. Rebounding is not only html but selected text
	*
	* @param snapshot - snapshot of editor resulting from the `[[Snapshot.make]]` method
	* @see make
	*/
	restore(snapshot) {
		this.transaction(() => {
			const scroll = this.storeScrollState();
			if (this.__getCleanedEditorValue(this.j.editor) !== snapshot.html) this.j.value = snapshot.html;
			this.restoreOnlySelection(snapshot);
			this.restoreScrollState(scroll);
		});
	}
	storeScrollState() {
		return [this.j.ow.scrollY, this.j.editor.scrollTop];
	}
	restoreScrollState(scrolls) {
		const { j } = this, { ow } = j;
		ow.scrollTo(ow.scrollX, scrolls[0]);
		j.editor.scrollTop = scrolls[1];
	}
	/**
	* Restore selection from snapshot
	*
	* @param snapshot - snapshot of editor resulting from the [[Snapshot.make]] method
	* @see make
	*/
	restoreOnlySelection(snapshot) {
		try {
			if (snapshot.range) {
				const range = this.j.ed.createRange();
				range.setStart(this.getElementByLadder(snapshot.range.startContainer), snapshot.range.startOffset);
				range.setEnd(this.getElementByLadder(snapshot.range.endContainer), snapshot.range.endOffset);
				this.j.s.selectRange(range);
			}
		} catch (__ignore) {
			this.j.editor.lastChild && this.j.s.setCursorAfter(this.j.editor.lastChild);
		}
	}
	destruct() {
		this.__block(false);
		super.destruct();
	}
	static isIgnoredNode(node) {
		return Dom.isText(node) && !node.nodeValue || Dom.isTemporary(node);
	}
	__getCleanedEditorValue(node) {
		const clone = node.cloneNode(true);
		Dom.temporaryList(clone).forEach(Dom.unwrap);
		return clone.innerHTML;
	}
};
//#endregion
//#region node_modules/jodit/esm/modules/history/stack.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var Stack = class {
	constructor(size) {
		this.size = size;
		this.commands = [];
		this.stackPosition = -1;
	}
	get length() {
		return this.commands.length;
	}
	clearRedo() {
		this.commands.length = this.stackPosition + 1;
	}
	clear() {
		this.commands.length = 0;
		this.stackPosition = -1;
	}
	push(command) {
		this.clearRedo();
		this.commands.push(command);
		this.stackPosition += 1;
		if (this.commands.length > this.size) {
			this.commands.shift();
			this.stackPosition -= 1;
		}
	}
	replace(command) {
		this.commands[this.stackPosition] = command;
	}
	current() {
		return this.commands[this.stackPosition];
	}
	undo() {
		if (this.canUndo()) {
			if (this.commands[this.stackPosition]) this.commands[this.stackPosition].undo();
			this.stackPosition -= 1;
			return true;
		}
		return false;
	}
	redo() {
		if (this.canRedo()) {
			this.stackPosition += 1;
			if (this.commands[this.stackPosition]) this.commands[this.stackPosition].redo();
			return true;
		}
		return false;
	}
	canUndo() {
		return this.stackPosition >= 0;
	}
	canRedo() {
		return this.stackPosition < this.commands.length - 1;
	}
};
//#endregion
//#region node_modules/jodit/esm/modules/history/history.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$17 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
Config.prototype.history = {
	enable: true,
	maxHistoryLength: Infinity,
	timeout: 1e3
};
/**
* The module monitors the status of the editor and creates / deletes the required number of Undo / Redo shots .
*/
var History = class extends ViewComponent {
	/** @override */
	className() {
		return "History";
	}
	/**
	* Return state of the WYSIWYG editor to step back
	*/
	redo() {
		if (this.__stack.redo()) {
			this.startValue = this.snapshot.make();
			this.fireChangeStack();
		}
	}
	canRedo() {
		return this.__stack.canRedo();
	}
	/**
	* Return the state of the WYSIWYG editor to step forward
	*/
	undo() {
		if (this.__stack.undo()) {
			this.startValue = this.snapshot.make();
			this.fireChangeStack();
		}
	}
	canUndo() {
		return this.__stack.canUndo();
	}
	clear() {
		this.startValue = this.snapshot.make();
		this.__stack.clear();
		this.fireChangeStack();
	}
	get length() {
		return this.__stack.length;
	}
	get startValue() {
		return this.__startValue;
	}
	set startValue(value) {
		this.__startValue = value;
	}
	constructor(editor, stack = new Stack(editor.o.history.maxHistoryLength), snapshot = new Snapshot(editor)) {
		super(editor);
		this.updateTick = 0;
		this.__stack = stack;
		this.snapshot = snapshot;
		if (editor.o.history.enable) editor.e.on("afterAddPlace.history", () => {
			if (this.isInDestruct) return;
			this.startValue = this.snapshot.make();
			editor.events.on("internalChange internalUpdate", () => {
				this.startValue = this.snapshot.make();
			}).on(editor.editor, [
				"changeSelection",
				"selectionstart",
				"selectionchange",
				"mousedown",
				"mouseup",
				"keydown",
				"keyup"
			].map((f) => f + ".history").join(" "), () => {
				if (this.startValue.html === this.j.getNativeEditorValue()) this.startValue = this.snapshot.make();
			}).on(this, "change.history", this.onChange);
		});
	}
	/**
	* Update change counter
	* @internal
	*/
	__upTick() {
		this.updateTick += 1;
	}
	/**
	* Push new command in stack on some changes
	*/
	onChange() {
		this.__processChanges();
	}
	/**
	* @internal
	*/
	__processChanges() {
		if (this.snapshot.isBlocked || !this.j.o.history.enable) return;
		this.updateStack();
	}
	/**
	* Update history stack
	*/
	updateStack(replace = false) {
		const newValue = this.snapshot.make();
		if (!Snapshot.equal(newValue, this.startValue)) {
			const newCommand = new Command(this.startValue, newValue, this, this.updateTick);
			if (replace) {
				const command = this.__stack.current();
				if (command && this.updateTick === command.tick) this.__stack.replace(newCommand);
			} else this.__stack.push(newCommand);
			this.startValue = newValue;
			this.fireChangeStack();
		}
	}
	fireChangeStack() {
		var _a;
		this.j && !this.j.isInDestruct && ((_a = this.j.events) === null || _a === void 0 || _a.fire("changeStack"));
	}
	destruct() {
		if (this.isInDestruct) return;
		if (this.j.events) this.j.e.off(".history");
		this.snapshot.destruct();
		super.destruct();
	}
};
__decorate$17([debounce()], History.prototype, "onChange", null);
//#endregion
//#region node_modules/jodit/esm/modules/status-bar/status-bar.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$16 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var StatusBar = class StatusBar extends ViewComponent {
	className() {
		return "StatusBar";
	}
	/**
	* Hide statusbar
	*/
	hide() {
		this.container.classList.add("jodit_hidden");
	}
	/**
	* Show statusbar
	*/
	show() {
		this.container.classList.remove("jodit_hidden");
	}
	/**
	* Status bar is shown
	*/
	get isShown() {
		return !this.container.classList.contains("jodit_hidden");
	}
	/**
	* Height of statusbar
	*/
	getHeight() {
		var _a, _b;
		return (_b = (_a = this.container) === null || _a === void 0 ? void 0 : _a.offsetHeight) !== null && _b !== void 0 ? _b : 0;
	}
	findEmpty(inTheRight = false) {
		const items = this.getElms(inTheRight ? "item-right" : "item");
		for (let i = 0; i < items.length; i += 1) if (!items[i].innerHTML.trim().length) return items[i];
	}
	/**
	* Add element in statusbar
	*/
	append(child, inTheRight = false) {
		const wrapper = this.findEmpty(inTheRight) || this.j.c.div(this.getFullElName("item"));
		if (inTheRight) wrapper.classList.add(this.getFullElName("item-right"));
		Dom.append(wrapper, child);
		this.container && Dom.append(this.container, wrapper);
		if (this.j.o.statusbar) this.show();
		this.j.e.fire("resize");
	}
	constructor(jodit, target) {
		super(jodit);
		this.target = target;
		this.mods = {};
		this.container = jodit.c.div("jodit-status-bar");
		Dom.append(target, this.container);
		this.hide();
	}
	destruct() {
		if (this.isInDestruct) return;
		this.setStatus(STATUSES.beforeDestruct);
		Dom.safeRemove(this.container);
		super.destruct();
	}
};
StatusBar = __decorate$16([component, derive(Mods, Elms)], StatusBar);
//#endregion
//#region node_modules/jodit/esm/modules/table/table.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$15 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var markedValue = /* @__PURE__ */ new WeakMap();
var Table = class Table extends ViewComponent {
	constructor() {
		super(...arguments);
		this.selected = /* @__PURE__ */ new Set();
	}
	/** @override */
	className() {
		return "Table";
	}
	__recalculateStyles() {
		const style = getContainer(this.j, Table, "style", true);
		const selectors = [];
		this.selected.forEach((td) => {
			const selector = cssPath(td);
			selector && selectors.push(selector);
		});
		style.innerHTML = selectors.length ? selectors.join(",") + `{${this.jodit.options.table.selectionCellStyle}}` : "";
	}
	addSelection(td) {
		this.selected.add(td);
		this.__recalculateStyles();
		const table = Dom.closest(td, "table", this.j.editor);
		if (table) {
			const cells = Table.__selectedByTable.get(table) || /* @__PURE__ */ new Set();
			cells.add(td);
			Table.__selectedByTable.set(table, cells);
		}
	}
	removeSelection(td) {
		this.selected.delete(td);
		this.__recalculateStyles();
		const table = Dom.closest(td, "table", this.j.editor);
		if (table) {
			const cells = Table.__selectedByTable.get(table);
			if (cells) {
				cells.delete(td);
				if (!cells.size) Table.__selectedByTable.delete(table);
			}
		}
	}
	/**
	* Returns array of selected cells
	*/
	getAllSelectedCells() {
		return toArray(this.selected);
	}
	static __getSelectedCellsByTable(table) {
		const cells = Table.__selectedByTable.get(table);
		return cells ? toArray(cells) : [];
	}
	/** @override **/
	destruct() {
		this.selected.clear();
		return super.destruct();
	}
	static __getRowsCount(table) {
		return table.rows.length;
	}
	/**
	* Returns rows count in the table
	*/
	getRowsCount(table) {
		return Table.__getRowsCount(table);
	}
	static __getColumnsCount(table) {
		return Table.__formalMatrix(table).reduce((max_count, cells) => Math.max(max_count, cells.length), 0);
	}
	/**
	* Returns columns count in the table
	*/
	getColumnsCount(table) {
		return Table.__getColumnsCount(table);
	}
	static __formalMatrix(table, callback) {
		const matrix = [[]];
		const rows = toArray(table.rows);
		const setCell = (cell, i) => {
			if (matrix[i] === void 0) matrix[i] = [];
			const colSpan = cell.colSpan, rowSpan = cell.rowSpan;
			let column, row, currentColumn = 0;
			while (matrix[i][currentColumn]) currentColumn += 1;
			for (row = 0; row < rowSpan; row += 1) for (column = 0; column < colSpan; column += 1) {
				if (matrix[i + row] === void 0) matrix[i + row] = [];
				if (callback && callback(cell, i + row, currentColumn + column, colSpan, rowSpan) === false) return false;
				matrix[i + row][currentColumn + column] = cell;
			}
		};
		for (let i = 0; i < rows.length; i += 1) {
			const cells = toArray(rows[i].cells);
			for (let j = 0; j < cells.length; j += 1) if (setCell(cells[j], i) === false) return matrix;
		}
		return matrix;
	}
	/**
	* Generate formal table martix columns*rows
	* @param table - Working table
	* @param callback - if return false cycle break
	*/
	formalMatrix(table, callback) {
		return Table.__formalMatrix(table, callback);
	}
	static __formalCoordinate(table, cell, max = false) {
		let i = 0, j = 0, width = 1, height = 1;
		Table.__formalMatrix(table, (td, ii, jj, colSpan, rowSpan) => {
			if (cell === td) {
				i = ii;
				j = jj;
				width = colSpan || 1;
				height = rowSpan || 1;
				if (max) {
					j += (colSpan || 1) - 1;
					i += (rowSpan || 1) - 1;
				}
				return false;
			}
		});
		return [
			i,
			j,
			width,
			height
		];
	}
	/**
	* Get cell coordinate in formal table (without colspan and rowspan)
	*/
	formalCoordinate(table, cell, max = false) {
		return Table.__formalCoordinate(table, cell, max);
	}
	static __appendRow(table, line, after, create) {
		let row;
		if (!line) {
			const columnsCount = Table.__getColumnsCount(table);
			row = create.element("tr");
			for (let j = 0; j < columnsCount; j += 1) Dom.append(row, create.element("td"));
		} else {
			row = line.cloneNode(true);
			Dom.each(line, (cell) => {
				if (!Dom.isCell(cell)) return;
				const rowspan = attr(cell, "rowspan");
				if (rowspan && parseInt(rowspan, 10) > 1) {
					const newRowSpan = parseInt(rowspan, 10) - 1;
					attr(cell, "rowspan", newRowSpan > 1 ? newRowSpan : null);
				}
			});
			Dom.each(row, (cell) => {
				if (Dom.isCell(cell)) Dom.detach(cell);
			});
		}
		if (after && line && line.nextSibling) Dom.after(line, row);
		else if (!after && line) Dom.before(line, row);
		else Dom.append(Dom.first(table, (node) => Dom.isTag(node, "tbody")) || table, row);
	}
	/**
	* Inserts a new line after row what contains the selected cell
	*
	* @param table - Working table
	* @param line - Insert a new line after/before this
	* line contains the selected cell
	* @param after - Insert a new line after line contains the selected cell
	*/
	appendRow(table, line, after) {
		return Table.__appendRow(table, line, after, this.j.createInside);
	}
	static __removeRow(table, rowIndex) {
		const box = Table.__formalMatrix(table);
		let dec;
		const row = table.rows[rowIndex];
		box[rowIndex].forEach((cell, j) => {
			dec = false;
			if (rowIndex - 1 >= 0 && box[rowIndex - 1][j] === cell) dec = true;
			else if (box[rowIndex + 1] && box[rowIndex + 1][j] === cell) {
				if (cell.parentNode === row && cell.parentNode.nextSibling) {
					dec = true;
					let nextCell = j + 1;
					while (box[rowIndex + 1][nextCell] === cell) nextCell += 1;
					const nextRow = Dom.next(cell.parentNode, (elm) => Dom.isTag(elm, "tr"), table);
					if (nextRow) {
						let referenceCell = null;
						for (let nextColumn = nextCell; nextColumn < box[rowIndex + 1].length; nextColumn += 1) {
							const candidate = box[rowIndex + 1][nextColumn];
							if (candidate && candidate.parentNode === nextRow) {
								referenceCell = candidate;
								break;
							}
						}
						if (referenceCell) Dom.before(referenceCell, cell);
						else Dom.append(nextRow, cell);
					}
				}
			} else Dom.safeRemove(cell);
			if (dec && (cell.parentNode === row || cell !== box[rowIndex][j - 1])) {
				const rowSpan = cell.rowSpan;
				attr(cell, "rowspan", rowSpan - 1 > 1 ? rowSpan - 1 : null);
			}
		});
		Dom.safeRemove(row);
	}
	/**
	* Remove row
	*/
	removeRow(table, rowIndex) {
		return Table.__removeRow(table, rowIndex);
	}
	/**
	* Insert column before / after all the columns containing the selected cells
	*/
	appendColumn(table, selectedCell, insertAfter = true) {
		const box = Table.__formalMatrix(table);
		if (!insertAfter && Dom.isCell(selectedCell.previousElementSibling)) return this.appendColumn(table, selectedCell.previousElementSibling, true);
		const [, formalCol] = Table.__formalCoordinate(table, selectedCell, insertAfter);
		const columnIndex = formalCol;
		const newColumnIndex = insertAfter ? columnIndex + 1 : columnIndex;
		for (let i = 0; i < box.length;) {
			const cells = box[i];
			if (cells[columnIndex] !== cells[newColumnIndex] || columnIndex === newColumnIndex) {
				const cell = this.j.createInside.element("td");
				if (insertAfter) Dom.after(cells[columnIndex], cell);
				else Dom.before(cells[columnIndex], cell);
				if (cells[columnIndex].rowSpan > 1) cell.rowSpan = cells[columnIndex].rowSpan;
			} else cells[columnIndex].colSpan += 1;
			i += cells[columnIndex].rowSpan || 1;
		}
	}
	static __removeColumn(table, j) {
		const box = Table.__formalMatrix(table);
		let dec;
		box.forEach((cells, i) => {
			const td = cells[j];
			dec = false;
			if (j - 1 >= 0 && box[i][j - 1] === td) dec = true;
			else if (j + 1 < cells.length && box[i][j + 1] === td) dec = true;
			else Dom.safeRemove(td);
			if (dec && (i - 1 < 0 || td !== box[i - 1][j])) {
				const colSpan = td.colSpan;
				attr(td, "colspan", colSpan - 1 > 1 ? (colSpan - 1).toString() : null);
			}
		});
	}
	/**
	* Remove column by index
	*/
	removeColumn(table, j) {
		return Table.__removeColumn(table, j);
	}
	static __getSelectedBound(table, selectedCells) {
		const bound = [[Infinity, Infinity], [0, 0]];
		const box = Table.__formalMatrix(table);
		let i, j, k;
		for (i = 0; i < box.length; i += 1) for (j = 0; box[i] && j < box[i].length; j += 1) if (selectedCells.includes(box[i][j])) {
			bound[0][0] = Math.min(i, bound[0][0]);
			bound[0][1] = Math.min(j, bound[0][1]);
			bound[1][0] = Math.max(i, bound[1][0]);
			bound[1][1] = Math.max(j, bound[1][1]);
		}
		for (i = bound[0][0]; i <= bound[1][0]; i += 1) for (k = 1, j = bound[0][1]; j <= bound[1][1]; j += 1) {
			while (box[i] && box[i][j - k] && box[i][j] === box[i][j - k]) {
				bound[0][1] = Math.min(j - k, bound[0][1]);
				bound[1][1] = Math.max(j - k, bound[1][1]);
				k += 1;
			}
			k = 1;
			while (box[i] && box[i][j + k] && box[i][j] === box[i][j + k]) {
				bound[0][1] = Math.min(j + k, bound[0][1]);
				bound[1][1] = Math.max(j + k, bound[1][1]);
				k += 1;
			}
			k = 1;
			while (box[i - k] && box[i][j] === box[i - k][j]) {
				bound[0][0] = Math.min(i - k, bound[0][0]);
				bound[1][0] = Math.max(i - k, bound[1][0]);
				k += 1;
			}
			k = 1;
			while (box[i + k] && box[i][j] === box[i + k][j]) {
				bound[0][0] = Math.min(i + k, bound[0][0]);
				bound[1][0] = Math.max(i + k, bound[1][0]);
				k += 1;
			}
		}
		return bound;
	}
	/**
	* Define bound for selected cells
	*/
	getSelectedBound(table, selectedCells) {
		return Table.__getSelectedBound(table, selectedCells);
	}
	static __normalizeTable(table) {
		const __marked = [], box = Table.__formalMatrix(table);
		Table.__removeExtraColspans(box, __marked);
		Table.__removeExtraRowspans(box, __marked);
		for (let i = 0; i < box.length; i += 1) for (let j = 0; j < box[i].length; j += 1) {
			if (box[i][j] === void 0) continue;
			if (box[i][j].hasAttribute("rowspan") && box[i][j].rowSpan === 1) attr(box[i][j], "rowspan", null);
			if (box[i][j].hasAttribute("colspan") && box[i][j].colSpan === 1) attr(box[i][j], "colspan", null);
			if (box[i][j].hasAttribute("class") && !attr(box[i][j], "class")) attr(box[i][j], "class", null);
		}
		Table.__unmark(__marked);
	}
	static __removeExtraColspans(box, __marked) {
		for (let j = 0; j < box[0].length; j += 1) {
			let min = 1e6;
			let not = false;
			for (let i = 0; i < box.length; i += 1) {
				if (box[i][j] === void 0) continue;
				if (box[i][j].colSpan < 2) {
					not = true;
					break;
				}
				min = Math.min(min, box[i][j].colSpan);
			}
			if (!not) for (let i = 0; i < box.length; i += 1) {
				if (box[i][j] === void 0) continue;
				Table.__mark(box[i][j], "colspan", box[i][j].colSpan - min + 1, __marked);
			}
		}
	}
	static __removeExtraRowspans(box, marked) {
		let i = 0;
		let j = 0;
		for (i = 0; i < box.length; i += 1) {
			let min = 1e6;
			let not = false;
			for (j = 0; j < box[i].length; j += 1) {
				if (box[i][j] === void 0) continue;
				if (box[i][j].rowSpan < 2) {
					not = true;
					break;
				}
				min = Math.min(min, box[i][j].rowSpan);
			}
			if (!not) for (j = 0; j < box[i].length; j += 1) {
				if (box[i][j] === void 0) continue;
				Table.__mark(box[i][j], "rowspan", box[i][j].rowSpan - min + 1, marked);
			}
		}
	}
	/**
	* Try recalculate all coluns and rows after change
	*/
	normalizeTable(table) {
		return Table.__normalizeTable(table);
	}
	static __mergeSelected(table, jodit) {
		const html = [], bound = Table.__getSelectedBound(table, Table.__getSelectedCellsByTable(table));
		let w = 0, first = null, first_j = 0, td, cols = 0, rows = 0;
		const alreadyMerged = /* @__PURE__ */ new Set(), __marked = [];
		if (bound && (bound[0][0] - bound[1][0] || bound[0][1] - bound[1][1])) {
			Table.__formalMatrix(table, (cell, i, j, cs, rs) => {
				if (i >= bound[0][0] && i <= bound[1][0]) {
					if (j >= bound[0][1] && j <= bound[1][1]) {
						td = cell;
						if (alreadyMerged.has(td)) return;
						alreadyMerged.add(td);
						if (i === bound[0][0] && cssInline(td, "width")) w += td.offsetWidth;
						if (trim(cell.innerHTML.replace(/<br(\/)?>/g, "")) !== "") html.push(cell.innerHTML);
						if (cs > 1) cols += cs - 1;
						if (rs > 1) rows += rs - 1;
						if (!first) {
							first = cell;
							first_j = j;
						} else {
							Table.__mark(td, "remove", 1, __marked);
							instance(jodit).removeSelection(td);
						}
					}
				}
			});
			cols = bound[1][1] - bound[0][1] + 1;
			rows = bound[1][0] - bound[0][0] + 1;
			if (first) {
				if (cols > 1) Table.__mark(first, "colspan", cols, __marked);
				if (rows > 1) Table.__mark(first, "rowspan", rows, __marked);
				if (w) {
					Table.__mark(first, "width", (w / table.offsetWidth * 100).toFixed(10) + "%", __marked);
					if (first_j) Table.__setColumnWidthByDelta(table, first_j, 0, true, __marked);
				}
				first.innerHTML = html.join("<br/>");
				instance(jodit).addSelection(first);
				alreadyMerged.delete(first);
				Table.__unmark(__marked);
				Table.__normalizeTable(table);
				toArray(table.rows).forEach((tr) => {
					if (!tr.cells.length) Dom.safeRemove(tr);
				});
			}
		}
	}
	/**
	* It combines all the selected cells into one. The contents of the cells will also be combined
	*/
	mergeSelected(table) {
		return Table.__mergeSelected(table, this.j);
	}
	static __splitHorizontal(table, jodit) {
		let coord, td, tr, parent, after;
		const __marked = [];
		Table.__getSelectedCellsByTable(table).forEach((cell) => {
			td = jodit.createInside.element("td");
			Dom.append(td, jodit.createInside.element("br"));
			tr = jodit.createInside.element("tr");
			coord = Table.__formalCoordinate(table, cell);
			if (cell.rowSpan < 2) {
				Table.__formalMatrix(table, (tdElm, i, j) => {
					if (coord[0] === i && coord[1] !== j && tdElm !== cell) Table.__mark(tdElm, "rowspan", tdElm.rowSpan + 1, __marked);
				});
				Dom.after(Dom.closest(cell, "tr", table), tr);
				Dom.append(tr, td);
			} else {
				Table.__mark(cell, "rowspan", cell.rowSpan - 1, __marked);
				Table.__formalMatrix(table, (tdElm, i, j) => {
					if (i > coord[0] && i < coord[0] + cell.rowSpan && coord[1] > j && tdElm.parentNode.rowIndex === i) after = tdElm;
					if (coord[0] < i && tdElm === cell) parent = table.rows[i];
				});
				if (after) Dom.after(after, td);
				else Dom.prepend(parent, td);
			}
			if (cell.colSpan > 1) Table.__mark(td, "colspan", cell.colSpan, __marked);
			Table.__unmark(__marked);
			instance(jodit).removeSelection(cell);
		});
		this.__normalizeTable(table);
	}
	/**
	* Divides all selected by `jodit_focused_cell` class table cell in 2 parts vertical. Those division into 2 columns
	*/
	splitHorizontal(table) {
		return Table.__splitHorizontal(table, this.j);
	}
	static __splitVertical(table, jodit) {
		let coord, td, percentage;
		const __marked = [];
		Table.__getSelectedCellsByTable(table).forEach((cell) => {
			coord = Table.__formalCoordinate(table, cell);
			if (cell.colSpan < 2) Table.__formalMatrix(table, (tdElm, i, j) => {
				if (coord[1] === j && coord[0] !== i && tdElm !== cell) Table.__mark(tdElm, "colspan", tdElm.colSpan + 1, __marked);
			});
			else Table.__mark(cell, "colspan", cell.colSpan - 1, __marked);
			td = jodit.createInside.element("td");
			Dom.append(td, jodit.createInside.element("br"));
			if (cell.rowSpan > 1) Table.__mark(td, "rowspan", cell.rowSpan, __marked);
			const oldWidth = cell.offsetWidth;
			Dom.after(cell, td);
			percentage = oldWidth / table.offsetWidth / 2;
			Table.__mark(cell, "width", (percentage * 100).toFixed(10) + "%", __marked);
			Table.__mark(td, "width", (percentage * 100).toFixed(10) + "%", __marked);
			Table.__unmark(__marked);
			instance(jodit).removeSelection(cell);
		});
		Table.__normalizeTable(table);
	}
	/**
	* It splits all the selected cells into 2 parts horizontally. Those. are added new row
	*/
	splitVertical(table) {
		return Table.__splitVertical(table, this.j);
	}
	static __setColumnWidthByDelta(table, column, delta, noUnmark, marked) {
		const box = Table.__formalMatrix(table);
		let clearWidthIndex = 0;
		for (let i = 0; i < box.length; i += 1) {
			const cell = box[i][column];
			if (cell.colSpan > 1 && box.length > 1) continue;
			const percent = (cell.offsetWidth + delta) / table.offsetWidth * 100;
			Table.__mark(cell, "width", percent.toFixed(10) + "%", marked);
			clearWidthIndex = i;
			break;
		}
		for (let i = clearWidthIndex + 1; i < box.length; i += 1) {
			const cell = box[i][column];
			Table.__mark(cell, "width", null, marked);
		}
		if (!noUnmark) Table.__unmark(marked);
	}
	/**
	* Set column width used delta value
	*/
	setColumnWidthByDelta(table, column, delta, noUnmark, marked) {
		return Table.__setColumnWidthByDelta(table, column, delta, noUnmark, marked);
	}
	static __mark(cell, key, value, marked) {
		var _a;
		marked.push(cell);
		const dict = (_a = markedValue.get(cell)) !== null && _a !== void 0 ? _a : {};
		dict[key] = value === void 0 ? 1 : value;
		markedValue.set(cell, dict);
	}
	static __unmark(marked) {
		marked.forEach((cell) => {
			const dict = markedValue.get(cell);
			if (dict) {
				Object.keys(dict).forEach((key) => {
					const value = dict[key];
					switch (key) {
						case "remove":
							Dom.safeRemove(cell);
							break;
						case "rowspan":
							attr(cell, "rowspan", isNumber(value) && value > 1 ? value : null);
							break;
						case "colspan":
							attr(cell, "colspan", isNumber(value) && value > 1 ? value : null);
							break;
						case "width":
							if (value == null) {
								cell.style.removeProperty("width");
								if (!attr(cell, "style")) attr(cell, "style", null);
							} else css(cell, "width", value.toString());
							break;
					}
					delete dict[key];
				});
				markedValue.delete(cell);
			}
		});
	}
};
Table.__selectedByTable = /* @__PURE__ */ new WeakMap();
__decorate$15([debounce()], Table.prototype, "__recalculateStyles", null);
var instance = (j) => j.getInstance("Table", j.o);
//#endregion
//#region node_modules/jodit/esm/modules/toolbar/button/button.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$14 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var ToolbarButton = class ToolbarButton extends UIButton {
	className() {
		return "ToolbarButton";
	}
	getRole() {
		return "listitem";
	}
	updateAriaLabel() {
		super.updateAriaLabel();
		if (this.trigger) {
			const i8nTooltip = this.state.tooltip ? this.jodit.i18n(this.state.tooltip) : null;
			attr(this.trigger, "aria-label", i8nTooltip);
		}
	}
	/**
	* Get parent toolbar
	*/
	get toolbar() {
		const ToolbarCollection = getComponentClass("ToolbarCollection");
		return this.closest(ToolbarCollection);
	}
	update() {
		var _a, _b;
		const { control, state } = this;
		const tc = this.toolbar;
		if (!tc) return;
		const value = (_a = control.value) === null || _a === void 0 ? void 0 : _a.call(control, tc.jodit, this);
		if (value !== void 0) state.value = value;
		state.disabled = this.__calculateDisabledStatus(tc);
		state.activated = this.__calculateActivatedStatus(tc);
		(_b = control.update) === null || _b === void 0 || _b.call(control, tc.jodit, this);
	}
	/**
	* Calculates whether the button is active
	*/
	__calculateActivatedStatus(tc) {
		var _a, _b;
		if (isJoditObject(this.j) && !this.j.editorIsActive) return false;
		if ((_b = (_a = this.control).isActive) === null || _b === void 0 ? void 0 : _b.call(_a, this.j, this)) return true;
		return Boolean(tc && tc.shouldBeActive(this));
	}
	/**
	* Calculates whether an element is blocked for the user
	*/
	__calculateDisabledStatus(tc) {
		var _a, _b;
		if (this.j.o.disabled) return true;
		if (this.j.o.readonly && (!this.j.o.activeButtonsInReadOnly || !this.j.o.activeButtonsInReadOnly.includes(this.control.name))) return true;
		if ((_b = (_a = this.control).isDisabled) === null || _b === void 0 ? void 0 : _b.call(_a, this.j, this)) return true;
		return Boolean(tc && tc.shouldBeDisabled(this));
	}
	onChangeActivated() {
		attr(this.button, "aria-pressed", this.state.activated);
		super.onChangeActivated();
	}
	onChangeText() {
		if (isFunction(this.control.template)) this.text.innerHTML = this.control.template(this.j, this.control.name, this.j.i18n(this.state.text));
		else super.onChangeText();
		this.setMod("text-icons", Boolean(this.text.innerText.trim().length));
	}
	onChangeTabIndex() {
		attr(this.button, "tabindex", this.state.tabIndex);
		attr(this.trigger, "tabindex", this.state.tabIndex);
	}
	createContainer() {
		const cn = this.componentName;
		const container = this.j.c.span(cn);
		const button = super.createContainer();
		button.classList.remove(cn);
		button.classList.add(cn + "__button");
		Object.defineProperty(button, "component", {
			value: this,
			configurable: true
		});
		Dom.append(container, button);
		const trigger = this.j.c.fromHTML(`<span role="button" aria-haspopup="true" aria-expanded="false" class="${cn}__trigger">${Icon.get("chevron")}</span>`);
		Dom.append(button, trigger);
		return container;
	}
	/** @override */
	focus() {
		var _a;
		(_a = Dom.first(this.container, (node) => Dom.isTag(node, "button"))) === null || _a === void 0 || _a.focus();
	}
	onChangeHasTrigger() {
		if (this.state.hasTrigger) Dom.append(this.container, this.trigger);
		else Dom.safeRemove(this.trigger);
		this.setMod("with-trigger", this.state.hasTrigger || null);
	}
	/** @override */
	onChangeDisabled() {
		const disabled = this.state.disabled ? "disabled" : null;
		attr(this.trigger, "disabled", disabled);
		attr(this.button, "disabled", disabled);
		attr(this.container, "disabled", disabled);
	}
	constructor(jodit, control, target = null) {
		super(jodit);
		this.control = control;
		this.target = target;
		this.state = {
			...UIButtonState(),
			theme: "toolbar",
			currentValue: "",
			hasTrigger: false
		};
		this.openedPopup = null;
		const button = this.getElm("button");
		assert(button, "Element button should exists");
		this.button = button;
		Object.defineProperty(button, "component", {
			value: this,
			configurable: true
		});
		const trigger = this.getElm("trigger");
		assert(trigger, "Element trigger should exists");
		this.trigger = trigger;
		trigger.remove();
		jodit.e.on([this.button, this.trigger], "mousedown", (e) => e.preventDefault());
		this.onAction(this.onClick);
		this.hookStatus(STATUSES.ready, () => {
			this.__initFromControl();
			this.update();
		});
		if (control.mods) Object.keys(control.mods).forEach((mod) => {
			control.mods && this.setMod(mod, control.mods[mod]);
		});
	}
	/**
	* Init constant data from control
	*/
	__initFromControl() {
		const { control: ctr, state } = this;
		this.updateSize();
		state.name = ctr.name;
		this.__initIconFromControl();
		if (ctr.tooltip) state.tooltip = isFunction(ctr.tooltip) ? ctr.tooltip(this.j, ctr, this) : ctr.tooltip;
		state.hasTrigger = Boolean(ctr.list || ctr.popup && ctr.exec);
	}
	__initIconFromControl() {
		var _a;
		const { control: ctr, state } = this;
		const { textIcons } = this.j.o;
		if (textIcons === true || isFunction(textIcons) && textIcons(ctr.name) || ctr.template) {
			state.icon = UIButtonState().icon;
			state.text = ctr.text || ctr.name;
			return;
		}
		if (!isString(ctr.icon) && ctr.icon != null) {
			state.icon = {
				name: ctr.icon.name || ctr.name,
				iconURL: ctr.icon.iconURL || "",
				fill: ctr.icon.fill || "",
				scale: ctr.icon.scale
			};
			return;
		}
		if (ctr.iconURL) state.icon.iconURL = ctr.iconURL;
		else {
			const name = ctr.icon || ctr.name;
			state.icon.name = Icon.exists(name) || ((_a = this.j.o.extraIcons) === null || _a === void 0 ? void 0 : _a[name]) ? name : "";
		}
		if (!ctr.iconURL && !state.icon.name) state.text = ctr.text || ctr.name;
	}
	/**
	* Click on trigger button
	*/
	onTriggerClick(e) {
		var _a, _b, _c;
		if (this.openedPopup) {
			this.__closePopup();
			return;
		}
		const { control: ctr } = this;
		attr(this.trigger, "aria-expanded", true);
		e.buffer = { actionTrigger: this };
		if (ctr.list) return this.__openControlList(ctr);
		if (isFunction(ctr.popup)) {
			const popup = this.openPopup();
			popup.parentElement = this;
			try {
				if (this.j.e.fire(camelCase(`before-${ctr.name}-open-popup`), this.target, ctr, popup) !== false) {
					const target = (_c = (_b = (_a = this.toolbar) === null || _a === void 0 ? void 0 : _a.getTarget(this)) !== null && _b !== void 0 ? _b : this.target) !== null && _c !== void 0 ? _c : null;
					const elm = ctr.popup(this.j, target, this.__closePopup, this);
					if (elm) popup.setContent(isString(elm) ? this.j.c.fromHTML(elm) : elm).open(() => position(this.container), false, this.j.o.allowTabNavigation ? this.container : void 0);
					else this.__closePopup();
				}
			} catch (e) {
				this.__closePopup();
				throw e;
			}
			/**
			* Fired after the popup was opened for some control button
			*/
			/**
			* Close all opened popups
			*/
			this.j.e.fire(camelCase(`after-${ctr.name}-open-popup`), popup.container);
		}
	}
	onTriggerKeyDown(e) {
		if (this.state.disabled || ![
			"Enter",
			"Space",
			" "
		].includes(e.key)) return;
		e.preventDefault();
		this.trigger.click();
	}
	/**
	* Create an open popup list
	*/
	__openControlList(control) {
		var _a;
		const controls = (_a = this.jodit.options.controls) !== null && _a !== void 0 ? _a : {}, getControl = (key) => findControlType(key, controls);
		const list = control.list;
		const menu = this.openPopup();
		const toolbar = makeCollection(this.j);
		menu.parentElement = this;
		toolbar.parentElement = menu;
		toolbar.mode = "vertical";
		const isListItem = (key) => isPlainObject(key) && "title" in key && "value" in key;
		const getButton = (key, value) => {
			if (isString(value) && getControl(value)) return {
				name: value.toString(),
				...getControl(value)
			};
			if (isString(key) && getControl(key)) return {
				name: key.toString(),
				...getControl(key),
				...typeof value === "object" ? value : {}
			};
			if (isListItem(key)) {
				value = key.value;
				key = key.title;
			}
			const { childTemplate } = control;
			const childControl = {
				name: key.toString(),
				template: childTemplate && ((j, k, v) => childTemplate(j, k, v, this)),
				exec: control.childExec ? (view, current, options) => {
					var _a;
					return (_a = control.childExec) === null || _a === void 0 ? void 0 : _a.call(control, view, current, {
						...options,
						parentControl: control
					});
				} : control.exec,
				data: control.data,
				command: control.command,
				isActive: control.isChildActive,
				value: control.value,
				isDisabled: control.isChildDisabled,
				mode: control.mode,
				args: [
					...control.args ? control.args : [],
					key,
					value
				]
			};
			if (isString(value)) childControl.text = value;
			return childControl;
		};
		toolbar.build(isArray(list) ? list.map(getButton) : keys(list, false).map((key) => getButton(key, list[key])), this.target);
		menu.setContent(toolbar).open(() => position(this.container), false, this.j.o.allowTabNavigation ? this.container : void 0);
		this.state.activated = true;
	}
	onOutsideClick(e) {
		if (!this.openedPopup) return;
		if (!e || !Dom.isNode(e.target) || !Dom.isOrContains(this.container, e.target) && !this.openedPopup.isOwnClick(e)) this.__closePopup();
	}
	openPopup() {
		this.__closePopup();
		this.openedPopup = new Popup(this.j, false);
		this.j.e.on(this.ow, "mousedown touchstart", this.onOutsideClick).on("escape closeAllPopups", this.onOutsideClick);
		return this.openedPopup;
	}
	__closePopup() {
		const popup = this.openedPopup;
		if (!popup) return;
		this.openedPopup = null;
		this.j.e.off(this.ow, "mousedown touchstart", this.onOutsideClick).off("escape closeAllPopups", this.onOutsideClick);
		this.state.activated = false;
		popup.close();
		popup.destruct();
		if (this.trigger) attr(this.trigger, "aria-expanded", false);
	}
	/**
	* Click handler
	*/
	onClick(originalEvent) {
		var _a, _b, _c, _d, _e, _f, _g;
		const { control: ctr } = this;
		if (isFunction(ctr.exec)) {
			const target = (_c = (_b = (_a = this.toolbar) === null || _a === void 0 ? void 0 : _a.getTarget(this)) !== null && _b !== void 0 ? _b : this.target) !== null && _c !== void 0 ? _c : null;
			const result = ctr.exec(this.j, target, {
				control: ctr,
				originalEvent,
				button: this
			});
			if (result !== false && result !== true) {
				(_e = (_d = this.j) === null || _d === void 0 ? void 0 : _d.e) === null || _e === void 0 || _e.fire("synchro");
				if (this.parentElement) this.parentElement.update();
				/**
				* Fired after calling `button.exec` function
				*/
				(_g = (_f = this.j) === null || _f === void 0 ? void 0 : _f.e) === null || _g === void 0 || _g.fire("closeAllPopups afterExec");
			}
			if (result !== false) return;
		}
		if (ctr.list) return this.__openControlList(ctr);
		if (isFunction(ctr.popup)) return this.onTriggerClick(originalEvent);
		if (ctr.command || ctr.name) {
			call(isJoditObject(this.j) ? this.j.execCommand.bind(this.j) : this.j.od.execCommand.bind(this.j.od), ctr.command || ctr.name, false, ctr.args && ctr.args[0]);
			this.j.e.fire("closeAllPopups");
		}
	}
	destruct() {
		this.__closePopup();
		return super.destruct();
	}
};
__decorate$14([cacheHTML], ToolbarButton.prototype, "createContainer", null);
__decorate$14([watch("state.hasTrigger", { immediately: false })], ToolbarButton.prototype, "onChangeHasTrigger", null);
__decorate$14([watch("trigger:click")], ToolbarButton.prototype, "onTriggerClick", null);
__decorate$14([watch("trigger:keydown")], ToolbarButton.prototype, "onTriggerKeyDown", null);
__decorate$14([autobind], ToolbarButton.prototype, "onOutsideClick", null);
__decorate$14([autobind], ToolbarButton.prototype, "__closePopup", null);
ToolbarButton = __decorate$14([component], ToolbarButton);
//#endregion
//#region node_modules/jodit/esm/modules/toolbar/button/content.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$13 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var FORM_CONTROL_TAGS = /* @__PURE__ */ new Set([
	"input",
	"button",
	"select",
	"textarea"
]);
var ToolbarContent = class ToolbarContent extends UIButton {
	/** @override */
	className() {
		return "ToolbarContent";
	}
	/** @override */
	update() {
		var _a, _b, _c;
		const { control } = this;
		const content = control.getContent(this.j, this);
		if (isString(content) || content.parentNode !== this.container) {
			Dom.detach(this.container);
			Dom.append(this.container, isString(content) ? this.j.create.fromHTML(content) : content);
		}
		this.state.disabled = Boolean((_a = control.isDisabled) === null || _a === void 0 ? void 0 : _a.call(control, this.j, this));
		this.state.activated = Boolean((_b = control.isActive) === null || _b === void 0 ? void 0 : _b.call(control, this.j, this));
		(_c = control.update) === null || _c === void 0 || _c.call(control, this.j, this);
		this.onChangeDisabled();
		this.onChangeActivated();
		super.update();
	}
	/**
	* The content is arbitrary HTML — propagate the disabled state to the
	* nested form controls (e.g. the file input of the Upload button),
	* otherwise they stay interactive.
	*/
	onChangeDisabled() {
		super.onChangeDisabled();
		Dom.each(this.container, (elm) => {
			if (Dom.isTag(elm, FORM_CONTROL_TAGS)) attr(elm, "disabled", this.state.disabled || null);
		});
	}
	/** @override */
	createContainer() {
		return this.j.c.span(this.componentName);
	}
	constructor(jodit, control, target = null) {
		super(jodit);
		this.control = control;
		this.target = target;
		this.container.classList.add(`${this.componentName}_${this.clearName(control.name)}`);
		attr(this.container, "role", "content");
	}
};
ToolbarContent = __decorate$13([component], ToolbarContent);
//#endregion
//#region node_modules/jodit/esm/modules/toolbar/button/select/select.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$12 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var ToolbarSelect = class ToolbarSelect extends ToolbarButton {
	className() {
		return "ToolbarSelect";
	}
	update() {
		var _a, _b, _c;
		super.update();
		this.state.icon.name = "";
		const { list, data } = this.control;
		if (list) {
			let key = this.state.value || (data && isString(data.currentValue) ? data.currentValue : void 0);
			if (!key) key = Object.keys(list)[0];
			const text = (isPlainObject(list) ? list[key.toString()] || key : key).toString();
			this.state.text = (_c = (_b = (_a = this.control).textTemplate) === null || _b === void 0 ? void 0 : _b.call(_a, this.jodit, text)) !== null && _c !== void 0 ? _c : text;
		}
	}
};
ToolbarSelect = __decorate$12([component], ToolbarSelect);
//#endregion
//#region node_modules/jodit/esm/modules/toolbar/button/index.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
//#endregion
//#region node_modules/jodit/esm/modules/toolbar/collection/collection.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$11 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var ToolbarCollection = class ToolbarCollection extends UIList {
	className() {
		return "ToolbarCollection";
	}
	getRole() {
		return "toolbar";
	}
	/**
	* First button in a list
	*/
	get firstButton() {
		const [button] = this.buttons;
		return button || null;
	}
	makeButton(control, target = null) {
		return makeButton(this.j, control, target);
	}
	makeSelect(control, target = null) {
		return makeSelect(this.j, control, target);
	}
	/**
	* Button should be active
	*/
	shouldBeActive(button) {}
	/**
	* The Button should be disabled
	*/
	shouldBeDisabled(button) {}
	/**
	* Returns current target for button
	*/
	getTarget(button) {
		return button.target || null;
	}
	__immediateUpdate() {
		if (this.isDestructed || this.j.isLocked) return;
		super.update();
		this.j.e.fire("afterUpdateToolbar", this);
	}
	update() {
		this.__immediateUpdate();
	}
	/**
	* Set direction
	*/
	setDirection(direction) {
		css(this.container, "direction", direction);
		attr(this.container, "dir", direction);
	}
	constructor(jodit) {
		super(jodit);
		this.__listenEvents = "updatePlugins updateToolbar changeStack mousedown mouseup keydown change readonly afterResize selectionchange changeSelection focus afterSetMode touchstart focus blur";
	}
	__initEvents() {
		this.j.e.on(this.__listenEvents, this.update).on("afterSetMode focus", this.__immediateUpdate);
	}
	hide() {
		this.container.remove();
	}
	show() {
		this.appendTo(this.j.toolbarContainer);
	}
	showInline(bound) {
		throw error("The method is not implemented for this class.");
	}
	/** @override **/
	build(items, target = null) {
		const itemsWithGroupps = this.j.e.fire("beforeToolbarBuild", items);
		if (itemsWithGroupps) items = itemsWithGroupps;
		super.build(items, target);
		return this;
	}
	/** @override **/
	destruct() {
		if (this.isDestructed) return;
		this.j.e.off(this.__listenEvents, this.update).off("afterSetMode focus", this.__immediateUpdate);
		super.destruct();
	}
};
__decorate$11([watch(":afterInit"), autobind], ToolbarCollection.prototype, "__immediateUpdate", null);
__decorate$11([debounce((ctx) => ctx.j.defaultTimeout, true)], ToolbarCollection.prototype, "update", null);
__decorate$11([hook("ready")], ToolbarCollection.prototype, "__initEvents", null);
ToolbarCollection = __decorate$11([component], ToolbarCollection);
//#endregion
//#region node_modules/jodit/esm/modules/toolbar/collection/editor-collection.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$10 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var ToolbarEditorCollection = class ToolbarEditorCollection extends ToolbarCollection {
	/** @override */
	className() {
		return "ToolbarEditorCollection";
	}
	/** @override */
	shouldBeDisabled(button) {
		const disabled = super.shouldBeDisabled(button);
		if (disabled !== void 0) return disabled;
		const mode = button.control.mode === void 0 ? 1 : button.control.mode;
		return !(mode === 3 || mode === this.j.getRealMode());
	}
	/** @override */
	shouldBeActive(button) {
		const active = super.shouldBeActive(button);
		if (active !== void 0) return active;
		const element = this.j.selection ? this.j.s.current() : null;
		if (!element) return false;
		let elm;
		if (button.control.tags) {
			const tags = button.control.tags;
			elm = element;
			if (Dom.up(elm, (node) => {
				if (node && tags.indexOf(node.nodeName.toLowerCase()) !== -1) return true;
			}, this.j.editor)) return true;
		}
		if (button.control.css) {
			const css = button.control.css;
			elm = element;
			if (Dom.up(elm, (node) => {
				if (node && !Dom.isText(node) && !Dom.isComment(node)) return this.checkActiveStatus(css, node);
			}, this.j.editor)) return true;
		}
		return false;
	}
	/** @override */
	getTarget(button) {
		return button.target || this.j.s.current() || null;
	}
	/** @override */
	constructor(jodit) {
		super(jodit);
		this.checkActiveStatus = (cssObject, node) => {
			let matches = 0, total = 0;
			Object.keys(cssObject).forEach((cssProperty) => {
				const cssValue = cssObject[cssProperty];
				if (isFunction(cssValue)) {
					if (cssValue(this.j, css(node, cssProperty).toString())) matches += 1;
				} else if (cssValue.indexOf(css(node, cssProperty).toString()) !== -1) matches += 1;
				total += 1;
			});
			return total === matches;
		};
		this.prependInvisibleInput(this.container);
	}
	/**
	* Adds an invisible element to the container that can handle the
	* situation when the editor is inside the <label>
	*
	* @see https://github.com/jodit/jodit-react/issues/138
	*/
	prependInvisibleInput(container) {
		const input = this.j.create.element("input", {
			name: "jodit-toolbar-focus-helper_" + this.j.id,
			tabIndex: -1,
			disabled: true,
			style: "width: 0; height:0; position: absolute; visibility: hidden;"
		});
		Dom.appendChildFirst(container, input);
	}
	/**
	* Show the inline toolbar inside WYSIWYG editor.
	* @param bound - you can set the place for displaying the toolbar,
	* or the place will be in the place of the cursor
	*/
	showInline(bound) {
		this.jodit.e.fire("showInlineToolbar", bound);
	}
	hide() {
		this.jodit.e.fire("hidePopup");
		super.hide();
		this.jodit.e.fire("toggleToolbar");
	}
	show() {
		super.show();
		this.jodit.e.fire("toggleToolbar");
	}
};
ToolbarEditorCollection = __decorate$10([component], ToolbarEditorCollection);
//#endregion
//#region node_modules/jodit/esm/modules/uploader/helpers/build-data.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
function buildData(uploader, data) {
	if (isFunction(uploader.o.buildData)) return uploader.o.buildData.call(uploader, data);
	const FD = uploader.ow.FormData;
	if (FD !== void 0) {
		if (data instanceof FD) return data;
		if (isString(data)) return data;
		const newData = new FD();
		const dict = data;
		Object.keys(dict).forEach((key) => {
			newData.append(key, dict[key]);
		});
		return newData;
	}
	return data;
}
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
//#endregion
//#region node_modules/jodit/esm/modules/uploader/helpers/send.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var ajaxInstances = /* @__PURE__ */ new WeakMap();
function send(uploader, data) {
	const requestData = buildData(uploader, data);
	const showProgress = (progress) => {
		uploader.j.progressbar.show().progress(progress);
		if (progress >= 100) uploader.j.progressbar.hide();
	};
	let sendData = (request, showProgress) => {
		const ajax = new Ajax({
			xhr: () => {
				const xhr = new XMLHttpRequest();
				if (uploader.j.ow.FormData !== void 0 && xhr.upload) {
					showProgress(10);
					xhr.upload.addEventListener("progress", (evt) => {
						if (evt.lengthComputable) {
							let percentComplete = evt.loaded / evt.total;
							percentComplete *= 100;
							showProgress(percentComplete);
						}
					}, false);
				} else showProgress(100);
				return xhr;
			},
			method: uploader.o.method || "POST",
			data: request,
			url: isFunction(uploader.o.url) ? uploader.o.url(request) : uploader.o.url,
			headers: uploader.o.headers,
			queryBuild: uploader.o.queryBuild,
			contentType: uploader.o.contentType.call(uploader, request),
			withCredentials: uploader.o.withCredentials || false
		});
		let instances = ajaxInstances.get(uploader);
		if (!instances) {
			instances = /* @__PURE__ */ new Set();
			ajaxInstances.set(uploader, instances);
		}
		instances.add(ajax);
		uploader.j.e.one("beforeDestruct", ajax.destruct);
		return ajax.send().then((resp) => resp.json()).catch((error) => {
			return {
				success: false,
				data: { messages: [error] }
			};
		}).finally(() => {
			ajax.destruct();
			instances === null || instances === void 0 || instances.delete(ajax);
		});
	};
	if (isFunction(uploader.o.customUploadFunction)) sendData = uploader.o.customUploadFunction;
	if (isPromise(requestData)) return requestData.then((data) => sendData(data, showProgress)).catch((error) => {
		uploader.o.error.call(uploader, error);
	});
	return sendData(requestData, showProgress);
}
//#endregion
//#region node_modules/jodit/esm/modules/uploader/helpers/send-files.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Send files to server
*/
function sendFiles(uploader, files, handlerSuccess, handlerError, process) {
	if (!files) return Promise.reject(error("Need files"));
	const { o } = uploader;
	let fileList = toArray(files);
	if (!fileList.length) return Promise.reject(error("Need files"));
	if (isFunction(o.beforeUpload)) {
		if (o.beforeUpload.call(uploader, fileList) === false) {
			const err = error("Upload canceled");
			(handlerError || o.defaultHandlerError).call(uploader, err);
			return Promise.reject(err);
		}
	}
	const promises = [];
	if (o.insertImageAsBase64URI) readImagesWithReader(fileList, o.imagesExtensions, promises, uploader, handlerSuccess, o.defaultHandlerSuccess);
	fileList = fileList.filter((a) => a);
	if (fileList.length) {
		const form = new FormData();
		form.append(o.pathVariableName, uploader.path);
		form.append("source", uploader.source);
		let file;
		for (let i = 0; i < fileList.length; i += 1) {
			file = fileList[i];
			if (file) {
				const hasRealExtension = /\.\w+$/.test(file.name);
				const mime = file.type.match(/\/([a-z0-9]+)/i);
				const extension = mime && mime[1] ? mime[1].toLowerCase() : "";
				let newName = fileList[i].name || Math.random().toString().replace(".", "");
				if (!hasRealExtension && extension) {
					let extForReg = extension;
					if (["jpeg", "jpg"].includes(extForReg)) extForReg = "jpeg|jpg";
					if (!new RegExp(".(" + extForReg + ")$", "i").test(newName)) newName += "." + extension;
				}
				const [key, iFile, name] = o.processFileName.call(uploader, o.filesVariableName(i), fileList[i], newName);
				form.append(key, iFile, name);
			}
		}
		if (process) process(form);
		if (o.data && isPlainObject(o.data)) Object.keys(o.data).forEach((key) => {
			form.append(key, o.data[key]);
		});
		o.prepareData.call(uploader, form);
		promises.push(send(uploader, form).then((resp) => {
			if (o.isSuccess.call(uploader, resp)) {
				(isFunction(handlerSuccess) ? handlerSuccess : o.defaultHandlerSuccess).call(uploader, o.process.call(uploader, resp));
				return resp;
			}
			(isFunction(handlerError) ? handlerError : o.defaultHandlerError).call(uploader, error(o.getMessage.call(uploader, resp)));
			return resp;
		}).then(() => {
			uploader.j.events && uploader.j.e.fire("filesWereUploaded");
		}));
	}
	return Promise.all(promises);
}
function readImagesWithReader(fileList, imagesExtensions, promises, uploader, handlerSuccess, defaultHandlerSuccess) {
	const readerPromises = [];
	let file, i;
	for (i = 0; i < fileList.length; i += 1) {
		file = fileList[i];
		if (file && file.type) {
			const mime = file.type.match(/\/([a-z0-9]+)/i);
			const extension = mime[1] ? mime[1].toLowerCase() : "";
			if (!imagesExtensions.includes(extension)) continue;
			const reader = new FileReader();
			const readerPromise = uploader.j.async.promise((resolve, reject) => {
				reader.onerror = reject;
				reader.onloadend = () => {
					const resp = {
						baseurl: "",
						files: [reader.result],
						isImages: [true]
					};
					(isFunction(handlerSuccess) ? handlerSuccess : defaultHandlerSuccess).call(uploader, resp);
					resolve(resp);
				};
				reader.readAsDataURL(file);
			});
			readerPromises.push(readerPromise);
			promises.push(readerPromise);
			fileList[i] = null;
		}
	}
	if (readerPromises.length) Promise.all(readerPromises).then(() => {
		uploader.j.events && uploader.j.e.fire("filesWereUploaded");
	}).catch(() => {});
}
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
//#endregion
//#region node_modules/jodit/esm/modules/uploader/helpers/index.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
function hasFiles(data) {
	return Boolean(data && data.files && data.files.length > 0);
}
function hasItems(data) {
	return Boolean(data && data.items && data.items.length > 0);
}
//#endregion
//#region node_modules/jodit/esm/modules/uploader/config.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Module for processing download documents and images by Drag and Drop
* Drag and Drop files
*/
Config.prototype.enableDragAndDropFileToEditor = true;
Config.prototype.uploader = {
	url: "",
	insertImageAsBase64URI: false,
	showTabInFileSelector: true,
	imagesExtensions: [
		"jpg",
		"jpeg",
		"png",
		"gif",
		"webp",
		"bmp",
		"svg",
		"tiff",
		"avif"
	],
	headers: null,
	data: null,
	filesVariableName(i) {
		return `files[${i}]`;
	},
	withCredentials: false,
	pathVariableName: "path",
	format: "json",
	method: "POST",
	prepareData(formData) {
		return formData;
	},
	isSuccess(resp) {
		return resp.success;
	},
	getMessage(resp) {
		return resp.data.messages !== void 0 && isArray(resp.data.messages) ? resp.data.messages.join(" ") : "";
	},
	/**
	* @see [[IUploader.processFileName]]
	*/
	processFileName(key, file, name) {
		return [
			key,
			file,
			name
		];
	},
	process(resp) {
		return resp.data;
	},
	error(e) {
		this.j.message.error(e.message, 4e3);
	},
	getDisplayName(baseurl, filename) {
		return baseurl + filename;
	},
	defaultHandlerSuccess(resp) {
		const j = this.j || this;
		if (!isJoditObject(j)) return;
		if (resp.files && resp.files.length) resp.files.forEach((filename, index) => {
			const [tagName, attrName] = resp.isImages && resp.isImages[index] ? ["img", "src"] : ["a", "href"];
			const elm = j.createInside.element(tagName);
			attr(elm, attrName, resp.baseurl + filename);
			if (tagName === "a") elm.textContent = j.o.uploader.getDisplayName.call(this, resp.baseurl, filename);
			if (tagName === "img") j.s.insertImage(elm, null, j.o.imageDefaultWidth);
			else j.s.insertNode(elm);
		});
	},
	defaultHandlerError(e) {
		this.j.message.error(e.message);
	},
	contentType(requestData) {
		return this.ow.FormData !== void 0 && typeof requestData !== "string" ? false : "application/x-www-form-urlencoded; charset=UTF-8";
	}
};
//#endregion
//#region node_modules/jodit/esm/modules/uploader/uploader.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var Uploader = class extends ViewComponent {
	get j() {
		return this.jodit;
	}
	/** @override */
	className() {
		return "Uploader";
	}
	get o() {
		return this.options;
	}
	/**
	* It sets the path for uploading files
	*/
	setPath(path) {
		this.path = path;
		return this;
	}
	/**
	* It sets the source for connector
	*/
	setSource(source) {
		this.source = source;
		return this;
	}
	/**
	* Set the handlers Drag and Drop to `$form`
	*
	* @param form - Form or any Node on which you can drag and drop the file. In addition will be processed
	* <code>&lt;input type="file" &gt;</code>
	* @param handlerSuccess - The function be called when a successful uploading files
	* to the server
	* @param handlerError - The function that will be called during a failed download files a server
	* @example
	* ```javascript
	* var $form = jQuery('<form><input type="text" typpe="file"></form>');
	* jQuery('body').append($form);
	* Jodit.editors.someidfoeditor.uploader.bind($form[0], function (files) {
	*     var i;
	*     for (i = 0; i < data.files.length; i += 1) {
	*         parent.s.insertImage(data.files[i])
	*     }
	* });
	* ```
	*/
	bind(form, handlerSuccess, handlerError) {
		const onFinally = () => {
			form.classList.remove("jodit_drag_hover");
		};
		const self = this, onPaste = (e) => {
			let i, file, extension;
			const cData = e.clipboardData;
			const processData = (formdata) => {
				if (file) {
					formdata.append("extension", extension);
					formdata.append("mimetype", file.type);
				}
			};
			if (!IS_IE && hasFiles(cData)) {
				sendFiles(self, cData.files, handlerSuccess, handlerError).finally(onFinally);
				return false;
			}
			if (hasItems(cData)) {
				const { items } = cData;
				for (i = 0; i < items.length; i += 1) if (items[i].kind === "file" && items[i].type === "image/png") {
					file = items[i].getAsFile();
					if (file) {
						const mime = file.type.match(/\/([a-z0-9]+)/i);
						extension = mime[1] ? mime[1].toLowerCase() : "";
						sendFiles(self, [file], handlerSuccess, handlerError, processData).finally(onFinally);
					}
					e.preventDefault();
					break;
				}
			}
		};
		if (self.j && self.j.editor !== form) self.j.e.on(form, "paste", onPaste);
		else self.j.e.on("beforePaste", onPaste);
		this.attachEvents(form, handlerSuccess, handlerError, onFinally);
	}
	attachEvents(form, handlerSuccess, handlerError, onFinally) {
		const self = this;
		self.j.e.on(form, "dragend dragover dragenter dragleave drop", (e) => {
			e.preventDefault();
		}).on(form, "dragover", (event) => {
			if (hasFiles(event.dataTransfer) || hasItems(event.dataTransfer)) {
				form.classList.add("jodit_drag_hover");
				event.preventDefault();
			}
		}).on(form, "dragend dragleave", (event) => {
			form.classList.remove("jodit_drag_hover");
			if (hasFiles(event.dataTransfer)) event.preventDefault();
		}).on(form, "drop", (event) => {
			form.classList.remove("jodit_drag_hover");
			if (hasFiles(event.dataTransfer)) {
				event.preventDefault();
				event.stopImmediatePropagation();
				sendFiles(self, event.dataTransfer.files, handlerSuccess, handlerError).finally(onFinally);
			}
		});
		const inputFile = Dom.first(form, (node) => {
			var _a;
			return Dom.isTag(node, "input") && ((_a = attr(node, "type")) === null || _a === void 0 ? void 0 : _a.toLowerCase()) === "file";
		});
		if (inputFile) self.j.e.on(inputFile, "change", () => {
			sendFiles(self, inputFile.files, handlerSuccess, handlerError).then(() => {
				inputFile.value = "";
				if (!/safari/i.test(navigator.userAgent)) {
					inputFile.type = "";
					inputFile.type = "file";
				}
			}).finally(onFinally);
		});
	}
	/**
	* Upload images to a server by its URL, making it through the connector server.
	*/
	uploadRemoteImage(url, handlerSuccess, handlerError) {
		const uploader = this, { o } = uploader;
		const handlerE = isFunction(handlerError) ? handlerError : o.defaultHandlerError;
		send(uploader, {
			action: "fileUploadRemote",
			url
		}).then((resp) => {
			if (o.isSuccess.call(uploader, resp)) {
				(isFunction(handlerSuccess) ? handlerSuccess : o.defaultHandlerSuccess).call(uploader, o.process.call(uploader, resp));
				return;
			}
			handlerE.call(uploader, error(o.getMessage.call(uploader, resp)));
		}).catch((e) => handlerE.call(uploader, e));
	}
	upload(files) {
		return this.async.promise((resolve, reject) => {
			sendFiles(this, files, resolve, reject);
		});
	}
	constructor(editor, options) {
		super(editor);
		this.path = "";
		this.source = "default";
		this.options = ConfigProto(options || {}, ConfigProto(Config.defaultOptions.uploader, isJoditObject(editor) ? editor.o.uploader : {}));
	}
	destruct() {
		this.setStatus(STATUSES.beforeDestruct);
		const instances = ajaxInstances.get(this);
		if (instances) {
			instances.forEach((ajax) => {
				try {
					ajax.destruct();
				} catch (_a) {}
			});
			instances.clear();
		}
		super.destruct();
	}
};
//#endregion
//#region node_modules/jodit/esm/core/selection/helpers/move-the-node-along-the-edge-outward.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Moves the fake node up until it encounters a non-empty sibling on the left(right)
* @private
*/
function moveTheNodeAlongTheEdgeOutward(node, start, root) {
	let item = node;
	while (item && item !== root) {
		if (Dom.findSibling(item, start)) return;
		if (Dom.isBlock(item.parentElement)) break;
		item = item.parentElement;
		if (item && item !== root) start ? Dom.before(item, node) : Dom.after(item, node);
	}
}
//#endregion
//#region node_modules/jodit/esm/core/selection/style/constants.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module selection
*/
/** @internal */
var WRAP = "wrap";
/** @internal */
var UNWRAP = "unwrap";
/** @internal */
var CHANGE = "change";
/** @internal */
var UNSET = "unset";
/** @internal */
var INITIAL = "initial";
/** @internal */
var REPLACE = "replace";
/** @internal */
var _PREFIX = "commitStyle";
//#endregion
//#region node_modules/jodit/esm/core/selection/style/api/extract.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* If the selection area is inside an element that matches the commit (suitable relative),
* but does not completely fill it.
* Then the method cuts the parent and leaves itself in a copy of the parent (suitable relative) in the middle.
*
* @example
* Apply strong to
* ```html
* 	<strong><span>some<font>SELECTED</font>text</span></strong>
* ```
* Should extract selection from parent `strong`
* ```html
* `<strong><span>some</span></strong><strong><span><font>SELECTED</font></span></strong><strong><span>test</span></strong>
* ```
* @private
*/
function extractSelectedPart(wrapper, font, jodit) {
	const range = jodit.s.createRange();
	const leftEdge = isMarker(font.previousSibling) ? font.previousSibling : font;
	range.setStartBefore(wrapper);
	range.setEndBefore(leftEdge);
	extractAndMove(wrapper, range, true);
	const rightEdge = isMarker(font.nextSibling) ? font.nextSibling : font;
	range.setStartAfter(rightEdge);
	range.setEndAfter(wrapper);
	extractAndMove(wrapper, range, false);
}
/**
* Retrieves content before after the selected area, clears it if it is empty, and inserts before after the framed selection
* @private
*/
function extractAndMove(wrapper, range, left) {
	const fragment = range.extractContents();
	if ((!fragment.textContent || !trim(fragment.textContent).length) && fragment.firstChild) Dom.unwrap(fragment.firstChild);
	if (wrapper.parentNode) call(left ? Dom.before : Dom.after, wrapper, fragment);
}
//#endregion
//#region node_modules/jodit/esm/core/selection/style/api/finite-state-machine.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* A state machine implementation for applying styles.
*/
var FiniteStateMachine = class {
	setState(state) {
		assert(!this.__previewsStates.has(state), "Circled states");
		this.__previewsStates.add(state);
		this.__state = state;
	}
	getState() {
		return this.__state;
	}
	disableSilent() {
		this.silent = false;
	}
	constructor(state, transitions) {
		this.transitions = transitions;
		this.silent = true;
		this.__previewsStates = /* @__PURE__ */ new Set();
		this.setState(state);
	}
	dispatch(actionName, value) {
		const action = this.transitions[this.getState()][actionName];
		if (action) {
			const res = action.call(this, value);
			assert(res && res !== value, "Action should return new value");
			assert(isString(res.next), "Value should contain the next state");
			assert(res.next !== this.getState(), "The new state should not be equal to the old one.");
			this.setState(res.next);
			return res;
		}
		throw new Error(`invalid action: ${this.getState()}.${actionName.toString()}`);
	}
};
//#endregion
//#region node_modules/jodit/esm/core/selection/style/api/is-normal-node.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Is normal usual element
* @private
*/
function isNormalNode(elm) {
	return Boolean(elm && !Dom.isEmptyTextNode(elm) && !Dom.isTemporary(elm) && !isMarker(elm));
}
//#endregion
//#region node_modules/jodit/esm/core/selection/style/api/has-same-style.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Element has the same styles as in the commit
* @private
*/
function hasSameStyle(elm, rules) {
	return Boolean(!Dom.isTag(elm, "font") && Dom.isHTMLElement(elm) && Object.keys(rules).every((property) => {
		const value = css(elm, property, true);
		if (value === "" && (rules[property] === "" || rules[property] == null)) return true;
		return !isVoid(value) && value !== "" && !isVoid(rules[property]) && normalizeCssValue(property, rules[property]).toString().toLowerCase() === value.toString().toLowerCase();
	}));
}
if (globalDocument) {
	const elm = globalDocument.createElement("div");
	elm.style.color = "red";
	assert(hasSameStyle(elm, { color: "red" }), "Style test");
	assert(hasSameStyle(elm, { fontSize: null }), "Style test");
	assert(hasSameStyle(elm, { fontSize: "" }), "Style test");
}
/**
* Element has the similar styles keys
*/
function hasSameStyleKeys(elm, rules) {
	return Boolean(!Dom.isTag(elm, "font") && Dom.isHTMLElement(elm) && Object.keys(rules).every((property) => {
		return css(elm, property, true) !== "";
	}));
}
if (globalDocument) {
	const elm2 = globalDocument.createElement("div");
	elm2.style.color = "red";
	assert(hasSameStyleKeys(elm2, { color: "red" }), "Style test");
	assert(!hasSameStyleKeys(elm2, {
		font: "Arial",
		color: "red"
	}), "Style test");
	assert(!hasSameStyleKeys(elm2, { border: "1px solid #ccc" }), "Style test");
}
//#endregion
//#region node_modules/jodit/esm/core/selection/style/api/is-suit-element.js
/**
* Checks if an item is suitable for applying a commit. The element suits us if it
*  - has the same styles as in the commit (commitStyle.options.style)
*  - has the same tag as in the commit (commitStyle.options.element)
*
* @param commitStyle - style commit
* @param elm - checked item
* @param strict - strict mode - false - the default tag is suitable for us if it is also in the commit
* @param strictStyle - strict style mode - true - the element must have the same style keys AND values
* as in the commit, false - matching style keys are enough
* @private
*/
function isSuitElement(commitStyle, elm, strict, strictStyle = true) {
	var _a;
	if (!elm || !isNormalNode(elm)) return false;
	const { element, elementIsDefault, options } = commitStyle;
	if (Dom.isList(elm) && commitStyle.elementIsList) return true;
	const elmIsSame = Dom.isTag(elm, element);
	if (elmIsSame && !(elementIsDefault && strict)) return true;
	if (Boolean(((_a = options.attributes) === null || _a === void 0 ? void 0 : _a.style) && (strictStyle ? hasSameStyle(elm, options.attributes.style) : hasSameStyleKeys(elm, options.attributes.style))) && !commitStyle.elementIsList) return true;
	return !elmIsSame && !strict && elementIsDefault && Dom.isInlineBlock(elm);
}
/**
* @private
*/
function suitableClosest(commitStyle, element, root) {
	return Dom.closest(element, (node) => isSuitElement(commitStyle, node, true, false), root);
}
/**
* Inside the parent element there is a block with the same styles
* @example
* For selection:
* ```html
* <p>|test<strong>test</strong>|</p>
* ```
* Apply `{element:'strong'}`
* @private
*/
function isSameStyleChild(commitStyle, elm) {
	var _a, _b;
	const { element, options } = commitStyle;
	if (!elm || !isNormalNode(elm)) return false;
	const elmIsSame = elm.nodeName.toLowerCase() === element;
	const elmHasSameStyle = Boolean(((_a = options.attributes) === null || _a === void 0 ? void 0 : _a.style) && hasSameStyleKeys(elm, (_b = options.attributes) === null || _b === void 0 ? void 0 : _b.style));
	return elmIsSame && elmHasSameStyle;
}
//#endregion
//#region node_modules/jodit/esm/core/selection/style/api/get-suit-child.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Checks if child elements are suitable for applying styles.
* An element is suitable for us only if it is the only significant child.
* If the child matches then returns it.
* @example
* `<font><strong>selected</strong></font>`
* @private
*/
function getSuitChild(style, font) {
	let { firstChild: child } = font;
	while (child && !isNormalNode(child)) {
		child = child.nextSibling;
		if (!child) return null;
	}
	if (child && !Dom.next(child, isNormalNode, font) && isSuitElement(style, child, false)) return child;
	return null;
}
//#endregion
//#region node_modules/jodit/esm/core/selection/style/api/get-suit-parent.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Checks if the parent of an element is suitable for applying styles, if applicable, then returns the parent *
*
* @param style - styles to be applied
* @param node - checked item
* @param root - editor root
* @private
*/
function getSuitParent(style, node, root) {
	var _a;
	const { parentNode } = node;
	if (parentNode === root || !Dom.isHTMLElement(parentNode) || Dom.next(node, isNormalNode, parentNode) || Dom.prev(node, isNormalNode, parentNode)) return null;
	if (style.isElementCommit && style.elementIsBlock && !Dom.isBlock(parentNode)) return getSuitParent(style, parentNode, root);
	if (Dom.isTag(parentNode, "li") && !style.isElementCommit && ((_a = style.options.attributes) === null || _a === void 0 ? void 0 : _a.style)) return parentNode;
	if (isSuitElement(style, parentNode, false) && (!Dom.isBlock(parentNode) || style.elementIsBlock)) return parentNode;
	if (style.isElementCommit && !Dom.isBlock(parentNode)) return getSuitParent(style, parentNode, root);
	return null;
}
//#endregion
//#region node_modules/jodit/esm/core/selection/style/api/is-inside-invisible-element.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Check if FONT inside STYLE or SCRIPT element
* @private
*/
function isInsideInvisibleElement(font, root) {
	return Boolean(Dom.closest(font, ["style", "script"], root));
}
//#endregion
//#region node_modules/jodit/esm/core/selection/style/api/is-same-attributes.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Checks that every attribute from `attrs` is present on the element with
* the same value (one-directional match: extra own attributes of the element
* are ignored, empty `attrs` matches any element)
* @private
*/
function isSameAttributes(elm, attrs) {
	if (!size$1(attrs)) return true;
	assert(attrs, "Attrs must be a non-empty object");
	return Object.keys(attrs).every((key) => {
		if (key === "class" || key === "className") return elm.classList.contains(attrs[key]);
		if (key === "style") return hasSameStyle(elm, attrs[key]);
		return attr(elm, key) === attrs[key];
	});
}
function elementsEqualAttributes(elm1, elm2) {
	return elm1.attributes.length === elm2.attributes.length && Array.from(elm1.attributes).every((attr) => attrRaw(elm2, attr.name) === attr.value);
}
//#endregion
//#region node_modules/jodit/esm/core/selection/style/api/toggle-attributes.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var tak = "toggleAttributes";
/**
* Toggles attributes
* @private
*/
function toggleAttributes(commitStyle, elm, jodit, mode, dry = false) {
	if (!dry && commitStyle.isApplied(elm, tak)) return mode;
	!dry && commitStyle.setApplied(elm, tak);
	const { attributes } = commitStyle.options;
	if (attributes && size$1(attributes) > 0) Object.keys(attributes).forEach((key) => {
		const value = attributes[key];
		switch (key) {
			case "style":
				mode = toggleStyle(commitStyle, jodit, value, elm, dry, mode);
				break;
			case "className":
			case "class":
				mode = toggleClass(jodit, value, elm, mode, dry);
				break;
			default: mode = toggleAttribute(jodit, value, elm, key, dry, mode);
		}
	});
	return mode;
}
function toggleStyle(commitStyle, jodit, style, elm, dry, mode) {
	assert(isPlainObject(style) && size$1(style), "Style must be an object");
	Object.keys(style).forEach((rule) => {
		const inlineValue = cssInline(elm, rule);
		const newValue = style[rule];
		if (inlineValue === "" && newValue == null) return;
		if (getNativeCSSValue(jodit, elm, rule) === normalizeCssValue(rule, newValue)) {
			if (!inlineValue) return;
			!dry && css(elm, rule, null);
			mode = UNSET;
			mode = removeExtraStyleAttribute(commitStyle, elm, mode);
			return;
		}
		mode = CHANGE;
		if (!dry) {
			css(elm, rule, newValue);
			mode = removeExtraStyleAttribute(commitStyle, elm, mode);
		}
	});
	return mode;
}
function toggleClass(jodit, value, elm, mode, dry) {
	assert(isString(value), "Class name must be a string");
	const hook = jodit.e.fire.bind(jodit.e, `${_PREFIX}AfterToggleAttribute`);
	if (elm.classList.contains(value.toString())) {
		mode = UNSET;
		if (!dry) {
			elm.classList.remove(value);
			if (elm.classList.length === 0) {
				attr(elm, "class", null);
				hook(mode, elm, "class", null);
			}
		}
	} else {
		mode = CHANGE;
		if (!dry) {
			elm.classList.add(value);
			hook(mode, elm, "class", value);
		}
	}
	return mode;
}
function toggleAttribute(jodit, value, elm, key, dry, mode) {
	assert(isString(value) || isNumber(value) || isBoolean(value) || value == null, "Attribute value must be a string or number or boolean or null");
	const hook = jodit.e.fire.bind(jodit.e, `${_PREFIX}AfterToggleAttribute`);
	if (attr(elm, key) === (value == null ? value : String(value))) {
		!dry && attr(elm, key, null);
		mode = UNSET;
		!dry && hook(mode, elm, key, value);
		return mode;
	}
	mode = CHANGE;
	if (!dry) {
		attr(elm, key, value);
		hook(mode, elm, key, value);
	}
	return mode;
}
/**
* If the element has an empty style attribute, it removes the attribute,
* and if it is default, it removes the element itself
*/
function removeExtraStyleAttribute(commitStyle, elm, mode) {
	if (!attr(elm, "style")) {
		attr(elm, "style", null);
		if (elm.tagName.toLowerCase() === commitStyle.defaultTag) {
			Dom.unwrap(elm);
			mode = UNWRAP;
		}
	}
	return mode;
}
/**
* Creates an iframe into which elements will be inserted to test their default styles in the browser
*/
function getShadowRoot(jodit) {
	var _a;
	if (dataBind(jodit, "shadowRoot") !== void 0) return dataBind(jodit, "shadowRoot");
	const container = getContainer(jodit);
	const iframe = globalDocument.createElement("iframe");
	css(iframe, {
		width: 0,
		height: 0,
		position: "absolute",
		border: 0
	});
	attr(iframe, "src", "about:blank");
	Dom.append(container, iframe);
	const doc = (_a = iframe.contentWindow) === null || _a === void 0 ? void 0 : _a.document;
	const shadowRoot = !doc ? jodit.od.body : doc.body;
	dataBind(jodit, "shadowRoot", shadowRoot);
	return shadowRoot;
}
/**
* `strong -> fontWeight 700`
*/
function getNativeCSSValue(jodit, elm, key) {
	const root = getShadowRoot(jodit);
	const wrapper = jodit.create.element("div");
	css(wrapper, "color", css(jodit.editor, "color"));
	const newElm = jodit.create.element(elm.tagName.toLowerCase());
	attr(newElm, "style", attr(elm, "style"));
	Dom.append(wrapper, newElm);
	Dom.append(root, wrapper);
	const result = css(newElm, key);
	Dom.safeRemove(wrapper);
	return result;
}
//#endregion
//#region node_modules/jodit/esm/core/selection/style/api/list/wrap-list.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Replaces non-leaf items with leaf items and either creates a new list or
* adds a new item to the nearest old list
* @private
*/
function wrapList(commitStyle, wrapper, jodit) {
	const result = jodit.e.fire(`${_PREFIX}BeforeWrapList`, REPLACE, wrapper, commitStyle);
	const newWrapper = result !== null && result !== void 0 ? result : Dom.replace(wrapper, "li", jodit.createInside);
	const prev = newWrapper.previousElementSibling;
	const next = newWrapper.nextElementSibling;
	let list = Dom.isTag(prev, commitStyle.element) ? prev : null;
	list !== null && list !== void 0 || (list = Dom.isTag(next, commitStyle.element) ? next : null);
	if (!Dom.isList(list) || !isSameAttributes(list, commitStyle.options.attributes)) {
		list = jodit.createInside.element(commitStyle.element);
		toggleAttributes(commitStyle, list, jodit, INITIAL);
		Dom.before(newWrapper, list);
	}
	if (prev === list) Dom.append(list, newWrapper);
	else Dom.prepend(list, newWrapper);
	if (Dom.isTag(list.nextElementSibling, commitStyle.element) && elementsEqualAttributes(list, list.nextElementSibling)) {
		Dom.append(list, Array.from(list.nextElementSibling.childNodes));
		Dom.safeRemove(list.nextElementSibling);
	}
	jodit.e.fire(`${_PREFIX}AfterWrapList`, WRAP, list, commitStyle);
	return list;
}
//#endregion
//#region node_modules/jodit/esm/core/selection/style/api/list/toggle-ordered-list.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Replaces `ul->ol` or `ol->ul`, apply styles to the list, or remove a list item from it
* @private
*/
function toggleOrderedList(commitStyle, li, jodit, mode) {
	if (!li) return mode;
	const list = li.parentElement;
	if (!list) return mode;
	const result = jodit.e.fire(`${_PREFIX}BeforeToggleList`, mode, commitStyle, list);
	if (result !== void 0) return result;
	const hook = jodit.e.fire.bind(jodit.e, `${_PREFIX}AfterToggleList`);
	if (mode !== "unwrap") {
		const isChangeMode = toggleAttributes(commitStyle, li.parentElement, jodit, INITIAL, true) === CHANGE;
		if (mode === "replace" || isChangeMode || list.tagName.toLowerCase() !== commitStyle.element) {
			hook(REPLACE, wrapList(commitStyle, unwrapList(REPLACE, list, li, jodit, commitStyle), jodit), commitStyle);
			return REPLACE;
		}
	}
	hook(UNWRAP, unwrapList(UNWRAP, list, li, jodit, commitStyle), commitStyle);
	return UNWRAP;
}
function unwrapList(mode, list, li, jodit, cs) {
	const result = jodit.e.fire(`${_PREFIX}BeforeUnwrapList`, mode, list, cs);
	if (result) {
		assert(Dom.isHTMLElement(result), `${_PREFIX}BeforeUnwrapList hook must return HTMLElement`);
		return result;
	}
	extractSelectedPart(list, li, jodit);
	assert(Dom.isHTMLElement(li.parentElement), "Element should be inside the list");
	Dom.unwrap(li.parentElement);
	if (mode === "replace") return li;
	return Dom.replace(li, jodit.o.enter.toLowerCase() !== "br" ? jodit.o.enter : jodit.createInside.fragment(), jodit.createInside);
}
//#endregion
//#region node_modules/jodit/esm/core/selection/style/api/unwrap-children.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Unwrap all suit elements inside
* @private
*/
function unwrapChildren(style, font) {
	var _a;
	const needUnwrap = [];
	const needChangeStyle = [];
	let firstElementSuit;
	const cssStyle = (_a = style.options.attributes) === null || _a === void 0 ? void 0 : _a.style;
	if (font.firstChild) {
		const gen = Dom.eachGen(font);
		let item = gen.next();
		while (!item.done) {
			const elm = item.value;
			if (isSuitElement(style, elm, true) && (!cssStyle || hasSameStyleKeys(elm, cssStyle))) {
				if (firstElementSuit === void 0) firstElementSuit = true;
				needUnwrap.push(elm);
			} else if (cssStyle && isSameStyleChild(style, elm)) {
				if (firstElementSuit === void 0) firstElementSuit = false;
				needChangeStyle.push(() => {
					css(elm, Object.keys(cssStyle).reduce((acc, key) => {
						acc[key] = null;
						return acc;
					}, {}));
					if (!attr(elm, "style")) attr(elm, "style", null);
					if (!attr(elm, "style") && elm.nodeName.toLowerCase() === style.element) needUnwrap.push(elm);
				});
			} else if (!Dom.isEmptyTextNode(elm)) {
				if (firstElementSuit === void 0) firstElementSuit = false;
			}
			item = gen.next();
		}
	}
	needChangeStyle.forEach((clb) => clb());
	needUnwrap.forEach(Dom.unwrap);
	return Boolean(firstElementSuit);
}
//#endregion
//#region node_modules/jodit/esm/core/selection/style/api/wrap-unwrapped-text.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Wrap text or inline elements inside Block element
* @private
*/
function wrapUnwrappedText(style, elm, jodit) {
	const root = jodit.editor, ci = jodit.createInside, edge = (n, key = "previousSibling") => {
		let edgeNode = n, node = n;
		while (node && !isMarker(node)) {
			if (Dom.isTag(node, jodit.o.enter)) break;
			edgeNode = node;
			if (node[key]) node = node[key];
			else node = node.parentNode && !Dom.isBlock(node.parentNode) && node.parentNode !== root ? node.parentNode : null;
			if (Dom.isBlock(node)) break;
		}
		return edgeNode;
	};
	const start = edge(elm), end = edge(elm, "nextSibling");
	const range = jodit.s.createRange();
	range.setStartBefore(start);
	range.setEndAfter(end);
	const fragment = range.extractContents();
	const wrapper = ci.element(style.element);
	Dom.append(wrapper, fragment);
	Dom.safeInsertNode(range, wrapper);
	if (style.elementIsBlock) {
		if (Dom.isEmpty(wrapper) && !Dom.isTag(wrapper.firstElementChild, "br")) Dom.append(wrapper, ci.element("br"));
	}
	return wrapper;
}
//#endregion
//#region node_modules/jodit/esm/core/selection/style/api/wrap.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Replaces the parent tag with the applicable one, or wraps the text and also replaces the tag
* @private
*/
function wrap(commitStyle, font, jodit) {
	const wrapper = findOrCreateWrapper(commitStyle, font, jodit);
	if (commitStyle.elementIsList) return wrapList(commitStyle, wrapper, jodit);
	const newWrapper = Dom.replace(wrapper, commitStyle.element, jodit.createInside, true);
	if (commitStyle.elementIsBlock) {
		css(newWrapper, "fontSize", null);
		css(newWrapper, "fontWeight", null);
		if (!attr(newWrapper, "style")) attr(newWrapper, "style", null);
	}
	return newWrapper;
}
var WRAP_NODES = /* @__PURE__ */ new Set([
	"td",
	"th",
	"tr",
	"tbody",
	"table",
	"li",
	"ul",
	"ol"
]);
/**
* If we apply a block element, then it finds the closest block parent (exclude table cell etc.),
* otherwise it wraps free text in an element.
*/
function findOrCreateWrapper(commitStyle, font, jodit) {
	if (commitStyle.elementIsBlock) {
		const box = Dom.up(font, (node) => Dom.isBlock(node) && !Dom.isTag(node, WRAP_NODES) && !hasBlockChildren(node), jodit.editor);
		if (box) return box;
		return wrapUnwrappedText(commitStyle, font, jodit);
	}
	attr(font, "size", null);
	return font;
}
/**
* A block that itself contains block-level children is a layout container:
* replacing it wholesale would merge all its blocks into the new element,
* so the selection must be wrapped in a new block inside it instead.
*/
function hasBlockChildren(node) {
	return Array.from(node.childNodes).some((child) => Dom.isBlock(child));
}
//#endregion
//#region node_modules/jodit/esm/core/selection/style/api/index.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
//#endregion
//#region node_modules/jodit/esm/core/selection/style/transactions.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var states = {
	START: "START",
	ELEMENT: "ELEMENT",
	UNWRAP: "UNWRAP",
	UNWRAP_CHILDREN: "UNWRAP_CHILDREN",
	CHANGE: "CHANGE",
	REPLACE_DEFAULT: "REPLACE_DEFAULT",
	LIST: "LIST",
	TOGGLE_LIST: "TOGGLE_LIST",
	WRAP: "WRAP",
	EXTRACT: "EXTRACT",
	END: "END"
};
var transactions = {
	[states.START]: { exec(value) {
		const { element, jodit, style, mode, collapsed } = value;
		if (isInsideInvisibleElement(element, jodit.editor) || !collapsed && Dom.isEmptyContent(element)) return {
			...value,
			next: states.END
		};
		const elm = getSuitParent(style, element, jodit.editor) || getSuitChild(style, element);
		if (elm) return {
			...value,
			next: states.ELEMENT,
			element: elm
		};
		const suit = suitableClosest(style, element, jodit.editor);
		if (style.elementIsList && Dom.isList(suit)) return {
			...value,
			next: states.LIST
		};
		if (suit) return {
			...value,
			next: states.EXTRACT
		};
		return {
			...value,
			next: mode !== "unwrap" ? states.UNWRAP_CHILDREN : states.END
		};
	} },
	[states.LIST]: { exec(value) {
		const { element, jodit, mode } = value;
		if (mode !== "initial" && mode !== "unwrap" && mode !== "replace") return {
			...value,
			next: states.END
		};
		const li = Dom.closest(element, "li", jodit.editor);
		if (!li) return {
			...value,
			next: states.END
		};
		if (Dom.closest(element, LIST_TAGS, jodit.editor)) return {
			...value,
			element: li,
			next: states.TOGGLE_LIST
		};
		return {
			...value,
			next: states.END
		};
	} },
	[states.TOGGLE_LIST]: { exec(value) {
		return {
			...value,
			mode: toggleOrderedList(value.style, value.element, value.jodit, value.mode),
			next: states.END
		};
	} },
	[states.EXTRACT]: { exec(value) {
		var _a;
		const { element, jodit, style } = value;
		const suit = suitableClosest(style, element, jodit.editor);
		assert(suit, "This place should have an element");
		if (!(!style.elementIsBlock && ((_a = style.options.attributes) === null || _a === void 0 ? void 0 : _a.style) && Dom.isBlock(suit))) {
			if (!style.elementIsBlock) extractSelectedPart(suit, element, jodit);
			return {
				...value,
				element: suit,
				next: states.ELEMENT
			};
		}
		return {
			...value,
			next: states.WRAP
		};
	} },
	[states.UNWRAP_CHILDREN]: { exec(value) {
		const { element, style } = value;
		if (!unwrapChildren(style, element)) return {
			...value,
			next: states.WRAP
		};
		return {
			...value,
			mode: UNWRAP,
			next: states.END
		};
	} },
	[states.WRAP]: { exec(value) {
		const { element, jodit, style } = value;
		const wrapper = wrap(style, element, jodit);
		return {
			...value,
			next: style.elementIsList ? states.END : states.CHANGE,
			mode: WRAP,
			element: wrapper
		};
	} },
	[states.ELEMENT]: { exec(value) {
		const { style, element, jodit } = value;
		if (toggleAttributes(style, element, jodit, "initial", true) !== "initial") return {
			...value,
			next: states.CHANGE
		};
		if (!Dom.isTag(element, style.element)) return {
			...value,
			next: states.END
		};
		return {
			...value,
			next: states.UNWRAP
		};
	} },
	[states.CHANGE]: { exec(value) {
		const { style, element, jodit, mode } = value;
		const newMode = toggleAttributes(style, element, jodit, value.mode);
		if (mode !== "wrap" && newMode === "unset" && !element.attributes.length && Dom.isTag(element, style.element)) return {
			...value,
			next: states.UNWRAP
		};
		return {
			...value,
			mode: newMode,
			next: states.END
		};
	} },
	[states.UNWRAP]: { exec(value) {
		const { element, style, jodit } = value;
		if (element.attributes.length && Dom.isTag(element, style.element)) return {
			...value,
			next: states.REPLACE_DEFAULT
		};
		const parent = element.parentElement;
		if (style.elementIsBlock && !style.elementIsDefault && parent && parent !== jodit.editor && Dom.isBlock(parent) && !Dom.isTag(parent, WRAP_NODES)) return {
			...value,
			next: states.REPLACE_DEFAULT
		};
		Dom.unwrap(element);
		return {
			...value,
			mode: UNWRAP,
			next: states.END
		};
	} },
	[states.REPLACE_DEFAULT]: { exec(value) {
		Dom.replace(value.element, value.style.defaultTag, value.jodit.createInside, true);
		return {
			...value,
			mode: REPLACE,
			next: states.END
		};
	} },
	[states.END]: { exec(value) {
		return value;
	} }
};
//#endregion
//#region node_modules/jodit/esm/core/selection/style/apply-style.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/** @internal */
function ApplyStyle(jodit, cs) {
	var _a;
	const { s: sel, editor } = jodit;
	(_a = editor.firstChild) === null || _a === void 0 || _a.normalize();
	const fakes = sel.fakes();
	const gen = jodit.s.wrapInTagGen(fakes);
	let font = gen.next();
	if (font.done) return;
	let state = {
		collapsed: sel.isCollapsed(),
		mode: INITIAL,
		element: font.value,
		next: states.START,
		jodit,
		style: cs
	};
	while (font && !font.done) {
		const machine = new FiniteStateMachine(states.START, transactions);
		state.element = font.value;
		while (machine.getState() !== states.END) state = machine.dispatch("exec", state);
		font = gen.next();
	}
	sel.restoreFakes(fakes);
}
//#endregion
//#region node_modules/jodit/esm/core/selection/style/commit-style.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var CommitStyle = class {
	isApplied(elm, key) {
		const data = this.__applyMap.get(elm);
		if (!data) return false;
		return Boolean(data[key]);
	}
	setApplied(elm, key) {
		var _a;
		const data = (_a = this.__applyMap.get(elm)) !== null && _a !== void 0 ? _a : {};
		data[key] = true;
		this.__applyMap.set(elm, data);
	}
	get elementIsList() {
		return Boolean(this.options.element && LIST_TAGS.has(this.options.element));
	}
	get element() {
		return this.options.element || this.defaultTag;
	}
	/**
	* New element is a block element
	*/
	get elementIsBlock() {
		return Boolean(this.options.element && IS_BLOCK.test(this.options.element));
	}
	/**
	* The commit applies the tag change
	*/
	get isElementCommit() {
		return Boolean(this.options.element && this.options.element !== this.options.defaultTag);
	}
	get defaultTag() {
		if (this.options.defaultTag) return this.options.defaultTag;
		return this.elementIsBlock ? "p" : "span";
	}
	get elementIsDefault() {
		return this.element === this.defaultTag;
	}
	constructor(options) {
		this.options = options;
		this.__applyMap = /* @__PURE__ */ new WeakMap();
	}
	apply(jodit) {
		const { hooks } = this.options;
		const keys = hooks ? Object.keys(hooks) : [];
		try {
			keys.forEach((key) => {
				jodit.e.on(camelCase(_PREFIX + "_" + key), hooks[key]);
			});
			ApplyStyle(jodit, this);
		} finally {
			keys.forEach((key) => {
				jodit.e.off(camelCase(_PREFIX + "_" + key), hooks[key]);
			});
			this.__applyMap = /* @__PURE__ */ new WeakMap();
		}
		jodit.synchronizeValues();
		jodit.e.fire("afterCommitStyle", this);
	}
};
//#endregion
//#region node_modules/jodit/esm/core/selection/helpers/move-node-inside-start.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Moves the fake node inside the adjacent element if it lies next to it but not inside.
* When the cursor is positioned in its place, it must be inside the element and not outside its border.
* @private
*/
function moveNodeInsideStart(j, node, start) {
	let sibling = Dom.findSibling(node, start), anotherSibling = Dom.findSibling(node, !start);
	while (Dom.isElement(sibling) && !Dom.isTag(sibling, INSEPARABLE_TAGS) && Dom.isContentEditable(sibling, j.editor) && (!anotherSibling || !Dom.closest(node, Dom.isElement, j.editor))) {
		if (start || !sibling.firstChild) Dom.append(sibling, node);
		else Dom.before(sibling.firstChild, node);
		sibling = Dom.sibling(node, start);
		anotherSibling = Dom.sibling(node, !start);
	}
}
//#endregion
//#region node_modules/jodit/esm/core/selection/helpers/index.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module selection
*/
/**
* Despite the name, returns `true` when the cursor is NOT at the checked edge
* of the text node: there is significant (non invisible-space) text between
* the offset and the start (`start = true`) or the end (`end = true`) of the string.
* @private
*/
function cursorInTheEdgeOfString(container, offset, start, end) {
	var _a;
	const text = ((_a = container.nodeValue) === null || _a === void 0 ? void 0 : _a.length) ? container.nodeValue : "";
	if (end && text.replace(INVISIBLE_SPACE_REG_EXP_END(), "").length > offset) return true;
	const inv = INVISIBLE_SPACE_REG_EXP_START().exec(text);
	return start && (inv && inv[0].length < offset || !inv && offset > 0);
}
function findCorrectCurrentNode(node, range, rightMode, isCollapsed, checkChild, child) {
	node = range.startContainer.childNodes[range.startOffset];
	if (!node) {
		node = range.startContainer.childNodes[range.startOffset - 1];
		rightMode = true;
	}
	if (node && isCollapsed && !Dom.isText(node)) {
		if (!rightMode && Dom.isText(node.previousSibling)) node = node.previousSibling;
		else if (checkChild) {
			let current = child(node);
			while (current) {
				if (current && Dom.isText(current)) {
					node = current;
					break;
				}
				current = child(current);
			}
		}
	}
	if (node && !isCollapsed && !Dom.isText(node)) {
		let leftChild = node, rightChild = node;
		do {
			leftChild = leftChild.firstChild;
			rightChild = rightChild.lastChild;
		} while (leftChild && rightChild && !Dom.isText(leftChild));
		if (leftChild === rightChild && leftChild && Dom.isText(leftChild)) node = leftChild;
	}
	return {
		node,
		rightMode
	};
}
//#endregion
//#region node_modules/jodit/esm/core/selection/selection.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$9 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var Selection = class {
	constructor(jodit) {
		this.jodit = jodit;
		jodit.e.on("removeMarkers", () => {
			this.removeMarkers();
		});
	}
	/**
	* Short alias for this.jodit
	*/
	get j() {
		return this.jodit;
	}
	/**
	* Throw Error exception if parameter is not Node
	*/
	errorNode(node) {
		if (!Dom.isNode(node)) throw error("Parameter node must be instance of Node");
	}
	/**
	* Return current work place - for Jodit is Editor
	*/
	get area() {
		return this.j.editor;
	}
	/**
	* Editor Window - it can be different for iframe mode
	*/
	get win() {
		return this.j.ew;
	}
	/**
	* Current jodit editor doc
	*/
	get doc() {
		return this.j.ed;
	}
	/**
	* Return current selection object
	*/
	get sel() {
		if (this.j.o.shadowRoot && isFunction(this.j.o.shadowRoot.getSelection)) return this.j.o.shadowRoot.getSelection();
		return this.win.getSelection();
	}
	/**
	* Return first selected range or create new
	*/
	get range() {
		const sel = this.sel;
		return sel && sel.rangeCount ? sel.getRangeAt(0) : this.createRange();
	}
	/**
	* Checks if the selected text is currently inside the editor
	*/
	get isInsideArea() {
		const { sel } = this;
		const range = (sel === null || sel === void 0 ? void 0 : sel.rangeCount) ? sel.getRangeAt(0) : null;
		return !(!range || !Dom.isOrContains(this.area, range.startContainer));
	}
	/**
	* Return current selection object
	* @param select - Immediately add in selection
	*/
	createRange(select = false) {
		const range = this.doc.createRange();
		if (select) this.selectRange(range);
		return range;
	}
	/**
	* Remove all selected content
	*/
	remove() {
		const sel = this.sel, current = this.current();
		if (sel && current) for (let i = 0; i < sel.rangeCount; i += 1) {
			sel.getRangeAt(i).deleteContents();
			sel.getRangeAt(i).collapse(true);
		}
	}
	/**
	* Clear all selection
	*/
	clear() {
		var _a, _b;
		if ((_a = this.sel) === null || _a === void 0 ? void 0 : _a.rangeCount) (_b = this.sel) === null || _b === void 0 || _b.removeAllRanges();
	}
	/**
	* Remove node element from editor
	*/
	removeNode(node) {
		if (!Dom.isOrContains(this.j.editor, node, true)) throw error("Selection.removeNode can remove only editor's children");
		Dom.safeRemove(node);
		this.j.e.fire("afterRemoveNode", node);
	}
	/**
	* Insert the cursor to any point x, y
	*
	* @param x - Coordinate by horizontal
	* @param y - Coordinate by vertical
	* @returns false - Something went wrong
	*/
	insertCursorAtPoint(x, y) {
		this.removeMarkers();
		try {
			const rng = this.createRange();
			(() => {
				if (this.doc.caretPositionFromPoint) {
					const caret = this.doc.caretPositionFromPoint(x, y);
					if (caret) {
						rng.setStart(caret.offsetNode, caret.offset);
						return;
					}
				}
				if (this.doc.caretRangeFromPoint) {
					const caret = this.doc.caretRangeFromPoint(x, y);
					assert(caret, "Incorrect caretRangeFromPoint behaviour");
					rng.setStart(caret.startContainer, caret.startOffset);
				}
			})();
			rng.collapse(true);
			this.selectRange(rng);
			return true;
		} catch (_a) {}
		return false;
	}
	/**
	* Check if editor has selection markers
	*/
	get hasMarkers() {
		return Boolean(this.markers.length);
	}
	/**
	* Check if editor has selection markers
	*/
	get markers() {
		return $$("span[data-" + MARKER_CLASS + "]", this.area);
	}
	/**
	* Remove all markers
	*/
	removeMarkers() {
		Dom.safeRemove(...this.markers);
	}
	/**
	* Create marker element
	*/
	marker(atStart = false, range) {
		let newRange = null;
		if (range) {
			newRange = range.cloneRange();
			newRange.collapse(atStart);
		}
		const marker = this.j.createInside.span();
		marker.id = MARKER_CLASS + "_" + Number(/* @__PURE__ */ new Date()) + "_" + String(Math.random()).slice(2);
		css(marker, {
			lineHeight: "0",
			display: "none"
		});
		Dom.markTemporary(marker);
		attr(marker, "data-" + MARKER_CLASS, atStart ? "start" : "end");
		Dom.append(marker, this.j.createInside.text("﻿"));
		if (newRange) {
			if (Dom.isOrContains(this.area, atStart ? newRange.startContainer : newRange.endContainer)) newRange.insertNode(marker);
		}
		return marker;
	}
	/**
	* Restores user selections using marker invisible elements in the DOM.
	*/
	restore() {
		let range = false;
		const markerFilter = (start) => (node) => Dom.isMarker(node) && attr(node, "data-jodit-selection_marker") === (start ? "start" : "end");
		const start = Dom.first(this.area, markerFilter(true));
		const end = Dom.first(this.area, markerFilter(false));
		if (!start) return;
		range = this.createRange();
		if (!end) {
			const previousNode = start.previousSibling;
			if (Dom.isText(previousNode)) range.setStart(previousNode, previousNode.nodeValue ? previousNode.nodeValue.length : 0);
			else range.setStartBefore(start);
			Dom.safeRemove(start);
			range.collapse(true);
		} else {
			range.setStartAfter(start);
			Dom.safeRemove(start);
			range.setEndBefore(end);
			Dom.safeRemove(end);
		}
		if (range) this.selectRange(range);
	}
	/**
	* Inserts invisible fake nodes on the boundaries of the current selection
	* and returns them. Unlike [[Select.save]] the selection stays valid while
	* the DOM around it is being modified. Restore it later with [[Select.restoreFakes]].
	*/
	fakes() {
		const sel = this.sel;
		if (!sel || !sel.rangeCount) return [];
		const range = sel.getRangeAt(0);
		assert(range, "Range is null");
		const left = range.cloneRange();
		left.collapse(true);
		const fakeLeft = this.j.createInside.fake();
		Dom.safeInsertNode(left, fakeLeft);
		range.setStartBefore(fakeLeft);
		const result = [fakeLeft];
		if (!range.collapsed) {
			const right = range.cloneRange();
			right.collapse(false);
			const fakeRight = this.j.createInside.fake();
			Dom.safeInsertNode(right, fakeRight);
			range.setEndAfter(fakeRight);
			result.push(fakeRight);
		}
		this.selectRange(range);
		return result;
	}
	/**
	* Restores the selection previously saved with [[Select.fakes]]
	* and removes the fake nodes (disconnected fakes are ignored).
	*/
	restoreFakes(fakes) {
		var _a, _b, _c, _d;
		const nodes = fakes.filter((n) => n.isConnected);
		if (!nodes.length) return;
		const [fakeLeft, fakeRight] = nodes;
		const range = this.createRange();
		range.setStartAfter(fakeLeft);
		if (fakeRight) range.setEndBefore(fakeRight);
		this.selectRange(range);
		if (((_a = fakeLeft.parentNode) === null || _a === void 0 ? void 0 : _a.firstChild) !== ((_b = fakeLeft.parentNode) === null || _b === void 0 ? void 0 : _b.lastChild)) Dom.safeRemove(fakeLeft);
		if (((_c = fakeRight === null || fakeRight === void 0 ? void 0 : fakeRight.parentNode) === null || _c === void 0 ? void 0 : _c.firstChild) !== ((_d = fakeRight === null || fakeRight === void 0 ? void 0 : fakeRight.parentNode) === null || _d === void 0 ? void 0 : _d.lastChild)) Dom.safeRemove(fakeRight);
	}
	/**
	* Saves selections using marker invisible elements in the DOM.
	* @param silent - Do not change current range
	*/
	save(silent = false) {
		if (this.hasMarkers) return [];
		const sel = this.sel;
		if (!sel || !sel.rangeCount) return [];
		const info = [], length = sel.rangeCount, ranges = [];
		for (let i = 0; i < length; i += 1) {
			ranges[i] = sel.getRangeAt(i);
			if (ranges[i].collapsed) {
				const start = this.marker(true, ranges[i]);
				info[i] = {
					startId: start.id,
					collapsed: true,
					startMarker: start.outerHTML
				};
			} else {
				const start = this.marker(true, ranges[i]);
				const end = this.marker(false, ranges[i]);
				info[i] = {
					startId: start.id,
					endId: end.id,
					collapsed: false,
					startMarker: start.outerHTML,
					endMarker: end.outerHTML
				};
			}
		}
		if (!silent) {
			sel.removeAllRanges();
			for (let i = length - 1; i >= 0; --i) {
				const startElm = this.doc.getElementById(info[i].startId);
				if (!startElm) continue;
				if (info[i].collapsed) {
					ranges[i].setStartAfter(startElm);
					ranges[i].collapse(true);
				} else {
					ranges[i].setStartBefore(startElm);
					if (info[i].endId) {
						const endElm = this.doc.getElementById(info[i].endId);
						if (endElm) ranges[i].setEndAfter(endElm);
					}
				}
				try {
					sel.addRange(ranges[i].cloneRange());
				} catch (_a) {}
			}
		}
		return info;
	}
	/**
	* Set focus in editor
	*/
	focus(options = { preventScroll: true }) {
		var _a, _b;
		if (!this.isFocused()) {
			const scrollParent = getScrollParent(this.j.container), scrollTop = scrollParent === null || scrollParent === void 0 ? void 0 : scrollParent.scrollTop;
			if (this.j.iframe) {
				if (this.doc.readyState === "complete") this.j.iframe.focus(options);
			}
			this.win.focus();
			this.area.focus(options);
			if (scrollTop && (scrollParent === null || scrollParent === void 0 ? void 0 : scrollParent.scrollTo)) scrollParent.scrollTo(0, scrollTop);
			const sel = this.sel, range = (sel === null || sel === void 0 ? void 0 : sel.rangeCount) ? sel === null || sel === void 0 ? void 0 : sel.getRangeAt(0) : null;
			if (!range || !Dom.isOrContains(this.area, range.startContainer)) {
				const range = this.createRange();
				range.setStart(this.area, 0);
				range.collapse(true);
				this.selectRange(range, false);
			}
			if (!this.j.editorIsActive) (_b = (_a = this.j) === null || _a === void 0 ? void 0 : _a.events) === null || _b === void 0 || _b.fire("focus");
			return true;
		}
		return false;
	}
	/**
	* Checks whether the current selection is something or just set the cursor is
	* @returns true Selection does't have content
	*/
	isCollapsed() {
		const sel = this.sel;
		for (let r = 0; sel && r < sel.rangeCount; r += 1) if (!sel.getRangeAt(r).collapsed) return false;
		return true;
	}
	/**
	* Checks whether the editor currently in focus
	*/
	isFocused() {
		return this.doc.hasFocus && this.doc.hasFocus() && this.area === this.doc.activeElement;
	}
	/**
	* Returns the current element under the cursor inside editor
	*/
	current(checkChild = true) {
		if (this.j.getRealMode() !== 1) return null;
		const sel = this.sel;
		if (!sel || sel.rangeCount === 0) return null;
		const range = sel.getRangeAt(0);
		let node = range.startContainer;
		let rightMode = false;
		const child = (nd) => rightMode ? nd.lastChild : nd.firstChild;
		if (Dom.isTag(node, "br") && sel.isCollapsed) return node;
		if (!Dom.isText(node)) {
			const ret = findCorrectCurrentNode(node, range, rightMode, sel.isCollapsed, checkChild, child);
			node = ret.node;
			rightMode = ret.rightMode;
		}
		if (node && Dom.isOrContains(this.area, node)) return node;
		return null;
	}
	/**
	* Insert element in editor
	*
	* @param node - Node for insert
	* @param insertCursorAfter - After insert, cursor will move after element
	* @param fireChange - After insert, editor fire change event. You can prevent this behavior
	*/
	insertNode(node, insertCursorAfter = true, fireChange = true) {
		this.errorNode(node);
		const child = Dom.isFragment(node) ? node.lastChild : node;
		this.j.e.fire("safeHTML", node);
		if (!this.isFocused() && this.j.isEditorMode()) {
			this.focus();
			this.restore();
		}
		const sel = this.sel;
		this.j.history.snapshot.transaction(() => {
			if (!this.isCollapsed()) this.j.execCommand("Delete");
			this.j.e.fire("beforeInsertNode", node);
			if (sel && sel.rangeCount) {
				const range = sel.getRangeAt(0);
				const { firstChild } = node;
				if (Dom.isOrContains(this.area, range.commonAncestorContainer)) Dom.safeInsertNode(range, node);
				else Dom.append(this.area, node);
				[
					() => firstChild === null || firstChild === void 0 ? void 0 : firstChild.previousSibling,
					() => firstChild === null || firstChild === void 0 ? void 0 : firstChild.previousSibling,
					() => {
						var _a;
						return (_a = firstChild === null || firstChild === void 0 ? void 0 : firstChild.previousSibling) === null || _a === void 0 ? void 0 : _a.lastChild;
					}
				].forEach((getChildNode) => {
					const childNode = getChildNode();
					if (childNode && Dom.isEmptyTextNode(childNode)) Dom.safeRemove(childNode);
				});
			} else Dom.append(this.area, node);
			const setCursor = (node) => {
				if (Dom.isBlock(node)) {
					const child = node.lastChild;
					if (child) return setCursor(child);
				}
				this.setCursorAfter(node);
			};
			if (insertCursorAfter) if (Dom.isFragment(node)) child && setCursor(child);
			else setCursor(node);
			if (this.j.o.scrollToPastedContent) scrollIntoViewIfNeeded(child !== null && child !== void 0 ? child : node, this.j.editor, this.doc);
		});
		if (fireChange && this.j.events) this.j.__imdSynchronizeValues();
		if (this.j.events) this.j.e.fire("afterInsertNode", Dom.isFragment(node) ? child : node);
	}
	/**
	* Inserts in the current cursor position some HTML snippet
	*
	* @param html - HTML The text to be inserted into the document
	* @param insertCursorAfter - After insert, cursor will move after element
	* @example
	* ```javascript
	* parent.s.insertHTML('<img src="image.png"/>');
	* ```
	*/
	insertHTML(html, insertCursorAfter = true) {
		if (html === "") return;
		const node = this.j.createInside.div();
		const fragment = this.j.createInside.fragment();
		if (!this.isFocused() && this.j.isEditorMode()) {
			this.focus();
			this.restore();
		}
		if (!Dom.isNode(html)) node.innerHTML = html.toString();
		else Dom.append(node, html);
		if (!this.j.isEditorMode() && this.j.e.fire("insertHTML", node.innerHTML) === false) return;
		if (!node.lastChild) return;
		Dom.moveContent(node, fragment);
		this.insertNode(fragment, insertCursorAfter, false);
		this.j.__imdSynchronizeValues();
	}
	/**
	* Insert image in editor
	*
	* @param url - URL for image, or HTMLImageElement
	* @param styles - If specified, it will be applied <code>$(image).css(styles)</code>
	* @param defaultWidth - If specified, it will be applied <code>css('width', defaultWidth)</code>
	*/
	insertImage(url, styles = null, defaultWidth = null) {
		const image = isString(url) ? this.j.createInside.element("img") : url;
		if (isString(url)) attr(image, "src", url);
		if (defaultWidth != null) {
			let dw = defaultWidth.toString();
			if (dw && "auto" !== dw && String(dw).indexOf("px") < 0 && String(dw).indexOf("%") < 0) dw += "px";
			attr(image, "width", dw);
		}
		if (styles && typeof styles === "object") css(image, styles);
		const onload = () => {
			if (image.naturalHeight < image.offsetHeight || image.naturalWidth < image.offsetWidth) css(image, {
				width: "",
				height: ""
			});
			image.removeEventListener("load", onload);
		};
		this.j.e.on(image, "load", onload);
		if (image.complete) onload();
		this.insertNode(image);
		/**
		* Triggered after image was inserted [[Select.insertImage]]. This method can executed from
		* [[FileBrowser]] or [[Uploader]]
		* @example
		* ```javascript
		* const editor = Jodit.make("#redactor");
		* editor.e.on('afterInsertImage', function (image) {
		*     image.className = 'bloghead4';
		* });
		* ```
		*/
		this.j.e.fire("afterInsertImage", image);
	}
	/**
	* Call callback for all selection node
	*/
	eachSelection(callback) {
		var _a;
		const sel = this.sel;
		if (!sel || !sel.rangeCount) return;
		const range = sel.getRangeAt(0);
		let root = range.commonAncestorContainer;
		if (!Dom.isHTMLElement(root)) root = root.parentElement;
		const nodes = [];
		const startOffset = range.startOffset;
		const length = root.childNodes.length;
		const elementOffset = startOffset < length ? startOffset : length - 1;
		let start = range.startContainer === this.area ? root.childNodes[elementOffset] : range.startContainer;
		let end = range.endContainer === this.area ? root.childNodes[range.endOffset - 1] : range.endContainer;
		if (Dom.isText(start) && start === range.startContainer && range.startOffset === ((_a = start.nodeValue) === null || _a === void 0 ? void 0 : _a.length) && start.nextSibling) start = start.nextSibling;
		if (Dom.isText(end) && end === range.endContainer && range.endOffset === 0 && end.previousSibling) end = end.previousSibling;
		const checkElm = (node) => {
			if (node && node !== root && !Dom.isEmptyTextNode(node) && !isMarker(node)) nodes.push(node);
		};
		checkElm(start);
		if (start !== end && Dom.isOrContains(root, start, true)) Dom.find(start, (node) => {
			checkElm(node);
			return node === end || node && node.contains && node.contains(end);
		}, root, true, false);
		const forEvery = (current) => {
			if (!Dom.isOrContains(this.j.editor, current, true)) return;
			if (current.nodeName.match(/^(UL|OL)$/)) return toArray(current.childNodes).forEach(forEvery);
			if (Dom.isTag(current, "li")) if (current.firstChild) current = current.firstChild;
			else {
				const currentB = this.j.createInside.text("﻿");
				Dom.append(current, currentB);
				current = currentB;
			}
			callback(current);
		};
		if (nodes.length === 0) {
			if (Dom.isEmptyTextNode(start)) nodes.push(start);
			if (start === null || start === void 0 ? void 0 : start.firstChild) nodes.push(start.firstChild);
		}
		nodes.forEach(forEvery);
	}
	/**
	* Checks if the cursor is at the end(start) block
	*
	* @param  start - true - check whether the cursor is at the start block
	* @param parentBlock - Find in this
	* @param fake - Node for cursor position
	*
	* @returns true - the cursor is at the end(start) block, null - cursor somewhere outside
	*/
	cursorInTheEdge(start, parentBlock, fake = null) {
		const end = !start, sel = this.sel, range = (sel === null || sel === void 0 ? void 0 : sel.rangeCount) ? sel.getRangeAt(0) : null;
		fake !== null && fake !== void 0 || (fake = this.current(false));
		if (!range || !fake || !Dom.isOrContains(parentBlock, fake, true)) return null;
		const container = start ? range.startContainer : range.endContainer;
		const offset = start ? range.startOffset : range.endOffset;
		const isSignificant = (elm) => Boolean(elm && !Dom.isTag(elm, "br") && !Dom.isEmptyTextNode(elm) && !Dom.isTemporary(elm) && !(Dom.isElement(elm) && this.j.e.fire("isInvisibleForCursor", elm) === true));
		if (Dom.isText(container)) {
			if (cursorInTheEdgeOfString(container, offset, start, end)) return false;
		} else {
			const children = toArray(container.childNodes);
			if (end) {
				if (children.slice(offset).some(isSignificant)) return false;
			} else if (children.slice(0, offset).some(isSignificant)) return false;
		}
		let next = fake;
		while (next && next !== parentBlock) {
			const nextOne = Dom.sibling(next, start);
			if (!nextOne) {
				next = next.parentNode;
				continue;
			}
			next = nextOne;
			if (next && isSignificant(next)) return false;
		}
		return true;
	}
	/**
	* Wrapper for cursorInTheEdge
	*/
	cursorOnTheLeft(parentBlock, fake) {
		return this.cursorInTheEdge(true, parentBlock, fake);
	}
	/**
	* Wrapper for cursorInTheEdge
	*/
	cursorOnTheRight(parentBlock, fake) {
		return this.cursorInTheEdge(false, parentBlock, fake);
	}
	/**
	* Set cursor after the node
	* @returns fake invisible textnode. After insert it can be removed
	*/
	setCursorAfter(node) {
		return this.setCursorNearWith(node, false);
	}
	/**
	* Set cursor before the node
	* @returns fake invisible textnode. After insert it can be removed
	*/
	setCursorBefore(node) {
		return this.setCursorNearWith(node, true);
	}
	/**
	* Add fake node for new cursor position
	*/
	setCursorNearWith(node, inStart) {
		var _a, _b;
		this.errorNode(node);
		if (!Dom.up(node, (elm) => elm === this.area || elm && elm.parentNode === this.area, this.area)) throw error("Node element must be in editor");
		const range = this.createRange();
		let fakeNode = null;
		if (!Dom.isText(node)) {
			fakeNode = this.j.createInside.fake();
			inStart ? range.setStartBefore(node) : range.setEndAfter(node);
			range.collapse(inStart);
			Dom.safeInsertNode(range, fakeNode);
			range.selectNode(fakeNode);
		} else if (inStart) range.setStart(node, 0);
		else range.setEnd(node, (_b = (_a = node.nodeValue) === null || _a === void 0 ? void 0 : _a.length) !== null && _b !== void 0 ? _b : 0);
		range.collapse(inStart);
		this.selectRange(range);
		return fakeNode;
	}
	/**
	* Set cursor in the node
	* @param node - Node element
	* @param inStart - set cursor in start of element
	*/
	setCursorIn(node, inStart = false) {
		this.errorNode(node);
		if (!Dom.up(node, (elm) => elm === this.area || elm && elm.parentNode === this.area, this.area)) throw error("Node element must be in editor");
		const range = this.createRange();
		let start = node, last = node;
		do {
			if (Dom.isText(start) || Dom.isTag(start, INSEPARABLE_TAGS)) break;
			last = start;
			start = inStart ? start.firstChild : start.lastChild;
		} while (start);
		if (!start) {
			const fakeNode = this.j.createInside.text("﻿");
			if (!Dom.isTag(last, INSEPARABLE_TAGS)) {
				Dom.append(last, fakeNode);
				last = fakeNode;
			} else start = last;
		}
		const workElm = start || last;
		if (!Dom.isTag(workElm, INSEPARABLE_TAGS)) {
			range.selectNodeContents(workElm);
			range.collapse(inStart);
		} else {
			inStart || Dom.isTag(workElm, "br") ? range.setStartBefore(workElm) : range.setEndAfter(workElm);
			range.collapse(inStart);
		}
		this.selectRange(range);
		return last;
	}
	/**
	* Set range selection
	*/
	selectRange(range, focus = true) {
		const sel = this.sel;
		if (focus && !this.isFocused() && this.j.e.current !== "focus") this.focus();
		if (sel) {
			sel.removeAllRanges();
			sel.addRange(range);
		}
		/**
		* Fired after change selection
		*/
		this.j.e.fire("changeSelection");
		return this;
	}
	/**
	* Select node
	* @param node - Node element
	* @param inward - select all inside
	*/
	select(node, inward = false) {
		this.errorNode(node);
		if (!Dom.up(node, (elm) => elm === this.area || elm && elm.parentNode === this.area, this.area)) throw error("Node element must be in editor");
		const range = this.createRange();
		range[inward ? "selectNodeContents" : "selectNode"](node);
		return this.selectRange(range);
	}
	/**
	* Return current selected HTML
	* @example
	* ```javascript
	* const editor = Jodit.make();
	* console.log(editor.s.html); // html
	* console.log(Jodit.modules.Helpers.stripTags(editor.s.html)); // plain text
	* ```
	*/
	get html() {
		const sel = this.sel;
		if (sel && sel.rangeCount > 0) {
			const clonedSelection = sel.getRangeAt(0).cloneContents();
			const div = this.j.createInside.div();
			Dom.append(div, clonedSelection);
			return div.innerHTML;
		}
		return "";
	}
	/**
	* Splits the boundaries of the current selection and wraps every
	* contiguous run of selected inline content (grouped by block) into a
	* `<font>` element, returning those wrappers in document order.
	*
	* This is a pure-DOM replacement for the old
	* `nativeExecCommand('fontsize', false, '7')` trick which relied on the
	* browser to split the selection into `<font size="7">` fragments.
	*/
	__wrapSelectionFragments() {
		const range = this.range;
		this.__splitSelectionBoundaries(range);
		let root = range.commonAncestorContainer;
		if (Dom.isText(root)) root = root.parentNode;
		if (!root) return [];
		return this.__wrapSelectionRuns(this.__collectContainedNodes(root, range));
	}
	/**
	* Splits the text nodes at both ends of the range so that its boundaries
	* always fall between nodes. Afterwards every node inside the range is
	* fully (not partially) selected.
	*/
	__splitSelectionBoundaries(range) {
		var _a, _b, _c, _d, _e, _f, _g, _h;
		let { startContainer, endContainer } = range;
		let { startOffset, endOffset } = range;
		if (Dom.isText(endContainer) && endOffset > 0 && endOffset < ((_b = (_a = endContainer.nodeValue) === null || _a === void 0 ? void 0 : _a.length) !== null && _b !== void 0 ? _b : 0)) {
			endContainer.splitText(endOffset);
			endOffset = (_d = (_c = endContainer.nodeValue) === null || _c === void 0 ? void 0 : _c.length) !== null && _d !== void 0 ? _d : endOffset;
		}
		if (Dom.isText(startContainer) && startOffset > 0 && startOffset < ((_f = (_e = startContainer.nodeValue) === null || _e === void 0 ? void 0 : _e.length) !== null && _f !== void 0 ? _f : 0)) {
			const middle = startContainer.splitText(startOffset);
			if (startContainer === endContainer) {
				endContainer = middle;
				endOffset = (_h = (_g = middle.nodeValue) === null || _g === void 0 ? void 0 : _g.length) !== null && _h !== void 0 ? _h : 0;
			}
			startContainer = middle;
			startOffset = 0;
		}
		range.setStart(startContainer, startOffset);
		range.setEnd(endContainer, endOffset);
		this.__normalizeRangeBoundary(range, true);
		this.__normalizeRangeBoundary(range, false);
	}
	__normalizeRangeBoundary(range, atStart) {
		var _a, _b;
		const container = atStart ? range.startContainer : range.endContainer;
		if (!Dom.isText(container)) return;
		const offset = atStart ? range.startOffset : range.endOffset;
		const length = (_b = (_a = container.nodeValue) === null || _a === void 0 ? void 0 : _a.length) !== null && _b !== void 0 ? _b : 0;
		if (offset === 0) atStart ? range.setStartBefore(container) : range.setEndBefore(container);
		else if (offset >= length) atStart ? range.setStartAfter(container) : range.setEndAfter(container);
	}
	/**
	* Collects the highest-level nodes that are completely inside the range,
	* descending into nodes that are only partially selected.
	*/
	__collectContainedNodes(root, range) {
		const result = [];
		toArray(root.childNodes).forEach((child) => {
			if (this.__isFullyContained(range, child)) result.push(child);
			else if (child.childNodes.length && range.intersectsNode(child)) result.push(...this.__collectContainedNodes(child, range));
		});
		return result;
	}
	__isFullyContained(range, node) {
		const nodeRange = this.createRange();
		nodeRange.selectNode(node);
		return range.compareBoundaryPoints(Range.START_TO_START, nodeRange) <= 0 && range.compareBoundaryPoints(Range.END_TO_END, nodeRange) >= 0;
	}
	/**
	* Wraps every contiguous run of selected inline siblings into a `<font>`
	* element. Block-level nodes are never wrapped themselves - their inline
	* content is wrapped instead, keeping every `<font>` inside a single block.
	*/
	__wrapSelectionRuns(nodes) {
		const fonts = [];
		let run = [];
		const flush = () => {
			if (run.length) {
				fonts.push(this.__wrapRunInFont(run));
				run = [];
			}
		};
		nodes.forEach((node) => {
			if (Dom.isElement(node) && this.__isOrContainsBlock(node)) {
				flush();
				fonts.push(...this.__wrapSelectionRuns(toArray(node.childNodes)));
			} else {
				if (run.length && run[run.length - 1].parentNode !== node.parentNode) flush();
				run.push(node);
			}
		});
		flush();
		return fonts;
	}
	__isOrContainsBlock(node) {
		if (Dom.isBlock(node)) return true;
		return toArray(node.childNodes).some((child) => this.__isOrContainsBlock(child));
	}
	__wrapRunInFont(run) {
		const font = this.j.createInside.element("font");
		const [first] = run;
		Dom.before(first, font);
		run.forEach((node) => Dom.append(font, node));
		return font;
	}
	/**
	* Wrap all selected fragments inside Tag or apply some callback
	*/
	*wrapInTagGen(fakes) {
		if (this.isCollapsed()) {
			const font = this.jodit.createInside.element("font", "﻿");
			this.insertNode(font, false, false);
			if (fakes && fakes[0]) Dom.append(font, fakes[0]);
			yield font;
			Dom.unwrap(font);
			return;
		}
		const elms = this.__wrapSelectionFragments();
		for (const font of elms) {
			const { firstChild, lastChild } = font;
			if (firstChild && firstChild === lastChild && isMarker(firstChild)) {
				Dom.unwrap(font);
				continue;
			}
			if (firstChild && isMarker(firstChild)) Dom.before(font, firstChild);
			if (lastChild && isMarker(lastChild)) Dom.after(font, lastChild);
			yield font;
			Dom.unwrap(font);
		}
	}
	/**
	* Wrap all selected fragments inside Tag or apply some callback
	*/
	wrapInTag(tagOrCallback) {
		const result = [];
		for (const font of this.wrapInTagGen()) try {
			if (font.firstChild && font.firstChild === font.lastChild && isMarker(font.firstChild)) continue;
			if (isFunction(tagOrCallback)) tagOrCallback(font);
			else result.push(Dom.replace(font, tagOrCallback, this.j.createInside));
		} finally {
			const pn = font.parentNode;
			if (pn) {
				Dom.unwrap(font);
				if (Dom.isEmpty(pn)) Dom.unwrap(pn);
			}
		}
		return result;
	}
	/**
	* Apply some css rules for all selections. It method wraps selections in nodeName tag.
	* @example
	* ```js
	* const editor = Jodit.make('#editor');
	* editor.value = 'test';
	* editor.execCommand('selectall');
	*
	* editor.s.commitStyle({
	* 	style: {color: 'red'}
	* }) // will wrap `text` in `span` and add style `color:red`
	* editor.s.commitStyle({
	* 	style: {color: 'red'}
	* }) // will remove `color:red` from `span`
	* ```
	*/
	commitStyle(options) {
		assert(size$1(options) > 0, "Need to pass at least one option");
		new CommitStyle(options).apply(this.j);
	}
	/**
	* Split selection on two parts: left and right
	*/
	splitSelection(currentBox, edge) {
		if (!this.isCollapsed()) return null;
		const leftRange = this.createRange();
		const range = this.range;
		leftRange.setStartBefore(currentBox);
		const cursorOnTheRight = this.cursorOnTheRight(currentBox, edge);
		const cursorOnTheLeft = this.cursorOnTheLeft(currentBox, edge);
		const br = this.j.createInside.element("br"), prevFake = this.j.createInside.fake(), nextFake = prevFake.cloneNode();
		try {
			if (cursorOnTheRight || cursorOnTheLeft) {
				if (edge) Dom.before(edge, br);
				else Dom.safeInsertNode(range, br);
				const clearBR = (start, getNext) => {
					let next = getNext(start);
					while (next) {
						const nextSib = getNext(next);
						if (next && (Dom.isTag(next, "br") || Dom.isEmptyTextNode(next))) Dom.safeRemove(next);
						else break;
						next = nextSib;
					}
				};
				clearBR(br, (n) => n.nextSibling);
				clearBR(br, (n) => n.previousSibling);
				Dom.after(br, nextFake);
				Dom.before(br, prevFake);
				if (cursorOnTheRight) {
					leftRange.setEndBefore(br);
					range.setEndBefore(br);
				} else {
					leftRange.setEndAfter(br);
					range.setEndAfter(br);
				}
			} else leftRange.setEnd(range.startContainer, range.startOffset);
			const fragment = leftRange.extractContents();
			const clearEmpties = (node) => Dom.each(node, (node) => Dom.isEmptyTextNode(node) && Dom.safeRemove(node));
			assert(currentBox.parentNode, "Splitting fails");
			try {
				clearEmpties(fragment);
				clearEmpties(currentBox);
				Dom.before(currentBox, fragment);
				if (!edge && cursorOnTheRight && (br === null || br === void 0 ? void 0 : br.parentNode)) {
					const range = this.createRange();
					range.setStartBefore(br);
					this.selectRange(range);
				}
			} catch (e) {}
			const fillFakeParent = (fake) => {
				const parent = fake === null || fake === void 0 ? void 0 : fake.parentNode;
				if (parent && parent.firstChild === parent.lastChild) Dom.append(parent, br.cloneNode());
			};
			fillFakeParent(prevFake);
			fillFakeParent(nextFake);
		} finally {
			Dom.safeRemove(prevFake);
			Dom.safeRemove(nextFake);
		}
		return currentBox.previousElementSibling;
	}
	/**
	* Expands the non-collapsed selection outward: boundaries positioned on the
	* edge of their parents are moved out of them (e.g. `<p><b>|test|</b></p>`
	* becomes `<p>|<b>test</b>|</p>`)
	*/
	expandSelection() {
		if (this.isCollapsed()) return this;
		const { range } = this;
		const c = range.cloneRange();
		if (!Dom.isOrContains(this.j.editor, range.commonAncestorContainer, true)) return this;
		const moveMaxEdgeFake = (start) => {
			const fake = this.j.createInside.fake();
			const r = range.cloneRange();
			r.collapse(start);
			Dom.safeInsertNode(r, fake);
			moveTheNodeAlongTheEdgeOutward(fake, start, this.j.editor);
			return fake;
		};
		const leftFake = moveMaxEdgeFake(true);
		const rightFake = moveMaxEdgeFake(false);
		c.setStartAfter(leftFake);
		c.setEndBefore(rightFake);
		const leftBox = Dom.findSibling(leftFake, false);
		const rightBox = Dom.findSibling(rightFake, true);
		if (leftBox !== rightBox) {
			const rightInsideLeft = Dom.isElement(leftBox) && Dom.isOrContains(leftBox, rightFake);
			const leftInsideRight = !rightInsideLeft && Dom.isElement(rightBox) && Dom.isOrContains(rightBox, leftFake);
			if (rightInsideLeft || leftInsideRight) {
				let child = rightInsideLeft ? leftBox : rightBox, container = child;
				while (Dom.isElement(child)) {
					child = rightInsideLeft ? child.firstElementChild : child.lastElementChild;
					if (child) {
						if (rightInsideLeft ? Dom.isOrContains(child, rightFake) : Dom.isOrContains(child, leftFake)) container = child;
					}
				}
				if (rightInsideLeft) c.setStart(container, 0);
				else c.setEnd(container, container.childNodes.length);
			}
		}
		this.selectRange(c);
		Dom.safeRemove(leftFake, rightFake);
		if (this.isCollapsed()) throw error("Selection is collapsed");
		return this;
	}
};
__decorate$9([autobind], Selection.prototype, "createRange", null);
__decorate$9([autobind], Selection.prototype, "focus", null);
__decorate$9([autobind], Selection.prototype, "setCursorAfter", null);
__decorate$9([autobind], Selection.prototype, "setCursorBefore", null);
__decorate$9([autobind], Selection.prototype, "setCursorIn", null);
//#endregion
//#region node_modules/jodit/esm/core/selection/index.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
//#endregion
//#region node_modules/jodit/esm/modules/index.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* [[include:modules/README.md]]
* @packageDocumentation
* @module modules
*/
var modules_exports = /* @__PURE__ */ __exportAll({
	Ajax: () => Ajax,
	Alert: () => Alert,
	Async: () => Async,
	AsyncStorage: () => AsyncStorage,
	Button: () => Button,
	CommitStyle: () => CommitStyle,
	Component: () => Component,
	Confirm: () => Confirm,
	ContextMenu: () => ContextMenu,
	Create: () => Create,
	Dialog: () => Dialog,
	Dom: () => Dom,
	EventEmitter: () => EventEmitter,
	EventHandlersStore: () => EventHandlersStore,
	Eventify: () => Eventify,
	FileBrowser: () => FileBrowser,
	Helpers: () => helpers_exports,
	History: () => History,
	Icon: () => Icon,
	ImageEditor: () => ImageEditor,
	IndexedDBProvider: () => IndexedDBProvider,
	LazyWalker: () => LazyWalker,
	LocalStorageProvider: () => LocalStorageProvider,
	MemoryStorageProvider: () => MemoryStorageProvider,
	Plugin: () => Plugin,
	PluginSystem: () => PluginSystem,
	Popup: () => Popup,
	ProgressBar: () => ProgressBar,
	Prompt: () => Prompt,
	Response: () => Response,
	STATUSES: () => STATUSES,
	Selection: () => Selection,
	Snapshot: () => Snapshot,
	StatusBar: () => StatusBar,
	Storage: () => Storage,
	StorageKey: () => StorageKey,
	Table: () => Table,
	ToolbarButton: () => ToolbarButton,
	ToolbarCollection: () => ToolbarCollection,
	ToolbarContent: () => ToolbarContent,
	ToolbarEditorCollection: () => ToolbarEditorCollection,
	ToolbarSelect: () => ToolbarSelect,
	UIBlock: () => UIBlock,
	UIButton: () => UIButton,
	UIButtonGroup: () => UIButtonGroup,
	UIButtonState: () => UIButtonState,
	UICheckbox: () => UICheckbox,
	UIElement: () => UIElement,
	UIFileInput: () => UIFileInput,
	UIForm: () => UIForm,
	UIGroup: () => UIGroup,
	UIInput: () => UIInput,
	UIList: () => UIList,
	UIMessages: () => UIMessages,
	UISelect: () => UISelect,
	UISeparator: () => UISeparator,
	UISpacer: () => UISpacer,
	UITextArea: () => UITextArea,
	UITooltip: () => UITooltip,
	Uploader: () => Uploader,
	View: () => View,
	ViewComponent: () => ViewComponent,
	ViewWithToolbar: () => ViewWithToolbar,
	canUseIndexedDB: () => canUseIndexedDB,
	canUsePersistentStorage: () => canUsePersistentStorage,
	clearUseIndexedDBCache: () => clearUseIndexedDBCache,
	defaultNameSpace: () => defaultNameSpace,
	observable: () => observable
});
//#endregion
//#region node_modules/jodit/esm/jodit.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$8 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var Jodit_1;
var __defaultStyleDisplayKey = "data-jodit-default-style-display";
var __defaultClassesKey = "data-jodit-default-classes";
var NOEDIT = { contenteditable: false };
var Jodit$1 = Jodit_1 = class Jodit extends ViewWithToolbar {
	/** @override */
	className() {
		return "Jodit";
	}
	/**
	* Return promise for ready actions
	* @example
	* ```js
	* const jodit = Jodit.make('#editor');
	* await jodit.waitForReady();
	* jodit.e.fire('someAsyncLoadedPluginEvent', (test) => {
	*   alert(test);
	* });
	* ```
	*/
	waitForReady() {
		if (this.isReady) return Promise.resolve(this);
		return this.async.promise((resolve) => {
			this.hookStatus("ready", () => resolve(this));
		});
	}
	/**
	* @deprecated I don't know why I wrote itp
	*/
	static get ready() {
		return new Promise((resolve) => {
			eventEmitter.on("joditready", resolve);
		});
	}
	/**
	* Plain text editor's value
	*/
	get text() {
		if (this.editor) return this.editor.innerText || "";
		const div = this.createInside.div();
		div.innerHTML = this.getElementValue();
		return div.innerText || "";
	}
	/**
	* Return a default timeout period in milliseconds for some debounce or throttle functions.
	* By default, `{history.timeout}` options
	*/
	get defaultTimeout() {
		return isNumber(this.o.defaultTimeout) ? this.o.defaultTimeout : Config.defaultOptions.defaultTimeout;
	}
	/**
	* Method wrap usual object in Object helper for prevent deep object merging in options*
	* ```js
	* const editor = Jodit.make('#editor', {
	* 	controls: {
	* 		fontsize: {
	* 			list: Jodit.atom([8, 9, 10])
	* 		}
	* 	}
	* });
	* ```
	* In this case, the array [8, 9, 10] will not be combined with other arrays, but will replace them
	*/
	static atom(object) {
		return markAsAtomic(object);
	}
	/**
	* Factory for creating Jodit instance
	*/
	static make(element, options) {
		return new this(element, options);
	}
	/**
	* Checks if the element has already been initialized when for Jodit
	*/
	static isJoditAssigned(element) {
		return element && isJoditObject(element.component) && !element.component.isInDestruct;
	}
	/**
	* Default settings
	*/
	static get defaultOptions() {
		return Config.defaultOptions;
	}
	/**
	* Deep-merges partial options into the global defaults without replacing
	* top-level objects. This lets you patch nested settings (e.g. a single
	* button inside `controls`) without losing the rest:
	*
	* ```js
	* // Add a custom button — all existing controls remain untouched
	* Jodit.configure({
	*   controls: {
	*     myButton: {
	*       icon: 'pencil',
	*       command: 'selectall'
	*     }
	*   }
	* });
	*
	* // Override only the `group` of an existing button
	* Jodit.configure({
	*   controls: {
	*     someButton: { group: 'custom' }
	*   }
	* });
	*
	* // Works with any nested option
	* Jodit.configure({
	*   createAttributes: {
	*     div: { class: 'my-class' }
	*   }
	* });
	*
	* // Use Jodit.atom() to replace a nested value entirely instead of merging
	* Jodit.configure({
	*   controls: {
	*     fontsize: {
	*       list: Jodit.atom([8, 9, 10])
	*     }
	*   }
	* });
	* ```
	*
	* @see {@link ConfigMerge} for the merge algorithm
	* @see {@link ConfigProto} for per-instance prototype-based merge used at editor creation time
	*/
	static configure(options) {
		ConfigMerge(Jodit_1.defaultOptions, options);
	}
	get createInside() {
		return new Create(() => this.ed, this.o.createAttributes);
	}
	__setPlaceField(field, value) {
		if (!this.currentPlace) {
			this.currentPlace = {};
			this.places = [this.currentPlace];
		}
		this.currentPlace[field] = value;
	}
	/**
	* element It contains source element
	*/
	get element() {
		return this.currentPlace.element;
	}
	/**
	* editor It contains the root element editor
	*/
	get editor() {
		return this.currentPlace.editor;
	}
	set editor(editor) {
		this.__setPlaceField("editor", editor);
	}
	/**
	* Container for all staff
	*/
	get container() {
		return this.currentPlace.container;
	}
	set container(container) {
		this.__setPlaceField("container", container);
	}
	/**
	* workplace It contains source and wysiwyg editors
	*/
	get workplace() {
		return this.currentPlace.workplace;
	}
	get message() {
		return this.getMessageModule(this.workplace);
	}
	/**
	* Statusbar module
	*/
	get statusbar() {
		return this.currentPlace.statusbar;
	}
	/**
	* iframe Iframe for iframe mode
	*/
	get iframe() {
		return this.currentPlace.iframe;
	}
	set iframe(iframe) {
		this.__setPlaceField("iframe", iframe);
	}
	get history() {
		return this.currentPlace.history;
	}
	/**
	* In iframe mode editor's window can be different by owner
	*/
	get editorWindow() {
		return this.currentPlace.editorWindow;
	}
	set editorWindow(win) {
		this.__setPlaceField("editorWindow", win);
	}
	/**
	* Alias for this.ew
	*/
	get ew() {
		return this.editorWindow;
	}
	/**
	* In iframe mode editor's window can be different by owner
	*/
	get editorDocument() {
		return this.currentPlace.editorWindow.document;
	}
	/**
	* Alias for this.ew
	*/
	get ed() {
		return this.editorDocument;
	}
	/**
	* options All Jodit settings default + second arguments of constructor
	*/
	get options() {
		return this.currentPlace.options;
	}
	set options(opt) {
		this.__options = opt;
		this.__setPlaceField("options", opt);
	}
	/**
	* Alias for this.selection
	*/
	get s() {
		return this.selection;
	}
	get uploader() {
		return this.getInstance("Uploader", this.o.uploader);
	}
	get filebrowser() {
		const jodit = this;
		const options = ConfigProto({
			defaultTimeout: jodit.defaultTimeout,
			uploader: jodit.o.uploader,
			language: jodit.o.language,
			license: jodit.o.license,
			theme: jodit.o.theme,
			shadowRoot: jodit.o.shadowRoot,
			defaultCallback(data) {
				if (data.files && data.files.length) data.files.forEach((file, i) => {
					const url = data.baseurl + file;
					if (data.isImages ? data.isImages[i] : false) jodit.s.insertImage(url, null, jodit.o.imageDefaultWidth);
					else jodit.s.insertNode(jodit.createInside.fromHTML(`<a href='${url}' title='${url}'>${url}</a>`));
				});
			}
		}, this.o.filebrowser);
		return jodit.getInstance("FileBrowser", options);
	}
	/**
	* Editor's mode
	*/
	get mode() {
		return this.__mode;
	}
	set mode(mode) {
		this.setMode(mode);
	}
	/**
	* Return real HTML value from WYSIWYG editor.
	* @internal
	*/
	getNativeEditorValue() {
		const value = this.e.fire("beforeGetNativeEditorValue");
		if (isString(value)) return value;
		if (this.editor) return this.editor.innerHTML;
		return this.getElementValue();
	}
	/**
	* Set value to native editor
	*/
	setNativeEditorValue(value) {
		const data = { value };
		if (this.e.fire("beforeSetNativeEditorValue", data)) return;
		if (this.editor) this.editor.innerHTML = data.value;
	}
	/**
	* HTML value
	*/
	get value() {
		return this.getEditorValue();
	}
	set value(html) {
		this.setEditorValue(html);
		this.history.__processChanges();
	}
	synchronizeValues() {
		this.__imdSynchronizeValues();
	}
	/**
	* This is an internal method, do not use it in your applications.
	* @private
	* @internal
	*/
	__imdSynchronizeValues() {
		this.setEditorValue();
	}
	/**
	* Return editor value
	*/
	getEditorValue(removeSelectionMarkers = true, consumer) {
		/**
		* Triggered before getEditorValue executed.
		* If returned not undefined, getEditorValue will return this value
		* @example
		* ```javascript
		* var editor = Jodit.make("#redactor");
		* editor.e.on('beforeGetValueFromEditor', function () {
		*     return editor.editor.innerHTML.replace(/a/g, 'b');
		* });
		* ```
		*/
		let value;
		value = this.e.fire("beforeGetValueFromEditor", consumer);
		if (value !== void 0) return value;
		value = this.getNativeEditorValue().replace(INVISIBLE_SPACE_REG_EXP(), "");
		if (removeSelectionMarkers) value = value.replace(/<span[^>]+id="jodit-selection_marker_[^>]+><\/span>/g, "");
		if (value === "<br>") value = "";
		/**
		* Triggered after getEditorValue got value from wysiwyg.
		* It can change new_value.value
		*
		* @example
		* ```javascript
		* var editor = Jodit.make("#redactor");
		* editor.e.on('afterGetValueFromEditor', function (new_value) {
		*     new_value.value = new_value.value.replace('a', 'b');
		* });
		* ```
		*/
		const new_value = { value };
		this.e.fire("afterGetValueFromEditor", new_value, consumer);
		return new_value.value;
	}
	/**
	* Set editor html value and if set sync fill source element value
	* When method was called without arguments - it is a simple way to synchronize editor to element
	*/
	setEditorValue(value) {
		/**
		* Triggered before getEditorValue set value to wysiwyg.
		* @example
		* ```javascript
		* var editor = Jodit.make("#redactor");
		* editor.e.on('beforeSetValueToEditor', function (old_value) {
		*     return old_value.value.replace('a', 'b');
		* });
		* editor.e.on('beforeSetValueToEditor', function () {
		*     return false; // disable setEditorValue method
		* });
		* ```
		*/
		const newValue = this.e.fire("beforeSetValueToEditor", value);
		if (newValue === false) return;
		if (isString(newValue)) value = newValue;
		if (!this.editor) {
			if (value !== void 0) this.__setElementValue(value);
			return;
		}
		if (!isString(value) && !isVoid(value)) throw error("value must be string");
		if (!isVoid(value) && this.getNativeEditorValue() !== value) this.setNativeEditorValue(value);
		this.e.fire("postProcessSetEditorValue");
		const old_value = this.getElementValue(), new_value = this.getEditorValue();
		if (!this.__isSilentChange && old_value !== new_value && this.__callChangeCount < 10) {
			this.__setElementValue(new_value);
			this.__callChangeCount += 1;
			try {
				this.history.__upTick();
				this.e.fire("change", new_value, old_value);
				this.e.fire(this.history, "change", new_value, old_value);
			} finally {
				this.__callChangeCount = 0;
			}
		}
	}
	/**
	* If some plugin changes the DOM directly, then you need to update the content of the original element
	*/
	updateElementValue() {
		this.__setElementValue(this.getEditorValue());
	}
	/**
	* Return source element value
	*/
	getElementValue() {
		return this.element.value !== void 0 ? this.element.value : this.element.innerHTML;
	}
	__setElementValue(value) {
		if (!isString(value)) throw error("value must be string");
		if (this.element !== this.container && value !== this.getElementValue()) {
			const data = { value };
			callPromise(this.e.fire("beforeSetElementValue", data), () => {
				if (this.element.value !== void 0) this.element.value = data.value;
				else this.element.innerHTML = data.value;
				this.e.fire("afterSetElementValue", data);
			});
		}
	}
	/**
	* Register custom handler for command
	*
	* @example
	* ```javascript
	* var jodit = Jodit.make('#editor);
	*
	* jodit.setEditorValue('test test test');
	*
	* jodit.registerCommand('replaceString', function (command, needle, replace) {
	*      var value = this.getEditorValue();
	*      this.setEditorValue(value.replace(needle, replace));
	*      return false; // stop execute native command
	* });
	*
	* jodit.execCommand('replaceString', 'test', 'stop');
	*
	* console.log(jodit.value); // stop test
	*
	* // and you can add hotkeys for command
	* jodit.registerCommand('replaceString', {
	*    hotkeys: 'ctrl+r',
	*    exec: function (command, needle, replace) {
	*     var value = this.getEditorValue();
	*     this.setEditorValue(value.replace(needle, replace));
	*    }
	* });
	*
	* ```
	*/
	registerCommand(commandNameOriginal, command, options) {
		const commandName = commandNameOriginal.toLowerCase();
		let commands = this.commands.get(commandName);
		if (commands === void 0) {
			commands = [];
			this.commands.set(commandName, commands);
		}
		commands.push(command);
		if (!isFunction(command)) {
			const hotkeys = this.o.commandToHotkeys[commandName] || this.o.commandToHotkeys[commandNameOriginal] || command.hotkeys;
			if (hotkeys) this.registerHotkeyToCommand(hotkeys, commandName, options === null || options === void 0 ? void 0 : options.stopPropagation);
		}
		return this;
	}
	/**
	* Register hotkey for command
	*/
	registerHotkeyToCommand(hotkeys, commandName, shouldStop = true) {
		const shortcuts = asArray(hotkeys).map(normalizeKeyAliases).map((hotkey) => hotkey + ".hotkey").join(" ");
		this.e.off(shortcuts).on(shortcuts, (type, stop) => {
			if (stop) stop.shouldStop = shouldStop !== null && shouldStop !== void 0 ? shouldStop : true;
			return this.execCommand(commandName);
		});
	}
	/**
	* Execute command editor
	*
	* @param command - command. It supports all the
	* @see https://developer.mozilla.org/ru/docs/Web/API/Document/execCommand#commands and a number of its own
	* for example applyStyleProperty. Comand fontSize receives the second parameter px,
	* formatBlock and can take several options
	* @example
	* ```javascript
	* this.execCommand('fontSize', 12); // sets the size of 12 px
	* this.execCommand('underline');
	* this.execCommand('formatBlock', 'p'); // will be inserted paragraph
	* ```
	*/
	execCommand(command, showUI, value, ...args) {
		if (!this.s.isFocused()) this.s.focus();
		if (this.o.readonly && !this.o.allowCommandsInReadOnly.includes(command)) return;
		let result;
		command = command.toLowerCase();
		/**
		* Called before any command
		* @param command - Command name in lowercase
		* @param second - The second parameter for the command
		* @param third - The third option is for the team
		* @example
		* ```javascript
		* parent.e.on('beforeCommand', function (command) {
		*  if (command === 'justifyCenter') {
		*      var p = parent.c.element('p')
		*      parent.s.insertNode(p)
		*      parent.s.setCursorIn(p);
		*      p.style.textAlign = 'justyfy';
		*      return false; // break executes native command
		*  }
		* })
		* ```
		*/
		result = this.e.fire(`beforeCommand${ucfirst(command)}`, showUI, value, ...args);
		if (result !== false) result = this.e.fire("beforeCommand", command, showUI, value, ...args);
		if (result !== false) result = this.__execCustomCommands(command, showUI, value, ...args);
		if (result !== false) {
			this.s.focus();
			try {
				result = this.nativeExecCommand(command, showUI, value);
			} catch (e) {}
		}
		/**
		* It called after any command
		* @param command - name command
		* @param second - The second parameter for the command
		* @param third - The third option is for the team
		*/
		this.e.fire("afterCommand", command, showUI, value);
		this.__imdSynchronizeValues();
		return result;
	}
	/**
	* Exec native command
	*/
	nativeExecCommand(command, showUI, value) {
		this.__isSilentChange = true;
		try {
			return this.ed.execCommand(command, showUI, value);
		} finally {
			this.__isSilentChange = false;
		}
	}
	__execCustomCommands(commandName, second, third, ...args) {
		commandName = commandName.toLowerCase();
		const commands = this.commands.get(commandName);
		if (commands !== void 0) {
			let result;
			commands.forEach((command) => {
				let callback;
				if (isFunction(command)) callback = command;
				else callback = command.exec;
				const resultCurrent = callback.call(this, commandName, second, third, ...args);
				if (resultCurrent !== void 0) result = resultCurrent;
			});
			return result;
		}
	}
	/**
	* Disable selecting
	*/
	lock(name = "any") {
		if (super.lock(name)) {
			this.__selectionLocked = this.s.save();
			this.s.clear();
			this.container.classList.add("jodit_lock");
			this.e.fire("lock", true);
			return true;
		}
		return false;
	}
	/**
	* Enable selecting
	*/
	unlock() {
		if (super.unlock()) {
			this.container.classList.remove("jodit_lock");
			if (this.__selectionLocked) this.s.restore();
			this.e.fire("lock", false);
			return true;
		}
		return false;
	}
	/**
	* Return current editor mode: Jodit.MODE_WYSIWYG, Jodit.MODE_SOURCE or Jodit.MODE_SPLIT
	*/
	getMode() {
		return this.mode;
	}
	isEditorMode() {
		return this.getRealMode() === 1;
	}
	/**
	* Return current real work mode. When editor in MODE_SOURCE or MODE_WYSIWYG it will
	* return them, but then editor in MODE_SPLIT it will return MODE_SOURCE if
	* Textarea(CodeMirror) focused or MODE_WYSIWYG otherwise
	*
	* @example
	* ```javascript
	* var editor = Jodit.make('#editor');
	* console.log(editor.getRealMode());
	* ```
	*/
	getRealMode() {
		if (this.getMode() !== 3) return this.getMode();
		const active = this.od.activeElement;
		if (active && (active === this.iframe || Dom.isOrContains(this.editor, active) || Dom.isOrContains(this.toolbar.container, active))) return 1;
		return 2;
	}
	/**
	* Set current mode
	*/
	setMode(mode) {
		const oldMode = this.getMode();
		const data = { mode: parseInt(mode.toString(), 10) }, modeClasses = [
			"jodit-wysiwyg_mode",
			"jodit-source__mode",
			"jodit_split_mode"
		];
		/**
		* Triggered before setMode executed. If returned false method stopped
		* @param data - PlainObject `{mode: {string}}` In handler you can change data.mode
		* @example
		* ```javascript
		* var editor = Jodit.make("#redactor");
		* editor.e.on('beforeSetMode', function (data) {
		*     data.mode = Jodit.MODE_SOURCE; // not respond to the mode change. Always make the source code mode
		* });
		* ```
		*/
		if (this.e.fire("beforeSetMode", data) === false) return;
		this.__mode = [
			2,
			1,
			3
		].includes(data.mode) ? data.mode : 1;
		if (this.o.saveModeInStorage) this.storage.set("jodit_default_mode", this.mode);
		modeClasses.forEach((className) => {
			this.container.classList.remove(className);
		});
		this.container.classList.add(modeClasses[this.mode - 1]);
		/**
		* Triggered after setMode executed
		* @example
		* ```javascript
		* var editor = Jodit.make("#redactor");
		* editor.e.on('afterSetMode', function () {
		*     editor.setEditorValue(''); // clear editor's value after change mode
		* });
		* ```
		*/
		if (oldMode !== this.getMode()) this.e.fire("afterSetMode");
	}
	/**
	* Toggle editor mode WYSIWYG to TEXTAREA(CodeMirror) to SPLIT(WYSIWYG and TEXTAREA) to again WYSIWYG
	*
	* @example
	* ```javascript
	* var editor = Jodit.make('#editor');
	* editor.toggleMode();
	* ```
	*/
	toggleMode() {
		let mode = this.getMode();
		if ([
			2,
			1,
			this.o.useSplitMode ? 3 : 9
		].includes(mode + 1)) mode += 1;
		else mode = 1;
		this.setMode(mode);
	}
	/**
	* Switch on/off the editor into the disabled state.
	* When disabled, the user is not able to change the editor content
	* This function firing the `disabled` event.
	*/
	setDisabled(isDisabled) {
		this.o.disabled = isDisabled;
		const readOnly = this.__wasReadOnly;
		this.setReadOnly(isDisabled || readOnly);
		this.__wasReadOnly = readOnly;
		if (this.editor) {
			attr(this.editor, "aria-disabled", isDisabled);
			this.container.classList.toggle("jodit_disabled", isDisabled);
			this.e.fire("disabled", isDisabled);
		}
	}
	/**
	* Return true if editor in disabled mode
	*/
	getDisabled() {
		return this.o.disabled;
	}
	/**
	* Switch on/off the editor into the read-only state.
	* When in readonly, the user is not able to change the editor content, but can still
	* use some editor functions (show source code, print content, or seach).
	* This function firing the `readonly` event.
	*/
	setReadOnly(isReadOnly) {
		if (this.__wasReadOnly === isReadOnly) return;
		this.__wasReadOnly = isReadOnly;
		this.o.readonly = isReadOnly;
		if (isReadOnly) this.editor && attr(this.editor, "contenteditable", null);
		else this.editor && attr(this.editor, "contenteditable", true);
		this.e && this.e.fire("readonly", isReadOnly);
	}
	/**
	* Return true if editor in read-only mode
	*/
	getReadOnly() {
		return this.o.readonly;
	}
	focus() {
		if (this.getMode() !== 2) this.s.focus();
	}
	get isFocused() {
		return this.s.isFocused();
	}
	/**
	* Hook before init
	*/
	beforeInitHook() {}
	/**
	* Hook after init
	*/
	afterInitHook() {}
	/** @override **/
	initOptions(options) {
		this.options = ConfigProto(options || {}, Config.defaultOptions);
	}
	/** @override **/
	initOwners() {
		this.editorWindow = this.o.ownerWindow;
		this.ownerWindow = this.o.ownerWindow;
	}
	/**
	* Create instance of Jodit
	*
	* @param element - Selector or HTMLElement
	* @param options - Editor's options
	*/
	constructor(element, options) {
		super(options, true);
		/**
		* Define if object is Jodit
		*/
		this.isJodit = true;
		this.commands = /* @__PURE__ */ new Map();
		this.__selectionLocked = null;
		this.__wasReadOnly = false;
		/**
		* Editor has focus in this time
		*/
		this.editorIsActive = false;
		this.__mode = 1;
		this.__callChangeCount = 0;
		/**
		* Don't raise a change event
		*/
		this.__isSilentChange = false;
		this.currentPlace = {
			options: this.__options,
			container: this.__container
		};
		this.places = [];
		this.__elementToPlace = /* @__PURE__ */ new Map();
		try {
			const elementSource = resolveElement(element, this.options.shadowRoot || this.od);
			if (Jodit_1.isJoditAssigned(elementSource)) return elementSource.component;
		} catch (e) {
			this.destruct();
			throw e;
		}
		this.setStatus(STATUSES.beforeInit);
		this.id = attr(resolveElement(element, this.o.shadowRoot || this.od), "id") || (/* @__PURE__ */ new Date()).getTime().toString();
		instances[this.id] = this;
		this.attachEvents(options);
		this.e.on(this.ow, "resize", () => {
			if (this.e) this.e.fire("resize");
		});
		this.e.on("prepareWYSIWYGEditor", this.__prepareWYSIWYGEditor);
		this.selection = new Selection(this);
		callPromise(this.beforeInitHook(), () => {
			if (this.isInDestruct) return;
			this.e.fire("beforeInit", this);
			pluginSystem.__init(this);
			this.e.fire("afterPluginSystemInit", this);
			this.e.on("changePlace", () => {
				this.setReadOnly(this.o.readonly);
				this.setDisabled(this.o.disabled);
			});
			this.places.length = 0;
			const addPlaceResult = this.addPlace(element, options);
			instances[this.id] = this;
			const init = () => {
				if (this.isInDestruct) return;
				if (this.e) this.e.fire("afterInit", this);
				callPromise(this.afterInitHook());
				this.setStatus(STATUSES.ready);
				this.e.fire("afterConstructor", this);
			};
			callPromise(addPlaceResult, init);
		});
	}
	/**
	* Create and init current editable place
	*/
	addPlace(source, options) {
		const element = resolveElement(source, this.o.shadowRoot || this.od);
		this.attachEvents(options);
		if (element.attributes) toArray(element.attributes).forEach((attr) => {
			const name = attr.name;
			let value = attr.value;
			if (Config.defaultOptions[name] !== void 0 && (!options || options[name] === void 0)) {
				if (["readonly", "disabled"].indexOf(name) !== -1) value = value === "" || value === "true";
				if (/^[0-9]+(\.)?([0-9]+)?$/.test(value.toString())) value = Number(value);
				this.options[name] = value;
			}
		});
		let container = this.c.div("jodit-container");
		container.classList.add("jodit");
		container.classList.add("jodit-container");
		container.classList.add(`jodit_theme_${this.o.theme || "default"}`);
		addClassNames(this.o.className, container);
		if (this.o.containerStyle) css(container, this.o.containerStyle);
		const { styleValues } = this.o;
		Object.keys(styleValues).forEach((key) => {
			const property = kebabCase(key);
			container.style.setProperty(`--jd-${property}`, styleValues[key]);
		});
		attr(container, "contenteditable", false);
		let buffer = null;
		if (this.o.inline) {
			if (["TEXTAREA", "INPUT"].indexOf(element.nodeName) === -1) {
				container = element;
				attr(element, __defaultClassesKey, element.className.toString());
				buffer = container.innerHTML;
				Dom.detach(container);
			}
			container.classList.add("jodit_inline");
			container.classList.add("jodit-container");
		}
		if (element !== container) {
			const display = cssInline(element, "display");
			if (display) attr(element, __defaultStyleDisplayKey, display);
			css(element, "display", "none");
		}
		const SLOT = "workplace-slot";
		const aboveSlot = this.c.div(this.getFullElName(SLOT, "above"), NOEDIT);
		attr(aboveSlot, "data-jodit-above-toolbar", "");
		Dom.appendChildFirst(container, aboveSlot);
		const topSlot = this.c.div(this.getFullElName(SLOT, "top"), NOEDIT);
		Dom.append(container, topSlot);
		const centerSlot = this.c.div(this.getFullElName(SLOT, "center"), NOEDIT);
		const workplace = this.c.div("jodit-workplace", NOEDIT);
		const leftSlot = this.c.div(this.getFullElName(SLOT, "left"), NOEDIT);
		const rightSlot = this.c.div(this.getFullElName(SLOT, "right"), NOEDIT);
		Dom.append(centerSlot, leftSlot);
		Dom.append(centerSlot, workplace);
		Dom.append(centerSlot, rightSlot);
		Dom.append(container, centerSlot);
		const bottomPanel = this.c.div(this.getFullElName(SLOT, "bottom"), NOEDIT);
		Dom.append(container, bottomPanel);
		if (element.parentNode && element !== container) Dom.before(element, container);
		Object.defineProperty(element, "component", {
			enumerable: false,
			configurable: true,
			value: this
		});
		const editor = this.c.div("jodit-wysiwyg", {
			contenteditable: true,
			"aria-disabled": false,
			tabindex: this.o.tabIndex
		});
		Dom.append(workplace, editor);
		const currentPlace = {
			editor,
			element,
			container,
			workplace,
			slots: {
				above: aboveSlot,
				top: topSlot,
				bottom: bottomPanel,
				center: centerSlot,
				left: leftSlot,
				right: rightSlot
			},
			statusbar: new StatusBar(this, container),
			options: this.isReady ? ConfigProto(options || {}, Config.defaultOptions) : this.options,
			history: new History(this),
			editorWindow: this.ow
		};
		this.__elementToPlace.set(editor, currentPlace);
		this.setCurrentPlace(currentPlace);
		this.places.push(currentPlace);
		this.setNativeEditorValue(this.getElementValue());
		const initResult = this.__initEditor(buffer);
		const opt = this.options;
		const init = () => {
			if (opt.enableDragAndDropFileToEditor && opt.uploader && (opt.uploader.url || opt.uploader.insertImageAsBase64URI)) this.uploader.bind(this.editor);
			else if (!opt.enableDragAndDropFileToEditor) this.e.on(editor, "drop", (e) => {
				var _a, _b;
				if ((_b = (_a = e.dataTransfer) === null || _a === void 0 ? void 0 : _a.files) === null || _b === void 0 ? void 0 : _b.length) e.preventDefault();
			});
			if (!this.__elementToPlace.get(this.editor)) this.__elementToPlace.set(this.editor, currentPlace);
			this.e.fire("afterAddPlace", currentPlace);
		};
		return callPromise(initResult, init);
	}
	addDisclaimer(elm) {
		Dom.append(this.workplace, elm);
	}
	/**
	* Set current place object
	*/
	setCurrentPlace(place) {
		if (this.currentPlace === place) return;
		if (!this.isEditorMode()) this.setMode(1);
		this.currentPlace = place;
		this.buildToolbar();
		if (this.isReady) this.e.fire("changePlace", place);
	}
	__initEditor(buffer) {
		return callPromise(this.__createEditor(), () => {
			if (this.isInDestruct) return;
			if (this.element !== this.container) {
				const value = this.getElementValue();
				if (value !== this.getEditorValue()) this.setEditorValue(value);
			} else buffer != null && this.setEditorValue(buffer);
			let mode = this.o.defaultMode;
			if (this.o.saveModeInStorage) {
				const localMode = this.storage.get("jodit_default_mode");
				if (typeof localMode === "string") mode = parseInt(localMode, 10);
			}
			this.setMode(mode);
			if (this.o.readonly) {
				this.__wasReadOnly = false;
				this.setReadOnly(true);
			}
			if (this.o.disabled) this.setDisabled(true);
			try {
				this.ed.execCommand("defaultParagraphSeparator", false, this.o.enter.toLowerCase());
			} catch (_a) {}
		});
	}
	/**
	* Create main DIV element and replace source textarea
	*/
	__createEditor() {
		const defaultEditorArea = this.editor;
		const stayDefault = this.e.fire("createEditor", this);
		return callPromise(stayDefault, () => {
			if (this.isInDestruct) return;
			if (stayDefault === false || isPromise(stayDefault)) Dom.safeRemove(defaultEditorArea);
			addClassNames(this.o.editorClassName, this.editor);
			if (this.o.style) css(this.editor, this.o.style);
			this.e.on("synchro", () => {
				this.setEditorValue();
			}).on("focus", () => {
				this.editorIsActive = true;
			}).on("blur", () => this.editorIsActive = false);
			this.__prepareWYSIWYGEditor();
			if (this.o.triggerChangeEvent) this.e.on("change", this.async.debounce(() => {
				this.e && this.e.fire(this.element, "change");
			}, this.defaultTimeout));
		});
	}
	/**
	* Attach some native event listeners
	*/
	__prepareWYSIWYGEditor() {
		const { editor } = this;
		if (this.o.direction) {
			const direction = this.o.direction.toLowerCase() === "rtl" ? "rtl" : "ltr";
			css(this.editor, "direction", direction);
			attr(this.editor, "dir", direction);
			css(this.container, "direction", direction);
			attr(this.container, "dir", direction);
			this.toolbar.setDirection(direction);
		}
		this.e.on(editor, "mousedown touchstart focus", () => {
			const place = this.__elementToPlace.get(editor);
			if (place) this.setCurrentPlace(place);
		}).on(editor, "compositionend", this.synchronizeValues).on(editor, "selectionchange selectionstart keydown keyup input keypress dblclick mousedown mouseup click copy cut dragstart drop dragover paste resize touchstart touchend focus blur", (event) => {
			if (this.o.readonly || this.__isSilentChange) return;
			if (event instanceof this.ew.KeyboardEvent && event.isComposing) return;
			if (this.e && this.e.fire) {
				if (this.e.fire(event.type, event) === false) return false;
				this.synchronizeValues();
			}
		}).on(this.ow, "mouseup", (event) => {
			if (this.o.readonly || this.__isSilentChange) return;
			const target = event.target;
			if (Boolean(target && isNumber(target.nodeType) && editor.contains(target)) || !this.s.isInsideArea) return;
			this.e.fire("changeSelection");
			this.synchronizeValues();
		});
	}
	fetch(url, options) {
		const ajax = new Ajax({
			url,
			...options
		}, this.o.defaultAjaxOptions);
		const destroy = () => {
			this.e.off("beforeDestruct", destroy);
			this.progressbar.progress(100).hide();
			ajax.destruct();
		};
		this.e.one("beforeDestruct", destroy);
		this.progressbar.show().progress(30);
		const promise = ajax.send();
		promise.finally(destroy).catch(() => null);
		return promise;
	}
	/**
	* Jodit's Destructor. Remove editor, and return source input
	*/
	destruct() {
		var _a, _b;
		if (this.isInDestruct) return;
		this.setStatus(STATUSES.beforeDestruct);
		this.__elementToPlace.clear();
		(_a = cached(this, "storage")) === null || _a === void 0 || _a.clear();
		(_b = cached(this, "buffer")) === null || _b === void 0 || _b.clear();
		this.commands.clear();
		this.__selectionLocked = null;
		this.e.off(this.ow, "resize");
		this.e.off(this.ow);
		this.e.off(this.od);
		this.e.off(this.od.body);
		const tmpValue = this.editor ? this.getEditorValue() : "";
		this.places.forEach(({ container, workplace, statusbar, element, iframe, editor, history }) => {
			if (!element) return;
			if (element !== container) if (element.hasAttribute(__defaultStyleDisplayKey)) {
				const display = attr(element, __defaultStyleDisplayKey);
				if (display) {
					css(element, "display", display);
					attr(element, __defaultStyleDisplayKey, null);
				}
			} else css(element, "display", "");
			else if (element.hasAttribute(__defaultClassesKey)) {
				element.className = attr(element, __defaultClassesKey) || "";
				attr(element, __defaultClassesKey, null);
			}
			if (element.hasAttribute("style") && !attr(element, "style")) attr(element, "style", null);
			statusbar.destruct();
			this.e.off(container);
			this.e.off(element);
			this.e.off(editor);
			Dom.safeRemove(workplace);
			Dom.safeRemove(editor);
			if (container !== element) Dom.safeRemove(container);
			Object.defineProperty(element, "component", {
				enumerable: false,
				configurable: true,
				value: null
			});
			Dom.safeRemove(iframe);
			if (container === element) element.innerHTML = tmpValue;
			history.destruct();
		});
		this.places.length = 0;
		this.currentPlace = {};
		delete instances[this.id];
		super.destruct();
	}
};
Jodit$1.fatMode = true;
Jodit$1.plugins = pluginSystem;
Jodit$1.modules = modules;
Jodit$1.ns = modules;
Jodit$1.decorators = {};
Jodit$1.constants = constants_exports;
Jodit$1.instances = instances;
Jodit$1.lang = lang;
Jodit$1.core = { Plugin };
__decorate$8([cache], Jodit$1.prototype, "createInside", null);
__decorate$8([cache], Jodit$1.prototype, "message", null);
__decorate$8([cache], Jodit$1.prototype, "s", null);
__decorate$8([cache], Jodit$1.prototype, "uploader", null);
__decorate$8([cache], Jodit$1.prototype, "filebrowser", null);
__decorate$8([throttle()], Jodit$1.prototype, "synchronizeValues", null);
__decorate$8([watch(":internalChange")], Jodit$1.prototype, "updateElementValue", null);
__decorate$8([autobind], Jodit$1.prototype, "__prepareWYSIWYGEditor", null);
Jodit$1 = Jodit_1 = __decorate$8([derive(Dlgs)], Jodit$1);
function addClassNames(className, elm) {
	if (className) className.split(/\s+/).forEach((cn) => elm.classList.add(cn));
}
//#endregion
//#region node_modules/jodit/esm/styles/icons/angle-down.svg.js
var angle_down_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"> <path d=\"M1395 736q0 13-10 23l-466 466q-10 10-23 10t-23-10l-466-466q-10-10-10-23t10-23l50-50q10-10 23-10t23 10l393 393 393-393q10-10 23-10t23 10l50 50q10 10 10 23z\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/styles/icons/angle-left.svg.js
var angle_left_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"> <path d=\"M1203 544q0 13-10 23l-393 393 393 393q10 10 10 23t-10 23l-50 50q-10 10-23 10t-23-10l-466-466q-10-10-10-23t10-23l466-466q10-10 23-10t23 10l50 50q10 10 10 23z\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/styles/icons/angle-right.svg.js
var angle_right_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"> <path d=\"M1171 960q0 13-10 23l-466 466q-10 10-23 10t-23-10l-50-50q-10-10-10-23t10-23l393-393-393-393q-10-10-10-23t10-23l50-50q10-10 23-10t23 10l466 466q10 10 10 23z\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/styles/icons/angle-up.svg.js
var angle_up_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"> <path d=\"M1395 1184q0 13-10 23l-50 50q-10 10-23 10t-23-10l-393-393-393 393q-10 10-23 10t-23-10l-50-50q-10-10-10-23t10-23l466-466q10-10 23-10t23 10l466 466q10 10 10 23z\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/styles/icons/bin.svg.js
var bin_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"> <path d=\"M704 1376v-704q0-14-9-23t-23-9h-64q-14 0-23 9t-9 23v704q0 14 9 23t23 9h64q14 0 23-9t9-23zm256 0v-704q0-14-9-23t-23-9h-64q-14 0-23 9t-9 23v704q0 14 9 23t23 9h64q14 0 23-9t9-23zm256 0v-704q0-14-9-23t-23-9h-64q-14 0-23 9t-9 23v704q0 14 9 23t23 9h64q14 0 23-9t9-23zm-544-992h448l-48-117q-7-9-17-11h-317q-10 2-17 11zm928 32v64q0 14-9 23t-23 9h-96v948q0 83-47 143.5t-113 60.5h-832q-66 0-113-58.5t-47-141.5v-952h-96q-14 0-23-9t-9-23v-64q0-14 9-23t23-9h309l70-167q15-37 54-63t79-26h320q40 0 79 26t54 63l70 167h309q14 0 23 9t9 23z\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/styles/icons/cancel.svg.js
var cancel_svg_default = "<svg viewBox=\"0 0 14 14\" xmlns=\"http://www.w3.org/2000/svg\"> <g stroke=\"none\" stroke-width=\"1\"> <path d=\"M14,1.4 L12.6,0 L7,5.6 L1.4,0 L0,1.4 L5.6,7 L0,12.6 L1.4,14 L7,8.4 L12.6,14 L14,12.6 L8.4,7 L14,1.4 Z\"/> </g> </svg> ";
//#endregion
//#region node_modules/jodit/esm/styles/icons/center.svg.js
var center_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"> <path d=\"M1792 1344v128q0 26-19 45t-45 19h-1664q-26 0-45-19t-19-45v-128q0-26 19-45t45-19h1664q26 0 45 19t19 45zm-384-384v128q0 26-19 45t-45 19h-896q-26 0-45-19t-19-45v-128q0-26 19-45t45-19h896q26 0 45 19t19 45zm256-384v128q0 26-19 45t-45 19h-1408q-26 0-45-19t-19-45v-128q0-26 19-45t45-19h1408q26 0 45 19t19 45zm-384-384v128q0 26-19 45t-45 19h-640q-26 0-45-19t-19-45v-128q0-26 19-45t45-19h640q26 0 45 19t19 45z\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/styles/icons/check.svg.js
var check_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"> <path d=\"M1472 930v318q0 119-84.5 203.5t-203.5 84.5h-832q-119 0-203.5-84.5t-84.5-203.5v-832q0-119 84.5-203.5t203.5-84.5h832q63 0 117 25 15 7 18 23 3 17-9 29l-49 49q-10 10-23 10-3 0-9-2-23-6-45-6h-832q-66 0-113 47t-47 113v832q0 66 47 113t113 47h832q66 0 113-47t47-113v-254q0-13 9-22l64-64q10-10 23-10 6 0 12 3 20 8 20 29zm231-489l-814 814q-24 24-57 24t-57-24l-430-430q-24-24-24-57t24-57l110-110q24-24 57-24t57 24l263 263 647-647q24-24 57-24t57 24l110 110q24 24 24 57t-24 57z\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/styles/icons/chevron.svg.js
var chevron_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 10 10\"> <path d=\"M.941 4.523a.75.75 0 1 1 1.06-1.06l3.006 3.005 3.005-3.005a.75.75 0 1 1 1.06 1.06l-3.549 3.55a.75.75 0 0 1-1.168-.136L.941 4.523z\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/styles/icons/dots.svg.js
var dots_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 24 24\" > <circle cx=\"12\" cy=\"12\" r=\"2.2\"/> <circle cx=\"12\" cy=\"5\" r=\"2.2\"/> <circle cx=\"12\" cy=\"19\" r=\"2.2\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/styles/icons/eye.svg.js
var eye_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"> <path d=\"M1664 960q-152-236-381-353 61 104 61 225 0 185-131.5 316.5t-316.5 131.5-316.5-131.5-131.5-316.5q0-121 61-225-229 117-381 353 133 205 333.5 326.5t434.5 121.5 434.5-121.5 333.5-326.5zm-720-384q0-20-14-34t-34-14q-125 0-214.5 89.5t-89.5 214.5q0 20 14 34t34 14 34-14 14-34q0-86 61-147t147-61q20 0 34-14t14-34zm848 384q0 34-20 69-140 230-376.5 368.5t-499.5 138.5-499.5-139-376.5-368q-20-35-20-69t20-69q140-229 376.5-368t499.5-139 499.5 139 376.5 368q20 35 20 69z\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/styles/icons/file.svg.js
var file_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"> <path d=\"M1152 512v-472q22 14 36 28l408 408q14 14 28 36h-472zm-128 32q0 40 28 68t68 28h544v1056q0 40-28 68t-68 28h-1344q-40 0-68-28t-28-68v-1600q0-40 28-68t68-28h800v544z\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/styles/icons/folder.svg.js
var folder_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"> <path d=\"M1728 608v704q0 92-66 158t-158 66h-1216q-92 0-158-66t-66-158v-960q0-92 66-158t158-66h320q92 0 158 66t66 158v32h672q92 0 158 66t66 158z\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/styles/icons/info-circle.svg.js
var info_circle_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"> <path d=\"M1152 1376v-160q0-14-9-23t-23-9h-96v-512q0-14-9-23t-23-9h-320q-14 0-23 9t-9 23v160q0 14 9 23t23 9h96v320h-96q-14 0-23 9t-9 23v160q0 14 9 23t23 9h448q14 0 23-9t9-23zm-128-896v-160q0-14-9-23t-23-9h-192q-14 0-23 9t-9 23v160q0 14 9 23t23 9h192q14 0 23-9t9-23zm640 416q0 209-103 385.5t-279.5 279.5-385.5 103-385.5-103-279.5-279.5-103-385.5 103-385.5 279.5-279.5 385.5-103 385.5 103 279.5 279.5 103 385.5z\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/styles/icons/left.svg.js
var left_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"> <path d=\"M1792 1344v128q0 26-19 45t-45 19h-1664q-26 0-45-19t-19-45v-128q0-26 19-45t45-19h1664q26 0 45 19t19 45zm-384-384v128q0 26-19 45t-45 19h-1280q-26 0-45-19t-19-45v-128q0-26 19-45t45-19h1280q26 0 45 19t19 45zm256-384v128q0 26-19 45t-45 19h-1536q-26 0-45-19t-19-45v-128q0-26 19-45t45-19h1536q26 0 45 19t19 45zm-384-384v128q0 26-19 45t-45 19h-1152q-26 0-45-19t-19-45v-128q0-26 19-45t45-19h1152q26 0 45 19t19 45z\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/styles/icons/lock.svg.js
var lock_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"><path d=\"M640 768h512v-192q0-106-75-181t-181-75-181 75-75 181v192zm832 96v576q0 40-28 68t-68 28h-960q-40 0-68-28t-28-68v-576q0-40 28-68t68-28h32v-192q0-184 132-316t316-132 316 132 132 316v192h32q40 0 68 28t28 68z\"/></svg> ";
//#endregion
//#region node_modules/jodit/esm/styles/icons/ok.svg.js
var ok_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 32 32\"> <path d=\"M27 4l-15 15-7-7-5 5 12 12 20-20z\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/styles/icons/pencil.svg.js
var pencil_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"><path d=\"M491 1536l91-91-235-235-91 91v107h128v128h107zm523-928q0-22-22-22-10 0-17 7l-542 542q-7 7-7 17 0 22 22 22 10 0 17-7l542-542q7-7 7-17zm-54-192l416 416-832 832h-416v-416zm683 96q0 53-37 90l-166 166-416-416 166-165q36-38 90-38 53 0 91 38l235 234q37 39 37 91z\"/></svg> ";
//#endregion
//#region node_modules/jodit/esm/styles/icons/plus.svg.js
var plus_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"><path d=\"M1600 736v192q0 40-28 68t-68 28h-416v416q0 40-28 68t-68 28h-192q-40 0-68-28t-28-68v-416h-416q-40 0-68-28t-28-68v-192q0-40 28-68t68-28h416v-416q0-40 28-68t68-28h192q40 0 68 28t28 68v416h416q40 0 68 28t28 68z\"/></svg> ";
//#endregion
//#region node_modules/jodit/esm/styles/icons/resize-handler.svg.js
var resize_handler_svg_default = "<svg viewBox=\"0 0 13 13\" xmlns=\"http://www.w3.org/2000/svg\"> <path d=\"M5.9814 11.8049C5.59087 11.4144 5.59087 10.7812 5.9814 10.3907L10.224 6.14806C10.6146 5.75754 11.2477 5.75754 11.6383 6.14806C12.0288 6.53859 12.0288 7.17175 11.6383 7.56228L7.39561 11.8049C7.00509 12.1954 6.37192 12.1954 5.9814 11.8049Z\"/> <path d=\"M0.707107 12.0208C0.316582 11.6303 0.316582 10.9971 0.707107 10.6066L10.6066 0.707121C10.9971 0.316597 11.6303 0.316596 12.0208 0.707121C12.4113 1.09764 12.4113 1.73081 12.0208 2.12133L2.12132 12.0208C1.7308 12.4114 1.09763 12.4114 0.707107 12.0208Z\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/styles/icons/right.svg.js
var right_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"> <path d=\"M1792 1344v128q0 26-19 45t-45 19h-1664q-26 0-45-19t-19-45v-128q0-26 19-45t45-19h1664q26 0 45 19t19 45zm0-384v128q0 26-19 45t-45 19h-1280q-26 0-45-19t-19-45v-128q0-26 19-45t45-19h1280q26 0 45 19t19 45zm0-384v128q0 26-19 45t-45 19h-1536q-26 0-45-19t-19-45v-128q0-26 19-45t45-19h1536q26 0 45 19t19 45zm0-384v128q0 26-19 45t-45 19h-1152q-26 0-45-19t-19-45v-128q0-26 19-45t45-19h1152q26 0 45 19t19 45z\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/styles/icons/save.svg.js
var save_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"> <path d=\"M512 1536h768v-384h-768v384zm896 0h128v-896q0-14-10-38.5t-20-34.5l-281-281q-10-10-34-20t-39-10v416q0 40-28 68t-68 28h-576q-40 0-68-28t-28-68v-416h-128v1280h128v-416q0-40 28-68t68-28h832q40 0 68 28t28 68v416zm-384-928v-320q0-13-9.5-22.5t-22.5-9.5h-192q-13 0-22.5 9.5t-9.5 22.5v320q0 13 9.5 22.5t22.5 9.5h192q13 0 22.5-9.5t9.5-22.5zm640 32v928q0 40-28 68t-68 28h-1344q-40 0-68-28t-28-68v-1344q0-40 28-68t68-28h928q40 0 88 20t76 48l280 280q28 28 48 76t20 88z\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/styles/icons/settings.svg.js
var settings_svg_default = "<svg viewBox=\"0 0 48 48\" xmlns=\"http://www.w3.org/2000/svg\"> <path stroke=\"null\" d=\"m42.276011,26.302547c0.098397,-0.76605 0.172194,-1.54407 0.172194,-2.33406s-0.073797,-1.56801 -0.172194,-2.33406l5.202718,-3.961917c0.467384,-0.359086 0.602679,-1.005441 0.29519,-1.532101l-4.919828,-8.29489c-0.307489,-0.51469 -0.947067,-0.730142 -1.500548,-0.51469l-6.125186,2.405877c-1.266856,-0.945594 -2.656707,-1.747553 -4.157255,-2.357999l-0.922468,-6.343855c-0.110696,-0.562568 -0.614979,-1.005441 -1.229957,-1.005441l-9.839656,0c-0.614979,0 -1.119261,0.442873 -1.217657,1.005441l-0.922468,6.343855c-1.500548,0.610446 -2.890399,1.400436 -4.157255,2.357999l-6.125186,-2.405877c-0.553481,-0.203482 -1.193058,0 -1.500548,0.51469l-4.919828,8.29489c-0.307489,0.51469 -0.172194,1.161045 0.29519,1.532101l5.190419,3.961917c-0.098397,0.76605 -0.172194,1.54407 -0.172194,2.33406s0.073797,1.56801 0.172194,2.33406l-5.190419,3.961917c-0.467384,0.359086 -0.602679,1.005441 -0.29519,1.532101l4.919828,8.29489c0.307489,0.51469 0.947067,0.730142 1.500548,0.51469l6.125186,-2.405877c1.266856,0.945594 2.656707,1.747553 4.157255,2.357999l0.922468,6.343855c0.098397,0.562568 0.602679,1.005441 1.217657,1.005441l9.839656,0c0.614979,0 1.119261,-0.442873 1.217657,-1.005441l0.922468,-6.343855c1.500548,-0.610446 2.890399,-1.400436 4.157255,-2.357999l6.125186,2.405877c0.553481,0.203482 1.193058,0 1.500548,-0.51469l4.919828,-8.29489c0.307489,-0.51469 0.172194,-1.161045 -0.29519,-1.532101l-5.190419,-3.961917zm-18.277162,6.044617c-4.759934,0 -8.609699,-3.746465 -8.609699,-8.378677s3.849766,-8.378677 8.609699,-8.378677s8.609699,3.746465 8.609699,8.378677s-3.849766,8.378677 -8.609699,8.378677z\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/styles/icons/unlock.svg.js
var unlock_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"> <path d=\"M1728 576v256q0 26-19 45t-45 19h-64q-26 0-45-19t-19-45v-256q0-106-75-181t-181-75-181 75-75 181v192h96q40 0 68 28t28 68v576q0 40-28 68t-68 28h-960q-40 0-68-28t-28-68v-576q0-40 28-68t68-28h672v-192q0-185 131.5-316.5t316.5-131.5 316.5 131.5 131.5 316.5z\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/styles/icons/update.svg.js
var update_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"> <path d=\"M1639 1056q0 5-1 7-64 268-268 434.5t-478 166.5q-146 0-282.5-55t-243.5-157l-129 129q-19 19-45 19t-45-19-19-45v-448q0-26 19-45t45-19h448q26 0 45 19t19 45-19 45l-137 137q71 66 161 102t187 36q134 0 250-65t186-179q11-17 53-117 8-23 30-23h192q13 0 22.5 9.5t9.5 22.5zm25-800v448q0 26-19 45t-45 19h-448q-26 0-45-19t-19-45 19-45l138-138q-148-137-349-137-134 0-250 65t-186 179q-11 17-53 117-8 23-30 23h-199q-13 0-22.5-9.5t-9.5-22.5v-7q65-268 270-434.5t480-166.5q146 0 284 55.5t245 156.5l130-129q19-19 45-19t45 19 19 45z\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/styles/icons/upload.svg.js
var upload_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"> <path d=\"M1344 1472q0-26-19-45t-45-19-45 19-19 45 19 45 45 19 45-19 19-45zm256 0q0-26-19-45t-45-19-45 19-19 45 19 45 45 19 45-19 19-45zm128-224v320q0 40-28 68t-68 28h-1472q-40 0-68-28t-28-68v-320q0-40 28-68t68-28h427q21 56 70.5 92t110.5 36h256q61 0 110.5-36t70.5-92h427q40 0 68 28t28 68zm-325-648q-17 40-59 40h-256v448q0 26-19 45t-45 19h-256q-26 0-45-19t-19-45v-448h-256q-42 0-59-40-17-39 14-69l448-448q18-19 45-19t45 19l448 448q31 30 14 69z\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/styles/icons/valign.svg.js
var valign_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"> <path d=\"M1216 320q0 26-19 45t-45 19h-128v1024h128q26 0 45 19t19 45-19 45l-256 256q-19 19-45 19t-45-19l-256-256q-19-19-19-45t19-45 45-19h128v-1024h-128q-26 0-45-19t-19-45 19-45l256-256q19-19 45-19t45 19l256 256q19 19 19 45z\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/styles/icons/index.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* [[include:styles/icons/README.md]]
* @packageDocumentation
* @module icons
*/
var icons_exports = /* @__PURE__ */ __exportAll({
	angle_down: () => angle_down_svg_default,
	angle_left: () => angle_left_svg_default,
	angle_right: () => angle_right_svg_default,
	angle_up: () => angle_up_svg_default,
	bin: () => bin_svg_default,
	cancel: () => cancel_svg_default,
	center: () => center_svg_default,
	check: () => check_svg_default,
	chevron: () => chevron_svg_default,
	dots: () => dots_svg_default,
	eye: () => eye_svg_default,
	file: () => file_svg_default,
	folder: () => folder_svg_default,
	info_circle: () => info_circle_svg_default,
	left: () => left_svg_default,
	lock: () => lock_svg_default,
	ok: () => ok_svg_default,
	pencil: () => pencil_svg_default,
	plus: () => plus_svg_default,
	resize_handler: () => resize_handler_svg_default,
	right: () => right_svg_default,
	save: () => save_svg_default,
	settings: () => settings_svg_default,
	unlock: () => unlock_svg_default,
	update: () => update_svg_default,
	upload: () => upload_svg_default,
	valign: () => valign_svg_default
});
//#endregion
//#region node_modules/jodit/esm/langs/ar.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var ar_default = {
	"Type something": "إبدأ في الكتابة...",
	"About Jodit": "حول جوديت",
	"Jodit Editor": "محرر جوديت",
	"Jodit User's Guide": "دليل مستخدم جوديت",
	"contains detailed help for using": "يحتوي على مساعدة مفصلة للاستخدام",
	"For information about the license, please go to our website:": "للحصول على معلومات حول الترخيص، يرجى الذهاب لموقعنا:",
	"Buy full version": "شراء النسخة الكاملة",
	"Copyright © XDSoft.net - Chupurnov Valeriy. All rights reserved.": "حقوق الطبع والنشر © XDSoft.net - Chupurnov Valeriy. كل الحقوق محفوظة.",
	Anchor: "مِرْساة",
	"Open in new tab": "فتح في نافذة جديدة",
	"Open in fullsize": "فتح المحرر في الحجم الكامل",
	"Clear Formatting": "مسح التنسيق",
	"Fill color or set the text color": "ملء اللون أو تعيين لون النص",
	Redo: "إعادة",
	Undo: "تراجع",
	Bold: "عريض",
	Italic: "مائل",
	"Insert Unordered List": "إدراج قائمة غير مرتبة",
	"Insert Ordered List": "إدراج قائمة مرتبة",
	"Align Center": "محاذاة للوسط",
	"Align Justify": "محاذاة مثبتة",
	"Align Left": "محاذاة لليسار",
	"Align Right": "محاذاة لليمين",
	"Insert Horizontal Line": "إدراج خط أفقي",
	"Insert Image": "إدراج صورة",
	"Insert file": "ادخال الملف",
	"Insert youtube/vimeo video": "إدراج فيديو يوتيوب/فيميو ",
	"Insert link": "إدراج رابط",
	"Font size": "حجم الخط",
	"Font family": "نوع الخط",
	"Insert format block": "إدراج كتلة تنسيق",
	Normal: "عادي",
	"Heading 1": "عنوان 1",
	"Heading 2": "عنوان 2",
	"Heading 3": "عنوان 3",
	"Heading 4": "عنوان 4",
	Quote: "إقتباس",
	Code: "كود",
	Insert: "إدراج",
	"Insert table": "إدراج جدول",
	"Decrease Indent": "تقليل المسافة البادئة",
	"Increase Indent": "زيادة المسافة البادئة",
	"Select Special Character": "تحديد أحرف خاصة",
	"Insert Special Character": "إدراج حرف خاص",
	"Paint format": "تنسيق الرسم",
	"Change mode": "تغيير الوضع",
	Margins: "هوامش",
	top: "أعلى",
	right: "يمين",
	bottom: "أسفل",
	left: "يسار",
	Styles: "الأنماط",
	Classes: "الطبقات",
	Align: "محاذاة",
	Right: "اليمين",
	Center: "الوسط",
	Left: "اليسار",
	"--Not Set--": "--غير مضبوط--",
	Src: "Src",
	Title: "العنوان",
	Alternative: "العنوان البديل",
	Link: "الرابط",
	"Open link in new tab": "افتح الرابط في نافذة جديدة",
	Image: "الصورة",
	file: "ملف",
	Advanced: "متقدم",
	"Image properties": "خصائص الصورة",
	Cancel: "إلغاء",
	Ok: "حسنا",
	"File Browser": "متصفح الملفات",
	"Error on load list": "حدث خطأ في تحميل القائمة ",
	"Error on load folders": "حدث خطأ في تحميل المجلدات",
	"Are you sure?": "هل أنت واثق؟",
	"Enter Directory name": "أدخل اسم المجلد",
	"Create directory": "إنشاء مجلد",
	"type name": "أكتب إسم",
	"type dir name": "أكتب إسم",
	"Drop image": "إسقاط صورة",
	"Drop file": "إسقاط الملف",
	"or click": "أو أنقر",
	"Alternative text": "النص البديل",
	Upload: "رفع",
	Browse: "تصفح",
	Background: "الخلفية",
	Text: "نص",
	Top: "أعلى",
	Middle: "الوسط",
	Bottom: "الأسفل",
	"Insert column before": "إدراج عمود قبل",
	"Insert column after": "إدراج عمود بعد",
	"Insert row above": "إدراج صف أعلى",
	"Insert row below": "إدراج صف أسفل",
	"Delete table": "حذف الجدول",
	"Delete row": "حذف الصف",
	"Delete column": "حذف العمود",
	"Empty cell": "خلية فارغة",
	"Chars: %d": "%d حرف",
	"Words: %d": "%d كلام",
	"Strike through": "اضرب من خلال",
	Underline: "أكد",
	superscript: "حرف فوقي",
	subscript: "مخطوطة",
	"Cut selection": "قطع الاختيار",
	"Select all": "اختر الكل",
	Break: "استراحة",
	"Search for": "البحث عن",
	"Replace with": "استبدل ب",
	Replace: "محل",
	Paste: "معجون",
	"Choose Content to Paste": "اختر محتوى للصق",
	source: "مصدر",
	bold: "بالخط العريض",
	italic: "مائل",
	brush: "شغل",
	link: "صلة",
	undo: "إلغاء",
	redo: "كرر",
	table: "طاولة",
	image: "صورة",
	eraser: "نظيف",
	paragraph: "فقرة",
	fontsize: "حجم الخط",
	video: "فيديو",
	font: "الخط",
	about: "حول المحرر",
	print: "طباعة",
	underline: "أكد",
	strikethrough: "شطب",
	indent: "المسافة البادئة",
	outdent: "نتوء",
	fullsize: "ملء الشاشة",
	shrink: "الحجم التقليدي",
	hr: "الخط",
	ul: "قائمة",
	ol: "قائمة مرقمة",
	cut: "قطع",
	selectall: "اختر الكل",
	"Embed code": "قانون",
	"Open link": "فتح الرابط",
	"Edit link": "تعديل الرابط",
	"No follow": "سمة Nofollow",
	Unlink: "إزالة الرابط",
	Update: "تحديث",
	pencil: "لتحرير",
	Eye: "مراجعة",
	" URL": "URL",
	Edit: "تحرير",
	"Horizontal align": "محاذاة أفقية",
	Filter: "فلتر",
	"Sort by changed": "عن طريق التغيير",
	"Sort by name": "بالاسم",
	"Sort by size": "حسب الحجم",
	"Add folder": "إضافة مجلد",
	Reset: "إعادة",
	Save: "احتفظ",
	"Save as ...": "حفظ باسم",
	Resize: "تغيير الحجم",
	Crop: "حجم القطع",
	Width: "عرض",
	Height: "ارتفاع",
	"Keep Aspect Ratio": "حافظ على النسب",
	Yes: "أن",
	No: "لا",
	Remove: "حذف",
	Select: "تميز",
	"Select %s": "تميز %s",
	"Vertical align": "محاذاة عمودية",
	Split: "انشق، مزق",
	Merge: "اذهب",
	"Add column": "أضف العمود",
	"Add row": "اضف سطر",
	"License: %s": "رخصة %s",
	Delete: "حذف",
	"Split vertical": "انقسام عمودي",
	"Split horizontal": "تقسيم أفقي",
	Border: "الحدود",
	"Your code is similar to HTML. Keep as HTML?": "يشبه الكود الخاص بك HTML. تبقي كما HTML؟",
	"Paste as HTML": "الصق ك HTML",
	Keep: "احتفظ",
	"Insert as Text": "إدراج كنص",
	"Insert only Text": "إدراج النص فقط",
	"You can only edit your own images. Download this image on the host?": "يمكنك فقط تحرير صورك الخاصة. تحميل هذه الصورة على المضيف؟",
	"The image has been successfully uploaded to the host!": "تم تحميل الصورة بنجاح على الخادم!",
	palette: "لوحة",
	"There are no files": "لا توجد ملفات في هذا الدليل.",
	Rename: "إعادة تسمية",
	"Enter new name": "أدخل اسم جديد",
	preview: "معاينة",
	download: "تحميل",
	"Paste from clipboard": "لصق من الحافظة",
	"Your browser doesn't support direct access to the clipboard.": "متصفحك لا يدعم إمكانية الوصول المباشر إلى الحافظة.",
	"Copy selection": "نسخ التحديد",
	copy: "نسخ",
	"Border radius": "دائرة نصف قطرها الحدود",
	"Show all": "عرض كل",
	Apply: "تطبيق",
	"Please fill out this field": "يرجى ملء هذا المجال",
	"Please enter a web address": "يرجى إدخال عنوان ويب",
	Default: "الافتراضي",
	Circle: "دائرة",
	Dot: "نقطة",
	Quadrate: "المربعة",
	Find: "البحث",
	"Find Previous": "تجد السابقة",
	"Find Next": "تجد التالي",
	"The pasted content is coming from a Microsoft Word/Excel document. Do you want to keep the format or clean it up?": "للصق المحتوى قادم من Microsoft Word/Excel الوثيقة. هل تريد أن تبقي شكل أو تنظيفه ؟ ",
	"Word Paste Detected": "كلمة لصق الكشف عن",
	Clean: "نظيفة",
	"Insert className": "أدخل اسم الفصل",
	"Press Alt for custom resizing": "اضغط البديل لتغيير حجم مخصص",
	"Line height": "ارتفاع الخط",
	spellcheck: "التدقيق الإملائي",
	"Speech Recognize": "التعرف على الكلام",
	All: "تحديد الكل"
};
//#endregion
//#region node_modules/jodit/esm/langs/cs_cz.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var cs_cz_default = {
	"Type something": "Napiš něco",
	"About Jodit": "O Jodit",
	"Jodit Editor": "Editor Jodit",
	"Free Non-commercial Version": "Verze pro nekomerční použití",
	"Jodit User's Guide": "Jodit Uživatelská příručka",
	"contains detailed help for using": "obsahuje detailní nápovědu",
	"For information about the license, please go to our website:": "Pro informace o licenci, prosím, přejděte na naši stránku:",
	"Buy full version": "Koupit plnou verzi",
	"Copyright © XDSoft.net - Chupurnov Valeriy. All rights reserved.": "Copyright © XDSoft.net - Chupurnov Valeriy. Všechna práva vyhrazena.",
	Anchor: "Anchor",
	"Open in new tab": "Otevřít v nové záložce",
	"Open in fullsize": "Otevřít v celoobrazovkovém režimu",
	"Clear Formatting": "Vyčistit formátování",
	"Fill color or set the text color": "Barva výplně a písma",
	Redo: "Vpřed",
	Undo: "Zpět",
	Bold: "Tučné",
	Italic: "Kurzíva",
	"Insert Unordered List": "Odrážky",
	"Insert Ordered List": "Číslovaný seznam",
	"Align Center": "Zarovnat na střed",
	"Align Justify": "Zarovnat do bloku",
	"Align Left": "Zarovnat vlevo",
	"Align Right": "Zarovnat vpravo",
	"Insert Horizontal Line": "Vložit horizontální linku",
	"Insert Image": "Vložit obrázek",
	"Insert file": "Vložit soubor",
	"Insert youtube/vimeo video": "Vložit video (YT/Vimeo)",
	"Insert link": "Vložit odkaz",
	"Font size": "Velikost písma",
	"Font family": "Typ písma",
	"Insert format block": "Formátovat blok",
	Normal: "Normální text",
	"Heading 1": "Nadpis 1",
	"Heading 2": "Nadpis 2",
	"Heading 3": "Nadpis 3",
	"Heading 4": "Nadpis 4",
	Quote: "Citát",
	Code: "Kód",
	Insert: "Vložit",
	"Insert table": "Vložit tabulku",
	"Decrease Indent": "Zmenšit odsazení",
	"Increase Indent": "Zvětšit odsazení",
	"Select Special Character": "Vybrat speciální symbol",
	"Insert Special Character": "Vložit speciální symbol",
	"Paint format": "Použít formát",
	"Change mode": "Změnit mód",
	Margins: "Okraje",
	top: "horní",
	right: "pravý",
	bottom: "spodní",
	left: "levý",
	Styles: "Styly",
	Classes: "Třídy",
	Align: "Zarovnání",
	Right: "Vpravo",
	Center: "Na střed",
	Left: "Vlevo",
	"--Not Set--": "--nenastaveno--",
	Src: "src",
	Title: "Titulek",
	Alternative: "Alternativní text (alt)",
	Link: "Link",
	"Open link in new tab": "Otevřít link v nové záložce",
	Image: "Obrázek",
	file: "soubor",
	Advanced: "Rozšířené",
	"Image properties": "Vlastnosti obrázku",
	Cancel: "Zpět",
	Ok: "Ok",
	"Your code is similar to HTML. Keep as HTML?": "Váš text se podobá HTML. Vložit ho jako HTML?",
	"Paste as HTML": "Vložit jako HTML",
	Keep: "Ponechat originál",
	Clean: "Vyčistit",
	"Insert as Text": "Vložit jako TEXT",
	"Insert only Text": "Vložit pouze TEXT",
	"Word Paste Detected": "Detekován fragment z Wordu nebo Excelu",
	"The pasted content is coming from a Microsoft Word/Excel document. Do you want to keep the format or clean it up?": "Obsah, který vkládáte, je pravděpodobně z Microsoft Word / Excel. Chcete ponechat formát nebo vložit pouze text?",
	"File Browser": "Prohlížeč souborů",
	"Error on load list": "Chyba při načítání seznamu souborů",
	"Error on load folders": "Chyba při načítání složek",
	"Are you sure?": "Jste si jistý(á)?",
	"Enter Directory name": "Název složky",
	"Create directory": "Vytvořit složku",
	"type name": "název",
	"type dir name": "název",
	"Drop image": "Přetáhněte sem obrázek",
	"Drop file": "Přetáhněte sem soubor",
	"or click": "nebo klikněte",
	"Alternative text": "Alternativní text",
	Browse: "Server",
	Upload: "Nahrát",
	Background: "Pozadí",
	Text: "Text",
	Top: "Nahoru",
	Middle: "Na střed",
	Bottom: "Dolu",
	"Insert column before": "Vložit sloupec před",
	"Insert column after": "Vložit sloupec za",
	"Insert row above": "Vložit řádek nad",
	"Insert row below": "Vložit řádek pod",
	"Delete table": "Vymazat tabulku",
	"Delete row": "Vymazat řádku",
	"Delete column": "Vymazat sloupec",
	"Empty cell": "Vyčistit buňku",
	source: "HTML",
	bold: "tučně",
	italic: "kurzíva",
	brush: "štětec",
	link: "odkaz",
	undo: "zpět",
	redo: "vpřed",
	table: "tabulka",
	image: "obrázek",
	eraser: "guma",
	paragraph: "odstavec",
	fontsize: "velikost písma",
	video: "video",
	font: "písmo",
	about: "о editoru",
	print: "tisk",
	underline: "podtrženo",
	strikethrough: "přeškrtnuto",
	indent: "zvětšit odsazení",
	outdent: "zmenšit odsazení",
	fullsize: "celoobrazovkový režim",
	shrink: "smrsknout",
	hr: "Linka",
	ul: "Odrážka",
	ol: "Číslovaný seznam",
	cut: "Vyjmout",
	selectall: "Označit vše",
	"Embed code": "Kód",
	"Open link": "Otevřít odkaz",
	"Edit link": "Upravit odkaz",
	"No follow": "Atribut no-follow",
	Unlink: "Odstranit odkaz",
	Eye: "Zobrazit",
	pencil: "Chcete-li upravit",
	Update: "Aktualizovat",
	" URL": "URL",
	Edit: "Editovat",
	"Horizontal align": "Horizontální zarovnání",
	Filter: "Filtr",
	"Sort by changed": "Dle poslední změny",
	"Sort by name": "Dle názvu",
	"Sort by size": "Dle velikosti",
	"Add folder": "Přidat složku",
	Reset: "Reset",
	Save: "Uložit",
	"Save as ...": "Uložit jako...",
	Resize: "Změnit rozměr",
	Crop: "Ořezat",
	Width: "Šířka",
	Height: "Výška",
	"Keep Aspect Ratio": "Ponechat poměr",
	Yes: "Ano",
	No: "Ne",
	Remove: "Vyjmout",
	Select: "Označit",
	"Chars: %d": "Znaky: %d",
	"Words: %d": "Slova: %d",
	All: "Vše",
	"Select %s": "Označit %s",
	"Select all": "Označit vše",
	"Vertical align": "Vertikální zarovnání",
	Split: "Rozdělit",
	"Split vertical": "Rozdělit vertikálně",
	"Split horizontal": "Rozdělit horizontálně",
	Merge: "Spojit",
	"Add column": "Přidat sloupec",
	"Add row": "Přidat řádek",
	Delete: "Vymazat",
	Border: "Okraj",
	"License: %s": "Licence: %s",
	"Strike through": "Přeškrtnuto",
	Underline: "Podtrženo",
	superscript: "Horní index",
	subscript: "Dolní index",
	"Cut selection": "Vyjmout označené",
	Break: "Zalomení",
	"Search for": "Najdi",
	"Replace with": "Nahradit za",
	Replace: "Vyměňte",
	Paste: "Vložit",
	"Choose Content to Paste": "Vyber obsah pro vložení",
	"You can only edit your own images. Download this image on the host?": "Můžete upravovat pouze své obrázky. Načíst obrázek?",
	"The image has been successfully uploaded to the host!": "Obrázek byl úspěšně nahrán!",
	palette: "paleta",
	"There are no files": "V tomto adresáři nejsou žádné soubory.",
	Rename: "přejmenovat",
	"Enter new name": "Zadejte nový název",
	preview: "náhled",
	download: "Stažení",
	"Paste from clipboard": "Vložit ze schránky",
	"Your browser doesn't support direct access to the clipboard.": "Váš prohlížeč nepodporuje přímý přístup do schránky.",
	"Copy selection": "Kopírovat výběr",
	copy: "kopírování",
	"Border radius": "Border radius",
	"Show all": "Zobrazit všechny",
	Apply: "Platí",
	"Please fill out this field": "Prosím, vyplňte toto pole",
	"Please enter a web address": "Prosím, zadejte webovou adresu",
	Default: "Výchozí",
	Circle: "Kruh",
	Dot: "Dot",
	Quadrate: "Quadrate",
	Find: "Najít",
	"Find Previous": "Najít Předchozí",
	"Find Next": "Najít Další",
	"Insert className": "Vložte název třídy",
	"Press Alt for custom resizing": "Stiskněte Alt pro vlastní změnu velikosti"
};
//#endregion
//#region node_modules/jodit/esm/langs/de.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var de_default = {
	"Type something": "Bitte geben Sie einen Text ein",
	Advanced: "Fortgeschritten",
	"About Jodit": "Über Jodit",
	"Jodit Editor": "Jodit Editor",
	"Jodit User's Guide": "Das Jodit Benutzerhandbuch",
	"contains detailed help for using": "beinhaltet ausführliche Informationen wie Sie den Editor verwenden können.",
	"For information about the license, please go to our website:": "Für Informationen zur Lizenz, besuchen Sie bitte unsere Web-Präsenz:",
	"Buy full version": "Vollversion kaufen",
	"Copyright © XDSoft.net - Chupurnov Valeriy. All rights reserved.": "Copyright © XDSoft.net - Chupurnov Valeriy. Alle Rechte vorbehalten.",
	Anchor: "Anker",
	"Open in new tab": "In neuer Registerkarte öffnen",
	"Open in fullsize": "Editor in voller Größe öffnen",
	"Clear Formatting": "Formatierung löschen",
	"Fill color or set the text color": "Füllfarbe oder Textfarbe ändern",
	Redo: "Wiederholen",
	Undo: "Rückgängig machen",
	Bold: "Fett",
	Italic: "Kursiv",
	"Insert Unordered List": "Unsortierte Liste einfügen",
	"Insert Ordered List": "Nummerierte Liste einfügen",
	"Align Center": "Mittig ausrichten",
	"Align Justify": "Blocksatz",
	"Align Left": "Links ausrichten",
	"Align Right": "Rechts ausrichten",
	"Insert Horizontal Line": "Horizontale Linie einfügen",
	"Insert Image": "Bild einfügen",
	"Insert file": "Datei einfügen",
	"Insert youtube/vimeo video": "Youtube/vimeo Video einfügen",
	"Insert link": "Link einfügen",
	"Font size": "Schriftgröße",
	"Font family": "Schriftfamilie",
	"Insert format block": "Formatblock einfügen",
	Normal: "Normal",
	"Heading 1": "Überschrift 1",
	"Heading 2": "Überschrift 2",
	"Heading 3": "Überschrift 3",
	"Heading 4": "Überschrift 4",
	Quote: "Zitat",
	Code: "Code",
	Insert: "Einfügen",
	"Insert table": "Tabelle einfügen",
	"Decrease Indent": "Einzug verkleinern",
	"Increase Indent": "Einzug vergrößern",
	"Select Special Character": "Sonderzeichen auswählen",
	"Insert Special Character": "Sonderzeichen einfügen",
	"Paint format": "Format kopieren",
	"Change mode": "Änderungsmodus",
	Margins: "Ränder",
	top: "Oben",
	right: "Rechts",
	bottom: "Unten",
	left: "Links",
	Styles: "CSS Stil",
	Classes: "CSS Klassen",
	Align: "Ausrichtung",
	Right: "Rechts",
	Center: "Zentriert",
	Left: "Links",
	"--Not Set--": "Keine",
	Src: "Pfad",
	Title: "Titel",
	Alternative: "Alternativer Text",
	Link: "Link",
	"Open link in new tab": "Link in neuem Tab öffnen",
	Image: "Bild",
	file: "Datei",
	"Image properties": "Bildeigenschaften",
	Cancel: "Abbrechen",
	Ok: "OK",
	"Your code is similar to HTML. Keep as HTML?": "Ihr Text ähnelt HTML-Code. Als HTML beibehalten?",
	"Paste as HTML": "Als HTML einfügen?",
	Keep: "Original speichern",
	Clean: "Säubern",
	"Insert as Text": "Als Text einfügen",
	"Word Paste Detected": "In Word formatierter Text erkannt",
	"The pasted content is coming from a Microsoft Word/Excel document. Do you want to keep the format or clean it up?": "Der Inhalt, den Sie einfügen, stammt aus einem Microsoft Word / Excel-Dokument. Möchten Sie das Format erhalten oder bereinigen?",
	"Insert only Text": "Nur Text einfügen",
	"File Browser": "Dateibrowser",
	"Error on load list": "Fehler beim Laden der Liste",
	"Error on load folders": "Fehler beim Laden der Ordner",
	"Are you sure?": "Sind Sie sicher?",
	"Enter Directory name": "Geben Sie den Verzeichnisnamen ein",
	"Create directory": "Verzeichnis erstellen",
	"type name": "Typname",
	"type dir name": "Typname",
	"Drop image": "Bild hier hinziehen",
	"Drop file": "Datei löschen",
	"or click": "oder hier klicken",
	"Alternative text": "Alternativtext",
	Browse: "Auswählen",
	Upload: "Hochladen",
	Background: "Hintergrund",
	Text: "Text",
	Top: "Oben",
	Middle: "Mittig",
	Bottom: "Unten",
	"Insert column before": "Spalte davor einfügen",
	"Insert column after": "Spalte danach einfügen",
	"Insert row above": "Zeile oberhalb einfügen",
	"Insert row below": "Zeile unterhalb einfügen",
	"Delete table": "Tabelle löschen",
	"Delete row": "Zeile löschen",
	"Delete column": "Spalte löschen",
	"Empty cell": "Zelle leeren",
	Delete: "Löschen",
	"Strike through": "Durchstreichen",
	Underline: "Unterstreichen",
	Break: "Pause",
	"Search for": "Suche nach",
	"Replace with": "Ersetzen durch",
	Replace: "Ersetzen",
	Edit: "Bearbeiten",
	"Vertical align": "Vertikale Ausrichtung",
	"Horizontal align": "Horizontale Ausrichtung",
	Filter: "Filter",
	"Sort by changed": "Sortieren nach geändert",
	"Sort by name": "Nach Name sortieren",
	"Sort by size": "Nach Größe sortiert",
	"Add folder": "Ordner hinzufügen",
	"Split vertical": "Vertikal unterteilen",
	"Split horizontal": "Horizontal unterteilen",
	Split: "Unterteilen",
	Merge: "Vereinen",
	"Add column": "Spalte hinzufügen",
	"Add row": "Zeile hinzufügen",
	Border: "Rand",
	"Embed code": "Code einbetten",
	Update: "Aktualisieren",
	superscript: "Hochstellen",
	subscript: "Tiefstellen",
	"Cut selection": "Auswahl ausschneiden",
	Paste: "Einfügen",
	"Choose Content to Paste": "Wählen Sie den Inhalt zum Einfügen aus",
	"Chars: %d": "Zeichen: %d",
	"Words: %d": "Wörter: %d",
	All: "Alles markieren",
	"Select %s": "Markieren: %s",
	"Select all": "Alles markieren",
	source: "HTML",
	bold: "Fett gedruckt",
	italic: "Kursiv",
	brush: "Bürste",
	link: "Verknüpfung",
	undo: "Rückgängig machen",
	redo: "Wiederholen",
	table: "Tabelle",
	image: "Bild",
	eraser: "Radiergummi",
	paragraph: "Absatz",
	fontsize: "Schriftgröße",
	video: "Video",
	font: "Schriftart",
	about: "Über",
	print: "Drucken",
	underline: "Unterstreichen",
	strikethrough: "Durchstreichen",
	indent: "Einzug",
	outdent: "Herausstellen",
	fullsize: "Vollgröße",
	shrink: "Schrumpfen",
	hr: "die Linie",
	ul: "Liste von",
	ol: "Nummerierte Liste",
	"Lower Alpha": "Standard, Alphabet (klein)",
	"Upper Alpha": "Standard, Alphabet (gross)",
	"Lower Roman": "Römisch (klein)",
	"Upper Roman": "Römisch (gross)",
	"Lower Greek": "Griechisch",
	cut: "Schneiden",
	selectall: "Wählen Sie Alle aus",
	"Open link": "Link öffnen",
	"Edit link": "Link bearbeiten",
	"No follow": "Nofollow-Attribut",
	Unlink: "Link entfernen",
	Eye: "Ansehen",
	pencil: "Bearbeiten",
	" URL": "URL",
	Reset: "Wiederherstellen",
	Save: "Speichern",
	"Save as ...": "Speichern als",
	Resize: "Größe ändern",
	Crop: "Größe anpassen",
	Width: "Breite",
	Height: "Höhe",
	"Keep Aspect Ratio": "Seitenverhältnis beibehalten",
	Yes: "Ja",
	No: "Nein",
	Remove: "Entfernen",
	Select: "Markieren",
	"You can only edit your own images. Download this image on the host?": "Sie können nur Ihre eigenen Bilder bearbeiten. Dieses Bild auf den Host herunterladen?",
	"The image has been successfully uploaded to the host!": "Das Bild wurde erfolgreich auf den Server hochgeladen!",
	palette: "Palette",
	"There are no files": "In diesem Verzeichnis befinden sich keine Dateien.",
	Rename: "Umbenennen",
	"Enter new name": "Geben Sie einen neuen Namen ein",
	preview: "Vorschau",
	download: "Herunterladen",
	"Paste from clipboard": "Aus Zwischenablage einfügen",
	"Your browser doesn't support direct access to the clipboard.": "Ihr Browser unterstützt keinen direkten Zugriff auf die Zwischenablage.",
	"Copy selection": "Auswahl kopieren",
	copy: "Kopieren",
	"Border radius": "Radius für abgerundete Ecken",
	"Show all": "Alle anzeigen",
	Apply: "Anwenden",
	"Please fill out this field": "Bitte füllen Sie dieses Feld aus",
	"Please enter a web address": "Bitte geben Sie eine Web-Adresse ein",
	Default: "Standard",
	Circle: "Kreis",
	Dot: "Punkte",
	Quadrate: "Quadrate",
	Find: "Suchen",
	"Find Previous": "Suche vorherige",
	"Find Next": "Weitersuchen",
	"Insert className": "className (CSS) einfügen",
	"Press Alt for custom resizing": "Drücken Sie Alt für benutzerdefinierte Größenanpassung",
	"License: %s": "Lizenz: %s"
};
//#endregion
//#region node_modules/jodit/esm/langs/en.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var en_default = {
	"Type something": "Start writing...",
	pencil: "Edit",
	Quadrate: "Square"
};
//#endregion
//#region node_modules/jodit/esm/langs/es.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var es_default = {
	"Type something": "Escriba algo...",
	Advanced: "Avanzado",
	"About Jodit": "Acerca de Jodit",
	"Jodit Editor": "Jodit Editor",
	"Jodit User's Guide": "Guía de usuario Jodit",
	"contains detailed help for using": "contiene ayuda detallada para el uso.",
	"For information about the license, please go to our website:": "Para información sobre la licencia, por favor visite nuestro sitio:",
	"Buy full version": "Compre la versión completa",
	"Copyright © XDSoft.net - Chupurnov Valeriy. All rights reserved.": "Copyright © XDSoft.net - Chupurnov Valeriy. Todos los derechos reservados.",
	Anchor: "Anclar",
	"Open in new tab": "Abrir en nueva pestaña",
	"Open in fullsize": "Abrir editor en pantalla completa",
	"Clear Formatting": "Limpiar formato",
	"Fill color or set the text color": "Color de relleno o de letra",
	Redo: "Rehacer",
	Undo: "Deshacer",
	Bold: "Negrita",
	Italic: "Cursiva",
	"Insert Unordered List": "Insertar lista no ordenada",
	"Insert Ordered List": "Insertar lista ordenada",
	"Align Center": "Alinear Centrado",
	"Align Justify": "Alinear Justificado",
	"Align Left": "Alinear Izquierda",
	"Align Right": "Alinear Derecha",
	"Insert Horizontal Line": "Insertar línea horizontal",
	"Insert Image": "Insertar imagen",
	"Insert file": "Insertar archivo",
	"Insert youtube/vimeo video": "Insertar video de Youtube/vimeo",
	"Insert link": "Insertar vínculo",
	"Font size": "Tamaño de letra",
	"Font family": "Familia de letra",
	"Insert format block": "Insertar bloque",
	Normal: "Normal",
	"Heading 1": "Encabezado 1",
	"Heading 2": "Encabezado 2",
	"Heading 3": "Encabezado 3",
	"Heading 4": "Encabezado 4",
	Quote: "Cita",
	Code: "Código",
	Insert: "Insertar",
	"Insert table": "Insertar tabla",
	"Decrease Indent": "Disminuir sangría",
	"Increase Indent": "Aumentar sangría",
	"Select Special Character": "Seleccionar caracter especial",
	"Insert Special Character": "Insertar caracter especial",
	"Paint format": "Copiar formato",
	"Change mode": "Cambiar modo",
	Margins: "Márgenes",
	top: "arriba",
	right: "derecha",
	bottom: "abajo",
	left: "izquierda",
	Styles: "Estilos CSS",
	Classes: "Clases CSS",
	Align: "Alinear",
	Right: "Derecha",
	Center: "Centrado",
	Left: "Izquierda",
	"--Not Set--": "--No Establecido--",
	Src: "Fuente",
	Title: "Título",
	Alternative: "Texto Alternativo",
	Filter: "Filtrar",
	Link: "Vínculo",
	"Open link in new tab": "Abrir vínculo en nueva pestaña",
	Image: "Imagen",
	file: "Archivo",
	"Image properties": "Propiedades de imagen",
	Cancel: "Cancelar",
	Ok: "Aceptar",
	"Your code is similar to HTML. Keep as HTML?": "El código es similar a HTML. ¿Mantener como HTML?",
	"Paste as HTML": "Pegar como HTML?",
	Keep: "Mantener",
	Clean: "Limpiar",
	"Insert as Text": "Insertar como texto",
	"Word Paste Detected": "Pegado desde Word detectado",
	"The pasted content is coming from a Microsoft Word/Excel document. Do you want to keep the format or clean it up?": "El contenido pegado proviene de un documento de Microsoft Word/Excel. ¿Desea mantener el formato o limpiarlo?",
	"Insert only Text": "Insertar solo texto",
	"File Browser": "Buscar archivo",
	"Error on load list": "Error al cargar la lista",
	"Error on load folders": "Error al cargar las carpetas",
	"Are you sure?": "¿Está seguro?",
	"Enter Directory name": "Entre nombre de carpeta",
	"Create directory": "Crear carpeta",
	"type name": "Entre el nombre",
	"type dir name": "Entre el nombre",
	"Drop image": "Soltar imagen",
	"Drop file": "Soltar archivo",
	"or click": "o click",
	"Alternative text": "Texto alternativo",
	Browse: "Buscar",
	Upload: "Subir",
	Background: "Fondo",
	Text: "Texto",
	Top: "Arriba",
	Middle: "Centro",
	Bottom: "Abajo",
	"Insert column before": "Insertar columna antes",
	"Insert column after": "Interar columna después",
	"Insert row above": "Insertar fila arriba",
	"Insert row below": "Insertar fila debajo",
	"Delete table": "Borrar tabla",
	"Delete row": "Borrar fila",
	"Delete column": "Borrar columna",
	"Empty cell": "Vaciar celda",
	Delete: "Borrar",
	"Strike through": "Tachado",
	Underline: "Subrayado",
	Break: "Pausa",
	"Search for": "Buscar",
	"Replace with": "Reemplazar con",
	Replace: "Reemplazar",
	Edit: "Editar",
	"Vertical align": "Alineación vertical",
	"Horizontal align": "Alineación horizontal",
	"Sort by changed": "Ordenar por fecha modificación",
	"Sort by name": "Ordenar por nombre",
	"Sort by size": "Ordenar por tamaño",
	"Add folder": "Agregar carpeta",
	Split: "Dividir",
	"Split vertical": "Dividir vertical",
	"Split horizontal": "Dividir horizontal",
	Merge: "Mezclar",
	"Add column": "Agregar columna",
	"Add row": "Agregar fila",
	Border: "Borde",
	"Embed code": "Incluir código",
	Update: "Actualizar",
	superscript: "superíndice",
	subscript: "subíndice",
	"Cut selection": "Cortar selección",
	Paste: "Pegar",
	"Choose Content to Paste": "Seleccionar contenido para pegar",
	"Chars: %d": "Caracteres: %d",
	"Words: %d": "Palabras: %d",
	All: "Todo",
	"Select %s": "Seleccionar: %s",
	"Select all": "Seleccionar todo",
	source: "HTML",
	bold: "negrita",
	italic: "cursiva",
	brush: "Brocha",
	link: "Vínculo",
	undo: "deshacer",
	redo: "rehacer",
	table: "Tabla",
	image: "Imagen",
	eraser: "Borrar",
	paragraph: "Párrafo",
	fontsize: "Tamaño de letra",
	video: "Video",
	font: "Letra",
	about: "Acerca de",
	print: "Imprimir",
	underline: "subrayar",
	strikethrough: "tachar",
	indent: "sangría",
	outdent: "quitar sangría",
	fullsize: "Tamaño completo",
	shrink: "encoger",
	hr: "línea horizontal",
	ul: "lista sin ordenar",
	ol: "lista ordenada",
	cut: "Cortar",
	selectall: "Seleccionar todo",
	"Open link": "Abrir vínculo",
	"Edit link": "Editar vínculo",
	"No follow": "No seguir",
	Unlink: "Desvincular",
	Eye: "Ver",
	pencil: "Para editar",
	" URL": "URL",
	Reset: "Resetear",
	Save: "Guardar",
	"Save as ...": "Guardar como...",
	Resize: "Redimensionar",
	Crop: "Recortar",
	Width: "Ancho",
	Height: "Alto",
	"Keep Aspect Ratio": "Mantener relación de aspecto",
	Yes: "Si",
	No: "No",
	Remove: "Quitar",
	Select: "Seleccionar",
	"You can only edit your own images. Download this image on the host?": "Solo puedes editar tus propias imágenes. ¿Descargar esta imagen en el servidor?",
	"The image has been successfully uploaded to the host!": "¡La imagen se ha subido correctamente al servidor!",
	palette: "paleta",
	"There are no files": "No hay archivos en este directorio.",
	Rename: "renombrar",
	"Enter new name": "Ingresa un nuevo nombre",
	preview: "avance",
	download: "Descargar",
	"Paste from clipboard": "Pegar desde el portapapeles",
	"Your browser doesn't support direct access to the clipboard.": "Su navegador no soporta el acceso directo en el portapapeles.",
	"Copy selection": "Selección de copia",
	copy: "copia",
	"Border radius": "Radio frontera",
	"Show all": "Mostrar todos los",
	Apply: "Aplicar",
	"Please fill out this field": "Por favor, rellene este campo",
	"Please enter a web address": "Por favor, introduzca una dirección web",
	Default: "Predeterminado",
	Circle: "Círculo",
	Dot: "Punto",
	Quadrate: "Cuadro",
	"Lower Alpha": "Letra Minúscula",
	"Lower Greek": "Griego Minúscula",
	"Lower Roman": "Romano Minúscula",
	"Upper Alpha": "Letra Mayúscula",
	"Upper Roman": "Romano Mayúscula",
	Find: "Encontrar",
	"Find Previous": "Buscar Anterior",
	"Find Next": "Buscar Siguiente",
	"Insert className": "Insertar nombre de clase",
	"Press Alt for custom resizing": "Presione Alt para cambiar el tamaño personalizado",
	"License: %s": "Licencia: %s"
};
//#endregion
//#region node_modules/jodit/esm/langs/fa.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var fa_default = {
	"Type something": "چیزی بنویسید",
	Advanced: "پیشرفته",
	"About Jodit": "درباره ویرایشگر",
	"Jodit Editor": "ویرایشگر جودیت",
	"Jodit User's Guide": "راهنمای کاربران",
	"contains detailed help for using": "راهنمایی برای استفاده",
	"For information about the license, please go to our website:": "برای کسب اطلاعات در رابطه با لایسنس لطفا به وب سایت مراجعه کنید:",
	"Buy full version": "خرید نسخه کامل",
	"Copyright © XDSoft.net - Chupurnov Valeriy. All rights reserved.": "Copyright © XDSoft.net - Chupurnov Valeriy. Alle Rechte vorbehalten.",
	Anchor: "لینک",
	"Open in new tab": "بازکردن در تب جدید",
	"Open in fullsize": "تمام صفحه کردن ویرایشگر",
	"Clear Formatting": "پاک کردن قالب ها",
	"Fill color or set the text color": "رنگ را پر کنید یا رنگ متن را تنظیم کنید",
	Redo: "باز انجام",
	Undo: "بازگردانی",
	Bold: "بولد",
	Italic: "ایتالیک",
	"Insert Unordered List": "افزودن لیست بدون ترتیب",
	"Insert Ordered List": "افزون لیست با ترتیب",
	"Align Center": "وسط چین",
	"Align Justify": "تراز کردن",
	"Align Left": "چپ چین",
	"Align Right": "راست چین",
	"Insert Horizontal Line": "افزودن خط افقی",
	"Insert Image": "افزودن عکس",
	"Insert file": "افزودن فایل",
	"Insert youtube/vimeo video": "افزودن ویدیو از یوتیوب و وایمو",
	"Insert link": "افزودن لینک",
	"Font size": "اندازه فونت",
	"Font family": "فونت",
	"Insert format block": "افزودن بلاک",
	Normal: "معمولی",
	"Heading 1": "سرتیتر ۱",
	"Heading 2": "سرتیتر ۲",
	"Heading 3": "سرتیتر ۳",
	"Heading 4": "سرتیتر ۴",
	Quote: "نقل قول",
	Code: "سورس کد",
	Insert: "افزودن",
	"Insert table": "افزودن جدول",
	"Decrease Indent": "افزودن فرورفتگی",
	"Increase Indent": "کاهش فرورفتگی",
	"Select Special Character": "کاراکتر ویژه را انتخاب کنید",
	"Insert Special Character": "افزودن کاراکتر ویژه",
	"Paint format": "قالب رنگ",
	"Change mode": "تغییر حالت",
	Margins: "فاصله ها",
	top: "بالا",
	right: "راست",
	bottom: "پایین",
	left: "چپ",
	Styles: "استایل ها",
	Classes: "کلاس ها",
	Align: "تراز کردن",
	Right: "راست",
	Center: "وسط",
	Left: "چپ",
	"--Not Set--": "--تنظیم نشده--",
	Src: "سورس",
	Title: "عنوان",
	Alternative: "جایگزین",
	Link: "لینک",
	"Open link in new tab": "باز کزدن لینک در تب جدید",
	Image: "عکس",
	file: "فایل",
	"Image properties": "مقادیر عکس",
	Cancel: "بیخیال",
	Ok: "تایید",
	"Your code is similar to HTML. Keep as HTML?": "به نظر کد شما از نوع HTML است , با همین قالب ادامه دهیم ؟",
	"Paste as HTML": "جایگزاری HTML",
	Keep: "نگه دار",
	Clean: "تمیز کزدن",
	"Insert as Text": "وارد کردن به عنوان متن عادی",
	"Word Paste Detected": "ظاهر در حال کپی پیست کردن هستید",
	"The pasted content is coming from a Microsoft Word/Excel document. Do you want to keep the format or clean it up?": "متن کپی شده از محصولات مایکروسافت است مایل هستید استایل مایکروسافت منتقل شود یا به عنوان متن عادی باکزاری شود ؟",
	"Insert only Text": "افزودن به عنوان متن عادی",
	"File Browser": "انتخاب فایل",
	"Error on load list": "خطا در بارگزاری لیست",
	"Error on load folders": "خطا در بازگزاری پوشه ها",
	"Are you sure?": "از انجام این اقدام اطمینان دارید ؟",
	"Enter Directory name": "نام مسیر را وارد کنید",
	"Create directory": "ساخت مسیر",
	"type name": "نام را وارد کنید",
	"type dir name": "نام را وارد کنید",
	"Drop image": "عکس را رها کنید",
	"Drop file": "فایل را رها کنید",
	"or click": "یا کلیک کنید",
	"Alternative text": "متن جایگزین",
	Browse: "جستجو",
	Upload: "آپلود",
	Background: "پس زمینه",
	Text: "متن",
	Top: "بالا",
	Middle: "وسط",
	Bottom: "پایین",
	"Insert column before": "افزودن ستون قبل از",
	"Insert column after": "افزودن ستون بعد از",
	"Insert row above": "افزودن خط قبل از",
	"Insert row below": "افزودن خط بعد از",
	"Delete table": "حذف جدول",
	"Delete row": "حذف خط",
	"Delete column": "حذف ستون",
	"Empty cell": "خالی کدن بلاک",
	Delete: "حذف",
	"Strike through": "خط زدن",
	Underline: "زیر خط (Underline)",
	Break: "شکستن",
	"Search for": "جستجو برای",
	"Replace with": "جایگزین با",
	Replace: "جایگزین",
	Edit: "ویرایش",
	"Vertical align": "تراز عمودی",
	"Horizontal align": "تراز افقی",
	Filter: "فیلتر",
	"Sort by changed": "مرتب سازی بر اساس تغییرات",
	"Sort by name": "مرتب سازی بر اساس نام",
	"Sort by size": "مرتب سازی بر اساس اندازه",
	"Add folder": "افزودن پوشه",
	Split: "شکاف",
	"Split vertical": "شکاف عمودی",
	"Split horizontal": "شکاف افقی",
	Merge: "تجمیع کردن",
	"Add column": "افزودن ستون",
	"Add row": "افزودن خط",
	Border: "خط",
	"Embed code": "درج کد",
	Update: "بروزرسانی",
	superscript: "Super Script",
	subscript: "Sub Script",
	"Cut selection": "برداشتن انتخاب شده ها",
	Paste: "چسباندن",
	"Choose Content to Paste": "انتخاب محتوای برای چسباندن",
	"Chars: %d": "کاراکترها: %d",
	"Words: %d": "کلمات: %d",
	All: "همه",
	"Select %s": "انتخاب: %s",
	"Select all": "انتخاب همه",
	source: "سورس",
	bold: "بولد",
	italic: "ایتالیک",
	brush: "قلم مو",
	link: "لینک",
	undo: "بازگردانی",
	redo: "باز انجام",
	table: "جدول",
	image: "عکس",
	eraser: "پاک کن",
	paragraph: "پاراگراف",
	fontsize: "سایز فونت",
	video: "ویدیو",
	font: "فونت",
	about: "درباره",
	print: "چاپ",
	underline: "خط زیرین",
	strikethrough: "خط روی متن",
	indent: "فرورفتگی",
	outdent: "فرورفتگی از بیرون",
	fullsize: "کامل کردن",
	shrink: "کوچک کردن",
	hr: "خط",
	ul: "لیست",
	ol: "لیست عدید",
	cut: "بریدن",
	selectall: "انتخاب همه",
	"Open link": "بازکردن لینک",
	"Edit link": "ویرایش لینک",
	"No follow": "No Follow",
	Unlink: "حذف لینک",
	Eye: "مرور",
	pencil: "اصلاح",
	" URL": " URL",
	Reset: "ریست",
	Save: "ذخیر",
	"Save as ...": "ذخیره به عنوان...",
	Resize: "تغییر اندازه",
	Crop: "بریدن",
	Width: "طول",
	Height: "ارتفاع",
	"Keep Aspect Ratio": "نسبت ابعاد را حفظ کن",
	Yes: "بله",
	No: "خیر",
	Remove: "حذف",
	Select: "انتخاب",
	"You can only edit your own images. Download this image on the host?": "شما فقط میتوانید عکس های خود را ویرایش کنید , میخواهید عکس را از هاست دانلود کنیم ؟",
	"The image has been successfully uploaded to the host!": "عکس با موفقیت در هاست آپلود شد",
	palette: "جعبه رنگ نقاشی",
	"There are no files": "در این مسیر فایل وجود ندارد",
	Rename: "تغییر اسم",
	"Enter new name": "اسم جدید را وارد کنید",
	preview: "نمایش",
	download: "دانلود",
	"Paste from clipboard": "چسباندن از کلیپ بورد",
	"Your browser doesn't support direct access to the clipboard.": "مرورگر شما اجازه دسترسی به کلیپ بورد را نمیدهد.",
	"Copy selection": "کپی کردن انتخاب شده ها",
	copy: "کپی",
	"Border radius": "Border radius",
	"Show all": "نمایش همه",
	Apply: "درخواست",
	"Please fill out this field": "لطفا با پر کردن این زمینه",
	"Please enter a web address": "لطفا وارد یک آدرس وب",
	Default: "به طور پیش فرض",
	Circle: "دایره",
	Dot: "پورنو نقطه",
	Quadrate: "Quadrate",
	Find: "پیدا کردن",
	"Find Previous": "پیدا کردن قبلی",
	"Find Next": "پیدا کردن بعدی",
	"Insert className": "Insertar nombre de clase",
	"Press Alt for custom resizing": "برای تغییر اندازه سفارشی فشار دهید",
	"License: %s": "مجوز: %s"
};
//#endregion
//#region node_modules/jodit/esm/langs/fi.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var fi_default = {
	"Type something": "Kirjoita jotain...",
	Advanced: "Avanzado",
	"About Jodit": "Tietoja Jodit:ista",
	"Jodit Editor": "Jodit Editor",
	"Jodit User's Guide": "Jodit käyttäjän ohje",
	"contains detailed help for using": "sisältää tarkempaa tietoa käyttämiseen",
	"For information about the license, please go to our website:": "Tietoa lisensoinnista, vieraile verkkosivuillamme:",
	"Buy full version": "Osta täysi versio",
	"Copyright © XDSoft.net - Chupurnov Valeriy. All rights reserved.": "Copyright © XDSoft.net - Chupurnov Valeriy. Kaikki oikeudet pidätetään.",
	Anchor: "Ankkuri",
	"Open in new tab": "Avaa uudessa välilehdessä",
	"Open in fullsize": "Avaa täysikokoisena",
	"Clear Formatting": "Poista muotoilu",
	"Fill color or set the text color": "Täytä värillä tai aseta tekstin väri",
	Redo: "Tee uudelleen",
	Undo: "Peruuta",
	Bold: "Lihavoitu",
	Italic: "Kursiivi",
	"Insert Unordered List": "Lisää järjestämätön lista",
	"Insert Ordered List": "Lisää järjestetty lista",
	"Align Center": "Asemoi keskelle",
	"Align Justify": "Asemoi tasavälein",
	"Align Left": "Asemoi vasemmalle",
	"Align Right": "Asemoi oikealle",
	"Insert Horizontal Line": "Lisää vaakasuuntainen viiva",
	"Insert Image": "Lisää kuva",
	"Insert file": "Lisää tiedosto",
	"Insert youtube/vimeo video": "Lisää Youtube-/vimeo- video",
	"Insert link": "Lisää linkki",
	"Font size": "Kirjasimen koko",
	"Font family": "Kirjasimen nimi",
	"Insert format block": "Lisää muotoilualue",
	Normal: "Normaali",
	"Heading 1": "Otsikko 1",
	"Heading 2": "Otsikko 2",
	"Heading 3": "Otsikko 3",
	"Heading 4": "Otsikko 4",
	Quote: "Lainaus",
	Code: "Koodi",
	Insert: "Lisää",
	"Insert table": "Lisää taulukko",
	"Decrease Indent": "Pienennä sisennystä",
	"Increase Indent": "Lisää sisennystä",
	"Select Special Character": "Valitse erikoismerkki",
	"Insert Special Character": "Lisää erikoismerkki",
	"Paint format": "Maalaa muotoilu",
	"Change mode": "Vaihda tilaa",
	Margins: "Marginaalit",
	top: "ylös",
	right: "oikealle",
	bottom: "alas",
	left: "vasemmalle",
	Styles: "CSS-tyylit",
	Classes: "CSS-luokat",
	Align: "Asemointi",
	Right: "Oikea",
	Center: "Keskellä",
	Left: "Vasen",
	"--Not Set--": "--Ei asetettu--",
	Src: "Fuente",
	Title: "Otsikko",
	Alternative: "Vaihtoehtoinen teksti",
	Filter: "Suodatin",
	Link: "Linkki",
	"Open link in new tab": "Avaa uudessa välilehdessä",
	Image: "Kuva",
	file: "Tiedosto",
	"Image properties": "Kuvan ominaisuudet",
	Cancel: "Peruuta",
	Ok: "Ok",
	"Your code is similar to HTML. Keep as HTML?": "Koodi on HTML:n tapaista. Säilytetäänkö HTML?",
	"Paste as HTML": "Liitä HTML:nä?",
	Keep: "Säilytä",
	Clean: "Tyhjennä",
	"Insert as Text": "Lisää tekstinä",
	"Word Paste Detected": "Word liittäminen havaittu",
	"The pasted content is coming from a Microsoft Word/Excel document. Do you want to keep the format or clean it up?": "Liitetty sisältö tulee Microsoft Word-/Excel- tiedostosta. Haluatko säilyttää muotoilun vai poistaa sen?",
	"Insert only Text": "Lisää vain teksti",
	"File Browser": "Tiedostoselain",
	"Error on load list": "Virhe listan latauksessa",
	"Error on load folders": "Virhe kansioiden latauksessa",
	"Are you sure?": "Oletko varma?",
	"Enter Directory name": "Syötä hakemiston nimi",
	"Create directory": "Luo hakemisto",
	"type name": "Syötä nimi",
	"type dir name": "Syötä nimi",
	"Drop image": "Pudota kuva",
	"Drop file": "Pudota tiedosto",
	"or click": "tai klikkaa",
	"Alternative text": "Vaihtoehtoinen teksti",
	Browse: "Selaa",
	Upload: "Lataa",
	Background: "Tausta",
	Text: "Teksti",
	Top: "Ylös",
	Middle: "Keskelle",
	Bottom: "Alas",
	"Insert column before": "Lisää sarake ennen",
	"Insert column after": "Lisää sarake jälkeen",
	"Insert row above": "Lisää rivi ylös",
	"Insert row below": "Lisää rivi alle",
	"Delete table": "Poista taulukko",
	"Delete row": "Poista rivi",
	"Delete column": "Poista sarake",
	"Empty cell": "Tyhjennä solu",
	Delete: "Poista",
	"Strike through": "Yliviivaus",
	Underline: "Alleviivaus",
	Break: "Vaihto",
	"Search for": "Etsi arvoa",
	"Replace with": "Korvaa arvolla",
	Replace: "Korvaa",
	Edit: "Muokkaa",
	"Vertical align": "Pystyasemointi",
	"Horizontal align": "Vaaka-asemointi",
	"Sort by changed": "Järjestä muuttuneilla",
	"Sort by name": "Järjestä nimellä",
	"Sort by size": "Järjestä koolla",
	"Add folder": "Lisää kansio",
	Split: "Jaa",
	"Split vertical": "Jaa pystysuuntaisesti",
	"Split horizontal": "Jaa vaakasuuntaisesti",
	Merge: "Yhdistä",
	"Add column": "Lisää sarake",
	"Add row": "Lisää rivi",
	Border: "Reuna",
	"Embed code": "Sisällytä koodi",
	Update: "Päivitä",
	superscript: "yläviite",
	subscript: "alaviite",
	"Cut selection": "Leikkaa valinta",
	Paste: "Liitä",
	"Choose Content to Paste": "Valitse liitettävä sisältö",
	"Chars: %d": "Merkit: %d",
	"Words: %d": "Sanat: %d",
	All: "Kaikki",
	"Select %s": "Valitse: %s",
	"Select all": "Valitse kaikki",
	source: "HTML",
	bold: "lihavoitu",
	italic: "kursiivi",
	brush: "sivellin",
	link: "linkki",
	undo: "peruuta",
	redo: "tee uudelleen",
	table: "taulukko",
	image: "kuva",
	eraser: "pyyhekumi",
	paragraph: "kappale",
	fontsize: "tekstin koko",
	video: "video",
	font: "kirjasin",
	about: "tietoja",
	print: "tulosta",
	underline: "alleviivaa",
	strikethrough: "yliviivaa",
	indent: "sisennä",
	outdent: "pienennä sisennystä",
	fullsize: "täysikokoinen",
	shrink: "pienennä",
	hr: "vaakaviiva",
	ul: "järjestetty lista",
	ol: "järjestämätön lista",
	cut: "leikkaa",
	selectall: "valitse kaikki",
	"Open link": "Avaa linkki",
	"Edit link": "Muokkaa linkkiä",
	"No follow": "Älä seuraa",
	Unlink: "Pura linkki",
	Eye: "Ver",
	pencil: "Muokkaa",
	" URL": "URL",
	Reset: "Nollaa",
	Save: "Tallenna",
	"Save as ...": "Tallenna nimellä ...",
	Resize: "Muuta kokoa",
	Crop: "Rajaa",
	Width: "Leveys",
	Height: "Korkeus",
	"Keep Aspect Ratio": "Säilytä kuvasuhde",
	Yes: "Kyllä",
	No: "Ei",
	Remove: "Poista",
	Select: "Valitse",
	"You can only edit your own images. Download this image on the host?": "Voit muokata vain omia kuvia. Lataa tämä kuva palvelimelle?",
	"The image has been successfully uploaded to the host!": "Kuva on onnistuneesti ladattu palvelimelle!",
	palette: "paletti",
	"There are no files": "Tiedostoja ei ole",
	Rename: "Nimeä uudelleen",
	"Enter new name": "Syötä uusi nimi",
	preview: "esikatselu",
	download: "Lataa",
	"Paste from clipboard": "Liitä leikepöydältä",
	"Your browser doesn't support direct access to the clipboard.": "Selaimesi ei tue suoraa pääsyä leikepöydälle.",
	"Copy selection": "Kopioi valinta",
	copy: "kopioi",
	"Border radius": "Reunan pyöristys",
	"Show all": "Näytä kaikki",
	Apply: "Käytä",
	"Please fill out this field": "Täytä tämä kenttä",
	"Please enter a web address": "Annan web-osoite",
	Default: "Oletus",
	Circle: "Ympyrä",
	Dot: "Piste",
	Quadrate: "Neliö",
	"Lower Alpha": "Pieni aakkosellinen",
	"Lower Greek": "Pieni kreikkalainen",
	"Lower Roman": "Pieni roomalainen",
	"Upper Alpha": "Suuri aakkosellinen",
	"Upper Roman": "Suuri roomalainen",
	Find: "Hae",
	"Find Previous": "Hae edellinen",
	"Find Next": "Hae seuraava",
	"Insert className": "Lisää luokkanimi",
	"Press Alt for custom resizing": "Paina Alt muokattuun koon muuttamiseen",
	"Class name": "Luokan nimi",
	"License: %s": "Lisenssi: %s"
};
//#endregion
//#region node_modules/jodit/esm/langs/fr.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var fr_default = {
	"Type something": "Ecrivez ici",
	"About Jodit": "A propos de Jodit",
	"Jodit Editor": "Editeur Jodit",
	"Jodit User's Guide": "Guide de l'utilisateur",
	"contains detailed help for using": "Aide détaillée à l'utilisation",
	"For information about the license, please go to our website:": "Consulter la licence sur notre site web:",
	"Buy full version": "Acheter la version complète",
	"Copyright © XDSoft.net - Chupurnov Valeriy. All rights reserved.": "Copyright © XDSoft.net - Chupurnov Valeriy. Tous droits réservés.",
	Anchor: "Ancre",
	"Open in new tab": "Ouvrir dans un nouvel onglet",
	"Open in fullsize": "Ouvrir l'éditeur en pleine page",
	"Clear Formatting": "Supprimer le formattage",
	"Fill color or set the text color": "Modifier la couleur du fond ou du texte",
	Redo: "Refaire",
	Undo: "Défaire",
	Bold: "Gras",
	Italic: "Italique",
	"Insert Unordered List": "Liste non ordonnée",
	"Insert Ordered List": "Liste ordonnée",
	"Align Center": "Centrer",
	"Align Justify": "Justifier",
	"Align Left": "Aligner à gauche ",
	"Align Right": "Aligner à droite",
	"Insert Horizontal Line": "Insérer une ligne horizontale",
	"Insert Image": "Insérer une image",
	"Insert file": "Insérer un fichier",
	"Insert youtube/vimeo video": "Insérer une vidéo",
	"Insert link": "Insérer un lien",
	"Font size": "Taille des caractères",
	"Font family": "Famille des caractères",
	"Insert format block": "Bloc formatté",
	Normal: "Normal",
	"Heading 1": "Titre 1",
	"Heading 2": "Titre 2",
	"Heading 3": "Titre 3",
	"Heading 4": "Titre 4",
	Quote: "Citation",
	Code: "Code",
	Insert: "Insérer",
	"Insert table": "Insérer un tableau",
	"Decrease Indent": "Diminuer le retrait",
	"Increase Indent": "Retrait plus",
	"Select Special Character": "Sélectionnez un caractère spécial",
	"Insert Special Character": "Insérer un caractère spécial",
	"Paint format": "Cloner le format",
	"Change mode": "Mode wysiwyg <-> code html",
	Margins: "Marges",
	top: "haut",
	right: "droite",
	bottom: "Bas",
	left: "gauche",
	Styles: "Styles",
	Classes: "Classes",
	Align: "Alignement",
	Right: "Droite",
	Center: "Centre",
	Left: "Gauche",
	"--Not Set--": "--Non disponible--",
	Src: "Source",
	Title: "Titre",
	Alternative: "Alternative",
	Filter: "Filtre",
	Link: "Lien",
	"Open link in new tab": "Ouvrir le lien dans un nouvel onglet",
	Image: "Image",
	file: "fichier",
	Advanced: "Avancé",
	"Image properties": "Propriétés de l'image",
	Cancel: "Annuler",
	Ok: "OK",
	"Your code is similar to HTML. Keep as HTML?": "Votre texte que vous essayez de coller est similaire au HTML. Collez-le en HTML?",
	"Paste as HTML": "Coller en HTML?",
	Keep: "Sauvegarder l'original",
	Clean: "Nettoyer",
	"Insert as Text": "Coller en tant que texte",
	"Word Paste Detected": "C'est peut-être un fragment de Word ou Excel",
	"The pasted content is coming from a Microsoft Word/Excel document. Do you want to keep the format or clean it up?": "Le contenu que vous insérez provient d'un document Microsoft Word / Excel. Voulez-vous enregistrer le format ou l'effacer?",
	"Insert only Text": "Coller le texte seulement",
	"File Browser": "Explorateur de fichiers",
	"Error on load list": "Erreur de liste de chargement",
	"Error on load folders": "Erreur de dossier de chargement",
	"Are you sure?": "Etes-vous sûrs ?",
	"Enter Directory name": "Entrer le nom de dossier",
	"Create directory": "Créer un dossier",
	"type name": "Nom du fichier",
	"type dir name": "Nom du dossier",
	"Drop image": "Coller une image",
	"Drop file": "Déposer un fichier",
	"or click": "ou cliquer",
	"Alternative text": "Texte de remplacemement",
	Browse: "Chercher",
	Upload: "Charger",
	Background: "Arrière-plan",
	Border: "Bordure",
	Text: "Texte",
	Top: "Haut",
	Middle: "Milieu",
	Bottom: "Bas",
	"Insert column before": "Insérer une colonne avant",
	"Insert column after": "Insérer une colonne après",
	"Insert row above": "Insérer une ligne au dessus",
	"Insert row below": "Insérer une ligne en dessous",
	"Delete table": "Supprimer le tableau",
	"Delete row": "Supprimer la ligne",
	"Delete column": "Supprimer la colonne",
	"Empty cell": "Vider la cellule",
	"Chars: %d": "Symboles: %d",
	"Words: %d": "Mots: %d",
	Split: "Split",
	"Split vertical": "Split vertical",
	"Split horizontal": "Split horizontal",
	"Strike through": "Barrer",
	Underline: "Souligner",
	superscript: "exposant",
	subscript: "indice",
	"Cut selection": "Couper la sélection",
	"Select all": "Tout sélectionner",
	Break: "Pause",
	"Search for": "Rechercher",
	"Replace with": "Remplacer par",
	Replace: "Remplacer",
	Paste: "Coller",
	"Choose Content to Paste": "Choisissez le contenu à coller",
	source: "la source",
	bold: "gras",
	italic: "italique",
	brush: "pinceau",
	link: "lien",
	undo: "annuler",
	redo: "refaire",
	table: "tableau",
	image: "image",
	eraser: "gomme",
	paragraph: "clause",
	fontsize: "taille de police",
	video: "Video",
	font: "police",
	about: "à propos de l'éditeur",
	print: "impression",
	underline: "souligné",
	strikethrough: "barré",
	indent: "indentation",
	outdent: "retrait",
	fullsize: "taille réelle",
	shrink: "taille conventionnelle",
	hr: "la ligne",
	ul: "Liste",
	ol: "Liste numérotée",
	cut: "Couper",
	selectall: "Sélectionner tout",
	"Open link": "Ouvrir le lien",
	"Edit link": "Modifier le lien",
	"No follow": "Attribut Nofollow",
	Unlink: "Supprimer le lien",
	Eye: "Voir",
	pencil: "Pour éditer",
	" URL": "URL",
	Reset: "Restaurer",
	Save: "Sauvegarder",
	"Save as ...": "Enregistrer sous",
	Resize: "Changer la taille",
	Crop: "Taille de garniture",
	Width: "Largeur",
	Height: "Hauteur",
	"Keep Aspect Ratio": "Garder les proportions",
	Yes: "Oui",
	No: "Non",
	Remove: "Supprimer",
	Select: "Mettre en évidence",
	"Select %s": "Mettre en évidence: %s",
	Update: "Mettre à jour",
	"Vertical align": "Alignement vertical",
	Merge: "aller",
	"Add column": "Ajouter une colonne",
	"Add row": "Ajouter une rangée",
	Delete: "Effacer",
	"Horizontal align": "Alignement horizontal",
	"Sort by changed": "Trier par modification",
	"Sort by name": "Trier par nom",
	"Sort by size": "Trier par taille",
	"Add folder": "Créer le dossier",
	"You can only edit your own images. Download this image on the host?": "Vous ne pouvez éditer que vos propres images. Téléchargez cette image sur l'hôte?",
	"The image has been successfully uploaded to the host!": "L'image a été téléchargée avec succès sur le serveur!",
	palette: "Palette",
	"There are no files": "Il n'y a aucun fichier dans ce répertoire.",
	Rename: "renommer",
	"Enter new name": "Entrez un nouveau nom",
	preview: "Aperçu",
	download: "Télécharger",
	"Paste from clipboard": "Coller à partir du presse-papiers",
	"Your browser doesn't support direct access to the clipboard.": "Votre navigateur ne prend pas en charge l'accès direct au presse-papiers.",
	"Copy selection": "Copier la sélection",
	copy: "copie",
	"Border radius": "Rayon des bordures",
	"Show all": "Afficher tous",
	Apply: "Appliquer",
	"Please fill out this field": "Veuillez remplir ce champ",
	"Please enter a web address": "Veuillez entrer une adresse web",
	Default: "Par défaut",
	Circle: "Cercle",
	Dot: "Point",
	Quadrate: "Quadratique",
	Find: "Trouver",
	"Find Previous": "Précédent",
	"Find Next": "Suivant",
	"Insert className": "Insérer un nom de classe",
	"Press Alt for custom resizing": "Appuyez sur Alt pour un redimensionnement personnalisé",
	"Embed code": "Code d'intégration",
	Edit: "Modifier",
	All: "Tout sélectionner",
	"License: %s": "Licence: %s"
};
//#endregion
//#region node_modules/jodit/esm/langs/he.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var he_default = {
	"Type something": "הקלד משהו...",
	Advanced: "מתקדם",
	"About Jodit": "About Jodit",
	"Jodit Editor": "Jodit Editor",
	"Jodit User's Guide": "Jodit User's Guide",
	"contains detailed help for using": "contains detailed help for using.",
	"For information about the license, please go to our website:": "For information about the license, please go to our website:",
	"Buy full version": "Buy full version",
	"Copyright © XDSoft.net - Chupurnov Valeriy. All rights reserved.": "Copyright © XDSoft.net - Chupurnov Valeriy. All rights reserved.",
	Anchor: "מקום עיגון",
	"Open in new tab": "פתח בכרטיסיה חדשה",
	"Open in fullsize": "פתח את העורך בחלון חדש",
	"Clear Formatting": "נקה עיצוב",
	"Fill color or set the text color": "שנה צבע טקסט או רקע",
	Redo: "בצע שוב",
	Undo: "בטל",
	Bold: "מודגש",
	Italic: "נטוי",
	"Insert Unordered List": "הכנס רשימת תבליטים",
	"Insert Ordered List": "הכנס רשימה ממוספרת",
	"Align Center": "מרכז",
	"Align Justify": "ישר ",
	"Align Left": "ישר לשמאל",
	"Align Right": "ישר לימין",
	"Insert Horizontal Line": "הכנס קו אופקי",
	"Insert Image": "הכנס תמונה",
	"Insert file": "הכנס קובץ",
	"Insert youtube/vimeo video": "הכנס סרטון וידאו מYouTube/Vimeo",
	"Insert link": "הכנס קישור",
	"Font size": "גודל גופן",
	"Font family": "גופן",
	"Insert format block": "מעוצב מראש",
	Normal: "רגיל",
	"Heading 1": "כותרת 1",
	"Heading 2": "כותרת 2",
	"Heading 3": "כותרת 3",
	"Heading 4": "כותרת 4",
	Quote: "ציטוט",
	Code: "קוד",
	Insert: "הכנס",
	"Insert table": "הכנס טבלה",
	"Decrease Indent": "הקטן כניסה",
	"Increase Indent": "הגדל כניסה",
	"Select Special Character": "בחר תו מיוחד",
	"Insert Special Character": "הכנס תו מיוחד",
	"Paint format": "העתק עיצוב",
	"Change mode": "החלף מצב",
	Margins: "ריווח",
	top: "עליון",
	right: "ימין",
	bottom: "תחתון",
	left: "שמאל",
	Styles: "עיצוב CSS",
	Classes: "מחלקת CSS",
	Align: "יישור",
	Right: "ימין",
	Center: "מרכז",
	Left: "שמאל",
	"--Not Set--": "--לא נקבע--",
	Src: "מקור",
	Title: "כותרת",
	Alternative: "כיתוב חלופי",
	Link: "קישור",
	"Open link in new tab": "פתח בכרטיסיה חדשה",
	Image: "תמונה",
	file: "קובץ",
	"Image properties": "מאפייני תמונה",
	Cancel: "ביטול",
	Ok: "אישור",
	"Your code is similar to HTML. Keep as HTML?": "הקוד דומה לHTML, האם להשאיר כHTML",
	"Paste as HTML": "הדבק כHTML",
	Keep: "השאר",
	Clean: "נקה",
	"Insert as Text": "הכנס כטקסט",
	"Word Paste Detected": "זוהתה הדבקה מ\"וורד\"",
	"The pasted content is coming from a Microsoft Word/Excel document. Do you want to keep the format or clean it up?": "התוכן המודבק מגיע ממסמך וורד/אקסל. האם ברצונך להשאיר את העיצוב או לנקותו",
	"Insert only Text": "הכנס טקסט בלבד",
	"File Browser": "סייר הקבצים",
	"Error on load list": "שגיאה  בזמן טעינת רשימה",
	"Error on load folders": "שגיאה בזמן טעינת תקיות",
	"Are you sure?": "האם אתה בטוח?",
	"Enter Directory name": "הכנס שם תקיה",
	"Create directory": "צור תקיה",
	"type name": "סוג הקובץ",
	"type dir name": "סוג הקובץ",
	"Drop image": "הסר תמונה",
	"Drop file": "הסר קובץ",
	"or click": "או לחץ",
	"Alternative text": "כיתוב חלופי",
	Browse: "סייר",
	Upload: "העלה",
	Background: "רקע",
	Text: "טקסט",
	Top: "עליון",
	Middle: "מרכז",
	Bottom: "תחתון",
	"Insert column before": "הכנס עמודה לפני",
	"Insert column after": "הכנס עמודה אחרי",
	"Insert row above": "הכנס שורה מעל",
	"Insert row below": "הכנס שורה מתחת",
	"Delete table": "מחק טבלה",
	"Delete row": "מחק שורה",
	"Delete column": "מחק עמודה",
	"Empty cell": "רוקן תא",
	Delete: "מחק",
	"Strike through": "קו חוצה",
	Underline: "קו תחתון",
	Break: "שבירת שורה",
	"Search for": "חפש",
	"Replace with": "החלף ב",
	Replace: "להחליף",
	Edit: "ערוך",
	"Vertical align": "יישור אנכי",
	"Horizontal align": "יישור אופקי",
	Filter: "סנן",
	"Sort by changed": "מין לפי שינוי",
	"Sort by name": "מיין לפי שם",
	"Sort by size": "מיין לפי גודל",
	"Add folder": "הוסף תקייה",
	Split: "פיצול",
	"Split vertical": "פיצול אנכי",
	"Split horizontal": "פיצול אופקי",
	Merge: "מזג",
	"Add column": "הוסף עמודה",
	"Add row": "הוסף שורה",
	Border: "מסגרת",
	"Embed code": "הוסף קוד",
	Update: "עדכן",
	superscript: "superscript",
	subscript: "subscript",
	"Cut selection": "גזור בחירה",
	Paste: "הדבק",
	"Choose Content to Paste": "בחר תוכן להדבקה",
	"Chars: %d": "תווים: %d",
	"Words: %d": "מילים: %d",
	All: "הכל",
	"Select %s": "נבחר: %s",
	"Select all": "בחר הכל",
	source: "HTML",
	bold: "מודגש",
	italic: "נטוי",
	brush: "מברשת",
	link: "קישור",
	undo: "בטל",
	redo: "בצע שוב",
	table: "טבלה",
	image: "תמונה",
	eraser: "מחק",
	paragraph: "פסקה",
	fontsize: "גודל גופן",
	video: "וידאו",
	font: "גופן",
	about: "עלינו",
	print: "הדפס",
	underline: "קו תחתון",
	strikethrough: "קו חוצה",
	indent: "הגדל כניסה",
	outdent: "הקטן כניסה",
	fullsize: "גודל מלא",
	shrink: "כווץ",
	hr: "קו אופקי",
	ul: "רשימת תבליטים",
	ol: "רשימה ממוספרת",
	cut: "חתוך",
	selectall: "בחר הכל",
	"Open link": "פתח קישור",
	"Edit link": "ערוך קישור",
	"No follow": "ללא מעקב",
	Unlink: "בטל קישור",
	Eye: "הצג",
	pencil: "כדי לערוך",
	" URL": "כתובת",
	Reset: "אפס",
	Save: "שמור",
	"Save as ...": "שמור בשם...",
	Resize: "שנה גודל",
	Crop: "חתוך",
	Width: "רוחב",
	Height: "גובה",
	"Keep Aspect Ratio": "שמור יחס",
	Yes: "כן",
	No: "לא",
	Remove: "הסר",
	Select: "בחר",
	"You can only edit your own images. Download this image on the host?": "רק קבצים המשוייכים שלך ניתנים לעריכה. האם להוריד את הקובץ?",
	"The image has been successfully uploaded to the host!": "התמונה עלתה בהצלחה!",
	palette: "לוח",
	"There are no files": "אין קבצים בספריה זו.",
	Rename: "הונגרית",
	"Enter new name": "הזן שם חדש",
	preview: "תצוגה מקדימה",
	download: "הורד",
	"Paste from clipboard": "להדביק מהלוח",
	"Your browser doesn't support direct access to the clipboard.": "הדפדפן שלך לא תומך גישה ישירה ללוח.",
	"Copy selection": "העתק בחירה",
	copy: "העתק",
	"Border radius": "רדיוס הגבול",
	"Show all": "הצג את כל",
	Apply: "החל",
	"Please fill out this field": "נא למלא שדה זה",
	"Please enter a web address": "אנא הזן כתובת אינטרנט",
	Default: "ברירת המחדל",
	Circle: "מעגל",
	Dot: "נקודה",
	Quadrate: "הריבוע הזה",
	Find: "למצוא",
	"Find Previous": "מצא את הקודם",
	"Find Next": "חפש את הבא",
	"Insert className": "הכנס את שם הכיתה",
	"Press Alt for custom resizing": "לחץ על אלט לשינוי גודל מותאם אישית",
	"License: %s": "רישיון: %s"
};
//#endregion
//#region node_modules/jodit/esm/langs/hu.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var hu_default = {
	"Type something": "Írjon be valamit",
	Advanced: "Haladó",
	"About Jodit": "Joditról",
	"Jodit Editor": "Jodit Editor",
	"Free Non-commercial Version": "Ingyenes változat",
	"Jodit User's Guide": "Jodit útmutató",
	"contains detailed help for using": "további segítséget tartalmaz",
	"For information about the license, please go to our website:": "További licence információkért látogassa meg a weboldalunkat:",
	"Buy full version": "Teljes verzió megvásárlása",
	"Copyright © XDSoft.net - Chupurnov Valeriy. All rights reserved.": "Copyright © XDSoft.net - Chupurnov Valeriy. Minden jog fenntartva.",
	Anchor: "Horgony",
	"Open in new tab": "Megnyitás új lapon",
	"Open in fullsize": "Megnyitás teljes méretben",
	"Clear Formatting": "Formázás törlése",
	"Fill color or set the text color": "Háttér/szöveg szín",
	Redo: "Újra",
	Undo: "Visszavon",
	Bold: "Félkövér",
	Italic: "Dőlt",
	"Insert Unordered List": "Pontozott lista",
	"Insert Ordered List": "Számozott lista",
	"Align Center": "Középre zárt",
	"Align Justify": "Sorkizárt",
	"Align Left": "Balra zárt",
	"Align Right": "Jobbra zárt",
	"Insert Horizontal Line": "Vízszintes vonal beszúrása",
	"Insert Image": "Kép beszúrás",
	"Insert file": "Fájl beszúrás",
	"Insert youtube/vimeo video": "Youtube videó beszúrása",
	"Insert link": "Link beszúrás",
	"Font size": "Betűméret",
	"Font family": "Betűtípus",
	"Insert format block": "Formázott blokk beszúrása",
	Normal: "Normál",
	"Heading 1": "Fejléc 1",
	"Heading 2": "Fejléc 2",
	"Heading 3": "Fejléc 3",
	"Heading 4": "Fejléc 4",
	Quote: "Idézet",
	Code: "Kód",
	Insert: "Beszúr",
	"Insert table": "Táblázat beszúrása",
	"Decrease Indent": "Behúzás csökkentése",
	"Increase Indent": "Behúzás növelése",
	"Select Special Character": "Speciális karakter kiválasztása",
	"Insert Special Character": "Speciális karakter beszúrása",
	"Paint format": "Kép formázása",
	"Change mode": "Nézet váltása",
	Print: "Nyomtatás",
	Margins: "Szegélyek",
	top: "felső",
	right: "jobb",
	bottom: "alsó",
	left: "bal",
	Styles: "CSS stílusok",
	Classes: "CSS osztályok",
	Align: "Igazítás",
	Right: "Jobbra",
	Center: "Középre",
	Left: "Balra",
	"--Not Set--": "Nincs",
	Src: "Forrás",
	Title: "Cím",
	Alternative: "Helyettesítő szöveg",
	Link: "Link",
	"Open link in new tab": "Link megnyitása új lapon",
	Image: "Kép",
	file: "Fájl",
	"Image properties": "Kép tulajdonságai",
	Cancel: "Mégsem",
	Ok: "OK",
	"Your code is similar to HTML. Keep as HTML?": "A beillesztett szöveg HTML-nek tűnik. Megtartsuk HTML-ként?",
	"Paste as HTML": "Beszúrás HTML-ként",
	Keep: "Megtartás",
	Clean: "Elvetés",
	"Insert as Text": "Beszúrás szövegként",
	"Word Paste Detected": "Word-ből másolt szöveg",
	"The pasted content is coming from a Microsoft Word/Excel document. Do you want to keep the format or clean it up?": "A beillesztett tartalom Microsoft Word/Excel dokumentumból származik. Meg szeretné tartani a formátumát?",
	"Insert only Text": "Csak szöveg beillesztése",
	"File Browser": "Fájl tallózó",
	"Error on load list": "Hiba a lista betöltése közben",
	"Error on load folders": "Hiba a mappák betöltése közben",
	"Are you sure?": "Biztosan ezt szeretné?",
	"Enter Directory name": "Írjon be egy mappanevet",
	"Create directory": "Mappa létrehozása",
	"type name": "írjon be bevet",
	"type dir name": "írjon be bevet",
	"Drop image": "Húzza ide a képet",
	"Drop file": "Húzza ide a fájlt",
	"or click": "vagy kattintson",
	"Alternative text": "Helyettesítő szöveg",
	Browse: "Tallóz",
	Upload: "Feltölt",
	Background: "Háttér",
	Text: "Szöveg",
	Top: "Fent",
	Middle: "Középen",
	Bottom: "Lent",
	"Insert column before": "Oszlop beszúrás elé",
	"Insert column after": "Oszlop beszúrás utána",
	"Insert row above": "Sor beszúrás fölé",
	"Insert row below": "Sor beszúrás alá",
	"Delete table": "Táblázat törlése",
	"Delete row": "Sor törlése",
	"Delete column": "Oszlop törlése",
	"Empty cell": "Cella tartalmának törlése",
	Delete: "Törlés",
	"Strike through": "Áthúzott",
	Underline: "Aláhúzott",
	Break: "Szünet",
	"Search for": "Keresés",
	"Replace with": "Csere erre",
	Replace: "Cserélje ki",
	Edit: "Szerkeszt",
	"Vertical align": "Függőleges igazítás",
	"Horizontal align": "Vízszintes igazítás",
	Filter: "Szűrő",
	"Sort by changed": "Rendezés módosítás szerint",
	"Sort by name": "Rendezés név szerint",
	"Sort by size": "Rendezés méret szerint",
	"Add folder": "Mappa hozzáadás",
	"Split vertical": "Függőleges felosztás",
	"Split horizontal": "Vízszintes felosztás",
	Merge: "Összevonás",
	"Add column": "Oszlop hozzáadás",
	"Add row": "Sor hozzáadás",
	Border: "Szegély",
	"Embed code": "Beágyazott kód",
	Update: "Frissít",
	superscript: "Felső index",
	subscript: "Alsó index",
	"Cut selection": "Kivágás",
	Paste: "Beillesztés",
	"Choose Content to Paste": "Válasszon tartalmat a beillesztéshez",
	Split: "Felosztás",
	"Chars: %d": "Karakterek száma: %d",
	"Words: %d": "Szavak száma: %d",
	All: "Összes",
	"Select %s": "Kijelöl: %s",
	"Select all": "Összes kijelölése",
	source: "HTML",
	bold: "Félkövér",
	italic: "Dőlt",
	brush: "Ecset",
	link: "Link",
	undo: "Visszavon",
	redo: "Újra",
	table: "Táblázat",
	image: "Kép",
	eraser: "Törlés",
	paragraph: "Paragráfus",
	fontsize: "Betűméret",
	video: "Videó",
	font: "Betű",
	about: "Rólunk",
	print: "Nyomtat",
	underline: "Aláhúzott",
	strikethrough: "Áthúzott",
	indent: "Behúzás",
	outdent: "Aussenseiter",
	fullsize: "Teljes méret",
	shrink: "Összenyom",
	hr: "Egyenes vonal",
	ul: "Lista",
	ol: "Számozott lista",
	cut: "Kivág",
	selectall: "Összes kijelölése",
	"Open link": "Link megnyitása",
	"Edit link": "Link szerkesztése",
	"No follow": "Nincs követés",
	Unlink: "Link leválasztása",
	Eye: "felülvizsgálat",
	pencil: "Szerkesztés",
	" URL": "URL",
	Reset: "Visszaállít",
	Save: "Mentés",
	"Save as ...": "Mentés másként...",
	Resize: "Átméretezés",
	Crop: "Kivág",
	Width: "Szélesség",
	Height: "Magasság",
	"Keep Aspect Ratio": "Képarány megtartása",
	Yes: "Igen",
	No: "Nem",
	Remove: "Eltávolít",
	Select: "Kijelöl",
	"You can only edit your own images. Download this image on the host?": "Csak a saját képeit tudja szerkeszteni. Letölti ezt a képet?",
	"The image has been successfully uploaded to the host!": "Kép sikeresen feltöltve!",
	palette: "Palette",
	"There are no files": "Er zijn geen bestanden in deze map.",
	Rename: "átnevezés",
	"Enter new name": "Adja meg az új nevet",
	preview: "előnézet",
	download: "Letöltés",
	"Paste from clipboard": "Illessze be a vágólap",
	"Your browser doesn't support direct access to the clipboard.": "A böngésző nem támogatja a közvetlen hozzáférést biztosít a vágólapra.",
	"Copy selection": "Másolás kiválasztása",
	copy: "másolás",
	"Border radius": "Határ sugár",
	"Show all": "Összes",
	Apply: "Alkalmazni",
	"Please fill out this field": "Kérjük, töltse ki ezt a mezőt,",
	"Please enter a web address": "Kérjük, írja be a webcímet",
	Default: "Alapértelmezett",
	Circle: "Kör",
	Dot: "Pont",
	Quadrate: "Quadrate",
	Find: "Találni",
	"Find Previous": "Megtalálja Előző",
	"Find Next": "Következő Keresése",
	"Insert className": "Helyezze be az osztály nevét",
	"Press Alt for custom resizing": "Nyomja meg az Alt egyéni átméretezés",
	"License: %s": "Licenc: %s"
};
//#endregion
//#region node_modules/jodit/esm/langs/id.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var id_default = {
	"Type something": "Ketik sesuatu",
	"About Jodit": "Tentang Jodit",
	"Jodit Editor": "Editor Jodit",
	"Free Non-commercial Version": "Versi Bebas Non-komersil",
	"Jodit User's Guide": "Panduan Pengguna Jodit",
	"contains detailed help for using": "mencakup detail bantuan penggunaan",
	"For information about the license, please go to our website:": "Untuk informasi tentang lisensi, silakan kunjungi website:",
	"Buy full version": "Beli versi lengkap",
	"Copyright © XDSoft.net - Chupurnov Valeriy. All rights reserved.": "Hak Cipta © XDSoft.net - Chupurnov Valeriy. Hak cipta dilindungi undang-undang.",
	Anchor: "Tautan",
	"Open in new tab": "Buka di tab baru",
	"Open in fullsize": "Buka editor dalam ukuran penuh",
	"Clear Formatting": "Hapus Pemformatan",
	"Fill color or set the text color": "Isi warna atau atur warna teks",
	Redo: "Ulangi",
	Undo: "Batalkan",
	Bold: "Tebal",
	Italic: "Miring",
	"Insert Unordered List": "Sisipkan Daftar Tidak Berurut",
	"Insert Ordered List": "Sisipkan Daftar Berurut",
	"Align Center": "Tengah",
	"Align Justify": "Penuh",
	"Align Left": "Kiri",
	"Align Right": "Kanan",
	"Insert Horizontal Line": "Sisipkan Garis Horizontal",
	"Insert Image": "Sisipkan Gambar",
	"Insert file": "Sisipkan Berkas",
	"Insert youtube/vimeo video": "Sisipkan video youtube/vimeo",
	"Insert link": "Sisipkan tautan",
	"Font size": "Ukuran font",
	"Font family": "Keluarga font",
	"Insert format block": "Sisipkan blok format",
	Normal: "Normal",
	"Heading 1": "Heading 1",
	"Heading 2": "Heading 2",
	"Heading 3": "Heading 3",
	"Heading 4": "Heading 4",
	Quote: "Kutip",
	Code: "Kode",
	Insert: "Sisipkan",
	"Insert table": "Sisipkan tabel",
	"Decrease Indent": "Kurangi Indentasi",
	"Increase Indent": "Tambah Indentasi",
	"Select Special Character": "Pilih Karakter Spesial",
	"Insert Special Character": "Sisipkan Karakter Spesial",
	"Paint format": "Formar warna",
	"Change mode": "Ubah mode",
	Margins: "Batas",
	top: "atas",
	right: "kanan",
	bottom: "bawah",
	left: "kiri",
	Styles: "Gaya",
	Classes: "Class",
	Align: "Rata",
	Right: "Kanan",
	Center: "Tengah",
	Left: "Kiri",
	"--Not Set--": "--Tidak diset--",
	Src: "Src",
	Title: "Judul",
	Alternative: "Teks alternatif",
	Link: "Tautan",
	"Open link in new tab": "Buka tautan di tab baru",
	Image: "Gambar",
	file: "berkas",
	Advanced: "Lanjutan",
	"Image properties": "Properti gambar",
	Cancel: "Batal",
	Ok: "Ya",
	"Your code is similar to HTML. Keep as HTML?": "Kode Anda cenderung ke HTML. Biarkan sebagai HTML?",
	"Paste as HTML": "Paste sebagai HTML",
	Keep: "Jaga",
	Clean: "Bersih",
	"Insert as Text": "Sisipkan sebagai teks",
	"Insert only Text": "Sisipkan hanya teks",
	"Word Paste Detected": "Terdeteksi paste dari Word",
	"The pasted content is coming from a Microsoft Word/Excel document. Do you want to keep the format or clean it up?": "Konten dipaste dari dokumen Microsoft Word/Excel. Apakah Anda ingin tetap menjaga format atau membersihkannya?",
	"File Browser": "Penjelajah Berkas",
	"Error on load list": "Error ketika memuat list",
	"Error on load folders": "Error ketika memuat folder",
	"Are you sure?": "Apakah Anda yakin?",
	"Enter Directory name": "Masukkan nama Direktori",
	"Create directory": "Buat direktori",
	"type name": "ketik nama",
	"type dir name": "ketik nama",
	"Drop image": "Letakkan gambar",
	"Drop file": "Letakkan berkas",
	"or click": "atau klik",
	"Alternative text": "Teks alternatif",
	Browse: "Jelajahi",
	Upload: "Unggah",
	Background: "Latar Belakang",
	Text: "Teks",
	Top: "Atas",
	Middle: "Tengah",
	Bottom: "Bawah",
	"Insert column before": "Sisipkan kolom sebelumnya",
	"Insert column after": "Sisipkan kolom setelahnya",
	"Insert row above": "Sisipkan baris di atasnya",
	"Insert row below": "Sisipkan baris di bawahnya",
	"Delete table": "Hapus tabel",
	"Delete row": "Hapus baris",
	"Delete column": "Hapus kolom",
	"Empty cell": "Kosongkan cell",
	source: "sumber",
	bold: "tebal",
	italic: "miring",
	brush: "sikat",
	link: "tautan",
	undo: "batalkan",
	redo: "ulangi",
	table: "tabel",
	image: "gambar",
	eraser: "penghapus",
	paragraph: "paragraf",
	fontsize: "ukuran font",
	video: "video",
	font: "font",
	about: "tentang",
	print: "cetak",
	underline: "garis bawah",
	strikethrough: "coret",
	indent: "menjorok ke dalam",
	outdent: "menjorok ke luar",
	fullsize: "ukuran penuh",
	shrink: "menyusut",
	hr: "hr",
	ul: "ul",
	ol: "ol",
	cut: "potong",
	selectall: "Pilih semua",
	"Embed code": "Kode embed",
	"Open link": "Buka tautan",
	"Edit link": "Edit tautan",
	"No follow": "No follow",
	Unlink: "Hapus tautan",
	Eye: "Mata",
	pencil: "pensil",
	Update: "Perbarui",
	" URL": "URL",
	Edit: "Edit",
	"Horizontal align": "Perataan horizontal",
	Filter: "Filter",
	"Sort by changed": "Urutkan berdasarkan perubahan",
	"Sort by name": "Urutkan berdasarkan nama",
	"Sort by size": "Urutkan berdasarkan ukuran",
	"Add folder": "Tambah folder",
	Reset: "Reset",
	Save: "Simpan",
	"Save as ...": "Simpan sebagai...",
	Resize: "Ubah ukuran",
	Crop: "Crop",
	Width: "Lebar",
	Height: "Tinggi",
	"Keep Aspect Ratio": "Jaga aspek rasio",
	Yes: "Ya",
	No: "Tidak",
	Remove: "Copot",
	Select: "Pilih",
	"Chars: %d": "Karakter: %d",
	"Words: %d": "Kata: %d",
	All: "Semua",
	"Select %s": "Pilih %s",
	"Select all": "Pilih semua",
	"Vertical align": "Rata vertikal",
	Split: "Bagi",
	"Split vertical": "Bagi secara vertikal",
	"Split horizontal": "Bagi secara horizontal",
	Merge: "Gabungkan",
	"Add column": "Tambah kolom",
	"Add row": "tambah baris",
	Delete: "Hapus",
	Border: "Bingkai",
	"License: %s": "Lisensi: %s",
	"Strike through": "Coret",
	Underline: "Garis Bawah",
	superscript: "Superskrip",
	subscript: "Subskrip",
	"Cut selection": "Potong pilihan",
	Break: "Berhenti",
	"Search for": "Mencari",
	"Replace with": "Ganti dengan",
	Replace: "Mengganti",
	Paste: "Paste",
	"Choose Content to Paste": "Pilih konten untuk dipaste",
	"You can only edit your own images. Download this image on the host?": "Anda hanya dapat mengedit gambar Anda sendiri. Unduh gambar ini di host?",
	"The image has been successfully uploaded to the host!": "Gambar telah sukses diunggah ke host!",
	palette: "palet",
	"There are no files": "Tidak ada berkas",
	Rename: "ganti nama",
	"Enter new name": "Masukkan nama baru",
	preview: "pratinjau",
	download: "Unduh",
	"Paste from clipboard": "Paste dari clipboard",
	"Your browser doesn't support direct access to the clipboard.": "Browser anda tidak mendukung akses langsung ke clipboard.",
	"Copy selection": "Copy seleksi",
	copy: "copy",
	"Border radius": "Border radius",
	"Show all": "Tampilkan semua",
	Apply: "Menerapkan",
	"Please fill out this field": "Silahkan mengisi kolom ini",
	"Please enter a web address": "Silahkan masukkan alamat web",
	Default: "Default",
	Circle: "Lingkaran",
	Dot: "Dot",
	Quadrate: "Kuadrat",
	Find: "Menemukan",
	"Find Previous": "Menemukan Sebelumnya",
	"Find Next": "Menemukan Berikutnya",
	"Insert className": "Masukkan nama kelas",
	"Press Alt for custom resizing": "Tekan Alt untuk mengubah ukuran kustom"
};
//#endregion
//#region node_modules/jodit/esm/langs/it.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var it_default = {
	"Type something": "Scrivi qualcosa...",
	Advanced: "Avanzato",
	"About Jodit": "A proposito di Jodit",
	"Jodit Editor": "Jodit Editor",
	"Jodit User's Guide": "Guida utente di Jodit",
	"contains detailed help for using": "contiene una guida dettagliata per l'uso.",
	"For information about the license, please go to our website:": "Per informazioni sulla licenza, si prega di visitare il nostro sito web:",
	"Buy full version": "Acquista la versione completa",
	"Copyright © XDSoft.net - Chupurnov Valeriy. All rights reserved.": "Copyright © XDSoft.net - Chupurnov Valeriy. Tutti i diritti riservati.",
	Anchor: "Link",
	"Open in new tab": "Apri in una nuova scheda",
	"Open in fullsize": "Apri l'editor a schermo intero",
	"Clear Formatting": "Pulisci Formattazione",
	"Fill color or set the text color": "Colore di sfondo o del testo",
	Redo: "Ripristina",
	Undo: "Annulla",
	Bold: "Grassetto",
	Italic: "Corsivo",
	"Insert Unordered List": "Inserisci lista non ordinata",
	"Insert Ordered List": "Inserisci lista ordinata",
	"Align Center": "Allinea al centro",
	"Align Justify": "Allineamento Giustificato",
	"Align Left": "Allinea a Sinistra",
	"Align Right": "Allinea a Destra",
	"Insert Horizontal Line": "Inserisci una linea orizzontale",
	"Insert Image": "Inserisci immagine",
	"Insert file": "Inserisci un file",
	"Insert youtube/vimeo video": "Inserisci video Youtube/Vimeo",
	"Insert link": "Inserisci link",
	"Font size": "Dimensione carattere",
	"Font family": "Tipo di font",
	"Insert format block": "Inserisci blocco",
	Normal: "Normale",
	"Heading 1": "Intestazione 1",
	"Heading 2": "Intestazione 2",
	"Heading 3": "Intestazione 3",
	"Heading 4": "Intestazione 4",
	Quote: "Citazione",
	Code: "Codice",
	Insert: "Inserisci",
	"Insert table": "Inserisci tabella",
	"Decrease Indent": "Riduci il rientro",
	"Increase Indent": "Aumenta il rientro",
	"Select Special Character": "Seleziona un carattere speciale",
	"Insert Special Character": "Inserisci un carattere speciale",
	"Paint format": "Copia formato",
	"Change mode": "Cambia modalita'",
	Margins: "Margini",
	top: "su",
	right: "destra",
	bottom: "giù",
	left: "sinistra",
	Styles: "Stili CSS",
	Classes: "Classi CSS",
	Align: "Allinea",
	Right: "Destra",
	Center: "Centro",
	Left: "Sinistra",
	"--Not Set--": "--Non Impostato--",
	Src: "Fonte",
	Title: "Titolo",
	Alternative: "Testo Alternativo",
	Link: "Link",
	"Open link in new tab": "Apri il link in una nuova scheda",
	Image: "Immagine",
	file: "Archivio",
	"Image properties": "Proprietà dell'immagine",
	Cancel: "Annulla",
	Ok: "Accetta",
	"Your code is similar to HTML. Keep as HTML?": "Il codice è simile all'HTML. Mantieni come HTML?",
	"Paste as HTML": "Incolla come HTML",
	Keep: "Mantieni",
	Clean: "Pulisci",
	"Insert as Text": "Inserisci come testo",
	"Word Paste Detected": "Incolla testo da Word rilevato",
	"The pasted content is coming from a Microsoft Word/Excel document. Do you want to keep the format or clean it up?": "Il contenuto incollato proviene da un documento Microsoft Word / Excel. Vuoi mantenere il formato o pulirlo?",
	"Insert only Text": "Inserisci solo il testo",
	"File Browser": "Cerca file",
	"Error on load list": "Errore durante il caricamento dell'elenco",
	"Error on load folders": "Errore durante il caricamento delle cartelle",
	"Are you sure?": "Sei sicuro?",
	"Enter Directory name": "Inserisci il nome della cartella",
	"Create directory": "Crea cartella",
	"type name": "Digita il nome",
	"type dir name": "Digita il nome",
	"Drop image": "Cancella immagine",
	"Drop file": "Cancella file",
	"or click": "o clicca",
	"Alternative text": "Testo alternativo",
	Browse: "Sfoglia",
	Upload: "Carica",
	Background: "Sfondo",
	Text: "Testo",
	Top: "Su",
	Middle: "Centro",
	Bottom: "Sotto",
	"Insert column before": "Inserisci la colonna prima",
	"Insert column after": "Inserisci la colonna dopo",
	"Insert row above": "Inserisci la riga sopra",
	"Insert row below": "Inserisci la riga sotto",
	"Delete table": "Elimina tabella",
	"Delete row": "Elimina riga",
	"Delete column": "Elimina colonna",
	"Empty cell": "Cella vuota",
	Delete: "Cancella",
	"Strike through": "Barrato",
	Underline: "Sottolineato",
	Break: "Pausa",
	"Search for": "Cerca per",
	"Replace with": "Sostituisci con",
	Replace: "Sostituisci",
	Edit: "Modifica",
	"Vertical align": "Allineamento verticala",
	"Horizontal align": "Allineamento orizzontale",
	Filter: "Filtro",
	"Sort by changed": "Ordina per data di modifica",
	"Sort by name": "Ordina per nome",
	"Sort by size": "Ordina per dimensione",
	"Add folder": "Aggiungi cartella",
	Split: "Dividi",
	"Split vertical": "Dividi verticalmente",
	"Split horizontal": "Dividi orizzontale",
	Merge: "Fondi",
	"Add column": "Aggiungi colonna",
	"Add row": "Aggiungi riga",
	Border: "Bordo",
	"Embed code": "Includi codice",
	Update: "Aggiorna",
	superscript: "indice",
	subscript: "pedice",
	"Cut selection": "Taglia selezione",
	Paste: "Incolla",
	"Choose Content to Paste": "Seleziona il contenuto da incollare",
	"Chars: %d": "Caratteri: %d",
	"Words: %d": "Parole: %d",
	All: "Tutto",
	"Select %s": "Seleziona: %s",
	"Select all": "Seleziona tutto",
	source: "risorsa",
	bold: "Grassetto",
	italic: "Corsivo",
	brush: "Pennello",
	link: "Link",
	undo: "Annulla",
	redo: "Ripristina",
	table: "Tabella",
	image: "Immagine",
	eraser: "Gomma",
	paragraph: "Paragrafo",
	fontsize: "Dimensione del carattere",
	video: "Video",
	font: "Font",
	about: "Approposito di",
	print: "Stampa",
	underline: "Sottolineato",
	strikethrough: "Barrato",
	indent: "aumenta rientro",
	outdent: "riduci rientro",
	fullsize: "espandi",
	shrink: "comprimi",
	hr: "linea orizzontale",
	ul: "lista non ordinata",
	ol: "lista ordinata",
	cut: "Taglia",
	selectall: "Seleziona tutto",
	"Open link": "Apri link",
	"Edit link": "Modifica link",
	"No follow": "Non seguire",
	Unlink: "Rimuovi link",
	Eye: "Recensione",
	pencil: "Per modificare",
	" URL": " URL",
	Reset: "Reset",
	Save: "Salva",
	"Save as ...": "Salva con nome...",
	Resize: "Ridimensiona",
	Crop: "Ritaglia",
	Width: "Larghezza",
	Height: "Altezza",
	"Keep Aspect Ratio": "Mantieni le proporzioni",
	Yes: "Si",
	No: "No",
	Remove: "Rimuovi",
	Select: "Seleziona",
	"You can only edit your own images. Download this image on the host?": "Puoi modificare solo le tue immagini. Vuoi scaricare questa immagine dal server?",
	"The image has been successfully uploaded to the host!": "L'immagine è stata caricata correttamente sul server!",
	palette: "tavolozza",
	"There are no files": "Non ci sono file in questa directory.",
	Rename: "Rinomina",
	"Enter new name": "Inserisci un nuovo nome",
	preview: "anteprima",
	download: "Scarica",
	"Paste from clipboard": "Incolla dagli appunti",
	"Your browser doesn't support direct access to the clipboard.": "Il tuo browser non supporta l'accesso diretto agli appunti.",
	"Copy selection": "Copia selezione",
	copy: "copia",
	"Border radius": "Border radius",
	"Show all": "Mostra tutti",
	Apply: "Applica",
	"Please fill out this field": "Si prega di compilare questo campo",
	"Please enter a web address": "Si prega di inserire un indirizzo web",
	Default: "Default",
	Circle: "Cerchio",
	Dot: "Punto",
	Quadrate: "Quadrato",
	"Lower Alpha": "Lettera Minuscola",
	"Lower Greek": "Lettera Greca Minuscola",
	"Lower Roman": "Numero Romano Minuscolo",
	"Upper Alpha": "Lettera Maiuscola",
	"Upper Roman": "Numero Romano Maiuscolo",
	Find: "Trova",
	"Find Previous": "Trova Precedente",
	"Find Next": "Trova Successivo",
	"Insert className": "Inserisci il nome della classe",
	"Press Alt for custom resizing": "Premere Alt per il ridimensionamento personalizzato",
	"License: %s": "Licenza: %s"
};
//#endregion
//#region node_modules/jodit/esm/langs/ja.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var ja_default = {
	"Type something": "なにかタイプしてください",
	Advanced: "高度な設定",
	"About Jodit": "Joditについて",
	"Jodit Editor": "Jodit Editor",
	"Jodit User's Guide": "Jodit ユーザーズ・ガイド",
	"contains detailed help for using": "詳しい使い方",
	"For information about the license, please go to our website:": "ライセンス詳細についてはJodit Webサイトを確認ください：",
	"Buy full version": "フルバージョンを購入",
	"Copyright © XDSoft.net - Chupurnov Valeriy. All rights reserved.": "Copyright © XDSoft.net - Chupurnov Valeriy. All rights reserved.",
	Anchor: "Anchor",
	"Open in new tab": "新しいタブで開く",
	"Open in fullsize": "エディターのサイズ（フル/ノーマル）",
	"Clear Formatting": "書式をクリア",
	"Fill color or set the text color": "テキストの色",
	Redo: "やり直し",
	Undo: "元に戻す",
	Bold: "太字",
	Italic: "斜体",
	"Insert Unordered List": "箇条書き",
	"Insert Ordered List": "番号付きリスト",
	"Align Center": "中央揃え",
	"Align Justify": "両端揃え",
	"Align Left": "左揃え",
	"Align Right": "右揃え",
	"Insert Horizontal Line": "区切り線を挿入",
	"Insert Image": "画像を挿入",
	"Insert file": "ファイルを挿入",
	"Insert youtube/vimeo video": "Youtube/Vimeo 動画",
	"Insert link": "リンクを挿入",
	"Font size": "フォントサイズ",
	"Font family": "フォント",
	"Insert format block": "テキストのスタイル",
	Normal: "指定なし",
	"Heading 1": "タイトル1",
	"Heading 2": "タイトル2",
	"Heading 3": "タイトル3",
	"Heading 4": "タイトル4",
	Quote: "引用",
	Code: "コード",
	Insert: "挿入",
	"Insert table": "表を挿入",
	"Decrease Indent": "インデント減",
	"Increase Indent": "インデント増",
	"Select Special Character": "特殊文字を選択",
	"Insert Special Character": "特殊文字を挿入",
	"Paint format": "書式を貼付け",
	"Change mode": "編集モード切替え",
	Margins: "マージン",
	top: "上",
	right: "右",
	bottom: "下",
	left: "左",
	Styles: "スタイル",
	Classes: "クラス",
	Align: "配置",
	Right: "右寄せ",
	Center: "中央寄せ",
	Left: "左寄せ",
	"--Not Set--": "指定なし",
	Src: "ソース",
	Title: "タイトル",
	Alternative: "代替テキスト",
	Link: "リンク",
	"Open link in new tab": "新しいタブで開く",
	Image: "画像",
	file: "ファイル",
	"Image properties": "画像のプロパティー",
	Cancel: "キャンセル",
	Ok: "確定",
	"Your code is similar to HTML. Keep as HTML?": "HTMLコードを保持しますか？",
	"Paste as HTML": "HTMLで貼付け",
	Keep: "HTMLを保持",
	Clean: "Clean",
	"Insert as Text": "HTMLをテキストにする",
	"Word Paste Detected": "Word Paste Detected",
	"The pasted content is coming from a Microsoft Word/Excel document. Do you want to keep the format or clean it up?": "The pasted content is coming from a Microsoft Word/Excel document. Do you want to keep the format or clean it up?",
	"Insert only Text": "テキストだけ",
	"File Browser": "File Browser",
	"Error on load list": "Error on load list",
	"Error on load folders": "Error on load folders",
	"Are you sure?": "Are you sure?",
	"Enter Directory name": "Enter Directory name",
	"Create directory": "Create directory",
	"type name": "type name",
	"type dir name": "type name",
	"Drop image": "ここに画像をドロップ",
	"Drop file": "ここにファイルをドロップ",
	"or click": "or クリック",
	"Alternative text": "代替テキスト",
	Browse: "ブラウズ",
	Upload: "アップロード",
	Background: "背景",
	Text: "文字",
	Top: "上",
	Middle: "中央",
	Bottom: "下",
	"Insert column before": "左に列を挿入",
	"Insert column after": "右に列を挿入",
	"Insert row above": "上に行を挿入",
	"Insert row below": "下に行を挿入",
	"Delete table": "表を削除",
	"Delete row": "行を削除",
	"Delete column": "列を削除",
	"Empty cell": "セルを空にする",
	"Chars: %d": "文字数: %d",
	"Words: %d": "単語数: %d",
	"Strike through": "取り消し線",
	Underline: "下線",
	superscript: "上付き文字",
	subscript: "下付き文字",
	"Cut selection": "切り取り",
	"Select all": "すべて選択",
	Break: "Pause",
	"Search for": "検索",
	"Replace with": "置換",
	Replace: "交換",
	Paste: "貼付け",
	"Choose Content to Paste": "選択した内容を貼付け",
	All: "全部",
	source: "source",
	bold: "bold",
	italic: "italic",
	brush: "brush",
	link: "link",
	undo: "undo",
	redo: "redo",
	table: "table",
	image: "image",
	eraser: "eraser",
	paragraph: "paragraph",
	fontsize: "fontsize",
	video: "video",
	font: "font",
	about: "about",
	print: "print",
	underline: "underline",
	strikethrough: "strikethrough",
	indent: "indent",
	outdent: "outdent",
	fullsize: "fullsize",
	shrink: "shrink",
	hr: "分割線",
	ul: "箇条書き",
	ol: "番号付きリスト",
	cut: "切り取り",
	selectall: "すべて選択",
	"Open link": "リンクを開く",
	"Edit link": "リンクを編集",
	"No follow": "No follow",
	Unlink: "リンク解除",
	Eye: "サイトを確認",
	" URL": "URL",
	Reset: "リセット",
	Save: "保存",
	"Save as ...": "Save as ...",
	Resize: "リサイズ",
	Crop: "Crop",
	Width: "幅",
	Height: "高さ",
	"Keep Aspect Ratio": "縦横比を保持",
	Yes: "はい",
	No: "いいえ",
	Remove: "移除",
	Select: "選択",
	"Select %s": "選択: %s",
	Update: "更新",
	"Vertical align": "垂直方向の配置",
	Merge: "セルの結合",
	"Add column": "列を追加",
	"Add row": "行を追加",
	Border: "境界線",
	"Embed code": "埋め込みコード",
	Delete: "削除",
	Edit: "編集",
	"Horizontal align": "水平方向の配置",
	Filter: "Filter",
	"Sort by changed": "Sort by changed",
	"Sort by name": "Sort by name",
	"Sort by size": "Sort by size",
	"Add folder": "Add folder",
	Split: "分割",
	"Split vertical": "セルの分割（垂直方向）",
	"Split horizontal": "セルの分割（水平方向）",
	"You can only edit your own images. Download this image on the host?": "You can only edit your own images. Download this image on the host?",
	"The image has been successfully uploaded to the host!": "The image has been successfully uploaded to the host!",
	palette: "パレット",
	pencil: "鉛筆",
	"There are no files": "There are no files",
	Rename: "Rename",
	"Enter new name": "Enter new name",
	preview: "プレビュー",
	download: "ダウンロード",
	"Paste from clipboard": "貼り付け",
	"Your browser doesn't support direct access to the clipboard.": "お使いのブラウザはクリップボードを使用できません",
	"Copy selection": "コピー",
	copy: "copy",
	"Border radius": "角の丸み",
	"Show all": "全て表示",
	Apply: "適用",
	"Please fill out this field": "まだこの分野",
	"Please enter a web address": "を入力してくださいウェブアドレス",
	Default: "デフォルト",
	Circle: "白丸",
	Dot: "黒丸",
	Quadrate: "四角",
	Find: "見",
	"Find Previous": "探前",
	"Find Next": "由来",
	"Lower Alpha": "英小文字",
	"Lower Greek": "ギリシャ文字",
	"Lower Roman": "ローマ数字小文字",
	"Upper Alpha": "英大文字",
	"Upper Roman": "ローマ数字大文字",
	"Insert className": "クラス名を挿入",
	"Press Alt for custom resizing": "カスタムサイズ変更のためのAltキーを押します",
	"License: %s": "ライセンス: %s"
};
//#endregion
//#region node_modules/jodit/esm/langs/keys.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var keys_default = {};
//#endregion
//#region node_modules/jodit/esm/langs/ko.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var ko_default = {
	"Type something": "무엇이든 입력하세요",
	"About Jodit": "Jodit에 대하여",
	"Jodit Editor": "Jodit Editor",
	"Jodit User's Guide": "Jodit 사용자 안내서",
	"contains detailed help for using": "자세한 도움말이 들어있어요",
	"For information about the license, please go to our website:": "라이센스에 관해서는 Jodit 웹 사이트를 방문해주세요：",
	"Buy full version": "풀 버전 구입하기",
	"Copyright © XDSoft.net - Chupurnov Valeriy. All rights reserved.": "© XDSoft.net - Chupurnov Valeriy. 에게 저작권과 모든 권리가 있습니다.",
	Anchor: "Anchor",
	"Open in new tab": "새 탭에서 열기",
	"Open in fullsize": "전체 크기로 보기",
	"Clear Formatting": "서식 지우기",
	"Fill color or set the text color": "글씨 색상",
	Redo: "재실행",
	Undo: "실행 취소",
	Bold: "굵게",
	Italic: "기울임",
	"Insert Unordered List": "글머리 목록",
	"Insert Ordered List": "번호 목록",
	"Align Center": "가운데 정렬",
	"Align Justify": "양쪽 정렬",
	"Align Left": "왼쪽 정렬",
	"Align Right": "오른쪽 정렬",
	"Insert Horizontal Line": "수평 구분선 넣기",
	"Insert Image": "이미지 넣기",
	"Insert file": "파일 넣기",
	"Insert youtube/vimeo video": "Youtube/Vimeo 동영상",
	"Insert link": "링크 넣기",
	"Font size": "글꼴 크기",
	"Font family": "글꼴",
	"Insert format block": "블록 요소 넣기",
	Normal: "일반 텍스트",
	"Heading 1": "제목 1",
	"Heading 2": "제목 2",
	"Heading 3": "제목 3",
	"Heading 4": "제목 4",
	Quote: "인용",
	Code: "코드",
	Insert: "붙여 넣기",
	"Insert table": "테이블",
	"Decrease Indent": "들여쓰기 감소",
	"Increase Indent": "들여쓰기 증가",
	"Select Special Character": "특수문자 선택",
	"Insert Special Character": "특수문자 입력",
	"Paint format": "페인트 형식",
	"Change mode": "편집모드 변경",
	Margins: "마진",
	top: "위",
	right: "오른쪽",
	bottom: "아래",
	left: "왼쪽",
	Styles: "스타일",
	Classes: "클래스",
	Align: "정렬",
	Right: "오른쪽으로",
	Center: "가운데로",
	Left: "왼쪽으로",
	"--Not Set--": "--지정 안 함--",
	Src: "경로(src)",
	Title: "제목",
	Alternative: "대체 텍스트(alt)",
	Link: "링크",
	"Open link in new tab": "새 탭에서 열기",
	file: "파일",
	Advanced: "고급",
	"Image properties": "이미지 속성",
	Cancel: "취소",
	Ok: "확인",
	"Your code is similar to HTML. Keep as HTML?": "HTML 코드로 감지했어요. 코드인채로 붙여넣을까요?",
	"Paste as HTML": "HTML로 붙여넣기",
	Keep: "원본 유지",
	Clean: "지우기",
	"Insert as Text": "텍스트로 넣기",
	"Insert only Text": "텍스트만 넣기",
	"Word Paste Detected": "Word 붙여넣기 감지",
	"The pasted content is coming from a Microsoft Word/Excel document. Do you want to keep the format or clean it up?": "Microsoft Word/Excel 문서로 감지했어요. 서식을 유지한채로 붙여넣을까요?",
	"File Browser": "파일 탐색기",
	"Error on load list": "목록 불러오기 에러",
	"Error on load folders": "폴더 불러오기",
	"Are you sure?": "정말 진행할까요?",
	"Enter Directory name": "디렉토리 이름 입력",
	"Create directory": "디렉토리 생성",
	"type name": "이름 입력",
	"type dir name": "이름 입력",
	"Drop image": "이미지 드래그",
	"Drop file": "파일 드래그",
	"or click": "혹은 클릭",
	"Alternative text": "대체 텍스트",
	Browse: "탐색",
	Upload: "업로드",
	Background: "배경",
	Text: "텍스트",
	Top: "위",
	Middle: "중앙",
	Bottom: "아래",
	"Insert column before": "이전 열에 삽입",
	"Insert column after": "다음 열에 삽입",
	"Insert row above": "위 행에 삽입",
	"Insert row below": "아래 행에 삽입",
	"Delete table": "테이블 삭제",
	"Delete row": "행 삭제",
	"Delete column": "열 삭제",
	"Empty cell": "빈 셀",
	source: "HTML 소스",
	bold: "볼드",
	italic: "이탤릭",
	brush: "브러시",
	link: "링크",
	undo: "실행 취소",
	redo: "재실행",
	table: "테이블",
	image: "이미지",
	eraser: "지우개",
	paragraph: "문단",
	fontsize: "글꼴 크기",
	video: "비디오",
	font: "글꼴",
	about: "편집기 정보",
	print: "프린트",
	underline: "밑줄",
	strikethrough: "취소선",
	indent: "들여쓰기",
	outdent: "내어쓰기",
	fullsize: "전체 화면",
	shrink: "일반 화면",
	hr: "구분선",
	ul: "글머리 목록",
	ol: "번호 목록",
	cut: "잘라내기",
	selectall: "모두 선택",
	"Embed code": "Embed 코드",
	"Open link": "링크 열기",
	"Edit link": "링크 편집",
	"No follow": "No follow",
	Unlink: "링크 제거",
	Eye: "사이트 확인",
	pencil: "연필",
	Update: "갱신",
	" URL": "URL",
	Edit: "편집",
	"Horizontal align": "수평 정렬",
	Filter: "필터",
	"Sort by changed": "변경일 정렬",
	"Sort by name": "이름 정렬",
	"Sort by size": "크기 정렬",
	"Add folder": "새 폴더",
	Reset: "초기화",
	Save: "저장",
	"Save as ...": "새로 저장하기 ...",
	Resize: "리사이즈",
	Crop: "크롭",
	Width: "가로 길이",
	Height: "세로 높이",
	"Keep Aspect Ratio": "비율 유지하기",
	Yes: "네",
	No: "아니오",
	Remove: "제거",
	Select: "선택",
	"Chars: %d": "문자수: %d",
	"Words: %d": "단어수: %d",
	All: "모두",
	"Select all": "모두 선택",
	"Select %s": "선택: %s",
	"Vertical align": "수직 정렬",
	Split: "분할",
	"Split vertical": "세로 셀 분할",
	"Split horizontal": "가로 셀 분할",
	Merge: "셀 병합",
	"Add column": "열 추가",
	"Add row": "행 추가",
	Delete: "삭제",
	Border: "외곽선",
	"License: %s": "라이센스: %s",
	"Strike through": "취소선",
	Underline: "밑줄",
	superscript: "윗첨자",
	subscript: "아래첨자",
	"Cut selection": "선택 잘라내기",
	Break: "구분자",
	"Search for": "검색",
	"Replace with": "대체하기",
	Replace: "대체",
	Paste: "붙여넣기",
	"Choose Content to Paste": "붙여넣을 내용 선택",
	"You can only edit your own images. Download this image on the host?": "외부 이미지는 편집할 수 없어요. 외부 이미지를 다운로드 할까요?",
	"The image has been successfully uploaded to the host!": "이미지를 무사히 업로드 했어요!",
	palette: "팔레트",
	"There are no files": "파일이 없어요",
	Rename: "이름 변경",
	"Enter new name": "새 이름 입력",
	preview: "미리보기",
	download: "다운로드",
	"Paste from clipboard": "클립보드 붙여넣기",
	"Your browser doesn't support direct access to the clipboard.": "사용중인 브라우저가 클립보드 접근을 지원하지 않아요.",
	"Copy selection": "선택 복사",
	copy: "복사",
	"Border radius": "둥근 테두리",
	"Show all": "모두 보기",
	Apply: "적용",
	"Please fill out this field": "이 항목을 입력해주세요!",
	"Please enter a web address": "웹 URL을 입력해주세요.",
	Default: "기본",
	Circle: "원",
	Dot: "점",
	Quadrate: "정사각형",
	Find: "찾기",
	"Find Previous": "이전 찾기",
	"Find Next": "다음 찾기",
	"Insert className": "className 입력",
	"Press Alt for custom resizing": "사용자 지정 크기 조정에 대 한 고도 누르십시오"
};
//#endregion
//#region node_modules/jodit/esm/langs/mn.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var mn_default = {
	"Type something": "Бичээд үзээрэй",
	Advanced: "Дэвшилтэт",
	"About Jodit": "Jodit-ын талаар ",
	"Jodit Editor": "Jodit програм",
	"Jodit User's Guide": "Jodit гарын авлага",
	"contains detailed help for using": "хэрэглээний талаар дэлгэрэнгүй мэдээллийг агуулна",
	"For information about the license, please go to our website:": "Лицензийн мэдээллийг манай вэб хуудаснаас авна уу:",
	"Buy full version": "Бүрэн хувилбар худалдан авах",
	"Copyright © XDSoft.net - Chupurnov Valeriy. All rights reserved.": "Зохиогчийн эрх хамгаалагдсан © XDSoft.net - Chupurnov Valeriy. Бүх эрхийг эзэмшинэ.",
	Anchor: "Холбоо барих",
	"Open in new tab": "Шинэ табаар нээх",
	"Open in fullsize": "Бүтэн дэлгэцээр нээх",
	"Clear Formatting": "Форматыг арилгах",
	"Fill color or set the text color": "Өнгөөр будах эсвэл текстийн өнгө сонгох",
	Redo: "Дахих",
	Undo: "Буцаах",
	Bold: "Тод",
	Italic: "Налуу",
	"Insert Unordered List": "Тэмдэгт жагсаалт нэмэх",
	"Insert Ordered List": "Дугаарт жагсаалт нэмэх",
	"Align Center": "Голлож байрлуулах",
	"Align Justify": "Тэгшитгэн байрлуулах",
	"Align Left": "Зүүнд байрлуулах",
	"Align Right": "Баруунд байрлуулах",
	"Insert Horizontal Line": "Хэвтээ зураас нэмэх",
	"Insert Image": "Зураг нэмэх",
	"Insert file": "Файл нэмэх",
	"Insert youtube/vimeo video": "Youtube/Vimeo видео нэмэх",
	"Insert link": "Холбоос нэмэх",
	"Font size": "Фонтын хэмжээ",
	"Font family": "Фонтын бүл",
	"Insert format block": "Блок нэмэх",
	Normal: "Хэвийн",
	"Heading 1": "Гарчиг 1",
	"Heading 2": "Гарчиг 2",
	"Heading 3": "Гарчиг 3",
	"Heading 4": "Гарчиг 4",
	Quote: "Ишлэл",
	Code: "Код",
	Insert: "Оруулах",
	"Insert table": "Хүснэгт оруулах",
	"Decrease Indent": "Доголын зай хасах",
	"Increase Indent": "Доголын зай нэмэх",
	"Select Special Character": "Тусгай тэмдэгт сонгох",
	"Insert Special Character": "Тусгай тэмдэгт нэмэх",
	"Paint format": "Зургийн формат",
	"Change mode": "Горим өөрчлөх",
	Margins: "Цаасны зай",
	top: "Дээрээс",
	right: "Баруунаас",
	bottom: "Доороос",
	left: "Зүүнээс",
	Styles: "CSS стиль",
	Classes: "CSS анги",
	Align: "Байрлуулах",
	Right: "Баруун",
	Center: "Төв",
	Left: "Зүүн",
	"--Not Set--": "--Тодорхойгүй--",
	Src: "Эх үүсвэр",
	Title: "Гарчиг",
	Alternative: "Алтернатив текст",
	Link: "Холбоос",
	"Open link in new tab": "Холбоосыг шинэ хавтсанд нээх",
	Image: "Зураг",
	file: "Файл",
	"Image properties": "Зургийн үзүүлэлт",
	Cancel: "Цуцлах",
	Ok: "Ok",
	"Your code is similar to HTML. Keep as HTML?": "Таны код HTML кодтой адил байна. HTML форматаар үргэлжлүүлэх үү?",
	"Paste as HTML": "HTML байдлаар буулгах",
	Keep: "Хадгалах",
	Clean: "Цэвэрлэх",
	"Insert as Text": "Текст байдлаар нэмэх",
	"Word Paste Detected": "Word байдлаар буулгасан байна",
	"The pasted content is coming from a Microsoft Word/Excel document. Do you want to keep the format or clean it up?": "Буулгасан агуулга Microsoft Word/Excel форматтай байна. Энэ форматыг хэвээр хадгалах уу эсвэл арилгах уу?",
	"Insert only Text": "Зөвхөн текст оруулах",
	"File Browser": "Файлын цонх",
	"Error on load list": "Жагсаалт татах үед алдаа гарлаа",
	"Error on load folders": "Хавтас татах үед алдаа гарлаа",
	"Are you sure?": "Итгэлтэй байна уу?",
	"Enter Directory name": "Хавтсын нэр оруулах",
	"Create directory": "Хавтас үүсгэх",
	"type name": "Нэр бичих",
	"type dir name": "Нэр бичих",
	"Drop image": "Зураг буулгах",
	"Drop file": "Файл буулгах",
	"or click": "эсвэл товш",
	"Alternative text": "Алтернатив текст",
	Browse: "Үзэх",
	Upload: "Байршуулах",
	Background: "Арын зураг",
	Text: "Текст",
	Top: "Дээр",
	Middle: "Дунд",
	Bottom: "Доор",
	"Insert column before": "Урд нь багана нэмэх",
	"Insert column after": "Ард нь багана нэмэх",
	"Insert row above": "Дээр нь мөр нэмэх",
	"Insert row below": "Доор нь мөр нэмэх",
	"Delete table": "Хүснэгт устгах",
	"Delete row": "Мөр устгах",
	"Delete column": "Багана устгах",
	"Empty cell": "Нүд цэвэрлэх",
	Delete: "Устгах",
	"Strike through": "Дээгүүр зураас",
	Underline: "Доогуур зураас",
	Break: "Мөрийг таслах",
	"Search for": "Хайх",
	"Replace with": "Үүгээр солих",
	Replace: "Солих",
	Edit: "Засах",
	"Vertical align": "Босоо эгнүүлэх",
	"Horizontal align": "Хэвтээ эгнүүлэх",
	Filter: "Шүүх",
	"Sort by changed": "Сүүлд өөрчлөгдсөнөөр жагсаах",
	"Sort by name": "Нэрээр жагсаах",
	"Sort by size": "Хэмжээгээр жагсаах",
	"Add folder": "Хавтас нэмэх",
	Split: "Задлах",
	"Split vertical": "Баганаар задлах",
	"Split horizontal": "Мөрөөр задлах",
	Merge: "Нэгтгэх",
	"Add column": "Багана нэмэх",
	"Add row": "Мөр нэмэх",
	Border: "Хүрээ",
	"Embed code": "Код оруулах",
	Update: "Шинэчлэх",
	superscript: "Дээд индекс",
	subscript: "Доод индекс",
	"Cut selection": "Сонголтыг таслах",
	Paste: "Буулгах",
	"Choose Content to Paste": "Буулгах агуулгаа сонгоно уу",
	"Chars: %d": "Тэмдэгт: %d",
	"Words: %d": "Үг: %d",
	All: "Бүгдийг",
	"Select %s": "Сонго: %s",
	"Select all": "Бүгдийг сонго",
	source: "Эх үүсвэр",
	bold: "Тод",
	italic: "Налуу",
	brush: "Будах",
	link: "Холбоос",
	undo: "Буцаах",
	redo: "Дахих",
	table: "Хүснэгт",
	image: "Зураг",
	eraser: "Баллуур",
	paragraph: "Параграф",
	fontsize: "Фонтын хэмжээ",
	video: "Видео",
	font: "Фонт",
	about: "Тухай",
	print: "Хэвлэх",
	underline: "Доогуур зураас",
	strikethrough: "Дээгүүр зураас",
	indent: "Догол нэмэх",
	outdent: "Догол багасгах",
	fullsize: "Бүтэн дэлгэц",
	shrink: "Багасга",
	hr: "Хаалт",
	ul: "Тэмдэгт жагсаалт",
	ol: "Дугаарласан жагсаалт",
	cut: "Таслах",
	selectall: "Бүгдийг сонго",
	"Open link": "Холбоос нээх",
	"Edit link": "Холбоос засах",
	"No follow": "Nofollow özelliği",
	Unlink: "Холбоос салгах",
	Eye: "Нүд",
	pencil: "Засах",
	" URL": "URL",
	Reset: "Буцаах",
	Save: "Хадгалах",
	"Save as ...": "Өөрөөр хадгалах",
	Resize: "Хэмжээг өөрчил",
	Crop: "Тайрах",
	Width: "Өргөн",
	Height: "Өндөр",
	"Keep Aspect Ratio": "Харьцааг хадгал",
	Yes: "Тийм",
	No: "Үгүй",
	Remove: "Арилга",
	Select: "Сонго",
	"You can only edit your own images. Download this image on the host?": "Та зөвхөн өөрийн зургуудаа янзлах боломжтой. Энэ зургийг өөр лүүгээ татмаар байна уу?",
	"The image has been successfully uploaded to the host!": "Зургийг хост руу амжилттай хадгалсан",
	palette: "Палет",
	"There are no files": "Энд ямар нэг файл алга",
	Rename: "Шинээр нэрлэх",
	"Enter new name": "Шинэ нэр оруулна уу",
	preview: "Урьдчилан харах",
	download: "Татах",
	"Paste from clipboard": "Самбараас хуулах ",
	"Your browser doesn't support direct access to the clipboard.": "Энэ вэб хөтчөөс самбарт хандах эрх алга.",
	"Copy selection": "Сонголтыг хуул",
	copy: "Хуулах",
	"Border radius": "Хүрээний радиус",
	"Show all": "Бүгдийг харуулах",
	Apply: "Хэрэгжүүл",
	"Please fill out this field": "Энэ талбарыг бөглөнө үү",
	"Please enter a web address": "Вэб хаягаа оруулна уу",
	Default: "Үндсэн",
	Circle: "Дугуй",
	Dot: "Цэг",
	Quadrate: "Дөрвөлжин",
	Find: "Хайх",
	"Find Previous": "Өмнөхийг ол",
	"Find Next": "Дараагийнхийг ол",
	"Insert className": "Бүлгийн нэрээ оруулна уу",
	"Press Alt for custom resizing": "Хэмжээсийг шинээр өөчрлөхийн тулд Alt товчин дээр дарна уу",
	"License: %s": "Лиценз: %s"
};
//#endregion
//#region node_modules/jodit/esm/langs/nl.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var nl_default = {
	"Type something": "Begin met typen..",
	Advanced: "Geavanceerd",
	"About Jodit": "Over Jodit",
	"Jodit Editor": "Jodit Editor",
	"Free Non-commercial Version": "Gratis niet-commerciële versie",
	"Jodit User's Guide": "Jodit gebruikershandleiding",
	"contains detailed help for using": "bevat gedetailleerde informatie voor gebruik.",
	"For information about the license, please go to our website:": "Voor informatie over de licentie, ga naar onze website:",
	"Buy full version": "Volledige versie kopen",
	"Copyright © XDSoft.net - Chupurnov Valeriy. All rights reserved.": "Copyright © XDSoft.net - Chupurnov Valeriy. Alle rechten voorbehouden.",
	Anchor: "Anker",
	"Open in new tab": "Open in nieuwe tab",
	"Open in fullsize": "Editor in volledig scherm openen",
	"Clear Formatting": "Opmaak verwijderen",
	"Fill color or set the text color": "Vulkleur of tekstkleur aanpassen",
	Redo: "Opnieuw",
	Undo: "Ongedaan maken",
	Bold: "Vet",
	Italic: "Cursief",
	"Insert Unordered List": "Geordende list invoegen",
	"Insert Ordered List": "Ongeordende lijst invoegen",
	"Align Center": "Centreren",
	"Align Justify": "Uitlijnen op volledige breedte",
	"Align Left": "Links uitlijnen",
	"Align Right": "Rechts uitlijnen",
	"Insert Horizontal Line": "Horizontale lijn invoegen",
	"Insert Image": "Afbeelding invoegen",
	"Insert file": "Bestand invoegen",
	"Insert youtube/vimeo video": "Youtube/Vimeo video invoegen",
	"Insert link": "Link toevoegen",
	"Font size": "Tekstgrootte",
	"Font family": "Lettertype",
	"Insert format block": "Format blok invoegen",
	Normal: "Normaal",
	"Heading 1": "Koptekst 1",
	"Heading 2": "Koptekst 2",
	"Heading 3": "Koptekst 3",
	"Heading 4": "Koptekst 4",
	Quote: "Citaat",
	Code: "Code",
	Insert: "Invoegen",
	"Insert table": "Tabel invoegen",
	"Decrease Indent": "Inspringing verkleinen",
	"Increase Indent": "Inspringing vergroten",
	"Select Special Character": "Symbool selecteren",
	"Insert Special Character": "Symbool invoegen",
	"Paint format": "Opmaak kopieren",
	"Change mode": "Modus veranderen",
	Margins: "Marges",
	top: "Boven",
	right: "Rechts",
	bottom: "Onder",
	left: "Links",
	Styles: "CSS styles",
	Classes: "CSS classes",
	Align: "Uitlijning",
	Right: "Rechts",
	Center: "Gecentreerd",
	Left: "Links",
	"--Not Set--": "--Leeg--",
	Src: "Src",
	Title: "Titel",
	Alternative: "Alternatieve tekst",
	Link: "Link",
	"Open link in new tab": "Link in nieuwe tab openen",
	Image: "Afbeelding",
	file: "Bestand",
	"Image properties": "Afbeeldingseigenschappen",
	Cancel: "Annuleren",
	Ok: "OK",
	"Your code is similar to HTML. Keep as HTML?": "Deze code lijkt op HTML. Als HTML behouden?",
	"Paste as HTML": "Invoegen als HTML",
	Keep: "Origineel behouden",
	Clean: "Opschonen",
	"Insert as Text": "Als tekst invoegen",
	"Word Paste Detected": "Word-tekst gedetecteerd",
	"The pasted content is coming from a Microsoft Word/Excel document. Do you want to keep the format or clean it up?": "De geplakte tekst is afkomstig van een Microsoft Word/Excel document. Wil je de opmaak behouden of opschonen?",
	"Insert only Text": "Als onopgemaakte tekst invoegen",
	"File Browser": "Bestandsbrowser",
	"Error on load list": "Fout bij het laden van de lijst",
	"Error on load folders": "Fout bij het laden van de mappenlijst",
	"Are you sure?": "Weet je het zeker?",
	"Enter Directory name": "Geef de map een naam",
	"Create directory": "Map aanmaken",
	"type name": "Type naam",
	"type dir name": "Type naam",
	"Drop image": "Sleep hier een afbeelding naartoe",
	"Drop file": "Sleep hier een bestand naartoe",
	"or click": "of klik",
	"Alternative text": "Alternatieve tekst",
	Browse: "Bladeren",
	Upload: "Uploaden",
	Background: "Achtergrond",
	Text: "Tekst",
	Top: "Boven",
	Middle: "Midden",
	Bottom: "Onder",
	"Insert column before": "Kolom invoegen (voor)",
	"Insert column after": "Kolom invoegen (na)",
	"Insert row above": "Rij invoegen (boven)",
	"Insert row below": "Rij invoegen (onder)",
	"Delete table": "Tabel verwijderen",
	"Delete row": "Rij verwijderen",
	"Delete column": "Kolom verwijderen",
	"Empty cell": "Cel leegmaken",
	Delete: "Verwijderen",
	"Strike through": "Doorstrepen",
	Underline: "Onderstrepen",
	Break: "Enter",
	"Search for": "Zoek naar",
	"Replace with": "Vervangen door",
	Replace: "Vervangen",
	Edit: "Bewerken",
	"Vertical align": "Verticaal uitlijnen",
	"Horizontal align": "Horizontaal uitlijnen",
	Filter: "Filteren",
	"Sort by changed": "Sorteren op wijzigingsdatum",
	"Sort by name": "Sorteren op naam",
	"Sort by size": "Sorteren op grootte",
	"Add folder": "Map toevoegen",
	Split: "Splitsen",
	"Split vertical": "Verticaal splitsen",
	"Split horizontal": "Horizontaal splitsen",
	Merge: "Samenvoegen",
	"Add column": "Kolom toevoegen",
	"Add row": "Rij toevoegen",
	Border: "Rand",
	"Embed code": "Embed code",
	Update: "Updaten",
	superscript: "Superscript",
	subscript: "Subscript",
	"Cut selection": "Selectie knippen",
	Paste: "Plakken",
	"Choose Content to Paste": "Kies content om te plakken",
	"Chars: %d": "Tekens: %d",
	"Words: %d": "Woorden: %d",
	All: "Alles",
	"Select %s": "Selecteer: %s",
	"Select all": "Selecteer alles",
	source: "Broncode",
	bold: "vet",
	italic: "cursief",
	brush: "kwast",
	link: "link",
	undo: "ongedaan maken",
	redo: "opnieuw",
	table: "tabel",
	image: "afbeelding",
	eraser: "gum",
	paragraph: "paragraaf",
	fontsize: "lettergrootte",
	video: "video",
	font: "lettertype",
	about: "over",
	print: "afdrukken",
	underline: "onderstreept",
	strikethrough: "doorgestreept",
	indent: "inspringen",
	outdent: "minder inspringen",
	fullsize: "volledige grootte",
	shrink: "kleiner maken",
	hr: "horizontale lijn",
	ul: "lijst",
	ol: "genummerde lijst",
	cut: "knip",
	selectall: "alles selecteren",
	"Open link": "Link openen",
	"Edit link": "Link aanpassen",
	"No follow": "Niet volgen",
	Unlink: "link verwijderen",
	Eye: "Recensie",
	pencil: "Om te bewerken",
	" URL": " URL",
	Reset: "Herstellen",
	Save: "Opslaan",
	"Save as ...": "Opslaan als ...",
	Resize: "Grootte aanpassen",
	Crop: "Bijknippen",
	Width: "Breedte",
	Height: "Hoogte",
	"Keep Aspect Ratio": "Verhouding behouden",
	Yes: "Ja",
	No: "Nee",
	Remove: "Verwijderen",
	Select: "Selecteren",
	"You can only edit your own images. Download this image on the host?": "Je kunt alleen je eigen afbeeldingen aanpassen. Deze afbeelding downloaden?",
	"The image has been successfully uploaded to the host!": "De afbeelding is succesvol geüploadet!",
	palette: "Palette",
	"There are no files": "Er zijn geen bestanden in deze map.",
	Rename: "Hernoemen",
	"Enter new name": "Voer een nieuwe naam in",
	preview: "Voorvertoning",
	download: "Download",
	"Paste from clipboard": "Plakken van klembord",
	"Your browser doesn't support direct access to the clipboard.": "Uw browser ondersteunt geen directe toegang tot het klembord.",
	"Copy selection": "Selectie kopiëren",
	copy: "kopiëren",
	"Border radius": "Border radius",
	"Show all": "Toon alle",
	Apply: "Toepassen",
	"Please fill out this field": "Vul dit veld in",
	"Please enter a web address": "Voer een webadres in",
	Default: "Standaard",
	Circle: "Cirkel",
	Dot: "Punt",
	Quadrate: "Kwadraat",
	Find: "Zoeken",
	"Find Previous": "Vorige Zoeken",
	"Find Next": "Volgende Zoeken",
	"Insert className": "Voeg de klassenaam in",
	"Press Alt for custom resizing": "Druk op Alt voor aangepaste grootte",
	"License: %s": "Licentie: %s"
};
//#endregion
//#region node_modules/jodit/esm/langs/no.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var no_default = {
	"Type something": "Skriv noe",
	"About Jodit": "Om Jodit",
	"Jodit Editor": "Jodit-redigerer",
	"Jodit User's Guide": "Jodit brukerveiledning",
	"contains detailed help for using": "Inneholder detaljert hjelp for bruk",
	"For information about the license, please go to our website:": "For informasjon om lisensen, besøk vår nettside:",
	"Buy full version": "Kjøp fullversjon",
	"Copyright © XDSoft.net - Chupurnov Valeriy. All rights reserved.": "Opphavsrett © XDSoft.net - Chupurnov Valeriy. Alle rettigheter forbeholdt.",
	Anchor: "Anker",
	"Open in new tab": "Åpne i ny fane",
	"Open in fullsize": "Åpne i fullskjerm",
	"Clear Formatting": "Fjern formatering",
	"Fill color or set the text color": "Endre bakgrunns- eller tekstfarge",
	Redo: "Gjør om",
	Undo: "Angre",
	Bold: "Fet",
	Italic: "Kursiv",
	"Insert Unordered List": "Sett inn punktliste",
	"Insert Ordered List": "Sett inn nummerert liste",
	"Align Center": "Midtstill",
	"Align Justify": "Juster",
	"Align Left": "Venstrejuster",
	"Align Right": "Høyrejuster",
	"Insert Horizontal Line": "Sett inn horisontal linje",
	"Insert Image": "Sett inn bilde",
	"Insert file": "Sett inn fil",
	"Insert youtube/vimeo video": "Sett inn YouTube/Vimeo-video",
	"Insert link": "Sett inn lenke",
	"Font size": "Skriftstørrelse",
	"Font family": "Skriftfamilie",
	"Insert format block": "Sett inn formateringsblokk",
	Normal: "Normal",
	"Heading 1": "Overskrift 1",
	"Heading 2": "Overskrift 2",
	"Heading 3": "Overskrift 3",
	"Heading 4": "Overskrift 4",
	Quote: "Sitat",
	Code: "Kode",
	Insert: "Sett inn",
	"Insert table": "Sett inn tabell",
	"Decrease Indent": "Reduser innrykk",
	"Increase Indent": "Øk innrykk",
	"Select Special Character": "Velg spesialtegn",
	"Insert Special Character": "Sett inn spesialtegn",
	"Paint format": "Kopier format",
	"Change mode": "Bytt modus (WYSIWYG/HTML)",
	Margins: "Marger",
	top: "topp",
	right: "høyre",
	bottom: "bunn",
	left: "venstre",
	Styles: "Stiler",
	Classes: "Klasser",
	Align: "Justering",
	Right: "Høyre",
	Center: "Senter",
	Left: "Venstre",
	"--Not Set--": "--Ikke satt--",
	Src: "Kilde",
	Title: "Tittel",
	Alternative: "Alternativ",
	Filter: "Filter",
	Link: "Lenke",
	"Open link in new tab": "Åpne lenke i ny fane",
	Image: "Bilde",
	file: "fil",
	Advanced: "Avansert",
	"Image properties": "Bildeegenskaper",
	Cancel: "Avbryt",
	Ok: "OK",
	"Your code is similar to HTML. Keep as HTML?": "Koden din ligner HTML. Beholde som HTML?",
	"Paste as HTML": "Lim inn som HTML",
	Keep: "Behold",
	Clean: "Rens",
	"Insert as Text": "Lim inn som tekst",
	"Word Paste Detected": "Word-innliming oppdaget",
	"The pasted content is coming from a Microsoft Word/Excel document. Do you want to keep the format or clean it up?": "Innholdet du limer inn kommer fra et Microsoft Word/Excel-dokument. Vil du beholde formatet eller rense det?",
	"Insert only Text": "Lim inn kun tekst",
	"File Browser": "Filutforsker",
	"Error on load list": "Feil ved lasting av liste",
	"Error on load folders": "Feil ved lasting av mapper",
	"Are you sure?": "Er du sikker?",
	"Enter Directory name": "Skriv inn mappenavn",
	"Create directory": "Opprett mappe",
	"type name": "skriv navn",
	"type dir name": "skriv navn",
	"Drop image": "Slipp bilde",
	"Drop file": "Slipp fil",
	"or click": "eller klikk",
	"Alternative text": "Alternativ tekst",
	Browse: "Bla gjennom",
	Upload: "Last opp",
	Background: "Bakgrunn",
	Border: "Kantlinje",
	Text: "Tekst",
	Top: "Topp",
	Middle: "Midt",
	Bottom: "Bunn",
	"Insert column before": "Sett inn kolonne før",
	"Insert column after": "Sett inn kolonne etter",
	"Insert row above": "Sett inn rad over",
	"Insert row below": "Sett inn rad under",
	"Delete table": "Slett tabell",
	"Delete row": "Slett rad",
	"Delete column": "Slett kolonne",
	"Empty cell": "Tøm celle",
	"Chars: %d": "Tegn: %d",
	"Words: %d": "Ord: %d",
	Split: "Del",
	"Split vertical": "Del vertikalt",
	"Split horizontal": "Del horisontalt",
	"Strike through": "Gjennomstreking",
	Underline: "Understreking",
	superscript: "hevet skrift",
	subscript: "senket skrift",
	"Cut selection": "Klipp ut markering",
	"Select all": "Velg alt",
	Break: "Pause",
	"Search for": "Søk etter",
	"Replace with": "Erstatt med",
	Replace: "Erstatt",
	Paste: "Lim inn",
	"Choose Content to Paste": "Velg innhold å lime inn",
	source: "kilde",
	bold: "fet",
	italic: "kursiv",
	brush: "pensel",
	link: "lenke",
	undo: "angre",
	redo: "gjør om",
	table: "tabell",
	image: "bilde",
	eraser: "viskelær",
	paragraph: "avsnitt",
	fontsize: "skriftstørrelse",
	video: "video",
	font: "skrift",
	about: "om redigeringsverktøyet",
	print: "skriv ut",
	underline: "understreking",
	strikethrough: "gjennomstreking",
	indent: "innrykk",
	outdent: "reduser innrykk",
	fullsize: "full størrelse",
	shrink: "krympe",
	hr: "linje",
	ul: "punktliste",
	ol: "nummerert liste",
	cut: "klipp ut",
	selectall: "velg alt",
	"Open link": "Åpne lenke",
	"Edit link": "Rediger lenke",
	"No follow": "Ingen oppfølging",
	Unlink: "Fjern lenke",
	Eye: "Forhåndsvisning",
	pencil: "Rediger",
	" URL": "URL",
	Reset: "Tilbakestill",
	Save: "Lagre",
	"Save as ...": "Lagre som ...",
	Resize: "Endre størrelse",
	Crop: "Beskjær",
	Width: "Bredde",
	Height: "Høyde",
	"Keep Aspect Ratio": "Behold proporsjoner",
	Yes: "Ja",
	No: "Nei",
	Remove: "Fjern",
	Select: "Velg",
	"Select %s": "Velg: %s",
	Update: "Oppdater",
	"Vertical align": "Vertikal justering",
	Merge: "Slå sammen",
	"Add column": "Legg til kolonne",
	"Add row": "Legg til rad",
	Delete: "Slett",
	"Horizontal align": "Horisontal justering",
	"Sort by changed": "Sorter etter endring",
	"Sort by name": "Sorter etter navn",
	"Sort by size": "Sorter etter størrelse",
	"Add folder": "Legg til mappe",
	palette: "Palett",
	preview: "Forhåndsvisning",
	"Line height": "Linjehøyde",
	"Insert className": "Sett inn klassenavn",
	apply: "Bruk",
	edit: "Rediger",
	"Show all": "Vis alle",
	sound: "Lyd",
	"Interim Results": "Foreløpige resultater",
	default: "Standard",
	circle: "Sirkel",
	dot: "Punkt",
	square: "Firkant",
	"Press Alt for custom resizing": "Trykk på Alt for å endre størrelse",
	"Copy selection": "Kopier utvalg",
	"Paste from clipboard": "Lim inn fra utklippstavlen",
	Find: "Finne",
	"Embed code": "Bygge inn kode",
	Edit: "Rediger",
	All: "Velg alle",
	"License: %s": "Lisens: %s",
	"You can only edit your own images. Download this image on the host?": "Du kan bare redigere dine egne bilder. Last ned dette bildet på verten?",
	"The image has been successfully uploaded to the host!": "Bildet har blitt lastet opp til verten!",
	"There are no files": "Det er ingen filer i denne katalogen",
	Rename: "Gi nytt navn",
	"Enter new name": "Skriv inn nytt navn",
	download: "Last ned",
	"Your browser doesn't support direct access to the clipboard.": "Nettleseren din støtter ikke direkte tilgang til utklippstavlen.",
	copy: "kopi",
	"Border radius": "Grenseradius",
	Apply: "Bruk",
	"Please fill out this field": "Vennligst fyll ut dette feltet",
	"Please enter a web address": "Vennligst skriv inn en webadresse",
	Default: "Standard",
	Circle: "Sirkel",
	Dot: "Prikk",
	Quadrate: "Firkant",
	"Find Previous": "Finn forrige",
	"Find Next": "Finn neste"
};
//#endregion
//#region node_modules/jodit/esm/langs/pl.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var pl_default = {
	"Type something": "Napisz coś",
	Advanced: "Zaawansowane",
	"About Jodit": "O Jodit",
	"Jodit Editor": "Edytor Jodit",
	"Jodit User's Guide": "Instrukcja Jodit",
	"contains detailed help for using": "zawiera szczegółowe informacje dotyczące użytkowania.",
	"For information about the license, please go to our website:": "Odwiedź naszą stronę, aby uzyskać więcej informacji na temat licencji:",
	"Buy full version": "Zakup pełnej wersji",
	"Copyright © XDSoft.net - Chupurnov Valeriy. All rights reserved.": "Copyright © XDSoft.net - Chupurnov Valeriy. Wszystkie prawa zastrzeżone.",
	Anchor: "Kotwica",
	"Open in new tab": "Otwórz w nowej zakładce",
	"Open in fullsize": "Otwórz edytor w pełnym rozmiarze",
	"Clear Formatting": "Wyczyść formatowanie",
	"Fill color or set the text color": "Kolor wypełnienia lub ustaw kolor tekstu",
	Redo: "Ponów",
	Undo: "Cofnij",
	Bold: "Pogrubienie",
	Italic: "Kursywa",
	"Insert Unordered List": "Wstaw listę wypunktowaną",
	"Insert Ordered List": "Wstaw listę numeryczną",
	"Align Center": "Wyśrodkuj",
	"Align Justify": "Wyjustuj",
	"Align Left": "Wyrównaj do lewej",
	"Align Right": "Wyrównaj do prawej",
	"Insert Horizontal Line": "Wstaw linię poziomą",
	"Insert Image": "Wstaw grafikę",
	"Insert file": "Wstaw plik",
	"Insert youtube/vimeo video": "Wstaw film Youtube/vimeo",
	"Insert link": "Wstaw link",
	"Font size": "Rozmiar tekstu",
	"Font family": "Krój czcionki",
	"Insert format block": "Wstaw formatowanie",
	Normal: "Normalne",
	"Heading 1": "Nagłówek 1",
	"Heading 2": "Nagłówek 2",
	"Heading 3": "Nagłówek 3",
	"Heading 4": "Nagłówek 4",
	Quote: "Cytat",
	Code: "Kod",
	Insert: "Wstaw",
	"Insert table": "Wstaw tabelę",
	"Decrease Indent": "Zmniejsz wcięcie",
	"Increase Indent": "Zwiększ wcięcie",
	"Select Special Character": "Wybierz znak specjalny",
	"Insert Special Character": "Wstaw znak specjalny",
	"Paint format": "Malarz formatów",
	"Change mode": "Zmień tryb",
	Margins: "Marginesy",
	top: "Górny",
	right: "Prawy",
	bottom: "Dolny",
	left: "Levy",
	Styles: "Style CSS",
	Classes: "Klasy CSS",
	Align: "Wyrównanie",
	Right: "Prawa",
	Center: "środek",
	Left: "Lewa",
	"--Not Set--": "brak",
	Src: "Źródło",
	Title: "Tytuł",
	Alternative: "Tekst alternatywny",
	Link: "Link",
	"Open link in new tab": "Otwórz w nowej zakładce",
	Image: "Grafika",
	file: "Plik",
	"Image properties": "Właściwości grafiki",
	Cancel: "Anuluj",
	Ok: "OK",
	"Your code is similar to HTML. Keep as HTML?": "Twój kod wygląda jak HTML. Zachować HTML?",
	"Paste as HTML": "Wkleić jako HTML?",
	Keep: "Oryginalny tekst",
	Clean: "Wyczyść",
	"Insert as Text": "Wstaw jako tekst",
	"Word Paste Detected": "Wykryto tekst w formacie Word",
	"The pasted content is coming from a Microsoft Word/Excel document. Do you want to keep the format or clean it up?": "Wklejany tekst pochodzi z dokumentu Microsoft Word/Excel. Chcesz zachować ten format czy wyczyścić go? ",
	"Insert only Text": "Wstaw tylko treść",
	"File Browser": "Przeglądarka plików",
	"Error on load list": "Błąd ładowania listy plików",
	"Error on load folders": "Błąd ładowania folderów",
	"Are you sure?": "Czy jesteś pewien?",
	"Enter Directory name": "Wprowadź nazwę folderu",
	"Create directory": "Utwórz folder",
	"type name": "wprowadź nazwę",
	"type dir name": "wprowadź nazwę",
	"Drop image": "Upuść plik graficzny",
	"Drop file": "Upuść plik",
	"or click": "lub kliknij tu",
	"Alternative text": "Tekst alternatywny",
	Browse: "Przeglądaj",
	Upload: "Wczytaj",
	Background: "Tło",
	Text: "Treść",
	Top: "Góra",
	Middle: "Środek",
	Bottom: "Dół",
	"Insert column before": "Wstaw kolumnę przed",
	"Insert column after": "Wstaw kolumnę po",
	"Insert row above": "Wstaw wiersz przed",
	"Insert row below": "Wstaw wiersz po",
	"Delete table": "Usuń tabelę",
	"Delete row": "Usuń wiersz",
	"Delete column": "Usuń kolumnę",
	"Empty cell": "Wyczyść komórkę",
	Delete: "Usuń",
	"Strike through": "Przekreślenie",
	Underline: "Podkreślenie",
	Break: "Przerwa",
	"Search for": "Szukaj",
	"Replace with": "Zamień na",
	Replace: "Wymienić",
	Edit: "Edytuj",
	"Vertical align": "Wyrównywanie w pionie",
	"Horizontal align": "Wyrównywanie w poziomie",
	Filter: "Filtruj",
	"Sort by changed": "Sortuj wg zmiany",
	"Sort by name": "Sortuj wg nazwy",
	"Sort by size": "Sortuj wg rozmiaru",
	"Add folder": "Dodaj folder",
	"Split vertical": "Podziel w pionie",
	"Split horizontal": "Podziel w poziomie",
	Split: "Podziel",
	Merge: "Scal",
	"Add column": "Dodaj kolumnę",
	"Add row": "Dodaj wiersz",
	Border: "Obramowanie",
	"Embed code": "Wstaw kod",
	Update: "Aktualizuj",
	superscript: "indeks górny",
	subscript: "index dolny",
	"Cut selection": "Wytnij zaznaczenie",
	Paste: "Wklej",
	"Choose Content to Paste": "Wybierz zawartość do wklejenia",
	"Chars: %d": "Znaki: %d",
	"Words: %d": "Słowa: %d",
	All: "Wszystko",
	"Select %s": "Wybierz: %s",
	"Select all": "Wybierz wszystko",
	source: "HTML",
	bold: "pogrubienie",
	italic: "kursywa",
	brush: "pędzel",
	link: "link",
	undo: "cofnij",
	redo: "ponów",
	table: "tabela",
	image: "grafika",
	eraser: "wyczyść",
	paragraph: "akapit",
	fontsize: "rozmiar czcionki",
	video: "wideo",
	font: "czcionka",
	about: "O programie",
	print: "drukuj",
	underline: "podkreślenie",
	strikethrough: "przekreślenie",
	indent: "wcięcie",
	outdent: "wycięcie",
	fullsize: "pełen rozmiar",
	shrink: "przytnij",
	hr: "linia pozioma",
	ul: "lista",
	ol: "lista numerowana",
	cut: "wytnij",
	selectall: "zaznacz wszystko",
	"Open link": "otwórz link",
	"Edit link": "edytuj link",
	"No follow": "Atrybut no-follow",
	Unlink: "Usuń link",
	Eye: "szukaj",
	pencil: "edytuj",
	" URL": "URL",
	Reset: "wyczyść",
	Save: "zapisz",
	"Save as ...": "zapisz jako",
	Resize: "Zmień rozmiar",
	Crop: "Przytnij",
	Width: "Szerokość",
	Height: "Wysokość",
	"Keep Aspect Ratio": "Zachowaj proporcje",
	Yes: "Tak",
	No: "Nie",
	Remove: "Usuń",
	Select: "Wybierz",
	"You can only edit your own images. Download this image on the host?": "Możesz edytować tylko swoje grafiki. Czy chcesz pobrać tą grafikę?",
	"The image has been successfully uploaded to the host!": "Grafika została pomyślnienie dodana na serwer",
	palette: "Paleta",
	"There are no files": "Brak plików.",
	Rename: "zmień nazwę",
	"Enter new name": "Wprowadź nową nazwę",
	preview: "podgląd",
	download: "pobierz",
	"Paste from clipboard": "Wklej ze schowka",
	"Your browser doesn't support direct access to the clipboard.": "Twoja przeglądarka nie obsługuje schowka",
	"Copy selection": "Kopiuj zaznaczenie",
	copy: "kopiuj",
	"Border radius": "Zaokrąglenie krawędzi",
	"Show all": "Pokaż wszystkie",
	Apply: "Zastosuj",
	"Please fill out this field": "Proszę wypełnić to pole",
	"Please enter a web address": "Proszę, wpisz adres sieci web",
	Default: "Domyślnie",
	Circle: "Koło",
	Dot: "Punkt",
	Quadrate: "Kwadrat",
	Find: "Znaleźć",
	"Find Previous": "Znaleźć Poprzednie",
	"Find Next": "Znajdź Dalej",
	"Insert className": "Wstaw nazwę zajęć",
	"Press Alt for custom resizing": "Naciśnij Alt, aby zmienić rozmiar",
	"License: %s": "Licencja: %s"
};
//#endregion
//#region node_modules/jodit/esm/langs/pt_br.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var pt_br_default = {
	"Type something": "Escreva algo...",
	Advanced: "Avançado",
	"About Jodit": "Sobre o Jodit",
	"Jodit Editor": "Editor Jodit",
	"Jodit User's Guide": "Guia de usuário Jodit",
	"contains detailed help for using": "contém ajuda detalhada para o uso.",
	"For information about the license, please go to our website:": "Para informação sobre a licença, por favor visite nosso site:",
	"Buy full version": "Compre a versão completa",
	"Copyright © XDSoft.net - Chupurnov Valeriy. All rights reserved.": "Copyright © XDSoft.net - Chupurnov Valeriy. Todos os direitos reservados.",
	Anchor: "Link",
	"Open in new tab": "Abrir em nova aba",
	"Open in fullsize": "Abrir editor em tela cheia",
	"Clear Formatting": "Limpar formatação",
	"Fill color or set the text color": "Cor de preenchimento ou cor do texto",
	Redo: "Refazer",
	Undo: "Desfazer",
	Bold: "Negrito",
	Italic: "Itálico",
	"Insert Unordered List": "Inserir lista não ordenada",
	"Insert Ordered List": "Inserir lista ordenada",
	"Align Center": "Centralizar",
	"Align Justify": "Justificar",
	"Align Left": "Alinhar à Esquerda",
	"Align Right": "Alinhar à Direita",
	"Insert Horizontal Line": "Inserir linha horizontal",
	"Insert Image": "Inserir imagem",
	"Insert file": "Inserir arquivo",
	"Insert youtube/vimeo video": "Inserir vídeo do Youtube/vimeo",
	"Insert link": "Inserir link",
	"Font size": "Tamanho da letra",
	"Font family": "Fonte",
	"Insert format block": "Inserir bloco",
	Normal: "Normal",
	"Heading 1": "Cabeçalho 1",
	"Heading 2": "Cabeçalho 2",
	"Heading 3": "Cabeçalho 3",
	"Heading 4": "Cabeçalho 4",
	Quote: "Citação",
	Code: "Código",
	Insert: "Inserir",
	"Insert table": "Inserir tabela",
	"Decrease Indent": "Diminuir recuo",
	"Increase Indent": "Aumentar recuo",
	"Select Special Character": "Selecionar caractere especial",
	"Insert Special Character": "Inserir caractere especial",
	"Paint format": "Copiar formato",
	"Change mode": "Mudar modo",
	Margins: "Margens",
	top: "cima",
	right: "direta",
	bottom: "baixo",
	left: "esquerda",
	Styles: "Estilos CSS",
	Classes: "Classes CSS",
	Align: "Alinhamento",
	Right: "Direita",
	Center: "Centro",
	Left: "Esquerda",
	"--Not Set--": "--Não Estabelecido--",
	Src: "Fonte",
	Title: "Título",
	Alternative: "Texto Alternativo",
	Link: "Link",
	"Open link in new tab": "Abrir link em nova aba",
	Image: "Imagem",
	file: "Arquivo",
	"Image properties": "Propriedades da imagem",
	Cancel: "Cancelar",
	Ok: "Ok",
	"Your code is similar to HTML. Keep as HTML?": "Seu código é similar ao HTML. Manter como HTML?",
	"Paste as HTML": "Colar como HTML?",
	Keep: "Manter",
	Clean: "Limpar",
	"Insert as Text": "Inserir como Texto",
	"Word Paste Detected": "Colado do Word Detectado",
	"The pasted content is coming from a Microsoft Word/Excel document. Do you want to keep the format or clean it up?": "O conteúdo colado veio de um documento Microsoft Word/Excel. Você deseja manter o formato ou limpa-lo?",
	"Insert only Text": "Inserir somente o Texto",
	"File Browser": "Procurar arquivo",
	"Error on load list": "Erro ao carregar a lista",
	"Error on load folders": "Erro ao carregar as pastas",
	"Are you sure?": "Você tem certeza?",
	"Enter Directory name": "Escreva o nome da pasta",
	"Create directory": "Criar pasta",
	"type name": "Escreva seu nome",
	"type dir name": "Escreva seu nome",
	"Drop image": "Soltar imagem",
	"Drop file": "Soltar arquivo",
	"or click": "ou clique",
	"Alternative text": "Texto alternativo",
	Browse: "Explorar",
	Upload: "Upload",
	Background: "Fundo",
	Text: "Texto",
	Top: "Cima",
	Middle: "Meio",
	Bottom: "Baixo",
	"Insert column before": "Inserir coluna antes",
	"Insert column after": "Inserir coluna depois",
	"Insert row above": "Inserir linha acima",
	"Insert row below": "Inserir linha abaixo",
	"Delete table": "Excluir tabela",
	"Delete row": "Excluir linha",
	"Delete column": "Excluir coluna",
	"Empty cell": "Limpar célula",
	Delete: "Excluir",
	"Strike through": "Tachado",
	Underline: "Sublinhar",
	Break: "Pausa",
	"Search for": "Procurar por",
	"Replace with": "Substituir com",
	Replace: "Substituir",
	Edit: "Editar",
	"Vertical align": "Alinhamento vertical",
	"Horizontal align": "Alinhamento horizontal",
	Filter: "filtrar",
	"Sort by changed": "Ordenar por modificação",
	"Sort by name": "Ordenar por nome",
	"Sort by size": "Ordenar por tamanho",
	"Add folder": "Adicionar pasta",
	Split: "Dividir",
	"Split vertical": "Dividir vertical",
	"Split horizontal": "Dividir horizontal",
	Merge: "Mesclar",
	"Add column": "Adicionar coluna",
	"Add row": "Adicionar linha",
	Border: "Borda",
	"Embed code": "Incluir código",
	Update: "Atualizar",
	superscript: "sobrescrito",
	subscript: "subscrito",
	"Cut selection": "Cortar seleção",
	Paste: "Colar",
	"Choose Content to Paste": "Escolher conteúdo para colar",
	"Chars: %d": "Caracteres: %d",
	"Words: %d": "Palavras: %d",
	All: "Tudo",
	"Select %s": "Selecionar: %s",
	"Select all": "Selecionar tudo",
	source: "HTML",
	bold: "negrito",
	italic: "itálico",
	brush: "pincel",
	link: "link",
	undo: "desfazer",
	redo: "refazer",
	table: "tabela",
	image: "imagem",
	eraser: "apagar",
	paragraph: "parágrafo",
	fontsize: "tamanho da letra",
	video: "vídeo",
	font: "fonte",
	about: "Sobre de",
	print: "Imprimir",
	underline: "sublinhar",
	strikethrough: "tachado",
	indent: "recuar",
	outdent: "diminuir recuo",
	fullsize: "Tamanho completo",
	shrink: "diminuir",
	hr: "linha horizontal",
	ul: "lista não ordenada",
	ol: "lista ordenada",
	cut: "Cortar",
	selectall: "Selecionar tudo",
	"Open link": "Abrir link",
	"Edit link": "Editar link",
	"No follow": "Não siga",
	Unlink: "Remover link",
	Eye: "Visualizar",
	pencil: "Editar",
	" URL": "URL",
	Reset: "Resetar",
	Save: "Salvar",
	"Save as ...": "Salvar como...",
	Resize: "Redimensionar",
	Crop: "Recortar",
	Width: "Largura",
	Height: "Altura",
	"Keep Aspect Ratio": "Manter a proporção",
	Yes: "Sim",
	No: "Não",
	Remove: "Remover",
	Select: "Selecionar",
	"You can only edit your own images. Download this image on the host?": "Você só pode editar suas próprias imagens. Baixar essa imagem pro servidor?",
	"The image has been successfully uploaded to the host!": "A imagem foi enviada com sucesso para o servidor!",
	palette: "Palette",
	"There are no files": "Não há arquivos nesse diretório.",
	Rename: "Húngara",
	"Enter new name": "Digite um novo nome",
	preview: "preview",
	download: "Baixar",
	"Paste from clipboard": "Colar da área de transferência",
	"Your browser doesn't support direct access to the clipboard.": "O seu navegador não oferece suporte a acesso direto para a área de transferência.",
	"Copy selection": "Selecção de cópia",
	copy: "cópia",
	"Border radius": "Border radius",
	"Show all": "Mostrar todos os",
	Apply: "Aplicar",
	"Please fill out this field": "Por favor, preencha este campo",
	"Please enter a web address": "Por favor introduza um endereço web",
	Default: "Padrão",
	Circle: "Círculo",
	Dot: "Ponto",
	Quadrate: "Quadro",
	"Lower Alpha": "Letra Minúscula",
	"Lower Greek": "Grego Minúscula",
	"Lower Roman": "Romano Minúscula",
	"Upper Alpha": "Letra Maiúscula",
	"Upper Roman": "Romano Maiúscula",
	Find: "Encontrar",
	"Find Previous": "Encontrar Anteriores",
	"Find Next": "Localizar Próxima",
	"Insert className": "Insira o nome da classe",
	"Press Alt for custom resizing": "Pressione Alt para redimensionamento personalizado",
	"License: %s": "Licença: %s"
};
//#endregion
//#region node_modules/jodit/esm/langs/ru.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var ru_default = {
	"Type something": "Напишите что-либо",
	"About Jodit": "О Jodit",
	"Jodit Editor": "Редактор Jodit",
	"Jodit User's Guide": "Jodit Руководство пользователя",
	"contains detailed help for using": "содержит детальную информацию по использованию",
	"For information about the license, please go to our website:": "Для получения сведений о лицензии , пожалуйста, перейдите на наш сайт:",
	"Buy full version": "Купить полную версию",
	"Copyright © XDSoft.net - Chupurnov Valeriy. All rights reserved.": "Авторские права © XDSoft.net - Чупурнов Валерий. Все права защищены.",
	Anchor: "Анкор",
	"Open in new tab": "Открывать ссылку в новой вкладке",
	"Open in fullsize": "Открыть редактор в полном размере",
	"Clear Formatting": "Очистить форматирование",
	"Fill color or set the text color": "Цвет заливки или цвет текста",
	Redo: "Повтор",
	Undo: "Отмена",
	Bold: "Жирный",
	Italic: "Наклонный",
	"Insert Unordered List": "Вставка маркированного списка",
	"Insert Ordered List": "Вставить нумерованный список",
	"Align Center": "Выровнять по центру",
	"Align Justify": "Выровнять по ширине",
	"Align Left": "Выровнять по левому краю",
	"Align Right": "Выровнять по правому краю",
	"Insert Horizontal Line": "Вставить горизонтальную линию",
	"Insert Image": "Вставить изображение",
	"Insert file": "Вставить файл",
	"Insert youtube/vimeo video": "Вставьте видео",
	"Insert link": "Вставить ссылку",
	"Font size": "Размер шрифта",
	"Font family": "Шрифт",
	"Insert format block": "Вставить блочный элемент",
	Normal: "Нормальный текст",
	"Heading 1": "Заголовок 1",
	"Heading 2": "Заголовок 2",
	"Heading 3": "Заголовок 3",
	"Heading 4": "Заголовок 4",
	Quote: "Цитата",
	Code: "Код",
	Insert: "Вставить",
	"Insert table": "Вставить таблицу",
	"Decrease Indent": "Уменьшить отступ",
	"Increase Indent": "Увеличить отступ",
	"Select Special Character": "Выберите специальный символ",
	"Insert Special Character": "Вставить специальный символ",
	"Paint format": "Формат краски",
	"Change mode": "Источник",
	Margins: "Отступы",
	top: "сверху",
	right: "справа",
	bottom: "снизу",
	left: "слева",
	Styles: "Стили",
	Classes: "Классы",
	Align: "Выравнивание",
	Right: "По правому краю",
	Center: "По центру",
	Left: "По левому краю",
	"--Not Set--": "--не устанавливать--",
	Src: "src",
	Title: "Заголовок",
	Alternative: "Альтернативный текст (alt)",
	Link: "Ссылка",
	"Open link in new tab": "Открывать ссылку в новом окне",
	file: "Файл",
	Advanced: "Расширенные",
	"Image properties": "Свойства изображения",
	Cancel: "Отмена",
	Ok: "Ок",
	"Your code is similar to HTML. Keep as HTML?": "Ваш текст, который вы пытаетесь вставить похож на HTML. Вставить его как HTML?",
	"Paste as HTML": "Вставить как HTML?",
	Keep: "Сохранить оригинал",
	Clean: "Почистить",
	"Insert as Text": "Вставить как текст",
	"Insert only Text": "Вставить только текст",
	"Word Paste Detected": "Возможно это фрагмент Word или Excel",
	"The pasted content is coming from a Microsoft Word/Excel document. Do you want to keep the format or clean it up?": "Контент который вы вставляете поступает из документа Microsoft Word / Excel. Вы хотите сохранить формат или очистить его?",
	"File Browser": "Браузер файлов",
	"Error on load list": "Ошибка при загрузке списка изображений",
	"Error on load folders": "Ошибка при загрузке списка директорий",
	"Are you sure?": "Вы уверены?",
	"Enter Directory name": "Введите название директории",
	"Create directory": "Создать директорию",
	"type name": "введите название",
	"type dir name": "введите название",
	"Drop image": "Перетащите сюда изображение",
	"Drop file": "Перетащите сюда файл",
	"or click": "или нажмите",
	"Alternative text": "Альтернативный текст",
	Browse: "Сервер",
	Upload: "Загрузка",
	Background: "Фон",
	Text: "Текст",
	Top: " К верху",
	Middle: "По середине",
	Bottom: "К низу",
	"Insert column before": "Вставить столбец до",
	"Insert column after": "Вставить столбец после",
	"Insert row above": "Вставить ряд выше",
	"Insert row below": "Вставить ряд ниже",
	"Delete table": "Удалить таблицу",
	"Delete row": "Удалять ряд",
	"Delete column": "Удалить столбец",
	"Empty cell": "Очистить ячейку",
	source: "HTML",
	bold: "жирный",
	italic: "курсив",
	brush: "заливка",
	link: "ссылка",
	undo: "отменить",
	redo: "повторить",
	table: "таблица",
	image: "Изображение",
	eraser: "очистить",
	paragraph: "параграф",
	fontsize: "размер шрифта",
	video: "видео",
	font: "шрифт",
	about: "о редакторе",
	print: "печать",
	underline: "подчеркнутый",
	strikethrough: "перечеркнутый",
	indent: "отступ",
	outdent: "выступ",
	fullsize: "во весь экран",
	shrink: "обычный размер",
	hr: "линия",
	ul: "Список",
	ol: "Нумерованный список",
	cut: "Вырезать",
	selectall: "Выделить все",
	"Embed code": "Код",
	"Open link": "Открыть ссылку",
	"Edit link": "Редактировать ссылку",
	"No follow": "Атрибут nofollow",
	Unlink: "Убрать ссылку",
	Eye: "Просмотр",
	pencil: "Редактировать",
	Update: "Обновить",
	" URL": "URL",
	Edit: "Редактировать",
	"Horizontal align": "Горизонтальное выравнивание",
	Filter: "Фильтр",
	"Sort by changed": "По изменению",
	"Sort by name": "По имени",
	"Sort by size": "По размеру",
	"Add folder": "Добавить папку",
	Reset: "Восстановить",
	Save: "Сохранить",
	"Save as ...": "Сохранить как",
	Resize: "Изменить размер",
	Crop: "Обрезать размер",
	Width: "Ширина",
	Height: "Высота",
	"Keep Aspect Ratio": "Сохранять пропорции",
	Yes: "Да",
	No: "Нет",
	Remove: "Удалить",
	Select: "Выделить",
	"Chars: %d": "Символов: %d",
	"Words: %d": "Слов: %d",
	All: "Выделить все",
	"Select %s": "Выделить: %s",
	"Select all": "Выделить все",
	"Vertical align": "Вертикальное выравнивание",
	Split: "Разделить",
	"Split vertical": "Разделить по вертикали",
	"Split horizontal": "Разделить по горизонтали",
	Merge: "Объединить в одну",
	"Add column": "Добавить столбец",
	"Add row": "Добавить строку",
	Delete: "Удалить",
	Border: "Рамка",
	"License: %s": "Лицензия: %s",
	"Strike through": "Перечеркнуть",
	Underline: "Подчеркивание",
	superscript: "верхний индекс",
	subscript: "индекс",
	"Cut selection": "Вырезать",
	Break: "Разделитель",
	"Search for": "Найти",
	"Replace with": "Заменить на",
	Replace: "Заменить",
	Paste: "Вставить",
	"Choose Content to Paste": "Выбрать контент для вставки",
	"You can only edit your own images. Download this image on the host?": "Вы можете редактировать только свои собственные изображения. Загрузить это изображение на ваш сервер?",
	"The image has been successfully uploaded to the host!": "Изображение успешно загружено на сервер!",
	palette: "палитра",
	"There are no files": "В данном каталоге нет файлов",
	Rename: "Переименовать",
	"Enter new name": "Введите новое имя",
	preview: "Предпросмотр",
	download: "Скачать",
	"Paste from clipboard": "Вставить из буфера обмена",
	"Your browser doesn't support direct access to the clipboard.": "Ваш браузер не поддерживает прямой доступ к буферу обмена.",
	"Copy selection": "Скопировать выделенное",
	copy: "копия",
	"Border radius": "Радиус границы",
	"Show all": "Показать все",
	Apply: "Применить",
	"Please fill out this field": "Пожалуйста, заполните это поле",
	"Please enter a web address": "Пожалуйста, введите веб-адрес",
	Default: "По умолчанию",
	Circle: "Круг",
	Dot: "Точка",
	Quadrate: "Квадрат",
	Find: "Найти",
	"Find Previous": "Найти Предыдущие",
	"Find Next": "Найти Далее",
	"Insert className": "Вставить название класса",
	"Press Alt for custom resizing": "Нажмите Alt для изменения пользовательского размера"
};
//#endregion
//#region node_modules/jodit/esm/langs/sk.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var sk_default = {
	"Type something": "Napíšte niečo",
	"About Jodit": "O editore Jodit",
	"Jodit Editor": "Editor Jodit",
	"Free Non-commercial Version": "Bezplatná verzia na nekomerčné použitie",
	"Jodit User's Guide": "Používateľská príručka Jodit",
	"contains detailed help for using": "obsahuje podrobnú pomoc s používaním",
	"For information about the license, please go to our website:": "Informácie o licencii nájdete na našej webovej stránke:",
	"Buy full version": "Kúpiť plnú verziu",
	"Copyright © XDSoft.net - Chupurnov Valeriy. All rights reserved.": "Copyright © XDSoft.net - Chupurnov Valeriy. Všetky práva vyhradené.",
	Anchor: "Kotva",
	"Open in new tab": "Otvoriť na novej karte",
	"Open in fullsize": "Otvoriť na celú obrazovku",
	"Clear Formatting": "Vymazať formátovanie",
	"Fill color or set the text color": "Farba výplne alebo textu",
	Redo: "Znova",
	Undo: "Späť",
	Bold: "Tučné písmo",
	Italic: "Kurzíva",
	"Insert Unordered List": "Vložiť zoznam s odrážkami",
	"Insert Ordered List": "Vložiť číslovaný zoznam",
	"Align Center": "Zarovnať na stred",
	"Align Justify": "Zarovnať do bloku",
	"Align Left": "Zarovnať doľava",
	"Align Right": "Zarovnať doprava",
	"Insert Horizontal Line": "Vložiť vodorovnú čiaru",
	"Insert Image": "Vložiť obrázok",
	"Insert file": "Vložiť súbor",
	"Insert youtube/vimeo video": "Vložiť video YouTube/Vimeo",
	"Insert link": "Vložiť odkaz",
	"Font size": "Veľkosť písma",
	"Font family": "Typ písma",
	"Insert format block": "Vložiť formátovaný blok",
	Normal: "Normálny text",
	"Heading 1": "Nadpis 1",
	"Heading 2": "Nadpis 2",
	"Heading 3": "Nadpis 3",
	"Heading 4": "Nadpis 4",
	Quote: "Citácia",
	Code: "Kód",
	Insert: "Vložiť",
	"Insert table": "Vložiť tabuľku",
	"Decrease Indent": "Zmenšiť odsadenie",
	"Increase Indent": "Zväčšiť odsadenie",
	"Select Special Character": "Vybrať špeciálny znak",
	"Insert Special Character": "Vložiť špeciálny znak",
	"Paint format": "Kopírovať formát",
	"Change mode": "Zmeniť režim",
	Margins: "Okraje",
	top: "horný",
	right: "pravý",
	bottom: "dolný",
	left: "ľavý",
	Styles: "Štýly",
	Classes: "Triedy",
	Align: "Zarovnanie",
	Right: "Vpravo",
	Center: "Na stred",
	Left: "Vľavo",
	"--Not Set--": "--nenastavené--",
	Src: "Zdroj",
	Title: "Názov",
	Alternative: "Alternatívny text (alt)",
	Link: "Odkaz",
	"Open link in new tab": "Otvoriť odkaz na novej karte",
	Image: "Obrázok",
	file: "súbor",
	Advanced: "Rozšírené",
	"Image properties": "Vlastnosti obrázka",
	Cancel: "Zrušiť",
	Ok: "OK",
	"Your code is similar to HTML. Keep as HTML?": "Váš kód sa podobá na HTML. Ponechať ho ako HTML?",
	"Paste as HTML": "Vložiť ako HTML",
	Keep: "Ponechať",
	Clean: "Vyčistiť",
	"Insert as Text": "Vložiť ako text",
	"Insert only Text": "Vložiť iba text",
	"Word Paste Detected": "Zistil sa obsah z Wordu alebo Excelu",
	"The pasted content is coming from a Microsoft Word/Excel document. Do you want to keep the format or clean it up?": "Vložený obsah pochádza z dokumentu Microsoft Word/Excel. Chcete zachovať formátovanie alebo ho vyčistiť?",
	"File Browser": "Prehliadač súborov",
	"Error on load list": "Chyba pri načítaní zoznamu súborov",
	"Error on load folders": "Chyba pri načítaní priečinkov",
	"Are you sure?": "Ste si istí?",
	"Enter Directory name": "Zadajte názov priečinka",
	"Create directory": "Vytvoriť priečinok",
	"type name": "zadajte názov",
	"Drop image": "Presuňte sem obrázok",
	"Drop file": "Presuňte sem súbor",
	"or click": "alebo kliknite",
	"Alternative text": "Alternatívny text",
	Browse: "Prehľadávať",
	Upload: "Nahrať",
	Background: "Pozadie",
	Text: "Text",
	Top: "Hore",
	Middle: "Na stred",
	Bottom: "Dole",
	"Insert column before": "Vložiť stĺpec pred",
	"Insert column after": "Vložiť stĺpec za",
	"Insert row above": "Vložiť riadok nad",
	"Insert row below": "Vložiť riadok pod",
	"Delete table": "Odstrániť tabuľku",
	"Delete row": "Odstrániť riadok",
	"Delete column": "Odstrániť stĺpec",
	"Empty cell": "Vyprázdniť bunku",
	source: "HTML",
	bold: "tučné písmo",
	italic: "kurzíva",
	brush: "štetec",
	link: "odkaz",
	undo: "späť",
	redo: "znova",
	table: "tabuľka",
	image: "obrázok",
	eraser: "guma",
	paragraph: "odstavec",
	fontsize: "veľkosť písma",
	video: "video",
	font: "písmo",
	about: "o editore",
	print: "tlačiť",
	underline: "podčiarknuté",
	strikethrough: "prečiarknuté",
	indent: "zväčšiť odsadenie",
	outdent: "zmenšiť odsadenie",
	fullsize: "režim celej obrazovky",
	shrink: "zmenšiť",
	hr: "vodorovná čiara",
	ul: "zoznam s odrážkami",
	ol: "číslovaný zoznam",
	cut: "vystrihnúť",
	selectall: "vybrať všetko",
	"Embed code": "Vložiť kód",
	"Open link": "Otvoriť odkaz",
	"Edit link": "Upraviť odkaz",
	"No follow": "Atribút nofollow",
	Unlink: "Odstrániť odkaz",
	Eye: "Zobraziť",
	pencil: "Upraviť",
	Update: "Aktualizovať",
	" URL": "URL",
	Edit: "Upraviť",
	"Horizontal align": "Vodorovné zarovnanie",
	Filter: "Filter",
	"Sort by changed": "Zoradiť podľa poslednej zmeny",
	"Sort by name": "Zoradiť podľa názvu",
	"Sort by size": "Zoradiť podľa veľkosti",
	"Add folder": "Pridať priečinok",
	Reset: "Obnoviť",
	Save: "Uložiť",
	"Save as ...": "Uložiť ako…",
	Resize: "Zmeniť veľkosť",
	Crop: "Orezať",
	Width: "Šírka",
	Height: "Výška",
	"Keep Aspect Ratio": "Zachovať pomer strán",
	Yes: "Áno",
	No: "Nie",
	Remove: "Odstrániť",
	Select: "Vybrať",
	"Chars: %d": "Znaky: %d",
	"Words: %d": "Slová: %d",
	All: "Všetko",
	"Select %s": "Vybrať %s",
	"Select all": "Vybrať všetko",
	"Vertical align": "Zvislé zarovnanie",
	Split: "Rozdeliť",
	"Split vertical": "Rozdeliť zvislo",
	"Split horizontal": "Rozdeliť vodorovne",
	Merge: "Zlúčiť",
	"Add column": "Pridať stĺpec",
	"Add row": "Pridať riadok",
	Delete: "Odstrániť",
	Border: "Okraj",
	"License: %s": "Licencia: %s",
	"Strike through": "Prečiarknuté",
	Underline: "Podčiarknuté",
	superscript: "Horný index",
	subscript: "Dolný index",
	"Cut selection": "Vystrihnúť výber",
	Break: "Zalomenie",
	"Search for": "Hľadať",
	"Replace with": "Nahradiť čím",
	Replace: "Nahradiť",
	Paste: "Vložiť",
	"Choose Content to Paste": "Vyberte obsah na vloženie",
	"You can only edit your own images. Download this image on the host?": "Môžete upravovať iba vlastné obrázky. Stiahnuť tento obrázok na server?",
	"The image has been successfully uploaded to the host!": "Obrázok bol úspešne nahraný na server!",
	palette: "paleta",
	"There are no files": "V tomto priečinku nie sú žiadne súbory.",
	Rename: "Premenovať",
	"Enter new name": "Zadajte nový názov",
	preview: "Ukážka",
	download: "Stiahnuť",
	"Paste from clipboard": "Vložiť zo schránky",
	"Your browser doesn't support direct access to the clipboard.": "Váš prehliadač nepodporuje priamy prístup do schránky.",
	"Copy selection": "Kopírovať výber",
	copy: "kopírovať",
	"Border radius": "Zaoblenie rohov",
	"Show all": "Zobraziť všetko",
	Apply: "Použiť",
	"Please fill out this field": "Vyplňte toto pole",
	"Please enter a web address": "Zadajte webovú adresu",
	Default: "Predvolené",
	Circle: "Kruh",
	Dot: "Bodka",
	Quadrate: "Štvorec",
	Find: "Nájsť",
	"Find Previous": "Nájsť predchádzajúce",
	"Find Next": "Nájsť nasledujúce",
	"Insert className": "Vložte názov triedy",
	"Press Alt for custom resizing": "Stlačte Alt na vlastnú zmenu veľkosti",
	"Upload file": "Nahrať súbor",
	"Remove file": "Odstrániť súbor",
	"Update file list": "Aktualizovať zoznam súborov",
	"Select file": "Vybrať súbor",
	"Edit image": "Upraviť obrázok",
	"Tiles view": "Zobrazenie dlaždíc",
	"List view": "Zobrazenie zoznamu"
};
//#endregion
//#region node_modules/jodit/esm/langs/sv.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var sv_default = {
	"Type something": "Skriv något...",
	"About Jodit": "Om Jodit",
	"Jodit Editor": "Jodit Editor",
	"Jodit User's Guide": "Jodit användarguide",
	"contains detailed help for using": "Innehåller detaljerad hjälp för användning",
	"For information about the license, please go to our website:": "För information om licensen, vänligen besök vår webbsida:",
	"Buy full version": "Köp full version",
	"Copyright © XDSoft.net - Chupurnov Valeriy. All rights reserved.": "Upphovsrätt © XDSoft.net - Chupurnov Valeriy. Alla rättigheter förbehållna.",
	Anchor: "Ankare",
	"Open in new tab": "Öppna i ny flik",
	"Open in fullsize": "Öppna i fullskärm",
	"Clear Formatting": "Rensa formatering",
	"Fill color or set the text color": "Fyll färg eller ställ in textfärg",
	Redo: "Gör om",
	Undo: "Ångra",
	Bold: "Fetstil",
	Italic: "Kursiv",
	"Insert Unordered List": "Infoga punktlista",
	"Insert Ordered List": "Infoga numrerad lista",
	"Align Center": "Centrera",
	"Align Justify": "Justera",
	"Align Left": "Vänsterjustera",
	"Align Right": "Högerjustera",
	"Insert Horizontal Line": "Infoga horisontell linje",
	"Insert Image": "Infoga bild",
	"Insert file": "Infoga fil",
	"Insert youtube/vimeo video": "Infoga YouTube/Vimeo-video",
	"Insert link": "Infoga länk",
	"Font size": "Teckenstorlek",
	"Font family": "Teckensnitt",
	"Insert format block": "Infoga formateringsblock",
	Normal: "Normal",
	"Heading 1": "Rubrik 1",
	"Heading 2": "Rubrik 2",
	"Heading 3": "Rubrik 3",
	"Heading 4": "Rubrik 4",
	Quote: "Citat",
	Code: "Kod",
	Insert: "Infoga",
	"Insert table": "Infoga tabell",
	"Decrease Indent": "Minska indrag",
	"Increase Indent": "Öka indrag",
	"Select Special Character": "Välj specialtecken",
	"Insert Special Character": "Infoga specialtecken",
	"Paint format": "Kopiera format",
	"Change mode": "Byt läge (WYSIWYG/HTML)",
	Margins: "Marginaler",
	top: "överst",
	right: "höger",
	bottom: "nederst",
	left: "vänster",
	Styles: "Stilar",
	Classes: "Klasser",
	Align: "Justering",
	Right: "Höger",
	Center: "Centrera",
	Left: "Vänster",
	"--Not Set--": "--Inte inställd--",
	Src: "Källa",
	Title: "Titel",
	Alternative: "Alternativ",
	Filter: "Filter",
	Link: "Länk",
	"Open link in new tab": "Öppna länk i ny flik",
	Image: "Bild",
	file: "fil",
	Advanced: "Avancerad",
	"Image properties": "Bildegenskaper",
	Cancel: "Avbryt",
	Ok: "OK",
	"Your code is similar to HTML. Keep as HTML?": "Din kod liknar HTML. Vill du behålla som HTML?",
	"Paste as HTML": "Klistra in som HTML",
	Keep: "Behåll",
	Clean: "Rensa",
	"Insert as Text": "Klistra in som text",
	"Word Paste Detected": "Word-klistring upptäckt",
	"The pasted content is coming from a Microsoft Word/Excel document. Do you want to keep the format or clean it up?": "Det inklistrade innehållet kommer från ett Microsoft Word/Excel-dokument. Vill du behålla formatet eller rensa det?",
	"Insert only Text": "Klistra in endast text",
	"File Browser": "Filutforskare",
	"Error on load list": "Fel vid inläsning av lista",
	"Error on load folders": "Fel vid inläsning av mappar",
	"Are you sure?": "Är du säker?",
	"Enter Directory name": "Ange mappnamn",
	"Create directory": "Skapa mapp",
	"type name": "skriv namn",
	"type dir name": "skriv namn",
	"Drop image": "Släpp bild",
	"Drop file": "Släpp fil",
	"or click": "eller klicka",
	"Alternative text": "Alternativ text",
	Browse: "Bläddra",
	Upload: "Ladda upp",
	Background: "Bakgrund",
	Border: "Kantlinje",
	Text: "Text",
	Top: "Topp",
	Middle: "Mitten",
	Bottom: "Botten",
	"Insert column before": "Infoga kolumn före",
	"Insert column after": "Infoga kolumn efter",
	"Insert row above": "Infoga rad ovanför",
	"Insert row below": "Infoga rad nedanför",
	"Delete table": "Radera tabell",
	"Delete row": "Radera rad",
	"Delete column": "Radera kolumn",
	"Empty cell": "Töm cell",
	"Chars: %d": "Tecken: %d",
	"Words: %d": "Ord: %d",
	Split: "Dela",
	"Split vertical": "Dela vertikalt",
	"Split horizontal": "Dela horisontellt",
	"Strike through": "Genomstrykning",
	Underline: "Understrykning",
	superscript: "Upphöjd skrift",
	subscript: "Sänkt skrift",
	"Cut selection": "Klipp ut markering",
	"Select all": "Välj allt",
	Break: "Paus",
	"Search for": "Sök efter",
	"Replace with": "Ersätt med",
	Replace: "Ersätt",
	Paste: "Klistra in",
	"Choose Content to Paste": "Välj innehåll att klistra in",
	source: "källa",
	bold: "fetstil",
	italic: "kursiv",
	brush: "pensel",
	link: "länk",
	undo: "ångra",
	redo: "gör om",
	table: "tabell",
	image: "bild",
	eraser: "radergummi",
	paragraph: "stycke",
	fontsize: "teckenstorlek",
	video: "video",
	font: "teckensnitt",
	about: "om redigeringsverktyget",
	print: "skriv ut",
	underline: "understrykning",
	strikethrough: "genomstrykning",
	indent: "indrag",
	outdent: "minska indrag",
	fullsize: "full storlek",
	shrink: "minska",
	hr: "linje",
	ul: "punktlista",
	ol: "numrerad lista",
	cut: "klipp ut",
	selectall: "välj allt",
	"Open link": "Öppna länk",
	"Edit link": "Redigera länk",
	"No follow": "Ingen uppföljning",
	Unlink: "Ta bort länk",
	Eye: "Förhandsvisning",
	pencil: "Redigera",
	" URL": "URL",
	Reset: "Återställ",
	Save: "Spara",
	"Save as ...": "Spara som ...",
	Resize: "Ändra storlek",
	Crop: "Beskär",
	Width: "Bredd",
	Height: "Höjd",
	"Keep Aspect Ratio": "Behåll aspektförhållande",
	Yes: "Ja",
	No: "Nej",
	Remove: "Ta bort",
	Select: "Välj",
	"Select %s": "Välj: %s",
	Update: "Uppdatera",
	"Vertical align": "Vertikal justering",
	Merge: "Sammanfoga",
	"Add column": "Lägg till kolumn",
	"Add row": "Lägg till rad",
	Delete: "Radera",
	"Horizontal align": "Horisontell justering",
	"Sort by changed": "Sortera efter ändrad",
	"Sort by name": "Sortera efter namn",
	"Sort by size": "Sortera efter storlek",
	"Add folder": "Lägg till mapp",
	"You can only edit your own images. Download this image on the host?": "Du kan endast redigera dina egna bilder. Ladda ner denna bild på värden?",
	"The image has been successfully uploaded to the host!": "Bilden har laddats upp till värden utan problem!",
	palette: "Palett",
	"There are no files": "Det finns inga filer",
	Rename: "Döp om",
	"Enter new name": "Ange nytt namn",
	preview: "Förhandsvisning",
	download: "Ladda ner",
	"Paste from clipboard": "Klistra in från urklipp",
	"Your browser doesn't support direct access to the clipboard.": "Din webbläsare stöder inte direkt åtkomst till urklipp.",
	"Copy selection": "Kopiera markering",
	copy: "kopiera",
	"Border radius": "Borderradie",
	"Show all": "Visa alla",
	Apply: "Tillämpa",
	"Please fill out this field": "Vänligen fyll i detta fält",
	"Please enter a web address": "Vänligen ange en webbadress",
	Default: "Standard",
	Circle: "Cirkel",
	Dot: "Punkt",
	Quadrate: "Fyrkant",
	Find: "Hitta",
	"Find Previous": "Föregående",
	"Find Next": "Nästa",
	"Insert className": "Infoga klassnamn",
	"Press Alt for custom resizing": "Tryck på Alt för anpassad storleksändring",
	"Line height": "Radavstånd",
	Edit: "Redigera",
	Sound: "Ljud",
	"Interim Results": "Preliminära resultat",
	Spellcheck: "Stavningskontroll",
	"Speech Recognize": "Taligenkänning",
	newline: "ny rad",
	delete: "radera",
	space: "mellanslag",
	"Embed code": "Bädda in kod",
	All: "Välj alla",
	"License: %s": "Licens: %s"
};
//#endregion
//#region node_modules/jodit/esm/langs/tr.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var tr_default = {
	"Type something": "Bir şeyler yaz",
	Advanced: "Gelişmiş",
	"About Jodit": "Jodit Hakkında",
	"Jodit Editor": "Jodit Editor",
	"Jodit User's Guide": "Jodit Kullanım Kılavuzu",
	"contains detailed help for using": "kullanım için detaylı bilgiler içerir",
	"For information about the license, please go to our website:": "Lisans hakkında bilgi için lütfen web sitemize gidin:",
	"Buy full version": "Tam versiyonunu satın al",
	"Copyright © XDSoft.net - Chupurnov Valeriy. All rights reserved.": "Copyright © XDSoft.net - Chupurnov Valeriy. Tüm hakları saklıdır.",
	Anchor: "Bağlantı",
	"Open in new tab": "Yeni sekmede aç",
	"Open in fullsize": "Editörü tam ekranda aç",
	"Clear Formatting": "Stili temizle",
	"Fill color or set the text color": "Renk doldur veya yazı rengi seç",
	Redo: "Yinele",
	Undo: "Geri Al",
	Bold: "Kalın",
	Italic: "İtalik",
	"Insert Unordered List": "Sırasız Liste Ekle",
	"Insert Ordered List": "Sıralı Liste Ekle",
	"Align Center": "Ortala",
	"Align Justify": "Kenarlara Yasla",
	"Align Left": "Sola Yasla",
	"Align Right": "Sağa Yasla",
	"Insert Horizontal Line": "Yatay Çizgi Ekle",
	"Insert Image": "Resim Ekle",
	"Insert file": "Dosya Ekle",
	"Insert youtube/vimeo video": "Youtube/Vimeo Videosu Ekle",
	"Insert link": "Bağlantı Ekle",
	"Font size": "Font Boyutu",
	"Font family": "Font Ailesi",
	"Insert format block": "Blok Ekle",
	Normal: "Normal",
	"Heading 1": "Başlık 1",
	"Heading 2": "Başlık 2",
	"Heading 3": "Başlık 3",
	"Heading 4": "Başlık 4",
	Quote: "Alıntı",
	Code: "Kod",
	Insert: "Ekle",
	"Insert table": "Tablo Ekle",
	"Decrease Indent": "Girintiyi Azalt",
	"Increase Indent": "Girintiyi Arttır",
	"Select Special Character": "Özel Karakter Seç",
	"Insert Special Character": "Özel Karakter Ekle",
	"Paint format": "Resim Biçimi",
	"Change mode": "Mod Değiştir",
	Margins: "Boşluklar",
	top: "Üst",
	right: "Sağ",
	bottom: "Alt",
	left: "Sol",
	Styles: "CSS Stilleri",
	Classes: "CSS Sınıfları",
	Align: "Hizalama",
	Right: "Sağ",
	Center: "Ortalı",
	Left: "Sol",
	"--Not Set--": "Belirsiz",
	Src: "Kaynak",
	Title: "Başlık",
	Alternative: "Alternatif Yazı",
	Link: "Link",
	"Open link in new tab": "Bağlantıyı yeni sekmede aç",
	Image: "Resim",
	file: "Dosya",
	"Image properties": "Resim özellikleri",
	Cancel: "İptal",
	Ok: "Tamam",
	"Your code is similar to HTML. Keep as HTML?": "Kodunuz HTML koduna benziyor. HTML olarak devam etmek ister misiniz?",
	"Paste as HTML": "HTML olarak yapıştır",
	Keep: "Sakla",
	Clean: "Temizle",
	"Insert as Text": "Yazı olarak ekle",
	"Word Paste Detected": "Word biçiminde yapıştırma algılandı",
	"The pasted content is coming from a Microsoft Word/Excel document. Do you want to keep the format or clean it up?": "Yapıştırılan içerik bir Microsoft Word/Excel belgesinden geliyor. Formatı korumak mı yoksa temizlemek mi istiyorsunuz?",
	"Insert only Text": "Sadece yazıyı ekle",
	"File Browser": "Dosya Listeleyici",
	"Error on load list": "Liste yüklenirken hata oluştu",
	"Error on load folders": "Klasörler yüklenirken hata oluştur",
	"Are you sure?": "Emin misiniz?",
	"Enter Directory name": "Dizin yolu giriniz",
	"Create directory": "Dizin oluştur",
	"type name": "İsim yaz",
	"type dir name": "İsim yaz",
	"Drop image": "Resim bırak",
	"Drop file": "Dosya bırak",
	"or click": "veya tıkla",
	"Alternative text": "Alternatif yazı",
	Browse: "Gözat",
	Upload: "Yükle",
	Background: "Arka plan",
	Text: "Yazı",
	Top: "Üst",
	Middle: "Orta",
	Bottom: "Aşağı",
	"Insert column before": "Öncesine kolon ekle",
	"Insert column after": "Sonrasına kolon ekle",
	"Insert row above": "Üstüne satır ekle",
	"Insert row below": "Altına satır ekle",
	"Delete table": "Tabloyu sil",
	"Delete row": "Satırı sil",
	"Delete column": "Kolonu sil",
	"Empty cell": "Hücreyi temizle",
	Delete: "Sil",
	"Strike through": "Üstü çizili",
	Underline: "Alt çizgi",
	Break: "Satır sonu",
	"Search for": "Ara",
	"Replace with": "Şununla değiştir",
	Replace: "Değiştir",
	Edit: "Düzenle",
	"Vertical align": "Dikey hizala",
	"Horizontal align": "Yatay hizala",
	Filter: "Filtre",
	"Sort by changed": "Değişime göre sırala",
	"Sort by name": "İsme göre sırala",
	"Sort by size": "Boyuta göre sırala",
	"Add folder": "Klasör ekle",
	Split: "Ayır",
	"Split vertical": "Dikey ayır",
	"Split horizontal": "Yatay ayır",
	Merge: "Birleştir",
	"Add column": "Kolon ekle",
	"Add row": "Satır ekle",
	Border: "Kenarlık",
	"Embed code": "Kod ekle",
	Update: "Güncelle",
	superscript: "Üst yazı",
	subscript: "Alt yazı",
	"Cut selection": "Seçilimi kes",
	Paste: "Yapıştır",
	"Choose Content to Paste": "Yapıştırılacak içerik seç",
	"Chars: %d": "Harfler: %d",
	"Words: %d": "Kelimeler: %d",
	All: "Tümü",
	"Select %s": "Seç: %s",
	"Select all": "Tümünü seç",
	source: "Kaynak",
	bold: "Kalın",
	italic: "italik",
	brush: "Fırça",
	link: "Bağlantı",
	undo: "Geri al",
	redo: "Yinele",
	table: "Tablo",
	image: "Resim",
	eraser: "Silgi",
	paragraph: "Paragraf",
	fontsize: "Font boyutu",
	video: "Video",
	font: "Font",
	about: "Hakkında",
	print: "Yazdır",
	underline: "Alt çizgi",
	strikethrough: "Üstü çizili",
	indent: "Girinti",
	outdent: "Çıkıntı",
	fullsize: "Tam ekran",
	shrink: "Küçült",
	hr: "Ayraç",
	ul: "Sırasız liste",
	ol: "Sıralı liste",
	cut: "Kes",
	selectall: "Tümünü seç",
	"Open link": "Bağlantıyı aç",
	"Edit link": "Bağlantıyı düzenle",
	"No follow": "Nofollow özelliği",
	Unlink: "Bağlantıyı kaldır",
	Eye: "Yorumu",
	pencil: "Düzenlemek için",
	" URL": "URL",
	Reset: "Sıfırla",
	Save: "Kaydet",
	"Save as ...": "Farklı kaydet",
	Resize: "Boyutlandır",
	Crop: "Kırp",
	Width: "Genişlik",
	Height: "Yükseklik",
	"Keep Aspect Ratio": "En boy oranını koru",
	Yes: "Evet",
	No: "Hayır",
	Remove: "Sil",
	Select: "Seç",
	"You can only edit your own images. Download this image on the host?": "Sadece kendi resimlerinizi düzenleyebilirsiniz. Bu görseli kendi hostunuza indirmek ister misiniz?",
	"The image has been successfully uploaded to the host!": "Görsel başarıyla hostunuza yüklendi",
	palette: "Palet",
	"There are no files": "Bu dizinde dosya yok",
	Rename: "Yeniden isimlendir",
	"Enter new name": "Yeni isim girin",
	preview: "Ön izleme",
	download: "İndir",
	"Paste from clipboard": "Panodan yapıştır ",
	"Your browser doesn't support direct access to the clipboard.": "Tarayıcınız panoya doğrudan erişimi desteklemiyor.",
	"Copy selection": "Seçimi kopyala",
	copy: "Kopyala",
	"Border radius": "Sınır yarıçapı",
	"Show all": "Tümünü Göster",
	Apply: "Uygula",
	"Please fill out this field": "Lütfen bu alanı doldurun",
	"Please enter a web address": "Lütfen bir web adresi girin",
	Default: "Varsayılan",
	Circle: "Daire",
	Dot: "Nokta",
	Quadrate: "Kare",
	Find: "Bul",
	"Find Previous": "Öncekini Bul",
	"Find Next": "Sonrakini Bul",
	"Insert className": "Sınıf adı girin",
	"Press Alt for custom resizing": "Özel yeniden boyutlandırma için Alt tuşuna basın",
	"License: %s": "Lisans: %s"
};
//#endregion
//#region node_modules/jodit/esm/langs/ua.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var ua_default = {
	"Type something": "Напишіть щось",
	"About Jodit": "Про Jodit",
	"Jodit Editor": "Редактор Jodit",
	"Jodit User's Guide": "Jodit інструкція користувача",
	"contains detailed help for using": "містить детальну інформацію щодо користування",
	"For information about the license, please go to our website:": "Щоб дізнатись більше про ліцензію , будь ласка, перейдіть на наш сайт:",
	"Buy full version": "Купити повну версію",
	"Copyright © XDSoft.net - Chupurnov Valeriy. All rights reserved.": "Авторські права © XDSoft.net - Чупурнов Валерій. Всі права захищені.",
	Anchor: "Анкор",
	"Open in new tab": "Відкрити у новій вкладці",
	"Open in fullsize": "Відкрити редактор в повному розмірі",
	"Clear Formatting": "Очистити форматування",
	"Fill color or set the text color": "Колір заливки або колір текста",
	Redo: "Повторити",
	Undo: "Відмінити",
	Bold: "Жирний",
	Italic: "Курсів",
	"Insert Unordered List": "Вставити невпорядкований список",
	"Insert Ordered List": "Вставити нумерованний список",
	"Align Center": "Вирівняти по центру",
	"Align Justify": "Вирівняти по ширині",
	"Align Left": "Вирівняти по лівому краю",
	"Align Right": "Вирівняти по правому краю",
	"Insert Horizontal Line": "Вставити горизонтальну лінію",
	"Insert Image": "Вставити зображення",
	"Insert file": "Вставити файл",
	"Insert youtube/vimeo video": "Вставити відео",
	"Insert link": "Вставити посилання",
	"Font size": "Розмір шрифту",
	"Font family": "Шрифт",
	"Insert format block": "Вставити блочний елемент",
	Normal: "Нормальний текст",
	"Heading 1": "Заголовок 1",
	"Heading 2": "Заголовок 2",
	"Heading 3": "Заголовок 3",
	"Heading 4": "Заголовок 4",
	Quote: "Цитата",
	Code: "Код",
	Insert: "Вставити",
	"Insert table": "Вставити таблицю",
	"Decrease Indent": "Збільшити відступ",
	"Increase Indent": "Зменшити відступ",
	"Select Special Character": "Оберіть спеціальный символ",
	"Insert Special Character": "Вставити спеціальный символ",
	"Paint format": "Формат краски",
	"Change mode": "Джерело",
	Margins: "Відступи",
	top: "зверху",
	right: "справа",
	bottom: "знизу",
	left: "зліва",
	Styles: "Стилі",
	Classes: "Класи",
	Align: "Вирівнювання",
	Right: "По правому краю",
	Center: "По центру",
	Left: "По лівому краю",
	"--Not Set--": "--не встановлено--",
	Src: "src",
	Title: "Заголовок",
	Alternative: "Альтернативний текст (alt)",
	Link: "Посилання",
	"Open link in new tab": "Відкрити посилання в новій вкладці",
	file: "Файл",
	Advanced: "Розширені",
	"Image properties": "Властивості зображення",
	Cancel: "Відміна",
	Ok: "Ок",
	"Your code is similar to HTML. Keep as HTML?": "Текст, який Ви намагаєтесь вставити схожий на HTML. Вставити його як HTML?",
	"Paste as HTML": "Вставити його як HTML",
	Keep: "Зберегти оригінал",
	Clean: "Почистити",
	"Insert as Text": "Вставити як текст",
	"Insert only Text": "Вставити тільки текст",
	"Word Paste Detected": "Можливо це фрагмент Word або Excel",
	"The pasted content is coming from a Microsoft Word/Excel document. Do you want to keep the format or clean it up?": "Ви вставляєте контент з документа Microsoft Word або Excel. Бажаєте зберегти форматування?",
	"File Browser": "Браузер файлів",
	"Error on load list": "Помилка при завантаженні списку зображень",
	"Error on load folders": "Помилка при завантаженні списку папок",
	"Are you sure?": "Ви впевнені?",
	"Enter Directory name": "Введіть назву папки",
	"Create directory": "Створити папку",
	"type name": "введіть назву",
	"type dir name": "введіть назву",
	"Drop image": "Перетягніть зображення сюди",
	"Drop file": "Перетягніть файл сюди",
	"or click": "або клікніть",
	"Alternative text": "Альтернативный текст",
	Browse: "Сервер",
	Upload: "Завантаження",
	Background: "Фон",
	Text: "Текст",
	Top: " Вгору",
	Middle: "По центру",
	Bottom: "Донизу",
	"Insert column before": "Вставити стовпець до",
	"Insert column after": "Вставити стовпець після",
	"Insert row above": "Вставити рядок вище",
	"Insert row below": "Вставити рядок нижче",
	"Delete table": "Видалити таблицю",
	"Delete row": "Видалити рядок",
	"Delete column": "Видалити стовпчик",
	"Empty cell": "Очистити кліинку",
	source: "HTML",
	bold: "жирний",
	italic: "курсів",
	brush: "заливка",
	link: "посилання",
	undo: "відмінити",
	redo: "повторити",
	table: "таблиця",
	image: "зображення",
	eraser: "видалення",
	paragraph: "параграф",
	fontsize: "розмір шрифту",
	video: "відео",
	font: "шрифт",
	about: "про редактор",
	print: "друк",
	underline: "підкреслений",
	strikethrough: "закреслений",
	indent: "відступ",
	outdent: "заступ",
	fullsize: "на весь екран",
	shrink: "звичайний розмір",
	hr: "лінія",
	ul: "Список",
	ol: "Нумерований список",
	cut: "Вирізати",
	selectall: "Виділити все",
	"Embed code": "Код",
	"Open link": "Відкрити посилання",
	"Edit link": "Редагувати посилання",
	"No follow": "Атрибут nofollow",
	Unlink: "Видалити посилання",
	Eye: "Перегляд",
	pencil: "Редагування",
	Update: "Оновити",
	" URL": "URL",
	Edit: "Редагувати",
	"Horizontal align": "Горизонтальне вирівнювання",
	Filter: "Фільтр",
	"Sort by changed": "Сортувати за зміною",
	"Sort by name": "Сортувати за ім'ям",
	"Sort by size": "Сортувати за розміром",
	"Add folder": "Додати папку",
	Reset: "Відновити",
	Save: "Зберегти",
	"Save as ...": "Зберегти як",
	Resize: "Змінити розмір",
	Crop: "Обрізати розмір",
	Width: "Ширина",
	Height: "Висота",
	"Keep Aspect Ratio": "Зберегти пропорції",
	Yes: "Так",
	No: "Ні",
	Remove: "Видалити",
	Select: "Вибрати",
	"Chars: %d": "Символів: %d",
	"Words: %d": "Слів: %d",
	All: "Вибрати все",
	"Select %s": "Вибрати: %s",
	"Select all": "Вибрати все",
	"Vertical align": "Вертикальне вирівнювання",
	Split: "Розділити",
	"Split vertical": "Розділити по вертикалі",
	"Split horizontal": "Розділити по горизонталі",
	Merge: "Об'єднати в одну",
	"Add column": "Додати стовпчик",
	"Add row": "Додати рядок",
	Delete: "Видалити",
	Border: "Рамка",
	"License: %s": "Ліцензія: %s",
	"Strike through": "Закреслений",
	Underline: "Підкреслений",
	superscript: "верхній індекс",
	subscript: "індекс",
	"Cut selection": "Обрізати вибране",
	Break: "Межа",
	"Search for": "Шукати",
	"Replace with": "Замінити на",
	Replace: "Замінити",
	Paste: "Вставити",
	"Choose Content to Paste": "Обрати контент для вставки",
	"You can only edit your own images. Download this image on the host?": "Ви можете редагувати лише власні зображення. Завантажити зображення на ваш сервер?",
	"The image has been successfully uploaded to the host!": "Зображення успішно завантажено на сервер!",
	palette: "палітра",
	"There are no files": "Файли відсутні",
	Rename: "Змінити назву",
	"Enter new name": "Введіть нове імя'",
	preview: "Попередній перегляд",
	download: "Завантажити",
	"Paste from clipboard": "Вставити з буфера обміну",
	"Your browser doesn't support direct access to the clipboard.": "Ваш браузер не підтримує доступ до буфера обміну.",
	"Copy selection": "Копіювати виділене",
	copy: "копія",
	"Border radius": "Радіус рамки",
	"Show all": "Показати все",
	Apply: "Застосувати",
	"Please fill out this field": "Будь ласка, заповніть це поле",
	"Please enter a web address": "Будь ласка, введіть веб-адресу",
	Default: "За замовченням",
	Circle: "Коло",
	Dot: "Крапка",
	Quadrate: "Квадрат",
	Find: "Знайти",
	"Find Previous": "Знайти попередні",
	"Find Next": "Знайти наступні",
	"Insert className": "Вставити клас",
	"Press Alt for custom resizing": "Натисніть Alt для зміни розміру"
};
//#endregion
//#region node_modules/jodit/esm/langs/zh_cn.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var zh_cn_default = {
	"Type something": "输入一些内容",
	Advanced: "高级",
	"About Jodit": "关于Jodit",
	"Jodit Editor": "Jodit Editor",
	"Free Non-commercial Version": "Free Non-commercial Version",
	"Jodit User's Guide": "开发者指南",
	"contains detailed help for using": "使用帮助",
	"For information about the license, please go to our website:": "有关许可证的信息，请访问我们的网站：",
	"Buy full version": "购买完整版本",
	"Copyright © XDSoft.net - Chupurnov Valeriy. All rights reserved.": "Copyright © XDSoft.net - Chupurnov Valeriy. 版权所有",
	Anchor: "Anchor",
	"Open in new tab": "在新窗口打开",
	"Open in fullsize": "全屏编辑",
	"Clear Formatting": "清除样式",
	"Fill color or set the text color": "颜色",
	Redo: "重做",
	Undo: "撤销",
	Bold: "粗体",
	Italic: "斜体",
	"Insert Unordered List": "符号列表",
	"Insert Ordered List": "编号",
	"Align Center": "居中",
	"Align Justify": "对齐文本",
	"Align Left": "左对齐",
	"Align Right": "右对齐",
	"Insert Horizontal Line": "分割线",
	"Insert Image": "图片",
	"Insert file": "文件",
	"Insert youtube/vimeo video": "视频",
	"Insert link": "链接",
	"Font size": "字号",
	"Font family": "字体",
	"Insert format block": "格式块",
	Normal: "默认",
	"Heading 1": "标题1",
	"Heading 2": "标题2",
	"Heading 3": "标题3",
	"Heading 4": "标题4",
	Quote: "引用",
	Code: "代码",
	Insert: "插入",
	"Insert table": "表格",
	"Decrease Indent": "减少缩进",
	"Increase Indent": "增加缩进",
	"Select Special Character": "选择特殊符号",
	"Insert Special Character": "特殊符号",
	"Paint format": "格式复制",
	"Change mode": "改变模式",
	Margins: "外边距（Margins）",
	top: "top",
	right: "right",
	bottom: "bottom",
	left: "left",
	Styles: "样式",
	Classes: "Classes",
	Align: "对齐方式",
	Right: "居右",
	Center: "居中",
	Left: "居左",
	"--Not Set--": "无",
	Src: "Src",
	Title: "Title",
	Alternative: "Alternative",
	Link: "Link",
	"Open link in new tab": "在新窗口打开链接",
	Image: "图片",
	file: "file",
	"Image properties": "图片属性",
	Cancel: "取消",
	Ok: "确定",
	"Your code is similar to HTML. Keep as HTML?": "你粘贴的文本是一段html代码，是否保留源格式",
	"Paste as HTML": "html粘贴",
	Keep: "保留源格式",
	Clean: "匹配目标格式",
	"Insert as Text": "把html代码视为普通文本",
	"Word Paste Detected": "文本粘贴",
	"The pasted content is coming from a Microsoft Word/Excel document. Do you want to keep the format or clean it up?": "正在粘贴 Word/Excel 的文本，是否保留源格式？",
	"Insert only Text": "只保留文本",
	"File Browser": "文件管理",
	"Error on load list": "加载list错误",
	"Error on load folders": "加载folders错误",
	"Are you sure?": "你确定吗？",
	"Enter Directory name": "输入路径",
	"Create directory": "创建路径",
	"type name": "type name",
	"type dir name": "type name",
	"Drop image": "拖动图片到此",
	"Drop file": "拖动文件到此",
	"or click": "或点击",
	"Alternative text": "Alternative text",
	Browse: "浏览",
	Upload: "上传",
	Background: "背景色",
	Text: "文字",
	Top: "顶部",
	Middle: "中间",
	Bottom: "底部",
	"Insert column before": "在之前插入列",
	"Insert column after": "在之后插入列",
	"Insert row above": "在之前插入行",
	"Insert row below": "在之后插入行",
	"Delete table": "删除表格",
	"Delete row": "删除行",
	"Delete column": "删除列",
	"Empty cell": "清除内容",
	"Chars: %d": "字符数: %d",
	"Words: %d": "单词数: %d",
	"Strike through": "删除线",
	Underline: "下划线",
	superscript: "上标",
	subscript: "下标",
	"Cut selection": "剪切",
	"Select all": "全选",
	Break: "Break",
	"Search for": "查找",
	"Replace with": "替换为",
	Replace: "替换",
	Edit: "编辑",
	Paste: "粘贴",
	"Choose Content to Paste": "选择内容并粘贴",
	All: "全部",
	source: "源码",
	bold: "粗体",
	italic: "斜体",
	brush: "颜色",
	link: "链接",
	undo: "撤销",
	redo: "重做",
	table: "表格",
	image: "图片",
	eraser: "橡皮擦",
	paragraph: "段落",
	fontsize: "字号",
	video: "视频",
	font: "字体",
	about: "关于",
	print: "打印",
	underline: "下划线",
	strikethrough: "上出现",
	indent: "增加缩进",
	outdent: "减少缩进",
	fullsize: "全屏",
	shrink: "收缩",
	hr: "分割线",
	ul: "无序列表",
	ol: "顺序列表",
	cut: "剪切",
	selectall: "全选",
	"Open link": "打开链接",
	"Edit link": "编辑链接",
	"No follow": "No follow",
	Unlink: "取消链接",
	Eye: "预览",
	" URL": "URL",
	Reset: "重置",
	Save: "保存",
	"Save as ...": "保存为",
	Resize: "调整大小",
	Crop: "剪切",
	Width: "宽",
	Height: "高",
	"Keep Aspect Ratio": "保持长宽比",
	Yes: "是",
	No: "不",
	Remove: "移除",
	Select: "选择",
	"Select %s": "选择: %s",
	Update: "更新",
	"Vertical align": "垂直对齐",
	Merge: "合并",
	"Add column": "添加列",
	"Add row": "添加行",
	Border: "边框",
	"Embed code": "嵌入代码",
	Delete: "删除",
	"Horizontal align": "水平对齐",
	Filter: "筛选",
	"Sort by changed": "修改时间排序",
	"Sort by name": "名称排序",
	"Sort by size": "大小排序",
	"Add folder": "新建文件夹",
	Split: "拆分",
	"Split vertical": "垂直拆分",
	"Split horizontal": "水平拆分",
	"You can only edit your own images. Download this image on the host?": "你只能编辑你自己的图片。Download this image on the host?",
	"The image has been successfully uploaded to the host!": "图片上传成功",
	palette: "调色板",
	pencil: "铅笔",
	"There are no files": "此目录中沒有文件。",
	Rename: "重命名",
	"Enter new name": "输入新名称",
	preview: "预览",
	download: "下载",
	"Paste from clipboard": "粘贴从剪贴板",
	"Your browser doesn't support direct access to the clipboard.": "你浏览器不支持直接访问的剪贴板。",
	"Copy selection": "复制选中内容",
	copy: "复制",
	"Border radius": "边界半径",
	"Show all": "显示所有",
	Apply: "应用",
	"Please fill out this field": "请填写这个字段",
	"Please enter a web address": "请输入一个网址",
	Default: "默认",
	Circle: "圆圈",
	Dot: "点",
	Quadrate: "方形",
	Find: "搜索",
	"Find Previous": "查找上一个",
	"Find Next": "查找下一个",
	"Insert className": "插入班级名称",
	"Press Alt for custom resizing": "按Alt自定义调整大小",
	"License: %s": "许可证: %s"
};
//#endregion
//#region node_modules/jodit/esm/langs/zh_tw.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var zh_tw_default = {
	"Type something": "輸入一些內容",
	Advanced: "進階",
	"About Jodit": "關於Jodit",
	"Jodit Editor": "Jodit Editor",
	"Jodit User's Guide": "開發者指南",
	"contains detailed help for using": "使用幫助",
	"For information about the license, please go to our website:": "相關授權條款資訊，請造訪我們的網站：",
	"Buy full version": "購買完整版本",
	"Copyright © XDSoft.net - Chupurnov Valeriy. All rights reserved.": "Copyright © XDSoft.net - Chupurnov Valeriy. All rights reserved.",
	Anchor: "錨點",
	"Open in new tab": "在新分頁開啟",
	"Open in fullsize": "全螢幕編輯",
	"Clear Formatting": "清除樣式",
	"Fill color or set the text color": "顏色",
	Redo: "取消復原",
	Undo: "復原",
	Bold: "粗體",
	Italic: "斜體",
	"Insert Unordered List": "項目符號清單",
	"Insert Ordered List": "編號清單",
	"Align Center": "置中",
	"Align Justify": "文字對齊",
	"Align Left": "靠左",
	"Align Right": "靠右",
	"Insert Horizontal Line": "分割線",
	"Insert Image": "圖片",
	"Insert file": "檔案",
	"Insert youtube/vimeo video": "插入 youtube/vimeo 影片",
	"Insert link": "插入連結",
	"Font size": "文字大小",
	"Font family": "字型",
	"Insert format block": "格式化區塊",
	Normal: "內文",
	"Heading 1": "標題1",
	"Heading 2": "標題2",
	"Heading 3": "標題3",
	"Heading 4": "標題4",
	Quote: "引文",
	Code: "程式碼",
	Insert: "插入",
	"Insert table": "表格",
	"Decrease Indent": "減少縮排",
	"Increase Indent": "增加縮排",
	"Select Special Character": "選擇特殊符號",
	"Insert Special Character": "特殊符號",
	"Paint format": "格式複製",
	"Change mode": "檢視原始碼",
	Margins: "邊距",
	top: "上",
	right: "右",
	bottom: "下",
	left: "左",
	Styles: "樣式",
	Classes: "Classes",
	Align: "對齊方式",
	Right: "靠右",
	Center: "置中",
	Left: "靠左",
	"--Not Set--": "無",
	Src: "Src",
	Title: "Title",
	Alternative: "替代",
	Link: "Link",
	"Open link in new tab": "在新分頁開啟連結",
	Image: "圖片",
	file: "檔案",
	"Image properties": "圖片屬性",
	Cancel: "取消",
	Ok: "確定",
	"Your code is similar to HTML. Keep as HTML?": "您的程式碼與 HTML 類似，是否貼上 HTML 格式？",
	"Paste as HTML": "貼上 HTML",
	Keep: "保留原始格式",
	Clean: "清除格式",
	"Insert as Text": "以純文字貼上",
	"Word Paste Detected": "貼上 Word 格式",
	"The pasted content is coming from a Microsoft Word/Excel document. Do you want to keep the format or clean it up?": "正在貼上 Word/Excel 文件的內容，是否保留原始格式？",
	"Insert only Text": "僅貼上內文",
	"File Browser": "檔案瀏覽",
	"Error on load list": "清單載入錯誤",
	"Error on load folders": "資料夾載入錯誤",
	"Are you sure?": "您確定嗎？",
	"Enter Directory name": "輸入路徑",
	"Create directory": "創建路徑",
	"type name": "type name",
	"type dir name": "type name",
	"Drop image": "拖曳圖片至此",
	"Drop file": "拖曳檔案至此",
	"or click": "或點擊",
	"Alternative text": "替代文字",
	Browse: "瀏覽",
	Upload: "上傳",
	Background: "背景色",
	Text: "文字",
	Top: "頂部",
	Middle: "中間",
	Bottom: "底部",
	"Insert column before": "插入左方欄",
	"Insert column after": "插入右方欄",
	"Insert row above": "插入上方列",
	"Insert row below": "插入下方列",
	"Delete table": "刪除表格",
	"Delete row": "刪除整列",
	"Delete column": "刪除整欄",
	"Empty cell": "清除內容",
	"Chars: %d": "字元數: %d",
	"Words: %d": "單字數: %d",
	"Strike through": "刪除線",
	Underline: "底線",
	superscript: "上標",
	subscript: "下標",
	"Cut selection": "剪下",
	"Select all": "全選",
	Break: "斷行",
	"Search for": "尋找",
	"Replace with": "取代為",
	Replace: "取代",
	Paste: "貼上",
	"Choose Content to Paste": "選擇內容並貼上",
	All: "全部",
	source: "原始碼",
	bold: "粗體",
	italic: "斜體",
	brush: "顏色",
	link: "連結",
	undo: "復原",
	redo: "取消復原",
	table: "表格",
	image: "圖片",
	eraser: "橡皮擦",
	paragraph: "段落",
	fontsize: "文字大小",
	video: "影片",
	font: "字型",
	about: "關於",
	print: "列印",
	underline: "底線",
	strikethrough: "刪除線",
	indent: "增加縮排",
	outdent: "減少縮排",
	fullsize: "全螢幕",
	shrink: "縮減",
	hr: "分隔線",
	ul: "項目符號清單",
	ol: "編號清單",
	cut: "剪下",
	selectall: "全選",
	"Open link": "打開連結",
	"Edit link": "編輯連結",
	"No follow": "No follow",
	Unlink: "取消連結",
	Eye: "查看",
	" URL": "URL",
	Reset: "重設",
	Save: "儲存",
	"Save as ...": "另存為...",
	Resize: "調整大小",
	Crop: "裁切",
	Width: "寬",
	Height: "高",
	"Keep Aspect Ratio": "維持長寬比",
	Yes: "是",
	No: "否",
	Remove: "移除",
	Select: "選擇",
	"Select %s": "選擇: %s",
	Update: "更新",
	"Vertical align": "垂直對齊",
	Merge: "合併",
	"Add column": "新增欄",
	"Add row": "新增列",
	Border: "邊框",
	"Embed code": "嵌入程式碼",
	Delete: "刪除",
	"Horizontal align": "水平對齊",
	Filter: "篩選",
	"Sort by changed": "修改時間排序",
	"Sort by name": "名稱排序",
	"Sort by size": "大小排序",
	"Add folder": "新增資料夾",
	Split: "分割",
	"Split vertical": "垂直分割",
	"Split horizontal": "水平分割",
	"You can only edit your own images. Download this image on the host?": "您只能編輯您自己的圖片。是否下載此圖片?",
	"The image has been successfully uploaded to the host!": "圖片上傳成功",
	palette: "調色盤",
	pencil: "鉛筆",
	"There are no files": "沒有檔案",
	Rename: "重新命名",
	"Enter new name": "輸入新名稱",
	preview: "預覽",
	download: "下載",
	"Paste from clipboard": "從剪貼簿貼上",
	"Your browser doesn't support direct access to the clipboard.": "瀏覽器無法存取剪貼簿。",
	"Copy selection": "複製已選取項目",
	copy: "複製",
	"Border radius": "邊框圓角",
	"Show all": "顯示全部",
	Apply: "應用",
	"Please fill out this field": "請輸入此欄位",
	"Please enter a web address": "請輸入網址",
	Default: "預設",
	Circle: "圓圈",
	Dot: "點",
	Quadrate: "方形",
	Find: "尋找",
	"Find Previous": "尋找上一個",
	"Find Next": "尋找下一個",
	"Insert className": "插入 class 名稱",
	"Press Alt for custom resizing": "按住 Alt 以調整自訂大小",
	Edit: "編輯",
	"License: %s": "許可證: %s"
};
//#endregion
//#region node_modules/jodit/esm/languages.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var exp = {};
exp = {
	ar: ar_default,
	cs_cz: cs_cz_default,
	de: de_default,
	en: en_default,
	es: es_default,
	fa: fa_default,
	fi: fi_default,
	fr: fr_default,
	he: he_default,
	hu: hu_default,
	id: id_default,
	it: it_default,
	ja: ja_default,
	ko: ko_default,
	mn: mn_default,
	nl: nl_default,
	no: no_default,
	pl: pl_default,
	pt_br: pt_br_default,
	ru: ru_default,
	sk: sk_default,
	sv: sv_default,
	tr: tr_default,
	ua: ua_default,
	zh_cn: zh_cn_default,
	zh_tw: zh_tw_default
};
var get = (value) => value ? value.default || value : {};
var hashLang = {};
if (isArray(get(keys_default))) get(keys_default).forEach((key, index) => {
	hashLang[index] = key;
});
Object.keys(exp).forEach((lang) => {
	const list = get(exp[lang]);
	if (isArray(list)) {
		exp[lang] = {};
		list.forEach((value, index) => {
			exp[lang][hashLang[index]] = value;
		});
	} else exp[lang] = list;
});
var languages_default = exp;
//#endregion
//#region node_modules/jodit/esm/plugins/about/about.svg.js
var about_svg_default = "<svg viewBox=\"0 0 1792 1792\" xmlns=\"http://www.w3.org/2000/svg\"> <path d=\"M1088 1256v240q0 16-12 28t-28 12h-240q-16 0-28-12t-12-28v-240q0-16 12-28t28-12h240q16 0 28 12t12 28zm316-600q0 54-15.5 101t-35 76.5-55 59.5-57.5 43.5-61 35.5q-41 23-68.5 65t-27.5 67q0 17-12 32.5t-28 15.5h-240q-15 0-25.5-18.5t-10.5-37.5v-45q0-83 65-156.5t143-108.5q59-27 84-56t25-76q0-42-46.5-74t-107.5-32q-65 0-108 29-35 25-107 115-13 16-31 16-12 0-25-8l-164-125q-13-10-15.5-25t5.5-28q160-266 464-266 80 0 161 31t146 83 106 127.5 41 158.5z\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/plugins/about/about.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
Config.prototype.controls.about = {
	exec: (editor) => {
		const dialog = editor.dlg({ closeOnClickOverlay: true }), i = editor.i18n.bind(editor);
		dialog.setMod("theme", editor.o.theme).setHeader(i("About Jodit")).setContent(`<div class="jodit-about">
					<div>${i("Jodit Editor")} v.${editor.getVersion()}</div>
					<div>${i("License: %s", !isLicense(editor.o.license) ? editor.o.license || "MIT" : normalizeLicense(editor.o.license))}</div>
					<div>
						<a href="${HOMEPAGE}" target="_blank">${HOMEPAGE}</a>
					</div>
					<div>
						<a href="https://xdsoft.net/jodit/docs/" target="_blank">${i("Jodit User's Guide")}</a>
						${i("contains detailed help for using")}
					</div>
					<div>${i("Copyright © XDSoft.net - Chupurnov Valerii. All rights reserved.")}</div>
				</div>`);
		css(dialog.dialog, {
			minHeight: 200,
			minWidth: 420
		});
		dialog.open(true, true);
	},
	tooltip: "About Jodit",
	mode: 3
};
function about(editor) {
	editor.registerButton({
		name: "about",
		group: "info"
	});
}
pluginSystem.add("about", about);
Icon.set("about", about_svg_default);
//#endregion
//#region node_modules/jodit/esm/plugins/backspace/config.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module plugins/backspace
*/
Config.prototype.delete = { hotkeys: {
	delete: ["delete", "cmd+backspace"],
	deleteWord: [
		"ctrl+delete",
		"cmd+alt+backspace",
		"ctrl+alt+backspace"
	],
	deleteSentence: ["ctrl+shift+delete", "cmd+shift+delete"],
	backspace: ["backspace"],
	backspaceWord: ["ctrl+backspace"],
	backspaceSentence: ["ctrl+shift+backspace", "cmd+shift+backspace"]
} };
//#endregion
//#region node_modules/jodit/esm/plugins/backspace/cases/check-not-collapsed.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* On Not collapsed selection - should only remove whole selected content
*
* @example
* ```html
* <p>first | stop</p><p>second | stop</p>
* ```
* result
* ```html
* <p>first | stop</p>
* ```
* @private
*/
function checkNotCollapsed(jodit) {
	if (!jodit.s.isCollapsed()) {
		jodit.execCommand("Delete");
		return true;
	}
	return false;
}
//#endregion
//#region node_modules/jodit/esm/plugins/backspace/helpers.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Finds the nearest neighbor that would be in the maximum nesting depth.
* Ie if neighbor `<DIV><SPAN>Text` then return Text node.
* @private
*/
function findMostNestedNeighbor(node, right, root, onlyInlide = false) {
	const nextChild = (node) => right ? node.firstChild : node.lastChild;
	let next = Dom.findNotEmptyNeighbor(node, !right, root);
	if (onlyInlide && Dom.isElement(next) && !Dom.isInlineBlock(next)) return null;
	if (next) do
		if (nextChild(next)) next = nextChild(next);
		else return next;
	while (next);
	return null;
}
/**
* @private
*/
function getMoveFilter(jodit) {
	return (node) => jodit.e.fire("backSpaceIsMovedIgnore", node) !== true;
}
//#endregion
//#region node_modules/jodit/esm/plugins/backspace/cases/check-join-neighbors.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Check if two separate elements can be connected
* @private
*/
function checkJoinNeighbors(jodit, fakeNode, backspace) {
	let nextBox = fakeNode, mainClosestBox = nextBox;
	while (nextBox && !Dom.findNotEmptySibling(nextBox, backspace) && nextBox.parentElement !== jodit.editor) {
		nextBox = nextBox.parentElement;
		mainClosestBox = nextBox;
	}
	if (Dom.isElement(mainClosestBox) && Dom.isContentEditable(mainClosestBox, jodit.editor)) {
		const sibling = Dom.findNotEmptySibling(mainClosestBox, backspace);
		if (Dom.isTag(sibling, "br")) {
			Dom.safeRemove(sibling);
			jodit.s.setCursorBefore(fakeNode);
			return true;
		}
		if (sibling && (checkMoveListContent(jodit, mainClosestBox, sibling, backspace) || moveContentAndRemoveEmpty(jodit, mainClosestBox, resolveTableSibling(sibling, backspace), backspace))) {
			jodit.s.setCursorBefore(fakeNode);
			return true;
		}
	}
	return false;
}
/**
* Content cannot be merged into the `<table>` element itself — it would land
* between the table sections (after `</tbody>`), which is invalid HTML and
* gets foster-parented out of the table on the next parse. Merge into the
* edge cell instead. See https://github.com/xdan/jodit/issues/1064
* @private
*/
function resolveTableSibling(sibling, backspace) {
	if (!Dom.isTag(sibling, "table")) return sibling;
	const cells = [];
	Dom.each(sibling, (node) => {
		Dom.isCell(node) && cells.push(node);
	});
	return cells.length ? cells[backspace ? cells.length - 1 : 0] : null;
}
function checkMoveListContent(jodit, mainClosestBox, sibling, backspace) {
	const siblingIsList = Dom.isTag(sibling, LIST_TAGS);
	const boxIsList = Dom.isTag(mainClosestBox, LIST_TAGS);
	const elementChild = (elm, side) => side ? elm.firstElementChild : elm.lastElementChild;
	if (boxIsList) {
		sibling = jodit.createInside.element(jodit.o.enterBlock);
		Dom.before(mainClosestBox, sibling);
		return moveContentAndRemoveEmpty(jodit, elementChild(mainClosestBox, backspace), sibling, backspace);
	}
	if (sibling && siblingIsList && !boxIsList) return moveContentAndRemoveEmpty(jodit, mainClosestBox, elementChild(sibling, !backspace), backspace);
	return false;
}
function moveContentAndRemoveEmpty(jodit, mainClosestBox, sibling, backspace) {
	if (mainClosestBox && Dom.isElement(sibling)) {
		Dom.moveContent(mainClosestBox, sibling, !backspace, getMoveFilter(jodit));
		let remove = mainClosestBox;
		while (remove && remove !== jodit.editor && Dom.isEmpty(remove)) {
			const parent = remove.parentElement;
			Dom.safeRemove(remove);
			remove = parent;
		}
		return true;
	}
	return false;
}
//#endregion
//#region node_modules/jodit/esm/plugins/backspace/cases/check-join-two-lists.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Try join two UL elements
*
* @example
* ```html
* <ul><li>one</li></ul>|<ol><li>two</li></ol>
* ```
* Result
* ```html
* <ul><li>one|</li><li>two</li></ul>
* ```
* @private
*/
function checkJoinTwoLists(jodit, fakeNode, backspace) {
	const next = Dom.findSibling(fakeNode, backspace), prev = Dom.findSibling(fakeNode, !backspace);
	if (!Dom.closest(fakeNode, Dom.isElement, jodit.editor) && Dom.isList(next) && Dom.isList(prev) && Dom.isTag(next.lastElementChild, "li") && Dom.isTag(prev.firstElementChild, "li")) {
		const { setCursorBefore, setCursorAfter } = jodit.s;
		const target = next.lastElementChild, second = prev.firstElementChild;
		call(!backspace ? Dom.append : Dom.prepend, second, fakeNode);
		Dom.moveContent(prev, next, !backspace, getMoveFilter(jodit));
		Dom.safeRemove(prev);
		call(backspace ? Dom.append : Dom.prepend, target, fakeNode);
		call(backspace ? setCursorBefore : setCursorAfter, fakeNode);
		return true;
	}
	return false;
}
//#endregion
//#region node_modules/jodit/esm/plugins/backspace/cases/check-remove-char.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Check possibility the char can be removed
*
* @example
* ```html
* te|st
* ```
* result
* ```html
* t|st
* ```
* @private
*/
function checkRemoveChar(jodit, fakeNode, backspace, mode) {
	var _a, _b;
	const step = backspace ? -1 : 1;
	const anotherSibling = Dom.sibling(fakeNode, !backspace);
	let sibling = Dom.sibling(fakeNode, backspace);
	let removeNeighbor = null;
	let charRemoved = false;
	let removed;
	if (!sibling) sibling = getNextInlineSibling(fakeNode, backspace, jodit.editor);
	while (sibling && (Dom.isText(sibling) || Dom.isInlineBlock(sibling))) {
		while (Dom.isInlineBlock(sibling)) sibling = backspace ? sibling === null || sibling === void 0 ? void 0 : sibling.lastChild : sibling === null || sibling === void 0 ? void 0 : sibling.firstChild;
		if (!sibling) break;
		if ((_a = sibling.nodeValue) === null || _a === void 0 ? void 0 : _a.length) {
			removed = tryRemoveChar(sibling, backspace, step, anotherSibling);
			if (!sibling.nodeValue.length && Dom.isInlineBlock(sibling.parentNode)) sibling.nodeValue = "﻿";
		}
		if (!((_b = sibling.nodeValue) === null || _b === void 0 ? void 0 : _b.length)) removeNeighbor = sibling;
		if (!isVoid(removed) && removed !== "﻿") {
			checkRepeatRemoveCharAction(backspace, sibling, fakeNode, mode, removed, jodit);
			charRemoved = true;
			break;
		}
		const nextSibling = getNextInlineSibling(sibling, backspace, jodit.editor);
		if (removeNeighbor) {
			Dom.safeRemove(removeNeighbor);
			removeNeighbor = null;
		}
		sibling = nextSibling;
	}
	if (removeNeighbor) {
		Dom.safeRemove(removeNeighbor);
		removeNeighbor = null;
	}
	if (charRemoved) {
		removeEmptyForParent(fakeNode, "a");
		addBRInsideEmptyBlock(jodit, fakeNode);
		jodit.s.setCursorBefore(fakeNode);
		if (Dom.isTag(fakeNode.previousSibling, "br") && !Dom.findNotEmptySibling(fakeNode, false)) Dom.after(fakeNode, jodit.createInside.element("br"));
	}
	return charRemoved;
}
function getNextInlineSibling(sibling, backspace, root) {
	let nextSibling = Dom.sibling(sibling, backspace);
	if (!nextSibling && sibling.parentNode && sibling.parentNode !== root) {
		nextSibling = findMostNestedNeighbor(sibling, !backspace, root, true);
		if (nextSibling && Dom.closest(sibling, "li", root) !== Dom.closest(nextSibling, "li", root)) return null;
	}
	return nextSibling;
}
/**
* Helper removes all empty inline parents
*/
function removeEmptyForParent(node, tags) {
	let parent = node.parentElement;
	while (parent && Dom.isInlineBlock(parent) && Dom.isTag(parent, tags)) {
		const p = parent.parentElement;
		if (Dom.isEmpty(parent)) {
			Dom.after(parent, node);
			Dom.safeRemove(parent);
		}
		parent = p;
	}
}
/**
* Helper add BR element inside empty block element
*/
function addBRInsideEmptyBlock(jodit, node) {
	if (node.parentElement !== jodit.editor && Dom.isBlock(node.parentElement) && Dom.each(node.parentElement, Dom.isEmptyTextNode)) Dom.after(node, jodit.createInside.element("br"));
}
function tryRemoveChar(sibling, backspace, step, anotherSibling) {
	let value = toArray(sibling.nodeValue);
	const length = value.length;
	let index = backspace ? length - 1 : 0;
	if (value[index] === "﻿") while (value[index] === "﻿") index += step;
	const removed = value[index];
	if (value[index + step] === "﻿") {
		index += step;
		while (value[index] === "﻿") index += step;
		index += backspace ? 1 : -1;
	}
	if (backspace && index < 0) value = [];
	else value = value.slice(backspace ? 0 : index + 1, backspace ? index : length);
	replaceSpaceOnNBSP(anotherSibling, backspace, value);
	sibling.nodeValue = value.join("");
	return removed;
}
function replaceSpaceOnNBSP(anotherSibling, backspace, value) {
	var _a;
	if (!anotherSibling || !Dom.isText(anotherSibling) || (!backspace ? / $/ : /^ /).test((_a = anotherSibling.nodeValue) !== null && _a !== void 0 ? _a : "") || !trimInv(anotherSibling.nodeValue || "").length) for (let i = backspace ? value.length - 1 : 0; backspace ? i >= 0 : i < value.length; i += backspace ? -1 : 1) if (value[i] === " ") value[i] = "\xA0";
	else break;
}
function checkRepeatRemoveCharAction(backspace, sibling, fakeNode, mode, removed, jodit) {
	call(backspace ? Dom.after : Dom.before, sibling, fakeNode);
	if (mode === "sentence" || mode === "word" && removed !== " " && removed !== "\xA0") checkRemoveChar(jodit, fakeNode, backspace, mode);
}
//#endregion
//#region node_modules/jodit/esm/plugins/backspace/cases/check-remove-content-not-editable.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Checks if a non-editable element can be deleted
* @private
*/
function checkRemoveContentNotEditable(jodit, fakeNode, backspace) {
	let neighbor = Dom.findSibling(fakeNode, backspace);
	if (!neighbor && fakeNode.parentElement && fakeNode.parentElement !== jodit.editor) neighbor = Dom.findSibling(fakeNode.parentElement, backspace);
	if (Dom.isElement(neighbor) && !Dom.isContentEditable(neighbor, jodit.editor)) {
		call(backspace ? Dom.before : Dom.after, neighbor, fakeNode);
		Dom.safeRemove(neighbor);
		moveNodeInsideStart(jodit, fakeNode, backspace);
		call(backspace ? jodit.s.setCursorBefore : jodit.s.setCursorAfter, fakeNode);
		return true;
	}
	return false;
}
//#endregion
//#region node_modules/jodit/esm/plugins/backspace/cases/check-remove-empty-neighbor.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Check if it is possible to remove an empty adjacent element.
*
* @example
* ```html
* <p><br></p><p>|second stop</p>
* ```
* result
* ```html
* <p>|second stop</p>
* ```
* @private
*/
function checkRemoveEmptyNeighbor(jodit, fakeNode, backspace) {
	const parent = Dom.closest(fakeNode, Dom.isElement, jodit.editor);
	if (!parent) return false;
	const neighbor = Dom.findNotEmptySibling(parent, backspace);
	if (neighbor && Dom.isEmpty(neighbor)) {
		Dom.safeRemove(neighbor);
		jodit.s.setCursorBefore(fakeNode);
		return true;
	}
	return false;
}
//#endregion
//#region node_modules/jodit/esm/plugins/backspace/cases/check-remove-empty-parent.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Check if the current empty item can be removed
*
* @example
* ```html
* <p>first stop</p><p>|<br></p>
* ```
* result
* ```html
* <p>first stop|</p>
* ```
*
* @private
*/
function checkRemoveEmptyParent(jodit, fakeNode, backspace) {
	let found = false;
	const { setCursorBefore, setCursorIn } = jodit.s;
	let prn = Dom.closest(fakeNode, Dom.isElement, jodit.editor);
	if (!prn || !Dom.isEmpty(prn)) return false;
	const neighbor = Dom.findNotEmptyNeighbor(fakeNode, backspace, jodit.editor);
	do
		if (prn && Dom.isEmpty(prn) && !Dom.isCell(prn)) {
			Dom.after(prn, fakeNode);
			const tmp = Dom.closest(prn, (n) => Dom.isElement(n) && n !== prn, jodit.editor);
			Dom.safeRemove(prn);
			found = true;
			prn = tmp;
		} else break;
	while (prn);
	if (found && checkJoinTwoLists(jodit, fakeNode, backspace)) return true;
	if (neighbor && !Dom.isText(neighbor) && !Dom.isTag(neighbor, INSEPARABLE_TAGS)) setCursorIn(neighbor, !backspace);
	else setCursorBefore(fakeNode);
	return found;
}
//#endregion
//#region node_modules/jodit/esm/plugins/backspace/cases/check-remove-unbreakable-element.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Check possibility inseparable Element can be removed (img, hr etc.)
*
* @example
* ```html
* <p>first second <img>| stop</p>
* ```
* result
* ```html
* <p>first second | stop</p>
* ```
*
* @private
*/
function checkRemoveUnbreakableElement(jodit, fakeNode, backspace) {
	const neighbor = Dom.findSibling(fakeNode, backspace);
	if (Dom.isElement(neighbor) && (Dom.isTag(neighbor, INSEPARABLE_TAGS) || Dom.isEmpty(neighbor))) {
		Dom.safeRemove(neighbor);
		if (Dom.isTag(neighbor, "br") && !Dom.findNotEmptySibling(fakeNode, false)) Dom.after(fakeNode, jodit.createInside.element("br"));
		jodit.s.setCursorBefore(fakeNode);
		if (Dom.isTag(neighbor, "br")) checkRemoveEmptyParent(jodit, fakeNode, backspace);
		return true;
	}
	return false;
}
//#endregion
//#region node_modules/jodit/esm/plugins/backspace/cases/check-table-cell.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Inside the CELL table - nothing to do
*
* @example
* ```html
* <table><tr><td>|test</td></tr></table>
* ```
* result
* ```html
* <table><tr><td>|test</td></tr></table>
* ```
*
* @private
*/
function checkTableCell(jodit, fakeNode) {
	const cell = fakeNode.parentElement;
	if (Dom.isCell(cell)) return true;
	return false;
}
//#endregion
//#region node_modules/jodit/esm/plugins/backspace/cases/check-unwrap-first-list-item.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* For the first item in a list on backspace, try to move his content in new P
*
* @example
* ```html
* <ul><li>|first</li><li>second</li></ul>
* ```
* Result
* ```html
* <p>|first</p><ul><li>second</li></ul>
* ```
*
* @private
*/
function checkUnwrapFirstListItem(jodit, fakeNode, backspace) {
	var _a;
	const li = Dom.closest(fakeNode, Dom.isElement, jodit.editor);
	const { s } = jodit;
	if (Dom.isLeaf(li) && ((_a = li === null || li === void 0 ? void 0 : li.parentElement) === null || _a === void 0 ? void 0 : _a[backspace ? "firstElementChild" : "lastElementChild"]) === li && s.cursorInTheEdge(backspace, li)) {
		const ul = li.parentElement;
		const p = jodit.createInside.element(jodit.o.enterBlock);
		call(backspace ? Dom.before : Dom.after, ul, p);
		Dom.moveContent(li, p);
		Dom.safeRemove(li);
		if (Dom.isEmpty(ul)) Dom.safeRemove(ul);
		call(backspace ? s.setCursorBefore : s.setCursorAfter, fakeNode);
		return true;
	}
	return false;
}
//#endregion
//#region node_modules/jodit/esm/plugins/backspace/cases/index.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Ordered delete/backspace cases with stable keys. The first one returning
* `true` wins. The keys are stable across minified builds (function names are
* mangled by terser) so they can be referenced by `delete.disableCases`.
* @private
*/
var casesMap = [
	["remove-unbreakable", checkRemoveUnbreakableElement],
	["remove-not-editable", checkRemoveContentNotEditable],
	["remove-char", checkRemoveChar],
	["table-cell", checkTableCell],
	["remove-empty-parent", checkRemoveEmptyParent],
	["remove-empty-neighbor", checkRemoveEmptyNeighbor],
	["join-two-lists", checkJoinTwoLists],
	["join-neighbors", checkJoinNeighbors],
	["unwrap-first-list-item", checkUnwrapFirstListItem]
];
casesMap.map(([, fn]) => fn);
//#endregion
//#region node_modules/jodit/esm/plugins/backspace/backspace.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var backspace = class extends Plugin {
	afterInit(jodit) {
		jodit.registerCommand("deleteButton", {
			exec: () => this.onDelete(false),
			hotkeys: jodit.o.delete.hotkeys.delete
		}, { stopPropagation: false }).registerCommand("backspaceButton", {
			exec: () => this.onDelete(true),
			hotkeys: jodit.o.delete.hotkeys.backspace
		}, { stopPropagation: false }).registerCommand("deleteWordButton", {
			exec: () => this.onDelete(false, "word"),
			hotkeys: jodit.o.delete.hotkeys.deleteWord
		}).registerCommand("backspaceWordButton", {
			exec: () => this.onDelete(true, "word"),
			hotkeys: jodit.o.delete.hotkeys.backspaceWord
		}).registerCommand("deleteSentenceButton", {
			exec: () => this.onDelete(false, "sentence"),
			hotkeys: jodit.o.delete.hotkeys.deleteSentence
		}).registerCommand("backspaceSentenceButton", {
			exec: () => this.onDelete(true, "sentence"),
			hotkeys: jodit.o.delete.hotkeys.backspaceSentence
		});
	}
	beforeDestruct(jodit) {
		jodit.e.off("afterCommand.delete");
	}
	/**
	* Listener BackSpace or Delete button
	*/
	onDelete(backspace, mode = "char") {
		const jodit = this.j;
		const sel = jodit.selection;
		if (!sel.isFocused()) sel.focus();
		if (checkNotCollapsed(jodit)) {
			jodit.e.fire("backSpaceAfterDelete", backspace);
			return false;
		}
		const range = sel.range;
		const fakeNode = jodit.createInside.text("﻿");
		try {
			Dom.safeInsertNode(range, fakeNode);
			if (!Dom.isOrContains(jodit.editor, fakeNode)) return;
			if (jodit.e.fire("backSpaceBeforeCases", backspace, fakeNode)) return false;
			moveNodeInsideStart(jodit, fakeNode, backspace);
			const disabled = jodit.o.delete.disableCases;
			if (casesMap.some(([key, func]) => {
				if (disabled && disabled.has(key)) return;
				if (isFunction(func) && func(jodit, fakeNode, backspace, mode)) return true;
			})) return false;
		} catch (e) {
			throw e;
		} finally {
			jodit.e.fire("backSpaceAfterDelete", backspace, fakeNode);
			this.safeRemoveEmptyNode(fakeNode);
		}
		return false;
	}
	/**
	* Remove node and replace cursor position out of it
	*/
	safeRemoveEmptyNode(fakeNode) {
		var _a, _b;
		const { range } = this.j.s;
		if (range.startContainer === fakeNode) {
			if (fakeNode.previousSibling) if (Dom.isText(fakeNode.previousSibling)) range.setStart(fakeNode.previousSibling, (_b = (_a = fakeNode.previousSibling.nodeValue) === null || _a === void 0 ? void 0 : _a.length) !== null && _b !== void 0 ? _b : 0);
			else range.setStartAfter(fakeNode.previousSibling);
			else if (fakeNode.nextSibling) if (Dom.isText(fakeNode.nextSibling)) range.setStart(fakeNode.nextSibling, 0);
			else range.setStartBefore(fakeNode.nextSibling);
			range.collapse(true);
			this.j.s.selectRange(range);
		}
		Dom.safeRemove(fakeNode);
	}
};
backspace.requires = ["hotkeys"];
pluginSystem.add("backspace", backspace);
//#endregion
//#region node_modules/jodit/esm/plugins/delete/delete.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var NOT_EMPTY_CONTENT_TAGS = /* @__PURE__ */ new Set([
	"img",
	"table",
	"jodit",
	"iframe",
	"hr"
]);
var deleteCommand = class extends Plugin {
	afterInit(jodit) {
		jodit.e.on("afterCommand.delete", (command) => {
			if (command === "delete") this.__afterDeleteCommand();
		});
		jodit.registerCommand("delete", { exec: this.__onDeleteCommand.bind(this) }, { stopPropagation: false });
	}
	beforeDestruct(jodit) {
		jodit.e.off("afterCommand.delete");
	}
	/**
	* After Delete command remove extra BR
	*/
	__afterDeleteCommand() {
		const jodit = this.j;
		const current = jodit.s.current();
		if (current && Dom.isTag(current.firstChild, "br")) jodit.s.removeNode(current.firstChild);
		if (!trim(jodit.editor.textContent || "") && !Dom.first(jodit.editor, (node) => Dom.isTag(node, NOT_EMPTY_CONTENT_TAGS)) && (!current || !Dom.closest(current, "table", jodit.editor))) {
			Dom.detach(jodit.editor);
			const node = jodit.s.setCursorIn(jodit.editor);
			jodit.s.removeNode(node);
		}
	}
	__onDeleteCommand() {
		const { jodit } = this;
		if (jodit.s.isCollapsed()) return;
		jodit.s.expandSelection();
		const range = jodit.s.range;
		range.deleteContents();
		const fake = jodit.createInside.fake();
		range.insertNode(fake);
		const leftSibling = Dom.findSibling(fake, true);
		const rightSibling = Dom.findSibling(fake, false);
		this.__moveContentInLeftSibling(fake, leftSibling, rightSibling);
		range.setStartBefore(fake);
		range.collapse(true);
		this.__moveCursorInEditableSibling(jodit, leftSibling, fake, range);
		this.__addBrInEmptyBlock(fake, rightSibling, range);
		Dom.safeRemove(fake);
		jodit.s.selectRange(range);
		return false;
	}
	__moveContentInLeftSibling(fake, leftSibling, rightSibling) {
		leftSibling = this.__defineRightLeftBox(leftSibling);
		if (!Dom.isList(rightSibling) && !Dom.isTag(rightSibling, "table") && Dom.isBlock(rightSibling) && Dom.isBlock(leftSibling)) {
			Dom.append(leftSibling, fake);
			Dom.moveContent(rightSibling, leftSibling);
			Dom.safeRemove(rightSibling);
		}
		if (Dom.isList(rightSibling) && Dom.isLeaf(rightSibling.firstElementChild) && Dom.isEmpty(rightSibling.firstElementChild)) Dom.safeRemove(rightSibling.firstElementChild);
	}
	/**
	* If left sibling is list - return last leaf
	*/
	__defineRightLeftBox(leftSibling) {
		if (!Dom.isList(leftSibling)) return leftSibling;
		let lastLeaf = leftSibling.lastElementChild;
		if (!Dom.isLeaf(lastLeaf)) {
			lastLeaf = this.j.createInside.element("li");
			Dom.append(leftSibling, lastLeaf);
		}
		return lastLeaf;
	}
	/**
	* Add BR in empty blocks left and right(for table cell)
	*/
	__addBrInEmptyBlock(fake, rightSibling, range) {
		const jodit = this.j;
		if (fake.isConnected && Dom.isBlock(fake.parentNode) && !fake.nextSibling && !fake.previousSibling) {
			const br = jodit.createInside.element("br");
			Dom.after(fake, br);
			range.setStartBefore(br);
			range.collapse(true);
		}
		if (Dom.isTag(rightSibling, "table")) {
			const firstCell = $$("td,th", rightSibling).shift();
			if (Dom.isCell(firstCell) && Dom.isEmpty(firstCell)) Dom.append(firstCell, jodit.createInside.element("br"));
		}
	}
	__moveCursorInEditableSibling(jodit, leftSibling, fake, range) {
		var _a;
		if (!leftSibling || !Dom.isText(leftSibling)) {
			const root = (_a = Dom.closest(fake, Dom.isBlock, jodit.editor)) !== null && _a !== void 0 ? _a : jodit.editor;
			const leftText = Dom.prev(fake, Dom.isText, root);
			if (leftText) {
				range.setStartAfter(leftText);
				range.collapse(true);
				Dom.safeRemove(fake);
			}
		}
	}
};
deleteCommand.requires = ["backspace"];
pluginSystem.add("deleteCommand", deleteCommand);
//#endregion
//#region node_modules/jodit/esm/plugins/bold/config.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
Config.prototype.controls.subscript = {
	tags: ["sub"],
	tooltip: "subscript"
};
Config.prototype.controls.superscript = {
	tags: ["sup"],
	tooltip: "superscript"
};
Config.prototype.controls.bold = {
	tagRegExp: /^(strong|b)$/i,
	tags: ["strong", "b"],
	css: { "font-weight": ["bold", "700"] },
	tooltip: "Bold"
};
Config.prototype.controls.italic = {
	tagRegExp: /^(em|i)$/i,
	tags: ["em", "i"],
	css: { "font-style": "italic" },
	tooltip: "Italic"
};
Config.prototype.controls.underline = {
	tagRegExp: /^(u)$/i,
	tags: ["u"],
	css: { "text-decoration-line": "underline" },
	tooltip: "Underline"
};
Config.prototype.controls.strikethrough = {
	tagRegExp: /^(s)$/i,
	tags: ["s"],
	css: { "text-decoration-line": "line-through" },
	tooltip: "Strike through"
};
//#endregion
//#region node_modules/jodit/esm/plugins/bold/icons/bold.svg.js
var bold_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"> <path d=\"M747 1521q74 32 140 32 376 0 376-335 0-114-41-180-27-44-61.5-74t-67.5-46.5-80.5-25-84-10.5-94.5-2q-73 0-101 10 0 53-.5 159t-.5 158q0 8-1 67.5t-.5 96.5 4.5 83.5 12 66.5zm-14-746q42 7 109 7 82 0 143-13t110-44.5 74.5-89.5 25.5-142q0-70-29-122.5t-79-82-108-43.5-124-14q-50 0-130 13 0 50 4 151t4 152q0 27-.5 80t-.5 79q0 46 1 69zm-541 889l2-94q15-4 85-16t106-27q7-12 12.5-27t8.5-33.5 5.5-32.5 3-37.5.5-34v-65.5q0-982-22-1025-4-8-22-14.5t-44.5-11-49.5-7-48.5-4.5-30.5-3l-4-83q98-2 340-11.5t373-9.5q23 0 68.5.5t67.5.5q70 0 136.5 13t128.5 42 108 71 74 104.5 28 137.5q0 52-16.5 95.5t-39 72-64.5 57.5-73 45-84 40q154 35 256.5 134t102.5 248q0 100-35 179.5t-93.5 130.5-138 85.5-163.5 48.5-176 14q-44 0-132-3t-132-3q-106 0-307 11t-231 12z\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/plugins/bold/icons/italic.svg.js
var italic_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"> <path d=\"M384 1662l17-85q6-2 81.5-21.5t111.5-37.5q28-35 41-101 1-7 62-289t114-543.5 52-296.5v-25q-24-13-54.5-18.5t-69.5-8-58-5.5l19-103q33 2 120 6.5t149.5 7 120.5 2.5q48 0 98.5-2.5t121-7 98.5-6.5q-5 39-19 89-30 10-101.5 28.5t-108.5 33.5q-8 19-14 42.5t-9 40-7.5 45.5-6.5 42q-27 148-87.5 419.5t-77.5 355.5q-2 9-13 58t-20 90-16 83.5-6 57.5l1 18q17 4 185 31-3 44-16 99-11 0-32.5 1.5t-32.5 1.5q-29 0-87-10t-86-10q-138-2-206-2-51 0-143 9t-121 11z\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/plugins/bold/icons/strikethrough.svg.js
var strikethrough_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"> <path d=\"M1760 896q14 0 23 9t9 23v64q0 14-9 23t-23 9h-1728q-14 0-23-9t-9-23v-64q0-14 9-23t23-9h1728zm-1277-64q-28-35-51-80-48-97-48-188 0-181 134-309 133-127 393-127 50 0 167 19 66 12 177 48 10 38 21 118 14 123 14 183 0 18-5 45l-12 3-84-6-14-2q-50-149-103-205-88-91-210-91-114 0-182 59-67 58-67 146 0 73 66 140t279 129q69 20 173 66 58 28 95 52h-743zm507 256h411q7 39 7 92 0 111-41 212-23 55-71 104-37 35-109 81-80 48-153 66-80 21-203 21-114 0-195-23l-140-40q-57-16-72-28-8-8-8-22v-13q0-108-2-156-1-30 0-68l2-37v-44l102-2q15 34 30 71t22.5 56 12.5 27q35 57 80 94 43 36 105 57 59 22 132 22 64 0 139-27 77-26 122-86 47-61 47-129 0-84-81-157-34-29-137-71z\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/plugins/bold/icons/subscript.svg.js
var subscript_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"> <path d=\"M1025 1369v167h-248l-159-252-24-42q-8-9-11-21h-3l-9 21q-10 20-25 44l-155 250h-258v-167h128l197-291-185-272h-137v-168h276l139 228q2 4 23 42 8 9 11 21h3q3-9 11-21l25-42 140-228h257v168h-125l-184 267 204 296h109zm639 217v206h-514l-4-27q-3-45-3-46 0-64 26-117t65-86.5 84-65 84-54.5 65-54 26-64q0-38-29.5-62.5t-70.5-24.5q-51 0-97 39-14 11-36 38l-105-92q26-37 63-66 80-65 188-65 110 0 178 59.5t68 158.5q0 66-34.5 118.5t-84 86-99.5 62.5-87 63-41 73h232v-80h126z\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/plugins/bold/icons/superscript.svg.js
var superscript_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"> <path d=\"M1025 1369v167h-248l-159-252-24-42q-8-9-11-21h-3l-9 21q-10 20-25 44l-155 250h-258v-167h128l197-291-185-272h-137v-168h276l139 228q2 4 23 42 8 9 11 21h3q3-9 11-21l25-42 140-228h257v168h-125l-184 267 204 296h109zm637-679v206h-514l-3-27q-4-28-4-46 0-64 26-117t65-86.5 84-65 84-54.5 65-54 26-64q0-38-29.5-62.5t-70.5-24.5q-51 0-97 39-14 11-36 38l-105-92q26-37 63-66 83-65 188-65 110 0 178 59.5t68 158.5q0 56-24.5 103t-62 76.5-81.5 58.5-82 50.5-65.5 51.5-30.5 63h232v-80h126z\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/plugins/bold/icons/underline.svg.js
var underline_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"> <path d=\"M176 223q-37-2-45-4l-3-88q13-1 40-1 60 0 112 4 132 7 166 7 86 0 168-3 116-4 146-5 56 0 86-2l-1 14 2 64v9q-60 9-124 9-60 0-79 25-13 14-13 132 0 13 .5 32.5t.5 25.5l1 229 14 280q6 124 51 202 35 59 96 92 88 47 177 47 104 0 191-28 56-18 99-51 48-36 65-64 36-56 53-114 21-73 21-229 0-79-3.5-128t-11-122.5-13.5-159.5l-4-59q-5-67-24-88-34-35-77-34l-100 2-14-3 2-86h84l205 10q76 3 196-10l18 2q6 38 6 51 0 7-4 31-45 12-84 13-73 11-79 17-15 15-15 41 0 7 1.5 27t1.5 31q8 19 22 396 6 195-15 304-15 76-41 122-38 65-112 123-75 57-182 89-109 33-255 33-167 0-284-46-119-47-179-122-61-76-83-195-16-80-16-237v-333q0-188-17-213-25-36-147-39zm1488 1409v-64q0-14-9-23t-23-9h-1472q-14 0-23 9t-9 23v64q0 14 9 23t23 9h1472q14 0 23-9t9-23z\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/plugins/bold/bold.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Adds `bold`,` strikethrough`, `underline` and` italic` buttons to Jodit
*/
function bold(editor) {
	const callBack = (command) => {
		const control = Config.defaultOptions.controls[command], cssOptions = { ...control.css };
		let cssRules;
		Object.keys(cssOptions).forEach((key) => {
			if (!cssRules) cssRules = {};
			cssRules[key] = isArray(cssOptions[key]) ? cssOptions[key][0] : cssOptions[key];
		});
		editor.s.commitStyle({ element: control.tags ? control.tags[0] : void 0 });
		editor.synchronizeValues();
		return false;
	};
	[
		"bold",
		"italic",
		"underline",
		"strikethrough"
	].forEach((name) => {
		editor.registerButton({
			name,
			group: "font-style"
		});
	});
	["superscript", "subscript"].forEach((name) => {
		editor.registerButton({
			name,
			group: "script"
		});
	});
	editor.registerCommand("bold", {
		exec: callBack,
		hotkeys: ["ctrl+b", "cmd+b"]
	}).registerCommand("italic", {
		exec: callBack,
		hotkeys: ["ctrl+i", "cmd+i"]
	}).registerCommand("underline", {
		exec: callBack,
		hotkeys: ["ctrl+u", "cmd+u"]
	}).registerCommand("strikethrough", { exec: callBack }).registerCommand("subscript", { exec: callBack }).registerCommand("superscript", { exec: callBack });
}
pluginSystem.add("bold", bold);
Icon.set("bold", bold_svg_default).set("italic", italic_svg_default).set("strikethrough", strikethrough_svg_default).set("subscript", subscript_svg_default).set("superscript", superscript_svg_default).set("underline", underline_svg_default);
//#endregion
//#region node_modules/jodit/esm/modules/widget/color-picker/color-picker.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
Icon.set("palette", "<svg xmlns='http://www.w3.org/2000/svg' x=\"0px\" y=\"0px\" viewBox=\"0 0 459 459\"> <g> <path d=\"M229.5,0C102,0,0,102,0,229.5S102,459,229.5,459c20.4,0,38.25-17.85,38.25-38.25c0-10.2-2.55-17.85-10.2-25.5 c-5.1-7.65-10.2-15.3-10.2-25.5c0-20.4,17.851-38.25,38.25-38.25h45.9c71.4,0,127.5-56.1,127.5-127.5C459,91.8,357,0,229.5,0z M89.25,229.5c-20.4,0-38.25-17.85-38.25-38.25S68.85,153,89.25,153s38.25,17.85,38.25,38.25S109.65,229.5,89.25,229.5z M165.75,127.5c-20.4,0-38.25-17.85-38.25-38.25S145.35,51,165.75,51S204,68.85,204,89.25S186.15,127.5,165.75,127.5z M293.25,127.5c-20.4,0-38.25-17.85-38.25-38.25S272.85,51,293.25,51s38.25,17.85,38.25,38.25S313.65,127.5,293.25,127.5z M369.75,229.5c-20.4,0-38.25-17.85-38.25-38.25S349.35,153,369.75,153S408,170.85,408,191.25S390.15,229.5,369.75,229.5z\" /> </g> </svg> ");
/**
* Build color picker
*
* @param callback - Callback 'function (color) \{\}'
* @param coldColor - Color value ex. #fff or rgb(123, 123, 123) or rgba(123, 123, 123, 1)
* @example
* ```javascript
* const tabs = TabsWidget(editor, {
*    'Text': ColorPickerWidget(editor, function (color) {
*         box.style.color = color;
*     }, box.style.color),
*     'Background': ColorPickerWidget(editor, function (color) {
*         box.style.backgroundColor = color;
*     }, box.style.backgroundColor),
* });
* ```
*/
var ColorPickerWidget = (editor, callback, coldColor) => {
	const cn = "jodit-color-picker", valueHex = normalizeColor(coldColor), form = editor.c.div(cn), iconPalette = editor.o.textIcons ? `<span>${editor.i18n("palette")}</span>` : Icon.get("palette"), eachColor = (colors) => {
		const stack = [];
		if (isPlainObject(colors)) Object.keys(colors).forEach((key) => {
			stack.push(`<div class="${cn}__group ${cn}__group-${key}">`);
			stack.push(eachColor(colors[key]));
			stack.push("</div>");
		});
		else if (isArray(colors)) colors.forEach((color) => {
			stack.push(`<span class='${cn}__color-item ${valueHex === color ? "jodit-color-picker__color-item_active_true" : ""}' title="${color}" style="background-color:${color}" data-color="${color}"></span>`);
		});
		return stack.join("");
	};
	Dom.append(form, editor.c.fromHTML(`<div class="${cn}__groups">${eachColor(editor.o.colors)}</div>`));
	Dom.append(form, editor.c.fromHTML(`<div data-ref="extra" class="${cn}__extra"></div>`));
	const { extra } = refs(form);
	Dom.append(extra, editor.c.fromHTML(`<div class="${cn}__hex"><input data-ref="hexInput" type="text" spellcheck="false" aria-label="HEX" placeholder="#FF0000" value="${valueHex || ""}"/></div>`));
	const { hexInput } = refs(form);
	let appliedHexValue = null;
	const applyHexInput = () => {
		const raw = hexInput.value.trim();
		if (raw === appliedHexValue) return;
		const isHex = /^#?[0-9a-f]{3}(?:[0-9a-f]{3})?$/i.test(raw);
		const isRgb = /^rgba?\([\d\s.,%]+\)$/i.test(raw);
		if (!isHex && !isRgb) return;
		const color = normalizeColor(isHex && !raw.startsWith("#") ? "#" + raw : raw);
		if (color && isFunction(callback)) {
			appliedHexValue = raw;
			callback(color);
		}
	};
	editor.e.on(hexInput, "keydown", (e) => {
		e.stopPropagation();
		if (e.key === "Enter") {
			e.preventDefault();
			applyHexInput();
		}
	}).on(hexInput, "change", (e) => {
		e.stopPropagation();
		applyHexInput();
	});
	if (editor.o.showBrowserColorPicker && hasBrowserColorPicker()) {
		Dom.append(extra, editor.c.fromHTML(`<div class="${cn}__native">${iconPalette}<input type="color" value="#ffffff"/></div>`));
		editor.e.on(form, "change", (e) => {
			e.stopPropagation();
			const target = e.target;
			if (!target || !target.tagName || !Dom.isTag(target, "input") || target.type !== "color") return;
			const color = target.value || "";
			if (isFunction(callback)) callback(color);
			e.preventDefault();
		});
	}
	editor.e.on(form, "mousedown touchend", (e) => {
		if (Dom.isTag(e.target, "input")) {
			e.stopPropagation();
			return;
		}
		e.stopPropagation();
		e.preventDefault();
		let target = e.target;
		if ((!target || !target.tagName || Dom.isTag(target, "svg") || Dom.isTag(target, "path")) && target.parentNode) target = Dom.closest(target.parentNode, "span", editor.editor);
		if (!Dom.isTag(target, "span") || !target.classList.contains("jodit-color-picker__color-item")) return;
		const color = attr(target, "-color") || "";
		if (callback && isFunction(callback)) callback(color);
	});
	editor.e.fire("afterGenerateColorPicker", form, extra, callback, valueHex);
	return form;
};
//#endregion
//#region node_modules/jodit/esm/modules/widget/tabs/tabs.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Build tabs system
*
* @param tabs - PlainObject where 'key' will be tab's Title and `value` is tab's content
* @param state - You can use for this param any HTML element for remembering active tab
*
* @example
* ```javascript
* const editor = Jodit.make('#editor');
* const tabs = Jodit.modules.TabsWidget(editor, [
*  { name: 'Images', content: '<div>Images</div>' },
*  {
*    name: 'Title 2',
*    content: editor.c.fromHTML('<div>Some content</div>')
*  },
*  {
*    name: 'Color Picker',
*    content: ColorPickerWidget(
*      editor,
*      function (color) {
*        box.style.color = color;
*      },
*      box.style.color
*    )
*  }
* ]);
* ```
*/
var TabsWidget = (jodit, tabs, state) => {
	const box = jodit.c.div("jodit-tabs");
	const tabBox = jodit.c.div("jodit-tabs__wrapper");
	const buttons = jodit.c.div("jodit-tabs__buttons");
	attr(buttons, {
		role: "tablist",
		"aria-orientation": "horizontal"
	});
	const nameToTab = {};
	const buttonList = [];
	let firstTab = "";
	Dom.append(box, buttons);
	Dom.append(box, tabBox);
	const setActive = (tab) => {
		if (!nameToTab[tab]) return;
		buttonList.forEach((b) => {
			b.state.activated = false;
		});
		Object.values(nameToTab).forEach(({ tab }) => tab.classList.remove("jodit-tab_active"));
		nameToTab[tab].button.state.activated = true;
		nameToTab[tab].tab.classList.add("jodit-tab_active");
	};
	tabs.forEach(({ icon, name, content }) => {
		const tab = jodit.c.div("jodit-tab");
		attr(tab, { role: "tabpanel" });
		const button = Button(jodit, icon || name, name);
		button.state.role = "tab";
		jodit.e.on(button.container, "pointerdown", (e) => e.preventDefault());
		if (!firstTab) firstTab = name;
		Dom.append(buttons, button.container);
		buttonList.push(button);
		button.container.classList.add("jodit-tabs__button", "jodit-tabs__button_columns_" + tabs.length);
		if (!isFunction(content)) Dom.append(tab, Component.isInstanceOf(content, UIElement) ? content.container : content);
		else tab.classList.add("jodit-tab_empty");
		Dom.append(tabBox, tab);
		button.onAction(() => {
			setActive(name);
			if (isFunction(content) && !Dom.isElement(content)) content.call(jodit);
			if (state) state.activeTab = name;
			return false;
		});
		nameToTab[name] = {
			button,
			tab
		};
	});
	setActive(!state || !state.activeTab || !nameToTab[state.activeTab] ? firstTab : state.activeTab);
	if (state) {
		let activeTab = state.activeTab;
		Object.defineProperty(state, "activeTab", {
			configurable: true,
			enumerable: false,
			get() {
				return activeTab;
			},
			set(value) {
				activeTab = value;
				setActive(value);
			}
		});
	}
	return box;
};
//#endregion
//#region node_modules/jodit/esm/modules/widget/file-selector/file-selector.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Generate 3 tabs
* upload - Use Drag and Drop
* url - By specifying the image url
* filebrowser - After opening the file browser. In the absence of one of the parameters will be less tabs
*
* @param callbacks - Object with keys `url`, `upload` and `filebrowser`, values which are callback
* functions with different parameters
*/
var FileSelectorWidget = (editor, callbacks, elm, close, isImage = true) => {
	let currentImage;
	const tabs = [];
	const options = editor.o.uploader;
	if (callbacks.upload && options.showTabInFileSelector !== false && (options.url || options.insertImageAsBase64URI || options.customUploadFunction)) {
		const dragBox = editor.c.fromHTML(`<div class="jodit-drag-and-drop__file-box"><strong>${editor.i18n(isImage ? "Drop image" : "Drop file")}</strong><span><br>${editor.i18n("or click")}</span><input type="file" accept="${isImage ? "image/*" : "*"}" tabindex="-1" dir="auto" multiple=""/></div>`);
		editor.uploader.bind(dragBox, (resp) => {
			const handler = isFunction(callbacks.upload) ? callbacks.upload : options.defaultHandlerSuccess;
			if (isFunction(handler)) handler.call(editor, resp);
			editor.e.fire("closeAllPopups");
		}, (error) => {
			editor.message.error(error.message);
			editor.e.fire("closeAllPopups");
		});
		tabs.push({
			icon: "upload",
			name: "Upload",
			content: dragBox
		});
	}
	if (callbacks.filebrowser) {
		if (editor.o.filebrowser.ajax.url || editor.o.filebrowser.items.url) tabs.push({
			icon: "folder",
			name: "Browse",
			content: () => {
				close && close();
				if (callbacks.filebrowser) editor.filebrowser.open(callbacks.filebrowser, isImage);
			}
		});
	}
	if (callbacks.url) {
		const button = new UIButton(editor, {
			type: "submit",
			variant: "primary",
			text: "Insert"
		});
		const form = new UIForm(editor, [
			new UIInput(editor, {
				required: true,
				label: "URL",
				name: "url",
				type: "text",
				placeholder: "https://"
			}),
			new UIInput(editor, {
				name: "text",
				label: "Alternative text"
			}),
			new UIBlock(editor, [button])
		]);
		currentImage = null;
		if (elm && !Dom.isText(elm) && !Dom.isComment(elm) && (Dom.isTag(elm, "img") || $$("img", elm).length)) {
			currentImage = elm.tagName === "IMG" ? elm : $$("img", elm)[0];
			val(form.container, "url", attr(currentImage, "src"));
			val(form.container, "text", attr(currentImage, "alt"));
			button.state.text = "Update";
		}
		if (elm && Dom.isTag(elm, "a")) {
			val(form.container, "url", attr(elm, "href"));
			val(form.container, "text", attr(elm, "title"));
			button.state.text = "Update";
		}
		form.onSubmit((data) => {
			if (isFunction(callbacks.url)) callbacks.url.call(editor, data.url, data.text);
		});
		tabs.push({
			icon: "link",
			name: "URL",
			content: form.container
		});
	}
	const box = TabsWidget(editor, tabs);
	box.classList.add("jodit-file-selector");
	return box;
};
function val(elm, name, value) {
	const child = Dom.first(elm, (node) => Dom.isTag(node, "input") && attr(node, "name") === name);
	if (!child) return "";
	if (value) child.value = value;
	return child.value;
}
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
//#endregion
//#region node_modules/jodit/esm/plugins/color/config.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
Icon.set("brush", "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"> <path d=\"M896 1152q0-36-20-69-1-1-15.5-22.5t-25.5-38-25-44-21-50.5q-4-16-21-16t-21 16q-7 23-21 50.5t-25 44-25.5 38-15.5 22.5q-20 33-20 69 0 53 37.5 90.5t90.5 37.5 90.5-37.5 37.5-90.5zm512-128q0 212-150 362t-362 150-362-150-150-362q0-145 81-275 6-9 62.5-90.5t101-151 99.5-178 83-201.5q9-30 34-47t51-17 51.5 17 33.5 47q28 93 83 201.5t99.5 178 101 151 62.5 90.5q81 127 81 275z\"/> </svg> ");
Config.prototype.controls.brushCell = {
	isVisible: (editor) => {
		return !editor.o.disablePlugins.includes("color");
	},
	icon: "brush",
	popup: (editor, _, close) => {
		if (!isJoditObject(editor)) return;
		const selected = editor.getInstance("Table", editor.o).getAllSelectedCells();
		if (!selected.length) return false;
		const makeColorPicker = (key) => ColorPickerWidget(editor, (value) => {
			selected.forEach((cell) => {
				css(cell, key, value);
			});
			editor.lock();
			editor.synchronizeValues();
			close();
			editor.unlock();
		}, css(selected[0], key));
		return TabsWidget(editor, [
			{
				name: "Background",
				content: makeColorPicker("background-color")
			},
			{
				name: "Text",
				content: makeColorPicker("color")
			},
			{
				name: "Border",
				content: makeColorPicker("border-color")
			}
		]);
	},
	tooltip: "Background"
};
Config.prototype.controls.brush = {
	isVisible: (editor) => {
		return !editor.o.disablePlugins.includes("color");
	},
	update(editor, button) {
		const color = dataBind(button, "color");
		const update = (key, value) => {
			if (value && value !== css(editor.editor, key).toString()) {
				button.state.icon.fill = value;
				return true;
			}
			return false;
		};
		if (color) {
			const mode = dataBind(button, "color");
			update(mode === "color" ? mode : "background-color", color);
			return;
		}
		const current = editor.s.current();
		if (current && !button.state.disabled) {
			const currentBpx = Dom.closest(current, Dom.isElement, editor.editor) || editor.editor;
			const hasColor = update("color", css(currentBpx, "color").toString());
			const hasBackground = update("background-color", css(currentBpx, "background-color").toString());
			if (hasColor || hasBackground) return;
		}
		button.state.icon.fill = "";
		button.state.activated = false;
	},
	popup: (editor, current, close, button) => {
		let colorHEX = "", bg_color = "", tabs = [], currentElement = null;
		if (current && current !== editor.editor && Dom.isNode(current)) {
			if (Dom.isElement(current) && editor.s.isCollapsed() && !Dom.isTag(current, /* @__PURE__ */ new Set(["br", "hr"]))) currentElement = current;
			Dom.up(current, (node) => {
				if (Dom.isHTMLElement(node)) {
					const color = css(node, "color", true), background = css(node, "background-color", true);
					if (color) {
						colorHEX = color.toString();
						return true;
					}
					if (background) {
						bg_color = background.toString();
						return true;
					}
				}
			}, editor.editor);
		}
		const backgroundTag = ColorPickerWidget(editor, (value) => {
			if (!currentElement) editor.execCommand("background", false, value);
			else css(currentElement, "backgroundColor", value);
			dataBind(button, "color", value);
			dataBind(button, "color-mode", "background");
			close();
		}, bg_color);
		const colorTab = ColorPickerWidget(editor, (value) => {
			if (!currentElement) editor.execCommand("forecolor", false, value);
			else css(currentElement, "color", value);
			dataBind(button, "color", value);
			dataBind(button, "color-mode", "color");
			close();
		}, colorHEX);
		tabs = [{
			name: "Background",
			content: backgroundTag
		}, {
			name: "Text",
			content: colorTab
		}];
		if (editor.o.colorPickerDefaultTab !== "background") tabs = tabs.reverse();
		return TabsWidget(editor, tabs, currentElement);
	},
	exec(jodit, current, { button }) {
		const mode = dataBind(button, "color-mode"), color = dataBind(button, "color");
		if (!mode) return false;
		if (current && current !== jodit.editor && Dom.isNode(current) && Dom.isElement(current)) switch (mode) {
			case "color":
				css(current, "color", color);
				break;
			case "background":
				css(current, "backgroundColor", color);
				break;
		}
		else jodit.execCommand(mode === "background" ? mode : "forecolor", false, color);
	},
	tooltip: "Fill color or set the text color"
};
//#endregion
//#region node_modules/jodit/esm/plugins/color/color.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Process commands `background` and `forecolor`
*/
function color(editor) {
	editor.registerButton({
		name: "brush",
		group: "color"
	});
	const callback = (command, second, third) => {
		var _a;
		const colorHEX = normalizeColor(third);
		const value = !colorHEX ? "" : colorHEX;
		const style = command === "background" ? { backgroundColor: value } : { color: value };
		const selectedCells = editor.getInstance("Table", editor.o).getAllSelectedCells();
		if (selectedCells.length && editor.s.isCollapsed()) {
			selectedCells.forEach((cell) => {
				editor.s.select(cell, true);
				editor.s.commitStyle({ attributes: { style } });
			});
			(_a = editor.s.sel) === null || _a === void 0 || _a.removeAllRanges();
		} else editor.s.commitStyle({ attributes: { style } });
		editor.synchronizeValues();
		return false;
	};
	editor.registerCommand("forecolor", callback).registerCommand("background", callback);
}
pluginSystem.add("color", color);
//#endregion
//#region node_modules/jodit/esm/plugins/enter/helpers/check-br.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Checks the possibility and necessity of inserting a BR instead of a block
* @private
*/
function checkBR(fake, jodit, shiftKeyPressed) {
	const isMultiLineBlock = Boolean(Dom.closest(fake, ["pre", "blockquote"], jodit.editor));
	const isCell = !isMultiLineBlock && Dom.closest(fake, ["td", "th"], jodit.editor);
	if (jodit.o.enter.toLowerCase() === "br".toLowerCase() || isCell || shiftKeyPressed && !isMultiLineBlock || !shiftKeyPressed && isMultiLineBlock) {
		if (isMultiLineBlock && checkSeveralBR(fake)) return false;
		const br = jodit.createInside.element("br");
		Dom.before(fake, br);
		if (!Dom.findNotEmptySibling(br, false)) {
			const clone = br.cloneNode();
			Dom.after(br, clone);
			Dom.before(clone, fake);
		}
		scrollIntoViewIfNeeded(br, jodit.editor, jodit.ed);
		return true;
	}
	return false;
}
function checkSeveralBR(fake) {
	const preBr = brBefore(brBefore(fake));
	if (preBr) {
		Dom.safeRemove(brBefore(fake));
		Dom.safeRemove(preBr);
		return true;
	}
	return false;
}
function brBefore(start) {
	if (!start) return false;
	const prev = Dom.findSibling(start, true);
	if (!prev || !Dom.isTag(prev, "br")) return false;
	return prev;
}
//#endregion
//#region node_modules/jodit/esm/plugins/enter/helpers/check-unsplittable-box.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Inside quote/tables cell, etc. you can't split so just add br
* @private
*/
function checkUnsplittableBox(fake, jodit, currentBox) {
	if (!Dom.canSplitBlock(currentBox)) {
		Dom.before(fake, jodit.createInside.element("br"));
		return false;
	}
	return true;
}
//#endregion
//#region node_modules/jodit/esm/plugins/enter/helpers/get-block-wrapper.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Finds a suitable parent block container
* @private
*/
function getBlockWrapper(fake, jodit, tagReg = IS_BLOCK) {
	let node = fake;
	const root = jodit.editor;
	do {
		if (!node || node === root) break;
		if (tagReg.test(node.nodeName)) {
			if (Dom.isLeaf(node)) return node;
			return getBlockWrapper(node.parentNode, jodit, /^li$/i) || node;
		}
		node = node.parentNode;
	} while (node && node !== root);
	return null;
}
//#endregion
//#region node_modules/jodit/esm/plugins/enter/helpers/has-previous-block.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @private
*/
function hasPreviousBlock(fake, jodit) {
	return Boolean(Dom.prev(fake, (elm) => Dom.isBlock(elm) || Dom.isImage(elm), jodit.editor));
}
//#endregion
//#region node_modules/jodit/esm/plugins/enter/helpers/insert-paragraph.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Insert default paragraph
* @private
*/
function insertParagraph(fake, editor, wrapperTag, style) {
	const isBR = wrapperTag.toLowerCase() === "br", { createInside } = editor, p = createInside.element(wrapperTag), br = createInside.element("br");
	if (!isBR) Dom.append(p, br);
	if (style && style.cssText) attr(p, "style", style.cssText);
	Dom.after(fake, p);
	Dom.before(isBR ? p : br, fake);
	scrollIntoViewIfNeeded(p, editor.editor, editor.ed);
	return p;
}
//#endregion
//#region node_modules/jodit/esm/plugins/enter/helpers/move-cursor-out-from-specal-tags.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Checks if the cursor is on the edge of a special tag and exits if so
* @private
*/
function moveCursorOutFromSpecialTags(jodit, fake, tags) {
	const { s } = jodit;
	const link = Dom.closest(fake, tags, jodit.editor);
	if (link) {
		if (s.cursorOnTheRight(link, fake)) Dom.after(link, fake);
		else if (s.cursorOnTheLeft(link, fake)) Dom.before(link, fake);
	}
}
//#endregion
//#region node_modules/jodit/esm/plugins/enter/helpers/process-empty-li-leaf.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Handles pressing the Enter key inside an empty LI inside a list
* @private
*/
function processEmptyLILeaf(fake, jodit, li) {
	const list = Dom.closest(li, ["ol", "ul"], jodit.editor);
	if (!list) return;
	const parentLi = list.parentElement, listInsideLeaf = Dom.isLeaf(parentLi);
	const container = listInsideLeaf ? parentLi : list;
	const leftRange = jodit.s.createRange();
	leftRange.setStartAfter(li);
	leftRange.setEndAfter(list);
	const rightPart = leftRange.extractContents();
	Dom.after(container, fake);
	Dom.safeRemove(li);
	if (!$$("li", list).length) Dom.safeRemove(list);
	const newLi = insertParagraph(fake, jodit, listInsideLeaf ? "li" : jodit.o.enter);
	if (!Dom.first(rightPart, (node) => Dom.isTag(node, "li"))) return;
	if (listInsideLeaf) Dom.append(newLi, rightPart);
	else Dom.after(newLi, rightPart);
}
//#endregion
//#region node_modules/jodit/esm/plugins/enter/helpers/split-fragment.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Splits a block element into two parts
* and adds a new default block in the middle/start/end
* @private
*/
function splitFragment(fake, jodit, block) {
	const sel = jodit.s, { enter } = jodit.o;
	const defaultTag = enter.toLowerCase();
	const isLi = Dom.isLeaf(block);
	const canSplit = block.tagName.toLowerCase() === defaultTag || isLi;
	const cursorOnTheRight = sel.cursorOnTheRight(block, fake);
	const cursorOnTheLeft = sel.cursorOnTheLeft(block, fake);
	if (!canSplit && (cursorOnTheRight || cursorOnTheLeft)) {
		if (cursorOnTheRight) Dom.after(block, fake);
		else Dom.before(block, fake);
		insertParagraph(fake, jodit, defaultTag);
		if (cursorOnTheLeft && !cursorOnTheRight) Dom.prepend(block, fake);
		return;
	}
	const { scrollTop } = jodit.editor;
	sel.splitSelection(block, fake);
	if (jodit.editor.scrollTop !== scrollTop) jodit.editor.scrollTop = scrollTop;
	scrollIntoViewIfNeeded(block, jodit.editor, jodit.ed);
}
//#endregion
//#region node_modules/jodit/esm/plugins/enter/helpers/wrap-text.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* If there is no container outside,
* then we wrap all the nearest inline nodes in a container
* @private
*/
function wrapText(fake, jodit) {
	let needWrap = fake;
	Dom.up(needWrap, (node) => {
		if (node && node.hasChildNodes() && node !== jodit.editor) needWrap = node;
	}, jodit.editor);
	const currentBox = Dom.wrapInline(needWrap, jodit.o.enter, jodit);
	if (Dom.isEmpty(currentBox)) {
		const br = jodit.createInside.element("br");
		Dom.append(currentBox, br);
		Dom.before(br, fake);
	}
	return currentBox;
}
//#endregion
//#region node_modules/jodit/esm/plugins/enter/helpers/index.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
//#endregion
//#region node_modules/jodit/esm/plugins/enter/enter.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$7 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
/**
* One of most important core plugins. It is responsible for all the browsers to have the same effect when the Enter
* button is pressed. By default, it should insert the <p>
*/
var enter = class extends Plugin {
	/** @override */
	afterInit(editor) {
		const defaultTag = editor.o.enter.toLowerCase();
		const brMode = defaultTag === "br".toLowerCase();
		if (!editor.o.enterBlock) editor.o.enterBlock = brMode ? "p" : defaultTag;
		editor.registerCommand("enter", (command, value, event = {}) => this.onEnter(event));
	}
	onEnterKeyDown(event) {
		if (event.key === "Enter") {
			const editor = this.j;
			const beforeEnter = editor.e.fire("beforeEnter", event);
			if (beforeEnter !== void 0) return beforeEnter;
			editor.history.snapshot.transaction(() => {
				if (!editor.s.isCollapsed()) editor.execCommand("Delete");
				editor.s.focus();
				this.onEnter(event);
				editor.e.fire("afterEnter", event);
			});
			editor.synchronizeValues();
			return false;
		}
	}
	onEnter(event) {
		const { jodit } = this;
		const fake = jodit.createInside.fake();
		try {
			Dom.safeInsertNode(jodit.s.range, fake);
			moveCursorOutFromSpecialTags(jodit, fake, ["a"]);
			let block = getBlockWrapper(fake, jodit);
			const isLi = Dom.isLeaf(block);
			if ((!isLi || (event === null || event === void 0 ? void 0 : event.shiftKey)) && checkBR(fake, jodit, event === null || event === void 0 ? void 0 : event.shiftKey)) return false;
			if (!block && !hasPreviousBlock(fake, jodit)) block = wrapText(fake, jodit);
			if (!block) {
				insertParagraph(fake, jodit, isLi ? "li" : jodit.o.enter);
				return false;
			}
			if (!checkUnsplittableBox(fake, jodit, block)) return false;
			if (isLi && this.__isEmptyListLeaf(block)) {
				processEmptyLILeaf(fake, jodit, block);
				return false;
			}
			splitFragment(fake, jodit, block);
		} finally {
			fake.isConnected && jodit.s.setCursorBefore(fake);
			Dom.safeRemove(fake);
		}
	}
	__isEmptyListLeaf(li) {
		const result = this.j.e.fire("enterIsEmptyListLeaf", li);
		return isBoolean(result) ? result : Dom.isEmpty(li);
	}
	/** @override */
	beforeDestruct(editor) {
		editor.e.off("keydown.enter");
	}
};
__decorate$7([watch(":keydown.enter")], enter.prototype, "onEnterKeyDown", null);
pluginSystem.add("enter", enter);
//#endregion
//#region node_modules/jodit/esm/plugins/font/icons/font.svg.js
var font_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"> <path d=\"M789 559l-170 450q33 0 136.5 2t160.5 2q19 0 57-2-87-253-184-452zm-725 1105l2-79q23-7 56-12.5t57-10.5 49.5-14.5 44.5-29 31-50.5l237-616 280-724h128q8 14 11 21l205 480q33 78 106 257.5t114 274.5q15 34 58 144.5t72 168.5q20 45 35 57 19 15 88 29.5t84 20.5q6 38 6 57 0 4-.5 13t-.5 13q-63 0-190-8t-191-8q-76 0-215 7t-178 8q0-43 4-78l131-28q1 0 12.5-2.5t15.5-3.5 14.5-4.5 15-6.5 11-8 9-11 2.5-14q0-16-31-96.5t-72-177.5-42-100l-450-2q-26 58-76.5 195.5t-50.5 162.5q0 22 14 37.5t43.5 24.5 48.5 13.5 57 8.5 41 4q1 19 1 58 0 9-2 27-58 0-174.5-10t-174.5-10q-8 0-26.5 4t-21.5 4q-80 14-188 14z\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/plugins/font/icons/fontsize.svg.js
var fontsize_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"> <path d=\"M1744 1408q33 0 42 18.5t-11 44.5l-126 162q-20 26-49 26t-49-26l-126-162q-20-26-11-44.5t42-18.5h80v-1024h-80q-33 0-42-18.5t11-44.5l126-162q20-26 49-26t49 26l126 162q20 26 11 44.5t-42 18.5h-80v1024h80zm-1663-1279l54 27q12 5 211 5 44 0 132-2t132-2q36 0 107.5.5t107.5.5h293q6 0 21 .5t20.5 0 16-3 17.5-9 15-17.5l42-1q4 0 14 .5t14 .5q2 112 2 336 0 80-5 109-39 14-68 18-25-44-54-128-3-9-11-48t-14.5-73.5-7.5-35.5q-6-8-12-12.5t-15.5-6-13-2.5-18-.5-16.5.5q-17 0-66.5-.5t-74.5-.5-64 2-71 6q-9 81-8 136 0 94 2 388t2 455q0 16-2.5 71.5t0 91.5 12.5 69q40 21 124 42.5t120 37.5q5 40 5 50 0 14-3 29l-34 1q-76 2-218-8t-207-10q-50 0-151 9t-152 9q-3-51-3-52v-9q17-27 61.5-43t98.5-29 78-27q19-42 19-383 0-101-3-303t-3-303v-117q0-2 .5-15.5t.5-25-1-25.5-3-24-5-14q-11-12-162-12-33 0-93 12t-80 26q-19 13-34 72.5t-31.5 111-42.5 53.5q-42-26-56-44v-383z\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/plugins/font/config.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Default font-size points
*/
Config.prototype.defaultFontSizePoints = "px";
Icon.set("font", font_svg_default).set("fontsize", fontsize_svg_default);
Config.prototype.controls.fontsize = {
	command: "fontsize",
	data: {
		cssRule: "font-size",
		normalize: (v) => {
			return v.toString().replace(/(px|pt)$/i, "");
		}
	},
	list: [
		8,
		9,
		10,
		11,
		12,
		14,
		16,
		18,
		24,
		30,
		32,
		34,
		36,
		48,
		60,
		72,
		96
	],
	textTemplate: (editor, value) => {
		return value + editor.o.defaultFontSizePoints;
	},
	childTemplate: (editor, key, value) => {
		return `${value}${editor.o.defaultFontSizePoints}`;
	},
	tooltip: "Font size",
	value: (editor, button) => {
		var _a;
		const current = editor.s.current();
		if (!current) return;
		const box = Dom.closest(current, Dom.isElement, editor.editor);
		if (!box) return;
		const cssKey = ((_a = button.control.data) === null || _a === void 0 ? void 0 : _a.cssRule) || "font-size";
		const value = css(box, cssKey);
		if (cssKey === "font-size") {
			const numValue = parseFloat(value.toString());
			if (editor.o.defaultFontSizePoints === "pt") return Math.round(numValue * .75).toString();
			return Math.round(numValue).toString();
		}
		return value.toString();
	},
	isChildActive: (editor, button) => {
		var _a, _b;
		const value = button.state.value;
		const normalize = (_b = (_a = button.control.data) === null || _a === void 0 ? void 0 : _a.normalize) !== null && _b !== void 0 ? _b : ((v) => v);
		return Boolean(value && button.control.args && normalize(button.control.args[0].toString()) === normalize(value.toString()));
	},
	isActive: (editor, button) => {
		var _a, _b;
		const value = button.state.value;
		if (!value) return false;
		const normalize = (_b = (_a = button.control.data) === null || _a === void 0 ? void 0 : _a.normalize) !== null && _b !== void 0 ? _b : ((v) => v);
		let keySet = button.control.data.cacheListSet;
		if (!keySet) {
			const keys = Object.keys(button.control.list).map(normalize);
			keySet = new Set(keys);
			button.control.data.cacheListSet = keySet;
		}
		return keySet.has(normalize(value.toString()));
	}
};
Config.prototype.controls.font = {
	...Config.prototype.controls.fontsize,
	command: "fontname",
	textTemplate: (j, value) => {
		const [first] = value.split(",");
		return trimChars(first, "\"'");
	},
	list: {
		"": "Default",
		"Arial, Helvetica, sans-serif": "Arial",
		"'Courier New', Courier, monospace": "Courier New",
		"Georgia, Palatino, serif": "Georgia",
		"'Lucida Sans Unicode', 'Lucida Grande', sans-serif": "Lucida Sans Unicode",
		"Tahoma, Geneva, sans-serif": "Tahoma",
		"'Times New Roman', Times, serif": "Times New Roman",
		"'Trebuchet MS', Helvetica, sans-serif": "Trebuchet MS",
		"Helvetica, sans-serif": "Helvetica",
		"Impact, Charcoal, sans-serif": "Impact",
		"Verdana, Geneva, sans-serif": "Verdana"
	},
	childTemplate: (editor, key, value) => {
		let isAvailable = false;
		try {
			isAvailable = key.indexOf("dings") === -1 && document.fonts.check(`16px ${key}`, value);
		} catch (_a) {}
		return `<span data-style="${key}" style="${isAvailable ? `font-family: ${key}!important;` : ""}">${value}</span>`;
	},
	data: {
		cssRule: "font-family",
		normalize: (v) => {
			return v.toLowerCase().replace(/['"]+/g, "").replace(/[^a-z0-9-]+/g, ",");
		}
	},
	value: (editor) => {
		const current = editor.s.current();
		if (!current) return;
		const box = Dom.closest(current, Dom.isElement, editor.editor);
		if (!box) return;
		const value = css(box, "font-family").toString();
		if (value === css(editor.editor, "font-family").toString()) return "";
		return value;
	},
	tooltip: "Font family"
};
//#endregion
//#region node_modules/jodit/esm/plugins/font/font.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Process commands `fontsize` and `fontname`
*/
function font(editor) {
	editor.registerButton({
		name: "font",
		group: "font"
	}).registerButton({
		name: "fontsize",
		group: "font"
	});
	const callback = (command, second, third) => {
		switch (command) {
			case "fontsize":
				editor.s.commitStyle({ attributes: { style: { fontSize: normalizeSize(third, editor.o.defaultFontSizePoints) } } });
				break;
			case "fontname":
				editor.s.commitStyle({ attributes: { style: { fontFamily: third } } });
				break;
		}
		editor.synchronizeValues();
		return false;
	};
	editor.registerCommand("fontsize", callback).registerCommand("fontname", callback);
}
pluginSystem.add("font", font);
//#endregion
//#region node_modules/jodit/esm/plugins/format-block/config.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
Icon.set("paragraph", "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"><path d=\"M1534 189v73q0 29-18.5 61t-42.5 32q-50 0-54 1-26 6-32 31-3 11-3 64v1152q0 25-18 43t-43 18h-108q-25 0-43-18t-18-43v-1218h-143v1218q0 25-17.5 43t-43.5 18h-108q-26 0-43.5-18t-17.5-43v-496q-147-12-245-59-126-58-192-179-64-117-64-259 0-166 88-286 88-118 209-159 111-37 417-37h479q25 0 43 18t18 43z\"/></svg> ");
Config.prototype.controls.paragraph = {
	command: "formatBlock",
	value(editor, button) {
		var _a, _b;
		const control = button.control, current = editor.s.current();
		const currentBox = Dom.closest(current, Dom.isBlock, editor.editor);
		return (_a = currentBox === null || currentBox === void 0 ? void 0 : currentBox.nodeName.toLowerCase()) !== null && _a !== void 0 ? _a : (_b = control.data) === null || _b === void 0 ? void 0 : _b.currentValue;
	},
	update(editor, button) {
		const control = button.control;
		if (!editor.s.current()) return false;
		const currentValue = button.state.value, list = control.list;
		if (isPlainObject(list) && list[currentValue.toString()]) {
			if (editor.o.textIcons) button.state.text = list[currentValue.toString()].toString();
		}
		return false;
	},
	data: { currentValue: "p" },
	list: {
		p: "Paragraph",
		h1: "Heading 1",
		h2: "Heading 2",
		h3: "Heading 3",
		h4: "Heading 4",
		blockquote: "Quote",
		pre: "Code"
	},
	isChildActive: (editor, button) => {
		var _a, _b;
		return Boolean(button.state.value === ((_b = (_a = button.control) === null || _a === void 0 ? void 0 : _a.args) === null || _b === void 0 ? void 0 : _b[0]));
	},
	isActive: (editor, button) => {
		return button.state.value !== editor.o.enter && isPlainObject(button.control.list) && Boolean(button.control.list[button.state.value]);
	},
	childTemplate: (e, key, value) => `<${key} style="margin:0;padding:0"><span>${e.i18n(value)}</span></${key}>`,
	tooltip: "Insert format block"
};
//#endregion
//#region node_modules/jodit/esm/plugins/format-block/format-block.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Process command - `formatblock`
*/
function formatBlock(editor) {
	editor.registerButton({
		name: "paragraph",
		group: "font"
	});
	editor.registerCommand("formatblock", (command, second, third) => {
		editor.s.commitStyle({ element: third });
		editor.synchronizeValues();
		return false;
	});
}
pluginSystem.add("formatBlock", formatBlock);
//#endregion
//#region node_modules/jodit/esm/plugins/hotkeys/config.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
Config.prototype.commandToHotkeys = {
	removeFormat: ["ctrl+shift+m", "cmd+shift+m"],
	insertOrderedList: ["ctrl+shift+7", "cmd+shift+7"],
	insertUnorderedList: ["ctrl+shift+8", "cmd+shift+8"],
	selectall: ["ctrl+a", "cmd+a"]
};
//#endregion
//#region node_modules/jodit/esm/plugins/hotkeys/hotkeys.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Allow set hotkey for command or button
*/
var hotkeys = class extends Plugin {
	constructor() {
		super(...arguments);
		this.onKeyPress = (event) => {
			const special = this.specialKeys[event.which];
			const character = (event.key || String.fromCharCode(event.which)).toLowerCase();
			const modif = [special || character];
			[
				"alt",
				"ctrl",
				"shift",
				"meta"
			].forEach((specialKey) => {
				if (event[specialKey + "Key"] && special !== specialKey) modif.push(specialKey);
			});
			return normalizeKeyAliases(modif.join("+"));
		};
		this.specialKeys = {
			8: "backspace",
			9: "tab",
			10: "return",
			13: "return",
			16: "shift",
			17: "ctrl",
			18: "alt",
			19: "pause",
			20: "capslock",
			27: "esc",
			32: "space",
			33: "pageup",
			34: "pagedown",
			35: "end",
			36: "home",
			37: "left",
			38: "up",
			39: "right",
			40: "down",
			45: "insert",
			46: "del",
			59: ";",
			61: "=",
			91: "meta",
			96: "0",
			97: "1",
			98: "2",
			99: "3",
			100: "4",
			101: "5",
			102: "6",
			103: "7",
			104: "8",
			105: "9",
			106: "*",
			107: "+",
			109: "-",
			110: ".",
			111: "/",
			112: "f1",
			113: "f2",
			114: "f3",
			115: "f4",
			116: "f5",
			117: "f6",
			118: "f7",
			119: "f8",
			120: "f9",
			121: "f10",
			122: "f11",
			123: "f12",
			144: "numlock",
			145: "scroll",
			173: "-",
			186: ";",
			187: "=",
			188: ",",
			189: "-",
			190: ".",
			191: "/",
			192: "`",
			219: "[",
			220: "\\",
			221: "]",
			222: "'"
		};
	}
	/** @override */
	afterInit(editor) {
		keys(editor.o.commandToHotkeys, false).forEach((commandName) => {
			const shortcuts = editor.o.commandToHotkeys[commandName];
			if (shortcuts && (isArray(shortcuts) || isString(shortcuts))) editor.registerHotkeyToCommand(shortcuts, commandName);
		});
		let itIsHotkey = false;
		editor.e.off(".hotkeys").on([editor.ow, editor.ew], "keydown.hotkeys", (e) => {
			if (e.key === "Escape") return this.j.e.fire("escape", e);
		}).on("keydown.hotkeys", (event) => {
			const shortcut = this.onKeyPress(event);
			const stop = { shouldStop: true };
			if (this.j.e.fire(shortcut + ".hotkey", event.type, stop) === false) if (stop.shouldStop) {
				itIsHotkey = true;
				editor.e.stopPropagation("keydown");
				return false;
			} else event.preventDefault();
		}, { top: true }).on("keyup.hotkeys", () => {
			if (itIsHotkey) {
				itIsHotkey = false;
				editor.e.stopPropagation("keyup");
				return false;
			}
		}, { top: true });
	}
	/** @override */
	beforeDestruct(jodit) {
		if (jodit.events) jodit.e.off(".hotkeys");
	}
};
pluginSystem.add("hotkeys", hotkeys);
//#endregion
//#region node_modules/jodit/esm/plugins/iframe/config.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module plugins/iframe
*/
Config.prototype.iframeBaseUrl = "";
Config.prototype.iframeTitle = "Jodit Editor";
Config.prototype.iframeDoctype = "<!DOCTYPE html>";
Config.prototype.iframeDefaultSrc = "about:blank";
Config.prototype.iframeStyle = "html{margin:0;padding:0;min-height: 100%;}body{box-sizing:border-box;font-size:13px;line-height:1.6;padding:10px;margin:0;background:transparent;color:#000;position:relative;z-index:2;user-select:auto;margin:0px;overflow:auto;outline:none;}table{width:100%;border:none;border-collapse:collapse;empty-cells: show;max-width: 100%;}th,td{padding: 2px 5px;border:1px solid #ccc;-webkit-user-select:text;-moz-user-select:text;-ms-user-select:text;user-select:text}p{margin-top:0;}.jodit_editor .jodit_iframe_wrapper{display: block;clear: both;user-select: none;position: relative;}.jodit_editor .jodit_iframe_wrapper:after {position:absolute;content:\"\";z-index:1;top:0;left:0;right: 0;bottom: 0;cursor: pointer;display: block;background: rgba(0, 0, 0, 0);} .jodit_disabled{user-select: none;-o-user-select: none;-moz-user-select: none;-khtml-user-select: none;-webkit-user-select: none;-ms-user-select: none}";
Config.prototype.iframeCSSLinks = [];
Config.prototype.iframeSandbox = null;
//#endregion
//#region node_modules/jodit/esm/plugins/iframe/iframe.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Iframe plugin - use `iframe` instead of DIV in editor. It can be need when you want to attach custom styles in editor
* in backend of you system
*/
function iframe(editor) {
	const opt = editor.options;
	editor.e.on("afterSetMode", () => {
		if (editor.isEditorMode()) editor.s.focus();
	}).on("generateDocumentStructure.iframe", (__doc, jodit) => {
		const doc = __doc || jodit.iframe.contentWindow.document;
		doc.open();
		doc.write(opt.iframeDoctype + `<html dir="${opt.direction}" class="jodit" lang="${defaultLanguage(opt.language)}"><head><title>${opt.iframeTitle}</title>` + (opt.iframeBaseUrl ? `<base href="${opt.iframeBaseUrl}"/>` : "") + "</head><body class=\"jodit-wysiwyg\"></body></html>");
		doc.close();
		if (opt.iframeCSSLinks) opt.iframeCSSLinks.forEach((href) => {
			const link = doc.createElement("link");
			attr(link, "rel", "stylesheet");
			attr(link, "href", href);
			doc.head && Dom.append(doc.head, link);
		});
		if (opt.iframeStyle) {
			const style = doc.createElement("style");
			style.innerHTML = opt.iframeStyle;
			doc.head && Dom.append(doc.head, style);
		}
	}).on("createEditor", () => {
		if (!opt.iframe) return;
		const iframe = editor.c.element("iframe");
		css(iframe, "display", "block");
		iframe.src = "about:blank";
		iframe.className = "jodit-wysiwyg_iframe";
		attr(iframe, {
			allowtransparency: "true",
			tabindex: opt.tabIndex.toString(),
			frameborder: "0"
		});
		if (opt.iframeSandbox != null) attr(iframe, "sandbox", opt.iframeSandbox);
		Dom.append(editor.workplace, iframe);
		editor.iframe = iframe;
		const result = editor.e.fire("generateDocumentStructure.iframe", null, editor);
		const init = () => {
			if (!editor.iframe) return false;
			const doc = editor.iframe.contentWindow.document;
			editor.editorWindow = editor.iframe.contentWindow;
			const docMode = opt.editHTMLDocumentMode;
			const toggleEditable = () => {
				attr(doc.body, "contenteditable", editor.getMode() !== 2 && !editor.getReadOnly() || null);
			};
			const clearMarkers = (html) => {
				const bodyReg = /<body.*<\/body>/im, bodyMarker = "{%%BODY%%}", body = bodyReg.exec(html);
				if (body) html = html.replace(bodyReg, bodyMarker).replace(/<span([^>]*?)>(.*?)<\/span>/gim, "").replace(/&lt;span([^&]*?)&gt;(.*?)&lt;\/span&gt;/gim, "").replace(bodyMarker, body[0].replace(/(<body[^>]+?)min-height["'\s]*:[\s"']*[0-9]+(px|%)/im, "$1").replace(/(<body[^>]+?)([\s]*["'])?contenteditable["'\s]*=[\s"']*true["']?/im, "$1").replace(/<(style|script|span)[^>]+jodit[^>]+>.*?<\/\1>/g, "")).replace(/(class\s*=\s*)(['"])([^"']*)(jodit-wysiwyg|jodit)([^"']*\2)/g, "$1$2$3$5").replace(/(<[^<]+?)\sclass="[\s]*"/gim, "$1").replace(/(<[^<]+?)\sstyle="[\s;]*"/gim, "$1").replace(/(<[^<]+?)\sdir="[\s]*"/gim, "$1");
				return html;
			};
			if (docMode) {
				const tag = editor.element.tagName;
				if (tag !== "TEXTAREA" && tag !== "INPUT") throw error("If enable `editHTMLDocumentMode` - source element should be INPUT or TEXTAREA");
				editor.e.on("beforeGetNativeEditorValue", () => clearMarkers(editor.o.iframeDoctype + doc.documentElement.outerHTML)).on("beforeSetNativeEditorValue", ({ value }) => {
					if (editor.isLocked) return false;
					if (/<(html|body)/i.test(value)) {
						const old = doc.documentElement.outerHTML;
						if (clearMarkers(old) !== clearMarkers(value)) {
							doc.open();
							doc.write(editor.o.iframeDoctype + clearMarkers(value));
							doc.close();
							editor.editor = doc.body;
							editor.e.fire("safeHTML", editor.editor);
							toggleEditable();
							editor.e.fire("prepareWYSIWYGEditor");
							editor.e.stopPropagation("beforeSetNativeEditorValue");
						}
					} else doc.body.innerHTML = value;
					return true;
				}, { top: true });
			}
			editor.editor = doc.body;
			editor.e.on("afterSetMode afterInit afterAddPlace", toggleEditable);
			if (opt.height === "auto") {
				doc.documentElement && css(doc.documentElement, "overflowY", "hidden");
				const resizeIframe = editor.async.throttle((...args) => {
					editor.async.requestAnimationFrame(() => {
						if (editor.editor && editor.iframe && opt.height === "auto") {
							const style = editor.ew.getComputedStyle(editor.editor), marginOffset = parseInt(style.marginTop || "0", 10) + parseInt(style.marginBottom || "0", 10);
							css(editor.iframe, "height", editor.editor.offsetHeight + marginOffset);
						}
					});
				}, editor.defaultTimeout / 2);
				editor.e.on("change afterInit afterSetMode resize", resizeIframe).on([
					editor.iframe,
					editor.ew,
					doc.documentElement
				], "load", resizeIframe).on(doc, "readystatechange DOMContentLoaded", resizeIframe);
				if (typeof ResizeObserver === "function") {
					const resizeObserver = new ResizeObserver(resizeIframe);
					resizeObserver.observe(doc.body);
					editor.e.on("beforeDestruct", () => {
						resizeObserver.disconnect();
					});
				}
			}
			if (doc.documentElement) editor.e.on(doc.documentElement, "mousedown touchend", () => {
				if (!editor.s.isFocused()) {
					editor.s.focus();
					if (editor.editor === doc.body) editor.s.setCursorIn(doc.body);
				}
			}).on(editor.ew, "mousedown touchstart keydown keyup touchend click mouseup mousemove scroll", (e) => {
				var _a;
				(_a = editor.events) === null || _a === void 0 || _a.fire(editor.ow, e);
			});
			return false;
		};
		return callPromise(result, init);
	});
}
pluginSystem.add("iframe", iframe);
//#endregion
//#region node_modules/jodit/esm/plugins/image/image.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
Icon.set("image", "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"> <path d=\"M576 576q0 80-56 136t-136 56-136-56-56-136 56-136 136-56 136 56 56 136zm1024 384v448h-1408v-192l320-320 160 160 512-512zm96-704h-1600q-13 0-22.5 9.5t-9.5 22.5v1216q0 13 9.5 22.5t22.5 9.5h1600q13 0 22.5-9.5t9.5-22.5v-1216q0-13-9.5-22.5t-22.5-9.5zm160 32v1216q0 66-47 113t-113 47h-1600q-66 0-113-47t-47-113v-1216q0-66 47-113t113-47h1600q66 0 113 47t47 113z\"/> </svg> ");
Config.prototype.controls.image = {
	popup: (editor, current, close) => {
		let sourceImage = null;
		if (current && !Dom.isText(current) && Dom.isHTMLElement(current) && (Dom.isTag(current, "img") || $$("img", current).length)) sourceImage = Dom.isTag(current, "img") ? current : $$("img", current)[0];
		editor.s.save();
		return FileSelectorWidget(editor, {
			filebrowser: (data) => {
				editor.s.restore();
				data.files && data.files.forEach((file) => editor.s.insertImage(data.baseurl + file, null, editor.o.imageDefaultWidth));
				close();
			},
			upload: true,
			url: async (url, text) => {
				editor.s.restore();
				if (/^[a-z\d_-]+(\.[a-z\d_-]+)+/i.test(url)) url = "//" + url;
				const image = sourceImage || editor.createInside.element("img");
				attr(image, "src", url);
				attr(image, "alt", text);
				if (!sourceImage) await editor.s.insertImage(image, null, editor.o.imageDefaultWidth);
				close();
			}
		}, sourceImage, close);
	},
	tags: ["img"],
	tooltip: "Insert Image"
};
function image(editor) {
	editor.registerButton({
		name: "image",
		group: "media"
	});
}
pluginSystem.add("image", image);
//#endregion
//#region node_modules/jodit/esm/plugins/inline-popup/config/items/a.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var isSafeHref = (href) => /^(https?:|mailto:|tel:)/i.test(href.trim()) && !/^(javascript:|data:|vbscript:)/i.test(href.trim());
var a_default = [
	{
		name: "eye",
		tooltip: "Open link",
		exec: (editor, current) => {
			const href = attr(current, "href");
			if (current && href && isSafeHref(href)) editor.ow.open(href);
		}
	},
	{
		name: "link",
		tooltip: "Edit link",
		icon: "pencil"
	},
	"unlink",
	"brush",
	"file"
];
//#endregion
//#region node_modules/jodit/esm/plugins/inline-popup/config/items/cells.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var cmd = (control) => control.args && isString(control.args[0]) ? control.args[0].toLowerCase() : "";
var cells_default = [
	"brushCell",
	{
		name: "valign",
		list: [
			"Top",
			"Middle",
			"Bottom",
			"Normal"
		],
		childTemplate: (_, __, value) => value,
		exec: (editor, table, { control }) => {
			const command = cmd(control);
			editor.getInstance("Table", editor.o).getAllSelectedCells().forEach((cell) => {
				css(cell, "vertical-align", command === "normal" ? "" : command);
			});
		},
		tooltip: "Vertical align"
	},
	{
		name: "splitv",
		list: {
			tablesplitv: "Split vertical",
			tablesplitg: "Split horizontal"
		},
		tooltip: "Split"
	},
	{
		name: "align",
		icon: "left"
	},
	"\n",
	{
		name: "merge",
		command: "tablemerge",
		tooltip: "Merge"
	},
	{
		name: "addcolumn",
		list: {
			tableaddcolumnbefore: "Insert column before",
			tableaddcolumnafter: "Insert column after"
		},
		exec: (editor, table, { control }) => {
			if (!isJoditObject(editor)) return;
			if (!control.args) return false;
			const command = cmd(control);
			editor.execCommand(command, false, table);
		},
		tooltip: "Add column"
	},
	{
		name: "addrow",
		list: {
			tableaddrowbefore: "Insert row above",
			tableaddrowafter: "Insert row below"
		},
		exec: (editor, table, { control }) => {
			if (!isJoditObject(editor)) return;
			if (!control.args) return false;
			const command = cmd(control);
			editor.execCommand(command, false, table);
		},
		tooltip: "Add row"
	},
	{
		name: "deleteTable",
		icon: "bin",
		list: {
			tablebin: "Delete table",
			tablebinrow: "Delete row",
			tablebincolumn: "Delete column",
			tableempty: "Empty cell"
		},
		exec: (editor, table, { control }) => {
			if (!isJoditObject(editor)) return;
			if (!control.args) return false;
			const command = cmd(control);
			editor.execCommand(command, false, table);
			editor.e.fire("hidePopup");
		},
		tooltip: "Delete"
	}
];
//#endregion
//#region node_modules/jodit/esm/plugins/inline-popup/config/items/img.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var align = {
	name: "left",
	childTemplate: (_, __, value) => value,
	list: [
		"Left",
		"Right",
		"Center",
		"Normal"
	],
	exec: (editor, elm, { control }) => {
		if (!Dom.isTag(elm, /* @__PURE__ */ new Set([
			"img",
			"jodit",
			"jodit-media"
		]))) return;
		const command = control.args && isString(control.args[0]) ? control.args[0].toLowerCase() : "";
		if (!command) return false;
		hAlignElement(elm, command);
		if (Dom.isTag(elm, /* @__PURE__ */ new Set(["jodit", "jodit-media"])) && elm.firstElementChild) hAlignElement(elm.firstElementChild, command);
		editor.synchronizeValues();
		editor.e.fire("recalcPositionPopup");
	},
	tooltip: "Horizontal align"
};
var img_default = [
	{
		name: "delete",
		icon: "bin",
		tooltip: "Delete",
		exec: (editor, image) => {
			image && editor.s.removeNode(image);
		}
	},
	{
		name: "pencil",
		exec(editor, current) {
			if (current.tagName.toLowerCase() === "img") editor.e.fire("openImageProperties", current);
		},
		tooltip: "Edit"
	},
	{
		name: "valign",
		list: [
			"Top",
			"Middle",
			"Bottom",
			"Normal"
		],
		tooltip: "Vertical align",
		exec: (editor, image, { control }) => {
			if (!Dom.isTag(image, "img")) return;
			const command = control.args && isString(control.args[0]) ? control.args[0].toLowerCase() : "";
			if (!command) return false;
			css(image, "vertical-align", command === "normal" ? "" : command);
			editor.e.fire("recalcPositionPopup");
		}
	},
	align
];
//#endregion
//#region node_modules/jodit/esm/plugins/inline-popup/config/items/iframe.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var iframe_default = [{
	name: "bin",
	tooltip: "Delete",
	exec: (editor, image) => {
		image && editor.s.removeNode(image);
	}
}, align];
//#endregion
//#region node_modules/jodit/esm/plugins/inline-popup/config/items/toolbar.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module plugins/inline-popup
*/
var toolbar_default = [
	"bold",
	"italic",
	"|",
	"ul",
	"ol",
	"eraser",
	"|",
	"fontsize",
	"brush",
	"paragraph",
	"---",
	"image",
	"table",
	"\n",
	"link",
	"|",
	"align",
	"|",
	"undo",
	"redo",
	"|",
	"copyformat",
	"fullsize",
	"---",
	"dots"
];
//#endregion
//#region node_modules/jodit/esm/plugins/inline-popup/icons/addcolumn.svg.js
var addcolumn_svg_default = "<svg viewBox=\"0 0 18.151 18.151\" xmlns=\"http://www.w3.org/2000/svg\"> <g> <path stroke-width=\"0\" d=\"M6.237,16.546H3.649V1.604h5.916v5.728c0.474-0.122,0.968-0.194,1.479-0.194 c0.042,0,0.083,0.006,0.125,0.006V0H2.044v18.15h5.934C7.295,17.736,6.704,17.19,6.237,16.546z\"/> <path stroke-width=\"0\" d=\"M11.169,8.275c-2.723,0-4.938,2.215-4.938,4.938s2.215,4.938,4.938,4.938s4.938-2.215,4.938-4.938 S13.892,8.275,11.169,8.275z M11.169,16.81c-1.983,0-3.598-1.612-3.598-3.598c0-1.983,1.614-3.597,3.598-3.597 s3.597,1.613,3.597,3.597C14.766,15.198,13.153,16.81,11.169,16.81z\"/> <polygon stroke-width=\"0\" points=\"11.792,11.073 10.502,11.073 10.502,12.578 9.03,12.578 9.03,13.868 10.502,13.868 10.502,15.352 11.792,15.352 11.792,13.868 13.309,13.868 13.309,12.578 11.792,12.578 \"/> </g> </svg> ";
//#endregion
//#region node_modules/jodit/esm/plugins/inline-popup/icons/addrow.svg.js
var addrow_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 432 432\"> <g> <polygon points=\"203.688,96 0,96 0,144 155.688,144 \"/> <polygon points=\"155.719,288 0,288 0,336 203.719,336 \"/> <path d=\"M97.844,230.125c-3.701-3.703-5.856-8.906-5.856-14.141s2.154-10.438,5.856-14.141l9.844-9.844H0v48h107.719 L97.844,230.125z\"/> <polygon points=\"232,176 232,96 112,216 232,336 232,256 432,256 432,176\"/> </g> </svg> ";
//#endregion
//#region node_modules/jodit/esm/plugins/inline-popup/icons/merge.svg.js
var merge_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 312 312\"> <g transform=\"translate(0.000000,312.000000) scale(0.100000,-0.100000)\" stroke=\"none\"> <path d=\"M50 3109 c0 -7 -11 -22 -25 -35 l-25 -23 0 -961 0 -961 32 -29 32 -30 501 -2 500 -3 3 -502 2 -502 31 -30 31 -31 958 0 958 0 23 25 c13 13 30 25 37 25 9 0 12 199 12 960 0 686 -3 960 -11 960 -6 0 -24 12 -40 28 l-29 27 -503 5 -502 5 -5 502 -5 503 -28 29 c-15 16 -27 34 -27 40 0 8 -274 11 -960 11 -710 0 -960 -3 -960 -11z m1738 -698 l2 -453 -40 -40 c-22 -22 -40 -43 -40 -47 0 -4 36 -42 79 -85 88 -87 82 -87 141 -23 l26 27 455 -2 454 -3 0 -775 0 -775 -775 0 -775 0 -3 450 -2 449 47 48 47 48 -82 80 c-44 44 -84 80 -87 80 -3 0 -25 -18 -48 -40 l-41 -40 -456 2 -455 3 -3 765 c-1 421 0 771 3 778 3 10 164 12 777 10 l773 -3 3 -454z\"/> <path d=\"M607 2492 c-42 -42 -77 -82 -77 -87 0 -6 86 -96 190 -200 105 -104 190 -197 190 -205 0 -8 -41 -56 -92 -107 -65 -65 -87 -94 -77 -98 8 -3 138 -4 289 -3 l275 3 3 275 c1 151 0 281 -3 289 -4 10 -35 -14 -103 -82 -54 -53 -103 -97 -109 -97 -7 0 -99 88 -206 195 -107 107 -196 195 -198 195 -3 0 -39 -35 -82 -78z\"/> <path d=\"M1470 1639 c-47 -49 -87 -91 -89 -94 -5 -6 149 -165 160 -165 9 0 189 179 189 188 0 12 -154 162 -165 161 -6 0 -48 -41 -95 -90z\"/> <path d=\"M1797 1303 c-9 -8 -9 -568 0 -576 4 -4 50 36 103 88 54 52 101 95 106 95 5 0 95 -85 199 -190 104 -104 194 -190 200 -190 6 0 46 36 90 80 l79 79 -197 196 c-108 108 -197 199 -197 203 0 4 45 52 99 106 55 55 98 103 95 108 -6 10 -568 11 -577 1z\"/> </g> </svg> ";
//#endregion
//#region node_modules/jodit/esm/plugins/inline-popup/icons/splitg.svg.js
var splitg_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 48 48\"> <path d=\"M6 42h4v-4h-4v4zm4-28h-4v4h4v-4zm-4 20h4v-4h-4v4zm8 8h4v-4h-4v4zm-4-36h-4v4h4v-4zm8 0h-4v4h4v-4zm16 0h-4v4h4v-4zm-8 8h-4v4h4v-4zm0-8h-4v4h4v-4zm12 28h4v-4h-4v4zm-16 8h4v-4h-4v4zm-16-16h36v-4h-36v4zm32-20v4h4v-4h-4zm0 12h4v-4h-4v4zm-16 16h4v-4h-4v4zm8 8h4v-4h-4v4zm8 0h4v-4h-4v4z\"/> <path d=\"M0 0h48v48h-48z\" fill=\"none\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/plugins/inline-popup/icons/splitv.svg.js
var splitv_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 48 48\"> <path d=\"M6 18h4v-4h-4v4zm0-8h4v-4h-4v4zm8 32h4v-4h-4v4zm0-16h4v-4h-4v4zm-8 0h4v-4h-4v4zm0 16h4v-4h-4v4zm0-8h4v-4h-4v4zm8-24h4v-4h-4v4zm24 24h4v-4h-4v4zm-16 8h4v-36h-4v36zm16 0h4v-4h-4v4zm0-16h4v-4h-4v4zm0-20v4h4v-4h-4zm0 12h4v-4h-4v4zm-8-8h4v-4h-4v4zm0 32h4v-4h-4v4zm0-16h4v-4h-4v4z\"/> <path d=\"M0 0h48v48h-48z\" fill=\"none\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/plugins/inline-popup/icons/th.svg.js
var th_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"> <path d=\"M512 1248v192q0 40-28 68t-68 28h-320q-40 0-68-28t-28-68v-192q0-40 28-68t68-28h320q40 0 68 28t28 68zm0-512v192q0 40-28 68t-68 28h-320q-40 0-68-28t-28-68v-192q0-40 28-68t68-28h320q40 0 68 28t28 68zm640 512v192q0 40-28 68t-68 28h-320q-40 0-68-28t-28-68v-192q0-40 28-68t68-28h320q40 0 68 28t28 68zm-640-1024v192q0 40-28 68t-68 28h-320q-40 0-68-28t-28-68v-192q0-40 28-68t68-28h320q40 0 68 28t28 68zm640 512v192q0 40-28 68t-68 28h-320q-40 0-68-28t-28-68v-192q0-40 28-68t68-28h320q40 0 68 28t28 68zm640 512v192q0 40-28 68t-68 28h-320q-40 0-68-28t-28-68v-192q0-40 28-68t68-28h320q40 0 68 28t28 68zm-640-1024v192q0 40-28 68t-68 28h-320q-40 0-68-28t-28-68v-192q0-40 28-68t68-28h320q40 0 68 28t28 68zm640 512v192q0 40-28 68t-68 28h-320q-40 0-68-28t-28-68v-192q0-40 28-68t68-28h320q40 0 68 28t28 68zm0-512v192q0 40-28 68t-68 28h-320q-40 0-68-28t-28-68v-192q0-40 28-68t68-28h320q40 0 68 28t28 68z\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/plugins/inline-popup/icons/th-list.svg.js
var th_list_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"> <path d=\"M512 1248v192q0 40-28 68t-68 28h-320q-40 0-68-28t-28-68v-192q0-40 28-68t68-28h320q40 0 68 28t28 68zm0-512v192q0 40-28 68t-68 28h-320q-40 0-68-28t-28-68v-192q0-40 28-68t68-28h320q40 0 68 28t28 68zm1280 512v192q0 40-28 68t-68 28h-960q-40 0-68-28t-28-68v-192q0-40 28-68t68-28h960q40 0 68 28t28 68zm-1280-1024v192q0 40-28 68t-68 28h-320q-40 0-68-28t-28-68v-192q0-40 28-68t68-28h320q40 0 68 28t28 68zm1280 512v192q0 40-28 68t-68 28h-960q-40 0-68-28t-28-68v-192q0-40 28-68t68-28h960q40 0 68 28t28 68zm0-512v192q0 40-28 68t-68 28h-960q-40 0-68-28t-28-68v-192q0-40 28-68t68-28h960q40 0 68 28t28 68z\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/plugins/inline-popup/config/config.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
Config.prototype.toolbarInline = true;
Config.prototype.toolbarInlineForSelection = false;
Config.prototype.toolbarInlineDisableFor = [];
Config.prototype.toolbarInlineDisabledButtons = ["source"];
Icon.set("addcolumn", addcolumn_svg_default).set("addrow", addrow_svg_default).set("merge", merge_svg_default).set("th", th_svg_default).set("splitg", splitg_svg_default).set("splitv", splitv_svg_default).set("th-list", th_list_svg_default);
cells_default.forEach((item) => {
	if (!isString(item) && item.name && !Config.prototype.controls[item.name]) Config.prototype.controls[item.name] = item;
});
Config.prototype.popup = {
	a: a_default,
	img: img_default,
	cells: cells_default,
	toolbar: toolbar_default,
	jodit: iframe_default,
	iframe: iframe_default,
	"jodit-media": iframe_default,
	selection: [
		"bold",
		"underline",
		"italic",
		"ul",
		"ol",
		"\n",
		"outdent",
		"indent",
		"fontsize",
		"brush",
		"cut",
		"\n",
		"paragraph",
		"link",
		"align",
		"dots"
	]
};
//#endregion
//#region node_modules/jodit/esm/plugins/inline-popup/inline-popup.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$6 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
/**
* Plugin for show inline popup dialog
*/
var inlinePopup = class extends Plugin {
	constructor() {
		super(...arguments);
		this.type = null;
		/**
		* The user pressed a button inside the selection toolbar — after the
		* command fires `closeAllPopups`, the toolbar should be shown again
		* while the selection is still there. See #1238
		*/
		this.__reopenSelectionPopup = false;
		this.snapRange = null;
		this.elmsList = keys(this.j.o.popup, false).filter((s) => !this.isExcludedTarget(s));
	}
	get popup() {
		return new Popup(this.jodit, false);
	}
	get toolbar() {
		return makeCollection(this.jodit, this.popup);
	}
	onClick(node) {
		const elements = this.elmsList, target = Dom.isTag(node, "img") ? node : Dom.closest(node, elements, this.j.editor);
		if (target && this.canShowPopupForType(target.nodeName.toLowerCase())) {
			this.showPopup(() => position(target, this.j), target.nodeName.toLowerCase(), target);
			return false;
		}
	}
	/**
	* Show inline popup with some toolbar
	*
	* @param type - selection, img, a etc.
	*/
	showPopup(rect, type, target) {
		type = type.toLowerCase();
		if (!this.canShowPopupForType(type)) return false;
		if (this.type !== type || target !== this.previousTarget) {
			this.previousTarget = target;
			const data = this.j.o.popup[type];
			let content;
			if (isFunction(data)) content = data(this.j, target, this.popup.close);
			else content = data;
			if (isArray(content)) {
				const disabled = this.j.o.toolbarInlineDisabledButtons;
				this.toolbar.build(disabled.length ? content.filter((item) => {
					const name = isString(item) ? item : item.name;
					return !disabled.includes(name !== null && name !== void 0 ? name : "");
				}) : content, target);
				this.toolbar.buttonSize = this.j.o.toolbarButtonSize;
				content = this.toolbar.container;
			}
			this.popup.setContent(content);
			this.type = type;
		}
		this.popup.open(rect);
		return true;
	}
	/**
	* Hide opened popup
	*/
	hidePopup(type) {
		if (this.popup.isOpened && (!isString(type) || type === this.type)) this.popup.close();
	}
	onOutsideClick() {
		this.popup.close();
	}
	/**
	* Can show popup for this type
	*/
	canShowPopupForType(type) {
		const data = this.j.o.popup[type.toLowerCase()];
		if (this.j.o.readonly || !this.j.o.toolbarInline || !data) return false;
		return !this.isExcludedTarget(type);
	}
	/**
	* For some elements do not show popup
	*/
	isExcludedTarget(type) {
		return splitArray(this.j.o.toolbarInlineDisableFor).map((a) => a.toLowerCase()).includes(type.toLowerCase());
	}
	/** @override **/
	afterInit(jodit) {
		this.j.e.on("getDiffButtons.mobile", (toolbar) => {
			if (this.toolbar === toolbar) {
				const names = this.toolbar.getButtonsNames();
				return toArray(jodit.registeredButtons).filter((btn) => !this.j.o.toolbarInlineDisabledButtons.includes(btn.name)).filter((item) => {
					const name = isString(item) ? item : item.name;
					return name && name !== "|" && name !== "\n" && !names.includes(name);
				});
			}
		}).on("hidePopup", this.hidePopup).on("showInlineToolbar", this.showInlineToolbar).on("showPopup", (elm, rect, type) => {
			this.showPopup(rect, type || (isString(elm) ? elm : elm.nodeName), isString(elm) ? void 0 : elm);
		}).on("mousedown keydown", this.onSelectionStart).on("change", () => {
			if (this.popup.isOpened && this.previousTarget && !this.previousTarget.parentNode) {
				this.hidePopup();
				this.previousTarget = void 0;
			}
		}).on([this.j.ew, this.j.ow], "mouseup keyup", this.onSelectionEnd).on([this.j.ew, this.j.ow], "mousedown touchstart", this.__onDocumentMouseDown).on("closeAllPopups", this.__onCloseAllPopups);
		this.addListenersForElements();
	}
	__onDocumentMouseDown(e) {
		if (this.popup.isOpened && this.type === "selection" && e.target && UIElement.closestElement(e.target, Popup)) this.__reopenSelectionPopup = true;
	}
	__onCloseAllPopups() {
		if (!this.__reopenSelectionPopup) return;
		this.__reopenSelectionPopup = false;
		if (!this.j.o.toolbarInlineForSelection) return;
		this.j.async.setTimeout(() => {
			const sel = this.j.s.sel;
			if (sel && !sel.isCollapsed) this.showPopup(() => this.__selectionBound(), "selection");
		}, 1);
	}
	/**
	* The selection rect comes from the editor document — in iframe mode its
	* coordinates are iframe-local, while the popup lives in the host
	* document, so the iframe offset must be added. See
	* https://github.com/xdan/jodit/issues/1058
	*/
	__selectionBound() {
		const rect = this.j.s.range.getBoundingClientRect();
		let { left, top } = rect;
		if (this.j.iframe) {
			const offset = position(this.j.iframe, this.j, true);
			left += offset.left;
			top += offset.top;
		}
		return {
			left,
			top,
			width: rect.width,
			height: rect.height
		};
	}
	onSelectionStart() {
		this.snapRange = this.j.s.range.cloneRange();
	}
	onSelectionEnd(e) {
		if (e && e.target && UIElement.closestElement(e.target, Popup)) return;
		const { snapRange } = this, { range } = this.j.s;
		if (!snapRange || range.collapsed || range.startContainer !== snapRange.startContainer || range.startOffset !== snapRange.startOffset || range.endContainer !== snapRange.endContainer || range.endOffset !== snapRange.endOffset) this.onSelectionChange();
	}
	/**
	* Selection change handler
	*/
	onSelectionChange() {
		if (!this.j.o.toolbarInlineForSelection) return;
		const type = "selection";
		const sel = this.j.s.sel;
		const range = this.j.s.range;
		if ((sel === null || sel === void 0 ? void 0 : sel.isCollapsed) || this.isSelectedTarget(range)) {
			if (this.type === type && this.popup.isOpened) this.hidePopup();
			return;
		}
		if (!this.j.s.current()) return;
		this.showPopup(() => this.__selectionBound(), type);
	}
	/**
	* In not collapsed selection - only one image
	*/
	isSelectedTarget(r) {
		const sc = r.startContainer;
		return Dom.isElement(sc) && sc === r.endContainer && Dom.isTag(sc.childNodes[r.startOffset], new Set(keys(this.j.o.popup, false))) && r.startOffset === r.endOffset - 1;
	}
	/**
	* Shortcut for Table module
	*/
	/** @override **/
	beforeDestruct(jodit) {
		jodit.e.off("showPopup").off([this.j.ew, this.j.ow], "mouseup keyup", this.onSelectionEnd).off([this.j.ew, this.j.ow], "mousedown touchstart", this.__onDocumentMouseDown).off("closeAllPopups", this.__onCloseAllPopups);
		this.removeListenersForElements();
	}
	_eventsList() {
		const el = this.elmsList;
		return el.map((e) => camelCase(`click_${e}`)).concat(el.map((e) => camelCase(`touchstart_${e}`))).join(" ");
	}
	addListenersForElements() {
		this.j.e.on(this._eventsList(), this.onClick);
	}
	removeListenersForElements() {
		this.j.e.off(this._eventsList(), this.onClick);
	}
	/**
	* Show the inline WYSIWYG toolbar editor.
	*/
	showInlineToolbar(bound) {
		this.showPopup(() => {
			if (bound) return bound;
			const { range } = this.j.s;
			return range.getBoundingClientRect();
		}, "toolbar");
	}
};
inlinePopup.requires = ["select"];
__decorate$6([cache], inlinePopup.prototype, "popup", null);
__decorate$6([cache], inlinePopup.prototype, "toolbar", null);
__decorate$6([autobind], inlinePopup.prototype, "onClick", null);
__decorate$6([wait((ctx) => !ctx.j.isLocked)], inlinePopup.prototype, "showPopup", null);
__decorate$6([watch([
	":clickEditor",
	":beforeCommandDelete",
	":backSpaceAfterDelete"
]), autobind], inlinePopup.prototype, "hidePopup", null);
__decorate$6([watch(":outsideClick")], inlinePopup.prototype, "onOutsideClick", null);
__decorate$6([autobind], inlinePopup.prototype, "__onDocumentMouseDown", null);
__decorate$6([autobind], inlinePopup.prototype, "__onCloseAllPopups", null);
__decorate$6([autobind], inlinePopup.prototype, "onSelectionStart", null);
__decorate$6([autobind], inlinePopup.prototype, "onSelectionEnd", null);
__decorate$6([debounce((ctx) => ctx.defaultTimeout)], inlinePopup.prototype, "onSelectionChange", null);
__decorate$6([autobind], inlinePopup.prototype, "showInlineToolbar", null);
pluginSystem.add("inlinePopup", inlinePopup);
//#endregion
//#region node_modules/jodit/esm/plugins/link/icons/link.svg.js
var link_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"> <path d=\"M1520 1216q0-40-28-68l-208-208q-28-28-68-28-42 0-72 32 3 3 19 18.5t21.5 21.5 15 19 13 25.5 3.5 27.5q0 40-28 68t-68 28q-15 0-27.5-3.5t-25.5-13-19-15-21.5-21.5-18.5-19q-33 31-33 73 0 40 28 68l206 207q27 27 68 27 40 0 68-26l147-146q28-28 28-67zm-703-705q0-40-28-68l-206-207q-28-28-68-28-39 0-68 27l-147 146q-28 28-28 67 0 40 28 68l208 208q27 27 68 27 42 0 72-31-3-3-19-18.5t-21.5-21.5-15-19-13-25.5-3.5-27.5q0-40 28-68t68-28q15 0 27.5 3.5t25.5 13 19 15 21.5 21.5 18.5 19q33-31 33-73zm895 705q0 120-85 203l-147 146q-83 83-203 83-121 0-204-85l-206-207q-83-83-83-203 0-123 88-209l-88-88q-86 88-208 88-120 0-204-84l-208-208q-84-84-84-204t85-203l147-146q83-83 203-83 121 0 204 85l206 207q83 83 83 203 0 123-88 209l88 88q86-88 208-88 120 0 204 84l208 208q84 84 84 204z\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/plugins/link/icons/unlink.svg.js
var unlink_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"> <path d=\"M503 1271l-256 256q-10 9-23 9-12 0-23-9-9-10-9-23t9-23l256-256q10-9 23-9t23 9q9 10 9 23t-9 23zm169 41v320q0 14-9 23t-23 9-23-9-9-23v-320q0-14 9-23t23-9 23 9 9 23zm-224-224q0 14-9 23t-23 9h-320q-14 0-23-9t-9-23 9-23 23-9h320q14 0 23 9t9 23zm1264 128q0 120-85 203l-147 146q-83 83-203 83-121 0-204-85l-334-335q-21-21-42-56l239-18 273 274q27 27 68 27.5t68-26.5l147-146q28-28 28-67 0-40-28-68l-274-275 18-239q35 21 56 42l336 336q84 86 84 204zm-617-724l-239 18-273-274q-28-28-68-28-39 0-68 27l-147 146q-28 28-28 67 0 40 28 68l274 274-18 240q-35-21-56-42l-336-336q-84-86-84-204 0-120 85-203l147-146q83-83 203-83 121 0 204 85l334 335q21 21 42 56zm633 84q0 14-9 23t-23 9h-320q-14 0-23-9t-9-23 9-23 23-9h320q14 0 23 9t9 23zm-544-544v320q0 14-9 23t-23 9-23-9-9-23v-320q0-14 9-23t23-9 23 9 9 23zm407 151l-256 256q-11 9-23 9t-23-9q-9-10-9-23t9-23l256-256q10-9 23-9t23 9q9 10 9 23t-9 23z\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/plugins/link/template.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var formTemplate = (editor) => {
	const { openInNewTabCheckbox, noFollowCheckbox, ariaLabelInput, modeClassName, selectSizeClassName, selectMultipleClassName, selectOptionsClassName } = editor.o.link;
	return new UIForm(editor, [
		new UIBlock(editor, [new UIInput(editor, {
			name: "url",
			type: "text",
			ref: "url_input",
			label: "URL",
			placeholder: "http://",
			required: true
		})]),
		ariaLabelInput ? new UIBlock(editor, [new UIInput(editor, {
			name: "ariaLabel",
			ref: "aria_label_input",
			label: "Aria label"
		})]) : null,
		new UIBlock(editor, [new UIInput(editor, {
			name: "content",
			ref: "content_input",
			label: "Text"
		})], { ref: "content_input_box" }),
		modeClassName ? new UIBlock(editor, [(() => {
			if (modeClassName === "input") return new UIInput(editor, {
				name: "className",
				ref: "className_input",
				label: "Class name"
			});
			if (modeClassName === "select") return new UISelect(editor, {
				name: "className",
				ref: "className_select",
				label: "Class name",
				size: selectSizeClassName,
				multiple: selectMultipleClassName,
				options: selectOptionsClassName
			});
			return null;
		})()]) : null,
		openInNewTabCheckbox ? new UICheckbox(editor, {
			name: "target",
			ref: "target_checkbox",
			label: "Open in new tab"
		}) : null,
		noFollowCheckbox ? new UICheckbox(editor, {
			name: "nofollow",
			ref: "nofollow_checkbox",
			label: "No follow"
		}) : null,
		new UIBlock(editor, [new UIButton(editor, {
			name: "unlink",
			variant: "default",
			text: "Unlink"
		}), new UIButton(editor, {
			name: "insert",
			type: "submit",
			variant: "primary",
			text: "Insert"
		})], { align: "full" })
	]);
};
//#endregion
//#region node_modules/jodit/esm/plugins/link/config.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
Config.prototype.link = {
	formTemplate,
	followOnDblClick: false,
	processVideoLink: true,
	processPastedLink: true,
	deriveUrlFromText: false,
	noFollowCheckbox: true,
	openInNewTabCheckbox: true,
	openInNewTabCheckboxDefaultChecked: false,
	ariaLabelInput: false,
	modeClassName: "input",
	selectMultipleClassName: true,
	preventReadOnlyNavigation: true,
	selectSizeClassName: 3,
	selectOptionsClassName: [],
	hotkeys: ["ctrl+k", "cmd+k"]
};
Icon.set("link", link_svg_default).set("unlink", unlink_svg_default);
Config.prototype.controls.unlink = {
	exec: (editor, current) => {
		const anchor = Dom.closest(current, "a", editor.editor);
		if (anchor) Dom.unwrap(anchor);
		editor.synchronizeValues();
		editor.e.fire("hidePopup");
	},
	tooltip: "Unlink"
};
Config.prototype.controls.link = {
	isActive: (editor) => {
		const current = editor.s.current();
		return Boolean(current && Dom.closest(current, "a", editor.editor));
	},
	popup: (editor, current, close) => {
		return editor.e.fire("generateLinkForm.link", current, close);
	},
	tags: ["a"],
	tooltip: "Insert link"
};
//#endregion
//#region node_modules/jodit/esm/plugins/link/link.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$5 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
/**
* Process link. Insert, dblclick or remove format
*/
var link = class extends Plugin {
	constructor() {
		super(...arguments);
		/** @override */
		this.buttons = [{
			name: "link",
			group: "insert"
		}];
	}
	/** @override */
	afterInit(jodit) {
		if (jodit.o.link.followOnDblClick) jodit.e.on("dblclick.link", this.__onDblClickOnLink);
		jodit.e.on(jodit.editor, "click.link", this.__onClickReadOnlyLink);
		if (jodit.o.link.processPastedLink) jodit.e.on("processPaste.link", this.onProcessPasteLink);
		jodit.e.on("generateLinkForm.link", this.__generateForm);
		jodit.registerCommand("openLinkDialog", {
			exec: () => {
				const dialog = jodit.dlg({ resizable: false });
				const htmlForm = this.__generateForm(jodit.s.current(), () => {
					dialog.close();
				});
				htmlForm.container.classList.add("jodit-dialog_alert");
				dialog.setContent(htmlForm);
				dialog.open();
				jodit.async.requestIdleCallback(() => {
					const { url_input } = refs(htmlForm.container);
					url_input === null || url_input === void 0 || url_input.focus();
				});
			},
			hotkeys: jodit.o.link.hotkeys
		});
	}
	__onDblClickOnLink(e) {
		if (!Dom.isTag(e.target, "a")) return;
		const href = attr(e.target, "href");
		if (href) {
			location.href = href;
			e.preventDefault();
		}
	}
	onProcessPasteLink(ignore, html) {
		var _a, _b, _c, _d;
		const { jodit } = this;
		if (!isURL(html) || !jodit.o.link.processPastedLink) return;
		jodit.e.stopPropagation("processPaste");
		if (jodit.o.link.processVideoLink) {
			const embed = call((_b = (_a = jodit.o.video) === null || _a === void 0 ? void 0 : _a.parseUrlToVideoEmbed) !== null && _b !== void 0 ? _b : convertMediaUrlToVideoEmbed, html, {
				width: (_c = jodit.o.video) === null || _c === void 0 ? void 0 : _c.defaultWidth,
				height: (_d = jodit.o.video) === null || _d === void 0 ? void 0 : _d.defaultHeight
			});
			if (embed !== html) return jodit.createInside.fromHTML(embed);
		}
		if (jodit.s.isCollapsed()) {
			const a = jodit.createInside.element("a");
			attr(a, "href", html);
			a.textContent = html;
			jodit.e.fire("applyLink", jodit, a, null);
			return a;
		}
		jodit.s.commitStyle({
			element: "a",
			attributes: { href: html }
		});
		return true;
	}
	__generateForm(current, close) {
		const { jodit } = this;
		const i18n = jodit.i18n.bind(jodit);
		const { openInNewTabCheckbox, openInNewTabCheckboxDefaultChecked, noFollowCheckbox, formTemplate, formClassName, modeClassName } = jodit.o.link;
		const html = formTemplate(jodit);
		const form = isString(html) ? jodit.c.fromHTML(html, {
			target_checkbox_box: openInNewTabCheckbox,
			nofollow_checkbox_box: noFollowCheckbox
		}) : html;
		const htmlForm = Dom.isElement(form) ? form : form.container;
		const elements = refs(htmlForm);
		const { insert, unlink, content_input_box } = elements;
		const { target_checkbox, nofollow_checkbox, url_input } = elements;
		const currentElement = current;
		const isImageContent = Dom.isImage(currentElement);
		let { content_input } = elements;
		const { className_input } = elements, { className_select } = elements, { aria_label_input } = elements;
		if (!content_input) content_input = jodit.c.element("input", {
			type: "hidden",
			ref: "content_input"
		});
		if (formClassName) htmlForm.classList.add(formClassName);
		if (isImageContent) Dom.hide(content_input_box);
		let link;
		const getSelectionText = () => link ? link.innerText : stripTags(jodit.s.range.cloneContents(), jodit.ed);
		if (current && Dom.closest(current, "a", jodit.editor)) link = Dom.closest(current, "a", jodit.editor);
		else link = false;
		if (!isImageContent && current) content_input.value = getSelectionText();
		if (aria_label_input) aria_label_input.value = link ? attr(link, "aria-label") || "" : "";
		if (link) {
			url_input.value = attr(link, "href") || "";
			if (modeClassName) readClassnames(modeClassName, className_input, link, className_select);
			if (openInNewTabCheckbox && target_checkbox) target_checkbox.checked = attr(link, "target") === "_blank";
			if (noFollowCheckbox && nofollow_checkbox) nofollow_checkbox.checked = (attr(link, "rel") || "").split(/\s+/).includes("nofollow");
			insert.textContent = i18n("Update");
		} else {
			Dom.hide(unlink);
			if (jodit.o.link.deriveUrlFromText && url_input && !isImageContent && !url_input.value.trim()) url_input.value = guessUrlFromText(content_input.value);
			if (openInNewTabCheckbox && target_checkbox) target_checkbox.checked = openInNewTabCheckboxDefaultChecked;
		}
		jodit.editor.normalize();
		const snapshot = jodit.history.snapshot.make();
		if (unlink) jodit.e.on(unlink, "click", (e) => {
			jodit.s.restore();
			jodit.history.snapshot.restore(snapshot);
			if (link) Dom.unwrap(link);
			jodit.synchronizeValues();
			close();
			e.preventDefault();
		});
		const onSubmit = () => {
			if (!url_input.value.trim().length) {
				url_input.focus();
				url_input.classList.add("jodit_error");
				return false;
			}
			let links;
			jodit.s.restore();
			jodit.s.removeMarkers();
			jodit.editor.normalize();
			jodit.history.snapshot.restore(snapshot);
			const textWasChanged = getSelectionText() !== content_input.value.trim();
			const ci = jodit.createInside;
			if (!link || !Dom.isOrContains(jodit.editor, link)) {
				if (!jodit.s.isCollapsed()) {
					const node = jodit.s.current();
					if (Dom.isTag(node, "img")) links = [Dom.wrap(node, "a", ci)];
					else links = jodit.s.wrapInTag("a");
				} else {
					const a = ci.element("a");
					jodit.s.insertNode(a, false, false);
					links = [a];
				}
				links.forEach((link) => jodit.s.select(link));
			} else links = [link];
			links.forEach((a) => {
				attr(a, "href", url_input.value);
				writeClasses(modeClassName, className_input, className_select, a);
				if (!isImageContent) writeImage(a, content_input, textWasChanged, url_input);
				if (openInNewTabCheckbox && target_checkbox) attr(a, "target", target_checkbox.checked ? "_blank" : null);
				if (noFollowCheckbox && nofollow_checkbox) {
					const relParts = (attr(a, "rel") || "").split(/\s+/).filter(Boolean);
					const hasNofollow = relParts.includes("nofollow");
					if (nofollow_checkbox.checked && !hasNofollow) relParts.push("nofollow");
					else if (!nofollow_checkbox.checked && hasNofollow) relParts.splice(relParts.indexOf("nofollow"), 1);
					attr(a, "rel", relParts.length ? relParts.join(" ") : null);
				}
				if (aria_label_input) attr(a, "aria-label", aria_label_input.value.trim() || null);
				jodit.e.fire("applyLink", jodit, a, form);
			});
			jodit.synchronizeValues();
			close();
			return false;
		};
		if (Dom.isElement(form)) jodit.e.on(form, "submit", (event) => {
			event.preventDefault();
			event.stopImmediatePropagation();
			onSubmit();
			return false;
		});
		else form.onSubmit(onSubmit);
		return form;
	}
	/** @override */
	beforeDestruct(jodit) {
		jodit.e.off("generateLinkForm.link", this.__generateForm).off("dblclick.link", this.__onDblClickOnLink).off(jodit.editor, "click.link", this.__onClickReadOnlyLink).off("processPaste.link", this.onProcessPasteLink);
	}
	__onClickReadOnlyLink(e) {
		const { jodit } = this;
		if (jodit.o.readonly && jodit.o.link.preventReadOnlyNavigation && Dom.isTag(e.target, "a")) e.preventDefault();
	}
};
__decorate$5([autobind], link.prototype, "__onDblClickOnLink", null);
__decorate$5([autobind], link.prototype, "onProcessPasteLink", null);
__decorate$5([autobind], link.prototype, "__generateForm", null);
__decorate$5([autobind], link.prototype, "__onClickReadOnlyLink", null);
pluginSystem.add("link", link);
/**
* Guess a usable `href` from the selected text for the link dialog.
* Returns an empty string when the text is not a plausible URL/email,
* so plain text (e.g. "click here") is left untouched.
*/
function guessUrlFromText(text) {
	const value = text.trim();
	if (!value || /\s/.test(value)) return "";
	if (/^(https?:|mailto:|tel:|ftp:|#|\/|\.{1,2}\/)/i.test(value)) return value;
	if (/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)) return `mailto:${value}`;
	if (/^[a-z0-9-]+(\.[a-z0-9-]+)+(\/\S*)?$/i.test(value)) return `https://${value}`;
	return "";
}
function writeClasses(modeClassName, className_input, className_select, a) {
	var _a;
	if (modeClassName && (className_input !== null && className_input !== void 0 ? className_input : className_select)) {
		if (modeClassName === "input") {
			if (className_input.value === "" && a.hasAttribute("class")) attr(a, "class", null);
			if (className_input.value !== "") attr(a, "class", className_input.value);
		} else if (modeClassName === "select") {
			if (a.hasAttribute("class")) attr(a, "class", null);
			for (let i = 0; i < className_select.selectedOptions.length; i++) {
				const className = (_a = className_select.selectedOptions.item(i)) === null || _a === void 0 ? void 0 : _a.value;
				if (className) className.split(/\s+/).filter((cn) => cn.trim().length > 0).forEach((cn) => {
					a.classList.add(cn);
				});
			}
		}
	}
}
function readClassnames(modeClassName, className_input, link, className_select) {
	switch (modeClassName) {
		case "input":
			if (className_input) className_input.value = attr(link, "class") || "";
			break;
		case "select":
			if (className_select) {
				for (let i = 0; i < className_select.selectedOptions.length; i++) {
					const option = className_select.options.item(i);
					if (option) option.selected = false;
				}
				(attr(link, "class") || "").split(/\s+/).filter((cn) => cn.trim().length > 0).forEach((className) => {
					if (className) for (let i = 0; i < className_select.options.length; i++) {
						const option = className_select.options.item(i);
						if ((option === null || option === void 0 ? void 0 : option.value) && option.value.split(/\s+/).map((cn) => cn.trim()).includes(className)) option.selected = true;
					}
				});
			}
			break;
	}
}
function writeImage(a, content_input, textWasChanged, url_input) {
	let newContent = a.textContent;
	if (content_input.value.trim().length) {
		if (textWasChanged) newContent = content_input.value;
	} else newContent = url_input.value;
	const content = a.textContent;
	if (newContent !== content) a.textContent = newContent;
}
//#endregion
//#region node_modules/jodit/esm/plugins/ordered-list/icons/ol.svg.js
var ol_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"> <path stroke-width=\"0\" d=\"M381 1620q0 80-54.5 126t-135.5 46q-106 0-172-66l57-88q49 45 106 45 29 0 50.5-14.5t21.5-42.5q0-64-105-56l-26-56q8-10 32.5-43.5t42.5-54 37-38.5v-1q-16 0-48.5 1t-48.5 1v53h-106v-152h333v88l-95 115q51 12 81 49t30 88zm2-627v159h-362q-6-36-6-54 0-51 23.5-93t56.5-68 66-47.5 56.5-43.5 23.5-45q0-25-14.5-38.5t-39.5-13.5q-46 0-81 58l-85-59q24-51 71.5-79.5t105.5-28.5q73 0 123 41.5t50 112.5q0 50-34 91.5t-75 64.5-75.5 50.5-35.5 52.5h127v-60h105zm1409 319v192q0 13-9.5 22.5t-22.5 9.5h-1216q-13 0-22.5-9.5t-9.5-22.5v-192q0-14 9-23t23-9h1216q13 0 22.5 9.5t9.5 22.5zm-1408-899v99h-335v-99h107q0-41 .5-122t.5-121v-12h-2q-8 17-50 54l-71-76 136-127h106v404h108zm1408 387v192q0 13-9.5 22.5t-22.5 9.5h-1216q-13 0-22.5-9.5t-9.5-22.5v-192q0-14 9-23t23-9h1216q13 0 22.5 9.5t9.5 22.5zm0-512v192q0 13-9.5 22.5t-22.5 9.5h-1216q-13 0-22.5-9.5t-9.5-22.5v-192q0-13 9.5-22.5t22.5-9.5h1216q13 0 22.5 9.5t9.5 22.5z\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/plugins/ordered-list/icons/ul.svg.js
var ul_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"> <path stroke-width=\"0\" d=\"M384 1408q0 80-56 136t-136 56-136-56-56-136 56-136 136-56 136 56 56 136zm0-512q0 80-56 136t-136 56-136-56-56-136 56-136 136-56 136 56 56 136zm1408 416v192q0 13-9.5 22.5t-22.5 9.5h-1216q-13 0-22.5-9.5t-9.5-22.5v-192q0-13 9.5-22.5t22.5-9.5h1216q13 0 22.5 9.5t9.5 22.5zm-1408-928q0 80-56 136t-136 56-136-56-56-136 56-136 136-56 136 56 56 136zm1408 416v192q0 13-9.5 22.5t-22.5 9.5h-1216q-13 0-22.5-9.5t-9.5-22.5v-192q0-13 9.5-22.5t22.5-9.5h1216q13 0 22.5 9.5t9.5 22.5zm0-512v192q0 13-9.5 22.5t-22.5 9.5h-1216q-13 0-22.5-9.5t-9.5-22.5v-192q0-13 9.5-22.5t22.5-9.5h1216q13 0 22.5 9.5t9.5 22.5z\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/plugins/ordered-list/config.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var memoExec = (jodit, _, { control }) => {
	var _a;
	const key = `button${control.command}`;
	const value = (_a = control.args && control.args[0]) !== null && _a !== void 0 ? _a : dataBind(jodit, key);
	dataBind(jodit, key, value);
	jodit.execCommand(control.command, false, value === "default" ? null : value);
};
Icon.set("ol", ol_svg_default).set("ul", ul_svg_default);
Config.prototype.controls.ul = {
	command: "insertUnorderedList",
	tags: ["ul"],
	tooltip: "Insert Unordered List",
	list: {
		default: "Default",
		circle: "Circle",
		disc: "Dot",
		square: "Quadrate"
	},
	exec: memoExec
};
Config.prototype.controls.ol = {
	command: "insertOrderedList",
	tags: ["ol"],
	tooltip: "Insert Ordered List",
	list: {
		default: "Default",
		"lower-alpha": "Lower Alpha",
		"lower-greek": "Lower Greek",
		"lower-roman": "Lower Roman",
		"upper-alpha": "Upper Alpha",
		"upper-roman": "Upper Roman"
	},
	exec: memoExec
};
//#endregion
//#region node_modules/jodit/esm/plugins/ordered-list/ordered-list.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$4 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
/**
* Process commands insertOrderedList and insertUnOrderedList
*/
var orderedList = class extends Plugin {
	constructor() {
		super(...arguments);
		this.buttons = [{
			name: "ul",
			group: "list"
		}, {
			name: "ol",
			group: "list"
		}];
	}
	afterInit(jodit) {
		jodit.registerCommand("insertUnorderedList", this.onCommand).registerCommand("insertOrderedList", this.onCommand);
	}
	onCommand(command, _, type) {
		this.jodit.s.commitStyle({
			element: command === "insertunorderedlist" ? "ul" : "ol",
			attributes: { style: { listStyleType: type !== null && type !== void 0 ? type : null } }
		});
		this.jodit.synchronizeValues();
		return false;
	}
	beforeDestruct(jodit) {}
};
__decorate$4([autobind], orderedList.prototype, "onCommand", null);
pluginSystem.add("orderedList", orderedList);
//#endregion
//#region node_modules/jodit/esm/plugins/placeholder/config.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module plugins/placeholder
*/
Config.prototype.showPlaceholder = true;
Config.prototype.placeholder = "Type something";
Config.prototype.useInputsPlaceholder = true;
//#endregion
//#region node_modules/jodit/esm/plugins/placeholder/placeholder.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$3 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
/**
* Check if root node is empty
* @private
*/
function isEditorEmpty(root) {
	var _a;
	if (!root.firstChild) return true;
	const first = root.firstChild;
	if (INSEPARABLE_TAGS.has((_a = first.nodeName) === null || _a === void 0 ? void 0 : _a.toLowerCase()) || /^(TABLE)$/i.test(first.nodeName)) return false;
	const next = Dom.next(first, (node) => node && !Dom.isEmptyTextNode(node), root);
	if (Dom.isText(first) && !next) return Dom.isEmptyTextNode(first);
	return !next && Dom.each(first, (elm) => !(Dom.isLeaf(elm) || Dom.isList(elm)) && (Dom.isEmpty(elm) || Dom.isTag(elm, "br")));
}
/**
* Show placeholder inside empty editor
*/
var placeholder = class extends Plugin {
	constructor() {
		super(...arguments);
		this.addNativeListeners = () => {
			this.j.e.off(this.j.editor, "input.placeholder keydown.placeholder").on(this.j.editor, "input.placeholder keydown.placeholder", this.toggle);
		};
		this.addEvents = () => {
			const editor = this.j;
			if (editor.o.useInputsPlaceholder && editor.element.hasAttribute("placeholder")) this.placeholderElm.innerHTML = attr(editor.element, "placeholder") || "";
			editor.e.fire("placeholder", this.placeholderElm.innerHTML);
			editor.e.off(".placeholder").on("changePlace.placeholder", this.addNativeListeners).on("change.placeholder focus.placeholder keyup.placeholder mouseup.placeholder keydown.placeholder mousedown.placeholder afterSetMode.placeholder changePlace.placeholder", this.toggle).on(window, "load", this.toggle);
			this.addNativeListeners();
			this.toggle();
		};
	}
	afterInit(editor) {
		if (!editor.o.showPlaceholder) return;
		this.placeholderElm = editor.c.fromHTML(`<span data-ref="placeholder" style="display: none;" class="jodit-placeholder">${editor.i18n(editor.o.placeholder)}</span>`);
		if (editor.o.direction === "rtl") css(this.placeholderElm, {
			right: 0,
			direction: "rtl"
		});
		editor.e.on("readonly", (isReadOnly) => {
			if (isReadOnly) this.hide();
			else this.toggle();
		}).on("changePlace", this.addEvents);
		this.addEvents();
	}
	show() {
		const editor = this.j;
		if (editor.o.readonly) return;
		let marginTop = 0, marginLeft = 0;
		const current = editor.s.current(), wrapper = current && Dom.closest(current, Dom.isBlock, editor.editor) || editor.editor;
		const style = editor.ew.getComputedStyle(wrapper);
		const styleEditor = editor.ew.getComputedStyle(editor.editor);
		Dom.append(editor.workplace, this.placeholderElm);
		const { firstChild } = editor.editor;
		if (Dom.isElement(firstChild) && !isMarker(firstChild)) {
			const style2 = editor.ew.getComputedStyle(firstChild);
			marginTop = parseInt(style2.getPropertyValue("margin-top"), 10);
			marginLeft = parseInt(style2.getPropertyValue("margin-left"), 10);
			css(this.placeholderElm, {
				fontSize: parseInt(style2.getPropertyValue("font-size"), 10),
				lineHeight: style2.getPropertyValue("line-height")
			});
		} else css(this.placeholderElm, {
			fontSize: parseInt(style.getPropertyValue("font-size"), 10),
			lineHeight: style.getPropertyValue("line-height")
		});
		css(this.placeholderElm, {
			display: "block",
			textAlign: style.getPropertyValue("text-align"),
			paddingTop: parseInt(styleEditor.paddingTop, 10) + "px",
			paddingLeft: parseInt(styleEditor.paddingLeft, 10) + "px",
			paddingRight: parseInt(styleEditor.paddingRight, 10) + "px",
			marginTop: Math.max(parseInt(style.getPropertyValue("margin-top"), 10), marginTop),
			marginLeft: Math.max(parseInt(style.getPropertyValue("margin-left"), 10), marginLeft)
		});
	}
	hide() {
		Dom.safeRemove(this.placeholderElm);
	}
	toggle() {
		const editor = this.j;
		if (!editor.editor || editor.isInDestruct) return;
		if (editor.getRealMode() !== 1) {
			this.hide();
			return;
		}
		if (!isEditorEmpty(editor.editor)) this.hide();
		else this.show();
	}
	beforeDestruct(jodit) {
		this.hide();
		jodit.e.off(".placeholder").off(window, "load", this.toggle);
	}
};
__decorate$3([debounce((ctx) => ctx.defaultTimeout / 10, true)], placeholder.prototype, "toggle", null);
pluginSystem.add("placeholder", placeholder);
//#endregion
//#region node_modules/jodit/esm/plugins/powered-by-jodit/powered-by-jodit.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
function poweredByJodit(jodit) {
	const { o } = jodit;
	if (!o.hidePoweredByJodit && !o.inline && (o.showCharsCounter || o.showWordsCounter || o.showXPathInStatusbar)) jodit.hookStatus("ready", () => {
		jodit.statusbar.append(jodit.create.fromHTML(`<a
						tabindex="-1"
						style="text-transform: uppercase"
						class="jodit-status-bar-link"
						target="_blank"
						href="https://xdsoft.net/jodit/">
							Powered by Jodit
						</a>`), true);
	});
}
pluginSystem.add("poweredByJodit", poweredByJodit);
//#endregion
//#region node_modules/jodit/esm/plugins/redo-undo/redo-undo.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
Icon.set("redo", "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"> <path d=\"M1664 256v448q0 26-19 45t-45 19h-448q-42 0-59-40-17-39 14-69l138-138q-148-137-349-137-104 0-198.5 40.5t-163.5 109.5-109.5 163.5-40.5 198.5 40.5 198.5 109.5 163.5 163.5 109.5 198.5 40.5q119 0 225-52t179-147q7-10 23-12 14 0 25 9l137 138q9 8 9.5 20.5t-7.5 22.5q-109 132-264 204.5t-327 72.5q-156 0-298-61t-245-164-164-245-61-298 61-298 164-245 245-164 298-61q147 0 284.5 55.5t244.5 156.5l130-129q29-31 70-14 39 17 39 59z\"/> </svg> ").set("undo", "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"> <path d=\"M1664 896q0 156-61 298t-164 245-245 164-298 61q-172 0-327-72.5t-264-204.5q-7-10-6.5-22.5t8.5-20.5l137-138q10-9 25-9 16 2 23 12 73 95 179 147t225 52q104 0 198.5-40.5t163.5-109.5 109.5-163.5 40.5-198.5-40.5-198.5-109.5-163.5-163.5-109.5-198.5-40.5q-98 0-188 35.5t-160 101.5l137 138q31 30 14 69-17 40-59 40h-448q-26 0-45-19t-19-45v-448q0-42 40-59 39-17 69 14l130 129q107-101 244.5-156.5t284.5-55.5q156 0 298 61t245 164 164 245 61 298z\"/> </svg> ");
Config.prototype.controls.redo = {
	mode: 3,
	isDisabled: (editor) => !editor.history.canRedo(),
	tooltip: "Redo"
};
Config.prototype.controls.undo = {
	mode: 3,
	isDisabled: (editor) => !editor.history.canUndo(),
	tooltip: "Undo"
};
/**
* Custom process Redo and Undo functionality
*/
var redoUndo = class extends Plugin {
	constructor() {
		super(...arguments);
		/** @override */
		this.buttons = [{
			name: "undo",
			group: "history"
		}, {
			name: "redo",
			group: "history"
		}];
	}
	beforeDestruct() {}
	afterInit(editor) {
		const callback = (command) => {
			editor.history[command]();
			return false;
		};
		editor.registerCommand("redo", {
			exec: callback,
			hotkeys: [
				"ctrl+y",
				"ctrl+shift+z",
				"cmd+y",
				"cmd+shift+z"
			]
		});
		editor.registerCommand("undo", {
			exec: callback,
			hotkeys: ["ctrl+z", "cmd+z"]
		});
	}
};
pluginSystem.add("redoUndo", redoUndo);
//#endregion
//#region node_modules/jodit/esm/plugins/size/config.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module plugins/size
*/
Config.prototype.minWidth = 200;
Config.prototype.maxWidth = "100%";
/**
* Editor's min-height
*
* ```javascript
* Jodit.make('.editor', {
*    minHeight: '30%' //min-height: 30%
* })
* ```
*
* ```javascript
* Jodit.make('.editor', {
*    minHeight: 200 //min-height: 200px
* })
* ```
*/
Config.prototype.minHeight = 200;
Config.prototype.maxHeight = "auto";
/**
* if set true and height !== auto then after reload editor will have the latest height
*/
Config.prototype.saveHeightInStorage = false;
//#endregion
//#region node_modules/jodit/esm/plugins/size/size.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$2 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
/**
* Calculate sizes for editor workspace and handle setHeight and setWidth events
*/
var size = class extends Plugin {
	constructor() {
		super(...arguments);
		/**
		* Debounced wrapper for resizeWorkspaceImd
		*/
		this.__resizeWorkspaces = this.j.async.debounce(this.__resizeWorkspaceImd, this.j.defaultTimeout, true);
	}
	afterInit(editor) {
		editor.e.on("setHeight.size", this.__setHeight).on("setWidth.size", this.__setWidth).on("afterInit.size changePlace.size", this.__initialize, { top: true }).on(editor.ow, "load.size", this.__resizeWorkspaces).on("afterInit.size resize.size afterUpdateToolbar.size scroll.size afterResize.size", this.__resizeWorkspaces).on("toggleFullSize.size toggleToolbar.size", this.__resizeWorkspaceImd);
		this.__immediateInitialize();
	}
	/**
	* Set editor size by options
	*/
	__initialize() {
		this.__immediateInitialize();
	}
	__immediateInitialize() {
		const { j } = this;
		if (j.o.inline) return;
		let { height } = j.o;
		if (j.o.saveHeightInStorage && height !== "auto") {
			const localHeight = j.storage.get("height");
			if (localHeight) height = localHeight;
		}
		css(j.editor, { minHeight: "100%" });
		css(j.container, {
			minHeight: j.o.minHeight,
			maxHeight: j.o.maxHeight,
			minWidth: j.o.minWidth,
			maxWidth: j.o.maxWidth
		});
		if (!j.isFullSize) {
			this.__setHeight(height);
			this.__setWidth(j.o.width);
		}
	}
	/**
	* Manually change height
	*/
	__setHeight(height) {
		const { clientHeight, clientWidth } = this.j.container;
		if (isNumber(height)) {
			const { minHeight, maxHeight } = this.j.o;
			if (isNumber(minHeight) && minHeight > height) height = minHeight;
			if (isNumber(maxHeight) && maxHeight < height) height = maxHeight;
		}
		css(this.j.container, "height", height);
		if (this.j.o.saveHeightInStorage) this.j.storage.set("height", height);
		this.__resizeWorkspaceImd({
			clientHeight,
			clientWidth
		});
	}
	/**
	* Manually change width
	*/
	__setWidth(width) {
		const { clientHeight, clientWidth } = this.j.container;
		if (isNumber(width)) {
			const { minWidth, maxWidth } = this.j.o;
			if (isNumber(minWidth) && minWidth > width) width = minWidth;
			if (isNumber(maxWidth) && maxWidth < width) width = maxWidth;
		}
		css(this.j.container, "width", width);
		this.__resizeWorkspaceImd({
			clientHeight,
			clientWidth
		});
	}
	/**
	* Returns service spaces: toolbar + statusbar
	*/
	__getNotWorkHeight() {
		var _a;
		const toolbar = this.j.toolbarContainer;
		return (toolbar && this.j.container.contains(toolbar) ? toolbar.offsetHeight : 0) + (((_a = this.j.statusbar) === null || _a === void 0 ? void 0 : _a.getHeight()) || 0) + 2;
	}
	/**
	* Calculate workspace height
	*/
	__resizeWorkspaceImd({ clientHeight, clientWidth } = this.j.container) {
		if (!this.j || this.j.isDestructed || !this.j.o || this.j.o.inline) return;
		if (!this.j.container || !this.j.container.parentNode) return;
		const minHeight = (css(this.j.container, "minHeight") || 0) - this.__getNotWorkHeight();
		if (isNumber(minHeight) && minHeight > 0) {
			[
				this.j.workplace,
				this.j.currentPlace.slots.center,
				this.j.iframe,
				this.j.editor
			].map((elm) => {
				elm && css(elm, "minHeight", minHeight);
			});
			this.j.e.fire("setMinHeight", minHeight);
		}
		if (isNumber(this.j.o.maxHeight)) {
			const maxHeight = this.j.o.maxHeight - this.__getNotWorkHeight();
			[
				this.j.workplace,
				this.j.currentPlace.slots.center,
				this.j.iframe,
				this.j.editor
			].map((elm) => {
				elm && css(elm, "maxHeight", maxHeight);
			});
			this.j.e.fire("setMaxHeight", maxHeight);
		}
		if (this.j.container) {
			const heightValue = this.j.o.height !== "auto" || this.j.isFullSize ? this.j.container.offsetHeight - this.__getNotWorkHeight() : "auto";
			css(this.j.workplace, "height", heightValue);
			this.j.container.style.setProperty("--jd-jodit-workplace-height", isNumber(heightValue) ? heightValue + "px" : heightValue);
		}
		const { clientHeight: newClientHeight, clientWidth: newClientWidth } = this.j.container;
		if (clientHeight !== newClientHeight || clientWidth !== newClientWidth) this.j.e.fire(this.j, "resize");
	}
	/** @override **/
	beforeDestruct(jodit) {
		jodit.e.off(jodit.ow, "load.size", this.__resizeWorkspaces).off(".size");
	}
};
__decorate$2([throttle()], size.prototype, "__initialize", null);
__decorate$2([autobind], size.prototype, "__setHeight", null);
__decorate$2([autobind], size.prototype, "__setWidth", null);
__decorate$2([autobind], size.prototype, "__resizeWorkspaceImd", null);
pluginSystem.add("size", size);
//#endregion
//#region node_modules/jodit/esm/plugins/stat/config.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* @module plugins/stat
*/
Config.prototype.showCharsCounter = true;
Config.prototype.countHTMLChars = false;
Config.prototype.countTextSpaces = false;
Config.prototype.showWordsCounter = true;
//#endregion
//#region node_modules/jodit/esm/plugins/stat/stat.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Show stat data - words and chars count
*/
var stat = class extends Plugin {
	constructor() {
		super(...arguments);
		this.charCounter = null;
		this.wordCounter = null;
		this.reInit = () => {
			if (this.j.o.showCharsCounter && this.charCounter) this.j.statusbar.append(this.charCounter, true);
			if (this.j.o.showWordsCounter && this.wordCounter) this.j.statusbar.append(this.wordCounter, true);
			this.j.e.off("change keyup", this.calc).on("change keyup", this.calc);
			this.calc();
		};
		this.calc = this.j.async.throttle(() => {
			const text = this.j.text;
			if (this.j.o.showCharsCounter && this.charCounter) {
				let chars;
				if (this.j.o.countHTMLChars) chars = this.j.value;
				else if (this.j.o.countTextSpaces) chars = text.replace(INVISIBLE_SPACE_REG_EXP(), "").replace(/[\r\n]/g, "");
				else chars = text.replace(SPACE_REG_EXP(), "");
				this.charCounter.textContent = this.j.i18n("Chars: %d", chars.length);
			}
			if (this.j.o.showWordsCounter && this.wordCounter) this.wordCounter.textContent = this.j.i18n("Words: %d", text.replace(INVISIBLE_SPACE_REG_EXP(), "").split(SPACE_REG_EXP()).filter((e) => e.length).length);
		}, this.j.defaultTimeout);
	}
	/** @override */
	afterInit() {
		this.charCounter = this.j.c.span();
		this.wordCounter = this.j.c.span();
		this.j.e.on("afterInit changePlace afterAddPlace", this.reInit);
		this.reInit();
	}
	/** @override */
	beforeDestruct() {
		Dom.safeRemove(this.charCounter);
		Dom.safeRemove(this.wordCounter);
		this.j.e.off("afterInit changePlace afterAddPlace", this.reInit);
		this.charCounter = null;
		this.wordCounter = null;
	}
};
pluginSystem.add("stat", stat);
//#endregion
//#region node_modules/jodit/esm/plugins/table/table.svg.js
var table_svg_default = "<svg xmlns='http://www.w3.org/2000/svg' viewBox=\"0 0 1792 1792\"> <path d=\"M576 1376v-192q0-14-9-23t-23-9h-320q-14 0-23 9t-9 23v192q0 14 9 23t23 9h320q14 0 23-9t9-23zm0-384v-192q0-14-9-23t-23-9h-320q-14 0-23 9t-9 23v192q0 14 9 23t23 9h320q14 0 23-9t9-23zm512 384v-192q0-14-9-23t-23-9h-320q-14 0-23 9t-9 23v192q0 14 9 23t23 9h320q14 0 23-9t9-23zm-512-768v-192q0-14-9-23t-23-9h-320q-14 0-23 9t-9 23v192q0 14 9 23t23 9h320q14 0 23-9t9-23zm512 384v-192q0-14-9-23t-23-9h-320q-14 0-23 9t-9 23v192q0 14 9 23t23 9h320q14 0 23-9t9-23zm512 384v-192q0-14-9-23t-23-9h-320q-14 0-23 9t-9 23v192q0 14 9 23t23 9h320q14 0 23-9t9-23zm-512-768v-192q0-14-9-23t-23-9h-320q-14 0-23 9t-9 23v192q0 14 9 23t23 9h320q14 0 23-9t9-23zm512 384v-192q0-14-9-23t-23-9h-320q-14 0-23 9t-9 23v192q0 14 9 23t23 9h320q14 0 23-9t9-23zm0-384v-192q0-14-9-23t-23-9h-320q-14 0-23 9t-9 23v192q0 14 9 23t23 9h320q14 0 23-9t9-23zm128-320v1088q0 66-47 113t-113 47h-1344q-66 0-113-47t-47-113v-1088q0-66 47-113t113-47h1344q66 0 113 47t47 113z\"/> </svg> ";
//#endregion
//#region node_modules/jodit/esm/plugins/table/config.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
Config.prototype.table = {
	splitBlockOnInsertTable: true,
	selectionCellStyle: "border: 1px double #1e88e5 !important;",
	useExtraClassesOptions: false
};
Icon.set("table", table_svg_default);
Config.prototype.controls.table = {
	data: {
		cols: 10,
		rows: 10,
		classList: {
			"table table-bordered": "Bootstrap Bordered",
			"table table-striped": "Bootstrap Striped",
			"table table-dark": "Bootstrap Dark"
		}
	},
	popup: (editor, current, close, button) => {
		editor.editor.normalize();
		const snapshot = editor.history.snapshot.make();
		const control = button.control;
		const default_rows_count = control.data && control.data.rows ? control.data.rows : 10, default_cols_count = control.data && control.data.cols ? control.data.cols : 10;
		const generateExtraClasses = () => {
			if (!editor.o.table.useExtraClassesOptions) return "";
			const out = [];
			if (control.data) {
				const classList = control.data.classList;
				Object.keys(classList).forEach((classes) => {
					out.push(`<label class="jodit_vertical_middle"><input class="jodit-checkbox" value="${classes}" type="checkbox"/>${classList[classes]}</label>`);
				});
			}
			return out.join("");
		};
		const form = editor.c.fromHTML("<form class=\"jodit-form jodit-form__inserter\"><div class=\"jodit-form__table-creator-box\"><div class=\"jodit-form__container\"></div><div class=\"jodit-form__options\">" + generateExtraClasses() + "</div></div><label class=\"jodit-form__center\"><span>1</span> &times; <span>1</span></label></form>"), [rows, cols] = (() => {
			const list = [];
			Dom.each(form, (node) => {
				Dom.isTag(node, "span") && list.push(node);
			});
			return list;
		})(), blocksContainer = Dom.first(form, (node) => Dom.isHTMLElement(node) && node.classList.contains("jodit-form__container")), options = Dom.first(form, (node) => Dom.isHTMLElement(node) && node.classList.contains("jodit-form__options")), cells = [];
		const cnt = default_rows_count * default_cols_count;
		attr(blocksContainer, {
			role: "grid",
			ariaLabel: "Table size",
			ariaRowcount: default_rows_count,
			ariaColcount: default_cols_count
		});
		for (let i = 0; i < cnt; i += 1) if (!cells[i]) {
			const row = Math.floor(i / default_cols_count) + 1, col = i % default_cols_count + 1;
			cells.push(editor.c.element("span", {
				dataIndex: i,
				role: "gridcell",
				tabindex: i === 0 ? 0 : -1,
				ariaLabel: `${row} by ${col}`
			}));
		}
		const highlightCell = (cell) => {
			const k = parseInt(attr(cell, "-index") || "0", 10);
			const rows_count = Math.ceil((k + 1) / default_cols_count), cols_count = k % default_cols_count + 1;
			for (let i = 0; i < cells.length; i += 1) if (cols_count >= i % default_cols_count + 1 && rows_count >= Math.ceil((i + 1) / default_cols_count)) cells[i].className = "jodit_hovered";
			else cells[i].className = "";
			cols.textContent = cols_count.toString();
			rows.textContent = rows_count.toString();
		};
		const insertTable = (cell) => {
			const k = parseInt(attr(cell, "-index") || "0", 10);
			const rows_count = Math.ceil((k + 1) / default_cols_count), cols_count = k % default_cols_count + 1;
			const crt = editor.createInside, tbody = crt.element("tbody"), table = crt.element("table");
			Dom.append(table, tbody);
			let first_td = null, tr, td;
			for (let i = 1; i <= rows_count; i += 1) {
				tr = crt.element("tr");
				for (let j = 1; j <= cols_count; j += 1) {
					td = crt.element("td");
					if (!first_td) first_td = td;
					css(td, "width", (100 / cols_count).toFixed(4) + "%");
					Dom.append(td, crt.element("br"));
					Dom.append(tr, crt.text("\n"));
					Dom.append(tr, crt.text("	"));
					Dom.append(tr, td);
				}
				Dom.append(tbody, crt.text("\n"));
				Dom.append(tbody, tr);
			}
			$$("input[type=checkbox]:checked", options).forEach((input) => {
				input.value.split(/[\s]+/).forEach((className) => {
					table.classList.add(className);
				});
			});
			editor.s.restore();
			editor.s.removeMarkers();
			editor.editor.normalize();
			editor.history.snapshot.restore(snapshot);
			const block = Dom.furthest(editor.s.current(), Dom.isBlock, editor.editor);
			if (block && Dom.isEmpty(block)) Dom.replace(block, table, void 0, false, true);
			else if (block) {
				const fake = crt.text("\n");
				if (!editor.o.table.splitBlockOnInsertTable) {
					Dom.after(block, fake);
					Dom.after(fake, table);
				} else {
					const range = editor.s.range;
					range.collapse(false);
					range.insertNode(fake);
					range.collapse(false);
					editor.s.selectRange(range);
					const firstPart = editor.s.splitSelection(block, fake);
					if (firstPart) Dom.after(firstPart, table);
					else Dom.after(block, table);
				}
			} else editor.s.insertNode(table, false);
			if (first_td) {
				editor.s.setCursorIn(first_td);
				scrollIntoViewIfNeeded(first_td, editor.editor, editor.ed);
			}
			close();
		};
		editor.e.on(blocksContainer, "mousemove", (e) => {
			if (Dom.isTag(e.target, "span")) highlightCell(e.target);
		}).on(blocksContainer, "touchstart mousedown", (e) => {
			if (!Dom.isTag(e.target, "span")) return;
			e.preventDefault();
			e.stopImmediatePropagation();
			insertTable(e.target);
		}).on(blocksContainer, "keydown", (e) => {
			if (!Dom.isTag(e.target, "span")) return;
			if (e.key === "Enter") {
				e.preventDefault();
				e.stopImmediatePropagation();
				insertTable(e.target);
				return;
			}
			if (e.key === "Escape") {
				e.preventDefault();
				e.stopImmediatePropagation();
				close();
				return;
			}
			const index = parseInt(attr(e.target, "-index") || "0", 10), row = Math.floor(index / default_cols_count), col = index % default_cols_count;
			let nextIndex = index;
			switch (e.key) {
				case KEY_LEFT:
					nextIndex = col > 0 ? index - 1 : index;
					break;
				case KEY_RIGHT:
					nextIndex = col < default_cols_count - 1 ? index + 1 : index;
					break;
				case KEY_UP:
					nextIndex = row > 0 ? index - default_cols_count : index;
					break;
				case KEY_DOWN:
					nextIndex = row < default_rows_count - 1 ? index + default_cols_count : index;
					break;
				default: return;
			}
			e.preventDefault();
			e.stopImmediatePropagation();
			if (nextIndex !== index) {
				cells[index].tabIndex = -1;
				cells[nextIndex].tabIndex = 0;
				cells[nextIndex].focus();
				highlightCell(cells[nextIndex]);
			}
		});
		if (button && button.parentElement) {
			for (let i = 0; i < default_rows_count; i += 1) {
				const row = editor.c.div();
				attr(row, "role", "row");
				for (let j = 0; j < default_cols_count; j += 1) Dom.append(row, cells[i * default_cols_count + j]);
				Dom.append(blocksContainer, row);
			}
			if (cells[0]) cells[0].className = "hovered";
		}
		return form;
	},
	tooltip: "Insert table"
};
//#endregion
//#region node_modules/jodit/esm/plugins/table/table.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
function table(editor) {
	editor.registerButton({
		name: "table",
		group: "insert"
	});
}
pluginSystem.add("table", table);
//#endregion
//#region node_modules/jodit/esm/plugins/wrap-nodes/config.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
Config.prototype.wrapNodes = {
	exclude: /* @__PURE__ */ new Set([
		"hr",
		"style",
		"br"
	]),
	emptyBlockAfterInit: true
};
//#endregion
//#region node_modules/jodit/esm/plugins/wrap-nodes/wrap-nodes.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate$1 = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
/**
* Wrap single text nodes in block wrapper
*/
var wrapNodes = class extends Plugin {
	constructor() {
		super(...arguments);
		/**
		* Found Node which should be wrapped
		*/
		this.isSuitableStart = (n) => {
			var _a;
			return Dom.isText(n) && isString(n.nodeValue) && (/[^\s]/.test(n.nodeValue) || ((_a = n.parentNode) === null || _a === void 0 ? void 0 : _a.firstChild) === n && this.isSuitable(n.nextSibling)) || this.isNotWrapped(n) && !Dom.isTemporary(n);
		};
		/**
		* Node should add in a block element
		*/
		this.isSuitable = (n) => Dom.isText(n) || this.isNotWrapped(n);
		/**
		* Some element which needs to append in block
		*/
		this.isNotWrapped = (n) => Dom.isElement(n) && !(Dom.isBlock(n) || Dom.isTag(n, this.j.o.wrapNodes.exclude));
	}
	/** @override **/
	afterInit(jodit) {
		if (jodit.o.enter.toLowerCase() === "br") return;
		jodit.e.on("drop.wtn focus.wtn keydown.wtn mousedown.wtn afterInit.wtn backSpaceAfterDelete.wtn", this.preprocessInput, { top: true }).on("afterInit.wtn postProcessSetEditorValue.wtn afterCommitStyle.wtn backSpaceAfterDelete.wtn", this.postProcessSetEditorValue);
	}
	/** @override **/
	beforeDestruct(jodit) {
		jodit.e.off(".wtn");
	}
	/**
	* Process changed value
	*/
	postProcessSetEditorValue() {
		const { jodit } = this;
		if (!jodit.isEditorMode()) return;
		let child = jodit.editor.firstChild;
		let isChanged = false;
		while (child) {
			child = checkAloneListLeaf(child, jodit);
			if (this.isSuitableStart(child)) {
				if (!isChanged) jodit.s.save();
				isChanged = true;
				const box = jodit.createInside.element(jodit.o.enter);
				Dom.before(child, box);
				while (child && this.isSuitable(child)) {
					const next = child.nextSibling;
					Dom.append(box, child);
					child = next;
				}
				box.normalize();
				child = box;
			}
			child = child && child.nextSibling;
		}
		if (isChanged) {
			jodit.s.restore();
			if (jodit.e.current === "afterInit") jodit.e.fire("internalChange");
		}
	}
	/**
	* Process input without parent box
	*/
	preprocessInput() {
		const { jodit } = this, isAfterInitEvent = jodit.e.current === "afterInit";
		if (!jodit.isEditorMode() || jodit.editor.firstChild || !jodit.o.wrapNodes.emptyBlockAfterInit && isAfterInitEvent) return;
		const box = jodit.createInside.element(jodit.o.enter);
		const br = jodit.createInside.element("br");
		Dom.append(box, br);
		Dom.append(jodit.editor, box);
		if (jodit.s.isFocused() || jodit.e.current === "backSpaceAfterDelete") jodit.s.setCursorBefore(br);
		jodit.e.fire("internalChange");
	}
};
__decorate$1([autobind], wrapNodes.prototype, "postProcessSetEditorValue", null);
__decorate$1([autobind], wrapNodes.prototype, "preprocessInput", null);
function checkAloneListLeaf(child, jodit) {
	let result = child;
	let next = child;
	do
		if (Dom.isElement(next) && Dom.isLeaf(next) && !Dom.isList(next.parentElement)) {
			const nextChild = Dom.findNotEmptySibling(next, false);
			if (Dom.isTag(result, "ul")) Dom.append(result, next);
			else result = Dom.wrap(next, "ul", jodit.createInside);
			next = nextChild;
		} else break;
	while (next);
	return result;
}
pluginSystem.add("wrapNodes", wrapNodes);
//#endregion
//#region node_modules/jodit/esm/plugins/dtd/config.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
Config.prototype.dtd = {
	removeExtraBr: true,
	checkBlockNesting: true,
	blockLimits: {
		article: 1,
		aside: 1,
		audio: 1,
		body: 1,
		caption: 1,
		details: 1,
		dir: 1,
		div: 1,
		dl: 1,
		fieldset: 1,
		figcaption: 1,
		figure: 1,
		footer: 1,
		form: 1,
		header: 1,
		hgroup: 1,
		main: 1,
		menu: 1,
		nav: 1,
		ol: 1,
		section: 1,
		table: 1,
		td: 1,
		th: 1,
		tr: 1,
		ul: 1,
		video: 1
	}
};
//#endregion
//#region node_modules/jodit/esm/plugins/dtd/after-insert/remove-extra-br.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var brBoxes = /* @__PURE__ */ new Set([
	"table",
	"pre",
	"blockquote",
	"code"
]);
/**
* Checks if there is a tag in the block element after the inserted br node,
* if so, removes it
* @internal
*/
function removeExtraBr(jodit, node) {
	if (!jodit.o.dtd.removeExtraBr || Dom.isTag(node, "br")) return;
	const parent = Dom.furthest(node, Dom.isBlock, jodit.editor);
	if (parent && !Dom.isTag(parent, brBoxes)) {
		const br = Dom.isTag(node, "br") ? node : Dom.findNotEmptySibling(node, false);
		if (!Dom.isTag(br, "br")) return;
		if (Dom.findNotEmptySibling(br, false)) return;
		jodit.s.setCursorBefore(br);
		Dom.safeRemove(br);
	}
}
//#endregion
//#region node_modules/jodit/esm/plugins/dtd/after-insert/index.js
var after_insert_exports = /* @__PURE__ */ __exportAll({ removeExtraBr: () => removeExtraBr });
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
//#endregion
//#region node_modules/jodit/esm/plugins/dtd/before-insert/check-block-nesting.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* Checks whether the insertion of an element at the current location is allowed,
* if it is not allowed, it deletes an empty block element or moves the cursor after it
* @internal
*/
function checkBlockNesting(jodit, node) {
	var _a, _b;
	if (Dom.isFragment(node)) node = node.firstChild;
	if (jodit.o.dtd.checkBlockNesting && Dom.isBlock(node)) {
		let parent = null;
		let current = (_b = (_a = jodit.s.current()) === null || _a === void 0 ? void 0 : _a.parentElement) !== null && _b !== void 0 ? _b : null;
		while (current && current !== jodit.editor) {
			if (Dom.isBlock(current)) {
				if (jodit.o.dtd.blockLimits[current.nodeName.toLowerCase()]) break;
				parent = current;
			}
			current = current.parentElement;
		}
		if (parent) {
			jodit.s.setCursorAfter(parent);
			if (Dom.isEmpty(parent)) Dom.safeRemove(parent);
		}
	}
}
//#endregion
//#region node_modules/jodit/esm/plugins/dtd/before-insert/index.js
var before_insert_exports = /* @__PURE__ */ __exportAll({ checkBlockNesting: () => checkBlockNesting });
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
//#endregion
//#region node_modules/jodit/esm/plugins/dtd/dtd.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
var __decorate = function(decorators, target, key, desc) {
	var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
	if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
	else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
	return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var dtd = class extends Plugin {
	afterInit(jodit) {}
	beforeDestruct(jodit) {}
	__onBeforeInsertNode(node) {
		Object.keys(before_insert_exports).forEach((key) => {
			before_insert_exports[key](this.j, node);
		});
	}
	__onAfterInsertNode(node) {
		Object.keys(after_insert_exports).forEach((key) => {
			after_insert_exports[key](this.j, node);
		});
	}
};
__decorate([watch(":beforeInsertNode")], dtd.prototype, "__onBeforeInsertNode", null);
__decorate([watch(":afterInsertNode")], dtd.prototype, "__onAfterInsertNode", null);
pluginSystem.add("dtd", dtd);
//#endregion
//#region node_modules/jodit/esm/plugins/index.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
//#endregion
//#region node_modules/jodit/esm/index.js
/*!
* Jodit Editor (https://xdsoft.net/jodit/)
* Released under MIT see LICENSE.txt in the project root for license information.
* Copyright (c) 2013-2026 Valerii Chupurnov. All rights reserved. https://xdsoft.net
*/
/**
* [[include:README.md]]
* @packageDocumentation
* @module jodit
*/
Object.keys(constants_exports).forEach((key) => {
	Jodit$1[key] = constants_exports[key];
});
var esFilter = (key) => key !== "__esModule";
Object.keys(icons_exports).filter(esFilter).forEach((key) => {
	Icon.set(key.replace("_", "-"), icons_exports[key]);
});
Object.keys(modules_exports).filter(esFilter).forEach((key) => {
	var _a;
	const module = modules_exports[key];
	const name = isFunction((_a = module.prototype) === null || _a === void 0 ? void 0 : _a.className) ? module.prototype.className() : key;
	if (!isString(name)) {
		console.warn("Module name must be a string", key);
		return;
	}
	Jodit$1.modules[name] = module;
});
Object.keys(decorators_exports).filter(esFilter).forEach((key) => {
	Jodit$1.decorators[key] = decorators_exports[key];
});
[
	"Confirm",
	"Alert",
	"Prompt"
].forEach((key) => {
	Jodit$1[key] = modules_exports[key];
});
Object.keys(languages_default).filter(esFilter).forEach((key) => {
	Jodit$1.lang[key] = languages_default[key];
});
var CommitMode = class {};
//#endregion
export { CommitMode, Jodit$1 as Jodit };
