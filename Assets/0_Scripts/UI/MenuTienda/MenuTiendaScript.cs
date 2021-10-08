using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public enum ShopMenuState
{
    None,
    Opening,
    Closing,
    Normal,
    PurchasingWithRealMoney,
    Result
}

public enum ShopMenuSections
{
    None = 0,
    GemStore = 1,
    Workshop = 2,
    Trader = 3,
    Deals = 4,
    Gifts = 5,
    Food = 10
}

public enum ScrollingState
{
    None = 0,
    Manual = 1,
    Automatic = 2,
}

[ExecuteAlways]
public class MenuTiendaScript : MonoBehaviour
{
    public static MenuTiendaScript instance;
    public ShopMenuState state = ShopMenuState.None;
    public ScrollingState scrollingState = ScrollingState.None;
    public GameObject backButton;
    public Image fondoBlur;
    [HideInInspector]
    //public GameState oldGameState;
    public Image blackVeil;
    //Vector3 oldClickingZoneScale;
    public ShopMenuIcon[] shopIcons;


    [Header("--- AUTO ADJUST ---")]
    public bool getParameters = false;
    public bool autoAdjustBackgrounds = false;
    public bool updateShopIcons = false;
    public RectTransform[] contents;//Deals, Gem Shop, Workshop, Trader, Gifts
    public RectTransform contentParent;
    Vector3 oldMenuPos;
    bool autoadjusting = false;

    //Scroll
    [Header("--- SCROLL ---")]
    public RectTransform scroll;
    public RectTransform[] scrollBackgrounds;//Deals, Gem Shop, Workshop, Trader, Gifts
    public RectTransform scrollBackgroundFranjaSuperior;

    float currentScrollHeight;
    float currentScrollSpeed = 0;
    public float scrollBreakAcceleration = 200;
    public float swipeMultiplier = 3;
    public float maxScrollSpeed = 500;

    public RectTransform scrollMaxHeightSafeCopy;
    public RectTransform maxScrollHeightTransform;
    public float maxScrollHeight;
    public float minScrollHeight;
    bool outOfScrollBounds = false;

    //Automatic Scroll
    [Header("--- AUTOMATIC SCROLL ---")]
    public Ease easingFunction;
    public float automaticScrollMaxTime = 0.5f;
    float currentAutomaticScrollTime = 0;
    float initialAutomaticScrollHeight = 0;
    float targetAutomaticScrollHeight = 0;
    ShopMenuSections startingSection = ShopMenuSections.Deals;

    public RectTransform gemStoreTitle;
    public RectTransform workShopTitle;
    public RectTransform traderTitle;
    public RectTransform dealsTitle;
    public RectTransform giftsTitle;
    float workShopTitleHeight;
    public RectTransform workShopTitlePosition;
    public RectTransform traderTitlePosition;
    float traderTitleHeight;
    public RectTransform gemStoreTitlePosition;
    float gemStoreTitleHeight;
    public RectTransform dealsTitlePosition;
    float dealsTitleHeight;
    public RectTransform giftsTitlePosition;
    float giftsTitleHeight;
    public RectTransform machinePartsPosition;
    float machinePartsHeight;
    public RectTransform foodTitlePosition;
    public float foodTitleHeight;

    //Finger drag
    [Header("--- FINGER DRAG ---")]
    public bool calculatingFingerDragStarted = false;
    public List<TouchInputInfo> touchInputInfoList;
    public class TouchInputInfo
    {
        public Vector2 position;
        public float time;

        public TouchInputInfo(Vector2 _position, float _time)
        {
            position = _position;
            time = _time;
        }
    }
    float currentFingerDragTime = 0;
    float fingerSwipeDistThreshold = 1;
    float fingerSwipeTimeThreshold = 0.2f;
    public bool stoppedFingerDragInThisFrame = false;


    //AreYouSure? PopUp
    [Header("--- ARE YOU SURE? POP UP ---")]
    public RectTransform blur;
    public TextMeshProUGUI areYouSureText;
    private ShopMenuIconData currentShopMenuIconData;
    bool areYouSurePopUpOpening = false;
    bool areYouSurePopUpOpened = false;
    public GameObject areYouSureOldVersion;
    public GameObject areYouSureConsumableVersion;
    public TextMeshProUGUI areYouSureTitleText;
    public TextMeshProUGUI areYouSureDescriptionText;
    public TextMeshProUGUI areYouSureAmountText;
    public TextMeshProUGUI areYouSurePriceText;
    public Image areYouSureCurrencyIcon;
    public Image areYouSureSquare;
    public Image areYouSureIcon;
    private int areYouSureCurrentAmount = 1;

    bool increaseAmountButtonPressed = false;
    float increaseAmountButtonCurrentTime = 0;
    bool increaseAmountButtonTurboStarted = false;
    float increaseAmountButtonPressedTurboMaxTime = 0.5f;
    float increaseAmountButtonPressedTurboFreq = 0.1f;

    bool decreaseAmountButtonPressed = false;
    float decreaseAmountButtonCurrentTime = 0;
    bool decreaseAmountButtonTurboStarted = false;
    float decreaseAmountButtonPressedTurboMaxTime = 0.5f;
    float decreaseAmountButtonPressedTurboFreq = 0.1f;


    [Header("--- MERCHANT AVAILABILITY ---")]
    public RectTransform merchantNotAvailableSection;
    public RectTransform[] merchantHideSections;
    public TextMeshProUGUI merchantTimeLeftText;
    [HideInInspector]
    public bool merchantAvailable = false;
    public float merchantCurrentTime = 0;
    public float merchantCDMaxTime = 1800;
    public float merchantStayMaxTime = 300;
    public GameObject merchantTimeLeftCounter;
    float checkMerchantAvailabilityTime = 0;
    float checkMerchantAvailabilityMaxTime = 5;
    //bool isGameStateGoodForMerchant
    //{
    //    get
    //    {
    //        return GeneralPauseScript.pause.estadoJuego == GameState.menuRight || GeneralPauseScript.pause.estadoJuego == GameState.menuLeft ||
    // GeneralPauseScript.pause.estadoJuego == GameState.mapa || GeneralPauseScript.pause.estadoJuego == GameState.menuLab ||
    // GeneralPauseScript.pause.estadoJuego == GameState.maquinaPyrosita;
    //    }
    //}


    [Header("--- UI ANIMATIONS ---")]
    public UIAnimation areYouSureAnim;
    public UIAnimation firstPurchaseTutoAnim;
    public UIAnimation firstPurchaseTutoAnimYes;
    public UIAnimation shopOpeningAnimation;
    float openingTime = 0;
    float maxBlurVal = 8;

    [Header("--- COUNTERS ---")]
    public Text coinsCounterText;
    public Text gemsCounterText;

    private Vector2 resolution;
    float scrollCurrenHeightB4ResChange;

