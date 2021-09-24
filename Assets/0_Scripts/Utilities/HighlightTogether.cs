using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
//using UnityEditor.Events;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(EventTrigger))]
public class HighlightTogether : MonoBehaviour
{
    public Text[] targetText;
    public TextMeshProUGUI[] targetTextMeshPro;
    [HideInInspector]
    public List<Color> colorsText;
    [HideInInspector]
    public List<Color> colorsFinalText;
    [HideInInspector]
    public List<Color> colorsTMP;
    [HideInInspector]
    public List<Color> colorsFinalTMP;
    public List<Image> targetImages;
    [HideInInspector]
    public List<Color> colorsImages;
    [HideInInspector]
    public List<Color> colorsFinalImages;
    public Color highLightShade = new Color(0.66f, 0.66f, 0.66f, 1);
    public Button boton;
    [HideInInspector]
    public bool pointerDown;
    [HideInInspector]
    public bool pointerExit;
    float transition1;
    float transition2;
    float fadeTime;
    bool ignoreTimeScale = true;
    float currentTime = 0;
    float lastRealTime = 0;
    float finalDeltaTime = 0;

    // Use this for initialization
    private void Awake()
    {
        if (boton == null) boton = GetComponent<Button>();
        if (GetComponent<EventTrigger>() == null) gameObject.AddComponent<EventTrigger>();
        EventTrigger eventTrig = GetComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerDown;
        entry.callback.RemoveListener((eventData) => { ActivateHighlight(); });
        entry.callback.AddListener((eventData) => { ActivateHighlight(); });
        eventTrig.triggers.Add(entry);

        EventTrigger.Entry entry2 = new EventTrigger.Entry();
        entry2.eventID = EventTriggerType.PointerUp;
        entry2.callback.RemoveListener((eventData) => { DeactivateHighlight(); });
        entry2.callback.AddListener((eventData) => { DeactivateHighlight(); });
        eventTrig.triggers.Add(entry2);

        pointerDown = false;
        pointerExit = false;
        transition1 = -1;
        transition2 = -1;
        fadeTime = boton.colors.fadeDuration;
        colorsText = new List<Color>();
        colorsFinalText = new List<Color>();
        colorsImages = new List<Color>();
        colorsFinalImages = new List<Color>();
        colorsTMP = new List<Color>();
        colorsFinalTMP = new List<Color>();
    }

    /*private void Start()
    {
        //boton.onClick.AddListener(highlightText);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        stopHighlightText();
    }

    public void OnPoinerDown(PointerEventData eventData)
    {
        highlightText();
    }*/



