using Backend.Data.Repositories;
using Backend.Data.UnitOfWork;
using Backend.Features.Courses.Dtos;
using Backend.Models.Courses;

namespace Backend.Features.Enrollments.Services;

public class EnrollmentService(
    ICourseRepository courseRepo,
    IEnrollmentRepository enrollmentRepo,
    IModuleRepository moduleRepo,
    IUnitOfWork uow) : IEnrollmentService
{
    private readonly ICourseRepository _courseRepo = courseRepo;
    private readonly IEnrollmentRepository _enrollmentRepo = enrollmentRepo;
    private readonly IModuleRepository _moduleRepo = moduleRepo;
    private readonly IUnitOfWork _uow = uow;

    public async Task<bool> EnrollStudentAsync(
        Guid courseId,
        Guid studentId)
    {
        var course = await _courseRepo.GetByIdAsync(courseId);
        if (course is null)
            return false;

        var existingEnrollment =
            await _enrollmentRepo.GetByStudentAndCourseAsync(
                studentId,
                courseId);

        if (existingEnrollment is not null)
            return false;

        var enrollment = new CourseEnrollment
        {
            CourseId = courseId,
            UserId = studentId,
            EnrolledAt = DateTime.UtcNow
        };

        await _enrollmentRepo.AddAsync(enrollment);

        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<List<ViewCourseDto>> GetStudentCoursesAsync(
        Guid studentId)
    {
        var enrollments =
            await _enrollmentRepo.GetByStudentIdAsync(studentId);

        var list = new List<ViewCourseDto>();
        foreach (var e in enrollments)
        {
            var course = e.Course;
            int moduleCount = await _moduleRepo.CountModulesAsync(course);
            int enrollmentCount = await _courseRepo.CountEnrollmentsAsync(course);

            list.Add(new ViewCourseDto(
                course.Id,
                course.Title,
                course.Description,
                course.ThumbnailUrl,
                course.IsPublished,
                course.InstructorId,
                course.Instructor.UserName,
                course.Instructor.FullName,
                moduleCount,
                enrollmentCount,
                course.CreatedAt,
                course.UpdatedAt
            ));
        }

        return list;
    }

    public async Task<bool> UnenrollStudentAsync(
        Guid courseId,
        Guid studentId)
    {
        var enrollment =
            await _enrollmentRepo.GetByStudentAndCourseAsync(
                studentId,
                courseId);

        if (enrollment is null)
            return false;

        _enrollmentRepo.Remove(enrollment);

        await _uow.SaveChangesAsync();
        return true;
    }
}
