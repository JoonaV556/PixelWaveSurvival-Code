using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerKnockBack : KnockBackEffect {

    [SerializeField]
    private PlayerMovement movementComp;
    
    [SerializeField, Tooltip("Duration in seconds during which player movement will be disabled to add the knockback effect")]
    private float PreventMoveDuration = 0.6f;

    protected override void OnKnockBack() {
        movementComp.EnableMovement = false;
        Invoke("EnableMovement", PreventMoveDuration);
    }

    private void EnableMovement() {
        movementComp.EnableMovement = true;
    }
}
