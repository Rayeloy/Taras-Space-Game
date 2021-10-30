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

    Cinemachine.CinemachineConfiner aimingVCamConfiner;

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
        aimingVCamConfiner = aimingVirtualCamera.GetComponent<Cinemachine.CinemachineConfiner>();
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

        if (currentVirtualCamera != null)
        {
            Cinemachine.CinemachineConfiner confiner = currentVirtualCamera.GetComponent<Cinemachine.CinemachineConfiner>();
            if (confiner != null)
            {
                aimingVCamConfiner.enabled = true;
                aimingVCamConfiner.m_BoundingShape2D = confiner.m_BoundingShape2D;
            }
            else
            {
                aimingVCamConfiner.enabled = false;
            }
        }
        else
        {
            aimingVCamConfiner.enabled = false;
        }
        SetCamera(aimingVirtualCamera);
    }

    public void SetCamera(GameObject newCamera)
    {
        //Debug.Log("SETTING NEW VIRTUAL CAMERA: " + newCamera.name);
        if (CameraManager.instance.currentVirtualCamera != null)
            currentVirtualCamera.SetActive(false);
        lastVirtualCamera = currentVirtualCamera;
        currentVirtualCamera = newCamera;
        if (CameraManager.instance.currentVirtualCamera != null)
            currentVirtualCamera.SetActive(true);

    }
}
