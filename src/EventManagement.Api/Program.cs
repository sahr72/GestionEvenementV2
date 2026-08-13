using EventManagement.Application.Commands;
using EventManagement.Application.Interfaces;
using EventManagement.Infrastructure.Persistence;
using EventManagement.Infrastructure.QueryServices;
using EventManagement.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//AppDbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

//Repositories
builder.Services.AddScoped<IEvenementRepository, EvenementRepository>();
builder.Services.AddScoped<IEvenementQueryService, EvenementQueryService>();
//Handlers
builder.Services.AddScoped<CreateEventCommandHandler>();

var app = builder.Build();

// Enable Swagger in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//app.MapGet("/", () => "Hello World!");
var events = app.MapGroup("/api/events");

events.MapPost("/", async (
    CreateEventCommand request,
    CreateEventCommandHandler handler,
    CancellationToken ct) =>
{
    var command = new CreateEventCommand(request.Titre, request.Description, request.PersonneImpliquee);
    var id = await handler.Handle(command, ct);
    return Results.Created($"/api/events/{id}", new { id });
});

events.MapGet("/{id:guid}", async (
    Guid id,
    IEvenementQueryService queryService,
    CancellationToken ct) =>
{
    var result = await queryService.GetEventByIdAsync(id, ct);
    return result is null ? Results.NotFound() : Results.Ok(result);
});
app.Run();