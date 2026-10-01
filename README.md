# SunamoPerformance

## Short description

Sada ručních výkonnostních benchmarků pro porovnávání implementací běžných operací: práce s řetězci (hromadné nahrazování, vyhledávání), čtení a zápis souborů. Slouží k ověření, že zvolená implementace v ostatních Sunamo balíčcích je rychlostně opodstatněná.
Balíček je self-contained: kromě `Microsoft.Extensions.Logging.Abstractions` nereferencuje žádné jiné balíčky.

Manual performance benchmarks for string replacing, file IO and string lookup structures

## Overview

SunamoPerformance is part of the Sunamo package ecosystem, providing modular,
platform-independent utilities for .NET development.

Migrated from the obsolete `sunamo.performance` console app
(`E:\vs_ObsoleteDueToAI\Projects\sunamo.performance`): the original private
scratch project used hardcoded local paths and blocking
`Console.ReadLine()` calls. This package exposes the same comparisons as
plain, callable, non-interactive static methods.

## Main Components

### Key Classes

- **ManyStringReplacingBenchmark**
- **ReadingFilesBenchmark**
- **WritingFilesBenchmark**
- **FastStringLookupBenchmark**
- **AllCharsBenchmark**
- **WrapperStringObject**

## Installation

```bash
dotnet add package SunamoPerformance
```

## Dependencies

- **SunamoStringSplit**
- **SunamoStringGetLines**
- **SunamoStringReplace**
- **SunamoStopwatch**
- **SunamoFileIO**
- **SunamoFileSystem**
- **SunamoRandom**
- **Microsoft.Extensions.Logging.Abstractions**

## Package Information

- **Package Name**: SunamoPerformance
- **Target Framework**: net10.0;net9.0;net8.0;netstandard2.0
- **Category**: Platform-Independent NuGet Package

## Related Packages

This package is part of the Sunamo package ecosystem. For more information
about related packages, visit the main repository.

## License

See the repository root for license information.
