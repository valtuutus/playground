using System.Text;
using System.Text.Json;
using Valtuutus.Playground.Models;

namespace Valtuutus.Playground.Services;

/// <summary>
/// Encodes and decodes <see cref="PlaygroundState"/> as a Base64-encoded JSON string,
/// suitable for embedding in URLs.
/// </summary>
public static class StateSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    /// <summary>
    /// Serializes <paramref name="state"/> to a URL-safe Base64 string.
    /// </summary>
    public static string Encode(PlaygroundState state)
    {
        var json = JsonSerializer.Serialize(state, JsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Deserializes a Base64 string produced by <see cref="Encode"/> back to a
    /// <see cref="PlaygroundState"/>. Returns <c>null</c> if the input is null,
    /// empty, or otherwise invalid.
    /// </summary>
    public static PlaygroundState? Decode(string? encoded)
    {
        if (string.IsNullOrWhiteSpace(encoded)) return null;
        try
        {
            var bytes = Convert.FromBase64String(encoded);
            var json = Encoding.UTF8.GetString(bytes);
            return JsonSerializer.Deserialize<PlaygroundState>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }
}
