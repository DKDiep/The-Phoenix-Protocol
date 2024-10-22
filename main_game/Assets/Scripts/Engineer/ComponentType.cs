﻿using System;

public enum ComponentType
{
    ShieldGenerator     = 0,
    Turret              = 1,
    Engine              = 2,
    Hull                = 3,
	Drone               = 4,
	ResourceStorage     = 5,
    None                = 6
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
		if (s.Contains("Turret"))
			return ComponentType.Turret;
		else if (s.Contains("Engine"))
			return ComponentType.Engine;
		else if (s.Contains("Hull"))
			return ComponentType.Hull;
		else if (s.Contains("Shield"))
			return ComponentType.ShieldGenerator;
		else if (s.Contains("Resource"))
			return ComponentType.ResourceStorage;
        else if (s.Contains("Drone"))
            return ComponentType.Drone;
        else
            return ComponentType.None;

        // TODO: make sure the shield generator and resrouce storage actually have hat name
    }

    public static string GetComponentString(this ComponentType type)
    {
        switch (type)
        {
            case ComponentType.Turret:
                return "Turrets";
            case ComponentType.Engine:
                return "Engines";
            case ComponentType.Hull:
                return "Hull";
            case ComponentType.ShieldGenerator:
                return "Shield Generator";
            case ComponentType.ResourceStorage:
                return "Resource Storage";
            case ComponentType.Drone:
                return "Engineer Drone";
            default:
                return "";
        }
    }
}