    private void Awake()
    {
        if (Application.isPlaying)
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(this);
                return;
            }
            //GetParameters();
            resolution = new Vector2(Screen.width, Screen.height);
            shopIcons = transform.GetComponentsInChildren<ShopMenuIcon>(true);
        }
        else
        {
            getParameters = autoAdjustBackgrounds = false;
        }
    }

    private void OnEnable()
    {
        if (Application.isEditor)
        {
            getParameters = autoAdjustBackgrounds = false;
        }
    }

    private void Start()
    {
        if (Application.isPlaying)
        {
            oldMenuPos = Vector3.zero;
            //Debug.Log("TIENDA START: oldMenuPos = " + oldMenuPos);
            //StartCoroutine(AutoAdjustBackgroundsDelayed(0.2f));
            blackVeil.enabled = false;
            lastConnected = MasterManager.connected;
            //UpdateAvailableMerchant(true);
        }
    }

    private void Update()
    {

        if (autoAdjustBackgrounds)
        {
            autoAdjustBackgrounds = false;
            AutoAdjustBackgrounds();
        }
        if (getParameters)
        {
            getParameters = false;
            GetParameters();
        }
#if UNITY_EDITOR
        if (updateShopIcons)
        {
            updateShopIcons = false;
            UpdateShopIcons();
        }
#endif
        if (Application.isPlaying)
        {
            //Debug.Log("resolution.x = "+ resolution.x + "; resolution.y = " + resolution.y+ "; Screen.width = "+ Screen.width + "; Screen.height = " + Screen.height);
            if (resolution.x != Screen.width || resolution.y != Screen.height)
            {
                // do your stuff
                if (state != ShopMenuState.None)
                {
                    scrollCurrenHeightB4ResChange = currentScrollHeight;
                    //ON RESOLUTION CHANGE shop opened
                    StartCoroutine(AutoAdjustBackgroundsDelayed(0.2f, true));

                }
                else
                {
                    //ON RESOLUTION CHANGE shop closed
                    StartCoroutine(AutoAdjustBackgroundsDelayed(0.2f));
                }
                resolution.x = Screen.width;
                resolution.y = Screen.height;
            }

            //Debug.LogWarning(state);
            //if (state == ShopMenuState.None)
            //    UpdateMerchantSection();
        }
    }

    #region --- AUTO ADJUST ---
    public static void RefreshLayoutGroupsImmediateAndRecursive(GameObject root)
    {
        foreach (var layoutGroup in root.GetComponentsInChildren<LayoutGroup>())
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
        }
    }

    void AutoAdjustBackgrounds(bool returnToOldScrollHeight = false)
    {
        //RefreshLayoutGroupsImmediateAndRecursive(contentParent.gameObject);
        //Canvas.ForceUpdateCanvases();
        AutoAdjustContentPosition();
        //Debug.LogWarning("AUTO ADJUSTING SCROLL BACKGROUNDS");
        for (int i = 0; i < scrollBackgrounds.Length; i++)
        {
            Rect newRect = scrollBackgrounds[i].rect;
            //Debug.Log("newRect.height = " + newRect.height + "; contents[i].rect.height = " + contents[i].rect.height);
            scrollBackgrounds[i].sizeDelta = new Vector2(scrollBackgrounds[i].sizeDelta.x, contents[i].rect.height - (scrollBackgroundFranjaSuperior.rect.height * 2));
            scrollBackgrounds[i].position = new Vector3(contents[i].position.x, contents[i].position.y - 20, 0);
        }

        GetParameters();
        if (returnToOldScrollHeight && state != ShopMenuState.None)
        {
            currentScrollHeight = scrollCurrenHeightB4ResChange;
        }
        UpdateShopMenuIconsInGame();
    }

    IEnumerator AutoAdjustBackgroundsDelayed(float waitTime = 0.2f, bool returnToOldScrollHeight = false)
    {
        autoadjusting = true;
        transform.localScale = Vector3.one;

        transform.position = transform.position + Vector3.left * 10000;
        if (state != ShopMenuState.None)
        {
            scrollCurrenHeightB4ResChange = currentScrollHeight;
            //GeneralPauseScript.pause.StartOpeningLoadingPopup();
        }
        if (state == ShopMenuState.Opening)
        {
            UIAnimationsManager.instance.StopUIAnimation(shopOpeningAnimation);
        }

        WaitForSeconds wait = new WaitForSeconds(waitTime);
        contentParent.GetComponent<ContentSizeFitter>().enabled = false;
        //Canvas.ForceUpdateCanvases();
        yield return wait;
        contentParent.GetComponent<ContentSizeFitter>().enabled = true;
        yield return wait;
        ShopMenuIcon[] shopIcons = GetComponentsInChildren<ShopMenuIcon>();
        for (int i = 0; i < shopIcons.Length; i++)
        {
            if (shopIcons[i].pricePanel == null) Debug.Log(" shopIcon " + shopIcons[i].gameObject.name + " has no pricePanel assigned");
            shopIcons[i].pricePanel.SetActive(false);
        }
        yield return wait;

        for (int i = 0; i < shopIcons.Length; i++)
        {
            shopIcons[i].pricePanel.SetActive(true);
        }

        AutoAdjustBackgrounds(returnToOldScrollHeight);
        //if (state != ShopMenuState.None)
        //{
        //    GeneralPauseScript.pause.StartClosingLoadingPopup();
        //}
        //else
        //{
        //    transform.localScale = Vector3.zero;
        //}
        //while (GeneralPauseScript.pause.loadingPopupActive)
        //{
        //    yield return null;
        //}
        if (state == ShopMenuState.Opening)
        {
            transform.localScale = Vector3.zero;
            UIAnimationsManager.instance.StartAnimation(shopOpeningAnimation);
        }
        transform.localPosition = Vector3.zero;
        autoadjusting = false;
        //transform.localScale = Vector3.zero;
    }

    void AutoAdjustContentPosition()
    {
        scroll.localPosition = scrollMaxHeightSafeCopy.localPosition;
        maxScrollHeightTransform.localPosition = scrollMaxHeightSafeCopy.localPosition;
        Vector3 topRightCorner = (Vector3.right * GetComponent<RectTransform>().rect.width / 2) + (Vector3.up * GetComponent<RectTransform>().rect.height / 2);
        float finalY = topRightCorner.y - contentParent.rect.height / 2 - GetComponent<RectTransform>().rect.height / 20;
        //Debug.Log("topRightCorner.y = "+ topRightCorner.y+ "; GetComponent<RectTransform>().rect.height = "+ GetComponent<RectTransform>().rect.height +
        //    "; contentParent.rect.height = " + contentParent.rect.height);
        contentParent.position = GetComponent<RectTransform>().TransformPoint(new Vector3(0, finalY, 0));
    }

    void GetParameters()
    {
        //GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        //Transform trader = traderTitle.parent;
        //Transform gemStore = gemStoreTitle.parent;

        //traderTitle.SetParent(transform,true);
        //gemStoreTitle.SetParent(transform, true);
        Canvas.ForceUpdateCanvases();

        maxScrollHeight = maxScrollHeightTransform.localPosition.y;
        Vector3 localTopBackGroundPos = transform.InverseTransformPoint(scrollBackgrounds[0].position);
        Vector3 localBottomBackGroundPos = transform.InverseTransformPoint(scrollBackgrounds[scrollBackgrounds.Length - 1].position);
        float backgroundTop = localTopBackGroundPos.y + scrollBackgrounds[0].rect.height / 2 + scrollBackgroundFranjaSuperior.rect.height;
        float backgroundBottom = localBottomBackGroundPos.y - scrollBackgrounds[scrollBackgrounds.Length - 1].rect.height / 2 - scrollBackgroundFranjaSuperior.rect.height;
        float scrollLength = Mathf.Abs(backgroundTop - backgroundBottom);
        minScrollHeight = maxScrollHeight + scrollLength - Screen.height + (Screen.height / 4);

        Vector3 localBackGroundTop = transform.InverseTransformPoint(new Vector3(0, backgroundTop, 0));
        Vector3 localBackgroundBottom = transform.InverseTransformPoint(new Vector3(0, backgroundBottom, 0));


        //Debug.Log("maxScrollHeight = " + maxScrollHeight + "; backgroundTop = " + backgroundTop + "; backgroundBottom = " + backgroundBottom
        //    + "; localTopBackGroundPos = " + localTopBackGroundPos + "; scrollBackgrounds[0].rect.height = " + scrollBackgrounds[0].rect.height + "; localBottomBackGroundPos = " + localBottomBackGroundPos
        //    + "; minScrollHeight = " + minScrollHeight + "; scrollLength = " + scrollLength + "; Screen.height = " + Screen.height);


        dealsTitleHeight = maxScrollHeight;

        float distToTitle = dealsTitlePosition.localPosition.y - gemStoreTitlePosition.localPosition.y;
        gemStoreTitleHeight = maxScrollHeight + distToTitle;

        distToTitle = dealsTitlePosition.localPosition.y - workShopTitlePosition.localPosition.y;
        float heightDiff = dealsTitle.rect.height / 2 - workShopTitle.rect.height / 2;
        workShopTitleHeight = maxScrollHeight + distToTitle + heightDiff;

        distToTitle = dealsTitlePosition.localPosition.y - traderTitlePosition.localPosition.y;
        traderTitleHeight = maxScrollHeight + distToTitle;

        //distToTitle = gemStoreTitlePosition.localPosition.y - traderTitlePosition.localPosition.y;
        //heightDiff = gemStoreTitle.rect.height / 2 - traderTitle.rect.height / 2;
        //traderTitleHeight = maxScrollHeight + distToTitle;

        distToTitle = dealsTitlePosition.localPosition.y - machinePartsPosition.localPosition.y;
        machinePartsHeight = maxScrollHeight + distToTitle - Screen.height / 3;

        distToTitle = dealsTitlePosition.localPosition.y - foodTitlePosition.localPosition.y;
        foodTitleHeight = maxScrollHeight + distToTitle /*- Screen.height / 3*/;
        //Debug.Log("dealsTitlePosition.localPosition.y = " + dealsTitlePosition.localPosition.y + "; foodTitlePosition.localPosition.y = " + foodTitlePosition.localPosition.y
        //+ "; distToTitle = " + distToTitle + "; maxScrollHeight = " + maxScrollHeight);
    }
