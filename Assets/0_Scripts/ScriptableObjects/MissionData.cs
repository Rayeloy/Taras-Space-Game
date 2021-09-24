
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[CreateAssetMenu(fileName = "New Mission", menuName = "Misiones/Mission")]
public class MissionData : ScriptableObject
{
    public bool copyFileName = false;
    public string missionName;
    public int stage = 1;//0 --> Tuto 1; -2 --> Tuto 2; 1 --> Stella Forest; 2 --> Frank Station; 3 --> 
    public int missionIndex = 0;
    public List<ChallengeData> challenges;
    public bool missionCompleted = false;
    public bool isCompleted
    {
        get
        {
            bool result = true;
            for (int i = 0; i < challenges.Count; i++)
            {
                challenges[i].UpdateChallenge(this);
                if (!challenges[i].completed) return false;
            }
            return result;
        }
    }
    bool delay = false;
    float delayTime = 0;
    float delayMaxTime = 0;
    public List<bool> foldouts;

    public void AddChallenge(ChallengeData challenge)
    {
        challenges.Add(challenge);
        if (foldouts == null) foldouts = new List<bool>();
        foldouts.Add(true);
    }
#if UNITY_EDITOR
    public void RemoveChallenge(ChallengeData challenge)
    {
        Debug.Log("Removing challenge: " + challenge.challengeName);
        for (int i = 0; i < challenges.Count; i++)
        {
            if(challenges[i] == challenge)
            {
                challenges.RemoveAt(i);
                foldouts.RemoveAt(i);

                var path = AssetDatabase.GetAssetPath(challenge);
                AssetDatabase.DeleteAsset(path);
            }
        }
    }
#endif

    public void CompleteMission()
    {
        ////Esto da por completada la misión pero no notifica nada para luchar contra Cedric
        //if (GestorDeMisionesScript.gestorMisiones.nivelEnCurso == 1 && GestorDeMisionesScript.gestorMisiones.misionActiva == 7 && !GestorDeMisionesScript.gestorMisiones.enemigoCedricDerrotado)
        //{
        //    missionCompleted = true;
        //    return;
        //}
        ////Esto se llama al vencer a Cedric. Pongo false la variable para que pueda notificar todo
        //if (GestorDeMisionesScript.gestorMisiones.nivelEnCurso == 1 && GestorDeMisionesScript.gestorMisiones.misionActiva == 7 && GestorDeMisionesScript.gestorMisiones.enemigoCedricDerrotado)
        //{
        //    missionCompleted = false;
        //}
        if (!missionCompleted)
        {
            Debug.Log("MISSION COMPLETE!");
            missionCompleted = true;
            //GestorDeMisionesScript.gestorMisiones.misionRecienTerminada = true;
            //GeneralPauseScript.pause.NotifyMissionCompleteWhilePlaying();
            //GeneralVars.instance.LanzarEvento("M_COMP_" + GestorDeMisionesScript.gestorMisiones.misionActiva);
            //GeneralPauseScript.pause.progressMission = true;
            //if (GestorDeMisionesScript.gestorMisiones.nivelEnCurso == 1 && missionIndex == 9) GeneralPauseScript.pause.tipoDePartida = TipoDePartida.juegoLibre;
        }
    }

    public void CompleteMissionAfterDelay()
    {
        if (!delay)
        {
            delay = true;
            delayMaxTime = 1;
            if (stage == 1 && missionIndex == 1) delayMaxTime = 2;
            else delayMaxTime = 1;
            delayTime = 0;
        }
    }

    public void ProcessMissionCompleteDelay()
    {
        if (delay)
        {
            delayTime += Time.deltaTime;
            if(delayTime >= delayMaxTime)
            {
                CompleteMission();
                delay = false;
            }
        }
    }

    public void LoadChallengesData()
    {
        for (int i = 0; i < challenges.Count; i++)
        {
            challenges[i].LoadChallengeData();
        }
        missionCompleted = isCompleted;
    }

    public void ResetChallengesData()
    {
        for (int i = 0; i < challenges.Count; i++)
        {
            challenges[i].ResetChallenge();
        }
        missionCompleted = false;
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }

    public void ResetChallengesOnComboEnd()
    {
        for (int i = 0; i < challenges.Count; i++)
        {
            //Debug.Log("Reset Challenges con COmbo End Mission " + missionIndex+"; stage = "+stage+ "; challenges[i].challengeIndex = "+ challenges[i].challengeIndex+
            //    "; challenges[i].situation = "+ challenges[i].situation + "; challenges[i].completed = " + challenges[i].completed);
            if (challenges[i].situation != ChallengeSituationType.Combo || (challenges[i].situation == ChallengeSituationType.Combo && challenges[i].completed)) continue;

            challenges[i].ResetChallenge();
        }
    }

    public void ResetChallengesOnGameEnd()
    {
        for (int i = 0; i < challenges.Count; i++)
        {
            if (challenges[i].situation != ChallengeSituationType.Game || (challenges[i].situation == ChallengeSituationType.Game && challenges[i].completed)) return;

            challenges[i].ResetChallenge();
        }
    }

    public void UpdateChallenges()
    {
        for (int i = 0; i < challenges.Count; i++)
        {
            challenges[i].UpdateChallenge(this);
        }
    }

    public bool CheckErrors()
    {
        bool result = false;
        if(missionName == "")
        {
            Debug.LogError("The mission " + name + " has no missionName!");
            result = true;
        }
        if (challenges.Count > 3)
        {
            Debug.LogError("The mission " + missionName + " has more than 3 challenges! This can't be");
            result = true;
        }
        if(challenges.Count==0)
        {
            Debug.LogError("The mission " + missionName + " has 0 challenges! This can't be");
            result = true;
        }

        bool challengesResult = false;
        for (int i = 0; i < challenges.Count; i++)
        {
            if (challenges[i].CheckErrors(this)) challengesResult = true;
        }

        result = challengesResult || result;
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
        missionName = name;
    }
}
