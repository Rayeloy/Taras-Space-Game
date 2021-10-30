using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    public Transform backgroundParent, middlegroundMinus1Parent, middlegroundParent, foregroundParent;
    public float parallaxSensitivity = 1;
    public float backgroundSpeed,middlegroundMinus1Speed, middlegroundSpeed, foregroundSpeed;

    [Header("--- READ ONLY ---")]
    public List<ParallaxItem> backgroundList;
    public List<ParallaxItem> middlegroundMinus1List;
    public List<ParallaxItem> middlegroundList;
    public List<ParallaxItem> foregroundList;

    float cameraWorldWidth;

    private void Awake()
    {
        backgroundList = new List<ParallaxItem>();
        middlegroundMinus1List = new List<ParallaxItem>();
        middlegroundList = new List<ParallaxItem>();
        foregroundList = new List<ParallaxItem>();

        FillParallaxList(backgroundList, backgroundParent);
        FillParallaxList(middlegroundMinus1List, middlegroundMinus1Parent);
        FillParallaxList(middlegroundList, middlegroundParent);
        FillParallaxList(foregroundList, foregroundParent);

        float aspect = (float)Screen.width / Screen.height;

        float worldHeight = Camera.main.orthographicSize * 2;

        cameraWorldWidth = worldHeight * aspect;
    }

    private void Update()
    {
        UpdateParallaxListPositions(backgroundList, backgroundSpeed);
        UpdateParallaxListPositions(middlegroundMinus1List, middlegroundMinus1Speed);
        UpdateParallaxListPositions(middlegroundList, middlegroundSpeed);
        UpdateParallaxListPositions(foregroundList, foregroundSpeed);
    }

    void FillParallaxList(List<ParallaxItem> list, Transform parallaxParent)
    {
        for (int i = 0; i < parallaxParent.childCount; i++)
        {
            list.Add(new ParallaxItem(parallaxParent.GetChild(i), parallaxParent.GetChild(i).position));
        }
    }

    void UpdateParallaxListPositions(List<ParallaxItem> list, float layerSpeed)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Vector3 cameraCenter = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, Camera.main.nearClipPlane));
            float distToCamera = list[i].originalPosition.x - cameraCenter.x;
            float parallaxOffsetX = (distToCamera * layerSpeed * parallaxSensitivity);
            Vector3 newPos = list[i].originalPosition + Vector3.right * parallaxOffsetX;
            ParallaxRestrainer restrainer = list[i].transform.GetComponent<ParallaxRestrainer>();
            Debug.Log("Object: "+list[i].transform.name+"; CameraCenter = " + cameraCenter + "; distToCamera = "+ distToCamera + "; parallaxOffsetX = "+ parallaxOffsetX + "; newPos = "+ newPos);
            if (restrainer != null)
            {
                if (restrainer.restrainXToLeft && newPos.x < list[i].originalPosition.x) newPos.x = list[i].originalPosition.x;
                if(restrainer.restrainXToRight && newPos.x > list[i].originalPosition.x) newPos.x = list[i].originalPosition.x;
                float cameraOffset =-(cameraWorldWidth / 2 * restrainer.cameraOffsetX);
                newPos.x += cameraOffset;
                Debug.Log("Restrainer found! = "+ cameraOffset + "; newPos.x = "+ newPos.x + ";restrainer.restrainXToLeft  = "+ restrainer.restrainXToLeft + "; restrainer.restrainXToRight = " + restrainer.restrainXToRight);
            }
            list[i].transform.position = newPos;
        }
    }
}

[System.Serializable]
public class ParallaxItem
{
    public Transform transform;
    public Vector3 originalPosition;
    public ParallaxItem(Transform _transform, Vector3 _originalPosition)
    {
        transform = _transform;
        originalPosition = _originalPosition;
    }
}


