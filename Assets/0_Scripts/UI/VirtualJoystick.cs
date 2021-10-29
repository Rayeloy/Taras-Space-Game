using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VirtualJoystick : MonoBehaviour
{
    public RectTransform joystickPressingArea;
    public RectTransform joystickImage;
    public RectTransform joystickOuterRing;
    public RectTransform arrow;
    Button joystickButton;

    public float deadZone = 0.2f;
    public Vector2 joystickInput;

    public bool joystickPressed = false;
    public int touchID = -1;
    public List<int> oldTouchIDs;

    Color joystickOriginalColor, ringOriginalColor;
    Color joystickTransparentColor, ringTransparentColor;

    Vector3 joystickRingOriginalPos;

    public delegate void SinglePress();
    public event SinglePress OnSinglePress;
    public float maxSinglePressTime = 0.3f;
    float pressedTime = 0;
    public float singlePressMaxMagnitude = 0.6f;

    public bool mobile= true;

    private void Awake()
    {
        joystickButton = joystickImage.GetComponent<Button>();
        oldTouchIDs = new List<int>();

        joystickOriginalColor = joystickImage.GetComponent<Image>().color;
        ringOriginalColor = joystickOuterRing.GetComponent<Image>().color;
        joystickTransparentColor = new Color(joystickOriginalColor.r, joystickOriginalColor.g, joystickOriginalColor.b, 0.2f);
        ringTransparentColor = new Color(ringOriginalColor.r, ringOriginalColor.g, ringOriginalColor.b, 0.1f);
        joystickImage.GetComponent<Image>().color = joystickTransparentColor;
        joystickOuterRing.GetComponent<Image>().color = ringTransparentColor;

        joystickRingOriginalPos = joystickOuterRing.localPosition;
        arrow.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (joystickPressed)
        {
            UpdateJoystickPosition();
        }
        else
        {
            oldTouchIDs.Clear();
            for (int i = 0; i < Input.touchCount; i++)
            {
                oldTouchIDs.Add(Input.GetTouch(i).fingerId);
            }
        }
    }

    public void PressDownJoystick()
    {
        if (!joystickPressed)
        {
            //Debug.Log("Joystick Pressed");
            int count = Input.touchCount;
            bool found = false;
            Vector2 localPoint = Vector2.zero;

            for (int i = 0; i < count && !found; i++)
            { // verify all touches
                Touch touch = Input.GetTouch(i);
                if (oldTouchIDs.Contains(touch.fingerId)) continue;

                RectTransform touchArea = mobile ? joystickPressingArea : joystickOuterRing;

                //Is the touch input inside the rect of joystickBackground?
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(touchArea, touch.position, null, out localPoint))
                {
                    if (localPoint.magnitude <= touchArea.rect.width)
                    {
                        found = true;
                        touchID = touch.fingerId;
                    }
                }
            }

            if (found)//PRESS JOYSTICK
            {
                joystickPressed = true;
                pressedTime = 0;
                //Move the whole joystick to the touch position
                if(mobile)joystickOuterRing.localPosition = localPoint;

                //Set Pressed Colors
                joystickImage.GetComponent<Image>().color = joystickOriginalColor;
                joystickOuterRing.GetComponent<Image>().color = ringOriginalColor;
            }
        }
    }

    public void ReleaseJoystick()
    {
        if (joystickPressed)
        {
            if (pressedTime <= maxSinglePressTime && OnSinglePress != null && joystickInput.magnitude < singlePressMaxMagnitude)
            {
                OnSinglePress();
            }
            joystickPressed = false;
            //Debug.Log("Joystick Released");
            joystickInput = Vector3.zero;
            joystickImage.transform.localPosition = Vector3.zero;
            touchID = -1;
            joystickImage.GetComponent<Image>().color = joystickTransparentColor;
            joystickOuterRing.GetComponent<Image>().color = ringTransparentColor;

            if(mobile)joystickOuterRing.localPosition = joystickRingOriginalPos;

            if (arrow.gameObject.activeInHierarchy) arrow.gameObject.SetActive(false);

        }
    }

    void UpdateJoystickPosition()
    {
        pressedTime += Time.deltaTime;
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
        RectTransformUtility.ScreenPointToLocalPointInRectangle(joystickOuterRing, touch.position, null, out localPoint);

        if (localPoint.magnitude > joystickOuterRing.rect.width * 0.4f)
        {
            localPoint = localPoint.normalized * (joystickOuterRing.rect.width * 0.4f);
        }
        joystickImage.localPosition = localPoint;
        float xInput = localPoint.x / (joystickOuterRing.rect.width * 0.4f);
        float yInput = localPoint.y / (joystickOuterRing.rect.width * 0.4f);
        //Debug.Log("Joystick pressed: localPoint.x = " + localPoint.x + "; localPoint.y = " + localPoint.y + ";  joystickBackground.rect.width * 0.4f= " + (joystickBackground.rect.width * 0.4f));

        joystickInput = new Vector2(xInput, yInput);

        if (joystickInput.magnitude >= deadZone)
        {
            if (!arrow.gameObject.activeInHierarchy) arrow.gameObject.SetActive(true);
            float arrowAngle = Vector2.SignedAngle(Vector2.right, joystickInput);
            joystickImage.rotation = Quaternion.Euler(0, 0, arrowAngle);
        }
        else
        {
            if (arrow.gameObject.activeInHierarchy) arrow.gameObject.SetActive(false);
        }
    }

}
