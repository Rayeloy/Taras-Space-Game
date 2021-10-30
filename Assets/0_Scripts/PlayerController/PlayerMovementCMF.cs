using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#region ----[ PUBLIC ENUMS ]----
public enum Team
{
    A = 1,// Blue - Green
    B = 2,// Red - Pink
    none = 0
}

public enum MoveState
{
    None = 0,
    Moving = 1,
    NotMoving = 2,//Not stunned, breaking
    MovingBreaking = 4,//Moving but reducing speed by breakAcc till maxMovSpeed
    NotBreaking = 8,
}

public enum VerticalMovementState
{
    None,
    Jumping,
    JumpBreaking,//Emergency stop
    Falling,
    Jetpack
}

public enum ForceType
{
    Additive,
    Forced
}

#endregion

#region ----[ REQUIRECOMPONENT ]----
#endregion
public class PlayerMovementCMF : MonoBehaviour
{
    public static PlayerMovementCMF instance;
    [Header(" --- Referencias --- ")]
    //public PlayerCombat myPlayerCombat;
    public CollisionsCheck2D collCheck;
    public Mover mover;
    public VirtualJoystick leftJoystick;
    public LayerMask UILayerMask;
    public PlayerJetpack myPlayerJetpack;
    public PlayerAnimations myPlayerAnimations;
    public PlayerWeapon myPlayerWeapon;
    public PlayerVFX myPlayerVFX;


    public Transform cameraFollow;
    public Transform rotateObj;


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
    public float maxMoveSpeedBackwards = 4;
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
    public float gravityMultiplier=1;
    [HideInInspector]
    public float currentGravity;
    public float gravityMultiplierOnFalling = 3f;

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
    [HideInInspector]
    public int jumpTouchID = -1;



    //[Header("Body Mass")]
    //[Tooltip("Body Mass Index. 1 is for normal body mass.")]
    [HideInInspector] public float bodyMass;

    //BOOL PARA PERMITIR O BLOQUEAR INPUTS
    [HideInInspector]
    public bool noInput = false;

    //MOVIMIENTO
    [Header("--- READ ONLY ---")]
    public MoveState moveSt = MoveState.NotMoving;
    public Vector3 currentVel;
    Vector3 oldCurrentVel;
    Vector3 finalVel;

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

    //JOYSTICK INPUT
    float joystickAngle;
    float deadzone = 0.2f;
    float lastJoystickSens = 0;
    float joystickSens = 0;

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

    #region ----[ MONOBEHAVIOUR FUNCTIONS ]----

    #region AWAKE

    public void KonoAwake(bool isMyCharacter = false)
    {
        if (instance == null) instance = this;
        else
        {
            Destroy(this);
            return;
        }

         mover = GetComponent<Mover>();
        collCheck.KonoAwake(mover.capsuleCollider);//we use capsule collider in our example

        currentSpeed = 0;
        noInput = false;

        PlayerAwakes();
    }

    //todos los konoAwakes
    void PlayerAwakes()
    {
        myPlayerJetpack.KonoAwake();
        myPlayerAnimations.KonoAwake();
        myPlayerWeapon.KonoAwake();
        myPlayerVFX.KonoAwake();
    }
    #endregion

    #region START
    public void KonoStart()
    {
        gravity = -(2 * jumpHeight) / Mathf.Pow(jumpApexTime, 2);
        currentGravity = gravity* gravityMultiplier;
        jumpVelocity = Mathf.Abs(gravity * jumpApexTime);
        maxTimePressingJump = jumpApexTime * pressingJumpActiveProportion;


        finalMaxMoveSpeed = currentMaxMoveSpeed = maxMoveSpeed;
        //EquipWeaponAtStart();

        PlayerStarts();
    }
    private void PlayerStarts()
    {
        collCheck.KonoStart();
        myPlayerWeapon.KonoStart();
    }

    #endregion

