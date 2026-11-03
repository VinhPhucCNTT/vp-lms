namespace Backend.Api.Core.Types;

public record StudentStatsResponse(
    int ActiveCourses,
    int PendingAssign,
    int PendingAssessments,
    int PendingProblems
);

public record InstructorStatsResponse(
    int TotalStudents,
    int CoursesPublished,
    int CoursesTotal,
    int PendingAssignments, // Assignments with at least 1 un-graded submission(s).
    int PendingSubmissions,
    int GradedThisWeek
    // int PendingAssess, // Assessments are automatic only => no manual grading required.
    // int PendingProblems // Also does not have manual grading
);
