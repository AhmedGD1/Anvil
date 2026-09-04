#if TOOLS
using Godot;

namespace Anvil;

[Tool, GlobalClass]
public partial class AnvilRule : Resource
{
    [Export(PropertyHint.Dir)] 
    public string SourceFolder { get; set; } = "";

    [Export] 
    public string OutputName { get; set; } = "";

    [Export]
    public bool Recursive { get; set; } = false;

    [Export] 
    public ForgeMode Mode { get; set; } = ForgeMode.Id;
}
#endif
