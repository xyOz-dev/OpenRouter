namespace OpenRouter.Models.Requests;

public class CompletionRequest
{
    public string Model { get; set; }
    public string Prompt { get; set; }

    // Add other common parameters from ChatCompletionRequest if applicable
    // based on the /v1/completions documentation and needs for consistency.
    // For now, stick to the minimum required: Model and Prompt.
}