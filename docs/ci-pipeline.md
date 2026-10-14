# CI pipeline

Majority of CI logic is written in C# using the [`ModularPipelines`](https://thomhurst.github.io/ModularPipelines/) package.

## Requirements

| Requirement | Run Condition | Description |
| - | - | - |
| VersionTagRequirement | Tag push | Asserts tag semantics: vMAJOR.MINOR.PATCH |

Requirements have no skip logic: handled with a log and early return.

## Modules

 | Module | Depends on | Run Condition | Description |
| - | - | - | - |
| `RestoreTemplateModule` | - | - | Restores NuGet packages. |
| `BuildTemplateModule` | `RestoreModule` | - | Builds the template itself. |
| `TestTemplateModule` | `TemplateBuildModule` | - | Runs Pipeline.Tests _only_. `IntegrationTests` project is empty; `UnitTests` & `WebTests` are ran after instantiations. |
| `PackTemplateModule` | `TemplateTestModule` | - | Packs template into a `.nupkg` and verifies that it only produced a single artifact. |
| `InstallPackedTemplateModule` | `PackTemplateModule` | - | Installs the packed template. |
| `InstantiateVariantsModule` | `InstallPackedTemplateModule` | PR or main push | Instantiates every single variant **in parrallel**. |
| `RestoreVariantsModule` | `InstantiateVariantsModule` | PR or main push | Restores all nuget packages for the variants **in parrallel**. |
| `BuildVariantsModule` | `RestoreVariantsModule` | PR or main push | Builds all variant **sequencially** - `dotnet build` uses all cores, collect Exceptions, rethrow as aggregate exception. |
| `TestVariantsModule` | `BuildVariantsModule` | PR or main push | Runs unit and web tests for all variants **in parrallel**. |
| `PublishModule` | `VerifyTemplateVariantsModule` | Tag push | Logs into NuGet and posts the packed template. |

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