#if TOOLS
using Godot;
using Godot.Collections;

namespace Anvil;

[Tool, GlobalClass]
public partial class AnvilRuleSet : Resource
{
    [Export] public Array<AnvilRule> FolderRules { get; set; } = [];
}
#endif
