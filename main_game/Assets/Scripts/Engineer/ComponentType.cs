using System;

public enum ComponentType
{
    None,
	Bridge,
	Engine,
	Turret,
	ShieldGenerator
}

public static class ComponentTypeMethods
{
    /// <summary>
    /// Gets a component type from a string.
    /// </summary>
    /// <returns>The component type.</returns>
    /// <param name="s">The string containing the type.</param>
    public static ComponentType GetComponentType(this string s)
    {
        if(s.Contains("Turret"))
            return ComponentType.Turret;
        else if(s.Contains("Engine"))
            return ComponentType.Engine;
        else if(s.Contains("Bridge"))
            return ComponentType.Bridge;
        else if(s.Contains("Shield"))
            return ComponentType.ShieldGenerator;
        else
            return ComponentType.None;

        // TODO: make sure the shield generator actually has that name
    }
}