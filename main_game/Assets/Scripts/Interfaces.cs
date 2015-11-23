using UnityEngine;
using System.Collections;

//Interface for arrow keys or WASD
public interface INavigatable
{
    void Up();
    void Down();
    void Left();
    void Right();
}

public interface IShootable
{

}