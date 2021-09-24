using UnityEngine;

[ExecuteAlways]
public class CheatsManager : MonoBehaviour
{ 
    public bool addReward = false;
    public RewardType rewardToAdd;
    public int rewardAmountToAdd = 10;

    public bool resetData = false;
    // Update is called once per frame

    private void Awake()
    {
        resetData = addReward = false;
    }

    private void OnEnable()
    {
        resetData = addReward = false;
    }
    void Update()
    {

        if (addReward)
        {
            addReward = false;
            MasterManager.GameDataManager.AddReward(rewardToAdd, rewardAmountToAdd);
        }

        if (resetData)
        {
            resetData = false;
            MasterManager.GameDataManager.ResetData();
        }

    }

    void Nothing()
    {

    }
}
