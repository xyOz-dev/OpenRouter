# Web Search Integration

The OpenRouter .NET library provides comprehensive web search capabilities that enable AI models to access real-time internet information for enhanced responses. This document covers web search configuration, optimization, result processing, and advanced integration patterns.

## Web Search Overview

### Real-time Internet Search Capabilities

Web search integration enables AI models to:
- Access current information and recent events
- Verify facts and provide citations
- Gather contextual information for better responses
- Research topics with up-to-date data
- Augment responses with authoritative sources
- Provide comprehensive analysis across multiple data points

### Benefits of Web Search Integration

**Enhanced Response Quality**
- Current and accurate information
- Fact-checked responses with sources
- Comprehensive topic coverage
- Real-time data access

**Improved User Experience**
- Relevant and timely information
- Source attribution and transparency
- Reduced hallucination in AI responses
- Access to specialized knowledge domains

**Business Applications**
- Market research and analysis
- Competitive intelligence
- News monitoring and alerts
- Customer support with current information

## Basic Web Search

### [`WithWebSearch()`](OpenRouter/Services/Chat/IChatRequestBuilder.cs:37) Enablement

**Simple Web Search Activation**
```csharp
var response = await client.Chat
    .WithModel("gpt-4")
    .WithMessages("What are the latest developments in renewable energy technology?")
    .WithWebSearch()
    .ExecuteAsync();
```

The basic [`WithWebSearch()`](OpenRouter/Services/Chat/IChatRequestBuilder.cs:37) method enables web search with default settings:
- Automatic query generation from user input
- Standard result count (typically 5-10 results)
- Default relevance scoring
- Basic source filtering

### Default Search Configuration

**Standard Search Behavior**
- Query extraction from conversation context
- Relevance-based result ranking
- Content summarization and integration
- Source citation in responses
- Automatic content filtering for quality

**Default Parameters**
```csharp
public class DefaultWebSearchOptions
{
    public int MaxResults { get; set; } = 5;
    public string[] AllowedDomains { get; set; } = null; // All domains
    public string[] BlockedDomains { get; set; } = null; // No restrictions
    public TimeRange TimeRange { get; set; } = TimeRange.AnyTime;
    public SearchSafety SafetyLevel { get; set; } = SearchSafety.Moderate;
    public bool IncludeCitations { get; set; } = true;
}
```

<!-- C# Code Example: Basic search integration patterns -->

## Advanced Web Search Configuration

### [`WithWebSearch(Action<WebSearchOptions>)`](OpenRouter/Services/Chat/IChatRequestBuilder.cs:38) Method

**Comprehensive Search Configuration**
```csharp
var response = await client.Chat
    .WithModel("claude-3-sonnet")
    .WithMessages("Research the financial performance of tech companies in 2024")
    .WithWebSearch(options =>
    {
        options.MaxResults = 15;
        options.TimeRange = TimeRange.LastYear;
        options.AllowedDomains = new[] { "sec.gov", "nasdaq.com", "bloomberg.com", "reuters.com" };
        options.SearchSafety = SearchSafety.Strict;
        options.IncludeCitations = true;
        options.RankBy = SearchRanking.Relevance;
        options.ContentType = ContentType.Articles;
    })
    .ExecuteAsync();
```

### Search Parameters and Filtering

**Advanced Filtering Options**
```csharp
public class WebSearchOptions
{
    // Result Control
    public int MaxResults { get; set; } = 5;
    public SearchRanking RankBy { get; set; } = SearchRanking.Relevance;
    
    // Time Filtering
    public TimeRange TimeRange { get; set; } = TimeRange.AnyTime;
    public DateTime? CustomStartDate { get; set; }
    public DateTime? CustomEndDate { get; set; }
    
    // Domain Filtering
    public string[] AllowedDomains { get; set; }
    public string[] BlockedDomains { get; set; }
    public DomainAuthority MinDomainAuthority { get; set; } = DomainAuthority.Any;
    
    // Content Filtering
    public ContentType ContentType { get; set; } = ContentType.All;
    public string[] RequiredKeywords { get; set; }
    public string[] ExcludedKeywords { get; set; }
    public SearchLanguage Language { get; set; } = SearchLanguage.English;
    
    // Quality Control
    public SearchSafety SafetyLevel { get; set; } = SearchSafety.Moderate;
    public bool VerifySourceCredibility { get; set; } = true;
    public int MinContentLength { get; set; } = 100;
    
    // Response Integration
    public bool IncludeCitations { get; set; } = true;
    public bool SummarizeResults { get; set; } = true;
    public CitationStyle CitationStyle { get; set; } = CitationStyle.APA;
}

public enum TimeRange
{
    AnyTime,
    LastHour,
    LastDay,
    LastWeek,
    LastMonth,
    LastYear,
    Custom
}

public enum ContentType
{
    All,
    Articles,
    News,
    Academic,
    Videos,
    Images,
    PDFs,
    Forums
}
```

