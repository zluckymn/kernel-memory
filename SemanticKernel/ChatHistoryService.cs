using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

public class ChatHistoryService
{
    private readonly Kernel _summarizationKernel;
    private readonly List<string> _database = new(); // In-memory database simulation

    public ChatHistoryService([FromKeyedServices("OpenAI_Translation")] IChatCompletionService service)
    {
        // Correctly build the Kernel by adding the specific service to a new builder.
        var builder = Kernel.CreateBuilder();
        builder.Services.AddSingleton<IChatCompletionService>(service);
        _summarizationKernel = builder.Build();
    }

    public async Task<string> ReduceHistoryAsync(List<ChatMessageContent> history)
    {
        if (history == null || !history.Any())
        {
            return "No history available.";
        }

        var historyText = string.Join("\n", history.Select(h => $"{h.Role}: {h.Content}"));
        const int tokenThreshold = 500;

        if (historyText.Length < tokenThreshold)
        {
            return historyText;
        }

        var result = await _summarizationKernel.InvokePromptAsync(
            $"Summarize this conversation concisely: {historyText}");
        
        var summary = result.GetValue<string>() ?? "";
        Console.WriteLine($"--- History Summarized ---\n{summary}\n------------------------");
        return summary;
    }

    public void SaveHistory(string userQuery, string assistantResponse)
    {
        _database.Add($"User: {userQuery}");
        _database.Add($"Assistant: {assistantResponse}");
        Console.WriteLine("--- History Saved ---");
        Console.WriteLine($"Current history entries: {_database.Count}");
        Console.WriteLine("---------------------");
    }
}