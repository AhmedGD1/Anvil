#if TOOLS
using Godot;
using System.IO;

namespace Anvil.Editor;

internal static class AnvilFileIO
{
    public static readonly string GeneratedDirPath = "res://addons/Anvil/Generated";

    public static void ClearOldGeneratedFiles()
    {
        string globalGeneratedDir = ProjectSettings.GlobalizePath(GeneratedDirPath);
        if (Directory.Exists(globalGeneratedDir))
        {
            foreach (string file in Directory.GetFiles(globalGeneratedDir, "*.anvilgen.cs"))
                File.Delete(file);
        }
    }

    public static void SaveGeneratedFile(string className, string content)
    {
        string globalGeneratedDir = ProjectSettings.GlobalizePath(GeneratedDirPath);
        
        if (!Directory.Exists(globalGeneratedDir))
        {
            Directory.CreateDirectory(globalGeneratedDir);
        }

        string gdignorePath = Path.Combine(globalGeneratedDir, ".gdignore");
        if (!File.Exists(gdignorePath))
        {
            File.WriteAllText(gdignorePath, "");
        }

        string fileName = $"{className}.anvilgen.cs";
        string filePath = Path.Combine(globalGeneratedDir, fileName);
        
        File.WriteAllText(filePath, content);
        GD.Print($"Anvil: Generated {fileName} successfully.");
    }
}
#endif
