using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//NOTA: Es un concepto básico de pool genérica, faltarú} refinarlo
//el defecto principal de este pool es que no acepta GameObjects
public abstract class GenericObjectPool<T> : MonoBehaviour where T : Component
{
    public bool incremental = true;

    [SerializeField] List<T> pool = new List<T>();

    public T GetElement()
    {
        return GetElement<T>((T element) => !element.gameObject.activeSelf,false);
    }

    public ElementType GetElement<ElementType>() where ElementType : T
    {
        Func<T, bool> sameTypeAndAvailableComparison = (T element) =>
        {
            Type searchType = typeof(ElementType);
            Type elementType = element.GetType();

            bool isSameType = (elementType == searchType);

            bool isDeactive = !element.gameObject.activeSelf;

            return isDeactive && isSameType;
        };

        return GetElement<ElementType>(sameTypeAndAvailableComparison, true) as ElementType;
    }

    private T GetElement<ElementType>(Func<T, bool> comparison, bool pureType) where ElementType : T
    {
        T element = FindElement(comparison);

        if (element)
        {
            element.gameObject.SetActive(true);
        }

        if(incremental && !element)
        {
            if (pureType)
            {
                element = NewPureElement<ElementType>();
            }
            else
            {
                element = NewElement<ElementType>();
            }

            if (element)
            {
                pool.Add(element);
            }
        }

        return element;
    }

    private T FindElement(Func<T, bool> comparison)
    {
        foreach(T element in pool)
        {
            if (comparison(element))
            {
                return element;
            }
        }

        return null;
    }

    protected virtual T NewElement()
    {
        return NewElement<T>();
    }

    protected virtual T NewPureElement<ElementType>() where ElementType : T
    {
        Func<T, bool> sameTypeComparison = (T element) =>
        {
            Type searchType = typeof(ElementType);
            Type elementType = element.GetType();

            bool isSameType = (elementType == searchType);

            return isSameType;
        };

        return NewElement<ElementType>(sameTypeComparison);
    }

    protected virtual T NewElement<ElementType>() where ElementType : T
    {
        Func<T, bool> isTypeComparison = (T element) =>
        {
            return element is ElementType;
        };

        return NewElement<ElementType>(isTypeComparison);
    }

    protected virtual T NewElement<ElementType>(Func<T, bool> comparison) where ElementType:T
    {
        T original = FindElement(comparison);

        if (original)
        {
            return Instantiate<T>(original, original.transform.parent);
        }
        else
        {
            return null;
        }
    }

    private void Reset()
    {
        pool.AddRange(GetComponentsInChildren<T>(true));
    }
}
