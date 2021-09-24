/* Author: Carlos Eloy Jose Sanz
 * https://www.linkedin.com/in/celoy-jose-sanz-505586156/
 * 
 * The MIT License (MIT)
 * 
 * Copyright (c) 2019
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * 
 * */
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using EloyExtensions;


public enum UIAnimType
{
    none,
    horizontalShake,
    color_alpha,
    scale,
    movement,
    TutorialFinger,
    shake,
    rotation
}

public enum RectCorner
{
    None,
    BottomLeft,
    TopLeft,
    TopRight,
    BottomRight,
    Length
}

public class UIAnimationsManager : MonoBehaviour
{
    public static UIAnimationsManager instance;
    public bool debug = false;
    public List<UIAnimation> uIAnimations;
    public List<string> animationsDone;

    public GameObject tutorialHandPrefab;
    public GameObject blackVeilPrefab;
    public float blackVeilZ;
    float lastRealTime = 0;
    float currentTime = 0;

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene current, Scene next)
    {
        if(debug)Debug.LogWarning("UIAnimations: CHANGING SCENE!");
        for (int i = 0; i < uIAnimations.Count; i++)
        {
            if (uIAnimations[i].stopOnSceneChange) StopUIAnimation(uIAnimations[i]);
        }
    }

    public void Awake()
    {
        if (UIAnimationsManager.instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }

        //Application.targetFrameRate = 60;
        DontDestroyOnLoad(this);
        instance = this;

        uIAnimations = new List<UIAnimation>();
        animationsDone = new List<string>();
    }

    private void Update()
    {
        currentTime = (Time.realtimeSinceStartup - lastRealTime);
        ProcessUIAnimations();

        lastRealTime = Time.realtimeSinceStartup;
    }

    #region UIAnimations

    public void StartAnimation(UIAnimation uIAnimation, bool playBack = false, Camera canvasCamera = null)
    {
        if (canvasCamera == null) canvasCamera = Camera.main;
        uIAnimation.CopySettings();

        for (int i = 0; i < uIAnimations.Count; i++)
        {
            if (uIAnimations[i].transf == uIAnimation.transf && uIAnimations[i] != uIAnimation && !uIAnimation.forcePlay)
            {
                Debug.LogError("UIAnimation Error: you are trying to animate the same RectTransform (" + uIAnimation.transf.name + ")at the same time with " +
                    "more than 1 animation(" + uIAnimation.animName + "). Are you sure this is correct?");
                return;
            }
        }
        if (!uIAnimations.Contains(uIAnimation))
        {
            if (canvasCamera != null)
            {
                float prop = Mathf.Min(canvasCamera.rect.width, canvasCamera.rect.height);
                uIAnimation.currentXAmplitude = uIAnimation.xAmplitude * prop;
            }
            uIAnimations.Add(uIAnimation);
            uIAnimation.StartAnimation(playBack);
        }
        else
        {
            if (!IsPlaying(uIAnimation))
            {
                Debug.LogError("The animation " + uIAnimation.animName + " is in the animations array but is not playing, this should not happen!");
            }
            uIAnimation.RestartAnimation(playBack);
        }
    }

    void ProcessUIAnimations()
    {
        for (int i = 0; i < uIAnimations.Count; i++)
        {
            uIAnimations[i].ProcessAnimation(currentTime);
        }
    }

    public void StopUIAnimation(UIAnimation uIAnimation)
    {
        for (int i = 0; i < uIAnimations.Count; i++)
        {
            if (uIAnimations[i] == uIAnimation)
            {
                if(debug)Debug.Log("STOPPING animation = " + uIAnimations[i].animName + "; i = " + i);
                uIAnimations[i].StopAnimation();
                uIAnimations.RemoveAt(i);
            }
        }
    }

    public void StopAllUIAnimations()
    {
        while (uIAnimations.Count > 0)
        {
            uIAnimations[0].StopAnimation();
            uIAnimations.RemoveAt(0);
        }

    }

    public bool IsPlaying(UIAnimation uIAnimation)
    {
        for (int i = 0; i < uIAnimations.Count; i++)
        {
            if (uIAnimation == uIAnimations[i] && uIAnimations[i].playing) return true;
        }
        return false;
    }

    public bool IsDone(UIAnimation anim)
    {
        return animationsDone.Contains(anim.animName);
    }
    #endregion
}

