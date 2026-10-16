# Restoration benchmarks — 2026-10-16

Benchmark: `VsaTemplate.Pipeline.Benchmarks`, `Restorations` cases, run at commit `9f97573` with the local workarounds
described [below](#run-conditions).

Every case first instantiates 10 solutions (`dotnet new vsa-sln --no-update-check`, capped at 4 at a time, the best
option from the [instantiation benchmarks](../2026-10-16-instantiations/README.md)), then restores all 10 with
`dotnet restore <name>/<name>.slnx` at the case's concurrency. The `…WithCloneAndRestoreRepo` cases run an extra step in
between: `git clone` of this repository's default branch followed by `dotnet restore` of `VsaTemplate.slnx`. That fills
NuGet's local package cache before the 10 restores.

Each case was run **3 times**, every run in a fresh container with an empty NuGet cache and no warmup. The tables show the
median and the range across the runs. All 516 commands (24 runs × 20 or 22) exited with code `0`.

## Results

### Restore, empty package cache

| Case | Max concurrency | Execution time (median, range) | Peak memory (median, range) | Disk space¹ |
|---|---|---|---|---|
| `Sequential` | 1 | **29.35 s** (28.88 – 29.79 s) | **1.89 GB** (1.88 – 1.90 GB) | 1.19 GB |
| `ConcurrentCappedToHalfCpuCount` | 2 | **28.00 s** (27.19 – 28.19 s) | **2.22 GB** (2.20 – 2.23 GB) | 1.19 GB |
| `ConcurrentCappedToCpuCount` | 4 | **33.53 s** (31.32 – 34.01 s) | **2.77 GB** (2.76 – 2.79 GB) | 1.19 GB |
| `ConcurrentUncapped` | `int.MaxValue` (all 10 at once) | **57.08 s** (53.86 – 59.20 s) | **5.13 GB** (5.06 – 5.13 GB) | 1.19 GB |

### Restore after `CloneAndRestoreRepo`

| Case | Max concurrency | Execution time (median, range) | Peak memory (median, range) | Disk space¹ |
|---|---|---|---|---|
| `SequentialWithCloneAndRestoreRepo` | 1 | **20.74 s** (20.45 – 21.22 s) | **2.06 GB** (2.06 – 2.09 GB) | 1.36 GB |
| `ConcurrentCappedToHalfCpuCountWithCloneAndRestoreRepo` | 2 | **17.54 s** (17.53 – 17.82 s) | **2.39 GB** (2.38 – 2.42 GB) | 1.36 GB |
| `ConcurrentCappedToCpuCountWithCloneAndRestoreRepo` | 4 | **21.68 s** (20.08 – 22.03 s) | **2.68 GB** (2.67 – 2.73 GB) | 1.36 GB |
| `ConcurrentUncappedWithCloneAndRestoreRepo` | `int.MaxValue` (all 10 at once) | **25.39 s** (24.10 – 26.61 s) | **4.08 GB** (4.01 – 4.16 GB) | 1.36 GB |

The `CloneAndRestoreRepo` step itself took a median of **13.69 s** (12.91 – 14.44 s across all 12 runs), peaked at about
**1.88 GB**, and brought disk usage to **1.34 GB**.

### Instantiate step

The instantiate step was identical in every case (10 instances, capped at 4): **2.79 – 3.90 s**, **0.27 – 0.54 GB** peak
memory and **6.21 MB** on disk. This matches the instantiation benchmarks.

¹ Disk space is the container's total writable-layer size (Docker `SizeRw`) at the end of the step. It is cumulative, so it
includes everything written by earlier steps. It was identical across runs of the same case.

### Observations

- **Capping at half the CPU count (2) was the fastest in both scenarios**: slightly faster than sequential with an
  empty cache (28.00 s vs 29.35 s) and about 15% faster after `CloneAndRestoreRepo` (17.54 s vs 20.74 s), for about
  0.3 GB more peak memory.
- **More concurrency made restore slower, not faster.** `dotnet restore` already restores the projects within a
  solution in parallel, so running several restores at once mostly adds contention. Capping at the CPU count (4) was
  slower than sequential, and running all 10 at once took **about twice as long** with an empty cache (57.08 s), at a
  **5.13 GB** memory peak.
- **A filled package cache shortened every restore by 29 – 37%**, and by 56% for the uncapped case. The generated
  solutions share most of their packages with this repository.
- **Filling the cache first was not a net win on its own.** `CloneAndRestoreRepo` plus the faster restore
  (13.69 s + 20.74 s sequential) took longer than restoring with an empty cache (29.35 s). It only pays off if the
  repository restore is needed anyway.
- **Restoring 10 solutions writes about 1.19 GB**, most of it extracted NuGet packages. Concurrency doesn't change that.

### Per-run values (restore step)

| Case | Run | Execution time | Peak memory | Log |
|---|---|---|---|---|
| `Sequential` | 1 | 29.35 s | 1.89 GB | [sequential-1.log](runs/sequential-1.log) |
| | 2 | 28.88 s | 1.88 GB | [sequential-2.log](runs/sequential-2.log) |
| | 3 | 29.79 s | 1.90 GB | [sequential-3.log](runs/sequential-3.log) |
| `ConcurrentCappedToHalfCpuCount` | 1 | 28.00 s | 2.22 GB | [concurrent-capped-to-half-cpu-count-1.log](runs/concurrent-capped-to-half-cpu-count-1.log) |
| | 2 | 28.19 s | 2.23 GB | [concurrent-capped-to-half-cpu-count-2.log](runs/concurrent-capped-to-half-cpu-count-2.log) |
| | 3 | 27.19 s | 2.20 GB | [concurrent-capped-to-half-cpu-count-3.log](runs/concurrent-capped-to-half-cpu-count-3.log) |
| `ConcurrentCappedToCpuCount` | 1 | 33.53 s | 2.76 GB | [concurrent-capped-to-cpu-count-1.log](runs/concurrent-capped-to-cpu-count-1.log) |
| | 2 | 31.32 s | 2.77 GB | [concurrent-capped-to-cpu-count-2.log](runs/concurrent-capped-to-cpu-count-2.log) |
| | 3 | 34.01 s | 2.79 GB | [concurrent-capped-to-cpu-count-3.log](runs/concurrent-capped-to-cpu-count-3.log) |
| `ConcurrentUncapped` | 1 | 59.20 s | 5.13 GB | [concurrent-uncapped-1.log](runs/concurrent-uncapped-1.log) |
| | 2 | 53.86 s | 5.06 GB | [concurrent-uncapped-2.log](runs/concurrent-uncapped-2.log) |
| | 3 | 57.08 s | 5.13 GB | [concurrent-uncapped-3.log](runs/concurrent-uncapped-3.log) |
| `SequentialWithCloneAndRestoreRepo` | 1 | 20.45 s | 2.09 GB | [sequential-with-clone-and-restore-repo-1.log](runs/sequential-with-clone-and-restore-repo-1.log) |
| | 2 | 21.22 s | 2.06 GB | [sequential-with-clone-and-restore-repo-2.log](runs/sequential-with-clone-and-restore-repo-2.log) |
| | 3 | 20.74 s | 2.06 GB | [sequential-with-clone-and-restore-repo-3.log](runs/sequential-with-clone-and-restore-repo-3.log) |
| `ConcurrentCappedToHalfCpuCountWithCloneAndRestoreRepo` | 1 | 17.53 s | 2.42 GB | [concurrent-capped-to-half-cpu-count-with-clone-and-restore-repo-1.log](runs/concurrent-capped-to-half-cpu-count-with-clone-and-restore-repo-1.log) |
| | 2 | 17.54 s | 2.38 GB | [concurrent-capped-to-half-cpu-count-with-clone-and-restore-repo-2.log](runs/concurrent-capped-to-half-cpu-count-with-clone-and-restore-repo-2.log) |
| | 3 | 17.82 s | 2.39 GB | [concurrent-capped-to-half-cpu-count-with-clone-and-restore-repo-3.log](runs/concurrent-capped-to-half-cpu-count-with-clone-and-restore-repo-3.log) |
| `ConcurrentCappedToCpuCountWithCloneAndRestoreRepo` | 1 | 22.03 s | 2.67 GB | [concurrent-capped-to-cpu-count-with-clone-and-restore-repo-1.log](runs/concurrent-capped-to-cpu-count-with-clone-and-restore-repo-1.log) |
| | 2 | 20.08 s | 2.68 GB | [concurrent-capped-to-cpu-count-with-clone-and-restore-repo-2.log](runs/concurrent-capped-to-cpu-count-with-clone-and-restore-repo-2.log) |
| | 3 | 21.68 s | 2.73 GB | [concurrent-capped-to-cpu-count-with-clone-and-restore-repo-3.log](runs/concurrent-capped-to-cpu-count-with-clone-and-restore-repo-3.log) |
| `ConcurrentUncappedWithCloneAndRestoreRepo` | 1 | 24.10 s | 4.01 GB | [concurrent-uncapped-with-clone-and-restore-repo-1.log](runs/concurrent-uncapped-with-clone-and-restore-repo-1.log) |
| | 2 | 25.39 s | 4.08 GB | [concurrent-uncapped-with-clone-and-restore-repo-2.log](runs/concurrent-uncapped-with-clone-and-restore-repo-2.log) |
| | 3 | 26.61 s | 4.16 GB | [concurrent-uncapped-with-clone-and-restore-repo-3.log](runs/concurrent-uncapped-with-clone-and-restore-repo-3.log) |

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

- **Network access goes through the sandbox's proxy.** Outbound traffic from the sandbox has to go through a proxy on
  the host. The benchmark containers were run on the host network and trusted the proxy's certificate, so NuGet
  downloads and `git clone` went through that proxy. **Download times reflect this sandbox's network, not a GitHub
  runner's**, so absolute restore times will differ in CI. The comparison between cases is still meaningful, because
  every case downloaded the same packages over the same connection.
- **The template was installed from a local package.** It was packed on the host with `dotnet pack nuspec.csproj`,
  copied into the container, and installed from the `.nupkg`, the same way [`ci.yml`](../../../../.github/workflows/ci.yml)
  installs it, instead of from nuget.org.
- **`StorageOpt size` (14G) is not enforced.** Docker's containerd image store silently ignores it, and this kernel was
  built without the XFS quota support the classic `overlay2` driver needs. Restoring used at most 1.36 GB, well under the
  limit. Disk usage is still measured correctly through `SizeRw`.
- **Peak memory** is read from cgroup v1 `memory.max_usage_in_bytes`, which is reset before each step. It includes the
  page cache, i.e. the kernel's cache of file data. Restore extracts about 1.2 GB of packages, so a large share of the
  restore peaks is likely cached file data rather than memory used by the `dotnet` processes themselves.
