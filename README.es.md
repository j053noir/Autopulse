# AutoPulse 🚗⚡

AutoPulse es una plataforma distribuida y de alta concurrencia diseñada para subastas de vehículos en vivo y monitoreo de telemetría en tiempo real. Construida sobre .NET 10 bajo los principios de Arquitectura Limpia (Clean Architecture), la aplicación integra almacenamiento relacional y NoSQL, caché distribuida y mensajería orientada a eventos.

---

## 🏛️ Arquitectura y Patrones de Diseño

AutoPulse está estructurado en base a **Arquitectura Limpia (Clean Architecture)**, lo que garantiza una segregación estricta de responsabilidades para mantener la lógica de negocio central independiente de frameworks externos, bases de datos y mecanismos de entrega.

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

### Patrones de Diseño Clave Implementados

1. **CQRS (Command Query Responsibility Segregation) con MediatR**
   - Las operaciones de escritura (**Commands**) y de lectura (**Queries**) están completamente desacopladas.
   - Utiliza `MediatR` para orquestar y despachar las solicitudes sin acoplar directamente la capa de API con la lógica de negocio.
   - Commands principales: `CreateAuction`, `CreateAuctionBid` y `ProcessPayment`.
   - Queries principales: `ActiveAuctionsWithVehicle` y `GetAuctionDashboard`.

2. **Sagas Orientadas a Eventos (Choreographed Saga Pattern)**
   - Gestionado mediante **MassTransit** a través de **Apache Kafka**.
   - El sistema coordina transacciones complejas distribuidas (como `AuctionBookingSaga`) que involucran el procesamiento de pagos y la generación de contratos de manera confiable.
   - Se ejecutan pasos de compensación automáticos si falla alguna de las fases de la transacción (por ejemplo, `ReopenAuctionCompensation` para revertir el estado de la subasta si el pago es rechazado).

3. **Procesamiento de Telemetría de Alto Rendimiento (Zero-Allocation Parsing)**
   - El procesamiento de los datos de telemetría provenientes de los sensores del vehículo maneja un alto volumen de transacciones.
   - El analizador (parser) utiliza características avanzadas de optimización de memoria de C# como `ReadOnlySpan<char>` y `Span<T>` para procesar cadenas crudas con casi cero asignación de memoria en el heap.
   - Un **Endpoint de Benchmark** permite realizar pruebas comparativas en tiempo real entre el parser tradicional (`string.Split`) y la versión optimizada con `Span`.

4. **Patrón Cache-Aside (Valkey)**
   - Capa de almacenamiento en caché distribuido implementada a través de `ICacheService` con Valkey (compatible con Redis) para optimizar consultas de lectura críticas:
     - `auctions:list_active` (listado paginado de subastas activas)
     - `auctions:detail:{id}` (detalle técnico de una subasta específica)
     - `auctions:user-bids:{userId}` (historial de pujas del usuario)
   - **Invalidación de Caché:** Cuando se procesa una nueva puja (`CreateAuctionBidCommandHandler`), las cachés del detalle de la subasta, del historial del usuario y del listado activo se invalidan de forma inmediata.

5. **Persistencia Políglota**
   - **PostgreSQL** (configuración Master para escrituras, Slave para replicación de lecturas) gestiona los datos transaccionales (Subastas, Pujas, Usuarios, Vehículos).
   - **MongoDB** almacena la ficha técnica y especificaciones no estructuradas del vehículo (`VehicleSpecificationDocument`).

---

## 📂 Estructura de Directorios

