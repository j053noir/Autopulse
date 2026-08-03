output "db_instance_name" {
  description = "Nombre de la instancia de Cloud SQL PostgreSQL creada."
  value       = google_sql_database_instance.postgres.name
}

output "db_connection_name" {
  description = "Connection Name de la instancia Cloud SQL para Cloud SQL Auth Proxy / Cloud Run."
  value       = google_sql_database_instance.postgres.connection_name
}

output "db_private_ip" {
  description = "IP privada asignada a la instancia de Cloud SQL dentro de la VPC."
  value       = google_sql_database_instance.postgres.private_ip_address
}

output "database_name" {
  description = "Nombre de la base de datos PostgreSQL."
  value       = google_sql_database.autopulse_db.name
}
