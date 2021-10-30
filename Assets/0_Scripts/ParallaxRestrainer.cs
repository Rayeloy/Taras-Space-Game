using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxRestrainer : MonoBehaviour
{
    public bool restrainXToLeft, restrainXToRight;
    [Range(-1,1)]
    public float cameraOffsetX;
}
