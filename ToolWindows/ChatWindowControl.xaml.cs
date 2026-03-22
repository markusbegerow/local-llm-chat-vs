using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LocalLLMChatVS.Models;
using LocalLLMChatVS.Options;
using LocalLLMChatVS.Services;
using Microsoft.VisualStudio.Shell;

namespace LocalLLMChatVS.ToolWindows
{
    public partial class ChatWindowControl : UserControl
    {
        private readonly ObservableCollection<MessageDisplay> displayMessages = new ObservableCollection<MessageDisplay>();
        private readonly List<ChatMessage> chatMessages = new List<ChatMessage>();
        private readonly LLMService llmService = new LLMService();
        private readonly WorkspaceService workspaceService = new WorkspaceService();

        public ChatWindowControl()
        {
            InitializeComponent();
            MessagesContainer.ItemsSource = displayMessages;
            ResetConversation();
        }

        private void ResetConversation()
        {
            chatMessages.Clear();
            var options = GetOptions();
            chatMessages.Add(new ChatMessage("system", options.SystemPrompt));
        }

        private GeneralOptions GetOptions()
        {
            var package = LocalLLMChatPackage.Instance;
            return (GeneralOptions)package.GetDialogPage(typeof(GeneralOptions));
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            var msg = (sender as Button)?.DataContext as MessageDisplay;
            if (msg != null)
                Clipboard.SetText(msg.Content);
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            displayMessages.Clear();
            ResetConversation();
            MessageBox.Show("Conversation cleared.", "Local LLM Chat", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            _ = SendMessageAsync();
        }

        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Enter to send, Shift+Enter for new line
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;
                _ = SendMessageAsync();
            }
        }

