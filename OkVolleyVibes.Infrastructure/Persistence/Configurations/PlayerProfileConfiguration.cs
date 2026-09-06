using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OkVolleyVibes.Domain.Players;
using OkVolleyVibes.Infrastructure.Identity;

namespace OkVolleyVibes.Infrastructure.Persistence.Configurations;

internal sealed class PlayerProfileConfiguration : IEntityTypeConfiguration<PlayerProfile>
{
    public void Configure(EntityTypeBuilder<PlayerProfile> builder)
    {
        builder.ToTable("PlayerProfiles");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.CreatedAtUtc).HasColumnType("datetime2");

        builder.Property(p => p.MembershipStatus)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(p => p.UserId).IsUnique();

        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<PlayerProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