[System.Serializable]
public class UIAnimation
{
    public string animName;
    public UIAnimationBaseSettingData baseSettings;
    public UIAnimType type = UIAnimType.none;
    [HideInInspector] public bool playing = false;
    public Transform transf;
    [Tooltip("If set to true, it ignores that other UIAnimation is already affecting this RectTransform.")]
    public bool forcePlay = false;
    public float duration = 0.5f;
    [Tooltip("If endless, duration is ignored.")]
    public bool endless = false;
    [Tooltip("If true, the animation will be cycled back when finished, and repeat until duration ends.")]
    public bool cycleAnimDir = true;
    public float frequency = 0.06f;
    [Range(0, 1)]
    public float cycleStartPoint = 0.5f;
    int animDir;//1 going right; -1 going left
    public Ease easeFunction = Ease.None;
    [HideInInspector] public bool playBack = false;
    public bool ignoreTimeScale = false;
    public bool stopOnSceneChange = true;
    public bool forceScaleTo1If0 = false;

    [Header("--- HORIZONTAL SHAKE ---")]
    public float xAmplitude = 7f;
    [HideInInspector] public float currentXAmplitude;
    float currentDuration, currentCycleTime, totalSpace;
    Vector3 originalLocalPos;

    [Header("--- SHAKE ---")]
    [Range(0, 10)]
    public float initialIntensity;
    [Range(0, 10)]
    public float finalIntensity;
    float jumpRadius = 0;
    float jumpFrequency = 0.05f;
    float jumpCurrentTime = 0;
    Vector2 lastJump;

    [Header("--- COLOR_ALPHA ---")]
    [Range(0, 1)]
    public float alphaMin = 0;
    [Range(0, 1)]
    public float alphaMax = 1;
    public bool recursive = false;
    List<ColorContainer> childrenColors;
    //public bool propagate = false;

    [Header("--- SCALE ---")]
    public float initialScale;
    public float finalScale;
    private float realInitialScale, realFinalScale;
    [Tooltip("If set to 0,0 it will take the localScale at the beggining of the animation.")]
    public Vector2 originalScale = Vector2.zero;
    private Vector2 finalOriginalScale;

    [Header("--- MOVEMENT ---")]
    [Tooltip("Automatically saves the initial pos as the current local position in the canvas, does not work on play back.")]
    public bool automaticInitialPos;
    [Tooltip("Use the positions as an increment of the original local position, instead of a preset local position")]
    public bool useAsIncrement = false;

    public Vector2 initialPos;
    public Vector2 finalPos;
    private Vector2 originalPos;
    bool originalPosSet = false;

    private Vector2 realInitialPos;
    private Vector2 realFinalPos;

    [Header("--- ROTATION ---")]
    public Vector3 initialRot;
    public Vector3 finalRot;
    private Vector3 originalRot;
    private Vector2 realInitialRot;
    private Vector2 realFinalRot;

    public bool alwaysRotateInSameDirection = false;
    bool originalRotSet = false;

    [Header("--- TUTORIAL HAND ---")]
    [Tooltip("Number of times the button has to be pressed to stop the animation. If this value is negative, it means it won't stop by itself and needs external help to stop!")]
    public int buttonPressesNeeded = 1;
    public bool useBlackVeil = true;
    private RectTransform blackVeil;
    private RectTransform tutorialHand;
    private Button targetButton;
    private ButtonExtraInfo buttonExtraInfo;
    public RectCorner handPosition = RectCorner.BottomRight;
    private Transform originalParent;
    private Transform movementRect;
    private GameObject buttonCopy;
    private int childIndex = 0;
    public List<Child> children;
    [System.Serializable]
    public class Child
    {
        public Transform transf;
        public Vector3 originalLocalPos;
        public Transform originalParent;
        public int originalSiblingIndex;
        public int depth = 0;

        public Child(Transform _transf, int _depth)
        {
            transf = _transf;
            originalLocalPos = transf.localPosition;
            originalParent = transf.parent;
            originalSiblingIndex = transf.GetSiblingIndex();
            depth = _depth;
        }

        public void ResetData()
        {
            transf.SetParent(originalParent);
            transf.localPosition = originalLocalPos;
            transf.SetSiblingIndex(originalSiblingIndex);
        }
    }

