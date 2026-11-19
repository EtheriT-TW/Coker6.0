using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace EtheriT.Coker.Application.SiteCrawl;

internal sealed record SiteFormRecipient(string Email, string Title);

// 把舊站的表單轉成新系統的動態表單（.ContactForm，送到 /api/Contact/submit）。
// 結構照現有站台實際使用的聯絡表單；舊驗證碼換成新系統驗證碼，收件信箱用網站資料的聯絡信箱。
// CleanAsync 會移除 form 與 data-* 屬性，所以先換成占位註解，清理完再放回轉好的表單。
internal static partial class SiteFormConverter
{
    private const string PlaceholderPrefix = "crawl-form:";
    private const int MinFields = 2;
    private const int MaxGroupTitleLength = 30;
    private static readonly HashSet<string> SkippedInputTypes =
        new(StringComparer.OrdinalIgnoreCase) { "hidden", "submit", "button", "image", "reset", "file" };

    [GeneratedRegex(@"validate|captcha|checkcode|verify", RegexOptions.IgnoreCase)]
    private static partial Regex CaptchaName();

    [GeneratedRegex(@"同意|隱私|agree|privacy", RegexOptions.IgnoreCase)]
    private static partial Regex AgreementText();

    [GeneratedRegex(@"^[\w\-]+$")]
    private static partial Regex SafeId();

    internal static List<string> Extract(HtmlNode root, SiteFormRecipient recipient)
    {
        var forms = new List<string>();
        foreach (var form in root.SelectNodes(".//form")?.ToArray() ?? [])
        {
            var converted = Convert(form, recipient);
            if (converted == null) continue; // 搜尋框這類不是表單內容的，交給 CleanAsync 移除
            form.ParentNode.ReplaceChild(form.OwnerDocument.CreateComment($"<!--{PlaceholderPrefix}{forms.Count}-->"), form);
            forms.Add(converted);
        }
        return forms;
    }

    internal static void Restore(HtmlNode root, IReadOnlyList<string> forms)
    {
        if (forms.Count == 0) return;
        foreach (var comment in root.SelectNodes(".//comment()")?.ToArray() ?? [])
        {
            var match = Regex.Match(((HtmlCommentNode)comment).Comment, PlaceholderPrefix + @"(\d+)");
            if (!match.Success) continue;
            comment.ParentNode.ReplaceChild(HtmlNode.CreateNode(forms[int.Parse(match.Groups[1].Value)]), comment);
        }
    }

    private static string? Convert(HtmlNode form, SiteFormRecipient recipient)
    {
        if (form.GetAttributeValue("method", "").Equals("get", StringComparison.OrdinalIgnoreCase)) return null;
        var controls = (form.SelectNodes(".//input|.//textarea|.//select") ?? Enumerable.Empty<HtmlNode>())
            .Where(IsField).ToList();
        if (controls.Count(control => !IsCaptcha(control)) < MinFields) return null;

        var html = new StringBuilder();
        html.Append("<form id=\"ContactForm\" novalidate=\"\" class=\"needs-validation ContactForm\">");
        html.Append("<input name=\"authentiity_token\" type=\"hidden\" value=\"@Model.token\">");
        html.Append($"<input data-title=\"{E(recipient.Title)}\" name=\"sender\" type=\"hidden\" value=\"{E(recipient.Email)}\">");
        var groups = new HashSet<string>(StringComparer.Ordinal);
        foreach (var control in controls.Where(control => !IsCaptcha(control)))
        {
            var type = TypeOf(control);
            if (type is "radio" or "checkbox")
            {
                var name = control.GetAttributeValue("name", "");
                if (!groups.Add(type + ":" + name)) continue;
                var items = controls.Where(x => TypeOf(x) == type && x.GetAttributeValue("name", "") == name).ToList();
                html.Append(type == "checkbox" && items.Count == 1 && AgreementText().IsMatch(LabelOf(form, items[0]) ?? "")
                    ? Agreement(form, items[0])
                    : ChoiceGroup(form, type, name, items));
            }
            else
            {
                html.Append(control.Name == "select" ? Select(form, control) : TextField(form, control, type));
            }
        }
        html.Append(CaptchaAndSubmit());
        html.Append("</form>");
        return html.ToString();
    }

