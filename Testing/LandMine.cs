using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandMine : DealDamage
{
    protected override void AfterDamageDealt()
    {
        Debug.Log("Landmine exploded");
        Destroy(gameObject);
    }
}
