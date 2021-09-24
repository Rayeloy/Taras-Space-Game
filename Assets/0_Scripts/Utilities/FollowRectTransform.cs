using UnityEngine;

[ExecuteInEditMode]
public class FollowRectTransform : MonoBehaviour
{
    public RectTransform followRect;
    public Vector3 offset;
    RectTransform myRT;
    Vector3 oldFollowPos;
    public bool ignoreParentScale = false;
    private void Awake()
    {
        myRT = GetComponent<RectTransform>();
    }

    private void Start()
    {
        oldFollowPos = followRect.position;
    }

    private void OnEnable()
    {
        oldFollowPos = followRect.position;
    }

    private void LateUpdate()
    {
        if(oldFollowPos != followRect.position)
        {
            if(ignoreParentScale) myRT.position = followRect.position + offset;
            else
            {
                myRT.position = followRect.parent.TransformPoint((followRect.localPosition + offset));
            }
                oldFollowPos = followRect.position;
        }
    }
}
