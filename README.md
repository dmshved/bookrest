<p align="center">
<img width="120" src="./docs/images/bookrest.png" alt="BookRest logo">
</p>

<h1 align="center">Restaurant Reservation System</h1>

BookRest is a restaurant reservation system. It provides a REST API to book restaurant tables clearly and painless.

## Table of Contents

- [Installation](#installation)
- [Core Technologies](#core-technologies)
- [Project Architecture](#project-architecture)
- [Testing approach](#testing-approach)
- [Architecture Decisions](#architecture-decisions)
- [Database schema](#database-schema)
- [License](#license)

## Installation
_in progress..._

## Core Technologies

- Framework:
    - ASP.NET Core Web API
- Languages:
    - C#
- Database:
    - PostgreSQL
    - EF Core
- IAM:
    - ASP.NET Core Identity + JWT (Refresh + Access tokens)
- Payment Service:
    - Stripe
- CQRS & Event Handling:
    - MediatR
- Validation:
    - FluentValidation
    - Ardalis.GuardClauses
- Unit Testing:
    - xUnit
    - Moq
- Integrational Testing:
    - Test Containers
    -  WebApplication Factory

## Project Architecture

This project is structured using **Clean Architecture** pattern

<p align="center">
    <img src="https://milanjovanovic.tech/blogs/mnw_004/clean_architecture.png" height="300" alt="Clean Architecture">
</p>

```
BookRest.Domain          // Entities, value objects, domain events
BookRest.Application     // Use cases, interfaces, DTOs
BookRest.Infrastructure  // Data access, external services
BookRest.Api             // Controllers, middleware, DI setup
BookRest.Shared          // Centralise service name constants
```

## Testing approach

This project follows the testing diamond framework to keep code well-tested.

<p align="center">
    <img src="https://www.code4it.dev/blog/testing-vial/the-testing-diamond.png" height="300" alt="Testing Diamond">
</p>

Following such framework project uses:
- **E2E Tests**: a small number of end-to-end (E2E) tests to verify complete application workflows.
- **Integration Tests**: the majority of tests are integration tests, covering interactions between components.
- **Unit Tests**: a small number of unit tests to verify isolated business logic.

## Architecture Decisions

While building and structuring BookRest I had to deal with various problems, they are documented at [docs/decisions](docs/decisions)

## Database schema

_in progress..._

## Contributing

Pull requests are welcome. For major changes, please open an issue first
to discuss what you would like to change.

Please make sure to update tests as appropriate.

___

## License

This software is licensed under the [MIT license](LICENSE)
