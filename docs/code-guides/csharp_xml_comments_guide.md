# C# XML Documentation Standards Guide

## Overview

XML documentation comments in C# use the triple-slash (`///`) syntax and provide IntelliSense support, API documentation generation, and improved code maintainability. This guide establishes different documentation standards based on element visibility and type.

## General Syntax

```csharp
/// <summary>
/// Brief description of the element
/// </summary>
/// <param name="parameterName">Description of parameter</param>
/// <returns>Description of return value</returns>
public void ExampleMethod(string parameterName)
{
    // Implementation
}
```

## Documentation Standards by Visibility

### Public Elements (Required Documentation)

All public elements **must** have comprehensive XML documentation including:

- `<summary>` - Clear, concise description
- `<param>` - For each parameter (methods/constructors)
- `<returns>` - For non-void methods
- `<exception>` - For documented exceptions
- `<example>` - For complex or non-obvious usage
- `<remarks>` - Additional important information when needed

### Internal Elements (Recommended Documentation)

Internal elements **should** have documentation, especially:

- `<summary>` - Brief description
- `<param>` - For complex parameters
- `<returns>` - For non-obvious return values

### Protected Elements (Recommended Documentation)

Protected elements **should** be documented since they're part of inheritance contracts:

- `<summary>` - Description focusing on inheritance behavior
- `<param>` - Parameter descriptions
- `<returns>` - Return value descriptions

### Private Elements (Minimal Documentation)

Private elements may have minimal documentation:

- `<summary>` - Only for complex private methods
- Focus on **why** rather than **what**
- Use regular comments (`//`) for simple explanations

## Documentation Standards by Element Type

### Classes and Interfaces

```csharp
/// <summary>
/// Represents a customer in the e-commerce system.
/// Provides functionality for managing customer data and order history.
/// </summary>
/// <remarks>
/// This class implements thread-safe operations for concurrent access.
/// Customer data is validated according to business rules defined in CustomerValidator.
/// </remarks>
/// <example>
/// <code>
/// var customer = new Customer("John", "Doe", "john.doe@example.com");
/// customer.AddAddress(new Address("123 Main St", "Anytown", "12345"));
/// </code>
/// </example>
public class Customer : ICustomer
{
    // Implementation
}
```

**Required for public classes/interfaces:**
- `<summary>` - Purpose and main functionality
- `<remarks>` - Important implementation details, thread safety, prerequisites
- `<example>` - Basic usage example for complex classes

### Enums

```csharp
/// <summary>
/// Specifies the status of an order in the fulfillment process.
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Order has been created but not yet processed.
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// Order is being prepared for shipment.
    /// </summary>
    Processing = 1,
    
    /// <summary>
    /// Order has been shipped to customer.
    /// </summary>
    Shipped = 2,
    
    /// <summary>
    /// Order has been delivered successfully.
    /// </summary>
    Delivered = 3,
    
    /// <summary>
    /// Order was cancelled before completion.
    /// </summary>
    Cancelled = 4
}
```

**Required for public enums:**
- `<summary>` on the enum itself
- `<summary>` on each enum value explaining its meaning

### Methods

```csharp
/// <summary>
/// Calculates the total price including tax and applicable discounts.
/// </summary>
/// <param name="basePrice">The base price before tax and discounts</param>
/// <param name="taxRate">Tax rate as decimal (e.g., 0.08 for 8%)</param>
/// <param name="discountCode">Optional discount code to apply</param>
/// <returns>
/// The final price including tax and discounts, rounded to two decimal places
/// </returns>
/// <exception cref="ArgumentException">
/// Thrown when <paramref name="basePrice"/> is negative or <paramref name="taxRate"/> is not between 0 and 1
/// </exception>
/// <exception cref="InvalidDiscountException">
/// Thrown when <paramref name="discountCode"/> is invalid or expired
/// </exception>
/// <example>
/// <code>
/// decimal total = CalculateTotalPrice(100.00m, 0.08m, "SAVE10");
/// // Returns 97.20 (100 * 1.08 * 0.9)
/// </code>
/// </example>
public decimal CalculateTotalPrice(decimal basePrice, decimal taxRate, string discountCode = null)
{
    // Implementation
}
```

**Required for public methods:**
- `<summary>` - What the method does (not how)
- `<param>` - Each parameter's purpose and constraints
- `<returns>` - What is returned and any special formatting
- `<exception>` - Documented exceptions with conditions
- `<example>` - For complex methods or non-obvious usage

### Properties

