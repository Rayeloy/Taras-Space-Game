using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponState
{
    None,
    Aiming,
    Shooting,
    Reloading
}
public class PlayerWeapon : MonoBehaviour
{
    PlayerMovementCMF myPlayerMov;
    public VirtualJoystick virtualJoystickRight;
    public float joystickMinInputMagnitudeToShoot = 0.8f;
    public Transform BulletsParent;
    public Transform aimTarget;
    Transform aimTargetParent;

    public GunData currentGun;

    bool startedShooting = false;
    float shootingTime = 0;

    bool reloadStarted = false;
    float reloadTime = 0;
    bool triggerPressed = false;
    int currentBurstShots = 0;

    [Header("--- READ ONLY ---")]
    public WeaponState weaponSt = WeaponState.None;

    private void OnEnable()
    {
        virtualJoystickRight.OnSinglePress += StartReloadGun;

    }

    private void OnDisable()
    {
        virtualJoystickRight.OnSinglePress -= StartReloadGun;
    }

    public void KonoAwake()
    {
        myPlayerMov = GetComponent<PlayerMovementCMF>();
        aimTargetParent = aimTarget.parent;
    }

    public void KonoUpdate()
    {
        ProcessJoystickInput();
        ProcessShooting();
        ProcessReloadGun();
    }

    void ProcessJoystickInput()
    {
        if (virtualJoystickRight.joystickInput.magnitude >= virtualJoystickRight.deadZone)
        {
            UpdateAim();

            if (virtualJoystickRight.joystickInput.magnitude < joystickMinInputMagnitudeToShoot)//AIM
            {
                weaponSt = WeaponState.Aiming;
                triggerPressed = false;
                EndShooting();
            }
            else//SHOOT
            {
                StartShooting();
            }
        }
        else
        {
            EndShooting();
        }
    }

    void StartShooting()
    {
        if (currentGun.firingMode == FiringMode.Burst && triggerPressed) return;
        if (!startedShooting && currentGun.currentBulletsInClip > 0)
        {
            weaponSt = WeaponState.Shooting;
            startedShooting = true;
            if(!(triggerPressed && currentGun.firingMode == FiringMode.Semi_Automatic))shootingTime = currentGun.shootFrequency;
        }else if(!startedShooting && currentGun.currentBulletsInClip == 0)
        {
            StartReloadGun();
        }
    }

    void ProcessShooting()
    {
        if (startedShooting)
        {
            if (currentGun.currentBulletsInClip == 0)
            {
                EndShooting();
                return;
            }
            if (shootingTime >= currentGun.shootFrequency)
            {
                Shoot();
                shootingTime = 0;
                if(currentGun.firingMode == FiringMode.Semi_Automatic)
                {
                    EndShooting();
                }
                else if(currentGun.firingMode == FiringMode.Burst && currentBurstShots >= currentGun.burstAmount)
                {
                    EndShooting();
                }
            }

                shootingTime += Time.deltaTime;
        }
    }

    void EndShooting()
    {
        if (startedShooting)
        {
            startedShooting = false;
            weaponSt = WeaponState.None;
            currentBurstShots = 0;
        }
    }

    void Shoot()
    {
        triggerPressed = true;
        int bulletsToShoot= Mathf.Clamp(currentGun.bulletsPerShot, 1, currentGun.currentBulletsInClip);
        for (int i = 0; i < bulletsToShoot; i++)
        {
            //Spawn bullets
            GameObject auxBullet = Instantiate(currentGun.bulletPrefab, BulletsParent);
            auxBullet.transform.position = aimTarget.position;
            //Shooting Dir
            Vector2 shootingDir = aimTarget.position - aimTargetParent.position;
            //Spread?
            if (currentGun.spreadAngle > 0 && currentGun.spreadType != SpreadType.None)
            {
                float finalAngle = 0;
                switch (currentGun.spreadType)
                {
                    case SpreadType.Random:
                        finalAngle = Random.Range(-currentGun.spreadAngle, currentGun.spreadAngle);
                        break;
                    case SpreadType.Equidistant:
                        float totalAngle = currentGun.spreadAngle * 2;
                        float angleSpacing = totalAngle / (currentGun.bulletsPerShot-1);
                        finalAngle = -currentGun.spreadAngle + (angleSpacing*i);
                        break;
                }

                shootingDir = Quaternion.AngleAxis(finalAngle, Vector3.forward) * (Vector3)shootingDir;
            }
            auxBullet.GetComponent<Bullet>().KonoAwake(currentGun.bulletSpeed, currentGun.bulletDamage, currentGun.maxRange, shootingDir);
        }

        //Gun VFX

        //Gun shot sound


        if (currentGun.firingMode == FiringMode.Burst) currentBurstShots++;
        currentGun.currentBulletsInClip -= bulletsToShoot;
        MasterManager.GameDataManager.SaveWeaponsState();
    }


    void StartReloadGun()
    {
        if (MasterManager.GameDataManager.GetReward(RewardType.Ammo) > 0 && currentGun.currentBulletsInClip<currentGun.maxClipSize && !reloadStarted)
        {
            reloadStarted = true;
            reloadTime = 0;
            weaponSt = WeaponState.Reloading;
            //Reload Spine Animation

            //Reload sound

        }
    }

    void ProcessReloadGun()
    {
        if (reloadStarted)
        {
            if (reloadTime >= currentGun.reloadMaxTime)
            {
                EndReloadGun();
                return;
            }
            reloadTime += Time.deltaTime;
        }
    }

    void EndReloadGun()
    {
        if (reloadStarted)
        {
            reloadStarted = false;
            currentGun.Reload();
            weaponSt = WeaponState.None;
        }
    }

    void UpdateAim()
    {
        float inputAngle = Vector2.SignedAngle(Vector2.up, virtualJoystickRight.joystickInput);
        aimTargetParent.localRotation = Quaternion.Euler(0, 0, inputAngle);
        if (virtualJoystickRight.joystickInput.x >= 0) myPlayerMov.RotateCharacter(true);
        else myPlayerMov.RotateCharacter(false);
    }

}