```lic
AutoPulse/
├── AutoPulse.Domain/             # Núcleo del Negocio: Entidades, Objetos de Valor, Eventos de Dominio
│   ├── Entities/                 
│   │   ├── Sql/                  # Entidades relacionales PostgreSQL (Auction, Bid, Vehicle, User)
│   │   └── NoSql/                # Documentos NoSQL MongoDB (VehicleSpecificationDocument)
│   └── ValueObjects/             # Tipos de dominio inmutables
├── AutoPulse.Application/        # Reglas de Negocio de la Aplicación: Commands, Queries, Validadores
│   └── Application/
│       ├── Auctions/             # Manejadores, DTOs y validaciones del dominio de Subastas
│       │   ├── Commands/         # Controladores de escritura CQRS (Close, Create, Bid, pasos de Saga)
│       │   └── Queries/          # Controladores de lectura CQRS (Dashboard, List, User Bids)
│       └── Common/               # Comportamientos, interfaces y mapeos comunes
├── AutoPulse.Infrastructure/     # Frameworks, Migraciones de BD, Adaptadores e Integraciones Externas
│   ├── Persistence/              
│   │   ├── Sql/                  # DbContext de EF Core (Configuraciones de Master/Slave)
│   │   └── NoSql/                # Cliente y colecciones de MongoDB
│   ├── Messaging/                # Máquinas de estado de Sagas en MassTransit, consumidores/productores de Kafka
│   └── Cache/                    # Implementaciones de caché distribuida con Valkey
├── AutoPulse.Api/                # Punto de Entrada: Controladores HTTP, Hubs de SignalR, Middlewares
│   └── Controllers/              # Endpoints de la API (Auctions, Auth, Telemetry)
└── AutoPulse.Notifications/      # Servicio independiente de notificaciones (Ingestión, Workers)
```

---

## 📦 Stack Tecnológico y Versiones de Paquetes

### Entorno Base
* **Plataforma:** .NET 10.0 (`net10.0`)
* **Bases de datos:** PostgreSQL (Relacional), MongoDB (NoSQL Documental)
* **Message Broker:** Apache Kafka (modo KRaft)
* **Caché:** Valkey (compatible con Redis)

### Dependencias de Paquetes

| Paquete | Versión | Descripción |
| :--- | :--- | :--- |
| `MediatR` | `14.1.0` | Despacho de peticiones/respuestas para CQRS |
| `MassTransit` | `8.4.1` | Abstracción de bus de servicios de mensajería sobre Kafka |
| `Microsoft.EntityFrameworkCore` | `10.0.9` | ORM para el acceso relacional a PostgreSQL |
| `MongoDB.Driver` | `3.9.0` | Biblioteca cliente para almacenamiento de fichas técnicas en MongoDB |
| `Polly` & `Polly.Extensions` | `8.7.0` | Políticas de resiliencia (reintentos, disyuntor/circuit-breaker) |
| `FluentValidation` | `11.11.0` | Validación fuertemente tipada de comandos del dominio |
| `BCrypt.Net-Next` | `4.2.0` | Utilidad de hashing y cifrado de contraseñas |
| `HtmlSanitizer` | `9.0.892` | Sanitización de entradas de texto generadas por usuarios |
| `Microsoft.Extensions.Caching.StackExchangeRedis` | `10.0.9` | Adaptador proveedor de caché para Valkey |

---

## 🔌 Referencia de Endpoints de la API

Todos los endpoints están alojados bajo `http://localhost:5000/api`.

### 🔐 Autenticación (`/api/auth`)

* **`POST /api/auth/register`**
  - Registra un nuevo usuario en la plataforma.
  - *Payload:* `RegisterUserCommand`
* **`POST /api/auth/login`**
  - Autentica credenciales, retorna los tokens JWT y establece las cookies seguras HTTP-Only (`autopulse-session`, `autopulse-refresh-token`).
  - *Payload:* `LoginUserCommand`
* **`POST /api/auth/refresh-token`**
  - Rota los tokens expirados utilizando las cookies de la petición o un valor alternativo en el payload.
  - *Payload:* `RefreshTokenCommand` (Opcional si las cookies están presentes)
* **`POST /api/auth/logout`**
  - Revoca la sesión activa del usuario y limpia las cookies del cliente.
  - *Payload:* `LogoutUserCommand`
* **`GET /api/auth/profile`** [Autorizado]
  - Recupera los datos del perfil y permisos de autorización del usuario autenticado.

### 🔨 Subastas (`/api/auctions`)

* **`GET /api/auctions/active`** [Autorizado: `Auctions.Read`]
  - Obtiene una lista filtrada y paganida de las subastas activas junto con los detalles del vehículo.
* **`GET /api/auctions/{id}`** [Autorizado: `Auctions.Read`]
  - Obtiene los datos detallados de una subasta específica.
* **`GET /api/auctions/{id}/dashboard`** [Autorizado: `Auctions.Read`]
  - Recupera estadísticas en tiempo real del panel de la subasta, incluyendo el historial completo de pujas.
* **`GET /api/auctions/bids/my`** [Autorizado: `Auctions.ReadBids`]
  - Recupera todas las pujas históricas realizadas por el usuario actualmente autenticado.