### Result Count and Source Control

**Fine-grained Source Management**
```csharp
var techNewsSearch = await client.Chat
    .WithMessages("Latest AI technology breakthroughs")
    .WithWebSearch(options =>
    {
        options.MaxResults = 12;
        options.AllowedDomains = new[]
        {
            "techcrunch.com",
            "wired.com",
            "arstechnica.com",
            "theverge.com",
            "ieee.org",
            "nature.com"
        };
        options.ContentType = ContentType.Articles;
        options.TimeRange = TimeRange.LastMonth;
        options.MinDomainAuthority = DomainAuthority.High;
    })
    .ExecuteAsync();
```

**Dynamic Source Selection**
```csharp
public class ContextualSearchConfigurator
{
    public WebSearchOptions ConfigureSearch(string query, SearchContext context)
    {
        var options = new WebSearchOptions();
        
        if (context.Topic == SearchTopic.Finance)
        {
            options.AllowedDomains = new[] { "sec.gov", "bloomberg.com", "reuters.com" };
            options.MaxResults = 10;
            options.TimeRange = TimeRange.LastMonth;
        }
        else if (context.Topic == SearchTopic.Medical)
        {
            options.AllowedDomains = new[] { "pubmed.ncbi.nlm.nih.gov", "nejm.org", "bmj.com" };
            options.ContentType = ContentType.Academic;
            options.VerifySourceCredibility = true;
        }
        else if (context.Topic == SearchTopic.News)
        {
            options.ContentType = ContentType.News;
            options.TimeRange = TimeRange.LastDay;
            options.MaxResults = 8;
        }
        
        return options;
    }
}
```

<!-- C# Code Example: Conditional search configuration based on user preferences -->

## Search Result Processing

### Integrated Search Results in Responses

**Automatic Result Integration**
The web search results are automatically integrated into AI responses with:
- Contextual information weaving
- Source attribution
- Fact verification
- Relevance scoring
- Content summarization

**Response with Search Integration**
```csharp
public class SearchEnhancedResponse
{
    public string MainResponse { get; set; }
    public SearchResult[] Sources { get; set; }
    public Citation[] Citations { get; set; }
    public SearchMetadata Metadata { get; set; }
}

public class SearchResult
{
    public string Title { get; set; }
    public string Url { get; set; }
    public string Snippet { get; set; }
    public DateTime PublishedDate { get; set; }
    public string Domain { get; set; }
    public float RelevanceScore { get; set; }
    public string[] Keywords { get; set; }
}
```

### Citation and Source Attribution

**Citation Management**
```csharp
public class CitationProcessor
{
    public string FormatCitations(SearchResult[] results, CitationStyle style)
    {
        return style switch
        {
            CitationStyle.APA => FormatAPA(results),
            CitationStyle.MLA => FormatMLA(results),
            CitationStyle.Chicago => FormatChicago(results),
            CitationStyle.Harvard => FormatHarvard(results),
            _ => FormatBasic(results)
        };
    }
    
    private string FormatAPA(SearchResult[] results)
    {
        var citations = results.Select(r => 
            $"{ExtractAuthor(r)}. ({r.PublishedDate.Year}). {r.Title}. " +
            $"Retrieved from {r.Url}");
        
        return string.Join("\n", citations);
    }
    
    public InlineCitation[] GenerateInlineCitations(string response, SearchResult[] sources)
    {
        var citations = new List<InlineCitation>();
        
        foreach (var source in sources)
        {
            var relevantSentences = ExtractRelevantSentences(response, source);
            foreach (var sentence in relevantSentences)
            {
                citations.Add(new InlineCitation
                {
                    Text = sentence,
                    SourceId = source.Url,
                    ConfidenceScore = CalculateRelevance(sentence, source)
                });
            }
        }
        
        return citations.ToArray();
    }
}
```

### Result Ranking and Relevance

**Custom Relevance Scoring**
```csharp
public class RelevanceEngine
{
    public float CalculateRelevance(SearchResult result, string originalQuery)
    {
        var scores = new[]
        {
            KeywordMatchScore(result, originalQuery) * 0.3f,
            DomainAuthorityScore(result) * 0.2f,
            RecencyScore(result) * 0.2f,
            ContentQualityScore(result) * 0.2f,
            UserEngagementScore(result) * 0.1f
        };
        
        return scores.Sum();
    }
    
    private float KeywordMatchScore(SearchResult result, string query)
    {
        var queryKeywords = ExtractKeywords(query);
        var resultKeywords = ExtractKeywords($"{result.Title} {result.Snippet}");
        
        var matches = queryKeywords.Intersect(resultKeywords, StringComparer.OrdinalIgnoreCase);
        return (float)matches.Count() / queryKeywords.Length;
    }
    
    public SearchResult[] RankResults(SearchResult[] results, string query, RankingCriteria criteria)
    {
        return results
            .Select(r => new { Result = r, Score = CalculateCustomScore(r, query, criteria) })
            .OrderByDescending(x => x.Score)
            .Select(x => x.Result)
            .ToArray();
    }
}
```