    public UIAnimation(UIAnimType _type, ref Transform _transf, float _xAmplitude = 7f, float _frequency = 0.06f,
        float _duration = 0.5f, float _cycleStartPoint = 0.5f)
    {
        type = _type;
        transf = _transf;
        xAmplitude = _xAmplitude;
        currentXAmplitude = xAmplitude;
        frequency = _frequency;//cycle max time
        duration = _duration;
        _cycleStartPoint = Mathf.Clamp01(_cycleStartPoint);
        cycleStartPoint = _cycleStartPoint;
        //StartAnimation(); // Es necesario???
    }

    public void StartAnimation(bool _playBack = false)
    {
        if (!playing)
        {
            playing = true;
            playBack = _playBack;
            if (animName == "")
            {
                //Debug.LogWarning("The animation is supposed to have a name to identify it later");
                CreateAutomaticName();
            }
            if(UIAnimationsManager.instance.debug) Debug.Log("Start Animation " + animName + "; Rect = " + transf.name + "; Playback = " + playBack);

            if (UIAnimationsManager.instance.animationsDone.Contains(animName))
            {
                UIAnimationsManager.instance.animationsDone.Remove(animName);
            }

            if (!transf.gameObject.activeInHierarchy) transf.gameObject.SetActive(true);
            if (transf.localScale.x == 0 && transf.localScale.y == 0)
            {
                if (forceScaleTo1If0)
                {
                    transf.localScale = Vector3.one;Debug.LogError("escala a 1");
                }
                else
                {
                    if(UIAnimationsManager.instance.debug)Debug.LogWarning("UIAnimations: Trying to start the animation " + animName + " but the transform " + transf.name + " has scale 0, is this intended or an error?");
                }
            }

            if (frequency == 0 && duration > 0) frequency = duration;
            currentDuration = 0;
            currentCycleTime = cycleStartPoint * frequency;
            animDir = 1;
            switch (type)
            {
                case UIAnimType.horizontalShake:
                    totalSpace = currentXAmplitude * 2;
                    originalLocalPos = transf.localPosition;
                    //Debug.LogWarning(" currentxAmplitude= " + currentxAmplitude + "; totalSpace = " + totalSpace);
                    break;
                case UIAnimType.shake:
                    originalLocalPos = transf.localPosition;
                    jumpCurrentTime = 0;
                    lastJump = originalLocalPos;
                    break;
                case UIAnimType.color_alpha:
                    CreateColorContainers();
                    break;
                case UIAnimType.scale:
                    
                    //totalScaleAmplitude = Mathf.Abs(initialScale - finalScale);
                    if (playBack)
                    {
                        realInitialScale = finalScale;
                        realFinalScale = initialScale;
                    }
                    else
                    {
                        realInitialScale = initialScale;
                        realFinalScale = finalScale;
                    }
                    
                    if (finalOriginalScale== Vector2.zero)finalOriginalScale = originalScale == Vector2.zero ? new Vector2(transf.localScale.x, transf.localScale.y) : originalScale;
                    if (finalOriginalScale.x == 0) finalOriginalScale.x = 1;
                    if (finalOriginalScale.y == 0) finalOriginalScale.y = 1;
                   
                    //Debug.LogError(transf.localScale + " " + realFinalScale);

                    if (transf.localScale.x == realFinalScale && transf.localScale.y == realFinalScale)
                    {
                        //Debug.LogError("test1");
                        break;
                    }
                    else
                    {
                        //Debug.LogError("test2");
                        transf.localScale = new Vector2(finalOriginalScale.x * realInitialScale, finalOriginalScale.y * realInitialScale);

                        //Debug.LogWarning("SCALEANIM: finalOriginalScale= " + finalOriginalScale);
                        break;
                    }
                   
                case UIAnimType.movement:
                    StartMovementAnimation(playBack);
                    //Debug.LogWarning("MOVEANIM: originalPos= " + originalPos);
                    break;
                case UIAnimType.rotation:
                    if (!originalRotSet)
                    {
                        originalRotSet = true;
                        originalRot = useAsIncrement ? (Vector2)transf.localRotation.eulerAngles : (Vector2)transf.rotation.eulerAngles;
                    }
                    //totalAmplitude = new Vector2(Mathf.Abs(initialPos.x - finalPos.x), Mathf.Abs(initialPos.y - finalPos.y));

                    realInitialRot = useAsIncrement ? originalRot + initialRot : initialRot;
                    realFinalRot = useAsIncrement ? originalRot + finalRot : finalRot;

                    transf.localRotation = Quaternion.Euler(realInitialRot);
                    break;
                case UIAnimType.TutorialFinger:
                    SaveChildrenData(transf);
                    originalLocalPos = transf.localPosition;
                    buttonCopy = GameObject.Instantiate(transf.gameObject, transf.parent);
                    childIndex = transf.GetSiblingIndex();
                    buttonCopy.transform.SetSiblingIndex(childIndex);
                    UnityExtensions.SetSpritesAlpha_r(buttonCopy.transform, 0);
                    Transform canvasParent = transf.GetComponentInParent<Canvas>().transform;
                    blackVeil = GameObject.Instantiate(UIAnimationsManager.instance.blackVeilPrefab, Vector3.zero, Quaternion.identity, canvasParent).GetComponent<RectTransform>();
                    blackVeil.SetAsLastSibling();
                    blackVeil.localPosition = Vector3.zero;
                    targetButton = transf.GetComponentInChildren<Button>();
                    buttonExtraInfo = targetButton.GetComponent<ButtonExtraInfo>();
                    if (buttonExtraInfo == null) buttonExtraInfo = targetButton.gameObject.AddComponent<ButtonExtraInfo>();
                    originalParent = transf.parent;
                    transf.SetParent(blackVeil);
                    tutorialHand = GameObject.Instantiate(UIAnimationsManager.instance.tutorialHandPrefab, transf).GetComponent<RectTransform>();
                    tutorialHand.SetAsLastSibling();
                    Vector3[] localCorners = new Vector3[4];
                    if (transf.GetComponent<RectTransform>() != null)
                    {
                        transf.GetComponent<RectTransform>().GetLocalCorners(localCorners);
                        switch (handPosition)
                        {
                            case RectCorner.BottomLeft:
                                tutorialHand.localPosition = localCorners[0];
                                tutorialHand.localRotation = Quaternion.Euler(0, 0, 270);
                                break;
                            case RectCorner.TopLeft:
                                tutorialHand.localPosition = localCorners[1];
                                tutorialHand.localRotation = Quaternion.Euler(0, 0, 180);
                                break;
                            case RectCorner.TopRight:
                                tutorialHand.localPosition = localCorners[2];
                                tutorialHand.localRotation = Quaternion.Euler(0, 0, 90);
                                break;
                            case RectCorner.BottomRight:
                                tutorialHand.localPosition = localCorners[3];
                                tutorialHand.localRotation = Quaternion.Euler(0, 0, 0);
                                break;
                        }
                    }
                    else if (transf.GetComponent<SpriteRenderer>() != null)
                    {
                        Debug.LogError("TutorialFinger is not implemented for SpriteRenderers yet!");
                        UIAnimationsManager.instance.StopUIAnimation(this);
                        return;
                        //tutorialHand.localPosition = new Vector3(transf.GetComponent<SpriteRenderer>().bounds.max.x, transf.GetComponent<SpriteRenderer>().bounds.min.y, tutorialHand.localPosition.z);
                    }
                    else
                    {
                        Debug.LogError("Can't find any rect or sprite renderer on the transform " + transf.name);
                        tutorialHand.localPosition = Vector3.zero;
                    }
                    blackVeil.GetComponent<Image>().enabled = useBlackVeil;

                    StartMovementAnimation(playBack);
                    break;
            }
        }
    }