    private void Update()
    {
        currentTime = (Time.realtimeSinceStartup - lastRealTime);
        lastRealTime = Time.realtimeSinceStartup;
        if (ignoreTimeScale)
        {
            finalDeltaTime = currentTime;
        }
        else finalDeltaTime = Time.deltaTime;
        if (!boton.interactable && !pointerExit && !pointerDown && transition1 == -1 && transition2 == -1)
        {
            return;
        }
        if (pointerDown)
        {
            pointerDown = false;
            transition1 = 0;
            transition2 = -1;
            for (int i = 0; i < targetImages.Count; i++)
            {
                //Debug.Log("gameObject.name = "+ gameObject.name + "; targetImages[i] = " + targetImages[i].gameObject.name);
                colorsImages.Add(targetImages[i].color);
                float alpha = highLightShade.a;
                float newR = targetImages[i].color.r + (highLightShade.r - targetImages[i].color.r) * alpha;
                float newG = targetImages[i].color.g + (highLightShade.g - targetImages[i].color.g) * alpha;
                float newB = targetImages[i].color.b + (highLightShade.b - targetImages[i].color.b) * alpha;
                Color cf = new Color(newR, newG, newB, 1f);
                colorsFinalImages.Add(cf);
            }
            for (int i = 0; i < targetTextMeshPro.Length; i++)
            {
                colorsTMP.Add(targetTextMeshPro[i].color);
                //Debug.Log("Color = " + targetTextMeshPro[i].color);
                float alpha = highLightShade.a;
                float newR = targetTextMeshPro[i].color.r + (highLightShade.r - targetTextMeshPro[i].color.r) * alpha;
                float newG = targetTextMeshPro[i].color.g + (highLightShade.g - targetTextMeshPro[i].color.g) * alpha;
                float newB = targetTextMeshPro[i].color.b + (highLightShade.b - targetTextMeshPro[i].color.b) * alpha;
                Color cf = new Color(newR, newG, newB, 1f);
                colorsFinalTMP.Add(cf);
            }
            for (int i = 0; i < targetText.Length; i++)
            {
                colorsText.Add(targetText[i].color);
                float alpha = highLightShade.a;
                float newR = targetText[i].color.r + (highLightShade.r - targetText[i].color.r) * alpha;
                float newG = targetText[i].color.g + (highLightShade.g - targetText[i].color.g) * alpha;
                float newB = targetText[i].color.b + (highLightShade.b - targetText[i].color.b) * alpha;
                Color cf = new Color(newR, newG, newB, 1f);
                colorsFinalText.Add(cf);
            }
        }
        else if (pointerExit)
        {
            pointerExit = false;
            transition1 = -1;
            transition2 = 0;
        }
        if (transition1 < fadeTime && transition1 >= 0)
        {
            //Debug.Log("Highlight Together-> Highlighting!");
            transition1 += finalDeltaTime;
            HighlightText();
            HighlightImage();
        }
        else if (transition1 >= fadeTime)
        {
            FinishHighlightText();
            FinishHighlightImage();
            transition1 = -1;
        }

        if (transition2 < fadeTime && transition2 >= 0)
        {
            //Debug.Log("transition2 = " + transition2 + "; fadeTime = " + fadeTime);
            transition2 += finalDeltaTime;
            StopHighlightText();
            StopHighlightImage();
        }
        else if (transition2 >= fadeTime)
        {
            FinishStopHighlightText();
            FinishStopHighlightImage();
            transition2 = -1;
        }

    }


    public void ActivateHighlight()
    {
        if (!pointerDown && boton.interactable)
        {
            pointerDown = true;
        }
    }

    public void DeactivateHighlight()
    {
        //Debug.Log("DeactivateHighlight -> pointerExit = " + pointerExit);
        if (!pointerExit && boton.interactable)
        {
            pointerExit = true;
        }
    }

