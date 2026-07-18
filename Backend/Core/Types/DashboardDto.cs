namespace Backend.Core.Types;

public record StudentDashboardStats(
    int ActiveCourses,
    int PendingAssign,
    int PendingAssess,
    int PendingProblems
);

public record StudentDashboardResponse(
    StudentDashboardStats Stats,
    List<CourseStudentResponse> RecentCourses,
    List<ICourseActivityResponse> ActivitiesDue,
    List<CourseProgressResponse> Progresses,
    List<CourseEventResponse> RecentActivities
);

public record InstructorDashboardStats(
    int TotalStudents,
    int CoursesPublished,
    int CoursesTotal,
    int PendingAssign,
    int PendingAssess,
    int PendingProblems
);

public record InstructorDashboardResponse(
    InstructorDashboardStats Stats,
    List<SubmissionResponse> PendingSubmissions,
    List<CourseStudentResponse> RecentCourses,
    List<AnnouncementResponse> RecentAnnouncements
);