    #region UPDATE
    public void KonoUpdate()
    {
        CheckForScreenPress();
        CheckForScreenRelease();
        myPlayerJetpack.KonoUpdate();
        myPlayerAnimations.KonoUpdate();
        myPlayerWeapon.KonoUpdate();
    }

    public void KonoFixedUpdate()
    {

        //Debug.LogWarning("Current pos = " + transform.position.ToString("F8"));
        lastPos = transform.position;


        Vector3 platformMovement = collCheck.ChangePositionWithPlatform(mover.instantPlatformMovement);

        collCheck.ResetVariables();
        ResetMovementVariables();

        collCheck.UpdateCollisionVariables(mover, vertMovSt,(fixedJumping && noInput));

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
    #endregion

    #endregion

    #region MOVEMENT -----------------------------------------------

    #region --- HORIZONTAL MOVEMENT ---
    public void SetVelocity(Vector3 vel)
    {
        currentVel = vel;
        Vector3 horVel = new Vector3(currentVel.x, 0, currentVel.z);
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
            float horiz = leftJoystick.joystickInput.x;
            float vert = leftJoystick.joystickInput.y;

            // Check that they're not BOTH zero - otherwise dir would reset because the joystick is neutral.
            Vector3 temp = new Vector3(horiz, 0, vert);
            lastJoystickSens = joystickSens;
            joystickSens = temp.magnitude;

            if (temp.magnitude >= deadzone && !collCheck.StopHorizontalOnSteepSlope(mover.GetGroundNormal(), temp))
            {
                joystickSens = joystickSens >= 0.8f ? 1 : joystickSens;//Eloy: esto evita un "bug" por el que al apretar el joystick 
                                                                       //contra las esquinas no da un valor total de 1, sino de 0.9 o así
                moveSt = MoveState.Moving;
                currentInputDir = temp;
                currentInputDir.Normalize();
                if(!myPlayerWeapon.isAiming) RotateCharacter(temp.x >= 0);
            }
            else
            {
                joystickSens = 1;//no estoy seguro de que esté bien
                moveSt = MoveState.NotMoving;
            }
        }
    }

