#if TOOLS
using Godot;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Anvil.Editor;

public static partial class AnvilScanner
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
        
            string pattern = @"\[(?:Anvil\.)?Forge\((?<Args>[^)]*)\)\]\s*public\s+static\s+partial\s+class\s+(?<Inner>\w+)";
            MatchCollection matches = Regex.Matches(content, pattern);
        
            foreach (Match match in matches)
            {
                string textBefore = content[..match.Index];
                Match outerMatch = OuterMatchRegex().Match(textBefore);
        
                if (outerMatch.Success)
                {
                    string args = match.Groups["Args"].Value;

                    Match pathMatch = ForgePathRegex().Match(args);
                    if (!pathMatch.Success)
                    {
                        GD.PushWarning($"Anvil: Found [Forge] on {match.Groups["Inner"].Value} but couldn't parse a path argument.");
                        continue;
                    }

                    Match recursiveMatch = ForgeRecursiveRegex().Match(args, pathMatch.Index + pathMatch.Length);
                    bool recursive = recursiveMatch.Success && recursiveMatch.Groups["Recursive"].Value == "true";

                    Match modeMatch = ForgeModeRegex().Match(args, pathMatch.Index + pathMatch.Length);
                    ForgeMode mode = modeMatch.Success && modeMatch.Groups["Mode"].Value == "FullPath"
                        ? ForgeMode.FullPath
                        : ForgeMode.Id;

                    forgeTargets.Add(new ForgeData
                    {
                        Namespace = namespaceName,
                        OuterClass = outerMatch.Groups["Outer"].Value,
                        InnerClass = match.Groups["Inner"].Value,
                        ResourcePath = pathMatch.Groups["Path"].Value,
                        Recursive = recursive,
                        Mode = mode
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

    [GeneratedRegex(@"""(?<Path>[^""]+)""")]
    private static partial Regex ForgePathRegex();

    [GeneratedRegex(@"recursive\s*:\s*(?<Recursive>true|false)|,\s*(?<Recursive>true|false)\s*(?:,|$)")]
    private static partial Regex ForgeRecursiveRegex();

    [GeneratedRegex(@"(?:mode\s*:\s*)?(?:ForgeMode\.)?(?<Mode>Id|FullPath)")]
    private static partial Regex ForgeModeRegex();

}
#endif
