using Xunit;
using Valtuutus.Core.Engines.Check;
using Valtuutus.Core.Engines.LookupEntity;
using Valtuutus.Core.Engines.LookupSubject;
using Valtuutus.Playground.Services;

namespace Valtuutus.Playground.Tests;

public sealed class SchemaServiceTests : IAsyncDisposable
{
    private readonly SchemaService _svc = new();

    private async Task SeedGitHub()
    {
        var preset = PresetRegistry.GitHub;
        _svc.TrySetSchema(preset.Schema);
        var tuples = TupleParser.ParseLines(preset.SeedTuples.Split('\n'));
        var attrs = preset.SeedAttributes.Length > 0
            ? AttributeParser.ParseLines(preset.SeedAttributes.Split('\n'))
            : [];
        await _svc.ResetDataAsync(tuples, attrs);
    }

    // -------------------------------------------------------------------------
    // CheckAsync — GitHub preset
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("user", "torvalds", "push",       "repository",  "linux", true)]
    [InlineData("user", "alice",    "push",       "repository",  "linux", true)]
    [InlineData("user", "bob",      "push",       "repository",  "linux", false)]
    [InlineData("user", "bob",      "pull",       "repository",  "linux", true)]
    [InlineData("user", "torvalds", "manage",     "repository",  "linux", true)]
    [InlineData("user", "bob",      "manage",     "repository",  "linux", false)]
    [InlineData("user", "torvalds", "manage_org", "organization","linux", true)]
    [InlineData("user", "alice",    "manage_org", "organization","linux", false)]
    public async Task CheckAsync_GitHubPreset_ReturnsExpectedResult(
        string subjectType, string subjectId, string permission,
        string entityType,  string entityId,  bool expected)
    {
        await SeedGitHub();

        var result = await _svc.CheckAsync(new CheckRequest(
            entityType:  entityType,
            entityId:    entityId,
            permission:  permission,
            subjectType: subjectType,
            subjectId:   subjectId));

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task CheckAsync_AllGitHubPresetAssertions_AllResolve()
    {
        await SeedGitHub();
        var assertions = PresetRegistry.GitHub.SeedAssertions!;

        foreach (var a in assertions)
        {
            var result = await _svc.CheckAsync(new CheckRequest(
                entityType:  a.EntityType,
                entityId:    a.EntityId,
                permission:  a.Permission,
                subjectType: a.SubjectType,
                subjectId:   a.SubjectId));

            Assert.Equal(a.Expected, result);
        }
    }

    // -------------------------------------------------------------------------
    // LookupEntityAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task LookupEntityAsync_TorvaldsCanPush_ReturnsLinux()
    {
        await SeedGitHub();

        var page = await _svc.LookupEntityAsync(new LookupEntityRequest(
            entityType:  "repository",
            permission:  "push",
            subjectType: "user",
            subjectId:   "torvalds"));

        Assert.Contains("linux", page.EntityIds);
    }

    [Fact]
    public async Task LookupEntityAsync_BobCannotPush_ReturnsEmpty()
    {
        await SeedGitHub();

        var page = await _svc.LookupEntityAsync(new LookupEntityRequest(
            entityType:  "repository",
            permission:  "push",
            subjectType: "user",
            subjectId:   "bob"));

        Assert.Empty(page.EntityIds);
    }

    // -------------------------------------------------------------------------
    // LookupSubjectAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task LookupSubjectAsync_WhoCanPushLinux_ReturnsTorvaldsAndAlice()
    {
        await SeedGitHub();

        var subjects = await _svc.LookupSubjectAsync(new LookupSubjectRequest(
            entityType:  "repository",
            permission:  "push",
            subjectType: "user",
            entityId:    "linux"));

        Assert.Contains("torvalds", subjects);
        Assert.Contains("alice",    subjects);
        Assert.DoesNotContain("bob", subjects);
    }

    // -------------------------------------------------------------------------
    // IsValid / Schema
    // -------------------------------------------------------------------------

    [Fact]
    public void TrySetSchema_ValidSchema_SetsIsValid()
    {
        var result = _svc.TrySetSchema("entity user {}");
        Assert.True(result.IsValid);
        Assert.True(_svc.IsValid);
    }

    [Fact]
    public void TrySetSchema_InvalidSchema_IsValidRemainsCorrect()
    {
        var result = _svc.TrySetSchema("not a valid schema !!!!");
        Assert.False(result.IsValid);
        Assert.False(_svc.IsValid);
    }

    public async ValueTask DisposeAsync()
    {
        _svc.Dispose();
        await ValueTask.CompletedTask;
    }
}