    void HorizontalMovement()
    {
        float finalMovingAcc = 0;
        Vector3 horizontalVel = new Vector3(currentVel.x, 0, currentVel.z);
        #region//------------------------------------------------ DECIDO TIPO DE MOVIMIENTO --------------------------------------------
        #region//----------------------------------------------------- Efecto externo --------------------------------------------
        #endregion
        #region //----------------------------------------------------- Efecto interno --------------------------------------------
        if (moveSt != MoveState.NotBreaking)
        {
            //------------------------------------------------ Direccion Joystick, aceleracion, maxima velocidad y velocidad ---------------------------------
            //------------------------------- Joystick Direction -------------------------------
            CalculateMoveDir();//Movement direction
            //ProcessHardSteer();

            #region ------------------------------ Max Move Speed ------------------------------
            currentMaxMoveSpeed = (currentVel.x < 0 && rotateObj.localRotation.eulerAngles.y == 0) || (currentVel.x > 0 && rotateObj.localRotation.eulerAngles.y == 180)?maxMoveSpeedBackwards:
                maxMoveSpeed;//maxAttackingMoveSpeed == maxMoveSpeed if not attacking

            if (currentSpeed > (currentMaxMoveSpeed + 0.1f) && (moveSt == MoveState.Moving || moveSt == MoveState.NotMoving))
            {
                //Debug.LogWarning("Warning: moveSt set to MovingBreaking!: currentSpeed = "+currentSpeed+ "; maxMoveSpeed2 = " + maxMoveSpeed2 + "; currentVel.magnitude = "+currentVel.magnitude);
                moveSt = MoveState.MovingBreaking;
            }

            finalMaxMoveSpeed = moveSt == MoveState.MovingBreaking ? float.MaxValue :
                //Moving while reducing joystick angle? If not normal parameters.
                lastJoystickSens > joystickSens && moveSt == MoveState.Moving ? (lastJoystickSens / 1) * currentMaxMoveSpeed : (joystickSens / 1) * currentMaxMoveSpeed;
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

                        finalAcc = lastJoystickSens > joystickSens ? finalBreakAcc : finalInitialAcc;
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
                float minSpeedClamp = (lastJoystickSens > joystickSens && moveSt == MoveState.Moving) ? (joystickSens / 1) * currentMaxMoveSpeed : 0;
                currentSpeed = Mathf.Clamp(currentSpeed, minSpeedClamp, maxSpeedClamp);
                //Debug.Log("Player " + playerNumber + ": finalMaxMoveSpeed = " + finalMaxMoveSpeed+ "; maxSpeedClamp = "+ maxSpeedClamp + "; currentSpeed = " + currentSpeed);
            }
        }
        else CalculateMoveDir();
        #endregion
        #endregion
        #endregion
        #region//------------------------------------------------ PROCESO EL TIPO DE MOVIMIENTO DECIDIDO ---------------------------------
        Vector3 horVel = new Vector3(currentVel.x, 0, currentVel.z);
        //if (currentSpeed != 0) print("CurrentVel before processing= " + currentVel.ToString("F6") + "; currentSpeed =" + currentSpeed.ToString("F4") +
        //    "; MoveState = " + moveSt + "; currentMaxMoveSpeed = " + finalMaxMoveSpeed + "; below = " + collCheck.below + "; horVel.magnitude = " + horVel.magnitude+ "; finalMovingAcc = " + finalMovingAcc.ToString("F4"));
        horizontalVel = new Vector3(currentVel.x, 0, currentVel.z);
        switch (moveSt)
        {
            case MoveState.Moving: //MOVING WITH JOYSTICK
                Vector3 newDir;
                    Vector3 oldDir = horizontalVel.magnitude == 0 ? rotateObj.forward.normalized : horizontalVel.normalized;
                    newDir = oldDir + (currentInputDir * (finalMovingAcc * Time.deltaTime));
                    float auxAngle = Vector3.Angle(oldCurrentVel, newDir);

                horizontalVel = newDir.normalized * currentSpeed;
                currentVel = new Vector3(horizontalVel.x, currentVel.y, horizontalVel.z);
                break;
            case MoveState.NotMoving: //NOT MOVING JOYSTICK
                horizontalVel = horizontalVel.normalized * currentSpeed;
                currentVel = new Vector3(horizontalVel.x, currentVel.y, horizontalVel.z);
                break;

            case MoveState.MovingBreaking://FRENADA FUERTE
                newDir = horizontalVel.normalized + (currentInputDir * finalMovingAcc * Time.deltaTime);
                horizontalVel = newDir.normalized * currentSpeed;
                currentVel = new Vector3(horizontalVel.x, currentVel.y, horizontalVel.z);
                break;
            case MoveState.NotBreaking:
                currentSpeed = horizontalVel.magnitude;
                break;
        }
        horVel = new Vector3(currentVel.x, 0, currentVel.z);
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

