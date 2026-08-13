# Script PowerShell pour générer tous les objets vides du template Avancé
# Usage : .\Generate-Template.ps1

param(
    [string]$RootPath = "."
)

function Ensure-Directory($path) {
    if (-not (Test-Path $path)) {
        New-Item -ItemType Directory -Path $path -Force | Out-Null
    }
}

function Write-EmptyFile($path) {
    $directory = Split-Path $path -Parent
    Ensure-Directory $directory
    if (-not (Test-Path $path)) {
        New-Item -ItemType File -Path $path -Force | Out-Null
    }
}

Write-Host "Génération du template Avancé dans '$RootPath'..." -ForegroundColor Green

# -----------------------------------------------------------------------------
# 1. EventManagement.Domain
# -----------------------------------------------------------------------------

$domainPath = Join-Path $RootPath "src/EventManagement.Domain"

# Common
Write-EmptyFile (Join-Path $domainPath "Common/DomainException.cs")
Write-EmptyFile (Join-Path $domainPath "Common/IDomainEvent.cs")

# Entities
Write-EmptyFile (Join-Path $domainPath "Entities/Event.cs")
Write-EmptyFile (Join-Path $domainPath "Entities/Comment.cs")

# ValueObjects
Write-EmptyFile (Join-Path $domainPath "ValueObjects/EventTitle.cs")
Write-EmptyFile (Join-Path $domainPath "ValueObjects/EventDescription.cs")
Write-EmptyFile (Join-Path $domainPath "ValueObjects/PersonName.cs")

# SmartEnums
Write-EmptyFile (Join-Path $domainPath "SmartEnums/EventStatus.cs")

# Specifications
Write-EmptyFile (Join-Path $domainPath "Specifications/DomainSpecification{T}.cs")
Write-EmptyFile (Join-Path $domainPath "Specifications/EventPublishableSpecification.cs")

# Events
Write-EmptyFile (Join-Path $domainPath "Events/EventConfirmed.cs")
Write-EmptyFile (Join-Path $domainPath "Events/EventCancelled.cs")
Write-EmptyFile (Join-Path $domainPath "Events/EventClosed.cs")
Write-EmptyFile (Join-Path $domainPath "Events/CommentAdded.cs")

# -----------------------------------------------------------------------------
# 2. EventManagement.Application
# -----------------------------------------------------------------------------

$appPath = Join-Path $RootPath "src/EventManagement.Application"

# Common
Write-EmptyFile (Join-Path $appPath "Common/PagedResult{T}.cs")
Write-EmptyFile (Join-Path $appPath "Common/TimeProviderExtensions.cs")

# Commands
Write-EmptyFile (Join-Path $appPath "Commands/CreateEvent/CreateEventCommand.cs")
Write-EmptyFile (Join-Path $appPath "Commands/CreateEvent/CreateEventCommandHandler.cs")

Write-EmptyFile (Join-Path $appPath "Commands/UpdateEvent/UpdateEventCommand.cs")
Write-EmptyFile (Join-Path $appPath "Commands/UpdateEvent/UpdateEventCommandHandler.cs")

Write-EmptyFile (Join-Path $appPath "Commands/DeleteEvent/DeleteEventCommand.cs")
Write-EmptyFile (Join-Path $appPath "Commands/DeleteEvent/DeleteEventCommandHandler.cs")

Write-EmptyFile (Join-Path $appPath "Commands/AddComment/AddCommentCommand.cs")
Write-EmptyFile (Join-Path $appPath "Commands/AddComment/AddCommentCommandHandler.cs")

Write-EmptyFile (Join-Path $appPath "Commands/UpdateComment/UpdateCommentCommand.cs")
Write-EmptyFile (Join-Path $appPath "Commands/UpdateComment/UpdateCommentCommandHandler.cs")

Write-EmptyFile (Join-Path $appPath "Commands/DeleteComment/DeleteCommentCommand.cs")
Write-EmptyFile (Join-Path $appPath "Commands/DeleteComment/DeleteCommentCommandHandler.cs")

Write-EmptyFile (Join-Path $appPath "Commands/ConfirmEvent/ConfirmEventCommand.cs")
Write-EmptyFile (Join-Path $appPath "Commands/ConfirmEvent/ConfirmEventCommandHandler.cs")

Write-EmptyFile (Join-Path $appPath "Commands/CancelEvent/CancelEventCommand.cs")
Write-EmptyFile (Join-Path $appPath "Commands/CancelEvent/CancelEventCommandHandler.cs")

Write-EmptyFile (Join-Path $appPath "Commands/CloseEvent/CloseEventCommand.cs")
Write-EmptyFile (Join-Path $appPath "Commands/CloseEvent/CloseEventCommandHandler.cs")

# Queries
Write-EmptyFile (Join-Path $appPath "Queries/GetEventById/GetEventByIdQuery.cs")
Write-EmptyFile (Join-Path $appPath "Queries/GetEventById/GetEventByIdQueryHandler.cs")

