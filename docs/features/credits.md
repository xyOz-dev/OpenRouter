# Credit Management

## Credits Service Overview

The [`ICreditsService`](../../OpenRouter/Services/Credits/ICreditsService.cs:1) interface provides comprehensive credit monitoring and management capabilities, enabling applications to track usage, monitor balances, and integrate payment processing for OpenRouter credits.

### Credit Monitoring Capabilities

Access credit operations through the OpenRouter client:

```csharp
var creditsService = client.Credits;
var balance = await creditsService.GetCreditsAsync();
```

## Checking Credit Balance

### CreditsRequest and CreditsResponse Usage

Monitor credit balance using [`CreditsRequest`](../../OpenRouter/Models/Requests/CreditsRequest.cs:1) and [`CreditsResponse`](../../OpenRouter/Models/Responses/CreditsResponse.cs:1):

```csharp
var creditsResponse = await client.Credits.GetCreditsAsync();
Console.WriteLine($"Current balance: ${creditsResponse.Data.Balance}");
Console.WriteLine($"Credits used today: {creditsResponse.Data.Usage.Today}");
```

### Real-time Balance Monitoring

Implement continuous balance monitoring:

<!-- C# Code Example: Real-time balance monitoring with polling and webhooks -->

## Usage Tracking

### WithUsageAccounting() Method

Enable detailed usage tracking using [`WithUsageAccounting()`](../../OpenRouter/Services/Chat/IChatRequestBuilder.cs:22):

```csharp
var response = await client.Chat.CreateRequest()
    .WithModel("anthropic/claude-3-haiku")
    .WithUserMessage("Hello, world!")
    .WithUsageAccounting(true)
    .ExecuteAsync();

Console.WriteLine($"Tokens used: {response.Usage.TotalTokens}");
Console.WriteLine($"Estimated cost: ${response.Usage.EstimatedCost}");
```

### Token Consumption Monitoring

Track token usage across requests:

<!-- C# Code Example: Token consumption tracking and aggregation -->

### Cost Calculation and Reporting

Calculate and report costs based on usage:

<!-- C# Code Example: Cost calculation with different models and token types -->

## Coinbase Payment Integration

### Payment Processing Setup

Set up Coinbase payment integration for credit purchases:

<!-- C# Code Example: Coinbase payment setup and configuration -->

### Credit Purchasing Workflows

Implement credit purchasing functionality:

<!-- C# Code Example: Complete credit purchase workflow with Coinbase -->

## Credit Alerts and Thresholds

### Low Credit Detection

Monitor and detect low credit balances:

```csharp
public async Task<bool> CheckLowCreditAlert(decimal threshold = 5.0m)
{
    var credits = await client.Credits.GetCreditsAsync();
    return credits.Data.Balance < threshold;
}
```

### Automated Top-up Strategies

Implement automated credit replenishment:

<!-- C# Code Example: Automated credit top-up with configurable thresholds -->

## Advanced Credit Management

### Usage Analytics

Analyze credit usage patterns:

<!-- C# Code Example: Usage analytics and trend analysis -->

### Budget Management

Implement budget controls and limits:

<!-- C# Code Example: Budget enforcement and spending controls -->

### Multi-Account Credit Tracking

Track credits across multiple accounts or projects:

<!-- C# Code Example: Multi-tenant credit tracking -->

## Credit Optimization

### Cost-Efficient Model Selection

Choose models based on credit efficiency:

<!-- C# Code Example: Model selection based on cost-per-token ratios -->

### Batch Processing for Credit Efficiency

Optimize requests for better credit utilization:

<!-- C# Code Example: Batch processing strategies for credit optimization -->

### Credit Pooling Strategies

Manage credits across multiple applications:

<!-- C# Code Example: Credit pooling and allocation strategies -->

## Code Examples

### Credit Monitoring and Payment Integration

Complete implementation of credit monitoring with payment integration:

<!-- C# Code Example: Comprehensive credit management system -->

### Real-time Usage Dashboard

Build a real-time usage monitoring dashboard:

<!-- C# Code Example: Dashboard implementation with live credit updates -->

### Credit-Aware Request Handling

Implement credit-aware request processing:

<!-- C# Code Example: Request processing with credit validation and fallbacks -->

## Best Practices

### Credit Security

Secure credit information and payment processing:

<!-- C# Code Example: Secure credit handling and PCI compliance -->

### Error Handling

Handle credit-related errors gracefully:

```csharp
try
{
    var response = await client.Chat.CreateRequest()
        .WithModel("anthropic/claude-3-haiku")
        .WithUserMessage("Hello")
        .ExecuteAsync();
}
catch (InsufficientCreditsException ex)
{
    // Handle insufficient credits
    await HandleLowCredits();
}
```

### Performance Optimization

Optimize credit monitoring for performance:

<!-- C# Code Example: Efficient credit monitoring with caching and batching -->