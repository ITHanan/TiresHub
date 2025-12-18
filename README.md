# CleanArchitectBoilerplate

🧩 Clean Architecture .NET Backend Boilerplate

This project serves as a comprehensive and reusable backend boilerplate built on .NET 8, adhering to the Clean Architecture principles.

The repository aims to provide a solid, organized foundation for any new Web API project, focusing on code that is testable, maintainable, and scalable.

📚 Table of Contents

Key Goals and Features

Architecture Structure

Getting Started (Local Execution)

Contributing

🎯 Key Goals and Features

Clean Architecture: Clear separation of layers ensures the independence of domain logic from technical details.

CQRS (Command Query Responsibility Segregation) with MediatR: Separates logic between commands (state-changing operations) and queries (data retrieval).

Generic Repository Pattern: Standardizes and simplifies data access via Entity Framework Core.

JWT Authentication: Full support for authentication using JWT tokens, including Swagger configuration for token handling.

📁 Architecture Structure

The project is divided into four primary layers (projects), with a dependency flow directed inward (towards the Domain).

Layer (Project)

Description and Responsibilities

DomainLayer

Contains the application's core: Entities, Value Objects, and core business logic. Has no external dependencies.

ApplicationLayer

Contains Use Cases, DTOs, Repository Interfaces (IGenericRepository<T>), and CQRS handlers (Commands & Queries).

InfrastructureLayer

Contains the actual implementation of the Repository interfaces using Entity Framework Core, data access details, and external services (like the JWT generator).

ApiLayer

The Entry Point of the application. Contains Controllers that receive HTTP requests and use IMediator to dispatch Commands and Queries.

🚀 Getting Started (Local Execution)

To run this project locally, ensure you meet the requirements and follow the steps below:

Prerequisites

.NET 8 SDK (or newer).

SQL Server LocalDB or equivalent database management tool.

Command Sequence

Clone the Project:

git clone  https://github.com/ITHanan/CleanArchitectBoilerplate.git # Replace with your repo URL
cd CleanArchitectBoilerplate


Restore NuGet Packages:

dotnet restore


Update Database (Apply Migrations):
Note: Specify both the project containing the DbContext (InfrastructureLayer) and the startup project (ApiLayer).

dotnet ef database update --project InfrastructureLayer --startup-project ApiLayer


Run the API:

dotnet run --project ApiLayer


🤝 Contributing

We welcome contributions! Please refer to the CONTRIBUTING.md file for guidelines on branch naming, pull requests, and coding standards.
