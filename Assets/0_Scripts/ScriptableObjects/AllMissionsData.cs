using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New All Missions Data", menuName = "All Missions Data")]
public class AllMissionsData : ScriptableObject
{
    public StageMissionsData tutorialMissions;
    public List<StageMissionsData> stagesMissions;

    public AllMissionsData()
    {
        stagesMissions = new List<StageMissionsData>();
    }
}

[System.Serializable]
public class StageMissionsData
{
    public int stage = 0;
    public List<MissionData> missions;
    public List<bool> foldouts;

    public StageMissionsData(int _stage)
    {
        stage = _stage;
        missions = new List<MissionData>();
        foldouts = new List<bool>();
    }

    public void AddMission(MissionData _mission)
    {
        missions.Add(_mission);
        if (foldouts == null) foldouts = new List<bool>();
        foldouts.Add(true);
    }
#if UNITY_EDITOR
    public void RemoveMission(MissionData _mission)
    {
        for (int i = 0; i < missions.Count; i++)
        {
            if (missions[i] == _mission)
            {
                for (int j = 0;_mission.challenges.Count>0;)
                {
                    _mission.RemoveChallenge(_mission.challenges[j]);
                }
                missions.RemoveAt(i);
                foldouts.RemoveAt(i);
                var path = AssetDatabase.GetAssetPath(_mission);
                AssetDatabase.DeleteAsset(path);
                break;
            }
        }
    }
#endif

    public string GetStageName()
    {
        switch (stage)
        {
            case -2:
            case 0:
                return "Tutorial";
            case 1:
                return "Stella Forest";
            case 2:
                return "Frank Station";
            case 3:
                return "CandanRock Mountain";
            case 4:
                return "Ashtun Summit";
            case 5:
                return "Ko'Yesha Marsh";
            case 6:
                return "Afer Kalon Islands";
            case 7:
                return "Crystal Caves";
            case 8:
                return "Pem'ba Volcano";
            case 9:
                return "Denabar Desert";
            case 10:
                return "Iruna City";
            default:
                return "error";
        }
    }
    public string GetStageAcronym()
    {
        switch (stage)
        {
            case -2:
            case 0:
                return "Tutorial";
            case 1:
                return "SF";
            case 2:
                return "FS";
            case 3:
                return "CM";
            case 4:
                return "AS";
            case 5:
                return "KYM";
            case 6:
                return "AKI";
            case 7:
                return "CC";
            case 8:
                return "PV";
            case 9:
                return "DD";
            case 10:
                return "IC";
            default:
                return "error";
        }
    }
}