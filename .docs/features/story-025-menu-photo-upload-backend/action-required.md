# Action Required: Menu Photo Upload — Backend

## Before Implementation

- [ ] **Configure storage path** — Ensure `wwwroot/photos/` directory will be created at startup. For production, set `PhotoStorage__BasePath` or configure blob storage credentials.

## After Implementation

- [ ] Run new EF migration after task-01: `dotnet ef migrations add AddPhotos --project server/src/Migrations`
