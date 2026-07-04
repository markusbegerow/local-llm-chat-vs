using System;
using System.Runtime.InteropServices;
using System.Threading;
using LocalLLMChatVS.Commands;
using LocalLLMChatVS.Options;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace LocalLLMChatVS
{
    /// <summary>
    /// Main package class for Local LLM Chat extension
    /// </summary>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("Local LLM Chat for Visual Studio", "Secure chat with local LLMs and create files in your workspace with AI assistance.", "1.0.0")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.LocalLLMChatPackageString)]
    [ProvideOptionPage(typeof(GeneralOptions), "Local LLM Chat", "General", 0, 0, true)]
    [ProvideToolWindow(typeof(ToolWindows.ChatWindow))]
    public sealed class LocalLLMChatPackage : AsyncPackage
    {
        public static LocalLLMChatPackage Instance { get; private set; }

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            Instance = this;

            // DialogPage only loads persisted settings from storage when the shell
            // is about to display it in Tools > Options. Without this, a chat request
            // sent before the user ever opens Options in this session would read the
            // hardcoded property defaults instead of previously saved values.
            ((GeneralOptions)GetDialogPage(typeof(GeneralOptions))).LoadSettingsFromStorage();

            await OpenChatCommand.InitializeAsync(this);
            await ClearConversationCommand.InitializeAsync(this);
            await SendFileToChatCommand.InitializeAsync(this);
            await SendActiveFileToChatCommand.InitializeAsync(this);
        }
    }

    /// <summary>
    /// Package GUIDs
    /// </summary>
    public static class PackageGuids
    {
        public const string LocalLLMChatPackageString = "7D8E9F0A-1B2C-4D5E-8F9A-0B1C2D3E4F5A";
        public const string LocalLLMChatPackageCmdSetString = "8E9F0A1B-2C3D-4E5F-9A0B-1C2D3E4F5A6B";

        public static readonly Guid LocalLLMChatPackage = new Guid(LocalLLMChatPackageString);
        public static readonly Guid LocalLLMChatPackageCmdSet = new Guid(LocalLLMChatPackageCmdSetString);
    }

    /// <summary>
    /// Command IDs
    /// </summary>
    public static class PackageIds
    {
        public const int OpenChatCommand = 0x0100;
        public const int ClearConversationCommand = 0x0101;
        public const int SendFileToChatCommand = 0x0102;
        public const int SendActiveFileToChatCommand = 0x0103;
    }
}
