#if TOOLS
using Godot;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Anvil.Editor;

internal static partial class AnvilScanner
{
    public static List<ForgeData> ScanProject()
    {
        string projectPath = ProjectSettings.GlobalizePath("res://");
        string[] csFiles = Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories);
        
        List<ForgeData> forgeTargets = [];

        foreach (string file in csFiles)
        {
            string normalizedPath = file.Replace('\\', '/').ToLower();
            
            if (normalizedPath.Contains("/addons/anvil/generated/")) 
                continue;
        
            string content = File.ReadAllText(file);
            string namespaceName = null;
            
            Match namespaceMatch = NamespaceRegex().Match(content);
            
            if (namespaceMatch.Success)
            {
                namespaceName = namespaceMatch.Groups["Namespace"].Value;
            }
        
            string pattern = @"\[(?:Anvil\.)?Forge\(""(?<Path>[^""]+)""(?:\s*,\s*(?:recursive\s*:\s*)?(?<Recursive>true|false))?\)\]\s*public\s+static\s+partial\s+class\s+(?<Inner>\w+)";
            MatchCollection matches = Regex.Matches(content, pattern);
        
            foreach (Match match in matches)
            {
                string textBefore = content[..match.Index];
                Match outerMatch = OuterMatchRegex().Match(textBefore);
        
                if (outerMatch.Success)
                {
                    bool recursive = match.Groups["Recursive"].Success &&
                                      match.Groups["Recursive"].Value == "true";

                    forgeTargets.Add(new ForgeData
                    {
                        Namespace = namespaceName,
                        OuterClass = outerMatch.Groups["Outer"].Value,
                        InnerClass = match.Groups["Inner"].Value,
                        ResourcePath = match.Groups["Path"].Value,
                        Recursive = recursive
                    });
                }
                else
                {
                    GD.PushWarning($"Anvil: Found [Forge] on {match.Groups["Inner"].Value} but couldn't find an Outer class.");
                }
            }
        }
        return forgeTargets;
    }

    [GeneratedRegex(@"public\s+partial\s+class\s+(?<Outer>\w+)", RegexOptions.RightToLeft)]
    private static partial Regex OuterMatchRegex();
    
    [GeneratedRegex(@"namespace\s+(?<Namespace>[\w\.]+)")]
    private static partial Regex NamespaceRegex();

}
#endif
