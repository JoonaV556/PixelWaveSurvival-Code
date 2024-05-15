using Pathfinding;
using UnityEngine;

public class EnemyKnockBack : KnockBackEffect {

    [SerializeField]
    private AIPath AIPath;
    [SerializeField, Tooltip("Duration in seconds during which AIPath movement will be disabled to add the knockback effect")]
    private float PreventMoveDuration = 1f;


    protected override void OnKnockBack() {
        // Set AIpath CanMove to false during knockback duration
        AIPath.canMove = false;
        Invoke("EnableMovement", PreventMoveDuration);
    }

    private void EnableMovement() {
        AIPath.canMove = true;
    }
}
