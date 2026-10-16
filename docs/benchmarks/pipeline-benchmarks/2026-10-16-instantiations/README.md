# Instantiation benchmarks — 2026-10-16

Benchmark: `VsaTemplate.Pipeline.Benchmarks`, `Instantiations` cases (10 × `dotnet new vsa-sln --no-update-check` per
case), run at commit `10979bb` with the local workaround described [below](#run-conditions).

Each case was run **3 times**, every run in a fresh container with no warmup. The tables show the median and the range
across the runs. The sequential run without `--no-update-check` was run once.

## Results

| Case | Max concurrency | Execution time (median, range) | Peak memory (median, range) | Disk space¹ |
|---|---|---|---|---|
| `Sequential` | 1 | **8.44 s** (8.22 – 9.08 s) | **176.49 MB** (163.08 – 264.85 MB) | 7.14 MB |
| `ConcurrentCappedToHalfCpuCount` | 2 | **4.60 s** (4.11 – 4.84 s) | **323.26 MB** (301.74 – 350.11 MB) | 7.14 MB |
| `ConcurrentCappedToCpuCount` | 4 | **3.23 s** (3.23 – 3.90 s) | **437.33 MB** (424.75 – 512.47 MB) | 7.14 MB |
| `ConcurrentUncapped` | `int.MaxValue` (all 10 at once) | **3.78 s** (3.16 – 5.40 s) | **1.06 GB** (1.03 – 1.11 GB) | 7.14 MB |

¹ Disk space is the container's total writable-layer size (Docker `SizeRw`) at the end of the step. It is cumulative, so it
includes the template install, not only what the step wrote. It was identical in every run.

All 130 instantiations (13 runs × 10) exited with code `0`.

- **Capping at the CPU count (4) gave the best trade-off.** It was about 2.6× faster than sequential (8.44 s → 3.23 s)
  and used about 40% of the uncapped case's peak memory (437 MB vs 1.06 GB).
- **Uncapped wasn't faster than capped-to-4.** Its median was slightly slower (3.78 s vs 3.23 s), and its range
  (3.16 – 5.40 s) overlaps the capped case's, so with 3 runs the two are a tie on time. Memory is the clear difference:
  all 10 `dotnet new` processes are in memory at once, each roughly 100 MB, competing for 4 CPUs.
- **Half the CPU count (2)** sits in between: about 1.8× faster than sequential and the lowest peak memory of the
  concurrent cases.
- **Disk usage doesn't depend on concurrency.** Ten instantiated solutions plus the installed template take up about 7 MB.

### Per-run values

| Case | Run | Execution time | Peak memory | Log |
|---|---|---|---|---|
| `Sequential` | 1 | 9.08 s | 264.85 MB | [sequential-1.log](runs/sequential-1.log) |
| | 2 | 8.44 s | 163.08 MB | [sequential-2.log](runs/sequential-2.log) |
| | 3 | 8.22 s | 176.49 MB | [sequential-3.log](runs/sequential-3.log) |
| `ConcurrentCappedToHalfCpuCount` | 1 | 4.11 s | 350.11 MB | [concurrent-capped-to-half-cpu-count-1.log](runs/concurrent-capped-to-half-cpu-count-1.log) |
| | 2 | 4.60 s | 301.74 MB | [concurrent-capped-to-half-cpu-count-2.log](runs/concurrent-capped-to-half-cpu-count-2.log) |
| | 3 | 4.84 s | 323.26 MB | [concurrent-capped-to-half-cpu-count-3.log](runs/concurrent-capped-to-half-cpu-count-3.log) |
| `ConcurrentCappedToCpuCount` | 1 | 3.90 s | 437.33 MB | [concurrent-capped-to-cpu-count-1.log](runs/concurrent-capped-to-cpu-count-1.log) |
| | 2 | 3.23 s | 424.75 MB | [concurrent-capped-to-cpu-count-2.log](runs/concurrent-capped-to-cpu-count-2.log) |
| | 3 | 3.23 s | 512.47 MB | [concurrent-capped-to-cpu-count-3.log](runs/concurrent-capped-to-cpu-count-3.log) |
| `ConcurrentUncapped` | 1 | 5.40 s | 1.03 GB | [concurrent-uncapped-1.log](runs/concurrent-uncapped-1.log) |
| | 2 | 3.78 s | 1.06 GB | [concurrent-uncapped-2.log](runs/concurrent-uncapped-2.log) |
| | 3 | 3.16 s | 1.11 GB | [concurrent-uncapped-3.log](runs/concurrent-uncapped-3.log) |
| `Sequential`, without `--no-update-check` | 1 | 1.07 min | 231.88 MB | [sequential-with-update-check.log](runs/sequential-with-update-check.log) |

## Finding: `--no-update-check`

By default, every `dotnet new <template>` call asks the configured NuGet feeds whether a newer version of the template's
package exists, and prints an "update available" hint if one does. It never updates anything; it only checks.
`--no-update-check` skips that request. Nothing else about the instantiation changes.

**The check also runs for templates installed from a local `.nupkg` file**, which is how CI installs the template
(`dotnet new install ./artifacts/package/release/*.nupkg`). A `.nupkg` carries a package ID and version, so `dotnet new`
looks up `Vertical.Slice.Architecture` on nuget.org regardless of where the package was installed from. Every call in
[sequential-with-update-check.log](runs/sequential-with-update-check.log) logs:

```
Failed to check update for Vertical.Slice.Architecture@4.0.0: no NuGet feeds are configured or they are invalid.
```

Without the flag, the `Sequential` case took **1.07 min** instead of a median of **8.44 s**. Timing a single
`dotnet new vsa-sln` directly in the SDK container (template installed from a local `.nupkg`) shows the same thing per call:

| Run | Default | `--no-update-check` |
|---|---|---|
| 1 | 6.279 s | 0.784 s |
| 2 | 6.411 s | 0.858 s |
| 3 | 6.444 s | 0.837 s |

The ~5.5 s difference here is the update check **timing out**, because the benchmark containers had no network access
(see [run conditions](#run-conditions)). With network access the check is one short NuGet HTTP request, not 5.5 s.
It still matters for the benchmarks and CI:

- It adds a network round trip per instantiation, and its latency depends on the network and NuGet, which makes timings noisy.
- In the concurrent cases it fires up to 10 simultaneous NuGet requests.
- In CI the template is always freshly installed at the intended version, so the check provides no useful information.
- If a newer `Vertical.Slice.Architecture` is ever published to nuget.org, CI output would show an "update available"
  hint for a package that was deliberately installed from a local build.

Recommendation: pass `--no-update-check` to `dotnet new vsa-sln` in CI as well. The benchmarks already do, since `3cae8b6`.

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

The benchmark container is limited to `Host.MaxCpu` = 4 CPUs and 16 GB of memory, which roughly matches the host itself.
The concurrency caps are derived from `Host.MaxCpu` (4 and 2), so they're the same on any machine. The host is about the
size of a GitHub-hosted `ubuntu-latest` runner for public repositories (4 vCPU, 16 GB RAM).

## Run conditions

The sandbox differs from a normal machine in ways that required local-only changes. None of them are committed.

- **No network access inside containers.** Outbound traffic from the sandbox has to go through a proxy on the host,
  which containers can't reach. As a result:
  - `dotnet new install Vertical.Slice.Architecture` (from nuget.org) fails with exit code 106. The template was instead
    packed on the host with `dotnet pack nuspec.csproj`, copied into the container, and installed from the `.nupkg`,
    the same way [`ci.yml`](../../../../.github/workflows/ci.yml) installs it.
  - For the run without `--no-update-check`, the flag was removed locally from the `Instantiate` command.
  - The `Restore`, `Build` and `Test` steps need NuGet and couldn't be benchmarked here.
- **`StorageOpt size` (14G) is not enforced.** Docker's containerd image store silently ignores it. The classic
  `overlay2` driver can only enforce it on XFS mounted with `pquota`, and this kernel was built without XFS quota
  support. A 1 GB-limited container accepted a 1.5 GB write. Disk usage is still measured correctly through `SizeRw`.
- **Peak memory** is read from cgroup v1 `memory.max_usage_in_bytes`, which is reset before each step. It includes the
  page cache, i.e. the kernel's cache of file data, so file-heavy steps will look larger than their process memory alone.
  That may be why the first `Sequential` run peaked higher (264.85 MB) than the other two: it was the first run after
  Docker started, so it may have been charged for reading files that later runs found already cached.
