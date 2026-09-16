using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Web.Platform.Models;

/// <summary>
/// 選填的 Email。空字串或空白視為未填，直接通過；有值時才檢查格式。
/// 內建的 [EmailAddress] 只放行 null，前端表單送來的空字串 "" 會被判為格式錯誤。
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class OptionalEmailAttribute : ValidationAttribute
{
    private static readonly EmailAddressAttribute EmailFormat = new();

    public override bool IsValid(object? value) =>
        value is not string text ||
        string.IsNullOrWhiteSpace(text) ||
        EmailFormat.IsValid(text.Trim());
}