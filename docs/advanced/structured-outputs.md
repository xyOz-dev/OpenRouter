# Structured Outputs and JSON Schema

The OpenRouter .NET library provides comprehensive support for structured outputs, enabling AI models to return strongly-typed, validated responses. This document covers typed response patterns, schema generation, validation, and advanced structured output scenarios.

## Structured Output Overview

### Typed Response Benefits

Structured outputs provide several key advantages:

- **Type Safety**: Compile-time validation of response structure and data types
- **Automatic Validation**: Built-in schema validation ensures response compliance
- **IntelliSense Support**: Full IDE support with auto-completion and type checking
- **Serialization Control**: Customizable JSON serialization behavior
- **Error Prevention**: Eliminates runtime errors from malformed responses
- **Documentation**: Self-documenting code through strongly-typed models
- **Consistency**: Guaranteed response format across different model providers

### Use Cases for Structured Outputs

Structured outputs excel in scenarios requiring:
- Data extraction from unstructured text
- Form generation and validation
- API response standardization
- Configuration file generation
- Report and document creation
- Multi-step workflows with typed intermediate results

## Generic Structured Output

### [`WithStructuredOutput<T>()`](OpenRouter/Services/Chat/IChatRequestBuilder.cs:20) Usage

**Basic Typed Response**
```csharp
public class PersonInfo
{
    public string Name { get; set; }
    public int Age { get; set; }
    public string Email { get; set; }
    public string[] Hobbies { get; set; }
    public Address Address { get; set; }
}

public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
}

var response = await client.Chat
    .WithModel("gpt-4")
    .WithMessages("Extract person info from: John Smith, 30 years old, lives at 123 Main St, Boston MA 02101, enjoys hiking and reading, email: john@example.com")
    .WithStructuredOutput<PersonInfo>()
    .ExecuteAsync();

PersonInfo person = response.GetStructuredOutput<PersonInfo>();
```

### C# Class to JSON Schema Mapping

**Automatic Schema Generation**
The library automatically converts C# classes to JSON schemas:

```csharp
// C# Class
public class ProductReview
{
    [Required]
    public string ProductName { get; set; }
    
    [Range(1, 5)]
    public int Rating { get; set; }
    
    [MaxLength(1000)]
    public string ReviewText { get; set; }
    
    public bool IsRecommended { get; set; }
    
    [JsonPropertyName("review_date")]
    public DateTime ReviewDate { get; set; }
    
    public ReviewCategory[] Categories { get; set; }
}

public enum ReviewCategory
{
    Quality,
    Value,
    Service,
    Delivery
}
```

The generated JSON schema includes:
- Required field validation
- Type constraints and formats
- String length limits
- Numeric ranges
- Enum value restrictions
- Custom property naming

### Type Constraint Requirements

**Generic Constraints for Schema Generation**
```csharp
public static class StructuredOutputExtensions
{
    public static IChatRequestBuilder WithStructuredOutput<T>(
        this IChatRequestBuilder builder) 
        where T : class, new()
    {
        var schema = JsonSchemaGenerator.Generate<T>();
        return builder.WithStructuredOutput(schema);
    }
}
```

**Supported Type Patterns**
- Classes with parameterless constructors
- Properties with public getters and setters
- Primitive types (string, int, bool, DateTime, etc.)
- Collections (arrays, lists, dictionaries)
- Nested complex types
- Enumerations
- Nullable types

<!-- C# Code Example: Complex nested type with validation attributes -->

## Custom JSON Schema

### [`WithStructuredOutput(object jsonSchema)`](OpenRouter/Services/Chat/IChatRequestBuilder.cs:21) Method

**Manual Schema Definition**
```csharp
var customSchema = new
{
    type = "object",
    properties = new
    {
        analysis = new
        {
            type = "object",
            properties = new
            {
                sentiment = new
                {
                    type = "string",
                    @enum = new[] { "positive", "negative", "neutral" }
                },
                confidence = new
                {
                    type = "number",
                    minimum = 0.0,
                    maximum = 1.0
                },
                keywords = new
                {
                    type = "array",
                    items = new { type = "string" },
                    maxItems = 10
                }
            },
            required = new[] { "sentiment", "confidence" }
        },
        summary = new
        {
            type = "string",
            maxLength = 500
        }
    },
    required = new[] { "analysis" }
};

var response = await client.Chat
    .WithModel("claude-3-sonnet")
    .WithMessages("Analyze the sentiment: 'I love this product! It works perfectly.'")
    .WithStructuredOutput(customSchema)
    .ExecuteAsync();
```

### Complex Schema Patterns

