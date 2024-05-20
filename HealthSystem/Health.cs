using UnityEngine;
using UnityEngine.Events;

public abstract class Health : MonoBehaviour
{
    // Handles behavior for entities with health, and which can be killed/destroyed
    #region Properties

    /// <summary>
    /// How much health the object should have at start
    /// </summary>
    [SerializeField]
    protected float MaxHealth = 10f;

    /// <summary>
    /// How much health the object has currently
    /// </summary>
    public float currentHealth;

    #endregion

    public UnityEvent_Vector2 OnReceiveDamage;
    public UnityEvent<float> OnTakeDamage;
    public UnityEvent<float> OnHealed;
    public UnityEvent OnDeath;

    private void Start()
    {
        currentHealth = MaxHealth;
    }

    /// <summary>
    /// Used to deal damage and knockback to this health component
    /// </summary>
    public void TakeDamage(float DamageToTake, Transform DamageGiver)
    {
        // Info listeners about damage giver position
        Vector2 DamageGiverPos = DamageGiver.transform.position;
        OnReceiveDamage?.Invoke(DamageGiverPos);

        // Take damage
        TakeDamage(DamageToTake);
    }

    /// <summary>
    /// Used to deal damage to this health component
    /// </summary>
    public void TakeDamage(float DamageToTake)
    {
        // print(gameObject.name + " Took damage");
        if (currentHealth - DamageToTake <= 0f)
        {
            currentHealth = 0f;
            OnTakeDamage?.Invoke(currentHealth);
            OnDeath?.Invoke();
            Die();
        }
        else
        {
            currentHealth -= DamageToTake;
            OnTakeDamage?.Invoke(currentHealth);
        }
    }

    /// <summary>
    /// Heals the object for the specified amount.
    /// </summary>
    /// <param name="HealAmount">How much to heal</param>
    public void Heal(float HealAmount)
    {
        // print("Healed");
        if (currentHealth + HealAmount >= MaxHealth)
        {
            currentHealth = MaxHealth;
        }
        else
        {
            currentHealth += HealAmount;
        }
        OnHealed?.Invoke(currentHealth);
    }

    /// <summary>
    /// Triggered when health reaches zero
    /// </summary>
    protected abstract void Die();
}
