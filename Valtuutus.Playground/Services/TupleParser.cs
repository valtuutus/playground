using System.Diagnostics.CodeAnalysis;
using Valtuutus.Core;

namespace Valtuutus.Playground.Services;

/// <summary>
/// Parses and serialises relation tuples using the shorthand format:
///   entityType:entityId#relation@subjectType:subjectId[#subjectRelation]
/// </summary>
public static class TupleParser
{
    /// <summary>
    /// Attempts to parse a single shorthand string into a <see cref="RelationTuple"/>.
    /// Returns <c>true</c> and sets <paramref name="tuple"/> on success.
    /// </summary>
    public static bool TryParse(string line, [NotNullWhen(true)] out RelationTuple? tuple)
    {
        tuple = null;
        if (string.IsNullOrWhiteSpace(line)) return false;

        // Split on '@' to separate entity#relation from subject
        var atIndex = line.IndexOf('@');
        if (atIndex < 0) return false;

        var entityPart = line[..atIndex];
        var subjectPart = line[(atIndex + 1)..];

        // Parse entity side: entityType:entityId#relation
        var hashIndex = entityPart.IndexOf('#');
        if (hashIndex < 0) return false;
        var entityColon = entityPart.IndexOf(':');
        if (entityColon < 0 || entityColon > hashIndex) return false;

        var entityType = entityPart[..entityColon];
        var entityId = entityPart[(entityColon + 1)..hashIndex];
        var relation = entityPart[(hashIndex + 1)..];

        if (string.IsNullOrEmpty(entityType) || string.IsNullOrEmpty(entityId) || string.IsNullOrEmpty(relation))
            return false;

        // Parse subject side: subjectType:subjectId[#subjectRelation]
        string subjectType, subjectId, subjectRelation;
        var subHashIndex = subjectPart.IndexOf('#');
        if (subHashIndex >= 0)
        {
            subjectRelation = subjectPart[(subHashIndex + 1)..];
            subjectPart = subjectPart[..subHashIndex];
        }
        else
        {
            subjectRelation = string.Empty;
        }

        var subjectColon = subjectPart.IndexOf(':');
        if (subjectColon < 0) return false;

        subjectType = subjectPart[..subjectColon];
        subjectId = subjectPart[(subjectColon + 1)..];

        if (string.IsNullOrEmpty(subjectType) || string.IsNullOrEmpty(subjectId)) return false;

        tuple = new RelationTuple(entityType, entityId, relation, subjectType, subjectId,
            string.IsNullOrEmpty(subjectRelation) ? null : subjectRelation);
        return true;
    }

    /// <summary>
    /// Parses multiple shorthand lines, silently skipping blank lines and invalid entries.
    /// </summary>
    public static IReadOnlyList<RelationTuple> ParseLines(IEnumerable<string> lines)
    {
        var result = new List<RelationTuple>();
        foreach (var line in lines)
        {
            if (TryParse(line.Trim(), out var tuple))
                result.Add(tuple.Value);
        }
        return result;
    }

    /// <summary>
    /// Converts a <see cref="RelationTuple"/> back to its shorthand string representation.
    /// </summary>
    public static string ToShorthand(RelationTuple tuple)
    {
        var subject = $"{tuple.SubjectType}:{tuple.SubjectId}";
        if (!string.IsNullOrEmpty(tuple.SubjectRelation))
            subject += $"#{tuple.SubjectRelation}";
        return $"{tuple.EntityType}:{tuple.EntityId}#{tuple.Relation}@{subject}";
    }
}
