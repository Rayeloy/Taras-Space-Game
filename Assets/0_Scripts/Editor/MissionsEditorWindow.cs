using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
[ExecuteAlways]
public class MissionsEditorWindow : EditorWindow
{
    public AllMissionsData allMissionsData;
    bool tutorialFoldout = false;
    bool[] stagesFoldOuts = new bool[10];
    Vector2 scrollPosition = Vector2.zero;

    GUISkin _skinChallenge = null;
    GUISkin _skinMission = null;
    GUISkin _skinStage = null;

    GUISkin _original = null;

    static float challengeGUIPosX = 150f;
    static float pirouetteGUIPosX = 210f;

    void SetUp()
    {
        stagesFoldOuts = new bool[10];
        for (int i = 0; i < stagesFoldOuts.Length; i++)
        {
            stagesFoldOuts[i] = false;
        }
    }

    [MenuItem("Tzuki's Tools/Missions Editor")]
    public static void ShowWindow()
    {
        GetWindow<MissionsEditorWindow>("Missions Editor");
    }

    private void OnEnable()
    {
        _skinChallenge = AssetDatabase.LoadAssetAtPath<GUISkin>(string.Format("Assets" + Path.DirectorySeparatorChar + "Editor/CustomSkinChallenge.guiskin"));
        _skinMission = AssetDatabase.LoadAssetAtPath<GUISkin>(string.Format("Assets" + Path.DirectorySeparatorChar + "Editor/CustomSkinMission.guiskin"));
        _skinStage = AssetDatabase.LoadAssetAtPath<GUISkin>(string.Format("Assets" + Path.DirectorySeparatorChar + "Editor/CustomSkinStage.guiskin"));
        if (allMissionsData == null) allMissionsData = (AllMissionsData)Resources.Load("AllMissionsData", typeof(AllMissionsData));
        if (allMissionsData.stagesMissions == null || allMissionsData.stagesMissions.Count == 0)
        {
            allMissionsData.stagesMissions = new List<StageMissionsData>();
            for (int i = 1; i < 11; i++)
            {
                StageMissionsData newStageMissions = new StageMissionsData(i);
                allMissionsData.stagesMissions.Add(newStageMissions);
            }
        }
    }

