using System.Text.Json;
using Valtuutus.Playground.Services;
using Xunit;

namespace Valtuutus.Playground.Tests;

public sealed class GraphDataBuilderTests
{
    // ── helpers ──────────────────────────────────────────────────────────────

    private static List<JsonElement> Parse(string json) =>
        JsonSerializer.Deserialize<List<JsonElement>>(json)!;

    private static JsonElement? FindNode(List<JsonElement> els, string id)
    {
        foreach (var el in els)
        {
            if (el.TryGetProperty("data", out var d) &&
                d.TryGetProperty("id", out var i) &&
                i.GetString() == id)
                return el;
        }
        return null;
    }

    private static List<JsonElement> Edges(List<JsonElement> els, string edgeType) =>
        els.Where(e => e.TryGetProperty("data", out var d) &&
                       d.TryGetProperty("edgeType", out var t) &&
                       t.GetString() == edgeType).ToList();

    private static string BuildFromSchema(string schemaText)
    {
        var svc = new SchemaService();
        var result = svc.TrySetSchema(schemaText);
        Assert.True(result.IsValid, $"Schema invalid: {string.Join(", ", result.Errors)}");
        return GraphDataBuilder.BuildJson(svc.Schema!);
    }

    // ── entity nodes ─────────────────────────────────────────────────────────

    [Fact]
    public void BuildJson_EmitsEntityNode_ForEachEntity()
    {
        var els = Parse(BuildFromSchema("entity user {} entity document {}"));

        Assert.NotNull(FindNode(els, "user"));
        Assert.NotNull(FindNode(els, "document"));
    }

    [Fact]
    public void BuildJson_EntityNode_HasCorrectNodeType()
    {
        var els = Parse(BuildFromSchema("entity user {}"));
        var node = FindNode(els, "user")!.Value;

        Assert.Equal("entity", node.GetProperty("data").GetProperty("nodeType").GetString());
        Assert.Equal("user", node.GetProperty("data").GetProperty("label").GetString());
    }

    // ── permission summary nodes ──────────────────────────────────────────────

    [Fact]
    public void BuildJson_EmitsPermissionSummaryNode_WhenEntityHasPermissions()
    {
        var els = Parse(BuildFromSchema("""
            entity user {}
            entity document {
                relation owner @user;
                permission read  := owner;
                permission write := owner;
            }
            """));

        var permNode = FindNode(els, "document_perms");
        Assert.NotNull(permNode);
        Assert.Equal("permission", permNode!.Value.GetProperty("data").GetProperty("nodeType").GetString());
        var label = permNode.Value.GetProperty("data").GetProperty("label").GetString()!;
        Assert.Contains("read", label);
        Assert.Contains("write", label);
    }

    [Fact]
    public void BuildJson_NoPermissionSummaryNode_WhenEntityHasNoPermissions()
    {
        var els = Parse(BuildFromSchema("entity user {}"));

        Assert.Null(FindNode(els, "user_perms"));
    }

    [Fact]
    public void BuildJson_EmitsPermissionEdge_FromEntityToSummaryNode()
    {
        var els = Parse(BuildFromSchema("""
            entity user {}
            entity document {
                relation owner @user;
                permission read := owner;
            }
            """));

        var permEdges = Edges(els, "permission");
        Assert.Single(permEdges);
        var data = permEdges[0].GetProperty("data");
        Assert.Equal("document", data.GetProperty("source").GetString());
        Assert.Equal("document_perms", data.GetProperty("target").GetString());
    }

    // ── attribute nodes ───────────────────────────────────────────────────────

    [Fact]
    public void BuildJson_EmitsAttributeNodeAndEdge_ForEachAttribute()
    {
        var els = Parse(BuildFromSchema("""
            entity document {
                attribute is_public bool;
            }
            """));

        var attrNode = FindNode(els, "document_attr_is_public");
        Assert.NotNull(attrNode);
        Assert.Equal("attribute", attrNode!.Value.GetProperty("data").GetProperty("nodeType").GetString());
        Assert.Equal("is_public: bool", attrNode.Value.GetProperty("data").GetProperty("label").GetString());

        var attrEdges = Edges(els, "attribute");
        Assert.Single(attrEdges);
        Assert.Equal("document", attrEdges[0].GetProperty("data").GetProperty("source").GetString());
    }

    // ── relation edges (grouping) ─────────────────────────────────────────────

    [Fact]
    public void BuildJson_GroupsParallelRelations_IntoSingleEdge()
    {
        var els = Parse(BuildFromSchema("""
            entity user {}
            entity document {
                relation owner  @user;
                relation viewer @user;
            }
            """));

        var relEdges = Edges(els, "relation");
        // Both relations go document→user, so they must be merged into one edge
        Assert.Single(relEdges);
        var label = relEdges[0].GetProperty("data").GetProperty("label").GetString()!;
        Assert.Contains("owner", label);
        Assert.Contains("viewer", label);
    }

    [Fact]
    public void BuildJson_EmitsSeparateEdges_ForDifferentTargets()
    {
        var els = Parse(BuildFromSchema("""
            entity user {}
            entity org  {}
            entity document {
                relation owner @user;
                relation org   @org;
            }
            """));

        var relEdges = Edges(els, "relation");
        Assert.Equal(2, relEdges.Count);
    }

    // ── GitHub preset ─────────────────────────────────────────────────────────

    [Fact]
    public void BuildJson_GitHubPreset_ProducesValidJson()
    {
        var svc = new SchemaService();
        svc.TrySetSchema(PresetRegistry.GitHub.Schema);
        var els = Parse(GraphDataBuilder.BuildJson(svc.Schema!));

        Assert.NotEmpty(els);
        Assert.NotNull(FindNode(els, "repository"));
        Assert.NotNull(FindNode(els, "user"));
        Assert.NotNull(FindNode(els, "organization"));
    }

    [Fact]
    public void BuildJson_GitHubPreset_GroupsRepositoryToUserRelations()
    {
        var svc = new SchemaService();
        svc.TrySetSchema(PresetRegistry.GitHub.Schema);
        var els = Parse(GraphDataBuilder.BuildJson(svc.Schema!));

        // repository has admin, maintainer, reader all pointing to user — should be one edge
        var repoToUser = Edges(els, "relation")
            .FirstOrDefault(e => e.GetProperty("data").GetProperty("source").GetString() == "repository" &&
                                 e.GetProperty("data").GetProperty("target").GetString() == "user");
        Assert.True(repoToUser.ValueKind != JsonValueKind.Undefined);
        var label = repoToUser.GetProperty("data").GetProperty("label").GetString()!;
        Assert.Contains("admin", label);
        Assert.Contains("maintainer", label);
        Assert.Contains("reader", label);
    }
}