    public void RestartAnimation(bool _playBack = false)
    {
        if (playing)
        {
            if (frequency == 0 && duration > 0) frequency = duration;
            currentDuration = 0;
            currentCycleTime = cycleStartPoint * frequency;
            animDir = 1;
            playBack = _playBack;
            if(UIAnimationsManager.instance.debug)Debug.Log("Restarting Animation " + animName + "; Rect = " + transf.name + "; frequency = " + frequency);
            switch (type)
            {
                case UIAnimType.horizontalShake:
                    totalSpace = currentXAmplitude * 2;
                    originalLocalPos = transf.localPosition;
                    //Debug.LogWarning(" currentxAmplitude= " + currentxAmplitude + "; totalSpace = " + totalSpace);
                    break;
                case UIAnimType.shake:
                    transf.localPosition = originalLocalPos;
                    jumpCurrentTime = 0;
                    lastJump = originalLocalPos;
                    break;
                case UIAnimType.color_alpha:
                    SetChildrenColorsAlphaValue(playBack ? alphaMax : alphaMin);
                    break;
                case UIAnimType.scale:
                    if (playBack)
                    {
                        realInitialScale = finalScale;
                        realFinalScale = initialScale;
                    }
                    else
                    {
                        realInitialScale = initialScale;
                        realFinalScale = finalScale;
                    }
                    transf.localScale = new Vector2(finalOriginalScale.x * realInitialScale, finalOriginalScale.y * realInitialScale);

                    break;
                case UIAnimType.movement:
                    StartMovementAnimation(playBack);
                    Debug.LogWarning("MOVEANIM: originalPos= " + originalPos);
                    break;
                case UIAnimType.rotation:
                    transf.localRotation = Quaternion.Euler(realInitialRot);
                    break;
                case UIAnimType.TutorialFinger:
                    break;
            }
        }
    }

