using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum EloyUIGroupPosition
{
    None,
    Middle,
    Right,
    Left
}

[ExecuteAlways]
public class EloyUIHorizontalGroup : MonoBehaviour
{
    private Vector2 resolution;

    public EloyUIGroupPosition disposition = EloyUIGroupPosition.Middle;
    public bool setChildSizeX = true;
    public bool setChildSizeY = true;
    public bool noSpace = false;
    [Tooltip("Instead of counting the rect width of the child TextMP, count the text size.")]
    public bool countTMPCharSize = true;

    private void Awake()
    {
        if (Application.isPlaying)
            resolution = new Vector2(Screen.width, Screen.height);

        UpdateChildren();
    }

    void LateUpdate()
    {
        if (Application.isPlaying)
        {
            if(MenuTiendaScript.instance != null)
            {
                if (MenuTiendaScript.instance.state == ShopMenuState.None) return;
            }
            else
            {
                return;
            }
            if (resolution.x != Screen.width || resolution.y != Screen.height)//On resolution change
            {
                // do your stuff
                UpdateChildren();

                resolution.x = Screen.width;
                resolution.y = Screen.height;
            }
            else if (transform.hasChanged)
            {
                UpdateChildren();
            }
        }
        else
        {
            UpdateChildren();
        }
    }

    void UpdateChildren()
    {
        RectTransform[] children = GetComponentsInChildren<RectTransform>();
        children = children.Skip(1).ToArray();
        float childrenWidth = 0;
        float childrenHeight = 0;

        childrenWidth = GetComponent<RectTransform>().rect.width / transform.childCount;
        if (setChildSizeY)
        {
            childrenHeight = GetComponent<RectTransform>().rect.height;
        }
        for (int i = 0; i < transform.childCount; i++)
        {
            if (setChildSizeX) transform.GetChild(i).GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, childrenWidth);
            if (setChildSizeY) transform.GetChild(i).GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, childrenHeight);
        }
        float childrenTotalWidth = 0;
        switch (disposition)
        {
            case EloyUIGroupPosition.Middle:
                if (noSpace)
                {
                    for (int i = 0; i < children.Length; i++)
                    {
                        //Debug.Log("child: " + children[i].name + "; size x = " + GetWidth(children[i]));
                        childrenTotalWidth += GetWidth(children[i]);
                    }
                    //Debug.Log("ChildrenTotalWidth = " + childrenTotalWidth);
                    float cumulativeChildrenWidthMiddle = 0;
                    for (int i = 0; i < children.Length; i++)
                    {
                        if (countTMPCharSize)
                        {
                            TMPro.TextMeshProUGUI textMP = children[i].GetComponent<TMPro.TextMeshProUGUI>();
                            if(textMP != null)
                            {
                                float charToRectDiff = Mathf.Clamp(GetWidth(children[i],true) - textMP.textBounds.size.x,0,float.MaxValue);
  
                                //Debug.Log("TEXT FOUND: charToRectDiff = " + charToRectDiff);
                                if (i == 0)
                                {
                                    cumulativeChildrenWidthMiddle += GetWidth(children[i],true)/2-charToRectDiff / 2;
                                    children[i].localPosition = new Vector3(-(childrenTotalWidth / 2) + cumulativeChildrenWidthMiddle, 0, 0);
                                }
                                else
                                {
                                    children[i].localPosition = new Vector3(-(childrenTotalWidth / 2) + cumulativeChildrenWidthMiddle + GetWidth(children[i - 1]) / 2 +
                                        GetWidth(children[i], true) / 2 - charToRectDiff / 2, 0, 0);
                                    cumulativeChildrenWidthMiddle += GetWidth(children[i - 1]) / 2 + GetWidth(children[i]) / 2;
                                    //Debug.Log("GetWidth(children[i - 1]) /2 = " + (GetWidth(children[i - 1]) / 2) + "; GetWidth(children[i]) / 2 = " + (GetWidth(children[i]) / 2));
                                }

                            }
                            else
                            {
                                if (i == 0) cumulativeChildrenWidthMiddle += GetWidth(children[i]) / 2;
                                else cumulativeChildrenWidthMiddle += GetWidth(children[i - 1]) / 2 + GetWidth(children[i]) / 2;
                                children[i].localPosition = new Vector3(-(childrenTotalWidth / 2) + cumulativeChildrenWidthMiddle, 0, 0);
                            }
                            //Debug.Log("childrenTotalWidth / 2 = "+(childrenTotalWidth/2)+"; cumulativeChildrenWidthMiddle = " + cumulativeChildrenWidthMiddle);
                        }
                        else
                        {
                            if (i == 0) cumulativeChildrenWidthMiddle += GetWidth(children[i]) / 2;
                            else cumulativeChildrenWidthMiddle += GetWidth(children[i - 1]) / 2 + GetWidth(children[i]) / 2;
                            children[i].localPosition = new Vector3(-(childrenTotalWidth / 2) + cumulativeChildrenWidthMiddle, 0, 0);
                        }
                    }

                }
                else
                {
                    float xSpacing = childrenWidth / 2;
                    float minX = -GetComponent<RectTransform>().rect.width / 2 + xSpacing;

                    //Debug.Log("children = " + transform.childCount + "; xSpacing = " + xSpacing + "; minX = " + minX + "; childrenWidth = " + childrenWidth + "; childrenHeight = " + childrenHeight);
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        transform.GetChild(i).GetComponent<RectTransform>().localPosition = new Vector3(minX + (childrenWidth * i), 0, 0);
                    }
                }
                break;
            case EloyUIGroupPosition.Right:
                for (int i = 0; i < children.Length; i++)
                {
                    childrenTotalWidth += GetWidth(children[i]);
                }

                float cumulativeChildrenWidthRight = 0;
                for (int i = children.Length-1; i >= 0; i--)
                {
                    if (i == children.Length-1) cumulativeChildrenWidthRight += GetWidth(children[i],true) / 2;
                    else cumulativeChildrenWidthRight += GetWidth(children[i + 1],true) / 2 + GetWidth(children[i],true) / 2;
                    children[i].localPosition = new Vector3(GetComponent<RectTransform>().rect.width / 2 - cumulativeChildrenWidthRight, 0, 0);
                }
                break;
        }
    }

    float GetWidth(RectTransform rect, bool getAlwaysRectSize=false)
    {
        if (countTMPCharSize && !getAlwaysRectSize)
        {
            TMPro.TextMeshProUGUI textMP = rect.GetComponent<TMPro.TextMeshProUGUI>();
            if (textMP != null)
            {
                return textMP.textBounds.size.x;
            }
            else
            {
                return rect.rect.width;
            }
        }
        else
        {
            return rect.rect.width;
        }
    }
}
