using Backend.Data.Repositories;
using Backend.Data.UnitOfWork;
using Backend.Features.Activities;
using Backend.Features.Activities.Dtos;
using Backend.Features.Courses.Dtos;
using Backend.Features.Modules.Dtos;
using Backend.Models.Courses;
using Backend.Models.Users;
using Microsoft.AspNetCore.Identity;

namespace Backend.Features.Courses.Services;

public class CourseService(
    ICourseRepository courseRepo,
    IModuleRepository moduleRepo,
    UserManager<ApplicationUser> userManager,
    IUnitOfWork uow) : ICourseService
{
    private readonly ICourseRepository _courseRepo = courseRepo;
    private readonly IModuleRepository _moduleRepo = moduleRepo;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IUnitOfWork _uow = uow;

    public async Task<Guid> CreateAsync(
        CreateCourseDto dto,
        Guid instructorId)
    {
        var course = new Course
        {
            Title = dto.Title,
            Description = dto.Description,
            ThumbnailUrl = dto.ThumpnailUrl,
            InstructorId = instructorId
        };

        await _courseRepo.AddAsync(course);

        await _uow.SaveChangesAsync();

        return course.Id;
    }

    public async Task<ViewCourseDto?> GetByIdAsync(Guid id)
    {
        var course = await _courseRepo.GetByIdAsync(id);
        if (course is null) return null;

        var moduleCount = await _moduleRepo.CountModulesAsync(course);
        var enrollmentCount = await _courseRepo.CountEnrollmentsAsync(course);

        return new ViewCourseDto(
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
        );
    }

    public async Task<List<ViewCourseDto>> GetAllAsync()
    {
        var courses = await _courseRepo.GetAllAsync();

        var list = new List<ViewCourseDto>();
        foreach (var course in courses)
        {
            var moduleCount = await _moduleRepo.CountModulesAsync(course);
            var enrollmentCount = await _courseRepo.CountEnrollmentsAsync(course);

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

    public async Task<List<ViewCourseDto>> GetInstructorCoursesAsync(Guid instructorId)
    {
        var instructor = await _userManager.FindByIdAsync(instructorId.ToString());
        if (instructor is null)
            return [];

        var courses = await _courseRepo.GetByInstructorAsync(instructor);

        var list = new List<ViewCourseDto>();
        foreach (var course in courses)
        {
            var moduleCount = await _moduleRepo.CountModulesAsync(course);
            var enrollmentCount = await _courseRepo.CountEnrollmentsAsync(course);

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

    public async Task<CourseContentDto?> GetFullCourseContentAsync(Guid courseId)
    {
        var course = await _courseRepo.GetFullContentByIdAsync(courseId);
        if (course is null)
            return null;

        return new CourseContentDto(
            course.Id,
            course.Title,
            course.Modules
                .OrderBy(m => m.OrderIndex)
                .Select(m => new ViewModuleDto(
                    m.Id,
                    m.Title,
                    m.OrderIndex,
                    m.Activities
                        .OrderBy(a => a.OrderIndex)
                        .Select(a => new ViewActivityDto(
                            a.Id,
                            a.Title,
                            a.Type,
                            a.OrderIndex,
                            a.IsPublished,
                            a.AvailableFrom,
                            a.AvailableUntil,
                            a.GetResourceId()
                        )).ToList()
                )).ToList()
        );
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateCourseDto dto)
    {
        var course = await _courseRepo.GetByIdAsync(id);
        if (course is null)
            return false;
        // throw new Exception("Course not found");

        course.Title = dto.Title;
        course.Description = dto.Description;

        _courseRepo.Update(course);

        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var course = await _courseRepo.GetByIdAsync(id);
        if (course is null)
            return false;
        // throw new Exception("Course not found");

        _courseRepo.Remove(course);

        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HardDeleteAsync(Guid id)
    {
        var course = await _courseRepo.GetDeletedByIdAsync(id);
        if (course is null)
            return false;
        // throw new Exception("Deleted course not found");

        _courseRepo.HardDelete(course);

        await _uow.SaveHardChangesAsync();
        return true;
    }

    public async Task<bool> RestoreAsync(Guid id)
    {
        var course = await _courseRepo.GetDeletedByIdAsync(id);
        if (course is null)
            return false;
        // throw new Exception("Deleted course not found");

        _courseRepo.Restore(course);

        await _uow.SaveChangesAsync();
        return true;
    }
}
