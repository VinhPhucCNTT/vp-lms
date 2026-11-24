using System.Text.Json;
using System.Reflection;
using Backend.Api.Core.Common;
using Backend.Api.Core.Types;
using Backend.Api.Endpoints.Course;
using Backend.Persistence.Entities.Courses;
using Microsoft.AspNetCore.Http;
using Sqids;

namespace Backend.Api.Tests.Core;

public class CourseReadContractTests
{
    [Theory]
    [InlineData("0", "10")]
    [InlineData("-1", "10")]
    [InlineData("1", "0")]
    [InlineData("1", "101")]
    [InlineData("abc", "10")]
    [InlineData("1", "abc")]
    [InlineData("2147483647", "2")]
    public async Task BothListHandlersRejectInvalidPagingBeforeCallingServices(string number, string size)
    {
        var encoder = new SqidsEncoder<long>();
        foreach (var (type, handler) in new[] {
            (typeof(CourseEndpoints), "HandleGetModules"),
            (typeof(ModuleEndpoints), "HandleGetActivities") })
        {
            var method = type.GetMethod(handler, BindingFlags.NonPublic | BindingFlags.Static)!;
            // A null service deliberately fails if invalid paging reaches a database read.
            var task = (Task)method.Invoke(null,
                [encoder.Encode(42), number, size, encoder, null, CancellationToken.None])!;
            await task;
            var result = (INestedHttpResult)task.GetType().GetProperty("Result")!.GetValue(task)!;
            Assert.Equal(400, ((IStatusCodeHttpResult)result.Result).StatusCode);
        }
    }

    [Theory]
    [InlineData(null, null, true)]
    [InlineData("2", "1", true)]
    [InlineData("abc", "10", false)]
    [InlineData("1", "", false)]
    [InlineData("999999999999", "10", false)]
    public void ReadPagingParsesQueryValuesExplicitly(string? number, string? size, bool expected)
        => Assert.Equal(expected, PageRequest.TryCreate(number, size, out _));

    [Theory]
    [InlineData(1, 10, true)]
    [InlineData(2, 100, true)]
    [InlineData(0, 10, false)]
    [InlineData(-1, 10, false)]
    [InlineData(1, 0, false)]
    [InlineData(1, -1, false)]
    [InlineData(1, 101, false)]
    [InlineData(int.MaxValue, 2, false)]
    public void ReadPagingRejectsInvalidOrOverflowingOffsets(int page, int size, bool expected)
        => Assert.Equal(expected, new PageRequest(page, size).IsValid());

    [Theory]
    [InlineData(ActivityType.Lesson, 0)]
    [InlineData(ActivityType.Assignment, 1)]
    [InlineData(ActivityType.Assessment, 2)]
    [InlineData(ActivityType.Problem, 3)]
    public void ActivityListUsesNumericTypesAndMetadataOnly(ActivityType type, int expected)
    {
        var page = new PaginatedResponse<ActivityResponse>(1, 10, 1,
            [new ActivityResponse("opaque-id", type, "Activity", 2)]);
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(page, JsonSerializerOptions.Web));
        Assert.Equal(1, json.RootElement.GetProperty("maxCount").GetInt32());
        var item = json.RootElement.GetProperty("data")[0];
        Assert.Equal(expected, item.GetProperty("type").GetInt32());
        Assert.Equal("opaque-id", item.GetProperty("id").GetString());
        Assert.Equal(4, item.EnumerateObject().Count());
    }

    [Fact]
    public void CourseAndModuleMetadataKeepNullableFieldsAndInstructorNames()
    {
        var course = new CourseDetailResponse("course", "instructor", "Jane", "Doe",
            null, "Title", null, 3, false);
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(course, JsonSerializerOptions.Web));
        Assert.Equal("Jane", json.RootElement.GetProperty("instructorFirstName").GetString());
        Assert.Equal("Doe", json.RootElement.GetProperty("instructorLastName").GetString());
        Assert.Equal(3, json.RootElement.GetProperty("enrollmentCount").GetInt32());
        Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("description").ValueKind);
        Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("backgroundFileId").ValueKind);

        using var module = JsonDocument.Parse(JsonSerializer.Serialize(
            new ModuleResponse("module", "Title", null, 0), JsonSerializerOptions.Web));
        Assert.Equal(JsonValueKind.Null, module.RootElement.GetProperty("description").ValueKind);
    }
}