                currentVel.y += currentGravity * gravityMultiplierOnFalling * Time.deltaTime;
                break;
            case VerticalMovementState.Jumping:
                currentVel.y += currentGravity * Time.deltaTime;
                timePressingJump += Time.deltaTime;
                if (timePressingJump >= maxTimePressingJump)
                {
                    StopJump();
                }
                else
                {
                    //This condition is actually correct, first one is for release stored on buffer from Update.
                    //Second one is for when you tap too fast the jump button
                    //if (inputsInfo.JumpWasReleased || !actions.A.IsPressed)                          
                    //{
                    //    vertMovSt = VerticalMovementState.JumpBreaking;
                    //}
                }
                break;
            case VerticalMovementState.JumpBreaking:
                if (currentVel.y <= 0)
                {
                    ResetJumpState();
                }
                currentVel.y += (currentGravity * breakJumpForce) * Time.deltaTime;
                break;
            case VerticalMovementState.Jetpack:
                float currentJetpackForce = currentVel.y < 0? myPlayerJetpack.jetpackForce * 3: myPlayerJetpack.jetpackForce;
                currentVel.y += (currentGravity + currentJetpackForce) * Time.deltaTime;
                currentVel.y = Mathf.Clamp(currentVel.y, -maxFallSpeed, myPlayerJetpack.jetpackMaxSpeed);
                break;

        }
        ProcessJumpInsurance();

        if (!fixedJumping)
        {
            currentVel.y = Mathf.Clamp(currentVel.y, -maxFallSpeed, maxAscendSpeed);
        }

        if (collCheck.above)
        {
            StopJump();
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
            //Vector3 wallNormal = mover.GetGroundNormal();
            //wallNormal.y = 0;
            //wallNormal = -wallNormal.normalized;
            //float angle = Vector3.Angle(wallNormal, horVel);
            //float a = Mathf.Sin(angle * Mathf.Deg2Rad) * horVel.magnitude;
            //Vector3 slideVel = Vector3.Cross(wallNormal, Vector3.up).normalized;
            //Debug.DrawRay(mover.GetGroundPoint(), slideVel * 2, Color.green);
            ////LEFT OR RIGHT ORIENTATION?
            //float ang = Vector3.Angle(slideVel, horVel);
            //slideVel = ang > 90 ? -slideVel : slideVel;
            ////print("SLIDE ANGLE= " + angle + "; vel = " + vel + "; slideVel = " + slideVel.ToString("F4") + "; a = " + a + "; wallAngle = " + wallAngle + "; distanceToWall = " + rayCast.distance);
            //slideVel *= a;

            //horVel = new Vector3(slideVel.x, 0, slideVel.z);

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

    #endregion

    #region JUMP ---------------------------------------------------
    void PressA()
    {
        Debug.Log("TRY JUMP");
        if (!StartJump())
        {
            Debug.Log("FAILED JUMP... TRY JETPACK");
            myPlayerJetpack.StartJetpack();
        }
    }

    #region --- Jump ---
    void CheckForScreenPress()
    {
        int count = Input.touchCount;
        bool found = false;
        Vector2 localPoint = Vector2.zero;

        for (int i = 0; i < count && !found; i++)
        { // verify all touches
            Touch touch = Input.GetTouch(i);
            int id = touch.fingerId;
            //Debug.Log("Touch Input! ID = " + id);
            if ((leftJoystick.joystickPressed && id == leftJoystick.touchID) || (myPlayerWeapon.virtualJoystickRight.joystickPressed && id == myPlayerWeapon.virtualJoystickRight.touchID))
                continue;
            if (touch.phase != TouchPhase.Began) continue;

            if (EventSystem.current.IsPointerOverGameObject(id))
            {
                // ui touched
                continue;
            }
            else 
            {
                found = true;
                jumpTouchID = id;
            }
            //// if touch inside some button Rect, set the corresponding value
            //if( Physics.Raycast(new Ray(touch.position, Vector3.forward),100, UILayerMask))

        }
        if (found)
        {
            PressA();
        }
    }

    void CheckForScreenRelease()
    {
        if (jumpTouchID != -1)
        {
            int count = Input.touchCount;
            bool found = false;
            Vector2 localPoint = Vector2.zero;

            for (int i = 0; i < count && !found; i++)
            { // verify all touches
                Touch touch = Input.GetTouch(i);
                int id = touch.fingerId;
                if(id == jumpTouchID)
                {
                    found = true;
                }
            }
            if (!found)
            {
                StopJump();
                myPlayerJetpack.EndJetpack();
            }
        }
    }

    bool StartJump(bool calledFromBuffer = false)
    {
        bool result = false;
        if (!noInput)
        {
            if (((collCheck.below && !collCheck.sliping) || jumpInsurance))
            {
                mover.stickToGround = false;
                /*if (!disableAllDebugs) */
                //Debug.LogWarning("stickToGround Off");
                result = true;
                currentVel.y = jumpVelocity;
                vertMovSt = VerticalMovementState.Jumping;
                timePressingJump = 0;
                collCheck.StartJump();
                //StopBufferedInput(PlayerInput.WallJump);
                myPlayerAnimations.skeletonAnim.Skeleton.SetAttachment("weapon", null);

            }
        }


        if (!result && !calledFromBuffer)
        {
            //BufferInput(PlayerInput.Jump);
        }
        return result;
    }

    public void StopJump()
    {
        //myPlayerAnimation.SetJump(false);
        vertMovSt = VerticalMovementState.None;
        timePressingJump = 0;
    }

    void ProcessJumpInsurance()
    {
        if (!jumpInsurance)
        {
            //Debug.LogWarning(" collCheck.lastBelow = " + (collCheck.lastBelow) + "; collCheck.below = " + (collCheck.below) +
            //   "; jumpSt = " + jumpSt+"; jumpedOutOfWater = "+jumpedOutOfWater);
            if ((collCheck.lastBelow && !collCheck.lastSliping) && (!collCheck.below || collCheck.sliping) &&
                (vertMovSt == VerticalMovementState.None || vertMovSt == VerticalMovementState.Falling))
            {
                //print("Jump Insurance");
                jumpInsurance = true;
                timeJumpInsurance = 0;
            }
        }
        else
        {
            timeJumpInsurance += Time.deltaTime;
            if (timeJumpInsurance >= maxTimeJumpInsurance || vertMovSt == VerticalMovementState.Jumping)
            {
                jumpInsurance = false;
            }
        }

    }
    #endregion


    #endregion

    #region --- CHARACTER ROTATION ---
    public void RotateCharacter(bool right)
    {
        if ((right && rotateObj.localRotation.eulerAngles.y == 0)||(!right && rotateObj.localRotation.eulerAngles.y==180)) return;
        Debug.Log("Rotation char to " + (right ? "right" : "left"));
        if (right)
        {
            rotateObj.localRotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            rotateObj.localRotation = Quaternion.Euler(0, 180, 0);
        }
        myPlayerWeapon.ResetAimPosition();
    }
    #endregion

    #region  CHECK WIN && GAME OVER ---------------------------------------------

    public void DoGameOver()
    {
        Debug.Log("GAME OVER");
        currentSpeed = 0;
        currentVel = Vector3.zero;
        mover.SetVelocity(currentVel, Vector3.zero);
        //TO REDO
        //controller.Move(currentVel * Time.deltaTime);
    }
    #endregion

    #region  AUXILIAR FUNCTIONS ---------------------------------------------

    public float SignedRelativeAngle(Vector3 referenceForward, Vector3 newDirection, Vector3 referenceUp)
    {
        // the vector perpendicular to referenceForward (90 degrees clockwise)
        // (used to determine if angle is positive or negative)
        Vector3 referenceRight = Vector3.Cross(referenceUp, referenceForward);
        // Get the angle in degrees between 0 and 180
        float angle = Vector3.Angle(newDirection, referenceForward);
        // Determine if the degree value should be negative.  Here, a positive value
        // from the dot product means that our vector is on the right of the reference vector   
        // whereas a negative value means we're on the left.
        float sign = Mathf.Sign(Vector3.Dot(newDirection, referenceRight));
        return (sign * angle);//final angle
    }

    public void TeleportPlayer(Vector3 worldPos)
    {
        transform.position = worldPos;
    }

    Vector3 AngleToVector(float angle)
    {
        angle = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle));
    }

    public Vector3 RotateVector(float angle, Vector3 vector)
    {
        //rotate angle -90 degrees
        float theta = angle * Mathf.Deg2Rad;
        float cs = Mathf.Cos(theta);
        float sn = Mathf.Sin(theta);
        float px = vector.x * cs - vector.z * sn;
        float py = vector.x * sn + vector.z * cs;
        return new Vector3(px, 0, py).normalized;
    }

    #endregion
}

#region --- [ STRUCTS & CLASSES ] ---

#endregion
