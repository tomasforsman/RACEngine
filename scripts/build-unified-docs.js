#!/usr/bin/env node

/**
 * Unified Documentation Builder
 *
 * Scans all documentation directories and creates one comprehensive JSON file
 * Usage: node build-unified-docs.js [--output docs.json] [--watch]
 */

const fs = require('fs');
const path = require('path');
const matter = require('gray-matter'); // npm install gray-matter
const chokidar = require('chokidar'); // npm install chokidar
const xml2js = require('xml2js'); // npm install xml2js

class UnifiedDocumentationBuilder {
    constructor(options = {}) {
        this.options = {
            outputFile: options.outputFile || './public/docs.json',
            rootDir: options.rootDir || '../',
            directories: options.directories || [
                'docs',
                'docs/docfx',
                'docs/docfx/api',
                'src',
                'tests',
                'samples'
            ],
            watchMode: options.watchMode || false,
            includeFileContents: options.includeFileContents || true,
            maxFileSize: options.maxFileSize || 1024 * 1024, // 1MB limit per file
            ...options
        };

        this.unifiedDocument = {
            metadata: {
                buildTime: new Date().toISOString(),
                version: '1.0.0',
                totalDocuments: 0,
                directories: this.options.directories
            },
            documents: {},
            crossReferences: {},
            searchIndex: {
                terms: {},
                apiCalls: {},
                breadcrumbs: []
            }
        };

        this.stats = {
            processed: 0,
            errors: 0,
            codeBlocks: 0,
            crossRefs: 0
        };
    }

    async build() {
        console.log('🔨 Building unified documentation...');
        console.log(`📁 Scanning directories: ${this.options.directories.join(', ')}`);

        try {
            // Clear previous data
            this.unifiedDocument.documents = {};
            this.unifiedDocument.crossReferences = {};
            this.stats = { processed: 0, errors: 0, codeBlocks: 0, crossRefs: 0 };

            // Scan all directories
            for (const dir of this.options.directories) {
                const fullPath = path.resolve(this.options.rootDir, dir);
                if (fs.existsSync(fullPath)) {
                    console.log(`📂 Processing ${dir}...`);
                    await this.scanDirectory(fullPath, dir);
                } else {
                    console.log(`⚠️  Directory not found: ${fullPath}`);
                }
            }

            // Build cross-references and search index
            this.buildCrossReferences();
            this.buildSearchIndex();

            // Update metadata
            this.unifiedDocument.metadata.totalDocuments = Object.keys(this.unifiedDocument.documents).length;
            this.unifiedDocument.metadata.buildTime = new Date().toISOString();

            // Write output
            await this.writeOutput();

            this.printStats();

            if (this.options.watchMode) {
                this.startWatching();
            }

        } catch (error) {
            console.error('❌ Build failed:', error);
            throw error;
        }
    }

    async scanDirectory(fullPath, relativePath) {
        const items = fs.readdirSync(fullPath);

        for (const item of items) {
            const itemPath = path.join(fullPath, item);
            const itemRelativePath = path.join(relativePath, item);

            if (fs.statSync(itemPath).isDirectory()) {
                // Skip common directories that aren't documentation
                if (!['node_modules', '.git', 'bin', 'obj', '.vs'].includes(item)) {
                    await this.scanDirectory(itemPath, itemRelativePath);
                }
            } else {
                await this.processFile(itemPath, itemRelativePath);
            }
        }
    }

    async processFile(fullPath, relativePath) {
        const ext = path.extname(relativePath).toLowerCase();

        try {
            if (ext === '.md') {
                await this.processMarkdownFile(fullPath, relativePath);
            } else if (ext === '.cs') {
                await this.processCSharpFile(fullPath, relativePath);
            } else if (ext === '.xml' && relativePath.includes('docfx')) {
                await this.processDocFxXmlFile(fullPath, relativePath);
            } else if (ext === '.yml' || ext === '.yaml') {
                await this.processYamlFile(fullPath, relativePath);
            }

            this.stats.processed++;
        } catch (error) {
            console.error(`❌ Error processing ${relativePath}:`, error.message);
            this.stats.errors++;
        }
    }