**Advanced Schema Construction**
```csharp
public class SchemaBuilder
{
    public static object CreateAnalyticsSchema()
    {
        return new
        {
            type = "object",
            properties = new
            {
                metrics = new
                {
                    type = "object",
                    properties = new
                    {
                        totalUsers = new { type = "integer", minimum = 0 },
                        activeUsers = new { type = "integer", minimum = 0 },
                        conversionRate = new { type = "number", minimum = 0, maximum = 1 },
                        revenue = new { type = "number", minimum = 0 }
                    },
                    required = new[] { "totalUsers", "activeUsers" }
                },
                timeframe = new
                {
                    type = "object",
                    properties = new
                    {
                        start = new { type = "string", format = "date-time" },
                        end = new { type = "string", format = "date-time" }
                    },
                    required = new[] { "start", "end" }
                },
                insights = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        properties = new
                        {
                            category = new { type = "string" },
                            impact = new { type = "string", @enum = new[] { "high", "medium", "low" } },
                            description = new { type = "string" },
                            actionable = new { type = "boolean" }
                        },
                        required = new[] { "category", "impact", "description" }
                    },
                    maxItems = 5
                }
            },
            required = new[] { "metrics", "timeframe" }
        };
    }
}
```

### Schema Validation Tools

**Runtime Schema Validation**
```csharp
public class SchemaValidator
{
    public bool ValidateSchema(object schema)
    {
        try
        {
            var jsonSchema = JsonSerializer.Serialize(schema);
            var parsedSchema = JsonDocument.Parse(jsonSchema);
            
            return ValidateSchemaStructure(parsedSchema);
        }
        catch (Exception)
        {
            return false;
        }
    }
    
    private bool ValidateSchemaStructure(JsonDocument schema)
    {
        var root = schema.RootElement;
        
        // Validate required schema properties
        if (!root.TryGetProperty("type", out _))
            return false;
            
        if (root.TryGetProperty("properties", out var props))
        {
            // Validate each property definition
            foreach (var property in props.EnumerateObject())
            {
                if (!ValidatePropertyDefinition(property.Value))
                    return false;
            }
        }
        
        return true;
    }
}
```

<!-- C# Code Example: Dynamic schema generation based on runtime data -->

## Response Processing

### Deserialization to Typed Objects

**Automatic Type Conversion**
```csharp
public class StructuredResponseProcessor<T>
{
    public T ProcessResponse(ChatCompletionResponse response)
    {
        var content = response.Choices[0].Message.Content;
        
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
            
            return JsonSerializer.Deserialize<T>(content, options);
        }
        catch (JsonException ex)
        {
            throw new StructuredOutputException(
                $"Failed to deserialize response to {typeof(T).Name}", ex);
        }
    }
}
```

### Validation and Error Handling

**Response Validation Framework**
```csharp
public class ResponseValidator<T>
{
    private readonly List<IValidator<T>> validators = new();
    
    public ResponseValidator<T> AddValidator(IValidator<T> validator)
    {
        validators.Add(validator);
        return this;
    }
    
    public ValidationResult Validate(T response)
    {
        var errors = new List<string>();
        
        foreach (var validator in validators)
        {
            var result = validator.Validate(response);
            if (!result.IsValid)
            {
                errors.AddRange(result.Errors);
            }
        }
        
        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }
}

public class PersonInfoValidator : IValidator<PersonInfo>
{
    public ValidationResult Validate(PersonInfo person)
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(person.Name))
            errors.Add("Name is required");
            
        if (person.Age <= 0 || person.Age > 150)
            errors.Add("Age must be between 1 and 150");
            
        if (!string.IsNullOrEmpty(person.Email) && !IsValidEmail(person.Email))
            errors.Add("Invalid email format");
            
        return new ValidationResult { IsValid = errors.Count == 0, Errors = errors };
    }
}
```

### Schema Compliance Verification

**Runtime Schema Validation**
```csharp
public class SchemaComplianceChecker
{
    public bool VerifyCompliance<T>(T response, object schema)
    {
        var responseJson = JsonSerializer.Serialize(response);
        var responseDocument = JsonDocument.Parse(responseJson);
        
        return ValidateAgainstSchema(responseDocument.RootElement, schema);
    }
    
    private bool ValidateAgainstSchema(JsonElement element, object schema)
    {
        var schemaJson = JsonSerializer.Serialize(schema);
        var schemaDocument = JsonDocument.Parse(schemaJson);
        
        // Implement JSON Schema validation logic
        return PerformSchemaValidation(element, schemaDocument.RootElement);
    }
}
```

<!-- C# Code Example: Custom validation rules and error reporting -->

## Schema Generation

### Automatic Schema from .NET Types

