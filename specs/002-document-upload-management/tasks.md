# Tasks: Document Upload and Management

**Input**: Design documents from `/specs/002-document-upload-management/`
**Prerequisites**: [plan.md](plan.md), [spec.md](spec.md), [research.md](research.md), [data-model.md](data-model.md), [contracts/document-service.md](contracts/document-service.md)

**Organization**: Tasks are grouped by user story so each story can be implemented and tested independently after the foundational phase.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Establish the production and test project structure required by the feature.

- [X] T001 Create the `ContosoDashboard.Tests` test project targeting `net9.0` and reference `ContosoDashboard\ContosoDashboard.csproj` in `ContosoDashboard.Tests\ContosoDashboard.Tests.csproj`
- [X] T002 [P] Add the focused test framework and EF Core SQLite test dependencies to `ContosoDashboard.Tests\ContosoDashboard.Tests.csproj`
- [X] T003 [P] Add feature test database and temporary-storage fixtures in `ContosoDashboard.Tests\TestDbFactory.cs`
- [X] T004 [P] Add the protected upload-root configuration key and local development default outside `wwwroot` in `ContosoDashboard\appsettings.json` and `ContosoDashboard\appsettings.Development.json`
- [X] T005 [P] Add document navigation and route placeholders in `ContosoDashboard\Shared\NavMenu.razor` and `ContosoDashboard\Pages\Documents.razor`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Implement shared entities, storage/security boundaries, persistence, and authorization helpers before any user story work.

**Checkpoint**: Foundation ready; user story implementation can begin.

- [X] T006 Create `Document`, `DocumentShare`, `TaskAttachment`, and `DocumentActivity` entities with integer keys, required fields, bounded lengths, text categories, and navigation properties in `ContosoDashboard\Models\Document.cs`, `ContosoDashboard\Models\DocumentShare.cs`, `ContosoDashboard\Models\TaskAttachment.cs`, and `ContosoDashboard\Models\DocumentActivity.cs`
- [X] T007 Extend `ApplicationDbContext` with document DbSets, User/Project/Task relationships, indexes for uploader/project/category/date/search access, unique file-path protection, and duplicate-share/attachment constraints in `ContosoDashboard\Data\ApplicationDbContext.cs`
- [X] T008 [P] Define `IFileStorageService` and safe relative-path result contracts for upload, download, replacement, and deletion in `ContosoDashboard\Services\FileStorageService.cs`
- [X] T009 [P] Implement `LocalFileStorageService` with GUID-based `{userId}/{projectId-or-personal}/{guid}.{extension}` paths, root containment checks, async streams, and cleanup behavior in `ContosoDashboard\Services\FileStorageService.cs`
- [X] T010 [P] Define `IFileSecurityScanner` and a fail-closed offline implementation that rejects unavailable or failed scans in `ContosoDashboard\Services\FileSecurityScanner.cs`
- [X] T011 Implement shared document validation for the 25 MB limit, allowed PDF/Word/Excel/PowerPoint/text/JPEG/PNG types, six text categories, required title, 255-character MIME type, safe names, and tag normalization in `ContosoDashboard\Services\DocumentValidation.cs`
- [X] T012 Implement reusable document authorization checks for owner, administrator, current project member, project manager, explicit user share, current project-document team share, and department Team Resources share in `ContosoDashboard\Services\DocumentAuthorization.cs`
- [X] T013 Register storage, scanner, validation, authorization, and document services and configure the protected document endpoint pipeline in `ContosoDashboard\Program.cs`
- [X] T014 Add test helpers for authenticated user identities, seeded users/projects/members, temporary upload roots, and SQLite contexts in `ContosoDashboard.Tests\TestDbFactory.cs`
- [X] T015 [P] Add foundational tests for allowed categories/types, 25 MB rejection, MIME length, path traversal rejection, GUID path generation, root containment, and fail-closed scanning in `ContosoDashboard.Tests\DocumentValidationTests.cs` and `ContosoDashboard.Tests\FileStorageServiceTests.cs`

