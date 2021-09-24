using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObject : MonoBehaviour
{
    public float vertDistance = 1;
    float maxHeight;
    float minHeight;

    bool movingStarted = false;
    [Tooltip("Time it takes to go from up to down or from down to up")]
    public float movingFrequency = 1;
    float movingTime = 0;
    bool movingUp = true;
    float progress = 0;

    private void Start()
    {
        StartMoving();
    }

    private void Update()
    {
        ProcessMoving();
    }

    void StartMoving()
    {
        if (!movingStarted)
        {
            movingStarted = true;
            movingTime = movingFrequency / 2;
            maxHeight = transform.position.y + vertDistance;
            minHeight = transform.position.y - vertDistance;
            progress = 0.5f;
            movingUp = false;
        }
    }


    void ProcessMoving()
    {
        if (movingStarted)
        {
            movingTime += Time.deltaTime;
            progress = movingTime / movingFrequency;
            progress = Mathf.Clamp01(progress);
            float newY = 0;
            if (movingUp)
            {
                newY = EasingFunction.EaseInOutQuad(minHeight, maxHeight, progress);
            }
            else
            {
                newY = EasingFunction.EaseInOutQuad(maxHeight, minHeight, progress);
            }
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            if (movingTime >= movingFrequency)
            {
                movingTime = 0;
                movingUp = !movingUp;
            }
        }
    }

    void StopMoving()
    {
        movingStarted = false;
    }
}
