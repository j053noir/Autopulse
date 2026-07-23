# AutoPulse

AutoPulse is a platform that hosts live auctions for vehicles and real-time telemetry monitoring. Designed for high concurrency, distributed systems and optimizing the performance, developed in .NET Core & React.

---

## Infraestructura Local con Docker Compose

El proyecto está diseñado bajo un enfoque modular, separando los servicios de infraestructura en archivos dedicados.

### Formas de Ejecutar la Infraestructura

#### 1. Iniciar Todo el Entorno (Por Defecto)
Inicia la API del Backend (.NET Core), PostgreSQL (Master/Slave), Valkey, MongoDB y Apache Kafka (KRaft):
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
| **API Backend** | `autopulse-api` | `5000` | API Principal del Backend (.NET Core) |
| **PostgreSQL (Master)** | `autopulse-postgres-master` | `5432` | Base de datos relacional (Escritura) |
| **PostgreSQL (Slave)** | `autopulse-postgres-slave` | `5433` | Base de datos relacional (Lectura) |
| **Valkey** | `autopulse-valkey` | `6379` | Caché distribuido y Rate Limiting |
| **MongoDB** | `autopulse-mongodb` | `27017` | Almacenamiento NoSQL (Especificaciones) |
| **Apache Kafka** | `autopulse-kafka` | `9092` | Broker de mensajería (KRaft mode) |

---

## Creación de Tópicos en Apache Kafka (Docker)

Para crear manualmente los tópicos de notificación en el contenedor de Kafka, ejecuta el siguiente comando en la terminal utilizando `docker exec` apuntando al contenedor `autopulse-kafka`:

```bash
# Crear un tópico específico
docker exec -it autopulse-kafka kafka-topics --create --bootstrap-server localhost:9092 --partitions 1 --replication-factor 1 --topic <nombre-del-topico>
```

### Tópicos del Sistema de Notificaciones
Puedes crear los siguientes tópicos requeridos para el servicio:

```bash
docker exec -it autopulse-kafka kafka-topics --create --bootstrap-server localhost:9092 --partitions 1 --replication-factor 1 --topic notification.telemetry.events
docker exec -it autopulse-kafka kafka-topics --create --bootstrap-server localhost:9092 --partitions 1 --replication-factor 1 --topic notification.transactional.email
docker exec -it autopulse-kafka kafka-topics --create --bootstrap-server localhost:9092 --partitions 1 --replication-factor 1 --topic notification.transactional.sms
docker exec -it autopulse-kafka kafka-topics --create --bootstrap-server localhost:9092 --partitions 1 --replication-factor 1 --topic notification.transactional.push
docker exec -it autopulse-kafka kafka-topics --create --bootstrap-server localhost:9092 --partitions 1 --replication-factor 1 --topic notification.marketing.bulk
```

# Listar Tópicos Existentes
Para verificar que los tópicos se hayan creado correctamente:
```bash
docker exec -it autopulse-kafka kafka-topics --list --bootstrap-server localhost:9092
```

---

## Estrategia de Caché e Invalación (Valkey)

Para soportar altos niveles de concurrencia y acelerar la lectura, AutoPulse implementa una capa de almacenamiento en caché distribuido usando **Valkey** (compatible con Redis) a través de `ICacheService`:

* **Subastas Activas**: Listado filtrado y paginado (`auctions:list_active`). Expiración por defecto de 5 minutos.
* **Detalle de Subasta**: Ficha técnica de subasta individual (`auctions:detail:{id}`).
* **Ofertas de Usuario**: Registro histórico y estado de pujas del usuario (`auctions:user-bids:{userId}`).

### Política de Invalidación de Caché
Para evitar inconsistencias visuales en el front-end, al momento de consolidar una puja a través de `CreateAuctionBidCommandHandler`:
1. Se remueve la caché del detalle de la subasta (`auctions:detail:{id}`).
2. Se remueve la caché histórica de pujas del usuario postor (`auctions:user-bids:{userId}`).
3. Se remueve la caché del listado general de subastas activas (`auctions:list_active`), forzando a los clientes a obtener los nuevos precios en tiempo real.

---

## Extensiones de Dominio del Vehículo (Base de Datos)

El esquema relacional de la tabla `Vehicles` en PostgreSQL se extendió para almacenar información adicional de negocio:
* `Title` (Título descriptivo comercial del vehículo).
* `BasePrice` (Precio de reserva base para la subasta).
* `MinimumBidIncrement` (Incremento mínimo requerido para cada puja sucesiva).
* `Category` (Categoría de mercado: sedan, suv, coupe, etc.).
* `DocumentStorageKey` (Identificador del documento legal/título almacenado en el file storage).

Estos campos son poblados automáticamente al levantar el contenedor de base de datos con el perfil `--profile seed`.