<!-- C# Code Example: Advanced result filtering and deduplication -->

## Search Optimization

### Query Refinement Strategies

**Intelligent Query Enhancement**
```csharp
public class QueryOptimizer
{
    public string OptimizeQuery(string originalQuery, SearchContext context)
    {
        var optimizedQuery = originalQuery;
        
        // Add domain-specific keywords
        optimizedQuery = AddDomainKeywords(optimizedQuery, context.Domain);
        
        // Include temporal modifiers
        if (context.RequiresRecent)
            optimizedQuery += " 2024";
            
        // Add specificity filters
        optimizedQuery = AddSpecificityFilters(optimizedQuery, context.Specificity);
        
        // Remove ambiguous terms
        optimizedQuery = RemoveAmbiguousTerms(optimizedQuery);
        
        return optimizedQuery;
    }
    
    public string[] GenerateQueryVariations(string baseQuery)
    {
        return new[]
        {
            baseQuery,
            AddSynonyms(baseQuery),
            SimplifyQuery(baseQuery),
            AddContextualTerms(baseQuery),
            CreateNaturalLanguageQuery(baseQuery)
        };
    }
    
    public async Task<string> RefineBasedOnResults(string originalQuery, SearchResult[] results)
    {
        if (results.Length == 0)
            return ExpandQuery(originalQuery);
            
        if (results.All(r => r.RelevanceScore < 0.3f))
            return RephaseQuery(originalQuery);
            
        return originalQuery; // Query is working well
    }
}
```

### Search Result Caching

**Performance Optimization through Caching**
```csharp
public class SearchResultCache
{
    private readonly IMemoryCache cache;
    private readonly SearchCacheOptions options;
    
    public SearchResultCache(IMemoryCache cache, SearchCacheOptions options)
    {
        this.cache = cache;
        this.options = options;
    }
    
    public async Task<SearchResult[]> GetOrSearchAsync(string query, WebSearchOptions searchOptions)
    {
        var cacheKey = GenerateCacheKey(query, searchOptions);
        
        if (cache.TryGetValue(cacheKey, out SearchResult[] cachedResults))
        {
            return FilterExpiredResults(cachedResults);
        }
        
        var results = await PerformWebSearch(query, searchOptions);
        
        cache.Set(cacheKey, results, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = GetCacheDuration(searchOptions),
            SlidingExpiration = options.SlidingExpiration,
            Priority = CacheItemPriority.Normal
        });
        
        return results;
    }
    
    private TimeSpan GetCacheDuration(WebSearchOptions options)
    {
        return options.TimeRange switch
        {
            TimeRange.LastHour => TimeSpan.FromMinutes(5),
            TimeRange.LastDay => TimeSpan.FromMinutes(30),
            TimeRange.LastWeek => TimeSpan.FromHours(2),
            TimeRange.LastMonth => TimeSpan.FromHours(6),
            _ => TimeSpan.FromHours(1)
        };
    }
}
```

### Performance Considerations

**Search Performance Optimization**
```csharp
public class SearchPerformanceOptimizer
{
    public async Task<SearchResult[]> OptimizedSearch(string query, WebSearchOptions options)
    {
        // Parallel search execution
        var tasks = new[]
        {
            SearchPrimaryIndex(query, options),
            SearchSecondaryIndex(query, options),
            SearchCachedResults(query)
        };
        
        var results = await Task.WhenAll(tasks);
        
        // Merge and deduplicate results
        var mergedResults = MergeSearchResults(results);
        
        // Apply performance-based filtering
        return FilterByPerformanceMetrics(mergedResults, options);
    }
    
    private SearchResult[] FilterByPerformanceMetrics(SearchResult[] results, WebSearchOptions options)
    {
        return results
            .Where(r => r.LoadTime < TimeSpan.FromSeconds(2))
            .Where(r => r.ContentQuality > 0.5f)
            .Take(options.MaxResults)
            .ToArray();
    }
    
    public SearchMetrics CollectMetrics(SearchOperation operation)
    {
        return new SearchMetrics
        {
            QueryTime = operation.Duration,
            ResultCount = operation.Results.Length,
            CacheHitRate = operation.CacheHits / (float)operation.TotalRequests,
            AverageRelevance = operation.Results.Average(r => r.RelevanceScore),
            SourceDiversity = operation.Results.Select(r => r.Domain).Distinct().Count()
        };
    }
}
```

