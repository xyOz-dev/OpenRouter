# API Key Management

## Keys Service Overview

The [`IKeysService`](../../OpenRouter/Services/Keys/IKeysService.cs:1) interface provides comprehensive API key lifecycle management capabilities, enabling secure creation, management, and revocation of API keys with granular permission control.

### API Key Lifecycle Management

Access key operations through the OpenRouter client:

```csharp
var keysService = client.Keys;
var keys = await keysService.GetKeysAsync();
```

## Creating API Keys

### KeysRequest Usage

Create new API keys using [`KeysRequest`](../../OpenRouter/Models/Requests/KeysRequest.cs:1):

```csharp
var newKeyRequest = new KeysRequest
{
    Name = "Production API Key",
    Permissions = new[] { "chat", "models" }
};

var createdKey = await client.Keys.CreateKeyAsync(newKeyRequest);
Console.WriteLine($"New key created: {createdKey.Key}");
```

### Key Generation with Specific Permissions

Generate keys with tailored permission sets:

<!-- C# Code Example: Creating keys with specific permission scopes -->

## Listing and Managing Keys

### KeysResponse Handling

Process key listings using [`KeysResponse`](../../OpenRouter/Models/Responses/KeysResponse.cs:1):

```csharp
var keysResponse = await client.Keys.GetKeysAsync();
foreach (var key in keysResponse.Data)
{
    Console.WriteLine($"Key: {key.Id} - {key.Name}");
    Console.WriteLine($"Created: {key.CreatedAt}");
    Console.WriteLine($"Permissions: {string.Join(", ", key.Permissions)}");
}
```

### Key Rotation and Updates

Implement secure key rotation strategies:

<!-- C# Code Example: Key rotation workflow with zero-downtime updates -->

## Key Permissions and Scoping

### Permission-based Access Control

Configure granular permissions for different use cases:

```csharp
// Read-only key for monitoring
var monitoringKey = new KeysRequest
{
    Name = "Monitoring Key",
    Permissions = new[] { "models:read", "credits:read" }
};

// Full access key for production
var productionKey = new KeysRequest
{
    Name = "Production Key",
    Permissions = new[] { "chat", "models", "credits", "keys" }
};
```

### Service-specific Key Restrictions

Create keys with service-specific limitations:

<!-- C# Code Example: Service-scoped keys with specific model access -->

## Key Revocation

### Secure Key Deletion

Safely revoke and delete API keys:

```csharp
public async Task RevokeKey(string keyId)
{
    await client.Keys.DeleteKeyAsync(keyId);
    Console.WriteLine($"Key {keyId} successfully revoked");
}
```

### Emergency Key Revocation

Implement emergency revocation procedures:

<!-- C# Code Example: Bulk key revocation and emergency procedures -->

## Security Best Practices

### Key Storage Recommendations

Secure storage patterns for API keys:

```csharp
// DO NOT store keys in code
// Use environment variables or secure key vaults
var apiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");

// Use Azure Key Vault, AWS Secrets Manager, or similar
var keyVaultSecret = await keyVaultClient.GetSecretAsync("openrouter-api-key");
```

### Access Control Patterns

Implement proper access control for key management:

<!-- C# Code Example: Role-based access control for key management -->

## Advanced Key Management

### Key Usage Monitoring

Monitor API key usage and detect anomalies:

<!-- C# Code Example: Key usage tracking and anomaly detection -->

### Automated Key Rotation

Set up automated key rotation schedules:

<!-- C# Code Example: Scheduled key rotation with overlap periods -->

### Multi-Environment Key Management

Manage keys across different environments:

```csharp
public class EnvironmentKeyManager
{
    private readonly Dictionary<string, string> _environmentKeys = new()
    {
        { "development", "dev-key" },
        { "staging", "staging-key" },
        { "production", "prod-key" }
    };

    public string GetKeyForEnvironment(string environment)
    {
        return _environmentKeys[environment];
    }
}
```

### Key Versioning

Implement key versioning for smooth transitions:

<!-- C# Code Example: Key versioning and migration strategies -->

## Code Examples

### Complete Key Management Workflows

Comprehensive key management implementation:

<!-- C# Code Example: Complete key lifecycle management -->

### Multi-Tenant Key Management

Manage keys for multiple tenants or applications:

<!-- C# Code Example: Multi-tenant key isolation and management -->

### Key Performance Monitoring

Monitor key performance and usage patterns:

<!-- C# Code Example: Key performance metrics and alerting -->

## Integration Patterns

### CI/CD Pipeline Integration

Integrate key management with deployment pipelines:

<!-- C# Code Example: Automated key management in CI/CD -->

### Container Orchestration

Manage keys in containerized environments:

<!-- C# Code Example: Kubernetes secret management for API keys -->

### Microservices Architecture

Distribute keys across microservices securely:

<!-- C# Code Example: Service mesh integration for key distribution -->

## Compliance and Auditing

### Audit Trail

Maintain comprehensive audit trails for key operations:

<!-- C# Code Example: Audit logging for key management operations -->

### Compliance Requirements

Meet regulatory compliance requirements:

<!-- C# Code Example: Compliance-focused key management -->

### Security Incident Response

Respond to security incidents involving API keys:

<!-- C# Code Example: Incident response procedures for compromised keys -->