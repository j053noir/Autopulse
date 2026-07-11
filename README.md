# AutoPulse

AutoPulse is a platform that hosts live auctions for vehicles and real-time telemetry monitoring. Designed for high concurrency, distributed systems and optimizing the performance, developed in .NET Core & React.

---

## Infraestructura Local con Docker Compose

El proyecto está diseñado bajo un enfoque modular, separando los servicios de infraestructura en archivos dedicados.

### Formas de Ejecutar la Infraestructura

#### 1. Iniciar Todo el Entorno (Por Defecto)
Inicia PostgreSQL (Master/Slave), Valkey, MongoDB y Apache Kafka (KRaft):
```bash
docker compose up -d
```

#### 2. Iniciar con Carga de Semillas (Seeds)
Si deseas popular las bases de datos PostgreSQL y MongoDB con datos de prueba realistas automáticamente:
```bash
docker compose --profile seed up -d
```
> **Nota:** Al usar el perfil de seed, se levantará un contenedor temporal con el SDK de .NET que instalará `dotnet-ef` y ejecutará de forma automática las migraciones en ambas bases de datos (Master y Slave) antes de proceder con la inserción de las semillas.

#### 3. Iniciar Servicios Específicos
Puedes levantar únicamente los módulos que requieras para tu sesión de desarrollo:

* **Bases de datos relacionales y NoSQL + Caché (Postgres, Mongo, Valkey)**:
  ```bash
  docker compose -f docker-compose.db.yml -f docker-compose.cache.yml -f docker-compose.mongodb.yml up -d
  ```
* **Solo Mensajería (Kafka)**:
  ```bash
  docker compose -f docker-compose.messaging.yml up -d
  ```

---

## Puertos y Servicios Locales

| Servicio | Contenedor | Puerto Local | Detalle |
| :--- | :--- | :--- | :--- |
| **PostgreSQL (Master)** | `autopulse-postgres-master` | `5432` | Base de datos relacional (Escritura) |
| **PostgreSQL (Slave)** | `autopulse-postgres-slave` | `5433` | Base de datos relacional (Lectura) |
| **Valkey** | `autopulse-valkey` | `6379` | Caché distribuido y Rate Limiting |
| **MongoDB** | `autopulse-mongodb` | `27017` | Almacenamiento NoSQL (Especificaciones) |
| **Apache Kafka** | `autopulse-kafka` | `9092` | Broker de mensajería (KRaft mode) |
