# Document Service Contract

This contract describes the user-visible document operations. All operations require an authenticated user ID and enforce resource authorization in the service layer.

## Operations

| Operation | Inputs | Success | Denial/failure |
|---|---|---|---|
| Upload | one or more file streams plus title, category, optional description/project/tags | Per-file result with document identity and captured metadata | Per-file validation, scan, storage, or authorization error; no available record |
| List/search | user ID plus search, sort, category, project, and date filters | Only documents currently accessible to the user | Empty result for no matches or denied scope |
| Preview | user ID and document ID | Authorized stream with safe content type for PDF/image | No document disclosure when denied; clear unsupported/damaged preview error |
| Download | user ID and document ID | Authorized original file stream and display name | No document disclosure when denied or deleted |
| Edit metadata | user ID, document ID, metadata | Updated searchable metadata and audit record | Denied or validation error; unchanged metadata |
| Replace file | user ID, document ID, replacement stream and metadata | New validated file available after scan | Denied or validation/scan/storage error; prior file remains available |
| Share | owner ID, document ID, user or team target | Share record, notification, and recipient visibility | Denied, invalid target, or duplicate share |
| Delete | user ID, document ID, confirmed | File and document removed, delete audit retained | Denied or storage failure; metadata remains until cleanup succeeds |
| Attach to task | user ID, document ID, task ID | Task attachment and audit record | Denied, wrong project context, or duplicate attachment |
| Activity report | administrator ID plus report filters | Aggregated upload/download/delete/share activity | Denied for non-administrator |

## Download and preview boundary

Stored files are never exposed through static-file middleware. A protected application endpoint or equivalent authorized stream operation must perform the access check before opening the storage stream and must set the response content type from validated metadata.
