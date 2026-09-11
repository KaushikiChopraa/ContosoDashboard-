# Quickstart Validation: Document Upload and Management

## Prerequisites

- .NET 9 SDK
- Local repository checkout
- No cloud services required
- A clean local SQLite database for repeatable runs

## Build and start

```powershell
cd ContosoDashboard
dotnet restore
dotnet build
dotnet run
```

Open the HTTPS URL printed by the application and sign in using the training login users.

## Core validation scenarios

1. **Upload validation**
   - Sign in as an employee.
   - Upload a supported file at or below 25 MB with a title and allowed category.
   - Confirm progress, success feedback, metadata, and a recent-document entry.
   - Repeat with an unsupported type, an oversized file, and a failed/unavailable security scan; confirm each is rejected and no document record is visible.

2. **Browse and search**
   - Create documents across categories and projects.
   - Confirm My Documents shows title, category, upload date, size, and project.
   - Exercise title/date/category/size sorting, category/project/date filters, and title/description/tag/uploader/project search.
   - Confirm empty results remain usable and inaccessible documents do not appear.

3. **Authorization and sharing**
   - Test an owner, project member, project manager, department recipient, and unrelated user against the same document.
   - Confirm project-document team shares follow current project membership and Team Resources shares follow current department membership.
   - Confirm direct download, preview, edit, replace, share, and delete attempts by unauthorized users are denied without metadata disclosure.

4. **Task and dashboard integration**
   - Attach an existing document to a task and upload from task context.
   - Confirm the task association uses the task project.
   - Confirm the dashboard shows five recent uploads and an accessible document count.
   - Confirm project additions and shares create in-app notifications.

5. **Audit and cleanup**
   - Perform upload, download, preview, replace, edit, share, attach, and delete operations.
   - Confirm administrators can see actor, document, action, and time in reports.
   - Confirm non-administrators cannot access reports and deleted-file links reveal neither content nor metadata.

## Focused automated validation

Run the solution build and the document test project after implementation:

```powershell
dotnet build ContosoDashboard\ContosoDashboard.csproj -nologo
dotnet test <document-test-project> -nologo
```

The focused tests must cover file validation, generated safe paths, save-before-metadata ordering and cleanup, authorization allow/deny cases, project/department sharing, task-project inheritance, audit records, and protected download/preview behavior.

## Performance checks

With a seeded dataset of up to 500 accessible documents, measure list and search completion. Verify the documented targets: list/search within 2 seconds, preview within 3 seconds, and valid 25 MB upload processing within 30 seconds under typical training conditions.
