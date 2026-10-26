#if TOOLS
using Godot;
using System.Collections.Generic;
using System.Text;

namespace Anvil;

public static class AnvilNaming
{
    public const string HintStringMember = "HintString";

    public static readonly HashSet<string> ReservedTypeNames = new()
    {
        "ForgeMode",
        "SceneSources",
        "GlobalGroups",
        "InputActions",
        "AudioBuses",
        "PhysicsLayers2D",
        "PhysicsLayers3D",
    };

    private static readonly HashSet<string> CSharpKeywords = new()
    {
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
        "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else",
        "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for",
        "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock",
        "long", "namespace", "new", "null", "object", "operator", "out", "override", "params",
        "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed",
        "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw",
        "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using",
        "virtual", "void", "volatile", "while",
    };

    public static string ToIdentifier(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        return Sanitize(raw.ToPascalCase());
    }

    public static string Sanitize(string value)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        var sb = new StringBuilder(value.Length + 1);
        bool upperNext = false;

        foreach (char c in value)
        {
            if (char.IsLetterOrDigit(c) || c == '_')
            {
                sb.Append(upperNext ? char.ToUpperInvariant(c) : c);
                upperNext = false;
            }
            else
            {
                upperNext = sb.Length > 0;
            }
        }

        if (sb.Length == 0)
            return null;

        if (char.IsDigit(sb[0]))
            sb.Insert(0, '_');

        return sb.ToString();
    }

    public static bool IsValidIdentifier(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        if (!(char.IsLetter(value[0]) || value[0] == '_'))
            return false;

        foreach (char c in value)
        {
            if (!(char.IsLetterOrDigit(c) || c == '_'))
                return false;
        }

        return !CSharpKeywords.Contains(value);
    }

    public static bool IsReservedOutputName(string value)
    {
        return ReservedTypeNames.Contains(value) || value.StartsWith("Anvil", System.StringComparison.Ordinal);
    }

    public static bool IsValidOutputName(string value)
    {
        return IsValidIdentifier(value) && !IsReservedOutputName(value);
    }

    public static string EscapeLiteral(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        var sb = new StringBuilder(value.Length + 8);

        foreach (char c in value)
        {
            switch (c)
            {
                case '\\': sb.Append("\\\\"); break;
                case '"': sb.Append("\\\""); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                case '\0': sb.Append("\\0"); break;
                default: sb.Append(c); break;
            }
        }

        return sb.ToString();
    }
}
#endif
