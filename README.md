<div align="center">
    <img src="https://images2.imgbox.com/a9/16/2Uwa3gC7_o.png" height="120" style="vertical-align:middle;">
    <span style="font-size:40px; vertical-align:middle;">
        <strong>bookrest</strong>
    </span>
</div>

## Project description

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
- Validation: 
    - FluentValidation
    - Ardalis.GuardClauses
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

<p align="center">
<img src="https://thumbs2.imgbox.com/ba/62/cSISRs2H_t.png" alt="Clean Architecture">
</p>

```
BookRest.Domain        // Entities, value objects, domain events
BookRest.Application   // Use cases, interfaces, DTOs
BookRest.Infrastructure// Data access, external services 
BookRest.Api           // Controllers, middleware, DI setup
BookRest.Shared        // Centralise service name constants
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
