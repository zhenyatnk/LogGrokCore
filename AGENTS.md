# AGENTS.md

Guidance for AI agents working in this repository.

## Project

**LogGrokCore** — a fast WPF log viewer for very large log files. It parses
structured log lines with configurable regex formats, builds in-memory indexes,
and supports search, filtering, colorization and marking while streaming files
that may be many gigabytes.

- Platform: **Windows only** (WPF).
- Target framework: `net10.0-windows` / `net10.0`.
- SDK pinned by `global.json` (10.0.100, `rollForward: latestMajor`).

## Commands

Run from the repository root unless noted. The OS shell is Windows PowerShell.

```powershell
dotnet restore
dotnet build LogGrokCore.sln
dotnet test LogGrokCore.sln
dotnet run --project LogGrokCore\LogGrokCore.csproj
dotnet format LogGrokCore.sln
```

- The build treats several warnings as errors (`NU1605` and the nullable
  `CS86xx` family). A clean build must have **0 warnings**.
- Build output is redirected to `bin\Debug\` / `bin\Release\` (not the default
  per-project `bin`).
- After changing code, always verify with `dotnet build` and `dotnet test`.
- Do not commit unless the user explicitly asks.

## Layout

| Project | Purpose |
| --- | --- |
| `LogGrokCore` | WPF app: views, view models, controls, theming, DI bootstrap. |
| `LogGrokCore.Data` | UI-agnostic core: stream loading, line parsing, indexes, search, virtualization. |
| `LogGrokCore.Tests` | Tests for the UI layer. |
| `LogGrokCore.Data.Tests` | Tests for the core data layer. |

Key areas in `LogGrokCore.Data`: `Loader`/`LoaderImpl` (buffered line-aware
reader), `RegexBasedLineParser`, `IndexTree`/`LineIndex`/`SearchLineIndex`,
`Search`/`Pipeline`, `Virtualization`.

## Conventions

- `LangVersion=latest`, `Nullable=enable`. Prefer file-scoped namespaces.
- **Do not add comments** unless they are necessary to explain non-obvious code.
- Tests use **MSTest** (`[TestClass]`, `[TestMethod]`, `[DataRow]`), not xUnit or
  NUnit. Assertion arguments are `(expected, actual)`.
- Keep changes minimal and mirror the style of neighbouring files.

## Gotchas

- **AvalonDock 5**: the XML layout serializer lives in the separate package
  `Dirkster.AvalonDock.Serializer.Xml`; its namespace is
  `AvalonDock.Serializer.Xml` (not `AvalonDock.Layout.Serialization`).
- **NLog 6**: `nlog.config` must stay compatible with NLog 6 — `concurrentWrites`
  was removed, and `${threadid}` accepts no properties.
- The app writes diagnostic logs to `%LOCALAPPDATA%\LogGrok2\`.
- Runtime configuration is `appsettings.yaml` (watched and hot-reloaded),
  next to the executable.

## Verification

Minimum bar for any change:

```powershell
dotnet build LogGrokCore.sln -t:Rebuild
dotnet test LogGrokCore.sln
```

Both must succeed with 0 warnings and 0 errors.
