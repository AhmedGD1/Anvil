#if TOOLS
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Anvil;

public static class AnvilScriptResolver
{
    public static Dictionary<string, Type> BuildScriptTypeMap()
    {
        var map = new Dictionary<string, Type>();

        Type[] types;
        try
        {
            types = typeof(AnvilScriptResolver).Assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            types = e.Types.Where(t => t is not null).ToArray();
        }

        foreach (Type type in types)
        {
            var attribute = type.GetCustomAttribute<ScriptPathAttribute>(false);
            if (attribute is not null)
                map[attribute.Path] = type;
        }

        return map;
    }

    public static bool DeclaresMember(Type type, string name)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static
                                   | BindingFlags.Public | BindingFlags.NonPublic
                                   | BindingFlags.DeclaredOnly;

        return type.GetMembers(flags).Any(m => m.Name == name && m.MemberType != MemberTypes.NestedType);
    }
}
#endif
