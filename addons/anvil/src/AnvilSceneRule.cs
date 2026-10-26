#if TOOLS
using Godot;

namespace Anvil;

[Tool, GlobalClass]
public partial class AnvilSceneRule : Resource
{
    [Export(PropertyHint.File, "*.tscn,*.scn")]
    public string ScenePath { get; set; } = "";

    [Export]
    public string SceneUid { get; set; } = "";

    [Export]
    public string OutputName { get; set; } = "";

    [Export]
    public SceneSources Sources { get; set; } = SceneSources.All;

    [Export]
    public bool Flatten { get; set; } = true;

    /// <summary>Returns the "uid://..." text for a resource path, or "" if it has none.</summary>
    public static string GetUidText(string path)
    {
        long id = ResourceLoader.GetResourceUid(path);
        return id == ResourceUid.InvalidId ? "" : ResourceUid.IdToText(id);
    }

    public string ResolvePath()
    {
        if (!string.IsNullOrEmpty(SceneUid))
        {
            long id = ResourceUid.TextToId(SceneUid);
            if (id != ResourceUid.InvalidId && ResourceUid.HasId(id))
                return ResourceUid.GetIdPath(id);
        }

        return ScenePath;
    }

    public bool Tracks(string path)
    {
        if (!string.IsNullOrEmpty(SceneUid))
        {
            string uid = GetUidText(path);
            if (uid.Length > 0)
                return uid == SceneUid;
        }

        return ScenePath == path;
    }
}
#endif