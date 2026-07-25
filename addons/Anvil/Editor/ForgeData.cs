#if TOOLS
namespace Anvil.Editor;

internal class ForgeData
{
    public string Namespace { get; set; }
    public string OuterClass { get; set; }
    public string InnerClass { get; set; }
    public string ResourcePath { get; set; }
    public bool Recursive { get; set; }
}
#endif
