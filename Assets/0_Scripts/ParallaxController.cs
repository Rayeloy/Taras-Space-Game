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
            float distToCamera = list[i].originalPosition.x - Camera.main.transform.position.x;
            float parallaxOffsetX = (distToCamera * layerSpeed * parallaxSensitivity);
            Vector3 newPos = list[i].originalPosition + Vector3.right * parallaxOffsetX;
            ParallaxRestrainer restrainer = list[i].transform.GetComponent<ParallaxRestrainer>();
            if (restrainer != null)
            {
                if (restrainer.restrainXToLeft && newPos.x < list[i].originalPosition.x) newPos.x = list[i].originalPosition.x;
                if(restrainer.restrainXToRight && newPos.x > list[i].originalPosition.x) newPos.x = list[i].originalPosition.x;
            }
            list[i].transform.position = newPos;
        }
    }
}

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


