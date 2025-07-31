using System;

namespace SemanticKernel.Models
{
    public class OpenAIStreamResponse
    {
        public string id { get; set; } = Guid.NewGuid().ToString();
        public string @object { get; set; } = "chat.completion.chunk";
        public long created { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public string model { get; set; } = "gpt-3.5-turbo";
        public Choice[] choices { get; set; } = Array.Empty<Choice>();

        public class Choice
        {
            public Delta delta { get; set; } = new();
            public string? finish_reason { get; set; }
            public int index { get; set; }
        }

        public class Delta
        {
            public string? role { get; set; }
            public string? content { get; set; }
            public string? name { get; set; }
            public string? function_call { get; set; }
            public ToolCall[]? tool_calls { get; set; }
        }

        public class ToolCall
        {
            public string id { get; set; } = string.Empty;
            public string type { get; set; } = string.Empty;
            public string function { get; set; } = string.Empty;
        }
    }
}
