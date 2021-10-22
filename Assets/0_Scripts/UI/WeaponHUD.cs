using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WeaponHUD : MonoBehaviour
{
    public static WeaponHUD instance;
    PlayerMovementCMF myPlayerCMF;

    public TextMeshProUGUI ammoText, totalAmmoText;
    public Image gunLogo;

    public UIAnimation reloadingAlphaAnim;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }
        else
        {
            instance = this;
        }
        myPlayerCMF = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovementCMF>();
    }

    private void Start()
    {
        Setup();
    }

    public void Setup()
    {
        gunLogo.sprite = myPlayerCMF.myPlayerWeapon.currentGun.gunLogo;
        UpdateAmmoText();
    }

    public void UpdateAmmoText()
    {
        if (myPlayerCMF.myPlayerWeapon.weaponSt != WeaponState.Reloading)
        {
            ammoText.text = myPlayerCMF.myPlayerWeapon.currentGun.currentBulletsInClip + "/" + myPlayerCMF.myPlayerWeapon.currentGun.maxClipSize;
        }
        totalAmmoText.text = ""+myPlayerCMF.myPlayerWeapon.currentGun.maxAmmoCapacity;
    }

    public void StartReloading()
    {
        UIAnimationsManager.instance.StartAnimation(reloadingAlphaAnim);
        ammoText.text = "Reloading";
    }

    public void StopReloading()
    {
        UIAnimationsManager.instance.StopUIAnimation(reloadingAlphaAnim);
        ammoText.color = new Color(ammoText.color.r, ammoText.color.g, ammoText.color.b, reloadingAlphaAnim.alphaMin);
        UpdateAmmoText();
    }
}
