using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordAnimationController : MonoBehaviour
{
    [SerializeField]
    private MeleeAttack MeleeScript;

    public void OnAnimationEnded() {
        MeleeScript.OnAttackEnded();
    }
}
