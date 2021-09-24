using System.Collections;
using UnityEditor;
using UnityEngine;
using System;
//using System.Net.NetworkInformation; NetworkInformation.Ping & UnityEngine.Ping have a conflict and I need Debug.Log so I commented this line
using System.Net;
using GleyInternetAvailability;

[CreateAssetMenu(menuName = "Managers/MasterManager")]
public class MasterManager : SingletonScriptableObject<MasterManager>
{
    [SerializeField]
    private GameResourcesManager _gameResourcesManager;
    public static GameResourcesManager GameResourcesManager { get { return Instance._gameResourcesManager; } }

    [SerializeField]
    private GameDataManager _gameData;
    public static GameDataManager GameDataManager { get { return Instance._gameData; } }

    [SerializeField]
    private MissionsDataManager _missionsData;
    public static MissionsDataManager MissionsDataManager { get { return Instance._missionsData; } }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void FirstInitialize()
    {
        //Debug.Log("This message will output before Awake.");
    }

    [SerializeField]
    public static bool connected;
    public static bool lastConnected;
    static float connectedTimer = 0;
    public static float connectedMaxTime = 5;

    public static void UpdateCheckInternetConnectionTimer()
    {
        connectedTimer += Time.deltaTime;
        if (connectedTimer >= connectedMaxTime)
        {
            CheckConnection();
            connectedTimer = 0;
        }
    }

    public static void CheckConnection()
    {
        GleyInternetAvailability.Network.IsAvailable(CompleteMethod);
    }

    private static void CompleteMethod(ConnectionResult connectionResult)
    {
        lastConnected = connected;
        if(connectionResult == ConnectionResult.Working)
        {
            //if(!lastConnected)Debug.Log("Connected");
            connected = true;
        }
        else
        {
            //if(lastConnected)Debug.Log("Disconnected");
            connected = false;
        }

        if(lastConnected != connected)
        {
            if (connected) ConnectedCallback();
            else DisconnectedCallback();
        }
    }

    public static void ConnectedCallback()
    {
        Debug.Log("CONNECTED");
    }

    public static void DisconnectedCallback()
    {

    }

    //private string linkURL;
    //public static void CheckConnection()
    //{
    //    string m_ReachabilityText = "";

    //    //Check if the device cannot reach the internet at all (that means if the "cable", "WiFi", etc. is connected or not)
    //    //if not, don't waste your time.
    //    if (Application.internetReachability == NetworkReachability.NotReachable)
    //    {
    //        m_ReachabilityText = "Not Connected.";
    //        //Debug.Log("Internet : " + m_ReachabilityText);
    //        connected = false;
    //    }
    //    else
    //    {
    //        GeneralPauseScript.pause.StartCoroutine(DoPing()); //It could be a network connection but not internet access so you have to ping your host/server to be sure.
    //    }
    //}
    //public static IEnumerator DoPing()
    //{
    //    lastConnected = connected;
    //    //Debug.Log("Do Ping");
    //    TestPing.DoPing();
    //    yield return new WaitUntil(() => TestPing.isDone);
    //    connected = TestPing.status;

    //    if (connected)
    //    {
    //        //Debug.Log("Connected");
    //        //Do your thing once the connection is confirmed
    //        //Debug.Log(TestPing.ipAdd); // just to be sure if that is your IP
    //        connected = true;
    //    }
    //    else
    //    {
    //        //if negative result awarn your user
    //        //and do your thing with this result
    //        //Debug.Log(TestPing.ipAdd);
    //        //Debug.Log("Please check your network connections or network permissions");
    //        connected = false;
    //    }
    //}
    //public static IEnumerator CheckInternetConnection()
    //{
    //    lastConnected = connected;
    //    UnityWebRequest request = new UnityWebRequest("http://google.com");
    //    float elapsedTime = 0.0f;

    //    //check for 10 sec, if more than 10 then fail
    //    while (!request.isDone)
    //    {
    //        elapsedTime += Time.deltaTime;
    //        if (elapsedTime >= 10f && request.downloadProgress <= 0.5f)
    //            break;
    //        yield return null;
    //    }

