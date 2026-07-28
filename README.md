# DevOpsHub — Full-Stack & High-Performance Observability Platform
<img width="1536" height="1024" alt="Image" src="https://github.com/user-attachments/assets/d81bc376-a52a-4cef-a26f-84c8575eb0e8" />

A production-ready full-stack platform built to orchestrate DevOps automation workflows, monitor infrastructure in real time, and execute high-performance native system operations through C modules.

The platform emphasizes **security-first architecture**, **containerized deployments**, **enterprise observability**, and **high-performance backend services**, making it suitable as a portfolio project demonstrating enterprise-grade engineering practices.

---

# Architecture Overview

DevOpsHub connects modern web technologies with low-level native performance modules while maintaining full observability across the stack.

```text
               ┌─────────────────────────────────────────┐
               │           React Frontend (UI)           │
               │ TanStack Query • Error Boundaries • TS  │
               └────────────────────┬────────────────────┘
                                    │
                              HTTP / REST
                                    │
                                    ▼
               ┌─────────────────────────────────────────┐
               │         Node.js / Express API           │
               │ Helmet • Zod • Rate Limit • Pino Logger │
               └──────────────┬───────────────┬──────────┘
                              │               │
                     Native FFI│         /metrics
                       Calls    │               │
                              ▼               ▼
      ┌────────────────────────────┐   ┌────────────────────────────┐
      │ High-Performance C Modules │   │ Prometheus + Grafana Stack │
      │ Safe Buffers • Native APIs │   │ Metrics • Dashboards • SLO │
      └────────────────────────────┘   └────────────────────────────┘
```

---

# Key Features

## Security

- Strict request validation using **Zod**
- Input sanitization for all API endpoints
- **Helmet** security headers
- **Rate limiting** for authentication and sensitive endpoints
- JWT authentication with Bearer authorization
- Zero-trust API design
- Secure C memory operations using:
  - `snprintf`
  - `strncpy`
  - Explicit buffer boundary checks

---

## Backend

- Node.js
- Express
- TypeScript
- Modular architecture
- RESTful API
- Global error handling
- Structured JSON logging
- Health check endpoints
- Connection retry mechanism
- OpenAPI documentation

---

## Frontend

- React
- TypeScript
- TanStack Query
- Error Boundaries
- Skeleton Loading
- Responsive Dashboard
- Optimistic caching
- Component-driven architecture

---

## Native Performance Layer

Native C modules execute performance-sensitive operations while maintaining strict memory safety.

Features include:

- Safe string handling
- Buffer overflow protection
- Explicit memory ownership
- Compiler hardening
- Sanitizer-ready compilation

---

## DevOps

- Docker
- Docker Compose
- Multi-stage Docker builds
- GitHub Actions CI
- Automated type checking
- Automated linting
- Native C compilation
- Production-ready containerization

---

## Observability

Enterprise monitoring stack powered by Prometheus and Grafana.

### Metrics

- HTTP request count
- Request latency histogram
- Active requests
- HTTP 5xx error counter
- Process CPU usage
- Process memory usage
- Custom business metrics
- Health probes

---

## API Documentation

Interactive OpenAPI documentation is available through Swagger UI.

Features include:

- Bearer JWT authentication
- Request schemas
- Response schemas
- HTTP status codes
- Interactive endpoint testing

---

# Technology Stack

## Frontend

- React
- TypeScript
- TanStack Query
- Tailwind CSS

## Backend

- Node.js
- Express
- TypeScript
- Zod
- Helmet
- Pino

## Native Layer

- C
- Safe Buffer Utilities
- Native Performance Modules

## DevOps

- Docker
- Docker Compose
- GitHub Actions

## Observability

- Prometheus
- Grafana
- OpenAPI 3.0
- Swagger UI

---

# Project Structure

```text
DevOpsHub
│
├── frontend/
│   ├── React
│   ├── TanStack Query
│   ├── Components
│   └── Pages
│
├── backend/
│   ├── Express API
│   ├── Controllers
│   ├── Services
│   ├── Middleware
│   ├── Validation
│   └── Native Bridge
│
├── native/
│   ├── C Modules
│   ├── Safe Buffers
│   └── Performance Tasks
│
├── docker/
│
├── prometheus/
│
├── grafana/
│
└── .github/
    └── workflows/
```

---

# Services

| Service | URL | Description |
|----------|-----|-------------|
| Web Dashboard | http://localhost:3000 | React Frontend |
| REST API | http://localhost:5000/api | Express Backend |
| Swagger UI | http://localhost:5000/api-docs | OpenAPI Documentation |
| Health Check | http://localhost:5000/healthz | Liveness & Readiness |
| Metrics | http://localhost:5000/metrics | Prometheus Metrics |
| Grafana | http://localhost:3001 | Monitoring Dashboard |

---

# Production Features

- Enterprise-ready architecture
- Multi-stage Docker builds
- Health probes
- Structured JSON logging
- JWT authentication
- Request validation
- Rate limiting
- Security headers
- OpenAPI documentation
- Prometheus metrics
- Grafana dashboards
- Native C performance modules
- CI/CD pipeline
- Container health monitoring
- Error boundaries
- Query caching
- Global error handling
- Connection retry
- Production logging
- Memory-safe native code

---

# Continuous Integration

Every commit automatically runs:

- ESLint
- TypeScript strict compilation
- Native C compilation
- Unit tests
- Integration tests
- Docker image build
- Security validation

---

# Getting Started

```bash
git clone https://github.com/Dpehect/DevOpsHub-C-Full-Stack-Web-App.git

cd DevOpsHub-C-Full-Stack-Web-App

docker compose up --build
```

---

# License

This project is licensed under the **MIT License**.

See the **LICENSE** file for details.
