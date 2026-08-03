# --------------------------------------------------------------------------------------------------------------------
# CLOUD SQL POSTGRESQL 16 MODULE FOR AUTOPULSE RELATIONAL STORAGE
# --------------------------------------------------------------------------------------------------------------------

# Generación de sufijo aleatorio para garantizar unicidad en el nombre de la instancia
resource "random_id" "db_suffix" {
  byte_length = 4
}

# 1. Instancia de PostgreSQL 16 con IP Privada (Sin IP Pública para garantizar seguridad)
resource "google_sql_database_instance" "postgres" {
  name             = "autopulse-postgres-${random_id.db_suffix.hex}"
  project          = var.project_id
  region           = var.region
  database_version = "POSTGRES_16"

  deletion_protection = false # Cambiar a true en entorno de producción estricto

  settings {
    tier              = var.tier
    availability_type = "ZONAL" # Usar REGIONAL para High Availability en prod
    disk_size         = 20
    disk_type         = "PD_SSD"
    disk_autoscale    = true

    ip_configuration {
      ipv4_enabled    = false # Desactiva acceso por IP Pública
      private_network = var.vpc_network_id
      ssl_mode        = "ENCRYPTED_ONLY"
    }

    backup_configuration {
      enabled                        = true
      start_time                     = "03:00"
      point_in_time_recovery_enabled = true
    }

    user_labels = {
      system      = "autopulse"
      environment = "production"
      database    = "postgresql-16"
    }
  }
}

# 2. Base de Datos Relacional 'autopulse_db'
resource "google_sql_database" "autopulse_db" {
  name     = var.db_name
  project  = var.project_id
  instance = google_sql_database_instance.postgres.name
}

# 3. Usuario de Aplicación de AutoPulse con Contraseña Parametrizada
resource "google_sql_user" "app_user" {
  name     = var.db_user
  project  = var.project_id
  instance = google_sql_database_instance.postgres.name
  password = var.db_password
}
