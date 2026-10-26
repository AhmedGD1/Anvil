#if TOOLS
using Godot;
using System;

namespace Anvil;

public partial class AnvilContextMenu : EditorContextMenuPlugin
{
    private const string IconPath = "res://addons/anvil/icons/anvil_icon.svg";
    private const string DialogName = "AnvilTrackFolderDialog";
    private const string SceneDialogName = "AnvilTrackSceneDialog";

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

        if (hostNode.GetNodeOrNull(SceneDialogName) is null)
        {
            var sceneDialog = new AnvilTrackSceneDialog { Name = SceneDialogName };
            hostNode.AddChild(sceneDialog);
        }

        _icon = ResourceLoader.Load<Texture2D>(IconPath);
    }

    public override void _PopupMenu(string[] paths)
    {
        if (paths.Length != 1)
            return;

        string path = paths[0];

        if (DirAccess.DirExistsAbsolute(path))
            AddFolderItems(path);
        else if (IsSceneFile(path))
            AddSceneItems(path);
    }

    private static bool IsSceneFile(string path)
    {
        string extension = path.GetExtension();

        return (extension.Equals("tscn", StringComparison.OrdinalIgnoreCase)
                || extension.Equals("scn", StringComparison.OrdinalIgnoreCase))
               && FileAccess.FileExists(path);
    }

    private void AddSceneItems(string path)
    {
        var existingRule = AnvilRuleManager.FindRuleForScene(path);

        if (existingRule is null)
        {
            AddContextMenuItem("Track Scene with Anvil...", Callable.From((Godot.Collections.Array _) => OpenSceneDialog(path)), _icon);
        }
        else
        {
            AddContextMenuItem("Edit Anvil Scene Tracking...", Callable.From((Godot.Collections.Array _) => OpenSceneDialog(path)), _icon);
            AddContextMenuItem("Untrack Scene from Anvil", Callable.From((Godot.Collections.Array _) => AnvilRuleManager.RemoveSceneRule(existingRule)), _icon);
        }
    }

    private void AddFolderItems(string path)
    {
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

    private void OpenSceneDialog(string path)
    {
        if (_hostNode.GetNodeOrNull(SceneDialogName) is AnvilTrackSceneDialog dialog)
            dialog.OpenFor(path);
    }
}
#endif
