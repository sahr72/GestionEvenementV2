// src/EventManagement.Infrastructure/Persistence/AppDbContext.cs

namespace EventManagement.Infrastructure.Persistence;

using EventManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public sealed class AppDbContext : DbContext
{
    /* Les commentaires n’existent que dans le contexte d’un événement, et on ne les manipule jamais seuls.
     * toutes les modifications passent par Event, ce qui garantit que 
     * les règles métier (validation, statut, etc.) sont respectées.
       Clarté : le DbContext reflète la structure des agrégats racines (Evenements), pas toutes les entités internes.
       On accède et modifie les enfants uniquement via l’agrégat racine.
       On ne sauvegarde pas les enfants directement ; 
       on sauvegarde l’agrégat racine, et EF Core persiste les enfants 
       automatiquement (grâce à DeleteBehavior.Cascade et la navigation).
    Même sans DbSet<Commentaire>, EF Core :
            crée la table comments (via CommentConfiguration),
            gère les clés étrangères,
            suit les changements sur les commentaires via le DbContext qui suit l’Event.
     */
    public DbSet<Evenement> Evenements => Set<Evenement>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}