Write-EmptyFile (Join-Path $appPath "Queries/GetEventsPaged/GetEventsPagedQuery.cs")
Write-EmptyFile (Join-Path $appPath "Queries/GetEventsPaged/GetEventsPagedQueryHandler.cs")

Write-EmptyFile (Join-Path $appPath "Queries/GetComments/GetCommentsQuery.cs")
Write-EmptyFile (Join-Path $appPath "Queries/GetComments/GetCommentsQueryHandler.cs")

# ReadModels
Write-EmptyFile (Join-Path $appPath "ReadModels/EventReadModel.cs")
Write-EmptyFile (Join-Path $appPath "ReadModels/EventDetailsReadModel.cs")
Write-EmptyFile (Join-Path $appPath "ReadModels/EventListItemReadModel.cs")
Write-EmptyFile (Join-Path $appPath "ReadModels/CommentReadModel.cs")
Write-EmptyFile (Join-Path $appPath "ReadModels/CommentListReadModel.cs")

# DTOs
Write-EmptyFile (Join-Path $appPath "DTOs/CreateEventRequest.cs")
Write-EmptyFile (Join-Path $appPath "DTOs/UpdateEventRequest.cs")
Write-EmptyFile (Join-Path $appPath "DTOs/AddCommentRequest.cs")
Write-EmptyFile (Join-Path $appPath "DTOs/UpdateCommentRequest.cs")
Write-EmptyFile (Join-Path $appPath "DTOs/ConfirmEventRequest.cs")

# Interfaces
Write-EmptyFile (Join-Path $appPath "Interfaces/IEventRepository.cs")
Write-EmptyFile (Join-Path $appPath "Interfaces/ICommentRepository.cs")
Write-EmptyFile (Join-Path $appPath "Interfaces/IEventQueryService.cs")
Write-EmptyFile (Join-Path $appPath "Interfaces/ICommentQueryService.cs")

# Validators
Write-EmptyFile (Join-Path $appPath "Validators/CreateEventCommandValidator.cs")
Write-EmptyFile (Join-Path $appPath "Validators/UpdateEventCommandValidator.cs")
Write-EmptyFile (Join-Path $appPath "Validators/AddCommentCommandValidator.cs")
Write-EmptyFile (Join-Path $appPath "Validators/UpdateCommentCommandValidator.cs")
Write-EmptyFile (Join-Path $appPath "Validators/ConfirmEventCommandValidator.cs")
Write-EmptyFile (Join-Path $appPath "Validators/CancelEventCommandValidator.cs")
Write-EmptyFile (Join-Path $appPath "Validators/CloseEventCommandValidator.cs")
Write-EmptyFile (Join-Path $appPath "Validators/GetEventsPagedQueryValidator.cs")
Write-EmptyFile (Join-Path $appPath "Validators/GetCommentsQueryValidator.cs")

# Exceptions
Write-EmptyFile (Join-Path $appPath "Exceptions/NotFoundException.cs")

# -----------------------------------------------------------------------------
# 3. EventManagement.Infrastructure
# -----------------------------------------------------------------------------

$infraPath = Join-Path $RootPath "src/EventManagement.Infrastructure"

# Persistence
Write-EmptyFile (Join-Path $infraPath "Persistence/AppDbContext.cs")

# Configurations
Write-EmptyFile (Join-Path $infraPath "Configurations/EventConfiguration.cs")
Write-EmptyFile (Join-Path $infraPath "Configurations/CommentConfiguration.cs")

# Repositories
Write-EmptyFile (Join-Path $infraPath "Repositories/EventRepository.cs")
Write-EmptyFile (Join-Path $infraPath "Repositories/CommentRepository.cs")

# QueryServices
Write-EmptyFile (Join-Path $infraPath "QueryServices/EventQueryService.cs")
Write-EmptyFile (Join-Path $infraPath "QueryServices/CommentQueryService.cs")

# DependencyInjection
Write-EmptyFile (Join-Path $infraPath "DependencyInjection/ServiceCollectionExtensions.cs")

# Migrations (dossier seulement, fichiers générés par EF Core)
Ensure-Directory (Join-Path $infraPath "Migrations")

# -----------------------------------------------------------------------------
# 4. EventManagement.Api
# -----------------------------------------------------------------------------

$apiPath = Join-Path $RootPath "src/EventManagement.Api"

# Endpoints
Write-EmptyFile (Join-Path $apiPath "Endpoints/EventsEndpoints.cs")
Write-EmptyFile (Join-Path $apiPath "Endpoints/CommentsEndpoints.cs")

# Middleware
Write-EmptyFile (Join-Path $apiPath "Middleware/GlobalExceptionHandler.cs")

# Extensions
Write-EmptyFile (Join-Path $apiPath "Extensions/ServiceCollectionExtensions.cs")

# Program.cs
Write-EmptyFile (Join-Path $apiPath "Program.cs")

