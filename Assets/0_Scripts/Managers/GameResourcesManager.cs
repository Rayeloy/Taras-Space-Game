using System.IO;
using System.Net;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
#endif
using UnityEngine;

[CreateAssetMenu(menuName = "Managers/GameResourcesManager")]
public class GameResourcesManager : ScriptableObject
{
    public Color cyanBlue = new Color(0.0990566f, 0.8924585f, 1, 1);

    public void ThisEarlyUpdate()
    {
        clickSoundCalledDuringThisFrame = false;
    }

    bool clickSoundCalledDuringThisFrame = false;
    public void SonidoClick()
    {
        if (!clickSoundCalledDuringThisFrame)
        {
            clickSoundCalledDuringThisFrame = true;
            FMODUnity.RuntimeManager.PlayOneShot("event:/Project 3/Interface/UI_Click", Vector3.zero);
            //Debug.Log("CLICK SFX");
        }
    }

    public void SonidoMoneda()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/Project 3/Items/GamePlay_Moneda2", Vector3.zero);
    }

    public void SetGlobalScale(Transform transform, Vector3 globalScale)
    {
        transform.localScale = Vector3.one;
        transform.localScale = new Vector3(globalScale.x / transform.lossyScale.x, globalScale.y / transform.lossyScale.y, globalScale.z / transform.lossyScale.z);
    }
#if UNITY_EDITOR
    public bool IsInPrefabMode(GameObject gO)
    {
        //Debug.Log("IsInPrefabMode = "+(PrefabStageUtility.GetPrefabStage(gO) != null && (PrefabUtility.GetPrefabInstanceStatus(gO) == PrefabInstanceStatus.NotAPrefab ||
        //    gO.transform.parent.name == "Canvas (Environment)" || gO.transform.parent.name == "Prefab Mode in Context")) +"; GameObject = " + gO + "; prefab mode? " + (PrefabStageUtility.GetPrefabStage(gO) != null) +
        //   "; PrefabInstanceStatus = " + PrefabUtility.GetPrefabInstanceStatus(gO) + "; PrefabAssetType = " + PrefabUtility.GetPrefabAssetType(gO) + "; parent = " + gO.transform.parent);
        return PrefabStageUtility.GetPrefabStage(gO) != null && (PrefabUtility.GetPrefabInstanceStatus(gO) == PrefabInstanceStatus.NotAPrefab ||
            gO.transform.parent.name == "Canvas (Environment)" || gO.transform.parent.name == "Prefab Mode in Context");
    }
#endif

    //public DateTime GetNetTime()
    //{
    //    var myHttpWebRequest = (HttpWebRequest)WebRequest.Create("http://www.microsoft.com");
    //    var response = myHttpWebRequest.GetResponse();
    //    string todaysDates = response.Headers["date"];
    //    return DateTime.ParseExact(todaysDates,
    //                               "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
    //                               CultureInfo.InvariantCulture.DateTimeFormat,
    //                               DateTimeStyles.AssumeUniversal);
    //}

    public string GetHtmlFromUri(string resource)
    {
        string html = string.Empty;
        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(resource);
        try
        {
            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
            {
                bool isSuccess = (int)resp.StatusCode < 299 && (int)resp.StatusCode >= 200;
                if (isSuccess)
                {
                    using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                    {
                        //We are limiting the array to 80 so we don't have
                        //to parse the entire html document feel free to 
                        //adjust (probably stay under 300)
                        char[] cs = new char[80];
                        reader.Read(cs, 0, cs.Length);
                        foreach (char ch in cs)
                        {
                            html += ch;
                        }
                    }
                }
            }
        }
        catch
        {
            return "";
        }
        return html;
    }

    public static GameResourcesData GetGameResourcesData()
    {
        return (GameResourcesData)Resources.Load("GameResourcesData", typeof(GameResourcesData));
    }

    public static Sprite GetSquareSprite(RewardQuality quality, ShopIconType iconType = ShopIconType.Small)
    {
        if (quality == RewardQuality.None) quality = RewardQuality.Common;
        //Debug.Log("Icon name = "+name+"; Icon type = "+iconType);
        if (iconType == ShopIconType.Small)
        {
            return GameResourcesManager.GetGameResourcesData().smallSquares[(int)quality - 1];
        }
        else if (iconType == ShopIconType.Medium)
        {
            return GameResourcesManager.GetGameResourcesData().bigSquares[(int)quality - 1];
        }
        else if (iconType == ShopIconType.Medium_Blueprint)
        {
            //Debug.Log("GETTING BLUEPRINT SQUARE");
            return GameResourcesManager.GetGameResourcesData().blueprintSquare;
        }
        return null;
    }
}
