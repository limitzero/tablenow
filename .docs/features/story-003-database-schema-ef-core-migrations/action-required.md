# Action Required: Database Schema & EF Core Migrations

No manual steps required for this feature. All tasks can be implemented automatically.

> Note: Applying the schema against **SQL Server (prod)** requires a reachable SQL Server instance and a `ConnectionStrings:Default` value supplied via environment configuration. For local development, the default SQLite provider needs no external service. Verifying the SQL Server path may require a developer to provide a connection string (e.g., a local Docker SQL Server) — this is environment setup, not code, and is covered by the dev's local configuration rather than committed files.
