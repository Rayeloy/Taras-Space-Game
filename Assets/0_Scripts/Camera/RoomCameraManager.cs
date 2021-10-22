using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomCameraManager : MonoBehaviour
{
    public GameObject vCam;
    private Collider2D roomCollider;
    public bool dark;

    private void Awake()
    {
        roomCollider = GetComponent<Collider2D>();
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !other.isTrigger)
        {
            if (!PlayerMovementCMF.instance.myPlayerWeapon.isAiming)
            {
                float charMinX = other.bounds.center.x - other.bounds.extents.x - 0.2f;
                float charMaxX = other.bounds.center.x + other.bounds.extents.x + 0.2f;
                float charMinY = other.bounds.center.y - other.bounds.extents.y - 0.2f;
                float charMaxY = other.bounds.center.y + other.bounds.extents.y + 0.2f;
                if (charMinX > (roomCollider.bounds.center.x - roomCollider.bounds.extents.x) && charMaxX < (roomCollider.bounds.center.x + roomCollider.bounds.extents.x) &&
                    charMinY > (roomCollider.bounds.center.y - roomCollider.bounds.extents.y) && charMaxY < (roomCollider.bounds.center.y + roomCollider.bounds.extents.y))
                {
                    if (CameraManager.instance.currentVirtualCamera == vCam) return;
                    CameraManager.instance.SetCamera(vCam);
                    if (dark)
                    {
                        PlayerMovementCMF.instance.myPlayerVFX.EnterDarkZone();
                    }
                    else
                    {
                        PlayerMovementCMF.instance.myPlayerVFX.ExitDarkZone();
                    }
                }
            }
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !other.isTrigger)
        {
            if (CameraManager.instance.currentVirtualCamera == vCam && !PlayerMovementCMF.instance.myPlayerWeapon.isAiming)
            {
                CameraManager.instance.SetToDefaultCamera();
                PlayerMovementCMF.instance.myPlayerVFX.ExitDarkZone();
            }

        }
    }

}
