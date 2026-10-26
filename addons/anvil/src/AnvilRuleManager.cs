#if TOOLS
using System.Collections.Generic;
using System.Linq;
using Godot;

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

    public static void RemoveRule(AnvilRule rule)
    {
        var rules = LoadOrCreate();
        rules.FolderRules.Remove(rule);
        ResourceSaver.Save(rules, RulesPath);

        GD.Print($"Anvil: Stopped tracking '{rule.SourceFolder}'. " +
                 "Run 'Generate Anvil IDs' from the tool menu to apply.");
    }

    public static AnvilSceneRule FindRuleForScene(string scenePath)
    {
        var rules = LoadOrCreate();
        return rules.SceneRules?.FirstOrDefault(r => r.Tracks(scenePath));
    }

    public static void RemoveSceneRule(AnvilSceneRule rule)
    {
        var rules = LoadOrCreate();
        rules.SceneRules?.Remove(rule);
        ResourceSaver.Save(rules, RulesPath);

        GD.Print($"Anvil: Stopped tracking scene '{rule.ScenePath}'. " +
                 "Run 'Generate Anvil IDs' from the tool menu to apply.");
    }

    public static void SaveRule(AnvilRule newRule, AnvilRule replacing)
    {
        var rules = LoadOrCreate();
        Upsert(rules.FolderRules, newRule, replacing);
        ResourceSaver.Save(rules, RulesPath);

        GD.Print($"Anvil: Tracking '{newRule.SourceFolder}' as '{newRule.OutputName}'. " +
                "Run 'Generate Anvil IDs' from the tool menu to apply.");
    }

    public static void SaveSceneRule(AnvilSceneRule newRule, AnvilSceneRule replacing)
    {
        var rules = LoadOrCreate();
        Upsert(rules.SceneRules, newRule, replacing);
        ResourceSaver.Save(rules, RulesPath);

        GD.Print($"Anvil: Tracking scene '{newRule.ScenePath}' as '{newRule.OutputName}'. " +
                "Run 'Generate Anvil IDs' from the tool menu to apply.");
    }

    private static void Upsert<T>(IList<T> list, T item, T replacing) where T : class
    {
        int index = replacing is null ? -1 : list.IndexOf(replacing);

        if (index >= 0)
            list[index] = item;
        else
            list.Add(item);
    }
}
#endif
