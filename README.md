# ASP.NET Core Vertical Slice Architecture Template

This template provides a lightweight starting point for ASP.NET solutions. While inspired by [Jason Taylor's Clean Architecture](https://github.com/jasontaylordev/CleanArchitecture/tree/main), this project focuses strictly on Vertical Slice Architecture.

## Install the template

```
dotnet new install Vertical.Slice.Architecture
```

## Create a new solution

```
dotnet new vsa-sln -n [SolutionName]
```

## Key Features
- **Target Framework**: .NET SDK 10.0.301.
- **No MediatR dependency**: Uses lightweight reflection at application startup to automatically discover and register IRequestHandler & IDomainEventHandler
- **Native Pipeline Filters**: Because MediatR was removed, MediatR PipelineBehaviors have been replaced with native IEndpointFilter implementations.
- **Persistence**: Configured with SQLite db out of the box ([**recent vulnerability issues with SQLitePCLRaw.lib.e_sqlite3**](https://github.com/advisories/GHSA-2m69-gcr7-jv3q) are suppressed in `Directory.Build.Props`).
- **App Host & SPA (WIP)**: Uses .NET App Host to orchestrate the backend and a React SPA. Note: The React client integration is currently a work in progress.

### Technologies
- [ASP.NET Core 10](https://learn.microsoft.com/en-us/aspnet/core/overview?view=aspnetcore-10.0)
- [Aspire](https://aspire.dev/)
- [React 19](https://react.dev/)
- [EF Core 10](https://learn.microsoft.com/en-us/ef/core/)
- [FluentValidation](https://docs.fluentvalidation.net/en/latest/)
- [Scalar](https://scalar.com/)
- [NUnit](https://nunit.org/), [Shouldly](https://docs.shouldly.org/), [Moq](https://github.com/devlooped/moq) & [Respawn](https://github.com/jbogard/Respawn)

## IEndpointFilter implementations
1. `LoggingFilter:`
   - logs incoming requests in the following format:
   - `"Request: {HttpMethod} {Path}, {@UserId}, {@Request}, {@ResponseStatusCode}"`
2. `ValidationFilter:` 
   - this filter automatically intercepts Minimal API requests, looking for a parameter that implements `IRequest`. If found, it resolves and executes the corresponding `IValidator` implementations.
   - **Important:** For automatic validation to occur, your endpoint method must explicitly include an `IRequest` parameter. Otherwise, validation is skipped and must be handled manually within the endpoint.
   - For HTTP GET/ DELETE: use `[AsParameters]` on the IRequest object
3. `PerformanceFilter:` 
   - logs requests that run for more than *500ms*.

## Structure
Code is grouped by feature rather than technical layer. Everything required to execute a specific feature lives in a single folder inside the Features directory.

```
ClientApp/ # React app
Common/
└──Pipeline/  # Native endpoint filters replacing MediatR behaviors
    ├── ExceptionHandler.cs
    ├── LoggingFilter.cs
    ├── PerformanceFilter.cs
    └── ValidationFilter.cs
    
Features/
└── Examples/
    ├── Commands/  # Commands, handlers and validators
    ├── Events/    # Domain events
    ├── Queries/   # Queries, handlers and validators
    ├── Example.cs # Entity
    ├── ExampleConfiguration.cs  # EF Core Entity Configuration
    ├── ExampleDto.cs            # DTO
    └── ExampleEndpoints.cs      # Minimal API Endpoints

Infrastructure/ # Services with external dependencies
    Database/
```

# Testing

This solution uses **NUnit** as its primary testing framework. Testing is currently focused on the `FunctionalTests` project, which validates application logic by integrating with a database orchestrated by **Aspire** and hosted in memory using `WebApplicationFactory`.

## Application Logic Validation

To test application `IRequest` and `IDomainEvent` implementations and their handlers you can use the `ApplicationTestBase` class which ensures that:
- The database is reset to a clean state before every test.
- A fresh Dependency Injection `IServiceScope` is created.
- The domain event spy is cleared.

## EF Core Entity Configuration Validation
- To test EF Core entity configuration without needing a database connection, inherit from `EntityConfigurationTestBase<TConfiguration, TEntity>`. The class provides helper methods to gain access to the required objects for validation.