---

## Phase 3: User Story 1 - Upload and Organize Documents (Priority: P1) MVP

**Goal**: Authorized employees can upload one or more supported files with required metadata and see successful or rejected per-file results.

**Independent Test**: Upload a valid supported file as an authorized employee and verify metadata/list visibility; repeat with unsupported, oversized, unsafe, and failed-scan files and verify no available document record remains.

### Tests for User Story 1

- [ ] T016 [P] [US1] Add upload contract tests covering valid metadata, optional description/project/tags, per-file results, captured uploader/date/size/type, and required category in `ContosoDashboard.Tests\DocumentServiceUploadTests.cs`
- [ ] T017 [P] [US1] Add upload failure tests covering unsupported type, over-25-MB file, failed/unavailable scan, storage failure, database failure cleanup, and no orphan document/file state in `ContosoDashboard.Tests\DocumentServiceUploadTests.cs`

### Implementation for User Story 1

- [X] T018 [US1] Implement `IDocumentService.UploadAsync` with validation, project authorization, security scan, generated path, save-before-metadata ordering, database rollback cleanup, audit creation, and notification dispatch in `ContosoDashboard\Services\DocumentService.cs`
- [X] T019 [US1] Add upload request/result models and per-file error state in `ContosoDashboard\Models\DocumentUploadModels.cs`
- [X] T020 [US1] Implement the authorized upload form with multi-file selection, required title/category, optional description/project/tags, progress state, per-file results, and `@key` reset behavior in `ContosoDashboard\Pages\Documents.razor`
- [X] T021 [US1] Load current user identity and authorized project choices for the document page from `ClaimTypes.NameIdentifier` and `IProjectService` in `ContosoDashboard\Pages\Documents.razor`
- [X] T022 [US1] Add user-facing validation and failure messages that never expose physical paths or unsafe filenames in `ContosoDashboard\Pages\Documents.razor`

**Checkpoint**: User Story 1 is independently usable as the MVP.

---

## Phase 4: User Story 2 - Find and Use Accessible Documents (Priority: P1)

**Goal**: Users can browse, sort, filter, search, preview, and download only documents they currently may access.

**Independent Test**: Seed accessible and inaccessible documents, exercise every supported list/search criterion, and verify authorized preview/download plus denial without metadata disclosure.

### Tests for User Story 2

- [ ] T023 [P] [US2] Add list/search tests for title, description, tags, uploader, project, category, date range, title/date/category/size sorting, empty results, and 500-document query behavior in `ContosoDashboard.Tests\DocumentServiceQueryTests.cs`
- [ ] T024 [P] [US2] Add authorization tests proving inaccessible documents are excluded from list/search and direct preview/download attempts disclose no metadata in `ContosoDashboard.Tests\DocumentAccessTests.cs`
- [ ] T025 [P] [US2] Add protected endpoint tests for authorized download, PDF/image preview content types, unsupported preview, damaged file, deleted file, and path containment in `ContosoDashboard.Tests\DocumentEndpointTests.cs`

### Implementation for User Story 2

- [X] T026 [US2] Implement authorized document list/search queries with server-side access predicates, filters, sorting, uploader/project joins, and bounded result ordering in `ContosoDashboard\Services\DocumentService.cs`
- [X] T027 [US2] Implement preview/download stream operations that authorize before opening storage and return validated display name/content type in `ContosoDashboard\Services\DocumentService.cs`
- [X] T028 [US2] Add protected download and preview application endpoints that never serve the upload root through static files in `ContosoDashboard\Program.cs` and `ContosoDashboard\Pages\DocumentDownload.cshtml.cs`
- [X] T029 [US2] Add document table, search box, category/project/date filters, sortable headers, empty state, preview action, and download action in `ContosoDashboard\Pages\Documents.razor`
- [ ] T030 [US2] Add authorized project-document browsing to the project details view in `ContosoDashboard\Pages\ProjectDetails.razor`