        private async Task SendMessageAsync()
        {
            string text = InputTextBox.Text?.Trim();
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            // Clear input
            InputTextBox.Text = string.Empty;

            // Display user message
            AppendMessage("user", text);

            try
            {
                // Check for special commands
                if (text.StartsWith("/"))
                {
                    await HandleCommandAsync(text);
                    return;
                }

                // Add to chat history
                chatMessages.Add(new ChatMessage("user", text));

                // Get options
                var options = GetOptions();

                // Trim history
                var trimmedMessages = llmService.TrimMessageHistory(chatMessages, options.MaxHistoryMessages);
                chatMessages.Clear();
                chatMessages.AddRange(trimmedMessages);

                // Call LLM
                string response = await llmService.CallLLMAsync(chatMessages, options);

                // Add assistant response
                chatMessages.Add(new ChatMessage("assistant", response));
                AppendMessage("assistant", response);

                // Extract and suggest files
                var fileSuggestions = llmService.ExtractFileSuggestions(response);
                foreach (var file in fileSuggestions)
                {
                    await SuggestFileCreationAsync(file);
                }
            }
            catch (Exception ex)
            {
                AppendMessage("error", $"Error: {ex.Message}");
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task HandleCommandAsync(string command)
        {
            try
            {
                string[] parts = command.Substring(1).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0)
                {
                    return;
                }

                string cmd = parts[0].ToLower();
                string[] args = parts.Skip(1).ToArray();

                switch (cmd)
                {
                    case "read":
                        await CommandReadFileAsync(args);
                        break;

                    case "list":
                        await CommandListFilesAsync(args);
                        break;

                    case "search":
                        await CommandSearchFilesAsync(args);
                        break;

                    case "workspace":
                    case "info":
                        await CommandWorkspaceInfoAsync();
                        break;

                    case "help":
                        CommandHelp();
                        break;

                    default:
                        AppendMessage("system", $"Unknown command: /{cmd}\nType /help for available commands.");
                        break;
                }
            }
            catch (Exception ex)
            {
                AppendMessage("error", $"Command error: {ex.Message}");
            }
        }

        private async Task CommandReadFileAsync(string[] args)
        {
            if (args.Length == 0)
            {
                AppendMessage("system", "Usage: /read <file-path>\nExample: /read src/Program.cs");
                return;
            }

            string filePath = string.Join(" ", args);
            string content = await workspaceService.ReadFileAsync(filePath);

            string message = $"File \"{filePath}\":\n\n```\n{content}\n```";
            AppendMessage("system", message);

            // Add to context for LLM
            chatMessages.Add(new ChatMessage("user",
                $"I'm showing you the content of file \"{filePath}\":\n\n```\n{content}\n```"));
        }

        private async Task CommandListFilesAsync(string[] args)
        {
            string dirPath = args.Length > 0 ? string.Join(" ", args) : "";
            var files = await workspaceService.ListFilesAsync(dirPath, recursive: false);

            if (files.Count == 0)
            {
                AppendMessage("system", $"No files found in \"{dirPath}\"");
                return;
            }

            string output = $"Files in \"{(string.IsNullOrEmpty(dirPath) ? "solution root" : dirPath)}\" ({files.Count} items):\n\n";
            foreach (var file in files)
            {
                string icon = file.Type == FileEntryType.Directory ? "📁" : "📄";
                output += $"{icon} {file.Name}\n";
            }

            AppendMessage("system", output);
        }

        private async Task CommandSearchFilesAsync(string[] args)
        {
            if (args.Length == 0)
            {
                AppendMessage("system", "Usage: /search <pattern>\nExample: /search *.cs");
                return;
            }

            string pattern = string.Join(" ", args);
            var files = await workspaceService.SearchFilesAsync(pattern, 50);

            if (files.Count == 0)
            {
                AppendMessage("system", $"No files found matching \"{pattern}\"");
                return;
            }

            string output = $"Files matching \"{pattern}\" ({files.Count} results):\n\n";
            foreach (var file in files)
            {
                output += $"📄 {file}\n";
            }

            if (files.Count == 50)
            {
                output += "\n(Limited to first 50 results)";
            }

            AppendMessage("system", output);
        }

        private async Task CommandWorkspaceInfoAsync()
        {
            var metadata = await workspaceService.GetWorkspaceMetadataAsync();

            string output = "Workspace Information:\n\n";
            output += $"📁 Name: {metadata.Name}\n";
            output += $"📂 Path: {metadata.Path}\n";
            output += $"{(metadata.HasGit ? "✅" : "❌")} Git Repository\n";
            output += $"{(metadata.HasPackageJson ? "✅" : "❌")} Node.js Project\n";

            AppendMessage("system", output);
        }

        private void CommandHelp()
        {
            string helpText = @"Available Commands:

📄 /read <file-path>
   Read a file and add to conversation context
   Example: /read src/Program.cs

📁 /list [directory]
   List files in a directory (default: root)
   Example: /list src

🔍 /search <pattern>
   Search for files matching a pattern
   Example: /search *.json

ℹ️  /workspace
   Show workspace information

❓ /help
   Show this help message";

            AppendMessage("system", helpText);
        }

        private async Task SuggestFileCreationAsync(FileSuggestion file)
        {
            var result = MessageBox.Show(
                $"The LLM suggests creating file: {file.Path}\n\nDo you want to create/overwrite this file?",
                "Create/Overwrite File",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var options = GetOptions();
                    await workspaceService.WriteFileAsync(file.Path, file.Content, options.MaxFileSize);

                    AppendMessage("system", $"Created file: {file.Path}");
                    MessageBox.Show($"File created: {file.Path}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    AppendMessage("error", $"Failed to create file: {ex.Message}");
                    MessageBox.Show($"Failed to create file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void AppendMessage(string role, string content)
        {
            displayMessages.Add(new MessageDisplay { Role = role, Content = content });

            // Scroll to bottom
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                MessagesScrollViewer.ScrollToEnd();
            });
        }

        public void SendMessageToChat(string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                AppendMessage("user", text);
                _ = Task.Run(async () =>
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    chatMessages.Add(new ChatMessage("user", text));

                    try
                    {
                        var options = GetOptions();
                        string response = await llmService.CallLLMAsync(chatMessages, options);
                        chatMessages.Add(new ChatMessage("assistant", response));
                        AppendMessage("assistant", response);
                    }
                    catch (Exception ex)
                    {
                        AppendMessage("error", $"Error: {ex.Message}");
                    }
                });
            }
        }
    }

    public class MessageDisplay
    {
        public string Role { get; set; }
        public string Content { get; set; }
    }
}
