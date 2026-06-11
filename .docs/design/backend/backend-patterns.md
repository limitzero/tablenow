## Key patterns

**CQRS via Mediator:** Application handlers send `DataQuery`/`DataCommand` requests; data handlers execute against EF DbContext directly. No repository pattern.

**Result<T>:** All handlers return `Result<T>` from `CM.OpenTable.Common`. Fields: `Data`, `Errors`, `StatusCode`, `IsSuccess`. Use `TypedResultHelper` to translate to `IResult` at the endpoint. Never throw for business logic failures.

**Static mappers:** Each module's Endpoints project has a static `[Context]Mapper` class for API Model ↔ Application request/response translation. AutoMapper has been removed from modules; existing `AutoMapper` profiles serve as the porting spec when migrating classic features.

**One EF model per entity:** Lives in `Modules.[Context].Data/Models/`. EF annotations on the model; Fluent API in `Configurations/`. `OnModelCreating` uses `ApplyConfigurationsFromAssembly`. No separate Persistence/Entity split.

**Cross-module communication:** Reference only the target module's `Contracts.Public` assembly and dispatch via IMediator. Never reference another module's Application or Data projects directly.

**Endpoint registration:** Minimal APIs grouped under `api/v1`. Endpoints declared as static classes with a `Map[Context]Endpoints(RouteGroupBuilder)` method.
