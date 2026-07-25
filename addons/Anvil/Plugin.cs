#if TOOLS
using Godot;
using Anvil.Editor;

namespace Anvil;

[Tool]
public partial class Plugin : EditorPlugin
{
    public override void _EnterTree()
    {
        AnvilSettings.Initialize();
        AddToolMenuItem("Generate Anvil IDs", Callable.From(OnGeneratePressed));
    }

    public override void _ExitTree()
    {
        RemoveToolMenuItem("Generate Anvil IDs");
    }

    private void OnGeneratePressed()
    {
        AnvilFileIO.ClearOldGeneratedFiles();

        GD.Print("Anvil: Starting ID Generation...");

        var targets = AnvilScanner.ScanProject();

        if (targets.Count > 0)
        {
            AnvilGenerator.GenerateAndSave(targets);
            GD.Print("Anvil: Generation Pipeline Complete!");
        }
        else
        {
            GD.Print("Anvil: No [Forge] attributes found in the project.");
        }
    }
}
#endif
