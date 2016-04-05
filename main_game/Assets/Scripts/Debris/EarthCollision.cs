using UnityEngine;
using System.Collections;

public class EarthCollision : MonoBehaviour 
{
    private const int PLAYER_COLLISION_DAMAGE = int.MaxValue; // Ensure player is killed
    private GameState gameState;

    void Start()
    {
        gameState = GameObject.Find("GameManager").GetComponent<GameState>();
    }

    // Cause damage if collided with
    void OnTriggerEnter (Collider col)
    {
        gameState.DamageShip(PLAYER_COLLISION_DAMAGE);
    }
}
