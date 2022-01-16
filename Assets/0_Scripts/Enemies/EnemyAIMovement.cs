using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAIMovement : MonoBehaviour
{
    public EnemyAI myEnemyAI;
    public Mover mover;
    public CollisionsCheck2D collCheck;

    //GROUND VARIABLES


    //VARIABLES DE MOVIMIENTO

    [Header(" --- ROTATION --- ")]
    [Tooltip("Min angle needed in a turn to activate the instant rotation of the character (and usually the hardSteer mechanic too)")]
    [Range(0, 180)]
    public float instantRotationMinAngle = 120;
    float rotationRestrictedPercentage = 1;
    [HideInInspector]
    public float targetRotAngle;
    float rotationVelocity = 2;

    [Header("--- SPEED ---")]
    public float maxFallSpeed = 40;
    public float maxAscendSpeed = 40;
    public float maxMoveSpeed = 10;
    float maxAttackingMoveSpeed = 10;
    [Tooltip("Maximum speed that you can travel at horizontally when hit by someone")]
    public float maxKnockbackSpeed = 300f;
    //public float maxVerticalSpeedInWater = 10f; 
    public float maxFloatingSpeed = 2f;

    [Header(" --- ACCELERATIONS --- ")]
    public float initialAcc = 30;
    public float airInitialAcc = 30;
    public float breakAcc = -30;
    public float airBreakAcc = -5;
    public float movingAcc = 11.5f;
    public float airMovingAcc = 0.5f;
    //public float breakAccOnHit = -2.0f;
    float gravity;
    public float gravityMultiplier = 1;
    [HideInInspector]
    public float currentGravity;

    [Header(" --- JUMP --- ")]
    public float jumpHeight = 4f;
    public float jumpApexTime = 0.4f;
    float jumpVelocity;
    float timePressingJump = 0.0f;
    float maxTimePressingJump;
    [Tooltip("How fast the 'stop jump early' stops in the air. This value is multiplied by the gravity and then applied to the vertical speed.")]
    public float breakJumpForce = 2.0f;
    [Tooltip("During how much part of the jump (in time to reach the apex) is the player able to stop the jump. 1 is equals to the whole jump, and 0.5 is equals the half of the jump time.")]
    public float pressingJumpActiveProportion = 0.7f;
    public float maxTimeJumpInsurance = 0.2f;
    float timeJumpInsurance = 0;
    [HideInInspector]
    public bool jumpInsurance;



    //[Header("Body Mass")]
    //[Tooltip("Body Mass Index. 1 is for normal body mass.")]
    [HideInInspector] public float bodyMass;



    //MOVIMIENTO
    [Header("--- READ ONLY ---")]
    public MoveState moveSt = MoveState.NotMoving;
    public Vector3 currentVel;
    Vector3 oldCurrentVel;
    Vector3 finalVel;
    //BOOL PARA PERMITIR O BLOQUEAR INPUTS
    public bool noInput = false;

    public float currentSpeed = 0;
    //[HideInInspector]
    //public Vector3 currentMovDir;//= a currentInputDir??? 
    [HideInInspector]
    public Vector3 currentInputDir;
    [HideInInspector]
    public Vector3 currentFacingDir = Vector3.forward;
    [HideInInspector]
    public float facingAngle = 0;

    #region ----[ VARIABLES ]----  
    int frameCounter = 0;

    //Movement
    [HideInInspector]
    public float currentMaxMoveSpeed; // is the max speed from which we aply the joystick sensitivity value
    float finalMaxMoveSpeed = 10.0f; // its the final max speed, after the joyjoystick sensitivity value
    //bool hardSteer = false;

    //JUMP
    public VerticalMovementState vertMovSt = VerticalMovementState.None;


    //FIXED JUMPS (Como el trampolín)
    bool fixedJumping;
    bool fixedJumpDone;
    float fixedJumpMaxTime;
    float noMoveMaxTime;
    float noMoveTime;
    bool fixedJumpBounceEnabled = false;
    #endregion

    public virtual void KonoAwake()
    {
        myEnemyAI = GetComponent<EnemyAI>();
        mover = GetComponent<Mover>();
        collCheck.KonoAwake(mover.capsuleCollider);//we use capsule collider in our example

        currentSpeed = 0;
        noInput = false;
        maxMoveSpeed = myEnemyAI.myEnemyData.movementSpeed;
    }

    //todos los konoAwakes

    public void KonoStart()
    {
        gravity = -(2 * jumpHeight) / Mathf.Pow(jumpApexTime, 2);
        currentGravity = gravity * gravityMultiplier;
        jumpVelocity = Mathf.Abs(gravity * jumpApexTime);
        maxTimePressingJump = jumpApexTime * pressingJumpActiveProportion;


        finalMaxMoveSpeed = currentMaxMoveSpeed = maxMoveSpeed;
        //EquipWeaponAtStart();

        collCheck.KonoStart();
    }


    public void KonoUpdate()
    {
    }

    public void KonoFixedUpdate()
    {

        //Debug.LogWarning("Current pos = " + transform.position.ToString("F8"));
        lastPos = transform.position;


        Vector3 platformMovement = collCheck.ChangePositionWithPlatform(mover.instantPlatformMovement);

        collCheck.ResetVariables();
        ResetMovementVariables();

        collCheck.UpdateCollisionVariables(mover, vertMovSt, (fixedJumping && noInput));

        collCheck.UpdateCollisionChecks(currentVel);
        frameCounter++;

        #region --- Calculate Movement ---
        //Debug.Log("Pre Hor Mov: currentVel = " + currentVel.ToString("F6"));
        HorizontalMovement();
        //Debug.Log("Post Hor Mov: currentVel = " + currentVel.ToString("F6"));

        VerticalMovement();
        //Debug.Log("Post Vert Mov: currentVel = " + currentVel.ToString("F6"));

        finalVel = currentVel;
        HandleSlopes();

        #endregion
        //If the character is grounded, extend ground detection sensor range;
        mover.SetExtendSensorRange(collCheck.below);
        //Set mover velocity;
        mover.SetVelocity(finalVel, platformMovement);


        collCheck.SavePlatformPoint();
    }

    Vector3 lastPos;
    public void KonoLateUpdate()
    {

    }


    #region MOVEMENT -----------------------------------------------

    #region --- HORIZONTAL MOVEMENT ---
    public void SetVelocity(Vector3 vel)
    {
        currentVel = vel;
        Vector3 horVel = new Vector3(currentVel.x, 0, 0);
        currentSpeed = horVel.magnitude;
    }

    void ResetMovementVariables()
    {
        currentInputDir = Vector3.zero;
        oldCurrentVel = new Vector3(currentVel.x, 0, currentVel.z);
    }

    public void CalculateMoveDir()
    {

        if (!noInput)
        {
            float horiz = 0;
            float vert = 0;
            switch (myEnemyAI.state)
            {
                case EnemyAIState.Pursuing:
                case EnemyAIState.Returning:
                case EnemyAIState.Patrolling:
                    horiz = 1 * (myEnemyAI.movingToRight ? 1:-1);
                    break;
                default:
                    horiz = 0;
                    break;
            }


            // Check that they're not BOTH zero - otherwise dir would reset because the joystick is neutral.
            Vector3 temp = new Vector3(horiz, 0, vert);

            if (temp.magnitude > 0 && !collCheck.StopHorizontalOnSteepSlope(mover.GetGroundNormal(), temp))
            {
                moveSt = MoveState.Moving;
                currentInputDir = temp;
                currentInputDir.Normalize();
                myEnemyAI.RotateCharacter(temp.x >= 0);
            }
            else
            {
                moveSt = MoveState.NotMoving;
            }
        }
        else
        {
            moveSt = MoveState.None;
        }
    }

    void HorizontalMovement()
    {
        float finalMovingAcc = 0;
        Vector3 horizontalVel = new Vector3(currentVel.x, 0, 0);
        #region//------------------------------------------------ DECIDO TIPO DE MOVIMIENTO --------------------------------------------
        #region//----------------------------------------------------- Efecto externo --------------------------------------------
        #endregion
        #region //----------------------------------------------------- Efecto interno --------------------------------------------
        if (moveSt != MoveState.NotBreaking)
        {
            //------------------------------------------------ Direccion Joystick, aceleracion, maxima velocidad y velocidad ---------------------------------
            //------------------------------- Joystick Direction -------------------------------
            CalculateMoveDir();

            #region ------------------------------ Max Move Speed ------------------------------
            currentMaxMoveSpeed = maxMoveSpeed;//maxAttackingMoveSpeed == maxMoveSpeed if not attacking

            if (currentSpeed > (currentMaxMoveSpeed + 0.1f) && (moveSt == MoveState.Moving || moveSt == MoveState.NotMoving))
            {
                //Debug.LogWarning("Warning: moveSt set to MovingBreaking!: currentSpeed = "+currentSpeed+ "; maxMoveSpeed2 = " + maxMoveSpeed2 + "; currentVel.magnitude = "+currentVel.magnitude);
                moveSt = MoveState.MovingBreaking;
            }

            finalMaxMoveSpeed = moveSt == MoveState.MovingBreaking ? float.MaxValue : currentMaxMoveSpeed;
            //Debug.Log("Player "+playerNumber+": finalMaxMoveSpeed = " + finalMaxMoveSpeed);

            #endregion

            #region ------------------------------- Acceleration -------------------------------
            float finalAcc = 0;
            float finalBreakAcc = collCheck.below ? breakAcc : airBreakAcc;
            float finalInitialAcc = collCheck.below ? initialAcc : airInitialAcc;
            finalMovingAcc = (collCheck.below || collCheck.sliping ? movingAcc : airMovingAcc) * rotationRestrictedPercentage; //Turning accleration
                                                                                                                               //if (!disableAllDebugs && rotationRestrictedPercentage!=1) Debug.LogWarning("finalMovingAcc = " + finalMovingAcc+ "; rotationRestrictedPercentage = " + rotationRestrictedPercentage+
                                                                                                                               //    "; attackStg = " + myPlayerCombat.attackStg);
                                                                                                                               //finalBreakAcc = currentSpeed < 0 ? -finalBreakAcc : finalBreakAcc;

            switch (moveSt)
            {
                case MoveState.Moving:

                    finalAcc = finalInitialAcc;
                    break;
                case MoveState.NotMoving:
                    finalAcc = (currentSpeed == 0) ? 0 : finalBreakAcc;
                    break;
                default:
                    finalAcc = finalBreakAcc;
                    break;
            }
            #endregion

            #region ----------------------------------- Speed ---------------------------------- 
            if (!fixedJumping)
            {
                float currentSpeedB4 = currentSpeed;
                currentSpeed = currentSpeed + finalAcc * Time.deltaTime;
                //Hard Steer
                if (moveSt == MoveState.NotMoving && Mathf.Sign(currentSpeedB4) != Mathf.Sign(currentSpeed))
                {
                    currentSpeed = 0;
                }

                float maxSpeedClamp = finalMaxMoveSpeed;
                float minSpeedClamp = 0;
                currentSpeed = Mathf.Clamp(currentSpeed, minSpeedClamp, maxSpeedClamp);
                //Debug.Log("Player " + playerNumber + ": finalMaxMoveSpeed = " + finalMaxMoveSpeed+ "; maxSpeedClamp = "+ maxSpeedClamp + "; currentSpeed = " + currentSpeed);
            }
        }
        else CalculateMoveDir();
        #endregion
        #endregion
        #endregion
        #region//------------------------------------------------ PROCESO EL TIPO DE MOVIMIENTO DECIDIDO ---------------------------------
        Vector3 horVel = new Vector3(currentVel.x, 0, 0);
        //if (currentSpeed != 0) print("CurrentVel before processing= " + currentVel.ToString("F6") + "; currentSpeed =" + currentSpeed.ToString("F4") +
        //    "; MoveState = " + moveSt + "; currentMaxMoveSpeed = " + finalMaxMoveSpeed + "; below = " + collCheck.below + "; horVel.magnitude = " + horVel.magnitude+ "; finalMovingAcc = " + finalMovingAcc.ToString("F4"));
        horizontalVel = new Vector3(currentVel.x, 0, 0);
        switch (moveSt)
        {
            case MoveState.Moving: //MOVING WITH JOYSTICK
                Vector3 newDir;
                Vector3 oldDir = horizontalVel.magnitude == 0 ? myEnemyAI.rotateObj.forward.normalized : horizontalVel.normalized;
                newDir = oldDir + (currentInputDir * (finalMovingAcc * Time.deltaTime));
                float auxAngle = Vector3.Angle(oldCurrentVel, newDir);

                horizontalVel = newDir.normalized * currentSpeed;
                currentVel = new Vector3(horizontalVel.x, currentVel.y, 0);
                break;
            case MoveState.NotMoving: //NOT MOVING JOYSTICK
                horizontalVel = horizontalVel.normalized * currentSpeed;
                currentVel = new Vector3(horizontalVel.x, currentVel.y, 0);
                break;

            case MoveState.MovingBreaking://FRENADA FUERTE
                newDir = horizontalVel.normalized + (currentInputDir * finalMovingAcc * Time.deltaTime);
                horizontalVel = newDir.normalized * currentSpeed;
                currentVel = new Vector3(horizontalVel.x, currentVel.y, 0);
                break;
            case MoveState.NotBreaking:
                currentSpeed = horizontalVel.magnitude;
                break;
        }
        horVel = new Vector3(currentVel.x, 0, 0);
        //print("CurrentVel after processing= " + currentVel.ToString("F6") + "; CurrentSpeed 1.4 = " + currentSpeed + "; horVel.magnitude = " 
        //    + horVel.magnitude + "; currentInputDir = " + currentInputDir.ToString("F6"));
        #endregion
    }

    public void SetPlayerAttackMovementSpeed(float movementPercent)
    {
        maxAttackingMoveSpeed = movementPercent * maxMoveSpeed;
    }

    //not in use?
    public void ResetPlayerAttackMovementSpeed()
    {
        maxAttackingMoveSpeed = maxMoveSpeed;
    }

    public void SetPlayerRotationSpeed(float rotSpeedPercentage)
    {
        rotationRestrictedPercentage = rotSpeedPercentage;
    }

    public void ResetPlayerRotationSpeed()
    {
        rotationRestrictedPercentage = 1;
    }

    #endregion

    #region --- VERTICAL MOVEMENT ---

    void VerticalMovement()
    {
        //Debug.Log("Vert Mov Inicio: currentVel = " + currentVel.ToString("F6") + "; below = " + collCheck.below);

        if (collCheck.below)
        {
            maxTimePressingJump = jumpApexTime * pressingJumpActiveProportion;
        }


        //if (inputsInfo.JumpWasPressed)//Input.GetButtonDown(contName + "A"))
        //{
        //    PressA();
        //}


        //Debug.Log("Vel y pre = " + currentVel.y.ToString("F6"));
        switch (vertMovSt)
        {
            case VerticalMovementState.None:
                if (!mover.stickToGround && currentVel.y <= 0)
                {
                    mover.stickToGround = true;
                }
                if (currentVel.y < 0 && (!collCheck.below || collCheck.sliping))
                {
                    vertMovSt = VerticalMovementState.Falling;
                }
                currentVel.y += currentGravity * Time.deltaTime;
                break;
            case VerticalMovementState.Falling:

                if (currentVel.y >= 0 || (collCheck.below && !collCheck.sliping))
                {
                    //Debug.Log("STOP FALLING");
                    vertMovSt = VerticalMovementState.None;
                }

                currentVel.y += currentGravity * Time.deltaTime;
                break;
            //case VerticalMovementState.Jumping:
            //    currentVel.y += currentGravity * Time.deltaTime;
            //    timePressingJump += Time.deltaTime;
            //    if (timePressingJump >= maxTimePressingJump)
            //    {
            //        StopJump();
            //    }
            //    else
            //    {
            //        //This condition is actually correct, first one is for release stored on buffer from Update.
            //        //Second one is for when you tap too fast the jump button
            //        //if (inputsInfo.JumpWasReleased || !actions.A.IsPressed)                          
            //        //{
            //        //    vertMovSt = VerticalMovementState.JumpBreaking;
            //        //}
            //    }
                //break;
            //case VerticalMovementState.JumpBreaking:
            //    if (currentVel.y <= 0)
            //    {
            //        ResetJumpState();
            //    }
            //    currentVel.y += (currentGravity * breakJumpForce) * Time.deltaTime;
            //    break;

        }
        //ProcessJumpInsurance();

        if (!fixedJumping)
        {
            currentVel.y = Mathf.Clamp(currentVel.y, -maxFallSpeed, maxAscendSpeed);
        }

        if (collCheck.above)
        {
            currentVel.y = 0;
        }
        if ((collCheck.below) && !collCheck.sliping && mover.stickToGround && !collCheck.onSlide)
        {
            currentVel.y = 0;
        }

        //Debug.Log(" collCheck .below = " + collCheck.below + "; collCheck.tooSteepSlope = " + collCheck.tooSteepSlope);
        if (collCheck.StopHorizontalOnSteepSlope(mover.GetGroundNormal(), finalVel))
        {
            finalVel.x = 0;
            if (finalVel.y > 0) finalVel.y = 0;
        }
    }

    void ResetJumpState()
    {
        vertMovSt = VerticalMovementState.None;
        mover.stickToGround = true;
    }

    void HandleSlopes()
    {
        if (collCheck.sliping)
        {
            // HORIZONTAL VELOCITY
            Vector3 horVel = new Vector3(currentVel.x, 0, currentVel.z);

            //VERTICAL VELOCITY
            //Apply slide gravity along ground normal, if controller is sliding;
            if (currentVel.y < 0)
            {
                Vector3 _slideDirection = Vector3.ProjectOnPlane(-Vector3.up, mover.GetGroundNormal()).normalized;
                Debug.DrawRay(mover.GetGroundPoint(), _slideDirection * 2, Color.yellow);
                Vector3 slipVel = _slideDirection * -currentVel.y;
                finalVel = slipVel + horVel;
            }
        }
    }

    #endregion

    public void StartImpulse()
    {
        if(myEnemyAI.state == EnemyAIState.Attacking)
        {
            Vector3 impulse = myEnemyAI.myEnemyData.attackData.impulseDirection *(myEnemyAI.rotateObj.localRotation.eulerAngles.y == 0?1:-1)* myEnemyAI.myEnemyData.attackData.impulseForce;
            currentVel = impulse;
            Debug.Log("CurrentVel = " + currentVel);
        }
    }

    #endregion
}