**Checkpoint**: User Stories 1 and 2 are independently usable and permission-filtered.

---

## Phase 5: User Story 3 - Collaborate on Project and Shared Documents (Priority: P1)

**Goal**: Project members and explicitly shared recipients can access documents while unauthorized users remain denied; shares create notifications.

**Independent Test**: Test owner, project member, project manager, department recipient, explicit recipient, and unrelated user against project and Team Resources documents.

### Tests for User Story 3

- [ ] T031 [P] [US3] Add access tests for owner, administrator, current project member, project manager, explicit user share, current project team share, department Team Resources share, and revoked membership in `ContosoDashboard.Tests\DocumentAuthorizationTests.cs`
- [ ] T032 [P] [US3] Add share tests for valid targets, project-vs-department team scope, duplicate shares, notifications, Shared with Me visibility, and unauthorized sharing in `ContosoDashboard.Tests\DocumentShareTests.cs`

### Implementation for User Story 3

- [X] T033 [US3] Implement access predicates and share-target resolution using current `ProjectMembers` and persisted `User.Department` values in `ContosoDashboard\Services\DocumentAuthorization.cs`
- [X] T034 [US3] Implement `ShareAsync`, `GetSharedWithMeAsync`, and recipient notification creation with duplicate-target prevention and audit records in `ContosoDashboard\Services\DocumentService.cs`
- [X] T035 [US3] Add share target models and project/department recipient selection in `ContosoDashboard\Models\DocumentShareModels.cs`
- [ ] T036 [US3] Add share and Shared with Me controls to `ContosoDashboard\Pages\Documents.razor`
- [ ] T037 [US3] Add project document visibility and project-manager management actions to `ContosoDashboard\Pages\ProjectDetails.razor`
- [ ] T038 [US3] Add document notification enum values and notification messages for shares and project additions in `ContosoDashboard\Models\Notification.cs` and `ContosoDashboard\Services\NotificationService.cs`

**Checkpoint**: Project and explicit sharing are independently testable with current-membership authorization.

---

## Phase 6: User Story 4 - Maintain Document Metadata and Files (Priority: P2)

**Goal**: Owners and authorized project managers can edit, replace, and permanently delete documents without exposing stale or broken state.

**Independent Test**: An owner, project manager, and unauthorized user each attempt metadata edit, file replacement, and confirmed deletion.

### Tests for User Story 4

- [ ] T039 [P] [US4] Add metadata edit tests for owner/project-manager authorization, title/category/tag validation, updated search visibility, and unauthorized no-op behavior in `ContosoDashboard.Tests\DocumentMaintenanceTests.cs`
- [ ] T040 [P] [US4] Add replacement/delete tests for successful replacement, failed scan/storage preserving the old file, confirmed deletion, physical-file cleanup, retained delete audit, and stale-link denial in `ContosoDashboard.Tests\DocumentMaintenanceTests.cs`

### Implementation for User Story 4

- [X] T041 [US4] Implement authorized metadata edit with validation and audit activity in `ContosoDashboard\Services\DocumentService.cs`
- [X] T042 [US4] Implement replacement workflow that scans and saves the new file before metadata commit, preserves the old file on failure, and cleans up obsolete files after success in `ContosoDashboard\Services\DocumentService.cs`
- [X] T043 [US4] Implement confirmed deletion with authorization, storage deletion, relationship cleanup, retained delete audit, and stale-link denial in `ContosoDashboard\Services\DocumentService.cs`
- [ ] T044 [US4] Add edit metadata, replace file, confirmation, and delete result states to `ContosoDashboard\Pages\Documents.razor`

**Checkpoint**: Authorized maintenance is independently functional and failed mutations preserve prior state.

---

## Phase 7: User Story 5 - Connect Documents to Daily Dashboard Work (Priority: P2)

**Goal**: Users can attach documents to tasks, upload from task context, and see document summaries and notifications on the dashboard.

