# Anvil

A Godot 4 (C#) editor plugin that generates compile-time safe constants for the names and paths your project already has. Typos become compile errors instead of silent runtime bugs.

```csharp
using Anvil;

GD.Load<AudioStream>(Sfx.Planted);
AddToGroup(GlobalGroups.Enemies);
Input.IsActionPressed(InputActions.Interact);
animationPlayer.Play(Player.Animations.Attack);
CollisionMask = PhysicsLayers2D.Walls | PhysicsLayers2D.Enemies;
```

**Requirements:** Godot 4 with C# (developed against 4.7), .NET 8.

## Installation

1. Copy `addons/anvil` into your project's `addons/` folder.
2. Enable it in **Project → Project Settings → Plugins**.
3. Anvil generates `AnvilIds.cs`, `AnvilSceneIds.cs` and `anvil_rules.tres` on load. Regenerate any time with **Project → Tools → Generate Anvil IDs**.

---

## Folder tracking

Right-click a folder in the FileSystem dock → **Track with Anvil...** to generate a constant for every file in it.

![Right-click menu](preview_images\folder_track.png)
![Edit menu](preview_images\folder_track_edit.png)

| Field | Description |
|---|---|
| Output Name | Generated class name (`Sfx` → `Sfx.Planted`) |
| Mode | `Id` or `FullPath` (see below) |
| Extensions | Optional filter, e.g. `png, ogg`. Empty = all files |
| Recursive | Include subfolders |

Already-tracked folders show **Edit Anvil Tracking...** and **Untrack from Anvil**.

```csharp
// Folder: res://sfx/  (Output Name: Sfx)
GD.Load<AudioStream>(Sfx.Planted);
```

## Generation modes

**`Id`** — constant is the file's base name, plus a `HintString`.

```csharp
// Sfx.Planted == "planted"
[Export(PropertyHint.Enum, Sfx.HintString)]
public string SoundId;
```

**`FullPath`** — constant is the full `res://` path, ready for `Load()`.

```csharp
// Sfx.Planted == "res://sfx/others/planted.mp3"
GD.Load<AudioStream>(Sfx.Planted);
```

## Recursive scanning

Tick **Recursive** to include subfolders.

```csharp
// res://sfx/digging.mp3
// res://sfx/others/planted.mp3   <- included when Recursive is on
Sfx.Digging;
Sfx.Planted;
```

## Extension filter

Only include the listed extensions. `.import` and `.uid` files are always excluded.

```text
Extensions: png, ogg
```
```csharp
Icons.Sword;     // sword.png   ✔
Icons.Hit;       // hit.ogg     ✔
                 // notes.txt   ✘ skipped
```

## Global groups

Generated automatically from **Project Settings → Globals → Groups**.

```csharp
AddToGroup(GlobalGroups.Enemies);

[Export(PropertyHint.Enum, GlobalGroups.HintString)]
public string TargetGroup;
```

## Input actions

Generated automatically from **Project Settings → Input Map**. Built-in `ui_*` actions are excluded.

```csharp
if (Input.IsActionPressed(InputActions.Interact)) { }
```

## Audio buses

Generated automatically from the current audio bus layout.

```csharp
AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex(AudioBuses.Music), -6f);
```

## Physics layers

Generated automatically from the *named* layers in **Project Settings → Layer Names → 2D / 3D Physics**. Each is a `uint` bit mask.

```csharp
CollisionLayer = PhysicsLayers2D.Player;
CollisionMask  = PhysicsLayers2D.Walls | PhysicsLayers2D.Enemies;

CollisionMask  = PhysicsLayers3D.World;
```

## HintString

Groups, input actions, audio buses, `Id`-mode folders and scene animation classes all expose a `HintString` for inspector dropdowns.

```csharp
[Export(PropertyHint.Enum, AudioBuses.HintString)]
public string Bus;
```

## `using static`

Drop the class prefix when you use one class a lot.

```csharp
using static Anvil.Sfx;
using static Anvil.GlobalGroups;

GD.Load<AudioStream>(Planted);
AddToGroup(Enemies);
```

## Scene tracking

Generates a constant for every animation in a scene, so `Play("atack")` typos become compile errors.

**Supported nodes:** `AnimationPlayer`, `AnimatedSprite2D`, `AnimatedSprite3D` (only nodes owned by the scene itself, not instanced sub-scenes).

### 1. Track a scene

![Scene-track menu](preview_images\scene_track_edit.png)

Right-click a `.tscn` / `.scn` file → **Track Scene with Anvil...**

| Field | Shown when | Description |
|---|---|---|
| Output Name | Root has **no** C# script | Name of the generated class |
| Flatten | Always | One merged class vs. one class per node (see below) |

Tracked scenes show **Edit Anvil Scene Tracking...** and **Untrack Scene from Anvil**. Tracking is by UID, so moving or renaming the scene keeps working.

### 2. Where do the ids go?

| Scene root | Result | Access |
|---|---|---|
| Has a built C# script | Added to that script's class | `Player.Animations.Idle` |
| No script, or script not built yet | Standalone class `Anvil.<OutputName>` | `Anvil.SampleScene.Animations.Idle` |

### 3. With a root script

Ids are added to your own class, in its namespace. Requirements: the class is `partial` and the project has been built.

```csharp
namespace Game;

public partial class Player : CharacterBody2D
{
    public override void _Ready()
    {
        GetNode<AnimationPlayer>("Anim").Play(Animations.Idle);
    }
}
```

### 4. Without a root script

Pick an **Output Name** in the dialog (e.g. `SampleScene`).

```csharp
using Anvil;

animationPlayer.Play(SampleScene.Animations.Idle);
```

### 5. Flatten

**On (default)** — all animations of all nodes merged into one `Animations` class.

```csharp
Player.Animations.Idle;     // from AnimationPlayer
Player.Animations.Hophop;   // from AnimatedSprite
```

**Off** — one class per node, named after the node (or its path if names repeat).

```csharp
Player.AnimationPlayer.Idle;
Player.AnimatedSprite.Hophop;
```

## File structure

```
addons/anvil/
├── plugin.cfg
├── AnvilPlugin.cs
├── anvil_rules.tres      # generated
├── generated/            # generated (with .gdignore)
│   ├── AnvilIds.cs
│   └── AnvilSceneIds.cs
├── icons/
│   └── anvil_icon.svg
└── src/
    ├── AnvilContextMenu.cs
    ├── AnvilFileIO.cs
    ├── AnvilGenerator.cs
    ├── AnvilNameScope.cs
    ├── AnvilNaming.cs
    ├── AnvilRule.cs
    ├── AnvilRuleManager.cs
    ├── AnvilRuleSet.cs
    ├── AnvilSceneGenerator.cs
    ├── AnvilSceneReader.cs
    ├── AnvilSceneRule.cs
    ├── AnvilScriptResolver.cs
    ├── AnvilTrackFolderDialog.cs
    ├── AnvilTrackSceneDialog.cs
    ├── AnvilValidator.cs
    ├── ForgeMode.cs
    └── SceneSources.cs
```

## Known limitations

- Renaming or moving a tracked **folder** isn't detected. Re-track the new location and remove the stale rule. (Scenes are tracked by UID and survive moves.)
- Scene ids come from the **root node** only: its C# script, and animation nodes owned by the scene itself.
- Ids go into a script class only after the project has been built; until then a standalone class is generated.
- Two scenes sharing a root script: only the first gets ids in the script class.
- A root-script member named `Animations` (Flatten on) or a node class name clashing with a Godot class falls back to a standalone class or a `Names` suffix, with a warning.
- Generated files are overwritten every time — don't hand-edit them.
- Generation runs on plugin load and via the Tools menu, not on every file or settings change.

## License

MIT