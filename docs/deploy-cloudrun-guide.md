# 🚀 Guía de Despliegue de AutoPulse (.NET 10 API & Next.js 16) en Google Cloud Run

Esta guía detalla los pasos de infraestructura en Google Cloud Platform (GCP) y la configuración de GitHub Actions para lograr despliegues continuos automatizados, seguros y con escalado a cero (*Scale-to-Zero*) para **AutoPulse**.

---

## 📋 Prerrequisitos en Google Cloud Platform

### 1. Variables Globales de Referencia
Reemplaza los valores entre corchetes con los datos de tu proyecto GCP:

```bash
export GCP_PROJECT_ID="tu-gcp-project-id"
export GCP_REGION="us-central1"
export ARTIFACT_REPO_NAME="autopulse-repository"
export SA_NAME="github-actions-deployer"
export SA_EMAIL="${SA_NAME}@${GCP_PROJECT_ID}.iam.gserviceaccount.com"
```

### 2. Habilitar las APIs Necesarias de GCP

```bash
gcloud services enable \
    run.googleapis.com \
    artifactregistry.googleapis.com \
    iamcredentials.googleapis.com \
    cloudresourcemanager.googleapis.com \
    --project="${GCP_PROJECT_ID}"
```

---

## 📦 Configuración de Google Artifact Registry

Crea el repositorio centralizado donde se almacenarán las imágenes Docker del backend (`autopulse-api`) y frontend (`autopulse-web`):

```bash
gcloud artifacts repositories create ${ARTIFACT_REPO_NAME} \
    --repository-format=docker \
    --location=${GCP_REGION} \
    --description="Repositorio de imágenes Docker para AutoPulse" \
    --project="${GCP_PROJECT_ID}"
```

---

## 🔐 Configuración de Service Account e Identidad IAM

### 1. Crear la Service Account para GitHub Actions

```bash
gcloud iam service-accounts create ${SA_NAME} \
    --display-name="GitHub Actions Deployer SA" \
    --project="${GCP_PROJECT_ID}"
```

### 2. Asignar Permisos IAM Mínimos Requeridos

```bash
# Permiso para administrar Cloud Run Services
gcloud projects add-iam-policy-binding ${GCP_PROJECT_ID} \
    --member="serviceAccount:${SA_EMAIL}" \
    --role="roles/run.admin"

# Permiso para publicar imágenes en Artifact Registry
gcloud projects add-iam-policy-binding ${GCP_PROJECT_ID} \
    --member="serviceAccount:${SA_EMAIL}" \
    --role="roles/artifactregistry.writer"

# Permiso para actuar como la Service Account en Cloud Run
gcloud projects add-iam-policy-binding ${GCP_PROJECT_ID} \
    --member="serviceAccount:${SA_EMAIL}" \
    --role="roles/iam.serviceAccountUser"
```

---

## 🔑 Autenticación Segura: Opción A vs Opción B

### Opción A: Workload Identity Federation (Recomendado - Sin llaves/No-Key Auth)

Workload Identity Federation elimina la necesidad de almacenar llaves JSON persistentes en GitHub Secrets.

#### 1. Crear el Workload Identity Pool:
```bash
gcloud iam workload-identity-pools create "github-pool" \
    --project="${GCP_PROJECT_ID}" \
    --location="global" \
    --display-name="GitHub Actions Pool"
```

#### 2. Crear el Provider para GitHub:
```bash
gcloud iam workload-identity-pools providers create-oidc "github-provider" \
    --project="${GCP_PROJECT_ID}" \
    --location="global" \
    --workload-identity-pool="github-pool" \
    --display-name="GitHub Provider" \
    --attribute-mapping="google.subject=assertion.sub,attribute.actor=assertion.actor,attribute.repository=assertion.repository" \
    --issuer-uri="https://token.actions.githubusercontent.com"
```

#### 3. Vincular el Repositorio de GitHub con la Service Account:
*(Reemplaza `ORGANIZACION_O_USUARIO/REPOSITORIO` con el repo de tu GitHub, ej: `j053noir/Autopulse`)*

```bash
gcloud iam service-accounts add-iam-policy-binding "${SA_EMAIL}" \
    --project="${GCP_PROJECT_ID}" \
    --role="roles/iam.workloadIdentityUser" \
    --member="principalSet://iam.googleapis.com/projects/$(gcloud projects describe ${GCP_PROJECT_ID} --format='value(projectNumber)')/locations/global/workloadIdentityPools/github-pool/attribute.repository/ORGANIZACION_O_USUARIO/REPOSITORIO"
```

---

### Opción B: Service Account Key (Llave JSON Convencional)

Si prefieres usar la autenticación basada en llaves JSON tradicionales:

```bash
gcloud iam service-accounts keys create gcp-key.json \
    --iam-account=${SA_EMAIL} \
    --project=${GCP_PROJECT_ID}
```

Copia el contenido entero de `gcp-key.json` para registrarlo como el secreto `GCP_SA_KEY` en GitHub.

---

## 🔑 Secretos a Configurar en GitHub Repository Settings

Dirígete a **GitHub Repository -> Settings -> Secrets and variables -> Actions** y registra lo siguiente:

| Nombre del Secreto | Descripción / Ejemplo | Requerido En |
| :--- | :--- | :--- |
| `GCP_PROJECT_ID` | ID de tu proyecto en Google Cloud (ej. `autopulse-prod-12345`) | Backend y Frontend |
| `GCP_SA_KEY` | *(Solo Opción B)* Contenido raw en JSON de `gcp-key.json` | Fallback / Opción B |
| `GCP_WIF_PROVIDER` | *(Solo Opción A)* Formato: `projects/NUMERO/locations/global/workloadIdentityPools/github-pool/providers/github-provider` | Opción A (WIF) |
| `GCP_WIF_SA_EMAIL` | *(Solo Opción A)* Email de la SA (`github-actions-deployer@...`) | Opción A (WIF) |
| `NEXT_PUBLIC_API_URL` | URL pública de Cloud Run de la API (ej. `https://autopulse-api-xyz-uc.a.run.app`) | Solo Frontend |

---

## ⚡ Características del Despliegue en Cloud Run

- **Scale-to-Zero (`--min-instances=0`)**: Cuando no hay peticiones entrantes, Cloud Run reduce la cantidad de contenedores a cero, eliminando costos por inactividad.
- **Zero-Downtime Releases**: Cloud Run maneja automáticamente las revisiones. Las peticiones continuarán dirigiéndose al contenedor anterior hasta que la nueva versión responda satisfactoriamente a los Health Checks.
