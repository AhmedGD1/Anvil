#if TOOLS
using Godot;

namespace Anvil;

public partial class AnvilContextMenu : EditorContextMenuPlugin
{
    private const string IconPath = "res://addons/anvil/icons/anvil_icon.svg";
    private const string DialogName = "AnvilTrackFolderDialog";

    // Deliberately NOT cached as an instance field. AnvilContextMenu is a RefCounted-based
    // EditorContextMenuPlugin, and Godot's C# hot-reload tries to serialize/restore all
    // instance fields on script rebuild. Caching a Node reference (or a closure that
    // captures one) here causes ObjectDisposedException / type-cast errors on every
    // rebuild, because the dialog node and this plugin object don't reload in lockstep.
    // Resolving the dialog fresh by name on every popup sidesteps that entirely, since
    // AnvilPlugin._EnterTree() recreates the whole tree on every (re)load anyway.
    private Node _hostNode;
    private Texture2D _icon;

    public void Initialize(Node hostNode)
    {
        _hostNode = hostNode;

        if (hostNode.GetNodeOrNull(DialogName) is null)
        {
            var dialog = new AnvilTrackFolderDialog { Name = DialogName };
            hostNode.AddChild(dialog);
        }

        _icon = ResourceLoader.Load<Texture2D>(IconPath);
    }

    public override void _PopupMenu(string[] paths)
    {
        // Only offer folder-tracking when exactly one item is selected and it's a directory.
        // AnvilRule tracks a single folder, so multi-selection or a file selection don't apply.
        if (paths.Length != 1)
            return;

        string path = paths[0];
        if (!DirAccess.DirExistsAbsolute(path))
            return;

        var existingRule = AnvilRuleManager.FindRuleForFolder(path);

        if (existingRule is null)
        {
            AddContextMenuItem("Track with Anvil...", Callable.From((Godot.Collections.Array _) => OpenDialog(path)), _icon);
        }
        else
        {
            AddContextMenuItem("Edit Anvil Tracking...", Callable.From((Godot.Collections.Array _) => OpenDialog(path)), _icon);
            AddContextMenuItem("Untrack from Anvil", Callable.From((Godot.Collections.Array _) => AnvilRuleManager.RemoveRule(existingRule)), _icon);
        }
    }

    private void OpenDialog(string path)
    {
        if (_hostNode.GetNodeOrNull(DialogName) is AnvilTrackFolderDialog dialog)
            dialog.OpenFor(path);
    }
}
#endif
