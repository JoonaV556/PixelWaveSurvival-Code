using System;
using UnityEngine;

public class EnemyInput : CharacterInput
{
    public Action OnMainAttackAttempted;

    public void AttemptMainAttack()
    {
        OnMainAttackAttempted?.Invoke();
    }
}