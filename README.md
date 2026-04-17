# Valtuutus Playground

An interactive browser-based sandbox for experimenting with [Valtuutus](https://github.com/valtuutus/valtuutus) authorization schemas — no server required.

## What it does

The playground lets you write a Valtuutus schema, seed relation and attribute data, and immediately run authorization queries against it — all in the browser.

**Schema Editor** — write and validate schemas using the Valtuutus DSL (`entity`, `relation`, `permission`, `attribute`, `fn`). The editor highlights keywords, types, and relation references in colours that mirror the entity graph, and reports parse errors inline.

**Entity Graph** — a live visual of the schema rendered with Cytoscape.js + ELK. Entity nodes, attribute pills, and permission summaries update as you edit the schema. Relations between entities are grouped into single labelled edges to keep the graph readable.

**Data Panel** — seed relation tuples (e.g. `repository:linux#admin@user:torvalds`) and attribute tuples (e.g. `post:hello#is_public=true`) that the query engine runs against.

**Query Panel** — three query modes:
- **Check** — ask "does subject X have permission Y on entity Z?" Supports batch assertions with pass/fail results and a progress bar.
- **Lookup Entity** — ask "what entities of type X can subject Y perform action Z on?"
- **Lookup Subject** — ask "who can perform action Z on entity X?"

**Presets** — six built-in schemas (GitHub, Google Docs, Notion, Slack, IoT, Social) each with pre-seeded data and sample assertions to get started instantly.

**Share** — the full playground state (schema, data, assertions) serialises to a Base64 URL parameter so you can share a link to any configuration.

## Tech stack

| Layer | Technology |
|---|---|
| Framework | Blazor WebAssembly (.NET 10) |
| Authorization engine | [Valtuutus.Data.InMemory](https://www.nuget.org/packages/Valtuutus.Data.InMemory) |
| Schema editor | Monaco Editor via [BlazorMonaco](https://github.com/serdarciplak/BlazorMonaco) |
| Entity graph | [Cytoscape.js](https://cytoscape.org/) + [cytoscape-elk](https://github.com/cytoscape/cytoscape.js-elk) (ELK layered layout) |
| Tests | xUnit + bUnit |

## Project structure

```
Valtuutus.Playground/
├── Pages/
│   └── Home.razor              # Top-level orchestrator — holds all shared state
├── Components/
│   ├── SchemaEditor.razor      # Monaco editor wrapper + validation badge
│   ├── EntityGraph.razor       # Cytoscape graph host
│   ├── DataPanel.razor         # Tuple / attribute seed data entry
│   └── QueryPanel.razor        # Check / LookupEntity / LookupSubject + assertions
├── Services/
│   ├── SchemaService.cs        # Compiles schema text → ServiceProvider (engine lifecycle)
│   ├── PresetRegistry.cs       # 6 built-in schemas with seed data and sample assertions
│   ├── GraphDataBuilder.cs     # Schema → Cytoscape.js elements JSON
│   ├── PersistenceService.cs   # localStorage persistence + share URL serialisation
│   ├── AssertionParser.cs      # Parse / serialise assertion shorthand strings
│   ├── TupleParser.cs          # Parse / serialise relation tuple shorthand
│   └── AttributeParser.cs      # Parse / serialise attribute tuple shorthand
├── Models/
│   ├── Assertion.cs            # Check assertion record
│   ├── PlaygroundState.cs      # Serialisable snapshot of full playground state
│   └── ValidationResult.cs     # Schema validation result
└── wwwroot/
    ├── css/app.css             # Design system (C3 amber dark theme, B2 grid layout)
    └── js/valtuutus.js         # Monaco language definition + Cytoscape render + resize

Valtuutus.Playground.Tests/
├── SchemaServiceTests.cs       # Engine integration tests (Check, LookupEntity, LookupSubject)
├── QueryPanelTests.cs          # bUnit component tests for the query panel UI
├── AssertionParserTests.cs     # Assertion shorthand parse / round-trip tests
└── GraphDataBuilderTests.cs    # Graph JSON output tests (nodes, edges, grouping)
```

## How it works

**No backend.** Everything runs in WebAssembly. When you edit the schema, `SchemaService` compiles it by calling `AddValtuutusCore(schemaText).AddInMemory()` to build a fresh in-process `ServiceProvider`. Each data mutation rebuilds the provider and re-seeds the in-memory store. Queries resolve directly against `ICheckEngine`, `ILookupEntityEngine`, and `ILookupSubjectEngine`.

**State persistence.** `PersistenceService` serialises the playground state (schema text, tuples, attributes, assertions) to JSON, compresses it with `GZipStream`, and stores it in `localStorage`. The share button encodes the same payload as a Base64 `?state=` URL parameter.

**Entity graph.** `GraphDataBuilder` transforms a compiled `Schema` into a Cytoscape.js elements array: entity nodes (amber), attribute nodes (teal), permission summary nodes (purple), and relation edges grouped by source/target pair to avoid parallel-edge clutter. Cytoscape renders this using the ELK `layered` algorithm (left-to-right, `LAYER_SWEEP` crossing minimisation).

**Editor syntax highlighting.** A custom Monarch tokeniser registers the `valtuutus` language with Monaco. Each keyword category (`entity`, `relation`, `permission`, `attribute`) gets its own colour matching the graph's visual language. A custom `vtt-dark` theme aligns the editor background and cursor with the app's navy/amber palette.

## Running locally

```bash
# Prerequisites: .NET 10 SDK

git clone https://github.com/valtuutus/playground
cd playground

# Run the dev server
dotnet watch run --project Valtuutus.Playground

# Run tests
dotnet test Valtuutus.Playground.sln
```

The app opens at `http://localhost:5270`.
