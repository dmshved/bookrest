<p align="center">
<img width="120" src="https://images2.imgbox.com/a9/16/2Uwa3gC7_o.png" alt="BookRest logo">
</p>

<h1 align="center">Restaurant Reservation System</h1>

## Project description

BookRest is a complex and modern restaurant reservation system. It provides a REST API to book restaurant tables clearly and painless.

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
    <img src="https://milanjovanovic.tech/blogs/mnw_004/clean_architecture.png" height="300" alt="Clean Architecture">
</p>

```
BookRest.Domain        // Entities, value objects, domain events
BookRest.Application   // Use cases, interfaces, DTOs
BookRest.Infrastructure// Data access, external services
BookRest.Api           // Controllers, middleware, DI setup
BookRest.Shared        // Centralise service name constants
```

## Database schema

_in progress..._

___

# License

This software is licensed under the [MIT license](LICENSE)