<!-- C# Code Example: Distributed search with load balancing -->

## Use Cases

### Research Assistance

**Academic and Professional Research**
```csharp
public class ResearchAssistant
{
    public async Task<ResearchSummary> ConductResearch(string topic, ResearchScope scope)
    {
        var searchOptions = ConfigureResearchSearch(scope);
        
        var response = await client.Chat
            .WithModel("gpt-4")
            .WithMessages($"Conduct comprehensive research on: {topic}")
            .WithWebSearch(searchOptions)
            .ExecuteAsync();
            
        return new ResearchSummary
        {
            Topic = topic,
            Summary = response.Choices[0].Message.Content,
            Sources = ExtractSources(response),
            KeyFindings = ExtractKeyFindings(response),
            ResearchDate = DateTime.UtcNow
        };
    }
    
    private WebSearchOptions ConfigureResearchSearch(ResearchScope scope)
    {
        return scope switch
        {
            ResearchScope.Academic => new WebSearchOptions
            {
                MaxResults = 20,
                ContentType = ContentType.Academic,
                AllowedDomains = new[] { "scholar.google.com", "pubmed.ncbi.nlm.nih.gov", "jstor.org" },
                VerifySourceCredibility = true
            },
            ResearchScope.Industry => new WebSearchOptions
            {
                MaxResults = 15,
                ContentType = ContentType.Articles,
                TimeRange = TimeRange.LastYear,
                MinDomainAuthority = DomainAuthority.High
            },
            ResearchScope.News => new WebSearchOptions
            {
                MaxResults = 10,
                ContentType = ContentType.News,
                TimeRange = TimeRange.LastMonth
            },
            _ => new WebSearchOptions()
        };
    }
}
```

### Current Event Awareness

**Real-time News and Updates**
```csharp
public class CurrentEventMonitor
{
    public async Task<EventSummary[]> GetLatestEvents(string[] topics, EventPriority priority)
    {
        var summaries = new List<EventSummary>();
        
        foreach (var topic in topics)
        {
            var searchOptions = new WebSearchOptions
            {
                MaxResults = priority == EventPriority.High ? 15 : 8,
                ContentType = ContentType.News,
                TimeRange = priority == EventPriority.Critical ? TimeRange.LastHour : TimeRange.LastDay,
                AllowedDomains = GetNewsSourcesByPriority(priority)
            };
            
            var response = await client.Chat
                .WithModel("gpt-4")
                .WithMessages($"Summarize the latest developments regarding: {topic}")
                .WithWebSearch(searchOptions)
                .ExecuteAsync();
                
            summaries.Add(new EventSummary
            {
                Topic = topic,
                Summary = response.Choices[0].Message.Content,
                Priority = priority,
                LastUpdated = DateTime.UtcNow
            });
        }
        
        return summaries.ToArray();
    }
}
```

### Fact Verification Workflows

**Automated Fact-Checking System**
```csharp
public class FactVerificationEngine
{
    public async Task<VerificationResult> VerifyFact(string claim)
    {
        var searchOptions = new WebSearchOptions
        {
            MaxResults = 10,
            AllowedDomains = GetFactCheckingSources(),
            VerifySourceCredibility = true,
            TimeRange = TimeRange.LastYear
        };
        
        var response = await client.Chat
            .WithModel("claude-3-sonnet")
            .WithMessages($"Verify this claim with authoritative sources: {claim}")
            .WithWebSearch(searchOptions)
            .ExecuteAsync();
            
        return new VerificationResult
        {
            Claim = claim,
            Verification = ParseVerificationResult(response),
            Sources = ExtractSources(response),
            ConfidenceLevel = CalculateConfidence(response)
        };
    }
    
    private string[] GetFactCheckingSources()
    {
        return new[]
        {
            "factcheck.org",
            "snopes.com",
            "politifact.com",
            "reuters.com/fact-check",
            "apnews.com/hub/ap-fact-check"
        };
    }
}
```

<!-- C# Code Example: Competitive intelligence gathering with web search -->
<!-- C# Code Example: Market research automation with structured data extraction -->

## Code Examples

### Web Search Integration Patterns

<!-- C# Code Example: Multi-query search strategy for comprehensive coverage -->
<!-- C# Code Example: Search result aggregation and ranking algorithms -->
<!-- C# Code Example: Real-time search monitoring and alerting system -->
<!-- C# Code Example: Search analytics and performance monitoring -->
<!-- C# Code Example: Custom search providers and API integration -->

The web search integration system provides powerful capabilities for accessing real-time information, enabling AI applications to deliver current, accurate, and well-sourced responses across a wide range of use cases.