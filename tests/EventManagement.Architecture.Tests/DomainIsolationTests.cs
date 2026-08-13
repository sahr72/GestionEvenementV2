using System.Reflection;


namespace EventManagement.Architecture.Tests
{
    public class DomainIsolationTests
    {
        private static readonly Assembly DomainAssembly =
                typeof(EventManagement.Domain.Entities.Evenement).Assembly;
        //en attendant de trouver une solution pour récupérer l'assembly du domaine
        //même si les entités métier(Evenement, Commentaire) n'existent pas encore, le test compile.
        //private static readonly Assembly DomainAssembly =
        //       typeof(EventManagement.Domain.AssemblyReference).Assembly;

        private static readonly string[] ForbiddenProjectReferences =
        {
        "EventManagement.Application",
        "EventManagement.Infrastructure",
        "EventManagement.Api"
    };

        private static readonly string[] ForbiddenPackagePrefixes =
        {
        "Microsoft.EntityFrameworkCore",
        "Npgsql.EntityFrameworkCore.PostgreSQL",
        "Microsoft.AspNetCore",
        "FluentValidation",
        "MediatR",
        "AutoMapper",
        "Dapper",
        "MassTransit",
        "RabbitMQ",
        "Kafka",
        "Azure.",
        "AWS.",
        "Google.Cloud.",
        "MongoDB.",
        "StackExchange.Redis"
        // Ajouter d’autres selon le contexte
    };

        [Fact]
        public void Domain_Should_Not_Reference_Other_Projects()
        {
            var referencedAssemblies = DomainAssembly.GetReferencedAssemblies();

            var forbiddenReferences = referencedAssemblies
                .Where(asm => ForbiddenProjectReferences.Any(name => asm.Name?.StartsWith(name) == true))
                .ToList();

            Assert.Empty(forbiddenReferences);
        }

        [Fact]
        public void Domain_Should_Not_Reference_Technical_Packages()
        {
            var referencedAssemblies = DomainAssembly.GetReferencedAssemblies();

            var technicalReferences = referencedAssemblies
                .Where(asm => ForbiddenPackagePrefixes.Any(prefix => asm.Name?.StartsWith(prefix) == true))
                .ToList();

            Assert.Empty(technicalReferences);
        }

        [Fact]
        public void Domain_Should_Only_Reference_Standard_Or_Allowed_Packages()
        {
            var referencedAssemblies = DomainAssembly.GetReferencedAssemblies();

            // Assemblies autorisés (exemples)
            var allowedPrefixes = new[]
            {
            "System",
            "Microsoft.Extensions.Primitives", // si utilisé pour les collections
            "netstandard",
            "mscorlib"
        };

            var suspiciousReferences = referencedAssemblies
                .Where(asm => asm.Name != null)
                .Where(asm => !allowedPrefixes.Any(prefix => asm.Name.StartsWith(prefix)))
                .Where(asm => !asm.Name.StartsWith("EventManagement.Domain")) // soi-même
                .ToList();

            // Si des références suspectes existent, on les liste pour faciliter le debug
            if (suspiciousReferences.Any())
            {
                var names = string.Join(", ", suspiciousReferences.Select(a => a.Name));
                Assert.Fail($"Domain référence des assemblies suspects : {names}");
            }
        }
    }
}