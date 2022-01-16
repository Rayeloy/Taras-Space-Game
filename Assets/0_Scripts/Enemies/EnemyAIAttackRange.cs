using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAIAttackRange : MonoBehaviour
{

    EnemyAI myEnemyAI;

    private void Awake()
    {
        myEnemyAI = GetComponentInParent<EnemyAI>();
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if(!other.isTrigger && other.CompareTag("Player"))
        {
            myEnemyAI.myEnemyAIAttack.StartAttacking();
        }
    }
}
