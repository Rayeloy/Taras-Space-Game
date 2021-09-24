
using System.Collections.Generic;
using UnityEngine;
using EloyExtensions;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum ChallengeSituationType
{
    None,
    Stage,
    Combo,
    Game
}

public enum ChallengeType
{
    None,
    Collect,
    Distance,
    Score,
    Pirouettes,
    ComboMultiplier,
    Animals,
    Enemies,
    Obstacles,
    Cleaning,
    Custom,
}

public enum ChallengeCollectType
{
    None,
    Coins,
    Gems,
}

public enum ChallengeObstacleType
{
    None,
    Mushroom,
    Stain,//Spot, Mancha
}

public enum ChallengeCleaningType
{
    None,
    Amount,
    Percentage,
}

public enum ChallengeEnemiesType
{
    None,
    Amount,
    Distance
}

public enum PirouetteType
{
    None,
    Slide,
    Chasm,
    Geyser,
    River,
    Flip,
    BounceOnObstacle,
    Roll,
    Fly,//Tzuki
    Glide,
    BreakRock,
}

[CreateAssetMenu(fileName = "New Challenge", menuName = "Misiones/Challenge")]
public class ChallengeData : ScriptableObject
{
    public bool copyFileName = false;
    public string challengeName;
    public ChallengeSituationType situation = ChallengeSituationType.None;
    public ChallengeType type = ChallengeType.None;
    public MissionData parentMission;

    public void SetParentMission(MissionData mission)
    {
        if (parentMission != null && parentMission != mission) Debug.LogError("There is more than one parent mission to this challenge; First parent mission: " + parentMission.missionName +
            "; Second parent mission: " + mission.missionName + "; Challenge: " + challengeName);

        parentMission = mission;
    }

    public float amountRequired = 0;
    public float currentAmount = 0;
    public bool completed = false;
    [HelpBox("Do not touch in editor", HelpBoxMessageType.Warning)]
    public int challengeIndex = 0;

    public string customChallengeTextID = "";

    [Header("--- ONLY FOR COLLECT ---")]
    //type collect
    public ChallengeCollectType collectType = ChallengeCollectType.None;

    [Header("--- ONLY FOR OBSTACLE ---")]
    //type obstacle
    public ChallengeObstacleType obstacleType = ChallengeObstacleType.None;

    [Header("--- ONLY FOR CLEANING ---")]
    //type cleaning
    public ChallengeCleaningType cleaningType = ChallengeCleaningType.None;
    [Range(0, 3)]
    public float percentage = 1;
    public float currentPercentage = 0;

    [Header("--- ONLY FOR PIROUETTES ---")]
    //type pirouettes
    public List<Pirouette> pirouettes;
    public bool inOrder = true;
    public List<bool> pirouettesFoldout;
    public float minHeight = float.MinValue;
    public float maxHeight = float.MaxValue;
    public bool allowOtherPirouettesInTheMiddle = false;

    [Header("--- ONLY FOR DISTANCE ---")]
    //type Distance
    float initialDist = 0;
    //public PersonajeEnJuego distanceCharacter = PersonajeEnJuego.None;

    [Header("--- ONLY FOR ENEMIES ---")]
    //type Enemies
    public ChallengeEnemiesType enemiesType = ChallengeEnemiesType.None;


    public void CompleteChallenge()
    {
        //if (GeneralPauseScript.pause.tipoDePartida == TipoDePartida.juegoLibre) return;
        //if (PlayerControllerScript.controller.muerto || GeneralPauseScript.pause.misionCompletadaJugando || GestorDeMisionesScript.gestorMisiones.misionRecienTerminada || GeneralPauseScript.pause.estadoJuego != GameState.jugando) return;

        //if (!completed)
        //{
        //    completed = true;
        //    PlayerPrefs.SetInt(challengeName + "Completed", 1);
           
        //    GestorDeMisionesScript.gestorMisiones.CompleteChallenge(parentMission.missionIndex, challengeIndex);

        //    //Debug.Log("Parent Mission is completed? " + (parentMission.isCompleted));
        //    if (parentMission.isCompleted) parentMission.CompleteMission();
        //}
    }

    public void ResetChallenge()
    {
        completed = false;
        PlayerPrefs.SetInt(challengeName + "Completed", 0);
        currentAmount = 0;
        if (type == ChallengeType.Pirouettes) ResetPirouettes();
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }

