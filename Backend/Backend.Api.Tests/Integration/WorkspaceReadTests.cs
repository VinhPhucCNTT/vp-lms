using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Backend.Api.Core.Common;
using Backend.Api.Core.Types;
using Backend.Persistence.Entities.Courses;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Tests.Integration;

[Collection("PostgreSQL API")]
[Trait("Category", "Integration")]
public sealed class WorkspaceReadTests(WorkspaceApiFactory api)
{
    [Fact]
    public async Task EnrolledStudentCanReadMetadataEvenWhenEnrollmentIsClosed()
    {
        await using var data = new WorkspaceData(api);
        await data.SeedAsync();
        using var client = await data.LoginAsync(data.Student);

        var course = await client.GetFromJsonAsync<CourseDetailResponse>(data.CourseUrl);
        Assert.NotNull(course);
        Assert.Equal(api.Encode(data.Course.Id), course.Id);
        Assert.Equal(data.Course.Title, course.Title);
        Assert.Equal(data.Owner.FirstName, course.InstructorFirstName);
        Assert.Equal(data.Owner.LastName, course.InstructorLastName);
        Assert.Equal(1, course.EnrollmentCount);
        Assert.False(course.EnrollmentOpen);

        var modules = await ReadPage<ModuleResponse>(client, data.ModulesUrl);
        Assert.Equal(2, modules.MaxCount);
        Assert.Equal(new[] { "Empty", "Main" }, modules.Data.Select(m => m.Title));
        using var response = await client.GetAsync(data.ActivitiesUrl(data.MainModule));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var items = json.RootElement.GetProperty("data");
        Assert.Collection(items.EnumerateArray(),
            item => AssertActivity(item, data.FirstActivity, 0),
            item => AssertActivity(item, data.SecondActivity, 1));
    }

