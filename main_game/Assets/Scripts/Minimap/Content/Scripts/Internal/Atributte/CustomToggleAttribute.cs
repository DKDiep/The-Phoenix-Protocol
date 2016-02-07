using UnityEngine;

public class CustomToggleAttribute : PropertyAttribute
{
    public readonly string title;


    public CustomToggleAttribute()
    {
        this.title = "";
    }

    public CustomToggleAttribute(string _title)
    {
        this.title = _title;
    }
}