using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles all status effects on a single character. Updates effects time left etc.
/// </summary>
public class StatusEffectHandler : MonoBehaviour
{
    [Serializable]
    public class ActiveStatusEffect
    {
        public StatusEffectBase effect;
        public Coroutine effectCoroutine;
    }

    public List<ActiveStatusEffect> statusEffects;

    public void TryApply(StatusEffectBase effectToApply)
    {
        // If similar effect is already applied, refresh its duration (Start the coroutine again with new duration) 
        if (statusEffects != null && statusEffects.Count > 0)
        {
            for (int i = 0; i < statusEffects.Count; i++)
            {
                var effectsOfSameType = statusEffects[i].effect.GetType() == effectToApply.GetType();
                if (effectsOfSameType)
                {
                    var oldEffect = statusEffects[i];
                    statusEffects[i].effectCoroutine = StartCoroutine(ProlongEffectCoroutine(effectToApply.duration, oldEffect));
                    break;
                }
            }
            return;
        }

        // No similar effect found, apply new effect
        statusEffects ??= new List<ActiveStatusEffect>();

        var newEffect = new ActiveStatusEffect
        {
            effect = effectToApply,
        };
        statusEffects.Add(newEffect);
        newEffect.effectCoroutine = StartCoroutine(EffectCoroutine(effectToApply.duration, newEffect));
    }

    private IEnumerator EffectCoroutine(float duration, ActiveStatusEffect ActiveEffect)
    {
        // Do stuff when effect is applied
        ActiveEffect.effect.OnApply(gameObject);

        // Wait for duration
        yield return new WaitForSeconds(duration);
        // Do stuff when effect is removed
        ActiveEffect.effect.OnRemove(gameObject);

        // Remove effect from active effects list
        statusEffects.Remove(ActiveEffect);
        yield break;
    }

    private IEnumerator ProlongEffectCoroutine(float duration, ActiveStatusEffect ActiveEffect)
    {
        // Wait for duration
        yield return new WaitForSeconds(duration);
        // Do stuff when effect is removed
        ActiveEffect.effect.OnRemove(gameObject);

        // Remove effect from active effects list
        statusEffects.Remove(ActiveEffect);
        yield break;
    }
}
