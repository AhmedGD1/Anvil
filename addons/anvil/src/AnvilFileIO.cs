#if TOOLS
using Godot;
using System.IO;

namespace Anvil;

public static class AnvilFileIO
{
    public static readonly string GeneratedDirPath = "res://addons/anvil/generated";

    public static void ClearOldGeneratedFiles(params string[] classNames)
    {
        string globalGeneratedDir = ProjectSettings.GlobalizePath(GeneratedDirPath);
        if (!Directory.Exists(globalGeneratedDir))
            return;

        foreach (string className in classNames)
        {
            string filePath = Path.Combine(globalGeneratedDir, $"{className}.cs");
            if (File.Exists(filePath))
                File.Delete(filePath);
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

        string fileName = $"{className}.cs";
        string filePath = Path.Combine(globalGeneratedDir, fileName);

        if (File.Exists(filePath) && File.ReadAllText(filePath) == content)
        {
            GD.Print($"Anvil: {fileName} is already up to date.");
            return;
        }

        File.WriteAllText(filePath, content);
        GD.Print($"Anvil: Generated {fileName} successfully.");
    }
}
#endif
