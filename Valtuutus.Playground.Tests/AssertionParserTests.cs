using Xunit;
using Valtuutus.Playground.Models;
using Valtuutus.Playground.Services;

namespace Valtuutus.Playground.Tests;

public class AssertionParserTests
{
    [Fact]
    public void TryParse_ValidAllow_ReturnsTrue()
    {
        var ok = AssertionParser.TryParse("user:alice push repository:linux allow", out var a);
        Assert.True(ok);
        Assert.NotNull(a);
        Assert.Equal("user", a.SubjectType);
        Assert.Equal("alice", a.SubjectId);
        Assert.Equal("push", a.Permission);
        Assert.Equal("repository", a.EntityType);
        Assert.Equal("linux", a.EntityId);
        Assert.True(a.Expected);
    }

    [Fact]
    public void TryParse_ValidDeny_ReturnsFalse()
    {
        var ok = AssertionParser.TryParse("user:bob push repository:linux deny", out var a);
        Assert.True(ok);
        Assert.NotNull(a);
        Assert.False(a!.Expected);
    }

    [Fact]
    public void TryParse_BlankLine_ReturnsFalse()
    {
        Assert.False(AssertionParser.TryParse("   ", out _));
    }

    [Fact]
    public void TryParse_TooFewParts_ReturnsFalse()
    {
        Assert.False(AssertionParser.TryParse("user:alice push repository:linux", out _));
    }

    [Fact]
    public void TryParse_MissingSubjectColon_ReturnsFalse()
    {
        Assert.False(AssertionParser.TryParse("useralice push repository:linux allow", out _));
    }

    [Fact]
    public void TryParse_MissingEntityColon_ReturnsFalse()
    {
        Assert.False(AssertionParser.TryParse("user:alice push repositorylinux allow", out _));
    }

    [Fact]
    public void TryParse_InvalidExpected_ReturnsFalse()
    {
        Assert.False(AssertionParser.TryParse("user:alice push repository:linux yes", out _));
    }

    [Fact]
    public void ParseLines_SkipsBlanksAndInvalid()
    {
        var lines = new[]
        {
            "user:alice push repository:linux allow",
            "",
            "invalid line",
            "user:bob push repository:linux deny",
        };
        var results = AssertionParser.ParseLines(lines);
        Assert.Equal(2, results.Count);
        Assert.True(results[0].Expected);
        Assert.False(results[1].Expected);
    }

    [Fact]
    public void ToShorthand_RoundTrips()
    {
        var a = new Assertion("user", "alice", "push", "repository", "linux", true);
        var s = AssertionParser.ToShorthand(a);
        Assert.Equal("user:alice push repository:linux allow", s);

        var ok = AssertionParser.TryParse(s, out var parsed);
        Assert.True(ok);
        Assert.Equal(a, parsed);
    }

    [Fact]
    public void ToShorthand_DenyRoundTrips()
    {
        var a = new Assertion("user", "bob", "push", "repository", "linux", false);
        var s = AssertionParser.ToShorthand(a);
        Assert.Equal("user:bob push repository:linux deny", s);
    }
}
