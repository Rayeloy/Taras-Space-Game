using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAIHealth : MonoBehaviour
{
    EnemyAI myEnemyAI;
    public float currentHealth;

    public void KonoAwake()
    {
        myEnemyAI = GetComponent<EnemyAI>();
        currentHealth = myEnemyAI.myEnemyData.maxHealth;
    }

    public void ReceiveDamage(float damageAmount)
    {
        if (myEnemyAI.state == EnemyAIState.Dying || currentHealth<=0) return;
        currentHealth -= damageAmount;
        if (currentHealth <= 0) StartDeath();
        else
        {
            myEnemyAI.StartStaggered();
        }
    }

    public void StartDeath()
    {
        //Death animation
        myEnemyAI.state = EnemyAIState.Dying;
    }
}
