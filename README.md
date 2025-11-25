# Flight Information API

The **Flight Information API** is an ASP.NET Core Web API for managing and querying flight information.  
It exposes endpoints to search, retrieve, create, update, and delete flights, including details such as:

- Airline  
- Flight number  
- Departure and arrival airports  
- Departure and arrival times  
- Flight status (e.g. Scheduled, Delayed, Cancelled, InAir, Landed)

---

## Design-First API & OpenAPI Specification

This project follows a **design-first** approach:

- The API contract is defined in an **OpenAPI 3.0.1** specification file.
- The spec is **generated and maintained using Stoplight Studio**, following the official **OpenAPI standard**.
- The OpenAPI document is the **single source of truth** for the API surface.

Key points:

- The OpenAPI spec describes core endpoints such as:
  - `GET /flights` – Retrieve all flights  
  - `GET /flights/{id}` – Retrieve a single flight by ID  
  - `GET /flights/search` – Search flights by airline, airports, and/or date range  
  - `POST /flights` – Create a new flight  
  - `PUT /flights/{id}` – Update an existing flight  
  - `DELETE /flights/{id}` – Delete a flight  

You should update the OpenAPI spec first, then regenerate the server stub and implement the logic. This keeps the implementation in sync with the contract at all times.

---

## NSwag-Generated Server Stub

The API server stub is generated using **NSwag**:

- NSwag reads the **OpenAPI 3.0.1** specification and generates:
  - An **abstract controller base class** containing all endpoint definitions.
  - All associated endpoint method signatures (actions) defined by the spec.

The project then:

- Defines a **concrete API controller** that:
  - **Inherits** from the NSwag-generated base controller class.
  - **Overrides** all endpoints from the base class.
  - Implements the actual business and persistence logic.

This pattern ensures:

- The controller implementation is strongly tied to the OpenAPI contract.
- Changes to the spec can be propagated by regenerating the base controller.
- You avoid manual drift between documentation and implementation.

---

## Architecture Overview

A typical architecture for this solution looks like:

- **API layer (Web project)**
  - ASP.NET Core Web API host.
  - NSwag-generated abstract controller base class.
  - Concrete controller inheriting from the base and overriding all endpoints.
  - Swagger/OpenAPI UI for exploring the API surface.

- **Application layer**
  - Application logic, commands, and queries (e.g. via MediatR).
  - Validation (e.g. FluentValidation).
  - DTOs and mapping between domain entities and API models.

- **Infrastructure layer**
  - Persistence (e.g. Entity Framework Core).
  - Database entities and configurations.
  - Data seeding (e.g. CSV-based seeding for initial flight data).
  - Integration with time abstractions (e.g. NodaTime’s `IClock`).

- **Tests**
  - Unit tests for application logic.
  - Integration tests using an in-memory test server to validate API behaviour end-to-end.

> Adjust project names and paths in this section to match your actual solution structure.

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/) (or the SDK version used by this project)
- An IDE such as:
  - Visual Studio  
  - JetBrains Rider  
  - VS Code with C# extensions

### Restore & Build

From the solution root:

```bash
dotnet restore
dotnet build
Run the API
From the API project folder (e.g. FlightInformationAPI):

bash

cd FlightInformationAPI
dotnet run
By default, the API will:

Start an ASP.NET Core web server.

Seed initial in-memory flight data (if configured in the Infrastructure layer).

Expose HTTP endpoints at http://localhost:<port> / https://localhost:<port>.

Swagger / OpenAPI UI
When running in Development, the Swagger UI should be available at something like:

text

https://localhost:<port>/swagger
This UI is generated from the OpenAPI 3.0.1 specification and lets you interactively explore and test the endpoints.

Running Tests
From the solution root:

bash

dotnet test
This runs all configured test projects (unit + integration), validating both business logic and API behaviour.

Regenerating the NSwag Server Stub
Whenever you change the OpenAPI spec (design-first):

Update the spec in Stoplight Studio (OpenAPI 3.0.1).

Export or save the updated OpenAPI JSON/YAML file into the project.

Run NSwag using your configuration (e.g. nswag.json) to regenerate:

The abstract controller base class.

Any related models or contracts.

Rebuild the solution and update your concrete controller implementation to handle any new or changed endpoints.

Example command (adjust paths/names as needed):

bash

nswag run nswag.json
