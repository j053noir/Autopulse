output "bucket_name" {
  description = "Nombre del bucket de Cloud Storage creado."
  value       = google_storage_bucket.vehicle_documents.name
}

output "bucket_url" {
  description = "URI gs:// del bucket de Cloud Storage."
  value       = google_storage_bucket.vehicle_documents.url
}