    void OnGUI()
    {
        //Apply the gui skin
        _original = GUI.skin;
        GUI.skin = _skinStage;

        if (stagesFoldOuts == null || stagesFoldOuts.Length == 0)
        {
            SetUp();
        }
        allMissionsData = EditorGUILayout.ObjectField("All Missions Data Save File", allMissionsData, typeof(AllMissionsData), false) as AllMissionsData;
        if (allMissionsData == null) return;
        //GUILayout.Label("Fill the levels data of every character", EditorStyles.boldLabel);
        if (GUILayout.Button("Save Data"))
        {
            AssetDatabase.SaveAssets();
        }
        if (GUILayout.Button("Set Initial Missions Values"))
        {
            SetInitialMissionsValues();
        }

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        //TUTORIAL
        tutorialFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(tutorialFoldout, allMissionsData.tutorialMissions.GetStageName());
        if (tutorialFoldout)
        {
            DrawStageMissions(allMissionsData.tutorialMissions);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        for (int i = 0; i < stagesFoldOuts.Length; i++)//STAGES
        {
            //Debug.Log("Foldout " + i + "; stagesFoldOuts.Length = "+ stagesFoldOuts.Length + "; allMissionsData.stagesMissions.Count = " + allMissionsData.stagesMissions.Count);
            stagesFoldOuts[i] = EditorGUILayout.BeginFoldoutHeaderGroup(stagesFoldOuts[i], allMissionsData.stagesMissions[i].GetStageName());//STAGES
            if (stagesFoldOuts[i])//Stage Opened
            {
                DrawStageMissions(allMissionsData.stagesMissions[i]);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        GUILayout.EndScrollView();
        EditorUtility.SetDirty(allMissionsData);
    }

    void AddNewMission(StageMissionsData stageMissionsData)
    {
        MissionData newMissionData = ScriptableObject.CreateInstance<MissionData>();
        string name = stageMissionsData.GetStageAcronym() + "_Mission" + (stageMissionsData.missions.Count + 1);
        newMissionData.missionName = name;
        newMissionData.foldouts = new List<bool>();
        newMissionData.challenges = new List<ChallengeData>();
        newMissionData.stage = stageMissionsData.stage;
        newMissionData.missionIndex = stageMissionsData.missions.Count;
        AssetDatabase.CreateAsset(newMissionData, "Assets/_Metas/Missions/Automatically Generated Missions/" + name + ".asset");

        stageMissionsData.AddMission(newMissionData);
    }

    void AddNewChallenge(StageMissionsData stageMissionsData, MissionData mission, int missionIndex)
    {
        ChallengeData newChallengeData = ScriptableObject.CreateInstance<ChallengeData>();
        string name = stageMissionsData.GetStageAcronym() + "_Mission" + (missionIndex) + "_Challenge" + (mission.challenges.Count + 1);

        newChallengeData.challengeName = name;
        newChallengeData.parentMission = mission;
        newChallengeData.challengeIndex = (mission.challenges.Count);
        newChallengeData.pirouettes = new List<Pirouette>();
        newChallengeData.pirouettesFoldout = new List<bool>();

        AssetDatabase.CreateAsset(newChallengeData, "Assets/_Metas/Missions/Automatically Generated Challenges/" + name + ".asset");
        //AssetDatabase.SaveAssets();

        mission.AddChallenge(newChallengeData);
    }

    void DrawStageMissions(StageMissionsData stageMissionsData)
    {
        EditorGUI.indentLevel++;
        GUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.Space();
        Rect currentRect;
        for (int j = 0; j < stageMissionsData.foldouts.Count; j++)//MISSIONS
        {
            //dibujamos las misiones ya añadidas
            EditorGUILayout.BeginHorizontal();
            currentRect = EditorGUILayout.GetControlRect();
            currentRect.x += 5;
            currentRect.width = 40;
            stageMissionsData.foldouts[j] =
                EditorGUI.Toggle(currentRect, stageMissionsData.foldouts[j]);
            currentRect.x += 15;
            currentRect.width = 280;
            EditorGUI.LabelField(currentRect, stageMissionsData.missions[j].missionName);
            currentRect.x += 300;
            currentRect.width = 130;
            if (GUI.Button(currentRect, "Delete Mission"))
            {
                stageMissionsData.RemoveMission(stageMissionsData.missions[j]);
            }
            EditorGUILayout.EndHorizontal();

            #region --- MISSION DATA ---
            if (stageMissionsData.foldouts[j])//Mission opened
            {
                EditorUtility.SetDirty(stageMissionsData.missions[j]);
                GUI.skin = _skinMission;
                EditorGUI.indentLevel++;
                GUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.Space();

                GUILayout.BeginHorizontal();
                currentRect = EditorGUILayout.GetControlRect();
                EditorGUI.LabelField(currentRect, "--- MISSION ---");
                GUILayout.EndHorizontal();

                //Mission Name
                EditorGUILayout.BeginHorizontal();
                currentRect = EditorGUILayout.GetControlRect();
                EditorGUI.LabelField(currentRect, "Mission name:");
                currentRect.x += 100;
                currentRect.width = 250;
                stageMissionsData.missions[j].missionName =
                    EditorGUI.TextArea(currentRect, stageMissionsData.missions[j].missionName);
                EditorGUILayout.EndHorizontal();

                //Mission Completed
                EditorGUILayout.BeginHorizontal();
                currentRect = EditorGUILayout.GetControlRect();
                EditorGUI.LabelField(currentRect, "Mission Completed:");
                currentRect.x += 120;
                currentRect.width = 0;
                stageMissionsData.missions[j].missionCompleted = EditorGUI.Toggle(currentRect, stageMissionsData.missions[j].missionCompleted);
                EditorGUILayout.EndHorizontal();

                //Debug.Log(" allMissionsData = "+(allMissionsData ==null)+ "; allMissionsData.stagesMissions[i] = " + (allMissionsData.stagesMissions[i] == null)+
                //    "; allMissionsData.stagesMissions[i].missions[j] = " + (allMissionsData.stagesMissions[i].missions[j] == null)+
                //    "; allMissionsData.stagesMissions[i].missions[j].foldouts = " + (allMissionsData.stagesMissions[i].missions[j].foldouts == null));
                for (int k = 0; k < stageMissionsData.missions[j].foldouts.Count; k++)//CHALLENGES
                {
                    EditorGUILayout.BeginHorizontal();
                    currentRect = EditorGUILayout.GetControlRect();
                    currentRect.x += 5;
                    currentRect.width = 80;
                    stageMissionsData.missions[j].foldouts[k] =
                        EditorGUI.Toggle(currentRect, stageMissionsData.missions[j].foldouts[k]);
                    currentRect.x += 15;
                    currentRect.width = 280;
                    EditorGUI.LabelField(currentRect, stageMissionsData.missions[j].challenges[k].challengeName);
                    currentRect.x += 300;
                    currentRect.width = 130;
                    if (GUI.Button(currentRect, "Delete Challenge"))
                    {
                        stageMissionsData.missions[j].RemoveChallenge(stageMissionsData.missions[j].challenges[k]);
                    }
                    EditorGUILayout.EndHorizontal();
                    if (stageMissionsData.missions[j].foldouts[k])//Challenge Opened
                    {
                        DrawChallengeInfo(stageMissionsData.missions[j].challenges[k]);
                    }

                }
                currentRect = EditorGUILayout.GetControlRect();
                currentRect.width = 140;
                currentRect.x += 40;

                if (GUI.Button(currentRect, "Add new challenge"))
                {
                    AddNewChallenge(stageMissionsData, stageMissionsData.missions[j], j + 1);
                }
                EditorGUILayout.Space();
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
                GUI.skin = _skinStage;
            }
            #endregion
        }
        currentRect = EditorGUILayout.GetControlRect();
        currentRect.width = 200;
        currentRect.x += 27;

        if (GUI.Button(currentRect, "Add new mission"))
        {
            AddNewMission(stageMissionsData);
        }
        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();
        EditorGUI.indentLevel--;
    }

    void DrawChallengeInfo(ChallengeData challenge)
    {
        EditorUtility.SetDirty(challenge);
        GUI.skin = _skinChallenge;

        EditorGUI.indentLevel++;
        GUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.Space();

        GUILayout.BeginHorizontal();
        Rect currentRect = EditorGUILayout.GetControlRect();
        EditorGUI.LabelField(currentRect, "--- CHALLENGE ---");
        GUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        currentRect = EditorGUILayout.GetControlRect();
        EditorGUI.LabelField(currentRect, "Challenge name:");
        currentRect.x += challengeGUIPosX;
        currentRect.width = 280;
        challenge.challengeName =
            EditorGUI.TextArea(currentRect, challenge.challengeName);
        EditorGUILayout.EndHorizontal();

        //Challenge Completed
        EditorGUILayout.BeginHorizontal();
        currentRect = EditorGUILayout.GetControlRect();
        EditorGUI.LabelField(currentRect, "Challenge Completed:");
        currentRect.x += challengeGUIPosX;
        currentRect.width = 80;
        challenge.completed = EditorGUI.Toggle(currentRect, challenge.completed);
        currentRect.x += 70;
        currentRect.width = 130;
        //if (GUI.Button(currentRect, "Complete Challenge") && GeneralPauseScript.pause!=null && GeneralPauseScript.pause.estadoJuego == GameState.jugando)
        //{
        //    challenge.CompleteChallenge();
        //}
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        currentRect = EditorGUILayout.GetControlRect();
        EditorGUI.LabelField(currentRect, "Custom challenge text ID:");
        currentRect.x += challengeGUIPosX;
        currentRect.width = 280;
        challenge.customChallengeTextID =
            EditorGUI.TextArea(currentRect, challenge.customChallengeTextID);
        EditorGUILayout.EndHorizontal();

        //Challenge Situation
        EditorGUILayout.BeginHorizontal();
        currentRect = EditorGUILayout.GetControlRect();
        EditorGUI.LabelField(currentRect, "Situation:");
        currentRect.x += challengeGUIPosX;
        currentRect.width = 200;
        challenge.situation = (ChallengeSituationType)EditorGUI.EnumPopup(currentRect, challenge.situation);
        EditorGUILayout.EndHorizontal();

        //Challenge Type
        EditorGUILayout.BeginHorizontal();
        currentRect = EditorGUILayout.GetControlRect();
        EditorGUI.LabelField(currentRect, "Type:");
        currentRect.x += challengeGUIPosX;
        currentRect.width = 200;
        challenge.type = (ChallengeType)EditorGUI.EnumPopup(currentRect, challenge.type);
        EditorGUILayout.EndHorizontal();

        #region --- ERROR CONTROL ---
        if (challenge.type != ChallengeType.None)
        {
            switch (challenge.situation)
            {
                case ChallengeSituationType.Combo:
                    if (challenge.type == ChallengeType.Cleaning || challenge.type == ChallengeType.Enemies ||
                        (challenge.type == ChallengeType.Collect && (challenge.collectType == ChallengeCollectType.Gems || challenge.collectType == ChallengeCollectType.None)))
                    {
                        if (challenge.type == ChallengeType.Collect && (challenge.collectType == ChallengeCollectType.Gems || challenge.collectType == ChallengeCollectType.None))
                        {
                            challenge.collectType = ChallengeCollectType.Coins;
                        }
                        else
                        {
                            Debug.LogError("The challenge " + challenge.challengeName + " in mission " + challenge.parentMission.missionName + " has the challenge type " + challenge.type +
                    (challenge.type == ChallengeType.Collect ? " with collect type " + challenge.collectType : "") + " can't be used in situation " + challenge.situation);
                            challenge.type = ChallengeType.None;
                        }
                    }
                    break;
                case ChallengeSituationType.Game:
                    if (challenge.type == ChallengeType.Cleaning || challenge.type == ChallengeType.ComboMultiplier ||
                    (challenge.type == ChallengeType.Collect && (challenge.collectType == ChallengeCollectType.Gems || challenge.collectType == ChallengeCollectType.None)))
                    {
                        if (challenge.type == ChallengeType.Collect && (challenge.collectType == ChallengeCollectType.Gems || challenge.collectType == ChallengeCollectType.None))
                        {
                            challenge.collectType = ChallengeCollectType.Coins;
                        }
                        else
                        {
                            Debug.LogError("The challenge " + challenge.challengeName + " in mission " + challenge.parentMission.missionName + " has the challenge type " + challenge.type +
                        (challenge.type == ChallengeType.Collect ? " with collect type " + challenge.collectType : "") + " can't be used in situation " + challenge.situation);
                            challenge.type = ChallengeType.None;
                        }
                    }
                    break;
                case ChallengeSituationType.Stage:
                    if (!(challenge.type == ChallengeType.Cleaning || challenge.type == ChallengeType.Animals ||
                        (challenge.type == ChallengeType.Collect && challenge.collectType == ChallengeCollectType.Gems)))
                    {
                        if (challenge.type == ChallengeType.Collect && (challenge.collectType == ChallengeCollectType.Coins || challenge.collectType == ChallengeCollectType.None))
                        {
                            challenge.collectType = ChallengeCollectType.Gems;
                        }
                        else
                        {
                            Debug.LogError("The challenge " + challenge.challengeName + " in mission " + challenge.parentMission.missionName + " has the challenge type " + challenge.type +
                        (challenge.type == ChallengeType.Collect ? " with collect type " + challenge.collectType : "") + " can't be used in situation " + challenge.situation);
                            challenge.type = ChallengeType.None;
                        }
                    }
                    break;
            }
        }
        #endregion

        if (challenge.type != ChallengeType.Pirouettes && challenge.type != ChallengeType.None && (challenge.type != ChallengeType.Cleaning))
        {
            DrawChallengeAmount(challenge);
        }
        switch (challenge.type)
        {
            case ChallengeType.Collect:
                //Collect Type
                EditorGUILayout.BeginHorizontal();
                currentRect = EditorGUILayout.GetControlRect();
                EditorGUI.LabelField(currentRect, "Collect Type:");
                currentRect.x += challengeGUIPosX;
                currentRect.width = 200;
                challenge.collectType = (ChallengeCollectType)EditorGUI.EnumPopup(currentRect, challenge.collectType);
                EditorGUILayout.EndHorizontal();
                break;
            case ChallengeType.Obstacles:
                //Obstacle Type
                EditorGUILayout.BeginHorizontal();
                currentRect = EditorGUILayout.GetControlRect();
                EditorGUI.LabelField(currentRect, "Obstacle Type:");
                currentRect.x += challengeGUIPosX;
                currentRect.width = 200;
                challenge.obstacleType = (ChallengeObstacleType)EditorGUI.EnumPopup(currentRect, challenge.obstacleType);
                EditorGUILayout.EndHorizontal();
                break;
            case ChallengeType.Enemies:
                //Obstacle Type
                EditorGUILayout.BeginHorizontal();
                currentRect = EditorGUILayout.GetControlRect();
                EditorGUI.LabelField(currentRect, "Enemies Type:");
                currentRect.x += challengeGUIPosX;
                currentRect.width = 200;
                challenge.enemiesType = (ChallengeEnemiesType)EditorGUI.EnumPopup(currentRect, challenge.enemiesType);
                EditorGUILayout.EndHorizontal();
                break;
            case ChallengeType.Distance:
                //Obstacle Type
                EditorGUILayout.BeginHorizontal();
                currentRect = EditorGUILayout.GetControlRect();
                EditorGUI.LabelField(currentRect, "Character:");
                currentRect.x += challengeGUIPosX;
                currentRect.width = 200;
                //challenge.distanceCharacter = (PersonajeEnJuego)EditorGUI.EnumPopup(currentRect, challenge.distanceCharacter);
                EditorGUILayout.EndHorizontal();
                break;
            case ChallengeType.Pirouettes:
                DrawPirouettes(challenge);
                break;
            case ChallengeType.Cleaning:
                //Cleaning Type
                EditorGUILayout.BeginHorizontal();
                currentRect = EditorGUILayout.GetControlRect();
                EditorGUI.LabelField(currentRect, "Cleaning Type:");
                currentRect.x += challengeGUIPosX;
                currentRect.width = 200;
                challenge.cleaningType = (ChallengeCleaningType)EditorGUI.EnumPopup(currentRect, challenge.cleaningType);
                EditorGUILayout.EndHorizontal();
                if (challenge.cleaningType == ChallengeCleaningType.Amount)
                {
                    DrawChallengeAmount(challenge);
                }
                else if (challenge.cleaningType == ChallengeCleaningType.Percentage)
                {
                    EditorGUILayout.BeginHorizontal();
                    currentRect = EditorGUILayout.GetControlRect();
                    EditorGUI.LabelField(currentRect, "Current Percentage:        " + challenge.currentPercentage);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    currentRect = EditorGUILayout.GetControlRect();
                    EditorGUI.LabelField(currentRect, "Required Percentage:");
                    currentRect.x += 150;
                    currentRect.width = 200;
                    challenge.percentage = EditorGUI.FloatField(currentRect, challenge.percentage);
                    EditorGUILayout.EndHorizontal();
                }
                break;
            case ChallengeType.Custom:
                break;
        }
        EditorGUILayout.Space();
        GUILayout.EndVertical();
        EditorGUI.indentLevel--;
        GUI.skin = _skinMission;

    }

    void DrawChallengeAmount(ChallengeData challenge)
    {
        EditorGUILayout.BeginHorizontal();
        Rect currentRect = EditorGUILayout.GetControlRect();
        EditorGUI.LabelField(currentRect, "Current Amount:               " + challenge.currentAmount);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        currentRect = EditorGUILayout.GetControlRect();
        EditorGUI.LabelField(currentRect, "Required Amount:");
        currentRect.x += 135;
        currentRect.width = 200;
        challenge.amountRequired = EditorGUI.FloatField(currentRect, challenge.amountRequired);
        EditorGUILayout.EndHorizontal();
    }

    void DrawPirouettes(ChallengeData challenge)
    {
        GUI.skin = _skinStage;

        EditorGUI.indentLevel+=1;
        GUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.Space();

        GUILayout.BeginHorizontal();
        Rect currentRect = EditorGUILayout.GetControlRect();
        EditorGUI.LabelField(currentRect, "--- PIROUETTES ---");
        GUILayout.EndHorizontal();

        currentRect = EditorGUILayout.GetControlRect();
        EditorGUI.LabelField(currentRect, "In Order:");
        currentRect.x += pirouetteGUIPosX;
        currentRect.width = 80;
        challenge.inOrder = EditorGUI.Toggle(currentRect, challenge.inOrder);

        if (challenge.inOrder)
        {
            currentRect = EditorGUILayout.GetControlRect();
            EditorGUI.LabelField(currentRect, "Allow other pirouettes in the middle:");
            currentRect.x += pirouetteGUIPosX;
            currentRect.width = 80;
            challenge.allowOtherPirouettesInTheMiddle = EditorGUI.Toggle(currentRect, challenge.allowOtherPirouettesInTheMiddle);
        }

        EditorGUILayout.BeginHorizontal();
        currentRect = EditorGUILayout.GetControlRect();
        EditorGUI.LabelField(currentRect, "Min Height:");
        currentRect.x += pirouetteGUIPosX;
        currentRect.width = 200;
        challenge.minHeight = EditorGUI.FloatField(currentRect, challenge.minHeight);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        currentRect = EditorGUILayout.GetControlRect();
        EditorGUI.LabelField(currentRect, "Max Height:");
        currentRect.x += pirouetteGUIPosX;
        currentRect.width = 200;
        challenge.maxHeight = EditorGUI.FloatField(currentRect, challenge.maxHeight);
        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < challenge.pirouettesFoldout.Count; i++)
        {
            //dibujamos las piruetas ya añadidas
            EditorGUILayout.BeginHorizontal();
            currentRect = EditorGUILayout.GetControlRect();
            currentRect.x += 5;
            currentRect.width = 80;
            challenge.pirouettesFoldout[i] =
                EditorGUI.Toggle(currentRect, challenge.pirouettesFoldout[i]);
            currentRect.x += 15;
            currentRect.width = 280;
            EditorGUI.LabelField(currentRect, challenge.pirouettes[i].type.ToString());
            currentRect.x += 175;
            currentRect.width = 130;
            if (GUI.Button(currentRect, "Delete Pirouette"))
            {
                challenge.RemovePirouette(challenge.pirouettes[i]);
            }
            EditorGUILayout.EndHorizontal();

            if (challenge.pirouettesFoldout[i])//Pirouette opened
            {
                GUI.skin = _skinMission;
                EditorGUI.indentLevel += 2;
                GUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.Space();

                //Challenge Type
                EditorGUILayout.BeginHorizontal();
                currentRect = EditorGUILayout.GetControlRect();
                EditorGUI.LabelField(currentRect, "Pirouette Type:");
                currentRect.x += 120;
                currentRect.width = 200;
                challenge.pirouettes[i].type = (PirouetteType)EditorGUI.EnumPopup(currentRect, challenge.pirouettes[i].type);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                currentRect = EditorGUILayout.GetControlRect();
                EditorGUI.LabelField(currentRect, "Done:");
                currentRect.x += 120;
                currentRect.width = 0;
                challenge.pirouettes[i].done = EditorGUI.Toggle(currentRect, challenge.pirouettes[i].done);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();         
                GUILayout.EndVertical();
                EditorGUI.indentLevel-=2;
            }
        }

        currentRect = EditorGUILayout.GetControlRect();
        currentRect.width = 200;
        currentRect.x += 27;

        if (GUI.Button(currentRect, "Add new Pirouette"))
        {
            AddNewPirouette(challenge);
        }

        EditorGUILayout.Space();
        GUILayout.EndVertical();
        EditorGUI.indentLevel--;
        GUI.skin = _skinChallenge;

    }

    void AddNewPirouette(ChallengeData challenge)
    {
        challenge.AddPirouette();
    }

    void SetInitialMissionsValues()
    {

            for (int j = 0; j < allMissionsData.tutorialMissions.missions.Count; j++)
            {
                allMissionsData.tutorialMissions.missions[j].missionIndex = j;
                allMissionsData.tutorialMissions.missions[j].missionName = allMissionsData.tutorialMissions.GetStageAcronym() + "_Mission" + (j + 1);
                allMissionsData.tutorialMissions.missions[j].stage = allMissionsData.tutorialMissions.stage;

                for (int k = 0; k < allMissionsData.tutorialMissions.missions[j].challenges.Count; k++)
                {
                    allMissionsData.tutorialMissions.missions[j].challenges[k].challengeIndex = k;
                    allMissionsData.tutorialMissions.missions[j].challenges[k].challengeName =
                        allMissionsData.tutorialMissions.GetStageAcronym() + "_Mission" + (j + 1) + "_Challenge" + (k + 1);
                allMissionsData.tutorialMissions.missions[j].challenges[k].parentMission = allMissionsData.tutorialMissions.missions[j];
                }
            }

        for (int i = 0; i < allMissionsData.stagesMissions.Count; i++)
        {
            for (int j = 0; j < allMissionsData.stagesMissions[i].missions.Count; j++)
            {
                allMissionsData.stagesMissions[i].missions[j].missionIndex = j;
                allMissionsData.stagesMissions[i].missions[j].missionName = allMissionsData.stagesMissions[i].GetStageAcronym() + "_Mission" + (j+1);
                allMissionsData.stagesMissions[i].missions[j].stage = allMissionsData.stagesMissions[i].stage;

                for (int k = 0; k < allMissionsData.stagesMissions[i].missions[j].challenges.Count; k++)
                {
                    allMissionsData.stagesMissions[i].missions[j].challenges[k].challengeIndex = k;
                    allMissionsData.stagesMissions[i].missions[j].challenges[k].challengeName =
                        allMissionsData.stagesMissions[i].GetStageAcronym() + "_Mission" + (j+1) + "_Challenge" + (k+1);
                    allMissionsData.stagesMissions[i].missions[j].challenges[k].parentMission = allMissionsData.stagesMissions[i].missions[j];
                }
            }
        }
    }
}
#endif
