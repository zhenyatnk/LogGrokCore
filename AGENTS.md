# AGENTS.md

Guidance for AI agents working in this repository.

## Project

**LogGrokX** — a fast WPF log viewer for very large log files. It parses
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
dotnet build LogGrokX.sln
dotnet test LogGrokX.sln
dotnet run --project LogGrokX\LogGrokX.csproj
dotnet format LogGrokX.sln
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
| `LogGrokX` | WPF app: views, view models, controls, theming, DI bootstrap. |
| `LogGrokX.Data` | UI-agnostic core: stream loading, line parsing, indexes, search, virtualization. |
| `LogGrokX.Tests` | Tests for the UI layer. |
| `LogGrokX.Data.Tests` | Tests for the core data layer. |

Key areas in `LogGrokX.Data`: `Loader`/`LoaderImpl` (buffered line-aware
reader), `RegexBasedLineParser`, `IndexTree`/`LineIndex`/`SearchLineIndex`,
`Search`/`Pipeline`, `Virtualization`.

The UI layer uses **WPF-UI 4.3.0** (Fluent controls/theming) and
**AvalonDock 5** for docking. Branding/window title is **LogGrokX** plus the
build version: `BuildInfo.Version` (release `-p:Version`, default `2.1`).
JSON folding lives in `Controls/TextRender` (`TextView`,
`TextViewSharedFoldingState`, `CollapsibleRegionsMachine`, `FoldingManager`).

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
- **WPF-UI theming**: switch themes through `ApplicationThemeManager` /
  `Theming/UiThemeService.cs`. Reference `DynamicResource` brushes
  (`ApplicationBackgroundBrush`, `TextFillColorPrimaryBrush`,
  `ControlStrokeColorDefaultBrush`, …) instead of hard-coded colors so both
  themes work. WPF-UI ships *keyed* styles (e.g. `UiGridViewColumnHeaderStyle`),
  so implicit styles are not always picked up — define overrides explicitly in
  `Bootstrap/App.xaml`.
- **AvalonDock themes**: the main `DockingManager` uses `Vs2013LightTheme` /
  `Vs2013DarkTheme`, but the search pane's inner `DockingManager` merges the
  light `AvalonDock.Themes.Metro` theme. Metro keys (e.g.
  `AvalonDock_ThemeMetro_BaseColor5`) therefore sometimes need theme-aware
  overrides in `Styles/DocumentSearchTemplate.xaml`.
- **JSON folding state** is shared per opened document: `DocumentContainer`
  registers one `TextViewSharedFoldingState` (exposed as `FoldingState` on
  `LogViewModel` / `SearchDocumentViewModel` / `DocumentViewModel`) and the
  templates bind it via `textRender:TextView.SharedFoldingState`. The marked-lines
  view must use `Document.FoldingState` and the same `TextModel.UniqueId` as the
  grid's JSON component, otherwise expansion falls out of sync.
- The app writes diagnostic logs to `%LOCALAPPDATA%\LogGrokX\`.
- Runtime configuration is `appsettings.yaml` (watched and hot-reloaded),
  next to the executable.
- **Build version** is injected by the `GenerateBuildInfo` / `GenerateAppManifest`
  targets in `LogGrokX.csproj`: they write `BuildInfo.g.cs`, `VersionInfo.g.cs`
  and a version-stamped `LogGrokX.generated.manifest`. The manifest is fed to
  the compiler via `Win32Manifest` (overriding `ApplicationManifest` alone is not
  enough, the SDK snapshots it at evaluation). `LogGrokX.exe --version` prints
  the version and exits before WPF starts; the release workflow smoke-tests it.

## Verification

Minimum bar for any change:

```powershell
dotnet build LogGrokX.sln -t:Rebuild
dotnet test LogGrokX.sln
```

Both must succeed with 0 warnings and 0 errors.
