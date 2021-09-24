
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Managers/MissionsManager")]
public class MissionsDataManager : ScriptableObject
{
    public bool renameAllMissionsAndChallenges = false;
    public int currentMaxStage = 2;
    public AllMissionsData allMissionsData;

    public List<MissionData> GetMissions(int stage)
    {
        switch (stage)
        {
            case 0:
            case -2:
                return allMissionsData.tutorialMissions.missions;
            case 1: return allMissionsData.stagesMissions[0].missions;
            case 2: return allMissionsData.stagesMissions[1].missions;
        }
        return null;
    }

    public MissionData currentMission
    {
        get
        {
            //Debug.Log("currentMission: " + GestorDeMisionesScript.gestorMisiones.nivelEnCurso);
            //switch (GestorDeMisionesScript.gestorMisiones.nivelEnCurso)
            //{
            //    case 0:
            //    case -2:
            //        return allMissionsData.tutorialMissions.missions[0];
            //    case 1:
            //        //Debug.Log("Mission index = " + (PlayerPrefs.GetInt("MisionActivaMaximaStella", 1) - 1));
            //        return GetMissions(1)[PlayerPrefs.GetInt("MisionActivaMaximaStella", 1) - 1];
            //    case 2:
            //        return GetMissions(2)[PlayerPrefs.GetInt("CurrentMaxMission" + MasterManager.GameDataManager.atributosNivel.GetAtributosDeNivel(2).siglas, 1) - 1];
            //}
            return null;
        }
    }

    //public List<MissionData> currentStageMissions
    //{
    //    get
    //    {
    //        //switch (GestorDeMisionesScript.gestorMisiones.nivelEnCurso)
    //        //{
    //        //    case -2:
    //        //    case 0:
    //        //        return allMissionsData.tutorialMissions.missions;
    //        //    default:
    //        //        return GetMissions(GestorDeMisionesScript.gestorMisiones.nivelEnCurso);
    //        //}
    //    }
    //}

    //public bool isCurrentStageFinished
    //{
    //    get
    //    {
    //        return currentStageMissions[currentStageMissions.Count - 1].missionCompleted;
    //    }
    //}

    public void LoadMissionsData()
    {
        for (int i = 0; i < allMissionsData.tutorialMissions.missions.Count; i++)
        {
            allMissionsData.tutorialMissions.missions[i].LoadChallengesData();
        }

        for (int i = 0; i < allMissionsData.stagesMissions.Count; i++)
        {
            for (int j = 0; j < allMissionsData.stagesMissions[i].missions.Count; j++)
            {
                allMissionsData.stagesMissions[i].missions[j].LoadChallengesData();
            }
        }
    }

    public void ResetMissionsData()
    {
        for (int i = 0; i < allMissionsData.tutorialMissions.missions.Count; i++)
        {
            allMissionsData.tutorialMissions.missions[i].ResetChallengesData();
        }

        for (int i = 0; i < allMissionsData.stagesMissions.Count; i++)
        {
            for (int j = 0; j < allMissionsData.stagesMissions[i].missions.Count; j++)
            {
                allMissionsData.stagesMissions[i].missions[j].ResetChallengesData();
            }
        }
    }

    public bool CheckMissionErrors()
    {
        bool result = false;

        for (int i = 0; i < allMissionsData.tutorialMissions.missions.Count; i++)
        {
            if (allMissionsData.tutorialMissions.missions[i].CheckErrors()) result = true;
        }

        for (int i = 0; i < allMissionsData.stagesMissions.Count; i++)
        {
            for (int j = 0; j < allMissionsData.stagesMissions[i].missions.Count; j++)
            {
                if (allMissionsData.stagesMissions[i].missions[j].CheckErrors()) result = true;
            }
        }
        return result;
    }

    public void NextMission()
    {
        //if (GestorDeMisionesScript.gestorMisiones.nivelEnCurso == 1)
        //{
        //    if (GestorDeMisionesScript.gestorMisiones.misionActiva == 7 && !GestorDeMisionesScript.gestorMisiones.enemigoCedricDerrotado) return;
        //}
        //if (currentMission.missionIndex + 1 >= currentStageMissions.Count)
        //{
        //        if (GestorDeMisionesScript.gestorMisiones.nivelEnCurso == 1)
        //        {
        //            GestorDeMisionesScript.gestorMisiones.nivelStellaTerminado = true;
        //        }
        //        //TO DO:
        //        //PASAR AL SIGUIENTE NIVEL
        //    return;
        //}


        switch (currentMission.stage)
        {
            case -2:
            case 0:
                break;
            case 1:
                if (currentMission.missionIndex +2 > PlayerPrefs.GetInt("MisionActivaMaximaStella", 1)) PlayerPrefs.SetInt("MisionActivaMaximaStella", currentMission.missionIndex + 2);
                break;
            default:
                //if (currentMission.missionIndex +2 > PlayerPrefs.GetInt("CurrentMaxMission" + MasterManager.GameDataManager.atributosNivel.GetAtributosDeNivel(currentMission.stage).siglas, 1))
                //    PlayerPrefs.SetInt("CurrentMaxMission" + MasterManager.GameDataManager.atributosNivel.GetAtributosDeNivel(currentMission.stage).siglas, currentMission.missionIndex + 2);
                break;
        }

        //if (GestorDeMisionesScript.gestorMisiones.nivelEnCurso == 1)
        //{
        //    if (GestorDeMisionesScript.gestorMisiones.misionActiva == 7) GestorDeMisionesScript.gestorMisiones.mision7terminadaSinVerVideo = true;
        //}

        //if (GestorDeMisionesScript.gestorMisiones.misionActiva == 4) GeneralPauseScript.pause.lanzarPopRatingAlReiniciar = true;
        //GestorDeMisionesScript.gestorMisiones.misionActiva++;
        
        //Debug.LogError("Nueva mision: " + GestorDeMisionesScript.gestorMisiones.misionActiva);

        //GeneralPauseScript.pause.GenerarTextoRetosIniciales();

        //Con cada misión aumentamos el número de eventos para que salga el popup de rating
        RateGame.Instance.IncreaseCustomEvents();
    }

