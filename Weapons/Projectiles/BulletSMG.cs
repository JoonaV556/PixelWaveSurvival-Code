using System.Collections;
using System.Collections.Generic;
using Pathfinding.Examples.RTS;
using UnityEngine;

public class BulletSMG : Projectile
{
    public float crippledDuration = 0.7f;
    public float crippledPercentage = 30f;
    protected override void OnCreated()
    {
        EffectsToApply = new()
        {
            new Crippled(crippledDuration, crippledPercentage)
        };
    }
}
