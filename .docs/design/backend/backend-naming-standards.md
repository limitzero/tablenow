## Conventions

**Naming:**
- Commands: `Create[Entity]Command`, Queries: `Get[Entity]Query`
- Application: `[UseCase]Request` / `[UseCase]Response` / `[UseCase]RequestHandler`
- Data handlers: `[Command/Query]Handler` in `Commands/[UseCase]/` or `Queries/[UseCase]/`

**Code style:** 
Primary constructors for DI, records for DTOs and commands, file-scoped namespaces, always pass `CancellationToken` to async methods, nullable enabled.

**Tests — BDD naming:**
- Namespace: `describe_[feature]` (without any suffixes i.e. `Handler`) in lower case
- Class: `when_[scenario]`
- Method: `it_should_[expectation]`
- Unit test base: `module_fixture` (mocked `IMediator`); integration test base: `api_fixture` (`WebApplicationFactory<Program>`)
- Shared data fixtures: `data_handler_fixture` (DbSet mock), `data_context_fixture` (EF in-memory)