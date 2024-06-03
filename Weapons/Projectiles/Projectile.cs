using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float Damage;
    public float FlyDistanceBeforeAutoDestroy = 100f;

    protected List<StatusEffectBase> EffectsToApply;

    private void Awake()
    {
        OnCreated();
    }

    /// <summary>
    /// Apply custom stuff on projectiles here. Such as status effects
    /// </summary>
    protected virtual void OnCreated()
    {

    }

    private void OnEnable()
    {
        // Init position for max fly distance stuff
        lastPosition = transform.position;
        currentPosition = transform.position;
    }

    private void Update()
    {
        HandleAutoDestroy();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        HandleDamage(other);

        HandleStatusEffects(other);
    }

    private void HandleDamage(Collision2D other)
    {
        // Try to deal damage to the object that was hit
        var health = other.gameObject.TryGetComponent(out Health healthComponent);
        if (health)
        {
            healthComponent.TakeDamage(Damage);
            // TypeLog(this, "Dealt damage to " + other.gameObject.name);
            Destroy(this.gameObject);
        }
        else
        {
            // Other object is not damageable, probably obstacle
            // TypeLog(this, "Hit obstacle");
            Destroy(this.gameObject);
        }
    }

    private void HandleStatusEffects(Collision2D other)
    {
        if (EffectsToApply != null)
        {
            // Apply effects to the object that was hit
            var canHandleStatusEffects = other.gameObject.TryGetComponent(out StatusEffectHandler effectHandler);
            if (canHandleStatusEffects)
            {
                foreach (var effect in EffectsToApply)
                {
                    effectHandler.TryApply(effect);
                }
            }
        }
    }

    // Create generic function which allows to pass instance of any type (generic) as a parameter
    public void TypeLog<T>(T type, string message)
    {
        Debug.Log(type.GetType().Name + ": " + message);
    }

    #region Destroy after flying too far
    float distanceFlown = 0f;
    Vector2 lastPosition;
    Vector2 currentPosition;
    private void HandleAutoDestroy()
    {
        currentPosition = transform.position;
        distanceFlown += Vector2.Distance(lastPosition, currentPosition);
        lastPosition = currentPosition;
        if (distanceFlown > FlyDistanceBeforeAutoDestroy)
        {
            // TypeLog(this, "Auto-destroyed projectile.");
            Destroy(this.gameObject);
        }
    }
    #endregion
}
