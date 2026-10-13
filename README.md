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

| Options                      | Values      | Default | Description                                                                                                                                                                                                                                               |
|------------------------------|-------------|---------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `--examples`, `-e`           | true, false | false   | Includes example implementations and tests                                                                                                                                                                                                                |
| `--modular-pipelines`, `-mp` | true, false | false   | Includes the [`Pipeline`](./src/VsaTemplate.Pipeline/) and [`Pipeline.Tests`](./tests/VsaTemplate.Pipeline.Tests) scaffold projects with [`ModularPipelines`](https://thomhurst.github.io/ModularPipelines/) allowing you to write your CI pipeline in C# |

> **Tip**: run `dotnet new vsa-sln --help` to see all available options.

---

## Technologies
- [ASP.NET Core 10](https://learn.microsoft.com/en-us/aspnet/core/overview?view=aspnetcore-10.0)
- [Aspire](https://aspire.dev/)
- [EF Core 10](https://learn.microsoft.com/en-us/ef/core/)
- [FluentValidation](https://docs.fluentvalidation.net/en/latest/)
- [Scalar](https://scalar.com/)
- [TUnit](https://tunit.dev/), [TUnit.Mocks](https://tunit.dev/docs/writing-tests/mocking/), [Shouldly](https://docs.shouldly.org/), & [Respawn](https://github.com/jbogard/Respawn)
- [ModularPipelines](https://thomhurst.github.io/ModularPipelines/) (`-mp` option only)

---

## Structure

Code is grouped by feature rather than technical layer. Test projects reflect the main project's structure, with their own testing infrastructure on top.

```
Common/  # Cross-cutting concerns; any slice may use these

Domain/
├── BaseClasses/
├── Constants/
└── Entities/

Features/
└── Examples/
    ├── CreateExample.cs
    ├── DeleteExample.cs
    ├── ExampleDto.cs
    ├── GetExampleById.cs
    ├── GetExamples.cs
    └── UpdateExample.cs

Infrastructure/ # External dependencies
├── Database/
└── Identity/

```

---

## Features

- **Target Framework**: [.NET SDK 10.0.401](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- **No [MediatR](https://github.com/LuckyPennySoftware/MediatR) dependency**: Uses lightweight reflection at application startup to automatically discover and register [`IRequestHandler`](./src/VsaTemplate/Common/Interfaces/IRequestHandler.cs) services.
- **Native Pipeline Filters**: Because MediatR was removed, `PipelineBehavior` has been replaced with native `IEndpointFilter` implementations.
- **Persistence**: Configured with SQLite db out of the box.
- **App Host**: Uses [Aspire](https://aspire.dev/) to orchestrate the backend and the SQLite database.

### `IEndpointFilter` implementations

1. [`LoggingFilter`](./src/VsaTemplate/Common/Pipeline/LoggingFilter.cs)
    - Logs incoming requests in the following format:
      - `Request: {HttpMethod} {Path}, {@UserId}, {@Request}, {@ResponseStatusCode}`.
    - This is a simple developer implementation rather something that should be used in production.

2. [`ValidationFilter`](./src/VsaTemplate/Common/Pipeline/ValidationFilter.cs)
    - The filter looks for a parameter that implements [`IRequest`](./src/VsaTemplate/Common/Interfaces/IRequest.cs). If found, it resolves and executes the corresponding `IValidator` implementations.
    - **Important:** For automatic validation to occur, your endpoint method must explicitly include an [`IRequest`](./src/VsaTemplate/Common/Interfaces/IRequest.cs) parameter. Otherwise, validation is skipped and must be handled manually within the endpoint.
    - For HTTP GET/ DELETE: use `[AsParameters]` on the [`IRequest`](./src/VsaTemplate/Common/Interfaces/IRequest.cs) object.

3. [`PerformanceFilter`](./src/VsaTemplate/Common/Pipeline/PerformanceFilter.cs)
    - Logs requests that run for more than *500ms*.

### [`IRequest`](./src/VsaTemplate/Common/Interfaces/IRequest.cs)

- Marker interface for [`ValidationFilter`](./src/VsaTemplate/Common/Pipeline/ValidationFilter.cs).
- It exists so that incoming HTTP payloads can be validated automatically.

### [`IRequestHandler`](./src/VsaTemplate/Common/Interfaces/IRequestHandler.cs)

- Marker interface for dependency injection registration
- It defines no methods for return and parameter type flexibility and simplicity (e.g: consuming a type such as `Guid` - which requires no manual validation - should not require creating a new [`IRequest`](./src/VsaTemplate/Common/Interfaces/IRequest.cs) type)

### [`IEndpoint`](./src/VsaTemplate/Common/Interfaces/IEndpoint.cs)

- Interface to register an endpoint automatically (see: [`EndpointRouteBuilderExtensions`](./src/VsaTemplate/Common/Extensions/EndpointRouteBuilderExtensions.cs))

---

## Testing

This solution uses **[TUnit](https://tunit.dev/)** as its testing framework.

### Test projects

Tests are split into projects by their dependencies and test infrastructure.

| Project                                                    | Tests                                                                    | Infrastructure                                                                                                                             |
|------------------------------------------------------------|--------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------|
| [`UnitTests`](./tests/VsaTemplate.UnitTests)               | Plain unit tests, validators, endpoint metadata, request handlers via DI | In-memory app (`WebApplicationFactory`, no server), database via [`TestAppHost`](./tests/VsaTemplate.TestAppHost), mocks via `TUnit.Mocks` |
| [`IntegrationTests`](./tests/VsaTemplate.IntegrationTests) | -                                                                        | -                                                                                                                                          |
| [`WebTests`](./tests/VsaTemplate.WebTests)                 | E2E tests of endpoints through `HttpClient`                              | In-memory app with Kestrel started, database via [`TestAppHost`](./tests/VsaTemplate.TestAppHost)                                          |

Database-backed tests run against the database provisioned by Aspire through the [`TestAppHost`](./tests/VsaTemplate.TestAppHost), so they use the same provider as production. Because the tests share one database instance, those tests are marked `NotInParallel` and the database is reset with Respawn before each test.

---

**[UnitTests infrastructure](./tests/VsaTemplate.UnitTests/TestInfrastructure)**

- [`UnitTestFixture`](./tests/VsaTemplate.UnitTests/TestInfrastructure/UnitTestFixture.cs) initializes asynchronously:
  - Database, set up via the [`TestAppHost`](./tests/VsaTemplate.TestAppHost)
  - [`UnitTestApplicationFactory`](./tests/VsaTemplate.UnitTests/TestInfrastructure/UnitTestApplicationFactory.cs), which replaces `IUser` with [`TestUser`](./tests/VsaTemplate.UnitTests/TestInfrastructure/TestUser.cs)
- [`UnitTestBase`](./tests/VsaTemplate.UnitTests/TestInfrastructure/UnitTestBase.cs) for testing **[`IRequest`](./src/VsaTemplate/Common/Interfaces/IRequest.cs)** handlers and other services resolved from DI:
  - Instantiates `UnitTestFixture` (injected via [`ClassDataSource<T>`](https://tunit.dev/docs/writing-tests/class-data-source/))
  - Resets the database and creates a new `IServiceScope` before each test
- [`EndpointUnitTestBase`](./tests/VsaTemplate.UnitTests/TestInfrastructure/EndpointUnitTestBase.cs) asserts the `Prefix`, `Tags` of `IEndpoint` implementations and attributes
- [`EndpointRouteBuilderSpy`](./tests/VsaTemplate.UnitTests/TestInfrastructure/EndpointRouteBuilderSpy.cs): an in-memory `IEndpointRouteBuilder` for asserting mapped endpoints (injected via [`ClassDataSource<T>`](https://tunit.dev/docs/writing-tests/class-data-source/))
- [`EntityConfigurationFixture`](./tests/VsaTemplate.UnitTests/TestInfrastructure/EntityConfigurationFixture.cs) for asserting `IEntityTypeConfiguration` implementations

**[WebTests infrastructure](./tests/VsaTemplate.WebTests/TestInfrastructure)**

- [`WebTestFixture`](./tests/VsaTemplate.WebTests/TestInfrastructure/WebTestFixture.cs) initializes asynchronously:
  - Database, set up via the [`TestAppHost`](./tests/VsaTemplate.TestAppHost), with seeded roles
  - [`WebTestApplicationFactory`](./tests/VsaTemplate.WebTests/TestInfrastructure/WebTestApplicationFactory.cs) with Kestrel started on a dynamic port, so `HttpClient` requests go over the network
- [`EndpointTestBase`](./tests/VsaTemplate.WebTests/TestInfrastructure/EndpointTestBase.cs) for E2E testing endpoints:
  - Resets the database and creates a new `IServiceScope` before each test
  - Provides helpers for logging in with roles, seeding and querying the database
