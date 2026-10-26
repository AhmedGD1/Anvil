using System;

namespace Anvil;

[Flags]
public enum SceneSources
{
    None = 0,
    AnimationPlayers = 1 << 0,
    AnimatedSprites = 1 << 1,
    All = AnimationPlayers | AnimatedSprites,
}