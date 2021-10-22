using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;
    public GameObject currentVirtualCamera;
    public GameObject defaultVirtualCamera;
    public GameObject aimingVirtualCamera;

    public GameObject lastVirtualCamera;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }
    }

    public void SetToDefaultCamera()
    {
        if (currentVirtualCamera == defaultVirtualCamera) return;

        SetCamera(defaultVirtualCamera);
    }

    public void SetToAimingCamera()
    {
        if (!PlayerMovementCMF.instance.myPlayerWeapon.isAiming) return;
        if (currentVirtualCamera == aimingVirtualCamera) return;

        SetCamera(aimingVirtualCamera);
    }

    public void SetCamera(GameObject newCamera)
    {
        Debug.Log("SETTING NEW VIRTUAL CAMERA: " + newCamera.name);
        if (CameraManager.instance.currentVirtualCamera != null)
            currentVirtualCamera.SetActive(false);
        lastVirtualCamera = currentVirtualCamera;
        currentVirtualCamera = newCamera;
        if (CameraManager.instance.currentVirtualCamera != null)
            currentVirtualCamera.SetActive(true);

    }
}
