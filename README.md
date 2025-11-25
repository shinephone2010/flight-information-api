# ✈️ Flight Information API

The **Flight Information API** is an **ASP.NET Core Web API** designed for managing and querying flight data. Built with an emphasis on maintainability and contract stability, it leverages a **design-first** approach.

This API provides endpoints to manage complete flight lifecycles:

* **Airline** and **Flight Number**
* **Departure/Arrival Airports** and **Times**
* **Flight Status** (e.g., Scheduled, Delayed, Cancelled, InAir, Landed)

---

## 🎨 Design-First Approach & OpenAPI Specification

This API strictly adheres to a **design-first** methodology to prevent implementation drift.

### The Contract

* The API contract is defined in an **OpenAPI 3.1.0 specification** file.
* This specification is generated and maintained using **Stoplight Studio**, ensuring compliance with the official **OpenAPI Standard**.
* **Any changes to the API surface MUST start with an update to the OpenAPI spec.**

### Core Endpoints

The OpenAPI specification defines the following RESTful resources:

| Method | Route             | Description                                       |
|--------|-------------------|---------------------------------------------------|
| GET    | `/flights`        | Retrieve all flights                              |
| GET    | `/flights/{id}`   | Retrieve a single flight by ID                    |
| GET    | `/flights/search` | Search flights by airline/airports/date range     |
| POST   | `/flights`        | Create a new flight                               |
| PUT    | `/flights/{id}`   | Update an existing flight                         |
| DELETE | `/flights/{id}`   | Delete a flight                                   |

> 💡 **Developer Workflow:** Always **update the OpenAPI spec first**, then regenerate the server stub before implementing the business logic.

---

## 🏗️ NSwag-Generated Server Stub

To guarantee that the implementation matches the design, the API server stub is automatically generated using **NSwag**.

### How NSwag is Used

1.  **Reads the OpenAPI 3.1.0 Specification** (the source of truth).
2.  **Generates an abstract controller base class** defining all endpoints.
3.  **Generates all associated data transfer objects (DTOs)** and endpoint method signatures.

### Implementation Pattern

* A **concrete API controller** is defined in the project.
* This controller **inherits** from the NSwag-generated abstract base class.
* It then **overrides** all abstract endpoint methods to inject the actual business and persistence logic.

This technique:

* **Strongly links** the implementation to the OpenAPI contract.
* **Enforces contract compliance** — the compiler will flag missing implementations if the spec changes.
* **Eliminates manual drift** between documentation and the actual API.

---

## 🧭 Architecture Overview

This solution follows a classic, layered architecture to ensure **Separation of Concerns (SoC)** and testability. 

### Project Layers

| Layer | Responsibility | Key Technologies/Components |
| :--- | :--- | :--- |
| **API Layer** (Web Project) | Handling HTTP requests and responses. | ASP.NET Core Web API Host, NSwag-generated controller, Swagger/OpenAPI UI. |
| **Application Layer** | Business logic, coordination, and command/query handling. | Commands & Queries (via **MediatR**), Validation (**FluentValidation**), DTO/Domain mapping. |
| **Infrastructure Layer** | External concerns, persistence, and external services. | Persistence (**Entity Framework Core**), Database Entities, Data Seeding, Time Abstractions (**NodaTime**'s `IClock`). |
| **Tests** | Ensuring quality and correctness. | Unit Tests (Application Logic), Integration Tests (In-memory server for E2E API validation). |

---

## 🚀 Getting Started

Follow these steps to get the Flight Information API running locally.

### Prerequisites

You'll need the following installed:

* **[.NET 8 SDK](https://dotnet.microsoft.com/)** (or the project's specified SDK version).
* A suitable **IDE** (Visual Studio, JetBrains Rider, or VS Code with C# extensions).

### Build and Run

From the **solution root** directory:

```bash
# Restore dependencies and build the entire solution
dotnet restore
dotnet build
```

From the API project folder (e.g. FlightInformationAPI):

```bash
cd FlightInformationAPI
dotnet run --launch-profile https
```

This will make sure the API accept the Https request,
If the browser complains about certificates, run once:

```bash
dotnet dev-certs https --trust
```

By default, the API will:

## Start an ASP.NET Core web server.

Seed initial in-memory flight data (if configured in the Infrastructure layer).

Expose HTTP endpoints at http://localhost:<port> / https://localhost:<port>.

Swagger / OpenAPI UI
When running in Development, the Swagger UI should be available at something like:


## Interacting with the API
You have two primary ways to explore and test the API endpoints:

1. Swagger / OpenAPI UI
This interactive interface is automatically generated from the OpenAPI 3.1.0 specification and lets you explore and test the endpoints directly in your browser.
```bash
https://localhost:<port>/swagger
```

2. REST Client File (FlightInformationAPI.http)
The project includes the FlightInformationAPI.http file, which contains examples for all API endpoints.

If you are using VS Code, install the "REST Client" extension (by Huachao Mao). This allows you to call each endpoint directly from the file by clicking the Send Request button above the request definition.

## Running Tests
From the solution root:

```bash
dotnet test
```
This runs all configured test projects (unit + integration), validating both business logic and API behaviour.


## Regenerating the NSwag Server Stub
This API uses a design-first approach, meaning the OpenAPI specification is defined first and server stubs are generated from it.

- Update the spec in Stoplight Studio (OpenAPI 3.1.0).

- Export or save the updated OpenAPI JSON file to the directory (e.g. OpenAPI/Specs/FlightInformationAPI.json) into the project.

- Run NSwag using the configuration file (e.g. OpenAPI/Config/nswag.json) to regenerate:

	- The abstract controller base class.

	- Any related models or contracts.

Rebuild the solution and update the concrete controller implementation to handle any new or changed endpoints.

```bash
nswag run nswag.json
```
