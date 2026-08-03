variable "project_id" {
  description = "ID del proyecto en Google Cloud Platform."
  type        = string
}

variable "topic_name" {
  description = "Nombre del tema principal de Pub/Sub para eventos de subasta."
  type        = string
  default     = "autopulse-auction-events"
}

variable "subscription_name" {
  description = "Nombre de la suscripción para el procesamiento de pujas."
  type        = string
  default     = "autopulse-bid-processing-sub"
}

variable "message_retention_duration" {
  description = "Duración de retención de mensajes no confirmados (ej: 604800s = 7 días)."
  type        = string
  default     = "604800s"
}

variable "max_delivery_attempts" {
  description = "Número máximo de reintentos antes de enviar el mensaje al Dead Letter Topic (DLQ)."
  type        = number
  default     = 5
}
