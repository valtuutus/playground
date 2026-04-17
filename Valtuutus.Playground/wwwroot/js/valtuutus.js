// v2
window.init_vtt = () => {
    const valtuutus = 'valtuutus';
    monaco.languages.register({id: valtuutus})
    const syntax = {
        defaultToken: 'invalid',

        kwEntity:     ['entity'],
        kwRelation:   ['relation'],
        kwPermission: ['permission'],
        kwAttribute:  ['attribute'],
        kwFn:         ['fn'],
        types: ['int', 'bool', 'string', 'decimal'],
        logicalOperators: ['and', 'or'],
        operators: ['=', '->', ':', ';', '.', '{', '}'],

        brackets: [
            { open: '{', close: '}', token: 'delimiter.curly' },
            { open: '(', close: ')', token: 'delimiter.parenthesis' }
        ],

        tokenizer: {
            root: [
                // #relation-ref (e.g. team#member — the #member part)
                [/#[a-zA-Z_][a-zA-Z0-9_]*/, 'kw.relation-ref'],

                [/[a-zA-Z_][a-zA-Z0-9_]*/, {
                    cases: {
                        '@kwEntity':     'kw.entity',
                        '@kwRelation':   'kw.relation',
                        '@kwPermission': 'kw.permission',
                        '@kwAttribute':  'kw.attribute',
                        '@kwFn':         'kw.fn',
                        '@types':        'type',
                        '@logicalOperators': 'operator',
                        '@default': 'identifier'
                    }
                }],

                [/@[a-zA-Z_][a-zA-Z0-9_]*/, 'annotation'],
                [/[0-9]+\.[0-9]+/, 'number.float'],
                [/[0-9]+/, 'number'],
                [/".*?"/, 'string'],
                [/\->|[:;.=]/, 'operator'],
                [/[{}()]/, '@brackets'],
                [/\/\/.*$/, 'comment'],
                [/\s+/, 'white']
            ]
        }
    };

    monaco.languages.setMonarchTokensProvider(valtuutus, syntax);
    monaco.languages.setLanguageConfiguration(valtuutus, {
        comments: {
            lineComment: '//',
            blockComment: ['/*', '*/'],
        },
        brackets: [
            ['{', '}'],
            ['(', ')'],
        ],
        autoClosingPairs: [
            { open: '{', close: '}' },
            { open: '(', close: ')' },
        ],
        surroundingPairs: [
            { open: '{', close: '}' },
            { open: '(', close: ')' },
        ],
    });

    monaco.languages.registerCompletionItemProvider(valtuutus, {
        provideCompletionItems(model, position) {
            const suggestions = [
                ...syntax.keywords.map(k => ({
                    label: k,
                    kind: monaco.languages.CompletionItemKind.Keyword,
                    insertText: k,
                })),
            ];
            return {suggestions: suggestions};
        }
    });

    monaco.editor.defineTheme('vtt-dark', {
        base: 'vs-dark',
        inherit: true,
        rules: [
            // Structural keywords — each maps to graph node/edge colors
            { token: 'kw.entity',      foreground: 'f5ae38', fontStyle: 'bold' },   // amber — entity nodes
            { token: 'kw.relation',    foreground: 'b49aee', fontStyle: 'bold' },   // purple — relation edges
            { token: 'kw.permission',  foreground: 'e87cff', fontStyle: 'bold' },   // pink — permission nodes
            { token: 'kw.attribute',   foreground: '40d0d8', fontStyle: 'bold' },   // teal — attribute nodes
            { token: 'kw.fn',          foreground: '7ec88a', fontStyle: 'bold' },   // green
            { token: 'kw.relation-ref',foreground: 'b49aee' },                      // purple, dimmer (same family as relation)
            { token: 'type',           foreground: '40d0d8' },
            { token: 'operator',       foreground: '8aa0c0' },
            { token: 'identifier',     foreground: 'c5d5e8' },
            { token: 'annotation',     foreground: 'e87cff' },
            { token: 'string',         foreground: '7ec88a' },
            { token: 'number',         foreground: 'b49aee' },
            { token: 'number.float',   foreground: 'b49aee' },
            { token: 'comment',        foreground: '3d5470', fontStyle: 'italic' },
            { token: 'invalid',        foreground: 'ef6878' },
            { token: 'delimiter.curly',       foreground: '6d84a0' },
            { token: 'delimiter.parenthesis', foreground: '6d84a0' },
        ],
        colors: {
            'editor.background':                  '#101c30',
            'editor.foreground':                  '#c5d5e8',
            'editor.lineHighlightBackground':     '#18203a',
            'editor.selectionBackground':         '#2d407080',
            'editor.inactiveSelectionBackground': '#1e2f5050',
            'editorLineNumber.foreground':        '#2d4060',
            'editorLineNumber.activeForeground':  '#f5ae38',
            'editorCursor.foreground':            '#f5ae38',
            'editorIndentGuide.background':       '#1a2640',
            'editorIndentGuide.activeBackground': '#2d4070',
            'editorBracketMatch.background':      '#2d407060',
            'editorBracketMatch.border':          '#f5ae3880',
            'editorWidget.background':            '#0d1628',
            'editorWidget.border':                '#1e2f50',
            'editorSuggestWidget.background':     '#0d1628',
            'editorSuggestWidget.border':         '#1e2f50',
            'editorSuggestWidget.selectedBackground': '#18203a',
            'scrollbarSlider.background':         '#1e2f4060',
            'scrollbarSlider.hoverBackground':    '#2d406080',
            'scrollbarSlider.activeBackground':   '#f5ae3840',
        }
    });

    monaco.editor.setTheme('vtt-dark');
};

let _cy = null;

window.renderCytoscape = (containerId, elementsJson) => {
    const container = document.getElementById(containerId);
    if (!container) return;

    if (_cy) { _cy.destroy(); _cy = null; }

    let elements;
    try { elements = JSON.parse(elementsJson); }
    catch (e) { container.innerHTML = '<p style="color:#ef6878;font-size:12px;padding:12px">Graph error: ' + e.message + '</p>'; return; }

    console.log('[vtt] dagre registered:', typeof cytoscape.prototype.layout !== 'undefined');

    _cy = cytoscape({
        container,
        elements,
        style: [
            // Entity nodes
            {
                selector: 'node[nodeType = "entity"]',
                style: {
                    'shape': 'round-rectangle',
                    'background-color': '#18203a',
                    'border-width': 2,
                    'border-color': '#f5ae38',
                    'label': 'data(label)',
                    'color': '#eaf2ff',
                    'font-family': 'JetBrains Mono, monospace',
                    'font-size': 13,
                    'font-weight': 600,
                    'text-valign': 'center',
                    'text-halign': 'center',
                    'padding': 16,
                    'width': 'label',
                    'height': 'label',
                    'min-width': 100,
                    'min-height': 40,
                }
            },
            // Attribute nodes
            {
                selector: 'node[nodeType = "attribute"]',
                style: {
                    'shape': 'round-rectangle',
                    'background-color': '#1a2640',
                    'border-width': 1,
                    'border-color': '#40d0d8',
                    'label': 'data(label)',
                    'color': '#40d0d8',
                    'font-family': 'JetBrains Mono, monospace',
                    'font-size': 11,
                    'text-valign': 'center',
                    'text-halign': 'center',
                    'padding': 10,
                    'width': 'label',
                    'height': 'label',
                    'min-width': 80,
                    'min-height': 28,
                }
            },
            // Relation edges
            {
                selector: 'edge[edgeType = "relation"]',
                style: {
                    'width': 1.5,
                    'line-color': '#b49aee',
                    'target-arrow-color': '#b49aee',
                    'target-arrow-shape': 'triangle',
                    'arrow-scale': 1.2,
                    'curve-style': 'bezier',
                    'label': 'data(label)',
                    'font-size': 10,
                    'font-family': 'IBM Plex Sans, sans-serif',
                    'color': '#8aa0c0',
                    'text-wrap': 'wrap',
                    'text-max-width': '120px',
                    'text-background-color': '#101525',
                    'text-background-opacity': 1,
                    'text-background-padding': 4,
                    'text-border-opacity': 0,
                    'edge-text-rotation': 'none',
                }
            },
            // Attribute dashed edges
            {
                selector: 'edge[edgeType = "attribute"]',
                style: {
                    'width': 1,
                    'line-color': '#28365a',
                    'target-arrow-color': '#28365a',
                    'target-arrow-shape': 'triangle',
                    'curve-style': 'bezier',
                    'line-style': 'dashed',
                    'line-dash-pattern': [5, 3],
                }
            },
            // Permission summary nodes
            {
                selector: 'node[nodeType = "permission"]',
                style: {
                    'shape': 'round-rectangle',
                    'background-color': '#140e22',
                    'border-width': 1,
                    'border-color': '#9b6dff',
                    'border-style': 'dashed',
                    'label': 'data(label)',
                    'color': '#9b6dff',
                    'font-family': 'IBM Plex Sans, sans-serif',
                    'font-size': 10,
                    'font-style': 'italic',
                    'text-valign': 'center',
                    'text-halign': 'center',
                    'text-wrap': 'wrap',
                    'text-max-width': '180px',
                    'padding': 8,
                    'width': 'label',
                    'height': 'label',
                    'min-width': 60,
                    'min-height': 20,
                }
            },
            // Permission edges
            {
                selector: 'edge[edgeType = "permission"]',
                style: {
                    'width': 1,
                    'line-color': '#2e1f4a',
                    'target-arrow-color': '#2e1f4a',
                    'target-arrow-shape': 'triangle',
                    'arrow-scale': 0.8,
                    'curve-style': 'bezier',
                    'line-style': 'dashed',
                    'line-dash-pattern': [3, 4],
                }
            },
            // Hover state
            {
                selector: 'node:active',
                style: { 'overlay-opacity': 0 }
            }
        ],
        layout: {
            name: 'elk',
            animate: false,
            elk: {
                algorithm: 'layered',
                'elk.direction': 'RIGHT',
                'elk.layered.crossingMinimization.strategy': 'LAYER_SWEEP',
                'elk.layered.crossingMinimization.greedySwitchType': 'TWO_SIDED',
                'elk.layered.nodePlacement.strategy': 'BRANDES_KOEPF',
                'elk.layered.thoroughness': '15',
                'elk.spacing.nodeNode': '60',
                'elk.layered.spacing.nodeNodeBetweenLayers': '120',
                'elk.padding': '[top=24,left=24,bottom=24,right=24]',
            },
        },
        userZoomingEnabled: true,
        userPanningEnabled: true,
        boxSelectionEnabled: false,
        autoungrabify: false,
    });

    _cy.fit(undefined, 32);
    if (_cy.zoom() < 0.25) {
        _cy.zoom(0.25);
        _cy.center();
    }
};

window.localStorage_set = (key, value) => localStorage.setItem(key, value);
window.localStorage_get = (key) => localStorage.getItem(key);
window.localStorage_remove = (key) => localStorage.removeItem(key);

window.get_url_param = (param) => new URL(window.location.href).searchParams.get(param);
window.set_url = (url) => history.replaceState({}, '', url);
window.get_current_url = () => window.location.href;
window.copy_to_clipboard = (text) => navigator.clipboard.writeText(text);

window.initSchemaResize = () => {
    const handle = document.querySelector('.b2-resize-handle');
    const layout = document.querySelector('.app-layout-b2');
    if (!handle || !layout) return;

    let startX, startW;

    handle.addEventListener('mousedown', e => {
        const cols = getComputedStyle(layout).gridTemplateColumns.split(' ');
        startX = e.clientX;
        startW = parseFloat(cols[0]);
        handle.classList.add('dragging');
        document.body.style.cursor = 'col-resize';
        document.body.style.userSelect = 'none';
        document.addEventListener('mousemove', onMove);
        document.addEventListener('mouseup', onUp);
        e.preventDefault();
    });

    function onMove(e) {
        const newW = Math.max(220, Math.min(800, startW + (e.clientX - startX)));
        layout.style.gridTemplateColumns = `${newW}px 4px 1fr 1fr`;
    }

    function onUp() {
        handle.classList.remove('dragging');
        document.body.style.cursor = '';
        document.body.style.userSelect = '';
        document.removeEventListener('mousemove', onMove);
        document.removeEventListener('mouseup', onUp);
    }
};