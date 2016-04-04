using System;

public class Officer {
    public const float MAX_AMMO = 100;
    public uint PlayerId { get; private set; }
    public float Ammo { get; set; }
    public string Name { get; set; }

    // We don't want to be able to create officers
    // without a PlayerId
    private Officer() { }

    public Officer (uint playerId)
    {
        this.PlayerId = playerId;
        this.Ammo = MAX_AMMO;
        this.Name = "<No name supplied>";
    }

    // Chained constructor. Calls the above constructor before
    // setting the name attribute
    public Officer(uint playerId, string name) : this(playerId)
    {
        this.Name = name;
    }

    /// <summary>
    /// Used to serialize this object
    /// into a string that is safe for
    /// sending over the network
    /// </summary>
    /// <returns></returns>
    public string SerializeToString()
    {
        string serializedObject = "";

        serializedObject += this.PlayerId.ToString() + ",";
        serializedObject += this.Name + ",";
        serializedObject += this.Ammo.ToString() + ",";

        return serializedObject;
    }

    /// <summary>
    /// Returns the Officer object represented by
    /// serializedObject
    /// </summary>
    /// <param name="serializedObject"></param>
    /// <returns></returns>
    public static Officer DeserializeFromString(string serializedObject)
    {
        string[] comma = { "," };
        string[] fields = serializedObject.Split(comma, StringSplitOptions.RemoveEmptyEntries);

        if (fields.Length < 3)
        {
            throw new Exception("Not enough fields!");
        }

        uint id;
        try
        {
            id = UInt32.Parse(fields[0]);
        } catch (Exception e)
        {
            throw e;
        }

        string name = fields[1];

        float ammo;
        try
        {
            ammo = float.Parse(fields[2]);
        }
        catch (Exception e)
        {
            throw e;
        }

        Officer deserialized = new Officer(id, name);
        deserialized.Ammo = ammo;
        return deserialized;
    }
}