```csharp
/// <summary>
/// Gets or sets the customer's email address.
/// </summary>
/// <value>
/// A valid email address string. Setting an invalid email throws an exception.
/// </value>
/// <exception cref="ArgumentException">
/// Thrown when setting an invalid email address format
/// </exception>
/// <remarks>
/// Email validation is performed using RFC 5322 standards.
/// The email is automatically converted to lowercase when set.
/// </remarks>
public string Email { get; set; }

/// <summary>
/// Gets the customer's full name in "Last, First" format.
/// </summary>
/// <value>
/// A formatted string combining LastName and FirstName properties.
/// Returns "Unknown" if both name properties are null or empty.
/// </value>
public string FullName => $"{LastName}, {FirstName}";
```

**Required for public properties:**
- `<summary>` - What the property represents
- `<value>` - Description of the value and any formatting
- `<exception>` - For setters that can throw exceptions
- `<remarks>` - Validation rules or side effects

### Events

```csharp
/// <summary>
/// Occurs when an order status changes.
/// </summary>
/// <remarks>
/// This event is raised after the status change has been persisted to the database.
/// Event handlers should not perform long-running operations as they may block the UI thread.
/// </remarks>
public event EventHandler<OrderStatusChangedEventArgs> OrderStatusChanged;
```

**Required for public events:**
- `<summary>` - When the event occurs
- `<remarks>` - Threading considerations and handler guidelines

### Fields (Public Constants/Static Readonly)

```csharp
/// <summary>
/// The maximum length allowed for customer names.
/// </summary>
/// <remarks>
/// This value is used for database schema validation and UI input limits.
/// </remarks>
public const int MaxNameLength = 100;

/// <summary>
/// Default timeout in milliseconds for API calls.
/// </summary>
public static readonly int DefaultTimeoutMs = 30000;
```

**Required for public fields:**
- `<summary>` - Purpose and usage
- `<remarks>` - Context or constraints when relevant

## Best Practices

### Writing Style

1. **Use present tense**: "Gets the value" not "Will get the value"
2. **Be concise but complete**: Focus on essential information
3. **Avoid redundancy**: Don't repeat the member name in the description
4. **Use proper grammar**: Complete sentences with proper punctuation

### Good Examples

```csharp
/// <summary>
/// Validates the email address format and domain.
/// </summary>
public bool IsValidEmail(string email)

/// <summary>
/// Gets the timestamp when the record was last modified.
/// </summary>
public DateTime LastModified { get; }
```

### Poor Examples

```csharp
/// <summary>
/// This method validates email.
/// </summary>
public bool IsValidEmail(string email) // Too brief, doesn't explain what validation includes

/// <summary>
/// Gets or sets the LastModified property.
/// </summary>
public DateTime LastModified { get; set; } // Redundant, doesn't add value
```

### Parameter Descriptions

- **Be specific about types and constraints**:
  ```csharp
  /// <param name="timeout">Timeout in milliseconds. Must be positive.</param>
  ```

- **Indicate optional parameters and defaults**:
  ```csharp
  /// <param name="includeDeleted">If true, includes soft-deleted records. Default is false.</param>
  ```

### Exception Documentation

Only document exceptions that:
1. Are part of the method's contract
2. Callers should handle
3. Indicate misuse of the API

```csharp
/// <exception cref="ArgumentNullException">
/// Thrown when <paramref name="customer"/> is null
/// </exception>
/// <exception cref="ValidationException">
/// Thrown when customer data fails business rule validation
/// </exception>
```

## Tools and Automation

### Visual Studio Settings

- Enable XML documentation warnings in project properties
- Use code analysis rules to enforce documentation coverage
- Consider tools like DocFX for generating documentation websites

### Recommended Extensions

- **GhostDoc**: Auto-generates documentation templates
- **XML Documentation Comments**: Provides IntelliSense for XML tags
- **SandCastle**: Generates professional documentation from XML comments

## Documentation Coverage Guidelines

| Element Type | Public | Internal | Protected | Private |
|--------------|--------|----------|-----------|---------|
| Classes      | ✅ Required | ⚠️ Recommended | ⚠️ Recommended | ❌ Optional |
| Interfaces   | ✅ Required | ⚠️ Recommended | N/A | N/A |
| Methods      | ✅ Required | ⚠️ Recommended | ⚠️ Recommended | ❌ Optional |
| Properties   | ✅ Required | ⚠️ If complex | ⚠️ Recommended | ❌ Optional |
| Events       | ✅ Required | ⚠️ Recommended | ⚠️ Recommended | ❌ Optional |
| Enums        | ✅ Required | ⚠️ Recommended | ⚠️ Recommended | ❌ Optional |
| Constants    | ✅ Required | ⚠️ If not obvious | ❌ Optional | ❌ Optional |

**Legend:**
- ✅ Required: Must have comprehensive documentation
- ⚠️ Recommended: Should have at least summary documentation
- ❌ Optional: Documentation at developer's discretion