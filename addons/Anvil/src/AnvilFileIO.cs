#if TOOLS
using Godot;
using System.IO;

namespace Anvil;

public static class AnvilFileIO
{
    public static readonly string GeneratedDirPath = "res://addons/anvil/generated";

    /// <summary>
    /// Deletes previously generated files by known class name. Generated files now use
    /// a plain ".cs" extension, so they can no longer be distinguished from user files
    /// by extension alone — pass the exact class name(s) this plugin generates.
    /// </summary>
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

        File.WriteAllText(filePath, content);
        GD.Print($"Anvil: Generated {fileName} successfully.");
    }
}
#endif