# -----------------------------------------------------------------------------
# 5. Tests (optionnel)
# -----------------------------------------------------------------------------

$testsPath = Join-Path $RootPath "tests"

# Domain.Tests
Write-EmptyFile (Join-Path $testsPath "EventManagement.Domain.Tests/EventTests.cs")
Write-EmptyFile (Join-Path $testsPath "EventManagement.Domain.Tests/CommentTests.cs")
Write-EmptyFile (Join-Path $testsPath "EventManagement.Domain.Tests/EventTitleTests.cs")
Write-EmptyFile (Join-Path $testsPath "EventManagement.Domain.Tests/EventStatusTests.cs")
Write-EmptyFile (Join-Path $testsPath "EventManagement.Domain.Tests/EventPublishableSpecificationTests.cs")

# Application.Tests
Write-EmptyFile (Join-Path $testsPath "EventManagement.Application.Tests/Commands/CreateEventCommandHandlerTests.cs")
Write-EmptyFile (Join-Path $testsPath "EventManagement.Application.Tests/Commands/UpdateEventCommandHandlerTests.cs")
Write-EmptyFile (Join-Path $testsPath "EventManagement.Application.Tests/Commands/DeleteEventCommandHandlerTests.cs")
Write-EmptyFile (Join-Path $testsPath "EventManagement.Application.Tests/Commands/AddCommentCommandHandlerTests.cs")
Write-EmptyFile (Join-Path $testsPath "EventManagement.Application.Tests/Commands/UpdateCommentCommandHandlerTests.cs")
Write-EmptyFile (Join-Path $testsPath "EventManagement.Application.Tests/Commands/DeleteCommentCommandHandlerTests.cs")
Write-EmptyFile (Join-Path $testsPath "EventManagement.Application.Tests/Commands/ConfirmEventCommandHandlerTests.cs")
Write-EmptyFile (Join-Path $testsPath "EventManagement.Application.Tests/Commands/CancelEventCommandHandlerTests.cs")
Write-EmptyFile (Join-Path $testsPath "EventManagement.Application.Tests/Commands/CloseEventCommandHandlerTests.cs")

Write-EmptyFile (Join-Path $testsPath "EventManagement.Application.Tests/Queries/GetEventByIdQueryHandlerTests.cs")
Write-EmptyFile (Join-Path $testsPath "EventManagement.Application.Tests/Queries/GetEventsPagedQueryHandlerTests.cs")
Write-EmptyFile (Join-Path $testsPath "EventManagement.Application.Tests/Queries/GetCommentsQueryHandlerTests.cs")

Write-EmptyFile (Join-Path $testsPath "EventManagement.Application.Tests/Validators/CreateEventCommandValidatorTests.cs")
Write-EmptyFile (Join-Path $testsPath "EventManagement.Application.Tests/Validators/UpdateEventCommandValidatorTests.cs")
Write-EmptyFile (Join-Path $testsPath "EventManagement.Application.Tests/Validators/AddCommentCommandValidatorTests.cs")
Write-EmptyFile (Join-Path $testsPath "EventManagement.Application.Tests/Validators/UpdateCommentCommandValidatorTests.cs")
Write-EmptyFile (Join-Path $testsPath "EventManagement.Application.Tests/Validators/ConfirmEventCommandValidatorTests.cs")
Write-EmptyFile (Join-Path $testsPath "EventManagement.Application.Tests/Validators/CancelEventCommandValidatorTests.cs")
Write-EmptyFile (Join-Path $testsPath "EventManagement.Application.Tests/Validators/CloseEventCommandValidatorTests.cs")
Write-EmptyFile (Join-Path $testsPath "EventManagement.Application.Tests/Validators/GetEventsPagedQueryValidatorTests.cs")
Write-EmptyFile (Join-Path $testsPath "EventManagement.Application.Tests/Validators/GetCommentsQueryValidatorTests.cs")

# Infrastructure.Tests
Write-EmptyFile (Join-Path $testsPath "EventManagement.Infrastructure.Tests/Repositories/EventRepositoryTests.cs")
Write-EmptyFile (Join-Path $testsPath "EventManagement.Infrastructure.Tests/Repositories/CommentRepositoryTests.cs")
Write-EmptyFile (Join-Path $testsPath "EventManagement.Infrastructure.Tests/QueryServices/EventQueryServiceTests.cs")
Write-EmptyFile (Join-Path $testsPath "EventManagement.Infrastructure.Tests/QueryServices/CommentQueryServiceTests.cs")

# Api.Tests
Write-EmptyFile (Join-Path $testsPath "EventManagement.Api.Tests/EventsEndpointsTests.cs")
Write-EmptyFile (Join-Path $testsPath "EventManagement.Api.Tests/CommentsEndpointsTests.cs")
Write-EmptyFile (Join-Path $testsPath "EventManagement.Api.Tests/GlobalExceptionHandlerTests.cs")

Write-Host "Template Avancé généré avec succès." -ForegroundColor Green