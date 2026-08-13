// src/EventManagement.Infrastructure/Configurations/CommentConfiguration.cs

namespace EventManagement.Infrastructure.Configurations;

using EventManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class CommentConfiguration : IEntityTypeConfiguration<Commentaire>
{
    public void Configure(EntityTypeBuilder<Commentaire> builder)
    {
        builder.ToTable("commentaires");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Description)
            .IsRequired();

        builder.Property(c => c.DateCreationUtc)
            .IsRequired();

        builder.Property(c => c.DateMiseAJourUtc);
    }
}