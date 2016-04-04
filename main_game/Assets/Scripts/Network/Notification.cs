using System.Collections;
using System.Collections.Generic;

/// <summary>
/// A singleton class that holds notification data
/// This is used to send upgrade/repair notifications to the phone server
/// which are then displayed on officers' phones
/// </summary>
public class Notification {
    // This table provides an O(1) lookup for Notification objects. It is used to ensure
    // that there is only one Notification object with particular values.
    private static Dictionary<ComponentType, Dictionary<bool, Notification>> objectTable
        = new Dictionary<ComponentType, Dictionary<bool, Notification>>();

    public ComponentType Component { get; private set; }
    public bool IsUpgrade { get; private set; }

    // Take away the publicly accessible empty constructor
    private Notification() { }

    private Notification(bool isUpgrade, ComponentType component)
    {
        this.Component = component;
        this.IsUpgrade = isUpgrade;
    }

    public static Notification create(bool isUpgrade, ComponentType component)
    {
        // Try to return the object from the object table
        // If that fails we create a new one
        try
        {
            return objectTable[component][isUpgrade];
        }
        catch (KeyNotFoundException)
        {
            Notification notification = new Notification(isUpgrade, component);
            objectTable.Add(component, new Dictionary<bool, Notification>());
            objectTable[component].Add(isUpgrade, notification);
            return notification;
        }
    }
}
