# Local LLM Chat for Visual Studio

<div align="center">

![Visual Studio](https://img.shields.io/badge/Visual%20Studio-2022-5C2D91?style=for-the-badge&logo=visualstudio&logoColor=white)
![C#](https://img.shields.io/badge/C%23-.NET%20Framework-239120?style=for-the-badge&logo=csharp&logoColor=white)
![Ollama](https://img.shields.io/badge/Ollama-Compatible-000000?style=for-the-badge&logo=ollama&logoColor=white)
![OpenAI](https://img.shields.io/badge/OpenAI-API%20Compatible-412991?style=for-the-badge&logo=openai&logoColor=white)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)

**A powerful Visual Studio extension for chatting with local Large Language Models directly in your IDE—completely private and secure**

[Features](#features) • [Installation](#installation) • [Getting Started](#getting-started) • [Commands](#commands) • [Configuration](#configuration)

</div>

---
<img src="https://github.com/markusbegerow/local-llm-chat-vs/blob/702ca02ee63646ffae1cdb13e145b269a337dc1b/Resources/local-llm-chat-visualstudio.png">

## Features

### 🤖 Local AI Chat
- **Privacy-First**: All data stays on your machine—no cloud APIs required
- **OpenAI-Compatible**: Works with Ollama, LM Studio, OpenAI, and any OpenAI-compatible endpoint
- **Persistent Conversations**: Chat history is maintained throughout your Visual Studio session
- **Integrated Tool Window**: Chat interface seamlessly integrated into Visual Studio's UI

### 📁 Solution Integration
- **Read Files**: Load any solution file into the conversation context
- **List Directories**: Browse your project structure directly from chat
- **Search Files**: Find files using wildcard patterns (e.g., `*.cs`, `*.json`)
- **Solution Info**: Get metadata about your solution (Git status, Node.js projects, etc.)
- **Smart Context**: Send active file to the AI with a single command

### ✨ File Creation & Management
- **AI-Powered File Generation**: Let the AI create complete files based on your requirements
- **Special Syntax**: AI responds with ` ```file path="relative/path.ext" ` blocks for file suggestions
- **Safe Operations**: Confirmation prompts before creating/overwriting files
- **Path Validation**: Security checks prevent file access outside your solution directory

### ⚡ Chat Commands
Built-in slash commands for quick actions:
- `/read <file-path>` - Read a file and add to conversation
- `/list [directory]` - List files in a directory
- `/search <pattern>` - Search for files with wildcard patterns
- `/workspace` - Show solution information
- `/help` - Display all available commands

## Installation

### Requirements
- **Visual Studio 2022** (version 17.0 or higher) - Community, Professional, or Enterprise
- **.NET Framework 4.7.2** or higher
- **Local LLM Server**: Ollama, LM Studio, or compatible OpenAI API endpoint

### From VSIX File
1. Download the `.vsix` file from the releases page
2. Close Visual Studio if running
3. Double-click the `.vsix` file to install
4. Or use the command line: `VSIXInstaller.exe /quiet LocalLLMChatVS.vsix`
5. Restart Visual Studio

### From Source
```bash
git clone https://github.com/markusbegerow/local-llm-chat-vs.git
cd local-llm-chat-vs
```

Open `LocalLLMChatVS.sln` in Visual Studio 2022, then press `F5` to build and launch in the experimental instance.

## Getting Started

### 1. Set Up Your Local LLM

**Option A: Ollama** (Recommended)
```bash
# Install Ollama from https://ollama.ai
ollama pull llama3.2
ollama serve
```

**Option B: LM Studio**
1. Download from [lmstudio.ai](https://lmstudio.ai)
2. Load a model
3. Start the local server (default: `http://localhost:1234/v1/chat/completions`)

**Option C: OpenAI API**
- Use directly with your OpenAI API key
- Endpoint: `https://api.openai.com/v1/chat/completions`

### 2. Configure the Extension

1. Open Visual Studio 2022
2. Go to **Tools → Options → Local LLM Chat → General**
3. Configure the following settings:
   - **API URL**: Full endpoint URL (e.g., `http://localhost:11434/v1/chat/completions`)
   - **API Token**: Your API token or "ollama" for local Ollama
   - **Model Name**: The model to use (e.g., `llama3.2`, `gpt-4`)

**Example Ollama Configuration:**
```
API URL: http://localhost:11434/v1/chat/completions
API Token: ollama
Model Name: llama3.2
```

**Example OpenAI Configuration:**
```
API URL: https://api.openai.com/v1/chat/completions
API Token: sk-your-api-key-here
Model Name: gpt-4
```

### 3. Start Chatting

1. Open a solution in Visual Studio
2. Go to **View → Other Windows → Local LLM Chat**
3. Or use the command: **Tools → Local LLM Chat → Open Chat**
4. Start asking questions!

## Usage Examples

### Analyze Code
```
/read Controllers/HomeController.cs
Can you explain how this controller works and suggest improvements?
```

### Explore Project Structure
```
/workspace
/list Controllers
What is the architecture of this project?
```

### Generate Files
```
Create a C# service class for user authentication with JWT tokens, including proper error handling and XML documentation.
```

The AI will suggest a file with path and content. Click "Yes" in the confirmation dialog to create it!

### Search and Refactor
```
/search *.cs
Find all C# files, then help me refactor the error handling patterns across the project.
```

### Quick File Analysis
1. Open any file in the editor
2. Go to **Tools → Local LLM Chat → Send Active File to Chat**
3. Ask questions about the code

### Send Specific File
1. Go to **Tools → Local LLM Chat → Send File to Chat**
2. Select the file from the dialog
3. The file content will be added to the conversation context

## Commands

### Tools Menu Commands
Access these commands from **Tools → Local LLM Chat** menu:

| Command | Description | Shortcut |
|---------|-------------|----------|
| **Open Chat** | Open the chat tool window | - |
| **Clear Conversation** | Reset the chat history | - |
| **Send Active File to Chat** | Send current file to chat | - |
| **Send File to Chat** | Browse and send any file to chat | - |

You can also access the chat window from **View → Other Windows → Local LLM Chat**

### In-Chat Slash Commands
| Command | Description | Example |
|---------|-------------|---------|
| `/read <path>` | Read file contents | `/read Program.cs` |
| `/list [dir]` | List directory files | `/list Controllers` |
| `/search <pattern>` | Find files by wildcard | `/search *.json` |
| `/workspace` | Show solution info | `/workspace` |
| `/help` | Show all commands | `/help` |

**Note:** Press `Ctrl+Enter` in the chat input to send your message.

## Configuration

Access settings through **Tools → Options → Local LLM Chat → General**

### API Configuration

| Setting | Default | Description |
|---------|---------|-------------|
| **API URL** | `http://localhost:11434/v1/chat/completions` | Full API endpoint URL |
| **API Token** | `ollama` | API authentication token (or dummy value for local) |
| **Model Name** | `llama3.2` | Model name to use |

### Model Parameters

| Setting | Default | Description |
|---------|---------|-------------|
| **Temperature** | `0.7` | Sampling temperature (0.0 = deterministic, 2.0 = very random) |
| **Max Tokens** | `2048` | Maximum tokens for model responses |
| **System Prompt** | (default) | System prompt sent to the LLM to define behavior |

### Conversation

| Setting | Default | Description |
|---------|---------|-------------|
| **Max History Messages** | `50` | Maximum messages to keep in conversation history |

### Network

| Setting | Default | Description |
|---------|---------|-------------|
| **Request Timeout (ms)** | `120000` | Request timeout in milliseconds (2 minutes) |

### Security

| Setting | Default | Description |
|---------|---------|-------------|
| **Max File Size (bytes)** | `1048576` | Maximum file size for LLM-generated files (1MB) |
| **Allow Write Without Prompt** | `false` | NOT recommended - allows file creation without confirmation |

### Configuration Examples

**Ollama (Local)**
```
API URL: http://localhost:11434/v1/chat/completions
API Token: ollama
Model Name: llama3.2
```

**LM Studio (Local)**
```
API URL: http://localhost:1234/v1/chat/completions
API Token: lmstudio
Model Name: your-model-name
```

**OpenAI (Cloud)**
```
API URL: https://api.openai.com/v1/chat/completions
API Token: sk-your-actual-api-key-here
Model Name: gpt-4
```

## Security Features

- ✅ **Path Traversal Protection**: Prevents access outside solution directory
- ✅ **File Size Limits**: Configurable maximum file sizes (default: 1MB)
- ✅ **Path Validation**: All file paths validated before operations
- ✅ **URL Validation**: API endpoints validated for security
- ✅ **Confirmation Prompts**: Review before creating/overwriting files
- ✅ **Local-Only Default**: No external API calls unless you configure them
- ✅ **Binary/Build Exclusion**: Automatically excludes bin/, obj/, node_modules/, .git/

## Requirements

- **Visual Studio 2022**: Version 17.0 or higher (Community, Professional, or Enterprise)
- **.NET Framework**: Version 4.7.2 or higher
- **Local LLM**: Ollama, LM Studio, or compatible OpenAI API server

## Development

### Building from Source

```bash
# Clone repository
git clone https://github.com/markusbegerow/local-llm-chat-vs.git
cd local-llm-chat-vs
```

1. Open `LocalLLMChatVS.sln` in Visual Studio 2022
2. Restore NuGet packages (right-click solution → Restore NuGet Packages)
3. Build the solution (`Ctrl+Shift+B`)
4. Press `F5` to launch in experimental instance

### Packaging as VSIX

1. Build the project in **Release** mode
2. The VSIX file will be generated in `bin\Release\LocalLLMChatVS.vsix`
3. Install it by double-clicking the VSIX file

### Project Structure

```
LocalLLMChatVS/
├── Commands/
│   ├── OpenChatCommand.cs           # Open chat window command
│   ├── ClearConversationCommand.cs  # Clear chat command
│   ├── SendFileToChatCommand.cs     # Send file command
│   └── SendActiveFileToChatCommand.cs # Send active file command
├── Services/
│   ├── LLMService.cs                # LLM API integration
│   └── WorkspaceService.cs          # Solution/file operations
├── ToolWindows/
│   ├── ChatWindow.cs                # Chat tool window
│   └── ChatWindowControl.xaml.cs    # Chat UI control (WPF)
├── Models/
│   └── ChatMessage.cs               # Data models
├── Utilities/
│   ├── PathValidator.cs             # Path security validation
│   └── SecurityValidator.cs         # Security utilities
├── Options/
│   └── GeneralOptions.cs            # Settings page
├── LocalLLMChatPackage.cs           # Main package class
├── source.extension.vsixmanifest    # Extension manifest
└── VSCommandTable.vsct              # Command definitions
```

## Troubleshooting

### Connection Issues
- **Verify LLM server is running:**
  - Ollama: `curl http://localhost:11434/v1/models` or `ollama list`
  - LM Studio: Check the server tab shows "Server Running"
- **Check API URL in settings:**
  - Go to **Tools → Options → Local LLM Chat → General**
  - Verify the API URL matches your server
  - Ensure the endpoint includes `/v1/chat/completions`
- **Test with curl:**
  ```bash
  curl http://localhost:11434/v1/models
  ```

### Chat Not Responding
- **Check error messages** in the chat window or Visual Studio Output window
- **Increase timeout:** Go to **Tools → Options → Local LLM Chat → General** and increase "Request Timeout"
- **Verify model is loaded** in your LLM server
- **Check API token** is correct (use "ollama" or any dummy value for local servers)

### File Operations Failing
- **Ensure a solution is open** in Visual Studio
- **Check file paths are relative** to the solution directory (no absolute paths)
- **Verify file size limits** in **Tools → Options → Local LLM Chat → Security**
- **Check permissions** on the solution directory

### Extension Not Loading
- **Check Visual Studio version**: Must be 2022 (17.0+)
- **Verify .NET Framework 4.7.2** or higher is installed
- **Check Extensions → Manage Extensions** to see if extension is enabled
- **Try resetting Visual Studio settings**: Tools → Import and Export Settings

## Recommended Models

### Coding Tasks (C# / .NET)
- **Qwen 2.5 Coder (7B/14B)**: Excellent for C# and .NET development
- **CodeLlama (13B/34B)**: Specialized for code generation and understanding
- **DeepSeek Coder (6.7B/33B)**: Strong at complex algorithms and refactoring
- **Llama 3.2 (3B/8B)**: Good balance of speed and capability

### General Tasks
- **Llama 3.2**: Best all-around performance for most tasks
- **Mistral 7B**: Fast and efficient for quick questions
- **Phi-3**: Compact but capable for lighter workloads

### Cloud/Enterprise
- **GPT-4 / GPT-4 Turbo**: Best quality, requires OpenAI API key
- **GPT-3.5 Turbo**: Faster and cheaper alternative

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Development Guidelines
- Follow C# coding conventions
- Test in Visual Studio 2022 experimental instance
- Update documentation for new features
- Ensure security validators are used for file operations

## License

This project is licensed under the GPL License - see the [LICENSE.txt](LICENSE.txt) file for details.

## Acknowledgments

- Thanks to the Ollama team for making local LLMs accessible
- LM Studio for providing an excellent local inference platform
- The Visual Studio extensibility team for comprehensive SDK and documentation
- OpenAI for the standardized API format

## 🙋‍♂️ Get Involved

If you encounter any issues or have questions:
- 🐛 [Report bugs](https://github.com/markusbegerow/local-llm-chat-vs/issues)
- 💡 [Request features](https://github.com/markusbegerow/local-llm-chat-vs/issues)
- ⭐ Star the repo if you find it useful!

## ☕ Support the Project

If you like this project, support further development with a repost or coffee:

<a href="https://www.linkedin.com/sharing/share-offsite/?url=https://github.com/markusbegerow/local-llm-chat-vs" target="_blank"> <img src="https://img.shields.io/badge/💼-Share%20on%20LinkedIn-blue" /> </a>

[![Buy Me a Coffee](https://img.shields.io/badge/☕-Buy%20me%20a%20coffee-yellow)](https://paypal.me/MarkusBegerow?country.x=DE&locale.x=de_DE)

## 📬 Contact

- 🧑‍💻 [Markus Begerow](https://linkedin.com/in/markusbegerow)
- 💾 [GitHub](https://github.com/markusbegerow)
- ✉️ [Twitter](https://x.com/markusbegerow)

---

**Privacy Notice**: This extension operates entirely locally by default. No data is sent to external servers unless you explicitly configure it to use a remote API endpoint (like OpenAI).
