using System;
using System.Collections.Generic;

namespace EtheriT.Coker.Application.Processor.Option
{
    public class HtmlSanitizeOptions
    {
        /// <summary>
        /// 允許前台發布的 HTML tag。
        /// 不在清單內的 tag 會被移除。
        /// </summary>
        public HashSet<string> AllowedTags { get; } = new(StringComparer.OrdinalIgnoreCase)
        {
            // HTML5 sectioning / layout
            "main",
            "section",
            "article",
            "aside",
            "header",
            "footer",
            "nav",

            // Generic containers
            "div",
            "span",

            // Text content
            "p",
            "br",
            "hr",
            "pre",
            "blockquote",
            "figure",
            "figcaption",

            // Headings
            "h1",
            "h2",
            "h3",
            "h4",
            "h5",
            "h6",

            // Inline text semantics
            "strong",
            "b",
            "em",
            "i",
            "u",
            "small",
            "sub",
            "sup",
            "mark",
            "del",
            "ins",
            "s",
            "code",
            "kbd",
            "samp",
            "var",
            "abbr",
            "cite",
            "q",
            "time",
            "wbr",

            // Lists
            "ul",
            "ol",
            "li",
            "dl",
            "dt",
            "dd",

            // Tables
            "table",
            "caption",
            "colgroup",
            "col",
            "thead",
            "tbody",
            "tfoot",
            "tr",
            "th",
            "td",

            // Link / media
            "a",
            "img",
            "picture",
            "source",
            "iframe",

            // Audio / video
            "audio",
            "video",
            "track",

            // Disclosure
            "details",
            "summary",

            // Forms
            "form",
            "fieldset",
            "legend",
            "input",
            "select",
            "optgroup",
            "option",
            "textarea",
            "label",
            "button",
            "output",
            "progress",
            "meter"
        };

        /// <summary>
        /// 大多數 HTML tag 都可使用的 global attributes。
        /// </summary>
        public HashSet<string> GlobalAttributes { get; } = new(StringComparer.OrdinalIgnoreCase)
        {
            "id",
            "class",
            "title",
            "role",
            "tabindex",
            "hidden",
            "lang",
            "dir",
            "draggable",
            "spellcheck",
            "translate",
        };

        /// <summary>
        /// 允許的屬性前綴。
        /// data-* 用於 Bootstrap、GrapesJS、DynamicForm。
        /// aria-* 用於 accessibility。
        /// </summary>
        public HashSet<string> GlobalAttributePrefixes { get; } = new(StringComparer.OrdinalIgnoreCase)
        {
            "data-",
            "aria-"
        };

        /// <summary>
        /// 明確禁止的屬性。
        /// 這些即使符合 data-* 或其他規則，也會被移除。
        /// </summary>
        public HashSet<string> BlockedAttributes { get; } = new(StringComparer.OrdinalIgnoreCase)
        {
            "style",
            // iframe srcdoc 可直接塞 HTML，禁止。
            "srcdoc",

            // Bootstrap tooltip/popover 若開啟 html=true，內容可能被當 HTML 注入。
            // 若未來有明確需求，再單獨評估開放。
            "data-bs-html"
        };

