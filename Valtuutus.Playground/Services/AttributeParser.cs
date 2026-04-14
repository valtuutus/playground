using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
using Valtuutus.Core;

namespace Valtuutus.Playground.Services;

/// <summary>
/// Parses and serialises attribute tuples using the shorthand format:
///   entityType:entityId$attribute=value
///
/// Value type auto-detection order: bool → int → decimal → string.
/// </summary>
public static class AttributeParser
{
    /// <summary>
    /// Attempts to parse a single shorthand string into an <see cref="AttributeTuple"/>.
    /// Returns <c>true</c> and sets <paramref name="tuple"/> on success.
    /// </summary>
    public static bool TryParse(string line, [NotNullWhen(true)] out AttributeTuple? tuple)
    {
        tuple = null;
        if (string.IsNullOrWhiteSpace(line)) return false;

        // Split on '$' to get entity side and attribute=value side
        var dollarIndex = line.IndexOf('$');
        if (dollarIndex < 0) return false;

        var entityPart = line[..dollarIndex];
        var attrValuePart = line[(dollarIndex + 1)..];

        // Parse entity side: entityType:entityId
        var colonIndex = entityPart.IndexOf(':');
        if (colonIndex < 0) return false;

        var entityType = entityPart[..colonIndex];
        var entityId = entityPart[(colonIndex + 1)..];

        if (string.IsNullOrEmpty(entityType) || string.IsNullOrEmpty(entityId)) return false;

        // Parse attribute=value
        var eqIndex = attrValuePart.IndexOf('=');
        if (eqIndex < 0) return false;

        var attribute = attrValuePart[..eqIndex];
        var rawValue = attrValuePart[(eqIndex + 1)..];

        if (string.IsNullOrEmpty(attribute)) return false;

        var jsonValue = ParseValue(rawValue);
        tuple = new AttributeTuple(entityType, entityId, attribute, jsonValue);
        return true;
    }

    private static JsonValue ParseValue(string raw)
    {
        // bool
        if (bool.TryParse(raw, out var boolVal))
            return JsonValue.Create(boolVal);

        // int
        if (int.TryParse(raw, System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture, out var intVal))
            return JsonValue.Create(intVal);

        // decimal
        if (decimal.TryParse(raw, System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture, out var decVal))
            return JsonValue.Create(decVal);

        // string fallback
        return JsonValue.Create(raw)!;
    }

    /// <summary>
    /// Parses multiple shorthand lines, silently skipping blank lines and invalid entries.
    /// </summary>
    public static IReadOnlyList<AttributeTuple> ParseLines(IEnumerable<string> lines)
    {
        var result = new List<AttributeTuple>();
        foreach (var line in lines)
        {
            if (TryParse(line.Trim(), out var tuple))
                result.Add(tuple);
        }
        return result;
    }

    /// <summary>
    /// Converts an <see cref="AttributeTuple"/> back to its shorthand string representation.
    /// </summary>
    public static string ToShorthand(AttributeTuple tuple)
    {
        // Serialize the value back to its raw string form
        var raw = SerializeValue(tuple);
        return $"{tuple.EntityType}:{tuple.EntityId}${tuple.Attribute}={raw}";
    }

    private static string SerializeValue(AttributeTuple tuple)
    {
        // Try each type in detection order and serialize appropriately
        try { return tuple.GetValue(typeof(bool))?.ToString()?.ToLowerInvariant() ?? ""; }
        catch { }
        try { return tuple.GetValue(typeof(int))?.ToString() ?? ""; }
        catch { }
        try
        {
            var d = (decimal?)tuple.GetValue(typeof(decimal));
            return d?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "";
        }
        catch { }
        try { return tuple.GetValue(typeof(string))?.ToString() ?? ""; }
        catch { }
        return tuple.Value.ToString() ?? "";
    }
}
