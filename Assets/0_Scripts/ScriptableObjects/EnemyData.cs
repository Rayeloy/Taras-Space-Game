using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyBehaviour
{
    None,
    Patrol,
    Ambush
}

[CreateAssetMenu(fileName = "New Enemy", menuName = "Enemy Data")]
public class EnemyData : ScriptableObject
{
    public float maxHealth;
    public float basicAttackDamage;
    [Header("--- PATROL ---")]
    public float maxPatrollingTime, minPatrollingTime;
    public float maxWaitingTime, minWaitingTime;
    public float maxPatrolDistanceX=6, maxDistanceToOriginX=12;
    public float movementSpeed = 4;

    [Header("--- ATTACK ---")]
    public EnemyAttackData attackData;

}
