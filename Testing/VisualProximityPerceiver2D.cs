using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VisualProximityPerceiver2D : Perceiver {
    // Perceives perceivables using a trigger collider and raycast
    // Collider is used to get list of considered targets
    // Raycast is done to check if targets can be seen



    #region Properties

    /// <summary>
    /// Frequency of the visibility checks in seconds
    /// </summary>
    [SerializeField]
    private float VisibilityCheckFrequency = 0.2f;
    [SerializeField, Tooltip("Which layers are included when doing visual check raycasts")]
    private LayerMask VisibilityLayerMask;
    [SerializeField, Tooltip("Which layers are included when doing visual check raycasts")]
    private ContactFilter2D VisibilityFilter;
    [SerializeField]
    private Transform VisibilityCheckOrigin;
    [SerializeField, Tooltip("Should the perceiver forget out of range targets?")]
    private bool ForgetTargets = true;
    /// <summary>
    /// How many seconds needs to pass before target is removed from the perceived targets list, if the target has exited out of sight
    /// </summary>
    [SerializeField, Tooltip("How many seconds it takes before non-visible target is forgotten")]
    private float ForgetDuration = 5f;
    [SerializeField]
    private bool EnableDebugRay = false;

    private float elapsedTime = 0f;
    private List<Perceivable> perceivablesInProximity = new();
    private List<LostSightPerceivable> LostSightPerceivables = new();
    List<Perceivable> TargetsToRemove;
    List<Perceivable> TargetsToAdd;
    List<LostSightPerceivable> lsPercsToRemove;

    #endregion

    void OnTriggerEnter2D(Collider2D otherCollider) {
        // Add perceivables which have entered proximity
        Perceivable perceivable = otherCollider.gameObject.GetComponent<Perceivable>();
        if (perceivable != null) {
            perceivablesInProximity.Add(perceivable);
        }
    }

    void OnTriggerExit2D(Collider2D otherCollider) {
        // Remove perceivables which are no longer in proximity
        Perceivable perceivable = otherCollider.gameObject.GetComponent<Perceivable>();
        if (perceivable == null) { return; }
        // Remove perceivable from visual checks
        if (perceivablesInProximity.Contains(perceivable)) {
            perceivablesInProximity.Remove(perceivable);
        }
        // Start removal cooldown
        if (PerceivedTargets.Contains(perceivable)) {
            // Check if target is already waiting to be removed
            bool IsTargetAlreadyUpForRemoval = IsTargetWaitingToBeForgotten(perceivable);
            // If target is not already to be removed, prepare it for removal
            if (!IsTargetAlreadyUpForRemoval && ForgetTargets) {
                PrepareTargetToBeForgotten(perceivable);
            }
        }
    }

    private void Update() {
        // Do visibility checks at specific interval
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= VisibilityCheckFrequency) {
            elapsedTime = 0f;
            LimitedUpdate();
        }
    }

    /// <summary>
    /// Update function with adjustable frequency
    /// </summary>
    private void LimitedUpdate() {
        // Prepare lists for perception loop    
        TargetsToRemove = new(); 
        TargetsToAdd = new();

        // Check if targets in proximity can be seen
        Vector3 RaycastOrigin = VisibilityCheckOrigin.position; // Cache origin position of raycast before loop
        CheckTargetsInProximity(RaycastOrigin);

        // Check if any perceivables have been out of sight for too long - If yes, prepare them for removal
        lsPercsToRemove = new();
        UpdateLostSightTargets();

        // Remove targets which the perceiver has lost sight on & Add new perceived target
        UpdatePerceivedTargets();
    }

    #region Perception

    private void CheckTargetsInProximity(Vector3 RaycastOrigin) {
        foreach (Perceivable perc in perceivablesInProximity) {
            Transform percTransform = perc.transform;
            // Calculate vector facing towards target
            Vector3 TargetVector = perc.VisualTransform.position - VisibilityCheckOrigin.position;
            // Do raycast
            RaycastHit2D HitResult = Physics2D.Raycast(RaycastOrigin, TargetVector, Mathf.Infinity, VisibilityLayerMask);
            // Do debug raycast
            if (EnableDebugRay) {
                Debug.DrawRay(RaycastOrigin, TargetVector, Color.green, 0.3f, false);
            }           
            // print(HitResult.collider.gameObject.name);

            // Prepare new targets to be added if they are seen
            bool CanSeeTarget = HitResult.transform == percTransform;
            bool IsTargetAlreadyPerceived = PerceivedTargets.Contains(perc);
            if (CanSeeTarget && !IsTargetAlreadyPerceived) {
                TargetsToAdd.Add(perc);
            }

            // Check if sight has been regained on target which was waiting to be forgotten
            // If yes, remove target from the forgotten cooldown
            bool IsTargetWaiting = IsTargetWaitingToBeForgotten(perc, out LostSightPerceivable lsPercWaiting);
            if (CanSeeTarget && IsTargetWaiting) {
                LostSightPerceivables.Remove(lsPercWaiting);
            }

            if (!ForgetTargets) { return; } // Cancel is forgetting targets is disabled
            // Prepare old targets for removal if they cannot be seen
            if (IsTargetAlreadyPerceived && !CanSeeTarget) {
                // Check if target is already waiting to be removed
                bool IsTargetAlreadyUpForRemoval = IsTargetWaitingToBeForgotten(perc);
                // If target is not already to be removed, prepare it for removal
                if (!IsTargetAlreadyUpForRemoval) {
                    PrepareTargetToBeForgotten(perc);
                }
            }
        }
    }

    private void PrepareTargetToBeForgotten(Perceivable perc) {
        LostSightPerceivable percToAdd = new LostSightPerceivable();
        percToAdd.Target = perc;
        // Set the time in future when the target should be forgotten, if visual sight is not regained
        percToAdd.TimeOfRemoval = Time.time + ForgetDuration;
        // Add the perceivable to be tracked
        LostSightPerceivables.Add(percToAdd);
    }

    private void UpdateLostSightTargets() {
        if (LostSightPerceivables.Any()) {
            foreach (LostSightPerceivable lsPerc in LostSightPerceivables) {
                if (Time.time >= lsPerc.TimeOfRemoval) {
                    lsPercsToRemove.Add(lsPerc);
                    TargetsToRemove.Add(lsPerc.Target);
                }
            }
        }
    }

    private void UpdatePerceivedTargets() {
        // Remove tracked lsPercs on cooldown
        if (lsPercsToRemove.Any()) {
            LostSightPerceivables = LostSightPerceivables.Except(lsPercsToRemove).ToList();
        }
        // Remove old targets
        if (TargetsToRemove.Any()) {
            RemoveTargets(TargetsToRemove);
        }     
        // Add new targets to perceived
        if (TargetsToAdd.Any()) {
            AddTargets(TargetsToAdd);
        }    
    }

    private bool IsTargetWaitingToBeForgotten(Perceivable perc) {
        // Check if the target is already up for removal
        foreach (LostSightPerceivable lsPerc in LostSightPerceivables) {
            if (lsPerc.Target == perc) {
                return true;
            }
        }
        return false;
    }

    private bool IsTargetWaitingToBeForgotten(Perceivable perc, out LostSightPerceivable LsPercWaiting) {
        // Check if the target is already up for removal
        foreach (LostSightPerceivable lsPerc in LostSightPerceivables) {
            if (lsPerc.Target == perc) {
                LsPercWaiting = lsPerc;
                return true;
            }
        }
        LsPercWaiting = null;
        return false;
    }

    /// <summary>
    /// A Perceivable target of which the perceiver has lost sight of. Will be removed from PerceivedTargets if sight is not regained before removal time
    /// </summary>
    protected class LostSightPerceivable {
        public Perceivable Target; // The target which is tracked
        public float TimeOfRemoval; // The point of time in future, at which the target will be removed if visual sight is not regained
    }

    #endregion
}
