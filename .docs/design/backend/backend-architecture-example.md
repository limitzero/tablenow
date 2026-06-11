## Example: Modular Monolith Feature Creation

For a given feature say creating a new user, the construction of the feature on the backend would look something like this, where the `Business` and `Product`
monikers are extracted from the product definition (PRD), and the `Context` in this scenario conforms to the use case of `Create New User` normalized as `CreateUser`:


**Domain Layer**: [Business].[Product].Modules.[Context].Domain
**Folder Location** : \Entities

```csharp
public class User
{
    // any metadata that describes this concept for business processing
}
```


**Application Layer** : [Business].[Product].Modules.[Context].Application
**Folder Location** : \Features\CreateUser

```csharp
public class CreateNewUserRequest : IRequest<CreateUserResponse>
{}

public class CreateNewUserResponse
{}

public class CreateNewUserRequestHandler : IRequestHandler<CreateUserResponse,CreateNewUserResponse>
{
    public ValueTask<CreateNewUserResponse> Handle(CreateUserRequest request, CancellationToken token = default)
    {
        // logic here to handle the request and return response...
    }
}

```

If the feature or use case needs to have data retrieved or persisted to accomplish the said functionality, then a `Data` layer project 
needs to be created to carry out this operation (the associated `Command\Query` mechanisms apply here) along with the EF Core associated 
DbContext representation as a constructor input.

**Data Layer** : [Business].[Product].Modules.[Context].Data
**Folder Location** : \Features\CreateUser

```csharp
public class CreateNewUserCommand : IRequest<CreateUserCommandResponse>
{
    // associated data to fulfill request...
}

public class CreateUserCommandResponse
{
    // associated data to verify the request...
}

public class CreateNewUserCommandHandler : IRequestHandler<CreateNewUserCommand,CreateUserCommandResponse>
{
    private AppDbContext _context 

    public CreateNewUserCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public ValueTask<CreateUserCommandResponse> Handle(CreateUserRequest request, CancellationToken token = default)
    {
        // perform the data operations for the entity/entities in question and return the response...
        return ValueTask.FromResult(new CreateUserCommandResponse());
    }
}

```

**API Layer** : [Business].[Product].Modules.[Context].Endpoints
- Minimal API registration + static mapper

```csharp
internal static class CustomerEndpoints
{
    internal static RouteGroupBuilder AddCustomerEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/customers", CreateCustomer)
            .Produces<CreateRestaurantResponse>((int)HttpStatusCode.Created)
            .Produces<CreateRestaurantResponse>((int)HttpStatusCode.BadRequest)
            .WithSummary("Creates a customer");
    }
        

    private static async Task<IResult> CreateCustomer(
        [FromBody] CreateCustomerEndpointRequest request,
        IMediator mediator)
    {
        // custom mapper from endpoint request model to application request model
        var appRequest = CustomerMapper.ToAppRequest(request); 

        // dispatch request message to application-level request handler
        var response = await mediator.Send(appRequest);

        // translate the application-level response to appropriate api contract version model
        var result = RestaurantsMapper.ToV1Response(response);

        // dispatch the endpoint response model to client:
        return Dispatch(result, response.Result?.StatusCode,
            $"/api/v1/customers/{response.Result?.Data}");
    }

    private static IResult Dispatch<T>(T data, int? statusCode, string? locationUrl = null) =>
        statusCode switch
        {
            (int)HttpStatusCode.Created => Results.Created(locationUrl, data),
            (int)HttpStatusCode.NotFound => Results.NotFound(data),
            (int)HttpStatusCode.BadRequest => Results.BadRequest(data),
            (int)HttpStatusCode.InternalServerError => Results.StatusCode((int)HttpStatusCode.InternalServerError),
            _ => Results.Ok(data)
        };
} 
```

Please adhere to this structure and extrapolate where possible to complete the task or feature to its entirety. 