namespace Backend.Api.Core.Types;

public record StudentDashboardResponse(
    int ActiveCourses,
    int PendingAssign,
    int PendingAssessments,
    int PendingProblems
);

public record InstructorDashboardResponse(
    int TotalStudents,
    int CoursesPublished,
    int CoursesTotal,
    int PendingAssign // Assignments with at least 1 un-graded submission(s).
    // int PendingAssess, // Assessments are automatic only => no manual grading required.
    // int PendingProblems // Also does not have manual grading
);
