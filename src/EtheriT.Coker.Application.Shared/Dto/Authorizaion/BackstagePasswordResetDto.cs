namespace EtheriT.Coker.Application.Shared.Dto.Authorizaion
{
    public class BackstagePasswordResetRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string WebsiteLink { get; set; } = string.Empty;
    }

    public class BackstageAccountAvailabilityDto
    {
        public Guid ForgetID { get; set; }
        public string Account { get; set; } = string.Empty;
    }

    public class BackstagePasswordResetDto
    {
        public Guid ForgetID { get; set; }
        public string? Account { get; set; }
        public string Password { get; set; } = string.Empty;
        public string PasswordConfirm { get; set; } = string.Empty;
    }
}
