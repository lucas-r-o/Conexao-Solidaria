# Conexão Solidária — Main API

This API was developed for the Conexão Solidária hackathon. It manages users (NGOs and donors), creates fundraising campaigns, and records donation intent events.

## Technologies used
- .NET (C#)
- Entity Framework Core
- PostgreSQL
- RabbitMQ & MassTransit (asynchronous messaging)
- Docker & Docker Compose
- Kubernetes (Docker Desktop)
- Prometheus & Grafana (observability)

## Requirements
- Docker Desktop installed and running, with Kubernetes enabled if you want to use the K8s environment.
- .NET SDK installed.
- `kubectl` configured for the local Docker Desktop cluster.

---

## Local infrastructure

The project supports two common environments:
- Local development with Docker Compose (RabbitMQ only, plus an external PostgreSQL instance such as Supabase or a local PostgreSQL server)
- Kubernetes deployment using the manifests in `src/ConexaoSolidaria.API/k8s`

### Start RabbitMQ locally

From the repository root:

```bash
docker compose -f src/docker-compose.yml up -d
```

This compose file starts only RabbitMQ, which is the component used for outbound events. The application expects a PostgreSQL connection string configured separately (for example via `appsettings.Development.json`, environment variables, or User Secrets).

> The project is configured to use Supabase/PostgreSQL in development by default, so the local PostgreSQL service is not spun up by the repo's Docker Compose file.

---

## Local API configuration

The API is configured using `appsettings.Development.json` and can also be complemented with .NET User Secrets. Do not commit real credentials or production connection strings.

1. Go to the API folder:

```bash
cd src/ConexaoSolidaria.API
```

2. Create or edit `appsettings.Development.json` with a configuration similar to:

```json
{
  "ConnectionStrings": {
    "CONEXAO_SOLIDARIA": "Host=localhost;Port=5432;Database=conexaosolidaria;Username=postgres;Password=sua_senha_aqui"
  },
  "Jwt": {
    "Key": "replace_with_a_secure_key_of_at_least_256_bits"
  },
  "Bus": {
    "Host": "localhost",
    "Port": "5672",
    "User": "guest",
    "Password": "guest"
  }
}
```

3. `Bus:Host` depends on the environment:

| Environment | `Bus:Host` value |
| --- | --- |
| Docker Compose | `localhost` |
| Kubernetes (Docker Desktop) | `host.docker.internal` |

4. RabbitMQ is exposed locally on port `5672` with credentials `guest` / `guest`.

5. Apply database migrations:

```bash
dotnet ef database update --project src/ConexaoSolidaria.API --startup-project src/ConexaoSolidaria.API
```

If you are already inside the API folder, this is enough:

```bash
dotnet ef database update
```

6. Run the API:

```bash
dotnet run --project src/ConexaoSolidaria.API
```

The project is configured with the `http` launch profile at `http://localhost:5155`.

---

## Running via Kubernetes

The application can also run inside a local Kubernetes cluster (Docker Desktop).

### Step 1: Configure secrets

The file `src/ConexaoSolidaria.API/k8s/secrets.yaml` is committed as a template with sensitive values left blank.

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: conexao-solidaria-secrets
  namespace: apps
type: Opaque
stringData:
  ConnectionStrings__CONEXAO_SOLIDARIA: ""
  Bus__Password: ""
  Jwt__Key: ""
```

Fill in these values locally before applying the manifests:
- `ConnectionStrings__CONEXAO_SOLIDARIA`: PostgreSQL connection string (Supabase or local DB)
- `Jwt__Key`: secure key with at least 256 bits

> Do not commit this file with real values.

### Step 2: Build the Docker image for the API

From the repository root:

```bash
docker build -f src/Docker/Dockerfile -t conexao-solidaria-api:latest .
```

> The image name must remain exactly `conexao-solidaria-api:latest`, because the Kubernetes manifests reference it directly.

### Step 3: Apply the Kubernetes manifests

Go to the API manifests folder:

```bash
cd src/ConexaoSolidaria.API/k8s
```

Create the namespace:

```bash
kubectl apply -f namespace.yaml
```

Then apply the remaining manifests:

```bash
kubectl apply -f .
```

### Step 4: Verify the pod is running

```bash
kubectl get pods -n apps
```

The API pod should show `Running` status.

> Since this is a local Docker Desktop cluster, the API running inside a pod must connect to RabbitMQ and Postgres using `host.docker.internal` instead of `localhost`.

---

## CI/CD

The pipeline is defined in `.github/workflows/ci.yml` and runs automatically on pushes to `main`.

It performs the following steps in order:
1. Restore dependencies
2. Build the solution
3. Run unit tests
4. Build and push the API Docker image to Docker Hub with the tags `latest` and the commit SHA

---

## Observability

The API exposes metrics at `/metrics` and health checks at `/health` (used by Kubernetes probes). These are consumed by Prometheus and visualized in Grafana.

### Local environment (Docker Compose)

Create a file named `prometheus.yml` in the repo root (same folder as the Docker Compose file) with:

```yaml
scrape_configs:
  - job_name: 'conexao-solidaria-api'
    tls_config:
      insecure_skip_verify: true
    static_configs:
      - targets: ['host.docker.internal:5155']
    metrics_path: '/metrics'
    scrape_interval: 15s
```

> Port `5155` is the local HTTP port used by the API (`http` launch profile from `launchSettings.json`). Adjust it if needed.

Start the infrastructure:

```bash
docker compose -f src/docker-compose.yml up -d
```

Then access:
- Prometheus: http://localhost:9090
- Grafana: http://localhost:3000 (default login: `admin` / `admin`)

In Grafana, add Prometheus as a data source (`http://prometheus:9090`) and create dashboards for the desired metrics (for example, HTTP request count and CPU/memory usage).

### Kubernetes

With the cluster running (`kubectl apply -f .` inside `src/ConexaoSolidaria.API/k8s`), access:
- Prometheus: http://localhost:30090
- Grafana: http://localhost:30000 (login: `admin` / `admin`)

Grafana is started with Prometheus already configured as a data source (`http://prometheus-service:9090`). The dashboard itself must be created/imported manually on first use.

---

## RabbitMQ management

To monitor events such as `DoacaoRecebidaEvent` flowing through the queue:

- Docker Compose: http://localhost:15672
- Kubernetes: http://localhost:30015

Default credentials in both environments: `guest` / `guest`.

---

## Running unit tests

Run the xUnit suite from the repository root:

```bash
dotnet test src/ConexaoSolidaria.Tests
```

Alternative from the solution root:

```bash
dotnet test src/ConexaoSolidaria.slnx
```

---

## Testing endpoints

You can exercise the API flows (public registration, login for multiple roles, campaigns, and donation processing via queue) by opening [src/ConexaoSolidaria.API/ConexaoSolidaria.API.http](src/ConexaoSolidaria.API/ConexaoSolidaria.API.http) in your editor and sending the requests in order.

---

## Integration with the Worker (multi-repository)

This project was designed with an event-driven architecture to support high scalability in donation processing. The solution is split across two repositories:

1. Main API (`application` — this repository): handles business rules, campaign persistence, authentication, and donation event publication to RabbitMQ.
2. Worker (`worker` — sibling repository): background service that consumes donation messages from the RabbitMQ queue, calculates amounts, and updates the fundraising totals and progress in the same PostgreSQL database.

### End-to-end flow:

![Architecture diagram](diagrama-arquitetura.png)

---

## Useful commands

```bash
# restore dependencies
dotnet restore src/ConexaoSolidaria.slnx

# build solution
dotnet build src/ConexaoSolidaria.slnx

# run tests
dotnet test src/ConexaoSolidaria.Tests

# start RabbitMQ
docker compose -f src/docker-compose.yml up -d

# run API
dotnet run --project src/ConexaoSolidaria.API
```

## Notes

- Keep secrets out of source control. Use environment variables, User Secrets, or Kubernetes secrets.
- Start RabbitMQ before starting the API.
- Do not modify event contracts without checking compatibility with the worker repository.
- The worker is not included in this solution and is maintained in a separate repository.
