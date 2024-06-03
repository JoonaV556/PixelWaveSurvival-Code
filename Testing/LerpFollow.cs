using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpFollow : MonoBehaviour
{
    public Transform target;

    [Range(0, 1)]
    public float alpha = 0.1f;

    public bool useMultiplier = false;

    public float speedMultiplier = 1;


    private void Update()
    {
        float timedAlpha;
        if (useMultiplier)
        {
            timedAlpha = alpha * Time.deltaTime * speedMultiplier;
        }
        else
        {
            timedAlpha = alpha * Time.deltaTime;
        }


        transform.position = Vector3.Lerp(transform.position, target.position, timedAlpha);
    }
}
