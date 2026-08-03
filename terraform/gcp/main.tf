# --------------------------------------------------------------------------------------------------------------------
# AUTOPULSE INFRASTRUCTURE IAC MAIN CONFIGURATION (GCP)
# --------------------------------------------------------------------------------------------------------------------

terraform {
  required_version = ">= 1.6.0"

  required_providers {
    google = {
      source  = "hashicorp/google"
      version = "~> 5.30.0"
    }
    google-beta = {
      source  = "hashicorp/google-beta"
      version = "~> 5.30.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.6.0"
    }
  }

  # Configuración del Backend Remoto para almacenar terraform.tfstate en GCS
  backend "gcs" {
    bucket = "autopulse-tf-state"
    prefix = "terraform/state/autopulse"
  }
}

# 1. Configuración de Proveedores Google
provider "google" {
  project = var.project_id
  region  = var.region
}

provider "google-beta" {
  project = var.project_id
  region  = var.region
}

# 2. Configuración de VPC Privada y Conexión de Servicios Networking para Cloud SQL
resource "google_compute_network" "vpc_network" {
  name                    = "autopulse-vpc-${var.environment}"
  auto_create_subnetworks = true
}

resource "google_compute_global_address" "private_ip_address" {
  name          = "autopulse-private-ip-alloc"
  purpose       = "VPC_PEERING"
  address_type  = "INTERNAL"
  prefix_length = 16
  network       = google_compute_network.vpc_network.id
}

resource "google_service_networking_connection" "private_vpc_connection" {
  network                 = google_compute_network.vpc_network.id
  service                 = "servicenetworking.googleapis.com"
  reserved_peering_ranges = [google_compute_global_address.private_ip_address.name]
}

# 3. Módulo de Mensajería Event-Driven (Pub/Sub + DLQ)
module "messaging" {
  source     = "./modules/messaging"
  project_id = var.project_id
  topic_name = "autopulse-auction-events-${var.environment}"
}

# 4. Módulo de Almacenamiento Seguro (Cloud Storage - GCS)
module "storage" {
  source       = "./modules/storage"
  project_id   = var.project_id
  bucket_name  = "autopulse-vehicle-documents-${var.environment}"
  cors_origins = ["http://localhost:3000"]
}

# 5. Módulo de Base de Datos Relacional (Cloud SQL PostgreSQL 16)
module "database" {
  source         = "./modules/database"
  project_id     = var.project_id
  region         = var.region
  vpc_network_id = google_compute_network.vpc_network.id
  tier           = var.db_tier
  db_password    = var.db_password

  depends_on = [google_service_networking_connection.private_vpc_connection]
}
