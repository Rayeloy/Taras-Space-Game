using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JetpackHUD : MonoBehaviour
{
    public static JetpackHUD instance;
    public Transform maxPos;
    public Transform minPos;
    public Transform fillImage;
    float totalFillDistance;
    public Transform minFuelIndicator;
    PlayerMovementCMF myPlayerCMF;
    public Image border;

    public Color readyColor, notReadyColor;

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
        totalFillDistance = (maxPos.localPosition - minPos.localPosition).magnitude;
        myPlayerCMF = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovementCMF>();
    }

    private void Start()
    {
        Setup();
    }

    private void Update()
    {
        UpdateFuelAmount();
    }

    public void Setup()
    {
        minFuelIndicator.localPosition = minPos.localPosition + (Vector3.up * totalFillDistance * myPlayerCMF.myPlayerJetpack.jetpackMinFuelToStartPercent)+
            (Vector3.up*fillImage.GetComponent<RectTransform>().rect.height/2);
    }

    public void UpdateFuelAmount()
    {
        float fillPercent = Mathf.Clamp01(myPlayerCMF.myPlayerJetpack.currentFuel / myPlayerCMF.myPlayerJetpack.totalFuel);
        fillImage.localPosition = minPos.localPosition + (Vector3.up * totalFillDistance * fillPercent);
        if (fillPercent < myPlayerCMF.myPlayerJetpack.jetpackMinFuelToStartPercent)
        {
            border.color = notReadyColor;
            minFuelIndicator.GetComponent<Image>().color = notReadyColor;
        }
        else
        {
            border.color = readyColor;
            minFuelIndicator.GetComponent<Image>().color = readyColor;
        }
    }


}
