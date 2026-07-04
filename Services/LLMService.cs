using System;
using System.Collections.Generic;
using System.IO;
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
        private static readonly HttpClient httpClient = new HttpClient()
        {
            Timeout = System.Threading.Timeout.InfiniteTimeSpan
        };

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
            var requestObj = new JObject
            {
                ["model"] = options.ModelName,
                ["messages"] = JArray.FromObject(messages.Select(m => new { role = m.Role, content = m.Content }).ToArray()),
                ["stream"] = false
            };

            if (options.SendTemperature) requestObj["temperature"] = options.Temperature;
            if (options.SendMaxTokens) requestObj["max_tokens"] = options.MaxTokens;
            if (options.SendTopP) requestObj["top_p"] = options.TopP;
            if (options.SendPresencePenalty) requestObj["presence_penalty"] = options.PresencePenalty;
            if (options.SendFrequencyPenalty) requestObj["frequency_penalty"] = options.FrequencyPenalty;

            if (options.EnableOllamaParameters)
            {
                requestObj["top_k"] = options.TopK;
                requestObj["min_p"] = options.MinP;
                requestObj["repeat_penalty"] = options.RepeatPenalty;
            }

            string jsonRequest = JsonConvert.SerializeObject(requestObj);

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
                                $"LLM API error ({(int)response.StatusCode}): {errorText}\n\nRequest sent:\n{jsonRequest}");
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
                            content = choices[0]["message"]?["reasoning_content"]?.ToString();
                        }

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
        /// Calls the LLM API with streaming enabled, firing callbacks for each token.
        /// Returns the full concatenated content when done.
        /// </summary>
        public async Task<string> CallLLMStreamAsync(
            List<ChatMessage> messages,
            GeneralOptions options,
            Action<string> onToken,
            Action<string> onThinking,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(options.ModelName))
                throw new InvalidOperationException("Model name is not configured. Please set it in Tools > Options > Local LLM Chat.");

            if (string.IsNullOrWhiteSpace(options.ApiUrl))
                throw new InvalidOperationException("API URL is not configured. Please set it in Tools > Options > Local LLM Chat.");

            if (!SecurityValidator.ValidateUrl(options.ApiUrl))
                throw new InvalidOperationException($"Invalid API URL: {options.ApiUrl}");

            var requestObj = new JObject
            {
                ["model"] = options.ModelName,
                ["messages"] = JArray.FromObject(messages.Select(m => new { role = m.Role, content = m.Content }).ToArray()),
                ["stream"] = true
            };

            if (options.SendTemperature) requestObj["temperature"] = options.Temperature;
            if (options.SendMaxTokens) requestObj["max_tokens"] = options.MaxTokens;
            if (options.SendTopP) requestObj["top_p"] = options.TopP;
            if (options.SendPresencePenalty) requestObj["presence_penalty"] = options.PresencePenalty;
            if (options.SendFrequencyPenalty) requestObj["frequency_penalty"] = options.FrequencyPenalty;

            if (options.EnableOllamaParameters)
            {
                requestObj["top_k"] = options.TopK;
                requestObj["min_p"] = options.MinP;
                requestObj["repeat_penalty"] = options.RepeatPenalty;
            }

            string jsonRequest = JsonConvert.SerializeObject(requestObj);

            using (var httpRequest = new HttpRequestMessage(HttpMethod.Post, options.ApiUrl))
            {
                httpRequest.Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                if (!string.IsNullOrWhiteSpace(options.ApiToken))
                    httpRequest.Headers.Add("Authorization", $"Bearer {options.ApiToken}");

                using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    cts.CancelAfter(options.RequestTimeout);

                    try
                    {
                        var response = await httpClient.SendAsync(
                            httpRequest,
                            HttpCompletionOption.ResponseHeadersRead,
                            cts.Token);

                        if (!response.IsSuccessStatusCode)
                        {
                            string errorText = await response.Content.ReadAsStringAsync();
                            throw new HttpRequestException(
                                $"LLM API error ({(int)response.StatusCode}): {errorText}\n\nRequest sent:\n{jsonRequest}");
                        }

                        var contentBuilder = new StringBuilder();
                        var thinkingBuilder = new StringBuilder();

                        using (var stream = await response.Content.ReadAsStreamAsync())
                        using (var reader = new StreamReader(stream))
                        {
                            while (!reader.EndOfStream)
                            {
                                string line = await reader.ReadLineAsync();
                                if (string.IsNullOrWhiteSpace(line)) continue;
                                if (!line.StartsWith("data: ")) continue;

                                string data = line.Substring(6).Trim();
                                if (data == "[DONE]") break;

                                JObject chunk;
                                try { chunk = JObject.Parse(data); }
                                catch { continue; }

                                var delta = chunk["choices"]?[0]?["delta"];
                                if (delta == null) continue;

                                string tokenContent = delta["content"]?.ToString();
                                string tokenThinking = delta["reasoning_content"]?.ToString();

                                if (!string.IsNullOrEmpty(tokenContent))
                                {
                                    contentBuilder.Append(tokenContent);
                                    onToken?.Invoke(tokenContent);
                                }

                                if (!string.IsNullOrEmpty(tokenThinking))
                                {
                                    thinkingBuilder.Append(tokenThinking);
                                    onThinking?.Invoke(tokenThinking);
                                }
                            }
                        }

                        string result = contentBuilder.ToString().Trim();

                        if (string.IsNullOrEmpty(result))
                        {
                            result = thinkingBuilder.ToString().Trim();
                            if (string.IsNullOrEmpty(result))
                                throw new InvalidOperationException("Empty response from LLM");
                        }

                        return result;
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
