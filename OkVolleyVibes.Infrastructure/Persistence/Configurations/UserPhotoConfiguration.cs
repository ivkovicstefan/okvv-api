using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OkVolleyVibes.Infrastructure.Identity;

namespace OkVolleyVibes.Infrastructure.Persistence.Configurations;

internal sealed class UserPhotoConfiguration : IEntityTypeConfiguration<UserPhoto>
{
    public void Configure(EntityTypeBuilder<UserPhoto> builder)
    {
        builder.ToTable("UserPhotos");

        builder.HasKey(p => p.UserId);

        builder.Property(p => p.Content).HasColumnType("varbinary(max)").IsRequired();
        builder.Property(p => p.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(p => p.UploadedAtUtc).HasColumnType("datetime2");

        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<UserPhoto>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
