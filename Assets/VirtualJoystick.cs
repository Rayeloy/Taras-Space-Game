using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VirtualJoystick : MonoBehaviour
{
    public RectTransform joystickBackground;
    public RectTransform joystickImage;
    Button joystickButton;


    public Vector2 joystickInput;

    public bool joystickPressed = false;
    public int touchID = -1;
    List<int> oldTouchIDs;

    private void Awake()
    {
        joystickButton = joystickImage.GetComponent<Button>();
        oldTouchIDs = new List<int>();
    }

    private void Update()
    {
        if (joystickPressed)
        {
            UpdateJoystickPosition();
        }
        else
        {
            oldTouchIDs = new List<int>();
            for (int i = 0; i < Input.touchCount; i++)
            {
                oldTouchIDs.Add(Input.GetTouch(0).fingerId);
            }
        }
    }

    public void PressDownJoystick()
    {
        if (!joystickPressed)
        {
            Debug.Log("Joystick Pressed");

            int count = Input.touchCount;
            bool found = false;
            Vector2 localPoint = Vector2.zero;

            for (int i = 0; i < count && !found; i++)
            { // verify all touches
                Touch touch = Input.GetTouch(i);
                if (oldTouchIDs.Contains(touch.fingerId)) continue;
                // if touch inside some button Rect, set the corresponding value

                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(joystickBackground, touch.position, null, out localPoint))
                {
                    if (localPoint.magnitude <= joystickBackground.rect.width / 2)
                    {
                        found = true;
                        touchID = touch.fingerId;
                    }
                }
            }

            if (found)
            {
                joystickPressed = true;
            }
        }
    }

    public void ReleaseJoystick()
    {
        if (joystickPressed)
        {
            joystickPressed = false;
            Debug.Log("Joystick Released");
            joystickInput = Vector3.zero;
            joystickImage.transform.localPosition = Vector3.zero;
            touchID = -1;
        }
    }

    void UpdateJoystickPosition()
    {
        int count = Input.touchCount;
        Vector2 localPoint = Vector2.zero;
        Touch touch = new Touch();
        bool found = false;
        for (int i = 0; i < count && !found; i++)
        {
            if (Input.GetTouch(i).fingerId == touchID)
            {
                touch = Input.GetTouch(i);
                found = true;
            }
        }
        if (!found)
        {
            ReleaseJoystick();
            return;
        }
        RectTransformUtility.ScreenPointToLocalPointInRectangle(joystickBackground, touch.position, null, out localPoint);

        if (localPoint.magnitude > joystickBackground.rect.width * 0.4f)
        {
            localPoint = localPoint.normalized * (joystickBackground.rect.width * 0.4f);
        }
        joystickImage.localPosition = localPoint;
        float xInput = localPoint.x / (joystickBackground.rect.width * 0.4f);
        float yInput = localPoint.y / (joystickBackground.rect.width * 0.4f);
        //Debug.Log("Joystick pressed: localPoint.x = " + localPoint.x + "; localPoint.y = " + localPoint.y + ";  joystickBackground.rect.width * 0.4f= " + (joystickBackground.rect.width * 0.4f));

        joystickInput = new Vector2(xInput, yInput);
    }

}
