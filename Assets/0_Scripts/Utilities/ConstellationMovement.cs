using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ConstellationMovement : MonoBehaviour
{
    public Transform[] constellationPoints;

    public int currentConstellationIndex = 0;
    public bool started = false;
    public bool playback = false;
    public float maxMovementTime = 4;
    float currentMovementTime = 0;
    public Ease easingFunction = Ease.EaseInOutCubic;
    float totalConstellationDistance = 0;
    float currentConstellationDistance = 0;
    float currentCompletedSegmentDist = 0;
    float currentSegmentDist = 0;
    float currentSegmentTotalDist = 0;
    float originalScaleX;

    UnityAction actionOnEnd;
    float currentAngle;

    private void Awake()
    {
        originalScaleX = transform.localScale.x;
        for (int i = 0; i < constellationPoints.Length - 1; i++)
        {
            totalConstellationDistance += (constellationPoints[i + 1].position - constellationPoints[i].position).magnitude;
        }
    }

    private void Update()
    {
        ProcessMoving();
    }


    public void StartMoving(bool _playback = false, UnityAction _actionOnEnd=null)
    {
        if (!started)
        {
            started = true;
            actionOnEnd = _actionOnEnd;
            playback = _playback;
            currentConstellationIndex = playback ? constellationPoints.Length - 1 : 0;
            currentConstellationDistance = 0;
            currentSegmentDist = 0;
            currentSegmentTotalDist = 0;
            currentCompletedSegmentDist = 0;
            currentMovementTime = 0;

            if (playback) transform.localScale = new Vector3(-originalScaleX, transform.localScale.y, transform.localScale.z);
            else transform.localScale = new Vector3(originalScaleX, transform.localScale.y, transform.localScale.z);
            currentAngle = constellationPoints[currentConstellationIndex].localRotation.eulerAngles.z;
            transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, currentAngle);
        }
    }

    void ProcessMoving()
    {
        if (started)
        {
            currentMovementTime += Time.deltaTime;
            float progress = Mathf.Clamp01(currentMovementTime / maxMovementTime);
            if (playback) progress = 1 - progress;
            currentConstellationDistance = EasingFunction.SelectEasingFunction(easingFunction, 0, totalConstellationDistance, progress);
            if (playback)
            {
                if ((currentConstellationIndex - 1) > 0) currentSegmentTotalDist =
         (constellationPoints[currentConstellationIndex - 1].position - constellationPoints[currentConstellationIndex].position).magnitude;
                else FinishMoving();
            }
            else
            {
                if ((currentConstellationIndex + 1) < constellationPoints.Length) currentSegmentTotalDist =
       (constellationPoints[currentConstellationIndex + 1].position - constellationPoints[currentConstellationIndex].position).magnitude;
                else FinishMoving();
            }


            float extraDist = playback? totalConstellationDistance - currentConstellationDistance - currentCompletedSegmentDist: currentConstellationDistance - currentCompletedSegmentDist;
    //        Debug.Log("B4 while -> progress = " + progress + "; currentConstellationDistance = " + currentConstellationDistance + "; currentCompletedSegmentDist = " + currentCompletedSegmentDist +
    //"; currentSegmentTotalDist = " + currentSegmentTotalDist + "; currentConstellationIndex = " + currentConstellationIndex + "; extraDist = " + extraDist);
            while (extraDist > currentSegmentTotalDist && ((playback && currentConstellationIndex - 1 > 0) || (!playback && currentConstellationIndex + 1 < constellationPoints.Length)))
            {
                extraDist -= currentSegmentTotalDist;
                currentCompletedSegmentDist += currentSegmentTotalDist;
                if (playback)
                {
                    currentConstellationIndex--;
                    if (currentConstellationIndex - 1 > 0)
                        currentSegmentTotalDist = (constellationPoints[currentConstellationIndex - 1].position - constellationPoints[currentConstellationIndex].position).magnitude;

                }
                else
                {
                    currentConstellationIndex++;
                    if ((currentConstellationIndex + 1) < constellationPoints.Length) currentSegmentTotalDist =
(constellationPoints[currentConstellationIndex + 1].position - constellationPoints[currentConstellationIndex].position).magnitude;
                }

                continue;
            }

            float angleProgress = Mathf.Clamp01(extraDist / currentSegmentTotalDist);
            float targetAngle = 0;
            if (playback)
            {
                if ((currentConstellationIndex - 1) > 0)
                {
                    Vector3 dir = (constellationPoints[currentConstellationIndex - 1].position - constellationPoints[currentConstellationIndex].position).normalized;
                    Vector3 newPos = constellationPoints[currentConstellationIndex].position + (dir * extraDist);
                    transform.position = newPos;
                    targetAngle = constellationPoints[currentConstellationIndex - 1].localRotation.eulerAngles.z;
                }
                else
                {
                    FinishMoving();
                }
            }
            else
            {
                if ((currentConstellationIndex + 1) < constellationPoints.Length)
                {
                    Vector3 dir = (constellationPoints[currentConstellationIndex + 1].position - constellationPoints[currentConstellationIndex].position).normalized;
                    Vector3 newPos = constellationPoints[currentConstellationIndex].position + (dir * extraDist);
                    transform.position = newPos;
                    targetAngle = constellationPoints[currentConstellationIndex + 1].localRotation.eulerAngles.z;
                }
                else
                {
                    FinishMoving();
                }
            }
            float angleDiff = Mathf.Abs(targetAngle - constellationPoints[currentConstellationIndex].localRotation.eulerAngles.z);
            if (angleDiff > 180)
            {
                if (constellationPoints[currentConstellationIndex].localRotation.eulerAngles.z > 180)
                    targetAngle += 360;
                else targetAngle -= 360;
            }
            currentAngle = EasingFunction.SelectEasingFunction(Ease.Linear, constellationPoints[currentConstellationIndex].localRotation.eulerAngles.z, targetAngle, angleProgress);
            transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y,currentAngle);
            //Debug.Log(/*"progress = "+ progress + "; currentConstellationDistance = "+ currentConstellationDistance + "; currentCompletedSegmentDist = "+ currentCompletedSegmentDist+
            //    "; currentSegmentTotalDist = "+ currentSegmentTotalDist + "; currentConstellationIndex = "+ currentConstellationIndex + "; extraDist = "+ extraDist +*/
            //    "; localRotation = " + constellationPoints[currentConstellationIndex].localRotation.eulerAngles.z+"; currentAngle = "+currentAngle+ "; targetAngle = " + targetAngle);
            if (progress >= 1) FinishMoving();
        }
    }

    public void FinishMoving()
    {
        if (started)
        {
            //Debug.Log("FINISH CONSTELLATION MOVEMENT");
            int lastIndex = playback ? 0 : constellationPoints.Length - 1;
            transform.position = constellationPoints[lastIndex].position;
            started = false;
            transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, constellationPoints[lastIndex].localRotation.eulerAngles.z);
            actionOnEnd();
        }
    }
}
