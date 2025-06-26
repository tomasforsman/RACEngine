# Greenfield Product Code Evaluation Process
*For new features and evolving products*

## Phase 1: Foundation and Future-Proofing

### Step 1: Product Vision Alignment
- [ ] Verify code aligns with long-term product roadmap and vision
- [ ] Check that feature implementation supports anticipated future requirements
- [ ] Ensure architectural decisions don't paint the product into a corner
- [ ] Validate that the code enables rather than constrains product evolution
- [ ] Review feature flags and toggles for gradual rollout capability
- [ ] Confirm the implementation supports A/B testing and experimentation

### Step 2: Architectural Flexibility Assessment
- [ ] Evaluate code for extensibility and future feature additions
- [ ] Check for over-engineering vs. under-engineering balance
- [ ] Verify modular design that allows independent feature development
- [ ] Assess API design for backward compatibility and versioning
- [ ] Review data models for schema evolution capabilities
- [ ] Ensure configuration management supports different environments and feature sets
- [ ] Validate that core abstractions are stable but implementation details are flexible

### Step 3: Development Velocity Impact
- [ ] Assess whether code accelerates or slows future development
- [ ] Check for reusable components and shared utilities
- [ ] Verify consistent patterns that new team members can follow
- [ ] Review development tooling and automation setup
- [ ] Ensure testing infrastructure supports rapid iteration
- [ ] Validate that debugging and troubleshooting are straightforward
- [ ] Check for proper separation of concerns to enable parallel development

## Phase 2: Code Quality for Growth

### Step 4: Clean Code and Maintainability
- [ ] Verify code is self-documenting with clear intent
- [ ] Check for appropriate abstraction levels (not too abstract, not too concrete)
- [ ] Ensure functions and classes have single, clear responsibilities
- [ ] Review naming conventions for consistency and clarity
- [ ] Validate that complex business logic is well-encapsulated
- [ ] Check for minimal coupling between components
- [ ] Ensure code complexity remains manageable as features are added

### Step 5: Technical Debt Prevention
- [ ] Identify potential technical debt early and document decisions
- [ ] Check for proper handling of temporary solutions and TODOs
- [ ] Verify that quick fixes don't compromise long-term architecture
- [ ] Review code for duplication that should be refactored
- [ ] Ensure performance optimizations don't sacrifice code clarity unnecessarily
- [ ] Validate that external dependencies are carefully chosen and abstracted
- [ ] Check for proper error handling that won't mask issues during development

## Phase 3: Scalability and Performance Foundation

### Step 6: Performance Baseline and Monitoring
- [ ] Establish performance baselines for key user journeys
- [ ] Implement performance monitoring from the start
- [ ] Check for obvious performance bottlenecks in new code
- [ ] Verify database queries are efficient and properly indexed
- [ ] Review caching strategies for data that will grow
- [ ] Ensure APIs are designed for efficient data transfer
- [ ] Test performance with realistic data volumes (not just test data)

### Step 7: Scalability Readiness
- [ ] Verify stateless design where appropriate
- [ ] Check for proper handling of concurrent operations
- [ ] Review data access patterns for future scaling needs
- [ ] Ensure external service integrations are resilient and cacheable
- [ ] Validate that background processing can handle increased load
- [ ] Check for proper resource cleanup and memory management
- [ ] Review pagination and bulk operation strategies

## Phase 4: Security and Data Protection

### Step 8: Security-by-Design
- [ ] Verify input validation and sanitization at all boundaries
- [ ] Check for proper authentication and authorization patterns
- [ ] Review data handling for privacy compliance (GDPR, CCPA, etc.)
- [ ] Ensure secure defaults in configuration and permissions
- [ ] Validate that sensitive data is properly encrypted and handled
- [ ] Check for secure communication between services
- [ ] Review audit logging for security-relevant events

### Step 9: Data Integrity and Privacy
- [ ] Verify data validation rules are comprehensive and future-proof
- [ ] Check for proper handling of user data deletion and updates
- [ ] Review data retention and archival strategies
- [ ] Ensure data migration paths are planned and tested
- [ ] Validate that data consistency is maintained across operations
- [ ] Check for proper handling of data relationships and constraints

## Phase 5: Testing and Quality Assurance

