using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyAttackState
{
    None,
    Ready,
    Startup,
    Active,
    Recovery,
    CD
}

public class EnemyAIAttack : MonoBehaviour
{
    EnemyAI myEnemyAI;
    public EnemyAttackState attackState = EnemyAttackState.None;
    float currentAttackTime = 0;
    public Transform hitboxParent;
    GameObject currentHB;
    public bool canAttack
    {
        get
        {
            return !myEnemyAI.staggered && myEnemyAI.state != EnemyAIState.Returning && attackState == EnemyAttackState.Ready;
        }
    }

    public void KonoAwake()
    {
        myEnemyAI = GetComponent<EnemyAI>();
        attackState = EnemyAttackState.Ready;
    }

    public void KonoUpdate()
    {
        ProcessAttacking();
    }

    public void StartAttacking()
    {
        if(canAttack)
        {
            ChangeAttackPhase(EnemyAttackState.Startup);
            myEnemyAI.state = EnemyAIState.Attacking;
        }
    }

    void ProcessAttacking()
    {
        switch (attackState)
        {
            case EnemyAttackState.Startup:
                if(currentAttackTime>= myEnemyAI.myEnemyData.attackData.startupPhase.duration)
                {
                    ChangeAttackPhase(EnemyAttackState.Active);
                }
                break;
            case EnemyAttackState.Active:
                if (currentAttackTime >= myEnemyAI.myEnemyData.attackData.activePhase.duration)
                {
                    ChangeAttackPhase(EnemyAttackState.Recovery);
                }
                break;
            case EnemyAttackState.Recovery:
                if (currentAttackTime >= myEnemyAI.myEnemyData.attackData.recoveryPhase.duration)
                {
                    ChangeAttackPhase(EnemyAttackState.CD);
                }
                break;
            case EnemyAttackState.CD:
                if (currentAttackTime >= myEnemyAI.myEnemyData.attackData.maxCDTime)
                {
                    ChangeAttackPhase(EnemyAttackState.Ready);
                }
                break;
        }
        currentAttackTime += Time.deltaTime;
    }

    void ChangeAttackPhase(EnemyAttackState _attackState)
    {
        attackState = _attackState;
        currentAttackTime = 0;
        switch (attackState)
        {
            case EnemyAttackState.Startup:
                currentHB = Instantiate(myEnemyAI.myEnemyData.attackData.hitboxPrefab, hitboxParent);
                currentHB.SetActive(false);
                break;
            case EnemyAttackState.Active:
                currentHB.SetActive(true);
                myEnemyAI.myEnemyAIMovement.StartImpulse();
                break;
            case EnemyAttackState.CD:
                Destroy(currentHB);
                myEnemyAI.StartPursuing(PlayerMovementCMF.instance.gameObject);
                break;
        }
    }

}
