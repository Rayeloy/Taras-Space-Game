using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Rendering;

public class CutoutMask : Image
{
    public override Material materialForRendering 
    {
        get
        {
            Material material = new Material(base.materialForRendering);
            material.SetInt("_StencilComp", (int)CompareFunction.NotEqual);
            return material;
        }
    }
}
