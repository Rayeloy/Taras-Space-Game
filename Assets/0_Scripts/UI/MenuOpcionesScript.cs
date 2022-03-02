using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using I2.Loc;
//using FMODUnity;
//using System;

public enum OptionsMenuState
{
    None = -1,
    Opening = 7,
    Closing = 8,
    Mission = 0,
    Settings = 1,
    Videos = 2,
    Statistics = 3,
    Credits = 4,
    Languages = 5,
    SocialShare = 6
}

public enum Settings
{
    None,
    Vibration,


    Length
}
public class MenuOpcionesScript : MonoBehaviour
{
    public static MenuOpcionesScript instance;
    public OptionsMenuState state = OptionsMenuState.None;
    public TextMeshProUGUI sectionTitleText;
    public RectTransform[] contents;//0-> Mission, 1-> Settings, 2-> Languages, 3-> Videos, 4-> Statistics, 5-> Credits, 6-> SocialShare
    public GameObject[] sectionButtons;//0-> Mission, 1-> Settings, 2-> Languages, 3-> Videos, 4-> Statistics, 5-> Credits, 6-> SocialShare
    int currentSection = 0;

    [Header("--- MISSION ---")]
    public Text tituloMision;
    public TextMeshProUGUI[] nombreReto;
    public Button cambiarMisionMenos;
    public Button cambiarMisionMas;
    int currentMission = 0;
    List<MissionData> missions;
    [Header("--- SETTINGS ---")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumenSlider;
    public Toggle vibrationToggle;
    public Toggle subtitlesToggle;
    public Toggle videoRewardToggle;
    public TextMeshProUGUI videoRewardText;
    [Header("--- VIDEOS ---")]
    public GameObject[] videos;
    public GameObject[] videoTexts;

    [Header("--- STATISTICS ---")]
    public TextMeshProUGUI[] statsTexts;

    [Header("--- OPENING ANIMATION ---")]
    public UIAnimation optionsMenuOpenAnim;
    public Image fondoBlur;
    public Image blackVeil;
    float openingTime = 0;
    float maxBlurVal = 8;

   

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

   
    public void EnableCurrentCanvasGroup(CanvasGroup currentCanvasGroup)
    {

        currentCanvasGroup.alpha = 1;
        currentCanvasGroup.blocksRaycasts = true;
        currentCanvasGroup.interactable = true;
    }

    public void DisableCurrentCanvasGroup(CanvasGroup currentCanvasGroup)
    {
        currentCanvasGroup.alpha = 0;
        currentCanvasGroup.blocksRaycasts = false;
        currentCanvasGroup.interactable = false;
    }

    public void ThisStart(int section = -1)
    {


        UIAnimationsManager.instance.StartAnimation(optionsMenuOpenAnim);
        if (section > -1)
        {
            ChangeOptionsSection(section);
        }
        else
        {
            //if (GeneralPauseScript.pause.estadoJuego == GameState.pausado)
            //{
            //    for (int i = 0; i < sectionButtons.Length; i++)
            //    {
            //        sectionButtons[i].SetActive(false);
            //    }
            //    ChangeOptionsSection(1);
            //}
            //else
            //{
            //    for (int i = 0; i < sectionButtons.Length-1; i++)
            //    {
            //        sectionButtons[i].SetActive(true);
            //    }
            //    ChangeOptionsSection(0);
            //}
        }
        state = OptionsMenuState.Opening;
        openingTime = 0;
    }

    void ProcessOpeningAnimation()
    {
        if (state == OptionsMenuState.Opening)
        {
            openingTime += Time.deltaTime;
            float value = openingTime / optionsMenuOpenAnim.duration;
            float distorsionFondo = value * maxBlurVal;
            distorsionFondo = Mathf.Clamp(distorsionFondo, 0, maxBlurVal);
            fondoBlur.material.SetFloat("_Size", distorsionFondo);
            if (!UIAnimationsManager.instance.IsPlaying(optionsMenuOpenAnim))
            {
                FinishOpeningAnimation();
            }
        }
    }

    void FinishOpeningAnimation()
    {
        if (state == OptionsMenuState.Opening)
        {
            fondoBlur.material.SetFloat("_Size", maxBlurVal);
            state = (OptionsMenuState)currentSection;
        }
    }

    public void ThisUpdate()
    {

        Tap();
        if (state != OptionsMenuState.None)
        {
            ProcessOpeningAnimation();
            ProcessClosingAnimation();
        }
    }

    public void BackButton()
    {
        if (state != OptionsMenuState.None && state != OptionsMenuState.Closing && state != OptionsMenuState.Opening)
        {
            MasterManager.GameResourcesManager.SonidoClick();
            SaveOptions();
            StartClosingAnimation();
        }
    }

    public void StartClosingAnimation()
    {
        if(state != OptionsMenuState.None && state != OptionsMenuState.Closing && state != OptionsMenuState.Opening)
        {
            //Debug.Log("Options Menu: Start Closing Animation");
            state = OptionsMenuState.Closing;
            UIAnimationsManager.instance.StartAnimation(optionsMenuOpenAnim, true);
            openingTime = 0;
        }
    }

    void ProcessClosingAnimation()
    {
        if (state == OptionsMenuState.Closing)
        {
            openingTime += Time.deltaTime;
            float value = openingTime / optionsMenuOpenAnim.duration;
            float distorsionFondo = value * maxBlurVal;
            distorsionFondo = maxBlurVal - distorsionFondo;
            distorsionFondo = Mathf.Clamp(distorsionFondo, 0, maxBlurVal);
            fondoBlur.material.SetFloat("_Size", distorsionFondo);
            if (!UIAnimationsManager.instance.IsPlaying(optionsMenuOpenAnim))
            {
                FinishClosingAnimation();
            }
        }
    }

    void FinishClosingAnimation()
    {
        if (state == OptionsMenuState.Closing)
        {
            fondoBlur.material.SetFloat("_Size", 0);
            state = OptionsMenuState.None;
        }
    }

    public void LoadOptions()
    {
        LoadSettings();

        SetMusicMixerVolume(PlayerPrefs.GetFloat("MusicVolumeSetting", 1));
        SetSFXMixerVolume(PlayerPrefs.GetFloat("SFXVolumeSetting", 1));
    }

    public void SaveOptions()
    {
        SaveSettings();
    }

    public void ResetDefaultOptions()
    {
        PlayerPrefs.SetFloat("MusicVolumeSetting", 1);
        PlayerPrefs.SetFloat("SFXVolumeSetting", 1);
        PlayerPrefs.SetInt("VibrationSetting", 1);
        PlayerPrefs.SetInt("SubtitlesSetting", 1);
        PlayerPrefs.SetInt("VideoRewardSetting", 1);
    }

    #region --- SECTIONS ---
    public void ChangeOptionsSectionButton(int section)
    {
        if (state == OptionsMenuState.None || state == OptionsMenuState.Opening || state == OptionsMenuState.Closing) return;

        ChangeOptionsSection(section);
        MasterManager.GameResourcesManager.SonidoClick();
    }

    public void ChangeOptionsSection(int section)
    {
        for (int i = 0; i < contents.Length; i++)
        {
            contents[i].gameObject.SetActive(false);
        }
        contents[section].gameObject.SetActive(true);
        state = (OptionsMenuState)section;
        currentSection = section;
        switch ((OptionsMenuState)section)
        {
            case OptionsMenuState.Mission:
                UptadeMission();
                break;
            case OptionsMenuState.Settings:
                UpdateSettings();
                break;
            case OptionsMenuState.Languages:
                break;
            case OptionsMenuState.Videos:
                SetupVideosSection();
                break;
            case OptionsMenuState.Statistics:
                UpdateStats();
                break;
            case OptionsMenuState.Credits:
                break;
            case OptionsMenuState.SocialShare:
                break;
        }
        UpdateSectionTitle();
    }

    void UpdateSectionTitle()
    {
        string title = "";
        switch (state)
        {
            case OptionsMenuState.Mission:
                title = I2.Loc.LocalizationManager.GetTranslation("_ui_opciones_mision_");
                break;
            case OptionsMenuState.Settings:
                title = I2.Loc.LocalizationManager.GetTranslation("_ui_opciones_ajustes_");
                break;
            case OptionsMenuState.Languages:
                title = I2.Loc.LocalizationManager.GetTranslation("_ui_opciones_idiomas_");
                break;
            case OptionsMenuState.Videos:
                title = I2.Loc.LocalizationManager.GetTranslation("_ui_opciones_videoteca_");
                break;
            case OptionsMenuState.Statistics:
                title = I2.Loc.LocalizationManager.GetTranslation("_ui_opciones_stats_");
                break;
            case OptionsMenuState.Credits:
                title = I2.Loc.LocalizationManager.GetTranslation("_ui_opciones_creditos_");
                break;
            case OptionsMenuState.SocialShare:
                title = I2.Loc.LocalizationManager.GetTranslation("_ui_opciones_social_");
                break;
        }
        sectionTitleText.text = title;
    }
    #endregion

    //Mission
    void UptadeMission()
    { 
        //cambiarMisionMenos.gameObject.SetActive(true);
        //if(currentMission == GestorDeMisionesScript.gestorMisiones.misionActiva - 1) cambiarMisionMas.gameObject.SetActive(false);
        //else cambiarMisionMas.gameObject.SetActive(true);
        if (currentMission == 0) cambiarMisionMenos.gameObject.SetActive(false);
        //else if (currentMission == missions.Count - 1) cambiarMisionMas.gameObject.SetActive(false);

        //Mostrar misiones
        tituloMision.text = I2.Loc.LocalizationManager.GetTranslation("_ui_missions_") + ": " + (currentMission + 1);
        for (int i = 0; i < 3; i++)
        {
            if (missions[currentMission].challenges[i].completed)
            {
                nombreReto[i].text = "[OK] ";
            }
            else
            {
                nombreReto[i].text = "[  ] ";
            }
            nombreReto[i].text = missions[currentMission].challenges[i].GetChallengeText();
        }
    }

    public void ChangeMission(int change)
    {
        if (state == OptionsMenuState.None || state == OptionsMenuState.Opening || state == OptionsMenuState.Closing) return;
        int indexResult = currentMission + change;

        Debug.Log("currentMission = " + currentMission + "; indexResult = " + indexResult + "; missions.Length = " + missions.Count);
        if (missions.Count > indexResult && indexResult >= 0)
        {
            currentMission = indexResult;
            UptadeMission();
        }

        MasterManager.GameResourcesManager.SonidoClick();
    }

    //Settings
    #region --- SETTINGS ---
    void SaveSettings()
    {
        PlayerPrefs.SetFloat("MusicVolumeSetting", musicVolumeSlider.value);
        PlayerPrefs.SetFloat("SFXVolumeSetting", sfxVolumenSlider.value);
        PlayerPrefs.SetInt("VibrationSetting", vibrationToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("SubtitlesSetting", subtitlesToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("VideoRewardSetting", videoRewardToggle.isOn ? 1 : 0);

        //Debug.Log("MusicVolumeSetting = " + PlayerPrefs.GetFloat("MusicVolumeSetting", 1) + "; SFXVolumeSetting = " + PlayerPrefs.GetFloat("SFXVolumeSetting", 1));
    }

    public void LoadSettings()
    {
        musicVolumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("MusicVolumeSetting", 1));
        sfxVolumenSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("SFXVolumeSetting", 1));
        vibrationToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt("VibrationSetting", 1) == 1 ? true : false);
        subtitlesToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt("SubtitlesSetting", 1) == 1 ? true : false);
        videoRewardToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt("VideoRewardSetting", 1) == 1 ? true : false);

        //Debug.Log("MusicVolumeSetting = " + PlayerPrefs.GetFloat("MusicVolumeSetting", 1) + "; SFXVolumeSetting = " + PlayerPrefs.GetFloat("SFXVolumeSetting", 1));
    }

    void UpdateSettings()
    {
        LoadSettings();
        if (IAPManager.Instance.IsActive(ShopProductNames.JuegoCompleto))
        {
            videoRewardText.color = new Color(videoRewardText.color.r, videoRewardText.color.g, videoRewardText.color.b, 1);
            videoRewardToggle.interactable = true;
        }
        else
        {
            videoRewardText.color = new Color(videoRewardText.color.r, videoRewardText.color.g, videoRewardText.color.b, 0.5f);
            videoRewardToggle.interactable = false;
        }
        SaveSettings();
    }

    public bool GetSettings(Settings setting)
    {
        switch (setting)
        {
            case Settings.Vibration:
                return PlayerPrefs.GetInt("VibrationSetting", 0) == 1;
            default:
                return false;
        }
    }

    public void OnMusicVolumeChanged()
    {
        if (state != OptionsMenuState.Settings) return;
        SetMusicMixerVolume(musicVolumeSlider.value);
        PlayerPrefs.SetFloat("MusicVolumeSetting", musicVolumeSlider.value);
        Debug.Log("MusicVolumeChanged to "+ musicVolumeSlider.value);
    }

    public void SetMusicMixerVolume(float value)
    {
        //float dbVolume = (value * 100f) - 80f;
        FMODUnity.RuntimeManager.GetBus("bus:/Master/Soundtrack").setVolume(value);
    }

    public void OnSFXVolumeChanged()
    {
        if (state != OptionsMenuState.Settings) return;
        SetSFXMixerVolume(sfxVolumenSlider.value);
        PlayerPrefs.SetFloat("SFXVolumeSetting", sfxVolumenSlider.value);
    }

    public void SetSFXMixerVolume(float value)
    {
        //float dbVolume = (value * 100f) - 80f;
        FMODUnity.RuntimeManager.GetBus("bus:/Master/SFX").setVolume(value);
    }

    public void OnVibrationValueChanged()
    {
        if (state != OptionsMenuState.Settings) return;
        PlayerPrefs.SetInt("VibrationSetting", vibrationToggle.isOn ? 1 : 0);
        MasterManager.GameResourcesManager.SonidoClick();
    }

    public void OnSubtitlesValueChanged()
    {
        if (state != OptionsMenuState.Settings) return;
        PlayerPrefs.SetInt("SubtitlesSetting", subtitlesToggle.isOn ? 1 : 0);
        MasterManager.GameResourcesManager.SonidoClick();
    }

    public void OnVideoRewardValueChanged()
    {
        if (state != OptionsMenuState.Settings) return;
        videoRewardToggle.isOn = PlayerPrefs.GetInt("VideoRewardSetting", 1) == 1 ? true : false;
        MasterManager.GameResourcesManager.SonidoClick();
    }

    public void RestoreShopButton()
    {
        //GeneralPauseScript.pause.ShowPopUp("_ui_opciones_are_you_sure_reset_shop_", RestoreShop);
        MasterManager.GameResourcesManager.SonidoClick();
    }

    public void RestoreShop()
    {
    }

    public void ResetDataButton()
    {
        //GeneralPauseScript.pause.ShowPopUp("_ui_opciones_are_you_sure_reset_data_", ResetData);
        MasterManager.GameResourcesManager.SonidoClick();
    }

    public void ResetData()
    {
        StartClosingAnimation();
        MasterManager.GameDataManager.ResetData();
    }

    #endregion

    //Languages
    public void ChangeLanguage(int index)
    {
        if (state == OptionsMenuState.None || state == OptionsMenuState.Opening || state == OptionsMenuState.Closing) return;

        switch (index)
        {
            case 0:
                I2.Loc.LocalizationManager.CurrentLanguage = "Spanish";
                break;
            case 1:
                I2.Loc.LocalizationManager.CurrentLanguage = "English";
                break;
            case 2:
                I2.Loc.LocalizationManager.CurrentLanguage = "Basque";
                break;
        }
        UpdateSectionTitle();
        MasterManager.GameResourcesManager.SonidoClick();
    }

    //Videos
    public void SetupVideosSection()
    {
        //videos[0].gameObject.SetActive(false);
        //videos[1].gameObject.SetActive(false);
        //videos[2].gameObject.SetActive(false);
        //videos[3].gameObject.SetActive(false);
        //videos[4].gameObject.SetActive(false);
        //videoTexts[0].gameObject.SetActive(false);
        //videoTexts[1].gameObject.SetActive(false);
        //videoTexts[2].gameObject.SetActive(false);
        //videoTexts[3].gameObject.SetActive(false);
        //videoTexts[4].gameObject.SetActive(false);
        //if (GestorDeMisionesScript.gestorMisiones.nivelEnCurso > 1)
        //{
        //    videos[0].gameObject.SetActive(true);
        //    videos[1].gameObject.SetActive(true);
        //    videos[2].gameObject.SetActive(true);
        //    videos[3].gameObject.SetActive(true);
        //    videos[4].gameObject.SetActive(true);
        //    videoTexts[0].gameObject.SetActive(true);
        //    videoTexts[1].gameObject.SetActive(true);
        //    videoTexts[2].gameObject.SetActive(true);
        //    videoTexts[3].gameObject.SetActive(true);
        //    videoTexts[4].gameObject.SetActive(true);
        //}
        //else if(GestorDeMisionesScript.gestorMisiones.nivelEnCurso == 1)
        //{
        //    videos[0].gameObject.SetActive(true);
        //    videos[1].gameObject.SetActive(true);
        //    videos[2].gameObject.SetActive(true);
        //    videos[3].gameObject.SetActive(GestorDeMisionesScript.gestorMisiones.misionActiva >= 7);
        //    videos[4].gameObject.SetActive(GestorDeMisionesScript.gestorMisiones.misionActiva >= 10);
        //    videoTexts[0].gameObject.SetActive(true);
        //    videoTexts[1].gameObject.SetActive(true);
        //    videoTexts[2].gameObject.SetActive(true);
        //    videoTexts[3].gameObject.SetActive(GestorDeMisionesScript.gestorMisiones.misionActiva >= 7);
        //    videoTexts[4].gameObject.SetActive(GestorDeMisionesScript.gestorMisiones.misionActiva >= 10);
        //}
    }

    //public void PlayVideo(int videoIndex)
    //{
    //    if (state == OptionsMenuState.None || state == OptionsMenuState.Opening || state == OptionsMenuState.Closing) return;
    //    MasterManager.GameDataManager.cutsceneReplayOn = true;
    //    switch (videoIndex)
    //    {
    //        case 0:
    //            GeneralPauseScript.pause.CambioEscena("3_CinematicaInicial");
    //            MasterManager.GameDataManager.playCutscene2 = false;
    //            break;
    //        case 1:
    //            GeneralPauseScript.pause.CambioEscena("3_CinematicaInicial");
    //            MasterManager.GameDataManager.playCutscene2 = true;
    //            break;
    //        case 2:
    //            GeneralPauseScript.pause.CambioEscena("5_CinematicaExplosionTutorial");
    //            break;
    //        case 3:
    //            GeneralPauseScript.pause.CambioEscena("7_CinematicaFinalTutorial");
    //            break;
    //        case 4:
    //            GeneralPauseScript.pause.CambioEscena("9_CinematicaFinalStella");
    //            break;
    //    }
    //    MasterManager.GameDataManager.sceneToLoadAfterCinematicVideo = SceneManager.GetActiveScene().name;
    //    MasterManager.GameDataManager.openOptionsOnStart = 2;
    //    MasterManager.GameDataManager.stopChangingTheGameStateFFS = true;
    //    MasterManager.GameDataManager.characterSelectedInMenu = (PersonajeEnJuego)GeneralPauseScript.pause.gestorPersonajes.selectedCharacter.idPersonaje;
    //    MasterManager.GameResourcesManager.SonidoClick();
    //    StartClosingAnimation();
    //}


    public void ChargeCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ChargeOtherScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);

    }

    public void Quit_Game()
    {
        Application.Quit();
    }

    private void OnApplicationQuit()
    {
    }

    //Stats
    void UpdateStats()
    {
        WorldTimeAPI.Instance.UpdateTimePlayed();
        float timePlayed = PlayerPrefs.GetFloat("TotalTimePlayed", 0);
        System.TimeSpan timePlayedSpan = System.TimeSpan.FromSeconds(timePlayed);
        //Debug.Log("timePlayed = "+ timePlayed + "; timePlayedSpan.Days = "+ timePlayedSpan.Days + "; hours.Hours = " + hours.Hours + "; minutes.Minutes = " + minutes.Minutes);
        statsTexts[0].text = timePlayedSpan.Days+" "+ I2.Loc.LocalizationManager.GetTranslation("_ui_opciones_stats_dias_acronimo_") + " "+
            timePlayedSpan.Hours+" "+ I2.Loc.LocalizationManager.GetTranslation("_ui_opciones_stats_horas_acronimo_") + " "+
            timePlayedSpan.Minutes+ " "+I2.Loc.LocalizationManager.GetTranslation("_ui_opciones_stats_minutos_acronimo_");
        statsTexts[1].text =""+ PlayerPrefs.GetInt("TotalChallengesCompleted", 0);
        statsTexts[2].text = ""+ PlayerPrefs.GetInt("TotalScore",0);
        statsTexts[3].text = ""+ PlayerPrefs.GetInt("HighestScore",0);
        statsTexts[4].text = "" + (int)PlayerPrefs.GetFloat("HighestCombo", 0);
        statsTexts[5].text = "" + PlayerPrefs.GetInt("EnemiesOutrun", 0);
        statsTexts[6].text = "" + PlayerPrefs.GetInt("AnimalesTotal", 0);
        statsTexts[7].text = "" + PlayerPrefs.GetInt("ManchasTotal", 0);
        statsTexts[8].text = "" + PlayerPrefs.GetInt("TotalFlips", 0);
        statsTexts[9].text = "" + PlayerPrefs.GetInt("TotalPowerUses", 0);
        statsTexts[10].text = "" + PlayerPrefs.GetInt("TotalSkillUses", 0);
        statsTexts[11].text = "" + PlayerPrefs.GetInt("TotalDeaths", 0);
        statsTexts[12].text = "" + PlayerPrefs.GetInt("TotalTaps", 0);
        statsTexts[13].text = "" + PlayerPrefs.GetInt("TotalMetters", 0);
    }

    public void Tap()
    {
        if (Input.GetMouseButtonDown(0))
        {
            int taps = PlayerPrefs.GetInt("TotalTaps", 0);
            taps++;
            PlayerPrefs.SetInt("TotalTaps", taps);
        }
    }

    public void AddMetters(int metters)
    {
        int totalMetters = PlayerPrefs.GetInt("TotalMetters", 0);
        totalMetters+= metters;
        PlayerPrefs.SetInt("TotalMetters", totalMetters);
    }

    //Credits

    //Social

}
