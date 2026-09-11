---
name: dotnet
description: Use when building, testing, running, formatting, or verifying the C#/.NET solution (LogGrokCore.sln), or when editing .cs, .xaml, .csproj, appsettings.yaml, or nlog.config. Covers the dotnet CLI workflow and this repo's warnings-as-errors rules.
---

# .NET workflow (LogGrokCore)

Windows PowerShell shell, repository root `C:\ai-projects\LogGrokCore`.

## Verify a change

Always finish code changes with a clean rebuild and the full test run:

```powershell
dotnet build LogGrokCore.sln -t:Rebuild
dotnet test LogGrokCore.sln
```

For a faster inner loop, `dotnet build LogGrokCore.sln` and
`dotnet test LogGrokCore.sln` (incremental) are fine; `-t:Rebuild` before the
final check.

## Running the app

```powershell
dotnet run --project LogGrokCore\LogGrokCore.csproj
```

The app is a WPF `WinExe`; it opens a window and keeps running. It writes
diagnostic logs to `%LOCALAPPDATA%\LogGrok2\`. If it exits immediately with
code 0, another instance is already running (single-instance manager).

## Constraints

- `Nullable=enable` and `WarningsAsErrors` for `NU1605` plus the nullable
  `CS86xx` family: a valid change has **0 warnings**.
- Prefer `dotnet build`/`dotnet test` over launching MSBuild or VS.
- Do not use `cd`; pass `workdir` to the shell tool or `--project`.
- Do not add comments to code.

## When touching configuration

- `appsettings.yaml` is hot-reloaded at runtime and deserialized into settings
  classes; keep YAML keys matching the C# property names.
- `nlog.config` must stay NLog 6 compatible: no `concurrentWrites`, and
  `${threadid}` takes no properties.
- AvalonDock layout XML uses the `Dirkster.AvalonDock.Serializer.Xml` package
  and the `AvalonDock.Serializer.Xml` namespace.
