using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Enemy Attack", menuName = "Enemy Attack Data")]
public class EnemyAttackData : ScriptableObject
{
    public GameObject hitboxPrefab;
    public AttackPhase startupPhase;
    public AttackPhase activePhase;
    public AttackPhase recoveryPhase;
    public float maxCDTime;
    public float damage;
    public Vector2 impulseDirection;
    public float impulseForce;
    //public Vector2 knockbackDirection;
    public float knockbackForce;
}

[System.Serializable]
public class AttackPhase
{
    public float duration;
}