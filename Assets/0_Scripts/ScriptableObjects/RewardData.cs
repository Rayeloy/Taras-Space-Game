using EloyExtensions;
using UnityEngine;

public enum RewardType
{
    RealMoney = -1,
    None = 0,
    Monedas = 1,
    Gemas = 2,
    Matraz_Comun = 3,
    Matraz_Raro = 4,
    Matraz_Epico = 5,
    Matraz_Legendario = 6,
    Comida_Sandia = 7,
    Comida_Hamburguesa = 8,
    Comida_Chocolate = 9,
    Comida_Brocoli = 10,
    Ovillo_Lana,
    Ovillo_Oro,
    Ovillo_Antiguo,
    Tijeras,
    Tijeras_Refinadas,
    Tijeras_Anticuario,
    PowerUp_Megafono,
    PowerUp_Lombrices_KoYesha,
    PowerUp_Semillas_Ocion,
    PowerUp_Bayas_Bosque_Stella,
    PowerUp_Hojas_Arbol_Kauri,
    PowerUp_Escudo,
    PowerUp_Silbato,
    PowerUp_ChileSuperpicante,
    PowerUp_Bomba_Oro,
    PowerUp_Bolsa_Tela,
    PowerUp_Bolsa_Magica,
    MateriaPrima_Tela = 30,
    MateriaPrima_Madera = 31,
    MateriaPrima_Hierro = 32,
    MateriaPrima_Componentes,
    Skin,
    Recolor,
    Decoracion_MonedaColgante,
    Decoracion_GuaridaArdillas,
    Decoracion_Luces,
    Decoracion_Hamaca,
    Decoracion_Telescopio,
    //Taller_DetectorMetales,
    //Taller_Eleonor,
    //Taller_ImanMonedas,
    //Taller_ImanPyrosita,
    //Taller_MaquinaFufi,
    //Taller_MochilaLimpieza,
    //Taller_PicoMagico,
    //FALTAN MÁS

    //--- IMPORTANTISIMO!!!! --- Poner "DineroReal" delante de los nombres de los rewards de dinero real
    DineroReal_JuegoCompleto,
    DineroReal_VIP,
    DineroReal_GrandesRecompensas,
    DineroReal_ComidaIlimitada,
    DineroReal_PackBienvenida,
    //--- IMPORTANTISIMO!!!! --- Poner "Blueprint" delante de los nombres de los rewards de blueprint
    Blueprint_Planear,
    Blueprint_Maquina,
    Blueprint_DetectorOro,
    Blueprint_DobleSalto,
    Blueprint_PicoPyrositico,
    Blueprint_ImanMonedas,
    Blueprint_ImanPyrositas,
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
            case RewardType.Monedas:
                return I2.Loc.LocalizationManager.GetTranslation("_ui_coins_");
            case RewardType.Gemas:
                return I2.Loc.LocalizationManager.GetTranslation("_ui_gems_");
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
            case RewardType.Monedas:
                switch (rewardQuality)
                {
                    case RewardQuality.Common:
                        int indice = Random.Range(0, MasterManager.GameDataManager.monedasTier1.Length);
                        amount = MasterManager.GameDataManager.monedasTier1[indice];
                        break;
                    case RewardQuality.Rare:
                        indice = Random.Range(0, MasterManager.GameDataManager.monedasTier1.Length);
                        amount = MasterManager.GameDataManager.monedasTier2[indice];
                        break;
                    case RewardQuality.Epic:
                        indice = Random.Range(0, MasterManager.GameDataManager.monedasTier1.Length);
                        amount = MasterManager.GameDataManager.monedasTier3[indice];
                        break;
                }
                break;
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
            case RewardType.Monedas:
                MasterManager.GameResourcesManager.SonidoMoneda();
                //GestorDeMisionesScript.gestorMisiones.variables[13].valorVariable += amount;
                MasterManager.GameDataManager.AddCoins(amount * times);
                break;
            case RewardType.Matraz_Comun:
                MasterManager.GameDataManager.AddFlask(RewardQuality.Common, totalAmount);
                break;
            case RewardType.Matraz_Raro:
                MasterManager.GameDataManager.AddFlask(RewardQuality.Rare, totalAmount);
                break;
            case RewardType.Matraz_Epico:
                MasterManager.GameDataManager.AddFlask(RewardQuality.Epic, totalAmount);
                break;
            case RewardType.Matraz_Legendario:
                MasterManager.GameDataManager.AddFlask(RewardQuality.Legendary, totalAmount);
                break;
            case RewardType.Gemas:
                MasterManager.GameDataManager.AddReward(rewardData.rewardType, totalAmount, realMoneyPurchase);//Saving done inside addGems already
                break;
            case RewardType.Comida_Sandia:
                MasterManager.GameDataManager.AddFood(RewardQuality.Common, totalAmount);
                break;
            case RewardType.Comida_Hamburguesa:
                MasterManager.GameDataManager.AddFood(RewardQuality.Rare, totalAmount);
                break;
            case RewardType.Comida_Chocolate:
                MasterManager.GameDataManager.AddFood(RewardQuality.Epic, totalAmount);
                break;
            case RewardType.Comida_Brocoli:
                MasterManager.GameDataManager.AddFood(RewardQuality.Legendary, totalAmount);
                break;
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
