using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EqualizeTextSizeMode
{
    None,
    Smallest,
    Biggest,
    AutoSize
}

[ExecuteAlways]
public class TMP_Extension_TextGroup : MonoBehaviour
{
    public TMPro.TextMeshProUGUI[] texts;
    public EqualizeTextSizeMode equalizeTextSizeMode = EqualizeTextSizeMode.Smallest;

    private void LateUpdate()
    {
        if (texts.Length <= 1) return;
        if(equalizeTextSizeMode == EqualizeTextSizeMode.AutoSize)
        {
            for (int i = 0; i < texts.Length; i++)
            {
                texts[i].enableAutoSizing = true;
                texts[i].ForceMeshUpdate();
            }
            return;
        }

        for (int i = 0; i < texts.Length; i++)
        {
            texts[i].enableAutoSizing = true;
            texts[i].ForceMeshUpdate();
        }
        switch (equalizeTextSizeMode)
        {
            case EqualizeTextSizeMode.Smallest:
                float minSize = float.MaxValue;
                for (int i = 0; i < texts.Length; i++)
                {
                    if (texts[i].fontSize < minSize) minSize = texts[i].fontSize;
                }
                //Debug.Log("minSize = " + minSize);
                for (int i = 0; i < texts.Length; i++)
                {
                    texts[i].fontSize = minSize;
                }
                break;
            case EqualizeTextSizeMode.Biggest:
                float maxSize = float.MinValue;
                for (int i = 0; i < texts.Length; i++)
                {
                    if (texts[i].fontSize > maxSize) maxSize = texts[i].fontSize;
                }

                for (int i = 0; i < texts.Length; i++)
                {
                    texts[i].fontSize = maxSize;
                }
                break;
        }
        for (int i = 0; i < texts.Length; i++)
        {
            texts[i].enableAutoSizing = false;
        }
    }
}
