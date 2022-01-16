using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxElement : MonoBehaviour
{
    public int order = 0;
    public bool rotateWithFloor;
    public Vector2 spawnOffset;

    public float minSlopeAngle = -90;
    public float maxSlopeAngle = 90;

    public bool CanSpawnAtAngle(float angle)
    {
            if(angle>= minSlopeAngle && angle <= maxSlopeAngle)
            {
                return true;
            }
            else
            {
                return false;
            }
    }
}
