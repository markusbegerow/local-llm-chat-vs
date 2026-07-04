using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace LocalLLMChatVS.Options
{
    /// <summary>
    /// Options page for configuring Local LLM Chat
    /// </summary>
    [ComVisible(true)]
    [Guid("4A5B6C7D-8E9F-4A5B-8C9D-0E1F2A3B4C5D")]
    public class GeneralOptions : DialogPage, ICustomTypeDescriptor
    {
        [Category("API Configuration")]
        [DisplayName("API URL")]
        [Description("Full API endpoint URL. Examples:\n- OpenAI: https://api.openai.com/v1/chat/completions\n- Ollama: http://localhost:11434/v1/chat/completions\n- Custom: http://localhost:1234/v1/chat/completions")]
        [DefaultValue("http://localhost:11434/v1/chat/completions")]
        public string ApiUrl { get; set; } = "http://localhost:11434/v1/chat/completions";

        [Category("API Configuration")]
        [DisplayName("API Token")]
        [Description("API authentication token. Examples:\n- OpenAI: sk-your-openai-api-key-here\n- Ollama: ollama (or any dummy value)\n- Custom: your-token-or-dummy-value")]
        [DefaultValue("ollama")]
        [PasswordPropertyText(true)]
        public string ApiToken { get; set; } = "ollama";

        [Category("API Configuration")]
        [DisplayName("Model Name")]
        [Description("Model name to use. Examples:\n- OpenAI: gpt-4, gpt-3.5-turbo, gpt-4-turbo\n- Ollama: llama3.2, mistral, codellama\n- Custom: your-model-name")]
        [DefaultValue("llama3.2")]
        public string ModelName { get; set; } = "llama3.2";

        [Category("Model Parameters")]
        [DisplayName("Temperature")]
        [Description("Sampling temperature for model responses (0.0 = deterministic, 2.0 = very random)")]
        [DefaultValue(0.7)]
        public double Temperature { get; set; } = 0.7;

        [Category("Model Parameters")]
        [DisplayName("Send Temperature")]
        [Description("Include temperature in requests. Disable if your backend rejects this parameter.")]
        [DefaultValue(true)]
        public bool SendTemperature { get; set; } = true;

        [Category("Model Parameters")]
        [DisplayName("Max Tokens")]
        [Description("Maximum tokens for model responses")]
        [DefaultValue(2048)]
        public int MaxTokens { get; set; } = 2048;

        [Category("Model Parameters")]
        [DisplayName("Send Max Tokens")]
        [Description("Include max_tokens in requests. Disable if your backend rejects this parameter.")]
        [DefaultValue(true)]
        public bool SendMaxTokens { get; set; } = true;

        [Category("Model Parameters")]
        [DisplayName("Top P")]
        [Description("Nucleus sampling probability (0.0–1.0). Lower values make output more focused. Default: 1.0 (disabled)")]
        [DefaultValue(1.0)]
        public double TopP { get; set; } = 1.0;

        [Category("Model Parameters")]
        [DisplayName("Send Top P")]
        [Description("Include top_p in requests. Disable if your backend rejects this parameter.")]
        [DefaultValue(true)]
        public bool SendTopP { get; set; } = true;

        [Category("Model Parameters")]
        [DisplayName("Presence Penalty")]
        [Description("Penalises tokens that have already appeared (-2.0 to 2.0). Positive values encourage new topics. Default: 0.0")]
        [DefaultValue(0.0)]
        public double PresencePenalty { get; set; } = 0.0;

        [Category("Model Parameters")]
        [DisplayName("Send Presence Penalty")]
        [Description("Include presence_penalty in requests. Disable if your backend rejects this parameter.")]
        [DefaultValue(true)]
        public bool SendPresencePenalty { get; set; } = true;

        [Category("Model Parameters")]
        [DisplayName("Frequency Penalty")]
        [Description("Penalises repeated tokens (-2.0 to 2.0). Positive values reduce repetition. Default: 0.0")]
        [DefaultValue(0.0)]
        public double FrequencyPenalty { get; set; } = 0.0;

        [Category("Model Parameters")]
        [DisplayName("Send Frequency Penalty")]
        [Description("Include frequency_penalty in requests. Disable if your backend rejects this parameter.")]
        [DefaultValue(true)]
        public bool SendFrequencyPenalty { get; set; } = true;

        [Category("Model Parameters (Ollama)")]
        [DisplayName("Enable Ollama Parameters")]
        [Description("Send Ollama-specific parameters (Top K, Min P, Repeat Penalty). Disable when using OpenAI or other providers that reject unknown fields.")]
        [DefaultValue(false)]
        public bool EnableOllamaParameters { get; set; } = false;

        [Category("Model Parameters (Ollama)")]
        [DisplayName("Top K")]
        [Description("Limits the next token selection to the K most probable tokens. Ollama-specific. Default: 40")]
        [DefaultValue(40)]
        public int TopK { get; set; } = 40;

        [Category("Model Parameters (Ollama)")]
        [DisplayName("Min P")]
        [Description("Minimum probability for a token to be considered (0.0–1.0). Ollama-specific. Default: 0.0")]
        [DefaultValue(0.0)]
        public double MinP { get; set; } = 0.0;

        [Category("Model Parameters (Ollama)")]
        [DisplayName("Repeat Penalty")]
        [Description("Penalty for repeated tokens. Values > 1.0 discourage repetition. Ollama-specific. Default: 1.1")]
        [DefaultValue(1.1)]
        public double RepeatPenalty { get; set; } = 1.1;

        [Category("Model Parameters")]
        [DisplayName("System Prompt")]
        [Description("System prompt sent to the LLM to define its behavior")]
        [DefaultValue("You are a helpful coding assistant inside Visual Studio. Keep answers concise. When proposing file content, respond with a fenced code block beginning with ```file path=\"relative/path.ext\" followed by the complete file content.")]
        public string SystemPrompt { get; set; } = "You are a helpful coding assistant inside Visual Studio. Keep answers concise. When proposing file content, respond with a fenced code block beginning with ```file path=\"relative/path.ext\" followed by the complete file content.";

        [Category("Conversation")]
        [DisplayName("Max History Messages")]
        [Description("Maximum number of messages to keep in conversation history")]
        [DefaultValue(50)]
        public int MaxHistoryMessages { get; set; } = 50;

        [Category("API Configuration")]
        [DisplayName("Enable Streaming")]
        [Description("Stream tokens as they are generated instead of waiting for the full response. Requires an OpenAI-compatible streaming endpoint.")]
        [DefaultValue(true)]
        public bool EnableStreaming { get; set; } = true;

        [Category("API Configuration")]
        [DisplayName("Show Thinking Content")]
        [Description("Display the model's internal reasoning/thinking blocks (e.g. DeepSeek R1, QwQ) above the response when available.")]
        [DefaultValue(true)]
        public bool ShowThinkingContent { get; set; } = true;

        [Category("Network")]
        [DisplayName("Request Timeout (ms)")]
        [Description("Request timeout in milliseconds (default: 120000 = 2 minutes)")]
        [DefaultValue(120000)]
        public int RequestTimeout { get; set; } = 120000;

        [Category("Security")]
        [DisplayName("Max File Size (bytes)")]
        [Description("Maximum file size in bytes for LLM-generated files (default: 1MB)")]
        [DefaultValue(1048576)]
        public int MaxFileSize { get; set; } = 1048576;

        [Category("Security")]
        [DisplayName("Allow Write Without Prompt")]
        [Description("If enabled, allow /write command to create/update files without confirmation. NOT recommended for security.")]
        [DefaultValue(false)]
        public bool AllowWriteWithoutPrompt { get; set; } = false;

        // Fixed display order for the Model Parameters section: each on/off toggle
        // immediately followed by the value it controls. PropertyGrid otherwise sorts
        // alphabetically within a category, which scatters these pairs unpredictably.
        private static readonly string[] ModelParameterDisplayOrder =
        {
            nameof(SendTemperature), nameof(Temperature),
            nameof(SendMaxTokens), nameof(MaxTokens),
            nameof(SendTopP), nameof(TopP),
            nameof(SendPresencePenalty), nameof(PresencePenalty),
            nameof(SendFrequencyPenalty), nameof(FrequencyPenalty),
        };

        // Maps each value property to the toggle that enables/disables editing it.
        private static readonly Dictionary<string, string> ToggledByProperty = new Dictionary<string, string>
        {
            [nameof(Temperature)] = nameof(SendTemperature),
            [nameof(MaxTokens)] = nameof(SendMaxTokens),
            [nameof(TopP)] = nameof(SendTopP),
            [nameof(PresencePenalty)] = nameof(SendPresencePenalty),
            [nameof(FrequencyPenalty)] = nameof(SendFrequencyPenalty),
        };

        private PropertyGrid _grid;

        protected override IWin32Window Window
        {
            get
            {
                if (_grid == null)
                {
                    _grid = new PropertyGrid
                    {
                        PropertySort = PropertySort.Categorized,
                        SelectedObject = this
                    };
                    // PropertyGrid skips a rebuild if SelectedObject is reassigned to the same
                    // reference, so route through null to force IsReadOnly to be re-evaluated
                    // right after a toggle checkbox is flipped.
                    _grid.PropertyValueChanged += (s, e) =>
                    {
                        _grid.SelectedObject = null;
                        _grid.SelectedObject = this;
                    };
                }

                return _grid;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _grid?.Dispose();
                _grid = null;
            }

            base.Dispose(disposing);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            PropertyDescriptorCollection baseProperties =
                TypeDescriptor.GetProvider(this).GetTypeDescriptor(this).GetProperties(attributes);

            var byName = new Dictionary<string, PropertyDescriptor>();
            foreach (PropertyDescriptor property in baseProperties)
            {
                byName[property.Name] = property;
            }

            var ordered = new List<PropertyDescriptor>();
            var placed = new HashSet<string>();

            foreach (string name in ModelParameterDisplayOrder)
            {
                if (!byName.TryGetValue(name, out PropertyDescriptor property))
                {
                    continue;
                }

                if (ToggledByProperty.TryGetValue(name, out string toggleName))
                {
                    property = new ToggledPropertyDescriptor(property, this, toggleName);
                }

                ordered.Add(property);
                placed.Add(name);
            }

            foreach (PropertyDescriptor property in baseProperties)
            {
                if (!placed.Contains(property.Name))
                {
                    ordered.Add(property);
                }
            }

            return new PropertyDescriptorCollection(ordered.ToArray());
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            return ((ICustomTypeDescriptor)this).GetProperties(null);
        }

        AttributeCollection ICustomTypeDescriptor.GetAttributes() =>
            TypeDescriptor.GetProvider(this).GetTypeDescriptor(this).GetAttributes();

        string ICustomTypeDescriptor.GetClassName() =>
            TypeDescriptor.GetProvider(this).GetTypeDescriptor(this).GetClassName();

        string ICustomTypeDescriptor.GetComponentName() =>
            TypeDescriptor.GetProvider(this).GetTypeDescriptor(this).GetComponentName();

        TypeConverter ICustomTypeDescriptor.GetConverter() =>
            TypeDescriptor.GetProvider(this).GetTypeDescriptor(this).GetConverter();

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent() =>
            TypeDescriptor.GetProvider(this).GetTypeDescriptor(this).GetDefaultEvent();

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty() =>
            TypeDescriptor.GetProvider(this).GetTypeDescriptor(this).GetDefaultProperty();

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType) =>
            TypeDescriptor.GetProvider(this).GetTypeDescriptor(this).GetEditor(editorBaseType);

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents() =>
            TypeDescriptor.GetProvider(this).GetTypeDescriptor(this).GetEvents();

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes) =>
            TypeDescriptor.GetProvider(this).GetTypeDescriptor(this).GetEvents(attributes);

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd) => this;

        /// <summary>
        /// Wraps a value property so it renders read-only in the PropertyGrid whenever the
        /// paired "SendX" toggle property is currently false.
        /// </summary>
        private sealed class ToggledPropertyDescriptor : PropertyDescriptor
        {
            private readonly PropertyDescriptor _inner;
            private readonly GeneralOptions _owner;
            private readonly string _toggleProperty;

            public ToggledPropertyDescriptor(PropertyDescriptor inner, GeneralOptions owner, string toggleProperty)
                : base(inner)
            {
                _inner = inner;
                _owner = owner;
                _toggleProperty = toggleProperty;
            }

            public override Type ComponentType => _inner.ComponentType;
            public override Type PropertyType => _inner.PropertyType;
            public override bool IsReadOnly => !(bool)_owner.GetType().GetProperty(_toggleProperty).GetValue(_owner);

            public override bool CanResetValue(object component) => _inner.CanResetValue(component);
            public override object GetValue(object component) => _inner.GetValue(component);
            public override void ResetValue(object component) => _inner.ResetValue(component);
            public override void SetValue(object component, object value) => _inner.SetValue(component, value);
            public override bool ShouldSerializeValue(object component) => _inner.ShouldSerializeValue(component);
        }
    }
}
