using System.Collections;
using AttackEffects;
using UnityEngine;

public class ZombieAttack : MonoBehaviour
{
    // Handles attack logic for zombie 

    // Has following attack types:
    // Directional main attack (more damage, more knockback)
    // Body attack (less damage, less knockback)

    // Main attack is triggered by input event from EnemyInput
    // Body attack is triggered by collision with other objects

    // Both attacks attempt to apply knockback to target object. See AttackEffects.cs for more info

    [Tooltip("How much damage zombie deals to player if player hits zombies body. Not same as attack")] public float hitDamage = 2;

    [Tooltip("How much damage zombie deals per attack")] public float attackDamage = 5;

    public LayerMask AttackLayers;

    public float HitKnockbackForce = 10f;
    public float AttackKnockbackForce = 20f;

    public float DisableMovementDurationOnKnockback = 0.9f;

    public float HitCooldown = 1f; // Minimum time in seconds between body hits
    public float AttackCooldown = 1f; // Minimum time in seconds between attacks
    public float MainAttackRadius = 0.5f;

    public bool EnableAttackGizmos = false;

    [Tooltip("How much distance is added to attack position in front of zombie in addition to radius")] public float AttackDistanceAddedToRadius = 0.3f;

    Vector2 attackPos = Vector2.zero;

    Coroutine hitCooldownCoroutine;

    Coroutine attackCooldownCoroutine;

    EnemyInput input;

    private void OnEnable()
    {
        input = GetComponent<EnemyInput>();
        input.OnMainAttackAttempted += AttemptMainAttack;
    }

    private void OnDisable()
    {
        input.OnMainAttackAttempted -= AttemptMainAttack;
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        AttemptBodyAttack(collision.gameObject);
    }

    public void OnCollisionStay2D(Collision2D collision)
    {
        AttemptBodyAttack(collision.gameObject);
    }

    private bool TryToDealDamageToPlayer(GameObject target, float damageAmount, float knockbackForce)
    {
        target.TryGetComponent(out Health health);
        bool shouldHitPlayer = target.CompareTag("Player") && health != null;
        if (shouldHitPlayer)
        {
            health.TakeDamage(damageAmount);

            // Apply hit knockback to target
            var knockback = new KnockbackEffect(
                target,
                knockbackForce,
                (target.transform.position - transform.position).normalized,
                DisableMovementDurationOnKnockback
                );
            return true;
        }
        return false;
    }

    // Damages other characters if they touch the zombie
    private void AttemptBodyAttack(GameObject target)
    {
        // print("Zombie attempted body attack");
        // Check if attack is on cooldown
        if (hitCooldownCoroutine != null)
        {
            return;
        }
        // If attack is succesful, start cooldown
        if (TryToDealDamageToPlayer(target, hitDamage, HitKnockbackForce))
        {
            hitCooldownCoroutine = StartCoroutine(HitCooldownRoutine());
        };
    }

    // Does the zombies directional main attack
    private void AttemptMainAttack()
    {
        //print("Zombie attempted main attack");
        // Check if attack is on cooldown
        if (attackCooldownCoroutine != null)
        {
            return;
        }

        var attackDirection = input.LookInput; // Get from input   
        attackPos = new Vector2(transform.position.x, transform.position.y) + attackDirection * (MainAttackRadius + AttackDistanceAddedToRadius); // Calculate from look direction - direction.normalized * circle radius

        // Check for objects in attack direction
        var hits = Physics2D.OverlapCircleAll(
            attackPos,
            MainAttackRadius,
            AttackLayers
            );

        // Try to damage each object hit
        foreach (var hit in hits)
        {
            // If attack is succesful, start cooldown
            if (TryToDealDamageToPlayer(hit.gameObject, attackDamage, AttackKnockbackForce))
            {
                attackCooldownCoroutine = StartCoroutine(AttackCooldownRoutine());
            };
        }
    }

    // Debugging only - Draw attack sphere with main attack
    private void OnDrawGizmos()
    {
        if (!EnableAttackGizmos)
        {
            return;
        }
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPos, MainAttackRadius);
    }

    private IEnumerator HitCooldownRoutine()
    {
        yield return new WaitForSeconds(HitCooldown);
        hitCooldownCoroutine = null;
    }

    private IEnumerator AttackCooldownRoutine()
    {
        yield return new WaitForSeconds(AttackCooldown);
        attackCooldownCoroutine = null;
    }
}
