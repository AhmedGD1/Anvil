#if TOOLS
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Anvil;

public static class AnvilSceneReader
{
    public sealed record AnimationNodeInfo(string NodeName, string NodePath, List<string> Animations);

    public sealed record SceneInfo(
        string ScriptPath,
        List<AnimationNodeInfo> AnimationPlayers,
        List<AnimationNodeInfo> AnimatedSprites);

    public static SceneInfo Read(string scenePath)
    {
        var scene = ResourceLoader.Load<PackedScene>(scenePath, "", ResourceLoader.CacheMode.Ignore);
        if (scene is null)
        {
            GD.PushWarning($"Anvil: Could not load scene '{scenePath}'.");
            return null;
        }

        Node root = scene.Instantiate();
        if (root is null)
        {
            GD.PushWarning($"Anvil: Could not instantiate scene '{scenePath}'.");
            return null;
        }

        List<AnimationNodeInfo> players = [];
        List<AnimationNodeInfo> sprites = [];

        string scriptPath = "";

        try
        {
            string path = root.GetScript().As<Script>()?.ResourcePath ?? "";
            if (path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                scriptPath = path;

            CollectAnimationNodes(root, root, players, sprites);
        }
        finally
        {
            root.Free();
        }

        return new SceneInfo(scriptPath, players, sprites);
    }

    private static void CollectAnimationNodes(
        Node root, Node node, List<AnimationNodeInfo> players, List<AnimationNodeInfo> sprites)
    {
        bool belongsToScene = node == root || node.Owner == root;

        if (belongsToScene)
        {
            if (node is AnimationPlayer player)
            {
                players.Add(ToInfo(root, node, player.GetAnimationList()));
            }
            else if (node is AnimatedSprite2D or AnimatedSprite3D)
            {
                SpriteFrames frames = node is AnimatedSprite2D sprite2D
                    ? sprite2D.SpriteFrames
                    : ((AnimatedSprite3D)node).SpriteFrames;

                sprites.Add(ToInfo(root, node, frames?.GetAnimationNames() ?? []));
            }
        }

        foreach (Node child in node.GetChildren())
        {
            CollectAnimationNodes(root, child, players, sprites);
        }
    }

    private static AnimationNodeInfo ToInfo(Node root, Node node, IEnumerable<string> animations)
    {
        List<string> sorted = animations.OrderBy(n => n, StringComparer.Ordinal).ToList();

        if (sorted.Count == 0)
            GD.PushWarning($"Anvil: {node.GetType().Name} '{node.Name}' in '{root.SceneFilePath}' has no animations.");

        return new AnimationNodeInfo(node.Name.ToString(), root.GetPathTo(node).ToString(), sorted);
    }
}
#endif