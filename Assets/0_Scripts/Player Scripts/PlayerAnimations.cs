using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public enum PlayerAnimationState
{
    None,
    Idle,
    Walking,
    Jumping,
    Falling,
    Landing
}

public class PlayerAnimations : MonoBehaviour
{
    private PlayerMovementCMF myPlayerMov;

    [Header("--- Spine Animations ---")]
    public SkeletonAnimation skeletonAnim;
    public AnimationReferenceAsset playerAnimIdle, playerAnimWalking, playerAnimJumping, playerAnimLanding;
    public PlayerAnimationState animationSt = PlayerAnimationState.None;
    public float idleTimeScale = 1, walkingTimeScale = 1, jumpingTimeScale = 1, LandingTimeScale = 1;
    public float timeToFloorToStartLandingAnim = 3;

    public void KonoAwake()
    {
        myPlayerMov = GetComponent<PlayerMovementCMF>();
    }

    public void KonoUpdate()
    {
        ManageAnimations();

        ProcessAnimationStates();
    }

    void ManageAnimations()
    {
        if (myPlayerMov.collCheck.below)
        {
            if (!myPlayerMov.collCheck.sliping)
            {
                //if (!myPlayerMov.collCheck.lastBelow)
                //{
                //    SetAnimationState(PlayerAnimationState.Landing);
                //}
                //else
                //{
                if (myPlayerMov.currentVel.x == 0 && (animationSt != PlayerAnimationState.Landing || (animationSt == PlayerAnimationState.Landing &&
                    skeletonAnim.state.GetCurrent(0).IsComplete))) SetAnimationState(PlayerAnimationState.Idle);
                else if (myPlayerMov.currentSpeed != 0) SetAnimationState(PlayerAnimationState.Walking);
            }
            else //Sliding
            {

            }
        }
        else if (myPlayerMov.vertMovSt == VerticalMovementState.Jumping) SetAnimationState(PlayerAnimationState.Jumping);
        else {
            if(myPlayerMov.currentVel.y < 0)
            {
                float timeToLand = myPlayerMov.collCheck.distanceToFloor / -myPlayerMov.currentVel.y;
            if (timeToLand <= timeToFloorToStartLandingAnim) SetAnimationState(PlayerAnimationState.Landing);
                else SetAnimationState(PlayerAnimationState.Falling);

            }
            else
            {
                SetAnimationState(PlayerAnimationState.Falling);

            }
        }
    }

    public void SetAnimation(AnimationReferenceAsset animation, bool loop, float timeScale)
    {
        skeletonAnim.state.SetAnimation(0, animation, loop).TimeScale = timeScale;
    }

    public void SetAnimationState(PlayerAnimationState state)
    {
        if (state == animationSt) return;
        animationSt = state;
        switch (animationSt)
        {
            case PlayerAnimationState.Idle:
                SetAnimation(playerAnimIdle, true, idleTimeScale);
                break;
            case PlayerAnimationState.Walking:
                float movementSpeedPercent = myPlayerMov.currentSpeed/myPlayerMov.maxMoveSpeed;
                float walkingAnimTimeScale = walkingTimeScale * movementSpeedPercent;
                Debug.Log("WALKING ANIMATION: currentSpeed = " + myPlayerMov.currentSpeed + "; maxMoveSpeed = " + myPlayerMov.maxMoveSpeed + "; movementSpeedPercent = " + movementSpeedPercent+
                    "; walkingAnimTimeScale = " + walkingAnimTimeScale);
                SetAnimation(playerAnimWalking, true, walkingAnimTimeScale);
                break;
            case PlayerAnimationState.Jumping:
                SetAnimation(playerAnimJumping, false, jumpingTimeScale);
                break;
            case PlayerAnimationState.Landing:
                SetAnimation(playerAnimLanding, false, LandingTimeScale);
                break;
            case PlayerAnimationState.Falling:
                SetAnimation(playerAnimJumping, false, 0);
                Spine.TrackEntry currentAnimation = skeletonAnim.state.GetCurrent(0);
                currentAnimation.TrackTime = skeletonAnim.Skeleton.Data.FindAnimation("walk").Duration;
                break;
        }
    }

    void ProcessAnimationStates()
    {
        switch (animationSt)
        {
            case PlayerAnimationState.Walking:
                float movementSpeedPercent = myPlayerMov.currentSpeed / myPlayerMov.maxMoveSpeed;
                float walkingAnimTimeScale = walkingTimeScale * movementSpeedPercent;
                skeletonAnim.state.GetCurrent(0).TimeScale = walkingAnimTimeScale;
                break;
        }
    }
}