    public void LoadChallengeData()
    {
        completed = PlayerPrefs.GetInt(challengeName + "Completed", 0) == 0 ? false : true;
        currentAmount = PlayerPrefs.GetFloat(challengeName + "CurrentAmount", 0);
    }

    public void UpdateChallenge(MissionData mission)
    {
        if (completed) return;

        switch (situation)
        {
            case ChallengeSituationType.Combo:
               
                break;
            case ChallengeSituationType.Game:
                break;
            case ChallengeSituationType.Stage:
                switch (type)
                {
                    case ChallengeType.Collect:
                        break;
                    case ChallengeType.Cleaning:
                        break;
                    case ChallengeType.Animals:
                        break;
                }
                break;
        }
    }

    public void UpdateGameScore(float score)
    {
        if (completed) return;
        if (type != ChallengeType.Score) return;
        if (situation != ChallengeSituationType.Game) return;

        currentAmount = score;
        if (currentAmount >= amountRequired) CompleteChallenge();
    }

    public void UpdateComboScore(float score)
    {
        if (completed) return;
        if (type != ChallengeType.Score) return;
        if (situation != ChallengeSituationType.Combo) return;

        currentAmount = score;
        if (currentAmount >= amountRequired) CompleteChallenge();
    }

    public void JumpOverObstacle(ChallengeObstacleType _obstacleType)
    {
        if (completed) return;
        if (type != ChallengeType.Obstacles) return;

        if (obstacleType != _obstacleType) return;
        currentAmount++;

        if (currentAmount >= amountRequired)
        {
            CompleteChallenge();
        }
    }

    public void GetComboMultiplier(int amount)
    {
        if (completed) return;
        if (type != ChallengeType.ComboMultiplier) return;

        currentAmount = amount;
        if (currentAmount >= amountRequired) CompleteChallenge();
    }

    public void CollectCoin(int amount)
    {
        if (completed) return;
        if (type != ChallengeType.Collect) return;
        if (collectType != ChallengeCollectType.Coins) return;

        currentAmount += amount;
        if (currentAmount >= amountRequired) CompleteChallenge();
    }

    public void CollectGem(int amount)
    {
        if (completed) return;
        if (type != ChallengeType.Collect) return;
        if (collectType != ChallengeCollectType.Gems) return;

        currentAmount += amount;
        if (currentAmount >= amountRequired) CompleteChallenge();
    }

    public void SaveAnimal()
    {
        if (completed) return;
        if (type != ChallengeType.Animals || situation == ChallengeSituationType.Stage) return;

        currentAmount++;
        if (currentAmount >= amountRequired) CompleteChallenge();
    }

