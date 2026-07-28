using System.Collections.Generic;
using System.Linq;

namespace JasmineYamo.ComponentAutoBind.Editor
{
    internal static class AutoBindFieldNameUtility
    {
        private static readonly HashSet<string> s_CSharpKeywords = new HashSet<string>
        {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
            "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else",
            "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for",
            "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock",
            "long", "namespace", "new", "null", "object", "operator", "out", "override", "params",
            "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short",
            "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true",
            "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual",
            "void", "volatile", "while", "add", "alias", "ascending", "async", "await", "by", "descending",
            "dynamic", "equals", "from", "get", "global", "group", "init", "into", "join", "let", "nameof",
            "notnull", "on", "orderby", "partial", "record", "remove", "select", "set", "unmanaged", "value",
            "var", "when", "where", "with", "yield"
        };

        public static string BuildFieldName(string bindName)
        {
            if (string.IsNullOrWhiteSpace(bindName))
            {
                return string.Empty;
            }

            string fieldName = bindName.Replace("_", string.Empty).Trim();
            if (fieldName.Length == 0)
            {
                return string.Empty;
            }

            char firstChar = fieldName[0];
            if (firstChar >= 'A' && firstChar <= 'Z')
            {
                fieldName = char.ToLowerInvariant(firstChar) + fieldName.Substring(1);
            }

            return fieldName;
        }

        public static string BuildKeyPrefix(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                return string.Empty;
            }

            string capitals = string.Concat(typeName.Where(char.IsUpper));
            if (capitals.Length > 1)
            {
                return capitals;
            }

            return typeName.Length <= 3 ? typeName : typeName.Substring(0, 3);
        }

        public static bool IsValidIdentifier(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier)
                || !IsIdentifierStartCharacter(identifier[0])
                || s_CSharpKeywords.Contains(identifier))
            {
                return false;
            }

            for (int i = 1; i < identifier.Length; i++)
            {
                if (!IsIdentifierPartCharacter(identifier[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsValidNamespace(string namespaceName)
        {
            if (string.IsNullOrWhiteSpace(namespaceName))
            {
                return true;
            }

            string[] segments = namespaceName.Split('.');
            for (int i = 0; i < segments.Length; i++)
            {
                if (!IsValidIdentifier(segments[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsIdentifierStartCharacter(char value)
        {
            return value == '_' || IsAsciiLetter(value);
        }

        private static bool IsIdentifierPartCharacter(char value)
        {
            return value == '_' || IsAsciiLetter(value) || (value >= '0' && value <= '9');
        }

        private static bool IsAsciiLetter(char value)
        {
            return (value >= 'a' && value <= 'z') || (value >= 'A' && value <= 'Z');
        }
    }
}
