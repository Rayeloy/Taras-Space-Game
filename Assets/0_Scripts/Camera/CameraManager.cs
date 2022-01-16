using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using ActionCode.Cinemachine;

public class CameraManager : MonoBehaviour
    {
        public static CameraManager instance;
        public GameObject currentVirtualCamera;
        public GameObject defaultVirtualCamera;
        public GameObject aimingVirtualCamera;

        public GameObject lastVirtualCamera;

        CinemachineRegionsConfiner aimingVCamConfiner;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(this);
                return;
            }
            aimingVCamConfiner = aimingVirtualCamera.GetComponent<CinemachineRegionsConfiner>();
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
            CinemachineRegionsConfiner confiner = currentVirtualCamera.GetComponent<CinemachineRegionsConfiner>();
                if (confiner != null)
                {
                    aimingVCamConfiner.enabled = true;
                    aimingVCamConfiner.regionsData = confiner.regionsData;
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
