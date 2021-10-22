using EloyExtensions;
using UnityEngine;

public enum RewardType
{
    RealMoney = -1,
    None = 0,
    Coins = 1,
    Fuel = 3,
    GunAmmo,
    ARAmmo,
    ShotgunAmmo,
    Length
}

public enum RewardQuality
{
    None = 0,
    Common = 1,
    Rare = 2,
    Epic = 3,
    Legendary = 4
}

[CreateAssetMenu(fileName = "New Reward", menuName = "Reward Type")]
public class RewardData : ScriptableObject
{
    public RewardType rewardType = RewardType.None;
    public RewardQuality rewardQuality = RewardQuality.None;
    public string rewardNameID;
    public Sprite sprite;
    public float iconScale = 1;
    public Sprite spriteWithoutBackground;
    public float spriteWithoutBackgroundScale = 1;
    public float value = 1; //For example: Matraz Raro = 10, Matraz Epico = 20;
    [Tooltip("For Tzuki's Workshop")]
    public GameObject materialPrefab;//For Tzuki's Workshop

    public string GetRewardCurrency()
    {
        switch (rewardType)
        {
            default:
                return I2.Loc.LocalizationManager.GetTranslation(rewardNameID);
            case RewardType.RealMoney:
                return "€";
        }
    }
}

[System.Serializable]
public class Reward
{
    public RewardData rewardData;
    public int amount;
    public bool collected = false;//solo se usa en recompensas, no en compras

    public Reward(RewardData _rewardData = null, int _amount = 0)
    {
        rewardData = _rewardData;
        amount = _amount;
        collected = false;
    }

    public void RandomizeAmount(RewardQuality rewardQuality)
    {
        switch (rewardData.rewardType)
        {
            default:
                amount = Random.Range(1, 50);
                break;
        }
    }

    public void GetReward(bool realMoneyPurchase = false, int times=1)
    {
        collected = true;
        //Debug.Log("GET REWARD " + rewardData.rewardType + ";  = " + System.Enum.GetName(typeof(RewardType), rewardData.rewardType) + "; name = " + rewardData.name);
        int totalAmount = amount * times;
        switch (rewardData.rewardType)
        {
            default:
                MasterManager.GameDataManager.AddReward(rewardData.rewardType, totalAmount);
                break;
        }
    }

    public string GetAmountText()
    {
        return UnityExtensions.AddThousandsSeparators(amount.ToString());
    }
}
