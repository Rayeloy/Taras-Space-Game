using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EloyExtensions;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.Events;
#endif

[ExecuteAlways]
public class ShopMenuIcon : MonoBehaviour
{
    public bool SetUp = false;
    public ShopMenuIconData shopMenuIconData;
    public Image square;
    public Image icon;
    public TextMeshProUGUI buttonText;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI priceRealMoneyText;
    public GameObject pricePanel;
    public GameObject lockImage;
    public GameObject tickImage;
    public Image currencyIcon;
    public GameObject specialOfferText;
    string currentLanguage;
    [Header("--- BANNERS ---")]
    public string[] textsIDs;
    public TextMeshProUGUI[] texts;
    public Button button;
    public Button infoButton;


    //public GameObject priceBackground;
#if UNITY_EDITOR
    private void Awake()
    {
        //if(!MasterManager.GameResourcesManager.IsInPrefabMode(gameObject) && PrefabStageUtility.GetCurrentPrefabStage()!=null)
        //SetUpShopIcon();
    }

    private void OnValidate()
    {
        //if(!MasterManager.GameResourcesManager.IsInPrefabMode(gameObject) && PrefabStageUtility.GetCurrentPrefabStage() != null)
        //SetUpShopIcon();
    }
#endif
    private void Update()
    {
#if UNITY_EDITOR
        if (SetUp)
        {
            SetUp = false;
            SetUpShopIconEditor();
        }
#endif
    }

#if UNITY_EDITOR
    /// <summary>
    /// Only to be used in editor, not while application is running
    /// </summary>
    public void SetUpShopIconEditor()
    {
        if (!enabled) return;
        if (gameObject != null && shopMenuIconData != null)
        {
            currentLanguage = I2.Loc.LocalizationManager.CurrentLanguage;
            gameObject.name = shopMenuIconData.name;
            buttonText.text = GetButtonText();

            if (shopMenuIconData.iconType != ShopIconType.None && shopMenuIconData.iconType != ShopIconType.Banner)
                square.sprite = GameResourcesManager.GetSquareSprite(shopMenuIconData.quality,shopMenuIconData.iconType);
            icon.sprite = shopMenuIconData.iconSprite;

            //LOCKED OR ALREADY OBTAINED?
            pricePanel.SetActive(true);
            //shopMenuIconData.locked = !shopMenuIconData.unlockedByMission;
            if (shopMenuIconData.tickOn)
            {
                tickImage.SetActive(true);
                if (shopMenuIconData.productID.ToString().Contains("Blueprint"))
                {
                    pricePanel.SetActive(false);
                }
            }
            else
            {
                tickImage.SetActive(false);
            }
            lockImage.SetActive(shopMenuIconData.locked);

            if (Application.isPlaying && shopMenuIconData.realShopProductName != ShopProductNames.None && !MasterManager.connected) lockImage.SetActive(true);

            UnityEventTools.RemovePersistentListener(button.onClick, ButtonClicked);
            UnityEventTools.AddPersistentListener(button.onClick, ButtonClicked);
            UpdateInteractable();

            Debug.Log("ShopIcon -> " + gameObject.name + "; icon type = " + shopMenuIconData.iconType);
            if(shopMenuIconData.iconType == ShopIconType.Banner)
            {
                infoButton.gameObject.SetActive(true);
                UnityEventTools.RemovePersistentListener(infoButton.onClick, InfoButtonClicked);
                UnityEventTools.AddPersistentListener(infoButton.onClick, InfoButtonClicked);
            }
            else
            {
                infoButton.gameObject.SetActive(false);
            }

            // --- PRICE TEXT ---
            UpdatePriceText();


            if (shopMenuIconData.iconType == ShopIconType.Banner)
            {
                specialOfferText.SetActive(shopMenuIconData.specialOffer);
            }

            //BANNERS
            UpdateBannerTexts();
        }
    }
#endif