        /// <summary>
        /// 特定 tag 專屬屬性。
        /// 原則：只有該 tag 合理需要的屬性才列在這裡。
        /// </summary>
        public Dictionary<string, HashSet<string>> TagAttributes { get; } = new(StringComparer.OrdinalIgnoreCase)
        {
            ["a"] = new(StringComparer.OrdinalIgnoreCase)
            {
                "href",
                "target",
                "rel",
                "download",
                "hreflang",
                "type"
            },

            ["img"] = new(StringComparer.OrdinalIgnoreCase)
            {
                "src",
                "srcset",
                "sizes",
                "alt",
                "width",
                "height",
                "loading",
                "decoding",
                "referrerpolicy",
                "usemap",
                "ismap"
            },

            ["picture"] = new(StringComparer.OrdinalIgnoreCase)
            {
            },

            ["source"] = new(StringComparer.OrdinalIgnoreCase)
            {
                "src",
                "srcset",
                "sizes",
                "type",
                "media"
            },

            ["iframe"] = new(StringComparer.OrdinalIgnoreCase)
            {
                "src",
                "title",
                "width",
                "height",
                "loading",
                "allow",
                "allowfullscreen",
                "referrerpolicy",
                "frameborder",
                "name"
            },

            ["audio"] = new(StringComparer.OrdinalIgnoreCase)
            {
                "src",
                "controls",
                "autoplay",
                "loop",
                "muted",
                "preload",
                "crossorigin"
            },

            ["video"] = new(StringComparer.OrdinalIgnoreCase)
            {
                "src",
                "poster",
                "controls",
                "autoplay",
                "loop",
                "muted",
                "playsinline",
                "preload",
                "width",
                "height",
                "crossorigin"
            },

            ["track"] = new(StringComparer.OrdinalIgnoreCase)
            {
                "src",
                "kind",
                "srclang",
                "label",
                "default"
            },

            ["blockquote"] = new(StringComparer.OrdinalIgnoreCase)
            {
                "cite"
            },

            ["q"] = new(StringComparer.OrdinalIgnoreCase)
            {
                "cite"
            },

            ["time"] = new(StringComparer.OrdinalIgnoreCase)
            {
                "datetime"
            },

            ["abbr"] = new(StringComparer.OrdinalIgnoreCase)
            {
                "title"
            },

            ["ins"] = new(StringComparer.OrdinalIgnoreCase)
            {
                "cite",
                "datetime"
            },

            ["del"] = new(StringComparer.OrdinalIgnoreCase)
            {
                "cite",
                "datetime"
            },

            ["ol"] = new(StringComparer.OrdinalIgnoreCase)
            {
                "start",
                "reversed",
                "type"
            },

            ["li"] = new(StringComparer.OrdinalIgnoreCase)
            {
                "value"
            },

            ["table"] = new(StringComparer.OrdinalIgnoreCase)
            {
                "summary"
            },

            ["colgroup"] = new(StringComparer.OrdinalIgnoreCase)
            {
                "span"
            },

            ["col"] = new(StringComparer.OrdinalIgnoreCase)
            {
                "span"
            },

            ["td"] = new(StringComparer.OrdinalIgnoreCase)
            {
                "colspan",
                "rowspan",
                "headers"
            },

            ["th"] = new(StringComparer.OrdinalIgnoreCase)
            {
                "colspan",
                "rowspan",
                "headers",
                "scope",
                "abbr"
            },

            ["details"] = new(StringComparer.OrdinalIgnoreCase)
            {
                "open"
            },

            ["form"] = new(StringComparer.OrdinalIgnoreCase)
            {
                "name",
                "autocomplete",
                "novalidate"
            },

            ["fieldset"] = new(StringComparer.OrdinalIgnoreCase)
            {
                "name",
                "disabled",
                "form"
            },

            ["legend"] = new(StringComparer.OrdinalIgnoreCase)
            {
            },

            ["label"] = new(StringComparer.OrdinalIgnoreCase)
            {
                "for",
                "form"
            },

            ["input"] = new(StringComparer.OrdinalIgnoreCase)
            {
                "type",
                "name",
                "value",
                "placeholder",

                "required",
                "disabled",
                "readonly",
                "checked",

                "maxlength",
                "minlength",
                "max",
                "min",
                "step",
                "pattern",

                "autocomplete",
                "inputmode",
                "multiple",
                "accept",
                "size",
                "list",

                "form",
                "formnovalidate"
            },

            ["select"] = new(StringComparer.OrdinalIgnoreCase)
            {
                "name",
                "required",
                "disabled",
                "multiple",
                "size",
                "autocomplete",
                "form"
            },

            ["optgroup"] = new(StringComparer.OrdinalIgnoreCase)
            {
                "label",
                "disabled"
            },

            ["option"] = new(StringComparer.OrdinalIgnoreCase)
            {
                "value",
                "label",
                "selected",
                "disabled"
            },

            ["textarea"] = new(StringComparer.OrdinalIgnoreCase)
            {
                "name",
                "placeholder",

                "required",
                "disabled",
                "readonly",

                "maxlength",
                "minlength",
                "rows",
                "cols",
                "wrap",

                "autocomplete",
                "form"
            },

            ["button"] = new(StringComparer.OrdinalIgnoreCase)
            {
                "type",
                "name",
                "value",
                "disabled",

                "form",
                "formnovalidate"
            },

            ["output"] = new(StringComparer.OrdinalIgnoreCase)
            {
                "name",
                "for",
                "form"
            },

            ["progress"] = new(StringComparer.OrdinalIgnoreCase)
            {
                "value",
                "max"
            },

            ["meter"] = new(StringComparer.OrdinalIgnoreCase)
            {
                "value",
                "min",
                "max",
                "low",
                "high",
                "optimum"
            }
        };

        /// <summary>
        /// 需要檢查 URL scheme 的屬性。
        /// 只要屬性在這裡，就算被允許，也要檢查值。
        /// </summary>
        public HashSet<string> UrlAttributes { get; } = new(StringComparer.OrdinalIgnoreCase)
        {
            "href",
            "src",
            "srcset",
            "poster",
            "cite"
        };

        /// <summary>
        /// 允許的一般 URL scheme。
        /// iframe domain 不在這裡管理，交給 CSP frame-src。
        /// </summary>
        public HashSet<string> AllowedUrlSchemes { get; } = new(StringComparer.OrdinalIgnoreCase)
        {
            "http",
            "https",
            "mailto",
            "tel"
        };

        /// <summary>
        /// data URI 只允許圖片格式。
        /// 不允許 data:text/html、data:application/xhtml+xml。
        /// </summary>
        public HashSet<string> AllowedDataImagePrefixes { get; } = new(StringComparer.OrdinalIgnoreCase)
        {
            "data:image/png",
            "data:image/jpeg",
            "data:image/jpg",
            "data:image/gif",
            "data:image/webp",
            "data:image/svg+xml"
        };

        public HashSet<string> DataUrlAttributes { get; } = new(StringComparer.OrdinalIgnoreCase)
        {
            "data-href",
            "data-src",
            "data-url",
            "data-link",
            "data-file",
            "data-image",
            "data-pdf-url",
            "data-background",
            "data-bg"
        };

        /// <summary>
        /// 是否移除後台專用節點。
        /// </summary>
        public bool RemoveBackstageNodes { get; set; } = true;

        /// <summary>
        /// a[target="_blank"] 強制補 rel。
        /// </summary>
        public bool ForceNoopenerForBlankTarget { get; set; } = true;
    }
}