    public void ProcessAnimation(float time)
    {        
        //Debug.Log("ProcessAnimation: playing = " + playing);
        if (playing)
        {
            if (transf == null)
            {
                UIAnimationsManager.instance.StopUIAnimation(this);
                return;
            }
            float progress = Mathf.Clamp01(currentCycleTime / frequency);
            switch (type)
            {
                case UIAnimType.horizontalShake:
                    float xIncrement = progress * totalSpace * animDir;
                    //float xIncrement = animDir * EasingFunction.EaseInOutQuart(0, xAmplitude, progress);
                    float originX = originalLocalPos.x + (currentXAmplitude * -animDir);
                    Vector3 finalPosition = originalLocalPos;
                    finalPosition.x = originX + xIncrement;
                    transf.localPosition = finalPosition;
                    //Debug.Log("xIncrement = "+ xIncrement + "; totalSpace = "+ totalSpace + "; progress = " + progress+ "; originX = " + originX+ "; finalPos.x ="+ finalPos.x);
                    break;
                case UIAnimType.shake:
                    float realAnimDir = playBack ? -animDir : animDir;
                    progress = realAnimDir == 1 ? progress : 1 - progress;
                    float currentIntensity = EasingFunction.SelectEasingFunction(easeFunction, initialIntensity, finalIntensity, progress);
                    jumpRadius = currentIntensity * 20;
                    if (jumpCurrentTime >= jumpFrequency)
                    {
                        jumpCurrentTime = 0;
                        float newX = Random.Range(transf.localPosition.x - jumpRadius, transf.localPosition.x + jumpRadius);
                        float newY = Random.Range(transf.localPosition.y - jumpRadius, transf.localPosition.y + jumpRadius);

                        if (lastJump.x < originalLocalPos.x && newX < originalLocalPos.x) newX = Random.Range(transf.localPosition.x, transf.localPosition.x + jumpRadius);
                        else if (lastJump.x >= originalLocalPos.x && newX >= originalLocalPos.x) newX = Random.Range(transf.localPosition.x - jumpRadius, transf.localPosition.x);

                        if (lastJump.y < originalLocalPos.y && newY < originalLocalPos.y) newY = Random.Range(transf.localPosition.y, transf.localPosition.y + jumpRadius);
                        else if (lastJump.y >= originalLocalPos.y && newY >= originalLocalPos.y) newY = Random.Range(transf.localPosition.y - jumpRadius, transf.localPosition.y);

                        transf.localPosition = new Vector3(newX, newY, transf.localPosition.z);
                        lastJump = transf.localPosition;
                    }
                    break;
                case UIAnimType.color_alpha:
                    realAnimDir = playBack ? -animDir : animDir;
                    progress = realAnimDir == 1 ? progress : 1 - progress;
                    float value = EasingFunction.SelectEasingFunction(easeFunction, alphaMin, alphaMax, progress);
                    //Debug.Log(" animName = "+ animName+ "; currentDuration = " + currentDuration+ "; duration = " + duration +
                    //"; currentCycleTime = " + currentCycleTime + "; frequency = "+ frequency + "; value = " + value + 
                    //   "; progress = " + progress + "; realAnimDir = " + realAnimDir + "; playBack = " + playBack);
                    SetChildrenColorsAlphaValue(value);

                    //Debug.Log("Progress = " + progress + "; value = " + value);
                    break;
                case UIAnimType.scale:
                    //Debug.LogError (transf.localScale.x +" "+ realFinalScale + " " + transf.localScale.y + " " + realFinalScale) ;
                    if (transf.localScale.x == realFinalScale && transf.localScale.y == realFinalScale)
                    {
                        break;
                    }
                    else
                    {
                        realAnimDir = playBack ? -animDir : animDir;
                        progress = realAnimDir == 1 ? progress : 1 - progress;
                        value = EasingFunction.SelectEasingFunction(easeFunction, initialScale, finalScale, progress);
                        //Debug.Log("Animation: " + animName + "; SCALENANIM: Progress = " + progress + "; value = " + value + "; originalScale.x = " + originalScale.x + "; originalScale.y = " + originalScale.y);

                        transf.localScale = new Vector3(finalOriginalScale.x * value, finalOriginalScale.y * value, 1);
                        break;
                    }
                        
                case UIAnimType.movement:
                    ProcessMovementAnimation(progress);
                    break;
                case UIAnimType.rotation:
                    realAnimDir = playBack ? -animDir : animDir;
                    progress = realAnimDir == 1 ? progress : 1 - progress;
                    float xVal = EasingFunction.SelectEasingFunction(easeFunction, realInitialRot.x, realFinalRot.x, progress);
                    float yVal = EasingFunction.SelectEasingFunction(easeFunction, realInitialRot.y, realFinalRot.y, progress);
                    float zVal = EasingFunction.SelectEasingFunction(easeFunction, realInitialRot.y, realFinalRot.y, progress);


                    transf.localRotation = Quaternion.Euler(new Vector3(xVal, yVal, zVal));
                    //Debug.Log("MOVEANIM: animName = "+animName+ "; movementRect = "+ movementRect.name+ "; Progress = " + progress + "; realAnimDir = " + realAnimDir +
                    //    "; animDir = " + animDir + "; xVal = " + xVal + "; yVal = " + yVal);
                    break;
                case UIAnimType.TutorialFinger:
                    /*if (GeneralPauseScript.pause.doingWorkshopTutorial && TzukisWorkshopScript.instance.state == TzukisWorkshopState.Building)
                        Debug.Log("buttonPressesNeeded = " + buttonPressesNeeded + "; buttonExtraInfo.buttonUpTimes = " + buttonExtraInfo.buttonUpTimes);*/
                    if (buttonPressesNeeded > 0)//If button presses needed < 0, don't stop animation by itself. Can only be stopped with a UIAnimationsManager.instance.StopAnimation() !
                    {
                        if (buttonExtraInfo.buttonUpTimes >= buttonPressesNeeded)
                        {
                            UIAnimationsManager.instance.StopUIAnimation(this);
                            return;
                        }
                        else
                        {
                            //Animate hand
                            ProcessMovementAnimation(progress);
                        }
                    }
                    break;
            }
            //CHANGE ANIM DIR
            float finalTime = 0;
            if (ignoreTimeScale) finalTime = time;
            else finalTime = Time.deltaTime;
            currentCycleTime += finalTime;

            if (type == UIAnimType.shake)
                jumpCurrentTime += finalTime;

            if (cycleAnimDir)
            {
                if (currentCycleTime >= frequency)
                {
                    if(type == UIAnimType.rotation && alwaysRotateInSameDirection)
                    {
                        currentCycleTime = 0;
                    }
                    else
                    {
                        animDir = animDir == 1 ? -1 : 1;
                        currentCycleTime = 0;
                    }
                }
            }
            //Debug.Log("UIANIMATION: cycleAnimDir = "+ cycleAnimDir + "; currentCycleTime = "+ currentCycleTime + "; frequency  = " + frequency);
            //END
            if (!endless && currentDuration >= duration)
            {
                UIAnimationsManager.instance.StopUIAnimation(this);
                return;
            }
            currentDuration += finalTime;
        }
    }

