variable "project_id" {
  description = "ID del proyecto en Google Cloud Platform."
  type        = string
}

variable "location" {
  description = "Ubicación del bucket de Cloud Storage (Multi-region o Region)."
  type        = string
  default     = "US"
}

variable "bucket_name" {
  description = "Nombre globalmente único del bucket de Cloud Storage."
  type        = string
  default     = "autopulse-vehicle-documents"
}

variable "cors_origins" {
  description = "Orígenes HTTP/HTTPS autorizados para CORS."
  type        = list(string)
  default     = ["http://localhost:3000"]
}