    public void ProcessCompleteMissionDelayed()
    {
        //if (GestorDeMisionesScript.gestorMisiones.nivelEnCurso == -2 || GestorDeMisionesScript.gestorMisiones.nivelEnCurso >= 0)
        //{
        //    for (int i = 0; i < GetMissions(GestorDeMisionesScript.gestorMisiones.nivelEnCurso).Count; i++)
        //    {
        //        GetMissions(GestorDeMisionesScript.gestorMisiones.nivelEnCurso)[i].ProcessMissionCompleteDelay();
        //    }
        //}
    }

    public void EndGame()
    {
        currentMission.ResetChallengesOnGameEnd();
    }

    public void EndCombo()
    {
        //if (GestorDeMisionesScript.gestorMisiones.nivelEnCurso == -2 || GestorDeMisionesScript.gestorMisiones.nivelEnCurso >= 0)
        //{
        //    currentMission.ResetChallengesOnComboEnd();
        //}
    }

    public void DoPirouette(PirouetteType pirouetteType)
    {
        for (int i = 0; i < currentMission.challenges.Count; i++)
        {
            currentMission.challenges[i].DoPirouette(pirouetteType);
        }
    }

    public void UpdateGameScoreChallenges(float score)
    {
        //if (GestorDeMisionesScript.gestorMisiones.nivelEnCurso == -2 || GestorDeMisionesScript.gestorMisiones.nivelEnCurso >= 0)
        //{
        //    for (int i = 0; i < currentMission.challenges.Count; i++)
        //    {
        //        currentMission.challenges[i].UpdateGameScore(score);
        //    }
        //}
    }

    public void UpdateComboScoreChallenges(float score)
    {
        //if (GestorDeMisionesScript.gestorMisiones.nivelEnCurso == -2 || GestorDeMisionesScript.gestorMisiones.nivelEnCurso >= 0)
        //{
        //    for (int i = 0; i < currentMission.challenges.Count; i++)
        //    {
        //        currentMission.challenges[i].UpdateComboScore(score);
        //    }
        //}
    }

    public void JumpOverObstacle(ChallengeObstacleType obstacleType)
    {
        for (int i = 0; i < currentMission.challenges.Count; i++)
        {
            currentMission.challenges[i].JumpOverObstacle(obstacleType);
        }
    }

    public void GetComboMultiplier(int amount)
    {
        for (int i = 0; i < currentMission.challenges.Count; i++)
        {
            currentMission.challenges[i].GetComboMultiplier(amount);
        }
    }

    public void CollectCoin(int amount)
    {
        for (int i = 0; i < currentMission.challenges.Count; i++)
        {
            currentMission.challenges[i].CollectCoin(amount);
        }
    }

    public void CollectGem(int amount)
    {
        for (int i = 0; i < currentMission.challenges.Count; i++)
        {
            currentMission.challenges[i].CollectGem(amount);
        }
    }

    public void SaveAnimal()
    {
        for (int i = 0; i < currentMission.challenges.Count; i++)
        {
            currentMission.challenges[i].SaveAnimal();
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (renameAllMissionsAndChallenges)
        {
            renameAllMissionsAndChallenges = false;
            for (int i = 0; i < GetMissions(1).Count; i++)
            {
                //MasterManager.RenameAsset(GetMissions(1)[i], MasterManager.GameDataManager.atributosNivel.GetAtributosDeNivel(1).siglas + "_Mission" + (i + 1), false);
                GetMissions(1)[i].CopyFileName();
                for (int j = 0; j < GetMissions(1)[i].challenges.Count; j++)
                {
                    MasterManager.RenameAsset(GetMissions(1)[i].challenges[j], GetMissions(1)[i].name + "_Challenge" + (j + 1), false);
                    GetMissions(1)[i].challenges[j].CopyFileName();
                }
            }
            //AssetDatabase.SaveAssets();
        }
    }
#endif
}