    [Theory]
    [InlineData("anonymous", HttpStatusCode.Unauthorized)]
    [InlineData("unenrolled", HttpStatusCode.NotFound)]
    [InlineData("unrelated instructor", HttpStatusCode.NotFound)]
    [InlineData("removed enrollment", HttpStatusCode.NotFound)]
    public async Task DirectReadsRespectEnrollmentAndRole(string caller, HttpStatusCode expected)
    {
        await using var data = new WorkspaceData(api);
        await data.SeedAsync();
        using var client = caller switch
        {
            "anonymous" => api.CreateClient(),
            "unenrolled" => await data.LoginAsync(data.Unenrolled),
            "unrelated instructor" => await data.LoginAsync(data.OtherInstructor),
            "removed enrollment" => await data.LoginAsync(data.Student),
            _ => throw new ArgumentOutOfRangeException(nameof(caller))
        };

        if (caller == "removed enrollment")
        {
            await using var db = await api.OpenDbAsync();
            var enrollment = await db.Enrollments.SingleAsync(e => e.CourseId == data.Course.Id);
            db.Enrollments.Remove(enrollment);
            await db.SaveChangesAsync();
        }

        await AssertWorkspaceStatus(client, data, expected);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DraftOrDeletedCourseBlocksChildReads(bool deleted)
    {
        await using var data = new WorkspaceData(api);
        await data.SeedAsync();
        using var client = await data.LoginAsync(data.Student);
        await using (var db = await api.OpenDbAsync())
        {
            var course = await db.Courses.SingleAsync(c => c.Id == data.Course.Id);
            if (deleted)
                db.Courses.Remove(course);
            else
                course.IsPublished = false;
            await db.SaveChangesAsync();
        }
        await AssertWorkspaceStatus(client, data, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task HiddenModulesRejectDirectActivityReadsAndVisiblePagesExcludeHiddenRows()
    {
        await using var data = new WorkspaceData(api);
        await data.SeedAsync();
        using var client = await data.LoginAsync(data.Student);
        await AssertStatus(client, data.ActivitiesUrl(data.DraftModule), HttpStatusCode.NotFound);
        await AssertStatus(client, data.ActivitiesUrl(data.DeletedModule), HttpStatusCode.NotFound);

        for (var page = 1; page <= 2; page++)
        {
            var query = $"?pageNumber={page}&pageSize=1";
            var modules = await ReadPage<ModuleResponse>(client, data.ModulesUrl + query);
            AssertPage(modules, page, 2);
            Assert.Equal(api.Encode((page == 1 ? data.EmptyModule : data.MainModule).Id), Assert.Single(modules.Data).Id);
            var activities = await ReadPage<ActivityResponse>(client, data.ActivitiesUrl(data.MainModule) + query);
            AssertPage(activities, page, 2);
            Assert.Equal(api.Encode((page == 1 ? data.FirstActivity : data.SecondActivity).Id), Assert.Single(activities.Data).Id);
        }
    }

    [Fact]
    public async Task ActivityPagingBreaksOrderTiesById()
    {
        await using var data = new WorkspaceData(api);
        await data.SeedAsync();
        await using (var db = await api.OpenDbAsync())
        {
            var second = await db.CourseActivities.SingleAsync(a => a.Id == data.SecondActivity.Id);
            second.OrderIndex = data.FirstActivity.OrderIndex;
            await db.SaveChangesAsync();
        }
        using var client = await data.LoginAsync(data.Student);
        var expected = new[] { data.FirstActivity.Id, data.SecondActivity.Id }.Order().Select(api.Encode).ToArray();
        for (var page = 1; page <= expected.Length; page++)
        {
            var result = await ReadPage<ActivityResponse>(client,
                data.ActivitiesUrl(data.MainModule) + $"?pageNumber={page}&pageSize=1");
            Assert.Equal(expected[page - 1], Assert.Single(result.Data).Id);
        }
    }

    [Fact]
    public async Task AccessibleEmptyCollectionsReturnOk()
    {
        await using var data = new WorkspaceData(api);
        await data.SeedAsync();
        using var client = await data.LoginAsync(data.Student);
        var activities = await ReadPage<ActivityResponse>(client, data.ActivitiesUrl(data.EmptyModule));
        Assert.Empty(activities.Data);
        Assert.Equal(0, activities.MaxCount);
        await using (var db = await api.OpenDbAsync())
        {
            await db.CourseModules.Where(m => m.CourseId == data.Course.Id)
                .ExecuteUpdateAsync(set => set.SetProperty(m => m.IsPublished, false));
        }
        var modules = await ReadPage<ModuleResponse>(client, data.ModulesUrl);
        Assert.Empty(modules.Data);
        Assert.Equal(0, modules.MaxCount);
    }

    [Fact]
    public async Task OwnerRetainsAccessToPublishedAndDraftContent()
    {
        await using var data = new WorkspaceData(api);
        await data.SeedAsync();
        using var client = await data.LoginAsync(data.Owner);
        await AssertWorkspaceStatus(client, data, HttpStatusCode.OK);

        await using (var db = await api.OpenDbAsync())
        {
            await db.Courses.Where(c => c.Id == data.Course.Id)
                .ExecuteUpdateAsync(set => set.SetProperty(c => c.IsPublished, false));
        }
        await AssertWorkspaceStatus(client, data, HttpStatusCode.OK);
        var modules = await ReadPage<ModuleResponse>(client, data.ModulesUrl);
        Assert.Equal(new[] { "Draft", "Empty", "Main" }, modules.Data.Select(m => m.Title));
        var main = await ReadPage<ActivityResponse>(client, data.ActivitiesUrl(data.MainModule));
        Assert.Equal(new[] { "Draft", "Lesson", "Assignment" }, main.Data.Select(a => a.Title));
        var draft = await ReadPage<ActivityResponse>(client, data.ActivitiesUrl(data.DraftModule));
        Assert.Single(draft.Data);
    }

    [Fact]
    public async Task MalformedIdentifiersAndInvalidPagingReturnBadRequest()
    {
        await using var data = new WorkspaceData(api);
        await data.SeedAsync();
        using var client = await data.LoginAsync(data.Student);
        foreach (var url in new[] { "/api/course/!", "/api/course/!/modules", "/api/module/!/activities" })
            await AssertStatus(client, url, HttpStatusCode.BadRequest);

        foreach (var url in new[] { data.ModulesUrl, data.ActivitiesUrl(data.MainModule) })
        {
            foreach (var query in new[] { "?pageNumber=0", "?pageSize=101", "?pageSize=no" })
                await AssertStatus(client, url + query, HttpStatusCode.BadRequest);
        }
    }

    [Fact]
    public async Task RecentAccessIsCreatedAndUpdatedOnlyForAuthorizedReads()
    {
        await using var data = new WorkspaceData(api);
        await data.SeedAsync();
        using var denied = await data.LoginAsync(data.Unenrolled);
        using var student = await data.LoginAsync(data.Student);
        await AssertStatus(denied, data.CourseUrl, HttpStatusCode.NotFound);
        Assert.Null(await ReadRecentAsync(data.Course.Id, data.Unenrolled.Id));

        await AssertStatus(student, data.CourseUrl, HttpStatusCode.OK);
        Assert.NotNull(await ReadRecentAsync(data.Course.Id, data.Student.Id));
        var oldTime = DateTime.UnixEpoch;
        await using (var db = await api.OpenDbAsync())
        {
            await db.RecentCourses.Where(r => r.CourseId == data.Course.Id && r.UserId == data.Student.Id)
                .ExecuteUpdateAsync(set => set.SetProperty(r => r.LastAccessedAt, oldTime));
        }
        await AssertStatus(student, data.CourseUrl, HttpStatusCode.OK);
        var updated = await ReadRecentAsync(data.Course.Id, data.Student.Id);
        Assert.True(updated > oldTime);
        await using (var db = await api.OpenDbAsync())
        {
            var enrollment = await db.Enrollments.SingleAsync(e => e.CourseId == data.Course.Id);
            db.Enrollments.Remove(enrollment);
            await db.SaveChangesAsync();
        }
        await AssertStatus(student, data.CourseUrl, HttpStatusCode.NotFound);
        Assert.Equal(updated, await ReadRecentAsync(data.Course.Id, data.Student.Id));
    }

    private async Task<DateTime?> ReadRecentAsync(long courseId, long userId)
    {
        await using var db = await api.OpenDbAsync();
        return await db.RecentCourses.Where(r => r.CourseId == courseId && r.UserId == userId)
            .Select(r => (DateTime?)r.LastAccessedAt).SingleOrDefaultAsync();
    }

    private void AssertActivity(JsonElement item, CourseActivity activity, int type)
    {
        Assert.Equal(api.Encode(activity.Id), item.GetProperty("id").GetString());
        Assert.Equal(activity.Title, item.GetProperty("title").GetString());
        Assert.Equal(activity.OrderIndex, item.GetProperty("orderIndex").GetInt32());
        Assert.Equal(type, item.GetProperty("type").GetInt32());
        Assert.Equal(4, item.EnumerateObject().Count());
    }

    private static void AssertPage<T>(PaginatedResponse<T> page, int number, int count)
    {
        Assert.Equal(number, page.PageNumber);
        Assert.Equal(1, page.PageSize);
        Assert.Equal(count, page.MaxCount);
    }

    private static async Task<PaginatedResponse<T>> ReadPage<T>(HttpClient client, string url)
    {
        using var response = await client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<PaginatedResponse<T>>();
        return Assert.IsType<PaginatedResponse<T>>(page);
    }

    private static async Task AssertWorkspaceStatus(HttpClient client, WorkspaceData data, HttpStatusCode expected)
    {
        await AssertStatus(client, data.CourseUrl, expected);
        await AssertStatus(client, data.ModulesUrl, expected);
        await AssertStatus(client, data.ActivitiesUrl(data.MainModule), expected);
    }

    private static async Task AssertStatus(HttpClient client, string url, HttpStatusCode expected)
    {
        using var response = await client.GetAsync(url);
        Assert.Equal(expected, response.StatusCode);
    }
}