    async processMarkdownFile(fullPath, relativePath) {
        const content = fs.readFileSync(fullPath, 'utf8');
        const fileSize = Buffer.byteLength(content, 'utf8');

        if (fileSize > this.options.maxFileSize) {
            console.warn(`⚠️  File too large, skipping: ${relativePath} (${fileSize} bytes)`);
            return;
        }

        const parsed = matter(content);
        const breadcrumbKey = this.generateBreadcrumbKey(relativePath);

        // Parse markdown structure
        const sections = this.parseMarkdownSections(parsed.content);
        const codeBlocks = this.extractAllCodeBlocks(parsed.content);
        const apiReferences = this.extractApiReferences(parsed.content);
        const links = this.extractMarkdownLinks(parsed.content);

        // Build document
        const document = {
            key: breadcrumbKey,
            title: parsed.data.title || this.generateTitleFromPath(relativePath),
            type: this.determineDocumentType(relativePath, parsed.data),
            path: relativePath,
            size: fileSize,
            lastModified: fs.statSync(fullPath).mtime.toISOString(),

            // Metadata
            frontmatter: parsed.data,
            xmlComment: parsed.data.summary || parsed.data.description,
            tags: parsed.data.tags || [],

            // Content structure
            sections: sections,
            codeBlocks: codeBlocks,
            apiReferences: apiReferences,
            links: links,

            // Full content (optional, for complete text search)
            fullContent: this.options.includeFileContents ? parsed.content : null,

            // Search terms
            searchTerms: this.extractSearchTerms(parsed.content, parsed.data)
        };

        this.unifiedDocument.documents[breadcrumbKey] = document;
        this.stats.codeBlocks += codeBlocks.length;

        console.log(`✅ ${breadcrumbKey} (${sections.length} sections, ${codeBlocks.length} code blocks)`);
    }

    async processCSharpFile(fullPath, relativePath) {
        // Basic C# file processing - extract classes, methods, etc.
        const content = fs.readFileSync(fullPath, 'utf8');
        const breadcrumbKey = this.generateBreadcrumbKey(relativePath);

        // Simple regex-based extraction (could be enhanced with a proper C# parser)
        const classes = this.extractCSharpClasses(content);
        const methods = this.extractCSharpMethods(content);
        const usings = this.extractCSharpUsings(content);

        if (classes.length > 0 || methods.length > 0) {
            const document = {
                key: breadcrumbKey,
                title: this.generateTitleFromPath(relativePath),
                type: 'source',
                path: relativePath,
                size: Buffer.byteLength(content, 'utf8'),
                lastModified: fs.statSync(fullPath).mtime.toISOString(),

                frontmatter: {},
                tags: ['source', 'csharp'],

                sections: [],
                codeBlocks: [content], // Full file as code block
                apiReferences: [],
                links: [],

                // C# specific data
                classes: classes,
                methods: methods,
                usings: usings,

                searchTerms: [...classes, ...methods, ...usings]
            };

            this.unifiedDocument.documents[breadcrumbKey] = document;
            console.log(`✅ ${breadcrumbKey} (${classes.length} classes, ${methods.length} methods)`);
        }
    }

