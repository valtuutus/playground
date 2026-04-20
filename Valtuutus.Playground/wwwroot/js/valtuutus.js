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
            { token: 'kw.entity',      foreground: 'f5ae38', fontStyle: 'bold' },
            { token: 'kw.relation',    foreground: 'b49aee', fontStyle: 'bold' },
            { token: 'kw.permission',  foreground: 'e87cff', fontStyle: 'bold' },
            { token: 'kw.attribute',   foreground: '40d0d8', fontStyle: 'bold' },
            { token: 'kw.fn',          foreground: '7ec88a', fontStyle: 'bold' },
            { token: 'kw.relation-ref',foreground: 'b49aee' },
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

    monaco.editor.defineTheme('vtt-light', {
        base: 'vs',
        inherit: true,
        rules: [
            { token: 'kw.entity',      foreground: 'b07800', fontStyle: 'bold' },
            { token: 'kw.relation',    foreground: '5040b0', fontStyle: 'bold' },
            { token: 'kw.permission',  foreground: '8020b0', fontStyle: 'bold' },
            { token: 'kw.attribute',   foreground: '007888', fontStyle: 'bold' },
            { token: 'kw.fn',          foreground: '1a7040', fontStyle: 'bold' },
            { token: 'kw.relation-ref',foreground: '6050c0' },
            { token: 'type',           foreground: '007888' },
            { token: 'operator',       foreground: '3a5070' },
            { token: 'identifier',     foreground: '1a3050' },
            { token: 'annotation',     foreground: '8020b0' },
            { token: 'string',         foreground: '1a7040' },
            { token: 'number',         foreground: '5040b0' },
            { token: 'number.float',   foreground: '5040b0' },
            { token: 'comment',        foreground: '7090b0', fontStyle: 'italic' },
            { token: 'invalid',        foreground: 'b02020' },
            { token: 'delimiter.curly',       foreground: '506080' },
            { token: 'delimiter.parenthesis', foreground: '506080' },
        ],
        colors: {
            'editor.background':                  '#f0f4ff',
            'editor.foreground':                  '#1a3050',
            'editor.lineHighlightBackground':     '#e4eaf8',
            'editor.selectionBackground':         '#b0bedd60',
            'editor.inactiveSelectionBackground': '#ccd4e840',
            'editorLineNumber.foreground':        '#7090b0',
            'editorLineNumber.activeForeground':  '#b07800',
            'editorCursor.foreground':            '#b07800',
            'editorIndentGuide.background':       '#ccd4e8',
            'editorIndentGuide.activeBackground': '#b0bedd',
            'editorBracketMatch.background':      '#b0bedd40',
            'editorBracketMatch.border':          '#b0787080',
            'editorWidget.background':            '#e4eaf8',
            'editorWidget.border':                '#b0bedd',
            'editorSuggestWidget.background':     '#e4eaf8',
            'editorSuggestWidget.border':         '#b0bedd',
            'editorSuggestWidget.selectedBackground': '#ccd4e8',
            'scrollbarSlider.background':         '#b0bedd40',
            'scrollbarSlider.hoverBackground':    '#8fa4c860',
            'scrollbarSlider.activeBackground':   '#b0787040',
        }
    });

    const isLight = document.documentElement.classList.contains('light');
    monaco.editor.setTheme(isLight ? 'vtt-light' : 'vtt-dark');
};

window.setEditorTheme = (isLight) => {
    monaco.editor.setTheme(isLight ? 'vtt-light' : 'vtt-dark');
};

