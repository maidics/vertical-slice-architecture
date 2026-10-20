# CI pipeline

Majority of CI logic is written in C# using the [`ModularPipelines`](https://thomhurst.github.io/ModularPipelines/) package.

## Requirements

| Requirement | Run Condition | Description |
| - | - | - |
| `LinuxRequirement` | - | Require Linux environment, because of shell commands at template variant verifications.| 
| `VersionTagRequirement` | Tag push | Asserts tag semantics: vMAJOR.MINOR.PATCH |

Requirements have no skip logic: handled with a log and early return.

## Modules

 | Module | Depends on | Run Condition | Description |
| - | - | - | - |
| `RestoreTemplateModule` | - | - | Restores NuGet packages. |
| `BuildTemplateModule` | `RestoreTemplateModule` | - | Builds the template itself. |
| `TestTemplateModule` | `BuildTemplateModule` | - | Runs Pipeline.Tests _only_. `IntegrationTests` project is empty; `UnitTests` & `WebTests` are run after instantiations. |
| `PackTemplateModule` | `TestTemplateModule` | - | Packs template into a `.nupkg` and verifies that it only produced a single artifact. |
| `InstallTemplateModule` | `PackTemplateModule` | - | Installs the packed template. |
| `InstantiateTemplateVariantsModule` | `InstallTemplateModule` | PR, main or tag push | Instantiates every single variant **in parallel**. |
| `RestoreVariantsModule` | `InstantiateTemplateVariantsModule` | PR, main or tag push | Restores all nuget packages for the variants **in parallel**. |
| `BuildVariantsModule` | `RestoreVariantsModule` | PR, main or tag push | Builds all variants **sequentially** - `dotnet build` uses all cores, collect Exceptions, rethrow as aggregate exception. |
| `TestVariantsModule` | `BuildVariantsModule` | PR, main or tag push | Runs unit and web tests for all variants **in parallel**. |
| `GitHubOidcModule` | `TestVariantsModule` | Tag push | Retrieves an OIDC token from GitHub to issue the short lived NuGet API token. |
| `NuGetLoginModule` | `GitHubOidcModule` | Tag push | Retrieves a short lived API key to NuGet (Trusted Publishing) using the GitHub issued OIDC token. |
|`NuGetPushModule` | `NuGetLoginModule` | Tag push | Publishes the template to NuGet. |

For parallelized modules:
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