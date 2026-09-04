#if TOOLS
using Godot;
using System.Linq;

namespace Anvil;

public static class AnvilRuleManager
{
    private const string RulesPath = "res://addons/anvil/anvil_rules.tres";

    public static AnvilRuleSet LoadOrCreate()
    {
        bool existsOnDisk = FileAccess.FileExists(RulesPath);
        bool existsForLoader = ResourceLoader.Exists(RulesPath);

        if (existsForLoader)
        {
            var loaded = ResourceLoader.Load(RulesPath);

            if (loaded is AnvilRuleSet ruleSet)
                return ruleSet;

            GD.PushWarning($"Anvil: '{RulesPath}' exists (disk: {existsOnDisk}, loader: {existsForLoader}) " +
                            $"but isn't a valid AnvilRuleSet — loaded type was '{loaded?.GetType().FullName ?? "null"}'. Recreating it.");
        }

        var fresh = new AnvilRuleSet();
        ResourceSaver.Save(fresh, RulesPath);
        return fresh;
    }

    public static AnvilRule FindRuleForFolder(string sourceFolder)
    {
        var rules = LoadOrCreate();
        return rules.FolderRules.FirstOrDefault(r => r.SourceFolder == sourceFolder);
    }

    public static void SaveRule(AnvilRule newRule, AnvilRule replacing)
    {
        var rules = LoadOrCreate();

        if (replacing is not null)
            rules.FolderRules.Remove(replacing);

        rules.FolderRules.Add(newRule);
        ResourceSaver.Save(rules, RulesPath);

        GD.Print($"Anvil: Tracking '{newRule.SourceFolder}' as '{newRule.OutputName}'. " +
                 "Run 'Generate Anvil IDs' from the tool menu to apply.");
    }

    public static void RemoveRule(AnvilRule rule)
    {
        var rules = LoadOrCreate();
        rules.FolderRules.Remove(rule);
        ResourceSaver.Save(rules, RulesPath);

        GD.Print($"Anvil: Stopped tracking '{rule.SourceFolder}'. " +
                 "Run 'Generate Anvil IDs' from the tool menu to apply.");
    }
}
#endif
