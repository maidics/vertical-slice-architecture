# ASP.NET Core Vertical Slice Architecture Template

This template provides a lightweight starting point for ASP.NET solutions with minimal dependencies. While inspired by [Jason Taylor's Clean Architecture](https://github.com/jasontaylordev/CleanArchitecture/tree/main), this project focuses strictly on Vertical Slice Architecture.

---

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

---

## Technologies
- [ASP.NET Core 10](https://learn.microsoft.com/en-us/aspnet/core/overview?view=aspnetcore-10.0)
- [Aspire](https://aspire.dev/)
- [EF Core 10](https://learn.microsoft.com/en-us/ef/core/)
- [FluentValidation](https://docs.fluentvalidation.net/en/latest/)
- [Scalar](https://scalar.com/)
- [TUnit](https://tunit.dev/), [Shouldly](https://docs.shouldly.org/), & [Respawn](https://github.com/jbogard/Respawn)

---

## Structure

Code is grouped by feature rather than technical layer. Everything required to execute a specific feature lives in a single folder inside the Features directory.

Tests project structure reflects the main project's structure with the testing infrastructure additionally.

```
Common/  # Cross-cutting concerns; any slice may use these
Features/
└── Examples/
    └── AppendContent/
        ├── AppendExampleContentCommand.cs          # IRequest object
        ├── AppendExampleContentCommandHandler.cs   # IRequestHandler
        ├── AppendExampleContentCommandValidator.cs # FluentValidation validator
        ├── AppendExampleContentEndpoint.cs         # IEndpoint
        └── ExampleContentAppendedEvent.cs          # IDomainEvent & IDomainEventHandler
    ├── Create/
    ├── Delete/
    ├── GetAll/
    ├── GetById/
    ├── Update/
    ├── Example.cs              # Entity
    ├── ExampleConfiguration.cs # EF Core configuration
    └── ExampleDto.cs           # DTO
Infrastructure/ # External dependencies
```

---

## Featues
- **Target Framework**: [.NET SDK 10.0.400](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- **No [MediatR](https://github.com/LuckyPennySoftware/MediatR) dependency**: Uses lightweight reflection at application startup to automatically discover and register [`IRequestHandler`](./src/Scratch/Common/Interfaces/Features/IRequestHandler.cs) & [`IDomainEventHandler`](./src/Scratch/Common/Interfaces/Features/IDomainEventHandler.cs) implementations
- **Native Pipeline Filters**: Because MediatR was removed, `PipelineBehavior` has been replaced with native IEndpointFilter implementations
- **Persistence**: Configured with SQLite db out of the box
- **App Host**: Uses [Aspire](https://aspire.dev/) to orchestrate the backend and the SQLite db

### `IEndpointFilter` implementations
1. [`LoggingFilter`](./src/Scratch/Common/Pipeline/LoggingFilter.cs)
    - Logs incoming requests in the following format:
    - `Request: {HttpMethod} {Path}, {@UserId}, {@Request}, {@ResponseStatusCode}`
    - This is a simple developer implementation rather something that should be used in production
2. [`ValidationFilter`](./src/Scratch/Common/Pipeline/ValidationFilter.cs)
    - The filter looks for a parameter that implements [`IRequest`](./src/Scratch/Common/Interfaces/Features/IRequest.cs). If found, it resolves and executes the corresponding `IValidator` implementations
    - **Important:** For automatic validation to occur, your endpoint method must explicitly include an [`IRequest`](./src/Scratch/Common/Interfaces/Features/IRequest.cs) parameter. Otherwise, validation is skipped and must be handled manually within the endpoint
    - For HTTP GET/ DELETE: use `[AsParameters]` on the [`IRequest`](./src/Scratch/Common/Interfaces/Features/IRequest.cs) object
3. [`PerformanceFilter`](./src/Scratch/Common/Pipeline/PerformanceFilter.cs)
    - Logs requests that run for more than *500ms*

### [`IRequest`](./src/Scratch/Common/Interfaces/Features/IRequest.cs)
- Marker interface for [`ValidationFilter`](./src/Scratch/Common/Pipeline/ValidationFilter.cs)
- It exists so that incoming HTTP payloads can be validated automatically
- **Important:** For automatic validation to occur, your endpoint method must explicitly include an [`IRequest`](./src/Scratch/Common/Interfaces/Features/IRequest.cs) parameter. Otherwise, validation is skipped and must be handled manually within the endpoint

### [`IRequestHandler`](./src/Scratch/Common/Interfaces/Features/IRequestHandler.cs)
- Marker interface for dependency injection registration
- It defines no methods for return and parameter type flexibility and simplicity (e.g: consuming a type such as `Guid` - which requires no manual validation - should not require creating a new [`IRequest`](./src/Scratch/Common/Interfaces/Features/IRequest.cs) type)

### [`IDomainEvent`](./src/Scratch/Common/Interfaces/Features/IDomainEvent.cs)
- Marker interface for domain events (equivalent to MediatR's `INotification`)
- Created events should be passed to [`BaseEntity`](./src/Scratch/Common/BaseClasses/BaseEntity.cs)'s `AddDomainEvent` method which will be used to publish the events via the [`DispatchDomainEventInterceptor`](./src/Scratch/Infrastructure/Database/Interceptors/DispatchDomainEventInterceptor.cs)

### [`IDomainEventHandler`](./src/Scratch/Common/Interfaces/Features/IDomainEventHandler.cs)
- Defines the Handle method that handles the passed [`IDomainEvent`](./src/Scratch/Common/Interfaces/Features/IDomainEvent.cs)

### [`IEndpoint`](./src/VsaTemplate/Common/Interfaces/Features/IEndpoint.cs) & [`IEndpoint<TResource>`](./src/VsaTemplate/Common/Interfaces/Features/IEndpoint.cs)
- Interface to register an endpoint automatically (see: [`EndpointRouteBuilderExtensions`](./src/Scratch/Common/Extensions/EndpointRouteBuilderExtensions.cs))
- [`IEndpoint<TResource>`](./src/VsaTemplate/Common/Interfaces/Features/IEndpoint.cs) defaults `Prefix` to the given TResource's name

---

## Testing

This solution uses **[TUnit](https://tunit.dev/)** as its testing framework for performance reasons. 

### Tests

Unit, functional, integration and web testing merged into one project to reduce project count in the solution. This also allows to reflect the [main ASP.NET project's](./src/VsaTemplate) structure for convenience.

**[`Common`](./tests/VsaTemplate.Tests/Common) folder**
- Unit tests for base, constants classes, extensions and more in the [main ASP.NET project's `Common` folder](./src/VsaTemplate/Common).

**[`Features`](./tests/VsaTemplate.Tests/Features) folder**
- Holds tests for everything inside the [main ASP.NET project's `Features` folder](./src/VsaTemplate/Features):
  - Entities
  - EF Core entity configurations. Instantiate the unit test helper for assertions: [`EntityConfigurationFixture`](./tests/VsaTemplate.Tests/TestInfrastructure/UnitTests)
  - `AbstractValidator` classes
  - **[`IRequest`](./src/Scratch/Common/Interfaces/Features/IRequest.cs)** and **[`IDomainEvent`](./src/Scratch/Common/Interfaces/Features/IDomainEvent.cs) implementations and their handlers**. You can use the [`FunctionalTestBase`](./tests/VsaTemplate.Tests/TestInfrastructure/FunctionalTests/FunctionalTestBase.cs) class:
    - Marked with `NotInParallel` attribute due to shared db instance
    - Instantiates [`FunctionalTestFixture`](./tests/VsaTemplate.Tests/TestInfrastructure/FunctionalTests/FunctionalTestFixture.cs) (injected via `ClassDataSource<T>`)
    - Resets the `Fixture` (resets db, creates a new `IServiceScope`)
  - IEndpoint implementations. Use the [`EndpointTestBase`](./tests/VsaTemplate.Tests/TestInfrastructure/WebTests/EndpointTestBase.cs):
    - Marked with `NotInParallel` attribute due to shared db instance
    - Assert `Prefix` and `Tags` attributes
    - E2E test the endpoint itself

### Template Tests

Tests the infrastructure shipped with the template such as the:
- [`AuditableEntityInterceptor`](./src/VsaTemplate/Infrastructure/Database/Interceptors/AuditableEntityInterceptor.cs)
- [`DispatchDomainEventInterceptor`](./src/VsaTemplate/Infrastructure/Database/Interceptors/DispatchDomainEventInterceptor.cs)
- `IEndpointFilter` implementations
- [`Result`](./src/VsaTemplate/Common/Models/Result.cs)
- Extension methods
- and more...

***This project can be explicitly included when instantiating the template (see [options](#create-a-new-solution))***.