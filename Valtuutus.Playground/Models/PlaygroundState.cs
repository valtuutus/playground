namespace Valtuutus.Playground.Models;

public sealed class PlaygroundState
{
    public string SchemaText { get; set; } = string.Empty;
    public List<string> Tuples { get; set; } = new();
    public List<string> Attributes { get; set; } = new();
}
