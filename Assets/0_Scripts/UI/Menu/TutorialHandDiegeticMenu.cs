using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialHandDiegeticMenu : MonoBehaviour
{
    public static TutorialHandDiegeticMenu instance;
    public bool tutorialHandAnimStarted = false;
    float currentTutorialHandAnimTime = 0;
    public Transform dedo;
    public GameObject blackVeil;
    float currentTutorialHandAnimFreq = 0.4f;
    int sentido = 1;
    Vector3 finalPos;
    Vector3 initialPos;
    ButtonExtraInfo buttonExtraInfo;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }
    }

    private void Update()
    {
        ProcessBlackVeilAnim();
    }

    public void StartBlackVeilAnim(SpriteRenderer diegeticButton, GameObject button, bool useBlackVeil=true)
    {
        if (!tutorialHandAnimStarted)
        {
            dedo.position = new Vector3(diegeticButton.bounds.max.x, diegeticButton.bounds.min.y, dedo.position.z);
            initialPos = dedo.localPosition;
            dedo.gameObject.SetActive(true);
            finalPos = dedo.localPosition + (new Vector3(1, -1, 0) * 0.5f);
            tutorialHandAnimStarted = true;
            currentTutorialHandAnimTime = 0;
            if(useBlackVeil)blackVeil.SetActive(true);
            if (button.GetComponent<Button>() == null) Debug.LogError("There is not Button component in " + button.name);
            buttonExtraInfo = button.AddComponent<ButtonExtraInfo>();
        }
    }

    public void ProcessBlackVeilAnim()
    {
        if (tutorialHandAnimStarted)
        {
            currentTutorialHandAnimTime += Time.deltaTime;
            float progress = currentTutorialHandAnimTime / currentTutorialHandAnimFreq;
            float realProgress = sentido > 0 ? progress : 1 - progress;
            float x = EasingFunction.Linear(initialPos.x, finalPos.x, realProgress);
            float y = EasingFunction.Linear(initialPos.y, finalPos.y, realProgress);
            dedo.localPosition = new Vector3(x, y, dedo.localPosition.z);
            if (progress >= 1)
            {
                currentTutorialHandAnimTime = 0;
                sentido = -sentido;
            }

            if (buttonExtraInfo.buttonUpTimes >= 1)
            {
                FinishBlackVeilAnim();
            }
        }
    }

    public void FinishBlackVeilAnim()
    {
        if (tutorialHandAnimStarted)
        {
            tutorialHandAnimStarted = false;
            dedo.gameObject.SetActive(false);
            if(blackVeil.activeInHierarchy)blackVeil.SetActive(false);
        }
    }
}
