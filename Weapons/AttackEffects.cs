namespace AttackEffects
{
    using UnityEngine;
    using System;
    using System.Collections;

    [Serializable]
    public abstract class AttackEffect
    {
        public GameObject targetObject;
    }

    public class KnockbackEffect : AttackEffect
    {
        /// <summary>
        /// Attempts to apply knockback to target object by adding force to its Rigidbody2D component.
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

