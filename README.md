<div align="center">

## 📜 bookrest
This is a REST API restaurant table booking system made using

### `ASP.NET Core Web API`
</div>

Bookrest is a complex and modern restaurant table booking REST API. It helps you to book restaurant table clearly and painless.

## Table of Contents

- [Core Technologies](#core-technologies) - _in progress..._
- [Features](#features) - _in progress..._
- [Project Architecture](#project-architecture) - _in progress..._
- [Database schema](#database-schema) - _in progress..._
- [License](#license)

## Core Technologies

- Framework:
    - ASP.NET Core Web API
- Languages:
    - C#
- Database:
    - PostgreSQL
    - EF Core
    - Redis
- IAM:
    - Identity 2FA + JWT
- Payment Service:
    - Stripe
- Emails Handling:
    - MailKit
- Event Handling:
    - MediatR
- Queue Handling:
    - Hangfire
- HealthChecks:
    - HealthChecks
- Logging:
    - Serilog
- Unit Testing:
    - xUnit
    - Moq
- Integrational Testing:
    - Test Containers
    -  WebApplication Factory

## Features

_in progress..._

## Project Architecture
This project is structured using **Clean Architecture** pattern

```
BookRest.Domain        // Entities, value objects, domain events
BookRest.Application   // Use cases, interfaces, DTOs
BookRest.Infrastructure// Data access, external services 
BookRest.Api           // Controllers, middleware, DI setup
```

#### Traffic rule

- Api references Application and Infrastructure.
- Infrastructure references Application and Domain.
- Application references Domain.
- Domain references nobody.

## Database schema

_in progress..._

___

# License

This software is licensed under the [MIT license](LICENSE)
