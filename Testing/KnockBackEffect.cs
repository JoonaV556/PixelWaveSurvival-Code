using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class KnockBackEffect : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D RigidBody;
    [SerializeField]
    float KnockBackForce = 1f;

    public void DoKnockBack(Vector2 SourcePosition) {
        OnKnockBack();
        Vector2 knockBackDir = (Vector2)transform.position - SourcePosition;
        RigidBody.AddForce(knockBackDir.normalized * KnockBackForce, ForceMode2D.Impulse);
    }

    protected abstract void OnKnockBack();
}