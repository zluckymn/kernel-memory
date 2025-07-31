public class OpenAIStreamResponse
{
    public Choice[]? choices { get; set; }

    public class Choice
    {
        public Delta? delta { get; set; }
    }

    public class Delta
    {
        public string? content { get; set; }
    }
}
