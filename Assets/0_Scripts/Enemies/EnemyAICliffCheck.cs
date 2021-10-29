using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAICliffCheck : MonoBehaviour
{
    EnemyAI myEnemyAI;
    public bool cliffImminent
    {
        get
        {
            return floorList.Count == 0;
        }
    }
    public List<GameObject> floorList;

    private void Awake()
    {
        myEnemyAI = GetComponentInParent<EnemyAI>();
        floorList = new List<GameObject>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Floor") && !floorList.Contains(other.gameObject))
        {
            floorList.Add(other.gameObject);
            Debug.Log("Floor Added to list");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Floor") && floorList.Contains(other.gameObject))
        {
            floorList.Remove(other.gameObject);
            if (cliffImminent) myEnemyAI.TurnAround();
        }
    }
}
