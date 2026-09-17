using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;

namespace EtheriT.Coker.Web.Platform.Services;

/// <summary>
/// 網域密碼的可逆加解密。purpose 固定為 Platform.DomainPassword，
/// 金鑰沿用 BackofficeAuthentication:DataProtectionKeysPath 那一組。
/// </summary>
public sealed class PlatformDomainPasswordProtector
{
    private const string ProtectorPurpose = "Platform.DomainPassword";

    private readonly IDataProtector _protector;

    public PlatformDomainPasswordProtector(IDataProtectionProvider dataProtectionProvider)
    {
        _protector = dataProtectionProvider.CreateProtector(ProtectorPurpose);
    }

    public string? Protect(string? plainText) =>
        string.IsNullOrWhiteSpace(plainText) ? null : _protector.Protect(plainText.Trim());

    /// <summary>金鑰目錄被重建時解不開是預期情境，回 Unreadable 而不是丟例外。</summary>
    public DomainPasswordReadResult Unprotect(string? cipherText)
    {
        if (string.IsNullOrWhiteSpace(cipherText))
            return DomainPasswordReadResult.Empty;

        try
        {
            return new DomainPasswordReadResult(DomainPasswordState.Ok, _protector.Unprotect(cipherText));
        }
        catch (CryptographicException)
        {
            return DomainPasswordReadResult.Unreadable;
        }
    }
}

public static class DomainPasswordState
{
    public const string Ok = "Ok";
    public const string Empty = "Empty";
    public const string Unreadable = "Unreadable";
}

public sealed record DomainPasswordReadResult(string State, string? Password)
{
    public static readonly DomainPasswordReadResult Empty = new(DomainPasswordState.Empty, null);
    public static readonly DomainPasswordReadResult Unreadable = new(DomainPasswordState.Unreadable, null);
}