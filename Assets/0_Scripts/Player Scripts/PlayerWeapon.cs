using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

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
    public int currentGunIndex = 0;

    bool startedShooting = false;
    float shootingTime = 0;

    bool reloadStarted = false;
    float reloadTime = 0;
    bool triggerPressed = false;
    int currentBurstShots = 0;

    [Header("--- CAMERA MOVEMENT ON AIM ---")]
    public float aimCameraDist = 1;
    Vector3 targetCameraFollowObjPos;
    float cameraFollowObjSmoothSpeedX;
    float cameraFollowObjSmoothSpeedY;
    public float cameraFollowObjSmoothTime=0.4f;
    public CinemachineVirtualCamera vCam;
    public bool isAiming
    {
        get
        {
            return virtualJoystickRight.joystickInput.magnitude >= virtualJoystickRight.deadZone;
        }
    }


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
        ResetAimPosition();
    }

    public void KonoStart()
    {
        currentGunIndex = -1;
        SwitchWeapon(true);
        ReloadInstanly();
    }

    public void KonoUpdate()
    {
        ProcessJoystickInput();
        ProcessShooting();
        ProcessReloadGun();
        UpdateCameraFollowObjOnAim();
        if (currentGunIndex >= 0)
        {
            myPlayerMov.myPlayerAnimations.ActivateAttatchment("weapon", "image0");
        }
        if (isAiming)
        {
            myPlayerMov.myPlayerAnimations.SetAimingPose();
        }
        else
        {
            myPlayerMov.myPlayerAnimations.SetNotAimingPose();
        }
    }

    void ProcessJoystickInput()
    {
        //Aiming or shooting
        if (isAiming)
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
            myPlayerMov.cameraFollow.localPosition = Vector3.zero;
            EndShooting();
            ResetAimPosition();
        }
    }

    #region --- SHOOTING ---
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
        int bulletsToShoot = currentGun.bulletsPerShot;
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
        currentGun.currentBulletsInClip -= 1;
        MasterManager.GameDataManager.SaveWeaponsState();

        WeaponHUD.instance.UpdateAmmoText();
    }
    #endregion

    #region --- RELOADING ---
    void StartReloadGun()
    {
        if (MasterManager.GameDataManager.GetReward(currentGun.ammoType) > 0 && currentGun.currentBulletsInClip<currentGun.maxClipSize && !reloadStarted)
        {
            reloadStarted = true;
            reloadTime = 0;
            weaponSt = WeaponState.Reloading;
            //Reload Spine Animation

            //Reload sound

            WeaponHUD.instance.StartReloading();
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
            WeaponHUD.instance.StopReloading();
        }
    }

    void ReloadInstanly()
    {
        if(currentGun.currentBulletsInClip< currentGun.maxClipSize && MasterManager.GameDataManager.GetReward(currentGun.ammoType) > 0)
        {
            currentGun.Reload();
            WeaponHUD.instance.StopReloading();
        }
    }
    #endregion

    #region --- AIM POSITION ---
    void UpdateAim()
    {
        float inputAngle = Vector2.SignedAngle(Vector2.up, virtualJoystickRight.joystickInput);
        aimTargetParent.localRotation = Quaternion.Euler(0, 0, inputAngle);
        if (virtualJoystickRight.joystickInput.x >= 0) myPlayerMov.RotateCharacter(true);
        else myPlayerMov.RotateCharacter(false);
        CameraManager.instance.SetToAimingCamera();
    }

    public void ResetAimPosition()
    {
        if (isAiming) return;

        if(myPlayerMov.rotateObj.localRotation.eulerAngles.y == 180)
        aimTargetParent.localRotation = Quaternion.Euler(0, 0, 90);
        else aimTargetParent.localRotation = Quaternion.Euler(0, 0, -90);
    }
    #endregion

    public void SwitchWeapon(bool right)
    {
        if (right)
        {
            if (currentGunIndex == (MasterManager.GameDataManager.allGuns.Length - 1))
            {
                currentGunIndex = -1;
                currentGun = null;
            }
            else
            {
                currentGunIndex++;
                currentGun = MasterManager.GameDataManager.allGuns[currentGunIndex];
            }
        }
        else
        {
            if (currentGunIndex == 0)
            {
                currentGunIndex = -1;
                currentGun = null;
            }
            else
            {
                if (currentGunIndex == -1) currentGunIndex = (MasterManager.GameDataManager.allGuns.Length - 1);
                else currentGunIndex--;
                currentGun = MasterManager.GameDataManager.allGuns[currentGunIndex];
            }
        }

        //Update Interface
        if (currentGunIndex == -1)
        {
            myPlayerMov.myPlayerAnimations.SetNoWeaponPose();
            WeaponHUD.instance.SetupNoWeapon();
            myPlayerMov.myPlayerAnimations.ActivateAttatchment("weapon", null);
        }
        else
        {
            WeaponHUD.instance.Setup();
            myPlayerMov.myPlayerAnimations.SetNotAimingPose();
            myPlayerMov.myPlayerAnimations.ActivateAttatchment("weapon", "image0");
        }
    }

    //CAMERA
    void UpdateCameraFollowObjOnAim()
    {
        if (isAiming)
        {
            targetCameraFollowObjPos = virtualJoystickRight.joystickInput.normalized * aimCameraDist;
        }
        else
        {
            targetCameraFollowObjPos = Vector3.zero;
        }
        Vector3 currentCameraFollowObjPos = myPlayerMov.cameraFollow.localPosition;
        //currentCameraFollowObjPos.x = Mathf.SmoothDamp(currentCameraFollowObjPos.x, targetCameraFollowObjPos.x, ref cameraFollowObjSmoothSpeedX, cameraFollowObjSmoothTime);
        //currentCameraFollowObjPos.y = Mathf.SmoothDamp(currentCameraFollowObjPos.y, targetCameraFollowObjPos.y, ref cameraFollowObjSmoothSpeedY, cameraFollowObjSmoothTime);
        currentCameraFollowObjPos.x = Mathf.Lerp(currentCameraFollowObjPos.x, targetCameraFollowObjPos.x, cameraFollowObjSmoothTime);
        currentCameraFollowObjPos.y = Mathf.Lerp(currentCameraFollowObjPos.y, targetCameraFollowObjPos.y, cameraFollowObjSmoothTime);
        myPlayerMov.cameraFollow.localPosition = currentCameraFollowObjPos;
    }

}
