#if TOOLS
using Godot;

namespace Anvil.Editor;

internal static class AnvilSettings
{
    private const string ExtensionsSettingPath = "anvil/generator/valid_extensions";

    public static void Initialize()
    {
        if (!ProjectSettings.HasSetting(ExtensionsSettingPath))
        {
            string[] defaultExtensions =
            [
                ".wav", ".ogg", ".mp3", ".tscn", ".tres",
                ".glb", ".blend", ".fbx",
            ];
            
            ProjectSettings.SetSetting(ExtensionsSettingPath, defaultExtensions);
            ProjectSettings.SetInitialValue(ExtensionsSettingPath, defaultExtensions);
        }

        var propertyInfo = new Godot.Collections.Dictionary
        {
            { "name", ExtensionsSettingPath },
            { "type", (int)Variant.Type.PackedStringArray }
        };
        ProjectSettings.AddPropertyInfo(propertyInfo);
    }

    public static string[] GetValidExtensions()
    {
        if (ProjectSettings.HasSetting(ExtensionsSettingPath))
            return ProjectSettings.GetSetting(ExtensionsSettingPath).AsStringArray();
        return [];
    }
}
#endif
