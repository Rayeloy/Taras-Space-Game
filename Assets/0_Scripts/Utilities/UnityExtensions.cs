/* Author: Carlos Eloy Jose Sanz
 * https://www.linkedin.com/in/celoy-jose-sanz-505586156/
 * 
 * The MIT License (MIT)
 * 
 * Copyright (c) 2019
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * 
 * */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EloyExtensions
{
    public static class UnityExtensions
    {
        public static T[] Append<T>(this T[] array, T item)
        {
            if (array == null)
            {
                return new T[] { item };
            }
            T[] result = new T[array.Length + 1];
            array.CopyTo(result, 0);
            result[array.Length] = item;
            return result;
        }

        public static string CapitalizeFirstLetter(string str)
        {
            if (str.Length == 0) return str;

            if (str.Length == 1)
                return ("" + char.ToUpper(str[0]));
            else
                return (char.ToUpper(str[0]) + str.Substring(1));
        }

        public static void SetSpritesAlpha_r(Transform transf, float alpha)
        {
            Image image = transf.GetComponent<Image>();
            SpriteRenderer spriteRend = transf.GetComponent<SpriteRenderer>();

            if (image != null)
            {
                Color newColor = image.color;
                newColor.a = alpha;
                image.color = newColor;
            }
            if (spriteRend != null)
            {
                Color newColor = spriteRend.color;
                newColor.a = alpha;
                spriteRend.color = newColor;
            }

            for (int i = 0; i < transf.childCount; i++)
            {
                SetSpritesAlpha_r(transf.GetChild(i), alpha);
            }
        }

        public static void SetSortingLayer_r(Transform transf, string sortingLayerID)
        {
            SpriteRenderer spriteRend = transf.GetComponent<SpriteRenderer>();
            if (spriteRend && spriteRend.sortingLayerName != sortingLayerID) spriteRend.sortingLayerName = sortingLayerID;
            for (int i = 0; i < transf.childCount; i++)
            {
                SetSortingLayer_r(transf.GetChild(i), sortingLayerID);
            }
        }

        public static string AddThousandsSeparators(string numberInt)
        {
            int cumulativeUnits = 0;
            string result = "";
            for (int i = numberInt.Length - 1; i >= 0; i--)
            {
                cumulativeUnits++;
                result = numberInt[i] + result;
                if (cumulativeUnits == 3 && i > 0)
                {
                    cumulativeUnits = 0;
                    result = "." + result;
                }
            }
            return result;
        }

        public static T[] FindComponentsInChildrenWithTag<T>(this GameObject parent, string tag, bool forceActive = false) where T : Component
        {
            if (parent == null) { throw new System.ArgumentNullException(); }
            if (string.IsNullOrEmpty(tag) == true) { throw new System.ArgumentNullException(); }
            List<T> list = new List<T>(parent.GetComponentsInChildren<T>(forceActive));
            if (list.Count == 0) { return null; }

            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i].CompareTag(tag) == false)
                {
                    list.RemoveAt(i);
                }
            }
            return list.ToArray();
        }

        public static T FindComponentInChildWithTag<T>(this GameObject parent, string tag, bool forceActive = false) where T : Component
        {
            if (parent == null) { throw new System.ArgumentNullException(); }
            if (string.IsNullOrEmpty(tag) == true) { throw new System.ArgumentNullException(); }

            T[] list = parent.GetComponentsInChildren<T>(forceActive);
            foreach (T t in list)
            {
                if (t.CompareTag(tag) == true)
                {
                    return t;
                }
            }
            return null;
        }

        public static string IntArrayToString(int[] array)
        {
            string result = "(";
            for (int i = 0; i < array.Length; i++)
            {
                result += array[i].ToString() + (i == array.Length - 1 ? ")" : ",");
            }
            return result;
        }

        public static string FloatArrayToString(float[] array)
        {
            string result = "(";
            for (int i = 0; i < array.Length; i++)
            {
                result += array[i].ToString() + (i == array.Length - 1 ? ")" : ",");
            }
            return result;
        }

        public static string Vector2ArrayToString(Vector2[] array)
        {
            string result = "(";
            for (int i = 0; i < array.Length; i++)
            {
                result += "(" + array[i].x + "," + array[i].y + ")" + (i == array.Length - 1 ? ")" : ", ");
            }
            return result;
        }

        public static string Vector2ListToString(List<Vector2> array)
        {
            string result = "(";
            for (int i = 0; i < array.Count; i++)
            {
                result += "(" + array[i].x + "," + array[i].y + ")" + (i == array.Count - 1 ? ")" : ", ");
            }
            return result;
        }

        public static string Vector3ArrayToString(Vector3[] array)
        {
            string result = "(";
            for (int i = 0; i < array.Length; i++)
            {
                result += "(" + array[i].x + "," + array[i].y + ")" + (i == array.Length - 1 ? ")" : ", ");
            }
            return result;
        }

        public static List<T> CloneList<T>(List<T> referenceList)
        {
            List<T> newList = new List<T>();
            for (int i = 0; i < referenceList.Count; i++)
            {
                newList.Add(referenceList[i]);
            }
            return newList;
        }

        [System.Serializable]
        public class ParentRendBounds
        {
            public Vector3 Min
            {
                get
                {
                    return min;
                }
            }
            public Vector3 Max
            {
                get
                {
                    return max; ;
                }
            }
            public Vector3 Center
            {
                get
                {
                    return center; ;
                }
            }
            public Vector3 Extents
            {
                get
                {
                    return extents; ;
                }
            }
            Vector3 min, max, center, extents;
            Transform parent;
            List<Renderer> rendList;
            public ParentRendBounds(Transform _parent)
            {
                parent = _parent;
                rendList = new List<Renderer>();
                R_AddChildSRendToList(parent);
                UpdateBounds();
            }

            void R_AddChildSRendToList(Transform parent)
            {
                Renderer rend = parent.GetComponent<Renderer>();
                if (rend != null) rendList.Add(rend);

                for (int i = 0; i < parent.childCount; i++)
                {
                    if (parent.GetChild(i).gameObject.activeInHierarchy)
                        R_AddChildSRendToList(parent.GetChild(i));
                }
            }

            void UpdateBounds()
            {
                //Debug.Log("Updating SRend Bounds for " + parent.name);
                max.x = float.MinValue;
                min.x = float.MaxValue;

                max.y = float.MinValue;
                min.y = float.MaxValue;
                for (int i = 0; i < rendList.Count; i++)
                {
                    //Debug.Log("Updating bounds: child "+i+" " + sRendList[i].name+ "; new max X = "+ sRendList[i].bounds.max + "; old max.x = " + max.x);

                    if (rendList[i].bounds.max.x > max.x) max.x = rendList[i].bounds.max.x;
                    if (rendList[i].bounds.min.x < min.x) min.x = rendList[i].bounds.min.x;
                    if (rendList[i].bounds.max.y > max.y) max.y = rendList[i].bounds.max.y;
                    if (rendList[i].bounds.min.y < min.y) min.y = rendList[i].bounds.min.y;
                }
                extents.x = (max.x - min.x) / 2;
                extents.y = (max.y - min.y) / 2;
                center.x = min.x + extents.x;
                center.y = min.y + extents.y;
            }
        }
    }
}