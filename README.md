# AutoPulse 🚗⚡

AutoPulse is a high-concurrency, distributed platform for live vehicle auctions and real-time telemetry monitoring. Built on .NET 10 using Clean Architecture, it integrates relational and NoSQL storage, distributed caching, and event-driven messaging.

---

## 🏛️ Architecture & Design Patterns

AutoPulse is structured around the principles of **Clean Architecture**, enforcing strict boundary segregation to keep core business logic independent of external frameworks, databases, and delivery mechanisms.

```
                  ┌──────────────────────────────┐
                  │          AutoPulse.Api       │
                  └──────────────┬───────────────┘
                                 │
                                 ▼
                  ┌──────────────────────────────┐
                  │    AutoPulse.Application     │
                  └──────────────┬───────────────┘
                                 │
                                 ▼
                  ┌──────────────────────────────┐
                  │       AutoPulse.Domain       │
                  └──────────────────────────────┘
                                 ▲
                                 │
                  ┌──────────────┴───────────────┐
                  │   AutoPulse.Infrastructure   │
                  └──────────────────────────────┘
```

### Key Design Patterns Implemented

1. **CQRS (Command Query Responsibility Segregation) with MediatR**
   - **Commands** (write operations) and **Queries** (read operations) are completely decoupled.
   - Handled via `MediatR` to orchestrate handlers without direct coupling between the API layer and the business logic.
   - Core Commands include: `CreateAuction`, `CreateAuctionBid`, and `ProcessPayment`.
   - Core Queries include: `ActiveAuctionsWithVehicle` and `GetAuctionDashboard`.

2. **Event-Driven Sagas (Choreographed Saga Pattern)**
   - Managed using **MassTransit** over **Apache Kafka**.
   - The system coordinates complex multi-service transactional boundaries (e.g., `AuctionBookingSaga`) across payment processing and document generation.
   - Compensation steps are executed automatically if a stage fails (e.g., `ReopenAuctionCompensation` to roll back state if payment fails).

3. **High-Performance Telemetry Processing (Zero-Allocation Parsing)**
   - Telemetry ingestion from car sensors handles huge throughput.
   - The parser utilizes modern C# memory optimization features such as `ReadOnlySpan<char>` and `Span<T>` to process raw strings with near-zero heap allocations.
   - A dedicated **Benchmark Endpoint** provides real-time comparisons between the Naive string-splitting parser and the optimized Span parser.

