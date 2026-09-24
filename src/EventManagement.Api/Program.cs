using EventManagement.Api.Endpoints;
using EventManagement.Api.Middleware;
using EventManagement.Application.Commands;
using EventManagement.Application.Commands.AddComment;
using EventManagement.Application.Commands.CancelEvent;
using EventManagement.Application.Commands.CloseEvent;
using EventManagement.Application.Commands.ConfirmEvent;
using EventManagement.Application.Commands.CreateEvent;
using EventManagement.Application.Commands.DeleteComment;
//using EventManagement.Application.Commands.DeletEvent;
using EventManagement.Application.Commands.UpdateComment;
using EventManagement.Application.Commands.UpdateEvent;
using EventManagement.Application.Interfaces;
using EventManagement.Application.Queries;
using EventManagement.Application.Queries.GetComments;
using EventManagement.Application.Queries.GetEventById;
using EventManagement.Application.Queries.GetEventsPaged;
using EventManagement.Application.Validators;
using EventManagement.Infrastructure.Persistence;
using EventManagement.Infrastructure.QueryServices;
using EventManagement.Infrastructure.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// AppDbContext
// Cela indique à EF Core que les migrations résident dans l’assembly Infrastructure, même si vous lancez la commande depuis le projet Api.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Default"),
        b => b.MigrationsAssembly("EventManagement.Infrastructure")));

// Repositories & Query Services
builder.Services.AddScoped<IEvenementRepository, EvenementRepository>();
builder.Services.AddScoped<IEvenementQueryService, EvenementQueryService>();
builder.Services.AddScoped<ICommentaireQueryService, CommentaireQueryService>();

// Command Handlers — Events
builder.Services.AddScoped<CreateEventCommandHandler>();
builder.Services.AddScoped<UpdateEventCommandHandler>();
//builder.Services.AddScoped<DeleteEventCommandHandler>();
builder.Services.AddScoped<ConfirmEventCommandHandler>();
builder.Services.AddScoped<CancelEventCommandHandler>();
builder.Services.AddScoped<CloseEventCommandHandler>();

// Command Handlers — Comments
builder.Services.AddScoped<AddCommentCommandHandler>();
builder.Services.AddScoped<UpdateCommentCommandHandler>();
builder.Services.AddScoped<DeleteCommentCommandHandler>();

// Query Handlers
builder.Services.AddScoped<GetEventByIdQueryHandler>();
builder.Services.AddScoped<GetEventsPagedQueryHandler>();
builder.Services.AddScoped<GetCommentsQueryHandler>();

// Validators (scan automatique de l'assembly Application)
builder.Services.AddValidatorsFromAssemblyContaining<CreateEventCommandValidator>();

// Gestion centralisée des erreurs avec ProblemDetails
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

// Enable Swagger in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Pipeline
app.UseExceptionHandler();
app.UseStatusCodePages();

// Endpoints
app.MapEventEndpoints();
app.MapCommentEndpoints();

app.Run();