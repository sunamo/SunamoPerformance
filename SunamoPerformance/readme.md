### SunamoPerformance

Part of PlatformIndependentNuGetPackages:

- [nuget.org](https://www.nuget.org/profiles/sunamo)
- [github.org](https://github.com/sunamo/PlatformIndependentNuGetPackages)

Another links:

- [Developer site](https://sunamo.cz)

Request for new features / bug report / etc: [Mail](mailto:radek.jancik@sunamo.cz) or on GitHub

## What it is

Manual, non-benchmarking-framework performance comparisons ported from the
obsolete `sunamo.performance` console app:

- `ManyStringReplacingBenchmark` - bulk string replacing via `SHReplace.ReplaceAll3`.
- `ReadingFilesBenchmark` - sync vs async file reading via `TF`.
- `WritingFilesBenchmark` - `TF.SaveFile` vs `TF.WriteAllText`.
- `FastStringLookupBenchmark` - lookup speed of `HashSet`, `Dictionary`,
  `ConcurrentDictionary`, `List.Contains`, `List.BinarySearch` and a linear
  array scan.
- `AllCharsBenchmark` - lookup speed over a small set of special characters.
- `WrapperStringObject` - helper reference-type wrapper used by the
  HashSet-of-objects lookup benchmark.

Every method returns the elapsed milliseconds instead of blocking on
`Console.ReadLine()`, and every path is either a caller-supplied parameter
or `Path.GetTempPath()` - no hardcoded local paths.

## Target Frameworks

**TargetFrameworks:** `net10.0;net9.0;net8.0`

**Reason:** Depends on other Sunamo packages (SunamoStringSplit, SunamoFileIO, ...)
that target `net10.0;net9.0;net8.0` only, no `netstandard2.0`.
