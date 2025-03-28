

window.init_vtt = () => {
    const valtuutus = 'valtuutus';
    monaco.languages.register({id: valtuutus})
    const syntax = {
        defaultToken: 'invalid',

        keywords: ['entity', 'relation', 'attribute', 'permission', 'fn'],
        types: ['int', 'bool', 'string', 'decimal'],
        logicalOperators: ['and', 'or'],
        operators: ['=', '->', ':', ';', '.', '{', '}'],

        brackets: [
            { open: '{', close: '}', token: 'delimiter.curly' },
            { open: '(', close: ')', token: 'delimiter.parenthesis' }
        ],

        tokenizer: {
            root: [
                [/[a-zA-Z_][a-zA-Z0-9_]*/, {
                    cases: {
                        '@keywords': 'keyword',
                        '@types': 'type',
                        '@logicalOperators': 'operator',
                        '@default': 'identifier'
                    }
                }],

                [/@[a-zA-Z_][a-zA-Z0-9_]*/, 'annotation'], // Handling annotations
                [/[0-9]+\.[0-9]+/, 'number.float'], // Decimal numbers
                [/[0-9]+/, 'number'], // Integers
                [/".*?"/, 'string'],
                [/\->|[:;.=]/, 'operator'],
                [/[{}()]/, '@brackets'],

                [/\/\/.*$/, 'comment'], // Handle `//` comments only
                [/\s+/, 'white'] // Whitespace handling
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
                ...syntax.keywords.map(k => {
                    return {
                        label: k,
                        kind: monaco.languages.CompletionItemKind.Keyword,
                        insertText: k,
                    }
                }),
            ];
            return {suggestions: suggestions};
        }
    })

};