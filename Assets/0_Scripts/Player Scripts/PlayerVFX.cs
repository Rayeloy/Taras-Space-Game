using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVFX : MonoBehaviour
{
    PlayerMovementCMF myPlayerMov;
    public GameObject playerAuraLight;
    public GameObject outsideGlobalLight;
    public GameObject interiorGlobalLight;


    public void KonoAwake()
    {
        myPlayerMov = GetComponent<PlayerMovementCMF>();
        ExitDarkZone();
    }

    public void EnterDarkZone()
    {
        playerAuraLight.SetActive(true);
        outsideGlobalLight.SetActive(false);
        interiorGlobalLight.SetActive(true);
    }

    public void ExitDarkZone()
    {
        playerAuraLight.SetActive(false);
        outsideGlobalLight.SetActive(true);
        interiorGlobalLight.SetActive(false);
    }
}
