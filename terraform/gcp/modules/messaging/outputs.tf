output "topic_id" {
  description = "ID completo del tema principal Pub/Sub para eventos de subasta."
  value       = google_pubsub_topic.auction_events.id
}

output "topic_name" {
  description = "Nombre del tema principal Pub/Sub."
  value       = google_pubsub_topic.auction_events.name
}

output "subscription_id" {
  description = "ID completo de la suscripción de procesamiento de pujas."
  value       = google_pubsub_subscription.bid_processing.id
}

output "dlq_topic_id" {
  description = "ID del Dead-Letter Topic (DLQ)."
  value       = google_pubsub_topic.dlq_topic.id
}
