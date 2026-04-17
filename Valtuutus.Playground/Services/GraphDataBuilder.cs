using System.Text;
using System.Text.Json;
using Valtuutus.Core.Schemas;

namespace Valtuutus.Playground.Services;

/// <summary>
/// Builds a Cytoscape.js elements JSON array from a <see cref="Schema"/>.
/// Entity types are rectangular nodes; attributes are smaller pill nodes;
/// relations are directed edges between entity nodes.
/// </summary>
public static class GraphDataBuilder
{
    public static string BuildJson(Schema schema)
    {
        var elements = new List<object>();

        // Entity nodes
        foreach (var entityType in schema.Entities.Keys)
        {
            elements.Add(new
            {
                data = new { id = Sanitize(entityType), label = entityType, nodeType = "entity" }
            });
        }

        // Permission summary nodes — one node per entity listing all its permissions
        foreach (var (entityType, entity) in schema.Entities)
        {
            var perms = entity.Permissions.Keys.ToList();
            if (perms.Count == 0) continue;
            var entityId = Sanitize(entityType);
            var permId = $"{entityId}_perms";
            var label = string.Join(" · ", perms);
            elements.Add(new
            {
                data = new { id = permId, label, nodeType = "permission" }
            });
            elements.Add(new
            {
                data = new { id = $"e_{permId}", source = entityId, target = permId, edgeType = "permission" }
            });
        }

        // Attribute nodes + dashed edges
        foreach (var (entityType, entity) in schema.Entities)
        {
            foreach (var (attrName, attr) in entity.Attributes)
            {
                var attrId = $"{Sanitize(entityType)}_attr_{Sanitize(attrName)}";
                var typeName = FriendlyType(attr.Type);
                elements.Add(new
                {
                    data = new { id = attrId, label = $"{attrName}: {typeName}", nodeType = "attribute" }
                });
                elements.Add(new
                {
                    data = new { id = $"e_{attrId}", source = Sanitize(entityType), target = attrId, edgeType = "attribute" }
                });
            }
        }

        // Relation edges — group parallel edges between the same pair into one labelled edge
        var relationGroups = new Dictionary<(string from, string to), List<string>>();
        foreach (var (entityType, entity) in schema.Entities)
        {
            var fromId = Sanitize(entityType);
            foreach (var (relationName, relation) in entity.Relations)
            {
                foreach (var relEntity in relation.Entities)
                {
                    var toId = Sanitize(relEntity.Type);
                    var label = relEntity.Relation is not null
                        ? $"{relationName}#{relEntity.Relation}"
                        : relationName;
                    var key = (fromId, toId);
                    if (!relationGroups.TryGetValue(key, out var list))
                        relationGroups[key] = list = new List<string>();
                    list.Add(label);
                }
            }
        }
        foreach (var ((fromId, toId), labels) in relationGroups)
        {
            var edgeId = $"e_{fromId}_{toId}";
            var combinedLabel = string.Join("\n", labels);
            elements.Add(new
            {
                data = new { id = edgeId, source = fromId, target = toId, label = combinedLabel, edgeType = "relation" }
            });
        }


        return JsonSerializer.Serialize(elements);
    }

    private static string FriendlyType(Type type)
    {
        if (type == typeof(string))  return "string";
        if (type == typeof(int))     return "int";
        if (type == typeof(long))    return "long";
        if (type == typeof(bool))    return "bool";
        if (type == typeof(decimal)) return "decimal";
        if (type == typeof(double))  return "double";
        return type.Name;
    }

    private static string Sanitize(string name)
    {
        var sb = new StringBuilder(name.Length);
        foreach (var c in name)
            sb.Append(char.IsLetterOrDigit(c) ? c : '_');
        return sb.ToString();
    }
}
