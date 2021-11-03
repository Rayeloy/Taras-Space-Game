using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyAIState
{
    None,
    Waiting,
    Patrolling,
    Pursuing,
    Attacking,
    Returning,
    Staggered,
    Dying
}

public class EnemyAI : MonoBehaviour
{
    public EnemyAIMovement myEnemyAIMovement;
    public EnemyData myEnemyData;
    public EnemyAICliffCheck myCliffCheck;
    public EnemyAIWallCheck myWallCheck;
    public EnemyAIHealth myEnemyAIHealth;

    public EnemyAIState state = EnemyAIState.None;


    public Transform rotateObj;
    public Vector3 originalPosition;
    
    //WAITING
    public float currentTimeWaiting = 0;
    public float currentMaxTimeWaiting = 1;

    //PATROLING
    public float currentTimePatrolling = 0;
    public float currentMaxTimePatrolling = 1;
    public bool movingToRight = true;
    bool lastTurnAround = false;
    bool turnAround = false;

    //PURSUING
    GameObject pursuedPlayer;

    //STAGGERED
    public float maxTimeStaggered = 0.5f;
    public float currentTimeStaggered = 0;
    public bool staggered
    {
        get
        {
            return state == EnemyAIState.Staggered;
        }
    }

    protected virtual void Awake()
    {
        originalPosition = transform.position;
        myEnemyAIMovement.KonoAwake();
        myEnemyAIHealth.KonoAwake();
    }

    protected virtual void Start()
    {
        myEnemyAIMovement.KonoStart();       
    }

    protected virtual void Update()
    {
        myEnemyAIMovement.KonoUpdate();
    }
    protected virtual void FixedUpdate()
    {
        ProcessBehaviour();
        myEnemyAIMovement.KonoFixedUpdate();
    }
    protected virtual void LateUpdate()
    {
        myEnemyAIMovement.KonoLateUpdate();
    }

    void ProcessBehaviour()
    {
        switch (state)
        {
            case EnemyAIState.Waiting:

                if (currentTimeWaiting >= currentMaxTimeWaiting)
                {
                    StartPatrolling();
                }
                currentTimeWaiting += Time.deltaTime;
                break;
            case EnemyAIState.Patrolling:
                if (MaxDistanceToOriginCheck())
                {
                    break;
                }
                if (currentTimePatrolling >= currentMaxTimePatrolling)
                {
                    StartWaiting();
                }
                Vector3 distToOrigin = transform.position - originalPosition;
                if ((Mathf.Abs(distToOrigin.x) > myEnemyData.maxPatrolDistanceX &&
                    ((distToOrigin.x > 0 && movingToRight) || (distToOrigin.x < 0 && !movingToRight))))
                {
                        TurnAround();
                }
                lastTurnAround = turnAround;
                if (turnAround)
                {
                    movingToRight = !movingToRight;
                    turnAround = false;
                }
                currentTimePatrolling += Time.deltaTime;
                break;
            case EnemyAIState.Returning:
                distToOrigin = originalPosition - transform.position;
                if (Mathf.Abs(distToOrigin.x) < 0.5f)
                {
                    StartWaiting();
                    return;
                }
                movingToRight = distToOrigin.x >= 0 ? true : false;
                break;
            case EnemyAIState.Pursuing:
                if (MaxDistanceToOriginCheck())
                {
                    break;
                }
                Vector3 playerDir = pursuedPlayer.transform.position - transform.position;
                movingToRight = playerDir.x >= 0;
                if (myCliffCheck.cliffImminent)
                {
                    myEnemyAIMovement.noInput = true;
                }
                else if (myEnemyAIMovement.noInput) myEnemyAIMovement.noInput = false;
                break;
            case EnemyAIState.Staggered:
                if(currentTimeStaggered >= maxTimeStaggered)
                {
                    StopStaggered();
                }
                currentTimeStaggered += Time.deltaTime;
                break;

            case EnemyAIState.Dying:
                //if(EnemyAIAnimations.dyingAnimation.finished)
                //DeathVFX?
                Destroy(gameObject);
                return;
                break;
        }
    }

    void StartPatrolling()
    {
        state = EnemyAIState.Patrolling;
        currentTimePatrolling = 0;
        currentMaxTimePatrolling = Random.Range(myEnemyData.minPatrollingTime, myEnemyData.maxPatrollingTime);
        movingToRight = (int)Random.Range(0, 2) == 0 ? false : true;
    }

    public void TurnAround()
    {
        if (turnAround ||lastTurnAround) return;
        if(state == EnemyAIState.Patrolling)
        {
            turnAround = true;
        }
    }

    void StartWaiting()
    {
        state = EnemyAIState.Waiting;
        currentTimeWaiting = 0;
        currentMaxTimeWaiting = Random.Range(myEnemyData.minWaitingTime, myEnemyData.maxWaitingTime);
    }

    public void StartPursuing(GameObject player)
    {
        if (staggered || state == EnemyAIState.Pursuing ||state == EnemyAIState.Returning) return;
        state = EnemyAIState.Pursuing;
        pursuedPlayer = player;
    }

    public void StopPursuing()
    {
        StartPatrolling();
        pursuedPlayer = null;
        myEnemyAIMovement.noInput = false;
    }

    public void StartStaggered()
    {
        state = EnemyAIState.Staggered;
        currentTimeStaggered = 0;
    }

    public void StopStaggered()
    {
        state = EnemyAIState.Patrolling;
        StartPursuing(PlayerMovementCMF.instance.gameObject);
    }

    #region --- CHARACTER ROTATION ---
    public void RotateCharacter(bool right)
    {
        if (right)
        {
            rotateObj.localRotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            rotateObj.localRotation = Quaternion.Euler(0, -180, 0);
        }
    }
    #endregion

    bool MaxDistanceToOriginCheck()
    {
        float distToOriginX = Mathf.Abs((transform.position - originalPosition).x);
        if (distToOriginX > myEnemyData.maxDistanceToOriginX)
        {
            state = EnemyAIState.Returning;
            return true;
        }

        return false;
    }
}
