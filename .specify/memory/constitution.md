<!--
Sync Impact Report
- Version change: scaffold -> 1.0.0
- Modified principles: none; all five principles are established for the initial constitution
- Added sections: Security and Technical Constraints; Development Workflow and Quality Gates
- Removed sections: none
- Follow-up TODOs: RATIFICATION_DATE is unknown and must be replaced with the original adoption date
-->

# ContosoDashboard Constitution

## Core Principles

### I. Training Scope and Simplicity
The application MUST remain suitable for offline training and MUST avoid production-only
dependencies unless they are required to demonstrate the feature contract. Features MUST fit
the existing Blazor Server, service-layer, and Entity Framework architecture. New abstractions
MUST remove a real dependency or enable a stated migration path; speculative complexity MUST
be rejected. This keeps the training project understandable and preserves its stated non-production
scope.

### II. Layered Design and Replaceable Infrastructure
User interface code MUST delegate business rules to services, services MUST use data and
infrastructure abstractions, and persistence concerns MUST remain in the data layer. External
or replaceable infrastructure, including file storage, MUST be accessed through interfaces when
the feature requires a migration path. Implementations MUST be selectable through dependency
injection without changing business workflows. This separation keeps domain behavior testable
and supports the project's offline implementation with future cloud migration.

### III. Authorization at Every Data Boundary
Every protected page, service operation, query, and document endpoint MUST enforce the current
user's identity, role, and resource relationship before returning data or performing a mutation.
Authorization MUST be evaluated for direct links and service calls, not only for navigation or
UI visibility. Resource access MUST follow current project membership and explicit sharing rules.
Tests for protected behavior MUST include both an authorized case and a denied case. This is
the primary defense against IDOR and accidental disclosure in a role-based dashboard.

### IV. Secure and Observable File Handling
Uploaded files MUST be validated for size, supported type, and security status before they are
made available. Files MUST be stored outside web-accessible directories using generated safe
names, and user-supplied names MUST NOT become storage paths. The upload sequence MUST generate
a unique path, save the file, and then persist metadata; failures MUST NOT leave usable-looking
records or orphaned references. Uploads, downloads, shares, and deletions MUST record the actor,
document, action, and time. These rules protect training data and make document behavior
auditable.

### V. Evidence-Driven Quality
Every feature change MUST define observable acceptance behavior before implementation. Tests MUST
cover changed business rules, authorization outcomes, persistence behavior, and important failure
paths. A change MUST pass the repository build and its focused tests before review. User-facing
performance requirements MUST be represented by measurable checks when the feature specifies a
threshold. This makes SDD artifacts and implementation behavior agree rather than relying on
manual inspection alone.

## Security and Technical Constraints

The project MUST target the repository's configured .NET and ASP.NET Core versions and MUST
preserve nullable reference safety. The training environment MUST work offline with local data
and storage. Protected files MUST remain outside `wwwroot`, use GUID-based relative paths, and
be served only through authorized endpoints. Authentication remains mock authentication for
training; production identity, password handling, MFA, transport security, and compliance
requirements MUST NOT be implied by this project. Sensitive configuration MUST remain outside
source control, and changes MUST preserve existing security headers and role distinctions.

## Development Workflow and Quality Gates

Work MUST begin with a feature specification that states user scenarios, acceptance criteria,
constraints, and measurable outcomes. Plans and task lists MUST identify affected layers and
security boundaries before implementation. Contributors MUST keep changes focused, update related
documentation when behavior or constraints change, and preserve existing user-facing workflows
unless the specification explicitly changes them. Before review, the contributor MUST run a
focused validation for the changed behavior and a repository build; reviewers MUST inspect
authorization, failure cleanup, and regression coverage for changes that handle user data or
files.

## Governance

This constitution governs project decisions and takes precedence over informal practices. An
amendment MUST explain the reason, affected principles or sections, compatibility impact, and
required migration or documentation work. Amendments MUST be reviewed with the related feature
artifacts and MUST update the version and last-amended date in the same change. Versioning uses
semantic increments: MAJOR for incompatible governance changes or removals, MINOR for new or
materially expanded principles or sections, and PATCH for clarifications and non-semantic edits.

Every feature review MUST verify compliance with the constitution, especially authorization,
secure file handling, and focused test coverage. Any justified exception MUST be documented in
the feature plan or review record with an owner and expiration or follow-up condition. The
constitution MUST be revisited when the technology stack, training scope, authentication model,
or storage strategy changes materially.

**Version**: 1.0.0 | **Ratified**: TODO(RATIFICATION_DATE): original adoption date is unknown | **Last Amended**: 2026-09-12
