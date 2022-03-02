using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardItem : MonoBehaviour
{
    public RewardType typeReward;
    public int quantityItem;

    public void GiveItem()
    {
        MasterManager.GameDataManager.AddReward(typeReward, quantityItem);

    }
}
