using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GunType
{
    None,
    Pistol,
    Machinegun,
    Shotgun,
    GrenadeLauncher,
    Raygun
}

public enum FiringMode
{
    None,
    Semi_Automatic,
    Automatic,
    Burst,
    Ray
}

public enum SpreadType
{
    None,
    Equidistant,
    Random
}

[CreateAssetMenu(fileName = "New gun data", menuName = "Gun Data")]
public class GunData : ScriptableObject
{
    public bool unlocked = false;
    public GunType gunType = GunType.None;
    public FiringMode firingMode = FiringMode.None;
    public int bulletsPerShot = 1;
    public float maxRange = 20;
    public float shootFrequency = 0.1f;
    public float shootingKnockback = 1;
    [Tooltip("Shotgun would be like 15, the reset maybe like 0-2")]
    public float spreadAngle = 0;
    public SpreadType spreadType = SpreadType.None;
    public int burstAmount = 3;

    [Header("--- RELOAD / AMMO ---")]
    public int maxAmmoCapacity = 200;
    public int maxClipSize = 20;
    public float reloadMaxTime = 1.5f;

    [Header("--- BULLET ---")]
    public float bulletDamage = 1;
    public float bulletSpeed = 20;
    public float bulletDropForce = 0;

    public GameObject bulletPrefab;
    [Tooltip("To activate it or deactivate it through Spine")]
    public string spineWeaponName;


    [Header("--- READ ONLY ---")]
    public int currentBulletsInClip = 0;

    public void Reload()
    {
        int bulletsNeeded = maxClipSize - currentBulletsInClip;
        int bulletsReloaded = Mathf.Clamp(bulletsNeeded, 1, MasterManager.GameDataManager.GetReward(RewardType.Ammo));
        currentBulletsInClip += bulletsReloaded;
        MasterManager.GameDataManager.AddReward(RewardType.Ammo, -bulletsReloaded);
        MasterManager.GameDataManager.SaveWeaponsState();
    }

}