4. **Cache-Aside Pattern (Valkey)**
   - Distribute caching is implemented via `ICacheService` backed by Valkey (Redis-compatible) to speed up critical reads:
     - `auctions:list_active` (paginated active auctions list)
     - `auctions:detail:{id}` (individual auction details)
     - `auctions:user-bids:{userId}` (user's bid history)
   - **Cache Invalidation:** When a new bid is processed (`CreateAuctionBidCommandHandler`), caches for the auction detail, the bidder's history, and the active list are aggressively invalidated.

5. **Polyglot Persistence**
   - **PostgreSQL** (Master for writes, Slave for read-replication) manages transactional data (Auctions, Bids, Users, Vehicles).
   - **MongoDB** stores unstructured vehicle specifications (`VehicleSpecificationDocument`).

---

## 📂 Directory Structure

```lic
AutoPulse/
├── AutoPulse.Domain/             # Enterprise Core: Entities, Value Objects, Domain Events
│   ├── Entities/                 
│   │   ├── Sql/                  # PostgreSQL relational entities (Auction, Bid, Vehicle, User)
│   │   └── NoSql/                # MongoDB document entities (VehicleSpecificationDocument)
│   └── ValueObjects/             # Immutable domain types
├── AutoPulse.Application/        # Application Business Rules: Commands, Queries, Validators
│   └── Application/
│       ├── Auctions/             # Handlers, Dtos and validation for Auction domain
│       │   ├── Commands/         # CQRS write handlers (Close, Create, Bid, Saga steps)
│       │   └── Queries/          # CQRS read handlers (Dashboard, List, User Bids)
│       └── Common/               # Behaviors, interfaces, mappings
├── AutoPulse.Infrastructure/     # Frameworks, Database Migrations, Adapters, External Integrations
│   ├── Persistence/              
│   │   ├── Sql/                  # Entity Framework Core DbContext (Master/Slave configurations)
│   │   └── NoSql/                # MongoDB Client & collections
│   ├── Messaging/                # MassTransit Saga state machines, Kafka producers/consumers
│   └── Cache/                    # Valkey caching implementations
├── AutoPulse.Api/                # Entry Point: HTTP Controllers, SignalR Hubs, Middleware
│   └── Controllers/              # API Endpoints (Auctions, Auth, Telemetry)
└── AutoPulse.Notifications/      # Independent notification service (Ingestion, Workers)
```

---

## 📦 Technology Stack & Package Versions

### Core Environment
* **Platform:** .NET 10.0 (`net10.0`)
* **Databases:** PostgreSQL (Relational), MongoDB (Document NoSQL)
* **Message Broker:** Apache Kafka (KRaft mode)
* **Caching:** Valkey (Redis-compatible)

### Package Dependencies

| Package | Version | Description |
| :--- | :--- | :--- |
| `MediatR` | `14.1.0` | CQRS request/response dispatching |
| `MassTransit` | `8.4.1` | Message bus abstraction over Kafka |
| `Microsoft.EntityFrameworkCore` | `10.0.9` | ORM for PostgreSQL relational access |
| `MongoDB.Driver` | `3.9.0` | Client library for MongoDB specs storage |
| `Polly` & `Polly.Extensions` | `8.7.0` | Resilience policies (retry, circuit breaker) |
| `FluentValidation` | `11.11.0` | Strongly-typed domain command validation |
| `BCrypt.Net-Next` | `4.2.0` | Password hashing utility |
| `HtmlSanitizer` | `9.0.892` | Input sanitization for user-generated strings |
| `Microsoft.Extensions.Caching.StackExchangeRedis` | `10.0.9` | Valkey caching provider adapter |

---

## 🔌 API Endpoints Reference

All endpoints are hosted under `http://localhost:5000/api`.

### 🔐 Authentication (`/api/auth`)

* **`POST /api/auth/register`**
  - Registers a new user.
  - *Payload:* `RegisterUserCommand`
* **`POST /api/auth/login`**
  - Authenticates credentials, returns JWT tokens and sets secure HTTP-Only cookies (`autopulse-session`, `autopulse-refresh-token`).
  - *Payload:* `LoginUserCommand`
* **`POST /api/auth/refresh-token`**
  - Refreshes expired session tokens via request cookies or payload fallback.
  - *Payload:* `RefreshTokenCommand` (Optional if cookie is present)
* **`POST /api/auth/logout`**
  - Revokes active sessions and clears client cookies.
  - *Payload:* `LogoutUserCommand`
* **`GET /api/auth/profile`** [Authorized]
  - Retrieves profile information and authorization permissions for the logged-in user.

### 🔨 Auctions (`/api/auctions`)

* **`GET /api/auctions/active`** [Authorized: `Auctions.Read`]
  - Retrieves a filtered, paginated list of active auctions with vehicle details.
* **`GET /api/auctions/{id}`** [Authorized: `Auctions.Read`]
  - Retrieves detailed data of a specific auction.
* **`GET /api/auctions/{id}/dashboard`** [Authorized: `Auctions.Read`]
  - Retrieves live dashboard statistics including full bidding history.
* **`GET /api/auctions/bids/my`** [Authorized: `Auctions.ReadBids`]
  - Retrieves all historic bids placed by the currently logged-in user.
* **`POST /api/auctions`** [Authorized: `Auctions.Create`]
  - Creates a new auction.
  - *Payload:* `CreateAuctionCommand`
* **`POST /api/auctions/{id}/bids`** [Authorized: `Auctions.Bid`]
  - Places a new bid on a running auction. Performs bounds validations.
  - *Payload:* `CreateAuctionBidCommand`
* **`POST /api/auctions/upload-url`** [Authorized: `Auctions.Create`]
  - Generates a secure pre-signed URL to upload vehicle titles/documents to cloud storage.

### 📈 Telemetry & Benchmarking (`/api/telemetry`)

* **`POST /api/telemetry?method={span|naive}`** [Authorized: `Telemetry.Process`]
  - Processes raw telemetric string inputs from vehicle sensors.
* **`POST /api/telemetry/benchmark`** [Authorized: `Telemetry.Benchmark`]
  - Executes a comparative load benchmark (500,000 iterations) testing Naive String Splitting vs. Zero-Allocation `Span<char>` parsing.
  - Returns execution time (ms) and Garbage Collector (Gen 0, 1, 2) collection count details.

---

## 🛠️ Local Infrastructure Setup (Docker Compose)

The infrastructure services are split into modular compose configurations.

### Ingestion & Run Commands

#### 1. Boot up the Entire Environment
Runs the backend API, Postgres (Master/Slave), Valkey, MongoDB, and Apache Kafka:
```bash
docker compose up -d
```

#### 2. Run with Automated Database Seeding (Recommended)
Bootstraps Postgres and MongoDB with EF Core migrations and realistic testing records automatically:
```bash
docker compose --profile seed up -d
```

#### 3. Run Specific Profiles
* **Only Data Stores (Postgres, Mongo, Valkey):**
  ```bash
  docker compose -f docker-compose.db.yml -f docker-compose.cache.yml -f docker-compose.mongodb.yml up -d
  ```
* **Only Messaging (Kafka):**
  ```bash
  docker compose -f docker-compose.messaging.yml up -d
  ```

### Local Service Map

| Service | Container Name | Local Port | Role |
| :--- | :--- | :--- | :--- |
| **API Backend** | `autopulse-api` | `5000` | Main .NET 10 API entrypoint |
| **PostgreSQL (Master)** | `autopulse-postgres-master` | `5432` | Write relational database |
| **PostgreSQL (Slave)** | `autopulse-postgres-slave` | `5433` | Read-only replica database |
| **Valkey** | `autopulse-valkey` | `6379` | High performance caching & rate limiting |
| **MongoDB** | `autopulse-mongodb` | `27017` | Vehicle specifications database |
| **Apache Kafka** | `autopulse-kafka` | `9092` | Event broker running in KRaft mode |

---

## ✉️ Apache Kafka Topic Management

To create required notification and transactional message topics inside the Kafka container:

```bash
# Create specific topics
docker exec -it autopulse-kafka kafka-topics --create --bootstrap-server localhost:9092 --partitions 1 --replication-factor 1 --topic notification.telemetry.events
docker exec -it autopulse-kafka kafka-topics --create --bootstrap-server localhost:9092 --partitions 1 --replication-factor 1 --topic notification.transactional.email
docker exec -it autopulse-kafka kafka-topics --create --bootstrap-server localhost:9092 --partitions 1 --replication-factor 1 --topic notification.transactional.sms
docker exec -it autopulse-kafka kafka-topics --create --bootstrap-server localhost:9092 --partitions 1 --replication-factor 1 --topic notification.transactional.push
docker exec -it autopulse-kafka kafka-topics --create --bootstrap-server localhost:9092 --partitions 1 --replication-factor 1 --topic notification.marketing.bulk
```

### Verification
```bash
docker exec -it autopulse-kafka kafka-topics --list --bootstrap-server localhost:9092
```
