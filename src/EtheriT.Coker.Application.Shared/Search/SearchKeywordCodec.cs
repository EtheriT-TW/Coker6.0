using System;
using System.Text;

namespace EtheriT.Coker.Application.Shared.Search;

/// <summary>Versioned UTF-8 Base64Url route segments. Unmarked and malformed legacy keywords remain literal.</summary>
public static class SearchKeywordCodec
{
    public const string Prefix = "~b64~";
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    public static string Encode(string value) => string.IsNullOrEmpty(value) ? string.Empty :
        Prefix + Convert.ToBase64String(StrictUtf8.GetBytes(value)).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    public static string Decode(string? value)
    {
        if (string.IsNullOrEmpty(value) || !value.StartsWith(Prefix, StringComparison.Ordinal))
            return value ?? string.Empty;
        var payload = value[Prefix.Length..];
        if (payload.Length == 0 || payload.Length % 4 == 1) return value;
        foreach (var c in payload)
            if (!(c >= 'A' && c <= 'Z') && !(c >= 'a' && c <= 'z') &&
                !(c >= '0' && c <= '9') && c != '-' && c != '_') return value;
        try
        {
            var base64 = payload.Replace('-', '+').Replace('_', '/');
            var decoded = StrictUtf8.GetString(Convert.FromBase64String(base64.PadRight((base64.Length + 3) / 4 * 4, '=')));
            return Encode(decoded) == value ? decoded : value;
        }
        catch (FormatException) { return value; }
        catch (DecoderFallbackException) { return value; }
    }
}
