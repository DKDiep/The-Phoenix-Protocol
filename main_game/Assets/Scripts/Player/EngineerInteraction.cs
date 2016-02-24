using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class EngineerInteraction : NetworkBehaviour {
    private bool upgradeable = false;
    private bool repairable = false;

    // Not using auto properties for getters and setters
    // because the interaction between auto properties and
    // sync vars is unknown
    public void setUpgradeable(bool value)
    {
        upgradeable = value;
    }

    public bool getUpgradeable()
    {
        return this.upgradeable;
    }

    public void setRepairable(bool value)
    {
        repairable = value;
    }

    public bool getRepairable()
    {
        return this.repairable;
    }
}
