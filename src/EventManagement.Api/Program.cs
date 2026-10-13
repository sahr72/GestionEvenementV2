using EventManagement.Api.Endpoints;
using EventManagement.Api.Middleware;
using EventManagement.Api.Security.Filters;
using EventManagement.Api.Security.Handlers;
using EventManagement.Api.Security.Requirements;
using EventManagement.Api.Security.TokenIssuance;
using EventManagement.Application.Authorization;
using EventManagement.Application.Commands.AddComment;
using EventManagement.Application.Commands.CancelEvent;
using EventManagement.Application.Commands.CloseEvent;
using EventManagement.Application.Commands.ConfirmEvent;
using EventManagement.Application.Commands.CreateEvent;
using EventManagement.Application.Commands.DeleteComment;
using EventManagement.Application.Commands.UpdateComment;
using EventManagement.Application.Commands.UpdateEvent;
using EventManagement.Application.Interfaces;
using EventManagement.Application.Queries.GetComments;
using EventManagement.Application.Queries.GetEventById;
using EventManagement.Application.Queries.GetEventsPaged;
using EventManagement.Application.Validators;
using EventManagement.Infrastructure.Persistence;
using EventManagement.Infrastructure.QueryServices;
using EventManagement.Infrastructure.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------------
// 1. Authentification & Autorisation (JWT + RBAC)
// -----------------------------------------------------------------------------
var jwt = builder.Configuration.GetSection("Jwt");
var signingKey = jwt["SigningKey"]
    ?? throw new InvalidOperationException("Jwt:SigningKey n'est pas configurée.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ClockSkew = TimeSpan.FromSeconds(30),
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.NameIdentifier
        };
    });

builder.Services.AddAuthorization(options =>
{
    // --- Policies de PERMISSION (endpoint-level) ---
    options.AddPolicy(Policies.Events.Read, p => p.Requirements.Add(new PermissionRequirement(Permissions.Events.Read)));
    options.AddPolicy(Policies.Events.Create, p => p.Requirements.Add(new PermissionRequirement(Permissions.Events.Create)));
    options.AddPolicy(Policies.Events.Update, p => p.Requirements.Add(new PermissionRequirement(Permissions.Events.Update)));
    options.AddPolicy(Policies.Events.Confirm, p => p.Requirements.Add(new PermissionRequirement(Permissions.Events.Confirm)));
    options.AddPolicy(Policies.Events.Cancel, p => p.Requirements.Add(new PermissionRequirement(Permissions.Events.Cancel)));
    options.AddPolicy(Policies.Events.Close, p => p.Requirements.Add(new PermissionRequirement(Permissions.Events.Close)));
    options.AddPolicy(Policies.Comments.Read, p => p.Requirements.Add(new PermissionRequirement(Permissions.Comments.Read)));
    options.AddPolicy(Policies.Comments.Create, p => p.Requirements.Add(new PermissionRequirement(Permissions.Comments.Create)));
    options.AddPolicy(Policies.Comments.Update, p => p.Requirements.Add(new PermissionRequirement(Permissions.Comments.Update)));
    options.AddPolicy(Policies.Comments.Delete, p => p.Requirements.Add(new PermissionRequirement(Permissions.Comments.Delete)));

    // --- Policies de RESSOURCE (usage avec AuthorizeAsync / Filtres) ---
    options.AddPolicy(ResourceOperations.Events.Manage, p => p.Requirements.Add(new EvenementResourceRequirement()));
    options.AddPolicy(ResourceOperations.Comments.Manage, p => p.Requirements.Add(new CommentaireResourceRequirement()));
});

// Services d'autorisation légers
builder.Services.AddScoped<IEvenementAuthorizationReadService, EvenementAuthorizationReadService>();
builder.Services.AddScoped<ICommentaireAuthorizationReadService, CommentaireAuthorizationReadService>();

// Handlers d'autorisation

builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, EvenementResourceAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, CommentaireResourceAuthorizationHandler>();

// Filtres d'endpoints
builder.Services.AddScoped<EvenementResourceAuthorizationFilter>();
builder.Services.AddScoped<CommentaireResourceAuthorizationFilter>();

// Fabrique de jetons pour tests / dev
builder.Services.AddSingleton<JwtTokenFactory>();

// -----------------------------------------------------------------------------
// 2. Swagger / OpenAPI (avec support Bearer JWT)
// -----------------------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EventManagement API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Format : Bearer {votre_token_jwt}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// -----------------------------------------------------------------------------
// 3. Persistance & Services applicatifs
// -----------------------------------------------------------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Default"),
        b => b.MigrationsAssembly("EventManagement.Infrastructure")));

builder.Services.AddScoped<IEvenementRepository, EvenementRepository>();
builder.Services.AddScoped<IEvenementQueryService, EvenementQueryService>();
builder.Services.AddScoped<ICommentaireQueryService, CommentaireQueryService>();

// Command Handlers
builder.Services.AddScoped<CreateEventCommandHandler>();
builder.Services.AddScoped<UpdateEventCommandHandler>();
builder.Services.AddScoped<ConfirmEventCommandHandler>();
builder.Services.AddScoped<CancelEventCommandHandler>();
builder.Services.AddScoped<CloseEventCommandHandler>();
builder.Services.AddScoped<AddCommentCommandHandler>();
builder.Services.AddScoped<UpdateCommentCommandHandler>();
builder.Services.AddScoped<DeleteCommentCommandHandler>();

// Query Handlers
builder.Services.AddScoped<GetEventByIdQueryHandler>();
builder.Services.AddScoped<GetEventsPagedQueryHandler>();
builder.Services.AddScoped<GetCommentsQueryHandler>();

// Validation
builder.Services.AddValidatorsFromAssemblyContaining<CreateEventCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CancelEventCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CloseEventCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ConfirmEventCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateEventCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<GetEventsPagedQueryValidator>();

builder.Services.AddValidatorsFromAssemblyContaining<AddCommentCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<DeleteCommentCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<GetCommentsQueryValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateCommentCommandValidator>();



// Gestion d'exceptions ProblemDetails
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

// -----------------------------------------------------------------------------
// 4. Pipeline HTTP (Ordre strict)
// -----------------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 1) Capture globale des erreurs (doit être en tout premier)
app.UseExceptionHandler();

// 2) Redirection HTTPS
app.UseHttpsRedirection();

// 3) Codes d'état ProblemDetails pour les réponses sans corps
app.UseStatusCodePages();

// 4) Sécurité : Authentification puis Autorisation (AVANT les endpoints)
app.UseAuthentication();
app.UseAuthorization();

// -----------------------------------------------------------------------------
// 5. Enregistrement des Endpoints
// -----------------------------------------------------------------------------
app.MapEventEndpoints();
app.MapCommentEndpoints();
app.MapUserEndpoints();

app.Run();