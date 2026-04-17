using Xunit;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Valtuutus.Playground.Components;
using Valtuutus.Playground.Models;
using Valtuutus.Playground.Services;

namespace Valtuutus.Playground.Tests;

public sealed class QueryPanelTests : BunitContext, IAsyncLifetime
{
    private readonly SchemaService _schemaService = new();

    public async Task InitializeAsync()
    {
        var preset = PresetRegistry.GitHub;
        _schemaService.TrySetSchema(preset.Schema);
        var tuples = TupleParser.ParseLines(preset.SeedTuples.Split('\n'));
        var attrs = preset.SeedAttributes.Length > 0
            ? AttributeParser.ParseLines(preset.SeedAttributes.Split('\n'))
            : [];
        await _schemaService.ResetDataAsync(tuples, attrs);

        Services.AddSingleton(_schemaService);
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    public new Task DisposeAsync()
    {
        _schemaService.Dispose();
        return Task.CompletedTask;
    }

    private List<Assertion> GitHubAssertions() =>
        PresetRegistry.GitHub.SeedAssertions!.ToList();

    // -------------------------------------------------------------------------
    // Rendering
    // -------------------------------------------------------------------------

    [Fact]
    public void Render_WithAssertions_ShowsAllRowsAsNotRun()
    {
        var assertions = GitHubAssertions();
        var cut = Render<QueryPanel>(p => p
            .Add(q => q.Assertions, assertions));

        var pendingIcons = cut.FindAll(".assertion-card-icon.pending");
        Assert.Equal(assertions.Count, pendingIcons.Count);
    }

    [Fact]
    public void Render_WithAssertions_ShowsCountBadge()
    {
        var assertions = GitHubAssertions();
        var cut = Render<QueryPanel>(p => p
            .Add(q => q.Assertions, assertions));

        // Before running, shows "N assertions — not run" text
        var notRunDiv = cut.FindAll("div").First(d => d.TextContent.Contains("assertions") && d.TextContent.Contains("not run"));
        Assert.Contains($"{assertions.Count} assertion", notRunDiv.TextContent);
    }

    // -------------------------------------------------------------------------
    // Run All
    // -------------------------------------------------------------------------

    [Fact]
    public async Task RunAll_AllAssertionsGetResult_NoneRemainsNotRun()
    {
        var assertions = GitHubAssertions();
        var cut = Render<QueryPanel>(p => p
            .Add(q => q.Assertions, assertions));

        var runAllBtn = cut.FindAll("button")
            .First(b => b.TextContent.Contains("Run All"));
        await cut.InvokeAsync(() => runAllBtn.Click());

        cut.WaitForAssertion(() =>
        {
            var pendingIcons = cut.FindAll(".assertion-card-icon.pending");
            Assert.Empty(pendingIcons);
        }, timeout: TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task RunAll_GitHubPresetAssertions_AllPassingBadgeShown()
    {
        var assertions = GitHubAssertions();
        var cut = Render<QueryPanel>(p => p
            .Add(q => q.Assertions, assertions));

        var runAllBtn = cut.FindAll("button")
            .First(b => b.TextContent.Contains("Run All"));
        await cut.InvokeAsync(() => runAllBtn.Click());

        cut.WaitForAssertion(() =>
        {
            var label = cut.FindAll(".assertion-progress-label").FirstOrDefault();
            Assert.NotNull(label);
            Assert.Contains($"{assertions.Count} / {assertions.Count} passed", label.TextContent);
        }, timeout: TimeSpan.FromSeconds(10));
    }

    // -------------------------------------------------------------------------
    // Run individual row
    // -------------------------------------------------------------------------

    [Fact]
    public async Task RunSingle_FirstRow_OnlyFirstRowGetsResult()
    {
        var assertions = GitHubAssertions();
        var cut = Render<QueryPanel>(p => p
            .Add(q => q.Assertions, assertions));

        var runRowBtn = cut.FindAll("button.assertion-run-btn").First();
        await cut.InvokeAsync(() => runRowBtn.Click());

        cut.WaitForAssertion(() =>
        {
            var pendingIcons = cut.FindAll(".assertion-card-icon.pending");
            Assert.Equal(assertions.Count - 1, pendingIcons.Count);
        }, timeout: TimeSpan.FromSeconds(5));
    }

    // -------------------------------------------------------------------------
    // Remove assertion
    // -------------------------------------------------------------------------

    [Fact]
    public async Task RemoveAssertion_FirstRow_RowCountDecreases()
    {
        var assertions = GitHubAssertions();
        int changed = 0;
        var cut = Render<QueryPanel>(p => p
            .Add(q => q.Assertions, assertions)
            .Add(q => q.OnAssertionsChanged, EventCallback.Factory.Create(this, () => changed++)));

        var initialCards = cut.FindAll(".assertion-card").Count;

        var removeBtn = cut.FindAll("button.assertion-del-btn").First();
        await cut.InvokeAsync(() => removeBtn.Click());

        Assert.Equal(initialCards - 1, cut.FindAll(".assertion-card").Count);
        Assert.Equal(1, changed);
    }

    // -------------------------------------------------------------------------
    // Add assertion
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AddAssertion_ValidFields_RowAppearsInTable()
    {
        var assertions = new List<Assertion>();
        int changed = 0;
        var cut = Render<QueryPanel>(p => p
            .Add(q => q.Assertions, assertions)
            .Add(q => q.OnAssertionsChanged, EventCallback.Factory.Create(this, () => changed++)));

        // Add-assertion form selects: [0]=subjectType, [1]=permission(entityType|key), [2]=expected
        // Inputs: [0]=subjectId, [1]=entityId
        await cut.InvokeAsync(() => cut.FindAll("select.vtt-select")[0].Change("user"));
        await cut.InvokeAsync(() => cut.FindAll("input.vtt-input")[0].Change("torvalds"));
        await cut.InvokeAsync(() => cut.FindAll("select.vtt-select")[1].Change("repository|push"));
        await cut.InvokeAsync(() => cut.FindAll("input.vtt-input")[1].Change("linux"));
        // select[2] = expected; default is "allow" — leave as-is

        var addBtn = cut.FindAll("button").First(b => b.TextContent.Contains("Add"));
        await cut.InvokeAsync(() => addBtn.Click());

        Assert.Single(assertions);
        Assert.Equal("torvalds", assertions[0].SubjectId);
        Assert.Equal("push",     assertions[0].Permission);
        Assert.True(assertions[0].Expected);
        Assert.Equal(1, changed);
    }

    // -------------------------------------------------------------------------
    // Bulk paste
    // -------------------------------------------------------------------------

    [Fact]
    public async Task BulkApply_ValidLines_AppendsAssertions()
    {
        var assertions = new List<Assertion>();
        int changed = 0;
        var cut = Render<QueryPanel>(p => p
            .Add(q => q.Assertions, assertions)
            .Add(q => q.OnAssertionsChanged, EventCallback.Factory.Create(this, () => changed++)));

        var textarea = cut.Find("textarea.vtt-textarea");
        textarea.Input("user:torvalds push repository:linux allow\nuser:bob push repository:linux deny");

        var applyBtn = cut.FindAll("button").First(b => b.TextContent.Contains("Apply"));
        await cut.InvokeAsync(() => applyBtn.Click());

        Assert.Equal(2, assertions.Count);
        Assert.True(assertions[0].Expected);
        Assert.False(assertions[1].Expected);
        Assert.Equal(1, changed);
    }

    [Fact]
    public async Task BulkApply_InvalidLines_SkipsInvalidKeepsValid()
    {
        var assertions = new List<Assertion>();
        var cut = Render<QueryPanel>(p => p
            .Add(q => q.Assertions, assertions));

        var textarea = cut.Find("textarea.vtt-textarea");
        textarea.Input("user:torvalds push repository:linux allow\nnot a valid line\nuser:bob pull repository:linux deny");

        var applyBtn = cut.FindAll("button").First(b => b.TextContent.Contains("Apply"));
        await cut.InvokeAsync(() => applyBtn.Click());

        Assert.Equal(2, assertions.Count);
    }

    // -------------------------------------------------------------------------
    // ClearAssertionResults
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ClearAssertionResults_AfterRunAll_AllRowsShowNotRun()
    {
        var assertions = GitHubAssertions();
        var cut = Render<QueryPanel>(p => p
            .Add(q => q.Assertions, assertions));

        var runAllBtn = cut.FindAll("button").First(b => b.TextContent.Contains("Run All"));
        await cut.InvokeAsync(() => runAllBtn.Click());

        cut.WaitForAssertion(() =>
            Assert.Empty(cut.FindAll(".assertion-card-icon.pending")),
            timeout: TimeSpan.FromSeconds(10));

        await cut.InvokeAsync(() => cut.Instance.ClearAssertionResults());

        cut.WaitForAssertion(() =>
        {
            var pendingIcons = cut.FindAll(".assertion-card-icon.pending");
            Assert.Equal(assertions.Count, pendingIcons.Count);
        });
    }
}
