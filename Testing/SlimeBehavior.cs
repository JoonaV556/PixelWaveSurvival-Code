using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeBehavior : MonoBehaviour
{
    [SerializeField]
    AIPath AIPath;
    [SerializeField]
    Perceiver SlimePerceiver;

    private bool SeesPlayer = false;
    private Transform PlayerTransform;

    // TODO
    // Check if has player target
    // If yes - Set destination to player target

    private void OnEnable() {
        SlimePerceiver.OnTargetsAdded += CheckIfPlayerWasAdded;
        SlimePerceiver.OnTargetsRemoved += CheckIfPlayerWasRemoved;
    }
    private void OnDisable() {
        SlimePerceiver.OnTargetsAdded -= CheckIfPlayerWasAdded;
        SlimePerceiver.OnTargetsRemoved -= CheckIfPlayerWasRemoved;
    }

    private void Update() {
        if (SeesPlayer) {
            AIPath.destination = PlayerTransform.position;
        }
    }

    private void CheckIfPlayerWasAdded(List<Perceivable> percs) {
        // print("Detected add");
        foreach (Perceivable perc in percs) {
            if (perc.gameObject.CompareTag("Player")) {
                PlayerTransform = perc.transform;
                SeesPlayer = true;
                break;
            }
        }
    }
    private void CheckIfPlayerWasRemoved(List<Perceivable> percs) {
        // print("Detected removal");
        if (!SeesPlayer) {
            return;
        }
        foreach (Perceivable perc in percs) {
            if (perc.gameObject.CompareTag("Player")) {
                SeesPlayer = false;
                PlayerTransform = null;
                break;
            }
        }
    }
}