    #region --- PIROUETTES ---
    public void DoPirouette(PirouetteType pirouetteType)
    {
        if (completed) return;
        if (type != ChallengeType.Pirouettes) return;
        //if(PlayerControllerScript.controller.currentHeightOverFloor<minHeight || PlayerControllerScript.controller.currentHeightOverFloor > maxHeight)
        //{
        //    ResetPirouettes();
        //    return;
        //}
        //Debug.Log("Do pirouette of type " + pirouetteType);

        if (inOrder)
        {
            bool pirouetteFound = false;
            for (int i = 0; i < pirouettes.Count && !pirouetteFound; i++)
            {
                if (pirouettes[i].done) continue;
                else
                {
                    if (pirouettes[i].type == pirouetteType)
                    {
                        //Debug.Log("In order: Pirouette of type " + pirouetteType + " found and done");
                        pirouettes[i].done = true;
                        pirouetteFound = true;
                    }
                    else
                    {
                        //if((pirouetteType == PirouetteType.BreakRock && LastCompletedPirouette().type == PirouetteType.MushroomJump) || 
                        //    (pirouetteType == PirouetteType.MushroomJump && LastCompletedPirouette().type == PirouetteType.BreakRock))
                        if(allowOtherPirouettesInTheMiddle)
                        {
                            pirouetteFound = true;
                        }
                        else
                        {
                            ResetPirouettes();
                            pirouetteFound = true;
                        }
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < pirouettes.Count; i++)
            {
                if (pirouettes[i].type == pirouetteType && !pirouettes[i].done)
                {
                    //Debug.Log("Any order: Pirouette of type " + pirouetteType + " found and done");
                    pirouettes[i].done = true;
                    break;
                }
            }
        }
        if (CheckPirouettesCompleted()) CompleteChallenge();
    }

    void ResetPirouettes()
    {
        for (int i = 0; i < pirouettes.Count; i++)
        {
            pirouettes[i].done = false;
        }
    }

    bool HasPirouette(PirouetteType pirouetteType)
    {
        for (int i = 0; i < pirouettes.Count; i++)
        {
            if (pirouettes[i].type == pirouetteType) return true;
        }
        return false;
    }

    Pirouette LastCompletedPirouette()
    {
        for (int i = 0; i < pirouettes.Count; i++)
        {
            if (!pirouettes[i].done)
            {
                if (i - 1 >= 0) return pirouettes[i - 1];
                else return pirouettes[0];
            }
        }
        return pirouettes[pirouettes.Count - 1];
    }

    bool CheckPirouettesCompleted()
    {
        for (int i = 0; i < pirouettes.Count; i++)
        {
            if (!pirouettes[i].done) return false;
        }
        return true;
    }

    public void AddPirouette()
    {
        pirouettes.Add(new Pirouette(PirouetteType.None));
        pirouettesFoldout.Add(true);
    }

    public void RemovePirouette(Pirouette pirouette)
    {
        for (int i = 0; i < pirouettes.Count; i++)
        {
            if(pirouettes[i] == pirouette)
            {
                pirouettes.RemoveAt(i);
                pirouettesFoldout.RemoveAt(i);
            }
        }
    }

    #endregion

    public string GetChallengeText(bool withStrikeThrough=true)
    {
        string result = "";

        if (customChallengeTextID!= "")
        {
            result = I2.Loc.LocalizationManager.GetTranslation(customChallengeTextID);
            //Debug.Log("result = "+ result + "; customChallengeTextID = " + customChallengeTextID);
        }
        else
        {
            switch (type)
            {
                case ChallengeType.Collect:
                    string collectable = collectType == ChallengeCollectType.Coins ? I2.Loc.LocalizationManager.GetTranslation("_ui_coins_") :
                        collectType == ChallengeCollectType.Gems ? I2.Loc.LocalizationManager.GetTranslation("_ui_gems_") : "";
                    result += I2.Loc.LocalizationManager.GetTranslation("_challenge_text_collect_") + " " + collectable;//Collect &amount coins
                    break;
                case ChallengeType.Distance:
                    result += I2.Loc.LocalizationManager.GetTranslation("_challenge_text_distance_")+" ";
                    break;
                case ChallengeType.Score:
                    result += I2.Loc.LocalizationManager.GetTranslation("_challenge_text_score_");
                    break;
                case ChallengeType.Pirouettes:
                    string minHeightString = minHeight > 0 ? I2.Loc.LocalizationManager.GetTranslation("_challenge_text_pirouettes_min_height_") + " " : "";
                    string maxHeightString = maxHeight > 0 && maxHeight != float.MaxValue ? I2.Loc.LocalizationManager.GetTranslation("_challenge_text_pirouettes_max_height_") + " " : "";
                    if (minHeightString != "" & maxHeightString != "") minHeightString += I2.Loc.LocalizationManager.GetTranslation("_and_") + " ";
                    string order = "";
                    if (pirouettes.Count > 1)
                    {
                        order = inOrder ? I2.Loc.LocalizationManager.GetTranslation("_challenge_text_pirouettes_in_order_") + " " :
                        I2.Loc.LocalizationManager.GetTranslation("_challenge_text_pirouettes_no_order_") + " ";
                        result += I2.Loc.LocalizationManager.GetTranslation("_challenge_text_pirouettes_");
                        result += " " + order + minHeightString + maxHeightString + ": ";
                    }
                    //            result += situation == ChallengeSituationType.Combo ? I2.Loc.LocalizationManager.GetTranslation("_challenge_text_combo_") : situation == ChallengeSituationType.Game ?
                    //I2.Loc.LocalizationManager.GetTranslation("_challenge_text_game_") : "";
                    for (int i = 0; i < pirouettes.Count; i++)
                    {
                        switch (pirouettes[i].type)
                        {
                            case PirouetteType.Chasm:
                                result += I2.Loc.LocalizationManager.GetTranslation("_challenge_text_pirouettes_chasm_");
                                break;
                            case PirouetteType.Flip:
                                result += I2.Loc.LocalizationManager.GetTranslation("_challenge_text_pirouettes_flip_");
                                break;
                            case PirouetteType.Geyser:
                                result += I2.Loc.LocalizationManager.GetTranslation("_challenge_text_pirouettes_geyser_");
                                break;
                            case PirouetteType.BounceOnObstacle:
                                result += I2.Loc.LocalizationManager.GetTranslation("_challenge_text_pirouettes_mushroom_jump_");
                                break;
                            case PirouetteType.Roll:
                                result += I2.Loc.LocalizationManager.GetTranslation("_challenge_text_pirouettes_roll_");
                                break;
                            case PirouetteType.Slide:
                                result += I2.Loc.LocalizationManager.GetTranslation("_challenge_text_pirouettes_slide_");
                                break;
                            case PirouetteType.River:
                                result += I2.Loc.LocalizationManager.GetTranslation("_challenge_text_pirouettes_river_");
                                break;
                            case PirouetteType.BreakRock:
                                result += I2.Loc.LocalizationManager.GetTranslation("_challenge_text_pirouettes_break_");
                                break;
                            case PirouetteType.Glide:
                                result += I2.Loc.LocalizationManager.GetTranslation("_challenge_text_pirouettes_glide_");
                                break;
                            case PirouetteType.Fly:
                                result += I2.Loc.LocalizationManager.GetTranslation("_challenge_text_pirouettes_fly_");
                                break;
                            //case PirouetteType.CleanStain:
                            //    result += I2.Loc.LocalizationManager.GetTranslation("_challenge_text_pirouettes_clean_stain_");
                            //    break;
                        }
                        if (pirouettes.Count > 1)
                        {
                            //result += completed || pirouettes[i].done ? "[X]" : "[ ]";
                            if (i < pirouettes.Count - 2) result += ", ";
                            else if (i == pirouettes.Count - 2) result += " " + I2.Loc.LocalizationManager.GetTranslation("_and_") + " ";
                        }
                    }
                    if (pirouettes.Count == 1)
                    {
                        UnityExtensions.CapitalizeFirstLetter(result);
                        result += minHeightString + maxHeightString;
                    }
                    break;
                case ChallengeType.ComboMultiplier:
                    result += I2.Loc.LocalizationManager.GetTranslation("_challenge_text_combo_counter_");
                    break;
                case ChallengeType.Animals:
                    result += I2.Loc.LocalizationManager.GetTranslation("_challenge_text_animals_");
                    break;
                case ChallengeType.Enemies:
                    result += enemiesType == ChallengeEnemiesType.Amount ? I2.Loc.LocalizationManager.GetTranslation("_challenge_text_enemies_") :
                        I2.Loc.LocalizationManager.GetTranslation("_challenge_text_enemies_distance_");
                    break;
                case ChallengeType.Obstacles:
                    string obstacle = obstacleType == ChallengeObstacleType.Mushroom ? I2.Loc.LocalizationManager.GetTranslation("_mushrooms_") :
                        obstacleType == ChallengeObstacleType.Stain ? I2.Loc.LocalizationManager.GetTranslation("_manchas_") : "";
                    result += I2.Loc.LocalizationManager.GetTranslation("_challenge_text_obstacle_") + " " + obstacle;//Collect &amount coins
                    break;
                case ChallengeType.Cleaning:
                    if (cleaningType == ChallengeCleaningType.Amount)
                        result += I2.Loc.LocalizationManager.GetTranslation("_challenge_text_cleaning_");
                    else if (cleaningType == ChallengeCleaningType.Percentage)
                    {
                        result += I2.Loc.LocalizationManager.GetTranslation("_challenge_text_cleaning_percentage_");
                        result = result.Replace("&percentage", (percentage * 100).ToString() + "%");
                    }
                    break;
            }
            if (type != ChallengeType.Pirouettes)
            {
                result += " ";
                switch (situation)
                {
                    case ChallengeSituationType.Combo:
                        result += I2.Loc.LocalizationManager.GetTranslation("_challenge_text_combo_");
                        break;
                    case ChallengeSituationType.Game:
                        result += I2.Loc.LocalizationManager.GetTranslation("_challenge_text_game_");
                        break;
                    case ChallengeSituationType.Stage:
                        //if (MasterManager.GameDataManager.atributosNivel.GetAtributosDeNivel(parentMission.stage).GetStageName() != "")
                        //{
                        //    result += I2.Loc.LocalizationManager.GetTranslation("_challenge_text_in_") + " ";//
                        //    result += MasterManager.GameDataManager.atributosNivel.GetAtributosDeNivel(parentMission.stage).GetStageName();
                        //}
                        break;
                }
            }
            result += ".";
            if (!(type == ChallengeType.Pirouettes || (type == ChallengeType.Cleaning && cleaningType == ChallengeCleaningType.Percentage)))
                result += "(" + (completed ? ((int)amountRequired).ToString() : ((int)currentAmount).ToString()) + "/" + ((int)amountRequired).ToString() + ")";
            if (type == ChallengeType.Cleaning && cleaningType == ChallengeCleaningType.Percentage) result += "(" + (completed ? (percentage * 100).ToString() : currentPercentage.ToString()) + "%/" + (percentage * 100).ToString() + "%)";

            result = result.Replace("&amount", Mathf.FloorToInt(amountRequired).ToString());
            result = result.Replace("&minHeight", minHeight.ToString());
            result = result.Replace("&maxHeight", maxHeight.ToString());
            //if (distanceCharacter != PersonajeEnJuego.None) result = result.Replace("&character", MasterManager.GameDataManager.GetCharacter(distanceCharacter).GetCharacterName());

            //result = "[" + (completed ? "X" : " ") + "] " + result;
        }

        if (withStrikeThrough && completed)
        {
            result = "<s>" + result + "</s>";
        }
        return result;
    }

    public bool CheckErrors(MissionData mission)
    {
        bool result = false;
        if (challengeName == "")
        {
            Debug.LogError("The challenge " + name + " in mission " + mission.missionName + " has no name defined");
            result = true;
        }

        if (situation == ChallengeSituationType.None || type == ChallengeType.None)
        {
            Debug.LogError("The challenge " + name + " in mission " + mission.missionName + " has no situation or type defined");
            result = true;
        }

        switch (situation)
        {
            case ChallengeSituationType.Combo:
                if (type == ChallengeType.Cleaning || type == ChallengeType.Enemies || (type == ChallengeType.Collect && collectType == ChallengeCollectType.Gems))
                {
                    Debug.LogError("The challenge " + challengeName + " in mission " + mission.missionName + " has the challenge type " + type +
                        (type == ChallengeType.Collect ? " with collect type " + collectType : "") + " can't be used in situation " + situation);
                    result = true;
                }
                break;
            case ChallengeSituationType.Game:
                if (type == ChallengeType.Cleaning || type == ChallengeType.ComboMultiplier || (type == ChallengeType.Collect && collectType == ChallengeCollectType.Gems))
                {
                    Debug.LogError("The challenge " + challengeName + " in mission " + mission.missionName + " has the challenge type " + type +
                        (type == ChallengeType.Collect ? " with collect type " + collectType : "") + " can't be used in situation " + situation);
                    result = true;
                }
                break;
            case ChallengeSituationType.Stage:
                if (!(type == ChallengeType.Cleaning || type == ChallengeType.Animals ||(type == ChallengeType.Collect && collectType == ChallengeCollectType.Gems)))
                {
                    Debug.LogError("The challenge " + challengeName + " in mission " + mission.missionName + " has the challenge type " + type +
                        (type == ChallengeType.Collect ? " with collect type " + collectType : "") + " can't be used in situation " + situation);
                    result = true;
                }
                break;
        }

        switch (type)
        {
            case ChallengeType.Pirouettes:
                if (pirouettes.Count == 0)
                {
                    Debug.LogError("The challenge " + challengeName + " in mission " + mission.missionName + " has the challenge type " + type + " but the pirouettes list is empty!");
                    result = true;
                }
                break;
            case ChallengeType.Cleaning:
                if (cleaningType == ChallengeCleaningType.None)
                {
                    Debug.LogError("The challenge " + challengeName + " in mission " + mission.missionName + " has the challenge type " + type + " but the Cleaning type is None!");
                    result = true;
                }
                break;
            case ChallengeType.Enemies:
                if (enemiesType == ChallengeEnemiesType.None)
                {
                    Debug.LogError("The challenge " + challengeName + " in mission " + mission.missionName + " has the challenge type " + type + " but the enemies type is None!");
                    result = true;
                }
                break;
            case ChallengeType.Collect:
                if (collectType == ChallengeCollectType.None)
                {
                    Debug.LogError("The challenge " + challengeName + " in mission " + mission.missionName + " has the challenge type " + type + " but the collect type is None!");
                    result = true;
                }
                break;
        }

        return result;
    }

    void OnValidate()
    {
        if (copyFileName)
        {
            CopyFileName();
        }
    }

    public void CopyFileName()
    {
        copyFileName = false;
        challengeName = name;
    }

}

[System.Serializable]
public class Pirouette
{
    public PirouetteType type = PirouetteType.None;
    public bool done = false;

    public Pirouette(PirouetteType _type)
    {
        done = false;
        type = _type;
    }
}

