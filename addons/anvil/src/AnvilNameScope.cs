#if TOOLS
using Godot;
using System.Collections.Generic;

namespace Anvil;

public sealed class AnvilNameScope
{
    private readonly HashSet<string> _used = new() { AnvilNaming.HintStringMember };
    private readonly string _enclosingName;

    public AnvilNameScope(string enclosingName)
    {
        _enclosingName = enclosingName;
    }

    public bool TryClaim(string rawName, string kind, out string identifier)
    {
        identifier = AnvilNaming.ToIdentifier(rawName);

        if (identifier is null)
        {
            GD.PushWarning($"Anvil: Skipped {kind} '{rawName}' (no valid C# identifier could be derived from it).");
            return false;
        }

        if (identifier == _enclosingName)
        {
            string renamed = identifier + "_";
            GD.PushWarning($"Anvil: {kind} '{rawName}' would share its name with the enclosing class '{_enclosingName}'. " +
                           $"Generated as '{renamed}' instead.");
            identifier = renamed;
        }

        if (!_used.Add(identifier))
        {
            GD.PushWarning($"Anvil: Skipped duplicate {kind} ID '{identifier}' from '{rawName}' " +
                           "(name collision after PascalCase conversion).");
            return false;
        }

        return true;
    }
}
#endif
