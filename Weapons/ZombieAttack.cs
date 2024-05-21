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
    public float attackCooldown = 1f; // Minimum time in seconds between attacks

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