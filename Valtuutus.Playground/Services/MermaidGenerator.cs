using System.Text;
using Valtuutus.Core.Schemas;

namespace Valtuutus.Playground.Services;

/// <summary>
/// Generates a Mermaid <c>flowchart LR</c> diagram from a <see cref="Schema"/>.
/// Each entity type is a node; each relation produces edges to its allowed subject types.
/// </summary>
public static class MermaidGenerator
{
    /// <summary>
    /// Generates a Mermaid flowchart string for the given schema.
    /// </summary>
    public static string Generate(Schema schema)
    {
        var sb = new StringBuilder();
        sb.AppendLine("flowchart LR");

        // Collect all entity type nodes
        foreach (var entityType in schema.Entities.Keys)
        {
            sb.AppendLine($"    {SanitizeId(entityType)}[\"{entityType}\"]");
        }

        // Emit edges for each relation
        foreach (var (entityType, entity) in schema.Entities)
        {
            foreach (var (relationName, relation) in entity.Relations)
            {
                foreach (var relEntity in relation.Entities)
                {
                    var targetType = relEntity.Type;
                    var edgeLabel = relEntity.Relation is not null
                        ? $"{relationName}#{relEntity.Relation}"
                        : relationName;

                    sb.AppendLine($"    {SanitizeId(entityType)} -->|\"{edgeLabel}\"| {SanitizeId(targetType)}");
                }
            }
        }

        return sb.ToString().TrimEnd();
    }

    private static string SanitizeId(string name)
    {
        // Mermaid node IDs must be alphanumeric + underscores
        var sb = new StringBuilder(name.Length);
        foreach (var c in name)
            sb.Append(char.IsLetterOrDigit(c) ? c : '_');
        return sb.ToString();
    }
}