function cytoscapeStyle(isLight) {
    return isLight ? [
        { selector: 'node[nodeType = "entity"]', style: {
            'shape': 'round-rectangle', 'background-color': '#f0f4ff',
            'border-width': 2, 'border-color': '#b07800',
            'label': 'data(label)', 'color': '#1a3050',
            'font-family': 'JetBrains Mono, monospace', 'font-size': 13, 'font-weight': 600,
            'text-valign': 'center', 'text-halign': 'center', 'padding': 16,
            'width': 'label', 'height': 'label', 'min-width': 100, 'min-height': 40,
        }},
        { selector: 'node[nodeType = "attribute"]', style: {
            'shape': 'round-rectangle', 'background-color': '#e4eaf8',
            'border-width': 1, 'border-color': '#007888',
            'label': 'data(label)', 'color': '#007888',
            'font-family': 'JetBrains Mono, monospace', 'font-size': 11,
            'text-valign': 'center', 'text-halign': 'center', 'padding': 10,
            'width': 'label', 'height': 'label', 'min-width': 80, 'min-height': 28,
        }},
        { selector: 'edge[edgeType = "relation"]', style: {
            'width': 1.5, 'line-color': '#5040b0',
            'target-arrow-color': '#5040b0', 'target-arrow-shape': 'triangle', 'arrow-scale': 1.2,
            'curve-style': 'bezier', 'label': 'data(label)', 'font-size': 10,
            'font-family': 'IBM Plex Sans, sans-serif', 'color': '#3a5070',
            'text-wrap': 'wrap', 'text-max-width': '120px',
            'text-background-color': '#f0f4ff', 'text-background-opacity': 1,
            'text-background-padding': 4, 'text-border-opacity': 0, 'edge-text-rotation': 'none',
        }},
        { selector: 'edge[edgeType = "attribute"]', style: {
            'width': 1, 'line-color': '#b0bedd', 'target-arrow-color': '#b0bedd',
            'target-arrow-shape': 'triangle', 'curve-style': 'bezier',
            'line-style': 'dashed', 'line-dash-pattern': [5, 3],
        }},
        { selector: 'node[nodeType = "permission"]', style: {
            'shape': 'round-rectangle', 'background-color': '#f0eeff',
            'border-width': 1, 'border-color': '#8020b0', 'border-style': 'dashed',
            'label': 'data(label)', 'color': '#8020b0',
            'font-family': 'IBM Plex Sans, sans-serif', 'font-size': 10, 'font-style': 'italic',
            'text-valign': 'center', 'text-halign': 'center',
            'text-wrap': 'wrap', 'text-max-width': '180px', 'padding': 8,
            'width': 'label', 'height': 'label', 'min-width': 60, 'min-height': 20,
        }},
        { selector: 'edge[edgeType = "permission"]', style: {
            'width': 1, 'line-color': '#ccc0e8', 'target-arrow-color': '#ccc0e8',
            'target-arrow-shape': 'triangle', 'arrow-scale': 0.8,
            'curve-style': 'bezier', 'line-style': 'dashed', 'line-dash-pattern': [3, 4],
        }},
        { selector: 'node:active', style: { 'overlay-opacity': 0 } },
    ] : [
        { selector: 'node[nodeType = "entity"]', style: {
            'shape': 'round-rectangle', 'background-color': '#18203a',
            'border-width': 2, 'border-color': '#f5ae38',
            'label': 'data(label)', 'color': '#eaf2ff',
            'font-family': 'JetBrains Mono, monospace', 'font-size': 13, 'font-weight': 600,
            'text-valign': 'center', 'text-halign': 'center', 'padding': 16,
            'width': 'label', 'height': 'label', 'min-width': 100, 'min-height': 40,
        }},
        { selector: 'node[nodeType = "attribute"]', style: {
            'shape': 'round-rectangle', 'background-color': '#1a2640',
            'border-width': 1, 'border-color': '#40d0d8',
            'label': 'data(label)', 'color': '#40d0d8',
            'font-family': 'JetBrains Mono, monospace', 'font-size': 11,
            'text-valign': 'center', 'text-halign': 'center', 'padding': 10,
            'width': 'label', 'height': 'label', 'min-width': 80, 'min-height': 28,
        }},
        { selector: 'edge[edgeType = "relation"]', style: {
            'width': 1.5, 'line-color': '#b49aee',
            'target-arrow-color': '#b49aee', 'target-arrow-shape': 'triangle', 'arrow-scale': 1.2,
            'curve-style': 'bezier', 'label': 'data(label)', 'font-size': 10,
            'font-family': 'IBM Plex Sans, sans-serif', 'color': '#8aa0c0',
            'text-wrap': 'wrap', 'text-max-width': '120px',
            'text-background-color': '#101525', 'text-background-opacity': 1,
            'text-background-padding': 4, 'text-border-opacity': 0, 'edge-text-rotation': 'none',
        }},
        { selector: 'edge[edgeType = "attribute"]', style: {
            'width': 1, 'line-color': '#28365a', 'target-arrow-color': '#28365a',
            'target-arrow-shape': 'triangle', 'curve-style': 'bezier',
            'line-style': 'dashed', 'line-dash-pattern': [5, 3],
        }},
        { selector: 'node[nodeType = "permission"]', style: {
            'shape': 'round-rectangle', 'background-color': '#140e22',
            'border-width': 1, 'border-color': '#9b6dff', 'border-style': 'dashed',
            'label': 'data(label)', 'color': '#9b6dff',
            'font-family': 'IBM Plex Sans, sans-serif', 'font-size': 10, 'font-style': 'italic',
            'text-valign': 'center', 'text-halign': 'center',
            'text-wrap': 'wrap', 'text-max-width': '180px', 'padding': 8,
            'width': 'label', 'height': 'label', 'min-width': 60, 'min-height': 20,
        }},
        { selector: 'edge[edgeType = "permission"]', style: {
            'width': 1, 'line-color': '#2e1f4a', 'target-arrow-color': '#2e1f4a',
            'target-arrow-shape': 'triangle', 'arrow-scale': 0.8,
            'curve-style': 'bezier', 'line-style': 'dashed', 'line-dash-pattern': [3, 4],
        }},
        { selector: 'node:active', style: { 'overlay-opacity': 0 } },
    ];
}

