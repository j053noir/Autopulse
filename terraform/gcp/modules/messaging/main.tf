# --------------------------------------------------------------------------------------------------------------------
# PUBLISHER & SUBSCRIBER PUB/SUB MODULE FOR AUTOPULSE EVENT-DRIVEN ARCHITECTURE (.NET 10 + Pub/Sub)
# --------------------------------------------------------------------------------------------------------------------

# 1. Dead Letter Topic (DLQ) para capturar mensajes defectuosos o no procesados tras exceder reintentos
resource "google_pubsub_topic" "dlq_topic" {
  name    = "${var.topic_name}-dlq"
  project = var.project_id

  labels = {
    system      = "autopulse"
    environment = "event-driven"
    type        = "dead-letter-queue"
  }
}

# Suscripción de monitoreo/auditoría para la DLQ
resource "google_pubsub_subscription" "dlq_subscription" {
  name    = "${var.topic_name}-dlq-sub"
  project = var.project_id
  topic   = google_pubsub_topic.dlq_topic.id

  message_retention_duration = "1209600s" # 14 días para análisis de errores

  expiration_policy {
    ttl = "" # No expira automáticamente
  }
}

# 2. Tema Principal Pub/Sub de Eventos de Subasta
resource "google_pubsub_topic" "auction_events" {
  name    = var.topic_name
  project = var.project_id

  labels = {
    system      = "autopulse"
    environment = "event-driven"
  }
}

# 3. Permisos IAM requeridos para que Pub/Sub pueda publicar mensajes no entregados a la DLQ
resource "google_project_service_identity" "pubsub_identity" {
  provider = google-beta
  project  = var.project_id
  service  = "pubsub.googleapis.com"
}

resource "google_pubsub_topic_iam_member" "pubsub_dlq_publisher" {
  project = var.project_id
  topic   = google_pubsub_topic.dlq_topic.name
  role    = "roles/pubsub.publisher"
  member  = "serviceAccount:${google_project_service_identity.pubsub_identity.email}"
}

resource "google_pubsub_subscription_iam_member" "pubsub_subscriber_dlq_subscriber" {
  project      = var.project_id
  subscription = google_pubsub_subscription.bid_processing.name
  role         = "roles/pubsub.subscriber"
  member       = "serviceAccount:${google_project_service_identity.pubsub_identity.email}"
}

# 4. Suscripción Principal Pub/Sub con Política de DLQ y Retención de Mensajes
resource "google_pubsub_subscription" "bid_processing" {
  name    = var.subscription_name
  project = var.project_id
  topic   = google_pubsub_topic.auction_events.id

  # Configuración de tiempo de espera y retención
  ack_deadline_seconds       = 20
  message_retention_duration = var.message_retention_duration
  retain_acked_messages      = false

  # Política de Dead Letter Topic (DLQ)
  dead_letter_policy {
    dead_letter_topic     = google_pubsub_topic.dlq_topic.id
    max_delivery_attempts = var.max_delivery_attempts
  }

  # Reintentos con exponencial backoff
  retry_policy {
    minimum_backoff = "10s"
    maximum_backoff = "600s"
  }

  depends_on = [
    google_pubsub_topic_iam_member.pubsub_dlq_publisher
  ]
}
