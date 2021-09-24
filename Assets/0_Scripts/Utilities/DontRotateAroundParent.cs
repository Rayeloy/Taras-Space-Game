using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class DontRotateAroundParent : MonoBehaviour
{
    Vector3 savedPosition;

    [HelpBox("When DontMoveWithParent is on, Ctrl+Z doesn't work for movement changes on this GameObject.", HelpBoxMessageType.Warning)]
    public bool dontRotateAroundParent = true;
    bool lastDontRotateAroundParent = true;

    Vector3 parentLastPos;

    private void OnEnable()
    {
        //Debug.Log("DontMoveWithParent OnEnable");
        savedPosition = transform.position - transform.parent.position;
    }

    private void OnDisable()
    {
        ThisUpdate();
    }

    public void SetUp()
    {
        Debug.Log("DontMoveWithParent SetUp");
        savedPosition = transform.position - transform.parent.position;
    }

    public void ThisUpdate()
    {
        if (transform.hasChanged && !transform.parent.hasChanged && savedPosition != transform.position)
        {
            savedPosition = transform.position - transform.parent.position;
            transform.hasChanged = false;
        }

        if (!lastDontRotateAroundParent && dontRotateAroundParent)
            savedPosition = transform.position - transform.parent.position;

        //parentLastPos = transform.parent.position;
        //StartCoroutine(CheckIfParentHasMoved(0.1f));

        lastDontRotateAroundParent = dontRotateAroundParent;

        if (dontRotateAroundParent)
        {
            if (savedPosition == Vector3.zero)
            {
                savedPosition = transform.position - transform.parent.position;
            }

            if (transform.parent.hasChanged)
            {
                savedPosition = transform.position - transform.parent.position;
                transform.parent.hasChanged = false;
            }
        }
    }

    private void Update()
    {
        if (transform.hasChanged && !transform.parent.hasChanged && savedPosition != transform.position)
        {
            savedPosition = transform.position - transform.parent.position;
            transform.hasChanged = false;
        }

        if (!lastDontRotateAroundParent && dontRotateAroundParent)
            savedPosition = transform.position - transform.parent.position;

        //parentLastPos = transform.parent.position;
        //StartCoroutine(CheckIfParentHasMoved(0.1f));

        lastDontRotateAroundParent = dontRotateAroundParent;
    }

    private void LateUpdate()
    {
        if (dontRotateAroundParent)
        {
            if (savedPosition == Vector3.zero)
            {
                savedPosition = transform.position - transform.parent.position;
            }

            if (transform.parent.hasChanged)
            {
                //Debug.Log("DontMoveWithParent:I'm " + gameObject + " and my parent has changed positions, so I set mine from " + transform.position + " to " + savedPosition);
                transform.position = transform.parent.position + savedPosition;
                transform.parent.hasChanged = false;
            }
        }
    }
}
