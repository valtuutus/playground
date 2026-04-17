namespace Valtuutus.Playground.Models;

public sealed record Assertion(
    string SubjectType,
    string SubjectId,
    string Permission,
    string EntityType,
    string EntityId,
    bool Expected
);
