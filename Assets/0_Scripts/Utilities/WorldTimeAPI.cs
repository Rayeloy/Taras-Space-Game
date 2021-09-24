using UnityEngine;
//using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine.Networking;

public class WorldTimeAPI : MonoBehaviour
{
    #region Singleton class: WorldTimeAPI

    public static WorldTimeAPI Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(this.gameObject);
        }
        //else
        //{
        //    Destroy(this.gameObject);
        //}
    }

    #endregion

    //json container
    struct TimeData
    {
        //public string client_ip;
        //...
        public string datetime;
        //..
    }

    const string API_URL = "http://worldtimeapi.org/api/ip";

    [HideInInspector] public bool IsTimeLoaded = false;

    private System.DateTime _currentDateTime = System.DateTime.Now;

    void Start()
    {
        StartCoroutine(GetRealDateTimeFromAPI());
    }

    public System.DateTime GetCurrentDateTime()
    {
        //here we don't need to get the datetime from the server again
        // just add elapsed time since the game start to _currentDateTime

        return _currentDateTime.AddSeconds(Time.realtimeSinceStartup);
    }

    IEnumerator GetRealDateTimeFromAPI()
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(API_URL);
        //Debug.Log("getting real datetime...");

        yield return webRequest.SendWebRequest();

        if (webRequest.isNetworkError)
        {
            //error
            Debug.Log("Error: " + webRequest.error);

        }
        else
        {
            //success
            TimeData timeData = JsonUtility.FromJson<TimeData>(webRequest.downloadHandler.text);
            //timeData.datetime value is : 2020-08-14T15:54:04+01:00

            _currentDateTime = ParseDateTime(timeData.datetime);
            IsTimeLoaded = true;

            //Debug.Log("Success.");
        }
    }
    //datetime format => 2020-08-14T15:54:04+01:00
    System.DateTime ParseDateTime(string datetime)
    {
        //match 0000-00-00
        string date = Regex.Match(datetime, @"^\d{4}-\d{2}-\d{2}").Value;

        //match 00:00:00
        string time = Regex.Match(datetime, @"\d{2}:\d{2}:\d{2}").Value;

        return System.DateTime.Parse(string.Format("{0} {1}", date, time));
    }

    #region --- TIME PLAYED ---
    bool playingApp = true;
#if UNITY_ANDROID
    void OnApplicationFocus(bool focus)
    {
        if (!focus)
        {
            playingApp = false;
            //Debug.LogError("Sale del juego");
        }
        else
        {
            playingApp = true;
            //Debug.LogError("Vuelve al juego");
        }
    }
#endif

#if UNITY_EDITOR || UNITY_IOS
    void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            playingApp = false;
            //Debug.LogError("Sale del juego");
        }
        else
        {
            playingApp = true;
            //Debug.LogError("Vuelve al juego");
        }
    }
#endif

    public void UpdateTimePlayed()
    {
        System.DateTime date = new System.DateTime();

        if (MasterManager.connected)
        {
            date = WorldTimeAPI.Instance.GetCurrentDateTime();
        }
        else
        {
            date = System.DateTime.Now;
        }
        //Grab the old time from the player prefs as a long
        long temp = System.Convert.ToInt64(PlayerPrefs.GetString("LastSavedDateTotalTime", System.DateTime.Now.ToBinary().ToString()));

        //Convert the old time from binary to a DataTime variable
        System.DateTime oldDate = System.DateTime.FromBinary(temp);

        //Use the Subtract method and store the result as a timespan variable
        System.TimeSpan timePassed = date.Subtract(oldDate);

        float secondsPlayed = PlayerPrefs.GetFloat("TotalTimePlayed", 0);
        //Debug.Log("MerchantCurrentTime = " + merchantCurrentTime);
        secondsPlayed += (float)timePassed.TotalSeconds;
        PlayerPrefs.SetFloat("TotalTimePlayed", secondsPlayed);
    }

    public static System.DateTime GetCurrentDate()
    {
        System.DateTime date = new System.DateTime();

        if (MasterManager.connected)
        {
            date = WorldTimeAPI.Instance.GetCurrentDateTime();
        }
        else
        {
            date = System.DateTime.Now;
        }
        return date;
    }
    #endregion
}


/* API (json)
{
	"abbreviation" : "CEST",
	"client_ip"    : "83.37.56.24",
	"datetime"     : "2021-05-06T13:19:35.251813+02:00",
    "day_of_week"  : 4,
    "day_of_year"  : 126,
	"dst"          : true,
	"dst_from"     : 2021-03-28T01:00:00+00:00,
	"dst_offset"   : 3600,
	"dst_until"    : 2021-10-31T01:00:00+00:00,
	"raw_offset"   : 3600,
	"timezone"     : "Europe/Madrid",
	"unixtime"     : 1620299975,
	"utc_datetime" : "2021-05-06T11:19:35.251813+00:00",
	"utc_offset"   : "+02:00",
    "week_number"  : 18
}
We only need "datetime" property.
*/
