#!/usr/bin/env node

/**
 * Documentation Validation Script
 * Validates the generated docs.json file
 */

const fs = require('fs');
const path = require('path');

class DocumentationValidator {
    constructor(docsFile = './public/docs.json') {
        this.docsFile = docsFile;
        this.errors = [];
        this.warnings = [];
    }

    validate() {
        console.log('🔍 Validating documentation...');

        try {
            // Check if file exists
            if (!fs.existsSync(this.docsFile)) {
                this.errors.push(`Documentation file not found: ${this.docsFile}`);
                return this.showResults();
            }

            // Parse JSON
            const content = fs.readFileSync(this.docsFile, 'utf8');
            let docs;

            try {
                docs = JSON.parse(content);
            } catch (parseError) {
                this.errors.push(`Invalid JSON: ${parseError.message}`);
                return this.showResults();
            }

            // Validate structure
            this.validateStructure(docs);
            this.validateMetadata(docs.metadata);
            this.validateDocuments(docs.documents);
            this.validateCrossReferences(docs.crossReferences);
            this.validateSearchIndex(docs.searchIndex);

            return this.showResults();

        } catch (error) {
            this.errors.push(`Validation failed: ${error.message}`);
            return this.showResults();
        }
    }

    validateStructure(docs) {
        const requiredFields = ['metadata', 'documents', 'crossReferences', 'searchIndex'];

        for (const field of requiredFields) {
            if (!(field in docs)) {
                this.errors.push(`Missing required field: ${field}`);
            }
        }
    }

    validateMetadata(metadata) {
        if (!metadata) {
            this.errors.push('Missing metadata section');
            return;
        }

        const requiredFields = ['buildTime', 'version', 'totalDocuments', 'directories'];

        for (const field of requiredFields) {
            if (!(field in metadata)) {
                this.errors.push(`Missing metadata field: ${field}`);
            }
        }

        // Check document count
        if (metadata.totalDocuments === 0) {
            this.warnings.push('No documents found - this might indicate a scanning problem');
        } else if (metadata.totalDocuments < 5) {
            this.warnings.push(`Only ${metadata.totalDocuments} documents found - expected more for a game engine`);
        }

        // Check build time
        const buildTime = new Date(metadata.buildTime);
        const now = new Date();
        const ageHours = (now - buildTime) / (1000 * 60 * 60);

        if (ageHours > 24) {
            this.warnings.push(`Documentation is ${Math.round(ageHours)} hours old`);
        }
    }

    validateDocuments(documents) {
        if (!documents || typeof documents !== 'object') {
            this.errors.push('Documents section is missing or invalid');
            return;
        }

        const documentKeys = Object.keys(documents);

        if (documentKeys.length === 0) {
            this.errors.push('No documents found');
            return;
        }

        // Track document types
        const typeCount = {};
        const pathCount = {};

        // Validate each document
        for (const [key, doc] of Object.entries(documents)) {
            this.validateDocument(key, doc);

            // Count types
            typeCount[doc.type] = (typeCount[doc.type] || 0) + 1;

            // Check for duplicate paths
            if (pathCount[doc.path]) {
                this.errors.push(`Duplicate document path: ${doc.path}`);
            }
            pathCount[doc.path] = true;
        }

        // Check for expected document types
        if (!typeCount.api) {
            this.warnings.push('No API documentation found');
        }
        if (!typeCount.sample && !typeCount.codesample) {
            this.warnings.push('No code samples found');
        }
        if (!typeCount.tutorial) {
            this.warnings.push('No tutorials found');
        }

        console.log('📊 Document type breakdown:');
        for (const [type, count] of Object.entries(typeCount)) {
            console.log(`   ${type}: ${count}`);
        }
    }

    validateDocument(key, doc) {
        const requiredFields = ['key', 'title', 'type', 'path', 'sections'];

        for (const field of requiredFields) {
            if (!(field in doc)) {
                this.errors.push(`Document ${key} missing field: ${field}`);
            }
        }

        // Validate key format
        if (key !== doc.key) {
            this.errors.push(`Document key mismatch: ${key} vs ${doc.key}`);
        }

        // Check breadcrumb format
        if (!key.match(/^[a-z0-9.-]+$/)) {
            this.warnings.push(`Document key format questionable: ${key}`);
        }

        // Validate sections
        if (doc.sections && Array.isArray(doc.sections)) {
            for (const section of doc.sections) {
                if (!section.id || !section.heading) {
                    this.warnings.push(`Document ${key} has section missing id or heading`);
                }
            }
        }

        // Check for content
        if (!doc.sections || doc.sections.length === 0) {
            this.warnings.push(`Document ${key} has no sections`);
        }
    }

    validateCrossReferences(crossRefs) {
        if (!crossRefs || typeof crossRefs !== 'object') {
            this.warnings.push('Cross-references section is missing or invalid');
            return;
        }

        let totalRefs = 0;
        for (const [apiName, refData] of Object.entries(crossRefs)) {
            if (!refData.references || !Array.isArray(refData.references)) {
                this.warnings.push(`Cross-reference ${apiName} has no references array`);
                continue;
            }
            totalRefs += refData.references.length;
        }

        if (totalRefs === 0) {
            this.warnings.push('No cross-references found - API usage tracking may not be working');
        }

        console.log(`🔗 Found ${Object.keys(crossRefs).length} cross-referenced APIs with ${totalRefs} total references`);
    }

    validateSearchIndex(searchIndex) {
        if (!searchIndex || typeof searchIndex !== 'object') {
            this.errors.push('Search index is missing or invalid');
            return;
        }

        if (!searchIndex.terms || typeof searchIndex.terms !== 'object') {
            this.errors.push('Search index terms are missing or invalid');
            return;
        }

        if (!searchIndex.breadcrumbs || !Array.isArray(searchIndex.breadcrumbs)) {
            this.errors.push('Search index breadcrumbs are missing or invalid');
            return;
        }

        const termCount = Object.keys(searchIndex.terms).length;
        const breadcrumbCount = searchIndex.breadcrumbs.length;

        if (termCount === 0) {
            this.errors.push('No search terms found');
        }

        if (breadcrumbCount === 0) {
            this.errors.push('No breadcrumbs found');
        }

        console.log(`🔍 Search index: ${termCount} terms, ${breadcrumbCount} breadcrumbs`);
    }

    showResults() {
        console.log('\n📋 Validation Results:');

        if (this.errors.length === 0 && this.warnings.length === 0) {
            console.log('✅ Documentation validation passed!');
            return true;
        }

        if (this.errors.length > 0) {
            console.log(`\n❌ Errors (${this.errors.length}):`);
            this.errors.forEach(error => console.log(`   • ${error}`));
        }

        if (this.warnings.length > 0) {
            console.log(`\n⚠️  Warnings (${this.warnings.length}):`);
            this.warnings.forEach(warning => console.log(`   • ${warning}`));
        }

        // Exit with error code if there are errors
        if (this.errors.length > 0) {
            console.log('\n💥 Validation failed due to errors');
            process.exit(1);
        }

        console.log('\n✅ Validation passed with warnings');
        return true;
    }
}

// CLI usage
if (require.main === module) {
    const args = process.argv.slice(2);
    const docsFile = args[0] || './public/docs.json';

    const validator = new DocumentationValidator(docsFile);
    validator.validate();
}

module.exports = DocumentationValidator;