#if UNITY_EDITOR
    void UpdateShopIcons()
    {
        ShopMenuIcon[] icons = GetComponentsInChildren<ShopMenuIcon>();
        for (int i = 0; i < icons.Length; i++)
        {
            //Debug.Log("Setting up icon " + icons[i].gameObject.name);
            icons[i].SetUpShopIconEditor();
        }
    }
#endif
    #endregion

    public void ThisStart(Color veilColor, float veilAlpha = 0.6f, ShopMenuSections _startingSection = ShopMenuSections.GemStore)
    {
        if (!gameObject.activeInHierarchy) gameObject.SetActive(true);
        transform.localPosition = Vector3.zero;

        ChangeVeil(veilAlpha, veilColor);
        startingSection = _startingSection;
        state = ShopMenuState.Opening;
        Debug.LogWarning(state);
        //UpdateAvailableMerchant();
        //UpdateMerchantSection();

        UIAnimationsManager.instance.StartAnimation(shopOpeningAnimation);
        openingTime = 0;

        touchInputInfoList = new List<TouchInputInfo>();
        currentScrollHeight = maxScrollHeight;
        currentScrollSpeed = 0;
        scroll.localPosition = new Vector3(scroll.localPosition.x, currentScrollHeight, scroll.localPosition.z);

        //oldClickingZoneScale = GeneralPauseScript.pause.clickingZone.transform.localScale;
        //GeneralPauseScript.pause.clickingZone.transform.localScale = new Vector3(0, 0, 1);
        //StartCoroutine(AutoAdjustBackgroundsDelayed());
        //GeneralPauseScript.pause.CurrentMenuButtonsAppearAnimation(true);
        blackVeil.enabled = true;
        areYouSurePopUpOpened = false;
        areYouSurePopUpOpening = false;
        calculatingFingerDragStarted = false;

        StartCoroutine(AutoAdjustBackgroundsDelayed());
    }

    public void ThisUpdate()
    {
        if (stoppedFingerDragInThisFrame) stoppedFingerDragInThisFrame = false;

        //CheckMerchantAvailable();

        if (state != ShopMenuState.None)
        {
            ProcessOpenMenu();
            ProcessCloseMenu();
            OpeningAreYouSurePopUp();
            ProcessClosingAreYouSure();
            //ProcessStartSecondTutoAnim();
            UpdateAreYouSurePopup();

            if (state == ShopMenuState.Normal && !(areYouSurePopUpOpened || areYouSurePopUpOpening))
            {
                if (Input.GetMouseButtonDown(0)) StartCalculatingFingerSwipe();
                else if (Input.GetMouseButtonUp(0)) FinishCalculatingFingerSwipe();
                ProcessCalculatingFingerSwipe();
                ProcessScrollMovement();
            }

            SwitchConnected();
            //if (!contentParent.GetComponent<ContentSizeFitter>().enabled) contentParent.GetComponent<ContentSizeFitter>().enabled = true;

        }
    }

    #region --- OPEN & CLOSE ANIMATION ---
    void ProcessOpenMenu()
    {
        if (state == ShopMenuState.Opening)
        {
            openingTime += Time.deltaTime;
            float value = openingTime / shopOpeningAnimation.duration;
            float distorsionFondo = value * maxBlurVal;
            distorsionFondo = Mathf.Clamp(distorsionFondo, 0, maxBlurVal);
            fondoBlur.material.SetFloat("_Size", distorsionFondo);
            if (!UIAnimationsManager.instance.IsPlaying(shopOpeningAnimation) && !autoadjusting)
            {
                FinishOpenMenu();
            }
        }
    }

    void FinishOpenMenu()
    {
        transform.localScale = new Vector3(1, 1, 1);
        fondoBlur.material.SetFloat("_Size", maxBlurVal);

        state = ShopMenuState.Normal;
        scrollingState = ScrollingState.Automatic;
        currentAutomaticScrollTime = 0;
        initialAutomaticScrollHeight = currentScrollHeight;
        //if (GeneralPauseScript.pause.doingMachineTutorial3)
        //{
        //    backButton.SetActive(false);
        //    targetAutomaticScrollHeight = machinePartsHeight;
        //}
        //else
        //{
        //    //scrollingState = ScrollingState.Manual;
        //    backButton.SetActive(true);
        //    JumpToSection((int)startingSection);
        //}

    }

    void StartCloseMenu()
    {
        if (state != ShopMenuState.Closing)
        {
            state = ShopMenuState.Closing;
            UIAnimationsManager.instance.StartAnimation(shopOpeningAnimation, true);
            openingTime = 0;
        }
    }

    void ProcessCloseMenu()
    {
        //Debug.Log("Start ProcessCloseMenu-> state = " + state);
        if (state == ShopMenuState.Closing)
        {
            openingTime += Time.deltaTime;
            float value = openingTime / shopOpeningAnimation.duration;
            float distorsionFondo = value * maxBlurVal;
            distorsionFondo = maxBlurVal - distorsionFondo;
            distorsionFondo = Mathf.Clamp(distorsionFondo, 0, maxBlurVal);
            fondoBlur.material.SetFloat("_Size", distorsionFondo);
            //Debug.Log("Closing Shop...");
            if (!UIAnimationsManager.instance.IsPlaying(shopOpeningAnimation))
            {
                FinishCloseMenu();
            }
        }
    }

    void FinishCloseMenu()
    {
        transform.localScale = new Vector3(0, 0, 1);
        fondoBlur.material.SetFloat("_Size", 0);
        state = ShopMenuState.None;
        //RestartMerchantCD();

        //GeneralPauseScript.pause.CambioEstado((int)oldGameState);
        //if (GeneralPauseScript.pause.estadoJuego == GameState.menuRight || GeneralPauseScript.pause.estadoJuego == GameState.menuLeft)
        //{
        //    //GeneralPauseScript.pause.clickingZone.transform.localScale = oldClickingZoneScale;
        //    GeneralPauseScript.pause.UpdateMenuCounters();
        //}
        //else if (MenuMejoraPersonajesScript.instance.state != MenuMejoraPersonajesState.None)
        //{
        //    MenuMejoraPersonajesScript.instance.UpdateFlaskCounters();
        //    MenuMejoraPersonajesScript.instance.UpdateGemCounter();
        //}
        //else if (GeneralPauseScript.pause.estadoJuego == GameState.maquinaPyrosita)
        //{
        //    MenuMaquinaScript.instance.UpdateGemCounter();
        //}

        transform.position = transform.position + Vector3.left * 10000;
        //GeneralPauseScript.pause.CurrentMenuButtonsAppearAnimation();
        blackVeil.enabled = false;
    }
    #endregion

    public void BackButton()
    {
        if (MenuTiendaScript.instance.state != ShopMenuState.Opening && MenuTiendaScript.instance.state != ShopMenuState.Closing &&
    MenuTiendaScript.instance.state != ShopMenuState.PurchasingWithRealMoney)
        {
            //GeneralPauseScript.pause.CambioEstado((int)GameState.menu);
            MasterManager.GameResourcesManager.SonidoClick();
            StartCloseMenu();
        }
    }

    #region --- SCROLL ---
    public void JumpToSection(int section)
    {
        if (state == ShopMenuState.Opening || state == ShopMenuState.Closing && state == ShopMenuState.PurchasingWithRealMoney) return;

        initialAutomaticScrollHeight = currentScrollHeight;
        switch ((ShopMenuSections)section)
        {
            case ShopMenuSections.GemStore:
                targetAutomaticScrollHeight = gemStoreTitleHeight;
                break;
            case ShopMenuSections.Workshop:
                targetAutomaticScrollHeight = workShopTitleHeight;
                break;
            case ShopMenuSections.Trader:
                targetAutomaticScrollHeight = traderTitleHeight;
                break;
            case ShopMenuSections.Deals:
                targetAutomaticScrollHeight = dealsTitleHeight;
                break;
            case ShopMenuSections.Gifts:
                targetAutomaticScrollHeight = giftsTitleHeight;
                break;
            case ShopMenuSections.Food:
                Debug.Log("JUMP TO FOOD");
                targetAutomaticScrollHeight = foodTitleHeight;
                break;
        }
        currentScrollSpeed = 0;
        scrollingState = ScrollingState.Automatic;
        currentAutomaticScrollTime = 0;
        outOfScrollBounds = false;
        //Debug.Log(" targetAutomaticScrollHeight = " + targetAutomaticScrollHeight);
    }

    void ProcessScrollMovement()
    {
        //Debug.Log("scrolling state = " + scrollingState);
        switch (scrollingState)
        {
            case ScrollingState.Manual:
                float oldScrollHeight = currentScrollHeight;
                float currentAcc = scrollBreakAcceleration;
                float sign = Mathf.Sign(currentScrollSpeed);
                if (!calculatingFingerDragStarted)
                {
                    if (currentScrollHeight < maxScrollHeight - 0.01f)//The - 0.01f is to avoid a bug where currentScrollHeight is == maxScrollHeight, but the code thinks is lower somehow
                    {
                        float multiplier = Mathf.Abs(maxScrollHeight - currentScrollHeight) * 0.04f;
                        currentAcc = scrollBreakAcceleration * multiplier;
                        outOfScrollBounds = true;
                        sign = -1;
                    }
                    else if (currentScrollHeight > minScrollHeight)
                    {
                        float multiplier = Mathf.Abs(currentScrollHeight - minScrollHeight) * 0.04f;
                        currentAcc = scrollBreakAcceleration * multiplier;
                        outOfScrollBounds = true;
                        sign = 1;
                    }
                    //Debug.Log(" currentScrollHeight = "+ currentScrollHeight + "; maxScrollHeight = "+ maxScrollHeight + "; currentScrollHeight = "+ currentScrollHeight + ";minScrollHeight  = " + minScrollHeight);
                }
                //Debug.Log("currentScrollSpeed = " + currentScrollSpeed);
                currentScrollSpeed += currentAcc * sign * Time.deltaTime;
                if (!outOfScrollBounds)
                {
                    currentScrollSpeed = sign > 0 ? Mathf.Clamp(currentScrollSpeed, 0, maxScrollSpeed) : Mathf.Clamp(currentScrollSpeed, -maxScrollSpeed, 0);
                }
                //Debug.Log("currentScrollSpeed = " + currentScrollSpeed + "; currentAcc = " + currentAcc + "; sign = " + sign + "; outOfScrollBounds = " + outOfScrollBounds);


                currentScrollHeight = currentScrollHeight + currentScrollSpeed * Time.deltaTime;
                if (outOfScrollBounds)
                {
                    //going too far down?
                    float currentMinHeight = oldScrollHeight > minScrollHeight ? minScrollHeight : -float.MaxValue;
                    //going too far up?
                    float currentMaxHeight = oldScrollHeight < maxScrollHeight ? maxScrollHeight : float.MaxValue;
                    currentScrollHeight = Mathf.Clamp(currentScrollHeight, currentMinHeight, currentMaxHeight);
                    if (currentScrollHeight == currentMinHeight || currentScrollHeight == currentMaxHeight)
                    {
                        currentScrollSpeed = 0;
                        outOfScrollBounds = false;
                    }
                    //Debug.Log("currentScrollHeight = " + currentScrollHeight + "; currentMinHeight = " + currentMinHeight + "; currentMaxHeight = " + currentMaxHeight);

                }
                break;
            case ScrollingState.Automatic:
                currentAutomaticScrollTime += Time.deltaTime;
                float progress = currentAutomaticScrollTime / automaticScrollMaxTime;
                currentScrollHeight = EasingFunction.SelectEasingFunction(easingFunction, initialAutomaticScrollHeight, targetAutomaticScrollHeight, progress);
                //Debug.Log("Automatic scroll: currentScrollHeight = " + currentScrollHeight+ "; currentAutomaticScrollTime = " + currentAutomaticScrollTime);
                if (progress >= 1)
                {
                    currentScrollHeight = targetAutomaticScrollHeight;
                    //if (GeneralPauseScript.pause.doingMachineTutorial3)
                    //{
                    //    UIAnimationsManager.instance.StartAnimation(firstPurchaseTutoAnim);
                    //}
                    scrollingState = ScrollingState.Manual;
                }
                break;
        }
        scroll.localPosition = new Vector3(scroll.localPosition.x, currentScrollHeight, scroll.localPosition.z);
        //Debug.Log("currentScrollHeight = " + currentScrollHeight+"; scroll.localPosition = " + scroll.localPosition);
    }

    public float fingerDragTotalDist = 0;
    Vector3 fingerDragInitialPos = Vector3.zero;
    public void StartCalculatingFingerSwipe()
    {
        if (!calculatingFingerDragStarted  && !areYouSurePopUpOpened/* && !GeneralPauseScript.pause.popUpActive*/)
        {
            if (scrollingState == ScrollingState.Automatic) currentScrollHeight = scroll.localPosition.y;
            scrollingState = ScrollingState.Manual;
            //Debug.Log("START DRAGGING");
            calculatingFingerDragStarted = true;
            touchInputInfoList = new List<TouchInputInfo>();
            touchInputInfoList.Add(new TouchInputInfo(Input.mousePosition, 0));
            currentFingerDragTime = 0;
            currentScrollSpeed = 0;
            outOfScrollBounds = false;
            fingerDragTotalDist = 0;
            fingerDragInitialPos = Input.mousePosition;
        }
    }

    void ProcessCalculatingFingerSwipe()
    {
        if (calculatingFingerDragStarted)
        {

            currentFingerDragTime += Time.deltaTime;
            touchInputInfoList.Add(new TouchInputInfo(Input.mousePosition, currentFingerDragTime));
            float scrollMovement = touchInputInfoList[touchInputInfoList.Count - 1].position.y - touchInputInfoList[touchInputInfoList.Count - 2].position.y;

            float outOfScrollBoundsMult = 1;
            //Too far up
            if (currentScrollHeight < maxScrollHeight)
            {
                outOfScrollBoundsMult -= (maxScrollHeight - currentScrollHeight) * 0.001f;
            }//Too Far down
            else if (currentScrollHeight > minScrollHeight)
            {
                outOfScrollBoundsMult -= (currentScrollHeight - minScrollHeight) * 0.001f;
            }
            outOfScrollBoundsMult = Mathf.Clamp01(outOfScrollBoundsMult);
            currentScrollHeight += scrollMovement * outOfScrollBoundsMult;

            List<TouchInputInfo> listCopy = touchInputInfoList;
            for (int i = 0; i < listCopy.Count; i++)
            {
                float timePassed = Mathf.Abs(listCopy[i].time - currentFingerDragTime);
                //Erase too old inputs
                if (timePassed > fingerSwipeTimeThreshold) touchInputInfoList.Remove(listCopy[i]);
            }
            //Debug.Log("DRAGGING: scrollMovement = " + scrollMovement);
            fingerDragTotalDist = (Input.mousePosition - fingerDragInitialPos).magnitude;
        }
    }

    public void FinishCalculatingFingerSwipe()
    {
        if (calculatingFingerDragStarted)
        {
            calculatingFingerDragStarted = false;
            //Calculate if swipe
            float swipeDist = touchInputInfoList[touchInputInfoList.Count - 1].position.y - touchInputInfoList[0].position.y;
            if (Mathf.Abs(swipeDist) >= fingerSwipeDistThreshold)
            {
                if (!outOfScrollBounds) currentScrollSpeed = swipeDist * swipeMultiplier;
                //Debug.Log("FINISH DRAGGING: currentScrollSpeed = " + currentScrollSpeed);
            }
            fingerDragTotalDist = (Input.mousePosition - fingerDragInitialPos).magnitude;
            if (fingerDragTotalDist > 0.1f)
            {
                stoppedFingerDragInThisFrame = true;
                fingerDragTotalDist = 0;
            }
        }
    }
    #endregion

    #region --- PURCHASE ---
    public void TryPurchaseItem(ShopMenuIconData shopMenuIconData)
    {
        currentShopMenuIconData = shopMenuIconData;

        //Compra ingame
        if (currentShopMenuIconData.realShopProductName == ShopProductNames.None)
        {
            blur.gameObject.SetActive(true);
            SetupAreYouSurePopup();
            UIAnimationsManager.instance.StartAnimation(areYouSureAnim);
            areYouSurePopUpOpening = true;
        }
        else//compra real
        {
            state = ShopMenuState.PurchasingWithRealMoney;
            //GeneralPauseScript.pause.StartOpeningLoadingPopup();
            if (!currentShopMenuIconData.Purchase(areYouSureCurrentAmount))
            {
                Debug.LogWarning("Tried to purchase but error ocurred");
                ShowResult(false);
                return;
            }
        }
        FinishCalculatingFingerSwipe();
    }

    public void ShowResult(bool success = false)
    {
        if (state == ShopMenuState.Normal)
        {
            state = ShopMenuState.Result;
            if (success)
            {
                Debug.Log("Show Normal Results");
                GetRewardsForRealAndShowResultPopUp();
            }
            else
            {
                //GeneralPauseScript.pause.ShowPopUp("_ui_tienda_result_fail_not_enough_", GeneralPauseScript.pause.Nothing, PopupMode.Ok);
                //GeneralPauseScript.pause.currentPopUpText.text = GeneralPauseScript.pause.currentPopUpText.text.Replace("&currency",
                //    MasterManager.GameDataManager.GetRewardData(currentShopMenuIconData.priceCurrency).GetRewardCurrency());
            }
        }
        else if (state == ShopMenuState.PurchasingWithRealMoney)
        {
            if (success)
            {
                switch (currentShopMenuIconData.productID)
                {
                    default:
                        GetRewardsForRealAndShowResultPopUp(true);
                        break;
                }
            }
            else
            {
                //GeneralPauseScript.pause.ShowPopUp("_ui_tienda_result_fail_error_", GeneralPauseScript.pause.Nothing, PopupMode.Ok);
            }
        }
        state = ShopMenuState.Normal;
    }

    void GetRewardsForRealAndShowResultPopUp(bool realMoneyPurchase = false)
    {
        Debug.Log("currentShopMenuIconData = " + (currentShopMenuIconData == null ? "null" : "not null"));
        UnityAction action =(UnityAction)null;

        if (state == ShopMenuState.PurchasingWithRealMoney)
        {
            List<Reward> rewards = new List<Reward>();
            for (int i = 0; i < currentShopMenuIconData.rewards.Length; i++)
            {
                rewards.Add(currentShopMenuIconData.rewards[i]);
            }
            //GeneralPauseScript.pause.ShowPopupRewards(rewards, action);
        }
        currentShopMenuIconData.GetRewards(realMoneyPurchase, areYouSureCurrentAmount);
        UpdateShopMenuIconsInGame();
    }

    public void RealMoneyProductBoughtCallback(IAPOperationStatus status)
    {
        if (state == ShopMenuState.PurchasingWithRealMoney)
        {
            //GeneralPauseScript.pause.StartClosingLoadingPopup();
            if (status == IAPOperationStatus.Success)
            {
                ShowResult(true);
            }
            else if (status == IAPOperationStatus.Fail)
            {
                ShowResult(false);
            }
            
        }
    }

    public void FinishTutorial()
    {
        state = ShopMenuState.Normal;
        scrollingState = ScrollingState.Manual;
        backButton.SetActive(true);
        //currentShopMenuIconData.locked = true;
        BackButton();
    }

    //void ProcessStartSecondTutoAnim()
    //{
    //    if (GeneralPauseScript.pause.doingMachineTutorial3 && areYouSurePopUpOpened && !UIAnimationsManager.instance.IsDone(firstPurchaseTutoAnimYes)
    //        && !UIAnimationsManager.instance.IsPlaying(firstPurchaseTutoAnimYes))
    //    {
    //        UIAnimationsManager.instance.StartAnimation(firstPurchaseTutoAnimYes);
    //    }
    //}

    #region --- IN GAME PURCHASE ---
    public void OpeningAreYouSurePopUp()
    {
        if (areYouSurePopUpOpening)
        {
            if (!UIAnimationsManager.instance.IsPlaying(areYouSureAnim))
            {
                areYouSurePopUpOpening = false;
                areYouSurePopUpOpened = true;
            }
        }
    }

    void SetupAreYouSurePopup()
    {
        //Reset variables
        areYouSureConsumableVersion.SetActive(false);
        areYouSureOldVersion.SetActive(false);
        areYouSureText.gameObject.SetActive(false);
        areYouSureTitleText.gameObject.SetActive(false);
        areYouSureDescriptionText.gameObject.SetActive(false);

        if (currentShopMenuIconData.consumable && currentShopMenuIconData.iconType == ShopIconType.Small)
        {
            areYouSureConsumableVersion.SetActive(true);
            areYouSureTitleText.gameObject.SetActive(true);
            areYouSureDescriptionText.gameObject.SetActive(true);
            areYouSureTitleText.text = currentShopMenuIconData.rewards[0].rewardData.GetRewardCurrency();
            areYouSureDescriptionText.text = I2.Loc.LocalizationManager.GetTranslation(currentShopMenuIconData.infoID);
            areYouSureCurrentAmount = 1;
            areYouSurePriceText.text = currentShopMenuIconData.GetPrice().ToString();
            areYouSureCurrencyIcon.sprite = MasterManager.GameDataManager.GetRewardData(currentShopMenuIconData.priceCurrency).spriteWithoutBackground;

            areYouSureSquare.sprite = GameResourcesManager.GetSquareSprite(currentShopMenuIconData.quality, currentShopMenuIconData.iconType);
            areYouSureIcon.sprite = currentShopMenuIconData.iconSprite;
        }
        else
        {
            areYouSureOldVersion.SetActive(true);
            areYouSureText.gameObject.SetActive(true);
            string currency = "" + MasterManager.GameDataManager.GetRewardData(currentShopMenuIconData.priceCurrency).GetRewardCurrency();
            string areYouSureString = I2.Loc.LocalizationManager.GetTranslation("_ui_tienda_are_you_sure_");// "Are you sure you want to purchase " + currentShopMenuIconData.purchaseName + " for " + currentShopMenuIconData.GetPrice() + " " + currency;
            areYouSureString = areYouSureString.Replace("&product", currentShopMenuIconData.GetProductName());
            string costString = currentShopMenuIconData.GetPriceString();
            if (currentShopMenuIconData.priceCurrency != RewardType.RealMoney) costString += " " + currency;
            areYouSureString = areYouSureString.Replace("&cost", "" + costString);
            areYouSureText.text = areYouSureString;
        }
        UpdateAreYouSurePopup();
    }

    void UpdateAreYouSurePopup()
    {
        if (areYouSurePopUpOpened || areYouSurePopUpOpening)
        {
            if (currentShopMenuIconData.consumable)
            {
                areYouSureAmountText.text = EloyExtensions.UnityExtensions.AddThousandsSeparators(areYouSureCurrentAmount.ToString());
                areYouSurePriceText.text = EloyExtensions.UnityExtensions.AddThousandsSeparators((currentShopMenuIconData.GetPrice() * areYouSureCurrentAmount).ToString());
                ProcessPressingIncreaseAmountButton();
                ProcessPressingDecreaseAmountButton();
            }
        }
    }

    public void PressNo()
    {
        MasterManager.GameResourcesManager.SonidoClick();
        StartClosingAreYouSure();
        state = ShopMenuState.Normal;
        areYouSureCurrentAmount = 1;
    }

    public void PressYes()
    {
        //Debug.Log("Pulsa sí en el pop up de compra");
        state = ShopMenuState.Normal;
        StartClosingAreYouSure(true);
        MasterManager.GameResourcesManager.SonidoClick();
    }

    public void PurchaseConsumableInGameMoney()
    {
        if (!currentShopMenuIconData.Purchase(areYouSureCurrentAmount))
        {
            Debug.LogWarning("Tried to purchase but error ocurred");
            state = ShopMenuState.Normal;
            ShowResult(false);
            MasterManager.GameResourcesManager.SonidoClick();
            return;
        }
        currentShopMenuIconData.GetRewards(false, areYouSureCurrentAmount);
        UpdateShopMenuIconsInGame();
        MasterManager.GameResourcesManager.SonidoMoneda();
        //if (GeneralPauseScript.pause.doingMachineTutorial3) FinishTutorial();
        StartClosingAreYouSure();
    }

    bool closingAreYouSureStarted = false;
    bool yes = false;
    void StartClosingAreYouSure(bool _yes = false)
    {
        if (areYouSurePopUpOpened && !closingAreYouSureStarted)
        {
            yes = _yes;
            closingAreYouSureStarted = true;
            UIAnimationsManager.instance.StartAnimation(areYouSureAnim, true);
            blur.gameObject.SetActive(false);
        }
    }

    public void ProcessClosingAreYouSure()
    {
        if (areYouSurePopUpOpened && closingAreYouSureStarted)
        {
            //Debug.Log("Process closing Are you sure");

            if (!UIAnimationsManager.instance.IsPlaying(areYouSureAnim))
            {
                FinishClosingAreYouSure();
            }
        }
    }

    void FinishClosingAreYouSure()
    {
        if (areYouSurePopUpOpened && closingAreYouSureStarted)
        {
            //Debug.Log("Finish closing Are you sure");
            areYouSurePopUpOpened = false;
            closingAreYouSureStarted = false;

            if (yes)
            {
                if (!currentShopMenuIconData.Purchase(areYouSureCurrentAmount))
                {
                    Debug.LogWarning("Tried to purchase but error ocurred");
                    state = ShopMenuState.Normal;
                    ShowResult(false);
                    return;
                }

                //Si llega aquí es que ha salido bien
                ShowResult(true);
            }
            else
            {
                currentShopMenuIconData = null;
            }
        }
    }

    #endregion

    #region --- SELECT AMOUNT IN GAME PURCHASE ---

    public void AreYouSureIncreaseAmount(int amount)
    {
        areYouSureCurrentAmount += amount;
        areYouSureCurrentAmount = Mathf.Clamp(areYouSureCurrentAmount, 1, 999999);
        MasterManager.GameResourcesManager.SonidoClick();
    }

    public void StartPressingIncreaseAmountButton()
    {
        if (!increaseAmountButtonPressed)
        {
            increaseAmountButtonPressed = true;
            increaseAmountButtonCurrentTime = 0;
            increaseAmountButtonTurboStarted = false;
            AreYouSureIncreaseAmount(1);
        }
    }

    void ProcessPressingIncreaseAmountButton()
    {
        if (increaseAmountButtonPressed)
        {
            increaseAmountButtonCurrentTime += Time.deltaTime;
            if (!increaseAmountButtonTurboStarted && increaseAmountButtonCurrentTime > increaseAmountButtonPressedTurboMaxTime)
            {
                increaseAmountButtonTurboStarted = true;
                increaseAmountButtonCurrentTime = 0;
            }

            if (increaseAmountButtonTurboStarted)
            {
                if (increaseAmountButtonCurrentTime >= increaseAmountButtonPressedTurboFreq)
                {
                    increaseAmountButtonCurrentTime = 0;
                    AreYouSureIncreaseAmount(1);
                }
            }
        }
    }

    public void StopPressingIncreaseAmountButton()
    {
        if (increaseAmountButtonPressed)
        {
            increaseAmountButtonPressed = false;
            //if (!addGemButtonPressedTurboStarted) AddGem();
        }
    }

    public void StartPressingDecreaseAmountButton()
    {
        if (!decreaseAmountButtonPressed)
        {
            decreaseAmountButtonPressed = true;
            decreaseAmountButtonCurrentTime = 0;
            decreaseAmountButtonTurboStarted = false;
            AreYouSureIncreaseAmount(-1);
        }
    }

    void ProcessPressingDecreaseAmountButton()
    {
        if (decreaseAmountButtonPressed)
        {
            decreaseAmountButtonCurrentTime += Time.deltaTime;
            if (!decreaseAmountButtonTurboStarted && decreaseAmountButtonCurrentTime > decreaseAmountButtonPressedTurboMaxTime)
            {
                decreaseAmountButtonTurboStarted = true;
                decreaseAmountButtonCurrentTime = 0;
            }

            if (decreaseAmountButtonTurboStarted)
            {
                if (decreaseAmountButtonCurrentTime >= decreaseAmountButtonPressedTurboFreq)
                {
                    decreaseAmountButtonCurrentTime = 0;
                    AreYouSureIncreaseAmount(-1);
                }
            }
        }
    }

    public void StopPressingDecreaseAmountButton()
    {
        if (decreaseAmountButtonPressed)
        {
            decreaseAmountButtonPressed = false;
            //if (!addGemButtonPressedTurboStarted) AddGem();
        }
    }
    #endregion

    #endregion

    public void ChangeVeil(float alpha, Color color)
    {
        blackVeil.color = new Color(color.r, color.g, color.b, alpha);
    }

    #region --- MERCHANT CD ---
    //void CheckMerchantAvailable()
    //{
    //    if (isGameStateGoodForMerchant)
    //    {
    //        checkMerchantAvailabilityTime += Time.deltaTime;
    //        if (checkMerchantAvailabilityTime >= checkMerchantAvailabilityMaxTime)
    //        {
    //            checkMerchantAvailabilityTime = 0;
    //            UpdateAvailableMerchant();
    //        }
    //    }
    //}

 //   public bool UpdateAvailableMerchant(bool forceDo=false)
 //   {
 //       if (isGameStateGoodForMerchant || forceDo)
 //       {
 //           bool oldMerchantAvailable = merchantAvailable;
 //           System.DateTime date = WorldTimeAPI.GetCurrentDate();

 //           //are we just b4 mision 9 or mision 10 tutorial? then don't call the merchant
 //           if ((GestorDeMisionesScript.gestorMisiones.nivelEnCurso == 1 && GestorDeMisionesScript.gestorMisiones.misionActiva == 9 &&
 //PlayerPrefs.GetInt("CraftMachineTutorial2Finished", 0) == 0 && !GeneralPauseScript.pause.doingMachineTutorial2) ||
 //(GestorDeMisionesScript.gestorMisiones.nivelEnCurso == 1 && GestorDeMisionesScript.gestorMisiones.misionActiva == 10 &&
 //PlayerPrefs.GetInt("CraftMachineTutorial3Finished", 0) == 0 && !GeneralPauseScript.pause.doingMachineTutorial3)) merchantAvailable = false;
 //           else if (MasterManager.GameDataManager.GetReward(RewardType.DineroReal_VIP) >= 1 && !GeneralPauseScript.pause.doingMachineTutorial1 &&
 //               !GeneralPauseScript.pause.doingMachineTutorial2 && !GeneralPauseScript.pause.doingMachineTutorial3)
 //           {
 //               merchantAvailable = true;
 //           }
 //           else
 //           {
 //               if (!merchantTimeLeftCounter.activeInHierarchy) merchantTimeLeftCounter.SetActive(true);

 //               //Grab the old time from the player prefs as a long
 //               long temp = System.Convert.ToInt64(PlayerPrefs.GetString("LastSavedDate", System.DateTime.Now.ToBinary().ToString()));

 //               //Convert the old time from binary to a DataTime variable
 //               System.DateTime oldDate = System.DateTime.FromBinary(temp);

 //               //Use the Subtract method and store the result as a timespan variable
 //               System.TimeSpan timePassed = date.Subtract(oldDate);
 //               //DateTime lastDate = DateTime.ParseExact(PlayerPrefs.GetString("LastSavedDate"), "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
 //               //                           CultureInfo.InvariantCulture.DateTimeFormat,
 //               //                           DateTimeStyles.AssumeUniversal);
 //               //float timePassed = (float)(date - lastDate).TotalSeconds;

 //               float maxTime = merchantStayMaxTime + merchantCDMaxTime;

 //               merchantCurrentTime = PlayerPrefs.GetFloat("MerchantCurrentTime", -1);
 //               //Debug.Log("MerchantCurrentTime = " + merchantCurrentTime);
 //               merchantCurrentTime += (float)timePassed.TotalSeconds;
 //               merchantCurrentTime %= maxTime;
 //               bool newMerchantAvailable;
 //               //Debug.Log("merchantCurrentTime = " + merchantCurrentTime + "; merchantCDMaxTime = " + merchantCDMaxTime);
 //               if (merchantCurrentTime < merchantCDMaxTime)//Merchant on CD
 //               {
 //                   newMerchantAvailable = false;
 //               }
 //               else// Merchant Ready
 //               {
 //                   newMerchantAvailable = true;
 //               }

 //               if (state != ShopMenuState.None && oldMerchantAvailable && !newMerchantAvailable)
 //               {
 //                   merchantAvailable = true;
 //               }
 //               else
 //               {
 //                   merchantAvailable = newMerchantAvailable;
 //               }
 //           }

 //           if (oldMerchantAvailable != merchantAvailable)
 //           {
 //               ShowMerchantSection();
 //               if (merchantAvailable) MercaderScript.instance.StartAppearAnimation();
 //               else MercaderScript.instance.StartAppearAnimation(false);

 //           }

 //           UpdateMerchantTimeLeft();
 //           PlayerPrefs.SetString("LastSavedDate", date.ToBinary().ToString());
 //           PlayerPrefs.SetFloat("MerchantCurrentTime", merchantCurrentTime);

 //           //Debug.Log("oldDate: " + oldDate + "; timePassed = " + timePassed + "; merchantAvailable = " + merchantAvailable + "; merchantCurrentTime = " + merchantCurrentTime);

 //           return merchantAvailable;
 //       }
 //       return false;
 //   }

    //void UpdateMerchantSection()
    //{
    //    if ((merchantAvailable && merchantNotAvailableSection.gameObject.activeInHierarchy) || (!merchantAvailable && !merchantNotAvailableSection.gameObject.activeInHierarchy))
    //    {
    //        ShowMerchantSection();
    //    }
    //}

    //void ShowMerchantSection()
    //{
    //    //Debug.Log("SHOW/HIDE MERCHANT SECTION!");

    //    if (merchantAvailable)
    //    {
    //        //Debug.LogWarning("Merchant Available!");
    //        for (int i = 0; i < merchantHideSections.Length; i++)
    //        {
    //            merchantHideSections[i].gameObject.SetActive(true);
    //        }
    //        merchantNotAvailableSection.gameObject.SetActive(false);
    //    }
    //    else
    //    {
    //        for (int i = 0; i < merchantHideSections.Length; i++)
    //        {
    //            //Debug.Log(merchantHideSections[i].name + " deactivated!");
    //            merchantHideSections[i].gameObject.SetActive(false);
    //        }
    //        merchantNotAvailableSection.gameObject.SetActive(true);
    //    }

    //    StartCoroutine(AutoAdjustBackgroundsDelayed(0.2f, true));
    //}

    //void UpdateMerchantTimeLeft()
    //{
    //    if (state != ShopMenuState.None)
    //    {
    //        if (!merchantAvailable)
    //        {
    //            float timeLeft = merchantCDMaxTime - merchantCurrentTime;
    //            string minutes = Mathf.FloorToInt(timeLeft / 60).ToString();
    //            if (minutes.Length == 1) minutes = "0" + minutes;
    //            string seconds = Mathf.FloorToInt(timeLeft % 60).ToString();
    //            if (seconds.Length == 1) seconds = "0" + seconds;

    //            merchantTimeLeftText.text = minutes + ":" + seconds;
    //        }
    //    }
    //}

    //void RestartMerchantCD()
    //{
    //    bool oldMerchantAvailable = merchantAvailable;
    //    if (oldMerchantAvailable)
    //    {
    //        PlayerPrefs.SetFloat("MerchantCurrentTime", 0);
    //        PlayerPrefs.SetString("LastSavedDate", WorldTimeAPI.GetCurrentDate().ToBinary().ToString());
    //    }
    //}
    #endregion

    public void UpdateShopMenuIconsInGame()
    {
        if (Application.isPlaying)
        {
            for (int i = 0; i < shopIcons.Length; i++)
            {
                shopIcons[i].SetupShopIconInGame();
            }
            SwitchConnected(1);
        }
    }

    public void LockShopMenuIcon(ShopMenuIconData shopMenuIconData)
    {
        for (int i = 0; i < shopIcons.Length; i++)
        {
            if (shopIcons[i].shopMenuIconData == shopMenuIconData)
            {
                shopIcons[i].Lock();
            }
        }
    }

    public void UnlockShopMenuIcon(ShopMenuIconData shopMenuIconData)
    {
        for (int i = 0; i < shopIcons.Length; i++)
        {
            if (shopIcons[i].shopMenuIconData == shopMenuIconData)
            {
                shopIcons[i].lockImage.SetActive(false);
                shopIcons[i].UpdateInteractable();
            }
        }
    }

    public void ActivateIconTick(ShopMenuIconData shopMenuIconData)
    {
        for (int i = 0; i < shopIcons.Length; i++)
        {
            if (shopIcons[i].shopMenuIconData == shopMenuIconData)
            {
                shopIcons[i].ActivateTickIcon();
                //Debug.Log("ACTIVATE TICK ON " + shopIcons[i].gameObject.name);
            }
        }
    }

    bool lastConnected = false;
    void SwitchConnected(int mode = 0)
    {
        switch (mode)
        {
            case 0:
                if (lastConnected != MasterManager.connected)
                {
                    //Debug.Log("connected = " + MasterManager.connected + "; lastConnected = " + lastConnected);
                    if (MasterManager.connected)
                    {
                        for (int i = 0; i < shopIcons.Length; i++)
                        {
                            if (shopIcons[i].shopMenuIconData.realShopProductName != ShopProductNames.None) shopIcons[i].shopMenuIconData.Unlock();
                        }
                    }
                    else
                    {
                        for (int i = 0; i < shopIcons.Length; i++)
                        {
                            if (shopIcons[i].shopMenuIconData.realShopProductName != ShopProductNames.None) shopIcons[i].shopMenuIconData.Lock();
                        }
                    }
                }
                break;
            case 1:
                if (MasterManager.connected)
                {
                    for (int i = 0; i < shopIcons.Length; i++)
                    {
                        if (shopIcons[i].shopMenuIconData.realShopProductName != ShopProductNames.None) shopIcons[i].shopMenuIconData.Unlock();
                    }
                }
                else
                {
                    for (int i = 0; i < shopIcons.Length; i++)
                    {
                        if (shopIcons[i].shopMenuIconData.realShopProductName != ShopProductNames.None) LockShopMenuIcon(shopIcons[i].shopMenuIconData);
                    }
                }
                break;
        }
        lastConnected = MasterManager.connected;
    }

    public ShopMenuIconData GetShopMenuIconData(ShopProductNames productType)
    {
        for (int i = 0; i < shopIcons.Length; i++)
        {
            if (shopIcons[i].shopMenuIconData.realShopProductName == productType)
            {
                return shopIcons[i].shopMenuIconData;
            }
        }
        return null;
    }
}
