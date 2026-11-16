# Sliplane Demo API

A small ASP.NET Core 10 API for practicing Sliplane deployments and private service connectivity.

## Endpoints

- `GET /` — app information
- `GET /health` — process health
- `GET /health/db` — PostgreSQL connectivity test
- `GET /health/cache` — Garnet connectivity test
- `GET /demo/cache` — writes and reads a sample Redis-compatible key

## Required environment variables

Set these in Sliplane. Do not commit real passwords or connection strings.

```text
DATABASE_URL=Host=<POSTGRES_INTERNAL_HOST>;Port=5432;Database=sliplane_demo;Username=demo_user;Password=<YOUR_PASSWORD>
REDIS_URL=<GARNET_INTERNAL_HOST>:6379,abortConnect=false
```

Use the actual internal hostnames shown on the PostgreSQL and Garnet service detail pages in Sliplane. The hostname may differ from the service display name.

## Local build

```bash
dotnet restore
dotnet run --project SliplaneDemo.Api
```

The Dockerfile listens on port `8080`.
