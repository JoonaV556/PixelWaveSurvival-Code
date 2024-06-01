using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Status effect which reduces targets movement speed by certain percentage.
/// </summary>
public class Crippled : StatusEffectBase
{
    float ReduceSpeedPercentage;
    float appliedModifier;
    public Crippled(float duration, float reduceSpeedPercentage) : base(duration)
    {
        this.ReduceSpeedPercentage = reduceSpeedPercentage;
    }

    public override void OnApply(GameObject target)
    {
        var movement = target.TryGetComponent(out CharacterMovement characterMovement);
        if (movement)
        {
            // movementComponent.ReduceSpeed(ReduceSpeedPercentage);
            // Calculate modifier to add using the ReduceSpeedPercentage
            appliedModifier = characterMovement.GetMovementForce() * (ReduceSpeedPercentage / 100);
            // Reduce player's movement speed by the passed percentage
            characterMovement.AddMovementModifier(-appliedModifier);
        }
    }

    public override void OnRemove(GameObject target)
    {
        var movement = target.TryGetComponent(out CharacterMovement characterMovement);
        if (movement)
        {
            // Return movement speed to normal
            characterMovement.AddMovementModifier(appliedModifier);
        }
    }
}
