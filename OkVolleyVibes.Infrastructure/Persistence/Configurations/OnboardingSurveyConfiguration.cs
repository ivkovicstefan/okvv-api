using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OkVolleyVibes.Domain.Onboarding;
using OkVolleyVibes.Infrastructure.Identity;

namespace OkVolleyVibes.Infrastructure.Persistence.Configurations;

internal sealed class OnboardingSurveyConfiguration : IEntityTypeConfiguration<OnboardingSurvey>
{
    public void Configure(EntityTypeBuilder<OnboardingSurvey> builder)
    {
        builder.ToTable("OnboardingSurveys");

        builder.HasKey(s => s.Id);
        builder.HasIndex(s => s.UserId).IsUnique();

        builder.Property(s => s.TrainingHistory).HasMaxLength(2000);
        builder.Property(s => s.Positions).HasConversion<int>();
        builder.Property(s => s.RecreationalExperience).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.ClubInterest).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.CompletedAtUtc).HasColumnType("datetime2");

        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<OnboardingSurvey>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
