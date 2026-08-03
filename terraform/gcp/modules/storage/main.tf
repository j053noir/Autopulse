# --------------------------------------------------------------------------------------------------------------------
# SECURE STORAGE BUCKET FOR AUTOPULSE VEHICLE DOCUMENTS (V4 SIGNED URL ACCESS ONLY)
# --------------------------------------------------------------------------------------------------------------------

resource "google_storage_bucket" "vehicle_documents" {
  name                     = var.bucket_name
  project                  = var.project_id
  location                 = var.location
  force_destroy            = false # Evita borrado accidental en producción
  storage_class            = "STANDARD"

  # Acceso uniforme activado para evitar ACLs legacy y garantizar Least Privilege
  uniform_bucket_level_access = true

  # Restricción de acceso público directo (Público bloqueado). El acceso es EXCLUSIVO por Signed URLs V4
  public_access_prevention = "enforced"

  # Reglas de CORS para permitir solicitudes directas PUT/GET/HEAD desde el Frontend (Next.js / Localhost)
  cors {
    origin          = var.cors_origins
    method          = ["GET", "PUT", "POST", "HEAD", "DELETE"]
    response_header = ["*"]
    max_age_seconds = 3600
  }

  # Configuración de Lifecycle Rule (Transición/Limpieza automática opcional para borradores)
  lifecycle_rule {
    action {
      type = "Delete"
    }
    condition {
      age        = 30 # Borra archivos temporales/incompletos de más de 30 días
      with_state = "ARCHIVED"
    }
  }

  labels = {
    system      = "autopulse"
    environment = "production"
    service     = "document-storage"
  }
}
