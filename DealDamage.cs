using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealDamage : MonoBehaviour
{
    // Attach to 2d colliders which should deal damage when something hits them (weapon attack colliders etc.)

    /// <summary>
    /// How much damage is dealt to the health component of the other object when hit (if the object has one)
    /// </summary>
    [SerializeField]
    private float DamageToDeal = 10;

    public bool DealDamageOnlyOnce = false;

    [SerializeField, Tooltip("Is the damage dealing collider a trigger collider?")]
    private bool IsTrigger;

    private Transform DealerTransform;
    bool dealtDamage = false;

    private void TryToDealDamage(Collider2D otherCollider)
    {
        Health otherObjHealth = otherCollider.gameObject.GetComponent<Health>();
        if (otherObjHealth != null && !dealtDamage)
        {
            otherObjHealth.TakeDamage(DamageToDeal, transform);
            dealtDamage = true;
            AfterDamageDealt();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Deal damage to the other object if it has a Health component
        if (IsTrigger)
        {
            return;
        }
        TryToDealDamage(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D otherCollider)
    {
        if (!IsTrigger)
        {
            return;
        }
        TryToDealDamage(otherCollider);
    }

    protected virtual void AfterDamageDealt()
    {
        // Override this method to add custom behavior after damage is dealt
        // For example, play a sound or particle effect 
    }
}