* **`POST /api/auctions`** [Autorizado: `Auctions.Create`]
  - Crea una nueva subasta en el sistema.
  - *Payload:* `CreateAuctionCommand`
* **`POST /api/auctions/{id}/bids`** [Autorizado: `Auctions.Bid`]
  - Registra una nueva puja en una subasta activa. Realiza validaciones de límites mínimos y de incremento.
  - *Payload:* `CreateAuctionBidCommand`
* **`POST /api/auctions/upload-url`** [Autorizado: `Auctions.Create`]
  - Genera una URL prefirmada segura para cargar documentos legales o títulos de vehículos directamente al almacenamiento en la nube.

### 📈 Telemetría y Benchmarking (`/api/telemetry`)

* **`POST /api/telemetry?method={span|naive}`** [Autorizado: `Telemetry.Process`]
  - Procesa cadenas de datos de sensores crudas provenientes de los dispositivos IoT del vehículo.
* **`POST /api/telemetry/benchmark`** [Autorizado: `Telemetry.Benchmark`]
  - Ejecuta una prueba comparativa de carga masiva (500,000 iteraciones) que mide el rendimiento del análisis tradicional (`string.Split`) frente a la optimización de asignación cero mediante `Span<char>`.
  - Retorna tiempos de ejecución (ms) y cantidad de recolecciones realizadas por el Garbage Collector (Gen 0, 1, 2).

---

## 🛠️ Configuración de Infraestructura Local (Docker Compose)

Los servicios de infraestructura se definen en configuraciones de Compose modulares.

### Comandos de Ejecución

#### 1. Iniciar el Entorno Completo
Levanta la API de backend, Postgres (Master/Slave), Valkey, MongoDB y Apache Kafka:
```bash
docker compose up -d
```

#### 2. Levantar con Inicialización Automática de Datos (Recomendado)
Aplica las migraciones de EF Core e inserta registros de prueba realistas en Postgres y MongoDB automáticamente:
```bash
docker compose --profile seed up -d
```

#### 3. Iniciar Perfiles Específicos
* **Solo Motores de Datos (Postgres, Mongo, Valkey):**
  ```bash
  docker compose -f docker-compose.db.yml -f docker-compose.cache.yml -f docker-compose.mongodb.yml up -d
  ```
* **Solo Mensajería (Kafka):**
  ```bash
  docker compose -f docker-compose.messaging.yml up -d
  ```

### Mapeo de Servicios Locales

| Servicio | Nombre del Contenedor | Puerto Local | Rol |
| :--- | :--- | :--- | :--- |
| **API Backend** | `autopulse-api` | `5000` | Punto de entrada principal de la API (.NET 10) |
| **PostgreSQL (Master)** | `autopulse-postgres-master` | `5432` | Base de datos relacional para escrituras |
| **PostgreSQL (Slave)** | `autopulse-postgres-slave` | `5433` | Réplica de base de datos para lecturas |
| **Valkey** | `autopulse-valkey` | `6379` | Almacenamiento de caché y limitación de tasa (rate limiting) |
| **MongoDB** | `autopulse-mongodb` | `27017` | Base de datos documental para fichas técnicas |
| **Apache Kafka** | `autopulse-kafka` | `9092` | Broker de eventos en modo KRaft |

---

## ✉️ Gestión de Tópicos en Apache Kafka

Para crear los tópicos de mensajería requeridos en el contenedor de Kafka local:

```bash
# Crear tópicos individuales
docker exec -it autopulse-kafka kafka-topics --create --bootstrap-server localhost:9092 --partitions 1 --replication-factor 1 --topic notification.telemetry.events
docker exec -it autopulse-kafka kafka-topics --create --bootstrap-server localhost:9092 --partitions 1 --replication-factor 1 --topic notification.transactional.email
docker exec -it autopulse-kafka kafka-topics --create --bootstrap-server localhost:9092 --partitions 1 --replication-factor 1 --topic notification.transactional.sms
docker exec -it autopulse-kafka kafka-topics --create --bootstrap-server localhost:9092 --partitions 1 --replication-factor 1 --topic notification.transactional.push
docker exec -it autopulse-kafka kafka-topics --create --bootstrap-server localhost:9092 --partitions 1 --replication-factor 1 --topic notification.marketing.bulk
```

### Verificación
```bash
docker exec -it autopulse-kafka kafka-topics --list --bootstrap-server localhost:9092
```
