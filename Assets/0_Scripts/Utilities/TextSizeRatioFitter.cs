using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
[ExecuteAlways]
public class TextSizeRatioFitter : MonoBehaviour
{
    TextMeshProUGUI myText;
    public float ratioToHeight;
    private void OnEnable()
    {
        myText = GetComponent<TextMeshProUGUI>();
    }
    private void LateUpdate()
    {
        myText.fontSize = GetComponent<RectTransform>().rect.height * ratioToHeight;
    }
}
