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

    private void Generate()
    {
        AnvilFileIO.ClearOldGeneratedFiles("Storage");

        GD.Print("Anvil: Starting ID Generation...");

        var rules = AnvilRuleManager.LoadOrCreate();
        AnvilGenerator.GenerateAndSave(rules);

        GD.Print("Anvil: Generation Pipeline Complete!");
    }
}
#endif
