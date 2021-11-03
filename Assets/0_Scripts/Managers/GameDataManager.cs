
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public enum SavingMode
{
    None,
    CurrentStageOnly,
    AllStages
}

[CreateAssetMenu(menuName = "Managers/GameDataManager")]
public class GameDataManager : ScriptableObject
{
    public int currentStage = 1;

   

    [Header("RECOMPENSAS")]
    public RewardData[] allRewardTypes;

    public RewardData GetRewardData(RewardType rewardType)
    {
        for (int i = 0; i < allRewardTypes.Length; i++)
        {
            if (rewardType == allRewardTypes[i].rewardType) return allRewardTypes[i];
        }
        return null;
    }

    public RewardData GetRandomRewardData()
    {
        int random = Random.Range(0, allRewardTypes.Length);
        return allRewardTypes[random];
    }

    public void AddReward(RewardType rewardType, int amount, bool withRealMoney=false)
    {

        Debug.Log("ADD REWARD: rewardType = " + rewardType + "; PlayerPrefs ID = " + rewardType + "Total" + "; Amount = " + PlayerPrefs.GetInt(rewardType + "Total", 0));
        switch (rewardType)
        {
            case RewardType.None:
            case RewardType.Length:
                return;
            default:
                if (!CanGetReward(rewardType)) return;

                PlayerPrefs.SetInt(rewardType + "Total", Mathf.Clamp(PlayerPrefs.GetInt(rewardType + "Total", 0) + amount,0,int.MaxValue));
                break;
        }
    }

    public void AddReward(Reward reward)
    {
        AddReward(reward.rewardData.rewardType, reward.amount);
    }

    public int GetReward(RewardType rewardType)
    {
        switch (rewardType)
        {
            case RewardType.None:
            case RewardType.Length:
                return -1;
            default:
                //Debug.Log("GetReward: rewardType = " + rewardType + "; PlayerPrefs ID = "+ rewardType + "Total" + "; Amount = " + PlayerPrefs.GetInt(rewardType + "Total", 0));
                return PlayerPrefs.GetInt(rewardType + "Total", 0);
        }
    }

    public void SetReward(RewardType rewardType, int amount)
    {
        switch (rewardType)
        {
            case RewardType.None:
            case RewardType.Length:
                return;
            default:
                PlayerPrefs.SetInt(rewardType + "Total", amount);
                break;
        }
    }

    public bool CanGetReward(RewardType rewardType)
    {
        if (rewardType.ToString().Contains("DineroReal"))
        {
            ShopMenuIconData shopMenuForThisReward = GetShopMenuIconData(rewardType);
            if (shopMenuForThisReward == null) return true;
            else if (GetReward(shopMenuForThisReward.productID) >= 1)
            {
                Debug.LogError("We already have 1 of " + rewardType + ", and 1 is the limit!");
                return false;
            }
        }
        else if (rewardType.ToString().Contains("Blueprint"))
        {
            ShopMenuIconData shopMenuForThisReward = GetShopMenuIconData(rewardType);
            if (shopMenuForThisReward == null) return true;
            else if (GetReward(shopMenuForThisReward.productID) >= 1)
            {
                Debug.LogError("We already have 1 of " + rewardType + ", and 1 is the limit!");
                return false;
            }
        }
        return true;
    }

    [Header("--- TIENDA ---")]
    public ShopMenuIconData[] allShopIcons;

    public ShopMenuIconData GetShopMenuIconData(ShopProductNames productName)
    {
        for (int i = 0; i < allShopIcons.Length; i++)
        {
            if (allShopIcons[i].realShopProductName == productName) return allShopIcons[i];
        }
        Debug.LogError("Could not find the shopMenuIconData for the product " + productName);
        return null;
    }

    public ShopMenuIconData GetShopMenuIconData(RewardType productID)
    {
        for (int i = 0; i < allShopIcons.Length; i++)
        {
            if (allShopIcons[i].productID == productID)
            {
                return allShopIcons[i];
            }
        }
        Debug.LogWarning("Could not find the shopMenuIconData for the product " + productID);
        return null;
    }

    [Header("--- WEAPONS ---")]
    public GunData[] allGuns;
    public float bulletMaxLifeTime = 6;
    public Sprite noWeaponLogo;


    #region --- SAVING SYSTEM---
    public void SaveWeaponsState()
    {
        for (int i = 0; i < allGuns.Length; i++)
        {
            PlayerPrefs.SetInt(allGuns[i].gunType + "BulletsInClip", allGuns[i].maxClipSize);
        }
    }
    #endregion

    #region --- RESET DATA ---
    public void ResetData()
    {
       
    }
    #endregion
}
