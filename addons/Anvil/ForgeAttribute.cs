using System;

namespace Anvil;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ForgeAttribute(string folderPath, bool recursive = false, ForgeMode mode = ForgeMode.Id) : Attribute
{
    public string Path { get; } = folderPath;
    public bool Recursive { get; } = recursive;
    public ForgeMode Mode { get; } = mode;
}

public enum ForgeMode
{
    Id,
    FullPath,
}
