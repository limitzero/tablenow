## Project Structure
- `src\Api` - ASP.NET Core, Entry point, endpoints, middleware
- `src\Application` - Requests, responses, usecase handlers
- `src\Domain` - Entities, value objects, domain events, Aggregates, DTO's
- `src\Data` - EF Core, Commands, queries, data handlers
- `src\Contracts` - Models for entry point API
- `src\Infrastructure` - Event handlers, external services, configuration
- `src\Shared` - Reusable components and contracts for all layers
- `src\Migrations` - Scripts for data migrations
- `tests\UnitTests` - Domain and application layer tests
- `tests\IntegrationTests` - API and database tests

## Component Structure

For the `Api` layer, the folder structure should follow these rules:
- A folder named `Endpoints` that will contain all of the minimal API definitions used
- A folder named `Extensions` where the components will participate in the DI process
- A folder named `Mappers` where `Application` layer requests and responses will be translated to `Contract` 
level requests amd responses that are transmitted back via the minimal api (all manual mapping for this exercise)


For the `Application` layer, the folder structure should follow these rules:
- A folder named `Features` where each use case will have a folder under this folder
- The rules for the use case folder name are:
    - A remove all spaces and punctuation 
    - The name should be in Title Case
    - The name cannot be any longer than 40 characters long
Examples of the use case folder name for a feature is `Features\CreateTask` or `Features\UpdateTask`


## Business Contexts

Please note that we should create a folder for each business context of the application pertaining to a set of use cases. For example, if we have a 
product that needs to have a shopping cart context and check-out context, each `context` will have its own set of use cases for completion. That correspond
to the `Component Structure` definition.

Example:

For the Shopping Cart business context, we could have the following arrangement, with `<business-context>` representing the `context` under construction:

- `\Api\Endpoints\<business-context>` - API code for interaction for shopping cart related behavior
- `\Application\<business-context>\Features` - Use the `Component Structure` definition and realizaing of feature use cases
- `\Domain\<business-context>` - Components directly related to the business context 
- `\Data\<business-context>` - Actions to manage data access to the data entities
- `\Infrastructure\<business-context>` - Any configuration or external contracts to communicate with external resources



All business contexts should live in separate projects from one another with naming for each project according to the following:

`company_name.product_name.business_context`


Example Business Context: `Shopping Cart`, `Time Management`

- Rules:
    - Only `a-z`, `0-9` and `-`
    - Remove spaces and punctuation
    - Collapse multiple `-` into one
    - Trim `-` from start and end
    - Maximum length of 80 characters


Example Library Naming: 

`ACME.ExpenseTracker.TimeManagement`, `Contoso.EShop.ShoppingCart`



## Overall Component Locations

Example: For the `business context` of a shopping cart, we should have the following component libraries:

Libraries:
- `Application` - `src\Application\<company-name>.<product-name>.<business-context>.Application`
- `Domain` - `src\Domain\<company-name>.<product-name>.<business-context>.Domain`
- `Data` - `src\Data\<company-name>.<product-name>.<business-context>.Data`
- `Infrastructure` - `src\Infrastructure\<company-name>.<product-name>.<business-context>.Infrastructure`
- `MIgrations` - `src\MIgrations\<company-name>.<product-name>.<business-context>.Migrations`


