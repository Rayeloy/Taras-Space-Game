#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[ExecuteAlways]
public class PlayerAtributesEditorWindow : EditorWindow
{
    //public InitialCharactersData initialCharactersData;
    bool[] characterFoldouts = new bool[10];
    Vector2 scrollPosition = Vector2.zero;

    void SetUp()
    {
        //initialCharactersData.initialDataArray = new CharacterAtributesInitialData[10];
        characterFoldouts = new bool[10];
        for (int i = 0; i < characterFoldouts.Length; i++)
        {
            //Debug.Log("Character = " + (PersonajeEnJuego)i);
            //initialCharactersData.initialDataArray[i] = new CharacterAtributesInitialData();
            //initialCharactersData.initialDataArray[i].character = (PersonajeEnJuego)i;
            characterFoldouts[i] = false;
        }
    }

    [MenuItem("Tzuki's Tools/Player Atributes Editor")]
    public static void ShowWindow()
    {
        GetWindow<PlayerAtributesEditorWindow>("Player Atributes Editor");
    }

    private void OnEnable()
    {
        //if (initialCharactersData == null) initialCharactersData = (InitialCharactersData)Resources.Load("InitialCharactersData", typeof(InitialCharactersData));
    }

    void OnGUI()
    {
        if (/*initialCharactersData.initialDataArray ==null || initialCharactersData.initialDataArray.Length ==0 || initialCharactersData.initialDataArray[0]==null ||*/ characterFoldouts == null || characterFoldouts.Length == 0)
        {
            SetUp();
        }
        //initialCharactersData = EditorGUILayout.ObjectField("Initial Data Save File", initialCharactersData, typeof(InitialCharactersData), false) as InitialCharactersData;
        //if (initialCharactersData == null) return;
        GUILayout.Label("Fill the levels data of every character", EditorStyles.boldLabel);

        if (GUILayout.Button("Fill!"))
        {
            //FillLevelsData();
        }

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        for (int i = 0; i < characterFoldouts.Length; i++)
        {
            //characterFoldouts[i] = EditorGUILayout.BeginFoldoutHeaderGroup(characterFoldouts[i], initialCharactersData.initialDataArray[i].character.ToString());
            if (characterFoldouts[i])
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Aguante Mínimo");
                //initialCharactersData.initialDataArray[i].minAguante = EditorGUILayout.FloatField(initialCharactersData.initialDataArray[i].minAguante);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Aguante Máximo");
                //initialCharactersData.initialDataArray[i].maxAguante = EditorGUILayout.FloatField(initialCharactersData.initialDataArray[i].maxAguante);
                EditorGUILayout.EndHorizontal();

               
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        GUILayout.EndScrollView();
        //EditorUtility.SetDirty(initialCharactersData);
    }

    //void FillLevelsData()
    //{
    //    EditorUtility.SetDirty(initialCharactersData);
    //    for (int i = 0; i < MasterManager.GameDataManager.allCharacters.Length; i++)
    //    {
    //        //AtributosJugador character = MasterManager.GameDataManager.allCharacters[i];
    //        EditorUtility.SetDirty(character);
    //        if (MasterManager.GameDataManager.allCharacters[i] != null)
    //        {
    //            initialCharactersData.initialDataArray[i].incAceleracion = (initialCharactersData.initialDataArray[i].maxAceleracion - initialCharactersData.initialDataArray[i].minAceleracion) / 9;
    //            initialCharactersData.initialDataArray[i].incAdrenalina = (initialCharactersData.initialDataArray[i].maxAdrenalina - initialCharactersData.initialDataArray[i].minAdrenalina) / 9;
    //            initialCharactersData.initialDataArray[i].incAgilidad = (initialCharactersData.initialDataArray[i].maxAgilidad - initialCharactersData.initialDataArray[i].minAgilidad) / 9;
    //            initialCharactersData.initialDataArray[i].incAguante = (initialCharactersData.initialDataArray[i].maxAguante - initialCharactersData.initialDataArray[i].minAguante) / 9;
    //            initialCharactersData.initialDataArray[i].incFuerzaSalto = (initialCharactersData.initialDataArray[i].maxFuerzaSalto - initialCharactersData.initialDataArray[i].minFuerzaSalto) / 9;
    //            initialCharactersData.initialDataArray[i].incPeso = (initialCharactersData.initialDataArray[i].maxPeso - initialCharactersData.initialDataArray[i].minPeso) / 9;
    //            initialCharactersData.initialDataArray[i].incPoder = (initialCharactersData.initialDataArray[i].maxPoder - initialCharactersData.initialDataArray[i].minPoder) / 9;
    //            initialCharactersData.initialDataArray[i].incTopeAdrenalina = (initialCharactersData.initialDataArray[i].maxTopeAdrenalina - initialCharactersData.initialDataArray[i].minTopeAdrenalina) / 9;
    //            initialCharactersData.initialDataArray[i].incVelocidad = (initialCharactersData.initialDataArray[i].maxVelocidad - initialCharactersData.initialDataArray[i].minVelocidad) / 9;

    //            for (int j = 1; j < 11; j++)
    //            {
    //                Debug.Log("filling up " + character.tipoPersonaje + "'s level " + j);
    //                Nivel lvl = character.GetLevel(j);
    //                CharacterAtributesInitialData initialData = GetInitialData(character.tipoPersonaje);
    //                lvl.aguanteMax = initialData.minAguante + (initialData.incAguante * (j - 1));
    //                lvl.adrenalina = initialData.minAdrenalina + (initialData.incAdrenalina * (j - 1));
    //                lvl.topeAdrenalina = initialData.minTopeAdrenalina + (initialData.incTopeAdrenalina * (j - 1));
    //                lvl.velocidadMaxima = initialData.minVelocidad + (initialData.incVelocidad * (j - 1));
    //                lvl.peso = initialData.minPeso + (initialData.incPeso * (j - 1));
    //                lvl.aceleracion = initialData.minAceleracion + (initialData.incAceleracion * (j - 1));
    //                lvl.fuerzaSalto = initialData.minFuerzaSalto + (initialData.incFuerzaSalto * (j - 1));
    //                lvl.agilidad = initialData.minAgilidad + (initialData.incAgilidad * (j - 1));
    //                lvl.poder = initialData.minPoder + (initialData.incPoder * (j - 1));
    //                lvl.maxXP = 20 + (20 * j);
    //                character.SetLevel(j, lvl);
    //            }
    //        }
    //    }
    //}

    //CharacterAtributesInitialData GetInitialData(PersonajeEnJuego character)
    //{
    //    for (int i = 0; i < initialCharactersData.initialDataArray.Length; i++)
    //    {
    //        if (initialCharactersData.initialDataArray[i] != null && initialCharactersData.initialDataArray[i].character == character)
    //        {
    //            return initialCharactersData.initialDataArray[i];
    //        }
    //    }
    //    return null;
    //}

}

#endif
