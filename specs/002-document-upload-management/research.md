# Research: Document Upload and Management

## Decision: Extend the existing single Blazor Server project

**Rationale**: The repository already separates Models, Data, Services, Pages, and Shared UI, and the constitution requires changes to fit the current architecture. A new application or major rewrite would add training complexity without user value.

**Alternatives considered**: A separate API and frontend were rejected because the feature is an internal Blazor workflow and the repository has no API boundary or test solution to support that split.

## Decision: Keep business authorization in the service layer

**Rationale**: `ProjectService` and `TaskService` enforce ownership, project membership, and manager access inside service operations. Document list, search, download, preview, edit, replace, share, and delete operations must follow the same pattern so direct links and UI omissions cannot bypass access checks.

**Alternatives considered**: UI-only checks and page-level role policies were rejected because they do not protect service calls or direct document links.

## Decision: Use an interface-backed local storage boundary

**Rationale**: The training environment is offline and requires local filesystem storage, while the specification requires a future managed-storage migration path. `IFileStorageService` will expose upload, download, delete, and URL/stream access operations; the local implementation will store generated relative paths outside `wwwroot`.

**Alternatives considered**: Saving files under `wwwroot` was rejected because it permits direct unauthenticated access. Embedding file bytes in SQLite was rejected because it conflicts with the required storage boundary and migration path.

## Decision: Persist metadata only after successful file save and security check

**Rationale**: The constitution requires the sequence of validation and security check, unique path generation, file save, and then metadata persistence. On database failure, the stored file must be deleted; on file or scan failure, no document record is created.

**Alternatives considered**: Creating a database record first was rejected because it creates orphaned or empty file references.

## Decision: Resolve department sharing from persisted users

**Rationale**: The accepted clarification defines Team Resources sharing by department, but the current login flow does not emit a department claim. The document service will query the requesting and recipient users from `ApplicationDbContext` for department membership, avoiding claim drift and keeping authorization tied to current data.

**Alternatives considered**: Adding department to the cookie claim was deferred because persisted user data is already the source of truth and membership can change during a session.

## Decision: Use integer keys and text categories

**Rationale**: Existing entities use integer keys and the feature constraints explicitly require integer `DocumentId` values and text category values. Categories will be validated against the six allowed strings at the service boundary.

**Alternatives considered**: GUID keys and enum-backed categories were rejected by the feature constraints.

## Decision: Validate with focused automated coverage plus manual browser flows

**Rationale**: The repository currently has no test project or test framework. The implementation plan should add a focused test project for service authorization, validation, persistence cleanup, and storage behavior, while the quickstart documents manual Blazor login and browser flows for upload, search, sharing, and download/preview.

**Alternatives considered**: Relying only on `dotnet build` was rejected because compilation cannot prove authorization or file cleanup behavior.

## Decision: Security scanning is fail-closed behind an abstraction

**Rationale**: The feature requires a malware/virus check but the offline repository has no scanner dependency. An `IFileSecurityScanner` boundary can provide a deterministic training implementation or configured unavailable state; an unavailable or failed scan rejects the file rather than exposing it.

**Alternatives considered**: Treating scan unavailability as success was rejected because it violates the security requirement and constitution.