**Independent Test**: Attach/upload from a task, verify inherited project association, then verify five recent documents, count, and project notifications on the dashboard.

### Tests for User Story 5

- [ ] T045 [P] [US5] Add task attachment tests for accessible task/document authorization, task-project inheritance, duplicate attachment prevention, and notification behavior in `ContosoDashboard.Tests\TaskAttachmentTests.cs`
- [ ] T046 [P] [US5] Add dashboard query tests for five recent uploads, accessible document count, and exclusion of inaccessible documents in `ContosoDashboard.Tests\DashboardDocumentTests.cs`

### Implementation for User Story 5

- [X] T047 [US5] Implement `AttachToTaskAsync` and task-scoped upload orchestration that enforces task access and inherits the task project in `ContosoDashboard\Services\DocumentService.cs`
- [ ] T048 [US5] Extend task queries and task detail UI with related documents, attach-existing, and upload-from-task controls in `ContosoDashboard\Services\TaskService.cs` and `ContosoDashboard\Pages\Tasks.razor`
- [X] T049 [US5] Extend dashboard service queries for five recent accessible uploads and accessible document count in `ContosoDashboard\Services\DashboardService.cs`
- [X] T050 [US5] Add Recent Documents and document-count summary rendering to `ContosoDashboard\Pages\Index.razor`
- [X] T051 [US5] Dispatch project-member notifications when a project-associated document is successfully added in `ContosoDashboard\Services\DocumentService.cs` and `ContosoDashboard\Services\NotificationService.cs`

**Checkpoint**: Task and dashboard integration are independently demonstrable.

---

## Phase 8: User Story 6 - Audit Document Activity (Priority: P3)

**Goal**: Administrators can review document activity and usage reports; non-administrators are denied.

**Independent Test**: Perform representative actions as users, verify administrator reports include actor/document/action/time and report aggregates, and verify employee denial.

### Tests for User Story 6

- [ ] T052 [P] [US6] Add audit-record tests for upload, download, preview, edit, replace, share, attach, and delete actor/document/action/time fields in `ContosoDashboard.Tests\DocumentActivityTests.cs`
- [ ] T053 [P] [US6] Add administrator report tests for document types, active uploaders, access patterns, and non-administrator denial in `ContosoDashboard.Tests\DocumentReportTests.cs`

### Implementation for User Story 6

- [X] T054 [US6] Implement immutable activity writes for governed document operations and administrator-only aggregation queries in `ContosoDashboard\Services\DocumentService.cs`
- [X] T055 [US6] Add administrator report models and protected report route/page in `ContosoDashboard\Models\DocumentReportModels.cs` and `ContosoDashboard\Pages\DocumentReports.razor`
- [X] T056 [US6] Add administrator-only navigation and report access checks in `ContosoDashboard\Shared\NavMenu.razor` and `ContosoDashboard\Pages\DocumentReports.razor`

**Checkpoint**: Audit reporting is independently available to administrators and unavailable to other roles.

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Complete documentation, performance, security, and end-to-end validation across all stories.

