using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAIWallCheck : MonoBehaviour
{
    EnemyAI myEnemyAI;

    private void Awake()
    {
        myEnemyAI = GetComponentInParent<EnemyAI>();
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.isTrigger && other.CompareTag("Floor"))
        {
            myEnemyAI.TurnAround();
        }
    }
}
