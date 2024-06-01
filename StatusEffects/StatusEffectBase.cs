using System;
using UnityEngine;

[Serializable]
public class StatusEffectBase
{
    public float duration;

    public StatusEffectBase(float duration)
    {
        this.duration = duration;
    }

    public virtual void OnApply(GameObject target)
    {

    }

    public virtual void OnRemove(GameObject target)
    {

    }
}