    public void StopAnimation()
    {
        if (playing)
        {
            if(UIAnimationsManager.instance.debug)Debug.Log("STOP UI ANIMATION " + animName);
            playing = false;
            if (currentDuration >= duration)
            {
                currentDuration = duration;
            }

            if (!UIAnimationsManager.instance.animationsDone.Contains(animName))
            {
                UIAnimationsManager.instance.animationsDone.Add(animName);
            }

            if (transf == null) return;

            switch (type)
            {
                case UIAnimType.horizontalShake:
                    transf.localPosition = originalLocalPos;
                    break;
                case UIAnimType.shake:
                    transf.localPosition = originalLocalPos;
                    break;
                case UIAnimType.color_alpha:
                    SetChildrenColorsAlphaValue(playBack ? alphaMin : alphaMax);
                    break;
                case UIAnimType.scale:
                    transf.localScale = new Vector3(finalOriginalScale.x * realFinalScale, finalOriginalScale.y * realFinalScale, 1);
                    //Debug.Log("Final Scale = ("+(originalScale.x * realFinalScale) + ", " + (originalScale.y * realFinalScale)+")");
                    break;
                case UIAnimType.movement:
                    StopMovementAnimation();
                    break;
                case UIAnimType.rotation:
                    transf.localRotation =Quaternion.Euler(!playBack ? realFinalRot : realInitialRot);
                    break;
                case UIAnimType.TutorialFinger:
                    GameObject.Destroy(tutorialHand.gameObject);
                    GameObject.Destroy(buttonCopy);
                    transf.SetParent(originalParent);
                    transf.transform.SetSiblingIndex(childIndex);
                    transf.transform.localPosition = originalLocalPos;
                    ResetChildrenData();
                    GameObject.Destroy(blackVeil.gameObject);
                    break;
            }
        }
    }