    //    if (!request.isDone || !string.IsNullOrEmpty(request.error))
    //    {
    //        Debug.LogWarning("OFFLINE!");
    //        connected = false;
    //    }
    //    else
    //    {
    //        Debug.LogWarning("CONNECTED");
    //        connected = true;
    //    }
    //    //UnityWebRequest www = new UnityWebRequest("http://google.com");
    //    //yield return www;
    //    //if (www.error != null)
    //    //{
    //    //    Debug.LogWarning("OFFLINE!");
    //    //    connected = false;
    //    //}
    //    //else
    //    //{
    //    //    Debug.LogWarning("CONNECTED");
    //    //    connected = true;
    //    //}
    //}

#if UNITY_EDITOR
    static public void RenameAsset(UnityEngine.Object asset, string newName, bool saveWhenFinished = true)
    {
        string assetPath = AssetDatabase.GetAssetPath(asset);
        AssetDatabase.RenameAsset(assetPath, newName);
        if (saveWhenFinished) AssetDatabase.SaveAssets();
    }
#endif
}

public static class TestPing
{
    public static bool status = false;
    public static bool isDone = false;
    public static string ipAdd; //The IP addres for the ping call

    public static bool PingThis()
    {
        try
        {
            //I strongly recommend to check Ping, Ping.Send & PingOptions on microsoft C# docu or other C# info source
            //in this block you configure the ping call to your host or server in order to check if there is network connection.

            //from https://stackoverflow.com/questions/55461884/how-to-ping-for-ipv4-only
            //from https://stackoverflow.com/questions/49069381/why-ping-timeout-is-not-working-correctly
            //and from https://stackoverflow.com/questions/2031824/what-is-the-best-way-to-check-for-internet-connectivity-using-net


            System.Net.NetworkInformation.Ping myPing = new System.Net.NetworkInformation.Ping();

            byte[] buffer = new byte[32]; //array that contains data to be sent with the ICMP echo
            int timeout = 10000; //in milliseconds
            System.Net.NetworkInformation.PingOptions pingOptions = new System.Net.NetworkInformation.PingOptions(64, true);
            System.Net.NetworkInformation.PingReply reply = myPing.Send(ipAdd, timeout, buffer, pingOptions); //the same method can be used without the timeout, data buffer & pingOptions overloadd but this works for me
            if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
            {
                return true;
            }
            else if (reply.Status == System.Net.NetworkInformation.IPStatus.TimedOut) //to handle the timeout scenario
            {
                return status;
            }
            else
            {
                return false;
            }
        }
        catch (Exception e) //To catch any exception of the method
        {
            Debug.Log(e);
            return false;
        }
        finally { } //To not get stuck in an error or exception, see "Try, Catch, Finally" docs.
    }

    public static string GetIPAddress() //Get the actual IP addres of your host/server
    {
        //Yes, I could use the "host name" or the "host IP address" direct on the ping.send method BUT!!
        //I find out and "Situation" in which due to my network setting in my PC any ping call (from script or cmd console)
        //returned the IPv6 instead of IPv4 which couse the Ping.Send thrown an exception
        //that could be the scenario for many of your users so you have to ensure this run for everyone.


        //from https://stackoverflow.com/questions/1059526/get-ipv4-addresses-from-dns-gethostentry

        IPHostEntry host;
        host = Dns.GetHostEntry("google.com"); //I use google.com as an example but it can be any host name (preferably yours)

        try
        {
            host = Dns.GetHostEntry("google.com"); //Get the IP host entry from your host/server
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
        finally { }


        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) //filter just the IPv4 IPs
            {                                                                      //you can play around with this and get all the IP arrays (if any)
                return ip.ToString();                                              //and check the connection with all of then if needed
            }
        }
        return string.Empty;
    }

    public static void DoPing()
    {
        ipAdd = GetIPAddress(); //call to get the IP address from your host/server

        if (PingThis()) //call to check if you can make ping to that host IP
        {
            status = true;
            isDone = true;
        }
        else
        {
            status = false;
            isDone = true;
        }
    }
}
