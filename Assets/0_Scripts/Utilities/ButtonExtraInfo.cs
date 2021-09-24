using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ButtonExtraInfo : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{

    public bool buttonPressed;
    public bool buttonDown;
    public bool buttonUp;

    public int buttonUpTimes = 0;

    private void Awake()
    {
        buttonUpTimes = 0;
    }

    private void Update()
    {
        if (buttonDown) buttonDown = false;
        if (buttonUp) buttonUp = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        buttonPressed = true;
        if (!buttonDown) buttonDown = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        buttonPressed = false;
        if (!buttonUp)
        {
            eventData.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            // Expose this as a variable in your script so other components can check for it.
            bool isPointerOverUI = false;

            for (int i = 0; i < results.Count && !isPointerOverUI; i++)
            {
                if (results[i].gameObject.GetComponent<RectTransform>() == GetComponent<RectTransform>()) isPointerOverUI = true;
            }
            if (isPointerOverUI)
            {

                    buttonUp = true;
                buttonUpTimes++;
            }
        }
    }
}