    #region --- Alpha Animation ---
    public void CreateColorContainers()
    {
        childrenColors = new List<ColorContainer>();
        CreateColorContainer_r(transf, recursive);
    }

    void CreateColorContainer_r(Transform transf, bool recursive)
    {
        childrenColors.Add(new ColorContainer(transf));
        if (recursive)
        {
            for (int i = 0; i < transf.childCount; i++)
            {
                CreateColorContainer_r(transf.GetChild(i), recursive);
            }
        }
    }

    public void SetChildrenColorsAlphaValue(float value)
    {
        for (int i = 0; i < childrenColors.Count; i++)
        {
            childrenColors[i].ChangeAlphaValue(value);
        }
    }
    #endregion

    #region --- Movement Animation ---
    void StartMovementAnimation(bool playBack)
    {
        movementRect = type == UIAnimType.TutorialFinger ? tutorialHand : transf;

        if (!originalPosSet)
        {
            originalPosSet = true;
            originalPos = useAsIncrement ? (Vector2)movementRect.localPosition : (Vector2)movementRect.position;
        }
        //totalAmplitude = new Vector2(Mathf.Abs(initialPos.x - finalPos.x), Mathf.Abs(initialPos.y - finalPos.y));


        if (automaticInitialPos && initialPos == Vector2.zero)
        {
            initialPos = movementRect.localPosition;
        }
        realInitialPos = useAsIncrement ? originalPos + initialPos : initialPos;
        realFinalPos = useAsIncrement ? originalPos + finalPos : finalPos;

        movementRect.localPosition = realInitialPos;
    }

    void ProcessMovementAnimation(float progress)
    {
        float realAnimDir = playBack ? -animDir : animDir;
        progress = realAnimDir == 1 ? progress : 1 - progress;
        float xVal = EasingFunction.SelectEasingFunction(easeFunction, realInitialPos.x, realFinalPos.x, progress);
        float yVal = EasingFunction.SelectEasingFunction(easeFunction, realInitialPos.y, realFinalPos.y, progress);

        movementRect.localPosition = new Vector3(xVal, yVal, transf.localPosition.z);
        //Debug.Log("MOVEANIM: animName = "+animName+ "; movementRect = "+ movementRect.name+ "; Progress = " + progress + "; realAnimDir = " + realAnimDir +
        //    "; animDir = " + animDir + "; xVal = " + xVal + "; yVal = " + yVal);
    }

    void StopMovementAnimation()
    {
        movementRect.localPosition = !playBack ? realFinalPos : realInitialPos;
    }
    #endregion

    #region --- Tutorial Hand ---
    void SaveChildrenData(Transform parent)
    {
        children = new List<Child>();
        for (int i = 0; i < parent.childCount; i++)
        {
            SaveChildrenData_r(parent.GetChild(i), 1);
        }
    }

    void SaveChildrenData_r(Transform parent, int depth)
    {
        children.Add(new Child(parent, depth));
        for (int i = 0; i < parent.childCount; i++)
        {
            SaveChildrenData_r(parent.GetChild(i), depth + 1);
        }
    }

    void ResetChildrenData()
    {
        int maxCounter = 100;
        int counter = 0;
        int depth = 1;
        while (children.Count > 0 && counter < maxCounter)
        {
            bool found = false;
            for (int i = 0; i < children.Count && !found; i++)
            {
                //Debug.Log("Depth = " + depth + "; child = " + children[i].transf + "; children[i].depth = " + children[i].depth+ ";  children.Count = " + children.Count);
                if (children[i].depth == depth)
                {
                    children[i].ResetData();
                    children.RemoveAt(i);
                    found = true;
                }
            }

            if (!found) depth++;
            counter++;
        }
    }
    #endregion

