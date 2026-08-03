# --------------------------------------------------------------------------------------------------------------------
# INFRASTRUCTURE OUTPUTS FOR API ENVIRONMENT & CI/CD PIPELINES
# --------------------------------------------------------------------------------------------------------------------

output "pubsub_topic_id" {
  description = "ID del tema Pub/Sub principal de subastas para la API .NET 10."
  value       = module.messaging.topic_id
}

output "pubsub_subscription_id" {
  description = "ID de la suscripción Pub/Sub para el worker/consumer de pujas."
  value       = module.messaging.subscription_id
}

output "pubsub_dlq_topic_id" {
  description = "ID del Dead Letter Topic (DLQ) para monitoreo de eventos erróneos."
  value       = module.messaging.dlq_topic_id
}

output "gcs_bucket_name" {
  description = "Nombre del bucket Cloud Storage para la carga de documentos de vehículos (V4 Signed URLs)."
  value       = module.storage.bucket_name
}

output "gcs_bucket_url" {
  description = "URL gs:// del bucket de Cloud Storage."
  value       = module.storage.bucket_url
}

output "db_connection_name" {
  description = "Connection Name de Cloud SQL PostgreSQL para la conexión Cloud Run / Cloud SQL Proxy."
  value       = module.database.db_connection_name
}

output "db_private_ip" {
  description = "IP privada asignada a la instancia PostgreSQL dentro de la VPC."
  value       = module.database.db_private_ip
}

output "db_name" {
  description = "Nombre de la base de datos PostgreSQL de AutoPulse."
  value       = module.database.database_name
}
