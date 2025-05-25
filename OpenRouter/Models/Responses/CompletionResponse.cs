using System.Collections.Generic;

namespace OpenRouter.Models.Responses;

public class CompletionResponse
{
    public string Id { get; set; }
    public List<CompletionChoice> Choices { get; set; }
    public long Created { get; set; }
    public string Model { get; set; }
    public string Object { get; set; }
    public UsageData Usage { get; set; }
}

public class CompletionChoice
{
    public string Text { get; set; }
    public string FinishReason { get; set; }
    public string NativeFinishReason { get; set; }
}
