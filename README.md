# Phoronix Result Viewer

A small Avalonia desktop app for comparing Phoronix benchmark results across systems.
It normalizes results against a base system and shows performance, power, and performance-per-watt charts.

## Requirements

- .NET SDK 10.0 (`global.json`)

## Run

```bash
dotnet run --project PhoronixResultViewer
```

## Usage

1. Download a benchmark result **JSON** file from `openbenchmarking.org`.
2. Open the app and click **Import result file**.
3. Select the downloaded JSON file.

On first use, the app also fetches suite metadata from OpenBenchmarking and caches it in `testSuites.json`.
