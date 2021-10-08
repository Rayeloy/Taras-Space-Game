#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
#endif
using UnityEngine;
using EloyExtensions;

public enum ShopIconType
{
    None,
    Small,
    Medium,
    Banner,
    Medium_Blueprint
}

[CreateAssetMenu(fileName = "New Shop Menu Icon Data", menuName = "Interface/Shop Menu Icon Data")]
public class ShopMenuIconData : ScriptableObject
{
    public ShopProductNames realShopProductName = ShopProductNames.None;
    public ShopIconType iconType = ShopIconType.None;
    public RewardType productID = RewardType.None;
    public RewardQuality quality;
    [Tooltip("Leave this empty if you want to show the reward amount instead of the product name!")]
    public string translationID;
    public string infoID;
    public Sprite iconSprite;
    public float price = 3.99f;
    public RewardType priceCurrency = RewardType.RealMoney;
    public float specialOfferPrice = 1.99f;
    public string buttonTextID = "Reward0"; //"_ui_tienda_ovillos_"
    public bool locked = true;
    public bool specialOffer = false;
    public Reward[] rewards;
    public bool consumable = true;
    public int unlockedStage = 1;
    public int unlockedMission = 1;
    public bool interactable
    {
        get
        {
            //Debug.Log("productID = "+ productID+"; reward0 = "+rewards[0].rewardData.rewardType+"; interactable-> locked = " + locked+"; tickOn = " + tickOn);
            return !locked && !tickOn;
        }
    }
    public bool tickOn
    {
        get
        {
            //if (productID == RewardType.DineroReal_PackBienvenida) Debug.Log("consumable = "+ consumable +
            //    "; MasterManager.GameDataManager.GetReward(productID) = " + MasterManager.GameDataManager.GetReward(productID));
            return (!consumable && MasterManager.GameDataManager.GetReward(productID) >= 1);
        }
    }
    public float UnitCost()
    {
        return price/rewards[0].amount;
    }

    public void ResetData()
    {
        locked = false;
        for (int i = 0; i < rewards.Length; i++)
        {
            rewards[i].collected = false;
        }
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }

    //public bool unlockedByMission
    //{
    //    get
    //    {
    //        if (GestorDeMisionesScript.gestorMisiones != null)
    //            return GestorDeMisionesScript.gestorMisiones.nivelEnCurso > unlockedStage || (GestorDeMisionesScript.gestorMisiones.nivelEnCurso == unlockedStage &&
    //                GestorDeMisionesScript.gestorMisiones.misionActiva >= unlockedMission);
    //        else return false;
    //    }
    //}

    public bool canPurchase(int amount)
    {
            bool result = true;
        float totalPrice = amount * GetPrice();
                switch (priceCurrency)
                {
                    case RewardType.RealMoney:
                        //TO DO: In App Purchase
                        if (MasterManager.connected)
                        {
                    return false;
                            //return IAPManager.Instance.IsInitialized();
                        }
                        else
                        {
                            return false;
                        }
                    default:
                        if (totalPrice > MasterManager.GameDataManager.GetReward(priceCurrency))
                        {
                            result = false;
                        }
                        break;
            }

            return result;
    }

    public bool Purchase(int amount)
    {
        bool result = canPurchase(amount);
        if (canPurchase(amount))
        {
                switch (priceCurrency)
                {
                    case RewardType.RealMoney:
                        //TO DO: In App Purchase
                        //GeneralAds.instance.ComprarItem(realShopProductName);
                        return true;
                    default:
                        MasterManager.GameDataManager.AddReward(priceCurrency, (int)-GetPrice() * amount);
                        result = true;
                        break;
            }
        }
        return result;
    }

    public void Lock()
    {
        locked = true;
        MenuTiendaScript.instance.LockShopMenuIcon(this);
    }

    public void Unlock()
    {
        locked = false;
        MenuTiendaScript.instance.UnlockShopMenuIcon(this);
    }

    public void ActivateIconTick()
    {
        Debug.Log(" consumable = " + consumable+"; MasterManager.GameDataManager.GetReward(" + productID+") = " + MasterManager.GameDataManager.GetReward(productID));
        if (tickOn)
        {
            MenuTiendaScript.instance.ActivateIconTick(this);
        }
    }

    public float GetPrice()
    {
        return specialOffer ? specialOfferPrice : price;
    }

    public string GetPriceString()
    {
        if (priceCurrency == RewardType.RealMoney)
        {
            return IAPManager.Instance.GetLocalizedPriceString(realShopProductName);
        }
        else
        {
            return ""+(int)GetPrice();
        }
    }

    public void GetRewards(bool realMoneyPurchase=false, int times=1)
    {
        if (productID != RewardType.None && !consumable)
        {
            MasterManager.GameDataManager.AddReward(productID, 1);
        }
        for (int i = 0; i < rewards.Length; i++)
        {
            rewards[i].GetReward(realMoneyPurchase, times);
        }
    }

    public string GetProductName()
    {
        if (translationID != "") return I2.Loc.LocalizationManager.GetTranslation(translationID);
        string result = "";
        for (int i = 0; i < rewards.Length; i++)
        {
            if ((consumable || rewards[i].amount>1)) result += UnityExtensions.AddThousandsSeparators(rewards[i].amount.ToString()) + " ";
            result +=  rewards[i].rewardData.GetRewardCurrency();
            if (i < rewards.Length - 2) result += ", ";
            else if(i<rewards.Length -1)result += " "+I2.Loc.LocalizationManager.GetTranslation("_and_")+" ";
        }
        return result;
    }

#if UNITY_EDITOR
    public void OnValidate()
    {
        ShopMenuIcon[] icons = FindObjectsOfType<ShopMenuIcon>();
        for (int i = 0; i < icons.Length; i++)
        {
            if (PrefabStageUtility.GetPrefabStage(icons[i].gameObject) != null && icons[i].shopMenuIconData == this)
                icons[i].SetUpShopIconEditor();
        }
    }
#endif
}
