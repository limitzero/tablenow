## Tech Stack
- .NET 10, ASP.NET Core Minimal API's
- Entity Framework Core 10 with SQL Server
- Mediator for CQRS pattern (source-generated)
- Fluent Validation for request validation 
- Scalar for API documentation (OpenAPI)
- xUnit and FluentAssertions for testing 

## Tech Stack Allocation

- `src/Api` is where the minimal API's will be created using .NET 10, ASP.NET Core and Scalar
- `src/Application` is where Mediator handlers are arranged by folder for feature and will contain:
    - A validator to inspect the command for basic rules
    - A request message that is passed to the handler 
    - An aggregate root that represents the unit of work to be done for the use case action
    - A response message that contains the message to be relayed to the API layer as a result of the actions from the aggregate root
- `src/Domain` - Core business context and related entities, no specific technology emphasized here
- `src/Data` - EF Core primarily for accessing entities
    - A command message that contains information that should mutate data in the persistence storage
    - A command result message that returns the result of the operation to mutate the data
    - A command handler that takes the command message and returns the command result message via the Mediator pattern
    - A query message that contains information that will be used to retreive information from the persistence storage 
    - A query result messge that contains the data in question that conforms to the criteria in the query message
    - A query handler that takes the query message and returns the query result message via the Mediator pattern