    async processDocFxXmlFile(fullPath, relativePath) {
        // Process DocFx XML documentation files
        const content = fs.readFileSync(fullPath, 'utf8');

        try {
            const parser = new xml2js.Parser();
            const result = await parser.parseStringPromise(content);

            // Extract API documentation from XML
            const apiDocs = this.extractApiDocsFromXml(result);

            for (const apiDoc of apiDocs) {
                const breadcrumbKey = `docs.docfx.api.${apiDoc.fullName.toLowerCase().replace(/\./g, '.')}`;

                const document = {
                    key: breadcrumbKey,
                    title: apiDoc.name,
                    type: 'api',
                    path: relativePath,
                    size: Buffer.byteLength(content, 'utf8'),
                    lastModified: fs.statSync(fullPath).mtime.toISOString(),

                    frontmatter: {
                        namespace: apiDoc.namespace,
                        class: apiDoc.className,
                        type: apiDoc.type
                    },
                    xmlComment: apiDoc.summary,
                    tags: ['api', 'docfx'],

                    sections: apiDoc.sections,
                    codeBlocks: apiDoc.examples,
                    apiReferences: [apiDoc.fullName],
                    links: [],

                    // API specific data
                    signature: apiDoc.signature,
                    parameters: apiDoc.parameters,
                    returnType: apiDoc.returnType,

                    searchTerms: [apiDoc.name, apiDoc.fullName, apiDoc.namespace, apiDoc.className]
                };

                this.unifiedDocument.documents[breadcrumbKey] = document;
                console.log(`✅ ${breadcrumbKey} (API: ${apiDoc.fullName})`);
            }
        } catch (error) {
            console.error(`❌ Failed to parse XML: ${relativePath}`, error.message);
        }
    }

    async processYamlFile(fullPath, relativePath) {
        // Process YAML files (often used by DocFx)
        // This would need a YAML parser like js-yaml
        console.log(`📄 YAML file found: ${relativePath} (processing not implemented yet)`);
    }

