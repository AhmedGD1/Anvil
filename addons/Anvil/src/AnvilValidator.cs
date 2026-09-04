#if TOOLS
using Godot;
using System.Collections.Generic;
using System.IO;

namespace Anvil;

public static class AnvilValidator
{
    public static List<AnvilRule> ValidateRules(AnvilRuleSet rules)
    {
        List<AnvilRule> validRules = [];
        HashSet<string> seenNames = [];

        foreach (var rule in rules.FolderRules)
        {
            if (string.IsNullOrWhiteSpace(rule.OutputName))
            {
                GD.PushWarning($"Anvil: Rule for '{rule.SourceFolder}' has an empty output name and was skipped.");
                continue;
            }

            if (!seenNames.Add(rule.OutputName))
            {
                GD.PushWarning($"Anvil: Duplicate output name '{rule.OutputName}' " +
                               $"(tracked by more than one folder). Skipped to avoid a naming conflict in Storage.");
                continue;
            }

            string globalPath = ProjectSettings.GlobalizePath(rule.SourceFolder);
            if (!Directory.Exists(globalPath))
            {
                GD.PushWarning($"Anvil: Tracked folder '{rule.SourceFolder}' (for '{rule.OutputName}') " +
                               $"no longer exists. Right-click it to re-track, or untrack it via the FileSystem dock if it moved.");
                continue;
            }

            validRules.Add(rule);
        }

        return validRules;
    }
}
#endif