    public void SetupShopIconInGame()
    {
        //Debug.Log("UpdateShopMenuIconsInGame: gameObject = "+ gameObject.name+ "; i = " +i);
        if (!enabled) return;

        //shopMenuIconData.locked = !shopMenuIconData.unlockedByMission;
        //Debug.Log("productID = " + shopMenuIconData.productID + "; reward0 = " + shopMenuIconData.rewards[0].rewardData.rewardType +
        //    "; shopMenuIconData.unlockedByMission = " + shopMenuIconData.unlockedByMission);
        UpdateInteractable();
        if (shopMenuIconData.locked) Lock();
        else lockImage.SetActive(false);

        //Debug.Log(" shopIcon = " + shopMenuIconData.name +"; gameObject = "+ gameObject.name);
        //Debug.Log("consumable = "+ shopMenuIconData.consumable + "; amount = " + MasterManager.GameDataManager.GetReward(shopMenuIconData.productID));
        pricePanel.SetActive(true);
        if (shopMenuIconData.tickOn)
        {
            ActivateTickIcon();
        }
        else tickImage.SetActive(false);
        UpdateButtonText();
        UpdatePriceText();
        UpdateBannerTexts();
    }

    public void Lock()
    {
        lockImage.SetActive(true);
        UpdateInteractable();
    }

    public void ActivateTickIcon()
    {
        tickImage.SetActive(true);
        UpdateInteractable();
        if (shopMenuIconData.productID.ToString().Contains("Blueprint"))
        {
            pricePanel.SetActive(false);
        }
    }

    public void UpdatePriceText()
    {
        TextMeshProUGUI currentText = shopMenuIconData.priceCurrency == RewardType.RealMoney ? priceRealMoneyText : priceText;
        if (shopMenuIconData.priceCurrency != RewardType.RealMoney)
        {
            priceRealMoneyText.transform.parent.gameObject.SetActive(false);
            priceText.transform.parent.gameObject.SetActive(true);

            currencyIcon.gameObject.SetActive(true);
            RewardData currency = MasterManager.GameDataManager.GetRewardData(shopMenuIconData.priceCurrency);
            currencyIcon.sprite = currency.spriteWithoutBackground;
            currencyIcon.SetNativeSize();
            currencyIcon.rectTransform.sizeDelta = new Vector2(currencyIcon.rectTransform.sizeDelta.x * currency.spriteWithoutBackgroundScale,
                currencyIcon.rectTransform.sizeDelta.y * currency.spriteWithoutBackgroundScale);
            currencyIcon.rectTransform.localScale = Vector3.one;
            //currencyIcon.rectTransform.localScale = new Vector3(shopMenuIconData.reward.rewardData.spriteWithoutBackgroundScale, shopMenuIconData.reward.rewardData.spriteWithoutBackgroundScale,
            //    shopMenuIconData.reward.rewardData.spriteWithoutBackgroundScale);
            currentText.text = UnityExtensions.AddThousandsSeparators(shopMenuIconData.GetPriceString());
        }
        else
        {
            priceText.transform.parent.gameObject.SetActive(false);
            priceRealMoneyText.transform.parent.gameObject.SetActive(true);
            //Debug.Log(" Connected = " + MasterManager.connected);

            //if (MasterManager.connected && Application.isPlaying && IAPManager.Instance.IsInitialized())
            //{
            //    priceRealMoneyText.text = IAPManager.Instance.GetLocalizedPriceString(shopMenuIconData.realShopProductName);
            //    //Debug.Log(" PRICE  = " + IAPManager.Instance.GetLocalizedPriceString(shopMenuIconData.realShopProductName));
            //}
            //else
            //{
            //    //Debug.Log("PRICE IS = " + shopMenuIconData.GetPrice().ToString());
            //    priceRealMoneyText.text = shopMenuIconData.GetPrice().ToString() + "€";
            //}

        }
    }

    public void UpdateButtonText()
    {
        if (currentLanguage != I2.Loc.LocalizationManager.CurrentLanguage)
        {
            currentLanguage = I2.Loc.LocalizationManager.CurrentLanguage;
            buttonText.text = GetButtonText();
        }
    }

