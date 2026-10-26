#if TOOLS
using Godot;

namespace Anvil;

[Tool]
public partial class AnvilPlugin : EditorPlugin
{
    private AnvilContextMenu _contextMenu;

    public override void _EnterTree()
    {
        _contextMenu = new AnvilContextMenu();
        _contextMenu.Initialize(this);
        
        AddContextMenuPlugin(EditorContextMenuPlugin.ContextMenuSlot.Filesystem, _contextMenu);
        AddToolMenuItem("Generate Anvil IDs", Callable.From(Generate));

        Generate();
    }

    public override void _ExitTree()
    {
        RemoveToolMenuItem("Generate Anvil IDs");
        RemoveContextMenuPlugin(_contextMenu);
    }

    private static void Generate()
    {
        // Migration: older versions generated Storage.cs / Storage.Scenes.cs. Those would now
        // declare duplicates of the new classes, so remove them (the new files have other names).
        AnvilFileIO.ClearOldGeneratedFiles("Storage", "Storage.Scenes");

        GD.Print("Anvil: Starting ID Generation...");

        var rules = AnvilRuleManager.LoadOrCreate();
        var folderNames = AnvilGenerator.GenerateAndSave(rules);
        AnvilSceneGenerator.GenerateAndSave(rules, folderNames);

        GD.Print("Anvil: Generation Pipeline Complete!");
    }
}
#endif