**Reflection-Based Schema Generator**
```csharp
public class JsonSchemaGenerator
{
    public static object Generate<T>()
    {
        return Generate(typeof(T));
    }
    
    public static object Generate(Type type)
    {
        if (type.IsPrimitive || type == typeof(string))
            return GeneratePrimitiveSchema(type);
            
        if (type.IsEnum)
            return GenerateEnumSchema(type);
            
        if (IsCollectionType(type))
            return GenerateArraySchema(type);
            
        return GenerateObjectSchema(type);
    }
    
    private static object GenerateObjectSchema(Type type)
    {
        var properties = new Dictionary<string, object>();
        var required = new List<string>();
        
        foreach (var prop in type.GetProperties())
        {
            var jsonName = GetJsonPropertyName(prop);
            properties[jsonName] = Generate(prop.PropertyType);
            
            if (IsRequired(prop))
                required.Add(jsonName);
        }
        
        return new
        {
            type = "object",
            properties = properties,
            required = required.ToArray()
        };
    }
}
```

### Annotation-Driven Schema Customization

**Attribute-Based Schema Control**
```csharp
[JsonSchema(Title = "Product Information", Description = "Complete product details")]
public class Product
{
    [JsonRequired]
    [JsonPropertyName("product_id")]
    [StringLength(50)]
    public string Id { get; set; }
    
    [JsonRequired]
    [StringLength(200)]
    public string Name { get; set; }
    
    [Range(0.01, 999999.99)]
    public decimal Price { get; set; }
    
    [JsonPropertyName("in_stock")]
    public bool InStock { get; set; }
    
    [MaxLength(1000)]
    public string Description { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProductCategory Category { get; set; }
    
    [JsonPropertyName("created_at")]
    [JsonFormat("date-time")]
    public DateTime CreatedAt { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProductCategory
{
    Electronics,
    Clothing,
    Books,
    Home,
    Sports
}
```

### Schema Validation Tools

**Schema Testing and Validation**
```csharp
public class SchemaTestFramework
{
    public void ValidateSchemaGeneration<T>(T sampleData)
    {
        // Generate schema from type
        var schema = JsonSchemaGenerator.Generate<T>();
        
        // Serialize sample data
        var json = JsonSerializer.Serialize(sampleData);
        
        // Validate sample data against generated schema
        var isValid = ValidateJsonAgainstSchema(json, schema);
        
        if (!isValid)
            throw new SchemaValidationException(
                $"Generated schema for {typeof(T).Name} does not validate sample data");
    }
    
    public void TestSchemaRoundTrip<T>(T originalData)
    {
        // Generate schema
        var schema = JsonSchemaGenerator.Generate<T>();
        
        // Use schema in structured output
        var response = SimulateStructuredResponse(schema, originalData);
        
        // Deserialize and compare
        var deserializedData = JsonSerializer.Deserialize<T>(response);
        
        AssertEquivalent(originalData, deserializedData);
    }
}
```

<!-- C# Code Example: Performance optimization for large schema generation -->

## Use Cases

### API Response Standardization

**Consistent API Response Format**
```csharp
public class ApiResponseWrapper<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }
    public ErrorInfo[] Errors { get; set; }
    public DateTime Timestamp { get; set; }
}

public class ErrorInfo
{
    public string Code { get; set; }
    public string Message { get; set; }
    public string Field { get; set; }
}
```

### Data Extraction Workflows

**Document Processing Pipeline**
```csharp
public class DocumentProcessor
{
    public async Task<ExtractedData> ProcessDocument(string documentText)
    {
        return await client.Chat
            .WithModel("gpt-4")
            .WithMessages($"Extract structured data from: {documentText}")
            .WithStructuredOutput<ExtractedData>()
            .ExecuteAsync()
            .ContinueWith(response => response.Result.GetStructuredOutput<ExtractedData>());
    }
}

public class ExtractedData
{
    public DocumentMetadata Metadata { get; set; }
    public ContactInfo[] Contacts { get; set; }
    public FinancialInfo[] Financials { get; set; }
    public KeyPoint[] KeyPoints { get; set; }
}
```

### Form Generation and Validation

**Dynamic Form Schema Generation**
<!-- C# Code Example: Form field generation from structured output schema -->
<!-- C# Code Example: Client-side validation rule generation -->
<!-- C# Code Example: Multi-step form wizard with typed intermediate steps -->

## Code Examples

### Structured Output Implementation Patterns

<!-- C# Code Example: Complex data extraction with nested validation -->
<!-- C# Code Example: Real-time structured output with streaming support -->
<!-- C# Code Example: Batch processing with structured output validation -->
<!-- C# Code Example: Schema evolution and backward compatibility -->
<!-- C# Code Example: Performance benchmarking for different schema complexities -->

The structured output system provides a robust foundation for building type-safe AI applications that can reliably extract, validate, and process structured data from natural language inputs.