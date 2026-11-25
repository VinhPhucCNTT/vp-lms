# VP-LMS Project Status

## Project overview

VP-LMS is a learning-management application with a React/Vite/TypeScript frontend and a .NET minimal-API backend using EF Core and PostgreSQL. The backend also has file storage, JWT authentication, and an unfinished RabbitMQ/Docker coding-judge worker.

## Current milestone

The PostgreSQL API Integration Testing Foundation is complete and independently verified locally. Following the test README, the project owner ran all 37 database-free tests successfully, provisioned the dedicated PostgreSQL database and role, and passed the full backend suite with `VP_LMS_TEST_DB` configured. The environment variable was removed after testing.

## Completed and verified

### Student Assignment Submission

**Code-verified**

- Backend API builds successfully.
- Assignment list, detail, file upload, submit, and own-submission endpoints use aligned frontend/backend DTOs.
- File downloads return ASP.NET Core `Results.File` responses with the stored content type and original filename.

**Manually verified**

- Student login; assignment list/detail retrieval; text, file, and combined submissions.
- Submission persistence and restored submitted state after reload.
- Downloading a submitted attachment.

Resolved issue: the download endpoint previously returned HTTP 500 by JSON-wrapping an existing `IResult`; it now returns the file result directly.

### Student Course Discovery and Enrollment

**Code-verified**

- Explore returns only published courses and exposes enrollment availability, instructor names, and enrollment counts.
- Enrollment uses the existing student authorization policy and rejects unpublished, closed, duplicate, and self-enrollment attempts.
- Frontend Explore/My Courses contracts match the structured and paginated backend responses; enrollment uses the authenticated API client.
- At this milestone, Explore and My Courses did not enter the incomplete course workspace.
- Backend and Vite production builds succeed. The full frontend TypeScript check still fails only on pre-existing unrelated errors.

**Manually verified against PostgreSQL**

- Published open/closed courses appear; unpublished courses are excluded from every Explore section.
- Instructor names and open, closed, and already-enrolled states render correctly.
- Enrollment updates immediately, persists after reload, and does not duplicate on double-click.
- My Courses shows new enrollments and paginates correctly beyond nine courses.
- Backend rejects duplicate, closed-course, unpublished-course, and instructor enrollment attempts.
- At this milestone, neither Explore nor My Courses navigated into `/student/courses/:courseId`.

Resolved issue: `GetExploreAsync` and `GetStudentCoursesAsync` initially produced EF Core query-translation errors because ordering/paging was applied outside the projected query. The query parentheses were corrected so projection, ordering, and paging translate and run successfully against PostgreSQL.

### Student Course Access and Read-only Module/Activity Browsing

**Automated verification**

- Backend API and Vite production builds passed.
- All 26 backend tests passed. The suite covers AutoMapper configuration, DTO serialization, paging validation, and direct handler validation. These tests do not exercise PostgreSQL-backed authorization or query execution.
- HTTP smoke checks covered anonymous requests (401), malformed IDs (400), and nonnumeric paging (400).
- The full frontend TypeScript check went from 117 to 103 diagnostics, with no new diagnostics.

**Manually verified against PostgreSQL**

- My Courses opens enrolled courses. The workspace displays correct course metadata, instructor names, published modules, and activity types. Activity selection displays metadata only.
- Breadcrumbs, back navigation, reload, switching courses, and module/activity pagination work. Loading and errors do not flash stale content, including under throttled and blocked requests.
- All three read routes return 200 for an authorized enrolled student. Unenrolled students, students whose enrollment was removed, and unrelated instructors receive 404.
- Publication and deletion restrictions protect courses and modules, and draft or deleted activities stay hidden. Accessible empty courses and modules return 200 with empty collections.
- Existing students retain access when enrollment is closed. The course-owning instructor can read published and draft content.
- Denied course requests do not create or update recent-access records.

Authorization changes cover `GET /api/course/{courseId}`, `GET /api/course/{courseId}/modules`, and `GET /api/module/{moduleId}/activities`. Authorization review of other routes, including module mutations and activity progress, remains deferred.

### PostgreSQL API Integration Testing Foundation

- The existing backend test project now runs the three workspace read routes through the real API host with PostgreSQL, JWT login, and independent test fixtures.
- A fresh dedicated PostgreSQL database successfully applied all seven migrations. The 13 integration cases passed twice, and both runs left zero fixture rows. The 37 database-free tests also passed.
- Coverage includes authorization, publication and deletion rules, pagination, response contracts, and recent-access persistence. Local provisioning and test commands are in `Backend/Backend.Api.Tests/README.md`.

## Current known issues

- The full frontend TypeScript check has unrelated errors in course workspace, activity, admin, assessment, shared UI, and legacy mock-data code; the Vite production build succeeds.
- Several frontend API modules/pages still use stale DTOs, missing methods, or backend routes that do not exist.
- The workspace appears to issue duplicate course, module, and activity GET requests during development. The cause is unconfirmed and investigation is deferred.
- The backend test-project build reported vulnerability warnings for AutoMapper 16.1.1 (`NU1903`, `GHSA-6vhm-j9h7-f9gh` and `GHSA-gpwp-5797-mjff`). Dependency remediation is outstanding.
- Development databases may retain the older file-only version of the seeded assignment.

## Deferred features

- Student lesson content, activity participation from the course workspace, and progress tracking.
- Assessment authoring, question-bank management, and complete student/instructor assessment flows.
- Coding problem listings/history and the RabbitMQ/Docker judge worker.
- Instructor assignment authoring/grading UI, admin reporting, activity feed, announcements, and gradebook.

## Next potential milestone

The next student-facing milestone has not been selected.

## Important development context

- The frontend API base URL defaults to the local backend; CORS is configured for the Vite development origin.
- Use the existing API client for authenticated mutations; it handles the application's antiforgery header flow.
- File downloads must return the `IResult` from `FileEndpoints`/`FileService` directly rather than wrapping it in `TypedResults.Ok`.
- Run database-free tests with `Category!=Integration` and PostgreSQL-backed tests with `Category=Integration`. The integration tests require an explicit `VP_LMS_TEST_DB` connection.
