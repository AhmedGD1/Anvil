# Anvil

A Godot 4 (C#) editor plugin that generates strongly-typed, compile-time-safe
`StringName` IDs for your files — no more typo-prone magic strings for asset
paths, sound names, or resource keys.

Point it at a folder, and Anvil generates a nested static class full of
`StringName` constants, one per file, named after the file itself.

Instead of this:

```csharp
GD.Print("main_theme"); // typo-prone, no autocomplete, breaks silently
```

You get this:

```csharp
GD.Print(Audio.Music.MainTheme); // compile-time checked, autocompletes
```

## Installation

1. Copy the `addons/Anvil` folder into your project's `addons/` directory.
2. In Godot, go to **Project → Project Settings → Plugins** and enable **Anvil**.

## Usage

### 1. Mark a partial class with `[Forge]`

```csharp
using Anvil;

public partial class Audio
{
    [Forge("res://audio/music")]
    public static partial class Music
    {
    }

    [Forge("res://audio/sfx", recursive: true)]
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
- `recursive` (optional, default `false`) — when `true`, subfolders are
  scanned too and all files are flattened into the same class. If two files
  in different subfolders share a name, the first one found wins and the
  rest are skipped with a console warning naming the conflicting file.

### 2. Generate

In the Godot editor, go to **Project → Tools → Generate Anvil IDs**.

Anvil scans your project for every `[Forge]` attribute, and for each one
writes a generated file at:

```
res://addons/Anvil/Generated/<OuterClassName>.anvilgen.cs
```

Re-running generation clears out old generated files first, so it's always
safe to run again after adding or removing assets.

### 3. Use the generated constants

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

Reference them anywhere, fully typed:

```csharp
audioPlayer.Stream = storage.LoadById<AudioStream>(Audio.Music.MainTheme);
```

### 4. `EnumNames` — for inspector dropdowns

Every generated class also gets an `EnumNames` const: a comma-joined string
of every file name in that folder, ready to drop straight into
`PropertyHint.Enum`:

```csharp
[Export(PropertyHint.Enum, Audio.Music.EnumNames)]
public string SelectedTrack;
```

This gives you an inspector dropdown of every file in the folder, kept in
sync automatically every time you regenerate. A file name containing a comma
is excluded from `EnumNames` (it would corrupt the hint string) but still
gets its own `StringName` constant; a warning is printed naming the file.

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

## License

MIT
