using Microsoft.Extensions.DependencyInjection;
using Valtuutus.Core;
using Valtuutus.Core.Configuration;
using Valtuutus.Core.Data;
using Valtuutus.Core.Engines.Check;
using Valtuutus.Core.Engines.LookupEntity;
using Valtuutus.Core.Engines.LookupSubject;
using Valtuutus.Core.Lang;
using Valtuutus.Core.Schemas;
using Valtuutus.Data.InMemory;
using Valtuutus.Playground.Models;

namespace Valtuutus.Playground.Services;

/// <summary>
/// Scoped service that owns a child <see cref="ServiceProvider"/> built from
/// <c>AddValtuutusCore + AddInMemory</c>.  Rebuilding the provider resets the
/// InMemory stores, which is how data is cleared between seed loads.
/// </summary>
public sealed class SchemaService : IDisposable
{
    private ServiceProvider? _provider;
    private string? _currentSchemaText;
    private SnapToken? _latestSnapToken;

    public bool IsValid => _provider is not null;

    public Schema? Schema
    {
        get
        {
            if (_provider is null) return null;
            return _provider.GetService<Schema>();
        }
    }

    /// <summary>
    /// Attempts to compile <paramref name="schemaText"/>.
    /// On success, rebuilds the inner provider and returns <see cref="ValidationResult.Valid()"/>.
    /// On failure, returns <see cref="ValidationResult.Invalid"/> with parsed errors.
    /// </summary>
    public ValidationResult TrySetSchema(string schemaText)
    {
        ServiceProvider? newProvider = null;
        try
        {
            newProvider = BuildProvider(schemaText);

            // Swap out old provider
            _provider?.Dispose();
            _provider = newProvider;
            _currentSchemaText = schemaText;
            _latestSnapToken = null;
            return ValidationResult.Valid();
        }
        catch (InvalidOperationException ex)
        {
            newProvider?.Dispose();
            var errors = ParseErrors(ex.Message);
            return ValidationResult.Invalid(errors);
        }
    }

    private static ServiceProvider BuildProvider(string schemaText)
    {
        var services = new ServiceCollection();
        services.AddValtuutusCore(schemaText).AddInMemory();
        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Rebuilds the inner provider (clearing all InMemory data), then writes the
    /// supplied tuples and attributes as a fresh seed.
    /// </summary>
    public async Task ResetDataAsync(
        IEnumerable<RelationTuple> tuples,
        IEnumerable<AttributeTuple> attributes,
        CancellationToken ct = default)
    {
        if (_currentSchemaText is null)
            throw new InvalidOperationException("No valid schema has been set.");

        // Build and seed a fresh provider BEFORE swapping _provider, so a
        // concurrent call that disposes _provider mid-await cannot touch the
        // ReaderWriterLockSlim we're still writing into.
        var newProvider = BuildProvider(_currentSchemaText);

        using var scope = newProvider.CreateScope();
        var writer = scope.ServiceProvider.GetRequiredService<IDataWriterProvider>();
        var snapToken = await writer.Write(tuples, attributes, ct);

        // Swap only after the write completes.
        var oldProvider = _provider;
        _provider = newProvider;
        _latestSnapToken = snapToken;
        oldProvider?.Dispose();
    }

    /// <summary>
    /// Evaluates a permission check against the latest seeded data snapshot.
    /// </summary>
    public async Task<bool> CheckAsync(CheckRequest request, CancellationToken ct = default)
    {
        EnsureProvider();
        request.SnapToken = _latestSnapToken;
        using var scope = _provider!.CreateScope();
        var engine = scope.ServiceProvider.GetRequiredService<ICheckEngine>();
        return await engine.Check(request, ct);
    }

    /// <summary>
    /// Runs Explain against the latest seeded data snapshot, returning the full resolution tree.
    /// </summary>
    public async Task<CheckExplainResult> ExplainAsync(CheckRequest request, CancellationToken ct = default)
    {
        EnsureProvider();
        request.SnapToken = _latestSnapToken;
        using var scope = _provider!.CreateScope();
        var engine = scope.ServiceProvider.GetRequiredService<ICheckEngine>();
        return await engine.Explain(request, ct);
    }

    /// <summary>
    /// Looks up entities against the latest seeded data snapshot.
    /// </summary>
    public async Task<LookupEntityPage> LookupEntityAsync(LookupEntityRequest request, CancellationToken ct = default)
    {
        EnsureProvider();
        request.SnapToken = _latestSnapToken;
        using var scope = _provider!.CreateScope();
        var engine = scope.ServiceProvider.GetRequiredService<ILookupEntityEngine>();
        return await engine.LookupEntity(request, ct);
    }

    /// <summary>
    /// Looks up subjects against the latest seeded data snapshot.
    /// </summary>
    public async Task<HashSet<string>> LookupSubjectAsync(LookupSubjectRequest request, CancellationToken ct = default)
    {
        EnsureProvider();
        request.SnapToken = _latestSnapToken;
        using var scope = _provider!.CreateScope();
        var engine = scope.ServiceProvider.GetRequiredService<ILookupSubjectEngine>();
        return await engine.Lookup(request, ct);
    }

    private void EnsureProvider()
    {
        if (_provider is null)
            throw new InvalidOperationException("No valid schema has been set.");
    }

    // ---------------------------------------------------------------------------
    // Error parsing
    // ---------------------------------------------------------------------------

    /// <summary>
    /// The exception message from <c>AddValtuutusCore</c> is a comma-separated list of
    /// <see cref="LangError.ToString()"/> values in the format <c>"Line {line}:{col} {message}"</c>.
    /// </summary>
    private static IReadOnlyList<LangError> ParseErrors(string message)
    {
        var errors = new List<LangError>();
        var parts = message.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var part in parts)
        {
            if (TryParseLangError(part, out var error))
                errors.Add(error);
            else
                errors.Add(new LangError(part, 0, 0));
        }
        return errors;
    }

    private static bool TryParseLangError(string text, out LangError error)
    {
        error = default!;
        // Format: "Line {line}:{col} {message}"
        if (!text.StartsWith("Line ", StringComparison.OrdinalIgnoreCase)) return false;

        var rest = text["Line ".Length..];
        var spaceIndex = rest.IndexOf(' ');
        if (spaceIndex < 0) return false;

        var lineCol = rest[..spaceIndex];
        var msg = rest[(spaceIndex + 1)..];

        var colonIndex = lineCol.IndexOf(':');
        if (colonIndex < 0) return false;

        if (!int.TryParse(lineCol[..colonIndex], out var line)) return false;
        if (!int.TryParse(lineCol[(colonIndex + 1)..], out var col)) return false;

        error = new LangError(msg, line, col);
        return true;
    }

    public void Dispose()
    {
        _provider?.Dispose();
        _provider = null;
    }
}
