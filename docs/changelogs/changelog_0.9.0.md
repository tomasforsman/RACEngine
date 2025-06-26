---
title: "Changelog 0.9.0"
description: "Hierarchical Transform System - Complete transform hierarchy implementation with parent-child relationships."
version: "0.9.0"
last_updated: "2025-06-26"
author: "Tomas Forsman"
---

# Changelog 0.9.0 - Hierarchical Transform System

Released: June 26, 2025

This release introduces a complete hierarchical transform system, enabling parent-child relationships between entities with proper transform inheritance. This is a foundational feature for complex scene management and object hierarchies.

## Added Features

### Core Transform System
* **Hierarchical Transforms**: Full parent-child transform relationships with inheritance
* **Transform Components**: `Transform`, `Parent`, and `Children` components for hierarchy management
* **Extension Methods**: Convenient entity hierarchy manipulation methods
* **Automatic Updates**: Proper transform propagation through hierarchy chains

### Entity Hierarchy Management
* **Parent-Child Relationships**: Establish and manage entity relationships
* **Transform Inheritance**: Child transforms automatically inherit parent transformations
* **Hierarchy Queries**: Methods to traverse and query entity hierarchies
* **Bulk Operations**: Efficient operations on entire hierarchy branches

## Improvements

### Architecture Changes
* **Component Design**: Clean component architecture for transform hierarchies
* **Performance**: Efficient transform propagation algorithms
* **Memory Management**: Optimized storage for hierarchy relationships
* **API Design**: Intuitive API for common hierarchy operations

### Sample Updates
* **Migration**: All sample applications updated to use new transform system
* **Demonstrations**: Examples showing hierarchy usage patterns
* **Documentation**: Comprehensive docs for transform system usage

## Breaking Changes

### Removed Features
* **Backwards Compatibility**: Old transform system completely removed
* **Legacy APIs**: Previous transform-related methods no longer available
* **Migration Required**: Existing code must be updated to use new system

### Migration Path
The old transform system has been completely replaced. Applications using transforms need to:
1. Update to use new `Transform`, `Parent`, and `Children` components
2. Use entity hierarchy extension methods for parent-child relationships
3. Leverage automatic transform inheritance instead of manual calculations

## Technical Details

### Component Architecture
- `Transform`: Position, rotation, and scale data
- `Parent`: Reference to parent entity in hierarchy
- `Children`: Collection of child entities
- Efficient storage and query patterns for hierarchy traversal

### Performance Characteristics
- O(1) parent-child relationship establishment
- Efficient batch updates for hierarchy changes
- Minimal memory overhead for transform storage
- Optimized transform propagation algorithms

## Educational Impact

This release teaches important concepts:
- Scene graph architecture and implementation
- Transform mathematics and coordinate spaces
- Efficient hierarchy traversal algorithms
- Component composition patterns in ECS systems

## Bug Fixes

No specific bugs were fixed in this release, as it represents a complete architectural improvement rather than bug resolution.

## Commits Included

- `1110176`: Remove backwards compatibility and update samples to use new transform system
- `1a45a4e`: Add Entity hierarchy extension methods and comprehensive documentation
- `1a50f6d`: Implement core hierarchical transform system with components and tests