    public void CopySettings()
    {
        if (baseSettings != null)
        {
            type = baseSettings.baseUIAnimation.type;
            duration = baseSettings.baseUIAnimation.duration;
            frequency = baseSettings.baseUIAnimation.frequency;
            endless = baseSettings.baseUIAnimation.endless;
            forcePlay = baseSettings.baseUIAnimation.forcePlay;
            cycleAnimDir = baseSettings.baseUIAnimation.cycleAnimDir;
            cycleStartPoint = baseSettings.baseUIAnimation.cycleStartPoint;
            easeFunction = baseSettings.baseUIAnimation.easeFunction;
            ignoreTimeScale = baseSettings.baseUIAnimation.ignoreTimeScale;
            stopOnSceneChange = baseSettings.baseUIAnimation.stopOnSceneChange;
            forceScaleTo1If0 = baseSettings.baseUIAnimation.forceScaleTo1If0;

            xAmplitude = baseSettings.baseUIAnimation.xAmplitude;

            initialIntensity = baseSettings.baseUIAnimation.initialIntensity;
            finalIntensity = baseSettings.baseUIAnimation.finalIntensity;

            alphaMin = baseSettings.baseUIAnimation.alphaMin;
            alphaMax = baseSettings.baseUIAnimation.alphaMax;
            recursive = baseSettings.baseUIAnimation.recursive;

            initialScale = baseSettings.baseUIAnimation.initialScale;
            finalScale = baseSettings.baseUIAnimation.finalScale;
            originalScale = baseSettings.baseUIAnimation.originalScale;

            automaticInitialPos = baseSettings.baseUIAnimation.automaticInitialPos;
            useAsIncrement = baseSettings.baseUIAnimation.useAsIncrement;
            initialPos = baseSettings.baseUIAnimation.initialPos;
            finalPos = baseSettings.baseUIAnimation.finalPos;

            initialRot = baseSettings.baseUIAnimation.initialRot;
            finalRot = baseSettings.baseUIAnimation.finalRot;
            alwaysRotateInSameDirection = baseSettings.baseUIAnimation.alwaysRotateInSameDirection;

            buttonPressesNeeded = baseSettings.baseUIAnimation.buttonPressesNeeded;
            useBlackVeil = baseSettings.baseUIAnimation.useBlackVeil;
            handPosition = baseSettings.baseUIAnimation.handPosition;
        }
    }

    void CreateAutomaticName()
    {
        animName = transf.gameObject.name + type + "Animation" + easeFunction + "Duration=" + duration + "RandomID:" + Random.Range(0, 10000);
    }
}

public class ColorContainer
{
    public Transform transf;
    public Image image;
    public Text text;
    public TextMeshProUGUI textMeshPro;
    public SpriteRenderer spriteRenderer;

    public ColorContainer(Transform _transf)
    {
        transf = _transf;
        image = transf.GetComponent<Image>();
        text = transf.GetComponent<Text>();
        textMeshPro = transf.GetComponent<TextMeshProUGUI>();
        spriteRenderer = transf.GetComponent<SpriteRenderer>();
    }

    public void ChangeAlphaValue(float value)
    {
        Color newColor = new Color();
        if (image != null) newColor = image.color;
        else if (text != null) newColor = text.color;
        else if (textMeshPro != null) newColor = textMeshPro.color;
        else if (spriteRenderer != null) newColor = spriteRenderer.color;
        //Debug.Log(" animName = "+ animName+ "; currentDuration = " + currentDuration+ "; duration = " + duration +"; currentCycleTime = " + currentCycleTime + "; frequency = "+ frequency + "; value = " + value + "; progress = " + progress + "; realAnimDir = " + realAnimDir + "; playBack = " + playBack);
        //float aIncrement = value * animDir;
        //float alphaStartPoint = animDir == 1?alphaMin:alphaMax;
        newColor.a = value;
        if (image != null) image.color = newColor;
        else if (text != null) text.color = newColor;
        else if (textMeshPro != null) textMeshPro.color = newColor;
        else if (spriteRenderer != null) spriteRenderer.color = newColor;
    }
}