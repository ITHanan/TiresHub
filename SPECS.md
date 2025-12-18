📋 System Specifications and Endpoints

This document outlines the current functionalities, constraints, and API endpoints available within the Clean Architecture API system.

1. Core Features

1.1. Authentication and Authorization (Auth)

Description: Provides a mechanism for user registration and secure login using a Hashed Password and Email/Username.

Output: A valid JWT (JSON Web Token) is issued upon successful registration or login.

Constraints: All requests related to TodoItems must be authenticated using this JWT token.

1.2. TodoItems Management

Description: Allows authenticated users to manage their personal list of TodoItems.

Entity: The core entity is TodoItem (Title, Description, completion status IsDone, and the creating user's ID CreatedByUserId).

2. API Endpoints

All endpoints start with the base path /api/.

Auth Group

Method

Path

Description

Request Body Example

POST

/api/Auth/register

Registers a new user and issues a JWT token.

{"userName": "...", "userEmail": "...", "password": "..."}

POST

/api/Auth/login

Logs in an existing user and issues a JWT token.

{"userName": "...", "password": "..."}

TodoItems Group

Method

Path

Description

Request Body Example

Authorization

POST

/api/TodoItems

Creates a new TodoItem.

{"title": "...", "description": "...", "createdByUserId": 1}

Required (JWT)

GET

/api/TodoItems

Retrieves a list of all TodoItems belonging to the authenticated user.

None

Required (JWT)

GET

/api/TodoItems/{id}

Retrieves a specific TodoItem by its ID.

None

Required (JWT)

3. Use Cases & Patterns

Repository Pattern: Data access is isolated via the IGenericRepository<T> interface in the Application layer, allowing for easy database technology substitution.

CQRS: Each modification operation (e.g., POST /TodoItems) is handled as a Command, while retrieval operations (e.g., GET /TodoItems) are handled as a Query, each with its own specific handler in the Application layer.

Assumptions: The userId obtained from the JWT token is used to link TodoItems to their respective users.