    parseMarkdownSections(content) {
        const sections = [];
        const lines = content.split('\n');
        let currentSection = null;

        for (let i = 0; i < lines.length; i++) {
            const line = lines[i];

            // Check for headings
            const headingMatch = line.match(/^(#{1,6})\s+(.+)$/);
            if (headingMatch) {
                // Save previous section
                if (currentSection) {
                    currentSection.content = currentSection.content.trim();
                    sections.push(currentSection);
                }

                // Start new section
                currentSection = {
                    id: this.generateSectionId(headingMatch[2]),
                    heading: headingMatch[2],
                    level: headingMatch[1].length,
                    fullHeading: line,
                    content: '',
                    codeBlocks: [],
                    apiReferences: [],
                    lineStart: i + 1
                };
            } else if (currentSection) {
                // Add content to current section
                if (line.trim().startsWith('```')) {
                    // Handle code blocks
                    const codeBlock = this.extractCodeBlockFromLines(lines, i);
                    if (codeBlock) {
                        currentSection.codeBlocks.push(codeBlock.code);
                        // Extract API references from code
                        currentSection.apiReferences.push(...this.extractApiReferences(codeBlock.code));
                        i = codeBlock.endIndex;
                    }
                } else {
                    currentSection.content += line + '\n';
                }
            }
        }

        // Don't forget the last section
        if (currentSection) {
            currentSection.content = currentSection.content.trim();
            sections.push(currentSection);
        }

        return sections;
    }

    extractAllCodeBlocks(content) {
        const codeBlocks = [];
        const lines = content.split('\n');

        for (let i = 0; i < lines.length; i++) {
            if (lines[i].trim().startsWith('```')) {
                const codeBlock = this.extractCodeBlockFromLines(lines, i);
                if (codeBlock) {
                    codeBlocks.push({
                        language: codeBlock.language,
                        code: codeBlock.code,
                        lineStart: i + 1,
                        lineEnd: codeBlock.endIndex + 1
                    });
                    i = codeBlock.endIndex;
                }
            }
        }

        return codeBlocks;
    }

    extractCodeBlockFromLines(lines, startIndex) {
        const startLine = lines[startIndex].trim();
        const language = startLine.substring(3).trim() || 'text';
        const codeLines = [];
        let i = startIndex + 1;

        while (i < lines.length && !lines[i].trim().startsWith('```')) {
            codeLines.push(lines[i]);
            i++;
        }

        if (i < lines.length) {
            return {
                language: language,
                code: codeLines.join('\n'),
                endIndex: i
            };
        }

        return null;
    }

    extractApiReferences(content) {
        const apiRefs = new Set();

        // Extract API calls like Rac.Rendering.IRenderer.SetColor
        const apiCallPattern = /\b[A-Z]\w*(?:\.[A-Z]\w*)+/g;
        const matches = content.match(apiCallPattern) || [];
        matches.forEach(match => apiRefs.add(match));

        // Extract method calls with parentheses
        const methodCallPattern = /\b\w+\.\w+(?:\.\w+)*\(/g;
        const methodMatches = content.match(methodCallPattern) || [];
        methodMatches.forEach(match => apiRefs.add(match.replace('(', '')));

        return Array.from(apiRefs);
    }

    extractMarkdownLinks(content) {
        const links = [];

        // Extract markdown links [text](url)
        const linkPattern = /\[([^\]]+)\]\(([^)]+)\)/g;
        let match;

        while ((match = linkPattern.exec(content)) !== null) {
            links.push({
                text: match[1],
                url: match[2],
                type: this.determineLinkType(match[2])
            });
        }

        return links;
    }

    extractCSharpClasses(content) {
        const classes = [];
        const classPattern = /(?:public|internal|private|protected)?\s*(?:static|abstract|sealed)?\s*class\s+(\w+)/g;
        let match;

        while ((match = classPattern.exec(content)) !== null) {
            classes.push(match[1]);
        }

        return classes;
    }

    extractCSharpMethods(content) {
        const methods = [];
        const methodPattern = /(?:public|internal|private|protected)?\s*(?:static|virtual|override|abstract)?\s*\w+\s+(\w+)\s*\(/g;
        let match;

        while ((match = methodPattern.exec(content)) !== null) {
            // Filter out constructors and common keywords
            if (!['if', 'for', 'while', 'using', 'return'].includes(match[1])) {
                methods.push(match[1]);
            }
        }

        return [...new Set(methods)]; // Remove duplicates
    }

    extractCSharpUsings(content) {
        const usings = [];
        const usingPattern = /using\s+([^;]+);/g;
        let match;

        while ((match = usingPattern.exec(content)) !== null) {
            usings.push(match[1].trim());
        }

        return usings;
    }

    extractApiDocsFromXml(xmlData) {
        // This would parse DocFx XML structure
        // Implementation depends on your specific DocFx XML format
        return [];
    }

    buildCrossReferences() {
        console.log('🔗 Building cross-references...');

        const crossRefs = {};

        // Find all API references across documents
        Object.values(this.unifiedDocument.documents).forEach(doc => {
            doc.apiReferences.forEach(apiRef => {
                if (!crossRefs[apiRef]) {
                    crossRefs[apiRef] = {
                        definition: null,
                        references: []
                    };
                }

                crossRefs[apiRef].references.push({
                    documentKey: doc.key,
                    documentTitle: doc.title,
                    context: 'usage'
                });
            });

            // Check if this document IS the definition of an API
            if (doc.type === 'api' || doc.type === 'source') {
                const apiName = this.extractApiNameFromDocument(doc);
                if (apiName && crossRefs[apiName]) {
                    crossRefs[apiName].definition = {
                        documentKey: doc.key,
                        documentTitle: doc.title
                    };
                }
            }
        });

        this.unifiedDocument.crossReferences = crossRefs;
        this.stats.crossRefs = Object.keys(crossRefs).length;
    }

    buildSearchIndex() {
        console.log('🔍 Building search index...');

        const termIndex = {};
        const breadcrumbs = [];

        Object.values(this.unifiedDocument.documents).forEach(doc => {
            try {
                // Add breadcrumb
                breadcrumbs.push(doc.key);

                // Ensure searchTerms is an array
                const searchTerms = Array.isArray(doc.searchTerms) ? doc.searchTerms : [];

                // Index search terms
                searchTerms.forEach(term => {
                    if (term && typeof term === 'string') {
                        const termLower = term.toLowerCase().trim();
                        if (termLower.length > 0) {
                            this.addToTermIndex(termIndex, termLower, doc.key, this.calculateTermScore(term, doc));
                        }
                    }
                });

                // Index title words
                if (doc.title && typeof doc.title === 'string') {
                    doc.title.split(/\s+/).forEach(word => {
                        const wordLower = word.toLowerCase().trim();
                        if (wordLower.length > 2 && /^[a-zA-Z0-9]+$/.test(wordLower)) {
                            this.addToTermIndex(termIndex, wordLower, doc.key, 50);
                        }
                    });
                }

                // Index API references
                if (doc.apiReferences && Array.isArray(doc.apiReferences)) {
                    doc.apiReferences.forEach(apiRef => {
                        if (apiRef && typeof apiRef === 'string') {
                            const apiLower = apiRef.toLowerCase().trim();
                            if (apiLower.length > 0) {
                                this.addToTermIndex(termIndex, apiLower, doc.key, 40);

                                // Also index parts of the API reference
                                const parts = apiRef.split('.');
                                parts.forEach(part => {
                                    const partLower = part.toLowerCase().trim();
                                    if (partLower.length > 2) {
                                        this.addToTermIndex(termIndex, partLower, doc.key, 30);
                                    }
                                });
                            }
                        }
                    });
                }

            } catch (error) {
                console.error(`❌ Error indexing document ${doc.key}:`, error.message);
            }
        });

        // Sort term references by score and remove duplicates
        Object.keys(termIndex).forEach(term => {
            if (Array.isArray(termIndex[term])) {
                // Remove duplicate document references
                const seen = new Set();
                termIndex[term] = termIndex[term].filter(item => {
                    const key = item.documentKey;
                    if (seen.has(key)) {
                        return false;
                    }
                    seen.add(key);
                    return true;
                });

                // Sort by score
                termIndex[term].sort((a, b) => b.score - a.score);
            } else {
                console.warn(`⚠️  Term index for '${term}' is not an array:`, termIndex[term]);
                termIndex[term] = [];
            }
        });

        this.unifiedDocument.searchIndex = {
            terms: termIndex,
            breadcrumbs: breadcrumbs.sort()
        };

        console.log(`✅ Search index built: ${Object.keys(termIndex).length} terms, ${breadcrumbs.length} breadcrumbs`);
    }

    addToTermIndex(termIndex, term, documentKey, score) {
        if (!termIndex[term]) {
            termIndex[term] = [];
        }

        // Ensure it's an array
        if (!Array.isArray(termIndex[term])) {
            console.warn(`⚠️  Fixing non-array term index for '${term}'`);
            termIndex[term] = [];
        }

        termIndex[term].push({
            documentKey: documentKey,
            score: score
        });
    }

    calculateTermScore(term, doc) {
        let score = 10; // Base score

        if (doc.type === 'api') score += 30;
        if (doc.title.toLowerCase().includes(term.toLowerCase())) score += 20;
        if (doc.tags.includes(term.toLowerCase())) score += 15;

        return score;
    }

    extractApiNameFromDocument(doc) {
        // Extract the main API name this document defines
        if (doc.type === 'api' && doc.frontmatter.class) {
            return `${doc.frontmatter.namespace}.${doc.frontmatter.class}`;
        }
        return null;
    }

    generateBreadcrumbKey(filePath) {
        return filePath
            .replace(/\\/g, '/')
            .replace(/\.md$|\.cs$|\.xml$/, '')
            .replace(/\//g, '.')
            .toLowerCase();
    }

    generateSectionId(headingText) {
        return headingText
            .toLowerCase()
            .replace(/[^a-z0-9\s-]/g, '')
            .replace(/\s+/g, '-')
            .trim();
    }

    generateTitleFromPath(filePath) {
        const basename = path.basename(filePath).replace(/\.(md|cs|xml)$/, '');
        return basename
            .split(/[-_]/)
            .map(word => word.charAt(0).toUpperCase() + word.slice(1))
            .join(' ');
    }

    determineDocumentType(filePath, frontmatter) {
        if (frontmatter.type) return frontmatter.type;
        if (filePath.includes('/api/')) return 'api';
        if (filePath.includes('/samples/')) return 'sample';
        if (filePath.includes('/codesamples/')) return 'codesample';
        if (filePath.includes('/tutorials/')) return 'tutorial';
        if (filePath.endsWith('.cs')) return 'source';
        return 'documentation';
    }

    determineLinkType(url) {
        if (url.startsWith('http')) return 'external';
        if (url.startsWith('#')) return 'anchor';
        if (url.endsWith('.md')) return 'document';
        return 'other';
    }

    extractSearchTerms(content, frontmatter) {
        const terms = new Set();

        // Add frontmatter terms
        if (frontmatter.tags) {
            frontmatter.tags.forEach(tag => terms.add(tag));
        }
        if (frontmatter.keywords) {
            frontmatter.keywords.forEach(keyword => terms.add(keyword));
        }

        // Extract API references
        this.extractApiReferences(content).forEach(ref => terms.add(ref));

        // Extract important words (simple approach)
        const words = content.match(/\b[A-Z][a-z]+\b/g) || [];
        words.forEach(word => {
            if (word.length > 3) terms.add(word);
        });

        return Array.from(terms);
    }

    async writeOutput() {
        const outputDir = path.dirname(this.options.outputFile);
        if (!fs.existsSync(outputDir)) {
            fs.mkdirSync(outputDir, { recursive: true });
        }

        const jsonOutput = JSON.stringify(this.unifiedDocument, null, 2);
        fs.writeFileSync(this.options.outputFile, jsonOutput);

        const sizeKB = Math.round(Buffer.byteLength(jsonOutput, 'utf8') / 1024);
        console.log(`💾 Written to ${this.options.outputFile} (${sizeKB} KB)`);
    }

    printStats() {
        console.log('\n📊 Build Statistics:');
        console.log(`   📄 Documents processed: ${this.stats.processed}`);
        console.log(`   ❌ Errors: ${this.stats.errors}`);
        console.log(`   📋 Total documents: ${Object.keys(this.unifiedDocument.documents).length}`);
        console.log(`   💻 Code blocks: ${this.stats.codeBlocks}`);
        console.log(`   🔗 Cross-references: ${this.stats.crossRefs}`);
        console.log(`   🔍 Search terms: ${Object.keys(this.unifiedDocument.searchIndex.terms).length}`);
    }

    startWatching() {
        console.log('\n👀 Watching for changes...');

        const watchPaths = this.options.directories.map(dir =>
            path.resolve(this.options.rootDir, dir, '**/*.{md,cs,xml,yml,yaml}')
        );

        const watcher = chokidar.watch(watchPaths, {
            ignoreInitial: true,
            ignored: /(^|[\/\\])\../ // Ignore dotfiles
        });

        let buildTimeout;
        const debouncedBuild = () => {
            clearTimeout(buildTimeout);
            buildTimeout = setTimeout(() => {
                console.log('\n🔄 Files changed, rebuilding...');
                this.build().catch(console.error);
            }, 500);
        };

        watcher
            .on('add', debouncedBuild)
            .on('change', debouncedBuild)
            .on('unlink', debouncedBuild);
    }
}

// CLI interface
if (require.main === module) {
    const args = process.argv.slice(2);
    const options = {};

    for (let i = 0; i < args.length; i++) {
        switch (args[i]) {
            case '--output':
                options.outputFile = args[++i];
                break;
            case '--watch':
                options.watchMode = true;
                break;
            case '--no-content':
                options.includeFileContents = false;
                break;
            case '--root':
                options.rootDir = args[++i];
                break;
        }
    }

    const builder = new UnifiedDocumentationBuilder(options);
    builder.build().catch(error => {
        console.error('❌ Build failed:', error);
        process.exit(1);
    });
}

module.exports = UnifiedDocumentationBuilder;