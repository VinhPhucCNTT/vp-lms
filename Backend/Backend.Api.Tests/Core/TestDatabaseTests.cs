using Backend.Api.Tests.Integration;

namespace Backend.Api.Tests.Core;

public class TestDatabaseTests
{
    private const string Valid = "Host=127.0.0.1;Database=vp_lms_integration_tests;Username=vp_lms_integration_runner";

    [Fact]
    public void AcceptsAnExplicitDedicatedTarget()
    {
        var target = TestDatabase.Parse(Valid + ";Port=55432");
        Assert.Equal(55432, target.Port);
        TestDatabase.VerifyEffectiveTarget(target, "Port=55432;" + Valid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not a connection string")]
    [InlineData("Host=localhost;Database=vp-lms;Username=vp_lms_integration_runner")]
    [InlineData("Host=localhost;Database=vp_lms_integration_tests;Username=postgres")]
    [InlineData("Database=vp_lms_integration_tests;Username=vp_lms_integration_runner")]
    [InlineData("Host=localhost;Username=vp_lms_integration_runner")]
    [InlineData("Host=db.example.test;Database=vp_lms_integration_tests;Username=vp_lms_integration_runner")]
    [InlineData(Valid + ";Options=-c role=postgres")]
    public void RejectsMissingOrUnsafeTargets(string? connection)
    {
        var error = Assert.Throws<InvalidOperationException>(() => TestDatabase.Parse(connection));
        Assert.Contains(TestDatabase.Variable, error.Message);
    }

    [Fact]
    public void RejectsRuntimeConnectionDrift()
    {
        var target = TestDatabase.Parse(Valid);
        Assert.Throws<InvalidOperationException>(() =>
            TestDatabase.VerifyEffectiveTarget(target, Valid + ";Port=55432"));
    }
}
