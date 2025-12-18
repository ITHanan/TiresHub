📝 Changelog

All notable changes to this project are documented in this file.

[1.0.1] - 2025-11-26

🐞 Fixed

Resolved CS1061 error in Program.cs by correcting the namespace for the AddSwaggerWithJwt function.

Resolved conflicting dependency issues through deep NuGet cache cleanup.

🛠️ Changed

Standardized all core packages to .NET 8 (as per updated project specifications).

Configured Swagger to support JWT authorization via the dedicated AddSwaggerWithJwt function.

✨ Added

Added Auth Controller with register and login endpoints.

Added TodoItems Controller with basic endpoints (POST, GET, GET/{id}).

Secured TodoItems endpoints using Authorization.

[1.0.0] - 2025-11-24

✨ Added

Initial boilerplate release (Clean Architecture Boilerplate).

Set up the four layers: DomainLayer, ApplicationLayer, InfrastructureLayer, ApiLayer.

Included Generic Repository Pattern with Entity Framework Core implementation.