    void HighlightText()
    {
        for (int i = 0; i < targetText.Length && i < colorsFinalText.Count; i++)
        {
            float progress = transition1 / fadeTime;
            float r = EasingFunction.Linear(colorsText[i].r, colorsFinalText[i].r, progress);
            float g = EasingFunction.Linear(colorsText[i].g, colorsFinalText[i].g, progress);
            float b = EasingFunction.Linear(colorsText[i].b, colorsFinalText[i].b, progress);
            float a = EasingFunction.Linear(colorsText[i].a, colorsFinalText[i].a, progress);
            Color lerpedColor = new Color(r, g, b, a);
            targetText[i].color = lerpedColor;
        }
        for (int i = 0; i < targetTextMeshPro.Length && i < colorsFinalTMP.Count; i++)
        {
            float progress = transition1 / fadeTime;
            float r = EasingFunction.Linear(colorsTMP[i].r, colorsFinalTMP[i].r, progress);
            float g = EasingFunction.Linear(colorsTMP[i].g, colorsFinalTMP[i].g, progress);
            float b = EasingFunction.Linear(colorsTMP[i].b, colorsFinalTMP[i].b, progress);
            float a = EasingFunction.Linear(colorsTMP[i].a, colorsFinalTMP[i].a, progress);
            Color lerpedColor = new Color(r, g, b, a);
            targetTextMeshPro[i].color = lerpedColor;
        }
    }
    void FinishHighlightText()
    {
        for (int i = 0; i < targetText.Length && i < colorsFinalText.Count; i++)
        {
            targetText[i].color = colorsFinalText[i];
        }
        for (int i = 0; i < targetTextMeshPro.Length && i < colorsFinalTMP.Count; i++)
        {
            targetTextMeshPro[i].color = colorsFinalTMP[i];
        }
    }
    void StopHighlightText()
    {
        for (int i = 0; i < targetText.Length && i < colorsText.Count; i++)
        {
            float progress = transition2 / fadeTime;
            float r = EasingFunction.Linear(targetText[i].color.r, colorsText[i].r, progress);
            float g = EasingFunction.Linear(targetText[i].color.g, colorsText[i].g, progress);
            float b = EasingFunction.Linear(targetText[i].color.b, colorsText[i].b, progress);
            float a = EasingFunction.Linear(targetText[i].color.a, colorsText[i].a, progress);
            Color lerpedColor = new Color(r, g, b, a);
            targetText[i].color = lerpedColor;
        }
        for (int i = 0; i < targetTextMeshPro.Length && i < colorsTMP.Count; i++)
        {
            float progress = transition2 / fadeTime;
            float r = EasingFunction.Linear(targetTextMeshPro[i].color.r, colorsTMP[i].r, progress);
            float g = EasingFunction.Linear(targetTextMeshPro[i].color.g, colorsTMP[i].g, progress);
            float b = EasingFunction.Linear(targetTextMeshPro[i].color.b, colorsTMP[i].b, progress);
            float a = EasingFunction.Linear(targetTextMeshPro[i].color.a, colorsTMP[i].a, progress);
            Color lerpedColor = new Color(r, g, b, a);
            targetTextMeshPro[i].color = lerpedColor;
        }
    }
    void FinishStopHighlightText()
    {
        for (int i = 0; i < targetText.Length && i < colorsText.Count; i++)
        {
            targetText[i].color = colorsText[i];
        }
        for (int i = 0; i < targetTextMeshPro.Length && i < colorsTMP.Count; i++)
        {
            targetTextMeshPro[i].color = colorsTMP[i];
        }
    }

    void HighlightImage()
    {
        {
            for (int i = 0; i < targetImages.Count && i < colorsImages.Count && i < colorsFinalImages.Count; i++)
            {
                float progress = transition1 / fadeTime;
                float r = EasingFunction.Linear(colorsImages[i].r, colorsFinalImages[i].r, progress);
                float g = EasingFunction.Linear(colorsImages[i].g, colorsFinalImages[i].g, progress);
                float b = EasingFunction.Linear(colorsImages[i].b, colorsFinalImages[i].b, progress);
                float a = EasingFunction.Linear(colorsImages[i].a, colorsFinalImages[i].a, progress);
                Color lerpedColor = new Color(r, g, b, a);
                targetImages[i].color = lerpedColor;
                //Debug.Log("Highlighting Image " + targetImages[i] + "; color = " + lerpedColor);
                //Debug.Log("LerpedColor = " + lerpedColor);
            }
        }
    }
    void FinishHighlightImage()
    {
        {
            for (int i = 0; i < targetImages.Count && i < colorsFinalImages.Count; i++)
            {
                targetImages[i].color = colorsFinalImages[i];
            }
        }
    }
    void StopHighlightImage()
    {
        for (int i = 0; i < targetImages.Count && i < colorsImages.Count; i++)
        {
            float progress = transition2 / fadeTime;
            float r = EasingFunction.Linear(targetImages[i].color.r, colorsImages[i].r, progress);
            float g = EasingFunction.Linear(targetImages[i].color.g, colorsImages[i].g, progress);
            float b = EasingFunction.Linear(targetImages[i].color.b, colorsImages[i].b, progress);
            float a = EasingFunction.Linear(targetImages[i].color.a, colorsImages[i].a, progress);
            Color lerpedColor = new Color(r, g, b, a);
            targetImages[i].color = lerpedColor;
            targetImages[i].color = lerpedColor;
        }
    }
    void FinishStopHighlightImage()
    {
        for (int i = 0; i < targetImages.Count && i < colorsImages.Count; i++)
        {
            targetImages[i].color = colorsImages[i];
        }
    }
}
