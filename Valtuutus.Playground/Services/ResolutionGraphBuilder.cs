using System.Text.Json;
using Valtuutus.Core.Engines.Check;

namespace Valtuutus.Playground.Services;

public static class ResolutionGraphBuilder
{
    public static string BuildJson(CheckExplainResult explainResult)
    {
        var elements = new List<object>();
        Walk(explainResult.Root, parentId: null, id: "n", elements);
        return JsonSerializer.Serialize(elements);
    }

    private static void Walk(CheckNode node, string? parentId, string id, List<object> elements)
    {
        var entityPart = node.EntityType is not null ? $"\n{node.EntityType}:{node.EntityId}" : "";
        var subjectPart = node.SubjectType is not null ? $"\n→ {node.SubjectType}:{node.SubjectId}" : "";
        var detailPart = string.IsNullOrEmpty(node.Detail) ? "" : $"\n{node.Detail}";
        var label = $"{node.Name}{entityPart}{subjectPart}{detailPart}";

        elements.Add(new
        {
            data = new
            {
                id,
                label,
                nodeType = node.Type.ToString().ToLowerInvariant(),
                result = node.Result ? "pass" : "fail"
            }
        });

        if (parentId is not null)
        {
            elements.Add(new
            {
                data = new { id = $"e_{parentId}_{id}", source = parentId, target = id }
            });
        }

        for (var i = 0; i < node.Children.Count; i++)
            Walk(node.Children[i], parentId: id, id: $"{id}_{i}", elements);
    }
}
