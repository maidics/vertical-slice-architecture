```

BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.6466/22H2/2022Update)
Intel Core i7-9700K CPU 3.60GHz (Coffee Lake), 1 CPU, 8 logical and 8 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3


```

# Record Result

| Method          | Mean           | Error      | StdDev     | Gen0    | Gen1   | Allocated |
|---------------- |---------------:|-----------:|-----------:|--------:|-------:|----------:|
| Async           |     15.9909 ns |  0.0613 ns |  0.0511 ns |  0.0051 |      - |      32 B |
| AsyncFailure    |     16.5648 ns |  0.0330 ns |  0.0276 ns |  0.0102 |      - |      64 B |
| AsyncGeneric    |     16.5737 ns |  0.0447 ns |  0.0418 ns |  0.0063 |      - |      40 B |
| Escaping        |  8,489.0810 ns | 19.1079 ns | 16.9386 ns |  5.0964 | 0.5646 |   32000 B |
| EscapingFailure | 11,352.6573 ns | 44.3251 ns | 37.0135 ns | 10.1929 | 2.0294 |   64000 B |
| EscapingGeneric |  8,127.2007 ns | 16.5738 ns | 13.8398 ns |  6.3629 | 0.9003 |   40000 B |
| Local           |      0.0037 ns |  0.0034 ns |  0.0032 ns |       - |      - |         - |
| LocalFailure    |      2.2687 ns |  0.0190 ns |  0.0168 ns |  0.0051 |      - |      32 B |
| LocalGeneric    |      0.0015 ns |  0.0018 ns |  0.0017 ns |       - |      - |         - |

---

```

BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.6466/22H2/2022Update)
Intel Core i7-9700K CPU 3.60GHz (Coffee Lake), 1 CPU, 8 logical and 8 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3

```

# Struct Result

| Method          | Mean          | Error      | StdDev     | Median        | Gen0   | Gen1   | Allocated |
|---------------- |--------------:|-----------:|-----------:|--------------:|-------:|-------:|----------:|
| Async           |    12.9589 ns |  0.0341 ns |  0.0319 ns |    12.9789 ns |      - |      - |         - |
| AsyncFailure    |    15.3119 ns |  0.0355 ns |  0.0297 ns |    15.3150 ns | 0.0051 |      - |      32 B |
| AsyncGeneric    |    12.9378 ns |  0.0347 ns |  0.0325 ns |    12.9531 ns |      - |      - |         - |
| Escaping        | 4,847.8809 ns |  9.2527 ns |  8.2023 ns | 4,849.0742 ns |      - |      - |         - |
| EscapingFailure | 8,237.2666 ns | 48.2323 ns | 42.7567 ns | 8,230.5038 ns | 5.0964 | 0.5646 |   32000 B |
| EscapingGeneric | 4,827.9455 ns | 19.4091 ns | 18.1553 ns | 4,830.9601 ns |      - |      - |         - |
| Local           |     0.0011 ns |  0.0019 ns |  0.0017 ns |     0.0000 ns |      - |      - |         - |
| LocalFailure    |     0.6571 ns |  0.0065 ns |  0.0060 ns |     0.6584 ns |      - |      - |         - |
| LocalGeneric    |     0.0027 ns |  0.0030 ns |  0.0028 ns |     0.0022 ns |      - |      - |         - |
