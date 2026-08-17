# ASP.NET Core Vertical Slice Architecture Template

This template provides a lightweight starting point for ASP.NET solutions with minimal dependencies. While inspired by [Jason Taylor's Clean Architecture](https://github.com/jasontaylordev/CleanArchitecture/tree/main), this project focuses strictly on Vertical Slice Architecture.

## Install the template

```
dotnet new install Vertical.Slice.Architecture
```

## Create a new solution

```
dotnet new vsa-sln -n [SolutionName]
```

| Options               | Values      | Default | Description                               |
|-----------------------|-------------|---------|-------------------------------------------|
| --examples, -e        | true, false | false   | Includes example implementations and tests |
| --template-tests, -tt | true, false | false   | Includes TemplateTest project             |

## Featues
- **Target Framework**: [.NET SDK 10.0.400](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- **No [MediatR](https://github.com/LuckyPennySoftware/MediatR) dependency**: Uses lightweight reflection at application startup to automatically discover and register [`IRequestHandler`](./src/VsaTemplate/Common/Interfaces/Features/IRequestHandler.cs) & [`IDomainEventHandler`](./src/VsaTemplate/Common/Interfaces/Features/IDomainEventHandler.cs) implementations
- **Native Pipeline Filters**: Because MediatR was removed, `PipelineBehavior` has been replaced with native IEndpointFilter implementations
- **Persistence**: Configured with SQLite db out of the box
- **App Host**: Uses [Aspire](https://aspire.dev/) to orchestrate the backend and the SQLite db

### `IEndpointFilter` implementations
1. [`LoggingFilter`](./src/VsaTemplate/Common/Pipeline/LoggingFilter.cs)
   - Logs incoming requests in the following format:
   - `Request: {HttpMethod} {Path}, {@UserId}, {@Request}, {@ResponseStatusCode}`
   - This is a simple developer implementation rather something that should be used in production  
2. [`ValidationFilter`](./src/VsaTemplate/Common/Pipeline/ValidationFilter.cs)
    - The filter looks for a parameter that implements [`IRequest`](./src/VsaTemplate/Common/Interfaces/Features/IRequest.cs). If found, it resolves and executes the corresponding `IValidator` implementations
    - **Important:** For automatic validation to occur, your endpoint method must explicitly include an [`IRequest`](./src/VsaTemplate/Common/Interfaces/Features/IRequest.cs) parameter. Otherwise, validation is skipped and must be handled manually within the endpoint
    - For HTTP GET/ DELETE: use `[AsParameters]` on the [`IRequest`](./src/VsaTemplate/Common/Interfaces/Features/IRequest.cs) object
3. [`PerformanceFilter`](./src/VsaTemplate/Common/Pipeline/PerformanceFilter.cs)
    - Logs requests that run for more than *500ms*

### [`IRequest`](./src/VsaTemplate/Common/Interfaces/Features/IRequest.cs)
- Marker interface for [`ValidationFilter`](./src/VsaTemplate/Common/Pipeline/ValidationFilter.cs)
- It exists so that incoming HTTP payloads can be validated automatically
- **Important:** For automatic validation to occur, your endpoint method must explicitly include an [`IRequest`](./src/VsaTemplate/Common/Interfaces/Features/IRequest.cs) parameter. Otherwise, validation is skipped and must be handled manually within the endpoint

### [`IRequestHandler`](./src/VsaTemplate/Common/Interfaces/Features/IRequestHandler.cs)
- Marker interface for dependency injection registration
- It defines no methods for return and parameter type flexibility and simplicity (e.g: consuming a type such as `Guid` - which requires no manual validation - should not require creating a new [`IRequest`](./src/VsaTemplate/Common/Interfaces/Features/IRequest.cs) type)

### [`IDomainEvent`](./src/VsaTemplate/Common/Interfaces/Features/IDomainEvent.cs)
- Marker interface for domain events (equivalent to MediatR's `INotification`)
- Created events should be passed to [`BaseEntity`](./src/VsaTemplate/Common/BaseClasses/BaseEntity.cs)'s `AddDomainEvent` method which will be used to publish the events via the [`DispatchDomainEventInterceptor`](./src/VsaTemplate/Infrastructure/Database/Interceptors/DispatchDomainEventInterceptor.cs)

### [`IDomainEventHandler`](./src/VsaTemplate/Common/Interfaces/Features/IDomainEventHandler.cs)
- Defines the Handle method that handles the passed [`IDomainEvent`](./src/VsaTemplate/Common/Interfaces/Features/IDomainEvent.cs)

### [`IEndpointGroup`](./src/VsaTemplate/Common/Interfaces/Features/IEndpointGroup.cs)
- Interface to automatically register Minimal API endpoints with reflection (see: [`EndpointRouteBuilderExtensions`](./src/VsaTemplate/Common/Extensions/EndpointRouteBuilderExtensions.cs))

## Technologies
- [ASP.NET Core 10](https://learn.microsoft.com/en-us/aspnet/core/overview?view=aspnetcore-10.0)
- [Aspire](https://aspire.dev/)
- [EF Core 10](https://learn.microsoft.com/en-us/ef/core/)
- [FluentValidation](https://docs.fluentvalidation.net/en/latest/)
- [Scalar](https://scalar.com/)
- [NUnit](https://nunit.org/), [Shouldly](https://docs.shouldly.org/), [Moq](https://github.com/devlooped/moq) & [Respawn](https://github.com/jbogard/Respawn)

## Structure
Code is grouped by feature rather than technical layer. Everything required to execute a specific feature lives in a single folder inside the Features directory.

```
Features/
└── Examples/
    ├── Commands/                # Commands, handlers and validators
    ├── Events/                  # Domain events
    ├── Queries/                 # Queries, handlers and validators
    ├── Example.cs               # Entity
    ├── ExampleConfiguration.cs  # EF Core Entity Configuration
    ├── ExampleDto.cs            # DTO
    └── ExampleEndpoints.cs      # Minimal API Endpoints
```

## Testing

This solution uses **NUnit** as its primary testing framework. Testing is currently focused on the `FunctionalTests` project, which validates application logic by integrating with a database orchestrated by **Aspire** and hosted in memory using `WebApplicationFactory`. It also concludes template related testing which can included at generation (see [options](#create-a-new-solution)).

### Application Logic Validation

To test application [`IRequest`](./src/VsaTemplate/Common/Interfaces/Features/IRequest.cs) and [`IDomainEvent`](./src/VsaTemplate/Common/Interfaces/Features/IDomainEvent.cs) implementations and their handlers you can use the [`ApplicationTestBase`](./tests/VsaTemplate.FunctionalTests/Infrastructure/Common/ApplicationTestBase.cs) class which ensures that:
- The database is reset to a clean state before every test
- New dependency injection `IServiceScope` is created
- The domain event spy is cleared

### EF Core Entity Configuration Validation
- To test EF Core entity configuration without needing a database connection, inherit from [`EntityConfigurationTestBase<TConfiguration, TEntity>`](./tests/VsaTemplate.FunctionalTests/Infrastructure/Common/EntityConfigurationTestBase.cs). The class provides helper methods to gain access to the required objects for validation.
