using EtheriT.Coker.Core.Models;
using EtheriT.Coker.Web.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EtheriT.Coker.EntityFrameworkCore.Configurations;

public sealed class AiMemoryEntryConfiguration : IEntityTypeConfiguration<AiMemoryEntry>
{
    public void Configure(EntityTypeBuilder<AiMemoryEntry> b)
    {
        b.ToTable("AiMemoryEntries", t => t.HasCheckConstraint("CK_AiMemoryEntries_Scope",
            "([ScopeKind] = 0 AND [WebsiteId] IS NULL AND [UserId] IS NULL) OR ([ScopeKind] = 1 AND [WebsiteId] IS NOT NULL AND [UserId] IS NULL) OR ([ScopeKind] = 2 AND [WebsiteId] IS NOT NULL AND [UserId] IS NOT NULL)"));
        b.HasKey(x => x.Id);
        b.Property(x => x.Feature).HasMaxLength(200).IsRequired();
        b.Property(x => x.Category).HasMaxLength(40).IsUnicode(false).IsRequired();
        b.Property(x => x.ScopeKey).HasMaxLength(64).IsUnicode(false).IsRequired();
        b.Property(x => x.MemoryKey).HasMaxLength(64).IsUnicode(false).IsRequired();
        b.Property(x => x.ContentHash).HasMaxLength(64).IsUnicode(false).IsRequired();
        b.Property(x => x.SourceHash).HasMaxLength(64).IsUnicode(false);
        b.Property(x => x.RuntimeHash).HasMaxLength(64).IsUnicode(false);
        b.Property(x => x.State).HasMaxLength(40).IsUnicode(false).IsRequired();
        b.Property(x => x.Payload).IsRequired();
        b.Property(x => x.ConcurrencyToken).IsConcurrencyToken();
        b.HasIndex(x => new { x.ScopeKey, x.MemoryKey }).IsUnique();
        b.HasIndex(x => new { x.ScopeKind, x.WebsiteId, x.UserId, x.UpdatedAtUtc });
        b.HasOne<Website>().WithMany().HasForeignKey(x => x.WebsiteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class AiMemoryRevisionConfiguration : IEntityTypeConfiguration<AiMemoryRevision>
{
    public void Configure(EntityTypeBuilder<AiMemoryRevision> b)
    {
        b.ToTable("AiMemoryRevisions");
        b.HasKey(x => x.Id);
        b.Property(x => x.ContentHash).HasMaxLength(64).IsUnicode(false).IsRequired();
        b.Property(x => x.SourceHash).HasMaxLength(64).IsUnicode(false);
        b.Property(x => x.RuntimeHash).HasMaxLength(64).IsUnicode(false);
        b.Property(x => x.State).HasMaxLength(40).IsUnicode(false).IsRequired();
        b.Property(x => x.Payload).IsRequired();
        b.HasIndex(x => new { x.EntryId, x.Version }).IsUnique();
        b.HasOne(x => x.Entry).WithMany().HasForeignKey(x => x.EntryId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<User>().WithMany().HasForeignKey(x => x.ActorUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
