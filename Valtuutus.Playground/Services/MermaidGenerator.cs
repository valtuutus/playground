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
    /// Entity types are rectangular nodes; attributes appear as stadium-shaped nodes
    /// attached to their entity with a dashed edge showing the attribute type.
    /// </summary>
    public static string Generate(Schema schema)
    {
        var sb = new StringBuilder();
        sb.AppendLine("flowchart LR");

        // Entity type nodes
        foreach (var entityType in schema.Entities.Keys)
        {
            sb.AppendLine($"    {SanitizeId(entityType)}[\"{entityType}\"]");
        }

        // Attribute nodes (stadium shape) + dashed edges from their entity
        foreach (var (entityType, entity) in schema.Entities)
        {
            foreach (var (attrName, attr) in entity.Attributes)
            {
                var attrNodeId = $"{SanitizeId(entityType)}_attr_{SanitizeId(attrName)}";
                var typeName = FriendlyTypeName(attr.Type);
                sb.AppendLine($"    {attrNodeId}([\"{attrName}: {typeName}\"])");
                sb.AppendLine($"    {SanitizeId(entityType)} -.-> {attrNodeId}");
            }
        }

        // Relation edges
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

    private static string FriendlyTypeName(Type type)
    {
        if (type == typeof(string))  return "string";
        if (type == typeof(int))     return "int";
        if (type == typeof(long))    return "long";
        if (type == typeof(bool))    return "bool";
        if (type == typeof(decimal)) return "decimal";
        if (type == typeof(double))  return "double";
        return type.Name;
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
