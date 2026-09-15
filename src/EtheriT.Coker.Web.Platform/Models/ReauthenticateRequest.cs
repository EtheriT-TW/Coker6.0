using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Web.Platform.Models;

public sealed class ReauthenticateRequest
{
    [Required]
    public string Ticket { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}
