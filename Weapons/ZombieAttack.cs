using System;
using System.Collections;
using System.Collections.Generic;
using AttackEffects;
using UnityEngine;

public class ZombieAttack : MonoBehaviour
{
    [Tooltip("How much damage zombie deals to player if player hits zombies body. Not same as attack")] public float hitDamage = 2;

    [Tooltip("How much damage zombie deals per attack")] public float attackDamage = 5;

    public float hitKnockbackForce = 10f;
    public float attackKnockbackForce = 20f;

    public float disableMovementDurationOnKnockback = 0.9f;

    public float hitCooldown = 1f; // Minimum time in seconds between body hits

    Coroutine hitCooldownCoroutine;

    public void OnCollisionEnter2D(Collision2D collision)
    {
        TryToDealDamageToPlayer(collision);
    }

    public void OnCollisionStay2D(Collision2D collision)
    {
        TryToDealDamageToPlayer(collision);
    }

    private void TryToDealDamageToPlayer(Collision2D collision)
    {
        collision.gameObject.TryGetComponent(out Health health);
        bool shouldHitPlayer = collision.gameObject.CompareTag("Player") && health != null && hitCooldownCoroutine == null;
        if (shouldHitPlayer)
        {
            // Deal damage to target
            // print("Zombie hit player");
            health.TakeDamage(10, transform);
            hitCooldownCoroutine = StartCoroutine(HitCooldown());
            // Apply hit knockback to target
            var knockback = new KnockbackEffect(
                collision.gameObject,
                hitKnockbackForce,
                (collision.transform.position - transform.position).normalized,
                disableMovementDurationOnKnockback
                );
        }
    }

    private IEnumerator HitCooldown()
    {
        yield return new WaitForSeconds(hitCooldown);
        hitCooldownCoroutine = null;
    }
}

// \/\/\/ Refactor to separate file \/\/\/
namespace AttackEffects
{
    using UnityEngine;

    [Serializable]
    public abstract class AttackEffect
    {
        public GameObject targetObject;
    }

    public class KnockbackEffect : AttackEffect
    {
        /// <summary>
        /// Applies knockback to target object
        /// </summary>
        /// <param name="targetObject"></param>
        /// <param name="knockbackForce"></param>
        /// <param name="knockbackDirection">Direction to apply the knockback towards. Does not need to be normalized</param>
        /// <param name="disableMovementDuration">Duration to disable movement on target object in seconds</param>
        public KnockbackEffect(GameObject targetObject, float knockbackForce, Vector2 knockbackDirection, float disableMovementDuration = 0f)
        {
            targetObject.TryGetComponent(out Rigidbody2D rb);

            bool canApply = rb != null;

            if (canApply)
            {
                // Disable movement to enable adding rigidbody force
                if (disableMovementDuration > 0)
                {
                    DisableMovementOnTarget(targetObject, disableMovementDuration);
                }

                float scaledKnockBackForce = knockbackForce * rb.mass;

                // Apply knockback to target object
                Debug.Log("Applied knockback to target object: " + targetObject.name);
                rb.AddForce(knockbackDirection.normalized * scaledKnockBackForce, ForceMode2D.Impulse);
            }
        }

        private void DisableMovementOnTarget(GameObject targetObject, float disableMovementDuration)
        {
            targetObject.TryGetComponent(out CharacterMovement movement);
            if (movement != null)
            {
                // Prevent clamping velocity while knockback is applied
                movement.ClampVelocity = false;
                // Stop clamping velocity after duration
                movement.StartCoroutine(EnableMovementOnTarget(movement, disableMovementDuration));
            }
        }

        private IEnumerator EnableMovementOnTarget(CharacterMovement movement, float disableMovementDuration)
        {
            yield return new WaitForSeconds(disableMovementDuration);
            movement.ClampVelocity = true;
        }
    }
}
