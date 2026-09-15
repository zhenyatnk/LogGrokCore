---
description: Rebuild the whole solution and run all tests
agent: build
---

Rebuild `LogGrokX.sln` and run the full test suite.

1. Run `dotnet build LogGrokX.sln -t:Rebuild`.
2. Run `dotnet test LogGrokX.sln`.
3. Report the number of warnings/errors and the test pass/fail counts.
4. If anything fails, show the relevant compiler or test output and propose a fix.

The build must end with 0 warnings and 0 errors.
