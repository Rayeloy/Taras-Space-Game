using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class DontScaleWithParent : MonoBehaviour
{
    Vector3 savedScale;

    [HelpBox("When DontScaleWithParent is on, Ctrl+Z doesn't work for scale changes on this GameObject.", HelpBoxMessageType.Warning)]
    public bool dontScaleWithParent = true;
    bool lastDontScaleWithParent = true;
    public Vector3 targetRealScale = Vector3.one;

    Vector3 parentLastScale;

    private void OnDisable()
    {
        if (Application.isPlaying) return;

        ThisUpdate();
    }

    public void ThisUpdate()
    {
        if (transform.hasChanged && !transform.parent.hasChanged && savedScale != transform.lossyScale)
        {
            savedScale = transform.lossyScale;
            transform.hasChanged = false;
        }

        if (!lastDontScaleWithParent && dontScaleWithParent)
            savedScale = transform.lossyScale;

        //parentLastPos = transform.parent.position;
        //StartCoroutine(CheckIfParentHasMoved(0.1f));

        lastDontScaleWithParent = dontScaleWithParent;

        if (dontScaleWithParent)
        {
            if (savedScale == Vector3.zero)
            {
                savedScale = transform.lossyScale;
            }

            if (transform.parent.hasChanged)
            {
                SetGlobalScale(transform, savedScale);
                transform.parent.hasChanged = false;
            }
        }
    }

    private void Update()
    {
        //if (transform.hasChanged && !transform.parent.hasChanged && savedScale != transform.localScale)
        //{
        //    Debug.Log("Saving New Scale");
        //    savedScale = transform.lossyScale;
        //    transform.hasChanged = false;
        //}

        //if (!lastDontScaleWithParent && dontScaleWithParent)
        //{
        //    Debug.Log("Saving New Scale");
        //    savedScale = transform.lossyScale;
        //}

        //parentLastPos = transform.parent.position;
        //StartCoroutine(CheckIfParentHasMoved(0.1f));

        lastDontScaleWithParent = dontScaleWithParent;
    }

    private void LateUpdate()
    {
        if (dontScaleWithParent)
        {
            //if (savedScale == Vector3.zero)
            //{
            //    Debug.Log("Saving New Scale");
            //    savedScale = transform.lossyScale;
            //}

            if (transform.parent.hasChanged)
            {
                //Debug.Log("DontScaleWithParent:I'm " + gameObject + " and my parent has changed scale, so I set mine from " + transform.lossyScale + " to " + targetRealScale);
                SetGlobalScale(transform, targetRealScale);
                transform.parent.hasChanged = false;
            }
        }
    }

    IEnumerator CheckIfParentHasScaled(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        if (parentLastScale == transform.parent.localScale)
        {
            transform.parent.hasChanged = false;
            //Debug.Log("My parent has changed? " + transform.parent.hasChanged);
        }
        else
        {
            //Debug.Log("My parent has changed? " + transform.parent.hasChanged);
        }
    }

    public void SetGlobalScale(Transform transform, Vector3 globalScale)
    {
        if (transform.lossyScale.x == 0 || transform.lossyScale.y == 0) return;
        transform.localScale = Vector3.one;
        transform.localScale = new Vector3(globalScale.x / transform.lossyScale.x, globalScale.y / transform.lossyScale.y, 1);
    }
}
