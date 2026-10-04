// src/EventManagement.Infrastructure/Configurations/EventConfiguration.cs

namespace EventManagement.Infrastructure.Configurations;

using EventManagement.Domain.Entities;
using EventManagement.Domain.SmartEnums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Logging;

public sealed class EventConfiguration : IEntityTypeConfiguration<Evenement>
{
    public void Configure(EntityTypeBuilder<Evenement> builder)
    {
        builder.ToTable("evenements");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Titre)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Description)
            .IsRequired();

        builder.Property(e => e.PersonneImpliquee)
            .IsRequired();
        
        builder.Property(e => e.DateCreationUtc)
            .IsRequired();
        
        builder.Property(e => e.DateMiseAjourUtc);
        
        builder.Property(e => e.ProprietaireId).IsRequired();

        builder.Property(e => e.Statut)
            .HasConversion(
                status => status.Code,
                code => EventStatus.FromCode(code))
            .HasMaxLength(20)
            .IsRequired();

        builder.HasMany(e => e.Commentaires)
            .WithOne()
            .HasForeignKey(c => c.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(e => e.Commentaires)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}