function resolutionStyle(isLight) {
    return isLight ? [
        { selector: 'node[result = "pass"]', style: {
            'shape': 'round-rectangle', 'background-color': '#f0fff8',
            'border-width': 2, 'border-color': '#1a8040',
            'label': 'data(label)', 'color': '#1a8040',
            'font-family': 'JetBrains Mono, monospace', 'font-size': 12, 'font-weight': 600,
            'text-valign': 'center', 'text-halign': 'center',
            'text-wrap': 'wrap', 'text-max-width': '160px', 'padding': 12,
            'width': 'label', 'height': 'label', 'min-width': 80, 'min-height': 32,
        }},
        { selector: 'node[result = "fail"]', style: {
            'shape': 'round-rectangle', 'background-color': '#fff5f5',
            'border-width': 2, 'border-color': '#b02020',
            'label': 'data(label)', 'color': '#b02020',
            'font-family': 'JetBrains Mono, monospace', 'font-size': 12, 'font-weight': 600,
            'text-valign': 'center', 'text-halign': 'center',
            'text-wrap': 'wrap', 'text-max-width': '160px', 'padding': 12,
            'width': 'label', 'height': 'label', 'min-width': 80, 'min-height': 32,
        }},
        { selector: 'edge', style: {
            'width': 1.5, 'line-color': '#7090b0',
            'target-arrow-color': '#7090b0', 'target-arrow-shape': 'triangle',
            'arrow-scale': 1, 'curve-style': 'bezier',
        }},
        { selector: 'node:active', style: { 'overlay-opacity': 0 } },
    ] : [
        { selector: 'node[result = "pass"]', style: {
            'shape': 'round-rectangle', 'background-color': '#0d2218',
            'border-width': 2, 'border-color': '#a6e3a1',
            'label': 'data(label)', 'color': '#a6e3a1',
            'font-family': 'JetBrains Mono, monospace', 'font-size': 12, 'font-weight': 600,
            'text-valign': 'center', 'text-halign': 'center',
            'text-wrap': 'wrap', 'text-max-width': '160px', 'padding': 12,
            'width': 'label', 'height': 'label', 'min-width': 80, 'min-height': 32,
        }},
        { selector: 'node[result = "fail"]', style: {
            'shape': 'round-rectangle', 'background-color': '#220d0d',
            'border-width': 2, 'border-color': '#f38ba8',
            'label': 'data(label)', 'color': '#f38ba8',
            'font-family': 'JetBrains Mono, monospace', 'font-size': 12, 'font-weight': 600,
            'text-valign': 'center', 'text-halign': 'center',
            'text-wrap': 'wrap', 'text-max-width': '160px', 'padding': 12,
            'width': 'label', 'height': 'label', 'min-width': 80, 'min-height': 32,
        }},
        { selector: 'edge', style: {
            'width': 1.5, 'line-color': '#45475a',
            'target-arrow-color': '#45475a', 'target-arrow-shape': 'triangle',
            'arrow-scale': 1, 'curve-style': 'bezier',
        }},
        { selector: 'node:active', style: { 'overlay-opacity': 0 } },
    ];
}

