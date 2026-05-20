# PulseEDR — Light EDR + Security Coach

> Personal Security Posture Analyzer with CVE matching and explainable risk scoring

[![CI](https://github.com/404AndreiaNotFound/PulseEDR-ProiectIPDP/actions/workflows/ci.yml/badge.svg)](https://github.com/404AndreiaNotFound/PulseEDR-ProiectIPDP/actions/workflows/ci.yml)

## Overview

PulseEDR is a desktop security companion that scans your local system, identifies risky patterns (suspicious processes, outdated software with known CVEs, risky network connections, recent unsafe downloads), computes an explainable risk score, and provides actionable remediation steps.

Unlike a classic antivirus, PulseEDR focuses on **posture**, **explainability**, and **what-if analysis**.

## Key Features

- Local scan: processes, TCP connections, installed software, recent files
- Explainable risk score with per-finding evidence
- CVE matching against NVD feed (subset)
- What-if simulator: "what happens to my score if I remove X?"
- Policy-as-code: detection rules in YAML
- Containerized backend (Docker + PostgreSQL)
- Native Windows desktop client (WPF, MVVM)

## Architecture

    Desktop (WPF/MVVM) --> API (ASP.NET Core) --> PostgreSQL
                                |
                                +--> CVE Feed Importer (Background Service)

See [docs/architecture.md](docs/architecture.md) for details.

## Tech Stack

| Layer | Technology |
|---|---|
| Desktop | WPF, MVVM, CommunityToolkit.Mvvm, ModernWpfUI |
| API | ASP.NET Core 8 (Minimal API), Swagger/OpenAPI |
| Persistence | PostgreSQL 16, EF Core 8 |
| Auth | JWT Bearer |
| Testing | xUnit, FluentAssertions, Moq, WebApplicationFactory, NBomber |
| CI/CD | GitHub Actions |
| Container | Docker, docker-compose |
| Installer | Inno Setup |

## Design Patterns

- **Strategy** — pluggable risk detectors
- **Factory** — detector instantiation
- **Repository** — data access abstraction
- **Observer** — event-driven scan pipeline
- **Adapter** — CVE source normalization
- **Chain of Responsibility** — rule evaluation pipeline

## Project Structure

    PulseEDR/
    ├── src/
    │   ├── PulseEDR.Domain/          # Entities, value objects, domain interfaces
    │   ├── PulseEDR.Application/     # Use cases, DTOs, service contracts
    │   ├── PulseEDR.Infrastructure/  # EF Core, repos, CVE feed adapter
    │   ├── PulseEDR.Agent/           # Local data collectors
    │   ├── PulseEDR.Api/             # REST API (ASP.NET Core)
    │   └── PulseEDR.Desktop/         # WPF client
    ├── tests/
    │   ├── PulseEDR.UnitTests/
    │   ├── PulseEDR.IntegrationTests/
    │   └── PulseEDR.PerformanceTests/
    ├── docker/
    ├── installer/
    ├── docs/
    └── .github/workflows/

## Getting Started

### Prerequisites

- .NET 8 SDK
- Docker Desktop
- Windows 10/11 (for the desktop client)

### Quick start

    git clone https://github.com/404AndreiaNotFound/PulseEDR-ProiectIPDP.git
    cd PulseEDR-ProiectIPDP
    docker compose -f docker/docker-compose.yml up -d
    dotnet run --project src/PulseEDR.Api
    dotnet run --project src/PulseEDR.Desktop

API will be available at `https://localhost:5001/swagger`.

## Team

| Role | Member |
|---|---|
| Backend & Architecture | Andreia |
| Frontend Desktop | Luca |
| Testing & Installer | Laurentiu |

## Documentation

- [Architecture](docs/architecture.md)
- [Threat Model (STRIDE)](docs/threat-model.md)
- [Test Plan](docs/test-plan.md)
- [Architecture Decision Records](docs/adr/)

## License

This project is for academic purposes.