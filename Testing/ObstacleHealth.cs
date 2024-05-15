using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleHealth : Health
{
    public static event Action<Collider2D> OnObstacleDestroyed;

    [SerializeField]
    private Collider2D BoxCollider; 

    protected override void Die() {
        // Disable box collider for navmesh update
        BoxCollider.enabled = false;
        // Invoke event to update navigation grid around the collider - NavigationManager is a subscriber
        OnObstacleDestroyed?.Invoke(BoxCollider);
        // Destroy the enemy object
        Destroy(gameObject);
    }
}
