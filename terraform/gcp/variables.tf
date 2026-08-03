variable "project_id" {
  description = "ID del proyecto GCP objetivo."
  type        = string
}

variable "region" {
  description = "Región predeterminada de GCP para el despliegue de recursos."
  type        = string
  default     = "us-central1"
}

variable "environment" {
  description = "Ambiente de despliegue (dev, staging, prod)."
  type        = string
  default     = "dev"
}

variable "db_password" {
  description = "Contraseña para la base de datos PostgreSQL de Cloud SQL."
  type        = string
  sensitive   = true
}

variable "db_tier" {
  description = "Tier/Tipo de máquina para la base de datos Cloud SQL."
  type        = string
  default     = "db-custom-1-3840"
}
