using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LocalLLMChatVS.Models;
using LocalLLMChatVS.Options;
using LocalLLMChatVS.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LocalLLMChatVS.Services
{
    /// <summary>
    /// Service for interacting with LLM APIs (OpenAI-compatible format)
    /// </summary>
    public class LLMService
    {
        private static readonly HttpClient httpClient = new HttpClient();

        /// <summary>
        /// Calls the LLM API with the provided messages
        /// </summary>
        public async Task<string> CallLLMAsync(
            List<ChatMessage> messages,
            GeneralOptions options,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(options.ModelName))
            {
                throw new InvalidOperationException("Model name is not configured. Please set it in Tools > Options > Local LLM Chat.");
            }

            if (string.IsNullOrWhiteSpace(options.ApiUrl))
            {
                throw new InvalidOperationException("API URL is not configured. Please set it in Tools > Options > Local LLM Chat.");
            }

            if (!SecurityValidator.ValidateUrl(options.ApiUrl))
            {
                throw new InvalidOperationException($"Invalid API URL: {options.ApiUrl}");
            }

            // Build request
            var request = new
            {
                model = options.ModelName,
                messages = messages.Select(m => new { role = m.Role, content = m.Content }).ToArray(),
                temperature = options.Temperature,
                max_tokens = options.MaxTokens,
                stream = false
            };

            string jsonRequest = JsonConvert.SerializeObject(request);

            using (var httpRequest = new HttpRequestMessage(HttpMethod.Post, options.ApiUrl))
            {
                httpRequest.Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                // Add authorization header if token is provided
                if (!string.IsNullOrWhiteSpace(options.ApiToken))
                {
                    httpRequest.Headers.Add("Authorization", $"Bearer {options.ApiToken}");
                }

                // Set timeout
                using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    cts.CancelAfter(options.RequestTimeout);

                    try
                    {
                        var response = await httpClient.SendAsync(httpRequest, cts.Token);

                        if (!response.IsSuccessStatusCode)
                        {
                            string errorText = await response.Content.ReadAsStringAsync();
                            throw new HttpRequestException(
                                $"LLM API error ({(int)response.StatusCode}): {errorText}");
                        }

                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        var responseObj = JObject.Parse(jsonResponse);

                        // Extract content from response
                        var choices = responseObj["choices"] as JArray;
                        if (choices == null || choices.Count == 0)
                        {
                            throw new InvalidOperationException("No response choices returned from LLM");
                        }

                        string content = choices[0]["message"]?["content"]?.ToString();
                        if (string.IsNullOrEmpty(content))
                        {
                            throw new InvalidOperationException("Empty response from LLM");
                        }

                        return content.Trim();
                    }
                    catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
                    {
                        throw new TimeoutException($"Request timed out after {options.RequestTimeout / 1000} seconds");
                    }
                }
            }
        }

        /// <summary>
        /// Trims message history to maximum length
        /// </summary>
        public List<ChatMessage> TrimMessageHistory(List<ChatMessage> messages, int maxMessages)
        {
            if (messages.Count <= maxMessages)
            {
                return new List<ChatMessage>(messages);
            }

            // Always keep the system message if present
            var systemMessages = messages.Where(m => m.Role == "system").ToList();
            var otherMessages = messages.Where(m => m.Role != "system").ToList();

            // Keep the most recent messages
            var recentMessages = otherMessages.Skip(Math.Max(0, otherMessages.Count - maxMessages + systemMessages.Count)).ToList();

            return systemMessages.Concat(recentMessages).ToList();
        }

        /// <summary>
        /// Extracts file suggestions from LLM response
        /// Format: ```file path="relative/path.ext"
        /// content here
        /// ```
        /// </summary>
        public List<FileSuggestion> ExtractFileSuggestions(string text)
        {
            var suggestions = new List<FileSuggestion>();

            // Regular expression to match file fence blocks
            var regex = new System.Text.RegularExpressions.Regex(
                @"```file\s+path=""([^""]+)""[\r\n]+([\s\S]*?)```",
                System.Text.RegularExpressions.RegexOptions.Multiline);

            var matches = regex.Matches(text);
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                if (match.Groups.Count >= 3)
                {
                    string path = match.Groups[1].Value.Trim();
                    string content = match.Groups[2].Value;

                    if (!string.IsNullOrEmpty(path))
                    {
                        suggestions.Add(new FileSuggestion(path, content));
                    }
                }
            }

            return suggestions;
        }
    }
}
