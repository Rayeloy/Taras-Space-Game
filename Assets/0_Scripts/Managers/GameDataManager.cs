
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
    public string sceneToLoadAfterCinematicVideo = "";
    public int openOptionsOnStart = -1;
    public bool cutsceneReplayOn = false;
    public bool stopChangingTheGameStateFFS = false;
    public bool playCutscene2 = false;


    /// <summary>
    /// Only works in play mode
    /// </summary>
    /// <returns></returns>


   

    [Header("RECOMPENSAS")]
    public RewardData[] allRewardTypes;
    public int[] monedasTier1 = new[] { 5, 15, 15 };
    public int[] monedasTier2 = new[] { 20, 25, 30 };
    public int[] monedasTier3 = new[] { 50, 55, 60 };

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

    public RewardData GetFlaskRewardData(RewardQuality quality)
    {
        switch (quality)
        {
            case RewardQuality.Common:
                return GetRewardData(RewardType.Matraz_Comun);
            case RewardQuality.Rare:
                return GetRewardData(RewardType.Matraz_Raro);
            case RewardQuality.Epic:
                return GetRewardData(RewardType.Matraz_Epico);
            case RewardQuality.Legendary:
                return GetRewardData(RewardType.Matraz_Legendario);
        }
        return null;
    }

    public RewardData GetFoodRewardData(RewardQuality quality)
    {
        switch (quality)
        {
            case RewardQuality.Common:
                return GetRewardData(RewardType.Comida_Sandia);
            case RewardQuality.Rare:
                return GetRewardData(RewardType.Comida_Hamburguesa);
            case RewardQuality.Epic:
                return GetRewardData(RewardType.Comida_Chocolate);
            case RewardQuality.Legendary:
                return GetRewardData(RewardType.Comida_Brocoli);
        }
        return null;
    }

    public int GetFood(RewardQuality quality)
    {
        switch (quality)
        {
            case RewardQuality.Common:
                return GetReward(RewardType.Comida_Sandia);
            case RewardQuality.Rare:
                return GetReward(RewardType.Comida_Hamburguesa);
            case RewardQuality.Epic:
                return GetReward(RewardType.Comida_Chocolate);
            case RewardQuality.Legendary:
                return GetReward(RewardType.Comida_Brocoli);
        }
        return 0;
    }

    public int GetFlaskAmount(RewardQuality quality)
    {
        switch (quality)
        {
            case RewardQuality.Common:
                return GetReward(RewardType.Matraz_Comun);
            case RewardQuality.Rare:
                return GetReward(RewardType.Matraz_Raro);
            case RewardQuality.Epic:
                return GetReward(RewardType.Matraz_Epico);
            case RewardQuality.Legendary:
                return GetReward(RewardType.Matraz_Legendario);
            default:
                return -1;
        }
    }

    public void AddFlask(RewardQuality quality, int amount)
    {
        switch (quality)
        {
            case RewardQuality.Common:
                AddReward(RewardType.Matraz_Comun, amount);
                break;
            case RewardQuality.Rare:
                AddReward(RewardType.Matraz_Raro, amount);
                break;
            case RewardQuality.Epic:
                AddReward(RewardType.Matraz_Epico, amount);
                break;
            case RewardQuality.Legendary:
                AddReward(RewardType.Matraz_Legendario, amount);
                break;
        }
    }

    public void AddCoins(int amount)
    {
        AddReward(RewardType.Monedas, amount);
    }

    public void AddFood(RewardQuality quality, int amount)
    {
        switch (quality)
        {
            case RewardQuality.Common:
                AddReward(RewardType.Comida_Sandia, amount);
                break;
            case RewardQuality.Rare:
                AddReward(RewardType.Comida_Hamburguesa, amount);
                break;
            case RewardQuality.Epic:
                AddReward(RewardType.Comida_Chocolate, amount);
                break;
            case RewardQuality.Legendary:
                AddReward(RewardType.Comida_Brocoli, amount);
                break;

        }
    }

    public bool haveAnyFood
    {
        get
        {
            return GetReward(RewardType.Comida_Sandia) > 0 || GetReward(RewardType.Comida_Hamburguesa) > 0 || GetReward(RewardType.Comida_Chocolate) > 0 || GetReward(RewardType.Comida_Brocoli) > 0;
        }
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

        //Add complete game if we have all 3 p2w improvements
        if (rewardType == RewardType.DineroReal_ComidaIlimitada || rewardType == RewardType.DineroReal_GrandesRecompensas || rewardType == RewardType.DineroReal_VIP)
        {
            if (GetReward(RewardType.DineroReal_ComidaIlimitada) >= 1 && GetReward(RewardType.DineroReal_GrandesRecompensas) >= 1 && GetReward(RewardType.DineroReal_VIP) >= 1)
            {
                AddReward(RewardType.DineroReal_JuegoCompleto, 1);
            }
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
            case RewardType.Gemas:
                break;
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


    #region --- SAVING SYSTEM---
    public void LoadInitialData()
    {
    }

    //Stage
    public void SetCurrentStage(int stage)
    {
        currentStage = stage;
        PlayerPrefs.SetInt("CurrentStage", currentStage);
    }
    public void LoadCurrentStage()
    {
        currentStage = PlayerPrefs.GetInt("CurrentStage", 1);
    }

    //Gems
    public void SaveGems(SavingMode savingMode = SavingMode.CurrentStageOnly, bool throughGestorDeMisiones = true)
    {
        switch (savingMode)
        {
            case SavingMode.CurrentStageOnly:
                break;
            case SavingMode.AllStages:
                for (int i = 1; i < 11; i++)
                {
                }
                break;
        }
    }
    public void LoadGems()
    {
        for (int i = 1; i < 11; i++)
        {
        }
    }
    public void ResetGems(SavingMode savingMode = SavingMode.CurrentStageOnly)
    {
        PlayerPrefs.SetInt("PyrositasTotal", 0);
        switch (savingMode)
        {
            case SavingMode.CurrentStageOnly:

                break;
            case SavingMode.AllStages:
                for (int i = 1; i < 11; i++)
                {
                }
                break;
        }
    }

    

    //REWARDS IN GENERAL
    public void ResetReward(RewardType rewardType)
    {
        if (!rewardType.ToString().Contains("DineroReal"))
        {
            switch (rewardType)
            {
                default:
                    PlayerPrefs.SetInt(rewardType + "Total", 0);
                    break;
            }
        }
        else
        {
            //Debug.Log("Won't reset the reward " + rewardType.ToString() + " because it was bought with real money!");
        }
    }
    void ResetAllRewards()
    {
        for (int i = 1; i < (int)RewardType.Length; i++)
        {
            if(System.Enum.IsDefined(typeof(RewardType), i))
            ResetReward((RewardType)i);
        }
    }

    void ResetShopIcons()
    {
        for (int i = 0; i < allShopIcons.Length; i++)
        {
            allShopIcons[i].ResetData();
        } 
    }

    #endregion

    #region --- RESET DATA ---
    public void ResetData()
    {
        //Debug.LogWarning("RESETING ALL DATA");
        ResetDataGestorDeMisiones();

        Time.timeScale = 1;
        PlayerPrefs.SetInt("TutorialFin", 0);
        PlayerPrefs.SetFloat("TotalTimePlayed", 0);
        PlayerPrefs.SetInt("TotalChallengesCompleted", 0);
        PlayerPrefs.SetInt("TotalScore", 0);
        PlayerPrefs.SetInt("HighestScore", 0);
        PlayerPrefs.SetFloat("HighestCombo", 0);
        PlayerPrefs.SetInt("EnemiesOutrun", 0);
        //PlayerPrefs.SetInt("AnimalesTotal", 0);
        //PlayerPrefs.SetInt("ManchasTotal", 0);
        PlayerPrefs.SetInt("TotalFlips", 0);
        PlayerPrefs.SetInt("TotalPowerUses", 0);
        PlayerPrefs.SetInt("TotalSkillUses", 0);
        PlayerPrefs.SetInt("TotalDeaths", 0);


        ResetGems(SavingMode.AllStages);
        ResetAllRewards();
        SetCurrentStage(0);
        PlayerPrefs.SetInt("MisionActivaMaximaStella", 1);

        //Flujo del juego
        PlayerPrefs.SetInt("MejoraPersonajesExplicada", 0);
        PlayerPrefs.SetInt("TutorialTzukisWorkshopFinished", 0);
        PlayerPrefs.SetInt("WelcomeBannerPopupAfterTutoWorkshopShown", 0);
        PlayerPrefs.SetInt("GliderGameplayTutorialDone", 0);
        PlayerPrefs.SetInt("CraftMachineTutorial1Finished", 0);
        PlayerPrefs.SetInt("CraftMachineTutorial2Finished", 0);
        PlayerPrefs.SetInt("CraftMachineTutorial3Finished", 0);
        PlayerPrefs.SetInt("CraftMachineTutorial4Finished", 0);
        PlayerPrefs.SetInt("Video3Visto", 0);
        PlayerPrefs.SetInt("ConversacionStellaMision1Terminada", 0);
        PlayerPrefs.SetInt("ConversacionStellaMision2Terminada", 0);
        PlayerPrefs.SetInt("ConversacionStellaMision3Terminada", 0);
        PlayerPrefs.SetInt("ConversacionStellaMision3bTerminada", 0);
        PlayerPrefs.SetInt("ConversacionStellaMision4Terminada", 0);
        PlayerPrefs.SetInt("ConversacionStellaMision5Terminada", 0);
        PlayerPrefs.SetInt("ConversacionStellaMision6Terminada", 0);
        PlayerPrefs.SetInt("ConversacionStellaMision7Terminada", 0);
        PlayerPrefs.SetInt("ConversacionStellaMision8Terminada", 0);
        PlayerPrefs.SetInt("ConversacionStellaMision9Terminada", 0);
        PlayerPrefs.SetInt("ConversacionStellaMision9bTerminada", 0);
        PlayerPrefs.SetInt("ConversacionStellaMision10Terminada", 0);
        PlayerPrefs.SetInt("ConversacionStellaMision10bTerminada", 0);
        PlayerPrefs.SetInt("EnemigoCedricDerrotado", 0);
        PlayerPrefs.SetInt("ConversacionMision16Terminada", 0);

        ResetShopIcons();

        MasterManager.MissionsDataManager.ResetMissionsData();
        if(NotificationsManagerScript.instance !=null)NotificationsManagerScript.instance.ResetNotifications();
    }

    public void ResetDataGestorDeMisiones()
    {
        int numeroMisiones = 16;
        int numeroRetos = 3;
        for (int i = 1; i <= numeroMisiones; i++)
        {
            for (int j = 0; j < numeroRetos; j++)
            {
                PlayerPrefs.DeleteKey("Mision" + i + "_Reto" + j);
            }
        }

        PlayerPrefs.DeleteKey("MisionActivaMaximaStella");
        PlayerPrefs.DeleteKey("GemasMaquinaStella");
        PlayerPrefs.DeleteKey("MonedasTotal");
        PlayerPrefs.DeleteKey("PyrositasTotal");
        PlayerPrefs.DeleteKey("AnimalesTotal");
        PlayerPrefs.DeleteKey("ManchasTotal");
        PlayerPrefs.DeleteKey("FragmentosPyrosita");
        PlayerPrefs.DeleteKey("ManchasDesbloqueadasStella");
        PlayerPrefs.DeleteKey("ManchasLimpiadasStella");
        PlayerPrefs.DeleteKey("TutorialFin");
        PlayerPrefs.DeleteKey("Video3Visto");
        PlayerPrefs.DeleteKey("NivelDesbloqueadoMapa");
        PlayerPrefs.DeleteKey("NivelVistoMapa");

        PlayerPrefs.DeleteKey("Explicacion1Mision1");
        PlayerPrefs.DeleteKey("Explicacion2Mision1");
        PlayerPrefs.DeleteKey("Explicacion3Mision1");
        PlayerPrefs.DeleteKey("Explicacion4Mision1");
        PlayerPrefs.DeleteKey("Explicacion1Mision2");
        PlayerPrefs.DeleteKey("Explicacion2Mision2");
        PlayerPrefs.DeleteKey("Explicacion1Mision3");
        PlayerPrefs.DeleteKey("Explicacion2Mision3");
        PlayerPrefs.DeleteKey("Explicacion3Mision3");
        PlayerPrefs.DeleteKey("Explicacion1Mision4");
        PlayerPrefs.DeleteKey("Explicacion2Mision4");
        PlayerPrefs.DeleteKey("Explicacion0Mision5");
        PlayerPrefs.DeleteKey("Explicacion1Mision5");
        PlayerPrefs.DeleteKey("Explicacion1Mision6");
        PlayerPrefs.DeleteKey("Explicacion2Mision6");
        PlayerPrefs.DeleteKey("Explicacion3Mision6");
        PlayerPrefs.DeleteKey("Explicacion4Mision6");
        PlayerPrefs.DeleteKey("Explicacion5Mision6");
        PlayerPrefs.DeleteKey("Explicacion1Mision7");
        PlayerPrefs.DeleteKey("Explicacion2Mision7");

        PlayerPrefs.DeleteKey("Explicacion1Mision8");
        PlayerPrefs.DeleteKey("Explicacion1Mision11");
        PlayerPrefs.DeleteKey("Explicacion2Mision11");
        PlayerPrefs.DeleteKey("Explicacion1Mision13");
        PlayerPrefs.DeleteKey("Explicacion2Mision13");
        PlayerPrefs.DeleteKey("Explicacion1Mision14");
        PlayerPrefs.DeleteKey("Explicacion2Mision14");
        PlayerPrefs.DeleteKey("Explicacion3Mision14");
        PlayerPrefs.DeleteKey("Explicacion1Mision15");
        PlayerPrefs.DeleteKey("Explicacion2Mision15");
        PlayerPrefs.DeleteKey("Explicacion3Mision15");
        PlayerPrefs.DeleteKey("Explicacion4Mision15");
        PlayerPrefs.DeleteKey("Explicacion5Mision15");
        PlayerPrefs.DeleteKey("PrimerEncontronazoCedric");
        PlayerPrefs.DeleteKey("EnemigoCedricDerrotado");
        PlayerPrefs.DeleteKey("ConversacionStellaMision2Terminada");
        PlayerPrefs.DeleteKey("ConversacionStellaMision3Terminada");
        PlayerPrefs.DeleteKey("ConversacionStellaMision4Terminada");
        PlayerPrefs.DeleteKey("ConversacionStellaMision6Terminada");
        PlayerPrefs.DeleteKey("ConversacionStellaMision8Terminada");
        PlayerPrefs.DeleteKey("ConversacionStellaMision9Terminada");
        PlayerPrefs.DeleteKey("ConversacionStellaMision9bTerminada");
        PlayerPrefs.DeleteKey("ConversacionStellaMision10Terminada");
        PlayerPrefs.DeleteKey("AvisoPrimerAd");

        PlayerPrefs.DeleteKey("Evento65");
        PlayerPrefs.DeleteKey("Evento66");
        PlayerPrefs.DeleteKey("Evento67");
        PlayerPrefs.DeleteKey("Evento68");
        PlayerPrefs.DeleteKey("Evento69");

        PlayerPrefs.DeleteKey("PowerUpBombaExplicado");
        PlayerPrefs.DeleteKey("PowerUpBolsaExplicado");
        PlayerPrefs.DeleteKey("PowerUpBolsaMagicaExplicado");
        PlayerPrefs.DeleteKey("PowerUpChileExplicado");
        PlayerPrefs.DeleteKey("PowerUpEscudoExplicado");
    }

    public void RestoreRealMoneyShop()
    {
        //GeneralAds.instance.RestorePurchases();
    }
    #endregion
}
