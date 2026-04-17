using System.Diagnostics.CodeAnalysis;
using Valtuutus.Playground.Models;

namespace Valtuutus.Playground.Services;

/// <summary>
/// Parses and serialises assertions using the shorthand format:
///   subjectType:subjectId permission entityType:entityId allow|deny
/// </summary>
public static class AssertionParser
{
    public static bool TryParse(string line, [NotNullWhen(true)] out Assertion? assertion)
    {
        assertion = null;
        if (string.IsNullOrWhiteSpace(line)) return false;

        var parts = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4) return false;

        var subjectPart = parts[0];
        var permission = parts[1];
        var entityPart = parts[2];
        var expectedStr = parts[3].ToLowerInvariant();

        if (expectedStr != "allow" && expectedStr != "deny") return false;

        var subjectColon = subjectPart.IndexOf(':');
        if (subjectColon < 0) return false;
        var subjectType = subjectPart[..subjectColon];
        var subjectId = subjectPart[(subjectColon + 1)..];
        if (string.IsNullOrEmpty(subjectType) || string.IsNullOrEmpty(subjectId)) return false;

        var entityColon = entityPart.IndexOf(':');
        if (entityColon < 0) return false;
        var entityType = entityPart[..entityColon];
        var entityId = entityPart[(entityColon + 1)..];
        if (string.IsNullOrEmpty(entityType) || string.IsNullOrEmpty(entityId)) return false;

        if (string.IsNullOrEmpty(permission)) return false;

        assertion = new Assertion(subjectType, subjectId, permission, entityType, entityId,
            expectedStr == "allow");
        return true;
    }

    public static IReadOnlyList<Assertion> ParseLines(IEnumerable<string> lines)
    {
        var result = new List<Assertion>();
        foreach (var line in lines)
        {
            if (TryParse(line.Trim(), out var assertion))
                result.Add(assertion);
        }
        return result;
    }

    public static string ToShorthand(Assertion a) =>
        $"{a.SubjectType}:{a.SubjectId} {a.Permission} {a.EntityType}:{a.EntityId} {(a.Expected ? "allow" : "deny")}";
}