### Step 10: Test Strategy for Evolving Features
- [ ] Verify comprehensive unit tests for core business logic
- [ ] Check that tests are maintainable and don't break easily with changes
- [ ] Ensure integration tests cover critical user flows
- [ ] Review test data management and factory patterns
- [ ] Validate that tests run quickly to support rapid development
- [ ] Check for proper mocking and stubbing of external dependencies
- [ ] Ensure tests document expected behavior clearly

### Step 11: Quality Gates and Automation
- [ ] Set up automated code quality checks (linting, formatting, complexity)
- [ ] Implement automated security scanning
- [ ] Configure automated performance regression testing
- [ ] Set up continuous integration with appropriate quality gates
- [ ] Ensure code coverage metrics are tracked and maintained
- [ ] Validate that quality checks don't slow down development unnecessarily

## Phase 6: Development Experience and Team Enablement

### Step 12: Developer Experience
- [ ] Ensure local development setup is straightforward and documented
- [ ] Check that debugging and troubleshooting are efficient
- [ ] Verify that common development tasks are automated
- [ ] Review logging and error messages for developer-friendliness
- [ ] Ensure development environment mirrors production sufficiently
- [ ] Check that new team members can contribute quickly
- [ ] Validate that development tools and IDE integration work well

### Step 13: Documentation and Knowledge Sharing
- [ ] Document architectural decisions and trade-offs (ADRs)
- [ ] Ensure API documentation is complete and up-to-date
- [ ] Document setup and deployment procedures
- [ ] Create troubleshooting guides for common issues
- [ ] Document coding standards and patterns used in the project
- [ ] Ensure business logic and domain concepts are well-documented
- [ ] Set up knowledge sharing processes for the team

## Phase 7: Deployment and Operational Readiness

### Step 14: Deployment Strategy
- [ ] Implement feature flags for safe rollouts
- [ ] Set up staging environments that mirror production
- [ ] Ensure deployment process is automated and repeatable
- [ ] Implement proper rollback procedures
- [ ] Set up monitoring and alerting for new features
- [ ] Plan for gradual rollout and user feedback collection
- [ ] Ensure zero-downtime deployment capabilities

### Step 15: Monitoring and Observability
- [ ] Implement structured logging with correlation IDs
- [ ] Set up metrics for key business and technical indicators
- [ ] Configure alerting for critical failures and performance issues
- [ ] Implement health checks and status endpoints
- [ ] Set up distributed tracing for complex operations
- [ ] Ensure monitoring data helps with debugging and optimization
- [ ] Plan for monitoring evolution as the product grows

## Phase 8: Product and Business Validation

### Step 16: User Experience and Feedback
- [ ] Verify that new features provide clear user value
- [ ] Ensure error handling provides helpful user feedback
- [ ] Check that performance meets user expectations
- [ ] Validate accessibility requirements are met
- [ ] Ensure analytics are in place to measure feature success
- [ ] Plan for user feedback collection and incorporation
- [ ] Verify that features work across different user segments

### Step 17: Business Metrics and Success Criteria
- [ ] Define and implement tracking for key business metrics
- [ ] Ensure features support business model requirements
- [ ] Validate that features can be measured for success/failure
- [ ] Check that features support different pricing tiers or plans
- [ ] Ensure compliance with business and legal requirements
- [ ] Plan for feature iteration based on user data

---

## Evaluation Scoring Framework for Greenfield Products

**Critical Issues (Must Fix Before Merge):**
- Breaks existing functionality
- Creates significant technical debt
- Blocks future development
- Security vulnerabilities
- Major performance regressions

**Major Issues (Should Fix Soon):**
- Inconsistent with established patterns
- Missing important tests
- Poor documentation
- Performance inefficiencies
- Difficult to extend or modify

**Minor Issues (Address in Future Iterations):**
- Code style inconsistencies
- Missing edge case handling
- Optimization opportunities
- Enhanced error messages
- Additional test coverage

**Success Indicators:**
- Accelerates future development
- Follows established patterns
- Well-tested and documented
- Provides clear user value
- Enables product evolution

---

## Decision Framework for Greenfield Code

**When to be flexible:** Early features, experimental functionality, rapid prototyping
**When to be strict:** Core infrastructure, shared utilities, user-facing APIs
**When to refactor:** When patterns emerge, when complexity increases, when performance matters
**When to accept debt:** Time-critical features, proof-of-concept work, with clear paydown plan