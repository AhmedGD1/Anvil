using System;

namespace Anvil;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ForgeAttribute(string folderPath, bool recursive = false) : Attribute
{
    public string Path { get; } = folderPath;
    public bool Recursive { get; } = recursive;
}
