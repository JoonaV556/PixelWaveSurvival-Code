using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpRotate : MonoBehaviour
{
    public enum RotateType
    {
        Lerp,
        Slerp
    }

    public Transform target1;
    public Transform target2;

    Transform target;

    [Range(0, 1)]
    public float alpha = 0.1f;

    public bool useMultiplier = false;

    public float speedMultiplier = 1;

    public RotateType rotateType = RotateType.Lerp;

    private void Start()
    {
        target = target1;
    }

    private void Update()
    {
        // Switch rotate target when space is pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (target == target2)
            {
                target = target1;
            }
            else
            {
                target = target2;
            }
        }

        var directionToTarget = (target.position - transform.position).normalized;

        float timedAlpha;
        if (useMultiplier)
        {
            timedAlpha = alpha * Time.deltaTime * speedMultiplier;
        }
        else
        {
            timedAlpha = alpha * Time.deltaTime;
        }

        switch (rotateType)
        {
            case RotateType.Lerp:
                //transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(directionToTarget, Vector3.forward), timedAlpha);
                transform.up = Vector3.Lerp(transform.up.normalized, directionToTarget, timedAlpha);
                break;
            case RotateType.Slerp:
                transform.up = Vector3.Slerp(transform.up.normalized, directionToTarget, timedAlpha);
                break;
        }

    }
}
