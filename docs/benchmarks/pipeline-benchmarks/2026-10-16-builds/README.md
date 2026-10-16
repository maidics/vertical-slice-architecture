# Build benchmarks — 2026-10-16

Benchmark: `VsaTemplate.Pipeline.Benchmarks`, `Builds` cases, run at commit `c5e3864` with the local workarounds
described [below](#run-conditions).

Each case builds on the best results of the previous benchmarks:

1. **Instantiate** 10 solutions with `dotnet new vsa-sln --no-update-check`, capped at 4 at a time
   ([instantiation benchmarks](../2026-10-16-instantiations/README.md)).
2. **CloneAndRestoreRepo**: `git clone` of this repository's default branch and `dotnet restore` of `VsaTemplate.slnx`.
3. **Restore** the 10 solutions, capped at 2 at a time ([restoration benchmarks](../2026-10-16-restorations/README.md)).
4. **Build** the 10 solutions with `dotnet build <name>/<name>.slnx --no-restore` at the case's concurrency. This is the
   step being compared.

Each case was run **3 times**, every run in a fresh container with no warmup. The tables show the median and the range
across the runs. All 384 commands (12 runs × 32) exited with code `0`.

## Results

### Build step

| Case | Max concurrency | Execution time (median, range)¹ | Peak memory (median, range) | Disk space² |
|---|---|---|---|---|
| `Sequential` | 1 | **58.79 s** (58.47 – 63.60 s) | **8.06 GB** (8.02 – 8.12 GB) | 6.23 GB |
| `ConcurrentCappedToHalfCpuCount` | 2 | **54.46 s** (52.54 – 63.00 s) | **8.26 GB** (8.26 – 8.26 GB) | 6.23 GB |
| `ConcurrentCappedToCpuCount` | 4 | **66.00 s** (64.80 – 70.20 s) | **9.28 GB** (9.27 – 9.29 GB) | 6.23 GB |
| `ConcurrentUncapped` | `int.MaxValue` (all 10 at once) | **98.40 s** (97.20 – 107.40 s) | **12.03 GB** (11.96 – 12.37 GB) | 6.23 GB |

¹ The benchmark prints times of a minute or more in minutes with two decimals (e.g. `1.06 min`), so those values are only
precise to about ±0.3 s.

² Disk space is the container's total writable-layer size (Docker `SizeRw`) at the end of the step. It is cumulative, so it
includes everything written by earlier steps (1.36 GB before the build). It was identical in every run.

### Earlier steps

These steps were the same in every case, so all 12 runs are summarized together:

| Step | Execution time (median, range) | Peak memory (range) | Disk space² |
|---|---|---|---|
| Instantiate (cap 4) | 2.92 s (2.58 – 3.53 s) | 0.31 – 0.60 GB | 6.21 MB |
| CloneAndRestoreRepo | 12.98 s (12.25 – 13.70 s) | 1.87 – 1.89 GB | 1.34 GB |
| Restore (cap 2) | 17.53 s (16.39 – 18.21 s) | 2.35 – 2.42 GB | 1.36 GB |

These match the earlier instantiation and restoration benchmarks.

### Observations

- **Capping at half the CPU count (2) was the fastest again**, about 7% faster than sequential (54.46 s vs 58.79 s) for
  about 0.2 GB more peak memory.
- **More concurrency made the build slower.** Capping at the CPU count (4) took about 12% longer than sequential, and
  building all 10 at once took **about 1.7× as long** (98.40 s) with a **12.03 GB** memory peak, three quarters of a
  16 GB runner.
- **A single `dotnet build` already uses all 4 CPUs.** It builds the solution's projects in parallel with one MSBuild
  worker per CPU, and the compiler parallelizes too. Running a second build alongside only fills the gaps where one
  build waits, e.g. on a project its next projects depend on. Beyond that, builds compete for the same CPUs.
- **Building 10 solutions writes about 4.9 GB** (1.36 GB → 6.23 GB), roughly 490 MB per solution. Concurrency doesn't
  change that.
- **Every build prints an `ASPIRE010` warning** for the `AppHost` and `TestAppHost` projects ("configured with
  `AspireUseCliBundle=false`"). It isn't treated as an error, so the builds succeed.

### How the time is spread across the 10 builds

`dotnet build` reports its own duration (`Time Elapsed`). In start order, from run 2 of each case (the other runs show
the same pattern):

| Max concurrency | Per-build duration (s) |
|---|---|
| 1 | **10.8**, 6.6, 5.3, 5.2, 4.3, 5.0, 4.9, 4.8, 4.2, 4.5 |
| 2 | **15.1, 15.1**, 11.6, 11.5, 8.6, 8.9, 8.2, 9.1, 8.6, 7.8 |
| 4 | **28.8, 29.0, 28.1, 29.2**, 23.5, 24.6, 24.5, 24.5, 10.7, 9.7 |
| 10 | 97.5, 97.1, 95.9, 97.1, 96.6, 97.4, 96.1, 96.1, 96.4, 95.7 |

- **The first build is the slowest** in every case: about 11 s alone versus about 4.5 s for later ones. It likely pays
  for starting the MSBuild workers and the compiler server, which `dotnet build` keeps running and reuses for the
  builds that follow.
- **Running two at a time roughly doubled each build's duration** (about 8.5 s instead of about 4.5 s), so two in
  parallel only gained a little over one after the other, consistent with each build already using all CPUs.
- **With 4 or 10 at once, each build took far longer than its share**, so the overhead of competing builds outweighed
  any overlap.

### Per-run values (build step)

| Case | Run | Execution time | Peak memory | Log |
|---|---|---|---|---|
| `Sequential` | 1 | 1.06 min | 8.12 GB | [sequential-1.log](runs/sequential-1.log) |
| | 2 | 58.79 s | 8.06 GB | [sequential-2.log](runs/sequential-2.log) |
| | 3 | 58.47 s | 8.02 GB | [sequential-3.log](runs/sequential-3.log) |
| `ConcurrentCappedToHalfCpuCount` | 1 | 1.05 min | 8.26 GB | [concurrent-capped-to-half-cpu-count-1.log](runs/concurrent-capped-to-half-cpu-count-1.log) |
| | 2 | 54.46 s | 8.26 GB | [concurrent-capped-to-half-cpu-count-2.log](runs/concurrent-capped-to-half-cpu-count-2.log) |
| | 3 | 52.54 s | 8.26 GB | [concurrent-capped-to-half-cpu-count-3.log](runs/concurrent-capped-to-half-cpu-count-3.log) |
| `ConcurrentCappedToCpuCount` | 1 | 1.17 min | 9.27 GB | [concurrent-capped-to-cpu-count-1.log](runs/concurrent-capped-to-cpu-count-1.log) |
| | 2 | 1.08 min | 9.28 GB | [concurrent-capped-to-cpu-count-2.log](runs/concurrent-capped-to-cpu-count-2.log) |
| | 3 | 1.10 min | 9.29 GB | [concurrent-capped-to-cpu-count-3.log](runs/concurrent-capped-to-cpu-count-3.log) |
| `ConcurrentUncapped` | 1 | 1.79 min | 12.37 GB | [concurrent-uncapped-1.log](runs/concurrent-uncapped-1.log) |
| | 2 | 1.64 min | 11.96 GB | [concurrent-uncapped-2.log](runs/concurrent-uncapped-2.log) |
| | 3 | 1.62 min | 12.03 GB | [concurrent-uncapped-3.log](runs/concurrent-uncapped-3.log) |

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

The benchmark container is limited to `Host.MaxCpu` = 4 CPUs. Its memory limit was lowered to 14 GB for these runs (see
[run conditions](#run-conditions)). The concurrency caps are derived from `Host.MaxCpu` (4 and 2), so they're the same on
any machine. The host is about the size of a GitHub-hosted `ubuntu-latest` runner for public repositories (4 vCPU,
16 GB RAM).

## Run conditions

The sandbox differs from a normal machine in ways that required local-only changes. None of them are committed.

- **Container memory was capped at 14 GB instead of `Host.MaxMemory` (16 GB).** This host has only 15.72 GB of RAM, so
  with a 16 GB limit, running out of memory would have hit the whole machine rather than the container. The highest peak
  was 12.37 GB, so the cap never came into play and didn't affect the results.
- **Network access goes through the sandbox's proxy.** The containers were run on the host network and trusted the
  proxy's certificate, so `git clone` and NuGet downloads went through that proxy. This only affects the earlier steps;
  the build step runs with `--no-restore` and doesn't download anything.
- **The template was installed from a local package.** It was packed on the host with `dotnet pack nuspec.csproj`,
  copied into the container, and installed from the `.nupkg`, the same way [`ci.yml`](../../../../.github/workflows/ci.yml)
  installs it, instead of from nuget.org.
- **`StorageOpt size` (14G) is not enforced.** Docker's containerd image store silently ignores it, and this kernel was
  built without the XFS quota support the classic `overlay2` driver needs. The builds used at most 6.23 GB, under the
  limit. Disk usage is still measured correctly through `SizeRw`.
- **Peak memory** is read from cgroup v1 `memory.max_usage_in_bytes`, which is reset before each step. It includes the
  page cache, i.e. the kernel's cache of file data. The build writes about 4.9 GB of output, so a large share of the
  build peaks is likely cached file data rather than memory used by the build processes themselves.