    private static string TextField(HtmlNode form, HtmlNode control, string type)
    {
        var (id, name) = Ids(control);
        var label = LabelOf(form, control) ?? name;
        var required = IsRequired(form, control);
        var isTextarea = control.Name == "textarea";
        var inputType = IsEmail(control, label) ? "email" : "text";
        var attrs = Attr("placeholder", control.GetAttributeValue("placeholder", ""))
            + Attr("maxlength", control.GetAttributeValue("maxlength", ""))
            + (required ? " required=\"\"" : "");
        var field = isTextarea
            ? $"<textarea id=\"{id}\" name=\"{E(name)}\"{attrs} class=\"form-control\"></textarea>"
            : $"<input id=\"{id}\" name=\"{E(name)}\"{attrs} type=\"{inputType}\" class=\"form-control\">";
        var kind = isTextarea ? "textarea-field full-line" : "text-field";
        return $"<div class=\"contact-field {kind}{(required ? " required-field" : "")} form-floating\">"
            + $"<label for=\"{id}\" class=\"contact-label{(required ? " required" : "")}\">{E(label)}</label>"
            + field + Feedback(required, label) + "</div>";
    }

    private static string Select(HtmlNode form, HtmlNode control)
    {
        var (id, name) = Ids(control);
        var label = LabelOf(form, control) ?? name;
        var required = IsRequired(form, control);
        var options = new StringBuilder();
        foreach (var option in control.SelectNodes(".//option") ?? Enumerable.Empty<HtmlNode>())
        {
            var text = Text(option);
            var value = option.Attributes["value"]?.DeEntitizeValue ?? text;
            options.Append($"<option value=\"{E(value)}\">{E(text)}</option>");
        }
        return $"<div class=\"contact-field select-field{(required ? " required-field" : "")} form-floating\">"
            + $"<div class=\"select-label-wrap\"><label for=\"{id}\" class=\"contact-label{(required ? " required" : "")}\">{E(label)}</label></div>"
            + $"<div class=\"select-input-wrap\"><select data-dynamic-select=\"1\" id=\"{id}\" name=\"{E(name)}\"{(required ? " required=\"\"" : "")} class=\"form-select\">{options}</select></div>"
            + Feedback(required, label) + "</div>";
    }

    private static string ChoiceGroup(HtmlNode form, string type, string name, List<HtmlNode> items)
    {
        var title = GroupTitle(form, name, items) ?? name;
        var required = items.Any(item => IsRequired(form, item)) || IsRequiredLabel(form, name);
        var choices = new StringBuilder();
        foreach (var item in items)
        {
            var (id, _) = Ids(item);
            var label = LabelOf(form, item) ?? item.GetAttributeValue("value", "");
            choices.Append("<div class=\"form-check choice-item\">"
                + $"<input id=\"{id}\" name=\"{E(name)}\"{(required && type == "radio" ? " required=\"\"" : "")} type=\"{type}\" value=\"{E(label)}\" class=\"form-check-input\">"
                + $"<label for=\"{id}\" class=\"form-check-label\">{E(label)}</label></div>");
        }
        return $"<div class=\"contact-field choice-field{(required ? " required-field" : "")} form-floating\">"
            + $"<div class=\"contact-label{(required ? " required" : "")} title\">{E(title)}</div>"
            + $"<div class=\"choice-list inline-choice-list d-flex\">{choices}</div>"
            + Feedback(required, title) + "</div>";
    }

    private static string Agreement(HtmlNode form, HtmlNode item)
    {
        var (id, name) = Ids(item);
        var label = LabelOf(form, item) ?? name;
        return "<div class=\"form-check agree-check\">"
            + $"<input id=\"{id}\" name=\"{E(name)}\" required=\"\" type=\"checkbox\" value=\"\" class=\"form-check-input\">"
            + $"<label for=\"{id}\" class=\"form-check-label\">{E(label)}</label>"
            + $"<div class=\"invalid-feedback\">請勾選{E(label)}</div></div>";
    }

    private static string CaptchaAndSubmit() =>
        "<div class=\"row gx-0 align-items-end mt-2 mt-md-3 endline-row\">"
        + "<div class=\"captcha-field-col\"><div class=\"captcha-field form-floating\">"
        + "<label for=\"InputCaptcha\" class=\"required\">驗證碼</label>"
        + "<input id=\"InputCaptcha\" minlength=\"4\" name=\"captcha\" placeholder=\"輸入右側圖形驗證碼\" required=\"\" type=\"text\" class=\"form-control\">"
        + "<div class=\"invalid-feedback\">請輸入正確的驗證碼</div></div></div>"
        + "<div class=\"captcha-image-field\"><a aria-label=\"重新產生驗證碼\" class=\"btn_refresh bg-transparent border-0\"><i class=\"fa-solid fa-repeat\"></i></a>"
        + "<img alt=\"驗證碼\" id=\"imgCaptcha\" src=\"/api/Captcha/index\" class=\"img-fluid\"></div>"
        + "<button class=\"btn_send border-0 rounded py-2 my-3 text-white\">送出</button></div>";

    private static string Feedback(bool required, string label) =>
        required ? $"<div class=\"invalid-feedback\">請填寫{E(label)}</div>" : "";

    private static bool IsField(HtmlNode control) =>
        control.Name != "input" || !SkippedInputTypes.Contains(control.GetAttributeValue("type", "text"));

    private static bool IsCaptcha(HtmlNode control) =>
        CaptchaName().IsMatch(control.GetAttributeValue("name", "") + " " + control.GetAttributeValue("id", ""));

    private static string TypeOf(HtmlNode control) =>
        control.Name == "input" ? control.GetAttributeValue("type", "text").ToLowerInvariant() : control.Name;

    private static bool IsEmail(HtmlNode control, string label) =>
        control.GetAttributeValue("type", "").Equals("email", StringComparison.OrdinalIgnoreCase)
        || control.GetAttributeValue("limit", "").Equals("email", StringComparison.OrdinalIgnoreCase)
        || control.GetAttributeValue("name", "").Equals("email", StringComparison.OrdinalIgnoreCase)
        || label.Contains("mail", StringComparison.OrdinalIgnoreCase) || label.Contains("信箱");

    // 舊站必填是 label 加 class="must"，新寫法是欄位上的 required。
    private static bool IsRequired(HtmlNode form, HtmlNode control) =>
        control.Attributes["required"] != null || IsRequiredLabel(form, control.GetAttributeValue("id", ""));

    private static bool IsRequiredLabel(HtmlNode form, string target) =>
        LabelNode(form, target)?.GetClasses().Contains("must") == true;

    private static (string Id, string Name) Ids(HtmlNode control)
    {
        var oldId = control.GetAttributeValue("id", "");
        var name = control.GetAttributeValue("name", "");
        if (name.Length == 0) name = oldId.Length > 0 ? oldId : "field";
        var id = "Crawl" + (SafeId().IsMatch(oldId) ? oldId : Regex.Replace(name, @"[^\w\-]", "_"));
        return (id, name);
    }

    private static string? LabelOf(HtmlNode form, HtmlNode control)
    {
        var label = LabelNode(form, control.GetAttributeValue("id", ""));
        return label == null ? null : CleanLabel(Text(label));
    }

    // 舊站群組標題會寫成 <span class="must" for="sex">，不一定是 label。
    private static HtmlNode? LabelNode(HtmlNode form, string target) =>
        SafeId().IsMatch(target) ? form.SelectSingleNode($".//*[self::label or self::span][@for='{target}']") : null;

    // 單選／複選群組的標題：優先找 for=群組名稱 的 label，否則取包住整組選項的容器裡、選項以外的短文字。
    private static string? GroupTitle(HtmlNode form, string name, List<HtmlNode> items)
    {
        if (LabelNode(form, name) is { } label) return CleanLabel(Text(label));
        var optionTexts = items.Select(item => LabelOf(form, item) ?? "").Where(text => text.Length > 0).ToList();
        for (var container = items[0].ParentNode; container != null && container != form; container = container.ParentNode)
        {
            if (!items.All(item => item.Ancestors().Contains(container))) continue;
            var text = Text(container);
            foreach (var option in optionTexts) text = text.Replace(option, "");
            text = CleanLabel(text);
            if (text.Length == 0) continue; // 這層只有選項本身，再往外找標題
            return text.Length <= MaxGroupTitleLength ? text : null;
        }
        return null;
    }

    private static string CleanLabel(string text) =>
        Regex.Replace(text.Replace("必填", "").Replace("*", ""), @"\s+", " ").Trim().TrimEnd(':', '：').Trim();

    private static string Text(HtmlNode node) =>
        Regex.Replace(HtmlEntity.DeEntitize(node.InnerText), @"\s+", " ").Trim();

    private static string Attr(string name, string value) =>
        value.Length == 0 ? "" : $" {name}=\"{E(HtmlEntity.DeEntitize(value))}\"";

    private static string E(string value) => WebUtility.HtmlEncode(value);
}
