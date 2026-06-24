Plan: Integrate Online Judge as Course Activity Type

1. Data Model Updates

    Add a new activity type CodingProblem to the types system
    Create an Activity union type that encompasses: Assignments, Assessments, and Coding Problems
    Add a CourseActivity relationship table linking activities to courses/modules
    Update the Module type to include an activities array with activity type discrimination
    Create new placeholder data linking coding problems to specific courses

2. Course Workspace Restructure

    Add a "Coding Problems" tab to the CourseWorkspace tabs (alongside Content, Assignments, Assessments, Announcements, Progress)
    Create a new CourseCodingProblems component showing problems for the selected module
    Integrate the Monaco code editor directly into the course workspace context
    Pass course/module context to the coding problem view

3. Student Sidebar and Navigation

    Remove the "Online Judge" navigation item from StudentLayout
    Students will access coding problems through their enrolled courses instead

4. Route Changes

    Remove standalone /student/judge and /student/judge/:problemId routes from App.tsx
    Add route for /student/courses/:courseId/problems/:problemId for in-course problem solving
    Keep the problem workspace but make it course-scoped

5. Instructor Course Builder Integration

    Add "Coding Problems" section to instructor course management
    Allow instructors to add/edit coding problems within their courses
    Create an InstructorCodingProblems management page

6. File Relocations and Cleanup

    Move problem-related pages from features/online-judge/pages/ to features/courses/pages/
    Remove the features/online-judge/ directory entirely
    Update imports throughout the codebase

7. Grading Integration

    Connect coding problem submissions to the instructor gradebook
    Include coding problem scores in student progress calculations
    Add problem submission notifications to instructor dashboard

8. Student Dashboard Updates

    Remove the "Judge Submissions" stat card from StudentDashboard
    Include coding problems in the general activity/pending items view
    Consolidate with assignments/assessments in the "Due Soon" section

Summary

This restructuring transforms the Online Judge from a separate standalone feature into an integrated activity type that instructors can add to their courses. Students will encounter coding problems as part of their course content, alongside videos, readings, assignments, and assessments. The Monaco editor and judge functionality remain intact but are now scoped to course context, enabling better organization and instructor control over which problems are assigned to which courses.
