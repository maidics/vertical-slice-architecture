# Test benchmarks — 2026-10-16

Benchmark: `VsaTemplate.Pipeline.Benchmarks`, `Tests` cases, run at commit `be8ab2a` with the local workarounds
described [below](#run-conditions).

Each case builds on the best results of the previous benchmarks:

1. **Instantiate** 10 solutions with `dotnet new vsa-sln --no-update-check`, capped at 4 at a time
   ([instantiation benchmarks](../2026-10-16-instantiations/README.md)).
2. **CloneAndRestoreRepo**: `git clone` of this repository's default branch and `dotnet restore` of `VsaTemplate.slnx`.
3. **Restore** the 10 solutions, capped at 2 at a time ([restoration benchmarks](../2026-10-16-restorations/README.md)).
4. **Build** the 10 solutions with `--no-restore`, capped at 2 at a time ([build benchmarks](../2026-10-16-builds/README.md)).
5. **Test** the 10 solutions at the case's concurrency. This is the step being compared. Each command runs
   `cd <name> && dotnet test --solution <name>.slnx --no-restore --no-build --ignore-exit-code 8` (see
   [why](#why-the-test-command-looks-like-this)).

Each case was run **3 times**, every run in a fresh container with no warmup. The tables show the median and the range
across the runs. **Every test run passed: 81 tests per solution, 810 per run, 0 failed.** All 120 test commands exited
with code `0`. 6 commands in earlier steps failed for a reason unrelated to the test step; see
[run conditions](#run-conditions).

## Results

### Test step

| Case | Max concurrency | Execution time (median, range)¹ | Peak memory (median, range) | Disk space² |
|---|---|---|---|---|
| `Sequential` | 1 | **5.85 min** (5.84 – 5.85 min) | **8.56 GB** (8.44 – 8.63 GB) | 6.24 GB |
| `ConcurrentCappedToHalfCpuCount` | 2 | **3.20 min** (3.13 – 3.20 min) | **8.95 GB** (8.88 – 9.07 GB) | 6.24 GB |
| `ConcurrentCappedToCpuCount` | 4 | **2.19 min** (2.16 – 2.26 min) | **10.13 GB** (9.81 – 10.24 GB) | 6.24 GB |
| `ConcurrentUncapped` | `int.MaxValue` (all 10 at once) | **1.46 min** (1.44 – 1.49 min) | **12.56 GB** (12.51 – 12.65 GB) | 6.24 GB |

¹ The benchmark prints times of a minute or more in minutes with two decimals, so these values are only precise to
about ±0.3 s.

² Disk space is the container's total writable-layer size (Docker `SizeRw`) at the end of the step. It is cumulative, so it
includes everything written by earlier steps. The test step itself adds only about 10 MB. Runs affected by the failed
repository restore (see [run conditions](#run-conditions)) end at 6.07 GB instead, because the repository's packages
were never downloaded.

### Earlier steps

These steps were the same in every case. The 6 runs where the repository's restore failed are shown separately:

| Step | Normal runs (6): time (median, range) | Runs with the failed repository restore (6) |
|---|---|---|
| Instantiate (cap 4) | 3.25 s (2.90 – 3.76 s) | 3.17 s (2.88 – 3.24 s) |
| CloneAndRestoreRepo | 13.38 s (12.68 – 13.85 s) | 4.90 s (4.66 – 5.11 s): clone only, restore failed |
| Restore (cap 2) | 18.17 s (17.99 – 18.76 s) | 25.52 s (24.72 – 26.74 s): fewer packages already cached |
| Build (cap 2) | 57.10 s (55.72 – 62.40 s) | 53.30 s (52.74 – 59.65 s) |

The normal runs match the earlier restoration and build benchmarks. In the affected runs, the 10 solutions still
restored, built and passed all their tests, and **the test step took the same time with or without the failure** (see
the per-run table).

### Observations

- **Unlike restore and build, tests get faster the more of them run at once.** Running all 10 at once was **4.0×
  faster** than sequential (1.46 min vs 5.85 min). Capping at the CPU count (4) was 2.7× faster, and at half the CPU
  count (2) 1.8× faster.
- **Test runs are mostly waiting, not computing.** Every solution's test run took **about 34 s on its own**, almost
  identical across all 30 solution test runs in the `Sequential` case (33.6 – 35.5 s). Both `UnitTests` and `WebTests` first start the Aspire
  `TestAppHost` (`DistributedApplicationTestingBuilder`), wait for its SQLite resource to be healthy, run their tests,
  and shut the app down again. The two projects run in parallel and each takes about 33 s. While a test run waits, the
  CPUs are free for other test runs.
- **Running more at once costs memory, not much time per run.** With 2 at a time each test run took about 37 s (vs
  34 s alone), with 4 about 45 s, and with all 10 about 80 s, still far shorter overall than running them one after
  another. Peak memory rose from 8.56 GB to **12.56 GB**, more than three quarters of a 16 GB runner.
- **The tests barely write to disk**, about 10 MB on top of the build output.

### How the time is spread across the 10 test runs

Each `dotnet test` reports its duration. In start order, from run 2 of each case (the other runs show the same pattern):

| Max concurrency | Per-solution test duration (s) |
|---|---|
| 1 | 35.1, 34.1, 34.3, 34.1, 34.0, 33.7, 34.1, 34.0, 34.1, 34.4 |
| 2 | 39.1, 38.6, 36.5, 36.0, 37.6, 37.3, 35.7, 37.3, 34.8, 34.7 |
| 4 | 49.0, 48.9, 49.3, 45.9, 38.8, 44.8, 43.5, 43.8, 34.3, 34.0 |
| 10 | 83.3, 82.9, 80.9, 77.9, 80.3, 79.9, 83.4, 81.1, 84.3, 79.4 |

### Per-run values (test step)

| Case | Run | Execution time | Peak memory | Failed repo restore | Log |
|---|---|---|---|---|---|
| `Sequential` | 1 | 5.84 min | 8.56 GB | | [sequential-1.log](runs/sequential-1.log) |
| | 2 | 5.85 min | 8.63 GB | | [sequential-2.log](runs/sequential-2.log) |
| | 3 | 5.85 min | 8.44 GB | yes | [sequential-3.log](runs/sequential-3.log) |
| `ConcurrentCappedToHalfCpuCount` | 1 | 3.20 min | 9.07 GB | | [concurrent-capped-to-half-cpu-count-1.log](runs/concurrent-capped-to-half-cpu-count-1.log) |
| | 2 | 3.20 min | 8.95 GB | yes | [concurrent-capped-to-half-cpu-count-2.log](runs/concurrent-capped-to-half-cpu-count-2.log) |
| | 3 | 3.13 min | 8.88 GB | yes | [concurrent-capped-to-half-cpu-count-3.log](runs/concurrent-capped-to-half-cpu-count-3.log) |
| `ConcurrentCappedToCpuCount` | 1 | 2.26 min | 10.24 GB | | [concurrent-capped-to-cpu-count-1.log](runs/concurrent-capped-to-cpu-count-1.log) |
| | 2 | 2.19 min | 9.81 GB | yes | [concurrent-capped-to-cpu-count-2.log](runs/concurrent-capped-to-cpu-count-2.log) |
| | 3 | 2.16 min | 10.13 GB | yes | [concurrent-capped-to-cpu-count-3.log](runs/concurrent-capped-to-cpu-count-3.log) |
| `ConcurrentUncapped` | 1 | 1.44 min | 12.65 GB | | [concurrent-uncapped-1.log](runs/concurrent-uncapped-1.log) |
| | 2 | 1.49 min | 12.56 GB | | [concurrent-uncapped-2.log](runs/concurrent-uncapped-2.log) |
| | 3 | 1.46 min | 12.51 GB | yes | [concurrent-uncapped-3.log](runs/concurrent-uncapped-3.log) |

## Why the test command looks like this

- **`cd <name>` and `--solution`:** the solutions use Microsoft.Testing.Platform, enabled by
  `"test": { "runner": "Microsoft.Testing.Platform" }` in their `global.json`. `dotnet test` only reads that setting
  from `global.json` in the directory it's started in. Started from `/`, it falls back to the old VSTest mode, which the
  test projects reject on the .NET 10 SDK. CI doesn't hit this because it runs `dotnet test` from the repository root,
  whose `global.json` has the same setting.
- **`--ignore-exit-code 8`:** exit code 8 means "a test project ran zero tests". Without the examples option (`-e`),
  `IntegrationTests` contains no tests. CI instantiates with `-e`, so it doesn't need this.

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

- **The repository's restore failed in 6 of the 12 runs.** The benchmark containers added a local `NuGetAuditSuppress`
  entry to every restore. While the runs were in progress, `main` gained the same entry in its own
  `Directory.Build.props` (`e11bdb8`, "build: temporarily ignore MessagePack vulnerability"). From then on, the
  `CloneAndRestoreRepo` step's `dotnet restore` of the cloned repository saw the entry twice and failed with `NU1508`
  (duplicate items, treated as an error). The generated solutions don't contain the entry, so their restore, build and
  tests were unaffected. The only effect was that the 10 restores started with fewer packages cached (see
  [earlier steps](#earlier-steps)).
- **Container memory was capped at 14 GB instead of `Host.MaxMemory` (16 GB).** This host has only 15.72 GB of RAM, so
  with a 16 GB limit, running out of memory would have hit the whole machine rather than the container. The highest peak
  was 12.65 GB, so the cap never came into play.
- **Network access goes through the sandbox's proxy.** The containers were run on the host network and trusted the
  proxy's certificate, so `git clone` and NuGet downloads went through that proxy. This only affects the earlier steps;
  the test step downloads nothing.
- **The template was installed from a local package.** It was packed on the host with `dotnet pack nuspec.csproj`,
  copied into the container, and installed from the `.nupkg`, the same way [`ci.yml`](../../../../.github/workflows/ci.yml)
  installs it, instead of from nuget.org.
- **`StorageOpt size` (14G) is not enforced.** Docker's containerd image store silently ignores it, and this kernel was
  built without the XFS quota support the classic `overlay2` driver needs. The runs used at most 6.24 GB, under the
  limit. Disk usage is still measured correctly through `SizeRw`.
- **Peak memory** is read from cgroup v1 `memory.max_usage_in_bytes`, which is reset before each step. It includes the
  page cache, i.e. the kernel's cache of file data. The test step starts right after a build that wrote about 4.9 GB, so
  part of every test peak is likely cached file data rather than memory used by the test processes.
