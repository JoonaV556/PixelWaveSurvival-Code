using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Perceiver : MonoBehaviour
{
    public List<Perceivable> PerceivedTargets = new();
    // public event Action<Perceivable> OnTargetRemoved;
    // public event Action<Perceivable> OnTargetAdded;
    public event Action<List<Perceivable>> OnTargetsRemoved;
    public event Action<List<Perceivable>> OnTargetsAdded;

    // TODO - Add continuous detection function with a frequency which can be adjusted
    // protected void AddTarget(Perceivable NewPerceivable) {
    //     PerceivedTargets.Add(NewPerceivable);
    // }
    // protected void RemoveTarget(Perceivable NewPerceivable) {
    //     if (PerceivedTargets.Contains(NewPerceivable)) {
    //         PerceivedTargets.Remove(NewPerceivable);
    //     }
    // }

    protected void AddTargets(List<Perceivable> NewTargets) {
        // Add new targets
        PerceivedTargets = PerceivedTargets.Union(NewTargets).ToList();
        // Invoke event
        OnTargetsAdded?.Invoke(NewTargets);
    }
    protected void RemoveTargets(List<Perceivable> NewTargets) {
        // Remove targets
        PerceivedTargets = PerceivedTargets.Except(NewTargets).ToList();
        // Invoke event
        OnTargetsRemoved?.Invoke(NewTargets);
    }
}
