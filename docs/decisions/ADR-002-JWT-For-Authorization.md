# ADR-002: Use JWT Access Tokens, Refresh Tokens and Refresh Token Rotation

## Status

Accepted

## Date

31.08.2026

## Context

The application must be secured, the modern and reliable practice is to use Jason Web Tokens (JWT) compared with Access Tokens, Refresh Tokens and Refresh Token rotation.

- `Access tokens` are used to allow an access to an API. The client receives an access token after a successfull authentication and authorization, then passes the access token as a credential when he calls the target API.

- `Refresh Tokens` are used to get a new access token without client interaction. This allows to shorten the access token lifetime for security purposes without involving the client when the access token expires.

- `Refresh token rotation` is a strategy of replacing refresh tokens to minimize vulnerability. With refresh token rotation, every time client exchanges a refresh token to get a new access token, BookRest also returns a new refresh token.

To store refresh tokens I'll use custom `RefreshTokens` table.

## Decision

BookRest uses the JWT Access Tokens + Refresh Tokens, while also performing Refresh token rotation, refresh tokens are stored in a custom `RefreshTokens` table.

## Rationale

While considering authentication for BookRest there was also a choice of using ASP.NET Core Identity's built-in session based authentication mechanisms, however session-based authentication is bad for REST APIs because it breaks the statelessness rule of REST. Each request must carry all the info needed to process it. Sessions store user data on the server, which breaks this rule. Using session-based authentication would also result in:

- Scaling Issues
- Uncommon practise
- Poor Mobile Support
- CORS and cross-domain complications

Using JWT Tokens (Access Tokens + Refresh Tokens) alongside Refresh Token Rotation and storing them in a dedicated custom table provides the balance between high application performance, robust security when developing a REST APIs and scaling opportunities. Server reads a cryptographically signed JWT Token on each request. It does not need to look up a user session in a database, which makes the API fast and easy to scale. Using JWT Token-based authentication would also result in

- Better Security
- Revocation Control
- High Performance
- Scaling opportunities

## Consequences

**Easier:**
- Full Control: Customize fields int the `RefreshTokens` table and behaviour as needed.
- Decoupled from Identity: Works with any authentication system.
- Scalability: Optimized for high-performance token management.
- Revoking Access: Instantly stop a client from processing actions by deleting his refresh token from the database.
- Keeping client logged in: Access tokens are short-lived for safety, while the refresh token is long-lived, which keeps the client logged in for days or weeks without typing their password again.

**Harder:**
- Additional Maintenance: Requires managing a separate `RefreshTokens` table.
- Manual Cleanup: Expired tokens must be purged periodically.
- Initial learning curve: Understanding how JWT works and how to use it in application with awareness requires additional learning process and research.
- Complicated testing: Test Authentication process requires additional knowledge in testing.
- Database Load: Refresh Token Rotation requires to check and update the database every single time a refresh token is used.