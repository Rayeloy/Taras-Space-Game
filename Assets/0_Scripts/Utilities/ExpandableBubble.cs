using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class ExpandableBubble : MonoBehaviour
{
    public Image leftBorder;
    public Image rightBorder;
    public Image expandableMiddle;
    public TMPro.TextMeshProUGUI text;
    public float widthPerCharacter = 1;
    public Transform smallPos;
    public Transform bigPos;

    private void LateUpdate()
    {
        if (transform.lossyScale.x == 0) return;
        if (text.rectTransform.rect.width <= 0) return;

        float percentageSizeTextByRect = text.textBounds.size.x / text.rectTransform.rect.width;
        //if (transform.parent.name == "PopUpIconNewGem") Debug.Log("percentageSizeTextByRect = " + percentageSizeTextByRect+ "; text.textBounds.size.x = " + text.textBounds.size.x);
        if (text.text.Length > 1)
        {
            expandableMiddle.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, text.textBounds.size.x - 30 * (text.textBounds.size.x * 0.007f)-
                (14 * (1-percentageSizeTextByRect)));
        }
        else
        {
            float percentageCharSizeByMaxCharSize = text.textBounds.size.x / 26.74f;
            expandableMiddle.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0);
        }

        float bubbleWidth = (rightBorder.transform.localPosition.x - leftBorder.transform.localPosition.x) / 2;
        float middleX = leftBorder.transform.localPosition.x + bubbleWidth;
        //float widthDiff = text.rectTransform.rect.width / 2 - bubbleWidth;
        //float textXOffset = 0;
        //if (widthDiff > 0)
        //{
        //    textXOffset = widthDiff + 
        //}
        text.transform.localPosition= new Vector3(middleX, text.transform.localPosition.y, text.transform.localPosition.z);

        /*widthPerCharacter * text.text.Length - 1*/
        expandableMiddle.transform.localPosition = leftBorder.transform.localPosition + Vector3.right * expandableMiddle.rectTransform.rect.width / 2;
        rightBorder.transform.localPosition = expandableMiddle.transform.localPosition + Vector3.right* expandableMiddle.rectTransform.rect.width / 2;
        if(text.text.Length > 1)
        {
            //Debug.Log("expandableMiddle.width / 2 = "+(expandableMiddle.rectTransform.rect.width / 2));
            float distToCenter = expandableMiddle.rectTransform.rect.width / 2;
            transform.localPosition = bigPos.localPosition + Vector3.left * distToCenter+Vector3.left * 1;
        }
        else
        {
            transform.localPosition = smallPos.localPosition;
        }
    }
}