- [ ] T057 [P] Add document upload, storage, authorization, sharing, task, dashboard, and audit behavior to `README.md` without describing the training mock authentication as production-ready
- [ ] T058 [P] Add focused authorization and file-handling regression coverage for direct links, revoked membership, department changes, path traversal, and stale deleted links in `ContosoDashboard.Tests\DocumentSecurityRegressionTests.cs`
- [ ] T059 [P] Add seeded performance fixtures and checks for 500-document list/search, 25 MB upload, and PDF/image preview targets in `ContosoDashboard.Tests\DocumentPerformanceTests.cs`
- [X] T060 Run `dotnet build ContosoDashboard\ContosoDashboard.csproj -nologo` and `dotnet test ContosoDashboard.Tests\ContosoDashboard.Tests.csproj -nologo`, then resolve feature-specific failures
- [ ] T061 Run every manual scenario in `specs\002-document-upload-management\quickstart.md` and record any deviations in `specs\002-document-upload-management\quickstart.md`
- [ ] T062 Review all document service and endpoint operations against the constitution's authorization, secure storage, fail-closed scanning, cleanup, and audit gates in `specs\002-document-upload-management\plan.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies; T001-T005 can begin immediately, with T002-T005 parallel after project paths are known.
- **Foundational (Phase 2)**: Depends on Setup; blocks all user stories. T006-T012 establish the shared model and boundaries before T013-T015 wire and validate them.
- **User Stories (Phases 3-8)**: Depend on Foundational completion. US1 is the MVP; US2 and US3 are P1 and can proceed in parallel after shared foundations, though both consume the upload model/service. US4 and US5 depend on the document service from US1 and can proceed in parallel after US1. US6 can proceed after activity persistence exists and is best finalized after the governed operations are complete.
- **Polish (Phase 9)**: Depends on all desired stories being complete.

### User Story Dependencies

- **US1 (P1)**: Foundational only; MVP starting point.
- **US2 (P1)**: Foundational plus the document entity/service contracts from US1; list/search can be implemented independently once the shared service boundary exists.
- **US3 (P1)**: Foundational plus document access from US1; sharing extends the same authorization boundary.
- **US4 (P2)**: Depends on US1 upload/storage lifecycle and US2 protected document access.
- **US5 (P2)**: Depends on US1 document creation and existing task/dashboard/notification services; does not depend on US4 or US6.
- **US6 (P3)**: Depends on activity writes from US1-US5; reporting can be implemented after the activity schema and governed operations are available.

### Parallel Opportunities

- Setup: T002, T003, T004, and T005 can run in parallel after T001.
- Foundation: T008, T009, T010, T011, and T012 can proceed in parallel after the entity contract is agreed; T015 can run in parallel with service registration once fixtures exist.
- P1: T016-T017, T023-T025, and T031-T032 are parallel test slices; US2 and US3 can be split between developers after US1 contracts are stable.
- P2: T039-T040 and T045-T046 are parallel test slices; US4 and US5 can proceed in parallel after US1.
- Cross-cutting: T057-T059 can proceed in parallel after implementation stabilizes.

## Parallel Example: User Story 1

```text
Task T016: Upload contract tests in ContosoDashboard.Tests\DocumentServiceUploadTests.cs
Task T017: Upload failure and cleanup tests in ContosoDashboard.Tests\DocumentServiceUploadTests.cs
Task T019: Upload request/result models in ContosoDashboard\Models\DocumentUploadModels.cs
```

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 setup.
2. Complete Phase 2 foundational entities, storage, scanning, validation, authorization, registration, and fixtures.
3. Complete Phase 3 User Story 1.
4. Run the focused US1 tests and the repository build.
5. Demonstrate valid and rejected uploads independently before expanding scope.

### Incremental Delivery

1. Add US2 browsing/search/preview/download and validate access filtering.
2. Add US3 project and department sharing with notifications.
3. Add US4 maintenance workflows.
4. Add US5 task/dashboard integration.
5. Add US6 administrator audit reports.
6. Complete Phase 9 performance, security, documentation, and quickstart validation.

### Parallel Team Strategy

1. One developer completes Setup and shared Foundation.
2. After the foundation and US1 service contract stabilize, assign US2 and US3 to separate developers.
3. Assign US4 and US5 in parallel after US1 upload/storage behavior is validated.
4. Assign US6 and cross-cutting validation after activity writes are present.

## Notes

- Every task starts with `- [ ]`, has a sequential ID, and includes a concrete file path.
- `[P]` appears only on tasks that can be performed in parallel without depending on incomplete work in the same file.
- Story tasks use exactly one `[US#]` label and map to the six prioritized stories in `spec.md`.
- Tests are included because the constitution and implementation plan require focused automated coverage for authorization, persistence, storage, and failure cleanup.
