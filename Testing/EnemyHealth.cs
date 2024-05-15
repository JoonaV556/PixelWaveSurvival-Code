using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : Health
{
    protected override void Die() {
        // Destroy the enemy object
        Destroy(gameObject);
    }
}
