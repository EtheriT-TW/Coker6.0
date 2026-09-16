using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Application.Member
{
    public sealed class FrontMemberEmailValidationResult
    {
        public string NormalizedEmail { get; init; } = string.Empty;
        public FrontUser? ConflictingUser { get; init; }
    }

    public sealed class FrontMemberEmailValidationService
    {
        private readonly CokerDbContext db;

        public FrontMemberEmailValidationService(CokerDbContext db)
        {
            this.db = db;
        }

        public async Task<FrontMemberEmailValidationResult> ValidateAsync(
            long websiteId,
            string? email,
            long? excludeFrontUserId = null)
        {
            var normalizedEmail = email?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(normalizedEmail))
                throw new Exception("請輸入電子郵件");

            if (!new EmailAddressAttribute().IsValid(normalizedEmail))
                throw new Exception("電子郵件格式不正確");

            var normalizedEmailUpper = normalizedEmail.ToUpper();
            var conflictingUser = await (
                from user in db.FrontUsers
                join map in db.MappingFrontUserAndWebsite on user.Id equals map.FK_UserId
                where !user.IsDeleted
                   && !map.IsDeleted
                   && map.FK_WebsiteId == websiteId
                   && (!excludeFrontUserId.HasValue || user.Id != excludeFrontUserId.Value)
                   && user.Email != null
                   && user.Email!.Trim().ToUpper() == normalizedEmailUpper
                orderby user.Id
                select user
            ).FirstOrDefaultAsync();

            return new FrontMemberEmailValidationResult
            {
                NormalizedEmail = normalizedEmail,
                ConflictingUser = conflictingUser
            };
        }
    }
}
