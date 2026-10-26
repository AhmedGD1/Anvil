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

            if (!AnvilNaming.IsValidIdentifier(rule.OutputName))
            {
                GD.PushWarning($"Anvil: Output name '{rule.OutputName}' (for '{rule.SourceFolder}') is not a valid C# " +
                               "class name (use letters, digits and '_', not starting with a digit, and no C# keywords). Skipped.");
                continue;
            }

            if (AnvilNaming.IsReservedOutputName(rule.OutputName))
            {
                GD.PushWarning($"Anvil: Output name '{rule.OutputName}' (for '{rule.SourceFolder}') is reserved " +
                               "(it would clash with a built-in generated class or an Anvil plugin type). Skipped.");
                continue;
            }

            if (!seenNames.Add(rule.OutputName))
            {
                GD.PushWarning($"Anvil: Duplicate output name '{rule.OutputName}' " +
                               $"(tracked by more than one folder). Skipped to avoid a naming conflict.");
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

    public static List<AnvilSceneRule> ValidateSceneRules(AnvilRuleSet rules)
    {
        List<AnvilSceneRule> validRules = [];

        if (rules.SceneRules is null)
            return validRules;

        foreach (var rule in rules.SceneRules)
        {
            if (rule.Sources == SceneSources.None)
            {
                GD.PushWarning($"Anvil: Scene rule for '{rule.ScenePath}' has nothing selected to generate. Skipped.");
                continue;
            }

            string scenePath = rule.ResolvePath();
            if (!ResourceLoader.Exists(scenePath))
            {
                GD.PushWarning($"Anvil: Tracked scene '{scenePath}' no longer exists and was skipped. " +
                                "Remove its entry from res://addons/anvil/anvil_rules.tres if it was deleted.");
                continue;
            }

            validRules.Add(rule);
        }

        return validRules;
    }
}
#endif
