#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class AutoAnchorsToCorners : MonoBehaviour
{
    public bool recursive = false;

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnScene;
    }
    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnScene;
    }

    private void LateUpdate()
    {
        if (!Application.isPlaying)
        {
            if (Input.GetMouseButtonUp(0))
            {
                if (recursive)
                {
                    AnchorsToCornersRecursive();
                }
                else
                {
                    AnchorsToCorners();
                }
            }
        }

    }

    void OnScene(SceneView scene)
    {
        Event e = Event.current;
        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        switch (e.GetTypeForControl(controlID))
        {
            case EventType.MouseUp:
                GUIUtility.hotControl = 0;
                e.Use();
                if (!Application.isPlaying)
                {
                        if (recursive)
                        {
                            AnchorsToCornersRecursive();
                        }
                        else
                        {
                            AnchorsToCorners();
                        }
                }
                break;
        }
    }

    void AnchorsToCorners()
    {

        RectTransform t = transform.GetComponent<RectTransform>();
        RectTransform pt = transform.parent.GetComponent<RectTransform>();

        if (t == null || pt == null) return;

        Vector2 newAnchorsMin = new Vector2(t.anchorMin.x + t.offsetMin.x / pt.rect.width,
                                            t.anchorMin.y + t.offsetMin.y / pt.rect.height);
        Vector2 newAnchorsMax = new Vector2(t.anchorMax.x + t.offsetMax.x / pt.rect.width,
                                            t.anchorMax.y + t.offsetMax.y / pt.rect.height);

        t.anchorMin = newAnchorsMin;
        t.anchorMax = newAnchorsMax;
        t.offsetMin = t.offsetMax = new Vector2(0, 0);
    }

    void AnchorsToCornersRecursive()
    {
        RectTransform t = transform.GetComponent<RectTransform>();
        RectTransform pt = transform.parent.GetComponent<RectTransform>();

        if (t == null || pt == null) return;

        Vector2 newAnchorsMin = new Vector2(t.anchorMin.x + t.offsetMin.x / pt.rect.width,
                                            t.anchorMin.y + t.offsetMin.y / pt.rect.height);
        Vector2 newAnchorsMax = new Vector2(t.anchorMax.x + t.offsetMax.x / pt.rect.width,
                                            t.anchorMax.y + t.offsetMax.y / pt.rect.height);

        t.anchorMin = newAnchorsMin;
        t.anchorMax = newAnchorsMax;
        t.offsetMin = t.offsetMax = new Vector2(0, 0);

        if (t.childCount > 0)
        {
            for (int i = 0; i < t.childCount; i++)
            {
                AnchorsToCornersRecursive(t.GetChild(i));
            }
        }
    }

    void AnchorsToCornersRecursive(Transform rectT)
    {
        RectTransform t = rectT.GetComponent<RectTransform>();
        RectTransform pt = rectT.parent.GetComponent<RectTransform>();

        if (t == null || pt == null) return;

        Vector2 newAnchorsMin = new Vector2(t.anchorMin.x + t.offsetMin.x / pt.rect.width,
                                            t.anchorMin.y + t.offsetMin.y / pt.rect.height);
        Vector2 newAnchorsMax = new Vector2(t.anchorMax.x + t.offsetMax.x / pt.rect.width,
                                            t.anchorMax.y + t.offsetMax.y / pt.rect.height);

        t.anchorMin = newAnchorsMin;
        t.anchorMax = newAnchorsMax;
        t.offsetMin = t.offsetMax = new Vector2(0, 0);

        if (t.childCount > 0)
        {
            for (int i = 0; i < t.childCount; i++)
            {
                AnchorsToCornersRecursive(t.GetChild(i));
            }
        }
    }
}
#endif
