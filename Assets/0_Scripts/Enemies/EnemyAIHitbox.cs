using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAIHitbox : MonoBehaviour
{
    bool playerAlreadyHit = false;
    public EnemyAI myEnemyAI;

    private void Awake()
    {
        myEnemyAI = GetComponentInParent<EnemyAI>();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if(!other.isTrigger && other.CompareTag("Player"))
        {
            Vector2 knockback = (other.transform.position - myEnemyAI.transform.position).normalized;
            knockback *= myEnemyAI.myEnemyData.attackData.knockbackForce;
            other.GetComponent<PlayerHealth>().ReceiveDamage(myEnemyAI.myEnemyData.attackData.damage,knockback);
        }
    }
}
