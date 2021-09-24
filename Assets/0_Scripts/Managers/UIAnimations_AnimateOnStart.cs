using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAnimations_AnimateOnStart : MonoBehaviour
{
    public UIAnimation uiAnimation;
    private void Start()
    {
        UIAnimationsManager.instance.StartAnimation(uiAnimation);
    }
    private void Update()
    {
        if (!UIAnimationsManager.instance.IsPlaying(uiAnimation) && gameObject != null) Destroy(gameObject);
    }
}
