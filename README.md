# Anvil

A Godot 4 (C#) editor plugin that generates strongly-typed, compile-time-safe
`StringName` constants for your files — no more typo-prone magic strings for
asset paths, sound names, or resource keys.

Point it at a folder, and Anvil generates a nested static class full of
constants, one per file, named after the file itself.

Instead of this:

```csharp
GD.Print("main_theme"); // typo-prone, no autocomplete, breaks silently
```

You get this:

```csharp
GD.Print(Audio.Music.MainTheme); // compile-time checked, autocompletes
```

## Two modes, two different jobs

Anvil can generate constants in one of two shapes, controlled by `ForgeMode`.
Picking the right one for each folder matters — they solve different
problems:

| Mode | Constant holds | Use it for |
|---|---|---|
| `ForgeMode.Id` (default) | The file's short name, e.g. `"main_theme"` | ID-based systems: audio bus names, animation names, save-data keys, `PropertyHint.Enum` dropdowns — anywhere you compare or store a name rather than load a file |
| `ForgeMode.FullPath` | The full loadable path, e.g. `"res://audio/music/main_theme.ogg"` | Directly loading a resource: `GD.Load<T>(...)`, `[Export]` preload paths — anywhere the string needs to point straight at the file |

If you're not sure which you need: **if you're about to call `GD.Load`,
you want `FullPath`. If you're about to compare, store, or display a name,
you want `Id`.**

## Installation

1. Copy the `addons/Anvil` folder into your project's `addons/` directory.
2. In Godot, go to **Project → Project Settings → Plugins** and enable **Anvil**.

## Usage

### 1. Mark a partial class with `[Forge]`

```csharp
using Anvil;

public partial class Audio
{
    // Id mode (default) — short names, for ID-based lookups
    [Forge("res://audio/music")]
    public static partial class Music
    {
    }

    // FullPath mode — full res:// paths, ready to GD.Load directly
    [Forge("res://audio/sfx", mode: ForgeMode.FullPath)]
    public static partial class Sfx
    {
    }
}
```

- The outer class (`Audio`) must be `partial`.
- The inner class (`Music`, `Sfx`) must be `static partial` and carry the
  `[Forge]` attribute.
- The path passed to `[Forge]` is a `res://` folder — every valid file inside
  it becomes a constant.
- `mode` (optional, default `ForgeMode.Id`) — see the table above. Can be
  passed positionally (`[Forge("res://audio/sfx", ForgeMode.FullPath)]`) or
  named (`mode: ForgeMode.FullPath`).
- `recursive` (optional, default `false`) — when `true`, subfolders are
  scanned too and all files are flattened into the same class. If two files
  in different subfolders share a name, the first one found wins and the
  rest are skipped with a console warning naming the conflicting file.
  Works with either mode, in any argument order:
  `[Forge("res://audio/sfx", recursive: true, mode: ForgeMode.FullPath)]`.

### 2. Generate

In the Godot editor, go to **Project → Tools → Generate Anvil IDs**.

Anvil scans your project for every `[Forge]` attribute, and for each one
writes a generated file at:

```
res://addons/Anvil/Generated/<OuterClassName>.anvilgen.cs
```

Re-running generation clears out old generated files first, so it's always
safe to run again after adding or removing assets.

### 3. Using `Id` mode

```csharp
public partial class Audio
{
    public static partial class Music
    {
        public static readonly StringName MainTheme = "main_theme";
        public static readonly StringName BossFight = "boss_fight";

        public const string EnumNames = "main_theme,boss_fight";
    }
}
```

These constants hold a **name**, not a path — use them wherever your code
compares, stores, or looks something up by that name:

```csharp
if (currentTrackName == Audio.Music.MainTheme) { /* ... */ }
saveData.LastPlayedTrack = Audio.Music.BossFight;
```

#### `EnumNames` — for inspector dropdowns

`Id` mode also generates an `EnumNames` const: a comma-joined string of every
file name in that folder, ready to drop straight into `PropertyHint.Enum`:

```csharp
[Export(PropertyHint.Enum, Audio.Music.EnumNames)]
public string SelectedTrack;
```

This gives you an inspector dropdown of every file in the folder, kept in
sync automatically every time you regenerate. A file name containing a comma
is excluded from `EnumNames` (it would corrupt the hint string) but still
gets its own constant; a warning is printed naming the file.

### 4. Using `FullPath` mode

```csharp
public partial class Audio
{
    public static partial class Sfx
    {
        public static readonly StringName Digging = "res://audio/sfx/digging.ogg";
        public static readonly StringName Planted = "res://audio/sfx/planted.ogg";
    }
}
```

These constants hold a **complete, loadable path** — pass them straight into
`GD.Load`, no string-building required:

```csharp
audioPlayer.Stream = GD.Load<AudioStream>(Audio.Sfx.Digging);
```

There's no `EnumNames` in this mode — a full path isn't a meaningful value
for an inspector dropdown, so it's skipped.

## Configuring valid file extensions

By default, Anvil picks up these extensions:

```
.wav .ogg .mp3 .tscn .tres .glb .blend .fbx
```

You can change this list in **Project Settings** under:

```
anvil/generator/valid_extensions
```

Any file in a `[Forge]`-marked folder whose extension isn't in this list is
ignored. `.import` files are always ignored.

## Notes

- Generated files live under `res://addons/Anvil/Generated/` and are marked
  with a `.gdignore`, so Godot won't try to import them as resources.
- Generated files are overwritten on every run — don't hand-edit them.
- File names are converted to PascalCase for the generated constant name
  (e.g. `main-theme.ogg` → `MainTheme`).
- With `recursive: true`, all matched files across every subfolder are
  flattened into one class. Duplicate names across subfolders are not both
  included — the first match wins, and later duplicates are skipped with a
  console warning.
- `mode` only changes what a constant holds (a name vs. a full path) — it
  doesn't change how files are discovered, so `recursive`, extension
  filtering, and duplicate handling all work the same way regardless of mode.

## License

MIT
