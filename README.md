# LogGrokCore

[![Run Unit tests](https://github.com/zhenyatnk/LogGrokCore/actions/workflows/run-tests.yml/badge.svg)](https://github.com/zhenyatnk/LogGrokCore/actions/workflows/run-tests.yml)
[![Upload Binaries](https://github.com/zhenyatnk/LogGrokCore/actions/workflows/build_upload.yml/badge.svg)](https://github.com/zhenyatnk/LogGrokCore/actions/workflows/build_upload.yml)

A fast WPF log viewer for very large log files. LogGrokCore parses structured
log lines with configurable regular expressions, builds in-memory indexes for
columns and fields, and lets you search, filter, colorize and mark lines while
remaining responsive even on multi-gigabyte files.

## Features

- **Large file support** — files are streamed and indexed in the background
  instead of being loaded into memory; UI virtualization keeps scrolling fast.
- **Configurable log formats** — describe your log layout with named-capture
  .NET regular expressions. Multiple formats can be configured, each with its
  own indexed fields.
- **Column / field indexing** — fields declared under `IndexedFields` are
  available as filterable columns and as search facets.
- **Search** — regex search with a progress indicator, result navigation and an
  autocomplete cache. Search patterns can be saved and reused.
- **Filtering** — filter by indexed column values directly from the grid, with
  removable filter chips.
- **Time filter & timeline** — a minimap/timeline strip (top or bottom) to
  filter by time range (or line numbers when no timestamps are available) and to
  navigate through marked lines.
- **JSON folding** — multi-line JSON blobs and oversized strings are formatted
  and can be expanded/collapsed inline. The folding state is shared across the
  log grid, search results and the marked-lines view of the same document.
- **Color rules** — highlight matching lines and text with rules in
  `appsettings.yaml`; colors adapt to the active theme.
- **Marked lines** — mark interesting lines and browse them in a dedicated view.
- **Text transformations** — rewrite matched fragments of a line before display
  (for example Base64/JSON decoding) via `Transformations`.
- **XOR-masked logs** — transparently de-obfuscate XOR-encoded log files.
- **Light & dark themes** — switch theme from the title bar; chrome, log colors
  and search highlighting follow the active theme.
- **Crash dumps** — optional Windows Error Reporting local dumps for diagnostics.
- **Multiple documents** — dockable tabs powered by AvalonDock.

## Requirements

- Windows
- .NET SDK 10.0.100 or later (see `global.json`)
- Visual Studio 2022+ or the .NET CLI

## Building

```powershell
dotnet restore
dotnet build --configuration Release
```

The build output is written to `bin\Release\`. Run the application with:

```powershell
dotnet run --project LogGrokCore\LogGrokCore.csproj
```

## Testing

The solution contains two test projects: `LogGrokCore.Tests` and
`LogGrokCore.Data.Tests`.

```powershell
dotnet test
```

## Project layout

| Project              | Description                                                       |
| -------------------- | ----------------------------------------------------------------- |
| `LogGrokCore`        | WPF application: views, view models, controls, theming.           |
| `LogGrokCore.Data`   | Platform-agnostic core: stream loading, line parsing, indexes, search, virtualization. |
| `LogGrokCore.Tests`  | Tests for the UI layer.                                           |
| `LogGrokCore.Data.Tests` | Tests for the core data layer.                                |

The UI is built on [WPF-UI](https://github.com/lepoco/wpfui) (Fluent controls
and theming) with [AvalonDock](https://github.com/Dirkster99/AvalonDock) for
docking.

### `LogGrokCore.Data`

- `Loader` / `LoaderImpl` — buffered, line-aware stream reader.
- `RegexBasedLineParser` — parses lines using the configured log formats.
- `IndexTree`, `LineIndex`, `SearchLineIndex` — in-memory indexes and
  search-result to source-line mapping.
- `Search` / `Pipeline` — asynchronous regex search pipeline.
- `Virtualization` — `IItemProvider`/`VirtualList` abstractions consumed by the UI.

## Configuration

Settings live in `appsettings.yaml`, next to the executable. The file is
watched and reloaded at runtime. Open it from the app with the
**settings** button in the title bar.

```yaml
Settings:
  DebugSettings:
    EnableCrashDumps: false
    MaxDumpsCount: 10

  ColorSettings:
    Rules:
      - RegexString: \tERR\t
        ForegroundColor: Red
      - RegexString: (?i)fatal
        BackgroundColor: "#FFCD5C5C"

  ViewSettings:
    # BigLine: prune|break
    BigLine: prune
    BigLineSize: 4096

  LogFormats:
    - Regex: ^(?<Time>\d{4}-\d{2}-\d{2}\s[^\s]+)\s+(?<Level>[^\s]+)\s+(?<Thread>[^\s]+)\s+(?<Component>[^\s]+)\s+(?<Message>.*)
      IndexedFields:
        - Level
        - Thread
        - Component
```

### Log format options

| Option            | Description                                                        |
| ----------------- | ------------------------------------------------------------------ |
| `Regex`           | Named-capture regex describing one logical log line.               |
| `IndexedFields`   | Capture group names exposed as indexed/filterable columns.         |
| `Transformations` | Ordered rewrite rules applied to matched line fragments.           |
| `XorMask`         | XOR mask used to decode encrypted log files.                       |

## Downloads

Build artifacts are produced by the **Upload Binaries** workflow and attached to
the workflow run under `LogGrokCore-build-<run_number>`.
