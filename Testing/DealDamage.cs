using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealDamage : MonoBehaviour {
    // Attach to 2d colliders which should deal damage when something hits them (weapon attack colliders etc.)

    /// <summary>
    /// How much damage is dealt to the health component of the other object when hit (if the object has one)
    /// </summary>
    [SerializeField]
    private float DamageToDeal = 100f;
    
    [SerializeField]
    private Transform DealerTransform;
    
    [SerializeField, Tooltip("Is the damage dealing collider a trigger collider?")]
    private bool IsTrigger;

    private void TryToDealDamage(Collider2D otherCollider) {
        Health otherObjHealth = otherCollider.gameObject.GetComponent<Health>();
        if (otherObjHealth != null) {
            otherObjHealth.TakeDamage(DamageToDeal, DealerTransform);
        } // asd
    }

    void OnCollisionEnter2D(Collision2D collision) {
        // Deal damage to the other object if it has a Health component
        if (IsTrigger) {
            return;
        }
        TryToDealDamage(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D otherCollider) {
        if (!IsTrigger) {
            return;
        }
        TryToDealDamage(otherCollider);
    }

}
