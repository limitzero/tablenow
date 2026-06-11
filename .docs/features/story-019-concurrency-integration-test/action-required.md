# Action Required: Concurrency Integration Test

No manual steps required for this story.

**Note:** The concurrency guarantee is strongest against SQL Server (row-version locking). SQLite uses a simpler locking model — the test will still pass because the EF concurrency exception path is exercised, but for production confidence use `Testcontainers.MsSql` (the package is already declared in `IntegrationTests.csproj`).
