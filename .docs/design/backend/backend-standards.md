@backend-project-structure.md
@backkend-tech-stack.md
@backed-naming-standards.md
@backend-patterns.md

### Request flow

```
HTTP → Minimal API Endpoint
     → IMediator.Send(AppRequest)       [Application layer]
     → AppRequestHandler
     → IMediator.Send(DataQuery/Command) [Data layer]
     → EF Core DbContext → SQL Server
```

Each module self-registers via `Add[Context]Module()` extension method called from `ServiceCollectionExtensions.RegisterServices()`.

## What to avoid

- Do not use AutoMapper in module code.
- Do not introduce a repository pattern — use `DbContext` directly.
- Do not reference one module's Application/Data project from another module.
- Do not split EF entity into Persistence + Domain model.