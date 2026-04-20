using Valtuutus.Core.Engines.Check;

namespace Valtuutus.Playground.Models;

public sealed record ExplainRequestedArgs(CheckExplainResult Result, string Label);
