using System.Collections.Generic;
using System.Text;

namespace JasmineYamo.ComponentAutoBind.Editor
{
    internal sealed class AutoBindValidationResult
    {
        private readonly List<string> m_Errors = new List<string>();
        private readonly List<string> m_Warnings = new List<string>();

        public bool IsValid => m_Errors.Count == 0;
        public int ErrorCount => m_Errors.Count;
        public int WarningCount => m_Warnings.Count;

        public void AddError(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                m_Errors.Add(message);
            }
        }

        public void AddWarning(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                m_Warnings.Add(message);
            }
        }

        public string BuildReport()
        {
            var builder = new StringBuilder();
            bool hasPreviousSection = false;

            if (m_Errors.Count > 0)
            {
                builder.AppendLine("Errors:");
                AppendMessages(builder, m_Errors);
                hasPreviousSection = true;
            }

            if (m_Warnings.Count > 0)
            {
                if (hasPreviousSection)
                {
                    builder.AppendLine();
                }

                builder.AppendLine("Warnings:");
                AppendMessages(builder, m_Warnings);
                hasPreviousSection = true;
            }

            if (!hasPreviousSection)
            {
                builder.Append("No issues found.");
            }

            return builder.ToString();
        }

        private static void AppendMessages(StringBuilder builder, List<string> messages)
        {
            for (int i = 0; i < messages.Count; i++)
            {
                builder.Append("- ").AppendLine(messages[i]);
            }
        }
    }
}
