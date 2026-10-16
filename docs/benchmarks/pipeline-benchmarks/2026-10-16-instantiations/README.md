# Instantiation benchmarks — 2026-10-16

Benchmark: `VsaTemplate.Pipeline.Benchmarks`, cases `Instantiations.Sequential` and `Instantiations.Concurrent`
(10 × `dotnet new vsa-sln`), run at commit `2c38d8c` with the local workarounds described [below](#run-conditions).

Each case was run **once**, with no warmup or repetition. Treat the numbers as indicative, not statistically meaningful.

## Results

| Case | Max concurrency | Execution time | Peak memory | Disk space¹ | Exit codes | Log |
|---|---|---|---|---|---|---|
| `Instantiations.Sequential` | 1 | 8.25 s | 202.75 MB | 7.14 MB | 10 × `0` | [sequential.log](sequential.log) |
| `Instantiations.Concurrent` | 10 | 3.81 s | 1003.04 MB | 7.14 MB | 10 × `0` | [concurrent.log](concurrent.log) |
| `Instantiations.Sequential`, without `--no-update-check` | 1 | 1.06 min | 332.68 MB | 7.15 MB | 10 × `0` | [sequential-with-update-check.log](sequential-with-update-check.log) |

¹ Disk space is the container's total writable-layer size (Docker `SizeRw`) at the end of the step. It is cumulative, so it
includes the template install, not only what the step wrote.

- Running concurrently made the step about **2.2× faster** (8.25 s → 3.81 s) but used about **5× the peak memory**
  (203 MB → 1003 MB). Each `dotnet new` is its own .NET process, at roughly 100 MB apiece.
- Disk usage is the same in both modes. Ten instantiated solutions plus the installed template take up about 7 MB.

## Finding: `--no-update-check`

By default, every `dotnet new <template>` call asks the configured NuGet feeds whether a newer version of the template's
package exists, and prints an "update available" hint if one does. It never updates anything; it only checks.
`--no-update-check` skips that request. Nothing else about the instantiation changes.

**The check also runs for templates installed from a local `.nupkg` file**, which is how CI installs the template
(`dotnet new install ./artifacts/package/release/*.nupkg`). A `.nupkg` carries a package ID and version, so `dotnet new`
looks up `Vertical.Slice.Architecture` on nuget.org regardless of where the package was installed from. Every call in
[sequential-with-update-check.log](sequential-with-update-check.log) logs:

```
Failed to check update for Vertical.Slice.Architecture@4.0.0: no NuGet feeds are configured or they are invalid.
```

Timing a single `dotnet new vsa-sln` directly in the SDK container (template installed from a local `.nupkg`):

| Run | Default | `--no-update-check` |
|---|---|---|
| 1 | 6.279 s | 0.784 s |
| 2 | 6.411 s | 0.858 s |
| 3 | 6.444 s | 0.837 s |

The ~5.5 s difference here is the update check **timing out**, because the benchmark containers had no network access
(see [run conditions](#run-conditions)). With network access the check is one short NuGet HTTP request, not 5.5 s.
It still matters for the benchmarks and CI:

- It adds a network round trip per instantiation, and its latency depends on the network and NuGet, which makes timings noisy.
- In the concurrent case it fires 10 simultaneous NuGet requests.
- In CI the template is always freshly installed at the intended version, so the check provides no useful information.
- If a newer `Vertical.Slice.Architecture` is ever published to nuget.org, CI output would show an "update available"
  hint for a package that was deliberately installed from a local build.

Recommendation: pass `--no-update-check` to `dotnet new vsa-sln` in the benchmarks and in CI.

## Machine

| | |
|---|---|
| Environment | Claude Code cloud sandbox (KVM virtual machine) |
| CPU | Intel Xeon @ 2.10 GHz, 4 vCPUs (4 cores × 1 thread) |
| Memory | 15.72 GB |
| Disk | 252 GB ext4 (`/dev/vda`) |
| OS | Ubuntu 24.04.5 LTS, kernel `6.18.44-fc-v139` |
| Docker | 29.8.2 (client and server), storage driver `overlayfs` (containerd image store), cgroup v1, `cgroupfs` driver |
| .NET SDK (host) | 10.0.401 |
| Benchmark image | `mcr.microsoft.com/dotnet/sdk:10.0.401` |
| Packages | Testcontainers 4.16.0, TomLonghurst.EnumerableAsyncProcessor 1.1.0 |

The benchmark container is limited to 4 CPUs and 16 GB of memory, which roughly matches the host itself, so those
limits didn't constrain these runs. The host is about the size of a GitHub-hosted `ubuntu-latest` runner for public
repositories (4 vCPU, 16 GB RAM).

## Run conditions

The sandbox differs from a normal machine in ways that required local-only changes. None of them are committed.

- **No network access inside containers.** Outbound traffic from the sandbox has to go through a proxy on the host,
  which containers can't reach. As a result:
  - `dotnet new install Vertical.Slice.Architecture` (from nuget.org) fails with exit code 106. The template was instead
    packed on the host with `dotnet pack nuspec.csproj`, copied into the container, and installed from the `.nupkg`,
    the same way [`ci.yml`](../../../.github/workflows/ci.yml) installs it.
  - `--no-update-check` was appended to `dotnet new vsa-sln` for the Sequential and Concurrent runs. The third run
    shows the effect without it.
  - The `Restore`, `Build` and `Test` steps need NuGet and couldn't be benchmarked here.
- **`StorageOpt size` (14G) is not enforced.** Docker's containerd image store silently ignores it. The classic
  `overlay2` driver can only enforce it on XFS mounted with `pquota`, and this kernel was built without XFS quota
  support. A 1 GB-limited container accepted a 1.5 GB write. Disk usage is still measured correctly through `SizeRw`.
- **Peak memory** is read from cgroup v1 `memory.max_usage_in_bytes`, which is reset before each step. It includes the
  page cache, i.e. the kernel's cache of file data, so file-heavy steps will look larger than their process memory alone.
