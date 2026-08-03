variable "project_id" {
  description = "ID del proyecto en Google Cloud Platform."
  type        = string
}

variable "region" {
  description = "Región de GCP donde se aprovisionará Cloud SQL."
  type        = string
}

variable "vpc_network_id" {
  description = "ID de la red VPC asignada a la conexión privada de la base de datos."
  type        = string
}

variable "tier" {
  description = "Tipo de máquina para Cloud SQL Instance (ej: db-custom-1-3840 o db-f1-micro para dev)."
  type        = string
  default     = "db-custom-1-3840"
}

variable "db_name" {
  description = "Nombre de la base de datos relacional de AutoPulse."
  type        = string
  default     = "autopulse_db"
}

variable "db_user" {
  description = "Nombre del usuario de la base de datos."
  type        = string
  default     = "autopulse_app_user"
}

variable "db_password" {
  description = "Contraseña segura del usuario de la base de datos."
  type        = string
  sensitive   = true
}
