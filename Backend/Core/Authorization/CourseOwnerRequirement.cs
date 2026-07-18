using Microsoft.AspNetCore.Authorization;

namespace Backend.Core.Authorization;

public sealed class CourseOwnerRequirement
    : IAuthorizationRequirement
{
}
