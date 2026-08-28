# ADR-001: Manage date and time

## Status

Accepted

## Date

04-08-2026

## Context

The application needs to handle date and time-related operations:

- storing the restaurant's opening time
- storing timestamps
- checking restaurant is open or close based on opening and closing times

The task is complicated by the need to account for various nuances when working with time:

- time zones
- Daylight Saving Time (DST)
- leap seconds and years
- culture-dependent calendars
- historical date changes

While `System.DateTime` is widely used, it has certain limitations and fundamental design flaws that make it highly prone to developer errors, especially when dealing with more complex scenarios involving time zones, daylight saving time changes, and precise duration calculations.

## Decision

BookRest uses [Noda Time](https://nodatime.org/) for date and time handling.

Noda Time provides explicit types for different temporal concepts and makes time zone conversions and duration calculations more predictable and semantically clear.

## Rationale

Noda Time makes the meaning of a time value explicit it's free, open source and was built by John Skeet (should I go on? Or is this reason enough? 😄). Developers cannot accidentally treat a local time as UTC or vice versa without making that decision visible in the code.

It provides well-defined handling of time zones and DST and follows established standards for date and time calculations, which reduces the risk of time-related bugs and makes the original intent of the code easier to understand.

Noda Time types, such as `Instant`, will be used in the Domain layer, since Noda Time contains only code logic and data structures. It does not talk to databases or web servers, so it doesn't break the dependency rule of Clean Architecture and Domain-Driven Design (DDD).

## Consequences

**Easier:**
- Easier logic maintenance, since Noda Time strictly follows major world standards for date and time handling.
- Time-related API and semantics are explicit in the code.
- Time zone and DST handling is less error-prone.
- Different temporal concepts are represented by appropriate types.

**Harder:**

- Additional dependencies and conversions may be required.
- Need for helper libraries to convert to and from Noda Time data structures: [Npgsql.NodaTime](https://www.npgsql.org/doc/types/nodatime.html?tabs=datasource), etc. Helper libraries leads to more dependencies and more points of failure in your app.
- Unfamiliarity with Noda Time requires to learn its API.

## When this decision does not apply

In case the project is small and heavily rely on standard .NET ecosystem libraries that lack native NodaTime integrations, or already find the native DateOnly and TimeOnly types sufficient - avoid using Noda Time.