window.updateGraphTheme = () => {
    const isLight = document.documentElement.classList.contains('light');
    if (_resCy) _resCy.style(resolutionStyle(isLight));
};

window.destroyEntityGraph = () => {
    if (_cy) { _cy.destroy(); _cy = null; }
};

let _cy = null;
let _cyVer = 0;

window.renderCytoscape = (containerId, elementsJson) => {
    const container = document.getElementById(containerId);
    if (!container) return;

    let elements;
    try { elements = JSON.parse(elementsJson); }
    catch (e) { container.innerHTML = '<p style="color:#ef6878;font-size:12px;padding:12px">Graph error: ' + e.message + '</p>'; return; }

    const ver = ++_cyVer;

    // Update elements in-place to avoid destroying _cy while a previous async ELK
    // layout is still running (its Promise callback would crash on the destroyed core).
    if (_cy) {
        _cy.elements().remove();
        _cy.add(elements);
    } else {
        _cy = cytoscape({
            container,
            elements,
            style: cytoscapeStyle(document.documentElement.classList.contains('light')),
            userZoomingEnabled: true,
            userPanningEnabled: true,
            boxSelectionEnabled: false,
            autoungrabify: false,
        });
    }

    _cy.one('layoutstop', () => {
        if (_cyVer !== ver) return;
        _cy.fit(undefined, 32);
        if (_cy.zoom() < 0.25) { _cy.zoom(0.25); _cy.center(); }
    });

    _cy.layout({
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
    }).run();
};

window.localStorage_set = (key, value) => localStorage.setItem(key, value);
window.localStorage_get = (key) => localStorage.getItem(key);
window.localStorage_remove = (key) => localStorage.removeItem(key);

window.getElementRect = (selector) => {
    const el = document.querySelector(selector);
    if (!el) return null;
    const r = el.getBoundingClientRect();
    return { top: r.top, left: r.left, width: r.width, height: r.height,
             viewportWidth: window.innerWidth, viewportHeight: window.innerHeight };
};

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

let _resCy = null;
let _resCyVer = 0;

window.renderResolutionGraph = (containerId, elementsJson) => {
    const container = document.getElementById(containerId);
    if (!container) return;

    let elements;
    try { elements = JSON.parse(elementsJson); }
    catch (e) {
        container.innerHTML = '<p style="color:#ef6878;font-size:12px;padding:12px">Graph error: ' + e.message + '</p>';
        return;
    }

    const ver = ++_resCyVer;

    if (_resCy) {
        _resCy.elements().remove();
        _resCy.add(elements);
    } else {
        _resCy = cytoscape({
            container,
            elements,
            style: resolutionStyle(document.documentElement.classList.contains('light')),
            userZoomingEnabled: true,
            userPanningEnabled: true,
            boxSelectionEnabled: false,
            autoungrabify: false,
        });
    }

    _resCy.one('layoutstop', () => {
        if (_resCyVer !== ver) return;
        _resCy.fit(undefined, 32);
    });

    _resCy.layout({
        name: 'elk',
        animate: false,
        elk: {
            algorithm: 'layered',
            'elk.direction': 'DOWN',
            'elk.layered.crossingMinimization.strategy': 'LAYER_SWEEP',
            'elk.layered.nodePlacement.strategy': 'BRANDES_KOEPF',
            'elk.spacing.nodeNode': '40',
            'elk.layered.spacing.nodeNodeBetweenLayers': '80',
            'elk.padding': '[top=24,left=24,bottom=24,right=24]',
        },
    }).run();
};