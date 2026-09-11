# Implementation Plan: Document Upload and Management

**Branch**: `002-document-upload-management` | **Date**: 2026-09-12 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/002-document-upload-management/spec.md`

## Summary

Add permission-aware document upload, browsing, sharing, maintenance, task attachment, dashboard integration, notifications, and administrator audit reporting to the existing offline Blazor Server dashboard. The implementation extends the current Models/Data/Services/Pages layers, stores files through an interface-backed local provider outside `wwwroot`, fails closed when security scanning cannot complete, and validates access in the service layer and protected download/preview boundary.

## Technical Context

**Language/Version**: C# on .NET 9.0 with nullable reference types enabled  
**Primary Dependencies**: ASP.NET Core Blazor Server, Entity Framework Core 9.0.3, SQLite, existing cookie authentication and notification services  
**Storage**: Existing SQLite database for metadata plus protected local filesystem storage under application data, outside `wwwroot`  
**Testing**: New focused .NET test project for service/storage behavior; `dotnet build`; manual Blazor browser validation from quickstart because the repository currently has no test project  
**Target Platform**: Windows/offline training environment hosting ASP.NET Core over HTTPS  
**Project Type**: Single web application  
**Performance Goals**: Upload up to 25 MB within 30 seconds; accessible list/search within 2 seconds for 500 documents; preview within 3 seconds  
**Constraints**: No cloud dependency; preserve mock roles and current project membership; integer document IDs; text categories; files outside `wwwroot`; fail closed on scan failure/unavailability; no major rewrite  
**Scale/Scope**: Existing training dashboard users and projects; one document feature spanning six user journeys, four new entities, one storage boundary, and existing task/dashboard/notification surfaces

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Training Scope and Simplicity**: PASS. The design stays in the existing single Blazor/EF application and uses only abstractions required by the storage and security contracts.
- **Layered Design and Replaceable Infrastructure**: PASS. UI calls services; services use EF and `IFileStorageService`/`IFileSecurityScanner`; storage selection is dependency-injection based.
- **Authorization at Every Data Boundary**: PASS. Service operations and protected stream access enforce owner, administrator, project membership, department sharing, and manager rules.
- **Secure and Observable File Handling**: PASS. Validation, fail-closed scanning, generated paths, save-before-metadata ordering, cleanup, and activity records are specified.
- **Evidence-Driven Quality**: PASS with explicit test-project addition. Focused automated coverage and manual acceptance flows are defined in `quickstart.md`.

## Project Structure

### Documentation (this feature)

```text
specs/002-document-upload-management/
├── plan.md
├── research.md
├── data-model.md
├── contracts/
│   └── document-service.md
├── quickstart.md
└── checklists/
    └── requirements.md
```

### Source Code (repository root)

```text
ContosoDashboard/
├── Data/
│   └── ApplicationDbContext.cs
├── Models/
│   ├── Document.cs
│   ├── DocumentActivity.cs
│   ├── DocumentShare.cs
│   └── TaskAttachment.cs
├── Services/
│   ├── DocumentService.cs
│   ├── FileStorageService.cs
│   ├── FileSecurityScanner.cs
│   └── DashboardService.cs (extend)
├── Pages/
│   ├── Documents.razor
│   ├── ProjectDetails.razor (extend)
│   ├── Tasks.razor (extend)
│   ├── Index.razor (extend)
│   └── DocumentDownload.cshtml or protected endpoint (add)
├── Shared/
│   └── NavMenu.razor (extend)
└── Program.cs (register services and protected boundary)

ContosoDashboard.Tests/
├── DocumentServiceTests.cs
├── FileStorageServiceTests.cs
└── TestDbFactory.cs
```

**Structure Decision**: Keep the production feature in the existing `ContosoDashboard` web project and add a focused `ContosoDashboard.Tests` project for service, authorization, persistence, and storage tests. The UI remains Blazor Server; protected file delivery is an application endpoint or equivalent authorized stream operation, never static-file exposure.

## Phase 0: Research Output

See [research.md](research.md). Decisions cover the existing layered architecture, service-level authorization, local storage abstraction, transaction/cleanup ordering, persisted department resolution, integer/text schema constraints, test strategy, and fail-closed security scanning.

## Phase 1: Design Output

See [data-model.md](data-model.md), [contracts/document-service.md](contracts/document-service.md), and [quickstart.md](quickstart.md). The design defines entity fields/relationships, lifecycle transitions, user-visible operation contracts, protected delivery, and runnable validation scenarios.

## Constitution Check (Post-Design)

- **Training Scope and Simplicity**: PASS. No separate API, cloud SDK, or document-specific team-management system is introduced.
- **Layered Design and Replaceable Infrastructure**: PASS. Storage and scanning are replaceable interfaces; business rules remain in `DocumentService`.
- **Authorization at Every Data Boundary**: PASS. Current project membership and department data are checked at operation time; direct file delivery is protected.
- **Secure and Observable File Handling**: PASS. File paths are generated and private, scan failures deny availability, cleanup is defined, and audit data is retained for governed actions.
- **Evidence-Driven Quality**: PASS. The focused test project and quickstart cover acceptance, security, failure cleanup, and performance checks.

## Complexity Tracking

No constitution violations require justification. The additional test project, storage abstraction, and scanner abstraction are required by the constitution and feature contract rather than speculative complexity.
