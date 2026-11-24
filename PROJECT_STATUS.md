# VP-LMS Project Status

## Project overview

VP-LMS is a learning-management application with a React/Vite/TypeScript frontend and a .NET minimal-API backend using EF Core and PostgreSQL. The backend also has file storage, JWT authentication, and an unfinished RabbitMQ/Docker coding-judge worker.

## Current milestone

Student Course Discovery and Enrollment is complete and manually verified: a student can browse published courses, enroll in an open course, and see it in the paginated My Courses list.

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
- Explore and My Courses do not enter the incomplete course workspace.
- Backend and Vite production builds succeed. The full frontend TypeScript check still fails only on pre-existing unrelated errors.

**Manually verified against PostgreSQL**

- Published open/closed courses appear; unpublished courses are excluded from every Explore section.
- Instructor names and open, closed, and already-enrolled states render correctly.
- Enrollment updates immediately, persists after reload, and does not duplicate on double-click.
- My Courses shows new enrollments and paginates correctly beyond nine courses.
- Backend rejects duplicate, closed-course, unpublished-course, and instructor enrollment attempts.
- Explore and My Courses never navigate into `/student/courses/:courseId`.

Resolved issue: `GetExploreAsync` and `GetStudentCoursesAsync` initially produced EF Core query-translation errors because ordering/paging was applied outside the projected query. The query parentheses were corrected so projection, ordering, and paging translate and run successfully against PostgreSQL.

## Current known issues

- The full frontend TypeScript check has unrelated errors in course workspace, activity, admin, assessment, shared UI, and legacy mock-data code; the Vite production build succeeds.
- Several frontend API modules/pages still use stale DTOs, missing methods, or backend routes that do not exist.
- Course workspace integration is incomplete and depends on APIs/types not provided by the current course API module.
- Development databases may retain the older file-only version of the seeded assignment.
- `EnsureSampleCoursesAsync` in the development seeder hardcodes department IDs `1`, `2`, and `3`; this should eventually use resolved department records instead.

## Deferred features

- Student course workspace, module/lesson navigation, and progress tracking.
- Assessment authoring, question-bank management, and complete student/instructor assessment flows.
- Coding problem listings/history and the RabbitMQ/Docker judge worker.
- Instructor assignment authoring/grading UI, admin reporting, activity feed, announcements, and gradebook.

## Next potential milestone

**Recommendation only — not started:** assess a minimal read-only student course workspace slice covering enrolled-course detail and published module/activity navigation. Its stale API/type dependencies must be inspected before implementation; lessons and progress should remain separate unless the existing code supports them cleanly.

## Important development context

- The frontend API base URL defaults to the local backend; CORS is configured for the Vite development origin.
- Use the existing API client for authenticated mutations; it handles the application's antiforgery header flow.
- File downloads must return the `IResult` from `FileEndpoints`/`FileService` directly rather than wrapping it in `TypedResults.Ok`.
- `Backend.Api.Tests` exists but has minimal coverage; continue using focused builds plus manual end-to-end verification for vertical slices.
