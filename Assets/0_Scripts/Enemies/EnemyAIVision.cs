using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAIVision : MonoBehaviour
{
    EnemyAI myEnemyAI;
    GameObject player = null;

    private void Awake()
    {
        myEnemyAI = GetComponentInParent<EnemyAI>();
    }

    private void Update()
    {
        if(player != null)
        {
            myEnemyAI.StartPursuing(player);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("enemy vision collided with " + other.name);
        if(!other.isTrigger && other.CompareTag("Player"))
        {
            player = other.gameObject;
            myEnemyAI.StartPursuing(player);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.isTrigger && other.CompareTag("Player") && player!=null)
        {
            player = null;
            myEnemyAI.StopPursuing();
        }
    }
}