    public string GetButtonText()
    {
        //Debug.Log(" shopMenuIconData.buttonTextID = " + shopMenuIconData.buttonTextID);
        if (shopMenuIconData.buttonTextID == "Reward0")
        {
            //Debug.Log("Icon Text = " + (shopMenuIconData.rewards[0].GetAmountText() + " " + shopMenuIconData.rewards[0].rewardData.GetRewardCurrency()));
            return shopMenuIconData.rewards[0].GetAmountText() + " " + shopMenuIconData.rewards[0].rewardData.GetRewardCurrency();
        }
        else if (shopMenuIconData.buttonTextID != "")
        {
            return I2.Loc.LocalizationManager.GetTranslation(shopMenuIconData.buttonTextID);
        }
        return "";
    }

    public void ButtonClicked()
    {
        if (MenuTiendaScript.instance.state != ShopMenuState.Opening && MenuTiendaScript.instance.state != ShopMenuState.Closing &&
            MenuTiendaScript.instance.state != ShopMenuState.PurchasingWithRealMoney && !(MenuTiendaScript.instance.fingerDragTotalDist > 0.1f ||
            MenuTiendaScript.instance.stoppedFingerDragInThisFrame))
        {
            //if(GeneralPauseScript.pause.BannerPopupActive && BannerPopupScript.instance.currentBanner == this)
            //{
            //    GeneralPauseScript.pause.StartOpeningLoadingPopup();
            //    if (!shopMenuIconData.Purchase(1))
            //    {
            //        Debug.LogWarning("Tried to purchase but error ocurred");
            //        GeneralPauseScript.pause.ShowPopUp("_ui_tienda_result_fail_error_", GeneralPauseScript.pause.Nothing, PopupMode.Ok);
            //        GeneralPauseScript.pause.StartClosingLoadingPopup();
            //        return;
            //    }
            //}
            //else
            //{
            //    MenuTiendaScript.instance.TryPurchaseItem(shopMenuIconData);
            //    MasterManager.GameResourcesManager.SonidoClick();
            //}
        }
    }

    public void InfoButtonClicked()
    {
        if (MenuTiendaScript.instance.state != ShopMenuState.Opening && MenuTiendaScript.instance.state != ShopMenuState.Closing &&
            MenuTiendaScript.instance.state != ShopMenuState.PurchasingWithRealMoney && !(MenuTiendaScript.instance.fingerDragTotalDist > 0.1f ||
            MenuTiendaScript.instance.stoppedFingerDragInThisFrame))
        {
            Debug.Log("Info Button Clicked");
            string infoTitle = shopMenuIconData.iconType == ShopIconType.Banner ? I2.Loc.LocalizationManager.GetTranslation(textsIDs[0]) : shopMenuIconData.buttonTextID!=""?
                I2.Loc.LocalizationManager.GetTranslation(shopMenuIconData.buttonTextID) : shopMenuIconData.rewards[0].rewardData.GetRewardCurrency();
            string closeText = I2.Loc.LocalizationManager.GetTranslation("_ui_popup_close_");

            List<Reward> rewardsList = new List<Reward>();
            if (shopMenuIconData.productID.ToString().Contains("Pack"))
            {
                for (int i = 0; i < shopMenuIconData.rewards.Length; i++)
                {
                    rewardsList.Add(new Reward(shopMenuIconData.rewards[i].rewardData, shopMenuIconData.rewards[i].amount));
                }
            }
            //GeneralPauseScript.pause.ShowPopUp(shopMenuIconData.infoID, GeneralPauseScript.pause.Nothing, PopupMode.Info, rewardsList, closeText, infoTitle);
            MasterManager.GameResourcesManager.SonidoClick();
        }
    }

    public void UpdateInteractable()
    {
        //Debug.Log(shopMenuIconData.name + ": "+"; object = "+gameObject.name);
        //Debug.Log(button == null ? "null" : "not null");
        button.interactable = shopMenuIconData.interactable;
        //Debug.Log("Icon: " + gameObject.name + ";INTERACTABLE "+ shopMenuIconData.interactable);
    }

    public void UpdateBannerTexts()
    {
        for (int i = 0; i < texts.Length; i++)
        {
            if (i >= textsIDs.Length || textsIDs[i] == "") texts[i].text = "";
            else texts[i].text = I2.Loc.LocalizationManager.GetTranslation(textsIDs[i]);
        }
    }
}
