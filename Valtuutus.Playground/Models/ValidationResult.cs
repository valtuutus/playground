using Valtuutus.Core.Lang;

namespace Valtuutus.Playground.Models;

public sealed record ValidationResult
{
    public bool IsValid { get; init; }
    public IReadOnlyList<LangError> Errors { get; init; } = Array.Empty<LangError>();

    private ValidationResult() { }

    public static ValidationResult Valid() => new() { IsValid = true };

    public static ValidationResult Invalid(IReadOnlyList<LangError> errors) =>
        new() { IsValid = false, Errors = errors };
}
