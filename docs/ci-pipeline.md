# CI pipeline

Majority of CI logic is written in C# using the [`ModularPipelines`](https://thomhurst.github.io/ModularPipelines/) package.

## Requirements

| Requirement | Skipped on | Description |
| - | - | - |
| VersionTagRequirement | Non tag pushes | Asserts tag semantics: vMAJOR.MINOR.PATCH |

Requirements have no skip logic: handled with a log and early return.

## Modules

 | Module | Depends on | Run Condition | Description |
| - | - | - | - |
| `RestoreTemplateModule` | - | - | Restores NuGet packages. |
| `BuildTemplateModule` | `RestoreModule` | - | Builds the template itself. |
| `TestTemplateModule` | `TemplateBuildModule` | - | Runs Pipeline.Tests _only_. `IntegrationTests` project is empty; `UnitTests` & `WebTests` are ran after instantiations. |
| `PackTemplateModule` | `TemplateTestModule` | - | Packs template into a `.nupkg` and verifies that it only produced a single artifact. |
| `InstallPackedTemplateModule` | `PackTemplateModule` | - | Installs the packed template. |
| `InstantiateVariantsModule` | `InstallPackedTemplateModule` | PR and main pushes | Instantiates every single variant **in parrallel**. |
| `RestoreVariantsModule` | `InstantiateVariantsModule` | PR and main pushes | Restores all nuget packages for the variants **in parrallel**. |
| `BuildVariantsModule` | `RestoreVariantsModule` | PR and main pushes | Builds all variant **sequencially** - `dotnet build` uses all cores, collect Exceptions, rethrow as aggregate exception. |
| `TestVariantsModule` | `BuildVariantsModule` | PR and main pushes | Tests all variants **in parrallel**. |
| `PublishModule` | `VerifyTemplateVariantsModule` | Tag pushes | Logs into NuGet and posts the packed template. |

For parrallelized modules:
```csharp
    var all = Task.WhenAll(tasks);
    
    try
    {
        return await all;
    }
    catch when (all.Exception is not null)
    {
        throw all.Exception;   // AggregateException holding every failure
    }
```

Additionally:
- `WebTests` and ports: if they use fixed ports, container names then testing will collide and fail