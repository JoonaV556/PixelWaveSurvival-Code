using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float Damage;

    private void OnCollisionEnter2D(Collision2D other)
    {
        // Try to deal damage to the object that was hit
        var health = other.gameObject.TryGetComponent(out Health healthComponent);
        if (health)
        {
            healthComponent.TakeDamage(Damage);
            TypeLog(this, "Dealt dammage to " + other.gameObject.name);
            Destroy(this.gameObject);
        }
        else
        {
            // Other object is not damageable, probably obstacle
            TypeLog(this, "Hit obstacle");
            Destroy(this.gameObject);
        }
    }

    // Create generic function which allows to pass instance of any type (generic) as a parameter
    public void TypeLog<T>(T type, string message)
    {
        Debug.Log(type.GetType().Name + ": " + message);
    }
}
